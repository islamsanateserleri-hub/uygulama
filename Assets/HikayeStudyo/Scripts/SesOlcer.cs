using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Gercek ses yuksekligi olcumu (ITU-R BS.1770 / EBU R128): K-agirlikli, 400 ms bloklar, -70 LUFS mutlak ve
    // -10 LU goreli kapi. AudioListener'a takilir; videoya giden son miks ne ise onu olcer. Hedef: -14 LUFS.
    [RequireComponent(typeof(AudioListener))]
    public class SesOlcer : MonoBehaviour
    {
        public const float Hedef = -14f;

        // her kanal icin iki asamali biquad durumu
        class Biquad
        {
            public double b0, b1, b2, a1, a2, z1, z2;
            public double Run(double x)
            {
                double y = b0 * x + z1;
                z1 = b1 * x - a1 * y + z2;
                z2 = b2 * x - a2 * y;
                return y;
            }
        }

        Biquad[] shelf, pass;
        int channels, rate, subLen, subCount;
        double[] subSum;                       // 100 ms alt blok, kanal basina kare toplami
        readonly List<double> subBlocks = new List<double>();   // her 100 ms alt blogun (kanal toplamli) ortalama karesi
        readonly object gate = new object();
        double peak;

        public bool Measuring = true;

        void Awake()
        {
            rate = AudioSettings.outputSampleRate;
            Setup(2);
        }

        void Setup(int ch)
        {
            channels = ch;
            shelf = new Biquad[ch]; pass = new Biquad[ch];
            for (int i = 0; i < ch; i++) { shelf[i] = Shelf(rate); pass[i] = HighPass(rate); }
            subLen = Mathf.Max(1, rate / 10);
            subSum = new double[ch];
            subCount = 0;
        }

        static Biquad Shelf(int fs)
        {
            double f0 = 1681.974450955533, G = 3.999843853973347, Q = 0.7071752369554196;
            double K = System.Math.Tan(System.Math.PI * f0 / fs);
            double Vh = System.Math.Pow(10.0, G / 20.0);
            double Vb = System.Math.Pow(Vh, 0.4996667741545416);
            double a0 = 1.0 + K / Q + K * K;
            return new Biquad
            {
                b0 = (Vh + Vb * K / Q + K * K) / a0,
                b1 = 2.0 * (K * K - Vh) / a0,
                b2 = (Vh - Vb * K / Q + K * K) / a0,
                a1 = 2.0 * (K * K - 1.0) / a0,
                a2 = (1.0 - K / Q + K * K) / a0
            };
        }

        static Biquad HighPass(int fs)
        {
            double f0 = 38.13547087602444, Q = 0.5003270373238773;
            double K = System.Math.Tan(System.Math.PI * f0 / fs);
            double d = 1.0 + K / Q + K * K;
            return new Biquad { b0 = 1.0, b1 = -2.0, b2 = 1.0, a1 = 2.0 * (K * K - 1.0) / d, a2 = (1.0 - K / Q + K * K) / d };
        }

        void OnAudioFilterRead(float[] data, int ch)
        {
            if (!Measuring) return;
            lock (gate)
            {
                if (ch != channels) Setup(ch);
                int frames = data.Length / ch;
                for (int f = 0; f < frames; f++)
                {
                    for (int c = 0; c < ch; c++)
                    {
                        double x = data[f * ch + c];
                        double ax = x < 0 ? -x : x;
                        if (ax > peak) peak = ax;
                        double y = pass[c].Run(shelf[c].Run(x));
                        subSum[c] += y * y;
                    }
                    if (++subCount >= subLen)
                    {
                        double z = 0;
                        // sol/sag kazanci 1; 5.1'de cevre kanallari 1.41 (burada stereo/mono)
                        for (int c = 0; c < ch && c < 2; c++) z += subSum[c] / subLen;
                        if (ch == 1) z *= 2.0;   // mono -> iki kulakta ayni ses gibi
                        subBlocks.Add(z);
                        for (int c = 0; c < ch; c++) subSum[c] = 0;
                        subCount = 0;
                    }
                }
            }
        }

        static double Lufs(double z) { return z <= 0 ? -200 : -0.691 + 10.0 * System.Math.Log10(z); }

        // Entegre ses yuksekligi (LUFS). Olcum yoksa -200.
        public double Integrated(out double peakDb)
        {
            List<double> subs;
            lock (gate) { subs = new List<double>(subBlocks); peakDb = peak > 0 ? 20.0 * System.Math.Log10(peak) : -200; }
            // 400 ms bloklar, %75 ortusme (4 alt blok, 1'er kaydirarak)
            var blocks = new List<double>();
            for (int i = 0; i + 3 < subs.Count; i++) blocks.Add((subs[i] + subs[i + 1] + subs[i + 2] + subs[i + 3]) / 4.0);
            var abs = blocks.FindAll(z => Lufs(z) > -70.0);
            if (abs.Count == 0) return -200;
            double mean = 0; foreach (var z in abs) mean += z; mean /= abs.Count;
            double rel = Lufs(mean) - 10.0;
            var fin = abs.FindAll(z => Lufs(z) > rel);
            if (fin.Count == 0) return Lufs(mean);
            double m2 = 0; foreach (var z in fin) m2 += z; m2 /= fin.Count;
            return Lufs(m2);
        }

        public string Rapor()
        {
            double pk;
            double l = Integrated(out pk);
            if (l < -100) return "Ses yüksekliği ölçülemedi (ses yok).";
            double fark = Hedef - l;
            string s = "Ses yüksekliği: " + l.ToString("0.0") + " LUFS (hedef " + Hedef.ToString("0") + "), tepe: " + pk.ToString("0.0") + " dBFS.";
            if (System.Math.Abs(fark) <= 1.0) s += " Hedefte.";
            else if (fark > 0) s += " " + fark.ToString("0.0") + " dB kısık: müziği/sesi biraz aç (StoryPlayer: voiceVolume, ambientVolume).";
            else s += " " + (-fark).ToString("0.0") + " dB yüksek: sesi biraz kıs.";
            if (pk > -1.0) s += " Tepe -1 dBFS üstünde: kırpılma olabilir.";
            return s;
        }
    }
}
