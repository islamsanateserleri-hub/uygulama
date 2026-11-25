using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 7: tek dosyalik hata raporu. HICBIR YERE GONDERILMEZ; kullanici isterse kendisi paylasir.
    // API anahtari ve kullanici klasoru yolu gizlenir.
    public static class HataRaporu
    {
        const int SatirSiniri = 300;

        public static string Klasor { get { return Path.Combine(AnaEkran.Kok, "Raporlar"); } }

        public static string Gizle(string metin, string anahtar, string kullaniciKlasoru)
        {
            if (string.IsNullOrEmpty(metin)) return "";
            if (!string.IsNullOrEmpty(anahtar) && anahtar.Length >= 8) metin = metin.Replace(anahtar, "[ANAHTAR GİZLENDİ]");
            metin = Regex.Replace(metin, @"AIza[0-9A-Za-z_\-]{20,}", "[ANAHTAR GİZLENDİ]");
            metin = Regex.Replace(metin, @"(?i)(x-goog-api-key\s*[:=]\s*|[?&]key=)[^\s&""']+", "$1[ANAHTAR GİZLENDİ]");
            if (!string.IsNullOrEmpty(kullaniciKlasoru) && kullaniciKlasoru.Length > 3)
            {
                metin = Regex.Replace(metin, Regex.Escape(kullaniciKlasoru), "%USERPROFILE%", RegexOptions.IgnoreCase);
                metin = Regex.Replace(metin, Regex.Escape(kullaniciKlasoru.Replace('\\', '/')), "%USERPROFILE%", RegexOptions.IgnoreCase);
            }
            return metin;
        }

        static string SonSatirlar(string yol)
        {
            try
            {
                if (string.IsNullOrEmpty(yol) || !File.Exists(yol)) return "(dosya yok)";
                var l = new Queue<string>();
                using (var fs = new FileStream(yol, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var r = new StreamReader(fs, Encoding.UTF8))
                {
                    string s;
                    while ((s = r.ReadLine()) != null) { l.Enqueue(s); if (l.Count > SatirSiniri) l.Dequeue(); }
                }
                return string.Join("\n", l.ToArray());
            }
            catch (Exception e) { return "(okunamadı: " + e.Message + ")"; }
        }

        // Ana is parcaciginden cagir. Donen: rapor dosyasinin yolu.
        public static string Olustur()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HİKÂYE STÜDYOSU — HATA RAPORU");
            sb.AppendLine("Tarih: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            sb.AppendLine("Sürüm: " + Application.version + "  ·  Unity " + Application.unityVersion);
            sb.AppendLine("Sistem: " + SystemInfo.operatingSystem);
            sb.AppendLine("İşlemci: " + SystemInfo.processorType + "  ·  Bellek: " + SystemInfo.systemMemorySize + " MB");
            sb.AppendLine("Ekran kartı: " + SystemInfo.graphicsDeviceName + "  ·  " + SystemInfo.graphicsMemorySize + " MB");
            sb.AppendLine("Pencere: " + Screen.width + "x" + Screen.height);
            sb.AppendLine("Video aracı (ffmpeg): " + (FfmpegPipeRecorder.FfmpegBul() ?? "BULUNAMADI"));
            sb.AppendLine("Gemini anahtarı: " + (GeminiUyg.Anahtar.Length > 0 ? "kayıtlı" + (GeminiUyg.AnahtarSifreli ? " (şifreli)" : " (ŞİFRELENEMEDİ)") : "yok"));
            sb.AppendLine("Ses modeli: " + GeminiUyg.Model);
            sb.AppendLine("Son ses raporu: " + (StoryPlayer.SonSesRaporu ?? ""));
            sb.AppendLine();
            sb.AppendLine("---- arayüz günlüğü (son " + SatirSiniri + " satır) ----");
            sb.AppendLine(SonSatirlar(AnaEkran.GunlukDosyasi));
            sb.AppendLine();
            sb.AppendLine("---- uygulama günlüğü (son " + SatirSiniri + " satır) ----");
            sb.AppendLine(SonSatirlar(Application.consoleLogPath));

            string metin = Gizle(sb.ToString(), GeminiUyg.Anahtar, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            Directory.CreateDirectory(Klasor);
            string yol = Path.Combine(Klasor, "hata_raporu_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            File.WriteAllText(yol, metin, new UTF8Encoding(false));
            return yol;
        }
    }
}
