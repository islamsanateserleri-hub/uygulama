using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 12: 3D yagmur, kar ve sis. Parcacik sistemi paketi gerekmez: tek ag (mesh) uzerinde dortgenler.
    // Dis mekanda butun sahneye, ic mekanda yalniz pencerelerin onune (camin disinda yagiyor gibi) cizilir.
    public class HavaDurumu3D : MonoBehaviour
    {
        class Tane { public Vector3 p; public float hiz, boy, faz; public int kutu; }

        class Katman
        {
            public GameObject go;
            public Mesh mesh;
            public Material mat;
            public Vector3[] v;
            public Color[] c;
            public Tane[] t;
        }

        Katman yagmur, kar, sis, bulut;
        readonly List<Bounds> kutular = new List<Bounds>();    // Origin'e gore
        bool pencereModu, haricVar, gece;
        Bounds haric;                                          // bu kutunun icine yagmaz (araba kabini)
        readonly List<Object> uretilen = new List<Object>();
        System.Random rnd = new System.Random(12);
        float zamanT;
        public Vector3 merkez = Sahne3D.Origin;      // mekanin kuruldugu nokta

        public bool Yagiyor { get { return yagmur != null; } }
        public bool KarVar { get { return kar != null; } }
        public bool SisVar { get { return sis != null; } }

        float R(float a, float b) { return a + (float)rnd.NextDouble() * (b - a); }

        // mekan degisince cagrilir
        public void Ayarla(bool yagmurAcik, bool karAcik, bool sisAcik, Mekan3D m, bool geceMi)
        {
            Temizle();
            gece = geceMi;
            if (m == null) return;
            kutular.Clear();
            haricVar = false;
            pencereModu = !m.Dis;
            if (pencereModu)
            {
                foreach (var p in m.Pencereler)
                    kutular.Add(new Bounds(new Vector3(p.x, p.y, p.z), new Vector3(p.w, p.h, 0.004f)));
            }
            else
            {
                kutular.Add(new Bounds(new Vector3(0f, 2.4f, 1.6f), new Vector3(8f, 4.8f, 6.4f)));
                if (m.Tur == "araba")
                {
                    haricVar = true;
                    // kabin ve on cami ile kamera arasi: yuzlerin onunden cizgi gecmesin
                    haric = new Bounds(new Vector3(0f, 1.2f, -3.2f), new Vector3(4f, 3.2f, 8.4f));
                }
            }
            // gunduz penceresi beyaza yakin: yagmur/kar gorunsun diye cam disi koyulastirilir
            if (pencereModu && (yagmurAcik || karAcik) && !geceMi && kutular.Count > 0)
            {
                bulut = KatmanKur("Bulutlu", kutular.Count, Doku(4, 4, (u, v) => 1f));
                if (bulut != null)
                {
                    bulut.mat.renderQueue = 2999;
                    for (int i = 0; i < kutular.Count; i++)
                    {
                        var b = kutular[i];
                        Kare(bulut, i, new Vector3(b.center.x, b.center.y, b.center.z + 0.002f), b.extents.x, b.extents.y, new Color(0.27f, 0.34f, 0.46f, 0.62f));
                    }
                    Yaz(bulut);
                }
            }
            if (kutular.Count > 0)
            {
                if (yagmurAcik) yagmur = KatmanKur("Yagmur", pencereModu ? 90 * kutular.Count : 700, Cizgi());
                if (karAcik) kar = KatmanKur("Kar", pencereModu ? 55 * kutular.Count : 520, Yumak());
            }
            if (sisAcik)
            {
                if (pencereModu) Debug.Log("[Hikaye] hava: sis yalnız açık mekânda görünür (iç mekânda atlandı).");
                else sis = KatmanKur("Sis", 14, Bulut());
            }
            Baslat(yagmur, 0);
            Baslat(kar, 1);
            Baslat(sis, 2);
            Ciz(0f);
        }

        public void Temizle()
        {
            foreach (var k in new[] { yagmur, kar, sis, bulut })
                if (k != null && k.go != null) Destroy(k.go);
            yagmur = kar = sis = bulut = null;
            foreach (var o in uretilen) if (o != null) Destroy(o);
            uretilen.Clear();
        }

        void OnDestroy() { Temizle(); }

        Katman KatmanKur(string ad, int adet, Texture2D doku)
        {
            var sh = Shader.Find("Sprites/Default");
            if (sh == null) return null;
            var k = new Katman();
            k.go = new GameObject("Hava_" + ad);
            k.go.transform.SetParent(transform, false);
            k.go.transform.position = merkez;
            k.mesh = new Mesh { name = "Hava_" + ad };
            k.mesh.MarkDynamic();
            k.mat = new Material(sh) { name = "Hava_" + ad, mainTexture = doku };
            k.mat.renderQueue = 3001;
            uretilen.Add(k.mesh); uretilen.Add(k.mat); uretilen.Add(doku);
            k.go.AddComponent<MeshFilter>().sharedMesh = k.mesh;
            var mr = k.go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = k.mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            k.v = new Vector3[adet * 4];
            k.c = new Color[adet * 4];
            var uv = new Vector2[adet * 4];
            var tri = new int[adet * 6];
            for (int i = 0; i < adet; i++)
            {
                int b = i * 4;
                uv[b] = new Vector2(0, 0); uv[b + 1] = new Vector2(0, 1); uv[b + 2] = new Vector2(1, 1); uv[b + 3] = new Vector2(1, 0);
                int t = i * 6;
                tri[t] = b; tri[t + 1] = b + 1; tri[t + 2] = b + 2; tri[t + 3] = b; tri[t + 4] = b + 2; tri[t + 5] = b + 3;
            }
            k.mesh.vertices = k.v;
            k.mesh.uv = uv;
            k.mesh.colors = k.c;
            k.mesh.triangles = tri;
            k.mesh.bounds = new Bounds(new Vector3(0f, 2f, 2f), new Vector3(30f, 16f, 30f));
            k.t = new Tane[adet];
            return k;
        }

        void Baslat(Katman k, int tur)
        {
            if (k == null) return;
            for (int i = 0; i < k.t.Length; i++)
            {
                var t = new Tane { faz = R(0f, 6.28f) };
                if (tur == 2)
                {
                    // sis: cogu karakterlerin arkasinda, ikisi onde ve alcakta
                    bool on = i >= k.t.Length - 3;
                    t.p = new Vector3(R(-5f, 5f), on ? R(0.05f, 0.3f) : R(0.2f, 2.2f), on ? R(-0.5f, 0.2f) : R(0.9f, 4.6f));
                    t.boy = on ? R(1.6f, 2.4f) : R(2.4f, 4.2f);
                    t.hiz = R(0.06f, 0.22f) * (i % 2 == 0 ? 1f : -1f);
                    t.kutu = on ? 1 : 0;
                }
                else
                {
                    t.kutu = i % kutular.Count;
                    var b = kutular[t.kutu];
                    t.p = new Vector3(R(b.min.x, b.max.x), R(b.min.y, b.max.y), R(b.min.z, b.max.z));
                    if (tur == 0)
                    {
                        t.hiz = R(5.5f, 8f) * (pencereModu ? 0.35f : 1f);
                        t.boy = pencereModu ? R(0.06f, 0.12f) : R(0.16f, 0.3f);
                    }
                    else
                    {
                        t.hiz = R(0.3f, 0.7f) * (pencereModu ? 0.5f : 1f);
                        t.boy = pencereModu ? R(0.012f, 0.025f) : R(0.022f, 0.055f);
                    }
                }
                k.t[i] = t;
            }
        }

        void Update()
        {
            if (yagmur == null && kar == null && sis == null) return;
            Ciz(Time.deltaTime);
        }

        void Ciz(float dt)
        {
            zamanT += dt;
            float karanlik = gece ? 0.72f : 1f;
            if (yagmur != null)
            {
                var renk = new Color(0.74f * karanlik, 0.82f * karanlik, 1f * karanlik, pencereModu ? 0.75f : 0.5f);
                for (int i = 0; i < yagmur.t.Length; i++)
                {
                    var t = yagmur.t[i];
                    var b = kutular[t.kutu];
                    t.p.y -= t.hiz * dt;
                    t.p.x -= t.hiz * dt * 0.08f;
                    if (t.p.y < b.min.y) { t.p.y += b.size.y; t.p.x = R(b.min.x, b.max.x); }
                    if (t.p.x < b.min.x) t.p.x += b.size.x;
                    float boy = t.boy;
                    float en = pencereModu ? 0.004f : 0.007f;
                    var ust = t.p + new Vector3(boy * 0.08f, boy, 0f);
                    if (pencereModu && ust.y > b.max.y) ust = t.p + new Vector3(0f, Mathf.Max(0f, b.max.y - t.p.y), 0f);
                    bool gizli = haricVar && haric.Contains(t.p);
                    Dortgen(yagmur, i, t.p, ust, en, gizli ? Color.clear : renk);
                }
                Yaz(yagmur);
            }
            if (kar != null)
            {
                var renk = new Color(karanlik, karanlik, Mathf.Min(1f, karanlik * 1.06f), 0.92f);
                for (int i = 0; i < kar.t.Length; i++)
                {
                    var t = kar.t[i];
                    var b = kutular[t.kutu];
                    t.p.y -= t.hiz * dt;
                    t.p.x += Mathf.Sin(zamanT * 1.3f + t.faz) * dt * (pencereModu ? 0.03f : 0.16f);
                    if (t.p.y < b.min.y) { t.p.y += b.size.y; t.p.x = R(b.min.x, b.max.x); }
                    if (t.p.x < b.min.x) t.p.x += b.size.x; else if (t.p.x > b.max.x) t.p.x -= b.size.x;
                    float r = t.boy * 0.5f;
                    bool gizli = haricVar && haric.Contains(t.p);
                    Kare(kar, i, t.p, r, r, gizli ? Color.clear : renk);
                }
                Yaz(kar);
            }
            if (sis != null)
            {
                var gri = gece ? new Color(0.28f, 0.31f, 0.42f) : new Color(0.82f, 0.84f, 0.88f);
                for (int i = 0; i < sis.t.Length; i++)
                {
                    var t = sis.t[i];
                    t.p.x += t.hiz * dt;
                    if (t.p.x > 6f) t.p.x -= 12f; else if (t.p.x < -6f) t.p.x += 12f;
                    var renk = gri;
                    renk.a = (t.kutu == 1 ? 0.2f : 0.34f) * (0.85f + 0.15f * Mathf.Sin(zamanT * 0.35f + t.faz));
                    var p = t.p;
                    if (haricVar && haric.Contains(new Vector3(0f, p.y, p.z))) renk.a = 0f;
                    Kare(sis, i, p, t.boy * 0.5f, t.boy * (t.kutu == 1 ? 0.16f : 0.28f), renk);
                }
                Yaz(sis);
            }
        }

        static void Dortgen(Katman k, int i, Vector3 alt, Vector3 ust, float en, Color c)
        {
            int b = i * 4;
            var yan = new Vector3(en * 0.5f, 0f, 0f);
            k.v[b] = alt - yan; k.v[b + 1] = ust - yan; k.v[b + 2] = ust + yan; k.v[b + 3] = alt + yan;
            k.c[b] = c; k.c[b + 1] = c; k.c[b + 2] = c; k.c[b + 3] = c;
        }

        static void Kare(Katman k, int i, Vector3 m, float rx, float ry, Color c)
        {
            int b = i * 4;
            k.v[b] = m + new Vector3(-rx, -ry, 0f); k.v[b + 1] = m + new Vector3(-rx, ry, 0f);
            k.v[b + 2] = m + new Vector3(rx, ry, 0f); k.v[b + 3] = m + new Vector3(rx, -ry, 0f);
            k.c[b] = c; k.c[b + 1] = c; k.c[b + 2] = c; k.c[b + 3] = c;
        }

        static void Yaz(Katman k)
        {
            k.mesh.vertices = k.v;
            k.mesh.colors = k.c;
        }

        // ---------- Dokular ----------
        static Texture2D Doku(int w, int h, System.Func<float, float, float> alfa)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float a = Mathf.Clamp01(alfa((x + 0.5f) / w, (y + 0.5f) / h));
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        static Texture2D Cizgi()
        {
            return Doku(8, 32, (u, v) => (1f - Mathf.Abs(u - 0.5f) * 2f) * Mathf.Sin(v * Mathf.PI) * 1.4f);
        }

        static Texture2D Yumak()
        {
            return Doku(32, 32, (u, v) =>
            {
                float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
                return (1f - d) * 1.8f;
            });
        }

        static Texture2D Bulut()
        {
            return Doku(128, 64, (u, v) =>
            {
                float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
                float n = Mathf.PerlinNoise(u * 5f + 3.1f, v * 3f + 7.7f);
                return (1f - d) * (0.35f + n * 1.1f);
            });
        }
    }
}
