using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 14: yazma ekranındaki "Seri" sekmesi (bölümler arası devamlılık).
    // Bütün değişiklikler hikâye metnine komut satırı olarak yazılır; Geri al aynen çalışır.
    public partial class AnaEkran
    {
        bool ySeri;
        readonly List<SeriBolum> seriDigerleri = new List<SeriBolum>();   // açık hikâye dışındakiler
        SeriRaporu seriRapor;
        string seriRaporMetin;
        string seriAdGirdi = "", seriNoGirdi = "1", seriYaziGirdi = "", seriMesaj = "";
        float seriKaydirma;
        string seriAdetGirdi = "5";

        void SeriAc()
        {
            ySeri = true; yPano = false; ySes = false; yYardim = false;
            seriMesaj = ""; seriKaydirma = 0f; seriRaporMetin = null;
            seriDigerleri.Clear();
            try
            {
                foreach (var f in Directory.GetFiles(HikayeKlasoru, "*.txt"))
                {
                    string ad = Path.GetFileNameWithoutExtension(f);
                    if (yH != null && string.Equals(ad, yH.ad, StringComparison.OrdinalIgnoreCase)) continue;
                    try { seriDigerleri.Add(SeriMotoru.Bolum(ad, File.ReadAllText(f))); }
                    catch (Exception e) { Gunluk("Seri: " + ad + " okunamadı: " + e.Message); }
                }
            }
            catch (Exception e) { seriMesaj = "Hikâye klasörü okunamadı: " + e.Message; }
        }

        // Var olan dosyanın üzerine yazmadan yeni hikâye oluşturur; adı döner (hata: null)
        string SeriDosyaYaz(string temelAd, string metin)
        {
            try
            {
                Directory.CreateDirectory(HikayeKlasoru);
                var sb = new System.Text.StringBuilder();
                foreach (var c in temelAd) sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
                string temel = sb.Length > 0 ? sb.ToString() : "hikaye";
                string ad = temel; int n = 2;
                while (File.Exists(Path.Combine(HikayeKlasoru, ad + ".txt"))) ad = temel + "_" + n++;
                File.WriteAllText(Path.Combine(HikayeKlasoru, ad + ".txt"), metin.Replace("\n", "\r\n"), new System.Text.UTF8Encoding(false));
                seriDigerleri.Add(SeriMotoru.Bolum(ad, metin));
                seriRaporMetin = null;
                Gunluk("Seri: oluşturuldu " + ad);
                return ad;
            }
            catch (Exception e) { seriMesaj = "Oluşturulamadı: " + e.Message; Gunluk("Seri HATA: " + e); return null; }
        }

        string SeriPlani(int no)
        {
            string plan = "";
            foreach (var b in seriRapor.Bolumler)
                foreach (var n in b.P.SeriNotlari)
                {
                    int k;
                    if (n.Tur == "plan" && int.TryParse(n.Kim, out k) && k == no) plan = n.Deger;
                }
            return plan;
        }

        bool SeriBolumVar(int no) { return seriRapor.Bolumler.Exists(b => b.No == no); }

        void SeriYeniBolum(string seri, int tur)
        {
            int son = 0;
            foreach (var b in seriRapor.Bolumler) if (b.No > son) son = b.No;
            var onceki = seriRapor.Bolumler.Find(b => b.No == son);
            string ozet = onceki != null ? SeriMotoru.Ozet(onceki.P) : "";
            string ad = SeriDosyaYaz(seri + "_bolum" + (son + 1), SeriUretici.Sablon(tur, seri, son + 1, SeriPlani(son + 1), ozet));
            if (ad != null) seriMesaj = "Yeni bölüm oluşturuldu: " + ad + " (Hikâyelerim)";
        }

        // 1'den hedef bölüm sayısına kadar eksik bölümlerin taslaklarını üretir
        void SeriSezonOlustur(string seri, int adet)
        {
            int yeni = 0;
            for (int no = 1; no <= adet; no++)
            {
                if (SeriBolumVar(no)) continue;
                if (SeriDosyaYaz(seri + "_bolum" + no, SeriUretici.Sablon(no == 1 ? 0 : (no == adet ? 3 : 0), seri, no, SeriPlani(no), "")) == null) return;
                SeriInceleGerekirse();
                yeni++;
            }
            seriMesaj = yeni > 0 ? yeni + " bölüm taslağı oluşturuldu (Hikâyelerim)" : "Eksik bölüm yok; dosyalar değiştirilmedi.";
        }

        // Fragman: hazır gerçek sesler yeni hikâyenin ses klasörüne kopyalanır (Gemini hakkı harcanmaz)
        void SeriFragman()
        {
            List<StoryItem> secilen;
            string metin = SeriUretici.Fragman(yMetin, 4, out secilen);
            if (secilen.Count == 0) { seriMesaj = "Fragman için konuşma satırı yok."; return; }
            string ad = SeriDosyaYaz(yH.ad + "_fragman", metin);
            if (ad == null) return;
            int kopya = 0;
            try
            {
                foreach (var it in secilen)
                {
                    string k = kutuphane.ClipKey(it);
                    string kaynak = DisSes.Dosya(yH.ad, k);
                    if (!File.Exists(kaynak)) continue;
                    Directory.CreateDirectory(DisSes.Klasor(ad));
                    File.Copy(kaynak, DisSes.Dosya(ad, k), true);
                    kopya++;
                }
            }
            catch (Exception e) { Gunluk("Seri fragman ses kopyası HATA: " + e.Message); }
            seriMesaj = "Fragman oluşturuldu: " + ad + " (" + secilen.Count + " replik, " + kopya + " hazır ses kopyalandı)";
        }

        void SeriAlternatif(string harf)
        {
            string ad = SeriDosyaYaz(yH.ad + "_son" + harf, SeriUretici.AlternatifSon(yMetin, harf));
            if (ad != null) seriMesaj = "Alternatif son taslağı oluşturuldu: " + ad + " (Hikâyelerim)";
        }

        void SeriInceleGerekirse()
        {
            if (seriRapor != null && seriRaporMetin == yMetin) return;
            try
            {
                var bu = SeriMotoru.Bolum(yH.ad, yMetin);
                var tum = new List<SeriBolum>(seriDigerleri) { bu };
                seriRapor = SeriMotoru.Incele(tum, bu.P.Seri, bu.No);
                if (seriAdGirdi.Length == 0)
                {
                    // seri yoksa var olan bir seri adı önerilir
                    foreach (var b in seriDigerleri) if (b.P.Seri.Length > 0) { seriAdGirdi = b.P.Seri; seriNoGirdi = (b.No + 1).ToString(); break; }
                }
            }
            catch (Exception e) { seriRapor = new SeriRaporu(); seriMesaj = "İnceleme hatası: " + e.Message; Gunluk("Seri HATA: " + e); }
            seriRaporMetin = yMetin;
        }

        // Komut satırını "son:" satırından önce (yoksa en sona) ekler
        void SeriSatirEkle(string satir)
        {
            var l = new List<string>(yMetin.Split('\n'));
            int yer = l.Count;
            for (int i = 0; i < l.Count; i++) if (Util.Norm(l[i]).StartsWith("son:")) { yer = i; break; }
            while (yer > 0 && yer == l.Count && l[yer - 1].Trim().Length == 0) { l.RemoveAt(yer - 1); yer--; }
            l.Insert(yer, satir);
            Degistir(string.Join("\n", l.ToArray()));
            yDenetimGerek = true; yDegisim = -10f;
            seriMesaj = "Eklendi: " + satir + "  (Geri al ile geri alınır)";
        }

        void SeriBasaEkle(string satirlar)
        {
            Degistir(satirlar + "\n" + yMetin);
            yDenetimGerek = true; yDegisim = -10f;
            seriMesaj = "Seri bilgisi eklendi";
        }

        // Önceki bölüm özetini kancadan sonra ekran kartı olarak ekler
        void SeriOzetKarti(string ozet)
        {
            string kisa = ozet.Replace("\n", " ").Trim();
            if (kisa.Length > 80) kisa = kisa.Substring(0, 80).TrimEnd() + "…";
            string satir = "yazi: Önceki bölümde: " + kisa;
            var l = new List<string>(yMetin.Split('\n'));
            int yer = -1;
            for (int i = 0; i < l.Count; i++) if (Util.Norm(l[i]).StartsWith("kanca:")) { yer = i + 1; break; }
            if (yer < 0)
                for (int i = 0; i < l.Count; i++) if (l[i].TrimStart().StartsWith("[")) { yer = i; break; }
            if (yer < 0) yer = l.Count;
            l.Insert(yer, satir);
            Degistir(string.Join("\n", l.ToArray()));
            yDenetimGerek = true; yDegisim = -10f;
            seriMesaj = "Özet kartı " + (yer + 1) + ". satıra eklendi";
        }

        string SeriMetinKutusu(Rect r, string deger, int azami)
        {
            Kutu(r, KimlikAlan);
            var tf = new GUIStyle(YaziStil(14, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(10, 10, 0, 0), clipping = TextClipping.Clip };
            return GUI.TextField(r, deger ?? "", azami, tf);
        }

        void SeriCiz(Rect a)
        {
            if (seriRapor == null || Event.current.type == EventType.Layout) SeriInceleGerekirse();
            var rap = seriRapor;
            var bu = rap.Bolumler.Find(b => b.Ad == yH.ad);
            Kutu(a, Kart);
            float x = a.x + 12, y = a.y + 8, w = a.width - 24;

            if (bu == null)
            {
                Yazdir(new Rect(x, y, w, 24), "Bu hikâye henüz bir seriye bağlı değil. Seri adını ve bölüm numarasını yazıp ekle.", 13, Yazi);
                y += 30;
                seriAdGirdi = SeriMetinKutusu(new Rect(x, y, 300, 34), seriAdGirdi, 60);
                Yazdir(new Rect(x + 310, y, 60, 34), "Bölüm", 13, Sönük);
                seriNoGirdi = SeriMetinKutusu(new Rect(x + 370, y, 60, 34), seriNoGirdi, 3);
                int no;
                bool tamam = seriAdGirdi.Trim().Length > 0 && int.TryParse(seriNoGirdi, out no) && no > 0;
                if (Dugme(new Rect(x + 440, y, 160, 34), "Hikâyeye ekle", Vurgu, 13, tamam))
                    SeriBasaEkle("seri: " + seriAdGirdi.Trim() + "\nbolum: " + seriNoGirdi.Trim());
                Yazdir(new Rect(x, y + 40, w, 30), seriMesaj, 12, Basari);
                return;
            }

            var kars = HikayeDenetci.KarakterAdlari(kutuphane);
            var kayma0 = Vector2.zero;
            Yazdir(new Rect(x, y, w, 22), bu.P.Seri + " · " + bu.No + ". bölüm · seride " + rap.Bolumler.Count + " hikâye", 15, Yazi, TextAnchor.MiddleLeft, true);
            y += 28;

            // ---- yazı girişi + ekleme düğmeleri ----
            seriYaziGirdi = SeriMetinKutusu(new Rect(x, y, w, 32), seriYaziGirdi, 200);
            y += 38;
            string t = seriYaziGirdi.Trim();
            bool dolu = t.Length > 0;
            float bw = (w - 4 * 6f) / 5f;
            string ekle = null;
            if (Dugme(new Rect(x, y, bw, 30), "Bölüm özeti yap", Vurgu, 12, dolu)) ekle = "ozet: " + t;
            if (Dugme(new Rect(x + (bw + 6), y, bw, 30), "Olay ekle", Vurgu, 12, dolu)) ekle = "olay: " + t;
            if (Dugme(new Rect(x + 2 * (bw + 6), y, bw, 30), "Soru ekle", Vurgu, 12, dolu)) ekle = "soru: " + t;
            int planNo;
            bool planOk = dolu && int.TryParse(seriNoGirdi, out planNo) && planNo > 0;
            if (Dugme(new Rect(x + 3 * (bw + 6), y, bw - 54, 30), "Plan ekle: bölüm", Vurgu, 12, planOk)) ekle = "plan: " + seriNoGirdi.Trim() + " = " + t;
            seriNoGirdi = SeriMetinKutusu(new Rect(x + 3 * (bw + 6) + bw - 50, y, 50, 30), seriNoGirdi, 3);
            if (Dugme(new Rect(x + 4 * (bw + 6), y, bw, 30), "Özet kartı (videoda)", new Color32(60, 66, 88, 255), 12, rap.OncekiOzet.Length > 0))
                SeriOzetKarti(rap.OncekiOzet);
            y += 38;

            // ---- karakter satırı ----
            string ad1 = SecG(new Rect(x, y, 150, 30), kayma0, "seri_a", kars, kars.Count > 1 ? kars[1] : "Anlatici", 12);
            string ad2 = SecG(new Rect(x + 156, y, 150, 30), kayma0, "seri_b", kars, kars.Count > 2 ? kars[2] : "Anlatici", 12);
            string dur = SecG(new Rect(x + 312, y, 130, 30), kayma0, "seri_d", SeriNotu.Durumlar, "olu", 12);
            if (Dugme(new Rect(x + 450, y, 120, 30), "Durum (1.)", Vurgu, 12)) ekle = "durum: " + ad1 + " = " + dur;
            if (Dugme(new Rect(x + 576, y, 120, 30), "Geçmiş (1.)", Vurgu, 12, dolu)) ekle = "gecmis: " + ad1 + " = " + t;
            if (Dugme(new Rect(x + 702, y, 120, 30), "İlişki (1.-2.)", Vurgu, 12, dolu && ad1 != ad2)) ekle = "iliski: " + ad1 + " - " + ad2 + " = " + t;
            y += 36;

            // ---- üretim satırı (yeni hikâye dosyaları; var olanların üzerine yazılmaz) ----
            var gri = new Color32(60, 66, 88, 255);
            string sablon = SecG(new Rect(x, y, 210, 30), kayma0, "seri_s", SeriUretici.SablonAdlari, SeriUretici.SablonAdlari[0], 12);
            int uretim = 0;
            if (Dugme(new Rect(x + 216, y, 110, 30), "Yeni bölüm", gri, 12)) uretim = 1;
            Yazdir(new Rect(x + 336, y, 50, 30), "Sezon:", 12, Sönük);
            seriAdetGirdi = SeriMetinKutusu(new Rect(x + 386, y, 40, 30), seriAdetGirdi, 2);
            int adet;
            bool adetOk = int.TryParse(seriAdetGirdi, out adet) && adet >= 2 && adet <= 30;
            if (Dugme(new Rect(x + 432, y, 110, 30), "Sezon oluştur", gri, 12, adetOk)) uretim = 2;
            if (Dugme(new Rect(x + 552, y, 90, 30), "Fragman", gri, 12)) uretim = 3;
            if (Dugme(new Rect(x + 648, y, 84, 30), "Son A", gri, 12)) uretim = 4;
            if (Dugme(new Rect(x + 738, y, 84, 30), "Son B", gri, 12)) uretim = 5;
            y += 36;
            Yazdir(new Rect(x, y, w, 20), seriMesaj, 12, Basari);
            y += 24;

            // ---- rapor ----
            var satirlar = new List<SeriSatiri>();
            satirlar.Add(SeriSatiri.Baslik("Çelişkiler ve uyarılar (" + rap.Celiskiler.Count + ")"));
            if (rap.Celiskiler.Count == 0) satirlar.Add(new SeriSatiri { Metin = "Çelişki bulunmadı (yalnız yazılan komutlara göre denetlenir)." });
            foreach (var c in rap.Celiskiler)
                satirlar.Add(new SeriSatiri
                {
                    Metin = (c.Agir ? "ÇELİŞKİ · " : "Uyarı · ") + c.Bolum + ". bölüm (" + c.Dosya + ")" + (c.Satir > 0 ? " satır " + c.Satir : "") + ": " + c.Mesaj,
                    Agir = c.Agir, Git = c.Dosya == yH.ad ? c.Satir : 0
                });
            satirlar.Add(SeriSatiri.Baslik("Önceki bölümün özeti" + (rap.OncekiNo > 0 ? " (" + rap.OncekiNo + ". bölüm)" : "")));
            satirlar.Add(new SeriSatiri { Metin = rap.OncekiOzet.Length > 0 ? rap.OncekiOzet : "Önceki bölüm yok ya da özeti boş." });
            satirlar.Add(SeriSatiri.Baslik("Karakter durumu (önceki bölümlerin sonunda)"));
            foreach (var d in rap.Durumlar) satirlar.Add(new SeriSatiri { Metin = d.Ad + ": " + d.Durum + "  (" + d.Bolum + ". bölümden beri)" });
            SeriListe(satirlar, "İlişkiler", rap.Iliskiler);
            SeriListe(satirlar, "Geçmiş olaylar", rap.Olaylar);
            SeriListe(satirlar, "Karakter geçmişi", rap.Gecmisler);
            satirlar.Add(SeriSatiri.Baslik("Açık kalan sorular (" + rap.AcikSorular.Count + ")"));
            foreach (var s in rap.AcikSorular) satirlar.Add(new SeriSatiri { Metin = s, Soru = s });
            SeriListe(satirlar, "Sezon planı", rap.Plan);
            satirlar.Add(SeriSatiri.Baslik("Bölümler"));
            foreach (var b in rap.Bolumler) satirlar.Add(new SeriSatiri { Metin = b.No + ". bölüm: " + b.Ad + (b.P.Baslik.Length > 0 ? " — " + b.P.Baslik : "") });

            var g = new Rect(x, y, w, a.yMax - y - 8);
            float gw = g.width - 14f;
            var stil = YaziStil(13, TextAnchor.MiddleLeft, Yazi, false, true);
            float toplam = 0;
            foreach (var s in satirlar)
            {
                float sag = s.Git > 0 || s.Soru != null ? 120f : 0f;
                s.H = s.Bas ? 28f : Mathf.Max(30f, stil.CalcHeight(new GUIContent(s.Metin), gw - 20 - sag) + 10f);
                toplam += s.H + 4;
            }
            ListeAl(ref seriKaydirma, g, Mathf.Max(g.height, toplam));
            float sy = 0; int git = 0; string cozulen = null;
            foreach (var s in satirlar)
            {
                var r = new Rect(0, sy, gw, s.H);
                sy += s.H + 4;
                if (s.Bas) { Yazdir(r, s.Metin, 14, Vurgu, TextAnchor.LowerLeft, true); continue; }
                Kutu(r, new Color32(26, 31, 46, 255));
                float sag = s.Git > 0 || s.Soru != null ? 120f : 0f;
                Yazdir(new Rect(10, r.y, r.width - 20 - sag, r.height), s.Metin, 13, s.Agir ? new Color(1f, 0.59f, 0.59f) : Yazi, TextAnchor.MiddleLeft, false, true);
                if (s.Git > 0 && Dugme(new Rect(r.xMax - 68, r.y + 2, 62, 26), "Git", new Color32(60, 66, 88, 255), 12)) git = s.Git;
                if (s.Soru != null && Dugme(new Rect(r.xMax - 108, r.y + 2, 102, 26), "Çözüldü", new Color32(60, 66, 88, 255), 12)) cozulen = s.Soru;
            }
            ListeVer(ref seriKaydirma, g);

            if (uretim == 1) SeriYeniBolum(bu.P.Seri, Mathf.Max(0, Array.IndexOf(SeriUretici.SablonAdlari, sablon)));
            if (uretim == 2) SeriSezonOlustur(bu.P.Seri, adet);
            if (uretim == 3) SeriFragman();
            if (uretim == 4) SeriAlternatif("A");
            if (uretim == 5) SeriAlternatif("B");
            if (ekle != null) { SeriSatirEkle(ekle); seriYaziGirdi = ""; GUI.FocusControl(null); }
            if (cozulen != null) SeriSatirEkle("cozuldu: " + cozulen);
            if (git > 0) AtlaSatir(git);
        }

        class SeriSatiri
        {
            public string Metin = "";
            public bool Bas, Agir;
            public int Git;
            public string Soru;
            public float H;
            public static SeriSatiri Baslik(string m) { return new SeriSatiri { Metin = m, Bas = true }; }
        }

        void SeriListe(List<SeriSatiri> l, string baslik, List<string> icerik)
        {
            if (icerik.Count == 0) return;
            l.Add(SeriSatiri.Baslik(baslik));
            foreach (var s in icerik) l.Add(new SeriSatiri { Metin = s });
        }
    }
}
