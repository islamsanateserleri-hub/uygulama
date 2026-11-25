using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Basit JSON yazma yardimcilari (JsonUtility dizi/dinamik yapilar icin yetmez)
    public static class HsJson
    {
        public static string Esc(string s)
        {
            if (s == null) return "";
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
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

        public static string Q(string s) { return "\"" + Esc(s) + "\""; }

        public static string B(bool b) { return b ? "true" : "false"; }

        public static string F(float f) { return f.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture); }

        public static string Dizi(IEnumerable<string> quoted)
        {
            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var q in quoted) { if (!first) sb.Append(", "); sb.Append(q); first = false; }
            return sb.Append("]").ToString();
        }

        public static void Yaz(string path, string content)
        {
            try { File.WriteAllText(path, content, new UTF8Encoding(false)); }
            catch (System.Exception e) { Debug.LogWarning("[Hikaye] Yazılamadı: " + path + " (" + e.Message + ")"); }
        }
    }

    // Dogrulama sonucundaki tek bir sorun
    public class Sorun
    {
        public int Satir;                 // 1'den; 0 = genel
        public string Kod = "";           // KARAKTER_YOK, MEKAN_YOK, KOMUT_BILINMIYOR ...
        public string Mesaj = "";
        public string Oneri = "";         // "Bunu mu demek istediniz: Anne?"
        public string Yanlis = "";        // satirdaki hatali parca
        public string Duzeltme = "";      // yerine yazilacak parca
        public bool Hata;                 // true = hata (hikaye_yaz reddeder), false = uyari
        public string Seviye { get { return Hata ? "hata" : "uyari"; } }
    }

    public class SahneBilgi
    {
        public int Satir;
        public string Ad = "";
        public float Sure;
        public int Konusma, Kelime;
    }

    public class SureOzeti
    {
        public float Sure;
        public int Konusma, Kelime, EksikSes;
        public bool GercekSesVar;
        public readonly List<SahneBilgi> Sahneler = new List<SahneBilgi>();
    }

    // Sure/kelime hesabi ve sahne bolme
    public static class StudyoAnaliz
    {
        public static float ItemSure(StoryItem it, StoryLibrary lib, string story, ref bool gercek)
        {
            switch (it.Type)
            {
                case ItemType.Dialogue:
                    {
                        float len = (lib != null && !string.IsNullOrEmpty(story)) ? GeminiSes.ClipSeconds(story, lib, it) : -1f;
                        if (len > 0f) { gercek = true; return len + 0.3f + 0.22f; }
                        return Mathf.Max(1.2f, it.Text.Length / 14f + 0.3f) + 0.22f;
                    }
                case ItemType.Hook: return 1.8f;
                case ItemType.Rewind: return 1.6f;
                case ItemType.Title: return 2.5f;
                case ItemType.Wait: return it.Seconds;
                case ItemType.Background: return 0.6f;
                case ItemType.Headlights: return 1.5f;
                case ItemType.End: return it.Arg == "sert" ? 1.2f : 2.8f;
                case ItemType.Choice: return 4f;
                case ItemType.Result: return 2.2f;
            }
            return 0f;
        }

        public static int Kelime(StoryItem it)
        {
            if (it.Type != ItemType.Dialogue && it.Type != ItemType.Hook) return 0;
            return it.Text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static SureOzeti Hesapla(ParseResult p, StoryLibrary lib, string story)
        {
            var o = new SureOzeti();
            SahneBilgi cur = null;
            bool gercek = false;
            foreach (var it in p.Items)
            {
                if (it.Type == ItemType.Background)
                {
                    cur = new SahneBilgi { Satir = it.Line, Ad = it.Arg };
                    o.Sahneler.Add(cur);
                }
                else if (cur == null)
                {
                    if (!HasContent(it)) continue;
                    cur = new SahneBilgi { Satir = it.Line, Ad = "(giriş)" };
                    o.Sahneler.Add(cur);
                }
                float s = ItemSure(it, lib, story, ref gercek);
                cur.Sure += s;
                o.Sure += s;
                if (it.Type == ItemType.Dialogue)
                {
                    cur.Konusma++;
                    o.Konusma++;
                    if (lib != null && !string.IsNullOrEmpty(story) && !GeminiSes.HasClip(story, lib, it)) o.EksikSes++;
                }
                int k = Kelime(it);
                cur.Kelime += k;
                o.Kelime += k;
            }
            o.GercekSesVar = gercek;
            return o;
        }

        static bool HasContent(StoryItem it)
        {
            switch (it.Type)
            {
                case ItemType.Dialogue: case ItemType.Hook: case ItemType.Rewind: case ItemType.Title: case ItemType.Wait:
                case ItemType.Headlights: case ItemType.End: case ItemType.Choice: case ItemType.Result:
                    return true;
            }
            return false;
        }

        // Yonetmen/yuz/altyazi gibi yeni komut turleri (enum'a sonradan eklenir; adla taninir ki derleme bagimsiz kalsin)
        public static bool SahneKomutu(ItemType t)
        {
            string n = t.ToString();
            return n == "Direct" || n == "Bak" || n == "Dinle" || n == "AltyaziStil";
        }

        // Kanca ilk 1,5 saniyede mi? (varsa true)  any=false: icerik yok
        public static bool KancaTamam(ParseResult p, out bool any, out int satir)
        {
            float t = 0.35f; bool ok = false; any = false; satir = 1;
            foreach (var it in p.Items)
            {
                if (it.Type == ItemType.Background || it.Type == ItemType.Music || it.Type == ItemType.Atmosphere ||
                    it.Type == ItemType.Camera || it.Type == ItemType.Pose || it.Type == ItemType.Cast || SahneKomutu(it.Type))
                { t += it.Type == ItemType.Background ? 0.3f : 0f; continue; }
                any = true;
                satir = it.Line;
                if (it.Type == ItemType.Hook || it.Type == ItemType.Dialogue || it.Type == ItemType.Sfx ||
                    it.Type == ItemType.Headlights || it.Type == ItemType.Flash || it.Type == ItemType.Result) ok = t <= 1.5f;
                break;
            }
            return ok;
        }

        // Bolum sonu (son: / yazi:) var mi
        public static bool SonVar(ParseResult p)
        {
            foreach (var it in p.Items) if (it.Type == ItemType.End || it.Type == ItemType.Title) return true;
            return false;
        }

        // Varsa pacing denetcisini (RitimDenetci) yansima ile cagirir; yoksa bos liste doner
        public static List<string> RitimRaporu(ParseResult p, StoryLibrary lib)
        {
            var res = new List<string>();
            try
            {
                var t = AsmYardim.TipBul("HikayeStudyo.RitimDenetci");
                if (t == null) return res;
                foreach (var m in t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    var ps = m.GetParameters();
                    if (ps.Length == 0 || ps.Length > 2) continue;
                    if (!ps[0].ParameterType.IsAssignableFrom(typeof(ParseResult))) continue;
                    object[] args;
                    if (ps.Length == 1) args = new object[] { p };
                    else if (ps[1].ParameterType.IsAssignableFrom(typeof(StoryLibrary))) args = new object[] { p, lib };
                    else continue;
                    object r = m.Invoke(null, args);
                    var s = r as string;
                    if (s != null) { if (s.Trim().Length > 0) res.AddRange(s.Replace("\r", "").Split('\n')); return res; }
                    var en = r as System.Collections.IEnumerable;
                    if (en != null)
                    {
                        foreach (var o in en) if (o != null) res.Add(o.ToString());
                        return res;
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Hikaye] RitimDenetci çağrılamadı: " + e.Message); }
            return res;
        }
    }

    // Hikaye metnini kutuphaneye gore denetler; sorunlari satir, kod, mesaj ve "Bunu mu demek istediniz" onerisiyle verir.
    public static class StudyoDogrulama
    {
        static readonly string[] HazirEfektler =
            { "carpma", "korna", "geri", "whoosh", "gecis", "kapi", "buyu", "kalp", "tahta", "kase", "yagmur", "flas", "gok", "gokgurultusu" };

        static readonly Dictionary<string, bool> komutOnbellek = new Dictionary<string, bool>();
        static string[] oyuncuAdlari;
        static double oyuncuZaman = -100;

        // ---------- Benzerlik ----------
        public static int Uzaklik(string a, string b)
        {
            if (a == null) a = "";
            if (b == null) b = "";
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;
            var prev = new int[b.Length + 1];
            var cur = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++) prev[j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                cur[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    cur[j] = Mathf.Min(Mathf.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
                }
                var tmp = prev; prev = cur; cur = tmp;
            }
            return prev[b.Length];
        }

        // En yakin aday (yoksa null). Karsilastirma Turkce harf/buyuk-kucuk harf duyarsiz.
        public static string EnYakin(string yanlis, IEnumerable<string> adaylar)
        {
            string n = Util.Norm(yanlis);
            if (n.Length == 0 || adaylar == null) return null;
            string best = null; int bd = int.MaxValue;
            foreach (var c in adaylar)
            {
                if (string.IsNullOrEmpty(c)) continue;
                string cn = Util.Norm(c);
                if (cn == n) continue;   // zaten dogru
                int d = Uzaklik(n, cn);
                if (n.Length >= 3 && cn.Contains(n)) d = Mathf.Min(d, 1);
                if (cn.Length >= 4 && n.Contains(cn)) d = Mathf.Min(d, 1);
                if (d < bd) { bd = d; best = c; }
            }
            int lim = n.Length <= 3 ? 1 : (n.Length <= 6 ? 2 : 3);
            return best != null && bd <= lim ? best : null;
        }

        static void Ekle(List<Sorun> l, int satir, bool hata, string kod, string mesaj, string yanlis, string aday)
        {
            var s = new Sorun { Satir = satir, Hata = hata, Kod = kod, Mesaj = mesaj, Yanlis = yanlis ?? "" };
            if (!string.IsNullOrEmpty(aday)) { s.Duzeltme = aday; s.Oneri = "Bunu mu demek istediniz: " + aday + "?"; }
            l.Add(s);
        }

        static List<string> KarakterAdlari(StoryLibrary lib)
        {
            var l = new List<string>();
            if (lib == null) return l;
            if (lib.narrator != null && !string.IsNullOrEmpty(lib.narrator.id)) l.Add(lib.narrator.id);
            foreach (var c in lib.characters) if (c != null && !string.IsNullOrEmpty(c.id)) l.Add(c.id);
            return l;
        }

        public static List<string> MekanAdlari(StoryLibrary lib)
        {
            var l = new List<string>();
            if (lib != null) foreach (var b in lib.backgrounds) if (b != null && !string.IsNullOrEmpty(b.id)) l.Add(b.id);
            return l;
        }

        public static List<string> EfektAdlari(StoryLibrary lib)
        {
            var l = new List<string>();
            if (lib != null) foreach (var s in lib.sfx) if (s != null && s.clip != null && !string.IsNullOrEmpty(s.id)) l.Add(s.id);
            foreach (var e in HazirEfektler) if (!l.Contains(e)) l.Add(e);
            return l;
        }

        // 3D oyuncu prefab adlari (Karakterler/Hazir)
        public static string[] OyuncuAdlari()
        {
            if (oyuncuAdlari == null || EditorApplication.timeSinceStartup - oyuncuZaman > 5.0)
            {
                var l = new List<string>();
                try
                {
                    if (AssetDatabase.IsValidFolder(Karakter3DKurulum.Hazir))
                        foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { Karakter3DKurulum.Hazir }))
                            l.Add(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(g)));
                }
                catch { }
                l.Sort();
                oyuncuAdlari = l.ToArray();
                oyuncuZaman = EditorApplication.timeSinceStartup;
            }
            return oyuncuAdlari;
        }

        static bool OyuncuVar(string ad)
        {
            string n = Util.Norm(ad);
            if (n.Length == 0) return false;
            foreach (var g in OyuncuAdlari()) { if (Util.Norm(g) == n) return true; }
            foreach (var g in OyuncuAdlari()) { if (Util.Norm(g).EndsWith("_" + n)) return true; }
            foreach (var g in OyuncuAdlari()) { if (Util.Norm(g).Contains(n)) return true; }
            return Varyantlar.AdGecerli(ad, OyuncuAdlari());   // karakter çeşidi: VRM_Anne_005
        }

        // Komut parser'da var mi? Kayit defterine ve parser'in kendisine bakar (StoryModel degisse de dogru kalir).
        public static bool KomutGecerli(string normKey)
        {
            if (KomutKayit.Var(normKey)) return true;
            bool v;
            if (komutOnbellek.TryGetValue(normKey, out v)) return v;
            bool gecerli = true;
            try
            {
                var r = StoryParser.Parse(normKey + ": 1");
                foreach (var w in r.Warnings) if (w.IndexOf("komutu bilinmiyor", System.StringComparison.OrdinalIgnoreCase) >= 0) { gecerli = false; break; }
            }
            catch { gecerli = true; }
            komutOnbellek[normKey] = gecerli;
            return gecerli;
        }

        public static List<string> KomutAdlari()
        {
            StoryParser.Parse("");   // parser'in statik kayitlarini tetikle
            var l = new List<string>();
            foreach (var b in KomutKayit.Hepsi()) l.Add(b.Ad);
            return l;
        }

        static int ParserSatir(string w)
        {
            var m = Regex.Match(w ?? "", "^(\\d+)\\.\\s*sat");
            int n;
            return m.Success && int.TryParse(m.Groups[1].Value, out n) ? n : 0;
        }

        // ---------- Ana dogrulama ----------
        // story: ses kontrolu icin hikaye adi (null ise ses kontrol edilmez)
        public static List<Sorun> Dogrula(string text, StoryLibrary lib, string story, bool sesKontrol)
        {
            var list = new List<Sorun>();
            var p = StoryParser.Parse(text ?? "");   // once calistir: statik kayitlar hazir olsun
            var lines = (text ?? "").Replace("\r", "").Split('\n');
            var komutlar = KomutAdlari();
            var kars = KarakterAdlari(lib);
            var mekanlar = MekanAdlari(lib);
            bool ilkMekanGoruldu = false, ilkKonusmaUyarildi = false;

            for (int i = 0; i < lines.Length; i++)
            {
                int no = i + 1;
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;

                if (line[0] == '[')
                {
                    int close = line.IndexOf(']');
                    if (close < 0)
                    {
                        Ekle(list, no, true, "PARANTEZ_EKSIK", "']' eksik. Konuşma satırı: [Karakter|duygu|dis] Metin", "", "");
                        continue;
                    }
                    var parts = line.Substring(1, close - 1).Split('|');
                    string ch = parts[0].Trim();
                    if (ch.Length == 0) Ekle(list, no, true, "KARAKTER_BOS", "Karakter adı boş: [Arda|normal|dis] Metin", "", "");
                    else if (lib != null && lib.FindCharacter(ch) == null)
                    {
                        string o = EnYakin(ch, kars);
                        Ekle(list, no, true, "KARAKTER_YOK", "'" + ch + "' karakteri kütüphanede yok. Kullanılabilir: " + string.Join(", ", kars.ToArray()) + ".", ch, o);
                    }
                    if (parts.Length > 1 && parts[1].Trim().Length > 0 && !Util.In(parts[1].Trim(), Util.Emotions))
                    {
                        string o = EnYakin(parts[1].Trim(), Util.Emotions);
                        Ekle(list, no, true, "DUYGU_BILINMIYOR", "'" + parts[1].Trim() + "' duygusu bilinmiyor. Geçerli: " + string.Join(", ", Util.Emotions) + ".", parts[1].Trim(), o);
                    }
                    if (parts.Length > 2 && parts[2].Trim().Length > 0)
                    {
                        string m = Util.Norm(parts[2]);
                        if (!m.StartsWith("i") && !m.StartsWith("d"))
                            Ekle(list, no, false, "MOD_BILINMIYOR", "Üçüncü alan 'dis' ya da 'ic' olmalı ('" + parts[2].Trim() + "' bulundu); dışarıdan konuşma varsayılır.", parts[2].Trim(), EnYakin(parts[2].Trim(), new[] { "dis", "ic" }));
                    }
                    if (parts.Length > 3 && parts[3].Trim().Length > 0 && !Util.In(parts[3].Trim(), Util.Motions))
                    {
                        string o = EnYakin(parts[3].Trim(), Util.Motions);
                        Ekle(list, no, true, "HAREKET_BILINMIYOR", "'" + parts[3].Trim() + "' hareketi bilinmiyor. Geçerli: " + string.Join(", ", Util.Motions) + ".", parts[3].Trim(), o);
                    }
                    if (line.Substring(close + 1).Trim().Length == 0)
                        Ekle(list, no, true, "BOS_METIN", "Konuşma metni boş.", "", "");
                    if (!ilkMekanGoruldu && !ilkKonusmaUyarildi)
                    {
                        ilkKonusmaUyarildi = true;
                        Ekle(list, no, false, "MEKAN_ILK", "İlk konuşmadan önce 'mekan:' yok; arka plan boş kalabilir.", "", "");
                    }
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon < 0)
                {
                    Ekle(list, no, true, "SATIR_ANLASILMADI", "Satır anlaşılmadı. Konuşma: [Karakter|duygu|dis] Metin — komut: komut: değer. (Yorum için satırı // ile başlat.)", "", "");
                    continue;
                }
                string rawKey = line.Substring(0, colon).Trim();
                string key = Util.Norm(rawKey);
                string val = line.Substring(colon + 1).Trim();
                if (key.Length == 0 || !KomutGecerli(key))
                {
                    string o = EnYakin(rawKey, komutlar);
                    Ekle(list, no, true, "KOMUT_BILINMIYOR", "'" + rawKey + ":' komutu bilinmiyor.", rawKey, o);
                    continue;
                }
                if (key == "mekan") ilkMekanGoruldu = true;

                switch (key)
                {
                    case "mekan":
                        if (lib != null && val.Length > 0 && lib.FindBackground(val) == null)
                            Ekle(list, no, true, "MEKAN_YOK", "'" + val + "' mekanı kütüphanede yok. Kullanılabilir: " + string.Join(", ", mekanlar.ToArray()) + ".", val, EnYakin(val, mekanlar));
                        break;
                    case "efekt":
                        if (lib != null && val.Length > 0 && lib.FindSfx(val) == null && !ProcAudio.Has(val))
                            Ekle(list, no, true, "EFEKT_YOK", "'" + val + "' efekti yok. Hazır olanlar: " + string.Join(", ", EfektAdlari(lib).ToArray()) + ".", val, EnYakin(val, EfektAdlari(lib)));
                        break;
                    case "muzik":
                        {
                            string n = Util.Norm(val);
                            if (lib != null && n.Length > 0 && n != "pad" && n != "ambiyans" && n != "yok" && lib.FindMusic(val) == null)
                            {
                                var cand = new List<string> { "pad", "yok" };
                                foreach (var m in lib.music) if (m != null && !string.IsNullOrEmpty(m.id)) cand.Add(m.id);
                                Ekle(list, no, true, "MUZIK_YOK", "'" + val + "' müziği yok (pad, yok ya da kütüphanedeki müzik).", val, EnYakin(val, cand));
                            }
                        }
                        break;
                    case "gorsel":
                        {
                            string n = Util.Norm(val);
                            if (lib != null && n.Length > 0 && n != "eller" && n != "yok" && lib.FindInsert(val) == null)
                            {
                                var cand = new List<string> { "eller", "yok" };
                                foreach (var m in lib.inserts) if (m != null && !string.IsNullOrEmpty(m.id)) cand.Add(m.id);
                                Ekle(list, no, true, "GORSEL_YOK", "'" + val + "' görseli yok (eller, yok ya da kütüphanedeki görsel).", val, EnYakin(val, cand));
                            }
                        }
                        break;
                    case "poz":
                        {
                            var sp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            if (sp.Length == 0) { Ekle(list, no, true, "POZ_EKSIK", "'poz: Karakter yatiyor' biçiminde yaz.", "", ""); break; }
                            if (lib != null && lib.FindCharacter(sp[0]) == null)
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + sp[0] + "' karakteri kütüphanede yok. Kullanılabilir: " + string.Join(", ", kars.ToArray()) + ".", sp[0], EnYakin(sp[0], kars));
                            if (sp.Length > 1 && !Util.In(sp[1].Trim(), Util.Poses))
                                Ekle(list, no, true, "POZ_BILINMIYOR", "'" + sp[1].Trim() + "' pozu bilinmiyor. Geçerli: " + string.Join(", ", Util.Poses) + ".", sp[1].Trim(), EnYakin(sp[1].Trim(), Util.Poses));
                        }
                        break;
                    case "cikar":
                        {
                            // 'cikar: Anne' ya da 'cikar: Anne kapi' (ilk kelime karakter)
                            var cp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            if (lib != null && cp.Length > 0 && lib.FindCharacter(cp[0]) == null)
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + cp[0] + "' karakteri kütüphanede yok.", cp[0], EnYakin(cp[0], kars));
                        }
                        break;
                    case "oyuncu":
                        {
                            int eq = val.IndexOf('=');
                            if (eq < 0) break;   // parser uyarir
                            string ch = val.Substring(0, eq).Trim(), model = val.Substring(eq + 1).Trim();
                            if (lib != null && ch.Length > 0 && lib.FindCharacter(ch) == null)
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + ch + "' karakteri kütüphanede yok.", ch, EnYakin(ch, kars));
                            if (model.Length > 0 && Util.Norm(model) != "yok" && !OyuncuVar(model))
                            {
                                var cand = new List<string>();
                                foreach (var g in OyuncuAdlari())
                                {
                                    cand.Add(g);
                                    int us = g.IndexOf('_');
                                    if (us >= 0 && us + 1 < g.Length) cand.Add(g.Substring(us + 1));
                                }
                                Ekle(list, no, true, "OYUNCU_YOK", "'" + model + "' adında 3D karakter yok (Karakterler/Hazir). `karakter_listele` ile bak.", model, EnYakin(model, cand));
                            }
                        }
                        break;
                    case "bekle":
                        if (!Regex.IsMatch(val, "^[0-9]+([.,][0-9]+)?$"))
                            Ekle(list, no, false, "BEKLE_SAYI", "'bekle:' saniye ister (örn. bekle: 0.5); '" + val + "' sayı değil, 0,5 varsayılır.", val, "");
                        break;
                    case "sonuc":
                        if (!Regex.IsMatch(val, "^%?\\s?[0-9]+\\s*%?\\s*[A-Za-z]"))
                            Ekle(list, no, false, "SONUC_BICIM", "'sonuc: %69 A' biçiminde yaz.", val, "");
                        break;
                    case "secim":
                        if (val.IndexOf('/') < 0)
                            Ekle(list, no, true, "SECIM_BICIM", "'secim: A) ... / B) ...' biçiminde iki seçenek yaz.", "", "");
                        break;
                    case "vfx":
                        {
                            var vp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            string vk = vp.Length > 0 ? vp[0] : "";
                            if (!Util.In(vk, StoryParser.VfxListesi))
                                Ekle(list, no, true, "VFX_BILINMIYOR", "'vfx: " + val + "' bilinmiyor. Geçerli: " + string.Join(", ", StoryParser.VfxListesi) + " (ses için efekt:, geçiş için gecis:).", vk, EnYakin(vk, StoryParser.VfxListesi));
                        }
                        break;
                    case "gecis":
                        if (!Util.In(val, YonetmenTablo.Gecisler))
                            Ekle(list, no, true, "GECIS_BILINMIYOR", "'gecis: " + val + "' bilinmiyor. Geçerli: " + string.Join(", ", YonetmenTablo.Gecisler) + ".", val, EnYakin(val, YonetmenTablo.Gecisler));
                        break;
                    case "kapak":
                        {
                            int ks = 0;
                            if (val.Length > 0 && (!int.TryParse(val, out ks) || ks <= 0))
                                Ekle(list, no, false, "KAPAK_SATIR", "'kapak:' satır numarası ister (örn. kapak: 24) ya da boş bırak (sonraki konuşma).", val, "");
                            else if (val.Length > 0 && ks > lines.Length)
                                Ekle(list, no, false, "KAPAK_SATIR", "'kapak: " + ks + "': hikâye " + lines.Length + " satır; kapak karesi otomatik seçilecek.", val, "");
                        }
                        break;
                    case "etiket":
                        if (val.Trim().TrimStart('#').Length == 0)
                            Ekle(list, no, false, "ETIKET_BOS", "'etiket:' boş. Örnek: etiket: aile, dram, duygusal", "", "");
                        break;
                }
            }

            // parser'in yakaladigi ama yukarida karsilanmayan uyarilar (ayni satirda baska sorun yoksa)
            var haveLine = new HashSet<int>();
            foreach (var s in list) haveLine.Add(s.Satir);
            foreach (var w in p.Warnings)
            {
                int ln = ParserSatir(w);
                if (ln > 0 && haveLine.Contains(ln)) continue;
                Ekle(list, ln, false, "PARSER_UYARI", w, "", "");
            }

            // ---- genel kontroller ----
            bool any; int hookLine;
            bool hookOk = StudyoAnaliz.KancaTamam(p, out any, out hookLine);
            if (any && !hookOk)
                Ekle(list, hookLine, false, "KANCA_YOK", "İlk 1,5 saniyede ekranda yazı, ses ya da hareket yok. En başa 'kanca:' ya da çarpıcı bir replik koy.", "", "");

            var ozet = StudyoAnaliz.Hesapla(p, lib, sesKontrol ? story : null);
            int secs = Mathf.RoundToInt(ozet.Sure);
            if (secs > 0 && secs < 61)
                Ekle(list, 0, false, "SURE_KISA", "Tahmini süre ~" + secs + " sn: TikTok Creator Rewards en az 1 dakikadan uzun (61 sn+) videoya ödeme yapar.", "", "");
            if (secs > 180)
                Ekle(list, 0, false, "SURE_UZUN", "Tahmini süre ~" + secs + " sn: 3 dakikayı aşan bölümlerde izlenme düşer; böl.", "", "");
            if (p.DialogueCount > 3 && !StudyoAnaliz.SonVar(p))
                Ekle(list, 0, false, "SON_YOK", "Bölüm sonu yok: 'son: yumusak Devamı yarın...' ya da gerilimde kesmek için 'son: sert' ekle.", "", "");

            // seçim kartından önceki satırın merak/twist bırakması
            for (int k = 0; k < p.Items.Count; k++)
            {
                if (p.Items[k].Type != ItemType.Choice) continue;
                StoryItem prev = null;
                for (int q = k - 1; q >= 0; q--)
                    if (p.Items[q].Type == ItemType.Dialogue || p.Items[q].Type == ItemType.Hook) { prev = p.Items[q]; break; }
                string pt = prev != null ? prev.Text.TrimEnd() : "";
                bool tw = pt.EndsWith("?") || pt.EndsWith("!") || pt.EndsWith("...") || pt.EndsWith("…");
                if (!tw)
                    Ekle(list, p.Items[k].Line, false, "SECIM_TWIST", "Seçim kartından hemen önce bir twist/soru bırak (son replik '?', '!' ya da '...' ile bitsin); izleyici oy vermek için merak etmeli.", "", "");
            }

            // stil metni (Gemini speech_metadata.style) ~6 kelimeyi gecmesin
            var seenTone = new HashSet<string>();
            foreach (var it in p.Items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                string tone = Util.ToneFor(it);
                int words = tone.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                if (words > 6 && seenTone.Add(tone))
                    Ekle(list, it.Line, false, "STIL_UZUN", "Stil metni " + words + " kelime ('" + tone + "'); 6 kelimeyi geçerse Gemini sesi bozabilir. Gönderilirken otomatik kısaltılır: '" + GeminiSes.StilKisalt(tone) + "'.", "", "");
            }

            // ses eksikleri
            if (sesKontrol && lib != null && !string.IsNullOrEmpty(story))
            {
                foreach (var it in p.Items)
                    if (it.Type == ItemType.Dialogue && !GeminiSes.HasClip(story, lib, it))
                        Ekle(list, it.Line, false, "SES_EKSIK", "Bu satırın sesi üretilmemiş (kayıt için gerekli).", "", "");
            }

            list.Sort((a, b) =>
            {
                int c = a.Satir.CompareTo(b.Satir);
                if (c != 0) return c;
                return b.Hata.CompareTo(a.Hata);
            });
            return list;
        }

        public static int HataSayisi(List<Sorun> l)
        {
            int n = 0;
            foreach (var s in l) if (s.Hata) n++;
            return n;
        }

        public static string ToJson(List<Sorun> l)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < l.Count; i++)
            {
                var s = l[i];
                sb.Append(i == 0 ? "\n  " : ",\n  ");
                sb.Append("{\"satir\": ").Append(s.Satir)
                  .Append(", \"seviye\": ").Append(HsJson.Q(s.Seviye))
                  .Append(", \"kod\": ").Append(HsJson.Q(s.Kod))
                  .Append(", \"mesaj\": ").Append(HsJson.Q(s.Mesaj))
                  .Append(", \"oneri\": ").Append(HsJson.Q(s.Oneri))
                  .Append(", \"duzeltme\": ").Append(HsJson.Q(s.Duzeltme)).Append("}");
            }
            sb.Append(l.Count > 0 ? "\n]" : "]");
            return sb.ToString();
        }

        // "Duzelt" dugmesi: satirdaki hatali parcayi onerilenle degistirir. Degismediyse null doner.
        public static string Duzelt(string text, Sorun s)
        {
            if (s == null || s.Satir <= 0 || string.IsNullOrEmpty(s.Yanlis) || string.IsNullOrEmpty(s.Duzeltme)) return null;
            var lines = (text ?? "").Split('\n');
            if (s.Satir > lines.Length) return null;
            string line = lines[s.Satir - 1];
            int at = line.IndexOf(s.Yanlis, System.StringComparison.OrdinalIgnoreCase);
            if (at < 0) return null;
            lines[s.Satir - 1] = line.Substring(0, at) + s.Duzeltme + line.Substring(at + s.Yanlis.Length);
            return string.Join("\n", lines);
        }
    }
}
