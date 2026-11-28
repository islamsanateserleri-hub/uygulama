using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // Gemini TTS ile seslendirme (uygulama/exe surumu). Editor'deki GeminiSes'in kopyasi degil, PlayerPrefs + kalici klasor kullanir.
    // Ses dosyalari: Documents\HikayeStudyosu\Ses\<hikaye>\<anahtar>.wav  (anahtar = StoryLibrary.ClipKey: ses+ton+metin)
    public static class GeminiUyg
    {
        public const string KeyPref = "HS_GeminiKey", ModelPref = "HS_GeminiModel";
        public const string VarsayilanModel = "gemini-3.8-flash-tts";
        public static readonly string[] Modeller = { "gemini-3.8-flash-tts", "gemini-3.8-flash-lite-tts", "gemini-2.5-flash-preview-tts" };
        public static readonly string[] ModelEtiketleri = { "Gemini 3.8 Flash TTS (en iyi)", "Gemini 3.8 Flash-Lite TTS (ucuz, hızlı)", "Gemini 2.5 (eski)" };

        public const float KisaOran = 0.70f;
        public const int KisaTekrar = 1;          // kesik ses en fazla 1 kez yeniden denenir (kota korunur)
        public const int YedekSiniri = 3;         // _yedek'te satir basina tutulan yedek sayisi
        public const int StilKelimeSiniri = 6;
        public const int KotaLimit = 10;
        const string KotaGunKey = "HS_KotaGun", KotaSayiKey = "HS_KotaSayi", KotaDolduKey = "HS_KotaDoldu";

        // Test icin: istek adresi degistirilebilir (varsayilan Google). Bos ise varsayilan.
        public static string TabanAdres = "https://generativelanguage.googleapis.com/v1beta/models/";

        public static bool Busy { get; private set; }
        public static string Status = "";
        public static readonly List<string> Rapor = new List<string>();
        static bool stopRequested;

        // Asama 7: anahtar Windows DPAPI ile sifreli saklanir; eski duz kayit ilk okumada tasinir.
        public const string KeyKorumaliPref = "HS_GeminiKeyKorumali";
        static string anahtarOnbellek;     // her karede cozmemek icin
        static bool anahtarSifreli;

        public static string Anahtar
        {
            get
            {
                if (anahtarOnbellek == null) anahtarOnbellek = SirKoruma.Oku(KeyPref, KeyKorumaliPref, out anahtarSifreli);
                return anahtarOnbellek;
            }
            set
            {
                string v = (value ?? "").Trim();
                anahtarSifreli = SirKoruma.Yaz(KeyPref, KeyKorumaliPref, v) && v.Length > 0;
                anahtarOnbellek = v;
            }
        }
        public static bool AnahtarSifreli { get { return Anahtar.Length > 0 && anahtarSifreli; } }

        public static string Model
        {
            get
            {
                var m = PlayerPrefs.GetString(ModelPref, VarsayilanModel).Trim();
                return m.Length == 0 ? VarsayilanModel : m;
            }
            set { PlayerPrefs.SetString(ModelPref, value); }
        }

        static bool YeniSema(string model) { return !model.StartsWith("gemini-2"); }
        public static void Durdur() { stopRequested = true; Status = "Durduruluyor..."; }

        // ---------- Kota (Pasifik saatine gore gunluk) ----------
        static string KotaGunu()
        {
            var now = DateTime.UtcNow;
            foreach (var id in new[] { "America/Los_Angeles", "Pacific Standard Time" })
            {
                try { return TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById(id)).ToString("yyyyMMdd"); }
                catch { }
            }
            return now.AddHours(-8).ToString("yyyyMMdd");
        }
        public static int KotaBugun() { return PlayerPrefs.GetString(KotaGunKey, "") == KotaGunu() ? PlayerPrefs.GetInt(KotaSayiKey, 0) : 0; }
        static void KotaArtir()
        {
            string g = KotaGunu();
            int n = PlayerPrefs.GetString(KotaGunKey, "") == g ? PlayerPrefs.GetInt(KotaSayiKey, 0) : 0;
            PlayerPrefs.SetString(KotaGunKey, g);
            PlayerPrefs.SetInt(KotaSayiKey, n + 1);
            PlayerPrefs.Save();
        }
        static void KotaDoldu() { PlayerPrefs.SetString(KotaDolduKey, KotaGunu()); PlayerPrefs.Save(); }
        public static bool KotaDolduMu() { return PlayerPrefs.GetString(KotaDolduKey, "") == KotaGunu(); }
        public static int KotaKalan() { return Mathf.Max(0, KotaLimit - KotaBugun()); }
        public static string KotaMetni()
        {
            int n = KotaBugun();
            string s = "Bugün ~" + n + "/" + KotaLimit + " ses isteği (yeniden denemeler de sayılır)";
            if (KotaDolduMu()) s += " · Günlük kota doldu: yarın ~11:00'den sonra";
            else if (n >= KotaLimit) s += " · Kota dolmak üzere";
            return s;
        }

        // ---------- Satir durumu ----------
        public static bool Hazir(string hikaye, StoryLibrary lib, StoryItem it) { return DisSes.Var(hikaye, lib.ClipKey(it)); }

        public static float Sure(string hikaye, StoryLibrary lib, StoryItem it)
        {
            string yol = DisSes.Dosya(hikaye, lib.ClipKey(it));
            return File.Exists(yol) ? DisSes.WavSuresi(yol) : -1f;
        }

        public static List<StoryItem> Eksikler(string hikaye, StoryLibrary lib, IList<StoryItem> items)
        {
            var l = new List<StoryItem>();
            var seen = new HashSet<string>();
            if (lib == null || items == null) return l;
            foreach (var it in items)
                if (it.Type == ItemType.Dialogue && !Hazir(hikaye, lib, it) && seen.Add(lib.ClipKey(it))) l.Add(it);
            return l;
        }

        public static string StilKisalt(string tone)
        {
            if (string.IsNullOrEmpty(tone)) return "doğal";
            string t = tone.Replace("iç ses gibi sakin ve derin", "sakin iç ses");
            var w = t.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (w.Length <= StilKelimeSiniri) return t;
            var sb = new StringBuilder();
            for (int i = 0; i < StilKelimeSiniri; i++) { if (i > 0) sb.Append(' '); sb.Append(w[i]); }
            return sb.ToString().TrimEnd(',', ';');
        }

        // ---------- Uretim ----------
        public static void EksikleriUret(string hikaye, StoryLibrary lib, List<StoryItem> items)
        {
            Calistir(hikaye, lib, Eksikler(hikaye, lib, items), false);
        }

        // Tek satiri yeniden uret (eskisi _yedek'e tasinir)
        public static void YenidenUret(string hikaye, StoryLibrary lib, StoryItem it)
        {
            if (Busy || it == null || it.Type != ItemType.Dialogue) return;
            Calistir(hikaye, lib, new List<StoryItem> { it }, true);
        }

        public static bool YedekVar(string hikaye, StoryLibrary lib, StoryItem it)
        {
            string bd = Path.Combine(DisSes.Klasor(hikaye), "_yedek");
            return it != null && Directory.Exists(bd) && Directory.GetFiles(bd, lib.ClipKey(it) + "_*.wav").Length > 0;
        }

        // Yedegi olan anahtarlar (Ses sekmesi "Geri al" dugmesi icin; klasor bir kez listelenir)
        public static HashSet<string> YedekAnahtarlari(string hikaye)
        {
            var s = new HashSet<string>();
            try
            {
                string bd = Path.Combine(DisSes.Klasor(hikaye), "_yedek");
                if (!Directory.Exists(bd)) return s;
                foreach (var f in Directory.GetFiles(bd, "*.wav"))
                {
                    string ad = Path.GetFileName(f);
                    int i = ad.IndexOf('_');
                    if (i > 0) s.Add(ad.Substring(0, i));
                }
            }
            catch { }
            return s;
        }

        // En yeni yedegi geri koyar; mevcut ses silinmez, once _yedek'e tasinir (tekrar basilirsa yer degistirir).
        public static bool YedegiGeriAl(string hikaye, StoryLibrary lib, StoryItem it)
        {
            if (Busy || it == null || lib == null) return false;
            string bd = Path.Combine(DisSes.Klasor(hikaye), "_yedek"), k = lib.ClipKey(it);
            string hedef = DisSes.Dosya(hikaye, k), gecici = hedef + ".geri", son = null;
            try
            {
                var l = Yedekler(bd, k);
                if (l.Count == 0) return false;
                son = l[l.Count - 1];
                if (File.Exists(gecici)) File.Delete(gecici);
                File.Move(son, gecici);          // secilen yedek budamadan korunsun
                Yedekle(hikaye, k);              // mevcut (yeni) ses _yedek'e
                File.Move(gecici, hedef);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Hikaye] Yedek geri alınamadı: " + e.Message);
                try { if (son != null && File.Exists(gecici) && !File.Exists(son)) File.Move(gecici, son); } catch { }
                return false;
            }
        }

        static List<string> Yedekler(string bd, string key)
        {
            var l = Directory.Exists(bd) ? new List<string>(Directory.GetFiles(bd, key + "_*.wav")) : new List<string>();
            l.Sort(StringComparer.Ordinal);      // ad = anahtar_yyyyMMdd_HHmmss[_fff[_n]] : eskiden yeniye
            return l;
        }

        static void Yedekle(string hikaye, string key)
        {
            string f = DisSes.Dosya(hikaye, key);
            if (!File.Exists(f)) return;
            string bd = Path.Combine(DisSes.Klasor(hikaye), "_yedek");
            Directory.CreateDirectory(bd);
            string taban = Path.Combine(bd, key + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            string ad = taban + ".wav";
            for (int n = 2; File.Exists(ad); n++) ad = taban + "_" + n + ".wav";
            File.Move(f, ad);
            // Satir basina son YedekSiniri yedek kalir
            var l = Yedekler(bd, key);
            for (int i = 0; i < l.Count - YedekSiniri; i++)
            {
                try { File.Delete(l[i]); } catch (Exception e) { Debug.LogWarning("[Hikaye] Eski yedek silinemedi: " + e.Message); }
            }
        }

        // tmp -> son: eski dosya once kenara alinir, tasima basarisizsa geri konur
        static void Yerlestir(string tmp, string son)
        {
            if (!File.Exists(son)) { File.Move(tmp, son); return; }
            string eski = son + ".eski";
            if (File.Exists(eski)) File.Delete(eski);
            File.Move(son, eski);
            try { File.Move(tmp, son); }
            catch { try { if (!File.Exists(son)) File.Move(eski, son); } catch { } throw; }
            try { File.Delete(eski); } catch { }
        }

        // 1 sn adimlarla bekler; Durdur'a basilinca hemen doner
        static async Task Bekle(int saniye)
        {
            for (int i = 0; i < saniye && !stopRequested; i++) await Task.Delay(1000);
        }

        static async void Calistir(string hikaye, StoryLibrary lib, List<StoryItem> todo, bool yeniden)
        {
            if (Busy) return;
            string key = Anahtar, model = Model;
            if (key.Length == 0) { Status = "Önce Gemini API anahtarını gir (Ses sekmesi > Seslendirme)."; return; }
            if (todo.Count == 0) { Status = "Bütün satırların sesi hazır."; return; }
            if (KotaDolduMu()) { Status = "Günlük ücretsiz kota doldu; istek gönderilmedi. Yarın ~11:00'den sonra yeniden dene."; return; }
            Busy = true; stopRequested = false; Rapor.Clear();
            int done = 0, atlanan = 0, ardisikHata = 0; bool halted = false;
            try
            {
                Directory.CreateDirectory(DisSes.Klasor(hikaye));
                foreach (var it in todo)
                {
                    if (stopRequested) { Status = "Durduruldu. " + done + " ses üretildi."; halted = true; break; }
                    string voice = lib.VoiceFor(it), tone = lib.ToneFor(it), ck = lib.ClipKey(it);
                    string speech = TurkceMetin.ForSpeech(string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech);
                    string label = it.Line + ". satır  " + (string.IsNullOrEmpty(it.Character) ? "Anlatıcı" : it.Character) + " (" + voice + ")";
                    Status = (done + atlanan + 1) + "/" + todo.Count + "  " + label;

                    var r = await Uret(key, model, voice, tone, speech, label);
                    if (r.ok)
                    {
                        ardisikHata = 0;
                        string son = DisSes.Dosya(hikaye, ck), tmp = son + ".tmp";
                        File.WriteAllBytes(tmp, r.wav);
                        if (yeniden)
                        {
                            try { Yedekle(hikaye, ck); }
                            catch (Exception ye)
                            {
                                // Eski ses yerinde kalir; yeni ses yazilmaz (veri kaybi olmasin)
                                try { File.Delete(tmp); } catch { }
                                Rapor.Add(it.Line + ". satır: eski ses yedeklenemedi, yeni ses kaydedilmedi (" + ye.Message + ").");
                                atlanan++;
                                await Task.Delay(1200);
                                continue;
                            }
                        }
                        Yerlestir(tmp, son);
                        done++;
                        if (r.kisaKaldi) Rapor.Add(it.Line + ". satır: ses hâlâ kısa (" + r.sure.ToString("0.0") + " sn, beklenen ~" + r.beklenen.ToString("0.0") + " sn). Metni kısalt ya da yeniden dene.");
                        else if (r.deneme > 1) Rapor.Add(it.Line + ". satır: kesik ses " + (r.deneme - 1) + " kez yeniden üretildi.");
                    }
                    else if (r.durduruldu) { Status = "Durduruldu. " + done + " ses üretildi."; halted = true; }
                    else if (r.daily)
                    {
                        Status = "Günlük ücretsiz kota doldu. " + done + " ses üretildi; kalanları yarın ~11:00'den sonra üret.";
                        halted = true;
                    }
                    else if (r.anahtar)
                    {
                        Status = "API anahtarı geçersiz ya da yetkisiz (" + r.code + "): " + r.error;
                        halted = true;
                    }
                    else if (r.ag)
                    {
                        Status = "Bağlantı hatası: " + r.error + " · " + done + " ses üretildi. İnternet bağlantısını kontrol edip yeniden dene.";
                        halted = true;
                    }
                    else
                    {
                        // Satira ozgu hata: raporla, sonraki satira gec
                        Rapor.Add(it.Line + ". satır: Hata (" + r.code + "): " + r.error);
                        atlanan++;
                        ardisikHata++;
                        if (ardisikHata >= 2)
                        {
                            // Ust uste iki satir hata verdiyse sorun satirda degil (model/istek): kota bosa harcanmasin
                            Status = "Üst üste iki satırda hata (" + r.code + "): " + r.error + " · Durduruldu, " + done + " ses üretildi.";
                            halted = true;
                        }
                    }
                    if (halted) break;
                    await Task.Delay(1200);
                }
                if (!halted)
                {
                    if (done == 0 && atlanan > 0 && Rapor.Count > 0) Status = "Ses üretilemedi. " + Rapor[0];
                    else Status = "Tamam: " + done + " ses üretildi." + (atlanan > 0 ? " " + atlanan + " satır atlandı (hata)." : "")
                                + (Rapor.Count > 0 ? " " + Rapor.Count + " uyarı var." : "");
                }
            }
            catch (Exception e) { Status = "Hata: " + e.Message; Debug.LogException(e); }
            finally { Busy = false; }
        }

        class Uretilen { public bool ok, daily, durduruldu, kisaKaldi, anahtar, ag; public int code, deneme; public string error = ""; public byte[] wav; public float sure, beklenen; }
        class Sonuc { public bool ok, daily, anahtar, ag; public long code; public string error = ""; public byte[] pcm; public int rate = 24000; }

        static async Task<Uretilen> Uret(string key, string model, string voice, string tone, string speech, string label)
        {
            var best = new Uretilen();
            float expected = TurkceMetin.BeklenenSure(speech);
            best.beklenen = expected;
            int kisaDeneme = 0, hizDeneme = 0, agDeneme = 0, toplam = 0;
            while (true)
            {
                if (stopRequested) { if (!best.ok) best.durduruldu = true; return best; }
                var r = await Istek(key, model, voice, tone, speech, false);
                if (!r.ok && r.code == 400 && !r.anahtar && !stopRequested && YeniSema(model)) r = await Istek(key, model, voice, tone, speech, true);
                if (!r.ok && stopRequested) { if (!best.ok) best.durduruldu = true; return best; }
                if (r.ok)
                {
                    KotaArtir();
                    toplam++;
                    bool riff = r.pcm.Length > 12 && r.pcm[0] == 'R' && r.pcm[1] == 'I';
                    float sec = (riff ? Mathf.Max(0, r.pcm.Length - 44) : r.pcm.Length) / 2f / r.rate;
                    if (!best.ok || sec > best.sure) { best.ok = true; best.sure = sec; best.wav = riff ? r.pcm : DisSes.WavYaz(r.pcm, r.rate); }
                    bool kisa = expected >= 1.2f && best.sure < expected * KisaOran;
                    // Kota azken (<=1 istek) kesik ses yeniden denenmez
                    if (kisa && kisaDeneme < KisaTekrar && KotaKalan() > 1 && !stopRequested)
                    {
                        kisaDeneme++;
                        Status = label + "\nKesik ses algılandı (" + best.sure.ToString("0.0") + " sn): yeniden üretiliyor (" + kisaDeneme + "/" + KisaTekrar + ")";
                        await Task.Delay(1500);
                        continue;
                    }
                    best.kisaKaldi = kisa; best.deneme = toplam;
                    return best;
                }
                if (r.code == 429 && !r.daily && hizDeneme < 4 && !stopRequested)
                {
                    hizDeneme++;
                    Status = "Dakikalık sınıra takıldı, 30 sn bekleniyor... (" + hizDeneme + "/4)";
                    await Bekle(30);
                    continue;
                }
                if (r.ag && agDeneme < 1 && !stopRequested)
                {
                    agDeneme++;
                    Status = label + "\nBağlantı hatası (" + r.error + "): 5 sn sonra bir kez yeniden deneniyor...";
                    await Bekle(5);
                    continue;
                }
                if (r.daily) KotaDoldu();
                if (best.ok) { best.kisaKaldi = true; best.deneme = toplam; return best; }
                best.daily = r.daily; best.anahtar = r.anahtar; best.ag = r.ag; best.code = (int)r.code; best.error = r.error;
                return best;
            }
        }

        static async Task<Sonuc> Istek(string key, string model, string voice, string tone, string text, bool altVoice)
        {
            var res = new Sonuc();
            string json;
            if (YeniSema(model))
            {
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
            string url = TabanAdres + model + ":generateContent";
            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("x-goog-api-key", key);
                req.timeout = 120;
                var op = req.SendWebRequest();
                while (!op.isDone)
                {
                    if (stopRequested) { req.Abort(); res.error = "Durduruldu."; return res; }
                    await Task.Delay(100);
                }

                res.code = req.responseCode;
                string body = req.downloadHandler != null ? req.downloadHandler.text : "";
                if (req.result != UnityWebRequest.Result.Success)
                {
                    string msg = req.error, durum = "";
                    try
                    {
                        var er = JsonUtility.FromJson<Resp>(body);
                        if (er != null && er.error != null)
                        {
                            if (!string.IsNullOrEmpty(er.error.message)) msg = er.error.message;
                            durum = er.error.status ?? "";
                        }
                    }
                    catch { }
                    var low = (body ?? "").ToLowerInvariant();
                    res.daily = res.code == 429 && (low.Contains("perday") || low.Contains("per day") || low.Contains("daily"));
                    res.anahtar = res.code == 401 || res.code == 403 || durum == "UNAUTHENTICATED" || durum == "PERMISSION_DENIED"
                                  || (res.code == 400 && (low.Contains("api key") || low.Contains("api_key_invalid")));
                    res.ag = res.code == 0 || req.result == UnityWebRequest.Result.ConnectionError;
                    res.error = msg;
                    return res;
                }
                var resp = JsonUtility.FromJson<Resp>(body);
                Inline inl = null;
                Cand c0 = resp != null && resp.candidates != null && resp.candidates.Length > 0 ? resp.candidates[0] : null;
                if (c0 != null && c0.content != null && c0.content.parts != null)
                {
                    // Ses parcasi ilk sirada olmayabilir: tum parcalarda ara
                    foreach (var p in c0.content.parts)
                        if (p != null && p.inlineData != null && !string.IsNullOrEmpty(p.inlineData.data)) { inl = p.inlineData; break; }
                }
                if (inl == null)
                {
                    string neden = "";
                    if (c0 != null && !string.IsNullOrEmpty(c0.finishReason)) neden += " Bitiş nedeni: " + c0.finishReason + ".";
                    if (resp != null && resp.promptFeedback != null && !string.IsNullOrEmpty(resp.promptFeedback.blockReason))
                        neden += " Engelleme: " + resp.promptFeedback.blockReason + ".";
                    res.error = "Cevapta ses yok." + neden;
                    return res;
                }
                res.pcm = Convert.FromBase64String(inl.data);
                if (!string.IsNullOrEmpty(inl.mimeType))
                {
                    int i = inl.mimeType.IndexOf("rate=");
                    if (i >= 0)
                    {
                        var num = new StringBuilder();
                        for (int k = i + 5; k < inl.mimeType.Length && char.IsDigit(inl.mimeType[k]); k++) num.Append(inl.mimeType[k]);
                        int rr;
                        if (int.TryParse(num.ToString(), out rr) && rr > 0) res.rate = rr;
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
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4")); else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        // ---------- Deneme sesi (kota harcamaz): sentetik "konusma" benzeri ses ----------
        // Yalniz sure/miks/ducking denemesi icindir; Gemini sesi degildir. Ayri klasore yazilir.
        public static int DenemeUret(string hikaye, StoryLibrary lib, IList<StoryItem> items)
        {
            int n = 0;
            var seen = new HashSet<string>();
            foreach (var it in items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                string k = lib.ClipKey(it);
                if (!seen.Add(k)) continue;
                string yol = DisSes.DenemeDosya(hikaye, k);
                if (File.Exists(yol)) continue;
                float sn = Mathf.Max(0.8f, TurkceMetin.BeklenenSure(TurkceMetin.ForSpeech(string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech)));
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(yol));
                    File.WriteAllBytes(yol, DisSes.WavYaz(SentetikPcm(sn, DisSes_Hash(k)), 24000));
                    n++;
                }
                catch (Exception e) { Debug.LogWarning("[Hikaye] Deneme sesi yazılamadı: " + e.Message); }
            }
            return n;
        }

        static int DisSes_Hash(string s) { int h = 17; foreach (char c in s) h = h * 31 + c; return h; }

        static byte[] SentetikPcm(float sn, int tohum)
        {
            int hz = 24000, adet = (int)(sn * hz);
            var pcm = new byte[adet * 2];
            var rnd = new System.Random(tohum);
            float temel = 110f + (tohum & 63);          // temel frekans (Hz)
            float faz = 0, silinme = 0.04f * hz;
            for (int i = 0; i < adet; i++)
            {
                float t = i / (float)hz;
                float hece = Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 3.6f));            // ~3.6 hece/sn
                float f = temel * (1f + 0.15f * Mathf.Sin(t * 2f * Mathf.PI * 0.7f));
                faz += 2f * Mathf.PI * f / hz;
                float v = Mathf.Sin(faz) + 0.5f * Mathf.Sin(2 * faz) + 0.25f * Mathf.Sin(3 * faz) + 0.1f * ((float)rnd.NextDouble() - 0.5f);
                float zarf = Mathf.Min(1f, Mathf.Min(i / silinme, (adet - i) / silinme));
                short s = (short)Mathf.Clamp(v * hece * zarf * 0.22f * 32767f, -32767f, 32767f);
                pcm[i * 2] = (byte)(s & 0xFF); pcm[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            }
            return pcm;
        }

        [Serializable] class Resp { public Cand[] candidates; public Err error; public PromptFb promptFeedback; }
        [Serializable] class Cand { public Cont content; public string finishReason; }
        [Serializable] class PromptFb { public string blockReason; }
        [Serializable] class Cont { public Part[] parts; }
        [Serializable] class Part { public Inline inlineData; }
        [Serializable] class Inline { public string mimeType; public string data; }
        [Serializable] class Err { public int code; public string message; public string status; }
    }
}
