using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Sahnedeki karakter. Gorsel verilmisse onu kullanir; verilmemisse duz cizgi film tarzinda kendisi cizer.
    // Surekli canlidir: nefes, goz kirpma, kafa egme, konusurken agiz ve kafa hareketi, yurume/kosma,
    // aglarken omuz sarsma, kalpler ve gozyaslari.
    public class CharacterView : MonoBehaviour
    {
        public CharacterDef Def;
        public bool Talking;
        public float MouthLevel;          // 0-1
        public bool Shaking;              // aglarken omuz sarsma
        public float TiltTarget;          // derece
        public bool Thinking;             // gozler saga sola kayar
        public float SlotX;
        public bool Busy { get { return moving; } }
        public string Pose { get { return pose; } }

        RectTransform root, body, head, armL, armR, legL, legR, eyeL, eyeR, pupilL, pupilR, browL, browR;
        RectTransform mouthO, mouthSmile, faceAnchor, fx, shadow;
        Image sprImage;
        EmotionSprite spr;
        readonly List<KeyValuePair<Graphic, Color>> tintables = new List<KeyValuePair<Graphic, Color>>();
        readonly List<Particle> particles = new List<Particle>();

        struct FaceSet
        {
            public float brow, browY, eye, mouthW, mouthH;
            public int mouth;     // 0 'O', 1 gulumseme, 2 uzgun, 3 duz
            public bool tears, blush, hearts;
            public float skin;    // 1 normal, <1 solgun
        }
        FaceSet face, faceTarget;
        string emotion = "normal";
        string pose = "ayakta";
        Vector2 basePos;
        bool moving, running;
        float walkPhase, dir = 1f;
        float phase, blinkNext, blinkUntil, lookX, lookNext, tilt, jumpT = -1f, pop, focus = 1f, focusTarget = 1f;
        float tearTimer, heartTimer;
        Vector2 headBase;

        class Particle { public RectTransform r; public Image img; public Vector2 v; public float life, max; public bool tear; }

        // ---------- Olusturma ----------
        public static CharacterView Create(Transform parent, CharacterDef def)
        {
            var r = Gfx.Rect("Char_" + def.id, parent);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0f);
            r.sizeDelta = new Vector2(420, 1100);
            var v = r.gameObject.AddComponent<CharacterView>();
            v.Def = def;
            v.root = r;
            v.phase = Random.value * 10f;
            v.blinkNext = Time.time + Random.Range(1.5f, 4f);
            if (def.HasSprites) v.BuildSprite(); else v.BuildPuppet();
            v.fx = Gfx.Rect("Fx", r);
            v.fx.anchorMin = v.fx.anchorMax = new Vector2(0.5f, 0f);
            r.localScale = Vector3.one * def.scale;
            v.SetEmotion("normal", true);
            return v;
        }

        RectTransform Part(string name, Transform parent, Sprite s, Color c, Vector2 size, Vector2 pos,
                           Vector2 anchor, Vector2 pivot, float outline, bool tint = true)
        {
            var img = Gfx.Img(name, parent, s, c, size, pos);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            if (outline > 0) Gfx.AddOutline(img, outline);
            if (tint) tintables.Add(new KeyValuePair<Graphic, Color>(img, c));
            return rt;
        }

        static readonly Vector2 Feet = new Vector2(0.5f, 0f);
        static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);
        static readonly Vector2 Top = new Vector2(0.5f, 1f);

        void BuildSprite()
        {
            var d = Def;
            var img = Gfx.Img("Sprite", root, null, Color.white, d.spriteSize, Vector2.zero);
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Feet; rt.pivot = Feet; rt.anchoredPosition = Vector2.zero;
            sprImage = img;
            tintables.Add(new KeyValuePair<Graphic, Color>(img, Color.white));
            body = rt;
            head = rt;
            faceAnchor = Gfx.Rect("Face", root);
            faceAnchor.anchorMin = faceAnchor.anchorMax = Feet;
            faceAnchor.anchoredPosition = new Vector2(0, d.spriteSize.y * 0.8f);
        }

        void BuildPuppet()
        {
            var d = Def;
            Color shoe = new Color(0.2f, 0.13f, 0.1f);
            shadow = Part("Golge", root, Gfx.Circle, new Color(0, 0, 0, 0.28f), new Vector2(360, 54), new Vector2(0, 6), Feet, Mid, 0, false);

            legL = Part("BacakSol", root, Gfx.RoundRect, d.pants, new Vector2(88, 280), new Vector2(-56, 300), Feet, Top, 4);
            legR = Part("BacakSag", root, Gfx.RoundRect, d.pants, new Vector2(88, 280), new Vector2(56, 300), Feet, Top, 4);
            Part("Ayakkabi", legL, Gfx.Circle, shoe, new Vector2(120, 62), new Vector2(-12, 8), Feet, Mid, 4);
            Part("Ayakkabi", legR, Gfx.Circle, shoe, new Vector2(120, 62), new Vector2(12, 8), Feet, Mid, 4);

            body = Part("Govde", root, Gfx.RoundRect, d.shirt, new Vector2(320, 410), new Vector2(0, 250), Feet, Feet, 5);
            Part("Kemer", body, Gfx.RoundRect, Gfx.Mul(d.pants, 0.85f), new Vector2(320, 56), new Vector2(0, 0), Feet, Feet, 0);
            Part("Yama", body, Gfx.RoundRect, Gfx.Mul(d.shirt, 0.78f), new Vector2(76, 66), new Vector2(78, 150), Feet, Mid, 3);
            Part("Kivrim", body, Gfx.RoundRect, Gfx.Mul(d.shirt, 0.88f), new Vector2(12, 150), new Vector2(-70, 230), Feet, Mid, 0);
            Part("Yaka", body, Gfx.Circle, d.skin, new Vector2(130, 70), new Vector2(0, 398), Feet, Mid, 0);

            Part("Boyun", root, Gfx.RoundRect, Gfx.Mul(d.skin, 0.92f), new Vector2(96, 90), new Vector2(0, 660), Feet, Mid, 0);

            armL = Part("KolSol", root, Gfx.RoundRect, d.shirt, new Vector2(76, 320), new Vector2(-178, 640), Feet, Top, 4);
            armR = Part("KolSag", root, Gfx.RoundRect, d.shirt, new Vector2(76, 320), new Vector2(178, 640), Feet, Top, 4);
            Part("El", armL, Gfx.Circle, d.skin, new Vector2(86, 86), new Vector2(0, 6), Feet, Mid, 4);
            Part("El", armR, Gfx.Circle, d.skin, new Vector2(86, 86), new Vector2(0, 6), Feet, Mid, 4);

            // Kafa grubu: boyundan doner
            head = Gfx.Rect("Kafa", root);
            head.anchorMin = head.anchorMax = Feet;
            head.pivot = new Vector2(0.5f, 0.1f);
            head.sizeDelta = new Vector2(320, 340);
            head.anchoredPosition = new Vector2(0, 650);
            headBase = head.anchoredPosition;

            if (d.hairStyle == HairStyle.Uzun)
            {
                Part("SacArka", head, Gfx.Circle, d.hair, new Vector2(370, 390), new Vector2(0, -10), Mid, Mid, 5);
                Part("SacYanSol", head, Gfx.RoundRect, d.hair, new Vector2(100, 360), new Vector2(-150, 60), Mid, Top, 5);
                Part("SacYanSag", head, Gfx.RoundRect, d.hair, new Vector2(100, 360), new Vector2(150, 60), Mid, Top, 5);
            }
            if (d.hairStyle == HairStyle.Topuz)
                Part("Topuz", head, Gfx.Circle, d.hair, new Vector2(160, 160), new Vector2(0, 178), Mid, Mid, 5);

            Part("KulakSol", head, Gfx.Circle, d.skin, new Vector2(74, 94), new Vector2(-150, 0), Mid, Mid, 4);
            Part("KulakSag", head, Gfx.Circle, d.skin, new Vector2(74, 94), new Vector2(150, 0), Mid, Mid, 4);
            var faceRt = Part("Yuz", head, Gfx.Circle, d.skin, new Vector2(310, 310), Vector2.zero, Mid, Mid, 5);
            faceAnchor = faceRt;

            switch (d.hairStyle)
            {
                case HairStyle.Kase: Part("Sac", head, Gfx.Circle, d.hair, new Vector2(342, 172), new Vector2(0, 136), Mid, Mid, 5); break;
                case HairStyle.Kisa: Part("Sac", head, Gfx.Circle, d.hair, new Vector2(322, 142), new Vector2(0, 142), Mid, Mid, 5); break;
                case HairStyle.Uzun: Part("Sac", head, Gfx.Circle, d.hair, new Vector2(338, 160), new Vector2(0, 140), Mid, Mid, 5); break;
                case HairStyle.Topuz: Part("Sac", head, Gfx.Circle, d.hair, new Vector2(328, 150), new Vector2(0, 140), Mid, Mid, 5); break;
            }
            if (d.hairStyle == HairStyle.Kase || d.hairStyle == HairStyle.Kisa)
            {
                Part("Tutam", head, Gfx.Circle, d.hair, new Vector2(62, 36), new Vector2(-2, 80), Mid, Mid, 0);
            }

            if (d.beard)
            {
                Part("Sakal", head, Gfx.Circle, d.hair, new Vector2(250, 150), new Vector2(0, -112), Mid, Mid, 4);
                Part("Biyik", head, Gfx.Circle, d.hair, new Vector2(160, 48), new Vector2(0, -56), Mid, Mid, 0);
            }

            Part("YanakSol", head, Gfx.Circle, new Color(1f, 0.45f, 0.45f, 0f), new Vector2(64, 38), new Vector2(-98, -40), Mid, Mid, 0, false).name = "YanakSol";
            Part("YanakSag", head, Gfx.Circle, new Color(1f, 0.45f, 0.45f, 0f), new Vector2(64, 38), new Vector2(98, -40), Mid, Mid, 0, false).name = "YanakSag";

            eyeL = Part("GozSol", head, Gfx.Circle, Color.white, new Vector2(64, 78), new Vector2(-62, 10), Mid, Mid, 3);
            eyeR = Part("GozSag", head, Gfx.Circle, Color.white, new Vector2(64, 78), new Vector2(62, 10), Mid, Mid, 3);
            var pupil = new Color(0.12f, 0.08f, 0.06f);
            pupilL = Part("Bebek", eyeL, Gfx.Circle, pupil, new Vector2(36, 44), new Vector2(0, -4), Mid, Mid, 0);
            pupilR = Part("Bebek", eyeR, Gfx.Circle, pupil, new Vector2(36, 44), new Vector2(0, -4), Mid, Mid, 0);
            Part("Parilti", pupilL, Gfx.Circle, Color.white, new Vector2(13, 13), new Vector2(7, 9), Mid, Mid, 0, false);
            Part("Parilti", pupilR, Gfx.Circle, Color.white, new Vector2(13, 13), new Vector2(7, 9), Mid, Mid, 0, false);

            var browC = Gfx.Mul(d.hair, 0.8f);
            browL = Part("KasSol", head, Gfx.Circle, browC, new Vector2(78, 20), new Vector2(-62, 58), Mid, Mid, 0);
            browR = Part("KasSag", head, Gfx.Circle, browC, new Vector2(78, 20), new Vector2(62, 58), Mid, Mid, 0);

            Part("Burun", head, Gfx.Circle, Gfx.Mul(d.skin, 0.86f), new Vector2(36, 26), new Vector2(0, -26), Mid, Mid, 0);

            var mouthC = new Color(0.42f, 0.09f, 0.11f);
            mouthO = Part("AgizO", head, Gfx.Circle, mouthC, new Vector2(70, 46), new Vector2(0, -84), Mid, Mid, 3);
            Part("Dil", mouthO, Gfx.Circle, new Color(0.95f, 0.45f, 0.5f), new Vector2(40, 18), new Vector2(0, 6), Feet, Mid, 0);
            mouthSmile = Part("AgizGulus", head, Gfx.Smile, Gfx.Ink, new Vector2(110, 62), new Vector2(0, -80), Mid, Mid, 0);
        }

        // ---------- Durum ----------
        static FaceSet FaceFor(string e)
        {
            var f = new FaceSet { brow = 0, browY = 0, eye = 1, mouth = 1, mouthW = 80, mouthH = 40, skin = 1 };
            switch (Util.Norm(e))
            {
                case "mutlu": f.browY = 8; f.eye = 0.8f; f.mouth = 1; f.mouthW = 125; f.blush = true; break;
                case "sevgi": f.browY = 6; f.eye = 0.75f; f.mouth = 1; f.mouthW = 110; f.blush = true; f.hearts = true; break;
                case "uzgun": f.brow = 14; f.eye = 0.8f; f.mouth = 2; f.mouthW = 80; break;
                case "aglamakli": f.brow = 18; f.browY = 4; f.eye = 0.7f; f.mouth = 2; f.mouthW = 90; f.tears = true; f.blush = true; break;
                case "korkmus": f.brow = 12; f.browY = 14; f.eye = 1.25f; f.mouth = 0; f.mouthW = 48; f.mouthH = 52; f.skin = 0.94f; break;
                case "sasirmis": f.browY = 22; f.eye = 1.3f; f.mouth = 0; f.mouthW = 56; f.mouthH = 62; break;
                case "kizgin": f.brow = -18; f.browY = -6; f.eye = 0.9f; f.mouth = 3; f.mouthW = 76; break;
                case "yorgun": f.brow = 6; f.eye = 0.5f; f.mouth = 3; f.mouthW = 60; break;
                case "hasta": f.brow = 8; f.eye = 0.45f; f.mouth = 2; f.mouthW = 60; f.skin = 0.88f; break;
                case "dusunceli": f.brow = -6; f.browY = 4; f.eye = 0.9f; f.mouth = 3; f.mouthW = 50; break;
            }
            return f;
        }

        public void SetEmotion(string e, bool instant = false)
        {
            if (Util.Norm(e) != Util.Norm(emotion)) pop = 1f;   // ifade degisirken kucuk bir sicrama
            emotion = e;
            faceTarget = FaceFor(e);
            if (instant) face = faceTarget;
            if (sprImage != null)
            {
                spr = Def.GetSprite(e);
                if (spr != null) sprImage.sprite = spr.body;
            }
        }

        public void SetFocus(bool f) { focusTarget = f ? 1f : 0.72f; }

        // pos: yatarken ayaklarin, ayakta/otururken ayaklarin konumu (StoryPlayer hesaplar)
        public void SetPose(string p, Vector2 pos)
        {
            pose = Util.Norm(p);
            root.localEulerAngles = pose == "yatiyor" ? new Vector3(0, 0, 90) : Vector3.zero;
            Place(pos);
            if (shadow != null) shadow.gameObject.SetActive(pose == "ayakta");
            bool sit = pose == "oturuyor";
            if (legL != null) { legL.gameObject.SetActive(!sit); legR.gameObject.SetActive(!sit); }
        }

        public void Place(Vector2 p) { basePos = p; root.anchoredPosition = p; }
        public Vector2 Position { get { return basePos; } }

        // Kamera icin: yuzun dunya katmanindaki konumu
        public Vector3 FaceWorld { get { return faceAnchor != null ? faceAnchor.position : root.position; } }

        // ---------- Hareketler ----------
        public IEnumerator MoveTo(float x, bool run)
        {
            if (pose == "yatiyor") yield break;
            moving = true; running = run;
            float speed = run ? 1500f : 650f;
            dir = Mathf.Sign(x - basePos.x);
            if (dir == 0) dir = 1;
            while (Mathf.Abs(basePos.x - x) > 2f)
            {
                basePos.x = Mathf.MoveTowards(basePos.x, x, speed * Time.deltaTime);
                yield return null;
            }
            basePos.x = x;
            moving = false; running = false;
        }

        public void Jump() { jumpT = 0f; }

        // ---------- Her kare ----------
        void Update()
        {
            float dt = Time.deltaTime;
            float t = Time.time + phase;
            float k = 1f - Mathf.Exp(-dt * 10f);

            face.brow = Mathf.Lerp(face.brow, faceTarget.brow, k);
            face.browY = Mathf.Lerp(face.browY, faceTarget.browY, k);
            face.eye = Mathf.Lerp(face.eye, faceTarget.eye, k);
            face.mouthW = Mathf.Lerp(face.mouthW, faceTarget.mouthW, k);
            face.mouthH = Mathf.Lerp(face.mouthH, faceTarget.mouthH, k);
            face.skin = Mathf.Lerp(face.skin, faceTarget.skin, k);
            face.mouth = faceTarget.mouth; face.tears = faceTarget.tears; face.blush = faceTarget.blush; face.hearts = faceTarget.hearts;
            tilt = Mathf.Lerp(tilt, TiltTarget, 1f - Mathf.Exp(-dt * 5f));
            pop = Mathf.MoveTowards(pop, 0f, dt * 5f);
            focus = Mathf.Lerp(focus, focusTarget, 1f - Mathf.Exp(-dt * 6f));

            // goz kirpma
            if (Time.time > blinkNext) { blinkUntil = Time.time + 0.12f; blinkNext = Time.time + Random.Range(2f, 5f); }
            bool blink = Time.time < blinkUntil;

            // dusunurken gozler saga sola
            if (Thinking && Time.time > lookNext) { lookX = lookX > 0 ? -1f : 1f; lookNext = Time.time + Random.Range(0.6f, 1.2f); }
            if (!Thinking) lookX = Mathf.MoveTowards(lookX, 0, dt * 3f);

            // konum: yurume/kosma sallanmasi, ziplama
            float bob = 0, lean = 0, swing = 0;
            if (moving)
            {
                walkPhase += dt * (running ? 16f : 9f);
                bob = Mathf.Abs(Mathf.Sin(walkPhase)) * (running ? 24f : 12f);
                swing = Mathf.Sin(walkPhase) * (running ? 42f : 24f);
                lean = running ? -9f * dir : -2f * dir;
            }
            if (jumpT >= 0)
            {
                jumpT += dt;
                float p = jumpT / 0.55f;
                bob += Mathf.Sin(Mathf.Clamp01(p) * Mathf.PI) * 95f;
                if (p >= 1) jumpT = -1;
            }
            float shakeX = Shaking ? Mathf.Sin(t * 38f) * 6f : 0f;
            float shakeY = Shaking ? Mathf.Abs(Mathf.Sin(t * 19f)) * 5f : 0f;

            bool lying = pose == "yatiyor";
            root.anchoredPosition = basePos + (lying ? Vector2.zero : new Vector2(0, bob));
            if (!lying) root.localEulerAngles = new Vector3(0, 0, lean);
            float s = Def.scale * (1f + pop * 0.035f);
            root.localScale = new Vector3(moving && dir < 0 ? -s : s, s, 1f);

            float breathe = Mathf.Sin(t * 1.8f);
            if (sprImage != null)
            {
                body.localScale = new Vector3(1f + shakeX * 0.002f, 1f + breathe * 0.012f + (Talking ? MouthLevel * 0.01f : 0), 1f);
                body.anchoredPosition = new Vector2(shakeX, shakeY);
                body.localEulerAngles = new Vector3(0, 0, tilt * 0.4f);
                if (spr != null)
                {
                    Sprite want = spr.body;
                    if (blink && spr.eyesClosed != null) want = spr.eyesClosed;
                    if (Talking && MouthLevel > 0.12f && spr.mouthOpen != null) want = spr.mouthOpen;
                    if (sprImage.sprite != want) sprImage.sprite = want;
                }
            }
            else
            {
                body.localScale = new Vector3(1f, 1f + breathe * 0.015f, 1f);
                body.anchoredPosition = new Vector2(shakeX, 250 + shakeY);
                head.anchoredPosition = headBase + new Vector2(shakeX * 1.2f, breathe * 3f + shakeY +
                    (Talking ? Mathf.Abs(Mathf.Sin(t * 10f)) * MouthLevel * 7f : 0));
                head.localEulerAngles = new Vector3(0, 0, tilt + Mathf.Sin(t * 0.7f) * 1.6f +
                    (Talking ? Mathf.Sin(t * 3.1f) * 1.8f : 0f));
                float armIdle = Mathf.Sin(t * 1.2f) * 3f;
                float armUp = Shaking ? 12f : 0f;
                armL.localEulerAngles = new Vector3(0, 0, -swing + armIdle - armUp);
                armR.localEulerAngles = new Vector3(0, 0, swing - armIdle + armUp);
                armL.anchoredPosition = new Vector2(-178 + shakeX, 640 + shakeY);
                armR.anchoredPosition = new Vector2(178 + shakeX, 640 + shakeY);
                legL.localEulerAngles = new Vector3(0, 0, swing * 0.8f);
                legR.localEulerAngles = new Vector3(0, 0, -swing * 0.8f);

                float open = blink ? 0.08f : face.eye;
                eyeL.localScale = eyeR.localScale = new Vector3(1f + (face.eye > 1 ? (face.eye - 1) * 0.5f : 0), open, 1);
                var look = new Vector2(lookX * 12f, -4f);
                pupilL.anchoredPosition = pupilR.anchoredPosition = look;
                browL.anchoredPosition = new Vector2(-62, 58 + face.browY);
                browR.anchoredPosition = new Vector2(62, 58 + face.browY);
                browL.localEulerAngles = new Vector3(0, 0, face.brow);
                browR.localEulerAngles = new Vector3(0, 0, -face.brow);

                if (Talking)
                {
                    float lv = Mathf.Clamp01(MouthLevel);
                    mouthO.gameObject.SetActive(true); mouthSmile.gameObject.SetActive(false);
                    mouthO.sizeDelta = new Vector2(Mathf.Lerp(52f, 78f, lv) * (face.mouthW / 90f + 0.2f), Mathf.Lerp(12f, 62f, lv));
                }
                else if (face.mouth == 0 || face.mouth == 3)
                {
                    mouthO.gameObject.SetActive(true); mouthSmile.gameObject.SetActive(false);
                    mouthO.sizeDelta = face.mouth == 0 ? new Vector2(face.mouthW, face.mouthH) : new Vector2(face.mouthW, 12f);
                }
                else
                {
                    mouthO.gameObject.SetActive(false); mouthSmile.gameObject.SetActive(true);
                    mouthSmile.sizeDelta = new Vector2(face.mouthW, face.mouthW * 0.56f);
                    mouthSmile.localEulerAngles = new Vector3(0, 0, face.mouth == 2 ? 180 : 0);
                }

                var blush = face.blush ? 0.45f : 0f;
                foreach (Transform ch in head)
                    if (ch.name.StartsWith("Yanak"))
                    {
                        var g = ch.GetComponent<Image>();
                        g.color = new Color(1f, 0.45f, 0.45f, Mathf.Lerp(g.color.a, blush, k));
                    }
            }

            // renk: odakta olmayan karakter hafif karanlik, hasta/korkmus yuz solgun
            foreach (var kv in tintables)
            {
                if (kv.Key == null) continue;
                var c = Gfx.Mul(kv.Value, focus);
                kv.Key.color = c;
            }
            if (faceAnchor != null && sprImage == null)
            {
                var g = faceAnchor.GetComponent<Image>();
                var c = Gfx.Mul(Def.skin, focus);
                g.color = new Color(c.r * face.skin, c.g * Mathf.Lerp(1f, face.skin, 0.4f), c.b * face.skin, 1f);
            }

            UpdateParticles(dt);
        }

        // ---------- Gozyasi ve kalpler ----------
        void UpdateParticles(float dt)
        {
            if (face.tears && sprImage == null)
            {
                tearTimer -= dt;
                if (tearTimer <= 0)
                {
                    tearTimer = Random.Range(0.35f, 0.7f);
                    Spawn(true, Random.value < 0.5f ? eyeL : eyeR);
                }
            }
            if (face.hearts)
            {
                heartTimer -= dt;
                if (heartTimer <= 0) { heartTimer = Random.Range(0.45f, 0.8f); Spawn(false, null); }
            }
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.life += dt;
                if (p.r == null) { particles.RemoveAt(i); continue; }
                p.r.anchoredPosition += p.v * dt;
                if (p.tear) p.v.y -= 260f * dt;
                else p.r.localEulerAngles = new Vector3(0, 0, Mathf.Sin(p.life * 4f) * 15f);
                float a = 1f - Mathf.Clamp01((p.life - p.max * 0.6f) / (p.max * 0.4f));
                var c = p.img.color; c.a = a * (p.tear ? 0.9f : 1f); p.img.color = c;
                if (p.life >= p.max) { Destroy(p.r.gameObject); particles.RemoveAt(i); }
            }
        }

        void Spawn(bool tear, RectTransform eye)
        {
            var p = new Particle();
            if (tear)
            {
                p.img = Gfx.Img("Gozyasi", head, Gfx.Circle, new Color(0.55f, 0.82f, 1f, 0.9f), new Vector2(20, 30), eye.anchoredPosition + new Vector2(eye == eyeL ? -18 : 18, -40));
                Gfx.AddOutline(p.img, 2);
                p.v = new Vector2(eye == eyeL ? -12f : 12f, -40f);
                p.max = 0.9f; p.tear = true;
            }
            else
            {
                float hy = sprImage != null ? Def.spriteSize.y : 980f;
                p.img = Gfx.Img("Kalp", fx, Gfx.Heart, new Color(1f, 0.3f, 0.42f), new Vector2(64, 64), new Vector2(Random.Range(-160f, 160f), hy));
                Gfx.AddOutline(p.img, 3);
                p.v = new Vector2(Random.Range(-25f, 25f), Random.Range(110f, 160f));
                p.max = 1.6f;
            }
            p.r = p.img.rectTransform;
            if (!tear) { p.r.anchorMin = p.r.anchorMax = new Vector2(0.5f, 0.5f); }
            particles.Add(p);
        }
    }
}
