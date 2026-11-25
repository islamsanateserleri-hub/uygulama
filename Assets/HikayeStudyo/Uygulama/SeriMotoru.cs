using System.Collections.Generic;

namespace HikayeStudyo
{
    // Aşama 14: bölümler arası devamlılık. İnternetsiz, kural tabanlı; yalnız hikâye metinlerinden okur
    // (ayrı bir veritabanı yoktur, hikâye dosyası kayıttır).
    public class SeriBolum
    {
        public string Ad = "";          // dosya adı (uzantısız)
        public ParseResult P;
        public int No;
    }

    public class SeriCeliski
    {
        public string Dosya = "";
        public int Bolum;
        public int Satir;               // 0 = genel
        public bool Agir;
        public string Mesaj = "";
    }

    public class SeriDurumKaydi
    {
        public string Ad = "", Durum = "";
        public int Bolum;
    }

    public class SeriRaporu
    {
        public readonly List<SeriBolum> Bolumler = new List<SeriBolum>();
        public readonly List<SeriCeliski> Celiskiler = new List<SeriCeliski>();
        // Hedef bölümün SONUNDAKİ birikim (açık bölüme yazılanlar da görünür)
        public readonly List<SeriDurumKaydi> Durumlar = new List<SeriDurumKaydi>();
        public readonly List<string> Iliskiler = new List<string>();
        public readonly List<string> Olaylar = new List<string>();
        public readonly List<string> Gecmisler = new List<string>();
        public readonly List<string> AcikSorular = new List<string>();
        public readonly List<string> Plan = new List<string>();
        public string OncekiOzet = "";
        public int OncekiNo;
    }

    public static class SeriMotoru
    {
        public static SeriBolum Bolum(string ad, string metin)
        {
            var p = StoryParser.Parse(metin);
            return new SeriBolum { Ad = ad, P = p, No = p.Bolum };
        }

        public static string Ozet(ParseResult p)
        {
            var l = new List<string>();
            foreach (var n in p.SeriNotlari) if (n.Tur == "ozet") l.Add(n.Deger);
            if (l.Count > 0) return string.Join(" ", l.ToArray());
            string kanca = "", son = "";
            foreach (var it in p.Items)
            {
                if (it.Type == ItemType.Hook && kanca.Length == 0) kanca = it.Text;
                if (it.Type == ItemType.End) son = it.Text;
            }
            return (kanca + (kanca.Length > 0 && son.Length > 0 ? " … " : "") + son).Trim();
        }

        class Olum
        {
            public string Ad = "", Durum = "";
            public int Bolum;
        }

        // tum: bütün hikâyeler (seriye ait olmayanlar elenir). hedef: şu an yazılan bölüm numarası (0 = hepsinin sonu).
        public static SeriRaporu Incele(IList<SeriBolum> tum, string seri, int hedef)
        {
            var r = new SeriRaporu();
            string sn = Util.Norm(seri);
            if (sn.Length == 0) return r;
            foreach (var b in tum) if (b != null && b.P != null && Util.Norm(b.P.Seri) == sn) r.Bolumler.Add(b);
            r.Bolumler.Sort((x, y) => x.No != y.No ? x.No.CompareTo(y.No) : string.CompareOrdinal(x.Ad, y.Ad));

            var durum = new Dictionary<string, Olum>();
            var iliski = new Dictionary<string, string>();       // anahtar -> tür
            var iliskiYazi = new Dictionary<string, string>();
            var model = new Dictionary<string, string>();        // karakter -> oyuncu modeli
            var modelBolum = new Dictionary<string, int>();
            var sorular = new List<string>();
            var adlar = new HashSet<string>();                   // seride sahneye çıkan adlar
            var durumAdlari = new List<SeriCeliski>();           // adı hiç görünmeyen durum kayıtları için aday
            var durumAdNorm = new List<string>();
            var planlar = new SortedDictionary<int, string>();
            bool fotoAlindi = false;
            int oncekiNo = 0;

            foreach (var b in r.Bolumler)
                foreach (var n in b.P.SeriNotlari)
                {
                    int no;
                    if (n.Tur == "plan" && int.TryParse(n.Kim, out no)) planlar[no] = n.Deger;
                }

            foreach (var b in r.Bolumler)
            {
                if (!fotoAlindi && hedef > 0 && b.No > hedef) { Foto(r, durum, iliskiYazi, sorular); fotoAlindi = true; }

                if (b.No <= 0) Ekle(r, b, 0, false, "'bolum:' numarası yok; sıraya doğru yerleşemez.");
                else
                {
                    if (b.No == oncekiNo) Ekle(r, b, 0, true, b.No + ". bölüm numarası iki hikâyede kullanılmış.");
                    else if (oncekiNo > 0 && b.No > oncekiNo + 1) Ekle(r, b, 0, false, "Bölüm numarası atlanmış: " + oncekiNo + " → " + b.No + ".");
                    oncekiNo = b.No;
                }

                // satır sırasına göre: sahne öğeleri ve devamlılık notları
                var sira = new List<KeyValuePair<int, object>>();
                foreach (var it in b.P.Items) sira.Add(new KeyValuePair<int, object>(it.Line, it));
                foreach (var n in b.P.SeriNotlari) sira.Add(new KeyValuePair<int, object>(n.Satir, n));
                sira.Sort((x, y) => x.Key.CompareTo(y.Key));

                bool ani = false, geriDonus = false;
                var olaylar = new List<string>();       // bu bolumde su ana kadarki olay: metinleri
                string mekan = "";
                var bildirilen = new HashSet<string>();

                foreach (var kv in sira)
                {
                    var n = kv.Value as SeriNotu;
                    if (n != null)
                    {
                        switch (n.Tur)
                        {
                            case "durum":
                                {
                                    string k = Util.Norm(n.Kim);
                                    Olum eski;
                                    if (durum.TryGetValue(k, out eski) && eski.Durum == "olu" && n.Deger != "olu" && !ani && !olaylar.Exists(o => o.Contains(SoruNorm(n.Kim))))
                                        Ekle(r, b, n.Satir, true, n.Kim + " " + eski.Bolum + ". bölümde ölmüştü; açıklamasız '" + n.Deger + "' yapılıyor. Önce adını geçiren bir 'olay:' satırıyla açıkla.");
                                    durum[k] = new Olum { Ad = n.Kim, Durum = n.Deger, Bolum = b.No };
                                    bildirilen.Remove(k);
                                    durumAdlari.Add(new SeriCeliski { Dosya = b.Ad, Bolum = b.No, Satir = n.Satir, Mesaj = "'" + n.Kim + "' adlı karakter seride hiç görünmüyor (yazım hatası olabilir)." });
                                    durumAdNorm.Add(k);
                                    break;
                                }
                            case "olay":
                                olaylar.Add(SoruNorm(n.Deger));
                                r.Olaylar.Add(b.No + ". bölüm: " + n.Deger);
                                break;
                            case "gecmis":
                                r.Gecmisler.Add(n.Kim + ": " + n.Deger);
                                break;
                            case "iliski":
                                {
                                    string x = Util.Norm(n.Kim), y = Util.Norm(n.Kim2);
                                    string k = string.CompareOrdinal(x, y) <= 0 ? x + "|" + y : y + "|" + x;
                                    string eski;
                                    if (iliski.TryGetValue(k, out eski) && eski != Util.Norm(n.Deger))
                                        Ekle(r, b, n.Satir, false, n.Kim + " - " + n.Kim2 + " ilişkisi değişti: '" + iliskiYazi[k] + "' iken şimdi '" + n.Deger + "'.");
                                    iliski[k] = Util.Norm(n.Deger);
                                    iliskiYazi[k] = n.Kim + " - " + n.Kim2 + " = " + n.Deger;
                                    break;
                                }
                            case "soru":
                                if (SoruBul(sorular, n.Deger) < 0) sorular.Add(n.Deger);
                                break;
                            case "cozuldu":
                                {
                                    int i = SoruBul(sorular, n.Deger);
                                    if (i >= 0) sorular.RemoveAt(i);
                                    else Ekle(r, b, n.Satir, false, "'cozuldu: " + n.Deger + "' için daha önce aynı metinle 'soru:' yok.");
                                    break;
                                }
                        }
                        continue;
                    }

                    var it = (StoryItem)kv.Value;
                    if (it.Type == ItemType.Rewind) geriDonus = true;
                    else if (it.Type == ItemType.Background) mekan = Util.Norm(it.Text);
                    else if (it.Type == ItemType.Direct && it.Character == "mekan3d") mekan = Util.Norm(it.Arg);
                    else if (it.Type == ItemType.Direct && it.Character == "ani") ani = Util.Norm(it.Arg) != "bitir";
                    else if (it.Type == ItemType.Cast)
                    {
                        string k = Util.Norm(it.Character), m = Util.Norm(it.Arg), eski;
                        if (model.TryGetValue(k, out eski) && eski != m && modelBolum[k] != b.No)
                            Ekle(r, b, it.Line, false, it.Character + " için 3D model değişti: " + eski + " → " + it.Arg + " (izleyici farklı kişi sanabilir).");
                        model[k] = m; modelBolum[k] = b.No;
                    }

                    foreach (var ad in SahnedekiAdlar(it, durum))
                    {
                        string k = Util.Norm(ad);
                        adlar.Add(k);
                        Olum o;
                        if (ani || bildirilen.Contains(k) || !durum.TryGetValue(k, out o)) continue;
                        string neZaman = o.Bolum == b.No ? "bu bölümde" : o.Bolum + ". bölümde";
                        string msg = null; bool agir = false;
                        if (o.Durum == "olu")
                        {
                            agir = true;
                            msg = ad + " " + neZaman + " öldü ama burada yine sahnede/konuşuyor. Anı ise 'ani: sepya' ekle; hayattaysa 'olay:' + 'durum: " + ad + " = hayatta' yaz.";
                        }
                        else if (o.Durum == "kayip") msg = ad + " " + neZaman + " 'kayıp' işaretlendi ama sahnede. Bulunduysa 'durum: " + ad + " = hayatta' yaz.";
                        else if (o.Durum == "uzakta") msg = ad + " " + neZaman + " 'uzakta' işaretlendi ama sahnede. Döndüyse 'durum: " + ad + " = hayatta' yaz.";
                        else if (o.Durum == "hastanede" && mekan.Length > 0 && !mekan.Contains("hastane"))
                            msg = ad + " 'hastanede' işaretli ama mekân hastane değil. Taburcu olduysa 'durum: " + ad + " = hayatta' yaz.";
                        if (msg == null) continue;
                        // geri: / vfx: vhs'nin nerede bittiği belli değil: susturulmaz, hafif uyarıya indirilir
                        if (geriDonus) { agir = false; msg += " (Geri sarmadan sonraki sahne geçmişteyse sorun yok.)"; }
                        bildirilen.Add(k);
                        Ekle(r, b, it.Line, agir, msg);
                    }
                }

                bool sonVar = false;
                foreach (var it in b.P.Items) if (it.Type == ItemType.End || it.Type == ItemType.DevamKarti) sonVar = true;
                if (!sonVar) Ekle(r, b, 0, false, "Bölüm sonu kancası yok ('devami:' ya da 'son:' ekle).");

                if (hedef <= 0 || b.No < hedef)
                {
                    r.OncekiOzet = Ozet(b.P);
                    r.OncekiNo = b.No;
                }
            }
            if (!fotoAlindi) Foto(r, durum, iliskiYazi, sorular);
            // Olay ve geçmiş listeleri sonraki bölümleri göstermesin
            if (hedef > 0)
            {
                r.Olaylar.Clear(); r.Gecmisler.Clear();
                foreach (var b in r.Bolumler)
                {
                    if (b.No > hedef) continue;
                    foreach (var n in b.P.SeriNotlari)
                    {
                        if (n.Tur == "olay") r.Olaylar.Add(b.No + ". bölüm: " + n.Deger);
                        if (n.Tur == "gecmis") r.Gecmisler.Add(n.Kim + ": " + n.Deger);
                    }
                }
            }

            var yazilmayan = new HashSet<string>();
            for (int i = 0; i < durumAdlari.Count; i++)
                if (!adlar.Contains(durumAdNorm[i]) && yazilmayan.Add(durumAdNorm[i])) r.Celiskiler.Add(durumAdlari[i]);

            foreach (var kv in planlar)
            {
                string dosya = "";
                foreach (var b in r.Bolumler) if (b.No == kv.Key) { dosya = b.Ad; break; }
                r.Plan.Add(kv.Key + ". bölüm: " + kv.Value + (dosya.Length > 0 ? "  (hikâye: " + dosya + ")" : "  (henüz yazılmadı)"));
            }
            return r;
        }

        static void Foto(SeriRaporu r, Dictionary<string, Olum> durum, Dictionary<string, string> iliskiYazi, List<string> sorular)
        {
            r.Durumlar.Clear(); r.Iliskiler.Clear(); r.AcikSorular.Clear();
            foreach (var o in durum.Values) r.Durumlar.Add(new SeriDurumKaydi { Ad = o.Ad, Durum = o.Durum, Bolum = o.Bolum });
            r.Iliskiler.AddRange(iliskiYazi.Values);
            r.AcikSorular.AddRange(sorular);
        }

        // Bu öğeyle sahneye çıkan / konuşan karakter adları
        static List<string> SahnedekiAdlar(StoryItem it, Dictionary<string, Olum> durum)
        {
            var l = new List<string>();
            if (it.Type == ItemType.Dialogue || it.Type == ItemType.Pose)
            {
                if (!string.IsNullOrEmpty(it.Character) && Util.Norm(it.Character) != "anlatici") l.Add(it.Character);
            }
            else if (it.Type == ItemType.Direct && it.Character != "ani" && it.Character != "mekan3d")
            {
                // gir: / git: / sarilma: gibi komutlarda yalnız durumu bilinen karakter adlarını yakala
                bool olununKalabilir = it.Character == "cikar" || it.Character == "sarilma" || it.Character == "saril";
                foreach (var p in (it.Arg ?? "").Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    Olum o;
                    if (durum.TryGetValue(Util.Norm(p), out o) && !(olununKalabilir && o.Durum == "olu")) l.Add(p);
                }
            }
            // cenaze: olunun yatmasi, sarilinmasi, cikarilmasi celiski degil
            if (it.Type == ItemType.Pose && it.Arg == "yatiyor")
                l.RemoveAll(ad => { Olum o; return durum.TryGetValue(Util.Norm(ad), out o) && o.Durum == "olu"; });
            return l;
        }

        static string SoruNorm(string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in Util.Norm(s)) if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }

        static int SoruBul(List<string> l, string soru)
        {
            string n = SoruNorm(soru);
            for (int i = 0; i < l.Count; i++) if (SoruNorm(l[i]) == n) return i;
            return -1;
        }

        static void Ekle(SeriRaporu r, SeriBolum b, int satir, bool agir, string mesaj)
        {
            r.Celiskiler.Add(new SeriCeliski { Dosya = b.Ad, Bolum = b.No, Satir = satir, Agir = agir, Mesaj = mesaj });
        }
    }
}
