using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HikayeStudyo
{
    // 3D oyuncuların hangi yöne baktığını ölçer (ayak -> parmak ucu yönü). Köprüden: menu: Hikaye/Yon Testi
    // Sonuç: HikayeKomut/yon_test.txt  (z>0: +Z'ye bakıyor; kamera -Z'de olduğu için yaw=180 gerekir)
    public static class YonTesti
    {
        [MenuItem("Hikaye/Yon Testi")]
        public static void Calistir()
        {
            var sb = new StringBuilder();
            var kat = KarakterKatalogu.Yukle();
            foreach (var kayit in kat.karakterler)
            {
                if (kayit == null || kayit.prefab == null) continue;
                var go = (GameObject)Object.Instantiate(kayit.prefab);
                try
                {
                    go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    var an = go.GetComponentInChildren<Animator>();
                    var k = go.GetComponent<Karakter3D>();
                    if (an == null || !an.isHuman) { sb.AppendLine(kayit.kimlik + "  (insan iskeleti yok)"); continue; }
                    sb.Append(kayit.kimlik).Append("  duruş=").Append(Yon(an));
                    foreach (var ad in new[] { "dur", "konus", "yuru", "otur", "agla" })
                    {
                        var clip = k != null ? k.Find(ad) : null;
                        if (clip == null || clip.legacy) continue;
                        var graph = PlayableGraph.Create("YonTesti");
                        try
                        {
                            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                            var cp = AnimationClipPlayable.Create(graph, clip);
                            AnimationPlayableOutput.Create(graph, "o", an).SetSourcePlayable(cp);
                            cp.SetTime(Mathf.Min(0.5f, clip.length * 0.5f));
                            graph.Evaluate(0f);
                            sb.Append("  ").Append(clip.name).Append('=').Append(Yon(an));
                        }
                        finally { graph.Destroy(); }
                    }
                    sb.AppendLine();
                }
                catch (System.Exception e) { sb.AppendLine(kayit.kimlik + "  HATA: " + e.Message); }
                finally { Object.DestroyImmediate(go); }
            }
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/yon_test.txt"), sb.ToString(), new UTF8Encoding(false));
            Debug.Log("Yon testi bitti: HikayeKomut/yon_test.txt");
        }

        // Mixamo modellerinin küçük resimleri (model seçerken bakmak için).
        // Köprüden: menu: Hikaye/Model Gorselleri -> HikayeKomut/modeller.png + modeller.txt (soldan sağa, üstten alta)
        [MenuItem("Hikaye/Model Gorselleri")]
        public static void Gorseller()
        {
            var secilen = new System.Collections.Generic.List<GameObject>();
            var adlar = new System.Collections.Generic.List<string>();
            foreach (var kayit in KarakterKatalogu.Yukle().karakterler)
                if (kayit != null && kayit.prefab != null && kayit.kimlik.StartsWith("Mixamo_")) { secilen.Add(kayit.prefab); adlar.Add(kayit.kimlik); }
            const int kolon = 8, w = 160, h = 260;
            int satir = (secilen.Count + kolon - 1) / kolon;
            var tablo = new Texture2D(w * kolon, h * Mathf.Max(1, satir), TextureFormat.RGBA32, false);
            var pru = new PreviewRenderUtility();
            try
            {
                for (int i = 0; i < secilen.Count; i++)
                {
                    pru.BeginStaticPreview(new Rect(0, 0, w, h));
                    var go = pru.InstantiatePrefabInScene(secilen[i]);
                    go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    pru.camera.transform.SetPositionAndRotation(new Vector3(0f, 0.95f, 3.4f), Quaternion.Euler(0f, 180f, 0f));
                    pru.camera.fieldOfView = 34f;
                    pru.camera.nearClipPlane = 0.1f; pru.camera.farClipPlane = 20f;
                    pru.camera.clearFlags = CameraClearFlags.SolidColor;
                    pru.camera.backgroundColor = new Color(0.45f, 0.45f, 0.5f);
                    pru.lights[0].intensity = 1.2f; pru.lights[0].transform.rotation = Quaternion.Euler(35f, 160f, 0f);
                    pru.Render(true);
                    var kare = pru.EndStaticPreview();
                    tablo.SetPixels((i % kolon) * w, (satir - 1 - i / kolon) * h, w, h, kare.GetPixels(0, 0, w, h));
                    Object.DestroyImmediate(kare);
                    Object.DestroyImmediate(go);
                }
                tablo.Apply();
                var kok = Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut");
                File.WriteAllBytes(Path.Combine(kok, "modeller.png"), tablo.EncodeToPNG());
                File.WriteAllText(Path.Combine(kok, "modeller.txt"), string.Join("\n", adlar.ToArray()), new UTF8Encoding(false));
                Debug.Log("Model gorselleri yazildi: " + secilen.Count);
            }
            finally { pru.Cleanup(); Object.DestroyImmediate(tablo); }
        }

        static string Yon(Animator an)
        {
            Vector3 f = Vector3.zero;
            var lf = an.GetBoneTransform(HumanBodyBones.LeftFoot); var lt = an.GetBoneTransform(HumanBodyBones.LeftToes);
            var rf = an.GetBoneTransform(HumanBodyBones.RightFoot); var rt = an.GetBoneTransform(HumanBodyBones.RightToes);
            if (lf != null && lt != null) f += lt.position - lf.position;
            if (rf != null && rt != null) f += rt.position - rf.position;
            // omuz çizgisinden ön yön (parmak kemiği olmayan modeller için de)
            var ls = an.GetBoneTransform(HumanBodyBones.LeftUpperArm); var rs = an.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Vector3 o = Vector3.zero;
            if (ls != null && rs != null) o = Vector3.Cross(rs.position - ls.position, Vector3.up);
            f.y = 0f; o.y = 0f;
            return "[ayak z=" + (f.sqrMagnitude > 1e-8f ? f.normalized.z.ToString("0.00") : "?") + " omuz z=" + (o.sqrMagnitude > 1e-8f ? o.normalized.z.ToString("0.00") : "?") + "]";
        }
    }
}
