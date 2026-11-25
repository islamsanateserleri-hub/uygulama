using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HikayeStudyo
{
    // Aşama 6: yazma yardımcıları. Tamamen kural tabanlıdır, internet ve API anahtarı gerektirmez.
    // Sahnelere ayırma, karakter algılama / eksik karakter, mekân - duygu - kamera önerisi,
    // tutarlılık ve yazım denetimi, sahne açıklaması. Metni kendiliğinden değiştirmez: öneri üretir.
    public enum OneriTuru { Karakter, Mekan, Duygu, Kamera, Tutarlilik, Yazim, Efekt }

    public class YardimOneri
    {
        public OneriTuru Tur;
        public int Satir;                  // 1'den; 0 = genel
        public string Mesaj = "";
        public string SatirMetni = "";     // öneri üretilirken satırın hali (satır değiştiyse uygulanmaz)
        public string YeniSatir = "";
        public bool OneEkle;               // true: YeniSatir satırın önüne eklenir; false: satırın yerine geçer
        public string EksikAd = "";        // kütüphanede olmayan karakter (arayüz seçim listesi gösterir)
        public string OnerilenAd = "";     // EksikAd bir yazım hatasına benziyorsa kütüphanedeki ad
        public bool Uygulanabilir { get { return Satir > 0 && YeniSatir.Length > 0; } }
    }

    public class YardimSahne
    {
        public int No, BasSatir, SonSatir, Konusma;
        public string Mekan = "";
        public readonly List<string> Karakterler = new List<string>();
        public string BaskinDuygu = "normal";
        public float Sure;
        public string Aciklama = "";
    }

    public class YardimAnaliz
    {
        public readonly List<YardimSahne> Sahneler = new List<YardimSahne>();
        public readonly List<string> Karakterler = new List<string>();   // "Arda (4 satır)"
        public readonly List<YardimOneri> Oneriler = new List<YardimOneri>();
        public int Sayi(OneriTuru t) { int n = 0; foreach (var o in Oneriler) if (o.Tur == t) n++; return n; }
    }

    public static class YardimciMotor
    {
        const string AciklamaOneki = "// Sahne açıklaması: ";

        // ---------------- sözlükler ("=kelime" tam eşleşme, diğerleri kelime başı) ----------------
        static readonly string[][] DuyguSozluk =
        {
            new[] { "aglamakli", "agliyor", "agladi", "aglayarak", "aglamakli", "hickir", "gozyas" },
            new[] { "korkmus", "korku", "korktu", "korkar", "imdat", "=eyvah", "dehset", "urper", "=kacin", "=yardim" },
            new[] { "kizgin", "kizgin", "=yeter", "defol", "=nefret", "sinirl", "ofke", "bagir" },
            new[] { "sasirmis", "sasir", "inanamiyorum", "inanamiyorum", "=nasil", "=imkansiz", "=gercekten", "=neresi" },
            new[] { "sevgi", "seviyorum", "sevgi", "=canim", "=askim", "ozledim", "sukur", "saril" },
            new[] { "uzgun", "uzgun", "uzul", "ozur", "maalesef", "=keske", "kaybet", "kaybol", "=yalnizim" },
            new[] { "mutlu", "mutlu", "sevin", "=harika", "=yasasin", "=super", "=muhtesem", "kutlu", "=gulerek" },
            new[] { "yorgun", "yorgun", "bitkin", "uykum", "tukendim" },
            new[] { "hasta", "=hasta", "hastayim", "atesim", "=agriyor", "midem", "oksur" },
            new[] { "dusunceli", "=acaba", "galiba", "=sanirim", "dusunuyorum", "dusunmeli", "=belki" },
            new[] { "fisilti", "fisil", "=sus", "=sessizce" },
        };

        static readonly string[] YolKelime = { "=yol", "=yolda", "=yola", "=yolu", "sokak", "sokag", "cadde", "araba", "kamyon", "otobus", "korna", "kaldirim" };
        static readonly string[] SofraKelime = { "sofra", "yemek", "yemeg", "corba", "kahvalti", "=masa", "=masada", "=masaya", "mutfak", "mutfag", "tabag", "=tabak" };
        static readonly string[] OdaKelime = { "=oda", "=odada", "=odaya", "odasi", "yatak", "yatag", "uyandi", "uyandim", "=ev", "=evde", "=eve", "=evden", "=evim", "=evimde" };
        static readonly string[] GeceKelime = { "gece", "aksam", "karanlik", "yildizlar", "mehtap" };
        static readonly string[] GunduzKelime = { "sabah", "gunaydin", "=gunes", "ogle", "=gunduz" };

        static readonly string[,] YazimSozluk =
        {
            { "herkez", "herkes" }, { "yalnış", "yanlış" }, { "yanlız", "yalnız" }, { "birşey", "bir şey" }, { "bişey", "bir şey" },
            { "herşey", "her şey" }, { "hiçbirşey", "hiçbir şey" }, { "herzaman", "her zaman" }, { "hiçkimse", "hiç kimse" },
            { "orjinal", "orijinal" }, { "şöför", "şoför" }, { "süpriz", "sürpriz" }, { "sürprüz", "sürpriz" }, { "klavuz", "kılavuz" },
            { "malesef", "maalesef" }, { "insiyatif", "inisiyatif" }, { "sarmısak", "sarımsak" }, { "tabiki", "tabii ki" },
            { "tabiiki", "tabii ki" }, { "yada", "ya da" }, { "birgün", "bir gün" }, { "birkere", "bir kere" }, { "pekçok", "pek çok" },
            { "değilmi", "değil mi" }, { "varmı", "var mı" }, { "yokmu", "yok mu" }, { "unvan", "unvan" }, { "deil", "değil" },
            { "diyil", "değil" }, { "bide", "bir de" }, { "napıyorsun", "ne yapıyorsun" }, { "gelicem", "geleceğim" }, { "gidicem", "gideceğim" },
        };

        // ---------------- yardımcılar ----------------
        public static string Kucuk(string s)
        {
            var sb = new StringBuilder((s ?? "").Length);
            foreach (var c in s ?? "")
                sb.Append(c == 'I' ? 'ı' : (c == 'İ' ? 'i' : char.ToLowerInvariant(c)));
            return sb.ToString();
        }

        static List<string> Kelimeler(string s)
        {
            var l = new List<string>();
            foreach (Match m in Regex.Matches(s ?? "", @"\p{L}+")) l.Add(Util.Norm(m.Value));
            return l;
        }

        static int Say(List<string> kelimeler, string[] kokler, int bas = 0)
        {
            int n = 0;
            foreach (var k in kelimeler)
                for (int i = bas; i < kokler.Length; i++)
                {
                    string kok = kokler[i];
                    if (kok[0] == '=' ? k == kok.Substring(1) : k.StartsWith(kok, StringComparison.Ordinal)) { n++; break; }
                }
            return n;
        }

        public static string DuyguTahmin(string metin)
        {
            var k = Kelimeler(metin);
            string en = "normal"; int enN = 0;
            foreach (var grup in DuyguSozluk)
            {
                int n = Say(k, grup, 1);
                if (n > enN) { enN = n; en = grup[0]; }
            }
            if (enN == 0 && (metin ?? "").Contains("?!")) en = "sasirmis";
            return en;
        }

        // Metne en uygun kütüphane mekânı; ipucu yoksa "".
        public static string MekanTahmin(string metin, IList<string> mekanlar)
        {
            var k = Kelimeler(metin);
            int yol = Say(k, YolKelime), sofra = Say(k, SofraKelime), oda = Say(k, OdaKelime);
            int gece = Say(k, GeceKelime), gunduz = Say(k, GunduzKelime);
            string sonuc = "";
            if (yol > 0 && yol >= sofra && yol >= oda) sonuc = "yol_gece";
            else if (sofra > 0 && sofra >= oda) sonuc = "sofra";
            else if (oda > 0 || gece > 0 || gunduz > 0) sonuc = gunduz > gece ? "oda_gunduz" : (gece > 0 ? "oda_gece" : "oda_gunduz");
            return VarMi(mekanlar, sonuc) ? sonuc : "";
        }

        static bool VarMi(IList<string> l, string ad)
        {
            if (string.IsNullOrEmpty(ad) || l == null) return false;
            foreach (var x in l) if (Util.Norm(x) == Util.Norm(ad)) return true;
            return false;
        }

        static string[] Satirlar(string metin) { return (metin ?? "").Replace("\r", "").Split('\n'); }

        static bool YorumMu(string t) { return t.StartsWith("//") || t.StartsWith("#"); }

        // Satır bir komutsa normalize adı, değilse ""
        static string Anahtar(string satir)
        {
            string t = satir.Trim();
            if (t.Length == 0 || t[0] == '[' || YorumMu(t)) return "";
            int c = t.IndexOf(':');
            if (c <= 0) return "";
            string ham = t.Substring(0, c).Trim();
            if (ham.IndexOf(' ') >= 0) return "";
            string k = Util.Norm(ham);
            return HikayeDenetci.KomutGecerli(k) ? k : "";
        }

        static string Deger(string satir)
        {
            int c = satir.IndexOf(':');
            return c < 0 ? "" : satir.Substring(c + 1).Trim();
        }

        class Konusma { public int Satir; public string Ham = "", Ad = "", Duygu = "", Mod = "", Hareket = "", Metin = ""; }

        static Konusma KonusmaCoz(string satir, int no)
        {
            string t = satir.Trim();
            if (t.Length == 0 || t[0] != '[') return null;
            int c = t.IndexOf(']');
            if (c < 0) return null;
            var p = t.Substring(1, c - 1).Split('|');
            return new Konusma
            {
                Satir = no, Ham = satir, Ad = p[0].Trim(),
                Duygu = p.Length > 1 ? p[1].Trim() : "",
                Mod = p.Length > 2 ? p[2].Trim() : "",
                Hareket = p.Length > 3 ? p[3].Trim() : "",
                Metin = t.Substring(c + 1).Trim()
            };
        }

        static string KonusmaYaz(string ad, string duygu, string mod, string hareket, string metin)
        {
            return "[" + ad + "|" + (string.IsNullOrEmpty(duygu) ? "normal" : duygu) + "|" + (string.IsNullOrEmpty(mod) ? "dis" : mod)
                   + (string.IsNullOrEmpty(hareket) ? "" : "|" + hareket) + "] " + metin;
        }

        static YardimOneri Ekle(YardimAnaliz a, OneriTuru tur, int satir, string mesaj, string[] satirlar, string yeni, bool oneEkle)
        {
            var o = new YardimOneri
            {
                Tur = tur, Satir = satir, Mesaj = mesaj, YeniSatir = yeni ?? "", OneEkle = oneEkle,
                SatirMetni = satir > 0 && satir <= satirlar.Length ? satirlar[satir - 1] : ""
            };
            a.Oneriler.Add(o);
            return o;
        }

        // duyguya / anlatilana gore sinematik efekt onerisi (her sahnede her turden en fazla bir kez)
        static void EfektOner(YardimAnaliz a, int satir, string ad, string duygu, string metin, bool uc, HashSet<string> var1, string[] l)
        {
            var kel = Kelimeler(metin);
            // gozyasi ve nefes yalniz 3D sahnede gorunur
            if (uc && ad.Length > 0 && duygu == "aglamakli" && var1.Add("gozyasi"))
                Ekle(a, OneriTuru.Efekt, satir, ad + " ağlarken gözyaşı efekti", l, "gozyasi: " + ad, true);
            if (uc && ad.Length > 0 && Say(kel, new[] { "usudum", "usuyorum", "soguk", "dondum", "buz", "=karda", "=ayaz" }) > 0 && var1.Add("nefes"))
                Ekle(a, OneriTuru.Efekt, satir, "Soğuk anlatılıyor: nefes buharı", l, "nefes: " + ad, true);
            if (uc && Say(kel, new[] { "hatirliyorum", "hatirladim", "cocukken", "cocuklugum", "=eskiden" }) > 0 && var1.Add("ani"))
                Ekle(a, OneriTuru.Efekt, satir, "Geçmiş anlatılıyor: anı görünümü (bitince 'ani: bitir' ekle)", l, "ani: sepya", true);
            if (duygu == "korkmus" && var1.Add("filtre"))
                Ekle(a, OneriTuru.Efekt, satir, "Korku anı: soğuk renk filtresi", l, "filtre: soguk", true);
            else if (duygu == "sevgi" && var1.Add("filtre"))
                Ekle(a, OneriTuru.Efekt, satir, "Sevgi anı: sıcak renk filtresi", l, "filtre: sicak", true);
            if (duygu == "sasirmis" && metin.IndexOf('!') >= 0 && var1.Add("flas"))
                Ekle(a, OneriTuru.Efekt, satir, "Ani şaşkınlık: beyaz flaş", l, "flas:", true);
        }

        // ---------------- ana analiz ----------------
        public static YardimAnaliz Incele(string metin, IList<string> karakterler, IList<string> mekanlar)
        {
            var a = new YardimAnaliz();
            var l = Satirlar(metin);

            // sahne sınırları: her "mekan:" yeni sahne; hemen yanındaki "mekan3d:" aynı sahnenin parçası
            var baslar = new List<int>();
            bool konusmaGoruldu = false;     // son başlangıçtan beri konuşma görüldü mü
            string sonBaslangic = "";
            for (int i = 0; i < l.Length; i++)
            {
                string k = Anahtar(l[i]);
                if (k == "mekan" || k == "mekan3d")
                {
                    bool esi = baslar.Count > 0 && !konusmaGoruldu && sonBaslangic.Length > 0 && sonBaslangic != k;
                    if (!esi) { baslar.Add(i); konusmaGoruldu = false; sonBaslangic = k; }
                }
                else if (l[i].TrimStart().StartsWith("["))
                {
                    if (baslar.Count == 0) baslar.Add(i);
                    konusmaGoruldu = true;
                }
            }

            var sayac = new Dictionary<string, int>();
            var gorulen = new List<string>();
            var eksik = new HashSet<string>();

            for (int s = 0; s < baslar.Count; s++)
            {
                int bas = baslar[s], son = s + 1 < baslar.Count ? baslar[s + 1] : l.Length;
                var sh = new YardimSahne { No = s + 1, BasSatir = bas + 1, SonSatir = son };
                var duygular = new Dictionary<string, int>();
                var tumMetin = new StringBuilder();
                var ham = new StringBuilder();
                bool mekanVar = false, mekan3d = false, kameraVar = false;
                int ilkKonusma = 0;
                var efektler = new HashSet<string>();   // sahnede zaten olan / onerilen efektler

                for (int i = bas; i < son; i++)
                {
                    string ek = Anahtar(l[i]);
                    if (ek == "gozyasi" || ek == "nefes" || ek == "filtre" || ek == "flas" || ek == "ani") efektler.Add(ek);
                }

                for (int i = bas; i < son; i++)
                {
                    string t = l[i].Trim();
                    if (t.Length == 0 || YorumMu(t)) continue;
                    ham.Append(t).Append('\n');
                    string k = Anahtar(l[i]);
                    if (k == "mekan") { mekanVar = true; sh.Mekan = Deger(t); }
                    else if (k == "mekan3d") { mekanVar = true; mekan3d = true; if (sh.Mekan.Length == 0) sh.Mekan = "3D " + Deger(t); }
                    else if (k == "kamera") kameraVar = true;
                    else if (k == "kanca" || k == "yazi") tumMetin.Append(Deger(t)).Append(' ');

                    var kn = KonusmaCoz(l[i], i + 1);
                    if (kn == null) continue;
                    if (ilkKonusma == 0) ilkKonusma = i + 1;
                    sh.Konusma++;
                    tumMetin.Append(kn.Metin).Append(' ');
                    if (kn.Ad.Length > 0)
                    {
                        if (!sh.Karakterler.Contains(kn.Ad)) sh.Karakterler.Add(kn.Ad);
                        if (!sayac.ContainsKey(kn.Ad)) { sayac[kn.Ad] = 0; gorulen.Add(kn.Ad); }
                        sayac[kn.Ad]++;
                        if (!VarMi(karakterler, kn.Ad) && eksik.Add(Util.Norm(kn.Ad)))
                        {
                            // yalnız tek harflik yazım hatası önerilir ("Ayşe" için "Anne" denmez)
                            string yakin = HikayeDenetci.EnYakin(kn.Ad, karakterler);
                            if (yakin != null && HikayeDenetci.Uzaklik(Util.Norm(kn.Ad), Util.Norm(yakin)) > 1) yakin = null;
                            var o = Ekle(a, OneriTuru.Karakter, kn.Satir,
                                "'" + kn.Ad + "' karakter kütüphanesinde yok" + (yakin != null ? ". Bunu mu demek istediniz: " + yakin + "?" : ". Mevcut bir karakter seç."),
                                l, yakin != null ? KonusmaYaz(yakin, kn.Duygu, kn.Mod, kn.Hareket, kn.Metin) : "", false);
                            o.EksikAd = kn.Ad;
                            o.OnerilenAd = yakin ?? "";
                        }
                    }

                    // duygu önerisi: yalnız boş ya da "normal" satırlara
                    string d = DuyguTahmin(kn.Metin);
                    string simdiki = kn.Duygu.Length == 0 ? "normal" : Util.Norm(kn.Duygu);
                    if (simdiki == "normal" && d != "normal")
                        Ekle(a, OneriTuru.Duygu, kn.Satir, "\"" + Kisalt(kn.Metin) + "\" için duygu: " + d,
                            l, KonusmaYaz(kn.Ad, d, kn.Mod, kn.Hareket, kn.Metin), false);
                    string etkin = simdiki != "normal" ? simdiki : d;
                    if (etkin != "normal") { int n; duygular.TryGetValue(etkin, out n); duygular[etkin] = n + 1; }
                    EfektOner(a, kn.Satir, kn.Ad, etkin, kn.Metin, mekan3d, efektler, l);
                }

                int enCok = 0;
                foreach (var kv in duygular) if (kv.Value > enCok) { enCok = kv.Value; sh.BaskinDuygu = kv.Key; }
                try { sh.Sure = HikayeDenetci.ToplamSure(StoryParser.Parse(ham.ToString())); } catch { sh.Sure = 0f; }

                // mekân önerisi
                string tahmin = MekanTahmin(tumMetin.ToString(), mekanlar);
                if (!mekanVar && ilkKonusma > 0)
                {
                    string m = tahmin.Length > 0 ? tahmin : (VarMi(mekanlar, "oda_gunduz") ? "oda_gunduz" : "");
                    if (m.Length > 0)
                        Ekle(a, OneriTuru.Mekan, bas + 1, "Sahne " + sh.No + " için mekân yok. Öneri: " + m, l, "mekan: " + m, true);
                }
                else if (mekanVar && !mekan3d && sh.Mekan.Length > 0 && !VarMi(mekanlar, sh.Mekan))
                {
                    string m = HikayeDenetci.EnYakin(sh.Mekan, mekanlar) ?? tahmin;
                    for (int i = bas; i < son; i++)
                        if (Anahtar(l[i]) == "mekan")
                        {
                            Ekle(a, OneriTuru.Mekan, i + 1, "'" + sh.Mekan + "' mekânı yok." + (m.Length > 0 ? " Öneri: " + m : ""), l, m.Length > 0 ? "mekan: " + m : "", false);
                            break;
                        }
                }

                // kamera önerisi
                if (!kameraVar && sh.Konusma > 0)
                {
                    string kam = "", neden = "";
                    string bd = sh.BaskinDuygu;
                    var kel = Kelimeler(tumMetin.ToString());
                    if (Say(kel, new[] { "carpma", "carpti", "patla", "deprem", "gumbur" }) > 0) { kam = "salla"; neden = "çarpma/sarsıntı anlatılıyor"; }
                    else if (bd == "aglamakli" || bd == "uzgun" || bd == "sevgi" || bd == "korkmus") { kam = "yakin"; neden = "duygusal sahne (" + bd + ")"; }
                    else if (bd == "sasirmis") { kam = "yaklas"; neden = "şaşırma anı"; }
                    else if (sh.Karakterler.Count >= 3 || sh.No == 1) { kam = "genis"; neden = sh.No == 1 ? "açılış sahnesi: ortamı göster" : "kalabalık sahne"; }
                    if (kam.Length > 0 && ilkKonusma > 0)
                        Ekle(a, OneriTuru.Kamera, ilkKonusma, "Sahne " + sh.No + ": kamera " + kam + " (" + neden + ")", l, "kamera: " + kam, true);
                }

                sh.Aciklama = Acikla(sh);
                a.Sahneler.Add(sh);

                if (sh.Konusma > 12)
                    Ekle(a, OneriTuru.Tutarlilik, bas + 1, "Sahne " + sh.No + " çok uzun (" + sh.Konusma + " konuşma). Araya yeni 'mekan:' ile bölmeyi düşün.", l, "", false);
            }

            foreach (var ad in gorulen) a.Karakterler.Add(ad + " (" + sayac[ad] + " satır)" + (VarMi(karakterler, ad) ? "" : " – kütüphanede yok"));
            // adı metinde geçen ama hiç konuşmayan kütüphane karakterleri (Anne/Baba/Adam sıradan sözcük olduğu için hariç)
            foreach (var ad in karakterler)
            {
                string n = Util.Norm(ad);
                if (n == "anne" || n == "baba" || n == "adam" || n == "anlatici" || sayac.ContainsKey(ad)) continue;
                if (Regex.IsMatch(metin ?? "", @"(?<!\p{L})" + Regex.Escape(ad) + @"(?!\p{L})")) a.Karakterler.Add(ad + " (adı geçiyor, konuşmuyor)");
            }

            Tutarlilik(a, l);
            Yazim(a, l);
            a.Oneriler.Sort((x, y) => x.Tur != y.Tur ? x.Tur.CompareTo(y.Tur) : x.Satir.CompareTo(y.Satir));
            return a;
        }

        static string Kisalt(string s) { return s.Length <= 40 ? s : s.Substring(0, 38) + "…"; }

        static string Acikla(YardimSahne s)
        {
            string yer = s.Mekan.Length == 0 ? "mekân belirtilmemiş" : s.Mekan.Replace("_gece", " (gece)").Replace("_gunduz", " (gündüz)").Replace('_', ' ');
            string kim = s.Karakterler.Count == 0 ? "konuşan yok" : string.Join(", ", s.Karakterler.ToArray());
            return "Sahne " + s.No + ": " + yer + "; " + kim + "; hava: " + s.BaskinDuygu + "; " + s.Konusma + " konuşma, ~" + (int)Math.Round(s.Sure) + " sn";
        }

        // ---------------- tutarlılık ----------------
        static void Tutarlilik(YardimAnaliz a, string[] l)
        {
            var yatan = new HashSet<string>();
            var cikan = new HashSet<string>();
            string mekan = "", oncekiMetin = "";
            for (int i = 0; i < l.Length; i++)
            {
                string t = l[i].Trim();
                if (t.Length == 0 || YorumMu(t)) continue;
                string k = Anahtar(l[i]);
                if (k == "mekan") { mekan = Util.Norm(Deger(t)); yatan.Clear(); cikan.Clear(); continue; }
                if (k == "mekan3d" || k == "temizle") { yatan.Clear(); cikan.Clear(); continue; }
                if (k == "zaman") { mekan = Util.Norm(Deger(t)) == "gece" ? "x_gece" : "x_gunduz"; continue; }
                if (k == "poz")
                {
                    var p = Deger(t).Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length == 2) { if (Util.Norm(p[1]) == "yatiyor") yatan.Add(Util.Norm(p[0])); else yatan.Remove(Util.Norm(p[0])); }
                    continue;
                }
                if (k == "cikar")
                {
                    var p = Deger(t).Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length > 0) cikan.Add(Util.Norm(p[0]));
                    continue;
                }
                if (k == "gir")
                {
                    var p = Deger(t).Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length > 0) cikan.Remove(Util.Norm(p[0]));
                    continue;
                }
                var kn = KonusmaCoz(l[i], i + 1);
                if (kn == null) continue;
                string ad = Util.Norm(kn.Ad), har = Util.Norm(kn.Hareket);
                bool dis = !Util.Norm(kn.Mod).StartsWith("i");
                if (cikan.Contains(ad) && dis)
                {
                    Ekle(a, OneriTuru.Tutarlilik, i + 1, kn.Ad + " sahneden çıkarılmıştı ama tekrar konuşuyor. Geri dönüyorsa sorun yok; değilse satırı taşı.", l, "", false);
                    cikan.Remove(ad);
                }
                if (yatan.Contains(ad) && (har == "kos" || har == "yuru" || har == "zipla"))
                    Ekle(a, OneriTuru.Tutarlilik, i + 1, kn.Ad + " yatıyor ama '" + har + "' hareketi verilmiş. Öncesine ayağa kalkma ekle.", l, "poz: " + kn.Ad + " ayakta", true);
                var kel = Kelimeler(kn.Metin);
                if (mekan.EndsWith("gece") && Say(kel, new[] { "gunaydin", "=tunaydin" }) > 0)
                    Ekle(a, OneriTuru.Tutarlilik, i + 1, "Mekân gece ama satırda 'günaydın' deniyor.", l, "", false);
                if (mekan.EndsWith("gunduz") && Regex.IsMatch(Util.Norm(kn.Metin), "iyi_geceler"))
                    Ekle(a, OneriTuru.Tutarlilik, i + 1, "Mekân gündüz ama satırda 'iyi geceler' deniyor.", l, "", false);
                if (kn.Metin.Length > 0 && kn.Metin == oncekiMetin)
                    Ekle(a, OneriTuru.Tutarlilik, i + 1, "Bu cümle bir önceki konuşmanın aynısı.", l, "", false);
                oncekiMetin = kn.Metin;
            }
        }

        // ---------------- yazım ----------------
        static void Yazim(YardimAnaliz a, string[] l)
        {
            bool devam = false;      // önceki konuşma virgülle bittiyse küçük harf doğaldır
            for (int i = 0; i < l.Length; i++)
            {
                var kn = KonusmaCoz(l[i], i + 1);
                string metin, bas;
                if (kn != null)
                {
                    int c = l[i].IndexOf(']');
                    bas = l[i].Substring(0, c + 1) + " ";
                    metin = kn.Metin;
                }
                else
                {
                    string k = Anahtar(l[i]);
                    if (k != "kanca" && k != "yazi" && k != "baslik") continue;
                    bas = k + ": ";
                    metin = Deger(l[i].Trim());
                }
                if (metin.Length == 0) continue;

                string yeni = metin;
                var notlar = new List<string>();
                for (int s = 0; s < YazimSozluk.GetLength(0); s++)
                {
                    string yanlis = YazimSozluk[s, 0], dogru = YazimSozluk[s, 1];
                    if (yanlis == dogru) continue;
                    var parca = new StringBuilder(yeni);
                    string kucuk = Kucuk(yeni);
                    var ms = Regex.Matches(kucuk, @"(?<![\p{L}'’])" + yanlis + @"(?![\p{L}'’])");
                    for (int m = ms.Count - 1; m >= 0; m--)
                    {
                        string d = dogru;
                        if (char.IsUpper(yeni[ms[m].Index])) d = (d[0] == 'i' ? "İ" : char.ToUpperInvariant(d[0]).ToString()) + d.Substring(1);
                        parca.Remove(ms[m].Index, ms[m].Length).Insert(ms[m].Index, d);
                    }
                    if (ms.Count > 0) { yeni = parca.ToString(); notlar.Add(yanlis + " → " + dogru); }
                }

                string t2 = Regex.Replace(yeni, @"(?<=\S)  +(?=\S)", " ");
                if (t2 != yeni) { yeni = t2; notlar.Add("çift boşluk"); }
                t2 = Regex.Replace(yeni, @"(?<=\p{L}) +(?=[,.;:!?](\s|$))", "");
                if (t2 != yeni) { yeni = t2; notlar.Add("noktalamadan önce boşluk"); }
                t2 = Regex.Replace(yeni, @"(?<=\p{L})([,;!?])(?=\p{L})", "$1 ");
                if (t2 != yeni) { yeni = t2; notlar.Add("noktalamadan sonra boşluk yok"); }
                t2 = Regex.Replace(yeni, @"(?<!\p{L})(ve|ama|bu|şu|de|da|ile|için) \1(?!\p{L})", "$1");
                if (t2 != yeni) { yeni = t2; notlar.Add("tekrarlanan kelime"); }
                bool devamSatiri = devam;
                if (kn != null) devam = metin.EndsWith(",") || metin.EndsWith(";");
                if (!devamSatiri && char.IsLower(yeni[0]))
                {
                    yeni = (yeni[0] == 'i' ? "İ" : (yeni[0] == 'ı' ? "I" : char.ToUpperInvariant(yeni[0]).ToString())) + yeni.Substring(1);
                    notlar.Add("cümle küçük harfle başlıyor");
                }

                if (yeni != metin)
                    Ekle(a, OneriTuru.Yazim, i + 1, string.Join(", ", notlar.ToArray()) + ":  " + Kisalt(yeni), l, bas + yeni, false);
            }
        }

        // ---------------- uygulama ----------------
        // Satır, öneri üretildiğinden beri değiştiyse null döner.
        public static string Uygula(string metin, YardimOneri o)
        {
            if (o == null || !o.Uygulanabilir) return null;
            var l = new List<string>((metin ?? "").Split('\n'));
            int i = o.Satir - 1;
            if (i < 0 || i >= l.Count || l[i] != o.SatirMetni) return null;
            if (o.OneEkle) l.Insert(i, o.YeniSatir); else l[i] = o.YeniSatir;
            return string.Join("\n", l.ToArray());
        }

        // Aynı türdeki bütün önerileri sondan başa uygular.
        public static string TumunuUygula(string metin, List<YardimOneri> oneriler, OneriTuru tur, out int sayi)
        {
            sayi = 0;
            var sec = oneriler.FindAll(o => o.Tur == tur && o.Uygulanabilir);
            sec.Sort((x, y) => y.Satir.CompareTo(x.Satir));
            foreach (var o in sec)
            {
                string y = Uygula(metin, o);
                if (y != null) { metin = y; sayi++; }
            }
            return metin;
        }

        public static string KarakterDegistir(string metin, string eski, string yeni, out int sayi)
        {
            sayi = 0;
            var l = (metin ?? "").Split('\n');
            for (int i = 0; i < l.Length; i++)
            {
                var kn = KonusmaCoz(l[i], i + 1);
                if (kn == null || Util.Norm(kn.Ad) != Util.Norm(eski)) continue;
                l[i] = KonusmaYaz(yeni, kn.Duygu, kn.Mod, kn.Hareket, kn.Metin);
                sayi++;
            }
            return string.Join("\n", l);
        }

        // Sahne açıklamalarını yorum satırı olarak ekler (oynatıcı yorumları yok sayar). Eskileri yenilenir.
        public static string AciklamalariEkle(string metin, IList<string> karakterler, IList<string> mekanlar)
        {
            var l = new List<string>((metin ?? "").Split('\n'));
            l.RemoveAll(s => s.TrimStart().StartsWith(AciklamaOneki.TrimEnd(), StringComparison.Ordinal));
            string temiz = string.Join("\n", l.ToArray());
            var a = Incele(temiz, karakterler, mekanlar);
            for (int i = a.Sahneler.Count - 1; i >= 0; i--)
            {
                var s = a.Sahneler[i];
                l.Insert(Math.Max(0, Math.Min(l.Count, s.BasSatir - 1)), AciklamaOneki + s.Aciklama);
            }
            return string.Join("\n", l.ToArray());
        }

        // ---------------- düz metni sahnelere ayırma ----------------
        static bool EtiketliSatir(string t)
        {
            return t.Length == 0 || t[0] == '[' || YorumMu(t) || Anahtar(t).Length > 0;
        }

        public static int DuzSatirSayisi(string metin)
        {
            int n = 0;
            foreach (var s in Satirlar(metin)) if (!EtiketliSatir(s.Trim())) n++;
            return n;
        }

        const string SozFiilleri = "dedi|diye|sordu|bağırdı|fısıldadı|seslendi|haykırdı|mırıldandı|ağladı";

        // Etiketsiz hikâyeyi [Karakter|duygu|dis] satırlarına ve "mekan:" sahnelerine çevirir.
        // Zaten etiketli satırlar aynen kalır.
        public static string DuzMetniCevir(string metin, IList<string> karakterler, IList<string> mekanlar)
        {
            var sonuc = new List<string>();
            string mekan = "";
            bool paragrafBasi = true;
            var l = Satirlar(metin);
            for (int i = 0; i < l.Length; i++)
            {
                string t = l[i].Trim();
                if (t.Length == 0) { sonuc.Add(""); paragrafBasi = true; continue; }
                if (EtiketliSatir(t))
                {
                    if (Anahtar(t) == "mekan") mekan = Deger(t);
                    sonuc.Add(l[i]); paragrafBasi = false; continue;
                }

                // yeni paragraf (ya da ilk satır): mekân ipucu varsa ve değiştiyse yeni sahne
                if (paragrafBasi || mekan.Length == 0)
                {
                    var p = new StringBuilder();
                    for (int j = i; j < l.Length && l[j].Trim().Length > 0; j++) p.Append(l[j]).Append(' ');
                    string m = MekanTahmin(p.ToString(), mekanlar);
                    if (m.Length == 0 && mekan.Length == 0) m = VarMi(mekanlar, "oda_gunduz") ? "oda_gunduz" : "";
                    if (m.Length > 0 && Util.Norm(m) != Util.Norm(mekan)) { sonuc.Add("mekan: " + m); mekan = m; }
                    paragrafBasi = false;
                }
                SatirCevir(t.Replace('[', '(').Replace(']', ')').Replace('|', '/'), karakterler, sonuc);
            }
            return string.Join("\n", sonuc.ToArray());
        }

        static string KarakterBul(string ad, IList<string> karakterler)
        {
            foreach (var k in karakterler) if (Util.Norm(k) == Util.Norm(ad)) return k;
            return null;
        }

        static void SatirCevir(string t, IList<string> karakterler, List<string> sonuc)
        {
            // 1) "Ad: söz"
            var m = Regex.Match(t, @"^(\p{Lu}\p{L}{1,20})\s*:\s*(.+)$");
            if (m.Success)
            {
                string ad = KarakterBul(m.Groups[1].Value, karakterler) ?? m.Groups[1].Value;
                KonusmaEkle(sonuc, ad, m.Groups[2].Value.Trim().Trim('"', '“', '”'));
                return;
            }

            // 2) tırnaklı söz: konuşanı "dedi Arda" / "Arda ... dedi" kalıbından bul
            var tirnak = Regex.Matches(t, "[\"“]([^\"“”]+)[\"”]");
            if (tirnak.Count > 0)
            {
                string disi = Regex.Replace(t, "[\"“]([^\"“”]+)[\"”]", " ").Trim();
                string konusan = null;
                foreach (var k in karakterler)
                    if (Regex.IsMatch(disi, @"(?<!\p{L})" + Regex.Escape(k) + @"(?!\p{L})", RegexOptions.IgnoreCase)) { konusan = k; break; }
                if (konusan == null)
                {
                    var am = Regex.Match(disi, @"(?:" + SozFiilleri + @")\s+(\p{Lu}\p{L}+)");
                    if (am.Success) konusan = am.Groups[1].Value;
                }
                // tırnak dışında yalnız "dedi Arda" varsa atılır; daha uzun anlatı Anlatıcı'ya okutulur
                bool anlatiVar = Regex.Matches(disi, @"\p{L}+").Count > 4;
                if (anlatiVar && t.IndexOf(tirnak[0].Value, StringComparison.Ordinal) > 0) Anlati(sonuc, disi);
                foreach (Match q in tirnak) KonusmaEkle(sonuc, konusan ?? "Anlatici", q.Groups[1].Value.Trim().TrimEnd(','));
                if (anlatiVar && t.IndexOf(tirnak[0].Value, StringComparison.Ordinal) == 0) Anlati(sonuc, disi.TrimStart(',', ' '));
                return;
            }

            // 3) düz anlatı
            Anlati(sonuc, t);
        }

        static void Anlati(List<string> sonuc, string t)
        {
            foreach (var cumle in Regex.Split(t.Trim(), @"(?<=[.!?…])\s+(?=\p{Lu})"))
                if (Regex.IsMatch(cumle, @"[\p{L}\p{Nd}]")) KonusmaEkle(sonuc, "Anlatici", cumle.Trim());
        }

        // Uzun cümleler virgülden bölünür (satır başına yaklaşık 14 kelime)
        static void KonusmaEkle(List<string> sonuc, string ad, string metin)
        {
            if (metin.Length == 0) return;
            var parcalar = new List<string>();
            if (metin.Split(' ').Length > 16)
            {
                var sb = new StringBuilder();
                foreach (var p in Regex.Split(metin, @"(?<=[,;])\s+"))
                {
                    if (sb.Length > 0 && (sb.ToString() + " " + p).Split(' ').Length > 14) { parcalar.Add(sb.ToString()); sb.Clear(); }
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(p);
                }
                if (sb.Length > 0) parcalar.Add(sb.ToString());
            }
            else parcalar.Add(metin);
            foreach (var p in parcalar) sonuc.Add(KonusmaYaz(ad, DuyguTahmin(p), "dis", "", p));
        }
    }
}
