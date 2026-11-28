using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Hangi 3D oyuncuda gerçekten oynayabilecek ağız/göz var? (Aşama 9)
    // Köprüden: menu: Hikaye/Yuz Testi  ->  HikayeKomut/yuz_test.txt
    public static class YuzTesti
    {
        [MenuItem("Hikaye/Yuz Testi")]
        public static void Calistir()
        {
            var satirlar = new StringBuilder();
            int toplam = 0, cene = 0, agizSekli = 0, goz = 0, duygu = 0, insanDegil = 0;
            foreach (var kayit in KarakterKatalogu.Yukle().karakterler)
            {
                if (kayit == null || kayit.prefab == null) continue;
                var go = (GameObject)Object.Instantiate(kayit.prefab);
                try
                {
                    toplam++;
                    var an = go.GetComponentInChildren<Animator>();
                    bool insan = an != null && an.isHuman;
                    if (!insan) insanDegil++;
                    bool ceneKemigi = insan && an.GetBoneTransform(HumanBodyBones.Jaw) != null;
                    string ceneAdli = null;
                    foreach (var t in go.GetComponentsInChildren<Transform>(true))
                        if (t.name.ToLowerInvariant().Contains("jaw")) { ceneAdli = t.name; break; }

                    int sekil = 0;
                    var kanallar = new SortedDictionary<string, int>();
                    foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (r.sharedMesh == null) continue;
                        for (int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                        {
                            sekil++;
                            var k = YuzCanlilik.KanalAdi(r.sharedMesh.GetBlendShapeName(i));
                            if (k == null) continue;
                            int n; kanallar.TryGetValue(k, out n); kanallar[k] = n + 1;
                        }
                    }
                    bool agiz = kanallar.ContainsKey("Agiz");
                    if (ceneKemigi) cene++;
                    if (agiz) agizSekli++;
                    if (kanallar.ContainsKey("Kirp")) goz++;
                    if (kanallar.ContainsKey("Mutlu") || kanallar.ContainsKey("Uzgun") || kanallar.ContainsKey("Gul") || kanallar.ContainsKey("Somurtma")) duygu++;

                    satirlar.Append(kayit.kimlik)
                      .Append(insan ? "" : "  (insan iskeleti yok)")
                      .Append("  cene_kemigi=").Append(ceneKemigi ? "EVET" : "hayir")
                      .Append(ceneAdli != null && !ceneKemigi ? " (eslenmemis: " + ceneAdli + ")" : "")
                      .Append("  blendshape=").Append(sekil)
                      .Append("  agiz_sekli=").Append(agiz ? "EVET" : "hayir");
                    if (kanallar.Count > 0)
                    {
                        satirlar.Append("  kanallar=");
                        foreach (var kv in kanallar) satirlar.Append(kv.Key).Append(':').Append(kv.Value).Append(' ');
                    }
                    satirlar.AppendLine();
                }
                catch (System.Exception e) { satirlar.AppendLine(kayit.kimlik + "  HATA: " + e.Message); }
                finally { Object.DestroyImmediate(go); }
            }
            var ozet = "OZET: toplam=" + toplam + " insan_iskeletsiz=" + insanDegil + " cene_kemikli=" + cene +
                       " agiz_sekilli=" + agizSekli + " goz_kirpabilen=" + goz + " yuz_duygulu=" + duygu;
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/yuz_test.txt"),
                ozet + "\n" + satirlar, new UTF8Encoding(false));
            Debug.Log(ozet);
        }

        // Çene kemiği olan oyuncularda ağız gerçekten açılıyor mu? Sol: kapalı, sağ: çene 12 derece açık.
        // Köprüden: menu: Hikaye/Cene Gorselleri  ->  HikayeKomut/cene.png + cene.txt (üstten alta)
        [MenuItem("Hikaye/Cene Gorselleri")]
        public static void CeneGorselleri()
        {
            var secilen = new List<GameObject>();
            var adlar = new List<string>();
            foreach (var kayit in KarakterKatalogu.Yukle().karakterler)
            {
                if (kayit == null || kayit.prefab == null) continue;
                var an = kayit.prefab.GetComponentInChildren<Animator>();
                if (an == null || an.avatar == null || !an.avatar.isHuman) continue;
                bool var_ = false;
                foreach (var kemik in an.avatar.humanDescription.human)
                    if (kemik.humanName == "Jaw") { var_ = true; break; }
                if (var_) { secilen.Add(kayit.prefab); adlar.Add(kayit.kimlik); }
            }
            if (secilen.Count == 0) { Debug.LogWarning("Cene gorseli: cene kemikli model bulunamadi"); return; }
            const int w = 360, h = 360;
            var tablo = new Texture2D(w * 2, h * secilen.Count, TextureFormat.RGBA32, false);
            var pru = new PreviewRenderUtility();
            try
            {
                for (int i = 0; i < secilen.Count; i++)
                    for (int acik = 0; acik < 2; acik++)
                    {
                        pru.BeginStaticPreview(new Rect(0, 0, w, h));
                        var go = pru.InstantiatePrefabInScene(secilen[i]);
                        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        var an = go.GetComponentInChildren<Animator>();
                        var bas = an.GetBoneTransform(HumanBodyBones.Head);
                        var kalca = an.GetBoneTransform(HumanBodyBones.Hips);
                        var cene = an.GetBoneTransform(HumanBodyBones.Jaw);
                        if (acik == 1 && cene != null) cene.rotation = Quaternion.AngleAxis(12f, Vector3.right) * cene.rotation;
                        float boy = Mathf.Max(0.05f, bas.position.y - kalca.position.y);
                        var hedef = bas.position + Vector3.up * boy * 0.1f;
                        pru.camera.transform.SetPositionAndRotation(hedef + new Vector3(0f, 0f, boy * 2.6f), Quaternion.Euler(0f, 180f, 0f));
                        pru.camera.fieldOfView = 28f;
                        pru.camera.nearClipPlane = boy * 0.05f; pru.camera.farClipPlane = boy * 20f;
                        pru.camera.clearFlags = CameraClearFlags.SolidColor;
                        pru.camera.backgroundColor = new Color(0.45f, 0.45f, 0.5f);
                        pru.lights[0].intensity = 1.2f; pru.lights[0].transform.rotation = Quaternion.Euler(25f, 170f, 0f);
                        pru.Render(true);
                        var kare = pru.EndStaticPreview();
                        tablo.SetPixels(acik * w, (secilen.Count - 1 - i) * h, w, h, kare.GetPixels(0, 0, w, h));
                        Object.DestroyImmediate(kare);
                        Object.DestroyImmediate(go);
                    }
                tablo.Apply();
                var kok = Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut");
                File.WriteAllBytes(Path.Combine(kok, "cene.png"), tablo.EncodeToPNG());
                File.WriteAllText(Path.Combine(kok, "cene.txt"), string.Join("\n", adlar.ToArray()), new UTF8Encoding(false));
                Debug.Log("Cene gorselleri yazildi: " + secilen.Count);
            }
            finally { pru.Cleanup(); Object.DestroyImmediate(tablo); }
        }
    }
}
