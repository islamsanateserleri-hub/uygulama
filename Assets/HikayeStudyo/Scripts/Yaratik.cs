using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Kodla yapilan yaratiklar (indirilen model degil, cizgi tarzi):
    //   nesne: ejderha | ejderha_kirmizi | ejderha_siyah | ejderha_mavi   (sol/sag/arka/gok yazilabilir)
    //   oyuncu: Karakter = Vampir | Vampir_Kadin
    public static class Yaratik
    {
        public static readonly string[] EjderhaAdlari = { "ejderha", "ejderha_kirmizi", "ejderha_siyah", "ejderha_mavi" };
        public static readonly string[] OyuncuAdlari = { "Vampir", "Vampir_Kadin" };

        static Transform depo;
        static readonly Dictionary<string, NesneKatalogu.Nesne> ejderhalar = new Dictionary<string, NesneKatalogu.Nesne>();
        static readonly Dictionary<string, GameObject> vampirler = new Dictionary<string, GameObject>();

        public static bool EjderhaAdi(string ad) { return System.Array.IndexOf(EjderhaAdlari, Util.Norm(ad)) >= 0; }

        public static bool OyuncuAdi(string ad)
        {
            var n = Util.Norm(ad);
            return n == "vampir" || n == "vampir_kadin";
        }

        static Transform Depo()
        {
            if (depo != null) return depo;
            var g = new GameObject("YaratikSablonlari");
            g.SetActive(false);   // sablonlar calismaz; kopyalari calisir
            if (Application.isPlaying) Object.DontDestroyOnLoad(g);
            else g.hideFlags = HideFlags.DontSave;
            return depo = g.transform;
        }

        // ---------- vampir ----------
        public static GameObject Sablon(string ad, IEnumerable<GameObject> kaynak)
        {
            var n = Util.Norm(ad);
            if (!OyuncuAdi(n) || kaynak == null) return null;
            GameObject go;
            if (vampirler.TryGetValue(n, out go) && go != null) return go;
            string[] tabanlar = n == "vampir_kadin"
                ? new[] { "Gercek_IsInsaniKadin01", "Gercek_Kadin01" }
                : new[] { "Gercek_IsInsaniErkek01", "Gercek_Erkek01" };
            GameObject taban = null;
            foreach (var t in tabanlar)
            {
                foreach (var g in kaynak) if (g != null && g.name == t) { taban = g; break; }
                if (taban != null) break;
            }
            if (taban == null) return null;
            go = Object.Instantiate(taban, Depo());
            go.name = n == "vampir_kadin" ? "Vampir_Kadin" : "Vampir";
            go.AddComponent<VampirGorunum>();
            vampirler[n] = go;
            return go;
        }

        // ---------- ejderha ----------
        public static NesneKatalogu.Nesne Ejderha(string ad)
        {
            var k = Util.Norm(ad);
            if (!EjderhaAdi(k)) return null;
            NesneKatalogu.Nesne n;
            if (ejderhalar.TryGetValue(k, out n) && n.model != null) return n;
            Color deri = k == "ejderha_kirmizi" ? new Color(0.6f, 0.09f, 0.07f) : k == "ejderha_siyah" ? new Color(0.09f, 0.08f, 0.11f)
                       : k == "ejderha_mavi" ? new Color(0.1f, 0.3f, 0.65f) : new Color(0.16f, 0.45f, 0.17f);
            n = new NesneKatalogu.Nesne { id = k, kategori = "yaratik", model = EjderhaYap(deri), boy = 3.2f, donus = 28f };
            ejderhalar[k] = n;
            return n;
        }

        public static Material Parlak(Color c, float guc)
        {
            var m = Mekan3D.DuzMalzeme(c);
            m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * guc);
            return m;
        }

        static Transform Bos(string ad, Transform ust, Vector3 yer)
        {
            var g = new GameObject(ad);
            g.transform.SetParent(ust, false);
            g.transform.localPosition = yer;
            return g.transform;
        }

        static Transform Sekil(PrimitiveType tip, Transform ust, Vector3 yer, Vector3 olcek, Material m, Vector3 aci = default(Vector3))
        {
            var g = GameObject.CreatePrimitive(tip);
            var col = g.GetComponent<Collider>();
            if (col != null) { if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col); }
            g.transform.SetParent(ust, false);
            g.transform.localPosition = yer;
            g.transform.localEulerAngles = aci;
            g.transform.localScale = olcek;
            g.GetComponent<MeshRenderer>().sharedMaterial = m;
            return g.transform;
        }

        public static Transform Ag(string ad, Transform ust, Vector3 yer, Vector3[] nokta, int[] ucgen, Material m)
        {
            var g = new GameObject(ad);
            g.transform.SetParent(ust, false);
            g.transform.localPosition = yer;
            var mesh = new Mesh { name = ad, vertices = nokta, triangles = ucgen };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = m;
            return g.transform;
        }

        static Mesh koni;
        // tabani y=0'da, ucu y=1'de, yaricapi 0.5
        public static Transform Koni(Transform ust, Vector3 yer, float cap, float boy, Vector3 aci, Material m)
        {
            if (koni == null)
            {
                const int d = 10;
                var v = new List<Vector3>();
                var t = new List<int>();
                for (int i = 0; i < d; i++)
                {
                    float a = i * Mathf.PI * 2f / d, b = (i + 1) * Mathf.PI * 2f / d;
                    int s = v.Count;
                    v.Add(new Vector3(Mathf.Cos(a) * 0.5f, 0f, Mathf.Sin(a) * 0.5f));
                    v.Add(new Vector3(0f, 1f, 0f));
                    v.Add(new Vector3(Mathf.Cos(b) * 0.5f, 0f, Mathf.Sin(b) * 0.5f));
                    t.Add(s); t.Add(s + 1); t.Add(s + 2);
                }
                koni = new Mesh { name = "Koni", vertices = v.ToArray(), triangles = t.ToArray() };
                koni.RecalculateNormals();
                koni.RecalculateBounds();
            }
            var g = new GameObject("Koni");
            g.transform.SetParent(ust, false);
            g.transform.localPosition = yer;
            g.transform.localEulerAngles = aci;
            g.transform.localScale = new Vector3(cap, boy, cap);
            g.AddComponent<MeshFilter>().sharedMesh = koni;
            g.AddComponent<MeshRenderer>().sharedMaterial = m;
            return g.transform;
        }

        // iki yuzlu yuzey: on yuz 'on', arka yuz 'arka' malzemesi
        public static void CiftYuz(string ad, Transform ust, Vector3 yer, Vector3[] nokta, int[] ucgen, Material on, Material arka)
        {
            Ag(ad, ust, yer, nokta, ucgen, on);
            var ters = (int[])ucgen.Clone();
            for (int i = 0; i + 2 < ters.Length; i += 3) { int x = ters[i + 1]; ters[i + 1] = ters[i + 2]; ters[i + 2] = x; }
            Ag(ad + "_ters", ust, yer, nokta, ters, arka);
        }

        static void Kemik(Transform ust, Vector3 a, Vector3 b, float cap, Material m)
        {
            var t = Sekil(PrimitiveType.Capsule, ust, (a + b) * 0.5f, new Vector3(cap, (b - a).magnitude * 0.5f, cap), m);
            t.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        // Yuzu -z'ye bakar. Yaklasik boyutlar metre; sahnede NesneKoy olcekler.
        static GameObject EjderhaYap(Color deri)
        {
            var mDeri = Mekan3D.DuzMalzeme(deri);
            var mKoyu = Mekan3D.DuzMalzeme(deri * 0.6f);
            var mKarin = Mekan3D.DuzMalzeme(new Color(0.85f, 0.72f, 0.36f));
            var mZar = Mekan3D.DuzMalzeme(Color.Lerp(deri, new Color(0.9f, 0.45f, 0.15f), 0.45f));
            var mKemik = Mekan3D.DuzMalzeme(new Color(0.92f, 0.88f, 0.75f));
            var mGoz = Parlak(new Color(1f, 0.78f, 0.1f), 1.6f);
            var mSiyah = Mekan3D.DuzMalzeme(new Color(0.03f, 0.02f, 0.02f));

            var kok = new GameObject("Ejderha");
            kok.transform.SetParent(Depo(), false);
            var govde = Bos("Govde", kok.transform, Vector3.zero);
            Sekil(PrimitiveType.Sphere, govde, new Vector3(0f, 0.78f, 0f), new Vector3(0.82f, 0.78f, 1.25f), mDeri);
            Sekil(PrimitiveType.Sphere, govde, new Vector3(0f, 0.68f, -0.16f), new Vector3(0.62f, 0.6f, 1.02f), mKarin);

            foreach (var s in new[] { -1f, 1f })
            {
                foreach (var z in new[] { -0.36f, 0.4f })
                {
                    Sekil(PrimitiveType.Capsule, kok.transform, new Vector3(s * 0.32f, 0.34f, z), new Vector3(0.23f, 0.28f, 0.25f), mDeri, new Vector3(z < 0 ? -8f : 12f, 0f, s * 8f));
                    Sekil(PrimitiveType.Sphere, kok.transform, new Vector3(s * 0.35f, 0.07f, z - 0.08f), new Vector3(0.26f, 0.14f, 0.34f), mKoyu);
                    for (int p = -1; p <= 1; p++)
                        Koni(kok.transform, new Vector3(s * 0.35f + p * 0.07f, 0.05f, z - 0.22f), 0.045f, 0.11f, new Vector3(-90f, 0f, 0f), mKemik);
                }
            }

            // boyun + kafa
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                var yer = new Vector3(0f, Mathf.Lerp(1.0f, 1.72f, t), -0.44f - 0.46f * Mathf.Sin(t * 1.5f));
                float d = Mathf.Lerp(0.42f, 0.27f, t);
                Sekil(PrimitiveType.Sphere, govde, yer, new Vector3(d, d, d), mDeri);
                Sekil(PrimitiveType.Sphere, govde, yer + new Vector3(0f, -0.04f, -0.07f), new Vector3(d * 0.7f, d * 0.7f, d * 0.6f), mKarin);
                Koni(govde, yer + new Vector3(0f, d * 0.3f, d * 0.3f), 0.09f, 0.17f, new Vector3(55f, 0f, 0f), mKemik);
            }
            var kafa = Bos("Kafa", govde, new Vector3(0f, 1.82f, -0.98f));
            Sekil(PrimitiveType.Sphere, kafa, Vector3.zero, new Vector3(0.46f, 0.38f, 0.44f), mDeri);
            Sekil(PrimitiveType.Sphere, kafa, new Vector3(0f, -0.05f, -0.27f), new Vector3(0.3f, 0.2f, 0.42f), mDeri);
            Sekil(PrimitiveType.Sphere, kafa, new Vector3(0f, -0.19f, -0.2f), new Vector3(0.25f, 0.09f, 0.36f), mKarin, new Vector3(14f, 0f, 0f));
            foreach (var s in new[] { -1f, 1f })
            {
                Sekil(PrimitiveType.Sphere, kafa, new Vector3(s * 0.145f, 0.08f, -0.15f), new Vector3(0.11f, 0.1f, 0.09f), mGoz);
                Sekil(PrimitiveType.Sphere, kafa, new Vector3(s * 0.155f, 0.08f, -0.196f), new Vector3(0.022f, 0.075f, 0.02f), mSiyah);
                Sekil(PrimitiveType.Cube, kafa, new Vector3(s * 0.14f, 0.16f, -0.15f), new Vector3(0.15f, 0.045f, 0.1f), mKoyu, new Vector3(0f, 0f, s * 22f));
                Sekil(PrimitiveType.Sphere, kafa, new Vector3(s * 0.06f, 0.01f, -0.47f), new Vector3(0.04f, 0.035f, 0.03f), mSiyah);
                Koni(kafa, new Vector3(s * 0.13f, 0.13f, 0.05f), 0.11f, 0.38f, new Vector3(48f, 0f, -s * 18f), mKemik);
                Koni(kafa, new Vector3(s * 0.2f, -0.02f, 0.08f), 0.07f, 0.2f, new Vector3(70f, 0f, -s * 55f), mKemik);
                for (int d = 0; d < 3; d++)
                    Koni(kafa, new Vector3(s * (0.1f - d * 0.015f), -0.12f, -0.27f - d * 0.07f), 0.035f, 0.075f, new Vector3(180f, 0f, 0f), mKemik);
            }

            // sirt dikenleri
            for (int i = 0; i < 5; i++)
                Koni(govde, new Vector3(0f, 1.14f - Mathf.Abs(i - 1) * 0.04f, -0.25f + i * 0.17f), 0.12f, 0.22f, new Vector3(18f, 0f, 0f), mKemik);

            // kanatlar
            var canli = kok.AddComponent<EjderhaCanli>();
            foreach (var s in new[] { -1f, 1f })
            {
                var kanat = Bos(s < 0 ? "KanatSol" : "KanatSag", govde, new Vector3(s * 0.27f, 1.08f, -0.05f));
                System.Func<float, float, float, Vector3> N = (x, y, z) => new Vector3(s * x, y, z);
                var v = new[]
                {
                    N(0f, 0f, 0f), N(0.72f, 0.82f, 0.05f), N(1.75f, 0.72f, 0.18f), N(1.42f, 0.14f, 0.22f),
                    N(0.98f, 0.24f, 0.25f), N(0.76f, -0.28f, 0.22f), N(0.4f, -0.04f, 0.18f), N(0.12f, -0.36f, 0.14f)
                };
                CiftYuz("Zar", kanat, Vector3.zero, v, new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7 }, mZar, mZar);
                Kemik(kanat, v[0], v[1], 0.09f, mKoyu);
                Kemik(kanat, v[1], v[2], 0.06f, mKoyu);
                Kemik(kanat, v[1], v[3], 0.035f, mKoyu);
                Kemik(kanat, v[1], v[5], 0.035f, mKoyu);
                Koni(kanat, v[1], 0.07f, 0.17f, Vector3.zero, mKemik);
                if (s < 0) canli.sol = kanat; else canli.sag = kanat;
            }

            // kuyruk
            Transform ust = Bos("Kuyruk0", govde, new Vector3(0f, 0.72f, 0.5f));
            for (int i = 0; i < 10; i++)
            {
                float d = Mathf.Lerp(0.44f, 0.09f, i / 9f);
                Sekil(PrimitiveType.Sphere, ust, Vector3.zero, new Vector3(d, d, d * 1.3f), mDeri);
                if (i % 2 == 0) Koni(ust, new Vector3(0f, d * 0.4f, 0f), d * 0.25f, d * 0.45f, new Vector3(25f, 0f, 0f), mKemik);
                ust = Bos("Kuyruk" + (i + 1), ust, new Vector3(i * 0.012f, i < 5 ? -0.055f : 0.035f, d * 0.55f + 0.03f));
            }
            Koni(ust, new Vector3(0f, 0f, -0.04f), 0.24f, 0.28f, new Vector3(90f, 0f, 0f), mZar);
            canli.kafa = kafa;
            canli.govde = govde;
            return kok;
        }
    }

    // Kanat cirpma, kuyruk sallama, nefes
    public class EjderhaCanli : MonoBehaviour
    {
        public Transform sol, sag, kafa, govde;
        readonly List<Transform> kuyruk = new List<Transform>();
        float t;

        void Start()
        {
            var p = govde != null ? govde.Find("Kuyruk0") : null;
            for (int i = 1; p != null; i++) { kuyruk.Add(p); p = p.Find("Kuyruk" + i); }
        }

        void Update()
        {
            t += Time.deltaTime * Karakter3D.Hiz;
            float cirp = Mathf.Sin(t * 2.6f) * 20f;
            if (sag != null) sag.localRotation = Quaternion.Euler(0f, cirp * 0.35f, cirp);
            if (sol != null) sol.localRotation = Quaternion.Euler(0f, -cirp * 0.35f, -cirp);
            if (kafa != null) kafa.localRotation = Quaternion.Euler(Mathf.Sin(t * 1.3f) * 6f, Mathf.Sin(t * 0.7f) * 10f, 0f);
            if (govde != null) govde.localPosition = new Vector3(0f, Mathf.Sin(t * 2.6f + 1.5f) * 0.02f, 0f);
            for (int i = 0; i < kuyruk.Count; i++) kuyruk[i].localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 1.8f - i * 0.45f) * 7f, 0f);
        }
    }

    // Takim elbiseli insan modelini vampire cevirir: soluk ten, koyu giysi, kirmizi gozler, yaka ve pelerin
    [DisallowMultipleComponent]
    public class VampirGorunum : MonoBehaviour
    {
        readonly List<Object> kopyalar = new List<Object>();

        void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    var m = new Material(ms[i]);
                    kopyalar.Add(m);
                    var ad = ms[i].name.ToLowerInvariant();
                    if (ad.Contains("body")) Boya(m, new Color(0.2f, 0.17f, 0.2f));
                    else if (ad.Contains("head")) Boya(m, new Color(1.08f, 1.2f, 1.34f));
                    else if (ad.Contains("opacity")) Boya(m, new Color(0.25f, 0.22f, 0.24f));
                    ms[i] = m;
                }
                r.sharedMaterials = ms;
            }
        }

        static void Boya(Material m, Color carpan)
        {
            foreach (var p in new[] { "_BaseColor", "_Color", "baseColorFactor" })
                if (m.HasProperty(p)) m.SetColor(p, m.GetColor(p) * carpan);
        }

        void Start()
        {
            var an = GetComponentInChildren<Animator>();
            if (an == null || !an.isHuman) return;
            var boyun = an.GetBoneTransform(HumanBodyBones.Neck);
            var gogus = an.GetBoneTransform(HumanBodyBones.UpperChest);
            if (gogus == null) gogus = an.GetBoneTransform(HumanBodyBones.Chest);
            var solKol = an.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var sagKol = an.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (boyun == null || gogus == null || solKol == null || sagKol == null) return;

            var siyah = Mekan3D.DuzMalzeme(new Color(0.035f, 0.025f, 0.04f));
            var kirmizi = Mekan3D.DuzMalzeme(new Color(0.2f, 0.01f, 0.03f));
            kopyalar.Add(siyah); kopyalar.Add(kirmizi);

            // karakterin yonune gore bir cerceve: x = sag, y = yukari, z = on
            var cerceve = new GameObject("Pelerin").transform;
            cerceve.SetPositionAndRotation(boyun.position, Quaternion.LookRotation(transform.forward, Vector3.up));
            float yer = transform.position.y;
            float h = Mathf.Max(0.2f, boyun.position.y - yer);          // boyun yuksekligi
            float omuz = Mathf.Max(0.05f, Vector3.Distance(solKol.position, sagKol.position));

            const int satir = 7, sutun = 9;
            var v = new Vector3[satir * sutun];
            for (int i = 0; i < satir; i++)
            {
                float a = i / (satir - 1f);
                float en = Mathf.Lerp(omuz * 0.56f, omuz * 0.92f, a);   // yari genislik
                for (int j = 0; j < sutun; j++)
                {
                    float b = j / (sutun - 1f) * 2f - 1f;
                    float kivrim = Mathf.Sin(b * 9f) * 0.012f * a;
                    float z = -Mathf.Lerp(h * 0.085f, h * 0.2f, a) + b * b * h * 0.07f + kivrim;
                    v[i * sutun + j] = new Vector3(b * en, -0.02f * h - a * h * 0.88f, z);
                }
            }
            var t = new List<int>();
            for (int i = 0; i < satir - 1; i++)
                for (int j = 0; j < sutun - 1; j++)
                {
                    int k = i * sutun + j;
                    t.Add(k); t.Add(k + sutun); t.Add(k + 1);
                    t.Add(k + 1); t.Add(k + sutun); t.Add(k + sutun + 1);
                }
            // ucgenler +z (on) yuzune bakar: on yuz kirmizi astar, arka yuz siyah
            Yaratik.CiftYuz("Kumas", cerceve, Vector3.zero, v, t.ToArray(), kirmizi, siyah);

            // dik yaka
            foreach (var s in new[] { -1f, 1f })
            {
                var y = new[]
                {
                    new Vector3(s * omuz * 0.1f, -0.03f * h, -h * 0.07f), new Vector3(s * omuz * 0.46f, -0.02f * h, -h * 0.05f),
                    new Vector3(s * omuz * 0.34f, 0.07f * h, -h * 0.06f)
                };
                Yaratik.CiftYuz("Yaka", cerceve, Vector3.zero, y, new[] { 0, 1, 2 }, kirmizi, kirmizi);
            }
            cerceve.SetParent(gogus, true);

            // kirmizi gozler
            var goz = Yaratik.Parlak(new Color(1f, 0.05f, 0.03f), 2.2f);
            kopyalar.Add(goz);
            foreach (var kemik in new[] { HumanBodyBones.LeftEye, HumanBodyBones.RightEye })
            {
                var e = an.GetBoneTransform(kemik);
                if (e == null) continue;
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.name = "VampirGoz";
                var col = g.GetComponent<Collider>();
                if (col != null) Destroy(col);
                g.transform.position = e.position + transform.forward * (h * 0.0105f);
                g.transform.localScale = Vector3.one * (h * 0.0062f);
                g.GetComponent<MeshRenderer>().sharedMaterial = goz;
                g.transform.SetParent(e.parent, true);
            }
        }

        void OnDestroy()
        {
            foreach (var o in kopyalar) if (o != null) Destroy(o);
            kopyalar.Clear();
        }
    }
}
