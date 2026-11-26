using System.IO;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Oynatma sırasında 2 saniyede bir Game görüntüsünü HikayeKomut/kareler/kare_NN.png olarak kaydeder.
    // Köprüden: menu: Hikaye/Kare Serisi  ->  oynat: <hikaye>  ->  dur   (en fazla 30 kare)
    // Uzun film için HikayeKomut/kare_ayar.txt: "aralik_sn en_fazla_kare" (örn. "4 90")
    [InitializeOnLoad]
    public static class KareSerisi
    {
        const string Bayrak = "HS_KareSerisi";
        const string AralikAnahtar = "HS_KareAralik", AdetAnahtar = "HS_KareAdet";
        static float sonraki;
        static int sayi;

        static KareSerisi()
        {
            EditorApplication.update += Ornekle;
            EditorApplication.playModeStateChanged += d =>
            {
                if (d == PlayModeStateChange.ExitingPlayMode && sayi > 0) { SessionState.SetBool(Bayrak, false); sayi = 0; }
            };
        }

        static string Klasor { get { return Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/kareler"); } }

        [MenuItem("Hikaye/Kare Serisi")]
        public static void Kur()
        {
            Directory.CreateDirectory(Klasor);
            foreach (var f in Directory.GetFiles(Klasor, "kare_*.png")) File.Delete(f);
            sayi = 0; sonraki = 0f;
            float aralik = 2f; int adet = 30;
            var ayar = Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/kare_ayar.txt");
            if (File.Exists(ayar))
            {
                var p = File.ReadAllText(ayar).Trim().Split(' ');
                float a; int n;
                if (p.Length == 2 && float.TryParse(p[0], out a) && int.TryParse(p[1], out n))
                {
                    aralik = Mathf.Clamp(a, 1f, 30f); adet = Mathf.Clamp(n, 1, 150);
                }
            }
            SessionState.SetFloat(AralikAnahtar, aralik);
            SessionState.SetInt(AdetAnahtar, adet);
            SessionState.SetBool(Bayrak, true);
        }

        static void Ornekle()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(Bayrak, false) || sayi >= SessionState.GetInt(AdetAnahtar, 30)) return;
            if (Time.time < sonraki) return;
            sonraki = Time.time + SessionState.GetFloat(AralikAnahtar, 2f);
            ScreenCapture.CaptureScreenshot(Path.Combine(Klasor, "kare_" + sayi.ToString("00") + ".png"));
            sayi++;
        }
    }
}
