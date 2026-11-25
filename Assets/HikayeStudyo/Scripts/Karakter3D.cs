using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace HikayeStudyo
{
    // 3D karakteri adla oynatir: Play("Walk_Loop"). Animasyonlar arasinda yumusak gecis yapar.
    // Insan karakterlerde ortak animasyon kutuphanesi (Universal Animation Library) kullanilir;
    // kendi animasyonu olan modellerde (hayvanlar, oyuncak karakterler) kendi klipleri.
    public class Karakter3D : MonoBehaviour
    {
        public string id = "";
        public string kategori = "";          // Erkek, Kadin, Hayvan, Oyuncak
        public string startClip = "Idle_Loop";
        public List<AnimationClip> clips = new List<AnimationClip>();
        [Tooltip("Otomatik canlilik (goz kirpma, nefes, bakis, agiz hareketi). Kapatmak icin isareti kaldir.")]
        public bool canliLik = true;
        [Tooltip("VRoid (VRM) karakterlerinde ten ve beyaz renkler parlamasin diye renk carpani.")]
        [Range(0.4f, 1f)] public float vrmParlaklik = 0.72f;

        public string CurrentClip { get; private set; }

        public static float Hiz = 1f;   // yavas: / dondur: komutlari (animasyon ve yurume hizi)

        // bas kemiginin dinlenme pozundaki ekseni: gozyasi/nefes govdenin degil yuzun yonunu kullanir
        Quaternion kafaDuzeltme;
        bool kafaKayitli;

        public float KafaYuksekligi { get; private set; }     // ayakta, yaratilis aninda (cocuk/yetiskin ayrimi)

        // Oyuncu yaratilir yaratilmaz (animasyon oynamadan once) cagrilir
        public void KafaEkseniKaydet(Transform kafa)
        {
            if (kafaKayitli || kafa == null) return;
            KafaYuksekligi = kafa.position.y - transform.position.y;
            kafaDuzeltme = Quaternion.Inverse(kafa.rotation) * transform.rotation;
            kafaKayitli = true;
        }

        public bool YuzDonusu(Transform kafa, out Quaternion donus)
        {
            donus = kafaKayitli && kafa != null ? kafa.rotation * kafaDuzeltme : transform.rotation;
            return kafaKayitli && kafa != null;
        }

        Animator animator;
        Animation legacy;
        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable cur, prev;
        float fade = 1f, fadeTime = 0.25f;

        void Start()
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            legacy = GetComponentInChildren<Animation>();
            if (!string.IsNullOrEmpty(startClip) && string.IsNullOrEmpty(CurrentClip)) Play(startClip, 0f);
            CanliLikEkle();
            // VRM modellerinin hazir gorunurluk sinirlari yanlis (yerin altinda kaliyor): yakin planda karakter
            // kadrajdayken cizilmeyebiliyordu. Sinirlar her karede kemiklerden hesaplansin.
            if (Application.isPlaying)
                foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>(true)) r.updateWhenOffscreen = true;
            if (Application.isPlaying && GetComponent<DizKapat>() == null) gameObject.AddComponent<DizKapat>();
            VrmRenkAyarla();
        }

        // MToon malzemeleri sahne isiklarinda (ana + dolgu + ortam) beyaza kaciyor: renkleri kis.
        // Yalnizca bu karakterin malzeme kopyalari degisir; asil dosyalara dokunulmaz.
        readonly List<Material> vrmKopyalar = new List<Material>();

        void VrmRenkAyarla()
        {
            if (kategori != "VRM" || !Application.isPlaying || vrmKopyalar.Count > 0) return;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.materials)
                {
                    if (m == null) continue;
                    vrmKopyalar.Add(m);
                    Kis(m, "_Color");
                    Kis(m, "_ShadeColor");
                }
        }

        void Kis(Material m, string ad)
        {
            if (!m.HasProperty(ad)) return;
            var c = m.GetColor(ad);
            m.SetColor(ad, new Color(c.r * vrmParlaklik, c.g * vrmParlaklik, c.b * vrmParlaklik, c.a));
        }

        // Yuz/govde canliligi (YuzCanlilik.cs). Eklenemezse sessizce atlanir.
        void CanliLikEkle()
        {
            if (!canliLik) return;
            if (GetComponent<YuzCanlilik>() != null) return;
            try { gameObject.AddComponent<YuzCanlilik>(); }
            catch (System.Exception e) { Debug.LogWarning("[Hikaye] " + id + ": canlılık eklenemedi: " + e.Message); }
        }

        // Ada gore klip bul: tam ad, sonra icerdigi ad ("walk" -> "Walk_Loop"), Turkce takma adlar da olur
        public AnimationClip Find(string name)
        {
            if (string.IsNullOrEmpty(name) || clips == null) return null;
            string n = Alias(Util.Norm(name));
            foreach (var c in clips) if (c != null && Util.Norm(Strip(c.name)) == n) return c;
            foreach (var c in clips) if (c != null && Util.Norm(Strip(c.name)).StartsWith(n)) return c;
            foreach (var c in clips) if (c != null && Util.Norm(Strip(c.name)).Contains(n)) return c;
            return null;
        }

        static string Strip(string clipName)
        {
            int i = clipName.LastIndexOf('|');   // "AnimalArmature|Idle" -> "Idle"
            return i >= 0 ? clipName.Substring(i + 1) : clipName;
        }

        static string Alias(string n)
        {
            switch (n)
            {
                case "dur": case "bekle": case "ayakta": return "idle";
                case "konus": case "konusma": return "idle_talking";
                case "yuru": return "walk";
                case "kos": return "jog";
                case "hizli_kos": return "sprint";
                case "otur": return "sitting_idle";
                case "oturarak_konus": return "sitting_talking";
                case "dans": return "dance";
                case "ol": case "olum": return "death";
                case "zipla": return "jump";
                case "egil": return "crouch_idle";
                case "araba": case "surus": return "driving";
                case "yumruk": return "punch";
                case "buyu": return "spell_simple_idle";
                case "al": return "pickup";
                case "tamir": return "fixing";
                case "salla": case "el_salla": return "wave";
                case "yemek": case "ye": return "eating";
                case "agla": return "crying";
                case "yat": case "uzan": return "lying_down";
                case "uyu": return "sleeping_idle";
                case "yatarak_uyu": return "laying_sleeping";
                case "dusun": return "thinking";
                case "kork": return "scared";
                case "sasir": return "surprised";
                case "sukret": case "tesekkur": return "thankful";
                case "uzgun": case "uzgun_dur": return "sad_idle";
                case "kalk": case "ayaga_kalk": return "getting_up";
                case "diz_cok": return "kneeling_down";
                case "goster": return "kneeling_pointing";
                case "ilac": case "ilac_ic": return "taking_a_pill";
                case "korna": return "honking_horn";
                case "arabaya_bin": return "entering_car";
                case "tepki": return "reacting";
                case "konus2": return "talking";
                case "serum": return "iv_pole_walking";
            }
            return n;
        }

        public bool Play(string name, float fadeSeconds = 0.25f)
        {
            var clip = Find(name);
            if (clip == null)
            {
                Debug.LogWarning("[Hikaye] " + id + " için '" + name + "' animasyonu yok.");
                return false;
            }
            return Play(clip, fadeSeconds);
        }

        public bool Play(AnimationClip clip, float fadeSeconds = 0.25f)
        {
            if (clip == null) return false;
            CurrentClip = clip.name;

            if (clip.legacy)
            {
                if (legacy == null) legacy = GetComponentInChildren<Animation>();
                if (legacy == null) legacy = gameObject.AddComponent<Animation>();
                if (legacy.GetClip(clip.name) == null) legacy.AddClip(clip, clip.name);
                legacy[clip.name].wrapMode = IsLoop(clip) ? WrapMode.Loop : WrapMode.ClampForever;
                legacy.CrossFade(clip.name, Mathf.Max(0.01f, fadeSeconds));
                return true;
            }

            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = gameObject.AddComponent<Animator>();
            if (!graph.IsValid())
            {
                graph = PlayableGraph.Create("Karakter_" + id);
                graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                var output = AnimationPlayableOutput.Create(graph, "Animasyon", animator);
                mixer = AnimationMixerPlayable.Create(graph, 2);
                output.SetSourcePlayable(mixer);
                graph.Play();
            }
            var next = AnimationClipPlayable.Create(graph, clip);
            if (!IsLoop(clip)) next.SetDuration(clip.length);
            if (cur.IsValid())
            {
                if (prev.IsValid()) { graph.Disconnect(mixer, 1); prev.Destroy(); }
                graph.Disconnect(mixer, 0);
                prev = cur;
                graph.Connect(prev, 0, mixer, 1);
            }
            cur = next;
            graph.Connect(cur, 0, mixer, 0);
            // ilk klipte yumusak gecis yapilmaz: karisacak onceki klip yok, karakter yarim saniye comelik gorunuyor
            // ve kamera o alcak basa kilitlenip yuzu kadraj disinda birakiyordu
            fadeTime = prev.IsValid() ? Mathf.Max(0f, fadeSeconds) : 0f;
            fade = fadeTime <= 0f ? 1f : 0f;
            ApplyWeights();
            return true;
        }

        static bool IsLoop(AnimationClip c)
        {
            if (c.isLooping) return true;
            var n = c.name.ToLowerInvariant();
            return n.Contains("loop") || n.Contains("idle") || n.Contains("walk") || n.Contains("run") ||
                   n.Contains("gallop") || n.Contains("dance") || n.Contains("swim") || n.Contains("flying");
        }

        void ApplyWeights()
        {
            if (!mixer.IsValid()) return;
            float w = Mathf.Clamp01(fade);
            mixer.SetInputWeight(0, w);
            mixer.SetInputWeight(1, prev.IsValid() ? 1f - w : 0f);
        }

        void Update()
        {
            if (mixer.IsValid()) mixer.SetSpeed(Hiz);
            if (legacy != null) foreach (AnimationState d in legacy) d.speed = Hiz;
            if (fade < 1f)
            {
                fade += fadeTime > 0f ? Time.deltaTime * Hiz / fadeTime : 1f;
                ApplyWeights();
            }
        }

        void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
            foreach (var m in vrmKopyalar) if (m != null) Destroy(m);
            vrmKopyalar.Clear();
        }
    }
}
