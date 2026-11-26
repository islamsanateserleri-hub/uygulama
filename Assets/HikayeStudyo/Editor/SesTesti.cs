using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // menu: Hikaye/Ses Testi -> butun kodla uretilen sesleri olusturur;
    // HikayeKomut/ses_test.txt (sure, tepe, ses duzeyi, hata) ve HikayeKomut/ses_ornekleri.wav (dinlemek icin arka arkaya) yazar.
    public static class SesTesti
    {
        const int Hiz = 44100;

        [MenuItem("Hikaye/Ses Testi")]
        public static void Calistir()
        {
            var rapor = new StringBuilder();
            var hepsi = new List<short>();
            int sorun = 0, sayi = 0;
            float basla = 0f;
            foreach (var ad in ProcAudio.Adlar)
            {
                sayi++;
                var clip = ProcAudio.Get(ad);
                if (clip == null) { rapor.AppendLine("HATA " + ad + ": ses uretilemedi"); sorun++; continue; }
                var d = new float[clip.samples * clip.channels];
                clip.GetData(d, 0);
                float tepe = 0f; double kare = 0; bool bozuk = false;
                foreach (var x in d)
                {
                    if (float.IsNaN(x) || float.IsInfinity(x)) { bozuk = true; continue; }
                    tepe = Mathf.Max(tepe, Mathf.Abs(x));
                    kare += x * x;
                }
                float rms = Mathf.Sqrt((float)(kare / Mathf.Max(1, d.Length)));
                bool kotu = bozuk || tepe < 0.05f || tepe > 1f || rms < 0.005f;
                if (kotu) sorun++;
                float sn = d.Length / (float)Hiz;
                rapor.AppendLine((kotu ? "SORUN " : "ok ") + ad + "  baslangic=" + Zaman(basla) + "  sure=" + sn.ToString("0.00") + "  tepe=" + tepe.ToString("0.00") + "  rms=" + rms.ToString("0.000"));
                foreach (var x in d) hepsi.Add((short)(Mathf.Clamp(float.IsNaN(x) ? 0f : x, -1f, 1f) * 32000f));
                for (int i = 0; i < Hiz / 2; i++) hepsi.Add(0);
                basla += sn + 0.5f;
            }
            var klasor = Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut");
            Directory.CreateDirectory(klasor);
            File.WriteAllText(Path.Combine(klasor, "ses_test.txt"), "ses sayisi: " + sayi + ", sorunlu: " + sorun + ", toplam sure: " + Zaman(basla) + "\n" + rapor, new UTF8Encoding(false));
            WavYaz(Path.Combine(klasor, "ses_ornekleri.wav"), hepsi);
            Debug.Log("[Hikaye] Ses testi: " + sayi + " ses, " + sorun + " sorunlu.");
        }

        static string Zaman(float sn) { return ((int)sn / 60) + ":" + ((int)sn % 60).ToString("00"); }

        static void WavYaz(string yol, List<short> ornek)
        {
            using (var w = new BinaryWriter(File.Create(yol)))
            {
                int veri = ornek.Count * 2;
                w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + veri);
                w.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); w.Write(16);
                w.Write((short)1); w.Write((short)1); w.Write(Hiz); w.Write(Hiz * 2); w.Write((short)2); w.Write((short)16);
                w.Write(Encoding.ASCII.GetBytes("data")); w.Write(veri);
                foreach (var s in ornek) w.Write(s);
            }
        }
    }
}
