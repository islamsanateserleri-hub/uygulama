using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // 3D oyuncular: karakterler uzakta (y = -1000) ayri bir kamerayla cekilir ve 2D sahnenin karakter katmaninda
    // seffaf bir resim olarak gosterilir. Boylece 2D mekan, altyazi, efektler ve kamera yakinlasmasi aynen calisir.
    public class Sahne3D : MonoBehaviour
    {
        const float W = 1080f, H = 1920f;
        static readonly Vector3 Origin = new Vector3(0f, -1000f, 0f);
        const float CamDist = 6.2f, CamFov = 30f;

        public Camera Cam { get; private set; }
        RenderTexture rt;
        RawImage image;
        float floorScreenY;   // zeminin 2D sahnedeki yuksekligi (kanvas birimi, merkez = 0)
        readonly Dictionary<string, Karakter3D> actors = new Dictionary<string, Karakter3D>();
        readonly Dictionary<string, float> slots = new Dictionary<string, float>();

        // Duygu / hareket etiketi -> animasyon (Karakter3D takma adlari). Kutuphanedeki tablo bunu ezer.
        public static readonly Dictionary<string, string[]> Varsayilan = new Dictionary<string, string[]>
        {
            { "normal", new[] { "konus", "konus2", "dur" } },
            { "mutlu", new[] { "konus2", "konus", "dur" } },
            { "sevgi", new[] { "sukret", "konus", "dur" } },
            { "uzgun", new[] { "uzgun", "dur" } },
            { "aglamakli", new[] { "agla", "uzgun", "dur" } },
            { "korkmus", new[] { "kork", "tepki", "dur" } },
            { "sasirmis", new[] { "sasir", "tepki", "dur" } },
            { "kizgin", new[] { "tepki", "konus2", "dur" } },
            { "yorgun", new[] { "uzgun", "dur" } },
            { "hasta", new[] { "uzgun", "dur" } },
            { "dusunceli", new[] { "dusun", "dur" } },
            { "fisilti", new[] { "konus", "dur" } },
            { "sus", new[] { "dur" } },                 // konusmuyor
            { "kos", new[] { "kos", "yuru" } },
            { "yuru", new[] { "yuru" } },
            { "zipla", new[] { "zipla" } },
            { "egil", new[] { "egil", "diz_cok" } },
            { "sarsil", new[] { "kork", "tepki" } },
            { "saril", new[] { "yuru" } },
            { "cik", new[] { "yuru" } },
            { "yatiyor", new[] { "yat", "uyu", "yatarak_uyu" } },
            { "oturuyor", new[] { "otur", "oturarak_konus" } },
            { "ayakta", new[] { "dur" } },
        };
        public Dictionary<string, string[]> Harita = new Dictionary<string, string[]>(Varsayilan);

        public static Sahne3D Create(RectTransform charRoot, float floorY)
        {
            var go = new GameObject("Sahne3D");
            var s = go.AddComponent<Sahne3D>();
            s.floorScreenY = floorY;
            s.Build(charRoot);
            return s;
        }

        void Build(RectTransform charRoot)
        {
            rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32) { name = "Sahne3D_RT", antiAliasing = 4 };
            var cgo = new GameObject("Kamera3D");
            cgo.transform.SetParent(transform, false);
            Cam = cgo.AddComponent<Camera>();
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = new Color(0, 0, 0, 0);
            Cam.fieldOfView = CamFov;
            Cam.nearClipPlane = 0.1f; Cam.farClipPlane = 60f;
            Cam.targetTexture = rt;
            Cam.allowHDR = false;
            cgo.transform.rotation = Quaternion.identity;
            SetFloor(floorScreenY);

            var lgo = new GameObject("Isik3D");
            lgo.transform.SetParent(transform, false);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.25f;
            l.color = new Color(1f, 0.96f, 0.9f);
            l.shadows = LightShadows.None;
            lgo.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            var fill = new GameObject("Dolgu3D").AddComponent<Light>();
            fill.transform.SetParent(transform, false);
            fill.type = LightType.Directional; fill.intensity = 0.45f; fill.color = new Color(0.75f, 0.82f, 1f);
            fill.transform.rotation = Quaternion.Euler(20f, 150f, 0f);

            var ri = Gfx.Rect("Oyuncular3D", charRoot);
            ri.anchorMin = ri.anchorMax = new Vector2(0.5f, 0.5f);
            ri.sizeDelta = new Vector2(W, H);
            ri.anchoredPosition = Vector2.zero;
            image = ri.gameObject.AddComponent<RawImage>();
            image.texture = rt;
            image.raycastTarget = false;
        }

        // Zemin cizgisi kanvasta floorY'de dursun: kamera yuksekligini buna gore ayarla (mekan degisince cagrilir)
        public void SetFloor(float floorY)
        {
            floorScreenY = floorY;
            float floorV = 0.5f + floorY / H;                      // 0..1 (alttan)
            float halfH = CamDist * Mathf.Tan(CamFov * 0.5f * Mathf.Deg2Rad);
            float camY = (0.5f - floorV) * 2f * halfH;             // zemin (y=0) bu viewport yuksekliginde gorunur
            if (Cam != null) Cam.transform.position = Origin + new Vector3(0f, camY, -CamDist);
        }

        public bool Has(string id) { return actors.ContainsKey(Util.Norm(id)); }

        float WorldX(float slotX2D)
        {
            float halfH = CamDist * Mathf.Tan(CamFov * 0.5f * Mathf.Deg2Rad);
            float halfW = halfH * (W / H);
            return slotX2D / (W * 0.5f) * halfW;
        }

        // Oyuncuyu sahneye koy (yoksa yarat). enterFrom: 0 = yerinde, -1 soldan, 1 sagdan yuruyerek
        public Karakter3D Ensure(string id, GameObject prefab, float slotX2D, float scale, int enterFrom, bool run)
        {
            var k = Util.Norm(id);
            Karakter3D a;
            if (actors.TryGetValue(k, out a) && a != null) return a;
            var go = Instantiate(prefab, transform);
            go.name = "Oyuncu_" + id;
            a = go.GetComponent<Karakter3D>();
            if (a == null) a = go.AddComponent<Karakter3D>();
            float x = WorldX(slotX2D);
            slots[k] = x;
            go.transform.localScale = Vector3.one * Mathf.Max(0.3f, scale);
            float startX = enterFrom == 0 ? x : WorldX(enterFrom * 820f);
            go.transform.position = Origin + new Vector3(startX, 0f, 0f);
            Face(go.transform, x);
            actors[k] = a;
            if (enterFrom != 0) StartCoroutine(WalkTo(a, x, run));
            return a;
        }

        static void Face(Transform t, float x)
        {
            // izleyiciye donuk, hafifce sahnenin ortasina bakar
            float yaw = 180f + Mathf.Clamp(-x * 12f, -30f, 30f);
            t.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        IEnumerator WalkTo(Karakter3D a, float x, bool run)
        {
            if (a == null) yield break;
            var t = a.transform;
            float from = t.position.x - Origin.x;
            float dir = Mathf.Sign(x - from);
            if (Mathf.Abs(x - from) < 0.05f) yield break;
            t.rotation = Quaternion.Euler(0f, dir > 0 ? 90f : -90f, 0f);
            Play(a, new[] { run ? "kos" : "yuru", "yuru" });
            float speed = run ? 3.2f : 1.4f;
            while (a != null && Mathf.Abs((t.position.x - Origin.x) - x) > 0.02f)
            {
                var p = t.position;
                p.x = Mathf.MoveTowards(p.x, Origin.x + x, speed * Time.deltaTime);
                t.position = p;
                yield return null;
            }
            if (a == null) yield break;
            Face(t, x);
            Play(a, new[] { "dur" });
        }

        public void MoveTo(string id, float slotX2D, bool run)
        {
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return;
            float x = WorldX(slotX2D);
            slots[Util.Norm(id)] = x;
            StartCoroutine(WalkTo(a, x, run));
        }

        bool Play(Karakter3D a, string[] names)
        {
            foreach (var n in names) if (a.Find(n) != null) return a.Play(n, 0.3f);
            return false;
        }

        string[] Map(string tag)
        {
            string[] v;
            return Harita.TryGetValue(Util.Norm(tag), out v) ? v : null;
        }

        // Konusma satiri: duyguya (ve konusup konusmadigina) gore animasyon sec
        public void Act(string id, string emotion, string motion, bool talking)
        {
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return;
            if (motion == "zipla" || motion == "egil" || motion == "sarsil") { var m = Map(motion); if (m != null && Play(a, m)) return; }
            string e = Util.Norm(emotion);
            if (!talking && (e == "normal" || e == "mutlu" || e == "fisilti")) e = "sus";
            var list = Map(e) ?? Map("normal");
            if (list != null) Play(a, list);
        }

        public void SetPose(string id, string pose)
        {
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return;
            var list = Map(pose);
            if (list != null) Play(a, list);
        }

        // Konusmadan sonra durgun hale don (yatan/oturan pozu bozma)
        public void Idle(string id, string pose)
        {
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return;
            var p = Util.Norm(pose);
            if (p == "yatiyor" || p == "oturuyor") return;
            Play(a, new[] { "dur" });
        }

        public IEnumerator Exit(string id)
        {
            var k = Util.Norm(id);
            Karakter3D a;
            if (!actors.TryGetValue(k, out a) || a == null) yield break;
            actors.Remove(k);
            float x = slots.ContainsKey(k) ? slots[k] : 0f;
            yield return WalkTo(a, WorldX(x >= 0 ? 900f : -900f), false);
            if (a != null) Destroy(a.gameObject);
        }

        public void Clear()
        {
            foreach (var kv in actors) if (kv.Value != null) Destroy(kv.Value.gameObject);
            actors.Clear(); slots.Clear();
        }

        // Kafanin 2D kanvastaki dunya konumu (kamera yakinlasmasi icin)
        public bool FaceWorld(string id, out Vector3 world)
        {
            world = Vector3.zero;
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null || image == null) return false;
            var an = a.GetComponent<Animator>();
            Transform head = an != null && an.isHuman ? an.GetBoneTransform(HumanBodyBones.Head) : null;
            Vector3 hp = head != null ? head.position : a.transform.position + Vector3.up * 1.6f * a.transform.localScale.y;
            var v = Cam.WorldToViewportPoint(hp);
            var r = image.rectTransform;
            var local = new Vector2((v.x - 0.5f) * r.rect.width, (v.y - 0.5f) * r.rect.height);
            world = r.TransformPoint(local);
            return true;
        }

        void OnDestroy()
        {
            if (rt != null) { rt.Release(); Destroy(rt); }
        }
    }
}
