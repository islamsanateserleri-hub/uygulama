using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    public enum HairStyle { Kisa, Kase, Uzun, Topuz, Kel }

    [System.Serializable]
    public class EmotionSprite
    {
        public string emotion = "normal";
        [Tooltip("Gozler acik, agiz kapali hali (zorunlu)")] public Sprite body;
        [Tooltip("Agiz acik hali (istege bagli, konusurken)")] public Sprite mouthOpen;
        [Tooltip("Gozler kapali hali (istege bagli, goz kirpma)")] public Sprite eyesClosed;
    }

    [System.Serializable]
    public class CharacterDef
    {
        public string id = "Karakter";
        [Header("Ses")]
        public string voice = "Puck";
        [Tooltip("Ic ses baska bir sesle okunacaksa (or. Arda'nin yetiskin zihni: Orus)")]
        public string innerVoice = "";
        public Color subtitleColor = Color.white;

        [Header("Sahne")]
        [Range(-1, 1)] public int side = 0;          // -1 sol, 0 orta, 1 sag
        [Range(0.4f, 1.3f)] public float scale = 1f; // cocuk ~0.72, yetiskin 1

        [Header("Cizim (gorsel eklenmezse kullanilir)")]
        public Color skin = new Color(1f, 0.82f, 0.68f);
        public Color hair = new Color(0.22f, 0.14f, 0.1f);
        public Color shirt = new Color(0.3f, 0.5f, 0.8f);
        public Color pants = new Color(0.25f, 0.25f, 0.32f);
        public HairStyle hairStyle = HairStyle.Kisa;
        public bool beard;

        [Header("Gorseller (istege bagli; duygu basina)")]
        public Vector2 spriteSize = new Vector2(700, 1000);
        public List<EmotionSprite> sprites = new List<EmotionSprite>();

        public bool HasSprites
        {
            get
            {
                if (sprites == null) return false;
                foreach (var s in sprites) if (s != null && s.body != null) return true;
                return false;
            }
        }

        public EmotionSprite GetSprite(string emotion)
        {
            if (sprites == null) return null;
            var k = Util.Norm(emotion);
            foreach (var e in sprites) if (e != null && e.body != null && Util.Norm(e.emotion) == k) return e;
            foreach (var e in sprites) if (e != null && e.body != null && Util.Norm(e.emotion) == "normal") return e;
            foreach (var e in sprites) if (e != null && e.body != null) return e;
            return null;
        }
    }

    [System.Serializable]
    public class BackgroundDef
    {
        public string id = "mekan";
        [Tooltip("Hazir arka plan gorseli (dikey 1080x1920 en iyisi). Bossa asagidaki renklerle cizilir.")]
        public Sprite sprite;
        public Color top = new Color(0.1f, 0.09f, 0.2f);
        public Color bottom = new Color(0.22f, 0.16f, 0.26f);
        public Color floor = new Color(0.28f, 0.2f, 0.18f);
        [Range(0.1f, 0.5f)] public float floorHeight = 0.3f;
        public bool window, moon, bed, road, table, rug;
        [Tooltip("Mekana girince acilan atmosfer: mum, toz, yagmur, buyu, gunes (virgulle)")]
        public string atmosphere = "";
    }

    [System.Serializable] public class SfxDef { public string id = "efekt"; public AudioClip clip; }
    [System.Serializable] public class InsertDef { public string id = "gorsel"; public Sprite sprite; }
    [System.Serializable] public class MusicDef { public string id = "muzik"; public AudioClip clip; }

    [CreateAssetMenu(fileName = "StoryLibrary", menuName = "Hikaye Studyo/Kutuphane")]
    public class StoryLibrary : ScriptableObject
    {
        public CharacterDef narrator = new CharacterDef
        {
            id = "Anlatici", voice = "Orus", subtitleColor = new Color(0.82f, 0.9f, 1f)
        };
        public List<CharacterDef> characters = new List<CharacterDef>();
        public List<BackgroundDef> backgrounds = new List<BackgroundDef>();
        public List<SfxDef> sfx = new List<SfxDef>();
        public List<InsertDef> inserts = new List<InsertDef>();
        public List<MusicDef> music = new List<MusicDef>();

        public CharacterDef FindCharacter(string id)
        {
            var k = Util.Norm(id);
            if (k == Util.Norm(narrator.id)) return narrator;
            foreach (var c in characters) if (c != null && Util.Norm(c.id) == k) return c;
            return null;
        }

        public bool IsNarrator(CharacterDef d) { return d == narrator; }

        public BackgroundDef FindBackground(string id)
        {
            var k = Util.Norm(id);
            foreach (var b in backgrounds) if (b != null && Util.Norm(b.id) == k) return b;
            return null;
        }

        public AudioClip FindSfx(string id)
        {
            var k = Util.Norm(id);
            foreach (var s in sfx) if (s != null && s.clip != null && Util.Norm(s.id) == k) return s.clip;
            return null;
        }

        public Sprite FindInsert(string id)
        {
            var k = Util.Norm(id);
            foreach (var s in inserts) if (s != null && s.sprite != null && Util.Norm(s.id) == k) return s.sprite;
            return null;
        }

        public AudioClip FindMusic(string id)
        {
            var k = Util.Norm(id);
            foreach (var s in music) if (s != null && s.clip != null && Util.Norm(s.id) == k) return s.clip;
            return null;
        }

        // Satiri hangi Gemini sesi okur
        public string VoiceFor(StoryItem it)
        {
            var d = FindCharacter(it.Character) ?? narrator;
            if (it.Inner && !string.IsNullOrEmpty(d.innerVoice)) return d.innerVoice;
            return d.voice;
        }

        // Ses dosyasinin adi: ses + ton + metin ozetinden. Satir eklemek digerlerini bozmaz.
        public string ClipKey(StoryItem it)
        {
            return Util.Hash(VoiceFor(it) + "|" + Util.ToneFor(it) + "|" + it.Text);
        }
    }
}
