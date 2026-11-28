using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // vfx: <ad> [saniye|surekli]  ile acilan gorsel efektler (kodla uretilir; indirme yok).
    // Efektler ust uste binebilir; 'vfx: yok' ya da mekan degisimi hepsini kapatir. Hikayeyi bekletmez.
    public class EfektKatmani : MonoBehaviour
    {
        const float W = 1080f, H = 1920f;

        // Butun gorsel efekt adlari
        static readonly string[] adlar =
        {
            "kar", "tipi", "yaprak", "cicek", "kul", "kor", "ates_bocegi", "baloncuk", "duman", "kum_firtinasi", "saganak", "su_alti", "meteor", "yildizlar",
            "konfeti", "havai_fisek", "balon", "para_yagmuru", "yildiz_yagmuru", "parilti", "spot", "disko",
            "kalp_yagmuru", "kalpler", "kirik_kalp", "gozyasi_yagmuru", "soru", "unlem", "uyku", "nota", "ofke", "sok", "odak",
            "patlama_isik", "kivilcim", "sok_dalgasi", "hiz_cizgileri", "darbe", "hasar", "cam_catlagi", "buz", "kararma", "beyazlama",
            "sepya", "gece", "gun_batimi", "zehir", "kirmizi", "altin", "buz_mavisi", "mor", "nostalji", "kabus",
            "sinema_seridi", "nabiz", "karincalanma", "glitch", "tarama", "isik_sizintisi", "portal", "hipnoz", "polis", "simsekler"
        };

        public static string[] Adlar { get { return (string[])adlar.Clone(); } }

        public static bool Var(string ad) { return System.Array.IndexOf(adlar, Util.Norm(ad)) >= 0; }

        static float VarsayilanSure(string ad)
        {
            switch (ad)
            {
                case "kirik_kalp": return 3f;
                case "soru": case "unlem": case "sok": case "nota": case "uyku": case "kalpler": return 3f;
                case "patlama_isik": case "sok_dalgasi": case "darbe": return 1.2f;
                case "kivilcim": return 1.8f;
                case "hasar": return 1.5f;
                case "hiz_cizgileri": return 2.5f;
                case "konfeti": case "havai_fisek": return 5f;
                default: return 0f;   // surekli
            }
        }

        // Sahneyi sarsmasi gereken efektler (StoryPlayer kullanir)
        public static bool Sarsar(string ad)
        {
            ad = Util.Norm(ad);
            return ad == "patlama_isik" || ad == "darbe" || ad == "sok_dalgasi";
        }

        public static EfektKatmani Create(Transform parent, FxLayer fx)
        {
            var r = Gfx.Rect("VfxKatmani", parent);
            Gfx.Stretch(r);
            var k = r.gameObject.AddComponent<EfektKatmani>();
            k.rt = r;
            k.fx = fx;
            return k;
        }

        RectTransform rt;
        FxLayer fx;
        Font font;
        readonly List<Efekt> acik = new List<Efekt>();

        class Parca
        {
            public RectTransform R;
            public Graphic G;
            public Vector2 P, V;
            public float Aci, Donme, Faz, Yas, Omur, Taban;
        }

        class Efekt
        {
            public string Ad;
            public RectTransform Kok;
            public CanvasGroup Grup;
            public float T, Sure;                    // Sure 0 = surekli
            public System.Action<float, float> Guncelle;   // (gecen, dt)
        }

        // "kar", "kar 5", "kar surekli", "yok"
        public void Calistir(string komut, bool yalnizSurekli = false)
        {
            var p = (komut ?? "").Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 0) return;
            var ad = Util.Norm(p[0]);
            if (ad == "yok") { Temizle(); return; }
            float sure = VarsayilanSure(ad);
            if (p.Length > 1)
            {
                if (Util.Norm(p[1]) == "surekli") sure = 0f;
                else sure = Mathf.Max(0.3f, Util.ParseFloat(p[1], sure));
            }
            if (yalnizSurekli && sure > 0f) return;   // satirdan baslatirken gecmisteki kisa efektler oynamasin
            Baslat(ad, sure);
        }

        public void Temizle()
        {
            foreach (var e in acik) if (e.Kok != null) Destroy(e.Kok.gameObject);
            acik.Clear();
        }

        void Baslat(string ad, float sure)
        {
            if (!Var(ad)) { Debug.LogWarning("[Hikaye] Gorsel efekt yok: " + ad); return; }
            for (int i = acik.Count - 1; i >= 0; i--)
                if (acik[i].Ad == ad) { Destroy(acik[i].Kok.gameObject); acik.RemoveAt(i); }

            var e = new Efekt { Ad = ad, Sure = sure, Kok = Gfx.Rect("vfx_" + ad, rt) };
            Gfx.Stretch(e.Kok);
            e.Grup = e.Kok.gameObject.AddComponent<CanvasGroup>();
            e.Grup.blocksRaycasts = false;
            e.Grup.alpha = 0f;
            int tohum = 23;
            foreach (var ch in ad) tohum = tohum * 31 + ch;
            Kur(e, new System.Random(tohum));
            acik.Add(e);
        }

        static bool Aninda(string ad)
        {
            return ad == "patlama_isik" || ad == "sok_dalgasi" || ad == "darbe" || ad == "kivilcim" || ad == "hasar" ||
                   ad == "cam_catlagi" || ad == "glitch" || ad == "simsekler" || ad == "sok";
        }

        void Update()
        {
            if (fx != null && fx.Donuk) return;
            float dt = Time.deltaTime;
            for (int i = acik.Count - 1; i >= 0; i--)
            {
                var e = acik[i];
                e.T += dt;
                if (e.Sure > 0f && e.T >= e.Sure) { Destroy(e.Kok.gameObject); acik.RemoveAt(i); continue; }
                float giris = Aninda(e.Ad) ? 1f : Mathf.Clamp01(e.T / 0.35f);
                float cikis = e.Sure > 0f ? Mathf.Clamp01((e.Sure - e.T) / Mathf.Min(0.5f, e.Sure * 0.35f)) : 1f;
                e.Grup.alpha = giris * cikis;
                if (e.Guncelle != null) e.Guncelle(e.T, dt);
            }
        }

        // ---------- Yardimcilar ----------
        static float S(System.Random r, float a, float b) { return a + (float)r.NextDouble() * (b - a); }
        static Color Renk(int hex, float a = 1f) { return new Color(((hex >> 16) & 255) / 255f, ((hex >> 8) & 255) / 255f, (hex & 255) / 255f, a); }

        Image Ekle(Efekt e, Sprite s, Color c, float w, float h, float x = 0, float y = 0)
        {
            return Gfx.Img("p", e.Kok, s, c, new Vector2(w, h), new Vector2(x, y));
        }

        Image Tam(Efekt e, Sprite s, Color c)
        {
            var img = Ekle(e, s, c, 0, 0);
            Gfx.Stretch(img.rectTransform);
            return img;
        }

        Text Yazi(Efekt e, string metin, int boyut, Color c, float x, float y)
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var r = Gfx.Rect("y", e.Kok);
            r.sizeDelta = new Vector2(boyut * 2f, boyut * 1.4f);
            r.anchoredPosition = new Vector2(x, y);
            var t = r.gameObject.AddComponent<Text>();
            t.font = font; t.fontSize = boyut; t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter; t.color = c; t.text = metin; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            Gfx.AddOutline(t, 5);
            return t;
        }

        static Parca P(Graphic g, Vector2 v = default(Vector2))
        {
            return new Parca { R = g.rectTransform, G = g, P = g.rectTransform.anchoredPosition, V = v, Taban = g.color.a };
        }

        static void Uygula(Parca p)
        {
            p.R.anchoredPosition = p.P;
            p.R.localEulerAngles = new Vector3(0, 0, p.Aci);
        }

        static void Alfa(Graphic g, float a) { var c = g.color; c.a = a; g.color = c; }

        // Surekli akan parcaciklar: ekrandan cikan karsi kenardan geri gelir
        void Akis(Efekt e, System.Random r, int n, Sprite s, int[] renkler, float boy0, float boy1, float en, float vx0, float vx1, float vy0, float vy1,
                  float salinim = 0f, float donme = 0f, float alfa = 1f, float aci = 0f, float yanip = 0f)
        {
            var l = new List<Parca>();
            for (int i = 0; i < n; i++)
            {
                float boy = S(r, boy0, boy1);
                var img = Ekle(e, s, Renk(renkler[r.Next(renkler.Length)], alfa * S(r, 0.65f, 1f)), boy * en, boy,
                    S(r, -W / 2, W / 2), S(r, -H / 2, H / 2));
                var p = P(img, new Vector2(S(r, vx0, vx1), S(r, vy0, vy1)));
                p.Faz = S(r, 0f, 6.28f); p.Donme = donme > 0 ? S(r, -donme, donme) : 0f; p.Aci = donme > 0 ? S(r, 0f, 360f) : aci;
                Uygula(p);
                l.Add(p);
            }
            var onceki = e.Guncelle;
            e.Guncelle = (t, dt) =>
            {
                if (onceki != null) onceki(t, dt);
                foreach (var p in l)
                {
                    p.P += p.V * dt;
                    if (salinim > 0) p.P.x += Mathf.Cos(t * 1.7f + p.Faz) * salinim * dt;
                    p.Aci += p.Donme * dt;
                    if (p.P.y < -H / 2 - 120) p.P.y += H + 240;
                    if (p.P.y > H / 2 + 120) p.P.y -= H + 240;
                    if (p.P.x < -W / 2 - 120) p.P.x += W + 240;
                    if (p.P.x > W / 2 + 120) p.P.x -= W + 240;
                    if (yanip > 0) Alfa(p.G, p.Taban * Mathf.Clamp01(0.5f + Mathf.Sin(t * yanip + p.Faz * 3f)));
                    Uygula(p);
                }
            };
        }

        // Tek renk ekran ortusu
        Image Ortu(Efekt e, int hex, float a) { return Tam(e, null, Renk(hex, a)); }
        Image Kenar(Efekt e, int hex, float a) { return Tam(e, Gfx.Vignette, Renk(hex, a)); }

        void Titret(Efekt e, Graphic g, float hz, float a0, float a1)
        {
            var onceki = e.Guncelle;
            e.Guncelle = (t, dt) =>
            {
                if (onceki != null) onceki(t, dt);
                Alfa(g, Mathf.Lerp(a0, a1, 0.5f + 0.5f * Mathf.Sin(t * hz * 6.2832f)));
            };
        }

        // Merkezden sacilan parcaciklar (patlama, kivilcim, havai fisek)
        void Sacil(List<Parca> l, Efekt e, System.Random r, int n, Vector2 merkez, int hex, float hiz0, float hiz1, float boy, float omur)
        {
            for (int i = 0; i < n; i++)
            {
                float a = S(r, 0f, 6.2832f), h = S(r, hiz0, hiz1);
                var img = Ekle(e, Gfx.SoftDot, Renk(hex), boy * S(r, 0.6f, 1.3f), boy * 3f, merkez.x, merkez.y);
                var p = P(img, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * h);
                p.Omur = omur * S(r, 0.7f, 1.2f);
                Uygula(p);
                l.Add(p);
            }
        }

        static void SacilanlariOynat(List<Parca> l, float dt, float yercekimi)
        {
            for (int i = l.Count - 1; i >= 0; i--)
            {
                var p = l[i];
                p.Yas += dt;
                if (p.Yas >= p.Omur) { Destroy(p.R.gameObject); l.RemoveAt(i); continue; }
                p.V.y -= yercekimi * dt;
                p.V *= Mathf.Max(0f, 1f - 1.2f * dt);
                p.P += p.V * dt;
                p.Aci = Mathf.Atan2(p.V.y, p.V.x) * Mathf.Rad2Deg - 90f;
                Alfa(p.G, 1f - p.Yas / p.Omur);
                Uygula(p);
            }
        }

        Image Cizgi(Efekt e, Vector2 a, Vector2 b, float kalinlik, Color c)
        {
            var d = b - a;
            var img = Ekle(e, null, c, d.magnitude, kalinlik, (a.x + b.x) / 2f, (a.y + b.y) / 2f);
            img.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            return img;
        }

        // Yukari dogru cikan simgeler (kalp, nota, z, balon)
        void Yuksel(Efekt e, System.Random r, int n, System.Func<int, Graphic> uret, float hiz0, float hiz1, float salinim, float omur)
        {
            var l = new List<Parca>();
            for (int i = 0; i < n; i++)
            {
                var g = uret(i);
                var p = P(g, new Vector2(0, S(r, hiz0, hiz1)));
                p.Faz = S(r, 0f, 6.28f); p.Omur = omur; p.Yas = -omur * i / n;
                Alfa(g, 0f);
                l.Add(p);
            }
            var bas = new List<Vector2>();
            foreach (var p in l) bas.Add(p.P);
            e.Guncelle = (t, dt) =>
            {
                for (int i = 0; i < l.Count; i++)
                {
                    var p = l[i];
                    p.Yas += dt;
                    if (p.Yas < 0) continue;
                    if (p.Yas > p.Omur) { p.Yas = 0f; p.P = bas[i]; }
                    p.P += p.V * dt;
                    p.P.x += Mathf.Cos(t * 2.2f + p.Faz) * salinim * dt;
                    Alfa(p.G, Mathf.Clamp01(p.Yas * 4f) * Mathf.Clamp01((p.Omur - p.Yas) * 2f));
                    Uygula(p);
                }
            };
        }

        // ---------- Efektler ----------
        void Kur(Efekt e, System.Random r)
        {
            var nokta = Gfx.SoftDot;
            switch (e.Ad)
            {
                // --- doga ve hava ---
                case "kar": Akis(e, r, 140, nokta, new[] { 0xFFFFFF, 0xEAF4FF }, 12, 34, 1f, -30, 30, -230, -110, 60f); break;
                case "tipi":
                    Ortu(e, 0xDDE8F5, 0.18f);
                    Akis(e, r, 240, nokta, new[] { 0xFFFFFF }, 10, 28, 2.2f, -1100, -700, -420, -220, 90f, 0f, 1f, 22f);
                    break;
                case "yaprak": Akis(e, r, 45, Gfx.Leaf, new[] { 0xC8641E, 0xD99A24, 0x9C3B1B, 0x7A8F2A }, 34, 70, 1f, -90, 40, -240, -110, 130f, 140f); break;
                case "cicek": Akis(e, r, 70, Gfx.Leaf, new[] { 0xFFB7CE, 0xFFD3E2, 0xFF8FB4, 0xFFFFFF }, 20, 40, 1f, -70, 30, -190, -90, 120f, 160f); break;
                case "kul":
                    Ortu(e, 0x3A3A3A, 0.2f);
                    Akis(e, r, 110, null, new[] { 0x6E6E6E, 0x9A9A9A, 0x4A4A4A }, 6, 16, 1f, -40, 40, -120, -50, 50f, 90f);
                    break;
                case "kor":
                    Kenar(e, 0xB33A00, 0.45f);
                    Akis(e, r, 90, nokta, new[] { 0xFFB030, 0xFF6A10, 0xFFD060 }, 18, 46, 1f, -50, 50, 120, 330, 80f, 0f, 1f, 0f, 6f);
                    break;
                case "ates_bocegi": Akis(e, r, 45, nokta, new[] { 0xD8FF6A, 0xFFF59A }, 34, 68, 1f, -40, 40, -30, 30, 90f, 0f, 1f, 0f, 2.2f); break;
                case "baloncuk": Akis(e, r, 55, Gfx.Ring, new[] { 0xCFEFFF, 0xFFFFFF }, 24, 90, 1f, -20, 20, 80, 220, 50f, 0f, 0.8f); break;
                case "duman": Akis(e, r, 22, nokta, new[] { 0x8C8C8C, 0xA8A8A8, 0x6A6A6A }, 420, 800, 1f, -40, 40, 40, 110, 30f, 0f, 0.55f); break;
                case "kum_firtinasi":
                    Ortu(e, 0xB88A4A, 0.34f);
                    Akis(e, r, 220, nokta, new[] { 0xE0B878, 0xC89858, 0xF0D098 }, 5, 14, 5f, -1500, -900, -80, 80);
                    break;
                case "saganak":
                    Ortu(e, 0x1A2438, 0.28f);
                    Akis(e, r, 230, nokta, new[] { 0xCFE0FF }, 70, 160, 0.07f, -330, -270, -2300, -1700, 0f, 0f, 0.9f, -9f);
                    break;
                case "su_alti":
                    {
                        Ortu(e, 0x0A5A88, 0.4f);
                        for (int i = 0; i < 5; i++)
                        {
                            var b = Ekle(e, Gfx.Beam, Renk(0x9FE8FF, 0.22f), S(r, 180, 380), 2000, -420 + i * 210, 300);
                            float taban = -14f + i * 7f, faz = i * 1.3f;
                            var onceki = e.Guncelle;
                            e.Guncelle = (t, dt) => { if (onceki != null) onceki(t, dt); b.rectTransform.localEulerAngles = new Vector3(0, 0, taban + Mathf.Sin(t * 0.8f + faz) * 6f); };
                        }
                        Akis(e, r, 35, Gfx.Ring, new[] { 0xDFF8FF }, 14, 46, 1f, -15, 15, 90, 200, 40f, 0f, 0.75f);
                        break;
                    }
                case "meteor":
                    Ortu(e, 0x050818, 0.18f);
                    Akis(e, r, 9, nokta, new[] { 0xFFE0A0, 0xFFFFFF, 0xFFB060 }, 220, 420, 0.07f, -900, -650, -1500, -1100, 0f, 0f, 1f, -31f);
                    break;
                case "yildizlar": Akis(e, r, 90, Gfx.Star, new[] { 0xFFFFFF, 0xFFF1B8, 0xCFE0FF }, 10, 34, 1f, 0, 0, 0, 0, 0f, 0f, 1f, 0f, 2.5f); break;

                // --- kutlama ---
                case "konfeti": Akis(e, r, 150, null, new[] { 0xFF4D5E, 0xFFC93C, 0x3DDC84, 0x4D9FFF, 0xC66BFF }, 16, 30, 0.55f, -90, 90, -520, -240, 90f, 420f); break;
                case "havai_fisek":
                    {
                        Ortu(e, 0x050818, 0.55f);
                        var l = new List<Parca>();
                        int[] renk = { 0xFF4D5E, 0xFFD24D, 0x4DFFA0, 0x5AB0FF, 0xE07BFF };
                        float siradaki = 0f;
                        e.Guncelle = (t, dt) =>
                        {
                            if (t >= siradaki)
                            {
                                siradaki = t + S(r, 0.35f, 0.7f);
                                Sacil(l, e, r, 46, new Vector2(S(r, -380, 380), S(r, 150, 800)), renk[r.Next(renk.Length)], 260, 620, 28, 1.6f);
                            }
                            SacilanlariOynat(l, dt, 260f);
                        };
                        break;
                    }
                case "balon":
                    {
                        int[] renk = { 0xFF5A6E, 0xFFC93C, 0x4D9FFF, 0x3DDC84, 0xC66BFF };
                        for (int i = 0; i < 11; i++)
                        {
                            float x = S(r, -480, 480), y = S(r, -950, -150), boy = S(r, 120, 190);
                            var g = Ekle(e, Gfx.Circle, Renk(renk[r.Next(renk.Length)], 0.95f), boy, boy * 1.22f, x, y);
                            Gfx.Img("ip", g.transform, null, Renk(0xFFFFFF, 0.7f), new Vector2(3, 150), new Vector2(0, -boy * 0.61f - 75));
                            Gfx.Img("isik", g.transform, nokta, Renk(0xFFFFFF, 0.8f), new Vector2(boy * 0.35f, boy * 0.45f), new Vector2(-boy * 0.2f, boy * 0.25f));
                        }
                        var l = new List<Parca>();
                        foreach (Transform c in e.Kok) { var p = P(c.GetComponent<Image>(), new Vector2(0, S(r, 220, 380))); p.Faz = S(r, 0, 6.28f); l.Add(p); }
                        e.Guncelle = (t, dt) =>
                        {
                            foreach (var p in l)
                            {
                                p.P += p.V * dt;
                                p.P.x += Mathf.Cos(t * 1.5f + p.Faz) * 40f * dt;
                                if (p.P.y > H / 2 + 300) p.P.y = -H / 2 - 300;
                                Uygula(p);
                            }
                        };
                        break;
                    }
                case "para_yagmuru":
                    Akis(e, r, 45, Gfx.Circle, new[] { 0xFFD447, 0xF2B81F }, 36, 62, 1f, -30, 30, -620, -320, 20f);
                    Akis(e, r, 26, null, new[] { 0x6FBF73, 0x8BD18E }, 52, 70, 2.1f, -60, 60, -360, -180, 90f, 130f);
                    break;
                case "yildiz_yagmuru": Akis(e, r, 70, Gfx.Star, new[] { 0xFFE45C, 0xFFF6B0, 0xFFC93C }, 24, 62, 1f, -60, 60, -460, -220, 40f, 140f); break;
                case "parilti": Akis(e, r, 55, Gfx.Spark, new[] { 0xFFFFFF, 0xFFF1A8 }, 30, 100, 1f, 0, 0, -20, 20, 0f, 0f, 1f, 0f, 5f); break;
                case "spot":
                    {
                        var delik = Ekle(e, Gfx.Hole, Renk(0x000000, 0.86f), 2600, 4600, 0, 120);
                        var koni = Ekle(e, Gfx.Beam, Renk(0xFFF3D0, 0.2f), 760, 1500, 0, 450);
                        e.Guncelle = (t, dt) =>
                        {
                            float x = Mathf.Sin(t * 0.7f) * 40f;
                            delik.rectTransform.anchoredPosition = new Vector2(x, 120);
                            koni.rectTransform.anchoredPosition = new Vector2(x, 450);
                        };
                        break;
                    }
                case "disko":
                    {
                        Ortu(e, 0x14002A, 0.3f);
                        int[] renk = { 0xFF2E88, 0x2EE6FF, 0xFFE32E, 0x6CFF2E, 0xA84DFF };
                        var l = new List<Image>();
                        for (int i = 0; i < 18; i++) l.Add(Ekle(e, nokta, Renk(renk[i % renk.Length], 0.7f), 260, 260));
                        var h = new List<Image>();
                        for (int i = 0; i < 4; i++)
                        {
                            var b = Ekle(e, Gfx.Beam, Renk(renk[i], 0.35f), 340, 2100, -360 + i * 240, 960);
                            b.rectTransform.pivot = new Vector2(0.5f, 1f);
                            h.Add(b);
                        }
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < l.Count; i++)
                                l[i].rectTransform.anchoredPosition = new Vector2(Mathf.Sin(t * (0.6f + i % 5 * 0.17f) + i) * 500, Mathf.Cos(t * (0.4f + i % 3 * 0.23f) + i * 2.1f) * 900);
                            for (int i = 0; i < h.Count; i++)
                                h[i].rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * 1.9f + i * 1.6f) * 32f);
                        };
                        break;
                    }

                // --- duygu ---
                case "kalp_yagmuru": Akis(e, r, 60, Gfx.Heart, new[] { 0xFF3B6B, 0xFF7A9C, 0xFFB3C6 }, 36, 90, 1f, -30, 30, -360, -180, 70f); break;
                case "kalpler":
                    Yuksel(e, r, 16, i => Ekle(e, Gfx.Heart, Renk(i % 2 == 0 ? 0xFF3B6B : 0xFF7A9C), 90, 90, (i % 2 == 0 ? -1 : 1) * S(r, 220, 480), S(r, -600, 0)), 180, 340, 70, 2.2f);
                    break;
                case "kirik_kalp":
                    {
                        var parca = new Image[2];
                        for (int i = 0; i < 2; i++)
                        {
                            var h = Ekle(e, Gfx.Heart, Renk(0xE0244E), 360, 360, 0, 520);
                            h.type = Image.Type.Filled;
                            h.fillMethod = Image.FillMethod.Horizontal;
                            h.fillOrigin = i == 0 ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;
                            h.fillAmount = 0.5f;
                            parca[i] = h;
                        }
                        e.Guncelle = (t, dt) =>
                        {
                            float k = Mathf.Clamp01((t - 0.6f) / 1.1f);
                            for (int i = 0; i < 2; i++)
                            {
                                float yon = i == 0 ? -1f : 1f;
                                float titre = t < 0.6f ? Mathf.Sin(t * 60f) * 6f * t : 0f;
                                parca[i].rectTransform.anchoredPosition = new Vector2(yon * k * 90f + titre, 520 - k * k * 260f);
                                parca[i].rectTransform.localEulerAngles = new Vector3(0, 0, -yon * k * 24f);
                            }
                        };
                        break;
                    }
                case "gozyasi_yagmuru":
                    Ortu(e, 0x14284A, 0.25f);
                    Akis(e, r, 60, Gfx.Drop, new[] { 0x8CC8FF, 0xBFE2FF }, 30, 66, 0.75f, 0, 0, -700, -420);
                    break;
                case "soru":
                    Yuksel(e, r, 7, i => Yazi(e, "?", 150 + i % 3 * 40, Renk(0xFFE45C), (i % 2 == 0 ? -1 : 1) * S(r, 140, 420), S(r, 250, 700)), 40, 90, 40, 1.8f);
                    break;
                case "unlem":
                    {
                        var u = Yazi(e, "!", 420, Renk(0xFF3B30), 260, 620);
                        e.Guncelle = (t, dt) =>
                        {
                            float k = t < 0.25f ? Mathf.Lerp(0.2f, 1.25f, t / 0.25f) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((t - 0.25f) / 0.2f));
                            u.rectTransform.localScale = new Vector3(k, k, 1f);
                            u.rectTransform.localEulerAngles = new Vector3(0, 0, -10f + Mathf.Sin(t * 18f) * 3f);
                        };
                        break;
                    }
                case "uyku":
                    Yuksel(e, r, 5, i => Yazi(e, "Z", 90 + i * 28, Renk(0xBFD8FF), 140 + i * 45, 380 + i * 30), 110, 140, 60, 2.5f);
                    break;
                case "nota":
                    Yuksel(e, r, 12, i => Ekle(e, Gfx.Note, Renk(new[] { 0xFFFFFF, 0xFFE45C, 0x8CE8FF }[i % 3]), 100, 100, (i % 2 == 0 ? -1 : 1) * S(r, 180, 460), S(r, -400, 200)), 160, 300, 90, 2.4f);
                    break;
                case "ofke":
                    {
                        var k = Kenar(e, 0xC40000, 0.8f);
                        Titret(e, k, 2.5f, 0.55f, 0.95f);
                        Akis(e, r, 30, nokta, new[] { 0xFF4020, 0xFF8030 }, 12, 34, 1f, -30, 30, 260, 560, 100f);
                        break;
                    }
                case "sok":
                    {
                        var beyaz = Ortu(e, 0xFFFFFF, 0.8f);
                        Kenar(e, 0x000000, 0.8f);
                        var cizgiler = new List<Image>();
                        for (int i = 0; i < 40; i++)
                        {
                            float a = i * 0.157f + S(r, -0.05f, 0.05f), d = S(r, 430, 600);
                            var yon = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 1.5f);
                            cizgiler.Add(Cizgi(e, yon * d, yon * (d + 600), S(r, 5, 16), Renk(0xFFFFFF, 0.85f)));
                        }
                        e.Guncelle = (t, dt) =>
                        {
                            Alfa(beyaz, 0.8f * Mathf.Clamp01(1f - t * 4f));
                            for (int i = 0; i < cizgiler.Count; i++) Alfa(cizgiler[i], ((int)(t * 14f) + i) % 3 == 0 ? 0.2f : 0.85f);
                        };
                        break;
                    }
                case "odak":
                    Ekle(e, Gfx.Hole, Renk(0x000000, 0.72f), 3000, 5200, 0, 250);
                    break;

                // --- aksiyon ---
                case "patlama_isik":
                    {
                        var beyaz = Ortu(e, 0xFFF2C0, 1f);
                        var top = Ekle(e, nokta, Renk(0xFF8A10), 100, 100, 0, 0);
                        var l = new List<Parca>();
                        Sacil(l, e, r, 60, Vector2.zero, 0xFFC040, 700, 1700, 20, 1.1f);
                        e.Guncelle = (t, dt) =>
                        {
                            Alfa(beyaz, Mathf.Clamp01(0.95f - t * 3.5f));
                            float boy = 300 + 2400 * Mathf.Sqrt(Mathf.Clamp01(t));
                            top.rectTransform.sizeDelta = new Vector2(boy, boy);
                            Alfa(top, Mathf.Clamp01(1.1f - t));
                            SacilanlariOynat(l, dt, 500f);
                        };
                        break;
                    }
                case "kivilcim":
                    {
                        var l = new List<Parca>();
                        float siradaki = 0f;
                        e.Guncelle = (t, dt) =>
                        {
                            if (t >= siradaki) { siradaki = t + 0.18f; Sacil(l, e, r, 18, new Vector2(S(r, -250, 250), S(r, -300, 300)), 0xFFD060, 350, 900, 24, 0.9f); }
                            SacilanlariOynat(l, dt, 1400f);
                        };
                        break;
                    }
                case "sok_dalgasi":
                    {
                        var halkalar = new List<Image>();
                        for (int i = 0; i < 3; i++) halkalar.Add(Ekle(e, Gfx.Ring, Renk(0xFFFFFF), 10, 10));
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < halkalar.Count; i++)
                            {
                                float k = Mathf.Clamp01((t - i * 0.14f) / 0.8f);
                                float boy = 50 + k * 2400;
                                halkalar[i].rectTransform.sizeDelta = new Vector2(boy, boy);
                                Alfa(halkalar[i], k <= 0 ? 0f : 1f - k);
                            }
                        };
                        break;
                    }
                case "hiz_cizgileri":
                    {
                        var l = new List<Image>();
                        var aci = new List<float>();
                        for (int i = 0; i < 60; i++)
                        {
                            float a = S(r, 0f, 6.2832f);
                            aci.Add(a);
                            l.Add(Ekle(e, nokta, Renk(0xFFFFFF, 0.9f), S(r, 300, 700), S(r, 8, 20)));
                            l[i].rectTransform.localEulerAngles = new Vector3(0, 0, a * Mathf.Rad2Deg);
                        }
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                float k = Mathf.Repeat(t * 2.2f + i * 0.137f, 1f);
                                float d = 380 + k * 800;
                                l[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(aci[i]), Mathf.Sin(aci[i])) * d;
                                Alfa(l[i], Mathf.Sin(k * Mathf.PI));
                            }
                        };
                        break;
                    }
                case "darbe":
                    {
                        var beyaz = Ortu(e, 0xFFFFFF, 0.9f);
                        var yildiz = Ekle(e, Gfx.Star, Renk(0xFFE45C), 200, 200, 0, 150);
                        var ic = Ekle(e, Gfx.Star, Renk(0xFFFFFF), 120, 120, 0, 150);
                        e.Guncelle = (t, dt) =>
                        {
                            Alfa(beyaz, Mathf.Clamp01(0.8f - t * 5f));
                            float k = Mathf.Clamp01(t * 7f);
                            yildiz.rectTransform.sizeDelta = Vector2.one * (200 + 600 * k);
                            ic.rectTransform.sizeDelta = Vector2.one * (120 + 320 * k);
                            yildiz.rectTransform.localEulerAngles = new Vector3(0, 0, t * 60f);
                            Alfa(yildiz, Mathf.Clamp01((0.9f - t) * 3f));
                            Alfa(ic, Mathf.Clamp01((0.8f - t) * 3f));
                        };
                        break;
                    }
                case "hasar":
                    {
                        var k = Kenar(e, 0xD00000, 1f);
                        var o = Ortu(e, 0xD00000, 0.45f);
                        e.Guncelle = (t, dt) => { Alfa(o, Mathf.Clamp01(0.45f - t)); Alfa(k, 0.7f + 0.3f * Mathf.Abs(Mathf.Sin(t * 6f))); };
                        break;
                    }
                case "cam_catlagi":
                    {
                        var merkez = new Vector2(S(r, -150, 150), S(r, 150, 450));
                        var halka = new List<Vector2>();
                        for (int i = 0; i < 13; i++)
                        {
                            float a = i * 6.2832f / 13f + S(r, -0.18f, 0.18f);
                            var p0 = merkez;
                            for (int k = 1; k <= 5; k++)
                            {
                                float aa = a + S(r, -0.14f, 0.14f);
                                var p1 = p0 + new Vector2(Mathf.Cos(aa), Mathf.Sin(aa)) * S(r, 110, 330);
                                Cizgi(e, p0, p1, Mathf.Max(2f, 8f - k * 1.4f), Renk(0xFFFFFF, 0.9f));
                                if (k == 1) halka.Add(p1);
                                p0 = p1;
                            }
                        }
                        for (int i = 0; i < halka.Count; i++) Cizgi(e, halka[i], halka[(i + 1) % halka.Count], 3f, Renk(0xFFFFFF, 0.75f));
                        Ekle(e, nokta, Renk(0xFFFFFF, 0.9f), 120, 120, merkez.x, merkez.y);
                        break;
                    }
                case "buz":
                    Kenar(e, 0xBFE8FF, 1f);
                    Kenar(e, 0xE8F8FF, 0.7f);
                    Akis(e, r, 40, Gfx.Spark, new[] { 0xE8F8FF }, 24, 70, 1f, 0, 0, -30, -10, 0f, 0f, 1f, 0f, 2f);
                    break;
                case "kararma":
                    {
                        var o = Ortu(e, 0x000000, 0f);
                        e.Guncelle = (t, dt) => Alfa(o, Mathf.Clamp01(t / 1.6f) * 0.94f);
                        break;
                    }
                case "beyazlama":
                    {
                        var o = Ortu(e, 0xFFFFFF, 0f);
                        e.Guncelle = (t, dt) => Alfa(o, Mathf.Clamp01(t / 1.6f) * 0.92f);
                        break;
                    }

                // --- renk ve his ---
                case "sepya": Ortu(e, 0x8A5A20, 0.34f); Kenar(e, 0x2A1600, 0.7f); break;
                case "gece": Ortu(e, 0x081440, 0.46f); Kenar(e, 0x000014, 0.7f); break;
                case "gun_batimi":
                    {
                        var raw = Gfx.Rect("gradyan", e.Kok).gameObject.AddComponent<RawImage>();
                        Gfx.Stretch(raw.rectTransform);
                        raw.texture = Gfx.Gradient(Renk(0x7A2A8A, 0.42f), Renk(0xFF8A20, 0.3f));
                        raw.raycastTarget = false;
                        Ekle(e, nokta, Renk(0xFFB040, 0.55f), 1100, 1100, 420, 300);
                        break;
                    }
                case "zehir":
                    Ortu(e, 0x2A8A10, 0.28f);
                    Kenar(e, 0x1A4A00, 0.8f);
                    Akis(e, r, 30, Gfx.Ring, new[] { 0xB8FF50 }, 20, 60, 1f, -20, 20, 60, 160, 40f, 0f, 0.7f);
                    break;
                case "kirmizi": Ortu(e, 0xB00010, 0.32f); Kenar(e, 0x400000, 0.7f); break;
                case "altin":
                    Ortu(e, 0xFFB800, 0.2f);
                    Kenar(e, 0x7A4A00, 0.6f);
                    Akis(e, r, 35, Gfx.Spark, new[] { 0xFFE880 }, 20, 60, 1f, 0, 0, 10, 50, 0f, 0f, 1f, 0f, 4f);
                    break;
                case "buz_mavisi": Ortu(e, 0x3AA0FF, 0.24f); Kenar(e, 0x0A3A70, 0.6f); break;
                case "mor": Ortu(e, 0x7A20C0, 0.3f); Kenar(e, 0x20003A, 0.7f); break;
                case "nostalji":
                    Ortu(e, 0xFFE0B0, 0.2f);
                    Kenar(e, 0xFFF0D8, 0.75f);
                    Akis(e, r, 30, nokta, new[] { 0xFFF6E0 }, 20, 70, 1f, -20, 20, 10, 50, 20f, 0f, 0.6f);
                    break;
                case "kabus":
                    {
                        Ortu(e, 0x10001A, 0.4f);
                        Kenar(e, 0x000000, 1f);
                        var k2 = Kenar(e, 0x5A0030, 0.6f);
                        Titret(e, k2, 0.7f, 0.15f, 0.85f);
                        Akis(e, r, 12, nokta, new[] { 0x000000 }, 350, 700, 1f, -80, 80, -50, 50, 120f, 0f, 0.65f);
                        break;
                    }

                // --- sinema ve teknik ---
                case "sinema_seridi":
                    {
                        for (int s = -1; s <= 1; s += 2) Ekle(e, null, Renk(0x000000), W, 210, 0, s * (H / 2 - 105));
                        var delik = new List<Parca>();
                        for (int s = -1; s <= 1; s += 2)
                            for (int i = 0; i < 12; i++)
                                delik.Add(P(Ekle(e, Gfx.RoundRect, Renk(0xE8E0D0, 0.9f), 54, 70, -W / 2 + i * 100, s * (H / 2 - 105))));
                        var cizik = Ekle(e, null, Renk(0xFFFFFF, 0.35f), 3, H);
                        e.Guncelle = (t, dt) =>
                        {
                            foreach (var p in delik) { p.P.x -= 260f * dt; if (p.P.x < -W / 2 - 100) p.P.x += 1200; Uygula(p); }
                            int kare = (int)(t * 12f);
                            cizik.rectTransform.anchoredPosition = new Vector2((kare * 397) % 900 - 450, 0);
                            cizik.enabled = kare % 3 != 0;
                        };
                        break;
                    }
                case "nabiz":
                    {
                        var k = Kenar(e, 0xB00000, 0f);
                        e.Guncelle = (t, dt) =>
                        {
                            float f = Mathf.Repeat(t, 0.85f);
                            float vurus = Mathf.Exp(-f * 9f) + (f > 0.22f ? 0.7f * Mathf.Exp(-(f - 0.22f) * 9f) : 0f);
                            Alfa(k, Mathf.Clamp01(0.2f + vurus));
                        };
                        break;
                    }
                case "karincalanma":
                    {
                        var raw = Gfx.Rect("gren", e.Kok).gameObject.AddComponent<RawImage>();
                        Gfx.Stretch(raw.rectTransform);
                        raw.texture = Gfx.Noise;
                        raw.color = new Color(1f, 1f, 1f, 0.45f);
                        raw.raycastTarget = false;
                        e.Guncelle = (t, dt) =>
                        {
                            int kare = (int)(t * 24f);
                            raw.uvRect = new Rect((kare * 0.371f) % 1f, (kare * 0.713f) % 1f, 2.4f, 4.2f);
                        };
                        break;
                    }
                case "glitch":
                    {
                        int[] renk = { 0x00FFF0, 0xFF0060, 0xFFFFFF, 0x2040FF };
                        var l = new List<Image>();
                        for (int i = 0; i < 16; i++) l.Add(Ekle(e, null, Renk(renk[i % renk.Length], 0.55f), 100, 10));
                        int onceki = -1;
                        e.Guncelle = (t, dt) =>
                        {
                            int kare = (int)(t * 11f);
                            if (kare == onceki) return;
                            onceki = kare;
                            foreach (var i in l)
                            {
                                i.enabled = r.NextDouble() < 0.55;
                                i.rectTransform.sizeDelta = new Vector2(S(r, 150, W), S(r, 4, 70));
                                i.rectTransform.anchoredPosition = new Vector2(S(r, -300, 300), S(r, -H / 2, H / 2));
                            }
                        };
                        break;
                    }
                case "tarama":
                    {
                        var raw = Gfx.Rect("cizgiler", e.Kok).gameObject.AddComponent<RawImage>();
                        Gfx.Stretch(raw.rectTransform);
                        raw.texture = Gfx.Scanlines;
                        raw.uvRect = new Rect(0, 0, 270, 240);
                        raw.raycastTarget = false;
                        Ortu(e, 0x00FF80, 0.08f);
                        var serit = Ekle(e, nokta, Renk(0x80FFC0, 0.55f), W * 2f, 90);
                        e.Guncelle = (t, dt) => serit.rectTransform.anchoredPosition = new Vector2(0, H / 2 - Mathf.Repeat(t * 700f, H));
                        break;
                    }
                case "isik_sizintisi":
                    {
                        int[] renk = { 0xFF7A20, 0xFFC040, 0xFF3060 };
                        var l = new List<Image>();
                        for (int i = 0; i < 5; i++) l.Add(Ekle(e, nokta, Renk(renk[i % 3], 0.55f), S(r, 800, 1400), S(r, 800, 1400)));
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                float yan = i % 2 == 0 ? -1f : 1f;
                                l[i].rectTransform.anchoredPosition = new Vector2(yan * (480 + Mathf.Sin(t * 0.5f + i) * 160), Mathf.Sin(t * 0.37f + i * 1.9f) * 800);
                                Alfa(l[i], 0.25f + 0.3f * (0.5f + 0.5f * Mathf.Sin(t * 1.3f + i * 2f)));
                            }
                        };
                        break;
                    }
                case "portal":
                    {
                        var l = new List<Image>();
                        var arka = Ekle(e, nokta, Renk(0x6A20FF, 0.7f), 1200, 1500, 0, 150);
                        for (int i = 0; i < 6; i++)
                        {
                            var h = Ekle(e, Gfx.Ring, Renk(i % 2 == 0 ? 0x40E0FF : 0xC060FF, 0.9f), 900 - i * 120, 1150 - i * 155, 0, 150);
                            l.Add(h);
                        }
                        var taneler = new List<Image>();
                        for (int i = 0; i < 40; i++) taneler.Add(Ekle(e, nokta, Renk(0xE0F0FF), 22, 22));
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < l.Count; i++) l[i].rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * (1f + i * 0.3f) + i) * 18f);
                            for (int i = 0; i < taneler.Count; i++)
                            {
                                float k = Mathf.Repeat(t * 0.45f + i * 0.0619f, 1f), a = i * 2.4f + t * 2f;
                                taneler[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(a) * 480 * (1f - k), 150 + Mathf.Sin(a) * 620 * (1f - k));
                                Alfa(taneler[i], Mathf.Sin(k * Mathf.PI));
                            }
                            Alfa(arka, 0.5f + 0.2f * Mathf.Sin(t * 3f));
                        };
                        break;
                    }
                case "hipnoz":
                    {
                        Ortu(e, 0x14002A, 0.3f);
                        var l = new List<Image>();
                        for (int i = 0; i < 8; i++) l.Add(Ekle(e, Gfx.Ring, Renk(i % 2 == 0 ? 0xFFFFFF : 0xC060FF), 10, 10, 0, 200));
                        e.Guncelle = (t, dt) =>
                        {
                            for (int i = 0; i < l.Count; i++)
                            {
                                float k = Mathf.Repeat(t * 0.35f + i / 8f, 1f);
                                l[i].rectTransform.sizeDelta = Vector2.one * (30 + k * 2200);
                                Alfa(l[i], 0.8f * Mathf.Sin(k * Mathf.PI));
                            }
                        };
                        break;
                    }
                case "polis":
                    {
                        var kirmizi = Ekle(e, nokta, Renk(0xFF0020), 1900, 2800, -560, 300);
                        var mavi = Ekle(e, nokta, Renk(0x0050FF), 1900, 2800, 560, 300);
                        e.Guncelle = (t, dt) =>
                        {
                            bool sol = Mathf.Repeat(t, 0.7f) < 0.35f;
                            float f = 0.55f + 0.25f * Mathf.Abs(Mathf.Sin(t * 28f));
                            Alfa(kirmizi, sol ? f : 0.08f);
                            Alfa(mavi, sol ? 0.08f : f);
                        };
                        break;
                    }
                case "simsekler":
                    {
                        Ortu(e, 0x0A1024, 0.25f);
                        var beyaz = Ortu(e, 0xE8F0FF, 0f);
                        float sonraki = 0.1f, son = -9f;
                        e.Guncelle = (t, dt) =>
                        {
                            if (t >= sonraki) { son = t; sonraki = t + S(r, 1.1f, 2.6f); }
                            float g = t - son;
                            float a = Mathf.Exp(-g * 14f) + (g > 0.17f ? 0.7f * Mathf.Exp(-(g - 0.17f) * 11f) : 0f);
                            Alfa(beyaz, Mathf.Clamp01(a * 0.85f));
                        };
                        break;
                    }
            }
        }
    }
}
