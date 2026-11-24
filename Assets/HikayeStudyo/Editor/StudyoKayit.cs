using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // "Videoya al": Play baslayinca Unity Recorder ile 1080x1920, 30 fps MP4 kaydeder, hikaye bitince durur.
    // Recorder paketine dogrudan bagli degildir (yansima ile calisir); paket yoksa kod yine derlenir.
    [InitializeOnLoad]
    public static class StudyoKayit
    {
        public const string Key = "HS_Kayit";
        public const string NameKey = "HS_KayitAdi";
        const string LastKey = "HS_SonVideo";
        static object controller;
        static MethodInfo stopMethod;

        static StudyoKayit()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        public static bool RecorderInstalled { get { return Type("UnityEditor.Recorder.RecorderController") != null; } }
        public static string LastVideo { get { return EditorPrefs.GetString(LastKey, ""); } }

        public static void InstallRecorder()
        {
            UnityEditor.PackageManager.Client.Add("com.unity.recorder");
            Debug.Log("[Hikaye Stüdyosu] Recorder paketi kuruluyor... Bitince 'Videoya al' kullanılabilir.");
        }

        static System.Type Type(string fullName)
        {
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
        }

        static void OnPlayMode(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
            {
                SessionState.SetBool(Key, false);
                if (StartRecording()) StoryPlayer.Finished += OnFinished;
                else { PlayerPrefs.DeleteKey("HS_Kapak"); PlayerPrefs.DeleteKey("HS_YayinKlasor"); }
            }
            if (s == PlayModeStateChange.ExitingPlayMode) StopRecording();
        }

        static double exitAt;

        static void OnFinished()
        {
            StoryPlayer.Finished -= OnFinished;
            StopRecording();
            // kapak ve aciklama yazilana kadar bekle (en fazla 6 sn), sonra oynatmayi kapat
            StoryPlayer.YayinHazir += OnYayin;
            exitAt = EditorApplication.timeSinceStartup + 6.0;
            EditorApplication.update += WaitExit;
        }

        static void OnYayin()
        {
            StoryPlayer.YayinHazir -= OnYayin;
            exitAt = 0;
        }

        static void WaitExit()
        {
            if (EditorApplication.timeSinceStartup < exitAt) return;
            EditorApplication.update -= WaitExit;
            StoryPlayer.YayinHazir -= OnYayin;
            EditorApplication.isPlaying = false;
            var dir = Path.GetDirectoryName(LastVideo);
            EditorApplication.delayCall += () => { if (Directory.Exists(dir)) EditorUtility.RevealInFinder(LastVideo); };
        }

        static void Set(object o, string name, object value)
        {
            var t = o.GetType();
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (p != null && p.CanWrite)
            {
                if (p.PropertyType.IsEnum && value is string) value = System.Enum.Parse(p.PropertyType, (string)value);
                p.SetValue(o, value, null);
                return;
            }
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (f != null)
            {
                if (f.FieldType.IsEnum && value is string) value = System.Enum.Parse(f.FieldType, (string)value);
                f.SetValue(o, value);
                return;
            }
            Debug.LogWarning("[Hikaye Stüdyosu] Recorder ayarı bulunamadı: " + t.Name + "." + name);
        }

        static object Get(object o, string name)
        {
            var p = o.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            return p != null ? p.GetValue(o, null) : null;
        }

        static bool StartRecording()
        {
            try
            {
                var tSettings = Type("UnityEditor.Recorder.RecorderControllerSettings");
                var tCtrl = Type("UnityEditor.Recorder.RecorderController");
                var tMovie = Type("UnityEditor.Recorder.MovieRecorderSettings");
                var tGame = Type("UnityEditor.Recorder.Input.GameViewInputSettings");
                if (tSettings == null || tCtrl == null || tMovie == null || tGame == null)
                {
                    Debug.LogWarning("[Hikaye Stüdyosu] Recorder paketi yok. Stüdyo penceresinden 'Recorder'ı kur'a bas.");
                    return false;
                }

                var settings = ScriptableObject.CreateInstance(tSettings);
                var movie = ScriptableObject.CreateInstance(tMovie);
                movie.name = "Hikaye";
                Set(movie, "Enabled", true);

                var tCore = Type("UnityEditor.Recorder.Encoder.CoreEncoderSettings");
                if (tCore != null)
                {
                    var enc = System.Activator.CreateInstance(tCore);
                    Set(enc, "Codec", "MP4");
                    Set(enc, "EncodingQuality", "High");
                    Set(movie, "EncoderSettings", enc);
                }
                else Set(movie, "OutputFormat", "MP4");

                var game = System.Activator.CreateInstance(tGame);
                Set(game, "OutputWidth", 1080);
                Set(game, "OutputHeight", 1920);
                Set(movie, "ImageInputSettings", game);
                var audio = Get(movie, "AudioInputSettings");
                if (audio != null) Set(audio, "PreserveAudio", true);

                string story = SessionState.GetString(NameKey, "hikaye");
                // Yayin klasoru: Videolar/Hikaye/<hikaye>_<tarih>/ icinde video.mp4, kapak.png, aciklama.txt
                string dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos), "Hikaye",
                                          story + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, story);
                PlayerPrefs.SetInt("HS_Kapak", 1);
                PlayerPrefs.SetString("HS_YayinKlasor", dir);
                PlayerPrefs.Save();
                Set(movie, "OutputFile", file.Replace('\\', '/'));
                EditorPrefs.SetString(LastKey, file + ".mp4");

                tSettings.GetMethod("AddRecorderSettings").Invoke(settings, new object[] { movie });
                tSettings.GetMethod("SetRecordModeToManual").Invoke(settings, null);
                Set(settings, "FrameRate", 30f);
                Set(settings, "CapFrameRate", true);
                var tPlayback = Type("UnityEditor.Recorder.FrameRatePlayback");
                if (tPlayback != null) Set(settings, "FrameRatePlayback", System.Enum.Parse(tPlayback, "Constant"));

                controller = System.Activator.CreateInstance(tCtrl, settings);
                tCtrl.GetMethod("PrepareRecording").Invoke(controller, null);
                var started = tCtrl.GetMethod("StartRecording").Invoke(controller, null);
                stopMethod = tCtrl.GetMethod("StopRecording");
                bool ok = !(started is bool) || (bool)started;
                Debug.Log(ok ? "[Hikaye Stüdyosu] Kayıt başladı: " + file + ".mp4" : "[Hikaye Stüdyosu] Kayıt başlatılamadı.");
                return ok;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Hikaye Stüdyosu] Kayıt başlatılamadı: " + (e.InnerException != null ? e.InnerException.Message : e.Message) +
                               "\nElle kayıt: Window > General > Recorder > Recorder Window (Movie, 1080x1920, 30 fps, MP4).");
                controller = null;
                return false;
            }
        }

        static void StopRecording()
        {
            if (controller == null) return;
            try
            {
                if (stopMethod != null) stopMethod.Invoke(controller, null);
                Debug.Log("[Hikaye Stüdyosu] Video kaydedildi: " + LastVideo);
            }
            catch (System.Exception e) { Debug.LogError("[Hikaye Stüdyosu] Kayıt durdurulamadı: " + e.Message); }
            controller = null;
        }
    }
}
