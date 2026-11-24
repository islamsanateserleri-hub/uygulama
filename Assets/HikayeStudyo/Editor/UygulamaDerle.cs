using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 0 denemesi: bağımsız Windows derlemesi alınabiliyor mu? Sonuç Build/derleme_sonuc.txt'ye yazılır.
    public static class UygulamaDerle
    {
        [MenuItem("Hikaye/Windows Derlemesi (Deneme)")]
        public static void Windows()
        {
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
    }
}
