using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Renk ayari ve zaman/atmosfer yonetimi.
    //  - 3D mekan acikken (mekan3d:) URP Volume (ColorAdjustments, Vignette, FilmGrain, Bloom, LensDistortion,
    //    ChromaticAberration, DepthOfField, WhiteBalance) 3D kameraya uygulanir. URP tipleri yansima (reflection) ile
    //    kullanilir: paket/surum farki derleme hatasi vermez, bulunamazsa sessizce atlanir.
    //  - 2D mekanlarda (Canvas Overlay post-process gormez) ayni gorunum yari saydam renk, kenar karartma, gren ve
    //    tarama cizgisi katmanlariyla taklit edilir.
    //  - zaman (isik rengi/acisi + ortam isigi), isik presetleri, gecis (karar/capraz/beyaz) ve sessizlik de burada.
    public class AtmosferVolume : MonoBehaviour
    {
        // Muzik ses carpani: sessizlik: komutu sirasinda 0'a iner. StoryPlayer ambSrc.volume'u bununla carpar.
        public static float MuzikCarpani = 1f;
        static float muzikHedef = 1f;

        // ---------- Gorunum (look) ----------
        class Look
        {
            public float exp, con, sat, vig, grain, bloom, lens, chroma, temp, tint, scan;
            public Color filter = Color.white;
            public Color ov = new Color(0f, 0f, 0f, 0f);    // alfa ile carpilmis (premultiplied) renk

            public static Color Pm(float r, float g, float b, float a) { return new Color(r * a, g * a, b * a, a); }

            public static Look Birlestir(Look a, Look b)
            {
                var l = new Look();
                l.exp = a.exp + b.exp; l.con = a.con + b.con; l.sat = a.sat + b.sat;
                l.vig = Mathf.Max(a.vig, b.vig); l.grain = Mathf.Max(a.grain, b.grain); l.scan = Mathf.Max(a.scan, b.scan);
                l.bloom = a.bloom + b.bloom; l.lens = a.lens + b.lens; l.chroma = a.chroma + b.chroma;
                l.temp = a.temp + b.temp; l.tint = a.tint + b.tint;
                l.filter = new Color(a.filter.r * b.filter.r, a.filter.g * b.filter.g, a.filter.b * b.filter.b, 1f);
                float ia = 1f - b.ov.a;
                l.ov = new Color(b.ov.r + a.ov.r * ia, b.ov.g + a.ov.g * ia, b.ov.b + a.ov.b * ia, b.ov.a + a.ov.a * ia);
                return l;
            }

            public void Yaklas(Look h, float k)
            {
                exp = Mathf.Lerp(exp, h.exp, k); con = Mathf.Lerp(con, h.con, k); sat = Mathf.Lerp(sat, h.sat, k);
                vig = Mathf.Lerp(vig, h.vig, k); grain = Mathf.Lerp(grain, h.grain, k); bloom = Mathf.Lerp(bloom, h.bloom, k);
                lens = Mathf.Lerp(lens, h.lens, k); chroma = Mathf.Lerp(chroma, h.chroma, k);
                temp = Mathf.Lerp(temp, h.temp, k); tint = Mathf.Lerp(tint, h.tint, k); scan = Mathf.Lerp(scan, h.scan, k);
                filter = Color.Lerp(filter, h.filter, k);
                ov = Color.Lerp(ov, h.ov, k);
            }

            // UI icin duz (premultiplied olmayan) renk
            public Color OvDuz(float agirlik)
            {
                if (ov.a < 0.002f) return new Color(0f, 0f, 0f, 0f);
                return new Color(ov.r / ov.a, ov.g / ov.a, ov.b / ov.a, Mathf.Clamp01(ov.a * agirlik));
            }
        }

        class LightSet
        {
            public Color key, fill, amb, sky;
            public float keyI, fillI;
            public Vector3 keyEuler, fillEuler;
        }

        static Look MoodLook(string n)
        {
            var l = new Look();
            switch (n)
            {
                case "huzun":
                    l.sat = -35f; l.con = 10f; l.exp = -0.25f; l.filter = new Color(0.86f, 0.92f, 1f); l.vig = 0.38f;
                    l.temp = -12f; l.grain = 0.15f; l.ov = Look.Pm(0.5f, 0.58f, 0.72f, 0.2f);
                    break;
                case "sicak_ani":
                    l.sat = 15f; l.exp = 0.25f; l.con = -8f; l.filter = new Color(1f, 0.93f, 0.8f); l.vig = 0.3f;
                    l.bloom = 0.5f; l.temp = 25f; l.ov = Look.Pm(1f, 0.78f, 0.45f, 0.16f);
                    break;
                case "kabus":
                    l.sat = -50f; l.con = 25f; l.exp = -0.5f; l.filter = new Color(0.78f, 0.92f, 0.82f); l.vig = 0.55f;
                    l.grain = 0.3f; l.chroma = 0.25f; l.lens = -0.15f; l.temp = -8f; l.tint = 15f;
                    l.ov = Look.Pm(0.08f, 0.18f, 0.12f, 0.3f);
                    break;
            }
            return l;
        }

        static Look AniLook(string n)
        {
            var l = new Look();
            switch (n)
            {
                case "sepya":
                    l.sat = -85f; l.filter = new Color(1f, 0.86f, 0.62f); l.con = 10f; l.vig = 0.45f; l.grain = 0.35f;
                    l.bloom = 0.25f; l.ov = Look.Pm(0.5f, 0.34f, 0.16f, 0.45f);
                    break;
                case "vhs":
                    l.sat = -15f; l.con = 8f; l.chroma = 0.6f; l.grain = 0.5f; l.lens = 0.08f; l.scan = 1f; l.vig = 0.25f;
                    l.ov = Look.Pm(0.5f, 0.55f, 0.7f, 0.1f);
                    break;
                case "soluk":
                    l.sat = -50f; l.exp = 0.35f; l.con = -25f; l.filter = new Color(0.96f, 0.96f, 1f); l.vig = 0.2f;
                    l.grain = 0.2f; l.ov = Look.Pm(0.93f, 0.91f, 0.86f, 0.35f);
                    break;
            }
            return l;
        }

        static Look ZamanLook(string n)
        {
            var l = new Look();
            switch (n)
            {
                case "sabah": l.temp = 8f; l.exp = 0.1f; l.ov = Look.Pm(1f, 0.95f, 0.8f, 0.06f); break;
                case "gun_batimi": l.temp = 30f; l.filter = new Color(1f, 0.9f, 0.8f); l.vig = 0.25f; l.ov = Look.Pm(1f, 0.5f, 0.2f, 0.2f); break;
                case "gece": l.exp = -0.1f; l.temp = -10f; l.filter = new Color(0.8f, 0.85f, 1f); l.vig = 0.35f; l.ov = Look.Pm(0.04f, 0.07f, 0.25f, 0.38f); break;
            }
            return l;
        }

        static LightSet ZamanLS(string n)
        {
            var s = new LightSet();
            s.key = new Color(1f, 0.96f, 0.9f); s.keyI = 1.25f; s.keyEuler = new Vector3(35f, -25f, 0f);
            s.fill = new Color(0.75f, 0.82f, 1f); s.fillI = 0.45f; s.fillEuler = new Vector3(20f, 150f, 0f);
            s.amb = new Color(0.45f, 0.47f, 0.52f); s.sky = new Color(0.55f, 0.75f, 0.95f);
            switch (n)
            {
                case "sabah":
                    s.key = new Color(1f, 0.93f, 0.8f); s.keyI = 1.35f; s.keyEuler = new Vector3(25f, -35f, 0f);
                    s.fill = new Color(0.75f, 0.85f, 1f); s.fillI = 0.5f; s.amb = new Color(0.5f, 0.55f, 0.65f); s.sky = new Color(0.62f, 0.78f, 0.95f);
                    break;
                case "gun_batimi":
                    s.key = new Color(1f, 0.58f, 0.3f); s.keyI = 1.3f; s.keyEuler = new Vector3(10f, -60f, 0f);
                    s.fill = new Color(0.7f, 0.5f, 0.6f); s.fillI = 0.35f; s.amb = new Color(0.42f, 0.3f, 0.3f); s.sky = new Color(0.95f, 0.55f, 0.35f);
                    break;
                case "gece":
                    s.key = new Color(0.5f, 0.6f, 1f); s.keyI = 0.6f; s.keyEuler = new Vector3(50f, -20f, 0f);
                    s.fill = new Color(0.3f, 0.35f, 0.7f); s.fillI = 0.25f; s.amb = new Color(0.1f, 0.12f, 0.2f); s.sky = new Color(0.02f, 0.03f, 0.08f);
                    break;
            }
            return s;
        }

        // ---------- Durum ----------
        Sahne3D owner;
        Camera cam;
        Light key, fill;
        readonly Look cur = new Look();
        Look zamanL = new Look(), moodL = new Look(), aniL = new Look();
        LightSet ls = ZamanLS("gun");
        float isikKeyMul = 1f, isikFillMul = 1f, isikAmbMul = 1f;
        Color isikTint = Color.white;
        bool isikEulerVar;
        Vector3 isikEuler;
        bool lightsTouched, ambientApplied;
        Color keyCur, fillCur, ambCur, skyCur;
        float keyICur, fillICur;
        bool lightsInit;
        string sonZaman = "gun";
        bool roomActive, skyDis, anlik;
        float gecisSure = 1.5f;
        bool dofOn;
        float odak = 6f;
        float ovW = 1f;
        UnityEngine.Rendering.AmbientMode origAmbMode;
        Color origAmb;

        public string SonZaman { get { return sonZaman; } }

        // overlay
        RectTransform gradeRoot, gecisRoot;
        Image ovTint, ovVig, gecisImg;
        RawImage ovGrain, ovScan, gecisRaw;

        // ses
        AudioSource odaSrc;
        float odaHedef;

        // volume (yansima)
        bool volTried, volOk;
        GameObject volGo;
        object profile;
        Component volComp, camData;
        readonly Dictionary<string, object> comps = new Dictionary<string, object>();
        readonly Dictionary<string, KeyValuePair<object, PropertyInfo>> pCache = new Dictionary<string, KeyValuePair<object, PropertyInfo>>();

        const float W = 1080f, H = 1920f;

        public static AtmosferVolume Kur(Sahne3D owner, Camera cam, Light key, Light fill, Transform canvasRoot)
        {
            var a = owner.gameObject.AddComponent<AtmosferVolume>();
            a.owner = owner; a.cam = cam; a.key = key; a.fill = fill;
            a.origAmbMode = RenderSettings.ambientMode;
            a.origAmb = RenderSettings.ambientLight;
            MuzikCarpani = 1f; muzikHedef = 1f;
            a.KurUI(canvasRoot);
            return a;
        }

        void KurUI(Transform cv)
        {
            if (cv == null) return;
            gradeRoot = Gfx.Rect("GradeKatmani", cv);
            Gfx.Stretch(gradeRoot);
            ovTint = Gfx.Img("Renk", gradeRoot, null, new Color(0, 0, 0, 0), Vector2.zero, Vector2.zero);
            Gfx.Stretch(ovTint.rectTransform);
            ovVig = Gfx.Img("Kenar", gradeRoot, Gfx.Vignette, new Color(0, 0, 0, 0), Vector2.zero, Vector2.zero);
            Gfx.Stretch(ovVig.rectTransform);
            ovGrain = Gfx.Rect("Gren", gradeRoot).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(ovGrain.rectTransform);
            ovGrain.texture = Gfx.Noise; ovGrain.color = new Color(1, 1, 1, 0); ovGrain.raycastTarget = false;
            ovGrain.uvRect = new Rect(0, 0, W / 256f, H / 256f);
            ovScan = Gfx.Rect("Tarama", gradeRoot).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(ovScan.rectTransform);
            ovScan.texture = Gfx.Scanlines; ovScan.color = new Color(1, 1, 1, 0); ovScan.raycastTarget = false;
            ovScan.uvRect = new Rect(0, 0, W / 4f, H / 8f / 2f);
            var fx = cv.Find("Efektler");
            if (fx != null) gradeRoot.SetSiblingIndex(fx.GetSiblingIndex() + 1);
            gradeRoot.gameObject.SetActive(false);

            gecisRoot = Gfx.Rect("GecisKatmani", cv);
            Gfx.Stretch(gecisRoot);
            gecisImg = Gfx.Img("Duz", gecisRoot, null, new Color(0, 0, 0, 0), Vector2.zero, Vector2.zero);
            Gfx.Stretch(gecisImg.rectTransform);
            gecisRaw = Gfx.Rect("Kare", gecisRoot).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(gecisRaw.rectTransform);
            gecisRaw.color = new Color(1, 1, 1, 0); gecisRaw.raycastTarget = false;
            var kt = cv.Find("Kararma");
            if (kt != null) gecisRoot.SetSiblingIndex(kt.GetSiblingIndex() + 1);
        }

        void OnDestroy()
        {
            if (ambientApplied) { RenderSettings.ambientMode = origAmbMode; RenderSettings.ambientLight = origAmb; }
            if (gradeRoot != null) Destroy(gradeRoot.gameObject);
            if (gecisRoot != null) Destroy(gecisRoot.gameObject);
            var po = profile as Object;
            if (po != null) Destroy(po);
            MuzikCarpani = 1f; muzikHedef = 1f;
        }

        // ---------- Komutlar ----------
        public void Atmosfer(string ad)
        {
            string n = Util.Norm(ad);
            if (n == "normal" || n == "yok" || n.Length == 0) { moodL = new Look(); return; }
            if (!Util.In(n, YonetmenTablo.Atmosferler)) { Debug.LogWarning("[Hikaye] atmosfer bilinmiyor: " + ad); return; }
            moodL = MoodLook(n);
        }

        public void Ani(string ad)
        {
            string n = Util.Norm(ad);
            if (n == "bitir" || n == "yok" || n.Length == 0) { aniL = new Look(); return; }
            if (!Util.In(n, YonetmenTablo.Animlar)) { Debug.LogWarning("[Hikaye] ani bilinmiyor: " + ad); return; }
            aniL = AniLook(n);
        }

        public void Zaman(string ad)
        {
            string n = Util.Norm(ad);
            if (!Util.In(n, YonetmenTablo.Zamanlar)) { Debug.LogWarning("[Hikaye] zaman bilinmiyor: " + ad); return; }
            zamanL = ZamanLook(n);
            ls = ZamanLS(n);
            lightsTouched = true;
            sonZaman = n;
            if (owner != null && owner.Mekan != null) owner.Mekan.ZamanUygula(n);
        }

        public void Isik(string ad)
        {
            string n = Util.Norm(ad);
            if (!Util.In(n, YonetmenTablo.Isiklar)) { Debug.LogWarning("[Hikaye] isik bilinmiyor: " + ad); return; }
            isikKeyMul = 1f; isikFillMul = 1f; isikAmbMul = 1f; isikTint = Color.white; isikEulerVar = false;
            switch (n)
            {
                case "dramatik": isikKeyMul = 1.4f; isikFillMul = 0.2f; isikAmbMul = 0.6f; isikEulerVar = true; isikEuler = new Vector3(25f, -70f, 0f); break;
                case "yumusak": isikKeyMul = 0.8f; isikFillMul = 1.6f; isikAmbMul = 1.2f; isikEulerVar = true; isikEuler = new Vector3(40f, -10f, 0f); break;
                case "soguk": isikTint = new Color(0.75f, 0.85f, 1f); break;
                case "sicak": isikTint = new Color(1f, 0.8f, 0.55f); break;
                case "karanlik": isikKeyMul = 0.35f; isikFillMul = 0.35f; isikAmbMul = 0.5f; break;
                case "mum": isikTint = new Color(1f, 0.62f, 0.32f); isikKeyMul = 0.55f; isikFillMul = 0.15f; isikAmbMul = 0.6f; break;
            }
            lightsTouched = true;
        }

        public void Bulaniklik(bool ac)
        {
            if (ac && !roomActive)
                Debug.Log("[Hikaye] kamera: bulanik sadece mekan3d icinde calisir (2D mekanda atlandi).");
            dofOn = ac;
        }

        public void OdakMesafe(float d) { odak = d; }
        public void Anlik() { anlik = true; }

        public void OdaDegisti(bool aktif, Mekan3D m)
        {
            roomActive = aktif;
            skyDis = aktif && m != null && m.Dis;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = aktif ? new Color(0.03f, 0.03f, 0.04f, 1f) : new Color(0f, 0f, 0f, 0f);
            }
            if (aktif)
            {
                KurVolume();
                if (m != null) m.ZamanUygula(sonZaman);
            }
            SetPost(aktif && volOk);
            if (volGo != null) volGo.SetActive(aktif && volOk);
            anlik = true;
        }

        public IEnumerator Gecis(string tur)
        {
            string k = Util.Norm(tur);
            if (gecisImg == null) yield break;
            if (k == "capraz")
            {
                yield return new WaitForEndOfFrame();
                Texture2D tex = null;
                try { tex = ScreenCapture.CaptureScreenshotAsTexture(); }
                catch (System.Exception) { tex = null; }
                if (tex == null) yield break;
                gecisRaw.texture = tex;
                gecisRaw.color = Color.white;
                StartCoroutine(CaprazSon(tex, 0.8f));
                yield break;
            }
            Color c = k == "beyaz" ? Color.white : Color.black;
            float girisSn = k == "beyaz" ? 0.15f : 0.3f;
            float t = 0f;
            while (t < girisSn)
            {
                t += Time.deltaTime;
                gecisImg.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(t / girisSn));
                yield return null;
            }
            gecisImg.color = new Color(c.r, c.g, c.b, 1f);
            StartCoroutine(GecisCikis(c, 0.25f, k == "beyaz" ? 0.5f : 0.45f));
        }

        IEnumerator GecisCikis(Color c, float bekle, float sure)
        {
            yield return new WaitForSeconds(bekle);
            float t = 0f;
            while (t < sure)
            {
                t += Time.deltaTime;
                if (gecisImg != null) gecisImg.color = new Color(c.r, c.g, c.b, 1f - Mathf.Clamp01(t / sure));
                yield return null;
            }
            if (gecisImg != null) gecisImg.color = new Color(c.r, c.g, c.b, 0f);
        }

        IEnumerator CaprazSon(Texture2D tex, float sure)
        {
            float t = 0f;
            while (t < sure)
            {
                t += Time.deltaTime;
                if (gecisRaw != null) gecisRaw.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(t / sure));
                yield return null;
            }
            if (gecisRaw != null) { gecisRaw.color = new Color(1f, 1f, 1f, 0f); gecisRaw.texture = null; }
            if (tex != null) Destroy(tex);
        }

        // Sessizlik: muzik kesilir, yalniz oda sesi kalir; sn saniye sonra muzik geri gelir
        public IEnumerator Sessizlik(float sn)
        {
            sn = Mathf.Clamp(sn, 0.2f, 30f);
            OdaSesiHazirla();
            muzikHedef = 0f; odaHedef = 0.2f;
            yield return new WaitForSeconds(sn);
            muzikHedef = 1f; odaHedef = 0f;
        }

        void OdaSesiHazirla()
        {
            if (odaSrc != null) return;
            var go = new GameObject("OdaSesi");
            go.transform.SetParent(transform, false);
            odaSrc = go.AddComponent<AudioSource>();
            odaSrc.playOnAwake = false; odaSrc.loop = true; odaSrc.spatialBlend = 0f; odaSrc.volume = 0f;
            odaSrc.clip = OdaSesiUret();
            odaSrc.Play();
        }

        // Dusuk gurultulu stereo "oda tonu": alcak gecirgen gurultu + cok hafif 50 Hz vizilti, dikissiz dongu
        static AudioClip OdaSesiUret()
        {
            int rate = 44100, n = rate * 4, f = rate / 5;
            var rnd = new System.Random(11);
            var g = new float[(n + f) * 2];
            float l1 = 0f, l2 = 0f, r1 = 0f, r2 = 0f;
            for (int i = 0; i < n + f; i++)
            {
                float wl = (float)(rnd.NextDouble() * 2.0 - 1.0), wr = (float)(rnd.NextDouble() * 2.0 - 1.0);
                l1 += (wl - l1) * 0.04f; l2 += (l1 - l2) * 0.04f;
                r1 += (wr - r1) * 0.04f; r2 += (r1 - r2) * 0.04f;
                float hum = Mathf.Sin(i * 2f * Mathf.PI * 50f / rate) * 0.004f;
                g[i * 2] = l2 * 2.5f + hum;
                g[i * 2 + 1] = r2 * 2.5f + hum;
            }
            var o = new float[n * 2];
            for (int i = 0; i < n; i++)
            {
                if (i < f)
                {
                    float t = i / (float)f;
                    o[i * 2] = g[i * 2] * t + g[(n + i) * 2] * (1f - t);
                    o[i * 2 + 1] = g[i * 2 + 1] * t + g[(n + i) * 2 + 1] * (1f - t);
                }
                else { o[i * 2] = g[i * 2]; o[i * 2 + 1] = g[i * 2 + 1]; }
            }
            var clip = AudioClip.Create("OdaSesi", n, 2, rate, false);
            clip.SetData(o, 0);
            return clip;
        }

        // ---------- Her kare ----------
        void Update()
        {
            float dt = Time.deltaTime;
            float tau = Mathf.Max(0.05f, gecisSure * 0.45f);
            float k = anlik ? 1f : 1f - Mathf.Exp(-dt / tau);

            var hedef = Look.Birlestir(Look.Birlestir(zamanL, moodL), aniL);
            cur.Yaklas(hedef, k);

            ApplyLights(k);

            // gokyuzu rengi (3D mekan disinda)
            if (roomActive && cam != null)
            {
                Color sky = skyDis ? ls.sky : new Color(0.03f, 0.03f, 0.04f);
                skyCur = anlik ? sky : Color.Lerp(skyCur, sky, k);
                cam.backgroundColor = new Color(skyCur.r, skyCur.g, skyCur.b, 1f);
            }

            // 2D katman
            ovW = Mathf.MoveTowards(ovW, roomActive ? 0f : 1f, dt * 2f);
            if (gradeRoot != null)
            {
                float w = ovW;
                var tc = cur.OvDuz(w);
                float va = Mathf.Clamp01(cur.vig * 0.9f * w);
                float ga = Mathf.Clamp01(cur.grain * 0.3f * w);
                float sa = Mathf.Clamp01(cur.scan * w);
                bool gorunur = tc.a > 0.003f || va > 0.003f || ga > 0.003f || sa > 0.003f;
                if (gradeRoot.gameObject.activeSelf != gorunur) gradeRoot.gameObject.SetActive(gorunur);
                if (gorunur)
                {
                    ovTint.color = tc;
                    ovVig.color = new Color(0f, 0f, 0f, va);
                    ovGrain.color = new Color(1f, 1f, 1f, ga);
                    ovScan.color = new Color(1f, 1f, 1f, sa);
                    var uv = ovGrain.uvRect; uv.x = Random.value; uv.y = Random.value; ovGrain.uvRect = uv;
                    var su = ovScan.uvRect; su.y -= dt * 0.8f; ovScan.uvRect = su;
                }
            }

            // 3D Volume
            if (roomActive && volOk) ApplyVolume();

            // ses
            MuzikCarpani = Mathf.MoveTowards(MuzikCarpani, muzikHedef, dt * 3f);
            if (odaSrc != null) odaSrc.volume = Mathf.MoveTowards(odaSrc.volume, odaHedef, dt * 0.8f);

            anlik = false;
        }

        void ApplyLights(float k)
        {
            if (!lightsTouched || key == null || fill == null) return;
            if (!lightsInit)
            {
                lightsInit = true;
                keyCur = key.color; keyICur = key.intensity;
                fillCur = fill.color; fillICur = fill.intensity;
                ambCur = RenderSettings.ambientLight;
                skyCur = ls.sky;
            }
            Color kc = new Color(ls.key.r * isikTint.r, ls.key.g * isikTint.g, ls.key.b * isikTint.b, 1f);
            Vector3 ke = isikEulerVar ? isikEuler : ls.keyEuler;
            keyCur = Color.Lerp(keyCur, kc, k);
            keyICur = Mathf.Lerp(keyICur, ls.keyI * isikKeyMul, k);
            fillCur = Color.Lerp(fillCur, ls.fill, k);
            fillICur = Mathf.Lerp(fillICur, ls.fillI * isikFillMul, k);
            ambCur = Color.Lerp(ambCur, ls.amb * isikAmbMul, k);
            key.color = keyCur; key.intensity = keyICur;
            fill.color = fillCur; fill.intensity = fillICur;
            key.transform.rotation = Quaternion.Slerp(key.transform.rotation, Quaternion.Euler(ke), k);
            fill.transform.rotation = Quaternion.Slerp(fill.transform.rotation, Quaternion.Euler(ls.fillEuler), k);
            if (!ambientApplied)
            {
                ambientApplied = true;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            }
            RenderSettings.ambientLight = new Color(ambCur.r, ambCur.g, ambCur.b, 1f);
        }

        // ---------- URP Volume (yansima ile) ----------
        static System.Type FindType(string full)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(full, false);
                    if (t != null) return t;
                }
                catch (System.Exception) { }
            }
            return null;
        }

        static void SetMember(object o, string name, object value)
        {
            if (o == null) return;
            try
            {
                var t = o.GetType();
                var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.CanWrite) { p.SetValue(o, Uyarla(value, p.PropertyType), null); return; }
                var f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
                if (f != null) f.SetValue(o, Uyarla(value, f.FieldType));
            }
            catch (System.Exception) { }
        }

        static object Uyarla(object v, System.Type hedef)
        {
            if (v == null || hedef.IsAssignableFrom(v.GetType())) return v;
            return System.Convert.ChangeType(v, hedef);
        }

        static PropertyInfo ValueProp(System.Type t)
        {
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (p.Name == "value" && p.CanWrite) return p;
            return null;
        }

        void KurVolume()
        {
            if (volTried) return;
            volTried = true;
            try
            {
                var tVolume = FindType("UnityEngine.Rendering.Volume");
                var tProfile = FindType("UnityEngine.Rendering.VolumeProfile");
                if (tVolume == null || tProfile == null)
                {
                    Debug.Log("[Hikaye] URP Volume bulunamadi: 3D renk ayari atlandi.");
                    return;
                }
                MethodInfo add = null;
                foreach (var m in tProfile.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (m.Name != "Add" || m.IsGenericMethodDefinition) continue;
                    var ps = m.GetParameters();
                    if (ps.Length >= 1 && ps[0].ParameterType == typeof(System.Type)) { add = m; break; }
                }
                if (add == null) { Debug.Log("[Hikaye] VolumeProfile.Add bulunamadi: 3D renk ayari atlandi."); return; }

                volGo = new GameObject("GradeVolume");
                volGo.transform.SetParent(transform, false);
                volComp = volGo.AddComponent(tVolume);
                var po = ScriptableObject.CreateInstance(tProfile);
                profile = po;
                SetMember(volComp, "isGlobal", true);
                SetMember(volComp, "priority", 20f);
                SetMember(volComp, "weight", 1f);

                string[] adlar = { "ColorAdjustments", "Vignette", "FilmGrain", "Bloom", "LensDistortion", "ChromaticAberration", "DepthOfField", "WhiteBalance" };
                foreach (var ad in adlar)
                {
                    var t = FindType("UnityEngine.Rendering.Universal." + ad);
                    if (t == null) continue;
                    var args = add.GetParameters().Length == 2 ? new object[] { t, true } : new object[] { t };
                    var c = add.Invoke(profile, args);
                    if (c != null) comps[ad] = c;
                }
                SetMember(volComp, "sharedProfile", profile);
                volOk = comps.Count > 0;
                volGo.SetActive(false);
                if (!volOk) Debug.Log("[Hikaye] URP post-process ayarlari bulunamadi: 3D renk ayari atlandi.");
            }
            catch (System.Exception e)
            {
                volOk = false;
                Debug.LogWarning("[Hikaye] URP Volume kurulamadi (3D renk ayari atlandi): " + e.Message);
            }
        }

        void SetPost(bool ac)
        {
            if (cam == null) return;
            try
            {
                var tData = FindType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
                if (tData == null) return;
                if (camData == null)
                {
                    camData = cam.GetComponent(tData);
                    if (camData == null) camData = cam.gameObject.AddComponent(tData);
                }
                SetMember(camData, "renderPostProcessing", ac);
            }
            catch (System.Exception) { }
        }

        void P(string comp, string field, object value)
        {
            object c;
            if (!comps.TryGetValue(comp, out c) || c == null) return;
            string key2 = comp + "." + field;
            KeyValuePair<object, PropertyInfo> kv;
            if (!pCache.TryGetValue(key2, out kv))
            {
                object par = null; PropertyInfo vp = null;
                try
                {
                    var f = c.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);
                    if (f != null) { par = f.GetValue(c); if (par != null) vp = ValueProp(par.GetType()); }
                }
                catch (System.Exception) { par = null; vp = null; }
                kv = new KeyValuePair<object, PropertyInfo>(par, vp);
                pCache[key2] = kv;
            }
            if (kv.Key == null || kv.Value == null) return;
            try
            {
                object v = value;
                var pt = kv.Value.PropertyType;
                if (pt.IsEnum && value is string) v = System.Enum.Parse(pt, (string)value);
                else if (v != null && !pt.IsAssignableFrom(v.GetType())) v = System.Convert.ChangeType(v, pt);
                kv.Value.SetValue(kv.Key, v, null);
                var os = kv.Key.GetType().GetProperty("overrideState", BindingFlags.Public | BindingFlags.Instance);
                if (os != null) os.SetValue(kv.Key, true, null);
            }
            catch (System.Exception) { }
        }

        void ApplyVolume()
        {
            P("ColorAdjustments", "postExposure", cur.exp);
            P("ColorAdjustments", "contrast", cur.con);
            P("ColorAdjustments", "saturation", cur.sat);
            P("ColorAdjustments", "colorFilter", cur.filter);
            P("Vignette", "intensity", Mathf.Clamp01(cur.vig));
            P("Vignette", "smoothness", 0.45f);
            P("Vignette", "color", Color.black);
            P("FilmGrain", "intensity", Mathf.Clamp01(cur.grain));
            P("FilmGrain", "response", 0.8f);
            P("Bloom", "intensity", Mathf.Max(0f, cur.bloom));
            P("Bloom", "threshold", 0.9f);
            P("Bloom", "scatter", 0.7f);
            P("LensDistortion", "intensity", Mathf.Clamp(cur.lens, -1f, 1f));
            P("ChromaticAberration", "intensity", Mathf.Clamp01(cur.chroma));
            P("WhiteBalance", "temperature", Mathf.Clamp(cur.temp, -100f, 100f));
            P("WhiteBalance", "tint", Mathf.Clamp(cur.tint, -100f, 100f));
            P("DepthOfField", "mode", dofOn ? "Gaussian" : "Off");
            if (dofOn)
            {
                P("DepthOfField", "gaussianStart", Mathf.Max(0.1f, odak + 0.9f));
                P("DepthOfField", "gaussianEnd", odak + 5f);
                P("DepthOfField", "gaussianMaxRadius", 1f);
                P("DepthOfField", "highQualitySampling", false);
            }
        }
    }
}
