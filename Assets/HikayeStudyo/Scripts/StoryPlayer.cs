using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Hikayeyi oynatir. Sahneye bos bir GameObject + bu bilesen yeterli; arayuzu kendisi kurar (1080x1920 dikey).
    public class StoryPlayer : MonoBehaviour
    {
        [Header("Girdi")]
        public StoryLibrary library;      // bossa Resources/StoryLibrary
        public TextAsset story;           // bossa Resources/Hikayeler/ornek
        public bool autoPlay = true;

        [Header("Tempo")]
        public float lineGap = 0.22f;
        public float charsPerSecond = 14f;

        [Header("Kamera")]
        public float closeZoom = 1.6f;
        public float pushIn = 0.05f;

        [Header("Ses")]
        [Range(0, 1)] public float ambientVolume = 0.09f;
        [Range(0, 1)] public float sfxVolume = 0.85f;
        [Range(0, 1)] public float voiceVolume = 1f;

        public static event System.Action Finished;
        public static bool Running { get; private set; }

        // ---------- ic durum ----------
        Font font, fontBlack;
        RectTransform world, bgRoot, charRoot, frontRoot, insertRt, handsRt, rewindIcon;
        Image insertImg, insertShade, fade;
        Text subtitle, bigText;
        FxLayer fx;
        AudioSource voiceSrc, sfxSrc, ambSrc, rainSrc;
        readonly Dictionary<string, CharacterView> onScreen = new Dictionary<string, CharacterView>();
        readonly Dictionary<StoryItem, AudioClip> voices = new Dictionary<StoryItem, AudioClip>();
        readonly float[] samples = new float[256];
        string storyName;

        string camMode = "otomatik";
        CharacterView camTarget;
        float camScale = 1f, camScaleT = 1f, shake;
        Vector2 camPos, camPosT;
        int wideRun;

        float floorY = -640f, bedTopY = -450f;
        bool hasBed;
        bool talkingNow;
        float lineProgress;
        string lineEmotion = "normal";
        bool paused;

        const float W = 1080f, H = 1920f;
        const float BgPadX = 500f, BgPadY = 300f;

        // ---------- Baslangic ----------
        void Start()
        {
            if (library == null) library = Resources.Load<StoryLibrary>("StoryLibrary");
            if (story == null) story = Resources.Load<TextAsset>("Hikayeler/ornek");
            if (library == null || story == null)
            {
                Debug.LogError("[Hikaye Studyosu] StoryLibrary veya hikaye bulunamadi. Menuden Hikaye > Kurulumu Yenile.");
                return;
            }
            storyName = story.name;
            Application.targetFrameRate = 30;
            BuildUI();
            var parsed = StoryParser.Parse(story.text);
            foreach (var w in parsed.Warnings) Debug.LogWarning("[Hikaye] " + w);
            PreloadVoices(parsed.Items);
            if (autoPlay) StartCoroutine(Run(parsed.Items));
        }

        static Font MakeFont(string[] names)
        {
            Font f = null;
            try { f = Font.CreateDynamicFontFromOSFont(names, 64); } catch { f = null; }
            if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return f;
        }

        void BuildUI()
        {
            font = MakeFont(new[] { "Segoe UI", "Arial", "Liberation Sans", "DejaVu Sans" });
            fontBlack = MakeFont(new[] { "Segoe UI Black", "Arial Black", "Impact", "Segoe UI", "Arial" });

            var cgo = new GameObject("HikayeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cgo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(W, H);
            sc.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var root = (RectTransform)cgo.transform;

            // Kara arka (dunya kucultuldugunde bosluk gorunmesin)
            var back = Gfx.Img("Zemin", root, null, Color.black, Vector2.zero, Vector2.zero);
            Gfx.Stretch(back.rectTransform);

            world = Gfx.Rect("Dunya", root);
            Gfx.Stretch(world);
            bgRoot = Gfx.Rect("Mekan", world);
            Gfx.Stretch(bgRoot);
            bgRoot.offsetMin = new Vector2(-BgPadX, -BgPadY);
            bgRoot.offsetMax = new Vector2(BgPadX, BgPadY);
            charRoot = Gfx.Rect("Karakterler", world);
            Gfx.Stretch(charRoot);
            frontRoot = Gfx.Rect("OnPlan", world);
            Gfx.Stretch(frontRoot);
            frontRoot.offsetMin = bgRoot.offsetMin; frontRoot.offsetMax = bgRoot.offsetMax;

            insertRt = Gfx.Rect("Gorsel", world);
            Gfx.Stretch(insertRt);
            insertShade = Gfx.Img("Golge", insertRt, null, new Color(0, 0, 0, 0.55f), Vector2.zero, Vector2.zero);
            Gfx.Stretch(insertShade.rectTransform);
            insertImg = Gfx.Img("Resim", insertRt, null, Color.white, Vector2.zero, Vector2.zero);
            Gfx.Stretch(insertImg.rectTransform);
            handsRt = Gfx.Rect("Eller", insertRt);
            Gfx.Stretch(handsRt);
            insertRt.gameObject.SetActive(false);

            fx = FxLayer.Create(root);

            fade = Gfx.Img("Kararma", root, null, Color.black, Vector2.zero, Vector2.zero);
            Gfx.Stretch(fade.rectTransform);

            // Buyuk ekran yazisi (kanca, bolum sonu)
            bigText = NewText("BuyukYazi", root, fontBlack, 120);
            bigText.resizeTextForBestFit = true; bigText.resizeTextMinSize = 56; bigText.resizeTextMaxSize = 130;
            bigText.fontStyle = FontStyle.Bold;
            AddTextEffects(bigText, 6);
            SetBigTop();

            rewindIcon = Gfx.Rect("GeriSimge", root);
            rewindIcon.anchorMin = rewindIcon.anchorMax = new Vector2(0.5f, 0.8f);
            rewindIcon.sizeDelta = new Vector2(220, 110);
            var t1 = Gfx.Img("U1", rewindIcon, Gfx.TriangleLeft, Color.white, new Vector2(100, 100), new Vector2(-48, 0));
            var t2 = Gfx.Img("U2", rewindIcon, Gfx.TriangleLeft, Color.white, new Vector2(100, 100), new Vector2(48, 0));
            Gfx.AddOutline(t1, 4); Gfx.AddOutline(t2, 4);
            rewindIcon.gameObject.SetActive(false);

            // Altyazi: TikTok alt bilgi alaninin (alt %20) ve sag dugmelerin disinda
            subtitle = NewText("Altyazi", root, font, 54);
            var srt = subtitle.rectTransform;
            srt.anchorMin = new Vector2(0.07f, 0.215f);
            srt.anchorMax = new Vector2(0.83f, 0.335f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            subtitle.fontStyle = FontStyle.BoldAndItalic;
            subtitle.resizeTextForBestFit = true; subtitle.resizeTextMinSize = 34; subtitle.resizeTextMaxSize = 58;
            subtitle.lineSpacing = 0.95f;
            AddTextEffects(subtitle, 4);

            voiceSrc = gameObject.AddComponent<AudioSource>();
            sfxSrc = gameObject.AddComponent<AudioSource>();
            ambSrc = gameObject.AddComponent<AudioSource>();
            rainSrc = gameObject.AddComponent<AudioSource>();
            foreach (var a in new[] { voiceSrc, sfxSrc, ambSrc, rainSrc }) { a.playOnAwake = false; a.spatialBlend = 0f; }
            ambSrc.loop = true; ambSrc.clip = ProcAudio.Pad(); ambSrc.volume = 0f; ambSrc.Play();
            rainSrc.loop = true; rainSrc.clip = ProcAudio.Rain(); rainSrc.volume = 0f;
            voiceSrc.volume = voiceVolume;
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
        }

        Text NewText(string name, Transform parent, Font f, int size)
        {
            var r = Gfx.Rect(name, parent);
            var t = r.gameObject.AddComponent<Text>();
            t.font = f; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false; t.text = "";
            return t;
        }

        static void AddTextEffects(Text t, float w)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = Color.black; o.effectDistance = new Vector2(w, -w);
            var o2 = t.gameObject.AddComponent<Outline>();
            o2.effectColor = Color.black; o2.effectDistance = new Vector2(-w, w);
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, 0.6f); s.effectDistance = new Vector2(w * 0.8f, -w * 1.4f);
        }

        void SetBigTop()
        {
            var r = bigText.rectTransform;
            r.anchorMin = new Vector2(0.06f, 0.63f); r.anchorMax = new Vector2(0.94f, 0.86f);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        void SetBigCenter()
        {
            var r = bigText.rectTransform;
            r.anchorMin = new Vector2(0.06f, 0.42f); r.anchorMax = new Vector2(0.94f, 0.58f);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        // ---------- Sesler ----------
        void PreloadVoices(List<StoryItem> items)
        {
            var byKey = new Dictionary<string, AudioClip>();
            int found = 0, total = 0;
            foreach (var it in items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                total++;
                string key = library.ClipKey(it);
                AudioClip clip;
                if (!byKey.TryGetValue(key, out clip))
                {
                    clip = Resources.Load<AudioClip>("Ses/" + storyName + "/" + key);
                    if (clip == null) clip = Resources.Load<AudioClip>("Ses/" + storyName + "/" + it.VoiceIndex.ToString("00"));
                    if (clip != null) clip = ProcAudio.Clean(clip);
                    byKey[key] = clip;
                }
                if (clip != null) { voices[it] = clip; found++; }
            }
            if (found < total)
                Debug.LogWarning("[Hikaye] " + (total - found) + "/" + total + " satırın sesi yok; süre metinden tahmin edilecek. (Hikaye > Stüdyo > Eksik sesleri üret)");
        }

        float Level()
        {
            voiceSrc.GetOutputData(samples, 0);
            float sum = 0;
            for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
            return Mathf.Clamp01(Mathf.Sqrt(sum / samples.Length) * 5f);
        }

        AudioClip FindSfx(string id)
        {
            var c = library.FindSfx(id);
            if (c == null) c = ProcAudio.Get(id);
            return c;
        }

        // ---------- Oynatma ----------
        IEnumerator Run(List<StoryItem> items)
        {
            Running = true;
            fade.color = Color.black;
            yield return new WaitForSeconds(0.35f);   // kayit baslasin diye kisa siyah
            bool faded = false;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                switch (it.Type)
                {
                    case ItemType.Background:
                        yield return SetBackground(it.Arg);
                        faded = true;
                        break;
                    case ItemType.Hook:
                        yield return Hook(it.Text);
                        break;
                    case ItemType.Rewind:
                        yield return Rewind(it.Text);
                        break;
                    case ItemType.Title:
                        if (!faded) { yield return Fade(1f, 0f, 0.3f); faded = true; }
                        yield return Title(it.Text, NextIsEnd(items, i));
                        break;
                    case ItemType.Wait:
                        yield return new WaitForSeconds(it.Seconds);
                        break;
                    case ItemType.Sfx:
                        PlaySfx(it.Arg);
                        break;
                    case ItemType.Remove:
                        StartCoroutine(Exit(it.Character));
                        break;
                    case ItemType.Clear:
                        ClearChars();
                        break;
                    case ItemType.Camera:
                        CameraCommand(it.Arg);
                        break;
                    case ItemType.Atmosphere:
                        fx.SetAtmosphere(Util.Norm(it.Arg) == "yok" ? "" : it.Arg);
                        break;
                    case ItemType.Music:
                        SetMusic(it.Arg);
                        break;
                    case ItemType.Pose:
                        {
                            var def = library.FindCharacter(it.Character);
                            if (def == null || library.IsNarrator(def)) { Debug.LogWarning("[Hikaye] Karakter yok: " + it.Character); break; }
                            var v = GetOrCreate(def, "");
                            v.SetPose(it.Arg, PosePosition(v, it.Arg));
                            UpdateBlanket(v);
                        }
                        break;
                    case ItemType.Headlights:
                        if (!faded) { fade.color = new Color(0, 0, 0, 0); faded = true; }
                        yield return fx.Headlights(1.5f);
                        shake = 1f;
                        break;
                    case ItemType.Flash:
                        fx.Flash(1f);
                        break;
                    case ItemType.Insert:
                        ShowInsert(it.Arg);
                        break;
                    case ItemType.Dialogue:
                        if (!faded) { yield return Fade(1f, 0f, 0.3f); faded = true; }
                        yield return Say(it);
                        yield return new WaitForSeconds(lineGap);
                        break;
                }
            }

            subtitle.text = "";
            yield return Fade(fade.color.a, 1f, 0.7f);
            bigText.text = "";
            Running = false;
            Debug.Log("[Hikaye] Bitti.");
            if (Finished != null) Finished();
        }

        static bool NextIsEnd(List<StoryItem> items, int i)
        {
            for (int k = i + 1; k < items.Count; k++)
                if (items[k].Type == ItemType.Dialogue || items[k].Type == ItemType.Background) return false;
            return true;
        }

        // ---------- Konusma ----------
        IEnumerator Say(StoryItem it)
        {
            var def = library.FindCharacter(it.Character);
            if (def == null) { Debug.LogWarning("[Hikaye] " + it.Line + ". satır: karakter yok: " + it.Character + " (anlatıcı sesiyle okunuyor)"); def = library.narrator; }
            bool narr = library.IsNarrator(def);
            string emo = Util.Norm(it.Emotion);
            CharacterView view = null;

            if (!narr)
            {
                view = GetOrCreate(def, it.Motion);
                view.SetEmotion(it.Emotion);
                foreach (var kv in onScreen) kv.Value.SetFocus(kv.Value == view || onScreen.Count == 1);
                ApplyMotion(view, it.Motion);
                view.Shaking = it.Motion == "sarsil" || emo == "aglamakli";
                view.TiltTarget = it.Motion == "egil" ? 11f : Random.Range(-4f, 4f);
                view.Thinking = emo == "dusunceli";
            }
            else foreach (var kv in onScreen) kv.Value.SetFocus(true);

            fx.Dim(it.Inner && emo == "dusunceli" ? 0.22f : (it.Inner ? 0.1f : 0f));
            ChooseCamera(it, view);

            subtitle.text = it.Text;
            subtitle.color = def.subtitleColor;
            StartCoroutine(PopText(subtitle.rectTransform, 0.92f));

            AudioClip clip;
            voices.TryGetValue(it, out clip);
            float dur = clip != null ? clip.length : Mathf.Max(1.2f, it.Text.Length / charsPerSecond + 0.3f);
            if (clip != null) { voiceSrc.clip = clip; voiceSrc.volume = voiceVolume; voiceSrc.Play(); }

            bool talk = view != null && !it.Inner;
            talkingNow = true; lineEmotion = emo;
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                lineProgress = Mathf.Clamp01(t / dur);
                if (talk)
                {
                    view.Talking = true;
                    view.MouthLevel = clip != null ? Level()
                        : Mathf.Clamp01(Mathf.PerlinNoise(Time.time * 9f, 0.5f) * 1.4f - 0.2f);
                }
                yield return null;
            }
            if (view != null) { view.Talking = false; view.MouthLevel = 0; view.Shaking = false; view.Thinking = false; view.TiltTarget = 0; }
            talkingNow = false; lineProgress = 0;
            fx.Dim(0f);
            subtitle.text = "";
        }

        IEnumerator PopText(RectTransform r, float from)
        {
            float t = 0;
            while (t < 0.14f)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(from, 1f, t / 0.14f);
                r.localScale = new Vector3(s, s, 1);
                yield return null;
            }
            r.localScale = Vector3.one;
        }

        // ---------- Karakterler ----------
        // Biri yatakta yatiyorsa ayaktakiler yuzunu kapatmasin diye yatagin ayak tarafina dizilir
        float SlotX(CharacterDef d)
        {
            foreach (var kv in onScreen) if (kv.Value != null && kv.Value.Pose == "yatiyor") return 110f + (d.side + 1) * 140f;
            return d.side * 285f;
        }

        CharacterView GetOrCreate(CharacterDef def, string motion)
        {
            var key = Util.Norm(def.id);
            CharacterView v;
            if (onScreen.TryGetValue(key, out v) && v != null) return v;
            v = CharacterView.Create(charRoot, def);
            v.SlotX = SlotX(def);
            bool enter = motion == "kos" || motion == "yuru" || motion == "saril";
            float startX = enter ? (def.side > 0 ? 820f : -820f) : v.SlotX;
            v.Place(new Vector2(startX, floorY));
            onScreen[key] = v;
            return v;
        }

        void ApplyMotion(CharacterView v, string m)
        {
            switch (m)
            {
                case "kos": StartCoroutine(v.MoveTo(v.SlotX, true)); break;
                case "yuru": StartCoroutine(v.MoveTo(v.SlotX, false)); break;
                case "zipla": v.Jump(); break;
                case "cik": StartCoroutine(Exit(v.Def.id)); break;
                case "saril":
                    {
                        CharacterView other = null;
                        foreach (var kv in onScreen) if (kv.Value != v) { other = kv.Value; break; }
                        float x = other != null ? other.Position.x + (v.Position.x < other.Position.x ? -170f : 170f) : v.SlotX;
                        if (other != null && other.Pose == "yatiyor") x = -20f;
                        StartCoroutine(v.MoveTo(x, true));
                    }
                    break;
            }
        }

        Vector2 PosePosition(CharacterView v, string pose)
        {
            float s = v.Def.scale;
            bool spr = v.Def.HasSprites;
            float headUp = spr ? v.Def.spriteSize.y * 0.8f : 786f;
            float half = spr ? v.Def.spriteSize.x * 0.25f : 150f;
            float top = hasBed ? bedTopY : floorY + 230f;
            switch (Util.Norm(pose))
            {
                case "yatiyor": return new Vector2(-330f + headUp * s, top + half * s * 0.7f);
                case "oturuyor": return new Vector2(hasBed ? -150f : v.SlotX, (hasBed ? bedTopY : floorY + 60f) - 250f * s);
                default: return new Vector2(v.SlotX, floorY);
            }
        }

        // Yatakta yatan karakterin uzerine yorgan (on planda)
        void UpdateBlanket(CharacterView v)
        {
            var old = charRoot.Find("Yorgan_" + v.Def.id);
            if (old != null) Destroy(old.gameObject);
            if (v.Pose != "yatiyor" || !hasBed) return;
            float headX = -330f;
            float left = headX + 120f, right = 450f;
            // yatan karakterin hemen ustunde, sonradan gelen (ayaktaki) karakterlerin altinda durur
            var b = Box(charRoot, Gfx.RoundRect, new Color(0.42f, 0.58f, 0.85f), new Vector2(right - left, 170f),
                        new Vector2((left + right) / 2f, v.Position.y - 10f), 5);
            b.name = "Yorgan_" + v.Def.id;
            b.transform.SetSiblingIndex(v.transform.GetSiblingIndex() + 1);
            Box(b.rectTransform, Gfx.RoundRect, new Color(0.95f, 0.95f, 0.98f), new Vector2(60f, 170f), new Vector2(-(right - left) / 2f + 30f, 0), 0);
            for (int i = 0; i < 3; i++)
                Box(b.rectTransform, Gfx.RoundRect, new Color(0.34f, 0.48f, 0.75f), new Vector2(70, 60),
                    new Vector2(-60 + i * 130, i % 2 == 0 ? 20 : -25), 3);
        }

        IEnumerator Exit(string id)
        {
            var k = Util.Norm(id);
            CharacterView v;
            if (!onScreen.TryGetValue(k, out v) || v == null) yield break;
            onScreen.Remove(k);
            var bl = charRoot.Find("Yorgan_" + v.Def.id);
            if (bl != null) Destroy(bl.gameObject);
            if (v.Pose != "yatiyor") yield return v.MoveTo(v.Position.x >= 0 ? 900f : -900f, false);
            if (camTarget == v) { camTarget = null; camScaleT = 1f; camPosT = Vector2.zero; }
            Destroy(v.gameObject);
        }

        void ClearChars()
        {
            foreach (var kv in onScreen) if (kv.Value != null) Destroy(kv.Value.gameObject);
            onScreen.Clear();
            for (int i = charRoot.childCount - 1; i >= 0; i--)
                if (charRoot.GetChild(i).name.StartsWith("Yorgan_")) Destroy(charRoot.GetChild(i).gameObject);
            camTarget = null; camScaleT = 1f; camPosT = Vector2.zero;
        }

        // ---------- Kamera ----------
        void CameraCommand(string a)
        {
            var n = Util.Norm(a);
            if (n == "salla") { shake = 1f; return; }
            if (n == "yakin" || n == "genis" || n == "otomatik") camMode = n;
            if (n == "genis") { camTarget = null; camScaleT = 1f; camPosT = Vector2.zero; }
        }

        void ChooseCamera(StoryItem it, CharacterView v)
        {
            string e = Util.Norm(it.Emotion);
            bool strong = e == "aglamakli" || e == "korkmus" || e == "sasirmis" || e == "sevgi" || e == "kizgin" || e == "uzgun";
            bool close;
            if (camMode == "yakin") close = v != null;
            else if (camMode == "genis") close = false;
            else close = v != null && (strong || it.Inner || wideRun >= 2 || v.Pose == "yatiyor");
            if (close) { wideRun = 0; camTarget = v; camScaleT = closeZoom; }
            else { wideRun++; camTarget = null; camScaleT = 1f; camPosT = Vector2.zero; }
        }

        void UpdateCamera(float dt)
        {
            float target = camScaleT + (talkingNow ? pushIn * lineProgress : 0f);
            if (camTarget != null)
            {
                var local = world.InverseTransformPoint(camTarget.FaceWorld);
                camPosT = new Vector2(-local.x, -local.y) * target;
            }
            float k = 1f - Mathf.Exp(-dt * 5f);
            camScale = Mathf.Lerp(camScale, target, k);
            float mx = Mathf.Max(0f, (W / 2f + BgPadX) * camScale - W / 2f);
            float my = Mathf.Max(0f, (H / 2f + BgPadY) * camScale - H / 2f);
            var want = new Vector2(Mathf.Clamp(camPosT.x, -mx, mx), Mathf.Clamp(camPosT.y, -my, my));
            camPos = Vector2.Lerp(camPos, want, k);
            shake = Mathf.MoveTowards(shake, 0f, dt * 2.5f);
            var sh = shake > 0 ? new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shake * 28f : Vector2.zero;
            if (!talkingNow && camTarget == null) camPos = Vector2.Lerp(camPos, want + new Vector2(Mathf.Sin(Time.time * 0.4f) * 6f, 0), k);
            world.localScale = new Vector3(camScale, camScale, 1f);
            world.anchoredPosition = camPos + sh;
        }

        // ---------- Mekan ----------
        IEnumerator SetBackground(string id)
        {
            if (fade.color.a < 0.99f) yield return Fade(fade.color.a, 1f, 0.25f);
            ClearChars();
            ShowInsert("yok");
            var b = library.FindBackground(id);
            if (b == null) Debug.LogWarning("[Hikaye] Mekan yok: " + id + " (Kütüphaneye ekle)");
            BuildBackground(b);
            fx.SetAtmosphere(b != null ? b.atmosphere : "");
            camScale = 1.08f; // girerken hafif yaklasan kamera
            yield return Fade(1f, 0f, 0.35f);
        }

        Image Box(Transform parent, Sprite s, Color c, Vector2 size, Vector2 pos, float outline = 0)
        {
            var img = Gfx.Img("p", parent, s, c, size, pos);
            if (outline > 0) Gfx.AddOutline(img, outline);
            return img;
        }

        void BuildBackground(BackgroundDef b)
        {
            for (int i = bgRoot.childCount - 1; i >= 0; i--) Destroy(bgRoot.GetChild(i).gameObject);
            for (int i = frontRoot.childCount - 1; i >= 0; i--) Destroy(frontRoot.GetChild(i).gameObject);
            hasBed = false;
            if (b == null) { floorY = -640f; return; }

            float fh = H * b.floorHeight;
            float floorTop = -H / 2f + fh;
            floorY = floorTop - fh * 0.45f;
            bedTopY = floorTop + 90f;   // yatak duvar dibinde: yatan karakterin yuzu altyaziyla cakismaz
            hasBed = b.bed;

            if (b.sprite != null)
            {
                var img = Gfx.Img("Resim", bgRoot, b.sprite, Color.white, Vector2.zero, Vector2.zero);
                Gfx.Stretch(img.rectTransform);
                var fit = img.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = b.sprite.rect.width / Mathf.Max(1f, b.sprite.rect.height);
                return;
            }

            var grad = Gfx.Rect("Gok", bgRoot).gameObject.AddComponent<RawImage>();
            Gfx.Stretch(grad.rectTransform);
            grad.texture = Gfx.Gradient(b.top, b.bottom);
            grad.raycastTarget = false;

            var wood = new Color(0.36f, 0.22f, 0.14f);

            if (b.road)
            {
                // ufukta sehir silueti
                var rnd = new System.Random(3);
                for (int i = 0; i < 14; i++)
                {
                    float w = 90 + rnd.Next(0, 90), h = 160 + rnd.Next(0, 380);
                    float x = -1000 + i * 150 + rnd.Next(-20, 20);
                    var bld = Box(bgRoot, null, Gfx.Mul(b.top, 0.6f), new Vector2(w, h), new Vector2(x, floorTop + h / 2f));
                    for (int j = 0; j < 4; j++)
                        if (rnd.NextDouble() < 0.5)
                            Box(bld.rectTransform, null, new Color(1f, 0.85f, 0.45f, 0.7f), new Vector2(14, 18),
                                new Vector2(rnd.Next(-30, 30), rnd.Next(-(int)(h / 2) + 20, (int)(h / 2) - 20)));
                }
            }

            if (b.window)
            {
                var night = b.moon;
                var pos = new Vector2(-250, 380);
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(340, 430), pos, 5);
                var pane = Gfx.Rect("Cam", bgRoot).gameObject.AddComponent<RawImage>();
                pane.raycastTarget = false;
                pane.rectTransform.sizeDelta = new Vector2(296, 386);
                pane.rectTransform.anchoredPosition = pos;
                pane.texture = night ? Gfx.Gradient(new Color(0.05f, 0.08f, 0.22f), new Color(0.18f, 0.22f, 0.42f))
                                     : Gfx.Gradient(new Color(0.55f, 0.78f, 1f), new Color(0.92f, 0.96f, 1f));
                if (night)
                {
                    Box(bgRoot, Gfx.SoftDot, new Color(1f, 0.95f, 0.75f, 0.5f), new Vector2(200, 200), pos + new Vector2(55, 100));
                    Box(bgRoot, Gfx.Circle, new Color(1f, 0.97f, 0.86f), new Vector2(84, 84), pos + new Vector2(55, 100), 3);
                    for (int i = 0; i < 6; i++)
                        Box(bgRoot, Gfx.SoftDot, Color.white, new Vector2(10, 10), pos + new Vector2(-110 + i * 40, 40 + (i % 3) * 45));
                }
                Box(bgRoot, null, wood, new Vector2(14, 386), pos);
                Box(bgRoot, null, wood, new Vector2(296, 14), pos);
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(400, 34), pos + new Vector2(0, -225), 4);
                Box(bgRoot, Gfx.RoundRect, new Color(0.6f, 0.22f, 0.26f), new Vector2(110, 500), pos + new Vector2(-195, -20), 4);
                Box(bgRoot, Gfx.RoundRect, new Color(0.6f, 0.22f, 0.26f), new Vector2(110, 500), pos + new Vector2(195, -20), 4);
            }

            // duvardaki raf ve takvim (ev icinde)
            if (b.window || b.bed || b.table)
            {
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(300, 28), new Vector2(270, 500), 4);
                Box(bgRoot, Gfx.RoundRect, new Color(0.75f, 0.35f, 0.3f), new Vector2(60, 90), new Vector2(200, 560), 3);
                Box(bgRoot, Gfx.Circle, new Color(0.4f, 0.6f, 0.85f), new Vector2(70, 70), new Vector2(290, 550), 3);
                Box(bgRoot, Gfx.RoundRect, new Color(0.85f, 0.7f, 0.35f), new Vector2(46, 70), new Vector2(360, 550), 3);
                Box(bgRoot, Gfx.RoundRect, new Color(0.95f, 0.93f, 0.88f), new Vector2(150, 180), new Vector2(330, 250), 3);
                Box(bgRoot, Gfx.RoundRect, new Color(0.8f, 0.2f, 0.2f), new Vector2(150, 44), new Vector2(330, 318), 0);
            }

            // zemin
            var floor = Box(bgRoot, null, b.floor, Vector2.zero, Vector2.zero);
            var fr = floor.rectTransform;
            fr.anchorMin = new Vector2(0, 0); fr.anchorMax = new Vector2(1, 0); fr.pivot = new Vector2(0.5f, 0f);
            fr.sizeDelta = new Vector2(0, fh + BgPadY); fr.anchoredPosition = Vector2.zero;
            var edge = Box(bgRoot, null, Gfx.Mul(b.floor, 0.7f), Vector2.zero, Vector2.zero).rectTransform;
            edge.anchorMin = new Vector2(0, 0); edge.anchorMax = new Vector2(1, 0); edge.pivot = new Vector2(0.5f, 0f);
            edge.sizeDelta = new Vector2(0, 14); edge.anchoredPosition = new Vector2(0, fh + BgPadY - 7);

            if (b.road)
            {
                var road = Box(bgRoot, null, new Color(0.13f, 0.13f, 0.15f), Vector2.zero, Vector2.zero).rectTransform;
                road.anchorMin = new Vector2(0, 0); road.anchorMax = new Vector2(1, 0); road.pivot = new Vector2(0.5f, 0f);
                road.sizeDelta = new Vector2(0, fh * 0.75f); road.anchoredPosition = new Vector2(0, BgPadY);
                for (int i = 0; i < 9; i++)
                    Box(bgRoot, null, new Color(0.95f, 0.9f, 0.6f, 0.9f), new Vector2(110, 14), new Vector2(-1000 + i * 260, floorTop - fh * 0.37f));
                for (int i = 0; i < 3; i++)
                {
                    float x = -420 + i * 420;
                    Box(bgRoot, null, new Color(0.2f, 0.2f, 0.22f), new Vector2(16, 520), new Vector2(x, floorTop + 260));
                    Box(bgRoot, Gfx.SoftDot, new Color(1f, 0.85f, 0.5f, 0.55f), new Vector2(260, 260), new Vector2(x, floorTop + 520));
                    Box(bgRoot, Gfx.Circle, new Color(1f, 0.9f, 0.6f), new Vector2(40, 40), new Vector2(x, floorTop + 520), 3);
                }
            }

            if (b.rug)
                Box(bgRoot, Gfx.Circle, new Color(0.62f, 0.2f, 0.18f), new Vector2(820, 190), new Vector2(150, floorY - 10), 5)
                    .gameObject.AddComponent<Outline>().effectColor = new Color(0.9f, 0.75f, 0.4f);

            if (b.bed)
            {
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(80, 380), new Vector2(-470, bedTopY + 40), 5);
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(940, 170), new Vector2(0, bedTopY - 115), 5);
                Box(bgRoot, Gfx.RoundRect, new Color(0.95f, 0.92f, 0.85f), new Vector2(900, 86), new Vector2(0, bedTopY - 38), 4);
                Box(bgRoot, Gfx.RoundRect, Color.white, new Vector2(230, 96), new Vector2(-330, bedTopY + 12), 4);
                // yanindaki tahta tas ve islak bezler (hastalik oldugunu gosterir)
                Box(bgRoot, Gfx.RoundRect, wood, new Vector2(170, 160), new Vector2(390, bedTopY + 60), 4);
                Box(bgRoot, Gfx.Circle, new Color(0.55f, 0.35f, 0.2f), new Vector2(130, 60), new Vector2(390, bedTopY + 160), 4);
                Box(bgRoot, Gfx.RoundRect, new Color(0.85f, 0.9f, 0.95f), new Vector2(100, 40), new Vector2(370, bedTopY + 185), 3);
            }

            if (b.table)
            {
                float ty = floorY + 330f;
                Box(frontRoot, Gfx.RoundRect, Gfx.Mul(wood, 0.85f), new Vector2(980, 230), new Vector2(0, ty - 130), 5);
                Box(frontRoot, Gfx.RoundRect, wood, new Vector2(1040, 52), new Vector2(0, ty), 5);
                Box(frontRoot, Gfx.Circle, new Color(0.55f, 0.33f, 0.18f), new Vector2(220, 100), new Vector2(-60, ty + 50), 4);
                Box(frontRoot, Gfx.Circle, new Color(0.9f, 0.75f, 0.4f), new Vector2(170, 46), new Vector2(-60, ty + 80), 0);
                Box(frontRoot, Gfx.Circle, new Color(0.85f, 0.6f, 0.35f), new Vector2(140, 90), new Vector2(220, ty + 45), 4);
                for (int i = 0; i < 3; i++)
                    Box(frontRoot, Gfx.SoftDot, new Color(1, 1, 1, 0.25f), new Vector2(60, 90), new Vector2(-90 + i * 30, ty + 150 + i * 40)).name = "Buhar";
            }
        }

        // ---------- Ozel anlar ----------
        IEnumerator Hook(string text)
        {
            yield return Fade(fade.color.a, 1f, 0.08f);
            ClearChars();
            SetBigCenter();
            bigText.text = text;
            bigText.color = Color.white;
            var clip = FindSfx("kalp");
            if (clip != null) sfxSrc.PlayOneShot(clip, sfxVolume);
            float t = 0;
            var r = bigText.rectTransform;
            while (t < 1.7f)
            {
                t += Time.deltaTime;
                float s = t < 0.12f ? Mathf.Lerp(1.5f, 1f, t / 0.12f) : 1f;
                r.localScale = new Vector3(s, s, 1);
                r.anchoredPosition = t < 0.35f ? new Vector2(Random.Range(-8f, 8f), Random.Range(-6f, 6f)) : Vector2.zero;
                yield return null;
            }
            r.localScale = Vector3.one; r.anchoredPosition = Vector2.zero;
            bigText.text = "";
            SetBigTop();
        }

        IEnumerator Rewind(string text)
        {
            fx.Rewinding = true;
            var clip = FindSfx("geri");
            if (clip != null) sfxSrc.PlayOneShot(clip, sfxVolume);
            bigText.text = text;
            bigText.color = Color.white;
            rewindIcon.gameObject.SetActive(true);
            float t = 0;
            while (t < 1.4f)
            {
                t += Time.deltaTime;
                fade.color = new Color(0, 0, 0, 0.55f + Random.value * 0.25f);
                bigText.rectTransform.anchoredPosition = new Vector2(Random.Range(-10f, 10f), Random.Range(-4f, 4f));
                rewindIcon.anchoredPosition = new Vector2(Random.Range(-6f, 6f), 0);
                bigText.color = Random.value < 0.15f ? new Color(0.6f, 1f, 1f) : Color.white;
                yield return null;
            }
            bigText.rectTransform.anchoredPosition = Vector2.zero;
            bigText.text = "";
            bigText.color = Color.white;
            rewindIcon.gameObject.SetActive(false);
            fx.Rewinding = false;
            yield return Fade(fade.color.a, 1f, 0.15f);
            ClearChars();
        }

        IEnumerator Title(string text, bool isEnd)
        {
            SetBigTop();
            bigText.text = text;
            bigText.color = Color.white;
            var clip = FindSfx("whoosh");
            if (clip != null) sfxSrc.PlayOneShot(clip, sfxVolume * 0.6f);
            yield return PopText(bigText.rectTransform, 1.35f);
            yield return new WaitForSeconds(isEnd ? 2.6f : 2.0f);
            if (!isEnd) bigText.text = "";
        }

        void PlaySfx(string id)
        {
            var clip = FindSfx(id);
            if (clip == null) { Debug.LogWarning("[Hikaye] Efekt yok: " + id + " (Kütüphanede sfx listesine ekle)"); return; }
            sfxSrc.PlayOneShot(clip, sfxVolume);
            var n = Util.Norm(id);
            if (n == "carpma") { shake = 1.2f; fx.Flash(0.8f); }
            if (n == "gok" || n == "gokgurultusu") fx.Flash(0.6f);
        }

        void SetMusic(string id)
        {
            var n = Util.Norm(id);
            if (n == "yok") { ambSrc.Stop(); return; }
            var clip = n.Length == 0 || n == "pad" || n == "ambiyans" ? ProcAudio.Pad() : library.FindMusic(id);
            if (clip == null) { Debug.LogWarning("[Hikaye] Müzik yok: " + id); return; }
            ambSrc.clip = clip; ambSrc.loop = true; ambSrc.Play();
        }

        void ShowInsert(string id)
        {
            var n = Util.Norm(id);
            for (int i = handsRt.childCount - 1; i >= 0; i--) Destroy(handsRt.GetChild(i).gameObject);
            if (n.Length == 0 || n == "yok") { insertRt.gameObject.SetActive(false); return; }
            var spr = library.FindInsert(id);
            insertRt.gameObject.SetActive(true);
            if (spr != null)
            {
                insertImg.sprite = spr; insertImg.color = Color.white; insertImg.enabled = true;
                insertShade.enabled = false;
                return;
            }
            insertImg.enabled = false;
            insertShade.enabled = true;
            if (n == "eller")
            {
                // POV: karakter kendi ellerine bakiyor
                Color skin = new Color(1f, 0.82f, 0.68f);
                foreach (var kv in onScreen) { skin = kv.Value.Def.skin; break; }
                for (int side = -1; side <= 1; side += 2)
                {
                    var hand = Gfx.Rect("El", handsRt);
                    hand.anchoredPosition = new Vector2(side * 200f, -420f);
                    hand.localEulerAngles = new Vector3(0, 0, side * -14f);
                    Box(hand, Gfx.RoundRect, skin, new Vector2(190, 520), new Vector2(0, -330), 6);
                    Box(hand, Gfx.RoundRect, skin, new Vector2(300, 300), new Vector2(0, 0), 6);
                    for (int f = 0; f < 4; f++)
                        Box(hand, Gfx.RoundRect, skin, new Vector2(62, 190), new Vector2(-105 + f * 70, 210 - Mathf.Abs(f - 1.5f) * 22f), 5);
                    var thumb = Box(hand, Gfx.RoundRect, skin, new Vector2(64, 170), new Vector2(-side * 170, 40), 5).rectTransform;
                    thumb.localEulerAngles = new Vector3(0, 0, side * 40f);
                }
                return;
            }
            Debug.LogWarning("[Hikaye] Görsel yok: " + id + " (Kütüphanede inserts listesine ekle)");
            insertRt.gameObject.SetActive(false);
        }

        IEnumerator Fade(float from, float to, float dur)
        {
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                fade.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / dur));
                yield return null;
            }
            fade.color = new Color(0, 0, 0, to);
        }

        // ---------- Her kare ----------
        void Update()
        {
            if (world == null) return;
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
            {
                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
                AudioListener.pause = paused;
            }
#endif
            float dt = Time.deltaTime;
            UpdateCamera(dt);

            // muzik: konusurken kisilir, duygusal anda biraz yukselir
            bool emotional = lineEmotion == "aglamakli" || lineEmotion == "sevgi" || lineEmotion == "uzgun";
            float amb = talkingNow ? ambientVolume * 0.45f : ambientVolume * (emotional ? 1.5f : 1f);
            if (talkingNow && emotional) amb = ambientVolume * 0.8f;
            ambSrc.volume = Mathf.MoveTowards(ambSrc.volume, amb, dt * 0.25f);

            float rv = fx.Raining ? 0.3f : 0f;
            rainSrc.volume = Mathf.MoveTowards(rainSrc.volume, rv, dt * 0.4f);
            if (rainSrc.volume > 0.001f && !rainSrc.isPlaying) rainSrc.Play();
            if (rainSrc.volume <= 0.001f && rainSrc.isPlaying) rainSrc.Stop();

            // POV ellerinde hafif titreme
            if (handsRt != null && handsRt.gameObject.activeInHierarchy)
                handsRt.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 2.1f) * 8f, Mathf.Sin(Time.time * 1.3f) * 10f);
            // sofradaki buhar
            foreach (Transform c in frontRoot)
                if (c.name == "Buhar")
                {
                    var r = (RectTransform)c;
                    var p = r.anchoredPosition; p.y += dt * 30f; p.x += Mathf.Sin(Time.time * 2f + p.y * 0.02f) * dt * 20f;
                    var img = c.GetComponent<Image>();
                    float a = img.color.a - dt * 0.08f;
                    if (a <= 0) { p.y -= 120f; a = 0.25f; }
                    img.color = new Color(1, 1, 1, a);
                    r.anchoredPosition = p;
                }
        }

        void OnDestroy()
        {
            Running = false;
            if (paused) { Time.timeScale = 1f; AudioListener.pause = false; }
        }
    }
}
