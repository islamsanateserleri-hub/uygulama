using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 5 (P2): yayın paketi. Kayıt durduktan ve StoryPlayer.YayinUret (kapak.png + aciklama.txt) bittikten sonra
    // KayitIsleyici çağırır. Klasörde: video.mp4, kapak.png, baslik.txt, aciklama.txt, etiketler.txt, yayin.json.
    // kapak.png 1080x1920 değilse (ya da yoksa) ffmpeg ile düzeltilir / videodan bir kare alınır (kapak ekranı videoya girmez).
    public static class YayinPaketi
    {
        public const int KapakGenislik = 1080, KapakYukseklik = 1920;

        [Serializable]
        class YayinBilgisi
        {
            public string hikaye = "", baslik = "", seri = "";
            public int bolum;
            public float sureSn;
            public string video = "", kapak = "", aciklama = "aciklama.txt", etiketler = "", baslikDosyasi = "baslik.txt";
            public int kapakGenislik, kapakYukseklik;
            public string kapakKaynagi = "";
            public string[] secimler = new string[0];
            public string kodlayici = "", tarih = "", surum = "";
        }

        // Dönüş: kısa durum notu (kayit_sonuc.txt'ye eklenir)
        public static string Yaz(string klasor, StoryPlayer sp, string hikaye, string videoAdi, float sureSn, string kodlayici, bool rtKapak)
        {
            Directory.CreateDirectory(klasor);
            var meta = sp != null ? sp.Meta : null;
            string seri = meta != null ? (meta.Seri ?? "") : "";
            int bolum = meta != null ? meta.Bolum : 0;
            string baslik = meta != null ? (meta.Baslik ?? "") : "";
            if (baslik.Length == 0 && meta != null)
                foreach (var it in meta.Items) if (it.Type == ItemType.Hook && !string.IsNullOrEmpty(it.Text)) { baslik = it.Text; break; }
            if (baslik.Length == 0) baslik = hikaye ?? "Hikâye";
            string etiket = sp != null ? sp.EtiketSatiri() : "#hikaye #dizi #duygusal #keşfet";
            var utf8 = new UTF8Encoding(false);
            var notlar = new List<string>();

            // baslik.txt (tek satır: TikTok başlığı)
            string tamBaslik = seri.Length > 0 ? seri + (bolum > 0 ? " – Bölüm " + bolum : "") + ": " + baslik : (bolum > 0 ? "Bölüm " + bolum + ": " + baslik : baslik);
            File.WriteAllText(Path.Combine(klasor, "baslik.txt"), tamBaslik + "\n", utf8);

            // aciklama.txt: StoryPlayer.Yayin yazdıysa korunur; yoksa sade bir gövde
            string aciklama = Path.Combine(klasor, "aciklama.txt");
            if (!File.Exists(aciklama))
            {
                File.WriteAllText(aciklama, tamBaslik + "\n\nDevamı gelsin mi? Yorumlara yaz 👇\n\n" + etiket + "\n", utf8);
                notlar.Add("açıklama yedekten yazıldı");
            }

            // etiketler.txt
            File.WriteAllText(Path.Combine(klasor, "etiketler.txt"), etiket + "\n", utf8);

            // kapak.png: 1080x1920 olsun
            string kapak = Path.Combine(klasor, "kapak.png");
            string video = Path.Combine(klasor, string.IsNullOrEmpty(videoAdi) ? "video.mp4" : videoAdi);
            string kaynak = rtKapak ? "kayıt RT'si" : "ekran görüntüsü";
            int kw, kh;
            bool var_ = PngBoyutu(kapak, out kw, out kh);
            if (var_ && (kw != KapakGenislik || kh != KapakYukseklik))
            {
                if (FfmpegKapak("-y -loglevel error -i \"" + kapak + "\" -vf \"scale=" + KapakGenislik + ":" + KapakYukseklik +
                                ":force_original_aspect_ratio=increase,crop=" + KapakGenislik + ":" + KapakYukseklik + "\" -frames:v 1 ", kapak))
                { kaynak += ", ffmpeg ile " + kw + "x" + kh + " → 1080x1920"; notlar.Add("kapak ölçeklendi"); }
                var_ = PngBoyutu(kapak, out kw, out kh);
            }
            if (!var_ && File.Exists(video))
            {
                // Kapak hiç yazılamadıysa: videonun %30'undan bir kare (yalnız okunur, videoya bir şey eklenmez)
                float t = Mathf.Max(0.5f, sureSn * 0.3f);
                if (FfmpegKapak("-y -loglevel error -ss " + t.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) +
                                " -i \"" + video + "\" -frames:v 1 -vf \"scale=" + KapakGenislik + ":" + KapakYukseklik + "\" ", kapak))
                { kaynak = "videodan kare (" + t.ToString("0.0") + " sn)"; notlar.Add("kapak videodan alındı"); }
                var_ = PngBoyutu(kapak, out kw, out kh);
            }
            if (!var_) notlar.Add("kapak yok");

            // yayin.json
            var b = new YayinBilgisi
            {
                hikaye = hikaye ?? "", baslik = tamBaslik, seri = seri, bolum = bolum, sureSn = Mathf.Round(sureSn * 10f) / 10f,
                video = Path.GetFileName(video), kapak = var_ ? "kapak.png" : "", etiketler = "etiketler.txt",
                kapakGenislik = var_ ? kw : 0, kapakYukseklik = var_ ? kh : 0, kapakKaynagi = var_ ? kaynak : "",
                kodlayici = kodlayici ?? "", tarih = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"), surum = Application.version
            };
            if (sp != null && sp.Secimler != null) { var l = new List<string>(sp.Secimler); b.secimler = l.ToArray(); }
            File.WriteAllText(Path.Combine(klasor, "yayin.json"), JsonUtility.ToJson(b, true), utf8);

            string not = "yayın paketi: " + (var_ ? "kapak " + kw + "x" + kh : "kapak YOK") + ", başlık, açıklama, etiketler, yayin.json";
            if (notlar.Count > 0) not += " (" + string.Join(", ", notlar) + ")";
            Debug.Log("[Kayıt] " + not);
            return not;
        }

        // PNG başlığından (IHDR) genişlik/yükseklik
        public static bool PngBoyutu(string yol, out int w, out int h)
        {
            w = h = 0;
            try
            {
                if (!File.Exists(yol)) return false;
                var bas = new byte[24];
                using (var f = File.OpenRead(yol)) { if (f.Read(bas, 0, 24) < 24) return false; }
                if (bas[0] != 0x89 || bas[1] != (byte)'P' || bas[2] != (byte)'N' || bas[3] != (byte)'G') return false;
                w = (bas[16] << 24) | (bas[17] << 16) | (bas[18] << 8) | bas[19];
                h = (bas[20] << 24) | (bas[21] << 16) | (bas[22] << 8) | bas[23];
                return w > 0 && h > 0;
            }
            catch { return false; }
        }

        // ffmpeg ile geçici dosyaya yazar, başarılıysa hedefin yerine koyar
        static bool FfmpegKapak(string girisVeFiltre, string hedef)
        {
            string ff = FfmpegPipeRecorder.FfmpegBul();
            if (string.IsNullOrEmpty(ff)) return false;
            string tmp = hedef + ".tmp.png";
            try
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                var psi = new ProcessStartInfo(ff, girisVeFiltre + "\"" + tmp + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
                using (var p = Process.Start(psi))
                {
                    string e = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(30000)) { try { p.Kill(); } catch { } return false; }
                    if (p.ExitCode != 0 || !File.Exists(tmp)) { Debug.LogWarning("[Kayıt] kapak ffmpeg: " + e); return false; }
                }
                if (File.Exists(hedef)) File.Delete(hedef);
                File.Move(tmp, hedef);
                return true;
            }
            catch (Exception ex) { Debug.LogWarning("[Kayıt] kapak ffmpeg: " + ex.Message); try { if (File.Exists(tmp)) File.Delete(tmp); } catch { } return false; }
        }

        // Projeler ekranı: "Açıklamayı kopyala" metni (başlık + açıklama gövdesi; notlar bölümü hariç; etiketler yoksa eklenir)
        public static string PaylasimMetni(string klasor)
        {
            var utf8 = Encoding.UTF8;
            string baslik = "", aciklama = "", etiket = "";
            try { string f = Path.Combine(klasor, "baslik.txt"); if (File.Exists(f)) baslik = File.ReadAllText(f, utf8).Trim(); } catch { }
            try { string f = Path.Combine(klasor, "aciklama.txt"); if (File.Exists(f)) aciklama = File.ReadAllText(f, utf8).Replace("\r", ""); } catch { }
            try { string f = Path.Combine(klasor, "etiketler.txt"); if (File.Exists(f)) etiket = File.ReadAllText(f, utf8).Trim(); } catch { }
            int n = aciklama.IndexOf("--- Notlar", StringComparison.Ordinal);
            if (n >= 0) aciklama = aciklama.Substring(0, n);
            aciklama = aciklama.Trim();
            var sb = new StringBuilder();
            if (baslik.Length > 0) sb.Append(baslik).Append("\n\n");
            if (aciklama.Length > 0) sb.Append(aciklama).Append("\n");
            if (etiket.Length > 0 && aciklama.IndexOf(etiket, StringComparison.Ordinal) < 0) sb.Append("\n").Append(etiket).Append("\n");
            return sb.ToString().Trim();
        }
    }
}
