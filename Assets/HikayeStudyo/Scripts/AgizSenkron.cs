using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Konusma ses klibinden agiz acikligi zarfi (envelope) cikarir. Klip basina bir kere hesaplanir (50 Hz).
    // Hicbir zaman hata firlatmaz: okunamayan klipte null doner (cagiran sentetik agiz hareketine gecer).
    public static class AgizSenkron
    {
        public const float Hz = 50f;
        static readonly Dictionary<AudioClip, float[]> onbellek = new Dictionary<AudioClip, float[]>();

        // 0..1 arasi agiz acikligi zarfi. Dizi indeksi i -> zaman i / Hz saniye.
        public static float[] Zarf(AudioClip clip)
        {
            if (clip == null) return null;
            float[] e;
            if (onbellek.TryGetValue(clip, out e)) return e;
            if (onbellek.Count > 200) onbellek.Clear();
            e = Hesapla(clip);
            onbellek[clip] = e;
            return e;
        }

        static float[] Hesapla(AudioClip clip)
        {
            try
            {
                if (clip.loadType == AudioClipLoadType.Streaming) return null;
                int ch = Mathf.Max(1, clip.channels);
                int frames = Mathf.Min(clip.samples, clip.frequency * 120);
                if (frames <= 0) return null;
                if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();
                var data = new float[frames * ch];
                if (!clip.GetData(data, 0)) return null;

                int win = Mathf.Max(1, Mathf.RoundToInt(clip.frequency / Hz));
                int count = frames / win + 1;
                var raw = new float[count];
                for (int w = 0; w < count; w++)
                {
                    int a = w * win;
                    int b = Mathf.Min(frames, a + win);
                    if (b <= a) break;
                    double sum = 0;
                    for (int i = a; i < b; i++)
                    {
                        float s = 0f;
                        for (int c = 0; c < ch; c++) s += data[i * ch + c];
                        s /= ch;
                        sum += s * s;
                    }
                    raw[w] = Mathf.Sqrt((float)(sum / (b - a)));
                }

                // normalle: %92'lik dilim = 1.0 (aykiri tek tepeler agzi hep kisik birakmasin)
                var sorted = (float[])raw.Clone();
                System.Array.Sort(sorted);
                float p = sorted[Mathf.Clamp((int)(sorted.Length * 0.92f), 0, sorted.Length - 1)];
                var env = new float[count];
                if (p < 1e-4f) return env;   // sessiz klip
                float inv = 1f / p;
                float prev = 0f;
                for (int i = 0; i < count; i++)
                {
                    float v = Mathf.Clamp01(raw[i] * inv);
                    v = v < 0.07f ? 0f : (v - 0.07f) / 0.93f;   // gurultu kapisi
                    v = Mathf.Sqrt(v);                           // kisik heceler de agzi biraz actsin
                    prev = Mathf.Lerp(prev, v, v > prev ? 0.65f : 0.22f);
                    env[i] = prev;
                }
                return env;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        // t saniyedeki agiz seviyesi (0..1). Zarf yoksa 0.
        public static float Seviye(float[] zarf, float t)
        {
            if (zarf == null || zarf.Length == 0 || t < 0f) return 0f;
            float f = t * Hz;
            int i = (int)f;
            if (i >= zarf.Length) return 0f;
            if (i >= zarf.Length - 1) return zarf[zarf.Length - 1];
            return Mathf.Lerp(zarf[i], zarf[i + 1], f - i);
        }

        // Zarf yokken kullanilan yapay agiz hareketi (kabaca konusma ritmi)
        public static float Sentetik(float t, float tohum)
        {
            return Mathf.Clamp01(Mathf.PerlinNoise(t * 9f, tohum) * 1.5f - 0.2f);
        }

        // Sesin gercekten duyuldugu aralik (basta/sonda sessizlik atilir). Zarf yoksa [0, sure].
        public static void SesAraligi(float[] zarf, float sure, out float bas, out float son)
        {
            bas = 0f; son = sure;
            if (zarf == null || zarf.Length < 4 || sure <= 0.2f) return;
            int a = -1, b = -1;
            for (int i = 0; i < zarf.Length; i++)
                if (zarf[i] > 0.12f) { if (a < 0) a = i; b = i; }
            if (a < 0) return;
            float s = Mathf.Max(0f, a / Hz - 0.04f);
            float e = Mathf.Min(sure, (b + 1) / Hz + 0.06f);
            if (s >= sure * 0.8f) return;
            if (e - s < sure * 0.45f) return;   // sacma sonuc: tum araligi kullan
            bas = s; son = e;
        }
    }
}
