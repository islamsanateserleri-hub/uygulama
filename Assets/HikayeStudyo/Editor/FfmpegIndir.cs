using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // LGPL ffmpeg derlemesini (BtbN) indirip Assets/StreamingAssets/ffmpeg/ içine koyar.
    public static class FfmpegIndir
    {
        const string Url = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-lgpl.zip";
        static UnityWebRequest istek;
        static string zip;

        [MenuItem("Hikaye/ffmpeg İndir")]
        public static void Indir()
        {
            if (istek != null) { Debug.Log("[ffmpeg] zaten iniyor"); return; }
            string hedef = Path.Combine(Application.dataPath, "StreamingAssets", "ffmpeg");
            if (File.Exists(Path.Combine(hedef, "ffmpeg.exe"))) { Rapor("ZATEN VAR: " + Path.Combine(hedef, "ffmpeg.exe")); return; }
            Directory.CreateDirectory(hedef);
            zip = Path.Combine(Path.GetTempPath(), "ffmpeg_lgpl.zip");
            istek = new UnityWebRequest(Url, "GET") { downloadHandler = new DownloadHandlerFile(zip) { removeFileOnAbort = true }, timeout = 0 };
            istek.SendWebRequest();
            EditorApplication.update += Izle;
            Debug.Log("[ffmpeg] indirme başladı: " + Url);
        }

        static double sonLog;
        static void Izle()
        {
            if (istek == null) { EditorApplication.update -= Izle; return; }
            if (!istek.isDone)
            {
                if (EditorApplication.timeSinceStartup - sonLog > 10) { sonLog = EditorApplication.timeSinceStartup; Debug.Log("[ffmpeg] indiriliyor %" + Mathf.RoundToInt(istek.downloadProgress * 100)); }
                return;
            }
            EditorApplication.update -= Izle;
            bool ok = istek.result == UnityWebRequest.Result.Success;
            string err = istek.error; istek.Dispose(); istek = null;
            if (!ok) { Rapor("HATA indirme: " + err); return; }
            try
            {
                string hedef = Path.Combine(Application.dataPath, "StreamingAssets", "ffmpeg");
                int n = 0;
                using (var z = ZipFile.OpenRead(zip))
                    foreach (var e in z.Entries)
                    {
                        string ad = e.FullName.Replace('\\', '/');
                        string kisa = Path.GetFileName(ad);
                        bool al = (ad.Contains("/bin/") && (kisa == "ffmpeg.exe" || kisa == "ffprobe.exe")) || kisa == "LICENSE.txt";
                        if (!al || string.IsNullOrEmpty(kisa)) continue;
                        e.ExtractToFile(Path.Combine(hedef, kisa), true); n++;
                    }
                File.Delete(zip);
                AssetDatabase.Refresh();
                Rapor("TAMAM: " + n + " dosya → " + hedef);
            }
            catch (Exception ex) { Rapor("HATA açma: " + ex.Message); }
        }

        static void Rapor(string m)
        {
            Debug.Log("[ffmpeg] " + m);
            try { File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut", "ffmpeg_indir.txt"), DateTime.Now.ToString("HH:mm:ss") + " " + m); } catch { }
        }
    }
}
