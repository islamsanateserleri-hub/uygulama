using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 5 (P1): arayüzden "Videoya al".
    // Ön kontrol: doğrulayıcı hatası ya da GERÇEK sesi olmayan (eksik / yalnız deneme sesi) konuşma satırı varsa kayıt başlamaz.
    // Kayıt ayrı bir süreçte yapılır (Aşama 1'de test edilen yol):  HikayeStudyo.exe -kayit <klasör\video.mp4> -hikayedosya <txt> | -hikaye <ad>  -yayin <klasör>
    // Yayın klasörü: Documents\HikayeStudyosu\Videolar\<ad>_<yyyyMMdd_HHmm>\ ; ilerleme kayit_ilerleme.txt'den 0,5 sn'de bir okunur.
    // İlerleme/sonuç kartı kenar çubuğunun boş alanında çizilir (VideoKartCiz), böylece hangi ekranda olunursa olunsun görünür.
    public partial class AnaEkran
    {
        Process vSurec;
        bool vKart;                 // kart görünür mü
        bool vBitti, vBasarili;
        string vAd = "", vKlasor = "", vVideo = "", vKisa = "", vDurum = "";
        float vSure, vToplam, vSonOkuma = -10f, vBaslangic;
        long vBoyut;
        // Aşama 5 (P5 entegrasyonu): kayıttan sonra otomatik kalite kontrolü (arka planda; arayüz donmaz)
        Task<KaliteKontrol.Rapor> vKk;
        KaliteKontrol.Rapor vKkRapor;
        string vKkOzet = "";
        float vBeklenen, vBeklenenYeni;
        const float KancaVarsayilanSn = 3f;

        bool VideoKaydediliyor
        {
            get
            {
                if (vSurec == null) return false;
                try { return !vSurec.HasExited; } catch { return false; }
            }
        }

        // Hikâyelerim satırı ve yazma ekranı üst çubuğu için tek çağrılık düğme
        void VideoDugmesi(Rect r, HikayeBilgi h, int boyut = 13)
        {
            bool mesgul = VideoKaydediliyor;
            string yazi = mesgul && h != null && h.ad == vAd ? "Kaydediliyor…" : "Videoya al";
            if (Dugme(r, yazi, new Color32(62, 130, 96, 255), boyut, !mesgul && h != null)) VideoyaAl(h);
        }

        void VideoyaAl(HikayeBilgi h)
        {
            if (h == null) return;
            if (VideoKaydediliyor) { VideoMesaj(true, "Zaten bir video kaydediliyor.", "Önce süren kaydın bitmesini bekle."); return; }
            if (yazmaAcik && yH != null && yH.ad == h.ad) KaydetYazma(true);

            // 1) metin (kullanıcı hikâyesi diskten: yazma ekranındaki son hâl)
            string metin = h.metin;
            if (!h.yerlesik && !string.IsNullOrEmpty(h.yol))
            {
                try { if (File.Exists(h.yol)) metin = File.ReadAllText(h.yol, Encoding.UTF8); }
                catch (Exception e) { VideoMesaj(true, "Hikâye dosyası okunamadı: " + e.Message, "Hikâye okunamadı."); return; }
            }
            if (string.IsNullOrEmpty(metin)) { VideoMesaj(true, "Hikâye boş: videoya alınacak bir şey yok.", "Hikâye boş."); return; }

            // 2) doğrulayıcı
            ParseResult pr;
            try { pr = StoryParser.Parse(metin); }
            catch (Exception e) { VideoMesaj(true, "Hikâye ayrıştırılamadı: " + e.Message, "Hikâye ayrıştırılamadı."); return; }
            var adlar = new List<string>();
            var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
            if (ol != null) foreach (var pf in ol.prefablar) if (pf != null) adlar.Add(pf.name);
            List<DenetimSorunu> sorunlar;
            try { sorunlar = HikayeDenetci.Denetle(metin, kutuphane, adlar); }
            catch (Exception e) { sorunlar = new List<DenetimSorunu>(); Gunluk("VideoyaAl denetim HATA: " + e.Message); }
            int hataSay = HikayeDenetci.HataSayisi(sorunlar);
            if (hataSay > 0)
            {
                var hs = new List<int>();
                foreach (var s in sorunlar) if (s.Hata && s.Satir > 0) hs.Add(s.Satir);
                VideoMesaj(true, "Hikâyede " + hataSay + " hata var" + (hs.Count > 0 ? " (satır " + SatirListesi(hs) + ")" : "") +
                    ". Önce yazma ekranında düzelt, sonra videoya al.", hataSay + " hata var" + (hs.Count > 0 ? ": satır " + SatirListesi(hs) : "") + ".");
                return;
            }

            // 3) ses ön kontrolü: deneme sesiyle video ALINMAZ
            if (kutuphane == null) { VideoMesaj(true, "Karakter kütüphanesi (StoryLibrary) yüklenemedi; video alınamaz.", "Kütüphane yok."); return; }
            var deneme = new List<int>();
            var eksik = DisSes.EksikGercekSesler(h.ad, pr.Items, kutuphane, deneme);
            if (eksik.Count > 0)
            {
                string uzun = eksik.Count + " konuşma satırının gerçek sesi yok (satır " + SatirListesi(eksik) + ").";
                if (deneme.Count > 0) uzun += " Bunların " + deneme.Count + " tanesinde yalnız deneme sesi var (satır " + SatirListesi(deneme) + "); deneme sesiyle video alınmaz.";
                uzun += " Önce yazma ekranındaki Ses sekmesinden üret.";
                VideoMesaj(true, uzun, eksik.Count + " satırın sesi yok: " + SatirListesi(eksik) + ". Ses sekmesinden üret.");
                return;
            }

            try { vBeklenenYeni = HikayeDenetci.ToplamSure(pr); } catch { vBeklenenYeni = 0f; }

            // 4) Editör: ayrı süreç başlatılmaz
            if (Application.isEditor)
            {
                VideoMesaj(false, "Ön kontrol geçti (hata yok, bütün sesler hazır). Videoya alma ayrı bir uygulama süreci başlatır; yalnız derlenmiş uygulamada (EXE) çalışır.",
                    "Ön kontrol geçti. Kayıt yalnız EXE'de.");
                return;
            }

            // 5) yayın klasörü
            string temiz = Sadelestir(h.ad);
            if (string.IsNullOrEmpty(temiz)) temiz = "hikaye";
            string klasor;
            try
            {
                Directory.CreateDirectory(VideoKlasoru);
                string taban = Path.Combine(VideoKlasoru, temiz + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm"));
                klasor = taban;
                for (int n = 2; Directory.Exists(klasor); n++) klasor = taban + "_" + n;
                Directory.CreateDirectory(klasor);
            }
            catch (Exception e) { VideoMesaj(true, "Video klasörü oluşturulamadı: " + e.Message, "Klasör oluşturulamadı."); return; }
            string video = Path.Combine(klasor, "video.mp4");

            // 6) kayıt süreci
            string exe = null;
            try { exe = Process.GetCurrentProcess().MainModule.FileName; } catch { exe = null; }
            if (string.IsNullOrEmpty(exe) || !File.Exists(exe)) exe = Path.Combine(Path.GetDirectoryName(Application.dataPath), "HikayeStudyo.exe");
            if (!File.Exists(exe)) { VideoMesaj(true, "Uygulama dosyası bulunamadı: " + exe, "EXE bulunamadı."); return; }
            string kaynak = h.yerlesik || string.IsNullOrEmpty(h.yol) ? "-hikaye \"" + h.ad + "\"" : "-hikayedosya \"" + h.yol + "\"";
            string arg = "-kayit \"" + video + "\" " + kaynak + " -yayin \"" + klasor + "\"";
            try
            {
                var psi = new ProcessStartInfo(exe, arg) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(exe) };
                vSurec = Process.Start(psi);
            }
            catch (Exception e) { vSurec = null; VideoMesaj(true, "Kayıt başlatılamadı: " + e.Message, "Kayıt başlatılamadı."); return; }
            Gunluk("VideoyaAl: " + exe + " " + arg);

            vAd = h.ad; vKlasor = klasor; vVideo = video; vBitti = false; vBasarili = false; vBoyut = 0;
            vSure = 0f; vToplam = 0f; vSonOkuma = -10f; vBaslangic = Time.realtimeSinceStartup;
            vBeklenen = vBeklenenYeni; vKk = null; vKkRapor = null; vKkOzet = "";
            vDurum = "Kayıt penceresi açılıyor…"; vKisa = ""; vKart = true;
            mesaj = "Video kaydı başladı: " + h.ad + ". Ayrı bir pencere açılır; kayıt bitince kendiliğinden kapanır. Klasör: " + klasor;
            mesajHata = false;
        }

        void VideoMesaj(bool hata, string uzun, string kisa)
        {
            mesaj = uzun; mesajHata = hata;
            if (!VideoKaydediliyor)
            {
                vKart = true; vBitti = true; vBasarili = !hata; vVideo = ""; vKlasor = ""; vAd = "";
                vDurum = hata ? "Video alınamadı" : "Ön kontrol"; vKisa = kisa;
            }
            Gunluk("VideoyaAl: " + uzun);
        }

        static string SatirListesi(List<int> l)
        {
            var sb = new StringBuilder();
            int n = Mathf.Min(l.Count, 8);
            for (int i = 0; i < n; i++) { if (i > 0) sb.Append(", "); sb.Append(l[i]); }
            if (l.Count > n) sb.Append(" … +" + (l.Count - n));
            return sb.ToString();
        }

        // ---- ilerleme / sonuç ----
        void VideoIlerlemeOku()
        {
            float now = Time.realtimeSinceStartup;
            if (now - vSonOkuma < 0.5f) return;
            vSonOkuma = now;
            if (string.IsNullOrEmpty(vKlasor)) return;
            string durum = null, not = null;
            try
            {
                string f = Path.Combine(vKlasor, "kayit_ilerleme.txt");
                if (File.Exists(f))
                {
                    foreach (var satir in File.ReadAllLines(f, Encoding.UTF8))
                    {
                        int e = satir.IndexOf('='); if (e < 0) continue;
                        string k = satir.Substring(0, e), v = satir.Substring(e + 1);
                        float x;
                        if (k == "durum") durum = v;
                        else if (k == "sure" && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) vSure = x;
                        else if (k == "toplam" && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out x)) vToplam = x;
                        else if (k == "not") not = v;
                    }
                }
            }
            catch { }   // dosya yazılırken okunursa bir sonraki turda tekrar denenir
            if (durum == "kayit") vDurum = "Kaydediliyor";
            else if (durum == "kodlaniyor") vDurum = "Video birleştiriliyor";
            else if (durum == "hazirlaniyor") vDurum = "Hazırlanıyor";
            else if (durum == "yayin") vDurum = "Kapak hazırlanıyor";

            if (!vBitti && !VideoKaydediliyor) VideoBitir(not);
        }

        void VideoBitir(string not)
        {
            vBitti = true;
            string sonuc = "";
            try { string f = Path.Combine(vKlasor, "kayit_sonuc.txt"); if (File.Exists(f)) sonuc = File.ReadAllText(f, Encoding.UTF8); } catch { }
            bool tamam = sonuc.Length > 0 && sonuc.Split('\n')[0].Contains("TAMAM");
            try { vBoyut = File.Exists(vVideo) ? new FileInfo(vVideo).Length : 0; } catch { vBoyut = 0; }
            vBasarili = tamam && vBoyut > 0;
            int kod = 0;
            try { kod = vSurec.ExitCode; } catch { }
            if (vBasarili)
            {
                KaliteBaslat();
                vDurum = "Video hazır";
                vKisa = (vBoyut / 1048576f).ToString("0.0") + " MB · " + vSure.ToString("0") + " sn";
                mesaj = "Video hazır: " + vVideo + " (" + vKisa + ")"; mesajHata = false;
            }
            else
            {
                string neden = !string.IsNullOrEmpty(not) ? not : (sonuc.Length > 0 ? SatirAl(sonuc, 2) : "kayıt süreci sonuç yazmadan kapandı (çıkış kodu " + kod + ")");
                vDurum = "Kayıt başarısız";
                vKisa = neden.Length > 90 ? neden.Substring(0, 90) + "…" : neden;
                mesaj = "Video kaydı başarısız: " + neden + " · Klasör: " + vKlasor; mesajHata = true;
            }
            Gunluk("VideoyaAl bitti: " + (vBasarili ? "TAMAM " : "HATA ") + vVideo + " " + vBoyut + " bayt");
            try { vSurec.Dispose(); } catch { }
            vSurec = null;
            VideolariYukle();
            yayinlar = null;   // Projeler listesi yeniden taransın
        }

        void KaliteBaslat()
        {
            vKkRapor = null;
            try
            {
                float beklenen = vBeklenen > 0.5f ? vBeklenen : vSure;
                vKk = KaliteKontrol.DenetleAsync(vVideo, beklenen, KancaVarsayilanSn);
                vKkOzet = "Kalite denetleniyor…";
            }
            catch (Exception e) { vKk = null; vKkOzet = "Kalite: denetlenemedi"; Gunluk("Kalite başlatılamadı: " + e.Message); }
        }

        // OnGUI'den çağrılır: arka plandaki denetim bitince raporu yazar
        void KaliteDenetle()
        {
            if (vKk == null || !vKk.IsCompleted) return;
            var t = vKk; vKk = null;
            try
            {
                vKkRapor = t.Result;
                if (!string.IsNullOrEmpty(vKlasor)) KaliteKontrol.Yaz(vKlasor, vKkRapor);
                vKkOzet = KaliteKontrol.OzetSatiri(vKkRapor);
                Gunluk("Kalite: " + vKkOzet + " " + vVideo);
            }
            catch (Exception e)
            {
                vKkRapor = null; vKkOzet = "Kalite: denetlenemedi";
                Gunluk("Kalite HATA: " + (e.InnerException != null ? e.InnerException.Message : e.Message));
            }
            yayinlar = null;
        }

        static readonly Color UyariRenk = new Color(0.94f, 0.75f, 0.31f);

        Color KaliteRengi()
        {
            if (vKkRapor == null) return Sönük;
            switch (vKkRapor.Genel)
            {
                case KaliteKontrol.Sonuc.Gecti: return Basari;
                case KaliteKontrol.Sonuc.Uyari: return UyariRenk;
                case KaliteKontrol.Sonuc.Hata: return Tehlike;
                default: return Sönük;
            }
        }

        static string SatirAl(string s, int i)
        {
            var p = s.Replace("\r", "").Split('\n');
            return i < p.Length ? p[i] : p[p.Length - 1];
        }

        void VideoIptal()
        {
            try { if (VideoKaydediliyor) vSurec.Kill(); } catch (Exception e) { Gunluk("VideoIptal: " + e.Message); }
            vBitti = true; vBasarili = false; vDurum = "İptal edildi"; vKisa = "Yarım kalan dosya klasörde kalabilir.";
            try { if (vSurec != null) vSurec.Dispose(); } catch { }
            vSurec = null;
        }

        // Kenar çubuğunun boş alanında (menü ile Çıkış arası) küçük kart; modal değil, başka düğmeyle çakışmaz.
        void VideoKartCiz()
        {
            if (vSurec != null) VideoIlerlemeOku();
            KaliteDenetle();
            if (!vKart) return;
            float ust = 524f, alt = gh - 92f;
            float h = alt - ust;
            if (h < 56f) return;
            bool kaliteSatiri = vBitti && vBasarili && vKkOzet.Length > 0;
            var r = new Rect(24, ust, 196, Mathf.Min(h, kaliteSatiri ? 156f : 132f));
            Kutu(r, new Color32(30, 36, 52, 255));
            Color baslikRenk = !vBitti ? Vurgu : (vBasarili ? Basari : Tehlike);
            Kutu(new Rect(r.x, r.y + 8, 4, 22), baslikRenk);
            Yazdir(new Rect(r.x + 12, r.y + 4, r.width - 20, 28), vDurum, 14, Yazi, TextAnchor.MiddleLeft, true);
            float y = r.y + 32;
            if (!vBitti)
            {
                float oran = vToplam > 0.5f ? Mathf.Clamp01(vSure / vToplam) : 0f;
                string ust2 = vToplam > 0.5f ? vSure.ToString("0") + " / " + vToplam.ToString("0") + " sn" : (Time.realtimeSinceStartup - vBaslangic).ToString("0") + " sn";
                Yazdir(new Rect(r.x + 12, y, r.width - 20, 18), vAd + " · " + ust2, 11, Sönük);
                y += 20;
                Kutu(new Rect(r.x + 12, y, r.width - 24, 8), new Color32(52, 60, 84, 255));
                if (oran > 0.01f) Kutu(new Rect(r.x + 12, y, Mathf.Max(8, (r.width - 24) * oran), 8), Vurgu);
                y += 14;
                if (y + 26 <= r.yMax && Dugme(new Rect(r.x + 12, y, 80, 24), "İptal", new Color32(110, 60, 60, 255), 11)) VideoIptal();
                return;
            }
            float by = r.yMax - 30;
            if (by < y) by = y;
            float kaliteY = by;
            if (kaliteSatiri && by - 24 >= y + 16)
            {
                kaliteY = by - 24;
                bool raporVar = vKkRapor != null && !string.IsNullOrEmpty(vKlasor) && File.Exists(Path.Combine(vKlasor, "kalite_raporu.txt"));
                Yazdir(new Rect(r.x + 12, kaliteY, r.width - (raporVar ? 74 : 20), 20), vKkOzet, 11, KaliteRengi(), TextAnchor.MiddleLeft, true);
                if (raporVar && Dugme(new Rect(r.xMax - 60, kaliteY, 52, 20), "Rapor", new Color32(60, 66, 88, 255), 10))
                    Ac(Path.Combine(vKlasor, "kalite_raporu.txt"), "");
            }
            float metinH = Mathf.Max(18f, kaliteY - y - 2f);
            Yazdir(new Rect(r.x + 12, y, r.width - 20, metinH), vKisa, 11, Sönük, TextAnchor.UpperLeft, false, true);
            if (vBasarili && !string.IsNullOrEmpty(vVideo))
            {
                if (Dugme(new Rect(r.x + 8, by, 62, 24), "Aç", Vurgu, 11)) Ac(vVideo, "");
                if (Dugme(new Rect(r.x + 74, by, 62, 24), "Klasör", new Color32(60, 66, 88, 255), 11)) Ac("explorer.exe", "\"" + vKlasor + "\"");
            }
            else if (!string.IsNullOrEmpty(vKlasor) && Directory.Exists(vKlasor))
            {
                if (Dugme(new Rect(r.x + 8, by, 128, 24), "Klasörü aç", new Color32(60, 66, 88, 255), 11)) Ac("explorer.exe", "\"" + vKlasor + "\"");
            }
            if (Dugme(new Rect(r.x + 140, by, 48, 24), "Kapat", new Color32(60, 66, 88, 255), 11)) vKart = false;
        }

        // ---- Projeler ekranı: yayın klasörleri (P2) ----
        class YayinBilgi
        {
            public string klasor, ad, baslik, video, kapak, kalite;
            public long boyut; public DateTime zaman; public float sure;
            public Color kaliteRenk;
        }
        [Serializable] class YayinJson { public string baslik = ""; public float sureSn; }

        List<YayinBilgi> yayinlar;
        float yayinYukleme = -100f;
        readonly Dictionary<string, Texture2D> kapakOnbellek = new Dictionary<string, Texture2D>();
        int kapakYuklenenKare = -1;

        // Videolar\<klasör>\video.mp4 içeren klasörler, yeniden eskiye
        void YayinlariYukle()
        {
            yayinlar = new List<YayinBilgi>();
            yayinYukleme = Time.realtimeSinceStartup;
            try
            {
                if (Directory.Exists(VideoKlasoru))
                    foreach (var d in Directory.GetDirectories(VideoKlasoru))
                    {
                        string v = Path.Combine(d, "video.mp4");
                        if (!File.Exists(v)) continue;
                        var fi = new FileInfo(v);
                        var y = new YayinBilgi { klasor = d, ad = Path.GetFileName(d), video = v, boyut = fi.Length, zaman = fi.LastWriteTime, baslik = "", kalite = "", kaliteRenk = Sönük };
                        string kp = Path.Combine(d, "kapak.png");
                        y.kapak = File.Exists(kp) ? kp : null;
                        try
                        {
                            string bt = Path.Combine(d, "baslik.txt");
                            if (File.Exists(bt)) y.baslik = File.ReadAllText(bt, Encoding.UTF8).Trim();
                            string js = Path.Combine(d, "yayin.json");
                            if (File.Exists(js))
                            {
                                var j = JsonUtility.FromJson<YayinJson>(File.ReadAllText(js, Encoding.UTF8));
                                if (j != null) { y.sure = j.sureSn; if (y.baslik.Length == 0 && !string.IsNullOrEmpty(j.baslik)) y.baslik = j.baslik; }
                            }
                            string kr = Path.Combine(d, "kalite_raporu.txt");
                            if (File.Exists(kr))
                                foreach (var satir in File.ReadAllLines(kr, Encoding.UTF8))
                                    if (satir.StartsWith("GENEL SONUÇ:", StringComparison.Ordinal))
                                    {
                                        y.kalite = "Kalite: " + satir.Substring("GENEL SONUÇ:".Length).Trim();
                                        y.kaliteRenk = satir.Contains("GEÇTİ") ? Basari : satir.Contains("UYARI") ? UyariRenk : satir.Contains("HATA") ? Tehlike : Sönük;
                                        break;
                                    }
                        }
                        catch (Exception e) { Debug.LogWarning("[AnaEkran] yayın klasörü okunamadı: " + d + " " + e.Message); }
                        if (y.baslik.Length == 0) y.baslik = y.ad;
                        yayinlar.Add(y);
                    }
            }
            catch (Exception e) { Debug.LogWarning("[AnaEkran] yayınlar: " + e.Message); }
            yayinlar.Sort((a, b) => b.zaman.CompareTo(a.zaman));
            // listede olmayan kapak dokularını bırak (bellek sızmasın)
            var sil = new List<string>();
            foreach (var k in kapakOnbellek.Keys)
            {
                bool var_ = false;
                foreach (var y in yayinlar) if (y.kapak == k) { var_ = true; break; }
                if (!var_) sil.Add(k);
            }
            foreach (var k in sil) { if (kapakOnbellek[k] != null) Destroy(kapakOnbellek[k]); kapakOnbellek.Remove(k); }
        }

        // Kapak küçük resmi (120x214); karede en fazla bir dosya yüklenir, başarısızsa null önbelleklenir
        Texture2D KapakKucuk(string yol)
        {
            if (string.IsNullOrEmpty(yol)) return null;
            Texture2D t;
            if (kapakOnbellek.TryGetValue(yol, out t)) return t;
            if (kapakYuklenenKare == Time.frameCount) return null;
            kapakYuklenenKare = Time.frameCount;
            t = null;
            Texture2D buyuk = null;
            RenderTexture rt = null;
            var onceki = RenderTexture.active;
            try
            {
                buyuk = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (buyuk.LoadImage(File.ReadAllBytes(yol)))
                {
                    rt = RenderTexture.GetTemporary(120, 214, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(buyuk, rt);
                    RenderTexture.active = rt;
                    t = new Texture2D(120, 214, TextureFormat.RGB24, false);
                    t.ReadPixels(new Rect(0, 0, 120, 214), 0, 0);
                    t.Apply();
                }
            }
            catch (Exception e) { Debug.LogWarning("[AnaEkran] kapak yüklenemedi: " + e.Message); if (t != null) { Destroy(t); t = null; } }
            finally
            {
                RenderTexture.active = onceki;
                if (rt != null) RenderTexture.ReleaseTemporary(rt);
                if (buyuk != null) Destroy(buyuk);
            }
            kapakOnbellek[yol] = t;
            return t;
        }
    }
}
