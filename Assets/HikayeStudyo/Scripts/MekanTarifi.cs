using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 12: 3D mekan "tarifi" = temel mekan + renkler + varsayilan zaman / hava / isik.
    // Hikayede:   mekan3d: mutfak duvar=mavi zemin=ahsap vurgu=kirmizi
    // Kaydedilen: Belgeler\HikayeStudyosu\Mekanlar\<ad>.txt  ->  mekan3d: <ad>
    public class MekanTarifi
    {
        public string Ad = "";      // kaydedilen mekanin adi (hazir mekanda bos)
        public string Tur = "";     // oda, ev, mutfak ...
        public string Duvar = "", Zemin = "", Vurgu = "";          // renk adlari
        public string Zaman = "", Hava = "", Isik = "";            // varsayilanlar

        public static readonly string[] Anahtarlar = { "duvar", "zemin", "vurgu", "zaman", "hava", "isik" };

        public static readonly string[] RenkAdlari =
        {
            "beyaz", "krem", "sari", "turuncu", "kirmizi", "bordo", "pembe", "mor",
            "mavi", "lacivert", "turkuaz", "yesil", "kahve", "ahsap", "gri", "siyah"
        };

        public static bool RenkBul(string ad, out Color c)
        {
            switch (Util.Norm(ad))
            {
                case "beyaz": c = new Color(0.9f, 0.9f, 0.88f); return true;
                case "krem": c = new Color(0.85f, 0.77f, 0.62f); return true;
                case "sari": c = new Color(0.88f, 0.72f, 0.25f); return true;
                case "turuncu": c = new Color(0.82f, 0.46f, 0.18f); return true;
                case "kirmizi": c = new Color(0.66f, 0.14f, 0.14f); return true;
                case "bordo": c = new Color(0.38f, 0.07f, 0.12f); return true;
                case "pembe": c = new Color(0.88f, 0.55f, 0.65f); return true;
                case "mor": c = new Color(0.42f, 0.26f, 0.56f); return true;
                case "mavi": c = new Color(0.24f, 0.45f, 0.72f); return true;
                case "lacivert": c = new Color(0.09f, 0.14f, 0.34f); return true;
                case "turkuaz": c = new Color(0.2f, 0.6f, 0.6f); return true;
                case "yesil": c = new Color(0.27f, 0.49f, 0.24f); return true;
                case "kahve": c = new Color(0.33f, 0.21f, 0.13f); return true;
                case "ahsap": c = new Color(0.55f, 0.38f, 0.23f); return true;
                case "gri": c = new Color(0.48f, 0.49f, 0.52f); return true;
                case "siyah": c = new Color(0.08f, 0.08f, 0.09f); return true;
            }
            c = Color.white;
            return false;
        }

        // ---------- Kaydedilen mekanlar ----------
        public static string Klasor
        {
            get { return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HikayeStudyosu"), "Mekanlar"); }
        }

        // Dosya adi / hikaye adi: yalniz harf, rakam, _
        public static string AdTemizle(string ad)
        {
            var sb = new StringBuilder();
            foreach (var ch in Util.Norm(ad))
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_') sb.Append(ch);
            var s = sb.ToString().Trim('_');
            return s.Length > 32 ? s.Substring(0, 32) : s;
        }

        public static List<MekanTarifi> Kayitlilar()
        {
            var l = new List<MekanTarifi>();
            try
            {
                if (!Directory.Exists(Klasor)) return l;
                var dosyalar = Directory.GetFiles(Klasor, "*.txt");
                Array.Sort(dosyalar, StringComparer.OrdinalIgnoreCase);
                foreach (var d in dosyalar)
                {
                    var t = Oku(Path.GetFileNameWithoutExtension(d));
                    if (t != null) l.Add(t);
                }
            }
            catch (Exception e) { Debug.LogWarning("[Hikaye] kayıtlı mekânlar okunamadı: " + e.Message); }
            return l;
        }

        public static MekanTarifi Oku(string ad)
        {
            string a = AdTemizle(ad);
            if (a.Length == 0) return null;
            try
            {
                string yol = Path.Combine(Klasor, a + ".txt");
                if (!File.Exists(yol)) return null;
                var t = new MekanTarifi { Ad = a };
                foreach (var satir in File.ReadAllLines(yol, Encoding.UTF8))
                {
                    int i = satir.IndexOf('=');
                    if (i <= 0) continue;
                    string k = Util.Norm(satir.Substring(0, i).Trim());
                    string v = Util.Norm(satir.Substring(i + 1).Trim());
                    if (k == "tur") { if (Util.In(v, Mekan3D.TurAdlari)) t.Tur = v; }
                    else t.Ata(k, v);
                }
                return t.Tur.Length > 0 ? t : null;
            }
            catch (Exception e) { Debug.LogWarning("[Hikaye] mekân okunamadı (" + ad + "): " + e.Message); return null; }
        }

        // null = basarili, degilse hata metni
        public string Kaydet()
        {
            string a = AdTemizle(Ad);
            if (a.Length == 0) return "Mekâna bir ad ver (harf ve rakam).";
            if (Util.In(a, Mekan3D.TurAdlari) || a == "yok" || a == "kapat") return "'" + a + "' hazır bir mekân adı; başka bir ad ver.";
            if (!Util.In(Tur, Mekan3D.TurAdlari)) return "Önce temel mekânı seç.";
            try
            {
                Directory.CreateDirectory(Klasor);
                var sb = new StringBuilder();
                sb.Append("tur=").Append(Tur).Append('\n');
                foreach (var k in Anahtarlar)
                {
                    string v = Deger(k);
                    if (v.Length > 0) sb.Append(k).Append('=').Append(v).Append('\n');
                }
                string yol = Path.Combine(Klasor, a + ".txt");
                if (File.Exists(yol))
                {
                    // ayni adli eski mekan ezilmeden once yedeklenir
                    string cop = Path.Combine(Klasor, "_silinen");
                    Directory.CreateDirectory(cop);
                    File.Copy(yol, Path.Combine(cop, a + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt"));
                }
                File.WriteAllText(yol, sb.ToString(), new UTF8Encoding(false));
                Ad = a;
                return null;
            }
            catch (Exception e) { return "Kaydedilemedi: " + e.Message; }
        }

        // Silinmez, _silinen klasorune tasinir
        public static string Kaldir(string ad)
        {
            string a = AdTemizle(ad);
            try
            {
                string yol = Path.Combine(Klasor, a + ".txt");
                if (!File.Exists(yol)) return "Bulunamadı.";
                string cop = Path.Combine(Klasor, "_silinen");
                Directory.CreateDirectory(cop);
                File.Move(yol, Path.Combine(cop, a + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt"));
                return null;
            }
            catch (Exception e) { return "Kaldırılamadı: " + e.Message; }
        }

        public string Deger(string anahtar)
        {
            switch (anahtar)
            {
                case "duvar": return Duvar;
                case "zemin": return Zemin;
                case "vurgu": return Vurgu;
                case "zaman": return Zaman;
                case "hava": return Hava;
                case "isik": return Isik;
            }
            return "";
        }

        // Gecerli degilse false ve degistirmez
        public bool Ata(string anahtar, string deger)
        {
            string v = Util.Norm(deger);
            Color c;
            switch (anahtar)
            {
                case "duvar": if (!RenkBul(v, out c)) return false; Duvar = v; return true;
                case "zemin": if (!RenkBul(v, out c)) return false; Zemin = v; return true;
                case "vurgu": if (!RenkBul(v, out c)) return false; Vurgu = v; return true;
                case "zaman": if (!Util.In(v, YonetmenTablo.Zamanlar)) return false; Zaman = v; return true;
                case "hava":
                    // birden cok hava virgulle yazilabilir: hava=yagmur,sis
                    var havalar = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (havalar.Length == 0) return false;
                    foreach (var h in havalar) if (!Util.In(h, YonetmenTablo.Havalar)) return false;
                    Hava = string.Join(",", havalar); return true;
                case "isik": if (!Util.In(v, YonetmenTablo.Isiklar)) return false; Isik = v; return true;
            }
            return false;
        }

        // "mutfak duvar=mavi" -> tarif. Hata varsa null doner ve 'hata' Turkce aciklamadir.
        public static MekanTarifi Coz(string arg, out string hata)
        {
            hata = null;
            var adParcalari = new List<string>();
            var ayarlar = new List<string>();
            // "duvar = mavi" yazimi da kabul edilir
            string s = (arg ?? "").Trim().Replace(" =", "=").Replace("= ", "=");
            // virgul ayirici sayilir ("mutfak, duvar=mavi"); yalniz "hava=yagmur, sis" icinde degerin parcasidir
            foreach (var p in s.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (p.IndexOf('=') >= 0) { ayarlar.Add(p); continue; }
                int son = ayarlar.Count - 1;
                if (son >= 0 && Util.Norm(ayarlar[son]).StartsWith("hava=") && Util.In(Util.Norm(p), YonetmenTablo.Havalar)) ayarlar[son] += "," + p;
                else adParcalari.Add(p);
            }
            string ad = Util.Norm(string.Join("_", adParcalari.ToArray()));
            if (ad.Length == 0 && ayarlar.Count > 0)
            {
                // eskiden 3D mekani sessizce kapatiyordu
                hata = "Önce mekân adını yaz (örnek: mekan3d: mutfak duvar=mavi).";
                return null;
            }
            if (ad.Length == 0 || ad == "yok" || ad == "kapat") return new MekanTarifi { Tur = "yok" };

            MekanTarifi t;
            if (Util.In(ad, Mekan3D.TurAdlari)) t = new MekanTarifi { Tur = ad };
            else
            {
                t = Oku(ad);
                if (t == null)
                {
                    hata = "'" + ad + "' 3D mekânı yok. Hazır olanlar: " + string.Join(", ", Mekan3D.TurAdlari) +
                           " (ya da Mekânlar sayfasında kaydettiğin bir ad).";
                    return null;
                }
            }
            foreach (var a in ayarlar)
            {
                int i = a.IndexOf('=');
                string k = Util.Norm(a.Substring(0, i));
                string v = a.Substring(i + 1);
                if (!Util.In(k, Anahtarlar)) { hata = "'" + k + "' ayarı bilinmiyor (" + string.Join(", ", Anahtarlar) + ")."; return null; }
                if (!t.Ata(k, v))
                {
                    string[] liste = k == "zaman" ? YonetmenTablo.Zamanlar : k == "hava" ? YonetmenTablo.Havalar : k == "isik" ? YonetmenTablo.Isiklar : RenkAdlari;
                    hata = "'" + k + "=" + v + "' geçersiz (" + string.Join(", ", liste) + ").";
                    return null;
                }
            }
            return t;
        }
    }
}
