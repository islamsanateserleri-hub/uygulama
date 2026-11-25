using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // "Mekânlar" sayfası: hazır 3D mekânı seç, renklerini / zamanını / havasını / ışığını değiştir,
    // canlı önizle, adıyla kaydet ve hikâyede "mekan3d: ad" ile kullan.
    public partial class AnaEkran
    {
        static readonly Vector3 MekanMerkez = new Vector3(0f, -4000f, 0f);   // sahnelerden uzakta

        MekanTarifi mekanForm = new MekanTarifi { Tur = "ev" };
        string mekanAd = "";
        List<MekanTarifi> mekanKayitli;

        GameObject mekanStudyo;
        Mekan3D mekanModel;
        HavaDurumu3D mekanHava;
        Camera mekanKamera;
        Light mekanIsik;
        RenderTexture mekanResim;
        string mekanCizilen = "";

        static string MekanEtiket(string t)
        {
            switch (t)
            {
                case "oda": return "Oda";
                case "ev": return "Ev (salon)";
                case "mutfak": return "Mutfak";
                case "yatak_odasi": return "Yatak odası";
                case "hastane": return "Hastane";
                case "sokak": return "Sokak";
                case "koy": return "Köy";
                case "park": return "Park";
                case "araba": return "Araba içi";
                case "sabah": return "Sabah";
                case "gun": return "Gündüz";
                case "gun_batimi": return "Gün batımı";
                case "gece": return "Gece";
                case "yagmur": return "Yağmur";
                case "kar": return "Kar";
                case "sis": return "Sis";
                case "yok": return "Yok";
                case "normal": return "Normal";
                case "gunduz": return "Gün ışığı";
                case "dramatik": return "Dramatik";
                case "yumusak": return "Yumuşak";
                case "soguk": return "Soğuk";
                case "sicak": return "Sıcak";
                case "karanlik": return "Karanlık";
                case "mum": return "Mum";
                case "romantik": return "Romantik";
                case "umut": return "Umut";
                case "korku": return "Korku";
                case "huzunlu": return "Hüzünlü";
            }
            return t;
        }

        // Hikâyeye yazılacak satır
        string MekanSatiri()
        {
            if (mekanForm.Ad.Length > 0) return "mekan3d: " + mekanForm.Ad;
            var s = "mekan3d: " + mekanForm.Tur;
            foreach (var k in MekanTarifi.Anahtarlar)
                if (mekanForm.Deger(k).Length > 0) s += " " + k + "=" + mekanForm.Deger(k);
            return s;
        }

        // Düğmeleri satırlara sığdırarak dizer; yeni y döner
        float MekanSecim(float x, float y, float genislik, string etiket, string[] degerler, string secili, Action<string> sec, float dugmeGen)
        {
            var gri = new Color32(60, 66, 88, 255);
            Yazdir(new Rect(x, y, 90, 34), etiket, 15, Yazi);
            float cx = x + 96;
            foreach (var d in degerler)
            {
                if (cx + dugmeGen > x + genislik) { cx = x + 96; y += 40; }
                if (Dugme(new Rect(cx, y, dugmeGen, 34), MekanEtiket(d), secili == d ? Vurgu : (Color)gri, 13)) sec(d);
                cx += dugmeGen + 6;
            }
            return y + 46;
        }

        float MekanRenk(float x, float y, float genislik, string etiket, string anahtar)
        {
            var gri = new Color32(60, 66, 88, 255);
            string secili = mekanForm.Deger(anahtar);
            Yazdir(new Rect(x, y, 90, 30), etiket, 15, Yazi);
            float cx = x + 96;
            if (Dugme(new Rect(cx, y, 56, 30), "Hazır", secili.Length == 0 ? Vurgu : (Color)gri, 12)) MekanDegistir(anahtar, "");
            cx += 62;
            foreach (var ad in MekanTarifi.RenkAdlari)
            {
                if (cx + 30 > x + genislik) { cx = x + 96; y += 36; }
                Color c;
                MekanTarifi.RenkBul(ad, out c);
                var r = new Rect(cx, y, 30, 30);
                if (secili == ad) Kutu(new Rect(r.x - 3, r.y - 3, 36, 36), Color.white);
                Kutu(r, c);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) MekanDegistir(anahtar, ad);
                cx += 36;
            }
            return y + 42;
        }

        void MekanDegistir(string anahtar, string deger)
        {
            mekanForm.Ad = "";      // değişiklik henüz kaydedilmedi
            switch (anahtar)
            {
                case "tur": mekanForm.Tur = deger; break;
                case "duvar": mekanForm.Duvar = deger; break;
                case "zemin": mekanForm.Zemin = deger; break;
                case "vurgu": mekanForm.Vurgu = deger; break;
                case "zaman": mekanForm.Zaman = deger; break;
                case "hava": mekanForm.Hava = deger; break;
                case "isik": mekanForm.Isik = deger; break;
            }
        }

        void MekanlarCiz(Rect ic)
        {
            Baslik(ic, "Mekânlar", "3D mekânı seç, renklerini ve havasını ayarla, adıyla kaydet; hikâyede o adla kullan");
            if (mekanKayitli == null) mekanKayitli = MekanTarifi.Kayitlilar();
            var gri = new Color32(60, 66, 88, 255);

            float onGen = Mathf.Min(270f, ic.width * 0.3f);
            float onYuk = Mathf.Min(onGen * 16f / 9f, ic.height - 96f);
            onGen = onYuk * 9f / 16f;
            var onizleme = new Rect(ic.xMax - onGen, ic.y + 88, onGen, onYuk);
            Kutu(onizleme, Kart);
            MekanOnizle();
            if (mekanResim != null) GUI.DrawTexture(onizleme, mekanResim, ScaleMode.ScaleToFit, false);

            var gorunum = new Rect(ic.x, ic.y + 88, ic.width - onGen - 18, ic.height - 88);
            float g = gorunum.width - 16;
            var kutuAdlari = new List<string>();
            if (kutuphane != null) foreach (var b in kutuphane.backgrounds) if (b != null) kutuAdlari.Add(b.id);
            float icerik = 640 + mekanKayitli.Count * 44 + 90;
            ListeBasla(gorunum, icerik);
            float y = 0;
            y = MekanSecim(0, y, g, "Mekân", Mekan3D.TurAdlari, mekanForm.Tur, d => MekanDegistir("tur", d), 104);
            y = MekanRenk(0, y, g, "Duvar", "duvar");
            y = MekanRenk(0, y, g, "Zemin", "zemin");
            y = MekanRenk(0, y, g, "Eşya", "vurgu");
            y = MekanSecim(0, y, g, "Zaman", YonetmenTablo.Zamanlar, mekanForm.Zaman, d => MekanDegistir("zaman", mekanForm.Zaman == d ? "" : d), 92);
            y = MekanSecim(0, y, g, "Hava", YonetmenTablo.Havalar, mekanForm.Hava, d => MekanDegistir("hava", mekanForm.Hava == d ? "" : d), 80);
            y = MekanSecim(0, y, g, "Işık", YonetmenTablo.Isiklar, mekanForm.Isik, d => MekanDegistir("isik", mekanForm.Isik == d ? "" : d), 88);

            Yazdir(new Rect(0, y, 90, 36), "Adı", 15, Yazi);
            var alan = new Rect(96, y, 220, 36);
            Kutu(alan, Kart);
            var tf = new GUIStyle(YaziStil(15, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(12, 12, 0, 0), clipping = TextClipping.Clip };
            mekanAd = GUI.TextField(alan, mekanAd, 32, tf);
            if (Dugme(new Rect(324, y, 100, 36), "Kaydet", Vurgu, 14))
            {
                var kayit = new MekanTarifi
                {
                    Ad = mekanAd, Tur = mekanForm.Tur, Duvar = mekanForm.Duvar, Zemin = mekanForm.Zemin, Vurgu = mekanForm.Vurgu,
                    Zaman = mekanForm.Zaman, Hava = mekanForm.Hava, Isik = mekanForm.Isik
                };
                string hata = kayit.Kaydet();
                if (hata == null)
                {
                    mekanForm = kayit; mekanAd = kayit.Ad; mekanKayitli = MekanTarifi.Kayitlilar();
                    mesaj = "Kaydedildi: mekan3d: " + kayit.Ad;
                }
                else mesaj = hata;
                mesajHata = hata != null;
            }
            y += 44;
            Yazdir(new Rect(0, y, g - 150, 36), MekanSatiri(), 14, Basari);
            if (Dugme(new Rect(g - 140, y, 140, 36), "Satırı kopyala", gri, 13))
            {
                GUIUtility.systemCopyBuffer = MekanSatiri();
                mesaj = "Kopyalandı: " + MekanSatiri();
            }
            y += 40;
            Yazdir(new Rect(0, y, g, 28), mesaj, 13, Sönük);
            y += 36;

            Yazdir(new Rect(0, y, g, 30), "Kaydettiğin mekânlar", 16, Yazi, TextAnchor.MiddleLeft, true);
            y += 34;
            if (mekanKayitli.Count == 0) { Yazdir(new Rect(0, y, g, 30), "Henüz yok.", 14, Sönük); y += 44; }
            for (int i = 0; i < mekanKayitli.Count; i++)
            {
                var k = mekanKayitli[i];
                Kutu(new Rect(0, y, g, 38), Kart);
                Yazdir(new Rect(12, y, g - 260, 38), k.Ad + "  (" + MekanEtiket(k.Tur) + ")", 14, Yazi);
                if (Dugme(new Rect(g - 240, y + 3, 74, 32), "Aç", gri, 13)) { mekanForm = MekanTarifi.Oku(k.Ad) ?? mekanForm; mekanAd = k.Ad; mesaj = ""; }
                if (Dugme(new Rect(g - 160, y + 3, 74, 32), "Kopyala", gri, 13)) { GUIUtility.systemCopyBuffer = "mekan3d: " + k.Ad; mesaj = "Kopyalandı: mekan3d: " + k.Ad; }
                if (Dugme(new Rect(g - 80, y + 3, 74, 32), "Kaldır", Tehlike, 13))
                {
                    mesaj = MekanTarifi.Kaldir(k.Ad) ?? (k.Ad + " kaldırıldı (Mekanlar\\_silinen klasörüne taşındı).");
                    if (mekanForm.Ad == k.Ad) mekanForm.Ad = "";
                    mekanKayitli = MekanTarifi.Kayitlilar();
                    break;
                }
                y += 44;
            }
            Yazdir(new Rect(0, y + 4, g, 48), "Çizgi arka planlar (mekan:): " + string.Join(", ", kutuAdlari.ToArray()), 13, Sönük, TextAnchor.UpperLeft, false, true);
            ListeBitir(gorunum);
        }

        // Seçim değişince önizleme mekânını yeniden kurar
        void MekanOnizle()
        {
            if (Event.current.type != EventType.Repaint) return;
            string anahtar = mekanForm.Tur;
            foreach (var k in MekanTarifi.Anahtarlar) anahtar += "|" + mekanForm.Deger(k);
            if (anahtar == mekanCizilen && mekanStudyo != null) return;
            mekanCizilen = anahtar;
            try
            {
                if (mekanStudyo == null) MekanStudyoKur();
                if (mekanModel != null) { mekanModel.gameObject.SetActive(false); Destroy(mekanModel.gameObject); }
                mekanModel = Mekan3D.Kur(mekanStudyo.transform, mekanForm.Tur, mekanForm);
                if (mekanModel == null) return;
                mekanModel.transform.position = MekanMerkez;
                string zaman = mekanForm.Zaman.Length > 0 ? mekanForm.Zaman : "gun";
                mekanModel.ZamanUygula(zaman);

                bool araba = mekanForm.Tur == "araba";
                // videodaki genis planla ayni kadraj: gorunmeyen esya burada da gorunmesin
                mekanKamera.transform.position = MekanMerkez + (araba ? new Vector3(0f, 1.1f, -4.4f) : new Vector3(0f, 1.1f, -Sahne3D.CamDist));
                mekanKamera.transform.rotation = Quaternion.identity;
                mekanKamera.fieldOfView = Sahne3D.CamFov;

                Color gok = new Color(0.55f, 0.75f, 0.95f), isik = new Color(1f, 0.96f, 0.9f);
                float siddet = 1.15f;
                if (zaman == "sabah") { gok = new Color(0.62f, 0.78f, 0.95f); isik = new Color(1f, 0.93f, 0.8f); }
                else if (zaman == "gun_batimi") { gok = new Color(0.95f, 0.55f, 0.35f); isik = new Color(1f, 0.6f, 0.34f); }
                else if (zaman == "gece") { gok = new Color(0.02f, 0.03f, 0.08f); isik = new Color(0.5f, 0.6f, 1f); siddet = 0.55f; }
                switch (mekanForm.Isik)
                {
                    case "romantik": isik *= new Color(1f, 0.68f, 0.72f); break;
                    case "umut": isik *= new Color(1f, 0.9f, 0.68f); siddet *= 1.3f; break;
                    case "korku": isik *= new Color(0.62f, 0.85f, 0.72f); siddet *= 0.6f; break;
                    case "huzunlu": case "soguk": isik *= new Color(0.68f, 0.76f, 0.95f); siddet *= 0.75f; break;
                    case "sicak": isik *= new Color(1f, 0.8f, 0.55f); break;
                    case "mum": isik *= new Color(1f, 0.62f, 0.32f); siddet *= 0.55f; break;
                    case "karanlik": siddet *= 0.35f; break;
                    case "dramatik": siddet *= 1.3f; break;
                }
                mekanKamera.backgroundColor = mekanModel.Dis ? gok : new Color(0.03f, 0.03f, 0.04f);
                mekanIsik.color = isik;
                mekanIsik.intensity = siddet;

                mekanHava.merkez = MekanMerkez;
                mekanHava.Ayarla(mekanForm.Hava == "yagmur", mekanForm.Hava == "kar", mekanForm.Hava == "sis", mekanModel, zaman == "gece");
            }
            catch (Exception e) { Gunluk("Mekân önizleme HATA: " + e); }
        }

        void MekanStudyoKur()
        {
            mekanStudyo = new GameObject("MekanStudyosu");
            var kg = new GameObject("Kamera");
            kg.transform.SetParent(mekanStudyo.transform, false);
            mekanKamera = kg.AddComponent<Camera>();
            mekanKamera.nearClipPlane = 0.1f; mekanKamera.farClipPlane = 60f;
            mekanKamera.clearFlags = CameraClearFlags.SolidColor;
            mekanKamera.allowHDR = false;
            mekanResim = new RenderTexture(540, 960, 24, RenderTextureFormat.ARGB32) { name = "MekanOnizleme", antiAliasing = 2 };
            mekanKamera.targetTexture = mekanResim;
            var ig = new GameObject("Isik");
            ig.transform.SetParent(mekanStudyo.transform, false);
            ig.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            mekanIsik = ig.AddComponent<Light>();
            mekanIsik.type = LightType.Directional;
            var hg = new GameObject("Hava");
            hg.transform.SetParent(mekanStudyo.transform, false);
            mekanHava = hg.AddComponent<HavaDurumu3D>();
        }

        // Sayfadan çıkılınca (ya da hikâye oynatılınca) önizleme kapanır
        void MekanOnizlemeDenetle()
        {
            bool acik = !oynuyor && !yazmaAcik && bolum == Bolum.Mekanlar;
            if (acik) return;
            mekanKayitli = null;
            mekanCizilen = "";
            mekanModel = null;
            if (mekanStudyo != null) { mekanStudyo.SetActive(false); Destroy(mekanStudyo); mekanStudyo = null; }
            if (mekanResim != null) { mekanResim.Release(); Destroy(mekanResim); mekanResim = null; }
        }
    }
}
