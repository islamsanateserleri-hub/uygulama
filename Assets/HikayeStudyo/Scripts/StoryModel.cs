using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HikayeStudyo
{
    public enum ItemType
    {
        Dialogue, Background, Hook, Rewind, Title, Wait, Sfx, Remove, Clear,
        Camera, Atmosphere, Music, Pose, Headlights, Flash, Insert
    }

    public class StoryItem
    {
        public ItemType Type;
        public int Line;                 // hikaye dosyasindaki satir numarasi
        public string Character = "";
        public string Emotion = "normal";
        public bool Inner;               // ic ses: agiz oynamaz
        public string Motion = "";       // kos, yuru, saril, zipla, egil, sarsil, cik
        public string Text = "";
        public string Arg = "";
        public float Seconds;
        public int VoiceIndex;           // 1'den baslayan konusma sirasi
    }

    public class ParseResult
    {
        public readonly List<StoryItem> Items = new List<StoryItem>();
        public readonly List<string> Warnings = new List<string>();
        public int DialogueCount;
    }

    public static class Util
    {
        public static readonly string[] Emotions =
            { "normal", "mutlu", "sevgi", "uzgun", "aglamakli", "korkmus", "sasirmis", "kizgin", "yorgun", "hasta", "dusunceli", "fisilti" };
        public static readonly string[] Motions = { "kos", "yuru", "saril", "zipla", "egil", "sarsil", "cik" };
        public static readonly string[] Poses = { "ayakta", "yatiyor", "oturuyor" };

        // "Ağlamaklı" -> "aglamakli". Isimler buyuk/kucuk harf ve Turkce harf farkina bakmaz.
        public static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s.Trim())
            {
                switch (ch)
                {
                    case 'İ': case 'I': case 'ı': case 'i': sb.Append('i'); break;
                    case 'Ğ': case 'ğ': sb.Append('g'); break;
                    case 'Ü': case 'ü': sb.Append('u'); break;
                    case 'Ş': case 'ş': sb.Append('s'); break;
                    case 'Ö': case 'ö': sb.Append('o'); break;
                    case 'Ç': case 'ç': sb.Append('c'); break;
                    case ' ': case '-': sb.Append('_'); break;
                    default: sb.Append(char.ToLowerInvariant(ch)); break;
                }
            }
            return sb.ToString();
        }

        public static bool In(string value, string[] list)
        {
            var n = Norm(value);
            foreach (var x in list) if (x == n) return true;
            return false;
        }

        // Sabit (FNV-1a) ozet: ayni ses + ton + metin her zaman ayni dosya adini verir
        public static string Hash(string s)
        {
            uint h = 2166136261;
            foreach (byte b in Encoding.UTF8.GetBytes(s)) { h ^= b; h *= 16777619; }
            return h.ToString("x8");
        }

        public static float ParseFloat(string s, float def)
        {
            if (float.TryParse((s ?? "").Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return v;
            return def;
        }

        // Duygu -> Gemini'ye verilen ses tonu
        public static string ToneFor(StoryItem it)
        {
            string t;
            switch (Norm(it.Emotion))
            {
                case "mutlu": t = "sevinçli"; break;
                case "sevgi": t = "sevgi dolu, sıcak"; break;
                case "uzgun": t = "üzgün"; break;
                case "aglamakli": t = "ağlamaklı, titrek"; break;
                case "korkmus": t = "korkmuş, nefes nefese"; break;
                case "sasirmis": t = "şaşkın"; break;
                case "kizgin": t = "öfkeli"; break;
                case "yorgun": t = "yorgun, halsiz"; break;
                case "hasta": t = "hasta, bitkin"; break;
                case "dusunceli": t = "düşünceli"; break;
                case "fisilti": t = "fısıltıyla"; break;
                default: t = "doğal"; break;
            }
            return it.Inner ? "iç ses gibi sakin ve derin, " + t : t;
        }
    }

    public static class StoryParser
    {
        public static ParseResult Parse(string text)
        {
            var r = new ParseResult();
            int voice = 0, lineNo = 0;
            foreach (var raw in (text ?? "").Replace("\r", "").Split('\n'))
            {
                lineNo++;
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;

                if (line[0] == '[')
                {
                    int close = line.IndexOf(']');
                    if (close < 0) { r.Warnings.Add(lineNo + ". satır: ']' eksik."); continue; }
                    var parts = line.Substring(1, close - 1).Split('|');
                    var it = new StoryItem
                    {
                        Type = ItemType.Dialogue,
                        Line = lineNo,
                        Character = parts[0].Trim(),
                        Emotion = parts.Length > 1 && parts[1].Trim().Length > 0 ? parts[1].Trim() : "normal",
                        Text = line.Substring(close + 1).Trim(),
                    };
                    if (parts.Length > 2) it.Inner = Util.Norm(parts[2]).StartsWith("i");
                    if (parts.Length > 3) it.Motion = Util.Norm(parts[3]);
                    if (it.Text.Length == 0) { r.Warnings.Add(lineNo + ". satır: konuşma metni boş."); continue; }
                    if (!Util.In(it.Emotion, Util.Emotions)) r.Warnings.Add(lineNo + ". satır: '" + it.Emotion + "' duygusu bilinmiyor, 'normal' kullanılacak.");
                    if (it.Motion.Length > 0 && !Util.In(it.Motion, Util.Motions)) r.Warnings.Add(lineNo + ". satır: '" + it.Motion + "' hareketi bilinmiyor.");
                    it.VoiceIndex = ++voice;
                    r.Items.Add(it);
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon < 0) { r.Warnings.Add(lineNo + ". satır anlaşılmadı: " + line); continue; }
                var key = Util.Norm(line.Substring(0, colon));
                var val = line.Substring(colon + 1).Trim();
                var item = new StoryItem { Line = lineNo, Text = val, Arg = val };
                switch (key)
                {
                    case "mekan": item.Type = ItemType.Background; break;
                    case "kanca": item.Type = ItemType.Hook; break;
                    case "geri": item.Type = ItemType.Rewind; break;
                    case "yazi": item.Type = ItemType.Title; break;
                    case "bekle": item.Type = ItemType.Wait; item.Seconds = Util.ParseFloat(val, 0.5f); break;
                    case "efekt": item.Type = ItemType.Sfx; break;
                    case "cikar": item.Type = ItemType.Remove; item.Character = val; break;
                    case "temizle": item.Type = ItemType.Clear; break;
                    case "kamera": item.Type = ItemType.Camera; break;
                    case "atmosfer": item.Type = ItemType.Atmosphere; break;
                    case "muzik": item.Type = ItemType.Music; break;
                    case "far": item.Type = ItemType.Headlights; break;
                    case "flas": item.Type = ItemType.Flash; break;
                    case "gorsel": item.Type = ItemType.Insert; break;
                    case "poz":
                        item.Type = ItemType.Pose;
                        var sp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                        item.Character = sp.Length > 0 ? sp[0] : "";
                        item.Arg = sp.Length > 1 ? Util.Norm(sp[1]) : "ayakta";
                        if (!Util.In(item.Arg, Util.Poses)) r.Warnings.Add(lineNo + ". satır: '" + item.Arg + "' pozu bilinmiyor (ayakta, yatiyor, oturuyor).");
                        break;
                    default:
                        r.Warnings.Add(lineNo + ". satır: '" + key + ":' komutu bilinmiyor.");
                        continue;
                }
                r.Items.Add(item);
            }
            r.DialogueCount = voice;
            return r;
        }
    }
}
