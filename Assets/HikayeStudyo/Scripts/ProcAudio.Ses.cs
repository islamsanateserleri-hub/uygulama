using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Kodla uretilen efekt sesleri (indirme ve lisans yok). Gercek ses kaydi DEGILDIR, benzetimdir.
    // Yeni ses eklemek: Tarifler tablosuna bir satir yazmak yeterli.
    public static partial class ProcAudio
    {
        static readonly string[] EskiAdlar =
            { "carpma", "korna", "geri", "whoosh", "gecis", "kapi", "buyu", "kalp", "tahta", "kase", "yagmur", "flas", "gok", "gokgurultusu" };

        // Hazir (kodla uretilen) butun efekt adlari
        public static List<string> Adlar
        {
            get
            {
                var l = new List<string>(EskiAdlar);
                l.AddRange(Tarifler.Keys);
                return l;
            }
        }

        static bool YeniVar(string id) { return Tarifler.ContainsKey(id); }

        static AudioClip YeniSes(string id)
        {
            Tarif t;
            if (!Tarifler.TryGetValue(id, out t)) return null;
            AudioClip c;
            if (cache.TryGetValue(id, out c) && c != null) return c;
            var d = Uret(id);
            c = AudioClip.Create(id, d.Length, 1, Rate, false);
            c.SetData(d, 0);
            cache[id] = c;
            return c;
        }

        // Ses verisini uretir (testlerde de kullanilir)
        public static float[] Uret(string id)
        {
            Tarif t;
            if (!Tarifler.TryGetValue(id, out t)) return null;
            int tohum = 17;
            foreach (var ch in id) tohum = tohum * 31 + ch;
            var s = new Tuval(t.Sure, tohum);
            t.Ciz(s);
            return s.Bitir(t.Duzey);
        }

        class Tarif
        {
            public float Sure, Duzey;
            public Action<Tuval> Ciz;
        }

        static Tarif T(float sure, float duzey, Action<Tuval> ciz) { return new Tarif { Sure = sure, Duzey = duzey, Ciz = ciz }; }

        // ---------- Ses tuvali: parcalar ust uste eklenir ----------
        const int Sin_ = 0, Kare = 1, Testere = 2, Ucgen = 3;

        class Tuval
        {
            public readonly float[] D;
            public readonly System.Random R;
            public Tuval(float sure, int tohum) { D = new float[Mathf.CeilToInt(sure * Rate)]; R = new System.Random(tohum); }

            public float Sans(float a, float b) { return a + (float)R.NextDouble() * (b - a); }
            float Gurultu() { return (float)(R.NextDouble() * 2.0 - 1.0); }

            static float Zarf(float t, float sure, float atak, float sonum)
            {
                float birak = Mathf.Min(0.03f, sure * 0.25f);
                float z = atak > 0 ? Mathf.Clamp01(t / atak) : 1f;
                if (sonum > 0) z *= Mathf.Exp(-sonum * Mathf.Max(0f, t - atak));
                return z * Mathf.Clamp01((sure - t) / Mathf.Max(birak, 1e-4f));
            }

            void Ekle(float[] parca, int bas, float sure, float g, float atak, float sonum)
            {
                float tepe = 0;
                foreach (var x in parca) tepe = Mathf.Max(tepe, Mathf.Abs(x));
                if (tepe < 1e-6f) return;
                for (int i = 0; i < parca.Length && bas + i < D.Length; i++)
                    if (bas + i >= 0) D[bas + i] += parca[i] / tepe * g * Zarf(i / (float)Rate, sure, atak, sonum);
            }

            static float Dalga(int tur, float faz)
            {
                float f = faz - Mathf.Floor(faz);
                switch (tur)
                {
                    case Kare: return f < 0.5f ? 0.7f : -0.7f;
                    case Testere: return 2f * f - 1f;
                    case Ucgen: return 4f * Mathf.Abs(f - 0.5f) - 1f;
                    default: return Mathf.Sin(2f * Mathf.PI * f);
                }
            }

            // Perdesi f0'dan f1'e kayan ton; vib = perde titresimi (oran), vibHz = hizi
            public Tuval Ton(float bas, float sure, float f0, float f1, float g, int tur = Sin_, float atak = 0.005f, float sonum = 0f, float vib = 0f, float vibHz = 0f)
            {
                var p = new float[(int)(sure * Rate)];
                float faz = 0;
                for (int i = 0; i < p.Length; i++)
                {
                    float t = i / (float)Rate;
                    float hz = f0 * Mathf.Pow(f1 / f0, t / sure);
                    if (vib > 0) hz *= 1f + vib * Mathf.Sin(2f * Mathf.PI * vibHz * t);
                    faz += hz / Rate;
                    p[i] = Dalga(tur, faz);
                }
                Ekle(p, (int)(bas * Rate), sure, g, atak, sonum);
                return this;
            }

            // Suzulmus gurultu: alt0->alt1 alcak geciren kesim (Hz), ust = yuksek geciren kesim (Hz)
            public Tuval Gur(float bas, float sure, float alt0, float alt1, float g, float atak = 0.005f, float sonum = 0f, float ust = 0f)
            {
                var p = new float[(int)(sure * Rate)];
                float a = 0, b = 0, h = 0;
                float kh = ust > 0 ? 1f - Mathf.Exp(-2f * Mathf.PI * ust / Rate) : 0f;
                for (int i = 0; i < p.Length; i++)
                {
                    float hz = alt0 * Mathf.Pow(alt1 / alt0, i / (float)p.Length);
                    float k = 1f - Mathf.Exp(-2f * Mathf.PI * Mathf.Min(hz, 18000f) / Rate);
                    a += k * (Gurultu() - a);
                    b += k * (a - b);
                    float x = b;
                    if (ust > 0) { h += kh * (x - h); x -= h; }
                    p[i] = x;
                }
                Ekle(p, (int)(bas * Rate), sure, g, atak, sonum);
                return this;
            }

            // Cinlama: uyumsuz kismi tonlar (can, cam, metal)
            public Tuval Cin(float bas, float sure, float hz, float g, float sonum, params float[] oranlar)
            {
                if (oranlar == null || oranlar.Length == 0) oranlar = new[] { 1f, 2f, 3f };
                var p = new float[(int)(sure * Rate)];
                for (int i = 0; i < p.Length; i++)
                {
                    float t = i / (float)Rate, x = 0;
                    for (int k = 0; k < oranlar.Length; k++)
                        x += Mathf.Sin(2f * Mathf.PI * hz * oranlar[k] * t) * Mathf.Exp(-t * sonum * 0.4f * k) / (k + 1);
                    p[i] = x;
                }
                Ekle(p, (int)(bas * Rate), sure, g, 0.002f, sonum);
                return this;
            }

            // Girtlak sesi (hayvan, canavar): testere + hirilti, rezonans fmt0->fmt1
            public Tuval Agiz(float bas, float sure, float f0, float f1, float fmt0, float fmt1, float g, float hirilti = 0.15f, float atak = 0.03f, float sonum = 0f, float vib = 0f, float vibHz = 0f)
            {
                var p = new float[(int)(sure * Rate)];
                float faz = 0, alt = 0, bant = 0;
                for (int i = 0; i < p.Length; i++)
                {
                    float t = i / (float)Rate, o = i / (float)p.Length;
                    float hz = f0 * Mathf.Pow(f1 / f0, o);
                    if (vib > 0) hz *= 1f + vib * Mathf.Sin(2f * Mathf.PI * vibHz * t);
                    faz += hz * (1f + Gurultu() * hirilti * 0.5f) / Rate;
                    float kaynak = Dalga(Testere, faz) + Gurultu() * hirilti;
                    float fc = 2f * Mathf.Sin(Mathf.PI * Mathf.Min(Mathf.Lerp(fmt0, fmt1, o), 6500f) / Rate);
                    alt += fc * bant;
                    float ust = kaynak - alt - 0.25f * bant;
                    bant += fc * ust;
                    p[i] = bant + alt * 0.25f;
                }
                Ekle(p, (int)(bas * Rate), sure, g, atak, sonum);
                return this;
            }

            // Tum sesin siddetini dalgalandirir
            public Tuval Nabiz(float hz, float derinlik)
            {
                for (int i = 0; i < D.Length; i++)
                    D[i] *= 1f - derinlik * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * hz * i / Rate));
                return this;
            }

            public float[] Bitir(float duzey)
            {
                float tepe = 0;
                foreach (var x in D) tepe = Mathf.Max(tepe, Mathf.Abs(x));
                if (tepe > 0) for (int i = 0; i < D.Length; i++) D[i] *= duzey / tepe;
                int yum = Mathf.Min(D.Length / 4, Rate / 200);   // bas/son 5 ms yumusatma
                for (int i = 0; i < yum; i++) { float f = i / (float)yum; D[i] *= f; D[D.Length - 1 - i] *= f; }
                return D;
            }
        }

        // ---------- Tarifler ----------
        static Dictionary<string, Tarif> tarifler;
        static Dictionary<string, Tarif> Tarifler
        {
            get
            {
                if (tarifler != null) return tarifler;
                var d = new Dictionary<string, Tarif>();
                VurusSesleri(d); EsyaSesleri(d); DogaSesleri(d); HayvanSesleri(d); InsanSesleri(d);
                MuzikSesleri(d); OyunSesleri(d); BilimSesleri(d); TasitSesleri(d);
                tarifler = d;
                return d;
            }
        }

        static float Nota(int n) { return 440f * Mathf.Pow(2f, (n - 69) / 12f); }   // 60 = orta do

        static void VurusSesleri(Dictionary<string, Tarif> d)
        {
            d["patlama"] = T(2.4f, 0.9f, s => s.Gur(0, 2.4f, 5000, 90, 1f, 0.004f, 1.8f).Ton(0, 1.6f, 90, 28, 0.9f, Sin_, 0.004f, 2.2f).Gur(0, 0.12f, 12000, 4000, 0.6f, 0.001f, 25f));
            d["yumruk"] = T(0.35f, 0.9f, s => s.Ton(0, 0.2f, 160, 50, 1f, Sin_, 0.002f, 18f).Gur(0, 0.12f, 1800, 300, 0.7f, 0.001f, 30f));
            d["tokat"] = T(0.3f, 0.85f, s => s.Gur(0, 0.18f, 7000, 2500, 1f, 0.001f, 28f, 900f).Ton(0, 0.08f, 260, 120, 0.35f, Sin_, 0.001f, 30f));
            d["dusme"] = T(1.1f, 0.85f, s => s.Ton(0, 0.55f, 900, 140, 0.35f, Sin_, 0.02f).Ton(0.55f, 0.35f, 110, 38, 1f, Sin_, 0.002f, 10f).Gur(0.55f, 0.4f, 1200, 120, 0.8f, 0.002f, 10f));
            d["cam"] = T(1.4f, 0.8f, s => { s.Gur(0, 0.25f, 14000, 6000, 1f, 0.001f, 15f, 2500f); for (int i = 0; i < 22; i++) s.Cin(s.Sans(0f, 0.55f), s.Sans(0.12f, 0.6f), s.Sans(1800, 5200), s.Sans(0.1f, 0.4f), 9f, 1f, 1.51f, 2.27f); });
            d["silah"] = T(0.9f, 0.9f, s => s.Gur(0, 0.08f, 14000, 5000, 1f, 0.0005f, 20f).Gur(0, 0.8f, 3000, 200, 0.6f, 0.002f, 6f).Ton(0, 0.3f, 140, 45, 0.8f, Sin_, 0.001f, 12f));
            d["makineli"] = T(1.3f, 0.85f, s => { for (int i = 0; i < 12; i++) { float b = i * 0.095f; s.Gur(b, 0.14f, 9000, 700, 1f, 0.0005f, 28f).Ton(b, 0.08f, 150, 60, 0.6f, Sin_, 0.001f, 25f); } });
            d["kilic"] = T(1.1f, 0.75f, s => s.Gur(0, 0.12f, 9000, 3000, 0.7f, 0.002f, 20f, 1500f).Cin(0.04f, 1f, 1450, 1f, 4.5f, 1f, 1.47f, 2.09f, 2.56f, 3.4f));
            d["ok"] = T(0.7f, 0.8f, s => s.Ton(0, 0.15f, 210, 170, 0.7f, Ucgen, 0.001f, 18f).Gur(0.03f, 0.32f, 1500, 6000, 0.45f, 0.15f, 0f, 700f).Ton(0.36f, 0.2f, 130, 70, 1f, Sin_, 0.001f, 20f).Gur(0.36f, 0.08f, 2500, 800, 0.5f, 0.001f, 35f));
            d["kirbac"] = T(0.6f, 0.85f, s => s.Gur(0, 0.28f, 500, 5000, 0.35f, 0.2f, 0f, 400f).Gur(0.28f, 0.16f, 14000, 3000, 1f, 0.0005f, 28f, 800f));
            d["kaya"] = T(2f, 0.85f, s => { s.Gur(0, 2f, 220, 120, 0.8f, 0.15f, 0.8f); for (int i = 0; i < 18; i++) s.Gur(s.Sans(0f, 1.5f), 0.18f, s.Sans(500, 1800), 150, s.Sans(0.3f, 1f), 0.001f, 22f); });
            d["zincir"] = T(1.5f, 0.7f, s => { for (int i = 0; i < 26; i++) s.Cin(i * 0.05f + s.Sans(0f, 0.03f), 0.18f, s.Sans(900, 2600), s.Sans(0.2f, 1f), 28f, 1f, 1.83f, 2.71f); });
        }

        static void EsyaSesleri(Dictionary<string, Tarif> d)
        {
            d["zil"] = T(1.6f, 0.65f, s => { for (int i = 0; i < 2; i++) s.Cin(i * 0.55f, 0.9f, 660, 1f, 3f, 1f, 1.26f, 2f, 2.52f); });
            d["tiktak"] = T(3f, 0.6f, s => { for (int i = 0; i < 6; i++) s.Cin(i * 0.5f, 0.06f, i % 2 == 0 ? 2100 : 1500, 1f, 60f, 1f, 2.3f).Gur(i * 0.5f, 0.02f, 8000, 4000, 0.5f, 0.0005f, 90f); });
            d["alarm"] = T(2f, 0.55f, s => { for (int i = 0; i < 4; i++) for (int k = 0; k < 4; k++) s.Ton(i * 0.5f + k * 0.08f, 0.06f, 2000, 2000, 1f, Kare); });
            d["telefon"] = T(2.6f, 0.55f, s => { for (int i = 0; i < 2; i++) for (int k = 0; k < 16; k++) { float b = i * 1.4f + k * 0.05f; s.Cin(b, 0.07f, k % 2 == 0 ? 1050 : 1320, 1f, 10f, 1f, 2f); } });
            d["mesaj"] = T(0.5f, 0.6f, s => s.Cin(0, 0.18f, Nota(88), 0.8f, 14f, 1f, 2f).Cin(0.11f, 0.35f, Nota(93), 1f, 10f, 1f, 2f));
            d["bildirim"] = T(0.8f, 0.6f, s => s.Cin(0, 0.35f, Nota(81), 0.8f, 8f, 1f, 2f, 3f).Cin(0.16f, 0.6f, Nota(86), 1f, 6f, 1f, 2f, 3f));
            d["klavye"] = T(2f, 0.55f, s => { float b = 0; while (b < 1.9f) { s.Gur(b, 0.035f, 5000, 2000, s.Sans(0.5f, 1f), 0.0005f, 90f, 600f).Ton(b, 0.03f, 380, 250, 0.2f, Sin_, 0.0005f, 90f); b += s.Sans(0.07f, 0.17f); } });
            d["dugme"] = T(0.18f, 0.6f, s => s.Gur(0, 0.03f, 6000, 2500, 1f, 0.0005f, 90f).Ton(0, 0.04f, 700, 500, 0.5f, Sin_, 0.0005f, 70f).Gur(0.09f, 0.03f, 4000, 1500, 0.6f, 0.0005f, 90f));
            d["kapi_carpma"] = T(0.9f, 0.9f, s => s.Ton(0, 0.5f, 110, 55, 1f, Sin_, 0.002f, 9f).Gur(0, 0.5f, 2200, 150, 0.9f, 0.001f, 12f).Cin(0.01f, 0.4f, 190, 0.35f, 10f, 1f, 2.4f, 3.9f).Gur(0.09f, 0.05f, 5000, 2000, 0.35f, 0.001f, 60f));
            d["kapi_vur"] = T(1.1f, 0.8f, s => { for (int i = 0; i < 3; i++) s.Cin(i * 0.28f, 0.2f, 210, 1f, 24f, 1f, 2.3f, 3.7f).Gur(i * 0.28f, 0.05f, 2500, 500, 0.6f, 0.0005f, 50f); });
            d["anahtar"] = T(1.1f, 0.55f, s => { for (int i = 0; i < 16; i++) s.Cin(s.Sans(0f, 0.85f), 0.16f, s.Sans(2600, 5400), s.Sans(0.3f, 1f), 26f, 1f, 1.41f); });
            d["para"] = T(0.9f, 0.65f, s => s.Cin(0, 0.12f, 1975, 0.7f, 20f, 1f, 2.76f).Cin(0.08f, 0.8f, 2637, 1f, 5f, 1f, 2.76f, 5.4f));
            d["kagit"] = T(0.9f, 0.5f, s => { for (int i = 0; i < 40; i++) s.Gur(s.Sans(0f, 0.8f), s.Sans(0.02f, 0.08f), s.Sans(4000, 12000), 3000, s.Sans(0.15f, 1f), 0.002f, 20f, 1800f); });
            d["deklansor"] = T(0.35f, 0.7f, s => s.Gur(0, 0.05f, 9000, 3000, 1f, 0.0005f, 60f, 700f).Gur(0.1f, 0.09f, 7000, 1500, 0.8f, 0.001f, 35f, 400f));
            d["su_dokme"] = T(2.2f, 0.55f, s => { s.Gur(0, 2.2f, 2500, 3500, 0.5f, 0.1f, 0f, 500f); for (int i = 0; i < 60; i++) { float f = s.Sans(500, 1500); s.Ton(s.Sans(0f, 2.1f), 0.05f, f, f * 1.6f, s.Sans(0.1f, 0.45f), Sin_, 0.004f, 30f); } });
            d["kaynama"] = T(2.5f, 0.55f, s => { s.Gur(0, 2.5f, 500, 500, 0.3f, 0.2f); for (int i = 0; i < 70; i++) { float f = s.Sans(160, 600); s.Ton(s.Sans(0f, 2.4f), 0.07f, f, f * 2f, s.Sans(0.2f, 0.7f), Sin_, 0.005f, 20f); } });
        }

        static void DogaSesleri(Dictionary<string, Tarif> d)
        {
            d["ruzgar"] = T(4f, 0.45f, s => s.Gur(0, 4f, 300, 700, 0.8f, 0.8f).Gur(0.8f, 2.4f, 900, 350, 0.6f, 1.1f).Nabiz(0.4f, 0.45f));
            d["firtina"] = T(4.5f, 0.65f, s => s.Gur(0, 4.5f, 500, 1200, 0.8f, 0.5f).Gur(0, 4.5f, 8000, 8000, 0.3f, 0.5f, 0f, 2000f).Gur(1.6f, 2.4f, 300, 60, 1f, 0.03f, 1.4f).Nabiz(0.55f, 0.3f));
            d["dalga"] = T(4.5f, 0.55f, s => s.Gur(0, 2.4f, 250, 2800, 1f, 2f).Gur(2f, 2.5f, 3000, 300, 0.9f, 0.2f, 0.9f));
            d["damla"] = T(1.6f, 0.6f, s => { for (int i = 0; i < 3; i++) { float b = i * 0.55f; s.Ton(b, 0.09f, 700, 1800, 1f, Sin_, 0.003f, 22f).Ton(b + 0.12f, 0.12f, 900, 1500, 0.2f, Sin_, 0.003f, 25f); } });
            d["ates"] = T(3.5f, 0.55f, s => { s.Gur(0, 3.5f, 350, 350, 0.45f, 0.4f); for (int i = 0; i < 80; i++) s.Gur(s.Sans(0f, 3.4f), s.Sans(0.004f, 0.03f), s.Sans(2000, 9000), 1500, s.Sans(0.15f, 1f), 0.0005f, 60f, 600f); s.Nabiz(1.3f, 0.2f); });
            d["simsek"] = T(2.8f, 0.9f, s => s.Gur(0, 0.15f, 14000, 5000, 1f, 0.001f, 14f, 900f).Gur(0.06f, 0.3f, 9000, 1000, 0.8f, 0.002f, 10f).Gur(0.15f, 2.6f, 700, 60, 0.9f, 0.05f, 1.3f).Ton(0.1f, 1.4f, 70, 35, 0.5f, Sin_, 0.02f, 1.8f));
            d["deprem"] = T(3.8f, 0.85f, s => { s.Gur(0, 3.8f, 130, 90, 1f, 0.6f).Ton(0, 3.8f, 36, 30, 0.7f, Sin_, 0.6f, 0f, 0.15f, 5f); for (int i = 0; i < 24; i++) s.Gur(s.Sans(0.4f, 3.4f), 0.15f, s.Sans(400, 1800), 150, s.Sans(0.15f, 0.55f), 0.001f, 25f); s.Nabiz(6f, 0.25f); });
            d["kus"] = T(2f, 0.55f, s => { float[] bs = { 0.05f, 0.19f, 0.33f, 0.8f, 0.92f, 1.4f, 1.52f, 1.66f }; foreach (var b in bs) { float f = s.Sans(2600, 3800); s.Ton(b, 0.1f, f, f * s.Sans(1.2f, 1.6f), s.Sans(0.5f, 1f), Sin_, 0.01f, 8f, 0.08f, 35f); } });
            d["karga"] = T(1.5f, 0.7f, s => { for (int i = 0; i < 3; i++) s.Agiz(i * 0.45f, 0.28f, 520, 430, 1500, 1100, 1f, 0.5f, 0.02f, 2f); });
            d["baykus"] = T(1.8f, 0.6f, s => s.Ton(0, 0.3f, 380, 350, 0.8f, Sin_, 0.07f).Ton(0.5f, 0.2f, 370, 360, 0.6f, Sin_, 0.05f).Ton(0.85f, 0.8f, 390, 330, 1f, Sin_, 0.1f, 1.2f, 0.015f, 7f));
            d["cirircir"] = T(3f, 0.35f, s => { for (int i = 0; i < 6; i++) for (int k = 0; k < 4; k++) s.Ton(i * 0.5f + k * 0.045f, 0.03f, 4300, 4300, 1f, Sin_, 0.006f); });
            d["ari"] = T(2.2f, 0.4f, s => s.Ton(0, 2.2f, 190, 215, 0.7f, Testere, 0.4f, 0f, 0.06f, 5f).Ton(0, 2.2f, 196, 208, 0.5f, Testere, 0.4f, 0f, 0.05f, 7f).Nabiz(1.1f, 0.4f));
        }

        static void HayvanSesleri(Dictionary<string, Tarif> d)
        {
            d["kurt"] = T(3f, 0.65f, s => s.Ton(0, 0.7f, 320, 620, 0.9f, Sin_, 0.2f).Ton(0.68f, 2.2f, 620, 430, 1f, Sin_, 0.03f, 0.4f, 0.02f, 5.5f).Ton(0.68f, 2.2f, 1240, 860, 0.25f, Sin_, 0.05f, 0.6f, 0.02f, 5.5f));
            d["kopek"] = T(1.1f, 0.8f, s => { for (int i = 0; i < 2; i++) s.Agiz(i * 0.45f, 0.22f, 330, 210, 900, 600, 1f, 0.45f, 0.01f, 6f); });
            d["kedi"] = T(1.1f, 0.65f, s => s.Agiz(0, 0.95f, 520, 400, 2600, 800, 1f, 0.05f, 0.09f, 0.6f, 0.03f, 6f).Ton(0, 0.9f, 620, 470, 0.35f, Sin_, 0.15f, 1f));
            d["aslan"] = T(2.6f, 0.9f, s => s.Agiz(0, 2.4f, 90, 55, 420, 250, 1f, 0.7f, 0.35f, 0.5f, 0.12f, 11f).Gur(0.1f, 2.3f, 900, 200, 0.4f, 0.4f, 0.6f));
            d["at"] = T(1.8f, 0.7f, s => s.Agiz(0, 1.5f, 900, 260, 2400, 900, 1f, 0.25f, 0.08f, 0.7f, 0.13f, 13f));
            d["nal"] = T(2.4f, 0.7f, s => { for (int i = 0; i < 6; i++) { float b = i * 0.38f; float[] k = { 0f, 0.1f, 0.19f }; foreach (var o in k) s.Cin(b + o, 0.08f, s.Sans(600, 900), s.Sans(0.6f, 1f), 45f, 1f, 1.9f).Gur(b + o, 0.04f, 2500, 600, 0.5f, 0.0005f, 60f); } });
            d["inek"] = T(2f, 0.7f, s => s.Agiz(0, 1.8f, 120, 100, 300, 560, 1f, 0.08f, 0.25f, 0.2f, 0.01f, 4f));
            d["koyun"] = T(1.3f, 0.65f, s => s.Agiz(0, 1.1f, 260, 230, 1000, 850, 1f, 0.15f, 0.05f, 0.5f, 0.07f, 9f).Nabiz(9f, 0.5f));
            d["horoz"] = T(2.1f, 0.7f, s => s.Agiz(0, 0.16f, 520, 640, 1300, 1600, 0.8f, 0.2f, 0.02f).Agiz(0.22f, 0.16f, 600, 700, 1500, 1700, 0.8f, 0.2f, 0.02f).Agiz(0.44f, 0.2f, 760, 880, 1800, 2100, 0.9f, 0.2f, 0.02f).Agiz(0.72f, 1.15f, 920, 520, 2300, 1100, 1f, 0.2f, 0.04f, 0.8f, 0.025f, 7f));
            d["yilan"] = T(1.8f, 0.45f, s => s.Gur(0, 1.8f, 10000, 7000, 1f, 0.25f, 0.5f, 3500f).Nabiz(14f, 0.15f));
            d["kurbaga"] = T(1.4f, 0.6f, s => { float[] bs = { 0f, 0.18f, 0.7f, 0.88f }; foreach (var b in bs) { for (int k = 0; k < 6; k++) s.Agiz(b + k * 0.022f, 0.02f, 180, 170, 800, 700, 1f, 0.2f, 0.002f, 20f); } });
            d["ejderha"] = T(3.2f, 0.9f, s => s.Agiz(0, 1.1f, 380, 150, 1400, 700, 0.8f, 0.6f, 0.15f, 0.4f, 0.1f, 17f).Agiz(0.5f, 2.6f, 75, 45, 350, 220, 1f, 0.8f, 0.3f, 0.5f, 0.15f, 9f).Gur(0.9f, 2.2f, 2500, 300, 0.55f, 0.3f, 0.7f));
            d["canavar"] = T(2.4f, 0.85f, s => s.Agiz(0, 2.2f, 62, 48, 260, 420, 1f, 0.9f, 0.25f, 0.4f, 0.18f, 6f).Ton(0, 2.2f, 41, 33, 0.5f, Testere, 0.3f, 0.6f).Nabiz(13f, 0.35f));
        }

        static void InsanSesleri(Dictionary<string, Tarif> d)
        {
            d["alkis"] = T(3f, 0.65f, s => { for (int i = 0; i < 260; i++) { float b = s.Sans(0f, 2.9f); float z = Mathf.Clamp01(b * 2.5f) * Mathf.Clamp01((3f - b) * 1.4f); s.Gur(b, 0.03f, s.Sans(2500, 6000), 1200, s.Sans(0.25f, 1f) * z, 0.0005f, 80f, 600f); } });
            d["kalabalik"] = T(4f, 0.4f, s => { s.Gur(0, 4f, 600, 600, 0.35f, 0.5f, 0f, 150f); for (int i = 0; i < 110; i++) { float f = s.Sans(100, 260); s.Agiz(s.Sans(0f, 3.7f), s.Sans(0.1f, 0.3f), f, f * s.Sans(0.85f, 1.15f), s.Sans(350, 1100), s.Sans(350, 1400), s.Sans(0.1f, 0.4f), 0.1f, 0.04f); } });
            d["adim"] = T(2.4f, 0.65f, s => { for (int i = 0; i < 4; i++) { float b = i * 0.6f; s.Ton(b, 0.1f, 110, 60, 0.8f, Sin_, 0.003f, 28f).Gur(b, 0.12f, 1500, 400, 0.7f, 0.003f, 28f).Gur(b + 0.07f, 0.08f, 3000, 1200, 0.25f, 0.01f, 30f); } });
            d["kosma"] = T(2.2f, 0.7f, s => { for (int i = 0; i < 8; i++) { float b = i * 0.27f; s.Ton(b, 0.09f, 120, 62, 0.9f, Sin_, 0.002f, 30f).Gur(b, 0.1f, 2000, 450, 0.7f, 0.002f, 32f); } });
            d["nefes"] = T(3.2f, 0.45f, s => { for (int i = 0; i < 2; i++) { float b = i * 1.6f; s.Gur(b, 0.65f, 900, 2200, 0.7f, 0.45f, 0f, 500f).Gur(b + 0.75f, 0.75f, 1600, 500, 1f, 0.2f, 1.5f, 250f); } });
            d["kalp_hizli"] = T(2.7f, 0.85f, s => { for (int i = 0; i < 6; i++) { float b = i * 0.44f; s.Ton(b, 0.14f, 70, 45, 1f, Sin_, 0.006f, 18f).Ton(b + 0.17f, 0.14f, 62, 42, 0.7f, Sin_, 0.006f, 18f); } });
        }

        static void Dizi(Tuval s, float ara, float uzun, float g, int tur, float sonum, params int[] n)
        {
            for (int i = 0; i < n.Length; i++) s.Ton(i * ara, uzun, Nota(n[i]), Nota(n[i]), g, tur, 0.01f, sonum);
        }

        static void Akor(Tuval s, float bas, float sure, float g, int tur, float atak, float sonum, params int[] n)
        {
            foreach (var x in n) s.Ton(bas, sure, Nota(x), Nota(x), g, tur, atak, sonum, 0.004f, 5f);
        }

        static void MuzikSesleri(Dictionary<string, Tarif> d)
        {
            d["zafer"] = T(2.2f, 0.65f, s => { Dizi(s, 0.15f, 0.2f, 0.6f, Ucgen, 3f, 60, 64, 67); Akor(s, 0.45f, 1.7f, 0.5f, Ucgen, 0.02f, 1.2f, 72, 76, 79); });
            d["kayip"] = T(2.2f, 0.6f, s => { Dizi(s, 0.35f, 0.4f, 0.7f, Ucgen, 2f, 67, 66, 65); s.Ton(1.05f, 1.1f, Nota(64), Nota(62), 0.8f, Ucgen, 0.02f, 1f, 0.025f, 6f); });
            d["gerilim"] = T(4f, 0.55f, s => { s.Ton(0, 4f, Nota(38), Nota(38), 0.7f, Testere, 1.5f).Ton(0, 4f, Nota(39), Nota(39), 0.5f, Testere, 2.2f).Ton(1f, 3f, Nota(74), Nota(76), 0.25f, Sin_, 2.5f, 0f, 0.012f, 7f).Nabiz(2f, 0.35f); });
            d["korku"] = T(3f, 0.65f, s => { Akor(s, 0, 2.9f, 0.4f, Testere, 0.02f, 1.1f, 73, 74, 79, 80); s.Ton(0, 2.5f, 55, 38, 0.8f, Sin_, 0.01f, 1f).Gur(0, 0.4f, 8000, 2000, 0.4f, 0.002f, 9f); });
            d["surpriz"] = T(1f, 0.65f, s => { Dizi(s, 0.06f, 0.12f, 0.5f, Ucgen, 8f, 67, 71, 74); Akor(s, 0.2f, 0.75f, 0.5f, Ucgen, 0.005f, 4f, 79, 83, 86); });
            d["davul"] = T(2.1f, 0.85f, s => { float[] bs = { 0f, 0.35f, 0.7f, 0.87f, 1.05f, 1.4f }; foreach (var b in bs) s.Ton(b, 0.5f, 130, 65, 1f, Sin_, 0.002f, 9f).Gur(b, 0.12f, 1200, 200, 0.5f, 0.001f, 28f); });
            d["gong"] = T(4f, 0.75f, s => s.Cin(0, 4f, 98, 1f, 0.9f, 1f, 1.48f, 2.04f, 2.68f, 3.17f, 4.22f).Gur(0, 0.2f, 1500, 300, 0.35f, 0.002f, 18f));
            d["can"] = T(3.5f, 0.65f, s => { for (int i = 0; i < 2; i++) s.Cin(i * 1.3f, 2.2f, 392, 1f, 1.6f, 1f, 2f, 2.4f, 3f, 4.2f, 5.4f); });
            d["dram"] = T(2.8f, 0.8f, s => { float[] bs = { 0f, 0.4f, 0.8f }; for (int i = 0; i < 3; i++) { float sure = i == 2 ? 1.9f : 0.3f; Akor(s, bs[i], sure, 0.35f, Testere, 0.02f, i == 2 ? 1.1f : 3f, i == 2 ? 38 : 45, i == 2 ? 41 : 48, i == 2 ? 44 : 52); s.Ton(bs[i], Mathf.Min(sure, 0.5f), 70, 40, 0.8f, Sin_, 0.004f, 6f); } });
            d["mutlu"] = T(2.4f, 0.6f, s => Dizi(s, 0.17f, 0.3f, 0.6f, Ucgen, 5f, 72, 76, 79, 76, 77, 81, 84, 88, 84));
            d["huzun"] = T(4f, 0.55f, s => { Akor(s, 0, 2f, 0.3f, Ucgen, 0.3f, 0.5f, 57, 60, 64); Akor(s, 1.9f, 2.1f, 0.3f, Ucgen, 0.3f, 0.6f, 53, 57, 60); s.Ton(0.2f, 1.6f, Nota(76), Nota(76), 0.35f, Sin_, 0.15f, 0.5f, 0.012f, 5f).Ton(1.9f, 2f, Nota(72), Nota(72), 0.35f, Sin_, 0.15f, 0.7f, 0.012f, 5f); });
            d["romantik"] = T(4f, 0.5f, s => { Akor(s, 0, 4f, 0.2f, Sin_, 0.8f, 0.2f, 53, 57, 60, 64); Dizi(s, 0.28f, 0.9f, 0.35f, Ucgen, 2.5f, 65, 69, 72, 76, 77, 76, 72, 69, 72, 77, 81); });
            d["komik"] = T(1.8f, 0.65f, s => { float[] bs = { 0f, 0.22f, 0.44f }; int[] n = { 55, 50, 55 }; for (int i = 0; i < 3; i++) s.Ton(bs[i], 0.18f, Nota(n[i]), Nota(n[i]) * 0.97f, 0.8f, Kare, 0.01f, 6f); s.Ton(0.7f, 0.9f, Nota(48), Nota(60), 0.8f, Kare, 0.02f, 1.5f, 0.07f, 9f); });
            d["tada"] = T(1.8f, 0.65f, s => { Akor(s, 0, 0.18f, 0.4f, Testere, 0.01f, 4f, 60, 67, 72); Akor(s, 0.24f, 1.5f, 0.4f, Testere, 0.02f, 1.4f, 65, 72, 77, 81); });
        }

        static void OyunSesleri(Dictionary<string, Tarif> d)
        {
            d["pop"] = T(0.16f, 0.7f, s => s.Ton(0, 0.1f, 350, 1100, 1f, Sin_, 0.003f, 22f));
            d["boing"] = T(1f, 0.65f, s => s.Ton(0, 0.95f, 150, 320, 1f, Ucgen, 0.005f, 3f, 0.28f, 14f));
            d["zip"] = T(0.4f, 0.65f, s => s.Ton(0, 0.35f, 220, 900, 0.9f, Ucgen, 0.005f, 3f));
            d["onay"] = T(0.55f, 0.55f, s => Dizi(s, 0.11f, 0.3f, 0.8f, Sin_, 7f, 76, 83));
            d["hata"] = T(0.65f, 0.55f, s => { for (int i = 0; i < 2; i++) s.Ton(i * 0.24f, 0.2f, 150, 140, 1f, Kare, 0.005f); });
            d["seviye"] = T(1.4f, 0.55f, s => { Dizi(s, 0.09f, 0.14f, 0.6f, Kare, 6f, 60, 64, 67, 72, 76, 79); Akor(s, 0.55f, 0.8f, 0.3f, Kare, 0.01f, 3f, 84, 88); });
            d["bip"] = T(0.3f, 0.5f, s => s.Ton(0, 0.25f, 1000, 1000, 1f));
            d["geri_sayim"] = T(3.8f, 0.55f, s => { for (int i = 0; i < 3; i++) s.Ton(i, 0.18f, 880, 880, 0.8f); s.Ton(3f, 0.7f, 1760, 1760, 1f); });
        }

        static void BilimSesleri(Dictionary<string, Tarif> d)
        {
            d["lazer"] = T(0.7f, 0.6f, s => { for (int i = 0; i < 2; i++) s.Ton(i * 0.28f, 0.25f, 2400, 220, 1f, Testere, 0.002f, 6f); });
            d["robot"] = T(1.8f, 0.5f, s => { float b = 0; while (b < 1.65f) { float f = s.Sans(180, 900); s.Ton(b, 0.1f, f, f, 1f, Kare, 0.004f, 0f, 0.15f, 45f); b += 0.13f; } });
            d["uzay"] = T(4.5f, 0.4f, s => s.Ton(0, 4.5f, 110, 112, 0.6f, Sin_, 1f).Ton(0, 4.5f, 165, 163, 0.4f, Sin_, 1.5f, 0f, 0.01f, 0.4f).Ton(0.5f, 4f, 880, 660, 0.15f, Sin_, 1.5f, 0f, 0.03f, 0.7f).Gur(0, 4.5f, 400, 900, 0.25f, 1.5f).Nabiz(0.35f, 0.35f));
            d["isinlanma"] = T(1.8f, 0.6f, s => { s.Ton(0, 1.7f, 200, 2600, 0.7f, Sin_, 0.2f, 0f, 0.12f, 22f).Gur(0, 1.7f, 600, 9000, 0.4f, 1f, 0f, 400f); for (int i = 0; i < 24; i++) s.Cin(s.Sans(0.5f, 1.6f), 0.15f, s.Sans(2000, 5000), 0.2f, 25f, 1f, 2f); });
            d["kalkan"] = T(1.5f, 0.6f, s => s.Ton(0, 1.4f, 110, 440, 0.7f, Testere, 0.25f, 1f).Ton(0, 1.4f, 112, 446, 0.7f, Testere, 0.25f, 1f).Cin(0.25f, 1.2f, 880, 0.4f, 3f, 1f, 1.5f, 2f));
            d["iyilesme"] = T(2f, 0.55f, s => { int[] n = { 72, 76, 79, 84, 88, 91 }; for (int i = 0; i < n.Length; i++) s.Cin(i * 0.16f, 1f, Nota(n[i]), 0.6f, 4f, 1f, 2f); s.Ton(0, 2f, Nota(60), Nota(72), 0.25f, Sin_, 0.5f); });
            d["sihir"] = T(1.8f, 0.55f, s => { for (int i = 0; i < 26; i++) s.Cin(i * 0.05f + s.Sans(0f, 0.02f), 0.4f, 1200 + i * 110 + s.Sans(0f, 300), s.Sans(0.3f, 0.8f), 9f, 1f, 2f); s.Gur(0, 1.6f, 3000, 12000, 0.15f, 0.6f, 0f, 3000f); });
            d["hayalet"] = T(3.5f, 0.45f, s => s.Ton(0, 1.8f, 300, 520, 0.8f, Sin_, 0.7f, 0f, 0.05f, 5f).Ton(1.6f, 1.9f, 520, 240, 0.8f, Sin_, 0.3f, 0f, 0.07f, 4.5f).Gur(0, 3.5f, 800, 1300, 0.3f, 1.2f, 0f, 300f));
            d["roket"] = T(3.5f, 0.75f, s => s.Gur(0, 3.5f, 300, 3500, 1f, 1.4f).Ton(0, 3.5f, 45, 110, 0.5f, Testere, 1f).Nabiz(17f, 0.15f));
            d["enerji"] = T(2.2f, 0.65f, s => s.Ton(0, 1.5f, 80, 900, 0.7f, Testere, 0.5f, 0f, 0.1f, 30f).Ton(0, 1.5f, 160, 1800, 0.3f, Sin_, 0.8f).Gur(1.45f, 0.75f, 9000, 300, 1f, 0.002f, 5f));
            d["siren"] = T(4f, 0.45f, s => { for (int i = 0; i < 4; i++) s.Ton(i, 0.5f, 650, 900, 1f, Ucgen, 0.01f).Ton(i + 0.5f, 0.5f, 900, 650, 1f, Ucgen, 0.01f); });
        }

        static void TasitSesleri(Dictionary<string, Tarif> d)
        {
            d["motor"] = T(3f, 0.55f, s => s.Ton(0, 1.2f, 40, 85, 0.8f, Testere, 0.1f).Ton(1.17f, 1.83f, 85, 60, 0.8f, Testere, 0.05f).Gur(0, 3f, 400, 600, 0.45f, 0.1f).Nabiz(23f, 0.4f));
            d["tren"] = T(4f, 0.6f, s => { s.Gur(0, 4f, 500, 500, 0.45f, 0.6f); for (int i = 0; i < 5; i++) { float b = i * 0.75f; s.Gur(b, 0.1f, 2500, 400, 1f, 0.001f, 30f).Gur(b + 0.16f, 0.1f, 2000, 350, 0.8f, 0.001f, 30f); } Akor(s, 1.2f, 1.2f, 0.2f, Kare, 0.1f, 0.4f, 62, 69, 72); });
            d["ucak"] = T(4.5f, 0.55f, s => s.Gur(0, 2.4f, 200, 2600, 1f, 2f).Gur(2.2f, 2.3f, 2600, 250, 1f, 0.2f, 0.8f).Ton(0, 4.5f, 320, 170, 0.15f, Testere, 2f));
            d["helikopter"] = T(4f, 0.6f, s => s.Gur(0, 4f, 700, 700, 1f, 0.6f).Ton(0, 4f, 55, 55, 0.6f, Testere, 0.6f).Nabiz(11f, 0.85f));
            d["gemi"] = T(3.2f, 0.65f, s => Akor(s, 0, 3f, 0.4f, Testere, 0.25f, 0.35f, 36, 43, 48));
            d["fren"] = T(1.5f, 0.55f, s => s.Ton(0, 1.4f, 1900, 1500, 0.6f, Testere, 0.03f, 0.9f, 0.012f, 31f).Ton(0, 1.4f, 2350, 1850, 0.4f, Sin_, 0.05f, 1f, 0.015f, 27f).Gur(0, 1.4f, 6000, 2500, 0.4f, 0.05f, 1f, 1200f));
        }
    }
}
