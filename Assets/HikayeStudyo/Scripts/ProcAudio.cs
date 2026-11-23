using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Ses temizleme (bosluk kirpma, seviye esitleme) ve hazir dosya yokken kodla uretilen efekt sesleri
    public static class ProcAudio
    {
        const int Rate = 44100;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        // ---------- Konusma temizleme ----------
        // Bas/son sessizligi atar, 'maxGap'ten uzun ic sessizlikleri 'keepGap'e indirir, sesi esitler.
        public static AudioClip Clean(AudioClip src, float maxGap = 0.5f, float keepGap = 0.32f, float targetRms = 0.12f)
        {
            if (src == null) return null;
            if (src.loadType != AudioClipLoadType.DecompressOnLoad) return src;
            if (src.loadState != AudioDataLoadState.Loaded) src.LoadAudioData();
            int ch = Mathf.Max(1, src.channels);
            int frames = src.samples;
            if (frames <= 0) return src;
            var data = new float[frames * ch];
            if (!src.GetData(data, 0)) return src;

            var mono = new float[frames];
            float peak = 0f;
            for (int i = 0; i < frames; i++)
            {
                float s = 0;
                for (int c = 0; c < ch; c++) s += data[i * ch + c];
                s /= ch;
                mono[i] = s;
                if (Mathf.Abs(s) > peak) peak = Mathf.Abs(s);
            }
            if (peak < 1e-4f) return src;

            int rate = src.frequency;
            int win = Mathf.Max(1, rate / 100); // 10 ms
            int n = frames / win;
            if (n < 3) return src;
            float thr = Mathf.Max(0.006f, peak * 0.035f);
            var active = new bool[n];
            double activeSum = 0; int activeCount = 0;
            for (int w = 0; w < n; w++)
            {
                double sum = 0;
                for (int i = w * win; i < (w + 1) * win; i++) sum += mono[i] * mono[i];
                float rms = Mathf.Sqrt((float)(sum / win));
                active[w] = rms > thr;
                if (active[w]) { activeSum += sum; activeCount += win; }
            }
            int first = System.Array.IndexOf(active, true);
            int last = System.Array.LastIndexOf(active, true);
            if (first < 0) return src;
            int pad = 5;
            first = Mathf.Max(0, first - pad);
            last = Mathf.Min(n - 1, last + pad);

            int maxGapW = Mathf.RoundToInt(maxGap * 100f);
            int keepHalf = Mathf.RoundToInt(keepGap * 50f);
            var keep = new bool[n];
            int wi = first;
            while (wi <= last)
            {
                if (active[wi]) { keep[wi] = true; wi++; continue; }
                int start = wi;
                while (wi <= last && !active[wi]) wi++;
                int len = wi - start;
                if (len <= maxGapW || start == first || wi > last)
                    for (int k = start; k < wi; k++) keep[k] = true;      // kisa bosluk ya da bas/son dolgu: aynen kalir
                else
                {
                    for (int k = start; k < start + keepHalf; k++) keep[k] = true;
                    for (int k = wi - keepHalf; k < wi; k++) keep[k] = true;
                }
            }

            float rmsAct = activeCount > 0 ? Mathf.Sqrt((float)(activeSum / activeCount)) : 0.1f;
            float gain = Mathf.Clamp(targetRms / Mathf.Max(rmsAct, 1e-4f), 0.5f, 4f);
            if (peak * gain > 0.95f) gain = 0.95f / peak;

            var outList = new List<float>(frames);
            for (int w = first; w <= last; w++)
            {
                if (!keep[w]) continue;
                for (int i = w * win; i < (w + 1) * win; i++) outList.Add(mono[i] * gain);
            }
            // tiklamayi onlemek icin 5 ms giris/cikis
            int fade = Mathf.Min(outList.Count / 4, rate / 200);
            for (int i = 0; i < fade; i++)
            {
                float f = i / (float)fade;
                outList[i] *= f;
                outList[outList.Count - 1 - i] *= f;
            }
            var clip = AudioClip.Create(src.name + "_temiz", outList.Count, 1, rate, false);
            clip.SetData(outList.ToArray(), 0);
            return clip;
        }

        // ---------- Uretilen sesler ----------
        delegate float Synth(float t, float dur, System.Random rnd);

        static AudioClip Make(string id, float dur, Synth f, bool normalize = true)
        {
            AudioClip c;
            if (cache.TryGetValue(id, out c) && c != null) return c;
            int n = Mathf.CeilToInt(dur * Rate);
            var d = new float[n];
            var rnd = new System.Random(id.GetHashCode());
            float peak = 0;
            for (int i = 0; i < n; i++) { d[i] = f(i / (float)Rate, dur, rnd); peak = Mathf.Max(peak, Mathf.Abs(d[i])); }
            if (normalize && peak > 0) for (int i = 0; i < n; i++) d[i] *= 0.85f / peak;
            c = AudioClip.Create(id, n, 1, Rate, false);
            c.SetData(d, 0);
            cache[id] = c;
            return c;
        }

        static float Noise(System.Random r) { return (float)(r.NextDouble() * 2.0 - 1.0); }
        static float Sin(float hz, float t) { return Mathf.Sin(2f * Mathf.PI * hz * t); }

        static float lp; // basit tek kutuplu alcak geciren filtre durumu
        static float LowPass(float x, float a) { lp += a * (x - lp); return lp; }

        public static bool Has(string id)
        {
            switch (Util.Norm(id))
            {
                case "carpma": case "korna": case "geri": case "whoosh": case "gecis": case "kapi":
                case "buyu": case "kalp": case "tahta": case "kase": case "yagmur": case "flas": case "gok": case "gokgurultusu":
                    return true;
            }
            return false;
        }

        public static AudioClip Get(string id)
        {
            switch (Util.Norm(id))
            {
                case "carpma":
                    lp = 0;
                    return Make("carpma", 1.4f, (t, d, r) =>
                    {
                        float body = Sin(48f + 30f * Mathf.Exp(-t * 8f), t) * Mathf.Exp(-t * 3.5f);
                        float crunch = LowPass(Noise(r), 0.35f) * Mathf.Exp(-t * 9f) * 1.4f;
                        float glass = (t > 0.05f ? Noise(r) * 0.25f * Mathf.Exp(-(t - 0.05f) * 6f) : 0f);
                        return body + crunch + glass;
                    });
                case "korna":
                    return Make("korna", 1.1f, (t, d, r) =>
                    {
                        float env = Mathf.Clamp01(t * 30f) * Mathf.Clamp01((d - t) * 8f);
                        float s = 0;
                        for (int k = 1; k <= 7; k += 2) s += (Sin(370f * k, t) + Sin(466f * k, t)) / k;
                        return s * env * 0.5f;
                    });
                case "geri":
                    lp = 0;
                    return Make("geri", 1.3f, (t, d, r) =>
                    {
                        float p = t / d;
                        float pitch = Mathf.Lerp(1400f, 180f, p);
                        float wobble = 0.6f + 0.4f * Sin(14f, t);
                        float tone = Mathf.Sin(2f * Mathf.PI * pitch * t * (1f - p * 0.5f)) * 0.5f;
                        float hiss = LowPass(Noise(r), 0.25f) * 0.6f;
                        return (tone + hiss) * wobble * Mathf.Clamp01(t * 20f) * Mathf.Clamp01((d - t) * 6f);
                    });
                case "whoosh":
                case "gecis":
                case "flas":
                    lp = 0;
                    return Make("whoosh", 0.7f, (t, d, r) =>
                    {
                        float p = t / d;
                        float env = Mathf.Sin(Mathf.PI * p);
                        return LowPass(Noise(r), Mathf.Lerp(0.03f, 0.35f, env)) * env * env;
                    });
                case "kapi":
                    return Make("kapi", 1.3f, (t, d, r) =>
                    {
                        float hz = 160f + 90f * Sin(1.7f, t) + 30f * Sin(7.3f, t);
                        float pulses = Mathf.Abs(Sin(23f + 8f * Sin(2f, t), t));
                        float s = Mathf.Sin(2f * Mathf.PI * hz * t + 3f * Sin(hz * 0.5f, t));
                        return s * pulses * Mathf.Clamp01(t * 10f) * Mathf.Clamp01((d - t) * 3f) * 0.6f;
                    });
                case "buyu":
                    return Make("buyu", 2.2f, (t, d, r) =>
                    {
                        float s = 0;
                        float[] notes = { 880f, 1174.7f, 1318.5f, 1760f, 2349.3f };
                        for (int k = 0; k < notes.Length; k++)
                        {
                            float st = k * 0.12f;
                            if (t < st) continue;
                            float tt = t - st;
                            s += (Sin(notes[k], tt) + 0.3f * Sin(notes[k] * 2.01f, tt)) * Mathf.Exp(-tt * 2.2f);
                        }
                        return s + Noise(r) * 0.02f * Mathf.Exp(-t);
                    });
                case "kalp":
                    return Make("kalp", 1.0f, (t, d, r) =>
                    {
                        float a = t < 0.25f ? Sin(55f, t) * Mathf.Exp(-t * 18f) : 0f;
                        float t2 = t - 0.28f;
                        float b = t2 > 0 ? Sin(50f, t2) * Mathf.Exp(-t2 * 16f) * 0.7f : 0f;
                        return a + b;
                    });
                case "tahta":
                case "kase":
                    return Make("tahta", 0.35f, (t, d, r) =>
                        (Sin(320f, t) * 0.7f + Sin(780f, t) * 0.3f) * Mathf.Exp(-t * 28f) + Noise(r) * Mathf.Exp(-t * 60f) * 0.4f);
                case "gok":
                case "gokgurultusu":
                    lp = 0;
                    return Make("gok", 3f, (t, d, r) =>
                        LowPass(Noise(r), 0.02f) * 6f * Mathf.Exp(-t * 1.2f) * (0.6f + 0.4f * Sin(3f, t)));
                case "yagmur":
                    return Rain();
            }
            return null;
        }

        // Dongulu yagmur sesi
        public static AudioClip Rain()
        {
            lp = 0;
            return Make("yagmur_dongu", 4f, (t, d, r) =>
            {
                float bed = LowPass(Noise(r), 0.18f) * 0.7f;
                float drop = r.NextDouble() < 0.0015 ? Noise(r) * 1.5f : 0f;
                float edge = Mathf.Clamp01(t * 20f) * Mathf.Clamp01((d - t) * 20f); // dongu birlesimi
                return (bed + drop) * Mathf.Lerp(0.6f, 1f, edge);
            });
        }

        // Yumusak arka plan ambiyansi (pad). Frekanslar 8 sn'de tam tur atar, dongu tiklamaz.
        public static AudioClip Pad()
        {
            return Make("pad", 8f, (t, d, r) =>
            {
                float lfo = 0.65f + 0.35f * Sin(0.25f, t);
                float s = Sin(110f, t) * 0.5f
                        + Sin(2093f / 8f, t) * 0.32f
                        + Sin(2637f / 8f, t) * 0.26f * (0.7f + 0.3f * Sin(0.125f, t))
                        + Sin(3136f / 8f, t) * 0.2f * lfo
                        + Sin(3953f / 8f, t) * 0.08f;
                return s * lfo;
            });
        }
    }
}
