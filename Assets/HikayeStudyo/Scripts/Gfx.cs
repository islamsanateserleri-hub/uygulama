using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Kodla uretilen yumusak kenarli sekiller ve dokular (hazir gorsel olmadan da sahne dolu gorunsun diye)
    public static class Gfx
    {
        delegate float Field(float x, float y);

        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        static float Len(float x, float y) { return Mathf.Sqrt(x * x + y * y); }

        // d: piksel cinsinden isaretli uzaklik (icerisi negatif) -> kenar yumusatilmis alfa
        static Sprite FromDistance(string key, int w, int h, Field d, Vector4 border)
        {
            return FromAlpha(key, w, h, (x, y) => Mathf.Clamp01(0.5f - d(x, y)), border);
        }

        static Sprite FromAlpha(string key, int w, int h, Field a, Vector4 border)
        {
            Sprite s;
            if (sprites.TryGetValue(key, out s) && s != null) return s;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float al = Mathf.Clamp01(a(x + 0.5f, y + 0.5f));
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(al * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s.name = key;
            sprites[key] = s;
            return s;
        }

        public static Sprite Circle
        {
            get { return FromDistance("circle", 128, 128, (x, y) => Len(x - 64, y - 64) - 62, Vector4.zero); }
        }

        // 9 dilimli yuvarlak kose dikdortgen (her boyuta uzar)
        public static Sprite RoundRect
        {
            get
            {
                return FromDistance("rrect", 64, 64, (x, y) =>
                {
                    const float r = 24f;
                    float qx = Mathf.Abs(x - 32f) - (30f - r);
                    float qy = Mathf.Abs(y - 32f) - (30f - r);
                    return Len(Mathf.Max(qx, 0), Mathf.Max(qy, 0)) + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
                }, new Vector4(28, 28, 28, 28));
            }
        }

        // Gulumseme yayi (U seklinde, yuvarlak uclu)
        public static Sprite Smile
        {
            get
            {
                return FromDistance("smile", 128, 72, (x, y) =>
                {
                    float dx = x - 64f, dy = y - 62f;
                    if (dy > 0) return Len(Mathf.Abs(dx) - 50f, dy) - 8f;
                    return Mathf.Abs(Len(dx, dy) - 50f) - 8f;
                }, Vector4.zero);
            }
        }

        // Sola bakan ucgen (geri sarma simgesi)
        public static Sprite TriangleLeft
        {
            get
            {
                return FromAlpha("triL", 64, 64, (x, y) =>
                {
                    float sum = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        float px = x + (i % 2) * 0.5f - 0.25f, py = y + (i / 2) * 0.5f - 0.25f;
                        // tepe (4,32), taban x=60 arasinda y=4..60
                        float t = (px - 4f) / 56f;
                        if (t >= 0 && t <= 1 && Mathf.Abs(py - 32f) <= t * 28f) sum += 0.25f;
                    }
                    return sum;
                }, Vector4.zero);
            }
        }

        public static Sprite SoftDot
        {
            get
            {
                return FromAlpha("softdot", 64, 64, (x, y) =>
                {
                    float r = Len(x - 32, y - 32) / 32f;
                    float a = Mathf.Clamp01(1f - r);
                    return a * a;
                }, Vector4.zero);
            }
        }

        public static Sprite Heart
        {
            get
            {
                return FromAlpha("heart", 96, 96, (x, y) =>
                {
                    // 4 alt ornekle kenar yumusatma; (x^2 + y^2 - 1)^3 - x^2 y^3 <= 0
                    float sum = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        float ox = (i % 2) * 0.5f - 0.25f, oy = (i / 2) * 0.5f - 0.25f;
                        float u = (x + ox - 48f) / 36f, v = (y + oy - 44f) / 36f;
                        float q = u * u + v * v - 1f;
                        if (q * q * q - u * u * v * v * v <= 0) sum += 0.25f;
                    }
                    return sum;
                }, Vector4.zero);
            }
        }

        // Kenar karartma
        public static Sprite Vignette
        {
            get
            {
                return FromAlpha("vignette", 256, 256, (x, y) =>
                {
                    float u = (x / 256f - 0.5f) * 2f, v = (y / 256f - 0.5f) * 2f;
                    float r = Mathf.Sqrt(u * u * 0.9f + v * v * 0.75f);
                    return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 1.35f, r)) * 0.9f;
                }, Vector4.zero);
            }
        }

        // Isik huzmesi: ustte parlak, asagi dogru sonen konik
        public static Sprite Beam
        {
            get
            {
                return FromAlpha("beam", 128, 256, (x, y) =>
                {
                    float v = y / 256f;                 // 0 alt, 1 ust
                    float halfW = Mathf.Lerp(0.5f, 0.18f, v);
                    float u = Mathf.Abs(x / 128f - 0.5f) / halfW;
                    float edge = Mathf.Clamp01(1f - u);
                    return edge * edge * Mathf.Lerp(0.15f, 1f, v);
                }, Vector4.zero);
            }
        }

        public static Texture2D Noise
        {
            get
            {
                Texture2D t;
                if (textures.TryGetValue("noise", out t) && t != null) return t;
                t = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                t.wrapMode = TextureWrapMode.Repeat;
                t.filterMode = FilterMode.Point;
                var px = new Color32[256 * 256];
                var rnd = new System.Random(7);
                for (int i = 0; i < px.Length; i++)
                {
                    byte g = (byte)rnd.Next(0, 256);
                    px[i] = new Color32(g, g, g, (byte)rnd.Next(0, 256));
                }
                t.SetPixels32(px);
                t.Apply(false, true);
                textures["noise"] = t;
                return t;
            }
        }

        public static Texture2D Scanlines
        {
            get
            {
                Texture2D t;
                if (textures.TryGetValue("scan", out t) && t != null) return t;
                t = new Texture2D(4, 8, TextureFormat.RGBA32, false);
                t.wrapMode = TextureWrapMode.Repeat;
                t.filterMode = FilterMode.Point;
                var px = new Color32[32];
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 4; x++)
                        px[y * 4 + x] = y < 3 ? new Color32(0, 0, 0, 150) : new Color32(255, 255, 255, 12);
                t.SetPixels32(px);
                t.Apply(false, true);
                textures["scan"] = t;
                return t;
            }
        }

        public static Texture2D Gradient(Color top, Color bottom)
        {
            string key = "grad" + ColorUtility.ToHtmlStringRGBA(top) + ColorUtility.ToHtmlStringRGBA(bottom);
            Texture2D t;
            if (textures.TryGetValue(key, out t) && t != null) return t;
            t = new Texture2D(2, 256, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            var px = new Color[512];
            for (int y = 0; y < 256; y++)
            {
                var c = Color.Lerp(bottom, top, y / 255f);
                px[y * 2] = c; px[y * 2 + 1] = c;
            }
            t.SetPixels(px);
            t.Apply(false, true);
            textures[key] = t;
            return t;
        }

        // ---------- UI yardimcilari ----------
        public static readonly Color Ink = new Color(0.13f, 0.08f, 0.08f, 1f);

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        public static Image Img(string name, Transform parent, Sprite sprite, Color color, Vector2 size, Vector2 pos)
        {
            var r = Rect(name, parent);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            if (sprite == RoundRect) img.type = Image.Type.Sliced;
            return img;
        }

        public static void AddOutline(Graphic g, float width)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = Ink;
            o.effectDistance = new Vector2(width, -width);
            o.useGraphicAlpha = true;
        }

        public static Color Mul(Color c, float f) { return new Color(c.r * f, c.g * f, c.b * f, c.a); }
        public static Color A(Color c, float a) { return new Color(c.r, c.g, c.b, a); }
    }
}
