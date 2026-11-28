using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HikayeStudyo
{
    // Seslendirmeden once metni Turkce okunusa cevirir: rakamlar, kisaltmalar, yuzde, saat, tarih, para birimi.
    // Altyazi orijinal metni gosterir; sadece Gemini'ye giden ve ses dosyasinin adini belirleyen metin degisir.
    // Ayrica Assets/Resources/telaffuz.txt (yazilis=okunus) sozlugunu uygular.
    public static class TurkceMetin
    {
        // ---------- Genel giris ----------
        static readonly Dictionary<string, string> onbellek = new Dictionary<string, string>();
        static TextAsset sozlukAsset;
        static List<KeyValuePair<Regex, string>> sozluk = new List<KeyValuePair<Regex, string>>();
        static float sonKontrol = -100f;

        static readonly Regex EtiketAyir = new Regex("(<[^>]*>)", RegexOptions.Compiled);

        // Seslendirmeye gidecek son metin: sayilar/kisaltmalar + telaffuz sozlugu. Etiketler (<sigh> vb.) korunur.
        public static string ForSpeech(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            SozlukKontrol();
            string r;
            if (onbellek.TryGetValue(s, out r)) return r;
            r = Uygula(s, true);
            if (onbellek.Count > 4000) onbellek.Clear();
            onbellek[s] = r;
            return r;
        }

        // Sadece sayi/kisaltma normalizasyonu (sozluk olmadan)
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return Uygula(s, false);
        }

        public static void Reload()
        {
            onbellek.Clear();
            sozlukAsset = null;
            sonKontrol = -100f;
            SozlukKontrol();
        }

        static string Uygula(string s, bool sozlukluMu)
        {
            var parts = EtiketAyir.Split(s);
            var sb = new StringBuilder(s.Length + 32);
            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i];
                if (p.Length == 0) continue;
                if (p[0] == '<' && p[p.Length - 1] == '>') { sb.Append(p); continue; }   // ses etiketi: dokunma
                p = Parca(p);
                if (sozlukluMu) p = SozlukUygula(p);
                sb.Append(p);
            }
            return sb.ToString();
        }

        // ---------- Telaffuz sozlugu ----------
        static void SozlukKontrol()
        {
            float now = Time.realtimeSinceStartup;
            if (now - sonKontrol < 1.5f) return;
            sonKontrol = now;
            TextAsset a = null;
            try { a = Resources.Load<TextAsset>("telaffuz"); } catch { }
            if (a == sozlukAsset && sozluk != null) return;
            sozlukAsset = a;
            onbellek.Clear();
            sozluk = SozlukOku(a != null ? a.text : "");
        }

        static List<KeyValuePair<Regex, string>> SozlukOku(string text)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var raw in (text ?? "").Replace("\r", "").Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string k = line.Substring(0, eq).Trim(), v = line.Substring(eq + 1).Trim();
                if (k.Length == 0) continue;
                list.Add(new KeyValuePair<string, string>(k, v));
            }
            list.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));   // uzun anahtar once
            var res = new List<KeyValuePair<Regex, string>>();
            foreach (var kv in list)
            {
                try
                {
                    var rx = new Regex("(?<![\\p{L}\\p{N}])" + Regex.Escape(kv.Key) + "(?![\\p{L}\\p{N}])",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    res.Add(new KeyValuePair<Regex, string>(rx, kv.Value));
                }
                catch { }
            }
            return res;
        }

        static string SozlukUygula(string s)
        {
            if (sozluk == null || sozluk.Count == 0) return s;
            foreach (var kv in sozluk)
            {
                string yeni = kv.Value;
                s = kv.Key.Replace(s, m => yeni);
            }
            return s;
        }

        // ---------- Normalizasyon ----------
        static readonly Dictionary<string, string> Kisaltma = new Dictionary<string, string>
        {
            { "Dr", "Doktor" }, { "dr", "doktor" }, { "Prof", "Profesör" }, { "Doç", "Doçent" }, { "Av", "Avukat" },
            { "Müh", "Mühendis" }, { "Yrd", "Yardımcı" }, { "Sn", "Sayın" }, { "Bkz", "Bakınız" }, { "bkz", "bakınız" },
            { "vb", "ve benzeri" }, { "vs", "vesaire" }, { "vd", "ve diğerleri" }, { "örn", "örneğin" }, { "Örn", "Örneğin" },
            { "yy", "yüzyıl" }, { "No", "numara" }, { "no", "numara" }, { "Tel", "telefon" }, { "Cad", "Caddesi" },
            { "Sok", "Sokak" }, { "Mah", "Mahallesi" }, { "Apt", "Apartmanı" }, { "Kpt", "Kaptan" }, { "Alb", "Albay" },
            { "Gen", "General" }, { "Uzm", "Uzman" }, { "Öğr", "Öğretmen" }, { "Hz", "Hazreti" }, { "St", "Aziz" },
            { "Bşk", "Başkan" }, { "Müd", "Müdür" }, { "Hnm", "Hanım" }, { "Bay", "Bay" }
        };
        static Regex kisaltmaRx;
        static readonly Regex MoMs = new Regex("(?<![\\p{L}\\d])M\\.\\s?([ÖS])\\.", RegexOptions.Compiled);
        static readonly Regex Tarih = new Regex("(?<![\\d.,/])(\\d{1,2})[./](\\d{1,2})[./](\\d{4})(?![\\d])", RegexOptions.Compiled);
        static readonly Regex Saat = new Regex("(?<![\\d:])(\\d{1,2}):(\\d{2})(?![\\d:])", RegexOptions.Compiled);
        static readonly Regex YuzdeOnce = new Regex("%\\s?(\\d+(?:[.,]\\d+)?)", RegexOptions.Compiled);
        static readonly Regex YuzdeSonra = new Regex("(\\d+(?:[.,]\\d+)?)\\s?%", RegexOptions.Compiled);
        static readonly Regex Lira = new Regex("(\\d+(?:[.,]\\d+)*)\\s?(?:TL|₺)(?![\\p{L}])", RegexOptions.Compiled);
        static readonly Regex Dolar = new Regex("(?:\\$\\s?(\\d+(?:[.,]\\d+)*))|(?:(\\d+(?:[.,]\\d+)*)\\s?\\$)", RegexOptions.Compiled);
        static readonly Regex Avro = new Regex("(?:€\\s?(\\d+(?:[.,]\\d+)*))|(?:(\\d+(?:[.,]\\d+)*)\\s?€)", RegexOptions.Compiled);
        static readonly Regex Birim = new Regex("(?<=\\d)\\s?(km|kg|cm|mm|dk|sn|gr)(?![\\p{L}\\d])", RegexOptions.Compiled);
        static readonly Regex Metre = new Regex("(?<=\\d)m(?![\\p{L}\\d])", RegexOptions.Compiled);
        static readonly Regex Derece = new Regex("°\\s?C", RegexOptions.Compiled);
        static readonly Regex Kesme = new Regex("(\\d+)['’](\\p{L}+)", RegexOptions.Compiled);
        static readonly Regex Sirali = new Regex("(?<![\\p{L}\\d.,])(\\d{1,3})\\.(?=\\s+(\\p{L}+))", RegexOptions.Compiled);
        static readonly Regex Binlik = new Regex("(?<![\\p{L}\\d])\\d{1,3}(?:\\.\\d{3})+(?:,\\d+)?(?!\\d)", RegexOptions.Compiled);
        static readonly Regex Ondalik = new Regex("(?<![\\p{L}\\d.,])(\\d+)[,.](\\d{1,2})(?![\\d])", RegexOptions.Compiled);
        static readonly Regex Duz = new Regex("\\d+", RegexOptions.Compiled);

        // Sirali sayi okunusu icin sonraki kelimeler (buyuk harfle baslasa bile)
        static readonly string[] SiraliKelime =
        {
            "bölüm", "bölümde", "bölümü", "kat", "katta", "katı", "kata", "sınıf", "sınıfta", "sınıfı", "gün", "günü", "günde",
            "kez", "defa", "yüzyıl", "yüzyılda", "yıl", "yıldönümü", "sezon", "hafta", "haftası", "nesil", "sayfa", "dünya", "lig",
            "koğuş", "oda", "odada", "sokak", "cadde", "perde", "sahne", "gece", "ay", "dönem", "sefer", "yaş", "yaşında", "yaşını",
            "kısım", "kitap", "cilt", "adım", "seviye", "basamak", "çeyrek", "tur", "turda", "bölge", "ünite", "ders", "sıra"
        };

        static string Parca(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            bool sayiVar = false;
            foreach (char c in s) if (char.IsDigit(c)) { sayiVar = true; break; }
            bool isaretVar = s.IndexOf('%') >= 0 || s.IndexOf('$') >= 0 || s.IndexOf('€') >= 0 || s.IndexOf('₺') >= 0 ||
                             s.IndexOf('°') >= 0 || s.IndexOf('&') >= 0 || s.IndexOf('.') >= 0;
            if (!sayiVar && !isaretVar) return s;

            // 1) kisaltmalar (nokta ile biten)
            s = MoMs.Replace(s, m => m.Groups[1].Value == "Ö" ? "Milattan önce" : "Milattan sonra");
            if (kisaltmaRx == null)
            {
                var keys = new List<string>(Kisaltma.Keys);
                keys.Sort((a, b) => b.Length.CompareTo(a.Length));
                var esc = new List<string>();
                foreach (var k in keys) esc.Add(Regex.Escape(k));
                kisaltmaRx = new Regex("(?<![\\p{L}\\d])(" + string.Join("|", esc.ToArray()) + ")\\.(?![\\p{L}])", RegexOptions.CultureInvariant);
            }
            s = kisaltmaRx.Replace(s, m => Kisaltma[m.Groups[1].Value]);

            if (s.IndexOf('&') >= 0) s = s.Replace(" & ", " ve ").Replace("&", " ve ");

            if (!sayiVar && s.IndexOf('%') < 0) return s;

            // 2) tarih, saat
            s = Tarih.Replace(s, m =>
            {
                int g = PI(m.Groups[1].Value), a = PI(m.Groups[2].Value);
                if (g < 1 || g > 31 || a < 1 || a > 12) return m.Value;
                return SayiYaz(g) + " " + Aylar[a - 1] + " " + OkuSayi(m.Groups[3].Value);
            });
            s = Saat.Replace(s, m =>
            {
                int h = PI(m.Groups[1].Value), mi = PI(m.Groups[2].Value);
                if (h < 0 || mi < 0 || h > 24 || mi > 59) return m.Value;
                string t = SayiYaz(h);
                if (mi == 0) return t;
                return t + " " + (m.Groups[2].Value[0] == '0' ? "sıfır " : "") + SayiYaz(mi);
            });

            // 3) yuzde, para, birim
            s = YuzdeOnce.Replace(s, m => "yüzde " + m.Groups[1].Value);
            s = YuzdeSonra.Replace(s, m => "yüzde " + m.Groups[1].Value);
            s = s.Replace("%", " yüzde ");
            s = Lira.Replace(s, m => m.Groups[1].Value + " lira");
            s = Dolar.Replace(s, m => (m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value) + " dolar");
            s = Avro.Replace(s, m => (m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value) + " avro");
            s = Birim.Replace(s, m => " " + BirimAdi(m.Groups[1].Value));
            s = Metre.Replace(s, " metre");
            s = Derece.Replace(s, " derece");

            // 4) sayilar
            s = Kesme.Replace(s, m => OkuSayi(m.Groups[1].Value) + m.Groups[2].Value);
            s = Sirali.Replace(s, m =>
            {
                int n = PI(m.Groups[1].Value);
                if (n < 0) return m.Value;
                string next = m.Groups[2].Value;
                bool ordinal = n <= 100 && (next.Length > 0 && char.IsLower(next[0]) || System.Array.IndexOf(SiraliKelime, next.ToLowerInvariant()) >= 0);
                return ordinal ? SiraliYaz(n) : SayiYaz(n) + ".";
            });
            s = Binlik.Replace(s, m => OkuOndalik(m.Value));
            s = Ondalik.Replace(s, m => OkuSayi(m.Groups[1].Value) + " virgül " + OkuBasamak(m.Groups[2].Value));
            s = Duz.Replace(s, m => OkuSayi(m.Value));
            return s.Replace("  ", " ");
        }

        // Yalnizca ASCII rakamlari kabul eder; olmazsa -1
        static int PI(string v)
        {
            int r;
            return int.TryParse(v, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out r) ? r : -1;
        }

        static string BirimAdi(string b)
        {
            switch (b)
            {
                case "km": return "kilometre";
                case "kg": return "kilo";
                case "cm": return "santimetre";
                case "mm": return "milimetre";
                case "dk": return "dakika";
                case "sn": return "saniye";
                case "gr": return "gram";
            }
            return b;
        }

        // "1.234,5" -> "bin iki yüz otuz dört virgül beş"
        static string OkuOndalik(string v)
        {
            string tam = v, kesir = "";
            int c = v.IndexOf(',');
            if (c >= 0) { tam = v.Substring(0, c); kesir = v.Substring(c + 1); }
            tam = tam.Replace(".", "");
            string r = OkuSayi(tam);
            if (kesir.Length > 0) r += " virgül " + OkuBasamak(kesir);
            return r;
        }

        // Ondalik kesir: "05" -> "sıfır beş", "5" -> "beş", "25" -> "yirmi beş"
        static string OkuBasamak(string d)
        {
            var sb = new StringBuilder();
            int i = 0;
            while (i < d.Length && d[i] == '0') { sb.Append("sıfır "); i++; }
            if (i < d.Length)
            {
                long v;
                if (long.TryParse(d.Substring(i), out v)) sb.Append(SayiYaz(v));
            }
            return sb.ToString().Trim();
        }

        // Duz rakam dizisi: cok uzun ya da 0 ile basliyorsa (telefon, kod) tek tek okunur
        static string OkuSayi(string digits)
        {
            if (digits.Length > 12 || (digits.Length > 1 && digits[0] == '0'))
            {
                var sb = new StringBuilder();
                foreach (char c in digits)
                {
                    int d = c - '0';
                    if (d < 0 || d > 9) continue;
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(Birler[d]);
                }
                return sb.ToString();
            }
            long v;
            return long.TryParse(digits, out v) ? SayiYaz(v) : digits;
        }

        static readonly string[] Birler = { "sıfır", "bir", "iki", "üç", "dört", "beş", "altı", "yedi", "sekiz", "dokuz" };
        static readonly string[] Onlar = { "", "on", "yirmi", "otuz", "kırk", "elli", "altmış", "yetmiş", "seksen", "doksan" };
        static readonly string[] Basamaklar = { "", "bin", "milyon", "milyar", "trilyon", "katrilyon" };
        static readonly string[] Aylar =
            { "ocak", "şubat", "mart", "nisan", "mayıs", "haziran", "temmuz", "ağustos", "eylül", "ekim", "kasım", "aralık" };

        static string Uc(int g)
        {
            var sb = new StringBuilder();
            int y = g / 100, o = (g / 10) % 10, b = g % 10;
            if (y == 1) sb.Append("yüz");
            else if (y > 1) sb.Append(Birler[y]).Append(" yüz");
            if (o > 0) { if (sb.Length > 0) sb.Append(' '); sb.Append(Onlar[o]); }
            if (b > 0) { if (sb.Length > 0) sb.Append(' '); sb.Append(Birler[b]); }
            return sb.ToString();
        }

        // 1993 -> "bin dokuz yüz doksan üç"
        public static string SayiYaz(long n)
        {
            if (n == 0) return "sıfır";
            if (n < 0) return "eksi " + SayiYaz(-n);
            var groups = new List<int>();
            long x = n;
            while (x > 0) { groups.Add((int)(x % 1000)); x /= 1000; }
            if (groups.Count > Basamaklar.Length) return n.ToString();
            var parts = new List<string>();
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                int g = groups[i];
                if (g == 0) continue;
                if (i == 1 && g == 1) { parts.Add("bin"); continue; }   // "bir bin" denmez
                string t = Uc(g);
                if (i > 0) t += " " + Basamaklar[i];
                parts.Add(t);
            }
            return string.Join(" ", parts.ToArray());
        }

        // 3 -> "üçüncü", 21 -> "yirmi birinci"
        public static string SiraliYaz(long n)
        {
            string w = SayiYaz(n);
            int sp = w.LastIndexOf(' ');
            string head = sp >= 0 ? w.Substring(0, sp + 1) : "";
            string last = sp >= 0 ? w.Substring(sp + 1) : w;
            string o;
            switch (last)
            {
                case "sıfır": o = "sıfırıncı"; break;
                case "bir": o = "birinci"; break;
                case "iki": o = "ikinci"; break;
                case "üç": o = "üçüncü"; break;
                case "dört": o = "dördüncü"; break;
                case "beş": o = "beşinci"; break;
                case "altı": o = "altıncı"; break;
                case "yedi": o = "yedinci"; break;
                case "sekiz": o = "sekizinci"; break;
                case "dokuz": o = "dokuzuncu"; break;
                case "on": o = "onuncu"; break;
                case "yirmi": o = "yirminci"; break;
                case "otuz": o = "otuzuncu"; break;
                case "kırk": o = "kırkıncı"; break;
                case "elli": o = "ellinci"; break;
                case "altmış": o = "altmışıncı"; break;
                case "yetmiş": o = "yetmişinci"; break;
                case "seksen": o = "sekseninci"; break;
                case "doksan": o = "doksanıncı"; break;
                case "yüz": o = "yüzüncü"; break;
                case "bin": o = "bininci"; break;
                case "milyon": o = "milyonuncu"; break;
                case "milyar": o = "milyarıncı"; break;
                case "trilyon": o = "trilyonuncu"; break;
                default: o = last + "inci"; break;
            }
            return head + o;
        }

        // ---------- Sure tahmini (kesik ses denetimi icin) ----------
        // Turkce'de hece sayisi = unlu harf sayisi
        public static int HeceSayisi(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int n = 0;
            bool etiket = false;
            foreach (char ch in s)
            {
                if (ch == '<') { etiket = true; continue; }
                if (ch == '>') { etiket = false; continue; }
                if (etiket) continue;
                switch (char.ToLowerInvariant(ch))
                {
                    case 'a': case 'e': case 'ı': case 'i': case 'o': case 'ö': case 'u': case 'ü':
                    case 'â': case 'î': case 'û': case 'I': case 'İ':
                        n++; break;
                }
            }
            return n;
        }

        // Beklenen konusma suresi (sn): ~6 hece/sn + etiket araliklari
        public static float BeklenenSure(string speech)
        {
            float t = HeceSayisi(speech) / 6f;
            foreach (Match m in Regex.Matches(speech ?? "", "<([^>]*)>"))
            {
                string tag = m.Groups[1].Value.ToLowerInvariant();
                if (tag == "long pause") t += 0.9f;
                else if (tag == "short pause") t += 0.35f;
                else t += 0.4f;
            }
            return t;
        }
    }
}
