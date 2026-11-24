using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 1 testi: komut satırı  "-kayit <cikis.mp4> -hikaye <ad> [-klaket]"  ya da çalışma klasöründeki ffkayit.txt
    // (satırlar: hikaye=<ad>, cikis=<yol>, klaket=1). Sonuç: ffkayit_sonuc.txt
    public static class KayitBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Basla()
        {
            string hikaye = null, cikis = null; bool klaket = false;
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == "-kayit" && i + 1 < a.Length) cikis = a[i + 1];
                else if (a[i] == "-hikaye" && i + 1 < a.Length) hikaye = a[i + 1];
                else if (a[i] == "-klaket") klaket = true;
            }
            string dosya = Path.Combine(Directory.GetCurrentDirectory(), "ffkayit.txt");
            if (hikaye == null && File.Exists(dosya))
            {
                try
                {
                    foreach (var satir in File.ReadAllLines(dosya, Encoding.UTF8))
                    {
                        int e = satir.IndexOf('='); if (e < 0) continue;
                        string k = satir.Substring(0, e).Trim().ToLowerInvariant(), v = satir.Substring(e + 1).Trim();
                        if (k == "hikaye") hikaye = v; else if (k == "cikis") cikis = v; else if (k == "klaket") klaket = v == "1";
                    }
                    File.Delete(dosya);
                }
                catch (Exception ex) { Debug.LogWarning("[Kayıt] ffkayit.txt: " + ex.Message); }
            }
            if (string.IsNullOrEmpty(hikaye)) return;
            // Pencere gizlenince/odak kaybolunca oynatıcı durmasın (kayıt ve ffmpeg birleştirme ana döngüye bağlı)
            Application.runInBackground = true;
            if (string.IsNullOrEmpty(cikis)) cikis = Path.Combine(Directory.GetCurrentDirectory(), "Kayitlar", hikaye + ".mp4");
            // Sahnedeki hazır StoryPlayer (autoPlay açık) kayıtla çakışmasın: Start() çalışmadan kapat
            foreach (var eski in UnityEngine.Object.FindObjectsByType<StoryPlayer>(FindObjectsInactive.Include))
                eski.gameObject.SetActive(false);
            var go = new GameObject("KayitIsleyici");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var k2 = go.AddComponent<KayitIsleyici>();
            k2.hikaye = hikaye; k2.cikis = cikis; k2.klaket = klaket;
        }
    }

    public class KayitIsleyici : MonoBehaviour
    {
        public string hikaye, cikis; public bool klaket;
        FfmpegPipeRecorder rec; bool bitti; bool sonuc; string mesaj = "";

        IEnumerator Start()
        {
            var ta = Resources.Load<TextAsset>("Hikayeler/" + hikaye);
            if (ta == null) { Yaz(false, "hikaye bulunamadı: Resources/Hikayeler/" + hikaye); yield break; }

            // TikTok güvenli alan rehberi yalnız önizleme içindir, videoya girmesin (1. kayıt denemesinde kırmızı bantlar videoya girmişti)
            PlayerPrefs.SetInt("HS_SafeArea", 0);
            // StoryPlayer Start() içinde arayüzü kurar; kayıt ondan sonra başlamalı
            var go = new GameObject("StoryPlayer");
            var sp = go.AddComponent<StoryPlayer>();
            sp.story = ta; sp.autoPlay = false;
            // EXE'de sahnedeki oyuncu listesi yok: Windows derlemesinin yazdığı Resources/OyuncuListesi'ni kullan
            var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
            if (ol != null) { foreach (var pf in ol.prefablar) if (pf != null && !sp.oyuncular.Contains(pf)) sp.oyuncular.Add(pf); }
            else Debug.LogWarning("[Kayıt] Resources/OyuncuListesi yok: 3D oyuncular bulunamaz");
            yield return null; yield return null;

            rec = go.AddComponent<FfmpegPipeRecorder>();
            rec.Klaket = klaket;
            string hata;
            if (!rec.Baslat(cikis, 1080, 1920, 30, out hata)) { Yaz(false, hata); yield break; }
            rec.Bitti += (ok, m) => { sonuc = ok; mesaj = m; bitti = true; };
            StoryPlayer.Finished += Bitis;
            sp.Baslat();
        }

        void Bitis() { StartCoroutine(Kapat()); }

        IEnumerator Kapat()
        {
            StoryPlayer.Finished -= Bitis;
            yield return new WaitForSeconds(1f);
            rec.Durdur();
            float t = 0; while (!bitti && t < 240f) { t += Time.unscaledDeltaTime; yield return null; }
            string rapor = "";
            if (bitti && sonuc) rapor = Bilgi(cikis);
            Yaz(bitti && sonuc, (bitti ? mesaj : "zaman aşımı") + "\n" + rapor);
        }

        static string Bilgi(string yol)
        {
            try
            {
                var psi = new ProcessStartInfo(FfmpegPipeRecorder.FfmpegBul(), "-hide_banner -i \"" + yol + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
                using (var p = Process.Start(psi)) { string e = p.StandardError.ReadToEnd(); p.WaitForExit(20000); return e; }
            }
            catch (Exception ex) { return "ffmpeg bilgi: " + ex.Message; }
        }

        void Yaz(bool ok, string m)
        {
            try
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "ffkayit_sonuc.txt"),
                    DateTime.Now.ToString("HH:mm:ss") + " " + (ok ? "TAMAM" : "HATA") + "\n" + cikis + "\n" + m, Encoding.UTF8);
            }
            catch { }
            Debug.Log("[Kayıt] " + (ok ? "TAMAM " : "HATA ") + m);
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}
