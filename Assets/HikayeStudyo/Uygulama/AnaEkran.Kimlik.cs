using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 8: "Karakterlerim > Kimlik kartları" sekmesi
    public partial class AnaEkran
    {
        KarakterKimligi kimlikForm;     // açık form (null = liste)
        string kimlikYas = "", kimlikYeniAd = "";
        bool kimlikKaldirOnay;
        float kimlikListeKaydirma, kimlikFormKaydirma;

        const string SesAyni = "Konuşma sesiyle aynı";
        const string GorunumCizgi = "Çizgi karakter";
        const string GorunumVarsayilan = "Varsayılan görünüm";

        static readonly Color KimlikAlan = new Color32(46, 54, 78, 255);
        static readonly Color KimlikGri = new Color32(60, 66, 88, 255);

        static readonly string[] TenRenkleri = { "#FFE0C4", "#FFD1AD", "#EFB98F", "#D69A6C", "#B07548", "#8A5732", "#6B4226", "#4A2C1A" };
        static readonly string[] SacRenkleri = { "#17110D", "#38241A", "#6B4423", "#A0652E", "#D9A441", "#B23A1E", "#8C8C8C", "#EDEDED" };
        static readonly string[] GiysiRenkleri = { "#4D80CC", "#D9534F", "#5CB85C", "#F0AD4E", "#8E6CC0", "#E87EA1", "#404052", "#F2F2F2" };
        static readonly string[] YaziRenkleri = { "#FFFFFF", "#FFE066", "#8FE3A0", "#8FD0FF", "#FFB0C8", "#FFC285", "#C9B6FF", "#D1E6FF" };
        static readonly string[] SacAdlari = { "Kısa", "Kâse", "Uzun", "Topuz", "Kel" };

        void KutuphaneYenile()
        {
            var asil = Resources.Load<StoryLibrary>("StoryLibrary");
            var eski = kutuphane;
            kutuphane = KimlikDeposu.Uygula(asil);
            if (eski != null && eski != asil && eski != kutuphane) Destroy(eski);
            // açılır liste ve ses durumu önbellekleri yeni karakter bilgisiyle yeniden okunsun
            var sil = new List<string>();
            foreach (var k in yDeger.Keys) if (k.StartsWith("sesat_", StringComparison.Ordinal)) sil.Add(k);
            foreach (var k in sil) yDeger.Remove(k);
            ySesMetin = null;
        }

        void KimlikCiz(Rect r)
        {
            if (kutuphane == null) { Yazdir(new Rect(r.x, r.y, r.width, 30), "Karakter kütüphanesi yüklenemedi.", 15, Tehlike); return; }
            GUI.enabled = acikId == null;
            if (kimlikForm == null) KimlikListe(r);
            else KimlikFormCiz(r);
            GUI.enabled = true;
            AcilirCiz();
        }

        void KimlikListe(Rect r)
        {
            Yazdir(new Rect(r.x, r.y, r.width, 22), "Kartı olan karakter bütün hikâyelerde aynı görünür ve aynı sesle konuşur. Kart yalnız sen değiştirince değişir.", 13, Sönük);
            var alan = new Rect(r.x, r.y + 30, 280, 36);
            kimlikYeniAd = KimlikYazi(alan, kimlikYeniAd, 24);
            if (kimlikYeniAd.Length == 0) Yazdir(new Rect(alan.x + 10, alan.y, alan.width - 20, alan.height), "Yeni karakter adı", 14, Sönük);
            if (Dugme(new Rect(r.x + 290, r.y + 30, 170, 36), "Yeni kart oluştur", Vurgu, 14))
            {
                string ad = kimlikYeniAd.Trim();
                string h = KarakterKimligi.AdHatasi(ad);
                if (h != null) { mesaj = h; mesajHata = true; }
                else
                {
                    var var0 = kutuphane.FindCharacter(ad);
                    KimlikAc(var0 != null ? var0.id : ad);
                    kimlikYeniAd = "";
                    return;
                }
            }
            MesajCiz(new Rect(r.x + 470, r.y + 28, r.width - 470, 40));

            var adlar = new List<CharacterDef>();
            foreach (var c in kutuphane.characters) if (c != null && !string.IsNullOrEmpty(c.id)) adlar.Add(c);
            var g = new Rect(r.x, r.y + 76, r.width, r.height - 76);
            const float sat = 62f;
            string acilacak = null;
            ListeAl(ref kimlikListeKaydirma, g, Mathf.Max(g.height, adlar.Count * sat));
            for (int i = 0; i < adlar.Count; i++)
            {
                var d = adlar[i];
                bool kartli = KimlikDeposu.Var(d.id);
                string atama = DisSes.Atama(d.id, false);   // "Ses ayarları" seçimi varsa geçerli ses odur
                string gecerliSes = string.IsNullOrEmpty(atama) ? d.voice : atama;
                var s = new Rect(0, i * sat, g.width - 18, sat - 8);
                Kutu(s, Kart);
                Kutu(new Rect(s.x + 10, s.y + 10, 10, s.height - 20), d.subtitleColor);
                Yazdir(new Rect(s.x + 30, s.y + 4, 240, 26), d.id, 16, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(s.x + 30, s.y + 29, s.width - 200, 20),
                    (kartli ? "Kimlik kartı var" : "Kart yok (varsayılan)") + "  ·  " +
                    (d.model3D != null ? "3D: " + d.model3D.name : "Çizgi karakter") + "  ·  Boy " + KarakterKimligi.Boy(d.scale) + " cm  ·  Ses: " + gecerliSes,
                    12, kartli ? Basari : Sönük);
                if (Dugme(new Rect(s.xMax - 150, s.y + 9, 138, 36), kartli ? "Düzenle" : "Kart oluştur", kartli ? Vurgu : KimlikGri, 14)) acilacak = d.id;
            }
            ListeVer(ref kimlikListeKaydirma, g);
            if (acilacak != null) KimlikAc(acilacak);
        }

        void KimlikAc(string ad)
        {
            var k = KimlikDeposu.Oku(ad);
            if (k == null)
            {
                var d = kutuphane.FindCharacter(ad);
                k = d != null && !kutuphane.IsNarrator(d) ? KarakterKimligi.Karakterden(d, katalog) : new KarakterKimligi { ad = ad };
            }
            // "Ses ayarları"nda yapılmış bir ses seçimi varsa kartta o görünsün (geçerli ses odur)
            string at = DisSes.Atama(k.ad, false), atIc = DisSes.Atama(k.ad, true);
            if (!string.IsNullOrEmpty(at)) k.ses = at;
            if (!string.IsNullOrEmpty(atIc)) k.icSes = atIc;
            var sil = new List<string>();
            foreach (var a in yDeger.Keys) if (a.StartsWith("kk_", StringComparison.Ordinal)) sil.Add(a);
            foreach (var a in sil) yDeger.Remove(a);
            kimlikForm = k;
            kimlikYas = k.yas > 0 ? k.yas.ToString() : "";
            kimlikKaldirOnay = false; kimlikFormKaydirma = 0f; mesaj = ""; mesajHata = false; acikId = null;
        }

        string KimlikYazi(Rect r, string deger, int azami)
        {
            Kutu(r, KimlikAlan);
            var tf = new GUIStyle(YaziStil(14, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(10, 10, 0, 0), clipping = TextClipping.Clip };
            return GUI.TextField(r, deger ?? "", azami, tf);
        }

        void KimlikFormCiz(Rect r)
        {
            var k = kimlikForm;
            bool kayitli = KimlikDeposu.Var(k.ad);
            Yazdir(new Rect(r.x, r.y, r.width - 480, 36), k.ad + " — kimlik kartı", 20, Yazi, TextAnchor.MiddleLeft, true);
            if (Dugme(new Rect(r.xMax - 466, r.y, 110, 36), "Geri", KimlikGri, 14)) { kimlikForm = null; acikId = null; mesaj = ""; return; }
            if (kayitli && Dugme(new Rect(r.xMax - 346, r.y, 180, 36), kimlikKaldirOnay ? "Emin misin? Kaldır" : "Kartı kaldır", kimlikKaldirOnay ? Tehlike : KimlikGri, 14, !GeminiUyg.Busy))
            {
                if (!kimlikKaldirOnay) kimlikKaldirOnay = true;
                else
                {
                    string h = KimlikDeposu.Kaldir(k.ad);
                    if (h != null) { mesaj = h; mesajHata = true; }
                    else
                    {
                        KutuphaneYenile();
                        kimlikForm = null; acikId = null;
                        mesaj = k.ad + ": kart kaldırıldı, varsayılana dönüldü."; mesajHata = false;
                        return;
                    }
                }
            }
            if (Dugme(new Rect(r.xMax - 156, r.y, 156, 36), "Kaydet", Vurgu, 15, !GeminiUyg.Busy)) KimlikKaydet();
            MesajCiz(new Rect(r.x, r.y + 42, r.width, 34));

            var g = new Rect(r.x, r.y + 82, r.width, r.height - 82);
            const float icH = 960f;
            ListeAl(ref kimlikFormKaydirma, g, Mathf.Max(g.height, icH));
            var kayma = new Vector2(g.x, g.y - kimlikFormKaydirma);
            float w = g.width - 24f, x = 190f, y = 0f;

            KimlikEtiket(y, "Yaşı");
            kimlikYas = KimlikYazi(new Rect(x, y, 90, 32), kimlikYas, 3);
            y += 42;

            KimlikEtiket(y, "Boyu");
            if (Dugme(new Rect(x, y, 40, 32), "-", KimlikGri, 18)) k.boyCm = Mathf.Max(70, k.boyCm - 5);
            Yazdir(new Rect(x + 46, y, 90, 32), k.boyCm + " cm", 15, Yazi, TextAnchor.MiddleCenter, true);
            if (Dugme(new Rect(x + 142, y, 40, 32), "+", KimlikGri, 18)) k.boyCm = Mathf.Min(228, k.boyCm + 5);
            Yazdir(new Rect(x + 200, y, w - x - 200, 32), "Yetişkin ≈ 175, küçük çocuk ≈ 125", 12, Sönük);
            y += 42;

            KimlikEtiket(y, "Görünümü (3D model)");
            var gorunumler = new List<string> { GorunumCizgi };
            if (k.gorunum == KarakterKimligi.VarsayilanGorunum) gorunumler.Add(GorunumVarsayilan);
            if (katalog != null) foreach (var kayit in katalog.karakterler) if (kayit != null && !string.IsNullOrEmpty(kayit.kimlik)) gorunumler.Add(kayit.kimlik);
            string simdiki = string.IsNullOrEmpty(k.gorunum) ? GorunumCizgi : k.gorunum == KarakterKimligi.VarsayilanGorunum ? GorunumVarsayilan : k.gorunum;
            string sg = SecG(new Rect(x, y, 300, 32), kayma, "kk_gorunum", gorunumler, simdiki, 14);
            k.gorunum = sg == GorunumCizgi ? KarakterKimligi.CizgiGorunum : sg == GorunumVarsayilan ? KarakterKimligi.VarsayilanGorunum : sg;
            Yazdir(new Rect(x + 312, y, w - x - 312, 32), "3D modelin yüzü ve kıyafeti modele aittir.", 12, Sönük);
            y += 42;

            KimlikEtiket(y, "Sahnedeki yeri");
            string[] yerler = { "Sol", "Orta", "Sağ" };
            for (int i = 0; i < 3; i++)
                if (Dugme(new Rect(x + i * 88, y, 82, 32), yerler[i], k.yer == i - 1 ? Vurgu : KimlikGri, 13)) k.yer = i - 1;
            y += 52;

            KimlikBaslik(y, "Ses kimliği ve konuşma tarzı"); y += 32;
            KimlikEtiket(y, "Sesi");
            var sesler = new List<string>();
            for (int i = 0; i < GeminiSesleri.GetLength(0); i++) sesler.Add(GeminiSesleri[i, 0] + " · " + GeminiSesleri[i, 1]);
            k.ses = SesAdi(SecG(new Rect(x, y, 300, 32), kayma, "kk_ses", sesler, SesEtiketi(k.ses), 14));
            y += 42;
            KimlikEtiket(y, "İç sesi");
            var icSesler = new List<string> { SesAyni };
            icSesler.AddRange(sesler);
            string si = SecG(new Rect(x, y, 300, 32), kayma, "kk_icses", icSesler, string.IsNullOrEmpty(k.icSes) ? SesAyni : SesEtiketi(k.icSes), 14);
            k.icSes = si == SesAyni ? "" : SesAdi(si);
            y += 42;
            KimlikEtiket(y, "Konuşma tarzı");
            k.konusmaTarzi = KimlikYazi(new Rect(x, y, 300, 32), k.konusmaTarzi, 40);
            Yazdir(new Rect(x + 312, y, w - x - 312, 32), "Örnek: yavaş ve kısık (en çok 3 kelime)", 12, Sönük);
            y += 36;
            Yazdir(new Rect(x, y, w - x, 34), "Sesi veya konuşma tarzını değiştirirsen bu karakterin satırları yeniden seslendirilmeli (Gemini kotası harcar).", 12, new Color(0.94f, 0.75f, 0.31f), TextAnchor.MiddleLeft, false, true);
            y += 50;

            KimlikBaslik(y, "Çizgi görünüm (3D modelde bu ayarlar görünmez)"); y += 32;
            KimlikEtiket(y, "Saç modeli");
            for (int i = 0; i < SacAdlari.Length; i++)
                if (Dugme(new Rect(x + i * 88, y, 82, 32), SacAdlari[i], k.sacModeli == i ? Vurgu : KimlikGri, 13)) k.sacModeli = i;
            y += 42;
            KimlikEtiket(y, "Sakal");
            k.sakal = Anahtar(new Rect(x, y + 3, 56, 28), k.sakal);
            y += 42;
            KimlikEtiket(y, "Ten rengi"); k.tenRengi = RenkSec(x, y, k.tenRengi, TenRenkleri); y += 42;
            KimlikEtiket(y, "Saç rengi"); k.sacRengi = RenkSec(x, y, k.sacRengi, SacRenkleri); y += 42;
            KimlikEtiket(y, "Üst giysi"); k.ustRengi = RenkSec(x, y, k.ustRengi, GiysiRenkleri); y += 42;
            KimlikEtiket(y, "Alt giysi"); k.altRengi = RenkSec(x, y, k.altRengi, GiysiRenkleri); y += 42;
            KimlikEtiket(y, "Altyazı rengi"); k.altyaziRengi = RenkSec(x, y, k.altyaziRengi, YaziRenkleri); y += 52;

            KimlikBaslik(y, "Notlar (görüntüyü değiştirmez, karakteri hatırlaman içindir)"); y += 32;
            KimlikEtiket(y, "Yüzü"); k.yuz = KimlikYazi(new Rect(x, y, w - x, 32), k.yuz, 200); y += 42;
            KimlikEtiket(y, "Kıyafeti"); k.kiyafet = KimlikYazi(new Rect(x, y, w - x, 32), k.kiyafet, 200); y += 42;
            KimlikEtiket(y, "Fiziksel özellikleri"); k.fiziksel = KimlikYazi(new Rect(x, y, w - x, 32), k.fiziksel, 200); y += 42;
            KimlikEtiket(y, "Duygusal özellikleri"); k.duygusal = KimlikYazi(new Rect(x, y, w - x, 32), k.duygusal, 200); y += 42;
            Yazdir(new Rect(0, y, w, 24), "Bir hikâyede \"oyuncu: " + k.ad + " = ...\" satırı yazarsan yalnız o hikâyede o model kullanılır.", 12, Sönük);

            ListeVer(ref kimlikFormKaydirma, g);
        }

        void KimlikEtiket(float y, string t) { Yazdir(new Rect(0, y, 184, 32), t, 14, Yazi); }

        void KimlikBaslik(float y, string t) { Yazdir(new Rect(0, y, 700, 28), t, 15, Vurgu, TextAnchor.MiddleLeft, true); }

        // renk kutuları; seçili olanın çevresi beyaz. Hazır listede olmayan mevcut renk başta gösterilir.
        string RenkSec(float x, float y, string secili, string[] hazir)
        {
            var l = new List<string>();
            bool hazirda = false;
            foreach (var h in hazir) if (string.Equals(h, secili, StringComparison.OrdinalIgnoreCase)) hazirda = true;
            if (!hazirda && !string.IsNullOrEmpty(secili)) l.Add(secili);
            l.AddRange(hazir);
            for (int i = 0; i < l.Count; i++)
            {
                var kr = new Rect(x + i * 42, y, 34, 32);
                if (string.Equals(l[i], secili, StringComparison.OrdinalIgnoreCase)) Kutu(new Rect(kr.x - 3, kr.y - 3, kr.width + 6, kr.height + 6), Color.white);
                Kutu(kr, KarakterKimligi.RenkOku(l[i], Color.gray));
                if (GUI.Button(kr, GUIContent.none, GUIStyle.none)) secili = l[i];
            }
            return secili;
        }

        void KimlikKaydet()
        {
            var k = kimlikForm;
            int yas = 0;
            if (kimlikYas.Trim().Length > 0 && (!int.TryParse(kimlikYas.Trim(), out yas) || yas < 0 || yas > 120))
            { mesaj = "Yaş 0-120 arası bir sayı olmalı."; mesajHata = true; return; }
            k.yas = yas;
            k.konusmaTarzi = (k.konusmaTarzi ?? "").Replace("\n", " ").Trim();
            if (k.konusmaTarzi.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length > 3)
            { mesaj = "Konuşma tarzı en çok 3 kelime olabilir (uzunu ses üretiminde kesilir)."; mesajHata = true; return; }
            string h = KimlikDeposu.Kaydet(k);
            if (h != null) { mesaj = h; mesajHata = true; Gunluk("Kimlik kartı HATA: " + h); return; }
            // ses artık karttan gelir: eski "Ses ayarları" ataması kartı ezmesin (kartta o ses zaten gösteriliyordu)
            DisSes.AtamaYaz(k.ad, false, "");
            DisSes.AtamaYaz(k.ad, true, "");
            PlayerPrefs.Save();
            KutuphaneYenile();
            kimlikKaldirOnay = false;
            mesaj = k.ad + ": kimlik kartı kaydedildi."; mesajHata = false;
            Gunluk("Kimlik kartı kaydedildi: " + k.ad);
        }
    }
}
