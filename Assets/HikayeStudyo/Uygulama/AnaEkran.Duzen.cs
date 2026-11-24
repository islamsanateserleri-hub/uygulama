using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 5 (P3): sahne düzenleme paneli (sahne panosundaki "Düzenle").
    // Tek doğruluk kaynağı hikâye metnidir: panel yalnızca sahnenin metnine komut satırı ekler / değiştirir / siler
    // ve bunu mevcut Degistir() ile yapar. Böylece Geri al / Yinele, doğrulayıcı, canlı önizleme ve kayıt aynı yolu izler.
    //   Süre     : sahne sonundaki "bekle: N" (+0,5 / -0,5 sn; 0'a inerse satır silinir)
    //   Kamera   : mevcut KameraUygula ("kamera: X", sahne başında)
    //   Müzik    : "muzik: X"     sahne başında (mekan satırının hemen altında)
    //   Geçiş    : "gecis: X"     sahnenin mekan satırının hemen ÖNÜNDE (belge: "geçişten hemen sonra mekan: yaz")
    //   Atmosfer : "atmosfer: X"  sahne başında
    //   Altyazı  : "altyazi: X"   sahne başında (stil); seçili konuşma satırının metni düzenlenebilir
    //   Ses efekti  : "efekt: X"  seçili konuşma satırından SONRA
    //   Görsel efekt: "vfx: X"    seçili konuşma satırından ÖNCE
    //   Kapak    : "kapak:"       seçili konuşma satırının ÖNÜNE (hikâyede tek kapak satırı kalır)
    public partial class AnaEkran
    {
        bool yDuzenAcik;
        int yDuzenSahneOnce = -1;
        string yDuzenMetin = "";
        string yDuzenMetinAnahtar = "";
        static readonly string[] AltyaziStilleri = { KaraokeAltyazi.Klasik, KaraokeAltyazi.Karaoke, KaraokeAltyazi.TekKelime };
        static readonly Color DuzenGri = new Color32(60, 66, 88, 255);

        void DuzenAc()
        {
            yDuzenAcik = true;
            yDuzenSahneOnce = -1;   // seçimleri sahneye göre yeniden kur
        }

        // =====================================================================
        // Panel çizimi (sahne kartlarının yerine; kaydırma grubu YOK, açılır listeler doğru yerde açılsın diye)
        // =====================================================================
        void DuzenPanelCiz(Rect g)
        {
            if (ySahneGerek) SahneleriKur();
            Kutu(g, Kart);
            if (ySahneler.Count == 0)
            {
                Yazdir(g, "Düzenlenecek sahne yok.", 14, Sönük, TextAnchor.MiddleCenter);
                if (Dugme(new Rect(g.xMax - 140, g.y + 8, 128, 30), "Kartlara dön", DuzenGri, 12)) yDuzenAcik = false;
                return;
            }
            int idx = Mathf.Clamp(ySecSahne, 0, ySahneler.Count - 1);
            if (idx != yDuzenSahneOnce)
            {
                // sahne değişti: açılır liste değerlerini o sahnenin mevcut değerlerinden yeniden al
                var sil = new List<string>();
                foreach (var k in yDeger.Keys) if (k.StartsWith("duzen_", StringComparison.Ordinal)) sil.Add(k);
                foreach (var k in sil) yDeger.Remove(k);
                yDuzenMetinAnahtar = "";
                yDuzenSahneOnce = idx;
            }
            var s = ySahneler[idx];
            var satirlar = ySatirlar;

            // ---- başlık ----
            Yazdir(new Rect(g.x + 12, g.y + 6, g.width - 170, 28), "Sahne düzenle: " + (idx + 1) + ". " + s.ad + "   (satır " + (s.bas + 1) + "–" + s.son + ")", 15, Yazi, TextAnchor.MiddleLeft, true);
            if (Dugme(new Rect(g.xMax - 140, g.y + 6, 128, 28), "Kartlara dön", DuzenGri, 12)) yDuzenAcik = false;
            Yazdir(new Rect(g.x + 12, g.y + 32, g.width - 24, 18), "Değişiklikler metne komut satırı olarak yazılır; Geri al (Ctrl+Z) ile geri alınır.", 11, Sönük);

            // ---- satır yerleşimi (geniş pencerede iki sütun) ----
            const int satirSayisi = 10;
            int kolon = g.width >= 920f ? 2 : 1;
            int kolonBasina = (satirSayisi + kolon - 1) / kolon;
            float ust = g.y + 54f;
            float rh = Mathf.Clamp((g.yMax - ust - 6f) / kolonBasina, 30f, 40f);
            float cw = (g.width - 24f - (kolon - 1) * 16f) / kolon;
            bool sigmadi = false;

            // seçili konuşma satırı (efekt / vfx / altyazı / kapak için)
            var konusmalar = new List<int>();
            for (int i = s.bas; i < s.son && i < satirlar.Length; i++) if (KonusmaSatiriMi(satirlar[i])) konusmalar.Add(i);
            var etiketler = new List<string>();
            foreach (var i in konusmalar) etiketler.Add(SatirEtiketi(i, satirlar[i]));
            int secSatir = -1;
            if (konusmalar.Count > 0)
            {
                string v = Deger("duzen_satir", etiketler[0]);
                int e = etiketler.IndexOf(v);
                if (e < 0) { e = 0; yDeger["duzen_satir"] = etiketler[0]; }
                secSatir = konusmalar[e];
            }

            for (int sira = 0; sira < satirSayisi; sira++)
            {
                int kc = sira / kolonBasina, ks = sira % kolonBasina;
                var r = new Rect(g.x + 12 + kc * (cw + 16f), ust + ks * rh, cw, rh - 4f);
                if (r.yMax > g.yMax + 1f) { sigmadi = true; continue; }
                DuzenSatiriCiz(sira, r, idx, s, satirlar, konusmalar, etiketler, secSatir);
            }
            if (sigmadi) Yazdir(new Rect(g.x + 12, g.yMax - 20, g.width - 24, 18), "Bazı ayarlar sığmadı: pencereyi büyüt.", 11, new Color(0.94f, 0.75f, 0.31f));
        }

        // Tek bir ayar satırı: etiket + açılır liste + düğmeler. Dar pencerede kontrol genişlikleri küçülür, üst üste binmez.
        void DuzenSatiriCiz(int sira, Rect r, int idx, Sahne s, string[] satirlar, List<int> konusmalar, List<string> etiketler, int secSatir)
        {
            const float etiketG = 96f, bosluk = 6f;
            float bw = Mathf.Clamp((r.width - etiketG) * 0.2f, 62f, 96f);       // düğme genişliği
            float x0 = r.x + etiketG;
            float h = Mathf.Min(r.height, 32f);
            float y = r.y + (r.height - h) * 0.5f;
            Rect Lst(int dugmeSayisi) { return new Rect(x0, y, Mathf.Max(60f, r.xMax - x0 - dugmeSayisi * (bw + bosluk)), h); }
            Rect Dgm(int sag) { return new Rect(r.xMax - (sag + 1) * bw - sag * bosluk, y, bw, h); }
            bool konusmaVar = secSatir >= 0;

            switch (sira)
            {
                case 0: // Süre
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Süre", 13, Sönük);
                    int bi = BekleSatiri(s, satirlar);
                    float bek = bi >= 0 ? BekleDegeri(satirlar[bi]) : 0f;
                    Yazdir(Lst(2), "~" + DuzenSn(s.sure) + " sn  ·  sahne sonu bekle: " + DuzenSn(bek) + " sn", 13, Yazi);
                    if (Dugme(Dgm(1), "-0,5 sn", DuzenGri, 12, bi >= 0)) BekleAyarla(idx, -0.5f);
                    if (Dugme(Dgm(0), "+0,5 sn", Vurgu, 12)) BekleAyarla(idx, 0.5f);
                    break;
                }
                case 1: // Kamera
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Kamera", 13, Sönük);
                    string mev = SahneDegeri(s, satirlar, "kamera", false);
                    string kam = Sec(Lst(1), "duzen_kamera", "", YonetmenTablo.Kameralar, mev.Length > 0 ? mev : "otomatik", 13);
                    if (Dugme(Dgm(0), "Uygula", Vurgu, 12)) { KameraUygula(idx, kam); DuzenMesaj(idx, "kamera: " + kam); }
                    break;
                }
                case 2: DuzenKomutSatiri(r, idx, s, satirlar, "Müzik", "muzik", DegerSecenekleri("muzik"), "pad", Lst(2), Dgm(1), Dgm(0)); break;
                case 3: DuzenKomutSatiri(r, idx, s, satirlar, "Geçiş", "gecis", new List<string>(YonetmenTablo.Gecisler), "karar", Lst(2), Dgm(1), Dgm(0)); break;
                case 4: DuzenKomutSatiri(r, idx, s, satirlar, "Atmosfer", "atmosfer", DegerSecenekleri("atmosfer"), "toz", Lst(2), Dgm(1), Dgm(0)); break;
                case 5: DuzenKomutSatiri(r, idx, s, satirlar, "Altyazı stili", "altyazi", new List<string>(AltyaziStilleri), KaraokeAltyazi.Karaoke, Lst(2), Dgm(1), Dgm(0)); break;
                case 6: // Satır seçimi + kapak
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Satır", 13, Sönük);
                    var kr = Dgm(0); kr.x -= bw * 0.6f; kr.width += bw * 0.6f;
                    var lr = Lst(1); lr.width = Mathf.Max(60f, kr.x - bosluk - lr.x);
                    if (!konusmaVar) Yazdir(lr, "Bu sahnede konuşma satırı yok.", 12, Sönük);
                    else Sec(lr, "duzen_satir", "", etiketler, etiketler[0], 12);
                    if (Dugme(kr, KapakSatiriMi(satirlar, secSatir) ? "Kapak (bu satır)" : "Kapak yap", Vurgu, 11, konusmaVar)) KapakYap(secSatir);
                    break;
                }
                case 7: // Ses efekti (satırdan sonra)
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Ses efekti", 13, Sönük);
                    var efl = DegerSecenekleri("efekt");
                    string ef = Sec(Lst(1), "duzen_efekt", "", efl, efl != null && efl.Count > 0 ? efl[0] : "kapi", 13);
                    if (Dugme(Dgm(0), "Sonra ekle", Vurgu, 11, konusmaVar)) SatiraEkle(secSatir, "efekt: " + ef, true, idx);
                    break;
                }
                case 8: // Görsel efekt (satırdan önce)
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Görsel efekt", 13, Sönük);
                    string vf = Sec(Lst(1), "duzen_vfx", "", StoryParser.VfxListesi, StoryParser.VfxListesi[0], 13);
                    if (Dugme(Dgm(0), "Önce ekle", Vurgu, 11, konusmaVar)) SatiraEkle(secSatir, "vfx: " + vf, false, idx);
                    break;
                }
                case 9: // Altyazı metni (seçili konuşma satırı)
                {
                    Yazdir(new Rect(r.x, y, etiketG, h), "Altyazı", 13, Sönük);
                    var tr = Lst(1);
                    if (!konusmaVar) { Yazdir(tr, "Konuşma satırı seç.", 12, Sönük); break; }
                    string orj = KonusmaMetni(satirlar[secSatir]);
                    string anahtar = secSatir + "\u0001" + satirlar[secSatir];
                    if (anahtar != yDuzenMetinAnahtar) { yDuzenMetinAnahtar = anahtar; yDuzenMetin = orj; }
                    Kutu(tr, new Color32(34, 40, 58, 255));
                    var tf = new GUIStyle(YaziStil(13, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(8, 8, 0, 0) };
                    GUI.SetNextControlName("DuzenAltyazi");
                    yDuzenMetin = GUI.TextField(tr, yDuzenMetin ?? "", 400, tf);
                    bool degisti = yDuzenMetin.Trim().Length > 0 && yDuzenMetin != orj;
                    if (Dugme(Dgm(0), "Kaydet", Vurgu, 12, degisti)) AltyaziKaydet(secSatir, yDuzenMetin, idx);
                    break;
                }
            }
        }

        // Sahne başı komutu (muzik / gecis / atmosfer / altyazi): Uygula = değiştir ya da ekle, Kaldır = sahnedeki satırları sil.
        void DuzenKomutSatiri(Rect r, int idx, Sahne s, string[] satirlar, string etiket, string anahtar, List<string> secenekler, string varsayilan, Rect lst, Rect dKaldir, Rect dUygula)
        {
            float h = lst.height;
            Yazdir(new Rect(r.x, lst.y, 96f, h), etiket, 13, Sönük);
            string mev = SahneDegeri(s, satirlar, anahtar, anahtar == "gecis");
            if (secenekler == null || secenekler.Count == 0) secenekler = new List<string> { varsayilan };
            string v = Sec(lst, "duzen_" + anahtar, "", secenekler, mev.Length > 0 ? mev : varsayilan, 13);
            if (Dugme(dKaldir, "Kaldır", DuzenGri, 12, mev.Length > 0)) { SahneKomutu(idx, anahtar, null); DuzenMesaj(idx, anahtar + " kaldırıldı"); }
            if (Dugme(dUygula, "Uygula", Vurgu, 12, v != mev)) { SahneKomutu(idx, anahtar, v); DuzenMesaj(idx, anahtar + ": " + v); }
        }

        // =====================================================================
        // Metin işlemleri (hepsi Degistir() üzerinden: Geri al / Yinele çalışır)
        // =====================================================================
        // "1,5" biçimi (kültür nesnesi kurmadan; bazı oyuncu derlemelerinde tr-TR kültürü olmayabilir)
        static string DuzenSn(float v) { return v.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', ','); }

        static bool KonusmaSatiriMi(string satir)
        {
            string t = satir.TrimStart();
            return t.StartsWith("[") && t.IndexOf(']') > 1;
        }

        static string SatirEtiketi(int i, string satir)
        {
            string t = satir.Trim();
            if (t.Length > 46) t = t.Substring(0, 44) + "…";
            return (i + 1) + ": " + t;
        }

        static string KonusmaMetni(string satir)
        {
            string t = satir.TrimStart();
            int c = t.IndexOf(']');
            return c < 0 ? "" : t.Substring(c + 1).Trim();
        }

        // Sahnenin mekan satırından sonraki ilk konum (komutların ekleneceği yer)
        int SahneBasiIndeks(Sahne s, IList<string> satirlar)
        {
            int at = s.bas;
            if (at < satirlar.Count && SatirAnahtari(satirlar[at]) == "gecis") at++;
            if (at < satirlar.Count && SatirAnahtari(satirlar[at]) == "mekan") at++;
            return at;
        }

        // Sahnedeki (gecis için mekan satırının hemen önündeki satır da) ilk "anahtar:" satırı; yoksa -1
        static int KomutSatiriBul(Sahne s, IList<string> satirlar, string anahtar, bool oncekiDahil)
        {
            if (oncekiDahil && s.bas > 0 && s.bas - 1 < satirlar.Count && SatirAnahtari(satirlar[s.bas - 1]) == anahtar) return s.bas - 1;
            for (int i = s.bas; i < s.son && i < satirlar.Count; i++) if (SatirAnahtari(satirlar[i]) == anahtar) return i;
            return -1;
        }

        static string SahneDegeri(Sahne s, IList<string> satirlar, string anahtar, bool oncekiDahil)
        {
            int i = KomutSatiriBul(s, satirlar, anahtar, oncekiDahil);
            if (i < 0) return "";
            string t = satirlar[i].Trim();
            int c = t.IndexOf(':');
            return c < 0 ? "" : t.Substring(c + 1).Trim();
        }

        void SahneKomutu(int idx, string anahtar, string deger)
        {
            if (ySahneGerek) SahneleriKur();
            if (idx < 0 || idx >= ySahneler.Count) return;
            var s = ySahneler[idx];
            var satirlar = new List<string>(SatirlariAl());
            bool gecis = anahtar == "gecis";
            if (deger == null)
            {
                // sahnedeki bütün "anahtar:" satırlarını sil (sondan başa; gecis için mekan önündeki satır da)
                for (int i = Mathf.Min(s.son, satirlar.Count) - 1; i >= s.bas; i--) if (SatirAnahtari(satirlar[i]) == anahtar) satirlar.RemoveAt(i);
                if (gecis && s.bas > 0 && s.bas - 1 < satirlar.Count && SatirAnahtari(satirlar[s.bas - 1]) == "gecis") satirlar.RemoveAt(s.bas - 1);
            }
            else
            {
                string yeniSatir = anahtar + ": " + deger;
                int bulunan = KomutSatiriBul(s, satirlar, anahtar, gecis);
                if (bulunan >= 0) satirlar[bulunan] = yeniSatir;
                else if (gecis)
                {
                    // geçiş mekan satırının hemen önüne; sahne mekanla başlamıyorsa sahnenin başına
                    satirlar.Insert(s.bas, yeniSatir);
                }
                else satirlar.Insert(SahneBasiIndeks(s, satirlar), yeniSatir);
            }
            Degistir(string.Join("\n", satirlar.ToArray()));
            ySahneGerek = true;
        }

        // Sahne sonundaki "bekle:" satırı (sondaki boş satırlar ve sonraki sahneye ait gecis satırı atlanır)
        static int BekleSatiri(Sahne s, IList<string> satirlar)
        {
            int k = SonIcerikSatiri(s, satirlar);
            return k >= 0 && SatirAnahtari(satirlar[k]) == "bekle" ? k : -1;
        }

        static int SonIcerikSatiri(Sahne s, IList<string> satirlar)
        {
            for (int i = Mathf.Min(s.son, satirlar.Count) - 1; i >= s.bas; i--)
            {
                string t = satirlar[i].Trim();
                if (t.Length == 0 || t.StartsWith("//")) continue;
                if (SatirAnahtari(satirlar[i]) == "gecis" && i + 1 == s.son) continue;   // sonraki sahnenin geçişi
                return i;
            }
            return -1;
        }

        static float BekleDegeri(string satir)
        {
            string t = satir.Trim();
            int c = t.IndexOf(':');
            return c < 0 ? 0f : Util.ParseFloat(t.Substring(c + 1).Trim().Replace(',', '.'), 0.5f);
        }

        void BekleAyarla(int idx, float fark)
        {
            if (ySahneGerek) SahneleriKur();
            if (idx < 0 || idx >= ySahneler.Count) return;
            var s = ySahneler[idx];
            var satirlar = new List<string>(SatirlariAl());
            int bi = BekleSatiri(s, satirlar);
            float yeni = Mathf.Round(((bi >= 0 ? BekleDegeri(satirlar[bi]) : 0f) + fark) * 2f) / 2f;
            if (bi >= 0)
            {
                if (yeni <= 0.01f) satirlar.RemoveAt(bi);
                else satirlar[bi] = "bekle: " + yeni.ToString("0.#", CultureInfo.InvariantCulture);
            }
            else if (yeni > 0.01f)
            {
                int k = SonIcerikSatiri(s, satirlar);
                satirlar.Insert(k >= 0 ? k + 1 : s.bas, "bekle: " + yeni.ToString("0.#", CultureInfo.InvariantCulture));
            }
            else return;
            Degistir(string.Join("\n", satirlar.ToArray()));
            ySahneGerek = true;
            DuzenMesaj(idx, yeni > 0.01f ? "sahne sonu bekle: " + DuzenSn(yeni) + " sn" : "sahne sonu bekle kaldırıldı");
        }

        // Seçili satırın önüne/sonrasına bir komut satırı ekler; seçim aynı konuşma satırında kalır.
        void SatiraEkle(int satir, string komut, bool sonra, int idx)
        {
            var satirlar = new List<string>(SatirlariAl());
            if (satir < 0 || satir >= satirlar.Count) return;
            int at = sonra ? satir + 1 : satir;
            satirlar.Insert(at, komut);
            int yeniSec = sonra ? satir : satir + 1;
            Degistir(string.Join("\n", satirlar.ToArray()));
            ySahneGerek = true;
            yDeger["duzen_satir"] = SatirEtiketi(yeniSec, satirlar[yeniSec]);
            DuzenMesaj(idx, (yeniSec + 1) + ". satır" + (sonra ? "dan sonra " : "dan önce ") + "eklendi: " + komut);
        }

        static bool KapakSatiriMi(string[] satirlar, int satir)
        {
            if (satir <= 0 || satir >= satirlar.Length) return false;
            for (int i = satir - 1; i >= 0; i--)
            {
                string t = satirlar[i].Trim();
                if (t.Length == 0) continue;
                if (SatirAnahtari(satirlar[i]) == "kapak") return t.Substring(t.IndexOf(':') + 1).Trim().Length == 0;
                if (KonusmaSatiriMi(satirlar[i])) return false;
            }
            return false;
        }

        // Hikâyede tek kapak satırı: eskileri silinir, seçili konuşma satırının önüne boş "kapak:" konur.
        void KapakYap(int satir)
        {
            var satirlar = new List<string>(SatirlariAl());
            if (satir < 0 || satir >= satirlar.Count) return;
            int hedef = satir;
            for (int i = satirlar.Count - 1; i >= 0; i--)
            {
                if (SatirAnahtari(satirlar[i]) != "kapak") continue;
                satirlar.RemoveAt(i);
                if (i < hedef) hedef--;
            }
            satirlar.Insert(hedef, "kapak:");
            Degistir(string.Join("\n", satirlar.ToArray()));
            ySahneGerek = true;
            yDeger["duzen_satir"] = SatirEtiketi(hedef + 1, satirlar[hedef + 1]);
            mesaj = "Kapak karesi: " + (hedef + 2) + ". satır (kapak: satırı eklendi)."; mesajHata = false;
        }

        // Konuşma satırının metnini değiştirir; "[Karakter|duygu|...]" öneki ve girinti korunur.
        void AltyaziKaydet(int satir, string yeniMetin, int idx)
        {
            var satirlar = SatirlariAl();
            if (satir < 0 || satir >= satirlar.Length || !KonusmaSatiriMi(satirlar[satir])) return;
            string l = satirlar[satir];
            string t = l.TrimStart();
            int ofs = l.Length - t.Length, c = t.IndexOf(']');
            satirlar[satir] = l.Substring(0, ofs) + t.Substring(0, c + 1) + " " + yeniMetin.Replace("\n", " ").Trim();
            Degistir(string.Join("\n", satirlar));
            ySahneGerek = true;
            yDeger["duzen_satir"] = SatirEtiketi(satir, satirlar[satir]);
            yDuzenMetinAnahtar = "";
            DuzenMesaj(idx, (satir + 1) + ". satırın metni güncellendi");
        }

        void DuzenMesaj(int idx, string m) { mesaj = (idx + 1) + ". sahne: " + m; mesajHata = false; }
    }
}
