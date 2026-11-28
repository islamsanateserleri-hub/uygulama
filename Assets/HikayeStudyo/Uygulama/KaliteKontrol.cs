using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 5 / B4: video üretildikten sonra otomatik kalite kontrolü (ffmpeg ile).
    // Denetimler: biçim (1080x1920, 30 fps, h264, aac, süre), bozuk dosya, siyah ekran, donmuş kare,
    // ses seviyesi (LUFS, true peak), sessizlik / eksik ses. Altyazı taşması, kadraj, ağız senkronu
    // bu aşamada ölçülmez: "kontrol edilmedi" yazılır, uydurma sonuç verilmez.
    //
    // ÖNEMLİ: FfmpegPipeRecorder.FfmpegBul() Application.streamingAssetsPath kullanır; Unity'de yalnız ana
    // iş parçacığında çağrılabilir. Bu yüzden DenetleAsync yolu ana iş parçacığında çözer, ağır işi Task.Run'da yapar.
    // İş parçacığından doğrudan çağıracaksan ffmpeg yolunu parametre olarak ver: Denetle(mp4, sure, kanca, ffmpegYolu).
    public static class KaliteKontrol
    {
        public enum Sonuc { Gecti, Uyari, Hata, KontrolEdilmedi }

        public class Madde
        {
            public string Ad;
            public Sonuc Durum;
            public string Ayrinti = "";
            public Madde(string ad, Sonuc durum, string ayrinti) { Ad = ad; Durum = durum; Ayrinti = ayrinti ?? ""; }
        }

        public struct Aralik
        {
            public double Bas, Son;
            public Aralik(double bas, double son) { Bas = bas; Son = son; }
            public double Sure { get { return Math.Max(0, Son - Bas); } }
        }

        public class Rapor
        {
            public string Dosya = "";
            public DateTime Zaman = DateTime.Now;
            public Sonuc Genel = Sonuc.KontrolEdilmedi;
            public string GenelNot = "";
            public readonly List<Madde> Maddeler = new List<Madde>();

            // ölçülenler (okunamazsa 0 / NaN / boş)
            public long Boyut;
            public double Sure;                 // sn
            public int Genislik, Yukseklik;
            public double Fps;
            public string VideoKodek = "", SesKodek = "";
            public bool VideoVar, SesVar, BicimOkundu;
            public double Lufs = double.NaN, Tepe = double.NaN;   // LUFS, dBTP
            public readonly List<Aralik> Siyah = new List<Aralik>();
            public readonly List<Aralik> Donuk = new List<Aralik>();
            public readonly List<Aralik> Sessiz = new List<Aralik>();
            public int CozmeHatasi;
            public string IlkCozmeHatasi = "";
        }

        // ---- ayarlar / eşikler ----
        public const int HedefGenislik = 1080, HedefYukseklik = 1920;
        public const double HedefFps = 30.0;
        public const double HedefLufs = -14.0, LufsTolerans = 2.0, TepeSiniri = -1.0;
        public const double MuafKenar = 1.0;          // ilk/son 1 sn
        public const double Pay = 0.25;               // ölçüm payı (sn)
        public const int ZamanAsimiSn = 300;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        // ================= ana giriş =================

        // Ana iş parçacığından çağır (ffmpeg yolunu burada çözer). Uzun sürebilir: arayüzde DenetleAsync kullan.
        public static Rapor Denetle(string mp4, float beklenenSure, float kancaSure = 0f)
        {
            return Denetle(mp4, beklenenSure, kancaSure, FfmpegPipeRecorder.FfmpegBul());
        }

        // Arayüz donmasın: yolu ana iş parçacığında bulur, denetimi arka planda yapar.
        public static Task<Rapor> DenetleAsync(string mp4, float beklenenSure, float kancaSure = 0f)
        {
            string ff = FfmpegPipeRecorder.FfmpegBul();
            return Task.Run(() => Denetle(mp4, beklenenSure, kancaSure, ff));
        }

        // Herhangi bir iş parçacığından çağrılabilir (Unity API'si kullanmaz).
        public static Rapor Denetle(string mp4, float beklenenSure, float kancaSure, string ffmpegYolu)
        {
            var r = new Rapor { Dosya = mp4 ?? "" };
            try { DenetleIc(r, mp4, beklenenSure, kancaSure, ffmpegYolu); }
            catch (Exception e)
            {
                r.Maddeler.Add(new Madde("Denetim", Sonuc.Uyari, "Kalite kontrolü tamamlanamadı: " + e.Message));
                Debug.LogWarning("[Hikaye] Kalite kontrolü hatası: " + e);
            }
            KontrolEdilmeyenleriEkle(r);
            r.Genel = GenelSonuc(r);
            return r;
        }

        static void DenetleIc(Rapor r, string mp4, float beklenenSure, float kancaSure, string ff)
        {
            if (string.IsNullOrEmpty(mp4) || !File.Exists(mp4))
            {
                r.Maddeler.Add(new Madde("Dosya", Sonuc.Hata, "Video dosyası bulunamadı: " + (mp4 ?? "")));
                return;
            }
            r.Boyut = new FileInfo(mp4).Length;
            if (r.Boyut == 0)
            {
                r.Maddeler.Add(new Madde("Dosya", Sonuc.Hata, "Video dosyası boş (0 bayt)."));
                return;
            }
            if (string.IsNullOrEmpty(ff) || !File.Exists(ff))
            {
                r.GenelNot = "ffmpeg bulunamadı; otomatik kalite kontrolü yapılamadı.";
                r.Maddeler.Add(new Madde("Denetim", Sonuc.Uyari, r.GenelNot));
                return;
            }
            int zaman = Math.Max(ZamanAsimiSn, (int)(beklenenSure * 5f));
            string q = "\"" + mp4 + "\"";

            // 1) Biçim: ffprobe (varsa) ya da ffmpeg -i
            string fp = FfprobeBul(ff);
            if (fp != null)
            {
                var p = Calistir(fp, "-v error -show_entries stream=codec_type,codec_name,width,height,avg_frame_rate,r_frame_rate:format=duration -of compact " + q, 60);
                if (p.Cikis == 0) ProbeAyristir(p.Cikti, r);
            }
            if (!r.BicimOkundu)
            {
                var p = Calistir(ff, "-hide_banner -nostdin -i " + q, 60);
                InputAyristir(p.Hata, r);
            }

            // 2) Bütünlük: yalnız hatalar
            var h = Calistir(ff, "-hide_banner -nostdin -nostats -v error -i " + q + " -f null -", zaman);
            BozukAyristir(h.Hata, r);
            bool acilamadi = h.ZamanAsimi ? false : (!r.BicimOkundu || (h.Cikis != 0 && r.CozmeHatasi > 0 && YaziVar(h.Hata, "moov atom not found", "Error opening input")));
            r.Maddeler.Add(BozukMaddesi(r, h, acilamadi));

            if (!r.BicimOkundu)
            {
                r.Maddeler.Add(new Madde("Biçim", Sonuc.Hata, "Video bilgisi okunamadı (dosya bozuk ya da yarım kalmış olabilir)."));
                return;   // diğer ölçümler anlamsız
            }
            BicimMaddeleri(r, beklenenSure);

            // 3) Analiz: siyah, donuk, ses seviyesi, sessizlik (tek geçiş)
            string vf = r.VideoVar ? " -vf \"blackdetect=d=0.5:pix_th=0.10,freezedetect=n=-60dB:d=2\"" : "";
            string af = r.SesVar ? " -af \"ebur128=peak=true,silencedetect=n=-50dB:d=3\"" : "";
            var a = Calistir(ff, "-hide_banner -nostdin -nostats -i " + q + vf + af + " -f null -", zaman);
            if (a.ZamanAsimi)
            {
                r.Maddeler.Add(new Madde("Görüntü ve ses analizi", Sonuc.Uyari, "Zaman aşımı (" + zaman + " sn): siyah/donuk/ses ölçülemedi."));
                return;
            }
            AnalizAyristir(a.Hata, r);
            double sure = r.Sure > 0 ? r.Sure : beklenenSure;
            if (r.VideoVar)
            {
                r.Maddeler.Add(SiyahMaddesi(r, sure, kancaSure));
                r.Maddeler.Add(DonukMaddesi(r, sure, kancaSure));
            }
            SesMaddeleri(r, sure, kancaSure);
        }

        // ================= süreç =================

        public struct SurecSonucu { public int Cikis; public string Hata, Cikti; public bool ZamanAsimi; }

        static SurecSonucu Calistir(string exe, string args, int zamanAsimiSn)
        {
            var s = new SurecSonucu { Cikis = -1, Hata = "", Cikti = "" };
            var psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            };
            try
            {
                using (var pr = Process.Start(psi))
                {
                    var eT = pr.StandardError.ReadToEndAsync();
                    var oT = pr.StandardOutput.ReadToEndAsync();
                    if (!pr.WaitForExit(zamanAsimiSn * 1000))
                    {
                        s.ZamanAsimi = true;
                        try { pr.Kill(); } catch { }
                    }
                    else pr.WaitForExit();
                    try { eT.Wait(5000); } catch { }
                    try { oT.Wait(5000); } catch { }
                    s.Hata = eT.IsCompleted && !eT.IsFaulted ? eT.Result : "";
                    s.Cikti = oT.IsCompleted && !oT.IsFaulted ? oT.Result : "";
                    if (!s.ZamanAsimi) s.Cikis = pr.ExitCode;
                }
            }
            catch (Exception e) { s.Hata = "Süreç başlatılamadı: " + e.Message; }
            return s;
        }

        static string FfprobeBul(string ffmpeg)
        {
            try
            {
                string d = Path.GetDirectoryName(ffmpeg);
                foreach (var ad in new[] { "ffprobe.exe", "ffprobe" })
                {
                    string p = Path.Combine(d ?? "", ad);
                    if (File.Exists(p)) return p;
                }
            }
            catch { }
            return null;
        }

        static bool YaziVar(string metin, params string[] parcalar)
        {
            if (string.IsNullOrEmpty(metin)) return false;
            foreach (var p in parcalar) if (metin.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        // ================= ayrıştırıcılar (public: test edilebilir) =================

        static double Sayi(string s)
        {
            if (string.IsNullOrEmpty(s)) return double.NaN;
            s = s.Trim();
            if (s == "-inf" || s == "-Inf") return double.NegativeInfinity;
            if (s == "inf" || s == "+inf") return double.PositiveInfinity;
            double d;
            return double.TryParse(s, NumberStyles.Float, Inv, out d) ? d : double.NaN;
        }

        static double Kesir(string s)     // "30/1", "30000/1001", "30", "0/0"
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int i = s.IndexOf('/');
            if (i < 0) { double v = Sayi(s); return double.IsNaN(v) ? 0 : v; }
            double a = Sayi(s.Substring(0, i)), b = Sayi(s.Substring(i + 1));
            if (double.IsNaN(a) || double.IsNaN(b) || b == 0) return 0;
            return a / b;
        }

        // ffprobe -of compact: "stream|codec_name=h264|codec_type=video|width=1080|..." / "format|duration=6.0"
        public static void ProbeAyristir(string cikti, Rapor r)
        {
            if (string.IsNullOrEmpty(cikti)) return;
            foreach (var hamSatir in cikti.Split('\n'))
            {
                string satir = hamSatir.Trim();
                if (satir.Length == 0) continue;
                var alan = new Dictionary<string, string>();
                var parca = satir.Split('|');
                for (int i = 1; i < parca.Length; i++)
                {
                    int e = parca[i].IndexOf('=');
                    if (e > 0) alan[parca[i].Substring(0, e)] = parca[i].Substring(e + 1);
                }
                string v;
                if (parca[0] == "stream")
                {
                    string tur = alan.TryGetValue("codec_type", out v) ? v : "";
                    string kod = alan.TryGetValue("codec_name", out v) ? v : "";
                    if (tur == "video" && !r.VideoVar)
                    {
                        r.VideoVar = true; r.VideoKodek = kod;
                        int w, hh;
                        if (alan.TryGetValue("width", out v) && int.TryParse(v, NumberStyles.Integer, Inv, out w)) r.Genislik = w;
                        if (alan.TryGetValue("height", out v) && int.TryParse(v, NumberStyles.Integer, Inv, out hh)) r.Yukseklik = hh;
                        double f = alan.TryGetValue("avg_frame_rate", out v) ? Kesir(v) : 0;
                        if (f <= 0 && alan.TryGetValue("r_frame_rate", out v)) f = Kesir(v);
                        r.Fps = f;
                    }
                    else if (tur == "audio" && !r.SesVar) { r.SesVar = true; r.SesKodek = kod; }
                }
                else if (parca[0] == "format")
                {
                    if (alan.TryGetValue("duration", out v)) { double d = Sayi(v); if (!double.IsNaN(d)) r.Sure = d; }
                }
            }
            r.BicimOkundu = r.VideoVar || r.SesVar;
        }

        static readonly Regex rxSure = new Regex(@"Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)");
        static readonly Regex rxVideo = new Regex(@"Stream\s+#\d+:\d+[^:]*:\s*Video:\s*([A-Za-z0-9_]+)");
        static readonly Regex rxAudio = new Regex(@"Stream\s+#\d+:\d+[^:]*:\s*Audio:\s*([A-Za-z0-9_]+)");
        static readonly Regex rxBoyut = new Regex(@",\s*(\d{2,5})x(\d{2,5})[\s,\]]");
        static readonly Regex rxFps = new Regex(@"([\d.]+)\s*fps");

        // ffmpeg -i stderr (yalnız "Input #" bölümü; Output akışları sayılmaz)
        public static void InputAyristir(string stderr, Rapor r)
        {
            if (string.IsNullOrEmpty(stderr)) return;
            string g = stderr;
            int kes = IlkIndeks(g, "Stream mapping:", "Output #");
            if (kes >= 0) g = g.Substring(0, kes);
            var m = rxSure.Match(g);
            if (m.Success)
            {
                double s = Sayi(m.Groups[1].Value) * 3600 + Sayi(m.Groups[2].Value) * 60 + Sayi(m.Groups[3].Value);
                if (!double.IsNaN(s)) r.Sure = s;
            }
            foreach (var hamSatir in g.Split('\n'))
            {
                string satir = hamSatir.TrimEnd('\r');
                var mv = rxVideo.Match(satir);
                if (mv.Success && !r.VideoVar)
                {
                    r.VideoVar = true; r.VideoKodek = mv.Groups[1].Value;
                    var mb = rxBoyut.Match(satir + " ");
                    if (mb.Success) { r.Genislik = int.Parse(mb.Groups[1].Value, Inv); r.Yukseklik = int.Parse(mb.Groups[2].Value, Inv); }
                    var mf = rxFps.Match(satir);
                    if (mf.Success) { double f = Sayi(mf.Groups[1].Value); if (!double.IsNaN(f)) r.Fps = f; }
                    continue;
                }
                var ma = rxAudio.Match(satir);
                if (ma.Success && !r.SesVar) { r.SesVar = true; r.SesKodek = ma.Groups[1].Value; }
            }
            r.BicimOkundu = r.VideoVar || r.SesVar;
        }

        static int IlkIndeks(string s, params string[] parcalar)
        {
            int en = -1;
            foreach (var p in parcalar)
            {
                int i = s.IndexOf(p, StringComparison.Ordinal);
                if (i >= 0 && (en < 0 || i < en)) en = i;
            }
            return en;
        }

        // "-v error" çıktısı: boş olmayan her satır bir çözme hatası
        public static void BozukAyristir(string stderr, Rapor r)
        {
            r.CozmeHatasi = 0; r.IlkCozmeHatasi = "";
            if (string.IsNullOrEmpty(stderr)) return;
            foreach (var hamSatir in stderr.Split('\n'))
            {
                string satir = hamSatir.Trim();
                if (satir.Length == 0) continue;
                r.CozmeHatasi++;
                if (r.IlkCozmeHatasi.Length == 0) r.IlkCozmeHatasi = satir.Length > 160 ? satir.Substring(0, 160) + "…" : satir;
            }
        }

        static readonly Regex rxSiyah = new Regex(@"black_start:\s*(-?[\d.]+)\s+black_end:\s*(-?[\d.]+)");
        static readonly Regex rxDonukBas = new Regex(@"freeze_start:\s*(-?[\d.]+)");
        static readonly Regex rxDonukSon = new Regex(@"freeze_end:\s*(-?[\d.]+)");
        static readonly Regex rxSessizBas = new Regex(@"silence_start:\s*(-?[\d.]+)");
        static readonly Regex rxSessizSon = new Regex(@"silence_end:\s*(-?[\d.]+)");
        static readonly Regex rxLufs = new Regex(@"\bI:\s*(-?[\d.]+|-inf)\s*LUFS");
        static readonly Regex rxTepe = new Regex(@"Peak:\s*(-?[\d.]+|-inf)\s*dB(?:FS|TP)");

        // Tek geçişli analiz stderr'i: blackdetect, freezedetect, silencedetect, ebur128 özeti
        public static void AnalizAyristir(string stderr, Rapor r)
        {
            r.Siyah.Clear(); r.Donuk.Clear(); r.Sessiz.Clear();
            if (string.IsNullOrEmpty(stderr)) return;
            double sure = r.Sure;
            double donukAcik = double.NaN, sessizAcik = double.NaN;
            foreach (var hamSatir in stderr.Split('\n'))
            {
                string satir = hamSatir.TrimEnd('\r');
                Match m;
                if ((m = rxSiyah.Match(satir)).Success) { r.Siyah.Add(new Aralik(Sayi(m.Groups[1].Value), Sayi(m.Groups[2].Value))); continue; }
                if ((m = rxDonukBas.Match(satir)).Success) { donukAcik = Sayi(m.Groups[1].Value); continue; }
                if ((m = rxDonukSon.Match(satir)).Success)
                {
                    if (!double.IsNaN(donukAcik)) r.Donuk.Add(new Aralik(donukAcik, Sayi(m.Groups[1].Value)));
                    donukAcik = double.NaN; continue;
                }
                if ((m = rxSessizBas.Match(satir)).Success) { sessizAcik = Math.Max(0, Sayi(m.Groups[1].Value)); continue; }
                if ((m = rxSessizSon.Match(satir)).Success)
                {
                    if (!double.IsNaN(sessizAcik)) r.Sessiz.Add(new Aralik(sessizAcik, Sayi(m.Groups[1].Value)));
                    sessizAcik = double.NaN; continue;
                }
            }
            // Dosya sonunda kapanmamış aralıklar (bazı ffmpeg sürümleri son "end" satırını yazmaz)
            if (!double.IsNaN(donukAcik) && sure > donukAcik) r.Donuk.Add(new Aralik(donukAcik, sure));
            if (!double.IsNaN(sessizAcik) && sure > sessizAcik) r.Sessiz.Add(new Aralik(sessizAcik, sure));

            int ozet = stderr.LastIndexOf("Summary:", StringComparison.Ordinal);
            if (ozet >= 0)
            {
                string o = stderr.Substring(ozet);
                var ml = rxLufs.Match(o);
                if (ml.Success) r.Lufs = Sayi(ml.Groups[1].Value);
                var mt = rxTepe.Match(o);
                if (mt.Success) r.Tepe = Sayi(mt.Groups[1].Value);
            }
        }

        // ================= sınıflandırma =================

        static bool Muaf(Aralik a, double sure, double kanca)
        {
            double bas = Math.Max(MuafKenar, kanca);
            return a.Son <= bas + Pay || (sure > 0 && a.Bas >= sure - MuafKenar - Pay);
        }

        static double Toplam(List<Aralik> l) { double t = 0; foreach (var a in l) t += a.Sure; return t; }

        static double Ortusme(Aralik a, Aralik b) { return Math.Max(0, Math.Min(a.Son, b.Son) - Math.Max(a.Bas, b.Bas)); }

        static Sonuc Kotu(Sonuc a, Sonuc b) { return (int)b > (int)a && b != Sonuc.KontrolEdilmedi ? b : a; }

        static Madde BozukMaddesi(Rapor r, SurecSonucu h, bool acilamadi)
        {
            if (h.ZamanAsimi) return new Madde("Dosya bütünlüğü", Sonuc.Uyari, "Zaman aşımı: dosya baştan sona çözülemedi.");
            if (acilamadi || YaziVar(h.Hata, "moov atom not found"))
                return new Madde("Dosya bütünlüğü", Sonuc.Hata, "Dosya açılamıyor (kayıt yarım kalmış olabilir): " + r.IlkCozmeHatasi);
            if (h.Cikis != 0 || r.CozmeHatasi >= 3 || YaziVar(h.Hata, "Invalid data found"))
                return new Madde("Dosya bütünlüğü", Sonuc.Hata, r.CozmeHatasi + " çözme hatası. İlki: " + r.IlkCozmeHatasi);
            if (r.CozmeHatasi > 0)
                return new Madde("Dosya bütünlüğü", Sonuc.Uyari, r.CozmeHatasi + " küçük çözme uyarısı: " + r.IlkCozmeHatasi);
            return new Madde("Dosya bütünlüğü", Sonuc.Gecti, "Baştan sona hatasız çözüldü.");
        }

        static void BicimMaddeleri(Rapor r, float beklenen)
        {
            // Çözünürlük
            if (!r.VideoVar) r.Maddeler.Add(new Madde("Çözünürlük", Sonuc.Hata, "Görüntü akışı yok."));
            else if (r.Genislik == HedefGenislik && r.Yukseklik == HedefYukseklik)
                r.Maddeler.Add(new Madde("Çözünürlük", Sonuc.Gecti, r.Genislik + "x" + r.Yukseklik));
            else r.Maddeler.Add(new Madde("Çözünürlük", Sonuc.Hata, r.Genislik + "x" + r.Yukseklik + " (beklenen " + HedefGenislik + "x" + HedefYukseklik + ")"));

            // Kare hızı
            if (r.VideoVar)
            {
                double fark = Math.Abs(r.Fps - HedefFps);
                if (fark < 0.05) r.Maddeler.Add(new Madde("Kare hızı", Sonuc.Gecti, F(r.Fps, "0.##") + " fps"));
                else if (fark < 0.5) r.Maddeler.Add(new Madde("Kare hızı", Sonuc.Uyari, F(r.Fps, "0.##") + " fps (beklenen 30)"));
                else r.Maddeler.Add(new Madde("Kare hızı", Sonuc.Hata, F(r.Fps, "0.##") + " fps (beklenen 30)"));
            }

            // Kodekler
            if (r.VideoVar)
                r.Maddeler.Add(r.VideoKodek == "h264"
                    ? new Madde("Görüntü kodeki", Sonuc.Gecti, "H.264")
                    : new Madde("Görüntü kodeki", Sonuc.Hata, r.VideoKodek + " (beklenen H.264)"));
            if (!r.SesVar) r.Maddeler.Add(new Madde("Ses kodeki", Sonuc.Hata, "Ses akışı yok."));
            else r.Maddeler.Add(r.SesKodek == "aac"
                ? new Madde("Ses kodeki", Sonuc.Gecti, "AAC")
                : new Madde("Ses kodeki", Sonuc.Hata, r.SesKodek + " (beklenen AAC)"));

            // Süre
            if (r.Sure < 1.0) r.Maddeler.Add(new Madde("Süre", Sonuc.Hata, F(r.Sure) + " sn (video çok kısa)"));
            else if (beklenen <= 0) r.Maddeler.Add(new Madde("Süre", Sonuc.Gecti, F(r.Sure) + " sn"));
            else
            {
                double fark = Math.Abs(r.Sure - beklenen), oran = fark / beklenen;
                string t = F(r.Sure) + " sn (tahmini " + F(beklenen) + " sn)";
                if (r.Sure < beklenen * 0.5) r.Maddeler.Add(new Madde("Süre", Sonuc.Hata, t + ": beklenenin yarısından kısa, kayıt kesilmiş olabilir"));
                else if (fark <= 3.0 || oran <= 0.15) r.Maddeler.Add(new Madde("Süre", Sonuc.Gecti, t));
                else r.Maddeler.Add(new Madde("Süre", Sonuc.Uyari, t + ": tahminden %" + Math.Round(oran * 100) + " farklı"));
            }
        }

        static Madde SiyahMaddesi(Rapor r, double sure, double kanca)
        {
            if (r.Siyah.Count == 0) return new Madde("Siyah ekran", Sonuc.Gecti, "Siyah ekran yok.");
            double toplam = Toplam(r.Siyah);
            Sonuc d = Sonuc.Gecti;
            var sb = new StringBuilder();
            if (sure > 0 && toplam >= sure * 0.5) { d = Sonuc.Hata; sb.Append("Videonun %" + Math.Round(toplam / sure * 100) + "'i siyah. "); }
            foreach (var a in r.Siyah)
            {
                string ar = Ar(a);
                if (Muaf(a, sure, kanca)) { d = Kotu(d, Sonuc.Uyari); sb.Append(ar + " (kanca/başlangıç/bitiş, bilinçli olabilir); "); }
                else if (a.Sure >= 2.0) { d = Kotu(d, Sonuc.Hata); sb.Append(ar + " (uzun siyah); "); }
                else { d = Kotu(d, Sonuc.Uyari); sb.Append(ar + " (sahne geçişi olabilir); "); }
            }
            return new Madde("Siyah ekran", d, sb.ToString().TrimEnd(' ', ';'));
        }

        static Madde DonukMaddesi(Rapor r, double sure, double kanca)
        {
            // Siyah aralıklar aynı zamanda donuk görünür; onlar siyah maddesinde raporlandı.
            var kalan = new List<Aralik>();
            foreach (var a in r.Donuk)
            {
                double ort = 0;
                foreach (var s in r.Siyah) ort += Ortusme(a, s);
                if (a.Sure > 0 && ort >= a.Sure * 0.8) continue;
                kalan.Add(a);
            }
            if (kalan.Count == 0) return new Madde("Donmuş kare", Sonuc.Gecti, r.Donuk.Count > 0 ? "Donuk aralıklar yalnız siyah ekranda." : "Donmuş kare yok.");
            double toplam = Toplam(kalan);
            Sonuc d = Sonuc.Gecti;
            var sb = new StringBuilder();
            if (sure > 0 && toplam >= sure * 0.5) { d = Sonuc.Hata; sb.Append("Görüntü sürenin %" + Math.Round(toplam / sure * 100) + "'inde donuk. "); }
            foreach (var a in kalan)
            {
                string ar = Ar(a);
                if (Muaf(a, sure, kanca)) { d = Kotu(d, Sonuc.Uyari); sb.Append(ar + " (kanca/başlangıç/bitiş); "); }
                else if (a.Sure >= 5.0) { d = Kotu(d, Sonuc.Hata); sb.Append(ar + " (uzun donma); "); }
                else { d = Kotu(d, Sonuc.Uyari); sb.Append(ar + " (bekle: olabilir); "); }
            }
            return new Madde("Donmuş kare", d, sb.ToString().TrimEnd(' ', ';'));
        }

        static void SesMaddeleri(Rapor r, double sure, double kanca)
        {
            if (!r.SesVar)
            {
                r.Maddeler.Add(new Madde("Eksik ses", Sonuc.Hata, "EKSİK SES: videoda ses akışı yok."));
                return;
            }
            double sessizToplam = Toplam(r.Sessiz);
            bool tamSessiz = double.IsNegativeInfinity(r.Tepe) || (!double.IsNaN(r.Lufs) && r.Lufs <= -60.0)
                             || (sure > 0 && sessizToplam >= sure * 0.9);
            if (tamSessiz)
            {
                r.Maddeler.Add(new Madde("Eksik ses", Sonuc.Hata, "EKSİK SES: video tamamen (ya da neredeyse tamamen) sessiz."));
                return;
            }

            // Sessiz aralıklar (≥3 sn)
            Sonuc d = Sonuc.Gecti;
            var sb = new StringBuilder();
            foreach (var a in r.Sessiz)
            {
                if (Muaf(a, sure, kanca)) { sb.Append(Ar(a) + " (kanca/başlangıç/bitiş); "); continue; }
                d = Kotu(d, Sonuc.Uyari);
                sb.Append(Ar(a) + " sessiz (eksik satır sesi olabilir); ");
            }
            r.Maddeler.Add(new Madde("Eksik ses", d, sb.Length > 0 ? sb.ToString().TrimEnd(' ', ';') : "Uzun sessizlik yok."));

            // Ses seviyesi
            if (double.IsNaN(r.Lufs)) r.Maddeler.Add(new Madde("Ses seviyesi", Sonuc.Uyari, "LUFS ölçülemedi."));
            else
            {
                double fark = r.Lufs - HedefLufs;
                string t = F(r.Lufs) + " LUFS (hedef " + F(HedefLufs) + " ±" + F(LufsTolerans) + ")";
                r.Maddeler.Add(Math.Abs(fark) <= LufsTolerans
                    ? new Madde("Ses seviyesi", Sonuc.Gecti, t)
                    : new Madde("Ses seviyesi", Sonuc.Uyari, t + (fark < 0 ? ": ses kısık" : ": ses yüksek")));
            }

            // Tepe
            if (double.IsNaN(r.Tepe)) r.Maddeler.Add(new Madde("Ses tepesi", Sonuc.Uyari, "Tepe değeri ölçülemedi."));
            else r.Maddeler.Add(r.Tepe <= TepeSiniri
                ? new Madde("Ses tepesi", Sonuc.Gecti, F(r.Tepe) + " dBTP (sınır " + F(TepeSiniri) + ")")
                : new Madde("Ses tepesi", Sonuc.Uyari, F(r.Tepe) + " dBTP: sınır " + F(TepeSiniri) + " dBTP aşılıyor, cızırtı riski"));
        }

        static void KontrolEdilmeyenleriEkle(Rapor r)
        {
            const string not = "kontrol edilmedi (bu aşamada otomatik ölçülmüyor)";
            r.Maddeler.Add(new Madde("Altyazı taşması", Sonuc.KontrolEdilmedi, not));
            r.Maddeler.Add(new Madde("Kadraj", Sonuc.KontrolEdilmedi, not));
            r.Maddeler.Add(new Madde("Ağız senkronu", Sonuc.KontrolEdilmedi, not));
        }

        static Sonuc GenelSonuc(Rapor r)
        {
            Sonuc g = Sonuc.Gecti;
            bool olculen = false;
            foreach (var m in r.Maddeler)
            {
                if (m.Durum == Sonuc.KontrolEdilmedi) continue;
                olculen = true;
                g = Kotu(g, m.Durum);
            }
            return olculen ? g : Sonuc.KontrolEdilmedi;
        }

        // ================= metin =================

        static string F(double d, string bicim = "0.0")
        {
            if (double.IsNegativeInfinity(d)) return "-∞";
            if (double.IsNaN(d)) return "?";
            return d.ToString(bicim, Inv).Replace('.', ',');
        }

        static string Ar(Aralik a) { return F(a.Bas) + "–" + F(a.Son) + " sn (" + F(a.Sure) + " sn)"; }

        public static string Etiket(Sonuc s)
        {
            switch (s)
            {
                case Sonuc.Gecti: return "GEÇTİ";
                case Sonuc.Uyari: return "UYARI";
                case Sonuc.Hata: return "HATA";
                default: return "KONTROL EDİLMEDİ";
            }
        }

        public static int Say(Rapor r, Sonuc s) { int n = 0; foreach (var m in r.Maddeler) if (m.Durum == s) n++; return n; }

        // Arayüz için tek satır: "Kalite: UYARI (0 hata, 2 uyarı)"
        public static string OzetSatiri(Rapor r)
        {
            if (r == null) return "Kalite: denetlenmedi";
            return "Kalite: " + Etiket(r.Genel) + " (" + Say(r, Sonuc.Hata) + " hata, " + Say(r, Sonuc.Uyari) + " uyarı)";
        }

        public static string RaporMetni(Rapor r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("HİKÂYE STÜDYOSU: KALİTE RAPORU");
            sb.AppendLine("Dosya : " + r.Dosya);
            sb.AppendLine("Tarih : " + r.Zaman.ToString("dd.MM.yyyy HH:mm:ss", Inv));
            if (r.Boyut > 0) sb.AppendLine("Boyut : " + F(r.Boyut / 1048576.0, "0.0") + " MB");
            if (r.BicimOkundu)
                sb.AppendLine("Biçim : " + r.Genislik + "x" + r.Yukseklik + ", " + F(r.Fps, "0.##") + " fps, " + (r.VideoKodek.Length > 0 ? r.VideoKodek : "-") + " / " + (r.SesKodek.Length > 0 ? r.SesKodek : "ses yok") + ", " + F(r.Sure) + " sn");
            sb.AppendLine("GENEL SONUÇ: " + Etiket(r.Genel) + "  (" + Say(r, Sonuc.Hata) + " hata, " + Say(r, Sonuc.Uyari) + " uyarı)");
            if (r.GenelNot.Length > 0) sb.AppendLine("Not: " + r.GenelNot);
            sb.AppendLine();
            foreach (var m in r.Maddeler)
                sb.AppendLine("[" + Etiket(m.Durum) + "] " + m.Ad + ": " + m.Ayrinti);
            return sb.ToString();
        }

        // Yayın klasörüne kalite_raporu.txt (UTF-8, BOM'lu: Not Defteri Türkçe karakterleri doğru gösterir)
        public static string Yaz(string klasor, Rapor r)
        {
            try
            {
                Directory.CreateDirectory(klasor);
                string p = Path.Combine(klasor, "kalite_raporu.txt");
                File.WriteAllText(p, RaporMetni(r), new UTF8Encoding(true));
                return p;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Hikaye] Kalite raporu yazılamadı: " + e.Message);
                return null;
            }
        }
    }
}
