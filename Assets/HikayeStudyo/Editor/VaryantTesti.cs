using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HikayeStudyo
{
    // Karakter çeşitleri denetimi. Köprüden: menu: Hikaye/Varyant Testi
    //   HikayeKomut/varyant_testi.txt : toplam, yinelenenler, model başına tanınan parçalar ve malzeme adları
    //   HikayeKomut/varyant.png       : 6 model x (asıl + 3 çeşit) = 24 örnek
    public static class VaryantTesti
    {
        const int Kolon = 4, EnFazlaSatir = 6, G = 220, Y = 330;

        [MenuItem("Hikaye/Varyant Testi")]
        public static void Calistir()
        {
            var hazir = new List<GameObject>();
            if (AssetDatabase.IsValidFolder(Karakter3DKurulum.Hazir))
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { Karakter3DKurulum.Hazir }))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                    if (go != null) hazir.Add(go);
                }
            var tabanlar = Varyantlar.Tabanlar(hazir);
            var liste = Varyantlar.Liste(hazir);

            var adlar = new List<string>();
            foreach (var g in hazir) adlar.Add(g.name);

            var adKumesi = new HashSet<string>();
            var gorunumKumesi = new HashSet<string>();
            var renkKumesi = new HashSet<string>();
            int yinelenenAd = 0, yinelenenGorunum = 0, cozulemeyen = 0;
            var adet = new Dictionary<GameObject, int>();
            foreach (var v in liste)
            {
                var t = v.tarif;
                string renk = v.taban.name + "|" + t.ust + "," + t.alt + "," + t.sac + "," + t.ten + "," + t.cantaGizli;
                if (!adKumesi.Add(Util.Norm(v.ad))) yinelenenAd++;
                if (!gorunumKumesi.Add(renk + "|" + t.olcek)) yinelenenGorunum++;
                if (t.ust + t.alt + t.sac + t.ten > 0 || t.cantaGizli) renkKumesi.Add(renk);
                if (!Varyantlar.AdGecerli(v.ad, adlar)) cozulemeyen++;
                int n; adet.TryGetValue(v.taban, out n); adet[v.taban] = n + 1;
            }
            // asıl 3D oyuncu adlarından hiçbiri çeşit adı gibi okunmamalı
            var karisan = new List<string>();
            foreach (var a in adlar) { string t; int no; if (Varyantlar.AdAyir(a, out t, out no)) karisan.Add(a); }

            var sb = new StringBuilder();
            sb.AppendLine("hazir_prefab=" + hazir.Count + " insan_temel_model=" + tabanlar.Count + " cesit=" + liste.Count);
            sb.AppendLine("renk_veya_aksesuar_ile_farkli_cesit=" + renkKumesi.Count + " (kalanlar yalniz boy/en farki)");
            sb.AppendLine("yinelenen_ad=" + yinelenenAd + " yinelenen_gorunum=" + yinelenenGorunum + " cozulemeyen_ad=" + cozulemeyen);
            sb.AppendLine("cesit_adiyla_karisan_asil_ad=" + (karisan.Count == 0 ? "yok" : string.Join(", ", karisan.ToArray())));
            sb.AppendLine();
            sb.AppendLine("== Temel modeller ==");
            foreach (var pf in tabanlar)
            {
                var p = Varyantlar.Parcalar(pf, pf.name);
                int n; adet.TryGetValue(pf, out n);
                sb.AppendLine(pf.name + ": cesit=" + n + "  tanınan: " + p.Ozet());
                var gorulen = new HashSet<string>();
                foreach (var y in Varyantlar.Yuvalar(pf, pf.name))
                {
                    var m = y.malzeme;
                    string s = "    " + y.rendererAdi + " / " + y.malzemeAdi + "  [" + (m.shader != null ? m.shader.name : "?") +
                               (Varyantlar.Dokulu(m) ? ", dokulu" : ", duz") + "] -> " + (y.parca == VaryantParca.Yok ? "dokunulmaz" : y.parca.ToString());
                    if (gorulen.Add(s)) sb.AppendLine(s);
                }
            }

            Directory.CreateDirectory(UzaktanKomut.Dir);
            var secilen = OrnekModeller(tabanlar);
            sb.AppendLine();
            sb.AppendLine("== varyant.png (üstten alta satırlar, soldan sağa sütunlar) ==");
            foreach (var pf in secilen)
            {
                sb.Append(pf.name + " (asıl)");
                var p = Varyantlar.Parcalar(pf, pf.name);
                for (int no = 1; no < Kolon; no++)
                    sb.Append(" | " + Varyantlar.Ad(pf.name, no) + " [" + Varyantlar.Aciklama(pf.name, Varyantlar.Tarif(pf.name, p, no)) + "]");
                sb.AppendLine();
            }
            var rapor = Path.Combine(UzaktanKomut.Dir, "varyant_testi.txt");
            File.WriteAllText(rapor, sb.ToString(), new UTF8Encoding(false));   // resim çizimi hata verse de rapor kalsın

            if (secilen.Count > 0)
            {
                try { Ciz(secilen, Path.Combine(UzaktanKomut.Dir, "varyant.png")); }
                catch (System.Exception e) { File.AppendAllText(rapor, "RESIM HATASI: " + e + "\n"); }
            }
            Debug.Log("[Hikaye Stüdyosu] Varyant testi: " + liste.Count + " çeşit → HikayeKomut/varyant_testi.txt, varyant.png");
        }

        // Her gruptan sırayla; rengi tanınan modeller önce
        static List<GameObject> OrnekModeller(List<GameObject> tabanlar)
        {
            var gruplar = new List<List<GameObject>>();
            foreach (var onek in new[] { "VRM_", "Erkek_", "Mixamo_", "Kadin_" })
            {
                var renkli = new List<GameObject>();
                var renksiz = new List<GameObject>();
                foreach (var g in tabanlar)
                {
                    if (!g.name.StartsWith(onek)) continue;
                    if (Varyantlar.Parcalar(g, g.name).RenkVar) renkli.Add(g); else renksiz.Add(g);
                }
                renkli.AddRange(renksiz);
                gruplar.Add(renkli);
            }
            var l = new List<GameObject>();
            for (int i = 0; l.Count < EnFazlaSatir; i++)
            {
                bool eklendi = false;
                foreach (var gr in gruplar)
                    if (i < gr.Count && l.Count < EnFazlaSatir) { l.Add(gr[i]); eklendi = true; }
                if (!eklendi) break;
            }
            return l;
        }

        static void Ciz(List<GameObject> modeller, string yol)
        {
            int satir = modeller.Count;
            var tablo = new Texture2D(G * Kolon, Y * satir, TextureFormat.RGBA32, false);
            var pru = new PreviewRenderUtility();
            try
            {
                for (int s = 0; s < satir; s++)
                    for (int no = 0; no < Kolon; no++)
                    {
                        pru.BeginStaticPreview(new Rect(0, 0, G, Y));
                        var go = pru.InstantiatePrefabInScene(modeller[s]);
                        VaryantGorunum v = null;
                        try
                        {
                            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            if (no > 0)
                            {
                                v = go.AddComponent<VaryantGorunum>();
                                v.Kur(modeller[s].name, no);
                                v.Olcekle();
                            }
                            pru.camera.transform.SetPositionAndRotation(new Vector3(0f, 1.05f, 4.4f), Quaternion.Euler(0f, 180f, 0f));
                            pru.camera.fieldOfView = 30f;
                            pru.camera.nearClipPlane = 0.1f; pru.camera.farClipPlane = 20f;
                            pru.camera.clearFlags = CameraClearFlags.SolidColor;
                            pru.camera.backgroundColor = new Color(0.45f, 0.45f, 0.5f);
                            pru.lights[0].intensity = 1.2f; pru.lights[0].transform.rotation = Quaternion.Euler(35f, 160f, 0f);
                            Durus(go, () => pru.Render(true));
                            var kare = pru.EndStaticPreview();
                            tablo.SetPixels(no * G, (satir - 1 - s) * Y, G, Y, kare.GetPixels(0, 0, G, Y));
                            Object.DestroyImmediate(kare);
                        }
                        finally
                        {
                            if (v != null) v.Temizle();
                            Object.DestroyImmediate(go);
                        }
                    }
                tablo.Apply();
                File.WriteAllBytes(yol, tablo.EncodeToPNG());
            }
            finally { pru.Cleanup(); Object.DestroyImmediate(tablo); }
        }

        // Kollar T duruşunda kalmasın: bekleme animasyonunun bir karesi uygulanıp çizilir
        static void Durus(GameObject go, System.Action ciz)
        {
            var an = go.GetComponentInChildren<Animator>();
            var k = go.GetComponent<Karakter3D>();
            var clip = k != null ? k.Find("dur") : null;
            if (an == null || clip == null || clip.legacy) { ciz(); return; }
            var graph = PlayableGraph.Create("VaryantTesti");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var cp = AnimationClipPlayable.Create(graph, clip);
                AnimationPlayableOutput.Create(graph, "o", an).SetSourcePlayable(cp);
                cp.SetTime(Mathf.Min(0.5f, clip.length * 0.5f));
                graph.Evaluate(0f);
                ciz();
            }
            finally { graph.Destroy(); }
        }
    }
}
