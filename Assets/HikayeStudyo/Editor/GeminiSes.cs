using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // Hikayedeki satirlari Gemini TTS ile seslendirir; her satiri karakterin sabit sesiyle ve duygusuna uygun tonla.
    // Dosyalar: Assets/Resources/Ses/<hikaye>/<ozet>.wav  (ayni satir bir daha uretilmez, kota bosa gitmez)
    // Okunacak metin once TurkceMetin ile normalize edilir (1993 -> "bin dokuz yuz doksan uc"); altyazi orijinali gosterir.
    public static class GeminiSes
    {
        public const string KeyPref = "HS_GeminiKey";
        public const string ModelPref = "HS_GeminiModel";
        public const string DefaultModel = "gemini-3.8-flash-tts";
        public const string OldModel = "gemini-2.5-flash-preview-tts";
        public static readonly string[] Models = { "gemini-3.8-flash-tts", "gemini-3.8-flash-lite-tts", "gemini-2.5-flash-preview-tts" };
        public static readonly string[] ModelLabels = { "Gemini 3.8 Flash TTS (en iyi)", "Gemini 3.8 Flash-Lite TTS (ucuz, hızlı)", "Gemini 2.5 (eski)" };

        // Kesik ses denetimi: beklenen sureden bu oranin altindaysa yeniden uretilir
        public const float KisaOran = 0.70f;
        public const int KisaTekrarVarsayilan = 3;
        const string KisaTekrarPref = "HS_KisaTekrar";
        public const int StilKelimeSiniri = 6;

        public static string Model
        {
            get
            {
                var m = EditorPrefs.GetString(ModelPref, DefaultModel).Trim();
                if (m.Length == 0 || m == OldModel && !EditorPrefs.GetBool("HS_ModelSecildi", false)) m = DefaultModel;   // eski varsayilandan yeniye gec
                return m;
            }
            set { EditorPrefs.SetString(ModelPref, value); EditorPrefs.SetBool("HS_ModelSecildi", true); }
        }

        public static int KisaTekrar
        {
            get { return Mathf.Clamp(EditorPrefs.GetInt(KisaTekrarPref, KisaTekrarVarsayilan), 0, 5); }
            set { EditorPrefs.SetInt(KisaTekrarPref, Mathf.Clamp(value, 0, 5)); }
        }

        static bool NewSchema(string model) { return !model.StartsWith("gemini-2"); }

        public static bool Busy { get; private set; }
        public static string Status = "";
        public static event System.Action Changed;
        public static event System.Action<string> Bitti;      // uretim bitince (ozet metin)
        static bool stopRequested;

        // Son uretimin uyari raporu (kesik ses, tekrar sayisi...)
        public static readonly List<string> Rapor = new List<string>();

        static readonly string[] Exts = { ".wav", ".mp3", ".ogg" };

        public static string Dir(string story) { return "Assets/Resources/Ses/" + story; }

        public static bool HasClip(string story, StoryLibrary lib, StoryItem it)
        {
            string d = Dir(story) + "/";
            string k = lib.ClipKey(it), n = it.VoiceIndex.ToString("00");
            foreach (var ext in Exts)
                if (File.Exists(d + k + ext) || File.Exists(d + n + ext)) return true;
            return false;
        }

        // Sesi olmayan konusma satirlari (kayit oncesi denetim icin)
        public static List<StoryItem> Eksikler(StoryLibrary lib, string story, IList<StoryItem> items)
        {
            var l = new List<StoryItem>();
            if (lib == null || items == null) return l;
            foreach (var it in items) if (it.Type == ItemType.Dialogue && !HasClip(story, lib, it)) l.Add(it);
            return l;
        }

        public static void Stop() { stopRequested = true; Status = "Durduruluyor..."; Notify(); }

        static void Notify() { if (Changed != null) Changed(); }

        // ---------- Kota sayaci (Gemini ucretsiz kota Pasifik saatine gore gunluk) ----------
        public const int KotaLimit = 10;
        const string KotaGunKey = "HS_KotaGun", KotaSayiKey = "HS_KotaSayi", KotaDolduKey = "HS_KotaDoldu";

        static string KotaGunu()
        {
            var now = System.DateTime.UtcNow;
            string[] ids = { "America/Los_Angeles", "Pacific Standard Time" };
            foreach (var id in ids)
            {
                try
                {
                    var tz = System.TimeZoneInfo.FindSystemTimeZoneById(id);
                    return System.TimeZoneInfo.ConvertTimeFromUtc(now, tz).ToString("yyyyMMdd");
                }
                catch { }
            }
            return now.AddHours(-8).ToString("yyyyMMdd");
        }

        public static int KotaBugun()
        {
            return EditorPrefs.GetString(KotaGunKey, "") == KotaGunu() ? EditorPrefs.GetInt(KotaSayiKey, 0) : 0;
        }

        public static void KotaArtir()
        {
            string g = KotaGunu();
            int n = EditorPrefs.GetString(KotaGunKey, "") == g ? EditorPrefs.GetInt(KotaSayiKey, 0) : 0;
            EditorPrefs.SetString(KotaGunKey, g);
            EditorPrefs.SetInt(KotaSayiKey, n + 1);
        }

        static void KotaDoldu() { EditorPrefs.SetString(KotaDolduKey, KotaGunu()); }

        public static bool KotaDolduMu() { return EditorPrefs.GetString(KotaDolduKey, "") == KotaGunu(); }

        public static string KotaMetni()
        {
            int n = KotaBugun();
            string s = "Bugün ~" + n + "/" + KotaLimit + " ses isteği (yeniden denemeler ve varyantlar da sayılır)";
            if (KotaDolduMu()) s += "  ·  Günlük kota doldu: yarın ~11:00'den sonra";
            else if (n >= KotaLimit) s += "  ·  Kota dolmak üzere";
            return s;
        }

        public static int KotaKalan() { return Mathf.Max(0, KotaLimit - KotaBugun()); }

        // ---------- Stil metni ----------
        public static int StilKelime(string tone)
        {
            return string.IsNullOrEmpty(tone) ? 0 : tone.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        // Stil ~6 kelimeyi gecerse Gemini'ye kisaltilmis hali gider (dosya adi/onbellek anahtari degismez)
        public static string StilKisalt(string tone)
        {
            if (string.IsNullOrEmpty(tone)) return "doğal";
            string t = tone.Replace("iç ses gibi sakin ve derin", "sakin iç ses");
            var w = t.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (w.Length <= StilKelimeSiniri) return t;
            var sb = new StringBuilder();
            for (int i = 0; i < StilKelimeSiniri; i++) { if (i > 0) sb.Append(' '); sb.Append(w[i]); }
            return sb.ToString().TrimEnd(',', ';');
        }

        // ---------- Klip suresi (gercek ses suresiyle sure tahmini) ----------
        struct SureKaydi { public long ticks; public float sure; }
        static readonly Dictionary<string, SureKaydi> sureOnbellek = new Dictionary<string, SureKaydi>();

        public static void SureOnbellegiTemizle() { sureOnbellek.Clear(); }

        // Satirin sesi varsa suresi (sn), yoksa -1
        public static float ClipSeconds(string story, StoryLibrary lib, StoryItem it)
        {
            if (lib == null || it == null || string.IsNullOrEmpty(story)) return -1f;
            string d = Dir(story) + "/";
            string k = lib.ClipKey(it), n = it.VoiceIndex.ToString("00");
            foreach (var ext in Exts)
            {
                string p1 = d + k + ext;
                if (File.Exists(p1)) return DosyaSuresi(p1);
                string p2 = d + n + ext;
                if (File.Exists(p2)) return DosyaSuresi(p2);
            }
            return -1f;
        }

        static float DosyaSuresi(string path)
        {
            long ticks;
            try { ticks = File.GetLastWriteTimeUtc(path).Ticks; }
            catch { return -1f; }
            SureKaydi kayit;
            if (sureOnbellek.TryGetValue(path, out kayit) && kayit.ticks == ticks) return kayit.sure;
            float s = -1f;
            if (path.EndsWith(".wav")) s = WavSuresi(path);
            if (s < 0f)
            {
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c != null) s = c.length;
            }
            if (s >= 0f) sureOnbellek[path] = new SureKaydi { ticks = ticks, sure = s };
            return s;
        }

        static float WavSuresi(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                using (var br = new BinaryReader(fs))
                {
                    if (fs.Length < 44) return -1f;
                    fs.Seek(12, SeekOrigin.Begin);
                    int rate = 0, ch = 1, bits = 16;
                    long dataLen = -1;
                    while (fs.Position + 8 <= fs.Length)
                    {
                        string id = Encoding.ASCII.GetString(br.ReadBytes(4));
                        int size = br.ReadInt32();
                        if (id == "fmt ")
                        {
                            br.ReadInt16(); ch = br.ReadInt16(); rate = br.ReadInt32(); br.ReadInt32(); br.ReadInt16(); bits = br.ReadInt16();
                            if (size > 16) fs.Seek(size - 16, SeekOrigin.Current);
                        }
                        else if (id == "data") { dataLen = size; break; }
                        else fs.Seek(size, SeekOrigin.Current);
                    }
                    if (rate <= 0 || dataLen < 0 || ch <= 0 || bits < 8) return -1f;
                    return (float)dataLen / (rate * ch * (bits / 8));
                }
            }
            catch { return -1f; }
        }

        // Editorde dinle (UnityEditor.AudioUtil yansima ile; olmazsa sessizce gecer)
        public static void Dinle(string assetPath)
        {
            try
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null) return;
                var t = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
                if (t == null) return;
                var fl = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                var stop = t.GetMethod("StopAllPreviewClips", fl);
                if (stop != null) stop.Invoke(null, null);
                var play = t.GetMethod("PlayPreviewClip", fl, null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
                if (play != null) { play.Invoke(null, new object[] { clip, 0, false }); return; }
                var play2 = t.GetMethod("PlayClip", fl, null, new[] { typeof(AudioClip) }, null);
                if (play2 != null) play2.Invoke(null, new object[] { clip });
            }
            catch (System.Exception e) { Debug.LogWarning("[Hikaye] Ses önizlenemedi: " + e.Message); }
        }

        struct Result { public bool ok, daily; public long code; public string error; public byte[] pcm; public int rate; }

        [System.Serializable] class Resp { public Cand[] candidates; public Err error; }
        [System.Serializable] class Cand { public Cont content; }
        [System.Serializable] class Cont { public Part[] parts; }
        [System.Serializable] class Part { public Inline inlineData; }
        [System.Serializable] class Inline { public string mimeType; public string data; }
        [System.Serializable] class Err { public int code; public string message; public string status; }

        // ---------- Yedek / varyant ----------
        // Etkin sesi _yedek klasorune tasir
        static void BackupActive(string story, string key)
        {
            string d = Dir(story) + "/";
            foreach (var ext in Exts)
            {
                string f = d + key + ext;
                if (!File.Exists(f)) continue;
                string bd = d + "_yedek";
                if (!AssetDatabase.IsValidFolder(bd)) AssetDatabase.CreateFolder(Dir(story), "_yedek");
                string dest = bd + "/" + key + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
                AssetDatabase.MoveAsset(f, dest);
            }
            SureOnbellegiTemizle();
        }

        // Tek satiri yeniden seslendir: eski ses _yedek klasorune tasinir, sonra yeniden uretilir
        public static void Regenerate(StoryLibrary lib, string story, StoryItem it)
        {
            if (Busy || it == null || it.Type != ItemType.Dialogue) return;
            BackupActive(story, lib.ClipKey(it));
            Generate(lib, story, new List<StoryItem> { it });
        }

        // Son yedegi geri al
        public static bool RestoreBackup(StoryLibrary lib, string story, StoryItem it)
        {
            if (it == null) return false;
            string d = Dir(story) + "/", bd = d + "_yedek", k = lib.ClipKey(it);
            if (!Directory.Exists(bd)) return false;
            var list = new List<string>(Directory.GetFiles(bd, k + "_*"));
            list.RemoveAll(f => f.EndsWith(".meta"));
            if (list.Count == 0) return false;
            list.Sort();
            string last = list[list.Count - 1].Replace('\\', '/');
            foreach (var ext in Exts) if (File.Exists(d + k + ext)) AssetDatabase.DeleteAsset(d + k + ext);
            AssetDatabase.MoveAsset(last, d + k + Path.GetExtension(last));
            SureOnbellegiTemizle();
            return true;
        }

        public static bool HasBackup(StoryLibrary lib, string story, StoryItem it)
        {
            if (it == null) return false;
            string bd = Dir(story) + "/_yedek";
            return Directory.Exists(bd) && Directory.GetFiles(bd, lib.ClipKey(it) + "_*.wav").Length > 0;
        }

        // Varyantlar: <anahtar>_v1.wav, _v2.wav ... Etkin ses degismez; "Bunu kullan" ile etkin yapilir.
        public static List<string> Variants(StoryLibrary lib, string story, StoryItem it)
        {
            var l = new List<string>();
            if (lib == null || it == null) return l;
            string d = Dir(story);
            if (!Directory.Exists(d)) return l;
            foreach (var f in Directory.GetFiles(d, lib.ClipKey(it) + "_v*.wav"))
                l.Add(f.Replace('\\', '/'));
            l.Sort();
            return l;
        }

        static string NextVariantPath(string story, string key)
        {
            for (int n = 1; n < 100; n++)
            {
                string p = Dir(story) + "/" + key + "_v" + n + ".wav";
                if (!File.Exists(p)) return p;
            }
            return Dir(story) + "/" + key + "_v99.wav";
        }

        public static void GenerateVariant(StoryLibrary lib, string story, StoryItem it)
        {
            if (Busy || it == null || it.Type != ItemType.Dialogue) return;
            Run(lib, story, new List<StoryItem> { it }, true);
        }

        // Varyanti etkin ses yapar (eski etkin ses _yedek'e gider: "Eski sesi geri al" ile donulur)
        public static bool UseVariant(StoryLibrary lib, string story, StoryItem it, string variantPath)
        {
            if (lib == null || it == null || string.IsNullOrEmpty(variantPath) || !File.Exists(variantPath)) return false;
            string k = lib.ClipKey(it);
            BackupActive(story, k);
            bool ok = AssetDatabase.CopyAsset(variantPath, Dir(story) + "/" + k + ".wav");
            SureOnbellegiTemizle();
            return ok;
        }

        public static void DeleteVariant(string variantPath)
        {
            if (string.IsNullOrEmpty(variantPath)) return;
            AssetDatabase.DeleteAsset(variantPath);
            SureOnbellegiTemizle();
        }

        // ---------- Uretim ----------
        public static void Generate(StoryLibrary lib, string story, List<StoryItem> items)
        {
            Run(lib, story, items, false);
        }

        static int ProgressBaslat(string story)
        {
            try
            {
                int id = Progress.Start("Seslendirme: " + story, "Gemini TTS", Progress.Options.None, -1);
                Progress.RegisterCancelCallback(id, () => { stopRequested = true; return true; });
                return id;
            }
            catch { return -1; }
        }

        static void ProgressRapor(int id, float oran, string metin)
        {
            if (id < 0) return;
            try { Progress.Report(id, Mathf.Clamp01(oran), metin); } catch { }
        }

        static void ProgressBitir(int id, bool basarili)
        {
            if (id < 0) return;
            try { Progress.Finish(id, basarili ? Progress.Status.Succeeded : Progress.Status.Failed); } catch { }
        }

        struct Uretilen
        {
            public bool ok, daily, durduruldu, kisaKaldi;
            public long code;
            public string error;
            public byte[] wav;
            public float sure, beklenen;
            public int deneme;
        }

        static async void Run(StoryLibrary lib, string story, List<StoryItem> items, bool variant)
        {
            if (Busy) return;
            string key = EditorPrefs.GetString(KeyPref, "").Trim();
            string model = Model;
            if (key.Length == 0) { Status = "Önce Gemini API anahtarını gir."; Notify(); return; }

            var todo = new List<StoryItem>();
            var seen = new HashSet<string>();
            foreach (var it in items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                if (variant || (!HasClip(story, lib, it) && seen.Add(lib.ClipKey(it)))) todo.Add(it);
            }
            if (todo.Count == 0) { Status = "Bütün satırların sesi hazır."; Notify(); return; }

            Busy = true; stopRequested = false;
            Rapor.Clear();
            Directory.CreateDirectory(Dir(story));
            int done = 0;
            bool halted = false;
            int pid = ProgressBaslat(story);
            try
            {
                foreach (var it in todo)
                {
                    if (stopRequested) { Status = "Durduruldu. " + done + " ses üretildi."; halted = true; break; }
                    string voice = lib.VoiceFor(it), tone = Util.ToneFor(it);
                    string raw = string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech;
                    string speech = TurkceMetin.ForSpeech(raw);
                    string label = it.Line + ". satır  " + it.Character + " (" + voice + "): " + Short(it.Text);
                    Status = (done + 1) + "/" + todo.Count + "  " + label;
                    ProgressRapor(pid, (float)done / todo.Count, Status);
                    Notify();

                    var r = await Uret(key, model, voice, tone, speech, label);
                    if (r.ok)
                    {
                        string path = variant ? NextVariantPath(story, lib.ClipKey(it)) : Dir(story) + "/" + lib.ClipKey(it) + ".wav";
                        File.WriteAllBytes(path, r.wav);
                        AssetDatabase.ImportAsset(path);
                        SureOnbellegiTemizle();
                        done++;
                        if (r.kisaKaldi)
                            Rapor.Add(it.Line + ". satır: ses hâlâ kısa (" + r.sure.ToString("0.0") + " sn, beklenen ~" + r.beklenen.ToString("0.0") + " sn, " + r.deneme + " deneme). Metni kısalt ya da 'Yeniden seslendir' dene.");
                        else if (r.deneme > 1)
                            Rapor.Add(it.Line + ". satır: kesik ses " + (r.deneme - 1) + " kez yeniden üretildi.");
                    }
                    else if (r.durduruldu)
                    {
                        Status = "Durduruldu. " + done + " ses üretildi.";
                        halted = true;
                    }
                    else
                    {
                        Status = r.daily
                            ? "Günlük ücretsiz kota doldu. " + done + " ses üretildi; kalanları yarın ~11:00'den sonra üret."
                            : "Hata (" + r.code + "): " + r.error;
                        halted = true;
                    }
                    if (halted) break;
                    await Task.Delay(1200);
                }
                if (!halted)
                    Status = variant ? "Varyant hazır (" + done + ")." : "Tamam: " + done + " ses üretildi." + (Rapor.Count > 0 ? " " + Rapor.Count + " uyarı var." : "");
            }
            catch (System.Exception e)
            {
                Status = "Hata: " + e.Message;
                halted = true;
                Debug.LogException(e);
            }
            finally
            {
                Busy = false;
                ProgressBitir(pid, !halted || done > 0);
                AssetDatabase.Refresh();
                SureOnbellegiTemizle();
                Notify();
                if (Bitti != null) Bitti(Status);
            }
        }

        static string Short(string s) { return s.Length > 40 ? s.Substring(0, 40) + "…" : s; }

        static bool IsRiff(byte[] b) { return b != null && b.Length > 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'; }

        static float PcmSuresi(Result r)
        {
            if (r.pcm == null || r.rate <= 0) return 0f;
            int bytes = IsRiff(r.pcm) ? Mathf.Max(0, r.pcm.Length - 44) : r.pcm.Length;
            return bytes / 2f / r.rate;
        }

        // Tek satir icin istek: 400'de alternatif sema, dakikalik 429'da bekleme, kesik sesi yeniden uretme
        static async Task<Uretilen> Uret(string key, string model, string voice, string tone, string speech, string label)
        {
            var best = new Uretilen();
            float expected = TurkceMetin.BeklenenSure(speech);
            best.beklenen = expected;
            int kisaDeneme = 0, hizDeneme = 0, toplam = 0;
            int maxKisa = KisaTekrar;
            while (true)
            {
                if (stopRequested) { if (!best.ok) best.durduruldu = true; return best; }
                var r = await Request(key, model, voice, tone, speech, false);
                if (!r.ok && r.code == 400 && NewSchema(model))
                    r = await Request(key, model, voice, tone, speech, true);
                if (r.ok)
                {
                    KotaArtir();
                    toplam++;
                    float sec = PcmSuresi(r);
                    if (!best.ok || sec > best.sure)
                    {
                        best.ok = true; best.sure = sec;
                        best.wav = IsRiff(r.pcm) ? r.pcm : Wav(r.pcm, r.rate);
                    }
                    bool kisa = expected >= 1.2f && best.sure < expected * KisaOran;
                    if (kisa && kisaDeneme < maxKisa && !stopRequested)
                    {
                        kisaDeneme++;
                        Status = label + "\nKesik ses algılandı (" + best.sure.ToString("0.0") + " sn < ~" + (expected * KisaOran).ToString("0.0") + " sn): yeniden üretiliyor (" + kisaDeneme + "/" + maxKisa + ")";
                        Notify();
                        await Task.Delay(1500);
                        continue;
                    }
                    best.kisaKaldi = kisa;
                    best.deneme = toplam;
                    return best;
                }
                if (r.code == 429 && !r.daily && hizDeneme < 4 && !stopRequested)
                {
                    hizDeneme++;
                    Status = "Dakikalık sınıra takıldı, 30 sn bekleniyor... (" + hizDeneme + "/4)";
                    Notify();
                    await Task.Delay(30000);
                    continue;
                }
                if (r.daily) KotaDoldu();
                if (best.ok) { best.kisaKaldi = true; best.deneme = toplam; return best; }   // onceki denemelerin en iyisini koru
                best.daily = r.daily; best.code = r.code; best.error = r.error;
                return best;
            }
        }

        // altVoice: yeni semada ses alanini eski bicimde (prebuiltVoiceConfig) gonder
        static async Task<Result> Request(string key, string model, string voice, string tone, string text, bool altVoice)
        {
            var res = new Result();
            string json;
            if (NewSchema(model))
            {
                // Gemini 3.8: metin birebir okunur; ton kisa bir stil olarak speech_metadata.style icinde gider
                string voiceJson = altVoice
                    ? "{\"prebuiltVoiceConfig\":{\"voiceName\":\"" + Esc(voice) + "\"}}"
                    : "{\"voice\":\"" + Esc(voice) + "\"}";
                json = "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"" + Esc(text) + "\"," +
                       "\"speech_metadata\":{\"style\":\"" + Esc(StilKisalt(tone)) + "\"}}]}]," +
                       "\"generationConfig\":{\"responseModalities\":[\"AUDIO\"]," +
                       "\"speechConfig\":{\"voiceConfig\":" + voiceJson + "}}}";
            }
            else
            {
                string prompt = StilKisalt(tone) + " bir sesle Türkçe söyle: " + text;
                json = "{\"contents\":[{\"parts\":[{\"text\":\"" + Esc(prompt) + "\"}]}]," +
                       "\"generationConfig\":{\"responseModalities\":[\"AUDIO\"]," +
                       "\"speechConfig\":{\"voiceConfig\":{\"prebuiltVoiceConfig\":{\"voiceName\":\"" + Esc(voice) + "\"}}}}}";
            }
            string url = "https://generativelanguage.googleapis.com/v1beta/models/" + model + ":generateContent";
            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("x-goog-api-key", key);
                req.timeout = 120;
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Delay(100);

                res.code = req.responseCode;
                string body = req.downloadHandler != null ? req.downloadHandler.text : "";
                if (req.result != UnityWebRequest.Result.Success)
                {
                    string msg = req.error;
                    try
                    {
                        var er = JsonUtility.FromJson<Resp>(body);
                        if (er != null && er.error != null && !string.IsNullOrEmpty(er.error.message)) msg = er.error.message;
                    }
                    catch { }
                    var low = (body ?? "").ToLowerInvariant();
                    res.daily = res.code == 429 && (low.Contains("perday") || low.Contains("per day") || low.Contains("daily"));
                    res.error = msg;
                    return res;
                }
                var resp = JsonUtility.FromJson<Resp>(body);
                if (resp == null || resp.candidates == null || resp.candidates.Length == 0 || resp.candidates[0].content == null
                    || resp.candidates[0].content.parts == null || resp.candidates[0].content.parts.Length == 0
                    || resp.candidates[0].content.parts[0].inlineData == null
                    || string.IsNullOrEmpty(resp.candidates[0].content.parts[0].inlineData.data))
                {
                    res.error = "Cevapta ses yok.";
                    return res;
                }
                var inl = resp.candidates[0].content.parts[0].inlineData;
                res.pcm = System.Convert.FromBase64String(inl.data);
                res.rate = 24000;
                if (!string.IsNullOrEmpty(inl.mimeType))
                {
                    int i = inl.mimeType.IndexOf("rate=");
                    if (i >= 0)
                    {
                        var num = new StringBuilder();
                        for (int k = i + 5; k < inl.mimeType.Length && char.IsDigit(inl.mimeType[k]); k++) num.Append(inl.mimeType[k]);
                        int r;
                        if (int.TryParse(num.ToString(), out r) && r > 0) res.rate = r;
                    }
                }
                res.ok = true;
                return res;
            }
        }

        static string Esc(string s)
        {
            var sb = new StringBuilder(s.Length + 8);
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // 16 bit mono PCM -> WAV
        static byte[] Wav(byte[] pcm, int rate)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Encoding.ASCII.GetBytes("RIFF"));
                w.Write(36 + pcm.Length);
                w.Write(Encoding.ASCII.GetBytes("WAVE"));
                w.Write(Encoding.ASCII.GetBytes("fmt "));
                w.Write(16);
                w.Write((short)1);       // PCM
                w.Write((short)1);       // mono
                w.Write(rate);
                w.Write(rate * 2);
                w.Write((short)2);
                w.Write((short)16);
                w.Write(Encoding.ASCII.GetBytes("data"));
                w.Write(pcm.Length);
                w.Write(pcm);
                w.Flush();
                return ms.ToArray();
            }
        }
    }

    // Ses klasorundeki dosyalar bosluk kirpma icin "Decompress On Load" olarak alinir
    public class SesIceAktarma : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Ses/")) return;
            var imp = (AudioImporter)assetImporter;
            var s = imp.defaultSampleSettings;
            s.loadType = AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.PCM;
            imp.defaultSampleSettings = s;
            imp.loadInBackground = false;
        }
    }

    // telaffuz.txt degisince sozlugu yeniden oku
    public class TelaffuzIceAktarma : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var p in imported)
                if (p.Replace('\\', '/').EndsWith("/Resources/telaffuz.txt")) { TurkceMetin.Reload(); GeminiSes.SureOnbellegiTemizle(); return; }
        }
    }
}
