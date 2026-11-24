using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace HikayeStudyo
{
    // Proje klasorundeki HikayeKomut/ klasoru uzerinden Unity'ye komut verilir ve sonuc okunur
    // (ekran gorulemezken de calismayi takip etmek icin).
    //   komut.txt  : her satir bir komut (islenince silinir)
    //       yenile                     -> dosyalari yeniden tara / derle
    //       menu: Hikaye/Karakter Galerisi
    //       oynat: deneme_3d [satir]   -> hikayeyi onizle
    //       kaydet: deneme_3d          -> hikayeyi videoya al
    //       dur                        -> oynatmayi durdur
    //       ekran                      -> Game goruntusunu HikayeKomut/ekran.png olarak kaydet
    //   konsol.txt : Unity konsolunun kopyasi (uyari/hata)
    //   derleme.txt: son derlemenin hata/uyari listesi
    //   sonuc.txt  : islenen komutlarin sonucu
    [InitializeOnLoad]
    public static class UzaktanKomut
    {
        public static readonly string Dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "HikayeKomut"));
        static double next;
        static readonly List<string> compileLines = new List<string>();

        static UzaktanKomut()
        {
            Directory.CreateDirectory(Dir);
            Application.logMessageReceivedThreaded += OnLog;
            CompilationPipeline.compilationStarted += o => { lock (compileLines) compileLines.Clear(); };
            CompilationPipeline.assemblyCompilationFinished += OnAssembly;
            CompilationPipeline.compilationFinished += o => WriteCompile();
            EditorApplication.update += Poll;
            Append("sonuc.txt", Stamp() + " Unity hazır (derleme yüklendi).");
        }

        static string Stamp() { return System.DateTime.Now.ToString("HH:mm:ss"); }

        static void Append(string file, string line)
        {
            try
            {
                var p = Path.Combine(Dir, file);
                if (File.Exists(p) && new FileInfo(p).Length > 400000) File.WriteAllText(p, "");
                File.AppendAllText(p, line + "\n", new UTF8Encoding(false));
            }
            catch { }
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            string t = type == LogType.Log ? "BİLGİ" : type == LogType.Warning ? "UYARI" : "HATA";
            string s = Stamp() + " [" + t + "] " + msg;
            if (type == LogType.Exception || type == LogType.Error)
            {
                var st = (stack ?? "").Split('\n');
                for (int i = 0; i < st.Length && i < 4; i++) if (st[i].Trim().Length > 0) s += "\n      " + st[i].Trim();
            }
            Append("konsol.txt", s);
        }

        static void OnAssembly(string asm, CompilerMessage[] msgs)
        {
            lock (compileLines)
                foreach (var m in msgs)
                    compileLines.Add((m.type == CompilerMessageType.Error ? "HATA " : "UYARI ") + m.file + "(" + m.line + "): " + m.message);
        }

        static void WriteCompile()
        {
            int err = 0;
            var sb = new StringBuilder();
            lock (compileLines)
                foreach (var l in compileLines) { if (l.StartsWith("HATA")) err++; sb.AppendLine(l); }
            try { File.WriteAllText(Path.Combine(Dir, "derleme.txt"), Stamp() + " Derleme bitti: " + err + " hata\n" + sb, new UTF8Encoding(false)); }
            catch { }
        }

        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + 1.5;
            var p = Path.Combine(Dir, "komut.txt");
            if (!File.Exists(p)) return;
            string[] lines;
            try { lines = File.ReadAllLines(p); File.Delete(p); } catch { return; }
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;
                string res;
                try { res = Run(line); }
                catch (System.Exception e) { res = "HATA: " + e.Message; }
                Append("sonuc.txt", Stamp() + " " + line + " -> " + res);
            }
        }

        static string Run(string line)
        {
            int c = line.IndexOf(':');
            string cmd = (c < 0 ? line : line.Substring(0, c)).Trim().ToLowerInvariant();
            string arg = c < 0 ? "" : line.Substring(c + 1).Trim();
            switch (cmd)
            {
                case "yenile":
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return "tamam";
                case "menu":
                    return EditorApplication.ExecuteMenuItem(arg) ? "tamam" : "menü bulunamadı";
                case "oynat":
                case "kaydet":
                    {
                        var parts = arg.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) return "hikaye adı yok";
                        int ln = 0;
                        if (parts.Length > 1) int.TryParse(parts[1], out ln);
                        return StudyoWindow.PlayByName(parts[0], ln, cmd == "kaydet");
                    }
                case "dur":
                    EditorApplication.isPlaying = false;
                    return "tamam";
                case "ekran":
                    {
                        var f = Path.Combine(Dir, "ekran.png");
                        ScreenCapture.CaptureScreenshot(f);
                        return EditorApplication.isPlaying ? "kaydediliyor: " + f : "oynatma kapalı (Game görüntüsü olmayabilir)";
                    }
            }
            return "bilinmeyen komut";
        }
    }
}
