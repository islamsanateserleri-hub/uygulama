using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Asama 7: Build/HikayeStudyo klasorunden tek kurulum dosyasi uretir (Inno Setup gerekir).
    // Sonuc: Build/kurulum_sonuc.txt  ·  Cikti: Build/Kurulum/
    public static class KurulumOlustur
    {
        const string Rapor = "Build/kurulum_sonuc.txt";
        static bool calisiyor;

        static string IsccBul()
        {
            foreach (var kok in new[]
            {
                Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                Environment.GetEnvironmentVariable("ProgramFiles"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
            })
            {
                if (string.IsNullOrEmpty(kok)) continue;
                var p = Path.Combine(kok, "Inno Setup 6", "ISCC.exe");
                if (File.Exists(p)) return p;
            }
            return null;
        }

        [MenuItem("Hikaye/Kurulum Dosyası Oluştur")]
        public static void Olustur()
        {
            if (calisiyor) { Debug.Log("[Hikaye Stüdyosu] Kurulum dosyası zaten hazırlanıyor"); return; }
            Directory.CreateDirectory("Build");
            string kok = Directory.GetCurrentDirectory();
            string klasor = Path.Combine(kok, "Build", "HikayeStudyo");
            string ffmpeg = Path.Combine(klasor, "HikayeStudyo_Data", "StreamingAssets", "ffmpeg");
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString("HH:mm:ss") + " sürüm " + PlayerSettings.bundleVersion);
            bool tamam = true;
            foreach (var f in new[] { Path.Combine(klasor, "HikayeStudyo.exe"), Path.Combine(ffmpeg, "ffmpeg.exe"), Path.Combine(ffmpeg, "ffprobe.exe"), Path.Combine(ffmpeg, "LICENSE.txt") })
            {
                bool var_ = File.Exists(f);
                tamam &= var_;
                sb.AppendLine((var_ ? "VAR " : "YOK ") + f.Substring(kok.Length + 1));
            }
            string betik = Path.Combine(kok, "Installer", "HikayeStudyosu.iss");
            if (!File.Exists(betik)) { tamam = false; sb.AppendLine("YOK Installer/HikayeStudyosu.iss"); }
            if (!tamam) { Yaz(sb + "HATA: eksik dosya. Önce Windows derlemesi al.\n"); return; }

            string iscc = IsccBul();
            if (iscc == null) { Yaz(sb + "INNO SETUP YOK: Inno Setup 6 kurulunca bu menüyü yeniden çalıştır.\n"); return; }

            Yaz(sb + "HAZIRLANIYOR (birkaç dakika sürebilir)...\n");
            var psi = new ProcessStartInfo(iscc, "/Qp \"/DSurum=" + PlayerSettings.bundleVersion + "\" \"" + betik + "\"")
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            string rapor = Path.Combine(kok, Rapor);
            string bas = sb.ToString();
            calisiyor = true;
            var t = new System.Threading.Thread(() =>
            {
                try
                {
                    using (var p = Process.Start(psi))
                    {
                        var hata = new StringBuilder();
                        p.ErrorDataReceived += (s, e) => { if (e.Data != null) lock (hata) hata.AppendLine(e.Data); };
                        p.BeginErrorReadLine();
                        string cikti = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        string cikisKlasoru = Path.Combine(kok, "Build", "Kurulum");
                        string dosya = Path.Combine(cikisKlasoru, "HikayeStudyosu_Kurulum_" + Surum(psi.Arguments) + ".exe");
                        bool oldu = p.ExitCode == 0 && File.Exists(dosya);
                        File.WriteAllText(rapor, bas + DateTime.Now.ToString("HH:mm:ss") + (oldu ? " TAMAM " + dosya + " " + (new FileInfo(dosya).Length / 1048576) + " MB" : " HATA cikis=" + p.ExitCode) + "\n" + (oldu ? "" : cikti + hata));
                    }
                }
                catch (Exception e) { File.WriteAllText(rapor, bas + "HATA: " + e.Message + "\n"); }
                finally { calisiyor = false; }
            }) { IsBackground = true };
            t.Start();
        }

        static string Surum(string argumanlar)
        {
            int i = argumanlar.IndexOf("/DSurum=", StringComparison.Ordinal) + 8;
            return argumanlar.Substring(i, argumanlar.IndexOf('"', i) - i);
        }

        static void Yaz(string m)
        {
            File.WriteAllText(Rapor, m);
            Debug.Log("[Hikaye Stüdyosu] " + m);
        }
    }
}
