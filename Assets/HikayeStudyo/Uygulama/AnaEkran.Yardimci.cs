using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 6: yazma ekranındaki "Yardımcı" sekmesi.
    // Bütün değişiklikler Degistir() ile yapılır: Geri al / Yinele ve otomatik kayıt aynen çalışır.
    public partial class AnaEkran
    {
        bool yYardim;
        YardimAnaliz yAnaliz;
        string yAnalizMetin;
        string yYardimMesaj = "";
        int yFiltre;                 // 0 hepsi, 1..7 OneriTuru + 1, 8 sahneler, 9 gemini
        float yYardimKaydirma;

        static readonly string[] FiltreAdlari = { "Hepsi", "Karakter", "Mekân", "Duygu", "Kamera", "Tutarlılık", "Yazım", "Efekt", "Sahneler", "Gemini" };
        static readonly Color YardimGri = new Color32(60, 66, 88, 255);

        void YardimAc()
        {
            yYardim = true; yPano = false; ySes = false; ySeri = false;
            yAnalizMetin = null; yYardimMesaj = ""; yYardimKaydirma = 0f;
            if (yH != null && YardimciGemini.CevapKaynagi != yH.ad) YardimciGemini.Temizle();
        }

        void YardimInceleGerekirse()
        {
            if (yAnaliz != null && yAnalizMetin == yMetin) return;
            try { yAnaliz = YardimciMotor.Incele(yMetin, HikayeDenetci.KarakterAdlari(kutuphane), HikayeDenetci.MekanAdlari(kutuphane)); }
            catch (Exception e) { yAnaliz = new YardimAnaliz(); yYardimMesaj = "İnceleme hatası: " + e.Message; Gunluk("Yardimci HATA: " + e); }
            yAnalizMetin = yMetin;
            Gunluk("Yardimci inceleme: " + yAnaliz.Sahneler.Count + " sahne, " + yAnaliz.Oneriler.Count + " öneri");
        }

        void YardimUygula(string yeni, string mesajMetni)
        {
            if (yeni == null || yeni == yMetin) { yYardimMesaj = "Değişiklik yapılmadı."; return; }
            Degistir(yeni);
            yDenetimGerek = true; yDegisim = -10f;
            yYardimMesaj = mesajMetni + "  (Geri al ile geri alınır)";
        }

        void YardimCiz(Rect a)
        {
            // çizim ortasında listenin değişmemesi için inceleme yalnız Layout olayında yenilenir
            if (yAnaliz == null || Event.current.type == EventType.Layout) YardimInceleGerekirse();
            var kars = HikayeDenetci.KarakterAdlari(kutuphane);
            var mekanlar = HikayeDenetci.MekanAdlari(kutuphane);
            Kutu(a, Kart);

            // ---- eylem düğmeleri ----
            float bw = (a.width - 24f - 3 * 8f) / 4f;
            float x = a.x + 12, y = a.y + 8;
            int duz = YardimciMotor.DuzSatirSayisi(yMetin);
            if (Dugme(new Rect(x, y, bw, 34), "Düz metni sahnelere çevir (" + duz + ")", Vurgu, 12, duz > 0))
                YardimUygula(YardimciMotor.DuzMetniCevir(yMetin, kars, mekanlar), duz + " düz satır sahnelere çevrildi");
            if (Dugme(new Rect(x + (bw + 8), y, bw, 34), "Sahne açıklamalarını ekle", YardimGri, 12, yAnaliz.Sahneler.Count > 0))
                YardimUygula(YardimciMotor.AciklamalariEkle(yMetin, kars, mekanlar), "Sahne açıklamaları yorum satırı olarak eklendi");
            bool anahtarVar = GeminiUyg.Anahtar.Length > 0;
            if (Dugme(new Rect(x + 2 * (bw + 8), y, bw, 34), "Gemini: öneri al", YardimGri, 12, anahtarVar && !YardimciGemini.Busy))
            { YardimciGemini.Calistir(yH.ad, yMetin, false, kars, mekanlar); yFiltre = 9; yYardimKaydirma = 0f; }
            if (Dugme(new Rect(x + 3 * (bw + 8), y, bw, 34), "Gemini: biçime çevir", YardimGri, 12, anahtarVar && !YardimciGemini.Busy))
            { YardimciGemini.Calistir(yH.ad, yMetin, true, kars, mekanlar); yFiltre = 9; yYardimKaydirma = 0f; }
            y += 40;
            Yazdir(new Rect(x, y, a.width - 24, 18), "Öneriler internetsiz çalışır. Yalnız 'Gemini' düğmeleri hikâye metnini Google'a gönderir" +
                (anahtarVar ? "." : " (API anahtarı yok: kapalı)."), 11, Sönük);
            y += 22;

            // ---- filtreler ----
            float fw = (a.width - 24f - 9 * 4f) / 10f;
            for (int i = 0; i < FiltreAdlari.Length; i++)
            {
                int n = i == 0 ? yAnaliz.Oneriler.Count : (i <= 7 ? yAnaliz.Sayi((OneriTuru)(i - 1)) : (i == 8 ? yAnaliz.Sahneler.Count : -1));
                if (Dugme(new Rect(x + i * (fw + 4), y, fw, 30), FiltreAdlari[i] + (n >= 0 ? " " + n : ""), yFiltre == i ? Vurgu : YardimGri, 11))
                { yFiltre = i; yYardimKaydirma = 0f; }
            }
            y += 36;

            if (yFiltre >= 1 && yFiltre <= 7)
            {
                var tur = (OneriTuru)(yFiltre - 1);
                int uyg = 0;
                foreach (var o in yAnaliz.Oneriler) if (o.Tur == tur && o.Uygulanabilir) uyg++;
                if (Dugme(new Rect(x, y, 210, 30), "Bu türün tümünü uygula (" + uyg + ")", Vurgu, 12, uyg > 0))
                {
                    int n;
                    string yeni = YardimciMotor.TumunuUygula(yMetin, yAnaliz.Oneriler, tur, out n);
                    YardimUygula(yeni, n + " öneri uygulandı");
                }
                Yazdir(new Rect(x + 220, y, a.xMax - x - 232, 30), yYardimMesaj, 12, Basari);
            }
            else Yazdir(new Rect(x, y, a.width - 24, 30), yYardimMesaj, 12, Basari);
            y += 34;

            var g = new Rect(x, y, a.width - 24, a.yMax - y - 8);
            if (yFiltre == 9) YardimGeminiCiz(g);
            else if (yFiltre == 8) YardimSahneCiz(g);
            else YardimListeCiz(g, kars);
        }

        void YardimListeCiz(Rect g, List<string> kars)
        {
            var l = yFiltre == 0 ? yAnaliz.Oneriler : yAnaliz.Oneriler.FindAll(o => (int)o.Tur == yFiltre - 1);
            string ozet = yAnaliz.Karakterler.Count == 0 ? "Karakter algılanmadı." : "Algılanan karakterler: " + string.Join(", ", yAnaliz.Karakterler.ToArray());
            const float sat = 46f, ust = 40f;
            var kayma = new Vector2(g.x, g.y - yYardimKaydirma);
            YardimOneri uygulanacak = null;
            string eksikAd = null, yerine = null;
            ListeAl(ref yYardimKaydirma, g, Mathf.Max(g.height, ust + l.Count * sat));
            float w = g.width - 14f;
            Yazdir(new Rect(4, 0, w - 8, ust - 4), ozet, 12, Yazi, TextAnchor.MiddleLeft, false, true);
            if (l.Count == 0) Yazdir(new Rect(4, ust, w, 30), "Bu başlıkta öneri yok.", 13, Sönük);
            for (int i = 0; i < l.Count; i++)
            {
                var o = l[i];
                var r = new Rect(0, ust + i * sat, w, sat - 4);
                if (r.yMax < yYardimKaydirma - 4 || r.y > yYardimKaydirma + g.height + 4) continue;
                Kutu(r, new Color32(26, 31, 46, 255));
                bool eksik = o.EksikAd.Length > 0;
                float sag = eksik ? 330f : 160f;
                Yazdir(new Rect(10, r.y, r.width - sag - 14, r.height),
                    FiltreAdlari[(int)o.Tur + 1] + " · satır " + o.Satir + ": " + o.Mesaj, 12, Yazi, TextAnchor.MiddleLeft, false, true);
                if (Dugme(new Rect(r.xMax - 156, r.y + 6, 62, 30), "Git", YardimGri, 12)) AtlaSatir(o.Satir);
                if (eksik)
                {
                    string sec = SecG(new Rect(r.xMax - 326, r.y + 6, 164, 30), kayma, "yardim_kar_" + o.EksikAd,
                        kars, o.OnerilenAd.Length > 0 ? o.OnerilenAd : (kars.Count > 0 ? kars[0] : "Anlatici"), 12);
                    if (Dugme(new Rect(r.xMax - 88, r.y + 6, 82, 30), "Değiştir", Vurgu, 12)) { eksikAd = o.EksikAd; yerine = sec; }
                }
                else if (Dugme(new Rect(r.xMax - 88, r.y + 6, 82, 30), "Uygula", Vurgu, 12, o.Uygulanabilir)) uygulanacak = o;
            }
            ListeVer(ref yYardimKaydirma, g);

            if (uygulanacak != null) YardimUygula(YardimciMotor.Uygula(yMetin, uygulanacak), "Satır " + uygulanacak.Satir + " güncellendi");
            if (eksikAd != null)
            {
                int n;
                string yeni = YardimciMotor.KarakterDegistir(yMetin, eksikAd, yerine, out n);
                YardimUygula(yeni, eksikAd + " → " + yerine + ": " + n + " satır");
            }
        }

        void YardimSahneCiz(Rect g)
        {
            const float sat = 46f;
            ListeAl(ref yYardimKaydirma, g, Mathf.Max(g.height, yAnaliz.Sahneler.Count * sat));
            float w = g.width - 14f;
            int git = 0;
            for (int i = 0; i < yAnaliz.Sahneler.Count; i++)
            {
                var s = yAnaliz.Sahneler[i];
                var r = new Rect(0, i * sat, w, sat - 4);
                Kutu(r, new Color32(26, 31, 46, 255));
                Yazdir(new Rect(10, r.y, r.width - 90, r.height), s.Aciklama + "  (satır " + s.BasSatir + "–" + s.SonSatir + ")", 12, Yazi, TextAnchor.MiddleLeft, false, true);
                if (Dugme(new Rect(r.xMax - 68, r.y + 6, 62, 30), "Git", YardimGri, 12)) git = s.BasSatir;
            }
            ListeVer(ref yYardimKaydirma, g);
            if (git > 0) AtlaSatir(git);
        }

        void YardimGeminiCiz(Rect g)
        {
            Yazdir(new Rect(g.x, g.y, g.width - 320, 30), YardimciGemini.Durum.Length > 0 ? YardimciGemini.Durum : "Yukarıdaki Gemini düğmelerinden birini kullan.", 12, Sönük);
            if (YardimciGemini.Busy && Dugme(new Rect(g.xMax - 100, g.y, 100, 30), "Durdur", YardimGri, 12)) YardimciGemini.Durdur();
            bool buHikaye = YardimciGemini.CevapKaynagi == yH.ad;
            if (!YardimciGemini.Busy && buHikaye && YardimciGemini.CevapBicim && YardimciGemini.Cevap.Length > 0
                && Dugme(new Rect(g.xMax - 140, g.y, 140, 30), "Metne uygula", Vurgu, 12))
            {
                YardimUygula(YardimciGemini.Cevap, "Gemini metni uygulandı; Hata panelini kontrol et");
                YardimciGemini.Temizle();
                return;
            }
            if (!buHikaye || YardimciGemini.Cevap.Length == 0) return;
            var alan = new Rect(g.x, g.y + 36, g.width, g.height - 36);
            Kutu(alan, new Color32(14, 17, 25, 255));
            var stil = YaziStil(14, TextAnchor.UpperLeft, Yazi, false, true);
            var icerik = new GUIContent(YardimciGemini.Cevap);
            float h = stil.CalcHeight(icerik, alan.width - 30) + 20;
            ListeAl(ref yYardimKaydirma, alan, Mathf.Max(alan.height, h));
            GUI.Label(new Rect(8, 6, alan.width - 30, h), icerik, stil);
            ListeVer(ref yYardimKaydirma, alan);
        }
    }
}
