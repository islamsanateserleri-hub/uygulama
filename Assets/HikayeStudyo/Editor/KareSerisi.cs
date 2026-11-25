using System.IO;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Oynatma sırasında 2 saniyede bir Game görüntüsünü HikayeKomut/kareler/kare_NN.png olarak kaydeder.
    // Köprüden: menu: Hikaye/Kare Serisi  ->  oynat: <hikaye>  ->  dur   (en fazla 30 kare)
    [InitializeOnLoad]
    public static class KareSerisi
    {
        const string Bayrak = "HS_KareSerisi";
        const float Aralik = 2f;
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
            SessionState.SetBool(Bayrak, true);
        }

        static void Ornekle()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(Bayrak, false) || sayi >= 30) return;
            if (Time.time < sonraki) return;
            sonraki = Time.time + Aralik;
            ScreenCapture.CaptureScreenshot(Path.Combine(Klasor, "kare_" + sayi.ToString("00") + ".png"));
            sayi++;
        }
    }
}
