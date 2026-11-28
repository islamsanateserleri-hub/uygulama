using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Atmosfer ve ekran efektleri: mum titremesi, isik huzmesi ve toz, yagmur, buyu isigi, far, flas,
    // dusunme karartmasi, geri sarma (VHS), kenar karartma ve film greni.
    public class FxLayer : MonoBehaviour
    {
        RectTransform rt;
        Image warm, tint, dim, flash, vignette, beam, candle, flame, glow;
        RawImage grain, scan;
        RectTransform dustRoot, rainRoot, magicRoot, lightsRoot;
        readonly List<RectTransform> dust = new List<RectTransform>();
        readonly List<RectTransform> rain = new List<RectTransform>();
        readonly List<RectTransform> magic = new List<RectTransform>();
        bool mum, toz, yagmur, buyu, gunes;
        float dimTarget, dimNow, flashA;
        public bool Rewinding;
        public bool Donuk;              // dondur: komutu - zerreler, yagmur, gren durur
        float grenAlpha = 0.07f;
        Image vinyetEk;
        RectTransform sisRoot;
        readonly List<RectTransform> sisler = new List<RectTransform>();
        bool sis;
        public bool Raining { get { return yagmur; } }

        const float W = 1080f, H = 1920f;

        public static FxLayer Create(Transform parent)
        {
            var r = Gfx.Rect("Efektler", parent);
            Gfx.Stretch(r);
            var f = r.gameObject.AddComponent<FxLayer>();
            f.rt = r;
            f.Build();
            return f;
        }

        Image Full(string name, Sprite s, Color c)
        {
            var img = Gfx.Img(name, rt, s, c, Vector2.zero, Vector2.zero);
            Gfx.Stretch(img.rectTransform);
            return img;
        }

        RectTransform Layer(string name)
        {
            var r = Gfx.Rect(name, rt);
            Gfx.Stretch(r);
            return r;
        }

        void Build()
        {
            tint = Full("Renk", null, new Color(0, 0, 0, 0));

            beam = Gfx.Img("Huzme", rt, Gfx.Beam, new Color(1f, 0.93f, 0.75f, 0f), new Vector2(620, 2300), new Vector2(-260, 420));
            beam.rectTransform.localEulerAngles = new Vector3(0, 0, 28);

            dustRoot = Layer("Toz");
            for (int i = 0; i < 46; i++)
            {
                float sz = Random.Range(6f, 16f);
                var d = Gfx.Img("zerre", dustRoot, Gfx.SoftDot, new Color(1f, 0.95f, 0.8f, 0f), new Vector2(sz, sz),
                    new Vector2(Random.Range(-W / 2, W / 2), Random.Range(-H / 2, H / 2)));
                dust.Add(d.rectTransform);
            }

            rainRoot = Layer("Yagmur");
            for (int i = 0; i < 90; i++)
            {
                var d = Gfx.Img("damla", rainRoot, null, new Color(0.75f, 0.85f, 1f, 0f), new Vector2(3, Random.Range(40f, 90f)),
                    new Vector2(Random.Range(-W / 2, W / 2), Random.Range(-H / 2, H / 2)));
                d.rectTransform.localEulerAngles = new Vector3(0, 0, -12);
                rain.Add(d.rectTransform);
            }

            sisRoot = Layer("Sis");
            for (int i = 0; i < 9; i++)
            {
                var d = Gfx.Img("bulut", sisRoot, Gfx.SoftDot, new Color(0.85f, 0.88f, 0.92f, 0f),
                    new Vector2(Random.Range(900f, 1400f), Random.Range(380f, 620f)),
                    new Vector2(Random.Range(-W / 2, W / 2), -H / 2 + 120f + i * 150f));
                sisler.Add(d.rectTransform);
            }

            magicRoot = Layer("Buyu");
            glow = Gfx.Img("BuyuIsik", magicRoot, Gfx.SoftDot, new Color(0.45f, 1f, 0.6f, 0f), new Vector2(1100, 1100), new Vector2(0, 100));
            for (int i = 0; i < 40; i++)
            {
                float sz = Random.Range(10f, 26f);
                var c = Random.value < 0.6f ? new Color(0.5f, 1f, 0.6f, 0f) : new Color(1f, 0.9f, 0.45f, 0f);
                var d = Gfx.Img("isik", magicRoot, Gfx.SoftDot, c, new Vector2(sz, sz),
                    new Vector2(Random.Range(-450f, 450f), Random.Range(-700f, 500f)));
                magic.Add(d.rectTransform);
            }

            candle = Gfx.Img("Mum", rt, Gfx.RoundRect, new Color(0.96f, 0.92f, 0.82f, 0f), new Vector2(46, 120), new Vector2(430, -330));
            Gfx.AddOutline(candle, 3);
            flame = Gfx.Img("Alev", rt, Gfx.SoftDot, new Color(1f, 0.75f, 0.3f, 0f), new Vector2(70, 110), new Vector2(430, -235));
            warm = Full("Sicaklik", null, new Color(1f, 0.6f, 0.25f, 0f));

            lightsRoot = Layer("Farlar");

            dim = Full("Karartma", null, new Color(0, 0, 0, 0));
            vignette = Full("KenarKarartma", Gfx.Vignette, new Color(0, 0, 0, 0.85f));
            vinyetEk = Full("KenarKarartmaEk", Gfx.Vignette, new Color(0, 0, 0, 0f));

            grain = Gfx.Rect("Gren", rt).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(grain.rectTransform);
            grain.texture = Gfx.Noise; grain.color = new Color(1, 1, 1, 0.07f); grain.raycastTarget = false;
            grain.uvRect = new Rect(0, 0, W / 256f, H / 256f);

            scan = Gfx.Rect("Tarama", rt).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(scan.rectTransform);
            scan.texture = Gfx.Scanlines; scan.color = new Color(1, 1, 1, 0); scan.raycastTarget = false;
            scan.uvRect = new Rect(0, 0, W / 4f, H / 8f / 2f);

            flash = Full("Flas", null, new Color(1, 1, 1, 0));
        }

        // "mum, toz" / "yagmur" / "yok"
        public void SetAtmosphere(string csv)
        {
            mum = toz = yagmur = buyu = gunes = sis = false;
            foreach (var raw in (csv ?? "").Split(','))
            {
                switch (Util.Norm(raw))
                {
                    case "mum": mum = true; break;
                    case "toz": toz = true; break;
                    case "yagmur": yagmur = true; break;
                    case "buyu": buyu = true; break;
                    case "gunes": gunes = true; toz = true; break;
                    case "sis": sis = true; break;
                }
            }
        }

        public void AddAtmosphere(string csv)
        {
            bool m = mum, t = toz, y = yagmur, b = buyu, g = gunes, s = sis;
            SetAtmosphere(csv);
            mum |= m; toz |= t; yagmur |= y; buyu |= b; gunes |= g; sis |= s;
        }

        public void Dim(float a) { dimTarget = a; }

        // gren: / vinyet:  yok | az | normal | cok
        public void Gren(string d) { grenAlpha = d == "yok" ? 0f : d == "az" ? 0.04f : d == "cok" ? 0.22f : 0.07f; }
        public void Vinyet(string d)
        {
            vignette.color = new Color(0, 0, 0, d == "yok" ? 0f : d == "az" ? 0.45f : d == "cok" ? 1f : 0.85f);
            vinyetEk.color = new Color(0, 0, 0, d == "cok" ? 0.8f : 0f);
        }
        public void Flash(float strength = 1f) { flashA = Mathf.Max(flashA, strength); }

        // Kamyon farlari: iki isik buyuyerek yaklasir, sonunda beyaz parlama
        public IEnumerator Headlights(float dur)
        {
            var l = Gfx.Img("FarSol", lightsRoot, Gfx.SoftDot, new Color(1f, 0.97f, 0.85f, 0f), new Vector2(60, 60), new Vector2(-70, 60));
            var r = Gfx.Img("FarSag", lightsRoot, Gfx.SoftDot, new Color(1f, 0.97f, 0.85f, 0f), new Vector2(60, 60), new Vector2(70, 60));
            var haze = Gfx.Img("FarPus", lightsRoot, null, new Color(1f, 0.97f, 0.88f, 0f), Vector2.zero, Vector2.zero);
            Gfx.Stretch(haze.rectTransform);
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / dur);
                float e = p * p * p;
                float size = Mathf.Lerp(60f, 1500f, e);
                float gap = Mathf.Lerp(70f, 520f, e);
                l.rectTransform.sizeDelta = r.rectTransform.sizeDelta = new Vector2(size, size);
                l.rectTransform.anchoredPosition = new Vector2(-gap, Mathf.Lerp(60f, -80f, e));
                r.rectTransform.anchoredPosition = new Vector2(gap, Mathf.Lerp(60f, -80f, e));
                var c = new Color(1f, 0.97f, 0.85f, Mathf.Clamp01(p * 3f));
                l.color = r.color = c;
                haze.color = new Color(1f, 0.97f, 0.88f, e * 0.6f);
                yield return null;
            }
            Flash(1f);
            Destroy(l.gameObject); Destroy(r.gameObject); Destroy(haze.gameObject);
        }

        void Update()
        {
            float dt = Time.deltaTime, t = Time.time;
            float k = 1f - Mathf.Exp(-dt * 3f);

            // film greni her kare kayar
            grain.color = new Color(1, 1, 1, grenAlpha);
            if (Donuk)
            {
                // zaman durdu: yalniz flas soner
                flashA = Mathf.MoveTowards(flashA, 0f, dt * 2.2f);
                flash.color = new Color(1, 1, 1, flashA);
                return;
            }
            var uv = grain.uvRect; uv.x = Random.value; uv.y = Random.value; grain.uvRect = uv;

            // sis
            for (int i = 0; i < sisler.Count; i++)
            {
                var d = sisler[i];
                var p = d.anchoredPosition;
                p.x += (18f + (i % 3) * 14f) * ((i % 2) * 2 - 1) * dt;
                if (p.x > W) p.x = -W; else if (p.x < -W) p.x = W;
                d.anchoredPosition = p;
                var img = d.GetComponent<Image>();
                img.color = Gfx.A(img.color, Mathf.Lerp(img.color.a, sis ? 0.2f : 0f, k));
            }

            // mum
            float flick = (Mathf.PerlinNoise(t * 7f, 0.3f) - 0.5f) * 0.6f + (Mathf.PerlinNoise(t * 23f, 4f) - 0.5f) * 0.25f;
            warm.color = new Color(1f, 0.6f, 0.25f, Mathf.Lerp(warm.color.a, mum ? 0.09f + flick * 0.05f : 0f, k * 4f));
            candle.color = Gfx.A(candle.color, Mathf.Lerp(candle.color.a, mum ? 1f : 0f, k));
            flame.color = new Color(1f, 0.75f, 0.3f, Mathf.Lerp(flame.color.a, mum ? 0.85f + flick * 0.3f : 0f, k * 3f));
            flame.rectTransform.localScale = new Vector3(1f + flick * 0.15f, 1f + flick * 0.3f, 1);

            // toz ve isik huzmesi
            beam.color = Gfx.A(beam.color, Mathf.Lerp(beam.color.a, toz ? (gunes ? 0.35f : 0.18f) + flick * 0.02f : 0f, k));
            for (int i = 0; i < dust.Count; i++)
            {
                var d = dust[i];
                var p = d.anchoredPosition;
                p += new Vector2(Mathf.Sin(t * 0.3f + i) * 8f, 6f + (i % 5)) * dt;
                if (p.y > H / 2) p.y = -H / 2;
                d.anchoredPosition = p;
                var img = d.GetComponent<Image>();
                float tw = 0.5f + 0.5f * Mathf.Sin(t * (1f + i % 3) + i);
                img.color = Gfx.A(img.color, Mathf.Lerp(img.color.a, toz ? 0.55f * tw : 0f, k));
            }

            // yagmur
            tint.color = Color.Lerp(tint.color, yagmur ? new Color(0.05f, 0.1f, 0.25f, 0.22f) : (buyu ? new Color(0.1f, 0.4f, 0.2f, 0.08f) : new Color(0, 0, 0, 0)), k);
            for (int i = 0; i < rain.Count; i++)
            {
                var d = rain[i];
                var p = d.anchoredPosition;
                p += new Vector2(-380f, -2600f) * dt * (0.8f + (i % 4) * 0.1f);
                if (p.y < -H / 2) { p.y = H / 2 + Random.Range(0, 200f); p.x = Random.Range(-W / 2, W / 2 + 300f); }
                d.anchoredPosition = p;
                var img = d.GetComponent<Image>();
                img.color = Gfx.A(img.color, Mathf.Lerp(img.color.a, yagmur ? 0.55f : 0f, k));
            }

            // buyu isiklari
            glow.color = Gfx.A(glow.color, Mathf.Lerp(glow.color.a, buyu ? 0.22f + Mathf.Sin(t * 2f) * 0.05f : 0f, k));
            for (int i = 0; i < magic.Count; i++)
            {
                var d = magic[i];
                var p = d.anchoredPosition;
                p += new Vector2(Mathf.Sin(t * 1.3f + i * 1.7f) * 30f, 70f + (i % 4) * 20f) * dt;
                if (p.y > 600f) { p.y = -700f; p.x = Random.Range(-450f, 450f); }
                d.anchoredPosition = p;
                var img = d.GetComponent<Image>();
                img.color = Gfx.A(img.color, Mathf.Lerp(img.color.a, buyu ? 0.75f + 0.25f * Mathf.Sin(t * 5f + i) : 0f, k));
            }

            // dusunme karartmasi, flas
            dimNow = Mathf.Lerp(dimNow, dimTarget, k * 2f);
            dim.color = new Color(0, 0, 0, dimNow);
            flashA = Mathf.MoveTowards(flashA, 0f, dt * 2.2f);
            flash.color = new Color(1, 1, 1, flashA);

            // geri sarma: tarama cizgileri kayar
            float sa = Mathf.Lerp(scan.color.a, Rewinding ? 1f : 0f, k * 6f);
            scan.color = new Color(1, 1, 1, sa);
            var su = scan.uvRect; su.y -= dt * 6f; scan.uvRect = su;
        }
    }
}
