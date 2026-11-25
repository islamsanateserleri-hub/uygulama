using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 3: uygulama içindeki yazma ekranının hata denetçisi (Editör'deki StudyoDogrulama'nın Editör'süz karşılığı).
    // Aynı kuralları kullanır: karakter/duygu/hareket/mekân/efekt/müzik/poz/oyuncu/komut denetimi + öneri ("Bunu mu demek istediniz").
    public class DenetimSorunu
    {
        public int Satir;              // 1'den; 0 = genel
        public string Kod = "";
        public string Mesaj = "";
        public string Yanlis = "";     // satırdaki hatalı parça
        public string Duzeltme = "";   // yerine yazılacak parça
        public bool Hata;              // true = hata, false = uyarı
    }

    public static class HikayeDenetci
    {
        static readonly string[] HazirEfektler =
            { "carpma", "korna", "geri", "whoosh", "gecis", "kapi", "buyu", "kalp", "tahta", "kase", "yagmur", "flas", "gok", "gokgurultusu" };

        public static int Uzaklik(string a, string b)
        {
            if (a == null) a = ""; if (b == null) b = "";
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;
            var prev = new int[b.Length + 1]; var cur = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++) prev[j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                cur[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    cur[j] = Mathf.Min(Mathf.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
                }
                var t = prev; prev = cur; cur = t;
            }
            return prev[b.Length];
        }

        public static string EnYakin(string yanlis, IEnumerable<string> adaylar)
        {
            string n = Util.Norm(yanlis);
            if (n.Length == 0 || adaylar == null) return null;
            string best = null; int bd = int.MaxValue;
            foreach (var c in adaylar)
            {
                if (string.IsNullOrEmpty(c)) continue;
                string cn = Util.Norm(c);
                if (cn == n) continue;
                int d = Uzaklik(n, cn);
                if (n.Length >= 3 && cn.Contains(n)) d = Mathf.Min(d, 1);
                if (cn.Length >= 4 && n.Contains(cn)) d = Mathf.Min(d, 1);
                if (d < bd) { bd = d; best = c; }
            }
            int lim = n.Length <= 3 ? 1 : (n.Length <= 6 ? 2 : 3);
            return best != null && bd <= lim ? best : null;
        }

        static void Ekle(List<DenetimSorunu> l, int satir, bool hata, string kod, string mesaj, string yanlis, string aday)
        {
            l.Add(new DenetimSorunu { Satir = satir, Hata = hata, Kod = kod, Mesaj = mesaj, Yanlis = yanlis ?? "", Duzeltme = aday ?? "" });
        }

        public static List<string> KarakterAdlari(StoryLibrary lib)
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

        static readonly Dictionary<string, bool> komutOnbellek = new Dictionary<string, bool>();

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
            StoryParser.Parse("");
            var l = new List<string>();
            foreach (var b in KomutKayit.Hepsi()) l.Add(b.Ad);
            return l;
        }

        static bool OyuncuVar(string ad, IList<string> oyuncular)
        {
            string n = Util.Norm(ad);
            if (n.Length == 0 || oyuncular == null) return false;
            foreach (var g in oyuncular) if (Util.Norm(g) == n) return true;
            foreach (var g in oyuncular) if (Util.Norm(g).EndsWith("_" + n)) return true;
            foreach (var g in oyuncular) if (Util.Norm(g).Contains(n)) return true;
            return Varyantlar.AdGecerli(ad, oyuncular);   // karakter çeşidi: VRM_Anne_005
        }

        static int ParserSatir(string w)
        {
            var m = Regex.Match(w ?? "", "^(\\d+)\\.\\s*sat");
            int n;
            return m.Success && int.TryParse(m.Groups[1].Value, out n) ? n : 0;
        }

        // Tahmini süre (ses dosyası yokken metinden); Editör'deki StudyoAnaliz.ItemSure ile aynı sabitler
        public static float OgeSuresi(StoryItem it)
        {
            switch (it.Type)
            {
                case ItemType.Dialogue: return Mathf.Max(1.2f, it.Text.Length / 14f + 0.3f) + 0.22f;
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

        public static float ToplamSure(ParseResult p)
        {
            float t = 0f;
            foreach (var it in p.Items) t += OgeSuresi(it);
            return t;
        }

        public static List<DenetimSorunu> Denetle(string text, StoryLibrary lib, IList<string> oyuncuAdlari)
        {
            var list = new List<DenetimSorunu>();
            var p = StoryParser.Parse(text ?? "");
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
                    if (close < 0) { Ekle(list, no, true, "PARANTEZ_EKSIK", "']' eksik. Konuşma satırı: [Karakter|duygu|dis] Metin", "", ""); continue; }
                    var parts = line.Substring(1, close - 1).Split('|');
                    string ch = parts[0].Trim();
                    if (ch.Length == 0) Ekle(list, no, true, "KARAKTER_BOS", "Karakter adı boş: [Arda|normal|dis] Metin", "", "");
                    else if (lib != null && lib.FindCharacter(ch) == null)
                        Ekle(list, no, true, "KARAKTER_YOK", "'" + ch + "' karakteri yok. Kullanılabilir: " + string.Join(", ", kars.ToArray()) + ".", ch, EnYakin(ch, kars));
                    if (parts.Length > 1 && parts[1].Trim().Length > 0 && !Util.In(parts[1].Trim(), Util.Emotions))
                        Ekle(list, no, true, "DUYGU_BILINMIYOR", "'" + parts[1].Trim() + "' duygusu bilinmiyor. Geçerli: " + string.Join(", ", Util.Emotions) + ".", parts[1].Trim(), EnYakin(parts[1].Trim(), Util.Emotions));
                    if (parts.Length > 2 && parts[2].Trim().Length > 0)
                    {
                        string m = Util.Norm(parts[2]);
                        if (!m.StartsWith("i") && !m.StartsWith("d"))
                            Ekle(list, no, false, "MOD_BILINMIYOR", "Üçüncü alan 'dis' ya da 'ic' olmalı ('" + parts[2].Trim() + "' bulundu).", parts[2].Trim(), EnYakin(parts[2].Trim(), new[] { "dis", "ic" }));
                    }
                    if (parts.Length > 3 && parts[3].Trim().Length > 0 && !Util.In(parts[3].Trim(), Util.Motions))
                        Ekle(list, no, true, "HAREKET_BILINMIYOR", "'" + parts[3].Trim() + "' hareketi bilinmiyor. Geçerli: " + string.Join(", ", Util.Motions) + ".", parts[3].Trim(), EnYakin(parts[3].Trim(), Util.Motions));
                    if (line.Substring(close + 1).Trim().Length == 0) Ekle(list, no, true, "BOS_METIN", "Konuşma metni boş.", "", "");
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
                    Ekle(list, no, true, "SATIR_ANLASILMADI", "Satır anlaşılmadı. Konuşma: [Karakter|duygu|dis] Metin — komut: komut: değer. (Yorum: satırı // ile başlat.)", "", "");
                    continue;
                }
                string rawKey = line.Substring(0, colon).Trim();
                string key = Util.Norm(rawKey);
                string val = line.Substring(colon + 1).Trim();
                if (key.Length == 0 || !KomutGecerli(key))
                {
                    Ekle(list, no, true, "KOMUT_BILINMIYOR", "'" + rawKey + ":' komutu bilinmiyor.", rawKey, EnYakin(rawKey, komutlar));
                    continue;
                }
                if (key == "mekan" || key == "mekan3d") ilkMekanGoruldu = true;

                switch (key)
                {
                    case "mekan":
                        if (lib != null && val.Length > 0 && lib.FindBackground(val) == null)
                            Ekle(list, no, true, "MEKAN_YOK", "'" + val + "' mekanı yok. Kullanılabilir: " + string.Join(", ", mekanlar.ToArray()) + ".", val, EnYakin(val, mekanlar));
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
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + sp[0] + "' karakteri yok.", sp[0], EnYakin(sp[0], kars));
                            if (sp.Length > 1 && !Util.In(sp[1].Trim(), Util.Poses))
                                Ekle(list, no, true, "POZ_BILINMIYOR", "'" + sp[1].Trim() + "' pozu bilinmiyor. Geçerli: " + string.Join(", ", Util.Poses) + ".", sp[1].Trim(), EnYakin(sp[1].Trim(), Util.Poses));
                        }
                        break;
                    case "cikar":
                        {
                            var cp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            if (lib != null && cp.Length > 0 && lib.FindCharacter(cp[0]) == null)
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + cp[0] + "' karakteri yok.", cp[0], EnYakin(cp[0], kars));
                        }
                        break;
                    case "oyuncu":
                        {
                            int eq = val.IndexOf('=');
                            if (eq < 0) break;
                            string ch = val.Substring(0, eq).Trim(), model = val.Substring(eq + 1).Trim();
                            if (lib != null && ch.Length > 0 && lib.FindCharacter(ch) == null)
                                Ekle(list, no, true, "KARAKTER_YOK", "'" + ch + "' karakteri yok.", ch, EnYakin(ch, kars));
                            if (model.Length > 0 && Util.Norm(model) != "yok" && oyuncuAdlari != null && oyuncuAdlari.Count > 0 && !OyuncuVar(model, oyuncuAdlari))
                            {
                                var cand = new List<string>();
                                foreach (var g in oyuncuAdlari)
                                {
                                    cand.Add(g);
                                    int us = g.IndexOf('_');
                                    if (us >= 0 && us + 1 < g.Length) cand.Add(g.Substring(us + 1));
                                }
                                Ekle(list, no, true, "OYUNCU_YOK", "'" + model + "' adında 3D karakter yok (Karakterlerim bölümüne bak).", model, EnYakin(model, cand));
                            }
                        }
                        break;
                    case "bekle":
                        if (!Regex.IsMatch(val, "^[0-9]+([.,][0-9]+)?$"))
                            Ekle(list, no, false, "BEKLE_SAYI", "'bekle:' saniye ister (örn. bekle: 0.5); '" + val + "' sayı değil.", val, "");
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
                    case "mekan3d":
                        if (!Util.In(val, YonetmenTablo.Mekanlar))
                            Ekle(list, no, true, "MEKAN3D_YOK", "'" + val + "' 3D mekânı yok. Geçerli: " + string.Join(", ", YonetmenTablo.Mekanlar) + ".", val, EnYakin(val, YonetmenTablo.Mekanlar));
                        break;
                }
            }

            var haveLine = new HashSet<int>();
            foreach (var s in list) haveLine.Add(s.Satir);
            foreach (var w in p.Warnings)
            {
                int ln = ParserSatir(w);
                if (ln > 0 && haveLine.Contains(ln)) continue;
                Ekle(list, ln, false, "PARSER_UYARI", w, "", "");
            }

            int secs = Mathf.RoundToInt(ToplamSure(p));
            if (p.DialogueCount > 0 && secs < 61)
                Ekle(list, 0, false, "SURE_KISA", "Tahmini süre ~" + secs + " sn: TikTok ödemesi için 61 sn+ hedefle.", "", "");
            if (secs > 180)
                Ekle(list, 0, false, "SURE_UZUN", "Tahmini süre ~" + secs + " sn: 3 dakikayı aşan bölümlerde izlenme düşer; böl.", "", "");
            bool sonVar = false;
            foreach (var it in p.Items) if (it.Type == ItemType.End) sonVar = true;
            if (p.DialogueCount > 3 && !sonVar)
                Ekle(list, 0, false, "SON_YOK", "Bölüm sonu yok: 'son: yumusak Devamı yarın...' ya da 'son: sert' ekle.", "", "");

            list.Sort((a, b) =>
            {
                int h = b.Hata.CompareTo(a.Hata);              // önce hatalar, sonra uyarılar
                if (h != 0) return h;
                int sa = a.Satir == 0 ? int.MaxValue : a.Satir, sb = b.Satir == 0 ? int.MaxValue : b.Satir;
                return sa.CompareTo(sb);
            });
            return list;
        }

        public static int HataSayisi(List<DenetimSorunu> l)
        {
            int n = 0;
            foreach (var s in l) if (s.Hata) n++;
            return n;
        }

        // "Düzelt" düğmesi: satırdaki hatalı parçayı önerilenle değiştirir; değişmediyse null.
        public static string Duzelt(string text, DenetimSorunu s)
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
