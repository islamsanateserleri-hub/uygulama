using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HikayeStudyo
{
    // KutuphaneHam/Rocketbox altindaki gercekci insanlari projeye alir ve resimlerini cizer.
    // Liste: HikayeKomut/rocketbox_liste.txt (her satirda bir klasor adi, or. Female_Adult_02; HEPSI = tumu); yoksa 4 ornek.
    // Kopruden: menu: Hikaye/Gercek Karakter Ekle  ->  HikayeKomut/gercek_rapor.txt, gercek_galeri.png
    public static class RocketboxKur
    {
        const int G = 360, Y = 640;
        static readonly string[] Ornek = { "Female_Adult_01", "Male_Adult_01", "Female_Child_01", "Male_Child_01" };

        [MenuItem("Hikaye/Gercek Karakter Ekle", priority = 43)]
        public static void Ekle()
        {
            SessionState.SetBool("HS_GaleriSoruldu", true);   // kurulum sonrasi soru penceresi kopruyu bekletmesin
            Directory.CreateDirectory(UzaktanKomut.Dir);
            var sb = new StringBuilder();
            var raporYolu = Path.Combine(UzaktanKomut.Dir, "gercek_rapor.txt");

            var istenen = new List<string>();
            var listeYolu = Path.Combine(UzaktanKomut.Dir, "rocketbox_liste.txt");
            if (File.Exists(listeYolu))
                foreach (var s in File.ReadAllLines(listeYolu))
                    if (s.Trim().Length > 0) istenen.Add(s.Trim());
            if (istenen.Count == 0) istenen.AddRange(Ornek);

            var kok = Path.Combine("KutuphaneHam", "Rocketbox", "Avatars");
            if (istenen.Contains("HEPSI") && Directory.Exists(kok))
            {
                var tumu = new SortedSet<string>(System.StringComparer.Ordinal);
                foreach (var grup in Directory.GetDirectories(kok))
                    foreach (var d in Directory.GetDirectories(grup)) tumu.Add(Path.GetFileName(d));
                istenen.Clear();
                istenen.AddRange(tumu);
            }

            // disk yeri: kopya + Unity'nin ice aktarma onbellegi
            long gerek = 0;
            foreach (var ad in istenen)
            {
                if (Directory.Exists(Karakter3DKurulum.RocketboxDir + "/" + ad)) continue;
                if (!Directory.Exists(kok)) break;
                foreach (var grup in Directory.GetDirectories(kok))
                {
                    var k = Path.Combine(grup, ad);
                    if (!Directory.Exists(k)) continue;
                    foreach (var f in Directory.GetFiles(k, "*", SearchOption.AllDirectories))
                        if (!f.EndsWith(ad + ".fbx")) gerek += new FileInfo(f).Length;
                    break;
                }
            }
            long bos = new DriveInfo(Path.GetPathRoot(Path.GetFullPath("."))).AvailableFreeSpace;
            sb.AppendLine("disk: bos=" + (bos >> 30) + " GB, kopyalanacak=" + (gerek >> 20) + " MB");
            if (bos < gerek * 5 / 2 + (5L << 30))
            {
                sb.AppendLine("DISK YETERSIZ: hicbir sey eklenmedi.");
                File.WriteAllText(raporYolu, sb.ToString(), new UTF8Encoding(false));
                Debug.LogWarning("[Hikaye Stüdyosu] Disk yetersiz, gerçek karakterler eklenmedi.");
                return;
            }
            int kopya = 0;
            var adlar = new List<string>();
            foreach (var ad in istenen)
            {
                string kaynak = null;
                if (Directory.Exists(kok))
                    foreach (var grup in Directory.GetDirectories(kok))
                        if (Directory.Exists(Path.Combine(grup, ad))) { kaynak = Path.Combine(grup, ad); break; }
                if (kaynak == null) { sb.AppendLine("BULUNAMADI: " + ad); continue; }

                var fbx = Path.Combine(kaynak, "Export", ad + "_facial.fbx");
                if (!File.Exists(fbx)) fbx = Path.Combine(kaynak, "Export", ad + ".fbx");
                if (!File.Exists(fbx)) { sb.AppendLine("FBX YOK: " + ad); continue; }

                var hedef = Karakter3DKurulum.RocketboxDir + "/" + ad;
                Directory.CreateDirectory(hedef);
                // once dokular, sonra model
                var doku = Path.Combine(kaynak, "Textures");
                if (Directory.Exists(doku))
                    foreach (var f in Directory.GetFiles(doku, "*.tga"))
                        kopya += Kopyala(f, hedef + "/" + Path.GetFileName(f));
                kopya += Kopyala(fbx, hedef + "/" + Path.GetFileName(fbx));
                adlar.Add(Karakter3DKurulum.RocketboxAdi(ad));
            }
            var lisans = Path.Combine("KutuphaneHam", "Rocketbox", "LICENSE.md");
            if (File.Exists(lisans) && Directory.Exists(Karakter3DKurulum.RocketboxDir))
                Kopyala(lisans, Karakter3DKurulum.RocketboxDir + "/LICENSE.md");

            AssetDatabase.Refresh();
            int kurulan = Karakter3DKurulum.GercekKur(true);
            sb.AppendLine("kopyalanan_dosya=" + kopya + " kurulan_prefab=" + kurulan);

            var modeller = new List<GameObject>();
            foreach (var ad in adlar)
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(Karakter3DKurulum.Hazir + "/" + ad + ".prefab");
                if (pf == null) { sb.AppendLine(ad + ": PREFAB YOK"); continue; }
                modeller.Add(pf);
                Anlat(pf, sb, adlar.Count <= 4);
            }
            File.WriteAllText(raporYolu, sb.ToString(), new UTF8Encoding(false));
            if (modeller.Count > 0)
            {
                try { Ciz(modeller, Path.Combine(UzaktanKomut.Dir, "gercek_galeri.png")); }
                catch (System.Exception e) { File.AppendAllText(raporYolu, "RESIM HATASI: " + e + "\n"); }
            }
            Debug.Log("[Hikaye Stüdyosu] Gerçek karakterler: " + modeller.Count + " hazır → HikayeKomut/gercek_rapor.txt, gercek_galeri.png");
        }

        static int Kopyala(string kaynak, string hedef)
        {
            if (File.Exists(hedef) && new FileInfo(hedef).Length == new FileInfo(kaynak).Length) return 0;
            File.Copy(kaynak, hedef, true);
            return 1;
        }

        static void Anlat(GameObject pf, StringBuilder sb, bool ayrintili)
        {
            var go = (GameObject)Object.Instantiate(pf);
            try
            {
                var b = new Bounds();
                bool ilk = true;
                var malzemeler = new List<string>();
                var sekiller = new List<string>();
                foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (ilk) { b = r.bounds; ilk = false; } else b.Encapsulate(r.bounds);
                    foreach (var m in r.sharedMaterials) if (m != null) malzemeler.Add(m.name);
                    if (r.sharedMesh != null)
                        for (int i = 0; i < r.sharedMesh.blendShapeCount; i++) sekiller.Add(r.sharedMesh.GetBlendShapeName(i));
                }
                sb.AppendLine(pf.name + ": boy=" + b.size.y.ToString("0.00") + " m, malzeme=[" + string.Join(", ", malzemeler.ToArray()) +
                              "], yuz_sekli=" + sekiller.Count);
                if (ayrintili && sekiller.Count > 0) sb.AppendLine("    " + string.Join(" ", sekiller.ToArray()));
            }
            finally { Object.DestroyImmediate(go); }
        }

        // 4 karaktere kadar: tam boy, yuz, yuz + agiz acik (her sutun bir karakter).
        // Daha fazlasi: karakter basina tek yuz resmi, 12 sutunlu izgara (sira rapordakiyle ayni).
        static void Ciz(List<GameObject> modeller, string yol)
        {
            bool izgara = modeller.Count > 4;
            int satir = izgara ? 1 : 3;
            int g = izgara ? 240 : G, y = izgara ? 320 : Y;
            int sutun = izgara ? 12 : modeller.Count;
            int sira = izgara ? (modeller.Count + sutun - 1) / sutun : satir;
            var tablo = new Texture2D(g * sutun, y * sira, TextureFormat.RGBA32, false);
            var pru = new PreviewRenderUtility();
            try
            {
                for (int s = 0; s < satir; s++)
                    for (int n = 0; n < modeller.Count; n++)
                    {
                        pru.BeginStaticPreview(new Rect(0, 0, g, y));
                        var go = pru.InstantiatePrefabInScene(modeller[n]);
                        var graph = PlayableGraph.Create("GercekGaleri");
                        try
                        {
                            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            var an = go.GetComponentInChildren<Animator>();
                            var k = go.GetComponent<Karakter3D>();
                            var clip = k != null ? k.Find("dur") : null;
                            if (an != null && clip != null && !clip.legacy)
                            {
                                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                                var cp = AnimationClipPlayable.Create(graph, clip);
                                AnimationPlayableOutput.Create(graph, "o", an).SetSourcePlayable(cp);
                                cp.SetTime(Mathf.Min(0.5f, clip.length * 0.5f));
                                graph.Evaluate(0f);
                            }
                            if (s == 2) AgizAc(go);

                            var bas = an != null && an.isHuman ? an.GetBoneTransform(HumanBodyBones.Head) : null;
                            Vector3 yuz = bas != null ? bas.position + new Vector3(0f, 0.05f, 0f) : new Vector3(0f, 1.6f, 0f);
                            bool tamBoy = !izgara && s == 0;
                            Vector3 kamera = tamBoy ? new Vector3(0f, 0.95f, 4.2f) : new Vector3(yuz.x, yuz.y, yuz.z + 1.0f);
                            pru.camera.transform.SetPositionAndRotation(kamera, Quaternion.Euler(0f, 180f, 0f));
                            pru.camera.fieldOfView = tamBoy ? 30f : 24f;
                            pru.camera.nearClipPlane = 0.05f; pru.camera.farClipPlane = 20f;
                            pru.camera.clearFlags = CameraClearFlags.SolidColor;
                            pru.camera.backgroundColor = new Color(0.45f, 0.45f, 0.5f);
                            pru.lights[0].intensity = 1.1f; pru.lights[0].transform.rotation = Quaternion.Euler(30f, 160f, 0f);
                            pru.lights[1].intensity = 0.6f; pru.lights[1].transform.rotation = Quaternion.Euler(10f, 220f, 0f);
                            pru.Render(true);
                            var kare = pru.EndStaticPreview();
                            int sx = izgara ? n % sutun : n, sy = izgara ? n / sutun : s;
                            tablo.SetPixels(sx * g, (sira - 1 - sy) * y, g, y, kare.GetPixels(0, 0, g, y));
                            Object.DestroyImmediate(kare);
                        }
                        finally
                        {
                            graph.Destroy();
                            Object.DestroyImmediate(go);
                        }
                    }
                tablo.Apply();
                File.WriteAllBytes(yol, tablo.EncodeToPNG());
            }
            finally { pru.Cleanup(); Object.DestroyImmediate(tablo); }
        }

        static void AgizAc(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (r.sharedMesh == null) continue;
                for (int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                    if (r.sharedMesh.GetBlendShapeName(i).ToLowerInvariant().Replace("_", "").EndsWith("jawopen"))
                        r.SetBlendShapeWeight(i, 70f);
            }
        }
    }
}
