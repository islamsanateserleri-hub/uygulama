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
    public static class GeminiSes
    {
        public const string KeyPref = "HS_GeminiKey";
        public const string ModelPref = "HS_GeminiModel";
        public const string DefaultModel = "gemini-3.8-flash-tts";
        public const string OldModel = "gemini-2.5-flash-preview-tts";
        public static readonly string[] Models = { "gemini-3.8-flash-tts", "gemini-3.8-flash-lite-tts", "gemini-2.5-flash-preview-tts" };
        public static readonly string[] ModelLabels = { "Gemini 3.8 Flash TTS (en iyi)", "Gemini 3.8 Flash-Lite TTS (ucuz, hızlı)", "Gemini 2.5 (eski)" };

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

        static bool NewSchema(string model) { return !model.StartsWith("gemini-2"); }

        public static bool Busy { get; private set; }
        public static string Status = "";
        public static event System.Action Changed;
        static bool stopRequested;

        public static string Dir(string story) { return "Assets/Resources/Ses/" + story; }

        public static bool HasClip(string story, StoryLibrary lib, StoryItem it)
        {
            string d = Dir(story) + "/";
            string k = lib.ClipKey(it), n = it.VoiceIndex.ToString("00");
            foreach (var ext in new[] { ".wav", ".mp3", ".ogg" })
                if (File.Exists(d + k + ext) || File.Exists(d + n + ext)) return true;
            return false;
        }

        public static void Stop() { stopRequested = true; Status = "Durduruluyor..."; Notify(); }

        static void Notify() { if (Changed != null) Changed(); }

        struct Result { public bool ok, daily; public long code; public string error; public byte[] pcm; public int rate; }

        [System.Serializable] class Resp { public Cand[] candidates; public Err error; }
        [System.Serializable] class Cand { public Cont content; }
        [System.Serializable] class Cont { public Part[] parts; }
        [System.Serializable] class Part { public Inline inlineData; }
        [System.Serializable] class Inline { public string mimeType; public string data; }
        [System.Serializable] class Err { public int code; public string message; public string status; }

        // Tek satiri yeniden seslendir: eski ses _yedek klasorune tasinir, sonra yeniden uretilir
        public static void Regenerate(StoryLibrary lib, string story, StoryItem it)
        {
            if (Busy || it == null || it.Type != ItemType.Dialogue) return;
            string d = Dir(story) + "/";
            string k = lib.ClipKey(it);
            foreach (var ext in new[] { ".wav", ".mp3", ".ogg" })
            {
                string f = d + k + ext;
                if (!File.Exists(f)) continue;
                string bd = d + "_yedek";
                if (!AssetDatabase.IsValidFolder(bd)) AssetDatabase.CreateFolder(Dir(story), "_yedek");
                string dest = bd + "/" + k + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
                AssetDatabase.MoveAsset(f, dest);
            }
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
            foreach (var ext in new[] { ".wav", ".mp3", ".ogg" }) if (File.Exists(d + k + ext)) AssetDatabase.DeleteAsset(d + k + ext);
            AssetDatabase.MoveAsset(last, d + k + Path.GetExtension(last));
            return true;
        }

        public static bool HasBackup(StoryLibrary lib, string story, StoryItem it)
        {
            if (it == null) return false;
            string bd = Dir(story) + "/_yedek";
            return Directory.Exists(bd) && Directory.GetFiles(bd, lib.ClipKey(it) + "_*.wav").Length > 0;
        }

        public static async void Generate(StoryLibrary lib, string story, List<StoryItem> items)
        {
            if (Busy) return;
            string key = EditorPrefs.GetString(KeyPref, "").Trim();
            string model = Model;
            if (key.Length == 0) { Status = "Önce Gemini API anahtarını gir."; Notify(); return; }

            var todo = new List<StoryItem>();
            var seen = new HashSet<string>();
            foreach (var it in items)
                if (it.Type == ItemType.Dialogue && !HasClip(story, lib, it) && seen.Add(lib.ClipKey(it))) todo.Add(it);
            if (todo.Count == 0) { Status = "Bütün satırların sesi hazır."; Notify(); return; }

            Busy = true; stopRequested = false;
            Directory.CreateDirectory(Dir(story));
            int done = 0;
            bool halted = false;
            try
            {
                foreach (var it in todo)
                {
                    if (stopRequested) { Status = "Durduruldu. " + done + " ses üretildi."; halted = true; break; }
                    string voice = lib.VoiceFor(it), tone = Util.ToneFor(it);
                    int tries = 0;
                    while (true)
                    {
                        Status = (done + 1) + "/" + todo.Count + "  " + it.Character + " (" + voice + "): " + Short(it.Text);
                        Notify();
                        var r = await Request(key, model, voice, tone, string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech, false);
                        if (!r.ok && r.code == 400 && NewSchema(model))
                            r = await Request(key, model, voice, tone, string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech, true);
                        if (r.ok)
                        {
                            string path = Dir(story) + "/" + lib.ClipKey(it) + ".wav";
                            File.WriteAllBytes(path, IsRiff(r.pcm) ? r.pcm : Wav(r.pcm, r.rate));
                            AssetDatabase.ImportAsset(path);
                            done++;
                            break;
                        }
                        if (r.code == 429 && !r.daily && tries < 4 && !stopRequested)
                        {
                            tries++;
                            Status = "Dakikalık sınıra takıldı, 30 sn bekleniyor... (" + tries + "/4)";
                            Notify();
                            await Task.Delay(30000);
                            continue;
                        }
                        Status = r.daily
                            ? "Günlük ücretsiz kota doldu. " + done + " ses üretildi; kalanları yarın ~11:00'den sonra üret."
                            : "Hata (" + r.code + "): " + r.error;
                        halted = true;
                        break;
                    }
                    if (halted) break;
                    await Task.Delay(1200);
                }
                if (!halted) Status = "Tamam: " + done + " ses üretildi.";
            }
            catch (System.Exception e)
            {
                Status = "Hata: " + e.Message;
                Debug.LogException(e);
            }
            finally
            {
                Busy = false;
                AssetDatabase.Refresh();
                Notify();
            }
        }

        static string Short(string s) { return s.Length > 40 ? s.Substring(0, 40) + "…" : s; }

        static bool IsRiff(byte[] b) { return b != null && b.Length > 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'; }

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
                       "\"speech_metadata\":{\"style\":\"" + Esc(tone) + "\"}}]}]," +
                       "\"generationConfig\":{\"responseModalities\":[\"AUDIO\"]," +
                       "\"speechConfig\":{\"voiceConfig\":" + voiceJson + "}}}";
            }
            else
            {
                string prompt = tone + " bir sesle Türkçe söyle: " + text;
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
}
