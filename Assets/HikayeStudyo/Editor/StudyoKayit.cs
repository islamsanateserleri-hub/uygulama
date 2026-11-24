using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // "Videoya al": Play baslayinca Unity Recorder ile 1080x1920, 30 fps MP4 kaydeder, hikaye bitince durur.
    // Recorder paketine dogrudan bagli degildir (yansima ile calisir); paket yoksa kod yine derlenir.
    //
    // Guvenilirlik:
    //  - Kayit oncesi kontrol: stereo ses cikisi, ses kapali/mute degil, Game penceresi gorunur ve 1080x1920,
    //    tek recorder, 30 fps sabit; TikTok guvenli alan rehberi kayitta gizlenir.
    //  - Kayit sonrasi denetim: dosya var mi, boyutu makul mu, boyut sabitlendi mi; sonuc konsola ve EditorPrefs'e yazilir.
    [InitializeOnLoad]
    public static class StudyoKayit
    {
        public const string Key = "HS_Kayit";
        public const string NameKey = "HS_KayitAdi";
        const string LastKey = "HS_SonVideo";
        const string RaporKey = "HS_SonKayitRapor";
        const string OkKey = "HS_SonKayitOk";
        const string DenetimKey = "HS_KayitDenetim";
        const string DenetimDosyaKey = "HS_KayitDenetimDosya";
        const string DenetimZamanKey = "HS_KayitDenetimT";
        const long MinBoyut = 150 * 1024;   // bunun altindaki mp4 buyuk ihtimalle bozuk/bos

        static object controller;
        static MethodInfo stopMethod;
        static double exitAt;
        static double guvenliBitis;
        static double sonrakiDenetim;
        static long sonBoyut = -2;
        static int kararli;

        static StudyoKayit()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
            if (SessionState.GetBool(DenetimKey, false)) DenetimBaslat();
        }

        public static bool RecorderInstalled { get { return Type("UnityEditor.Recorder.RecorderController") != null; } }
        public static string LastVideo { get { return EditorPrefs.GetString(LastKey, ""); } }
        // Son kayit denetiminin metni ve sonucu (Studyo penceresi gosterebilir)
        public static string SonRapor { get { return EditorPrefs.GetString(RaporKey, ""); } }
        public static bool SonKayitTamam { get { return EditorPrefs.GetBool(OkKey, false); } }

        public static void InstallRecorder()
        {
            UnityEditor.PackageManager.Client.Add("com.unity.recorder");
            Debug.Log("[Hikaye Stüdyosu] Recorder paketi kuruluyor... Bitince 'Videoya al' kullanılabilir.");
        }

        static System.Type Type(string fullName)
        {
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = a.GetType(fullName, false);
                    if (t != null) return t;
                }
                catch (System.Exception) { }
            }
            return null;
        }

        // ---------- Play modu olaylari ----------
        static void OnPlayMode(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.ExitingEditMode && SessionState.GetBool(Key, false))
                KayitOncesiDuzenle();

            if (s == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key, false))
            {
                SessionState.SetBool(Key, false);
                GuvenliAlanKapat();
                if (PlayKontrol() && StartRecording()) StoryPlayer.Finished += OnFinished;
                else { PlayerPrefs.DeleteKey("HS_Kapak"); PlayerPrefs.DeleteKey("HS_YayinKlasor"); }
            }
            if (s == PlayModeStateChange.ExitingPlayMode) StopRecording();
            if (s == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(DenetimKey, false)) DenetimBaslat();
        }

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
            EditorApplication.delayCall += () => { if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) EditorUtility.RevealInFinder(LastVideo); };
        }

        // ---------- Kayit oncesi: Edit modunda duzeltilebilenler ----------
        static void KayitOncesiDuzenle()
        {
            var log = new StringBuilder();
            // guvenli alan rehberi videoya girmesin
            PlayerPrefs.SetInt("HS_SafeArea", 0);
            PlayerPrefs.Save();

            // ses: Default Speaker Mode = Stereo, ses kapali degil, Editor mute degil
            try
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset");
                if (assets != null && assets.Length > 0 && assets[0] != null)
                {
                    var so = new SerializedObject(assets[0]);
                    var sm = so.FindProperty("m_DefaultSpeakerMode");
                    if (sm != null && sm.intValue != 2)
                    {
                        sm.intValue = 2;
                        so.ApplyModifiedProperties();
                        log.Append("hoparlör modu Stereo yapıldı; ");
                    }
                    var da = so.FindProperty("m_DisableAudio");
                    if (da != null && da.boolValue) log.Append("UYARI: Project Settings > Audio > Disable Unity Audio açık (video sessiz olur); ");
                }
            }
            catch (System.Exception) { }
            try
            {
                var pi = typeof(EditorUtility).GetProperty("audioMasterMute", BindingFlags.Public | BindingFlags.Static);
                if (pi != null && pi.PropertyType == typeof(bool) && (bool)pi.GetValue(null, null))
                {
                    pi.SetValue(null, false, null);
                    log.Append("Editor sesi kapalıydı, açıldı; ");
                }
            }
            catch (System.Exception) { }

            // Game penceresi: 1080x1920 ve one alinmis olsun (arkada kalan sekme render edilmez, kayit donar)
            try
            {
                GameViewUtil.Portrait();
                var gv = Type("UnityEditor.GameView");
                if (gv != null) EditorWindow.GetWindow(gv, false, null, true);
            }
            catch (System.Exception) { }

            if (RecorderPenceresiKaydediyor()) log.Append("UYARI: Recorder penceresi zaten kayıt yapıyor; çift kayıt olmaması için durdur; ");
            if (log.Length > 0) Debug.Log("[Hikaye Stüdyosu] Kayıt öncesi: " + log);
        }

        // ---------- Kayit oncesi: Play modunda dogrulananlar ----------
        static bool PlayKontrol()
        {
            var log = new StringBuilder();
            bool engel = false;
            if (Screen.width != 1080 || Screen.height != 1920)
                log.Append("UYARI: Game görüntüsü " + Screen.width + "x" + Screen.height + " (1080x1920 olmalı; Game penceresindeki boyut menüsünden 'TikTok 1080x1920' seç); ");
            if (AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
                log.Append("UYARI: ses çıkışı " + AudioSettings.speakerMode + " (Stereo olmalı; Edit > Project Settings > Audio); ");
            if (RecorderPenceresiKaydediyor())
            {
                log.Append("HATA: Recorder penceresi zaten kayıt yapıyor, ikinci kayıt başlatılmadı; ");
                engel = true;
            }
            if (controller != null)
            {
                log.Append("önceki kayıt denetleyicisi kapatıldı; ");
                StopRecording();
            }
            if (log.Length > 0)
            {
                if (engel) Debug.LogError("[Hikaye Stüdyosu] Kayıt öncesi kontrol: " + log);
                else Debug.LogWarning("[Hikaye Stüdyosu] Kayıt öncesi kontrol: " + log);
            }
            return !engel;
        }

        static bool RecorderPenceresiKaydediyor()
        {
            try
            {
                var tw = Type("UnityEditor.Recorder.RecorderWindow");
                if (tw == null) return false;
                var wins = Resources.FindObjectsOfTypeAll(tw);
                foreach (var w in wins)
                {
                    var m = tw.GetMethod("IsRecording", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    if (m == null || m.GetParameters().Length != 0) continue;
                    object o = m.Invoke(m.IsStatic ? null : (object)w, null);
                    if (o is bool && (bool)o) return true;
                }
            }
            catch (System.Exception) { }
            return false;
        }

        // Guvenli alan rehberi sadece onizleme icindir: kayitta gizle (StoryPlayer olusturduysa kaldir)
        static void GuvenliAlanKapat()
        {
            PlayerPrefs.SetInt("HS_SafeArea", 0);
            guvenliBitis = EditorApplication.timeSinceStartup + 3.0;
            EditorApplication.update -= GuvenliTick;
            EditorApplication.update += GuvenliTick;
        }

        static void GuvenliTick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > guvenliBitis)
            {
                EditorApplication.update -= GuvenliTick;
                return;
            }
            var go = GameObject.Find("GuvenliAlan");
            if (go != null) Object.Destroy(go);
        }

        // ---------- Recorder ayarlari ----------
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
                EditorPrefs.SetString(RaporKey, "");
                EditorPrefs.SetBool(OkKey, false);

                tSettings.GetMethod("AddRecorderSettings").Invoke(settings, new object[] { movie });
                tSettings.GetMethod("SetRecordModeToManual").Invoke(settings, null);
                Set(settings, "FrameRate", 30f);
                Set(settings, "CapFrameRate", true);
                var tPlayback = Type("UnityEditor.Recorder.FrameRatePlayback");
                if (tPlayback != null) Set(settings, "FrameRatePlayback", System.Enum.Parse(tPlayback, "Constant"));

                // dogrulama: tek recorder, 30 fps sabit
                string kontrol = "";
                try
                {
                    int adet = 0;
                    var list = Get(settings, "RecorderSettings") as System.Collections.IEnumerable;
                    if (list != null) foreach (var x in list) adet++;
                    object fr = Get(settings, "FrameRate");
                    object pb = Get(settings, "FrameRatePlayback");
                    kontrol = "recorder=" + adet + ", fps=" + fr + ", oynatma=" + pb;
                    if (adet != 1) Debug.LogWarning("[Hikaye Stüdyosu] Beklenmeyen recorder sayısı: " + adet + " (1 olmalı).");
                    if (fr is float && Mathf.Abs((float)fr - 30f) > 0.01f) Debug.LogWarning("[Hikaye Stüdyosu] Kare hızı 30 değil: " + fr);
                }
                catch (System.Exception) { }

                controller = null;
                var ctors = tCtrl.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var c in ctors)
                {
                    var ps = c.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(tSettings)) { controller = c.Invoke(new object[] { settings }); break; }
                }
                if (controller == null)
                {
                    var sigs = new StringBuilder();
                    foreach (var c in ctors) { sigs.Append("("); foreach (var pr in c.GetParameters()) sigs.Append(pr.ParameterType.Name + " "); sigs.Append(") "); }
                    throw new System.Exception("RecorderController kurucusu bulunamadı: " + sigs);
                }
                tCtrl.GetMethod("PrepareRecording").Invoke(controller, null);
                var started = tCtrl.GetMethod("StartRecording").Invoke(controller, null);
                stopMethod = tCtrl.GetMethod("StopRecording");
                bool ok = !(started is bool) || (bool)started;
                Debug.Log(ok ? "[Hikaye Stüdyosu] Kayıt başladı: " + file + ".mp4  (" + kontrol + ", captureFramerate=" + Time.captureFramerate + ")"
                             : "[Hikaye Stüdyosu] Kayıt başlatılamadı.");
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
                Debug.Log("[Hikaye Stüdyosu] Kayıt durduruldu, dosya denetleniyor: " + LastVideo);
            }
            catch (System.Exception e) { Debug.LogError("[Hikaye Stüdyosu] Kayıt durdurulamadı: " + e.Message); }
            controller = null;
            // play modundan cikinca domain reload olabilir: denetim SessionState ile surer
            SessionState.SetString(DenetimDosyaKey, LastVideo);
            SessionState.SetString(DenetimZamanKey, EditorApplication.timeSinceStartup.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            SessionState.SetBool(DenetimKey, true);
        }

        // ---------- Kayit sonrasi denetim ----------
        static void DenetimBaslat()
        {
            sonBoyut = -2; kararli = 0; sonrakiDenetim = 0;
            EditorApplication.update -= DenetimTick;
            EditorApplication.update += DenetimTick;
        }

        static void DenetimTick()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (EditorApplication.timeSinceStartup < sonrakiDenetim) return;
            sonrakiDenetim = EditorApplication.timeSinceStartup + 0.6;

            string f = SessionState.GetString(DenetimDosyaKey, "");
            double t0;
            if (!double.TryParse(SessionState.GetString(DenetimZamanKey, "0"), System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out t0)) t0 = 0;
            long size = -1;
            try { if (!string.IsNullOrEmpty(f) && File.Exists(f)) size = new FileInfo(f).Length; } catch (System.Exception) { size = -1; }
            if (size > 0 && size == sonBoyut) kararli++; else kararli = 0;
            sonBoyut = size;
            bool zamanAsimi = EditorApplication.timeSinceStartup - t0 > 45.0;
            if (kararli >= 3 || zamanAsimi) DenetimBitir(f, size, zamanAsimi);
        }

        static void DenetimBitir(string f, long size, bool zamanAsimi)
        {
            EditorApplication.update -= DenetimTick;
            SessionState.SetBool(DenetimKey, false);
            var sb = new StringBuilder();
            bool ok;
            if (size < 0)
            {
                ok = false;
                sb.Append("HATA: video dosyası yok: " + f);
            }
            else if (size < MinBoyut)
            {
                ok = false;
                sb.Append("HATA: video çok küçük (" + Boyut(size) + "), bozuk olabilir: " + f);
            }
            else
            {
                ok = true;
                sb.Append("Video tamam: " + Path.GetFileName(f) + " (" + Boyut(size) + ")");
                if (zamanAsimi) sb.Append(" [dosya hâlâ büyüyordu, yine de denetlendi]");
            }
            try
            {
                string dir = Path.GetDirectoryName(f);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    bool kapak = File.Exists(Path.Combine(dir, "kapak.png"));
                    bool acik = File.Exists(Path.Combine(dir, "aciklama.txt"));
                    sb.Append("; kapak.png " + (kapak ? "var" : "YOK") + ", aciklama.txt " + (acik ? "var" : "YOK"));
                    if (!kapak || !acik) ok = false;
                }
            }
            catch (System.Exception) { }
            string rapor = sb.ToString();
            EditorPrefs.SetString(RaporKey, rapor);
            EditorPrefs.SetBool(OkKey, ok);
            if (ok) Debug.Log("[Hikaye Stüdyosu] Kayıt denetimi: " + rapor);
            else Debug.LogError("[Hikaye Stüdyosu] Kayıt denetimi: " + rapor);
        }

        static string Boyut(long b)
        {
            if (b >= 1024 * 1024) return (b / (1024.0 * 1024.0)).ToString("0.0") + " MB";
            return (b / 1024.0).ToString("0") + " KB";
        }
    }
}
