using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // Aşama 6: isteğe bağlı Gemini metin yardımcısı. Yalnız kullanıcı düğmeye basınca çalışır ve hikâye metnini
    // Google'a gönderir. Uygulamanın diğer işlevleri buna bağlı değildir (internetsiz yardımcı: YardimciMotor).
    public static class YardimciGemini
    {
        public const string Model = "gemini-3.5-flash-lite";
        public const int AzamiKarakter = 20000;

        public static bool Busy { get; private set; }
        public static string Durum = "";
        public static string Cevap = "";
        public static bool CevapBicim;          // true: Cevap yeni hikâye metnidir
        public static string CevapKaynagi = ""; // cevabın üretildiği hikâyenin adı
        static bool durdur;

        public static void Durdur() { durdur = true; Durum = "Durduruluyor..."; }
        public static void Temizle() { if (!Busy) { Cevap = ""; Durum = ""; } }

        public static async void Calistir(string hikayeAdi, string metin, bool bicim, IList<string> karakterler, IList<string> mekanlar)
        {
            if (Busy) return;
            string anahtar = GeminiUyg.Anahtar;
            if (anahtar.Length == 0) { Durum = "API anahtarı yok (Ses bölümünden girilir). İnternetsiz öneriler çalışmaya devam eder."; return; }
            if (string.IsNullOrWhiteSpace(metin)) { Durum = "Hikâye boş."; return; }
            if (metin.Length > AzamiKarakter) { Durum = "Hikâye çok uzun (" + metin.Length + " karakter; sınır " + AzamiKarakter + ")."; return; }

            // yorum satırları (sahne açıklamaları dahil) hikâyenin parçası değildir; satır numaraları korunur
            metin = YorumlariBosalt(metin);
            Busy = true; durdur = false; Cevap = ""; CevapBicim = bicim; CevapKaynagi = hikayeAdi;
            Durum = "Gemini yanıtlıyor…";
            try
            {
                string istem = bicim
                    ? "Aşağıdaki Türkçe hikâyeyi şu biçime çevir. Her konuşma ya da anlatım satırı: [Karakter|duygu|dis] Metin (iç ses için ic). " +
                      "Anlatım için Karakter = Anlatici. Sahne değişiminde 'mekan: ad' satırı yaz. Karakterler yalnız: " + string.Join(", ", karakterler) +
                      ". Mekânlar yalnız: " + string.Join(", ", mekanlar) + ". Duygular yalnız: " + string.Join(", ", Util.Emotions) +
                      ". Olayları değiştirme. Mevcut komut satırlarını koru. Yalnız hikâye metnini döndür, açıklama ve kod çiti yazma.\n\nHİKÂYE:\n" + metin
                    : "Aşağıdaki kısa dikey video hikâyesini incele. Türkçe, düz metin, en çok 12 kısa madde yaz: tutarlılık sorunları, yazım hataları, " +
                      "sahne bazında mekân / duygu / kamera önerisi ve açılış cümlesi (kanca) önerisi. Satır numarası ver. Hikâyeyi baştan yazma.\n\nHİKÂYE (satır numaralı):\n" + Numarala(metin);

                string json = "{\"contents\":[{\"role\":\"user\",\"parts\":[{\"text\":\"" + Kacis(istem) + "\"}]}]}";
                using (var req = new UnityWebRequest(GeminiUyg.TabanAdres + Model + ":generateContent", "POST"))
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("x-goog-api-key", anahtar);
                    req.timeout = 90;
                    var op = req.SendWebRequest();
                    while (!op.isDone)
                    {
                        if (durdur) { req.Abort(); Durum = "Durduruldu."; return; }
                        await Task.Delay(100);
                    }
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        long kod = req.responseCode;
                        if (kod == 0) Durum = "İnternet bağlantısı yok. İnternetsiz öneriler kullanılabilir.";
                        else if (kod == 429) Durum = "Gemini kotası doldu (429). Daha sonra dene.";
                        else if (kod == 401 || kod == 403) Durum = "API anahtarı kabul edilmedi (" + kod + ").";
                        else Durum = "Gemini hatası " + kod + ": " + req.error;
                        return;
                    }
                    var resp = JsonUtility.FromJson<Resp>(req.downloadHandler.text);
                    var sb = new StringBuilder();
                    var c = resp != null && resp.candidates != null && resp.candidates.Length > 0 ? resp.candidates[0] : null;
                    if (c != null && c.content != null && c.content.parts != null)
                        foreach (var p in c.content.parts) if (p != null && !string.IsNullOrEmpty(p.text)) sb.Append(p.text);
                    string t = sb.ToString().Replace("\r", "").Trim();
                    if (t.StartsWith("```")) { int i = t.IndexOf('\n'); if (i >= 0) t = t.Substring(i + 1); }
                    if (t.EndsWith("```")) t = t.Substring(0, t.Length - 3);
                    Cevap = t.Trim();
                    Durum = Cevap.Length == 0 ? "Gemini boş yanıt döndürdü." : (bicim ? "Hazır. İncele, uygunsa 'Metne uygula'." : "Hazır.");
                }
            }
            catch (Exception e) { Durum = "Hata: " + e.Message; }
            finally { Busy = false; }
        }

        public static string YorumlariBosalt(string metin)
        {
            var l = (metin ?? "").Replace("\r", "").Split('\n');
            for (int i = 0; i < l.Length; i++)
            {
                string t = l[i].TrimStart();
                if (t.StartsWith("//") || t.StartsWith("#")) l[i] = "";
            }
            return string.Join("\n", l);
        }

        static string Numarala(string metin)
        {
            var l = metin.Replace("\r", "").Split('\n');
            var sb = new StringBuilder();
            for (int i = 0; i < l.Length; i++) sb.Append(i + 1).Append(": ").Append(l[i]).Append('\n');
            return sb.ToString();
        }

        static string Kacis(string s)
        {
            var sb = new StringBuilder(s.Length + 16);
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
                        if (c < 32) sb.Append(' '); else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        [Serializable] class Resp { public Cand[] candidates; }
        [Serializable] class Cand { public Cont content; }
        [Serializable] class Cont { public Part[] parts; }
        [Serializable] class Part { public string text; }
    }
}
