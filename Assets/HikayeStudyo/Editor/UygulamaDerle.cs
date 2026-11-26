using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 0 denemesi: bağımsız Windows derlemesi alınabiliyor mu? Sonuç Build/derleme_sonuc.txt'ye yazılır.
    // surum: arayuz-2
    public static class UygulamaDerle
    {
        [MenuItem("Hikaye/Windows Derlemesi (Deneme)")]
        public static void Windows()
        {
            OncekileriKapat();
            OyuncuListesiYaz();
            UygulamaKimligi();
            string klasor = "Build/HikayeStudyo";
            Directory.CreateDirectory(klasor);
            string rapor = "Build/derleme_sonuc.txt";
            var sahneler = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
            if (sahneler.Count == 0)
            {
                foreach (var g in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
                { sahneler.Add(AssetDatabase.GUIDToAssetPath(g)); break; }
            }
            if (sahneler.Count == 0) { File.WriteAllText(rapor, "HATA: derlenecek sahne yok"); return; }

            var secenek = new BuildPlayerOptions
            {
                scenes = sahneler.ToArray(),
                locationPathName = klasor + "/HikayeStudyo.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport r;
            try { r = BuildPipeline.BuildPlayer(secenek); }
            catch (System.Exception e) { File.WriteAllText(rapor, "ISTISNA: " + e.Message); return; }
            var s = r.summary;
            File.WriteAllText(rapor, System.DateTime.Now.ToString("HH:mm:ss") + " " + s.result + " hata=" + s.totalErrors + " uyari=" + s.totalWarnings +
                " boyut=" + (s.totalSize / 1048576) + "MB sure=" + s.totalTime + " sahneler=" + string.Join(",", sahneler) + "\n");
            Debug.Log("[Hikaye Stüdyosu] Windows derlemesi: " + s.result + " (" + s.totalErrors + " hata)");
        }

        // Aşama 1 madde 12: bu bilgisayarda hangi H.264 kodlayıcıları çalışıyor? Sonuç: HikayeKomut/kodlayici_raporu.txt
        [MenuItem("Hikaye/Kodlayıcı Raporu")]
        public static void KodlayiciRaporu()
        {
            string ff = Path.GetFullPath("Assets/StreamingAssets/ffmpeg/ffmpeg.exe");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(System.DateTime.Now.ToString("HH:mm:ss") + " ffmpeg: " + ff);
            foreach (var enc in new[] { "h264_nvenc", "h264_amf", "h264_qsv", "h264_mf", "libx264", "libopenh264" })
            {
                string sonuc;
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(ff, "-hide_banner -loglevel error -f lavfi -i color=c=black:s=256x256:d=0.3:r=30 -c:v " + enc + " -pix_fmt yuv420p -f null -")
                    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
                    using (var pr = System.Diagnostics.Process.Start(psi))
                    {
                        string err = pr.StandardError.ReadToEnd();
                        if (!pr.WaitForExit(15000)) { try { pr.Kill(); } catch { } sonuc = "ZAMAN ASIMI"; }
                        else sonuc = pr.ExitCode == 0 ? "CALISIYOR" : "ÇALIŞMIYOR: " + err.Trim().Replace("\n", " | ");
                    }
                }
                catch (System.Exception e) { sonuc = "İSTİSNA: " + e.Message; }
                sb.AppendLine(enc + ": " + sonuc);
            }
            File.WriteAllText("HikayeKomut/kodlayici_raporu.txt", sb.ToString());
            Debug.Log("[Hikaye Stüdyosu] Kodlayıcı raporu yazıldı");
        }

        // Aşama 2: uygulama kimliği (pencere başlığı, simge, pencere boyutu). Exe Unity Editör'e bağlı değildir.
        static void UygulamaKimligi()
        {
            PlayerSettings.bundleVersion = "4.2.0";   // yeni icerik eklendikce arttirin (kurulum dosyasi adi)
            PlayerSettings.companyName = "HikayeStudyosu";
            PlayerSettings.productName = "Hikâye Stüdyosu";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;
            try
            {
                var tex = SimgeUret(256);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Standalone, new[] { tex }, UnityEditor.IconKind.Application);
            }
            catch (System.Exception e) { Debug.LogWarning("[Hikaye] Simge ayarlanamadı: " + e.Message); }
        }

        // Mavi yuvarlatılmış kare + beyaz "oynat" üçgeni
        static Texture2D SimgeUret(int n)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var px = new Color[n * n];
            float r = n * 0.22f;
            var mavi = new Color(0.35f, 0.55f, 1f, 1f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Max(r - (x + 0.5f), (x + 0.5f) - (n - r), 0f);
                    float dy = Mathf.Max(r - (y + 0.5f), (y + 0.5f) - (n - r), 0f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float a = Mathf.Clamp01(0.5f - d);
                    Color c = new Color(mavi.r, mavi.g, mavi.b, a);
                    // üçgen: x 0.38..0.72, merkez y 0.5
                    float u = (x + 0.5f) / n, v = (y + 0.5f) / n;
                    if (u > 0.38f && u < 0.72f && Mathf.Abs(v - 0.5f) < (0.72f - u) * 0.75f) c = new Color(1, 1, 1, a);
                    px[y * n + x] = c;
                }
            t.SetPixels(px); t.Apply();
            return t;
        }

        // Masaüstüne "Hikâye Stüdyosu" kısayolu koyar (Unity açmadan exe'yi çift tıklayarak başlatmak için)
        [MenuItem("Hikaye/Masaüstü Kısayolu Oluştur")]
        public static void MasaustuKisayolu()
        {
            string exe = Path.GetFullPath("Build/HikayeStudyo/HikayeStudyo.exe");
            string lnk = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory), "Hikaye Studyosu.lnk");
            string ps = "$s=(New-Object -ComObject WScript.Shell).CreateShortcut('" + lnk.Replace("'", "''") + "');" +
                        "$s.TargetPath='" + exe.Replace("'", "''") + "';$s.WorkingDirectory='" + Path.GetDirectoryName(exe).Replace("'", "''") + "';$s.Save()";
            var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe", "-NoProfile -Command \"" + ps.Replace("\"", "\\\"") + "\"")
            { UseShellExecute = false, CreateNoWindow = true };
            string sonuc = "Build/kisayol_sonuc.txt";
            try
            {
                var p = System.Diagnostics.Process.Start(psi);
                p.WaitForExit(20000);
                File.WriteAllText(sonuc, System.DateTime.Now.ToString("HH:mm:ss") + " cikis=" + p.ExitCode + " kisayol=" + (File.Exists(lnk) ? "VAR" : "YOK") + " " + lnk + "\n");
            }
            catch (System.Exception e) { File.WriteAllText(sonuc, "HATA: " + e.Message); }
        }

        // Önceki testten kalan (takılı) EXE ve ffmpeg süreçlerini kapatır (derleme dosyaları kilitli kalmasın)
        static void OncekileriKapat()
        {
            foreach (var ad in new[] { "HikayeStudyo", "ffmpeg" })
                foreach (var pr in System.Diagnostics.Process.GetProcessesByName(ad))
                { try { pr.Kill(); } catch { } }
        }

        // Karakterler/Hazir'daki 3D oyuncu prefablarını EXE'ye girecek Resources/OyuncuListesi.asset'e yazar
        static void OyuncuListesiYaz()
        {
            const string yol = "Assets/Resources/OyuncuListesi.asset";
            var liste = AssetDatabase.LoadAssetAtPath<OyuncuListesi>(yol);
            if (liste == null)
            {
                liste = ScriptableObject.CreateInstance<OyuncuListesi>();
                AssetDatabase.CreateAsset(liste, yol);
            }
            liste.prefablar.Clear();
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/HikayeStudyo/Karakterler/Hazir" }))
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                if (pf != null) liste.prefablar.Add(pf);
            }
            EditorUtility.SetDirty(liste);
            AssetDatabase.SaveAssets();
            Debug.Log("[Hikaye Stüdyosu] OyuncuListesi: " + liste.prefablar.Count + " prefab");
        }

        // Aşama 2: ana ekranı (argümansız EXE) test etmek için.
        [MenuItem("Hikaye/Exe Arayüz Testi (derle + aç)")]
        public static void ExeArayuzTesti()
        {
            Windows();
            string rapor = "Build/derleme_sonuc.txt";
            string d = File.Exists(rapor) ? File.ReadAllText(rapor) : "";
            if (!d.Contains("Succeeded")) { File.WriteAllText("Build/exe_arayuz_sonuc.txt", "DERLEME BASARISIZ: " + d); return; }
            ExeArayuzAc();
        }

        [MenuItem("Hikaye/Exe Arayüz Testi (yalnız aç)")]
        public static void ExeArayuzAc()
        {
            OncekileriKapat();
            string kok = Directory.GetCurrentDirectory();
            string klasor = Path.Combine(kok, "Build", "HikayeStudyo");
            string exe = Path.Combine(klasor, "HikayeStudyo.exe");
            string log = Path.Combine(kok, "Build", "exe_arayuz_log.txt");
            string sonucYol = Path.Combine(kok, "Build", "exe_arayuz_sonuc.txt");
            if (!File.Exists(exe)) { File.WriteAllText(sonucYol, "HATA: exe yok: " + exe); return; }
            var psi = new System.Diagnostics.ProcessStartInfo(exe,
                "-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile \"" + log + "\"")
            { WorkingDirectory = klasor, UseShellExecute = false };
            try { System.Diagnostics.Process.Start(psi); File.WriteAllText(sonucYol, System.DateTime.Now.ToString("HH:mm:ss") + " EXE arayuz acildi\n"); }
            catch (System.Exception e) { File.WriteAllText(sonucYol, "HATA: baslatilamadi: " + e.Message); }
        }

        // Aşama 1 madde 10: derlenen EXE'yi Unity Editör olmadan çalıştırıp klaketli video kaydeder.
        // Sonuç: Build/exe_test_sonuc.txt (çıkış kodu, mp4 boyutu, ffkayit_sonuc.txt içeriği), günlük: Build/exe_log.txt
        [MenuItem("Hikaye/Exe Kayıt Testi (derle + kaydet)")]
        public static void ExeKayitTesti()
        {
            Windows();
            string rapor = "Build/derleme_sonuc.txt";
            string d = File.Exists(rapor) ? File.ReadAllText(rapor) : "";
            if (!d.Contains("Succeeded")) { File.WriteAllText("Build/exe_test_sonuc.txt", "DERLEME BASARISIZ: " + d); return; }
            ExeKayitBaslat();
        }

        [MenuItem("Hikaye/Exe Kayıt Testi (yalnız çalıştır)")]
        public static void ExeKayitBaslat()
        {
            OncekileriKapat();
            string kok = Directory.GetCurrentDirectory();
            string klasor = Path.Combine(kok, "Build", "HikayeStudyo");
            string exe = Path.Combine(klasor, "HikayeStudyo.exe");
            // Başka hikaye denemek için: HikayeKomut/exe_hikaye.txt içine hikaye adını yaz (yoksa deneme_3d)
            string hikaye = "deneme_3d";
            string secim = Path.Combine(kok, "HikayeKomut", "exe_hikaye.txt");
            if (File.Exists(secim))
            {
                string ad = File.ReadAllText(secim).Trim();
                if (System.Text.RegularExpressions.Regex.IsMatch(ad, "^[A-Za-z0-9_-]+$")) hikaye = ad;
            }
            string mp4 = Path.Combine(kok, "Kayitlar", "exe_" + hikaye + ".mp4");
            string log = Path.Combine(kok, "Build", "exe_log.txt");
            string sonucYol = Path.Combine(kok, "Build", "exe_test_sonuc.txt");
            if (!File.Exists(exe)) { File.WriteAllText(sonucYol, "HATA: exe yok: " + exe); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(mp4));
            if (File.Exists(mp4)) File.Delete(mp4);
            string ks = Path.Combine(klasor, "ffkayit_sonuc.txt");
            if (File.Exists(ks)) File.Delete(ks);
            File.WriteAllText(sonucYol, System.DateTime.Now.ToString("HH:mm:ss") + " EXE basladi\n");
            var psi = new System.Diagnostics.ProcessStartInfo(exe,
                "-screen-fullscreen 0 -screen-width 540 -screen-height 960 -logFile \"" + log + "\" -kayit \"" + mp4 + "\" -hikaye " + hikaye + " -klaket")
            { WorkingDirectory = klasor, UseShellExecute = false };
            System.Diagnostics.Process p;
            try { p = System.Diagnostics.Process.Start(psi); }
            catch (System.Exception e) { File.WriteAllText(sonucYol, "HATA: baslatilamadi: " + e.Message); return; }
            var t = new System.Threading.Thread(() =>
            {
                bool bitti = p.WaitForExit(600000);
                string s = System.DateTime.Now.ToString("HH:mm:ss") + (bitti ? " EXE bitti, cikis=" + p.ExitCode : " ZAMAN ASIMI (10 dk), EXE hala calisiyor") + "\n";
                s += "mp4=" + (File.Exists(mp4) ? new FileInfo(mp4).Length + " bayt" : "YOK") + "\n";
                s += "ffkayit_sonuc.txt: " + (File.Exists(ks) ? File.ReadAllText(ks) : "YOK") + "\n";
                File.WriteAllText(sonucYol, s);
            }) { IsBackground = true };
            t.Start();
        }
    }
}
