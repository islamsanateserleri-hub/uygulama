using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 3: hikâye yazma ekranı + sahne panosu (AnaEkran'ın ikinci parçası).
    // - Metin alanı (satır numaraları, imleç satırı), hızlı ekleme çubuğu (menülerden seçim), hata paneli ("Düzelt"),
    //   geri al/yinele (Ctrl+Z / Ctrl+Y), otomatik kayıt, imleç satırından oynat, yan canlı önizleme.
    // - Sahne panosu: sahneler küçük kareler; sahneyi yenile, karakter/kamera değiştir, sırala. Küçük resimler
    //   sahne metninin özetine göre önbelleğe alınır: bir sahne değişince yalnız o sahne yeniden üretilir.
    public partial class AnaEkran
    {
        class Sahne
        {
            public int bas, son;                       // satır dizisinde [bas, son)
            public string ad = "", mekan = "", ilk = "", hash = "";
            public readonly List<string> kars = new List<string>();
            public int konusma;
            public float sure;
            public bool giris;
        }

        // ---- durum ----
        bool yazmaAcik;
        HikayeBilgi yH;
        string yMetin = "", yDiskte = "", yYol = "";
        float yDegisim = -10f;
        string yDurum = "";
        readonly List<string> yGeri = new List<string>();
        readonly List<string> yIleri = new List<string>();
        float yUndoZaman = -10f;
        float yKaydirma, ySorunKaydirma, yPanoKaydirma;
        int yImlec, yImlecIstek = -1, ySecimIstek = -1, yIstekSayaci;
        int yAtlaIdx = -1;
        bool yTakip;
        int yOncekiImlec = -1;
        List<DenetimSorunu> ySorunlar = new List<DenetimSorunu>();
        bool yDenetimGerek = true;
        float[] ySatirY = new float[1];
        float ySatirH = 20f;
        bool ySatirYGerek = true;
        float ySatirYGenislik;
        bool yPano;
        string[] yOyuncuAdlari = new string[0];
        bool yYedeklendi;

        // sahne panosu
        List<Sahne> ySahneler = new List<Sahne>();
        string[] ySatirlar = new string[0];
        int yBaslikSon, yAltBilgiBas;
        bool ySahneGerek = true;
        int ySecSahne;
        bool yUretiyor;
        string yUretimMesaj = "";
        readonly Dictionary<string, Texture2D> yKucukler = new Dictionary<string, Texture2D>();

        // açılır liste
        readonly Dictionary<string, string> yDeger = new Dictionary<string, string>();
        string acikId;
        Rect acikRect;
        List<string> acikSecenekler = new List<string>();
        float acikKaydirma;

        const string MetinAdi = "HikayeMetni";

        // =====================================================================
        // Açma / kapama / kaydetme
        // =====================================================================
        void YazmaAc(HikayeBilgi h)
        {
            try
            {
                if (h.yerlesik || string.IsNullOrEmpty(h.yol))
                {
                    // örnek hikâyeler düzenlenmez: kopyasını açarız
                    Directory.CreateDirectory(HikayeKlasoru);
                    string ad = h.ad + "_kopya"; int n = 2;
                    while (File.Exists(Path.Combine(HikayeKlasoru, ad + ".txt"))) ad = h.ad + "_kopya" + (n++);
                    string yol = Path.Combine(HikayeKlasoru, ad + ".txt");
                    File.WriteAllText(yol, h.metin, new UTF8Encoding(false));
                    HikayeleriYukle();
                    mesaj = "Örnek hikâye düzenlenemez; kopyası açıldı: " + ad; mesajHata = false;
                    h = new HikayeBilgi { ad = ad, yol = yol, metin = h.metin, yerlesik = false };
                }
                yH = h; yYol = h.yol;
                yMetin = File.ReadAllText(yYol, Encoding.UTF8).Replace("\r\n", "\n").Replace("\r", "\n");
                yDiskte = yMetin;
            }
            catch (Exception e) { mesaj = "Hikâye açılamadı: " + e.Message; mesajHata = true; return; }

            yGeri.Clear(); yIleri.Clear(); yUndoZaman = -10f; yDegisim = -10f;
            yKaydirma = ySorunKaydirma = yPanoKaydirma = 0f;
            yImlec = 0; yImlecIstek = 0; ySecimIstek = -1; yOncekiImlec = -1; yIstekSayaci = 0; yAtlaIdx = -1;
            yDenetimGerek = true; ySatirYGerek = true; ySahneGerek = true; ySecSahne = 0;
            yPano = false; acikId = null; yUretiyor = false; yUretimMesaj = "";
            foreach (var t in yKucukler.Values) if (t != null) Destroy(t);
            yKucukler.Clear();
            yDurum = "Açıldı";
            yYedeklendi = false;
            YedekAl();
            try { GUI.skin.settings.cursorColor = Color.white; GUI.skin.settings.selectionColor = new Color(0.35f, 0.55f, 1f, 0.55f); GUI.skin.settings.cursorFlashSpeed = 2f; } catch { }
            var adlar = new List<string>();
            var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
            if (ol != null) foreach (var pf in ol.prefablar) if (pf != null) adlar.Add(pf.name);
            yOyuncuAdlari = adlar.ToArray();
            yazmaAcik = true;
            Gunluk("YazmaAc: " + yH.ad + " (" + yMetin.Length + " karakter)");
        }

        void YedekAl()
        {
            if (yYedeklendi || string.IsNullOrEmpty(yYol) || !File.Exists(yYol)) return;
            try
            {
                string kl = Path.Combine(Kok, "Yedek");
                Directory.CreateDirectory(kl);
                File.Copy(yYol, Path.Combine(kl, yH.ad + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"), true);
                yYedeklendi = true;
            }
            catch { }
        }

        void YazmaKapat()
        {
            if (!yazmaAcik) return;
            KaydetYazma(true);
            if (CanliOnizleme.Al().Calisiyor) CanliOnizleme.Al().Durdur();
            yazmaAcik = false; acikId = null; yUretiyor = false;
            HikayeleriYukle();
        }

        bool KaydetYazma(bool sessiz)
        {
            if (!yazmaAcik && string.IsNullOrEmpty(yYol)) return false;
            if (yMetin == yDiskte && !string.IsNullOrEmpty(yDiskte)) return true;
            try
            {
                File.WriteAllText(yYol, yMetin.Replace("\n", "\r\n"), new UTF8Encoding(false));
                yDiskte = yMetin;
                yDurum = "Kaydedildi " + DateTime.Now.ToString("HH:mm:ss");
                return true;
            }
            catch (Exception e) { yDurum = "Kaydedilemedi: " + e.Message; return false; }
        }

        void OnApplicationQuit() { if (yazmaAcik) KaydetYazma(true); }

        // =====================================================================
        // Metin değişiklikleri (geri al / yinele)
        // =====================================================================
        void Isaretle()
        {
            yDegisim = Time.realtimeSinceStartup;
            yDenetimGerek = true; ySatirYGerek = true; ySahneGerek = true;
        }

        void Degistir(string yeni)
        {
            if (yeni == yMetin) return;
            yGeri.Add(yMetin); if (yGeri.Count > 300) yGeri.RemoveAt(0);
            yIleri.Clear();
            yMetin = yeni; yUndoZaman = -10f;
            Isaretle();
        }

        void MetinDegisti(string yeni)
        {
            float now = Time.realtimeSinceStartup;
            if (now - yUndoZaman > 0.8f || yGeri.Count == 0) { yGeri.Add(yMetin); if (yGeri.Count > 300) yGeri.RemoveAt(0); }
            yUndoZaman = now;
            yIleri.Clear();
            yMetin = yeni;
            Isaretle();
        }

        void GeriAl()
        {
            if (yGeri.Count == 0) return;
            yIleri.Add(yMetin);
            yMetin = yGeri[yGeri.Count - 1]; yGeri.RemoveAt(yGeri.Count - 1);
            yUndoZaman = -10f; Isaretle();
            yImlecIstek = Mathf.Min(yImlec, yMetin.Length); yIstekSayaci = 0;
        }

        void Yinele()
        {
            if (yIleri.Count == 0) return;
            yGeri.Add(yMetin);
            yMetin = yIleri[yIleri.Count - 1]; yIleri.RemoveAt(yIleri.Count - 1);
            yUndoZaman = -10f; Isaretle();
            yImlecIstek = Mathf.Min(yImlec, yMetin.Length); yIstekSayaci = 0;
        }

        // =====================================================================
        // Yardımcılar
        // =====================================================================
        string Deger(string id, string varsayilan)
        {
            string v;
            if (!yDeger.TryGetValue(id, out v) || string.IsNullOrEmpty(v)) { v = varsayilan; yDeger[id] = v; }
            return v;
        }

        int ImlecSatiri()
        {
            int n = 1;
            int son = Mathf.Min(yImlec, yMetin.Length);
            for (int i = 0; i < son; i++) if (yMetin[i] == '\n') n++;
            return n;
        }

        int SatirBaslangici(int satirNo)
        {
            int idx = 0, n = 1;
            while (n < satirNo && idx < yMetin.Length)
            {
                int k = yMetin.IndexOf('\n', idx);
                if (k < 0) return yMetin.Length;
                idx = k + 1; n++;
            }
            return idx;
        }

        static string SatirAnahtari(string satir)
        {
            string t = satir.Trim();
            if (t.Length == 0 || t[0] == '[' || t.StartsWith("//") || t[0] == '#') return "";
            int c = t.IndexOf(':');
            return c <= 0 ? "" : Util.Norm(t.Substring(0, c));
        }

        void SatirEkle(string satir, bool metniSec)
        {
            var lines = new List<string>(yMetin.Split('\n'));
            int cur = Mathf.Clamp(ImlecSatiri() - 1, 0, lines.Count - 1);
            int hedef;
            if (lines[cur].Trim().Length == 0) { lines[cur] = satir; hedef = cur; }
            else { lines.Insert(cur + 1, satir); hedef = cur + 1; }
            Degistir(string.Join("\n", lines.ToArray()));
            int bas = 0;
            for (int i = 0; i < hedef; i++) bas += lines[i].Length + 1;
            int son = bas + satir.Length;
            yImlecIstek = son; yIstekSayaci = 0; ySecimIstek = -1;
            if (metniSec)
            {
                int k = satir.IndexOf("] ", StringComparison.Ordinal);
                if (k >= 0) ySecimIstek = bas + k + 2;
            }
            yTakip = true;
        }

        void AtlaSatir(int satirNo)
        {
            if (satirNo <= 0) return;
            yPano = false;
            yAtlaIdx = SatirBaslangici(satirNo);
            yIstekSayaci = 0; yTakip = true;
        }

        List<string> KomutSirasi()
        {
            var l = new List<string> { "mekan", "mekan3d", "kamera", "efekt", "atmosfer", "muzik", "poz", "cikar", "oyuncu", "bekle", "kanca", "son", "secim", "sonuc", "gorsel", "yazi", "far", "flas", "temizle", "geri", "seri", "bolum", "baslik" };
            foreach (var b in KomutKayit.Hepsi()) if (!l.Contains(b.Ad)) l.Add(b.Ad);
            return l;
        }

        List<string> DegerSecenekleri(string cmd)
        {
            var l = new List<string>();
            switch (cmd)
            {
                case "mekan": l.AddRange(HikayeDenetci.MekanAdlari(kutuphane)); break;
                case "mekan3d": l.AddRange(YonetmenTablo.Mekanlar); break;
                case "kamera": l.AddRange(YonetmenTablo.Kameralar); break;
                case "efekt": l.AddRange(HikayeDenetci.EfektAdlari(kutuphane)); break;
                case "atmosfer": l.AddRange(new[] { "mum", "toz", "yagmur", "buyu", "gunes", "yok" }); l.AddRange(YonetmenTablo.Atmosferler); break;
                case "muzik":
                    l.Add("pad"); l.Add("yok");
                    if (kutuphane != null) foreach (var m in kutuphane.music) if (m != null && !string.IsNullOrEmpty(m.id)) l.Add(m.id);
                    break;
                case "gorsel":
                    l.Add("eller"); l.Add("yok");
                    if (kutuphane != null) foreach (var m in kutuphane.inserts) if (m != null && !string.IsNullOrEmpty(m.id)) l.Add(m.id);
                    break;
                case "poz": l.AddRange(Util.Poses); break;
                case "oyuncu": l.AddRange(yOyuncuAdlari); break;
                case "bekle": l.AddRange(new[] { "0.5", "1", "1.5", "2", "3" }); break;
                case "son": l.AddRange(new[] { "yumusak", "sert" }); break;
                case "isik": l.AddRange(YonetmenTablo.Isiklar); break;
                case "zaman": l.AddRange(YonetmenTablo.Zamanlar); break;
                case "gecis": l.AddRange(YonetmenTablo.Gecisler); break;
                case "ani": l.AddRange(YonetmenTablo.Animlar); break;
                default: return null;
            }
            return l;
        }

        string KomutSatiri(string cmd, string deger, string karakter)
        {
            switch (cmd)
            {
                case "poz": return "poz: " + karakter + " " + deger;
                case "oyuncu": return "oyuncu: " + karakter + " = " + deger;
                case "cikar": return "cikar: " + karakter;
                case "kanca": return "kanca: Buraya ilk cümleni yaz.";
                case "son": return "son: " + deger + (deger == "yumusak" ? " Devamı yarın..." : "");
            }
            if (!string.IsNullOrEmpty(deger)) return cmd + ": " + deger;
            foreach (var b in KomutKayit.Hepsi()) if (b.Ad == cmd && !string.IsNullOrEmpty(b.Ornek)) return b.Ornek;
            return cmd + ": ";
        }

        // =====================================================================
        // Açılır liste
        // =====================================================================
        string Sec(Rect r, string id, string etiket, IList<string> secenekler, string varsayilan, int boyut = 14)
        {
            string v = Deger(id, varsayilan);
            bool ustte = r.Contains(Event.current.mousePosition) && GUI.enabled;
            Kutu(r, ustte ? new Color32(62, 72, 100, 255) : new Color32(46, 54, 78, 255));
            Yazdir(new Rect(r.x + 8, r.y, r.width - 26, r.height), (etiket.Length > 0 ? etiket + ": " : "") + v, boyut, Yazi);
            Yazdir(new Rect(r.xMax - 22, r.y, 18, r.height), "v", 12, Sönük, TextAnchor.MiddleCenter);
            if (secenekler != null && secenekler.Count > 0 && GUI.Button(r, GUIContent.none, GUIStyle.none))
            {
                acikId = id; acikRect = r; acikKaydirma = 0f;
                acikSecenekler = new List<string>(secenekler);
            }
            return v;
        }

        void AcilirCiz()
        {
            if (acikId == null) return;
            GUI.enabled = true;
            const float oge = 30f;
            int n = acikSecenekler.Count;
            float h = Mathf.Min(n * oge + 8f, 330f);
            float w = Mathf.Max(acikRect.width, 170f);
            var r = new Rect(acikRect.x, acikRect.yMax + 2, w, h);
            if (r.yMax > gh - 6) r.y = Mathf.Max(6f, acikRect.y - h - 2);
            if (r.xMax > gw - 6) r.x = gw - 6 - w;
            Kutu(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), new Color32(90, 140, 255, 255));
            Kutu(r, new Color32(20, 24, 36, 255));
            var g = new Rect(r.x + 4, r.y + 4, r.width - 8, r.height - 8);
            string secilen = null;
            ListeAl(ref acikKaydirma, g, n * oge);
            for (int i = 0; i < n; i++)
            {
                var ir = new Rect(0, i * oge, g.width - 12f, oge - 2f);
                if (ir.Contains(Event.current.mousePosition)) Kutu(ir, new Color32(52, 64, 100, 255));
                Yazdir(new Rect(ir.x + 10, ir.y, ir.width - 12, ir.height), acikSecenekler[i], 14, Yazi);
                if (GUI.Button(ir, GUIContent.none, GUIStyle.none)) secilen = acikSecenekler[i];
            }
            ListeVer(ref acikKaydirma, g);
            if (secilen != null) { yDeger[acikId] = secilen; acikId = null; }
            else if (GUI.Button(new Rect(-4, -4, gw + 8, gh + 8), GUIContent.none, GUIStyle.none)) acikId = null;
        }

        // kaydirma alanını (ortak kaydırma kodu) ayrı bir değişkenle kullanmak için sarmalayıcılar
        void ListeAl(ref float k, Rect g, float icerik)
        {
            float s = kaydirma; kaydirma = k;
            ListeBasla(g, icerik);
            k = kaydirma; kaydirma = s;
        }

        void ListeVer(ref float k, Rect g)
        {
            float s = kaydirma; kaydirma = k;
            ListeBitir(g);
            k = kaydirma; kaydirma = s;
        }

        // =====================================================================
        // Ana çizim
        // =====================================================================
        void YazmaOnGUI(Rect ic)
        {
            var ev = Event.current;
            float now = Time.realtimeSinceStartup;

            if (acikId == null && !yUretiyor && ev.type == EventType.KeyDown && ev.control)
            {
                if (ev.keyCode == KeyCode.Z && !ev.shift) { GeriAl(); ev.Use(); }
                else if (ev.keyCode == KeyCode.Y || (ev.keyCode == KeyCode.Z && ev.shift)) { Yinele(); ev.Use(); }
                else if (ev.keyCode == KeyCode.S) { KaydetYazma(false); ev.Use(); }
            }

            if (yDenetimGerek && now - yDegisim > 0.5f)
            {
                try { ySorunlar = HikayeDenetci.Denetle(yMetin, kutuphane, yOyuncuAdlari); }
                catch (Exception e) { ySorunlar = new List<DenetimSorunu>(); Gunluk("Denetim HATA: " + e.Message); }
                yDenetimGerek = false;
            }
            if (yMetin != yDiskte && now - yDegisim > 1.5f && !yUretiyor) KaydetYazma(true);
            if (yMetin != yDiskte) yDurum = "Kaydedilmedi…";

            GUI.enabled = acikId == null && !yUretiyor;

            // ---- üst çubuk ----
            if (Dugme(new Rect(ic.x, ic.y, 150, 38), "Hikâyelerim", new Color32(60, 66, 88, 255), 14)) { Git(Bolum.Hikayelerim); GUI.enabled = true; return; }
            Yazdir(new Rect(ic.x + 164, ic.y, ic.width - 520, 38), yH.ad, 22, Yazi, TextAnchor.MiddleLeft, true);
            bool kayitli = yMetin == yDiskte;
            Yazdir(new Rect(ic.xMax - 350, ic.y, 140, 38), yDurum, 12, kayitli ? Basari : new Color32(240, 190, 80, 255), TextAnchor.MiddleRight);
            if (Dugme(new Rect(ic.xMax - 200, ic.y, 98, 38), "Geri al", new Color32(60, 66, 88, 255), 13, yGeri.Count > 0)) GeriAl();
            if (Dugme(new Rect(ic.xMax - 96, ic.y, 96, 38), "Yinele", new Color32(60, 66, 88, 255), 13, yIleri.Count > 0)) Yinele();

            // ---- sekmeler ----
            float ty = ic.y + 44;
            if (Dugme(new Rect(ic.x, ty, 150, 34), "Yaz", !yPano ? Vurgu : new Color32(60, 66, 88, 255), 14)) yPano = false;
            if (Dugme(new Rect(ic.x + 158, ty, 170, 34), "Sahne panosu", yPano ? Vurgu : new Color32(60, 66, 88, 255), 14)) { yPano = true; ySahneGerek = true; }
            int hata = HikayeDenetci.HataSayisi(ySorunlar);
            int uyari = ySorunlar.Count - hata;
            string ozet = hata == 0 && uyari == 0 ? "Sorun yok" : hata + " hata, " + uyari + " uyarı";
            Yazdir(new Rect(ic.x + 340, ty, 260, 34), ozet, 13, hata > 0 ? new Color(1f, 0.59f, 0.59f) : (uyari > 0 ? new Color(0.94f, 0.75f, 0.31f) : Basari));
            Yazdir(new Rect(ic.xMax - 330, ty, 330, 34), "İmleç: satır " + ImlecSatiri() + "   ·   Ctrl+Z geri al, Ctrl+Y yinele", 12, Sönük, TextAnchor.MiddleRight);

            var alan = new Rect(ic.x, ty + 40, ic.width, ic.yMax - (ty + 40));
            if (yPano) PanoCiz(alan); else YazCiz(alan);

            GUI.enabled = true;
            if (yUretiyor)
            {
                var r = new Rect(ic.x + ic.width * 0.5f - 260, ic.y + ic.height * 0.5f - 40, 520, 80);
                Kutu(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), Vurgu);
                Kutu(r, new Color32(20, 24, 36, 255));
                Yazdir(r, yUretimMesaj, 16, Yazi, TextAnchor.MiddleCenter, true, true);
            }
            AcilirCiz();
        }

        // =====================================================================
        // Yaz sekmesi
        // =====================================================================
        void YazCiz(Rect a)
        {
            float onizGen = 250f;
            float solGen = a.width - onizGen - 12f;

            // ---- hızlı ekleme çubuğu ----
            Kutu(new Rect(a.x, a.y, solGen, 76), Kart);
            Yazdir(new Rect(a.x + 10, a.y + 4, 70, 32), "Konuşma", 13, Sönük);
            var kars = HikayeDenetci.KarakterAdlari(kutuphane);
            var duygular = new List<string>(Util.Emotions);
            var hareketler = new List<string> { "yok" }; hareketler.AddRange(Util.Motions);
            float x = a.x + 80, yy = a.y + 5;
            string k = Sec(new Rect(x, yy, 150, 30), "karakter", "Karakter", kars, kars.Count > 0 ? kars[0] : "Anlatici"); x += 156;
            string d = Sec(new Rect(x, yy, 112, 30), "duygu", "", duygular, "normal"); x += 118;
            string m = Sec(new Rect(x, yy, 72, 30), "mod", "", new[] { "dis", "ic" }, "dis"); x += 78;
            string h = Sec(new Rect(x, yy, 112, 30), "hareket", "", hareketler, "yok"); x += 118;
            if (Dugme(new Rect(x, yy, a.x + solGen - x - 8, 30), "Satır ekle", Vurgu, 13))
                SatirEkle("[" + k + "|" + d + "|" + m + (h != "yok" ? "|" + h : "") + "] Metni buraya yaz", true);

            Yazdir(new Rect(a.x + 10, a.y + 40, 70, 30), "Komut", 13, Sönük);
            var komutlar = KomutSirasi();
            float x2 = a.x + 80, y2 = a.y + 41;
            string cmd = Sec(new Rect(x2, y2, 150, 30), "komut", "", komutlar, "mekan"); x2 += 156;
            var secenek = DegerSecenekleri(cmd);
            string deger = "";
            if (secenek != null && secenek.Count > 0)
            {
                if (!secenek.Contains(Deger("deger_" + cmd, secenek[0]))) yDeger["deger_" + cmd] = secenek[0];
                deger = Sec(new Rect(x2, y2, 170, 30), "deger_" + cmd, "", secenek, secenek[0]);
            }
            else Yazdir(new Rect(x2, y2, 170, 30), "(örnek satır eklenir)", 12, Sönük);
            x2 += 176;
            if (Dugme(new Rect(x2, y2, 90, 30), "Ekle", Vurgu, 13)) SatirEkle(KomutSatiri(cmd, deger, k), false);
            string aciklama = "";
            foreach (var b in KomutKayit.Hepsi()) if (b.Ad == cmd) { aciklama = b.Aciklama; break; }
            Yazdir(new Rect(x2 + 96, y2 - 1, a.xMax - onizGen - 12 - (x2 + 96) + a.x, 32), aciklama, 11, Sönük, TextAnchor.MiddleLeft, false, true);

            // ---- metin alanı ----
            var ed = new Rect(a.x, a.y + 82, solGen, a.height - 82 - 142);
            MetinAlani(ed);

            // ---- sorun paneli ----
            SorunPaneli(new Rect(a.x, ed.yMax + 6, solGen, a.yMax - ed.yMax - 6));

            // ---- önizleme ----
            OnizlemeCiz(new Rect(a.xMax - onizGen, a.y, onizGen, a.height));
        }

        void MetinAlani(Rect ed)
        {
            Kutu(ed, new Color32(14, 17, 25, 255));
            var stil = new GUIStyle(YaziStil(16, TextAnchor.UpperLeft, Yazi, false, true));
            stil.padding = new RectOffset(8, 8, 6, 6);
            stil.clipping = TextClipping.Clip;
            stil.focused.textColor = Yazi; stil.hover.textColor = Yazi; stil.active.textColor = Yazi;
            stil.richText = false;

            const float numGen = 46f;
            var gorunum = new Rect(ed.x + 4, ed.y + 4, ed.width - 8, ed.height - 8);
            float metinGen = gorunum.width - numGen - 14f;
            var icerik = new GUIContent(yMetin + " ");
            float icH = Mathf.Max(gorunum.height, stil.CalcHeight(icerik, metinGen) + 24f);
            ySatirH = Mathf.Max(18f, stil.lineHeight);
            var metinRect = new Rect(numGen, 0, metinGen, icH);

            if (ySatirYGerek || Mathf.Abs(ySatirYGenislik - metinGen) > 0.5f)
            {
                ySatirYHesapla(stil, metinRect, icerik);
                ySatirYGenislik = metinGen; ySatirYGerek = false;
            }

            ListeAl(ref yKaydirma, gorunum, icH);

            // satır numaraları + imleç satırı + hata işaretleri
            int imlecSatir = ImlecSatiri();
            var sorunSatir = new Dictionary<int, bool>();
            foreach (var s in ySorunlar) if (s.Satir > 0) { bool e0; if (!sorunSatir.TryGetValue(s.Satir, out e0) || (s.Hata && !e0)) sorunSatir[s.Satir] = s.Hata; }
            for (int i = 0; i < ySatirY.Length; i++)
            {
                float sy = ySatirY[i];
                if (sy + ySatirH < yKaydirma - 4 || sy > yKaydirma + gorunum.height + 4) continue;
                if (i + 1 == imlecSatir) Kutu(new Rect(0, sy, numGen + metinGen, ySatirH), new Color(1f, 1f, 1f, 0.07f));
                bool hataVar;
                bool var0 = sorunSatir.TryGetValue(i + 1, out hataVar);
                Color nc = var0 ? (hataVar ? new Color(1f, 0.47f, 0.47f) : new Color(0.94f, 0.75f, 0.31f)) : Sönük;
                Yazdir(new Rect(0, sy, numGen - 8, ySatirH), (i + 1).ToString(), 12, nc, TextAnchor.MiddleRight);
                if (var0) Kutu(new Rect(2, sy + ySatirH * 0.5f - 3, 6, 6), hataVar ? Tehlike : new Color32(240, 190, 80, 255));
            }

            GUI.SetNextControlName(MetinAdi);
            string yeni = GUI.TextArea(metinRect, yMetin, stil);
            if (yeni != yMetin) MetinDegisti(yeni);

            // imleç bilgisi / istekleri
            bool odakta = GUI.GetNameOfFocusedControl() == MetinAdi;
            if (odakta)
            {
                var te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                if (te != null)
                {
                    if (yAtlaIdx >= 0) { te.cursorIndex = te.selectIndex = Mathf.Min(yAtlaIdx, yMetin.Length); yAtlaIdx = -1; yTakip = true; }
                    else if (yImlecIstek >= 0)
                    {
                        te.cursorIndex = Mathf.Min(yImlecIstek, yMetin.Length);
                        te.selectIndex = ySecimIstek >= 0 ? Mathf.Min(ySecimIstek, yMetin.Length) : te.cursorIndex;
                        yImlecIstek = -1; ySecimIstek = -1; yTakip = true;
                    }
                    yImlec = Mathf.Min(te.cursorIndex, yMetin.Length);
                }
            }
            else if ((yAtlaIdx >= 0 || yImlecIstek >= 0) && yIstekSayaci++ < 30) GUI.FocusControl(MetinAdi);
            else { yAtlaIdx = -1; if (yIstekSayaci >= 30) yImlecIstek = -1; }

            ListeVer(ref yKaydirma, gorunum);

            // imleci görünür tut
            if ((yImlec != yOncekiImlec || yTakip) && Event.current.type == EventType.Repaint)
            {
                float cy = ySatirY.Length > 0 ? ySatirY[Mathf.Clamp(ImlecSatiri() - 1, 0, ySatirY.Length - 1)] : 0f;
                try
                {
                    var p = stil.GetCursorPixelPosition(metinRect, icerik, Mathf.Min(yImlec, yMetin.Length));
                    cy = p.y;
                }
                catch { }
                if (cy < yKaydirma) yKaydirma = Mathf.Max(0f, cy - 4f);
                else if (cy + ySatirH > yKaydirma + gorunum.height - 8f) yKaydirma = cy + ySatirH - gorunum.height + 12f;
                yOncekiImlec = yImlec; yTakip = false;
            }
        }

        void ySatirYHesapla(GUIStyle stil, Rect metinRect, GUIContent icerik)
        {
            var yler = new List<float>();
            int idx = 0;
            while (true)
            {
                float yv;
                try { yv = stil.GetCursorPixelPosition(metinRect, icerik, idx).y; }
                catch { yv = yler.Count > 0 ? yler[yler.Count - 1] + ySatirH : 6f; }
                yler.Add(yv);
                int k = yMetin.IndexOf('\n', idx);
                if (k < 0) break;
                idx = k + 1;
            }
            ySatirY = yler.ToArray();
        }

        void SorunPaneli(Rect r)
        {
            Kutu(r, Kart);
            int hata = HikayeDenetci.HataSayisi(ySorunlar);
            Yazdir(new Rect(r.x + 12, r.y + 4, r.width - 24, 24),
                ySorunlar.Count == 0 ? "Hata paneli: sorun yok" : "Hata paneli: " + hata + " hata, " + (ySorunlar.Count - hata) + " uyarı  (satıra tıkla, atlar)", 13, Sönük);
            var g = new Rect(r.x + 8, r.y + 30, r.width - 16, r.height - 36);
            const float sat = 28f;
            ListeAl(ref ySorunKaydirma, g, Mathf.Max(g.height, ySorunlar.Count * sat));
            for (int i = 0; i < ySorunlar.Count; i++)
            {
                var s = ySorunlar[i];
                var sr = new Rect(0, i * sat, g.width - 14, sat - 2);
                if (sr.Contains(Event.current.mousePosition)) Kutu(sr, new Color32(46, 54, 78, 255));
                Kutu(new Rect(sr.x + 4, sr.y + 9, 8, 8), s.Hata ? Tehlike : new Color32(240, 190, 80, 255));
                string etiket = s.Satir > 0 ? "Satır " + s.Satir + ": " : "Genel: ";
                bool duzelt = !string.IsNullOrEmpty(s.Duzeltme) && s.Satir > 0;
                string metin = etiket + s.Mesaj + (duzelt ? "  → " + s.Duzeltme : "");
                Yazdir(new Rect(sr.x + 20, sr.y, sr.width - (duzelt ? 130 : 24), sr.height), metin, 12, Yazi);
                if (GUI.Button(new Rect(sr.x, sr.y, sr.width - (duzelt ? 112 : 0), sr.height), GUIContent.none, GUIStyle.none)) AtlaSatir(s.Satir);
                if (duzelt && Dugme(new Rect(sr.xMax - 106, sr.y + 2, 100, sat - 6), "Düzelt", Vurgu, 12))
                {
                    string yeni = HikayeDenetci.Duzelt(yMetin, s);
                    if (yeni != null) { Degistir(yeni); yDenetimGerek = true; yDegisim = -10f; }
                }
            }
            ListeVer(ref ySorunKaydirma, g);
        }

        // =====================================================================
        // Canlı önizleme paneli
        // =====================================================================
        void OnizlemeCiz(Rect r)
        {
            var o = CanliOnizleme.Al();
            const float kareY = 444f;
            var kare = new Rect(r.x, r.y, r.width, kareY);
            Kutu(kare, Color.black);
            if (o.RT != null && o.Aktif)
            {
                GUI.DrawTexture(new Rect(kare.x + 2, kare.y + 2, kare.width - 4, kare.height - 4), o.RT, ScaleMode.StretchToFill, false);
                if (o.Bitti) Yazdir(new Rect(kare.x, kare.yMax - 36, kare.width, 30), "Bitti", 14, Color.white, TextAnchor.MiddleCenter, true);
            }
            else
            {
                string t = o.Hazirlaniyor ? "Önizleme hazırlanıyor…" : (o.Hata.Length > 0 ? o.Hata : "Canlı önizleme\n\nBaştan oynat ya da imlecin\nbulunduğu satırdan oynat.");
                Yazdir(kare, t, 14, o.Hata.Length > 0 ? new Color32(255, 150, 150, 255) : Sönük, TextAnchor.MiddleCenter, false, true);
            }
            float by = kare.yMax + 8;
            float bw = (r.width - 8) / 2f;
            bool calisiyor = o.Calisiyor;
            if (Dugme(new Rect(r.x, by, bw, 36), "Baştan oynat", Vurgu, 13)) Oynat(0);
            if (Dugme(new Rect(r.x + bw + 8, by, bw, 36), "Satır " + ImlecSatiri() + "’den", Vurgu, 13)) Oynat(ImlecSatiri());
            if (Dugme(new Rect(r.x, by + 42, r.width, 32), "Durdur", new Color32(60, 66, 88, 255), 13, calisiyor)) o.Durdur();
        }

        void Oynat(int satir)
        {
            KaydetYazma(true);
            CanliOnizleme.Al().Baslat(yH.ad, yMetin, satir, false);
        }

        // =====================================================================
        // Sahne panosu
        // =====================================================================
        static bool BaslikSatiriMi(string satir)
        {
            string t = satir.Trim();
            if (t.Length == 0 || t.StartsWith("//") || t[0] == '#') return true;
            string k = SatirAnahtari(satir);
            return k == "seri" || k == "bolum" || k == "baslik" || k == "kanca" || k == "oyuncu" || k == "geri";
        }

        static bool AltBilgiSatiriMi(string satir)
        {
            string t = satir.Trim();
            if (t.Length == 0 || t.StartsWith("//") || t[0] == '#') return true;
            string k = SatirAnahtari(satir);
            return k == "son" || k == "secim" || k == "sonuc";
        }

        // Boş satırlar, yorumlar ve satır sonu boşlukları özeti değiştirmez (sahne taşınınca küçük resim eskimesin)
        static string Normallestir(string metin)
        {
            var sb = new StringBuilder();
            foreach (var l in (metin ?? "").Split('\n'))
            {
                string t = l.Trim();
                if (t.Length == 0 || t.StartsWith("//") || t[0] == '#') continue;
                sb.Append(t).Append('\n');
            }
            return sb.ToString();
        }

        static string Ozet(string metin)
        {
            metin = Normallestir(metin);
            using (var md5 = MD5.Create())
            {
                var b = md5.ComputeHash(Encoding.UTF8.GetBytes(metin ?? ""));
                var sb = new StringBuilder();
                for (int i = 0; i < 8; i++) sb.Append(b[i].ToString("x2"));
                return sb.ToString();
            }
        }

        string BaslikMetni()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < yBaslikSon && i < ySatirlar.Length; i++) sb.Append(ySatirlar[i]).Append('\n');
            return sb.ToString();
        }

        void SahneleriKur()
        {
            ySahneler = new List<Sahne>();
            ySatirlar = yMetin.Split('\n');
            int n = ySatirlar.Length;
            int fb = n;
            while (fb > 0 && AltBilgiSatiriMi(ySatirlar[fb - 1])) fb--;
            int hb = 0;
            while (hb < fb && BaslikSatiriMi(ySatirlar[hb])) hb++;
            yBaslikSon = hb; yAltBilgiBas = fb;

            var basSatirlar = new List<int>();
            for (int i = hb; i < fb; i++) if (SatirAnahtari(ySatirlar[i]) == "mekan") basSatirlar.Add(i);
            bool girisVar = false;
            if (basSatirlar.Count == 0 || basSatirlar[0] > hb)
            {
                int ilkMekan = basSatirlar.Count > 0 ? basSatirlar[0] : fb;
                bool konusmaVar = false;
                for (int i = hb; i < ilkMekan; i++) if (ySatirlar[i].TrimStart().StartsWith("[")) { konusmaVar = true; break; }
                if (konusmaVar) { basSatirlar.Insert(0, hb); girisVar = true; }
                else if (basSatirlar.Count > 0) basSatirlar[0] = hb;
                else if (fb > hb) basSatirlar.Add(hb);
            }
            string baslikMetni = Normallestir(BaslikMetni());
            for (int s = 0; s < basSatirlar.Count; s++)
            {
                int bas = basSatirlar[s], son = s + 1 < basSatirlar.Count ? basSatirlar[s + 1] : fb;
                var sh = new Sahne { bas = bas, son = son, giris = girisVar && s == 0 };
                var sb = new StringBuilder();
                for (int i = bas; i < son; i++)
                {
                    string l = ySatirlar[i]; sb.Append(l).Append('\n');
                    string t = l.Trim();
                    if (t.StartsWith("["))
                    {
                        int c = t.IndexOf(']');
                        if (c > 1)
                        {
                            string ch = t.Substring(1, c - 1).Split('|')[0].Trim();
                            if (ch.Length > 0 && !sh.kars.Contains(ch)) sh.kars.Add(ch);
                            sh.konusma++;
                            if (sh.ilk.Length == 0) sh.ilk = t.Substring(c + 1).Trim();
                        }
                    }
                    else if (SatirAnahtari(l) == "mekan" && sh.mekan.Length == 0)
                    {
                        int c = t.IndexOf(':'); sh.mekan = t.Substring(c + 1).Trim();
                    }
                }
                string sm = sb.ToString();
                sh.ad = sh.giris ? "Giriş" : (sh.mekan.Length > 0 ? sh.mekan : "Sahne");
                try { sh.sure = HikayeDenetci.ToplamSure(StoryParser.Parse(sm)); } catch { sh.sure = 0f; }
                sh.hash = Ozet(baslikMetni + "\u0001" + sm);
                ySahneler.Add(sh);
            }
            if (ySecSahne >= ySahneler.Count) ySecSahne = Mathf.Max(0, ySahneler.Count - 1);
            ySahneGerek = false;
        }

        string KucukYolu(string hash) { return Path.Combine(Path.Combine(Path.Combine(Kok, "Onbellek"), yH.ad), hash + ".png"); }

        Texture2D KucukAl(string hash)
        {
            Texture2D t;
            if (yKucukler.TryGetValue(hash, out t)) return t;
            string yol = KucukYolu(hash);
            if (File.Exists(yol))
            {
                try
                {
                    t = new Texture2D(2, 2, TextureFormat.RGB24, false);
                    if (t.LoadImage(File.ReadAllBytes(yol))) { yKucukler[hash] = t; return t; }
                    Destroy(t);
                }
                catch { }
            }
            yKucukler[hash] = null;
            return null;
        }

        int EksikKucukSayisi()
        {
            int n = 0;
            foreach (var s in ySahneler) if (KucukAl(s.hash) == null) n++;
            return n;
        }

        void PanoCiz(Rect a)
        {
            if (ySahneGerek) SahneleriKur();
            int eksik = EksikKucukSayisi();
            Yazdir(new Rect(a.x, a.y, a.width - 360, 30), ySahneler.Count + " sahne  ·  " + (eksik == 0 ? "tüm küçük resimler güncel" : eksik + " küçük resim eksik/eski") +
                "  ·  Bir sahneyi değiştirince yalnız o sahne yenilenir", 13, Sönük);
            if (Dugme(new Rect(a.xMax - 350, a.y, 170, 30), "Eksikleri üret (" + eksik + ")", Vurgu, 13, eksik > 0 && ySahneler.Count > 0)) KucukUretBaslat(false);
            if (Dugme(new Rect(a.xMax - 170, a.y, 170, 30), "Tümünü yeniden üret", new Color32(60, 66, 88, 255), 13, ySahneler.Count > 0)) KucukUretBaslat(true);

            // ---- sahne kartları ----
            const float kartG = 226f, kartY = 230f, bosluk = 12f;
            int sutun = Mathf.Max(1, (int)((a.width - 14f + bosluk) / (kartG + bosluk)));
            int satirSayisi = (ySahneler.Count + sutun - 1) / sutun;
            var gorunum = new Rect(a.x, a.y + 36, a.width, a.height - 36 - 112);
            ListeAl(ref yPanoKaydirma, gorunum, Mathf.Max(gorunum.height, satirSayisi * (kartY + bosluk)));
            for (int i = 0; i < ySahneler.Count; i++)
            {
                var s = ySahneler[i];
                int sat = i / sutun, sut = i % sutun;
                var r = new Rect(sut * (kartG + bosluk), sat * (kartY + bosluk), kartG, kartY);
                bool secili = i == ySecSahne;
                Kutu(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), secili ? Vurgu : new Color(0, 0, 0, 0));
                Kutu(r, Kart);
                var kt = new Rect(r.x + 8, r.y + 8, 108, 192);
                Kutu(kt, Color.black);
                var tex = KucukAl(s.hash);
                if (tex != null) GUI.DrawTexture(kt, tex, ScaleMode.StretchToFill, false);
                else Yazdir(kt, "Küçük resim\nyok / eski", 12, Sönük, TextAnchor.MiddleCenter, false, true);
                Yazdir(new Rect(r.x + 122, r.y + 8, r.width - 128, 22), (i + 1) + ". " + s.ad, 14, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(r.x + 122, r.y + 32, r.width - 128, 18), s.konusma + " konuşma", 12, Sönük);
                Yazdir(new Rect(r.x + 122, r.y + 50, r.width - 128, 18), "~" + Mathf.RoundToInt(s.sure) + " sn", 12, Sönük);
                Yazdir(new Rect(r.x + 122, r.y + 70, r.width - 128, 56), s.kars.Count == 0 ? "-" : string.Join(", ", s.kars.ToArray()), 12, Yazi, TextAnchor.UpperLeft, false, true);
                Yazdir(new Rect(r.x + 122, r.y + 128, r.width - 128, 72), s.ilk.Length > 0 ? "“" + s.ilk + "”" : "", 11, Sönük, TextAnchor.UpperLeft, false, true);
                Yazdir(new Rect(r.x + 8, r.y + 204, r.width - 16, 20), "Satır " + (s.bas + 1) + "–" + s.son, 11, Sönük);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) ySecSahne = i;
            }
            ListeVer(ref yPanoKaydirma, gorunum);

            // ---- seçili sahne işlemleri ----
            var pr = new Rect(a.x, a.yMax - 106, a.width, 106);
            Kutu(pr, Kart);
            if (ySahneler.Count == 0) { Yazdir(pr, "Henüz sahne yok. Yaz sekmesinde 'mekan:' satırı ekle.", 14, Sönük, TextAnchor.MiddleCenter); return; }
            var sc = ySahneler[Mathf.Clamp(ySecSahne, 0, ySahneler.Count - 1)];
            Yazdir(new Rect(pr.x + 12, pr.y + 6, 220, 30), "Seçili: " + (ySecSahne + 1) + ". " + sc.ad, 15, Yazi, TextAnchor.MiddleLeft, true);
            float x = pr.x + 240, y1 = pr.y + 8;
            if (Dugme(new Rect(x, y1, 150, 30), "Küçük resmi yenile", Vurgu, 12)) { KucukUretBaslat(false, ySecSahne); }
            x += 156;
            if (Dugme(new Rect(x, y1, 130, 30), "Bu sahneyi oynat", Vurgu, 12)) { yPano = false; Oynat(sc.bas + 1); }
            x += 136;
            if (Dugme(new Rect(x, y1, 88, 30), "Yukarı", new Color32(60, 66, 88, 255), 12, ySecSahne > 0 && !sc.giris)) SahneTasi(ySecSahne, -1);
            x += 94;
            if (Dugme(new Rect(x, y1, 88, 30), "Aşağı", new Color32(60, 66, 88, 255), 12, ySecSahne < ySahneler.Count - 1)) SahneTasi(ySecSahne, 1);

            // karakter değiştir
            float y2 = pr.y + 56;
            Yazdir(new Rect(pr.x + 12, y2, 80, 30), "Karakter", 13, Sönük);
            var eskiler = sc.kars.Count > 0 ? sc.kars : new List<string> { "-" };
            string eski = Sec(new Rect(pr.x + 84, y2, 130, 30), "pano_eski", "", eskiler, eskiler[0]);
            if (!eskiler.Contains(eski)) { yDeger["pano_eski"] = eskiler[0]; eski = eskiler[0]; }
            Yazdir(new Rect(pr.x + 218, y2, 22, 30), "→", 14, Sönük, TextAnchor.MiddleCenter);
            var kars = HikayeDenetci.KarakterAdlari(kutuphane);
            string yeni = Sec(new Rect(pr.x + 244, y2, 130, 30), "pano_yeni", "", kars, kars.Count > 0 ? kars[0] : "-");
            if (Dugme(new Rect(pr.x + 380, y2, 90, 30), "Değiştir", Vurgu, 12, eski != "-" && yeni != "-" && Util.Norm(eski) != Util.Norm(yeni))) KarakterDegistir(ySecSahne, eski, yeni);
            // kamera
            Yazdir(new Rect(pr.x + 492, y2, 60, 30), "Kamera", 13, Sönük);
            string kam = Sec(new Rect(pr.x + 552, y2, 130, 30), "pano_kamera", "", YonetmenTablo.Kameralar, "otomatik");
            if (Dugme(new Rect(pr.x + 688, y2, 90, 30), "Uygula", Vurgu, 12)) KameraUygula(ySecSahne, kam);
            if (!string.IsNullOrEmpty(mesaj)) Yazdir(new Rect(pr.x + 12, pr.y + 86, pr.width - 24, 18), mesaj, 12, mesajHata ? new Color(1f, 0.59f, 0.59f) : Basari);
        }

        string[] SatirlariAl() { return yMetin.Split('\n'); }

        void SahneTasi(int idx, int yon)
        {
            if (ySahneGerek) SahneleriKur();
            int j = idx + yon;
            if (idx < 0 || j < 0 || idx >= ySahneler.Count || j >= ySahneler.Count) return;
            var satirlar = SatirlariAl();
            var bloklar = new List<string[]>();
            foreach (var s in ySahneler)
            {
                var b = new string[s.son - s.bas];
                Array.Copy(satirlar, s.bas, b, 0, b.Length);
                bloklar.Add(b);
            }
            var t = bloklar[idx]; bloklar[idx] = bloklar[j]; bloklar[j] = t;
            var yeni = new List<string>();
            for (int i = 0; i < yBaslikSon; i++) yeni.Add(satirlar[i]);
            foreach (var b in bloklar) yeni.AddRange(b);
            for (int i = yAltBilgiBas; i < satirlar.Length; i++) yeni.Add(satirlar[i]);
            Degistir(string.Join("\n", yeni.ToArray()));
            ySecSahne = j; ySahneGerek = true;
        }

        void KarakterDegistir(int idx, string eski, string yeni)
        {
            if (ySahneGerek) SahneleriKur();
            if (idx < 0 || idx >= ySahneler.Count) return;
            var s = ySahneler[idx];
            var satirlar = SatirlariAl();
            int say = 0;
            for (int i = s.bas; i < s.son; i++)
            {
                string l = satirlar[i];
                string t = l.TrimStart();
                int ofs = l.Length - t.Length;
                if (t.StartsWith("["))
                {
                    int c = t.IndexOf(']');
                    if (c > 1)
                    {
                        var parts = t.Substring(1, c - 1).Split('|');
                        if (Util.Norm(parts[0].Trim()) == Util.Norm(eski))
                        {
                            parts[0] = yeni;
                            satirlar[i] = l.Substring(0, ofs) + "[" + string.Join("|", parts) + "]" + t.Substring(c + 1);
                            say++;
                        }
                    }
                }
                else
                {
                    string k = SatirAnahtari(l);
                    if (k == "poz" || k == "cikar")
                    {
                        int c = t.IndexOf(':');
                        string deger = t.Substring(c + 1).TrimStart();
                        var sp = deger.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (sp.Length > 0 && Util.Norm(sp[0]) == Util.Norm(eski))
                        {
                            satirlar[i] = l.Substring(0, ofs) + t.Substring(0, c + 1) + " " + yeni + (sp.Length > 1 ? " " + sp[1] : "");
                            say++;
                        }
                    }
                }
            }
            if (say > 0) { Degistir(string.Join("\n", satirlar)); mesaj = (idx + 1) + ". sahnede " + say + " satır güncellendi: " + eski + " → " + yeni; mesajHata = false; }
            ySahneGerek = true;
        }

        void KameraUygula(int idx, string kamera)
        {
            if (ySahneGerek) SahneleriKur();
            if (idx < 0 || idx >= ySahneler.Count) return;
            var s = ySahneler[idx];
            var satirlar = new List<string>(SatirlariAl());
            int bulunan = -1;
            for (int i = s.bas; i < s.son; i++) if (SatirAnahtari(satirlar[i]) == "kamera") { bulunan = i; break; }
            string yeniSatir = "kamera: " + kamera;
            if (bulunan >= 0) satirlar[bulunan] = yeniSatir;
            else
            {
                int at = SatirAnahtari(satirlar[s.bas]) == "mekan" ? s.bas + 1 : s.bas;
                satirlar.Insert(at, yeniSatir);
            }
            Degistir(string.Join("\n", satirlar.ToArray()));
            ySahneGerek = true;
        }

        // ---- küçük resim üretimi (yalnızca eksik/eski olanlar; "tümü" seçilirse hepsi) ----
        void KucukUretBaslat(bool hepsi, int tek = -1)
        {
            if (yUretiyor) return;
            if (ySahneGerek) SahneleriKur();
            KaydetYazma(true);
            var liste = new List<int>();
            for (int i = 0; i < ySahneler.Count; i++)
            {
                if (tek >= 0) { if (i == tek) liste.Add(i); }
                else if (hepsi || KucukAl(ySahneler[i].hash) == null) liste.Add(i);
            }
            if (liste.Count == 0) return;
            StartCoroutine(KucukUret(liste));
        }

        IEnumerator KucukUret(List<int> liste)
        {
            yUretiyor = true;
            var o = CanliOnizleme.Al();
            string metin = yMetin;
            string ad = yH.ad;
            var sahneler = new List<Sahne>(ySahneler);
            int tamam = 0, hata = 0;
            if (o.Calisiyor) yield return o.DurdurRutini();
            for (int n = 0; n < liste.Count; n++)
            {
                var s = sahneler[liste[n]];
                yUretimMesaj = "Küçük resimler üretiliyor… " + (n + 1) + " / " + liste.Count + "\n(" + s.ad + ")";
                o.Baslat(ad, metin, s.bas + 1, true, 360, 640);
                float t0 = Time.realtimeSinceStartup;
                while (!(o.Aktif && o.Gecen >= 3.2f) && Time.realtimeSinceStartup - t0 < 20f && o.Hata.Length == 0) yield return null;
                bool yakalandi = false;
                if (o.Aktif && o.RT != null)
                {
                    yield return new WaitForEndOfFrame();
                    var tex = o.Yakala(180, 320);
                    if (tex != null)
                    {
                        CanliOnizleme.PngKaydet(tex, KucukYolu(s.hash));
                        Texture2D eski;
                        if (yKucukler.TryGetValue(s.hash, out eski) && eski != null) Destroy(eski);
                        yKucukler[s.hash] = tex;
                        yakalandi = true;
                    }
                }
                if (yakalandi) tamam++; else { hata++; Gunluk("Küçük resim HATA: sahne " + (liste[n] + 1) + " " + o.Hata); }
                yield return o.DurdurRutini();
            }
            yUretiyor = false;
            mesaj = tamam + " küçük resim üretildi" + (hata > 0 ? ", " + hata + " başarısız" : "") + ".";
            mesajHata = hata > 0;
            Gunluk("Küçük resim: " + mesaj);
        }
    }
}
