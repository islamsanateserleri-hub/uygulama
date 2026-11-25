using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Kodla kurulan basit 3D mekanlar: oda, ev, mutfak, yatak_odasi, hastane, sokak, koy, park, araba.
    // Hepsi Sahne3D'nin Origin noktasina gore kurulur (karakterler z = 0 dizisinde, kamera -z tarafinda).
    // Adli duragi noktalar (kapi, pencere, yatak, sofra ...) 'yer:', 'gir:', 'cikar:' komutlarinda kullanilir.
    // Esya prefablari (Assets/HikayeStudyo/Karakterler/Esyalar vb.) varsa kullanilir, yoksa ilkel sekillerle cizilir.
    public partial class Mekan3D : MonoBehaviour
    {
        public string Tur { get; private set; }
        public bool Dis { get; private set; }               // sokak / koy / park / araba: gokyuzu gorunur
        public bool KapiVar { get { return mentese != null; } }
        public bool Aktif { get { return !string.IsNullOrEmpty(Tur) && !yikildi; } }

        class Nk { public Vector3 pos; public float yaw; }
        struct LampInfo { public Light isik; public float baz; public bool mum; }

        static readonly string[] Turler = { "oda", "ev", "mutfak", "yatak_odasi", "hastane", "sokak", "koy", "park", "araba" };
        static Shader litShader;
        static readonly Dictionary<string, GameObject> propCache = new Dictionary<string, GameObject>();

        Transform root;
        readonly Dictionary<string, Nk> noktalar = new Dictionary<string, Nk>();
        readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
        readonly List<Material> tumMat = new List<Material>();
        readonly List<LampInfo> lambalar = new List<LampInfo>();
        readonly List<Material> lambaMat = new List<Material>();
        readonly List<Color> lambaRenk = new List<Color>();
        Material camMat, ekranMat;
        Transform mentese;
        float kapiT;
        bool kapiHedef, yikildi;
        float lambaCarpan = 0.6f, lambaCarpanHedef = 0.6f;
        float xMin = -1.6f, xMax = 1.6f, zMin = 1.6f, zMax = 2.6f;   // figuran bolgesi

        public static string[] TurAdlari { get { return Turler; } }

        // ---------- Kurulum ----------
        public static Mekan3D Kur(Transform parent, string tur, MekanTarifi tarif = null)
        {
            string t = Util.Norm(tur);
            bool bilinen = false;
            foreach (var x in Turler) if (x == t) bilinen = true;
            if (!bilinen) return null;
            var go = new GameObject("Mekan3D_" + t);
            go.transform.SetParent(parent, false);
            go.transform.position = Sahne3D.Origin;
            var m = go.AddComponent<Mekan3D>();
            m.root = go.transform;
            m.Tur = t;
            m.tarif = tarif;
            try
            {
                switch (t)
                {
                    case "oda": m.KurOda(false); break;
                    case "hastane": m.KurOda(true); break;
                    case "sokak": m.KurSokak(); break;
                    case "koy": m.KurKoy(); break;
                    case "ev": m.KurEv(); break;
                    case "mutfak": m.KurMutfak(); break;
                    case "yatak_odasi": m.KurYatakOdasi(); break;
                    case "park": m.KurPark(); break;
                    case "araba": m.KurAraba(); break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] 3D mekan kurulurken hata: " + e.Message);
            }
            m.N("orta", 0f, 0f, 0f, 180f);
            m.N("sol", -0.5f, 0f, 0f, 180f);
            m.N("sag", 0.5f, 0f, 0f, 180f);
            return m;
        }

        void OnDestroy()
        {
            yikildi = true;
            foreach (var m in tumMat) if (m != null) Destroy(m);
            tumMat.Clear(); cache.Clear();
        }

        // ---------- Noktalar ----------
        void N(string ad, float x, float y, float z, float yaw)
        {
            noktalar[Util.Norm(ad)] = new Nk { pos = new Vector3(x, y, z), yaw = yaw };
        }

        public bool NoktaBul(string ad, out Vector3 world, out float yaw)
        {
            Nk n;
            if (noktalar.TryGetValue(Util.Norm(ad), out n))
            {
                world = Sahne3D.Origin + n.pos; yaw = n.yaw; return true;
            }
            world = Sahne3D.Origin; yaw = 180f;
            return false;
        }

        public string NoktaListesi()
        {
            var l = new List<string>(noktalar.Keys);
            l.Sort();
            return string.Join(", ", l.ToArray());
        }

        // Figuran yeri: bolge icinde i. kisi (n kisi arasinda)
        public bool FiguranYeri(int i, int n, out Vector3 pos, out float yaw)
        {
            n = Mathf.Max(1, n);
            float t = (i + 0.5f) / n;
            float x = Mathf.Lerp(xMin, xMax, t) + Random.Range(-0.12f, 0.12f);
            float z = Mathf.Lerp(zMin, zMax, ((i * 5) % n) / (float)n) + Random.Range(-0.1f, 0.1f);
            pos = Sahne3D.Origin + new Vector3(x, 0f, z);
            yaw = 180f + Mathf.Clamp(-x * 18f, -35f, 35f) + Random.Range(-12f, 12f);
            return true;
        }

        // ---------- Kapi ----------
        public void KapiAc(bool ac) { kapiHedef = ac; }

        void Update()
        {
            float dt = Time.deltaTime;
            if (akan.Count > 0) AkisGuncelle(dt);
            if (mentese != null)
            {
                kapiT = Mathf.MoveTowards(kapiT, kapiHedef ? 1f : 0f, dt * 2.2f);
                float e = kapiT * kapiT * (3f - 2f * kapiT);
                mentese.localRotation = Quaternion.Euler(0f, 80f * e, 0f);
            }
            lambaCarpan = Mathf.MoveTowards(lambaCarpan, lambaCarpanHedef, dt * 0.8f);
            for (int i = 0; i < lambalar.Count; i++)
            {
                var l = lambalar[i];
                if (l.isik == null) continue;
                float fl = l.mum ? 1f + (Mathf.PerlinNoise(Time.time * 6f, i * 3.1f) - 0.5f) * 0.35f : 1f;
                l.isik.intensity = l.baz * lambaCarpan * fl;
            }
            for (int i = 0; i < lambaMat.Count; i++)
            {
                var m = lambaMat[i];
                if (m == null) continue;
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", lambaRenk[i] * (0.15f + 1.6f * Mathf.Clamp01(lambaCarpan)));
            }
            if (ekranMat != null && ekranMat.HasProperty("_EmissionColor"))
            {
                float p = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.time * 2.4f)), 8f);
                ekranMat.SetColor("_EmissionColor", new Color(0.15f, 1f, 0.45f) * (0.6f + p * 1.2f));
            }
        }

        // ---------- Zaman ----------
        public void ZamanUygula(string zaman)
        {
            string z = Util.Norm(zaman);
            Color glass; float emis;
            switch (z)
            {
                case "sabah": glass = new Color(0.8f, 0.92f, 1f); emis = 0.9f; lambaCarpanHedef = 0.15f; break;
                case "gun_batimi": glass = new Color(1f, 0.62f, 0.3f); emis = 1.1f; lambaCarpanHedef = 0.7f; break;
                case "gece": glass = new Color(0.05f, 0.09f, 0.24f); emis = 0.5f; lambaCarpanHedef = 1f; break;
                default: glass = new Color(0.85f, 0.93f, 1f); emis = 0.8f; lambaCarpanHedef = 0.3f; break;
            }
            if (camMat != null)
            {
                if (camMat.HasProperty("_BaseColor")) camMat.SetColor("_BaseColor", glass);
                if (camMat.HasProperty("_Color")) camMat.SetColor("_Color", glass);
                if (camMat.HasProperty("_EmissionColor")) camMat.SetColor("_EmissionColor", glass * emis);
            }
        }

        // ---------- Malzeme ve ilkel sekil yardimcilari ----------
        static Color K(float r, float g, float b) { return new Color(r, g, b, 1f); }

        static Shader Lit()
        {
            if (litShader != null) return litShader;
            bool urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            if (urp) litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");
            return litShader;
        }

        Material Mat(Color c, float smooth, float emis, bool unique)
        {
            string key = ColorUtility.ToHtmlStringRGBA(c) + "|" + smooth.ToString("0.00") + "|" + emis.ToString("0.00");
            Material m;
            if (!unique && cache.TryGetValue(key, out m) && m != null) return m;
            var sh = Lit();
            m = new Material(sh);
            m.name = "Mekan3D_Mat";
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (emis > 0f)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * emis);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            tumMat.Add(m);
            if (!unique) cache[key] = m;
            return m;
        }

        GameObject Part(PrimitiveType tip, string ad, float x, float y, float z, float sx, float sy, float sz,
                        Color c, float smooth, float emis, Transform parent, bool unique)
        {
            var go = GameObject.CreatePrimitive(tip);
            go.name = ad;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(parent != null ? parent : root, false);
            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localScale = new Vector3(sx, sy, sz);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = Mat(c, smooth, emis, unique);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }
            return go;
        }

        GameObject B(string ad, float x, float y, float z, float sx, float sy, float sz, Color c,
                     float smooth = 0.1f, float emis = 0f, Transform parent = null)
        {
            return Part(PrimitiveType.Cube, ad, x, y, z, sx, sy, sz, c, smooth, emis, parent, false);
        }

        // silindir: r yaricap, h yukseklik, (x,y,z) merkez
        GameObject C(string ad, float x, float y, float z, float r, float h, Color c,
                     float smooth = 0.1f, float emis = 0f, Transform parent = null)
        {
            return Part(PrimitiveType.Cylinder, ad, x, y, z, r * 2f, h * 0.5f, r * 2f, c, smooth, emis, parent, false);
        }

        // kure: d cap
        GameObject S(string ad, float x, float y, float z, float d, Color c,
                     float smooth = 0.1f, float emis = 0f, Transform parent = null)
        {
            return Part(PrimitiveType.Sphere, ad, x, y, z, d, d, d, c, smooth, emis, parent, false);
        }

        Light Lamba(string ad, float x, float y, float z, Color c, float siddet, float menzil, bool mum)
        {
            var go = new GameObject(ad);
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(x, y, z);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c; l.intensity = siddet; l.range = menzil;
            l.shadows = LightShadows.None;
            lambalar.Add(new LampInfo { isik = l, baz = siddet, mum = mum });
            return l;
        }

        // Parlayan yuzey (lamba abajuru, sokak lambasi, pencere): zamana gore parlakligi degisir
        void Parlak(GameObject g, Color c)
        {
            var mr = g.GetComponent<MeshRenderer>();
            if (mr == null) return;
            var m = Mat(c, 0.2f, 1f, true);
            mr.sharedMaterial = m;
            lambaMat.Add(m); lambaRenk.Add(c);
        }

        // ---------- Esya prefablari (varsa) ----------
        static GameObject PropPrefab(string anahtar)
        {
            GameObject g;
            if (propCache.TryGetValue(anahtar, out g)) return g;
            g = null;
#if UNITY_EDITOR
            try
            {
                string[] adaylar =
                {
                    "Assets/HikayeStudyo/Karakterler/Esyalar", "Assets/HikayeStudyo/Karakterler/Esya",
                    "Assets/HikayeStudyo/Karakterler/Props", "Assets/HikayeStudyo/Esyalar"
                };
                var klasor = new List<string>();
                foreach (var f in adaylar) if (UnityEditor.AssetDatabase.IsValidFolder(f)) klasor.Add(f);
                if (klasor.Count > 0)
                {
                    foreach (var tip in new[] { "t:Prefab", "t:Model" })
                    {
                        var guids = UnityEditor.AssetDatabase.FindAssets(tip + " " + anahtar, klasor.ToArray());
                        foreach (var guid in guids)
                        {
                            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (obj != null && Util.Norm(obj.name).Contains(anahtar) && obj.GetComponent<Karakter3D>() == null) { g = obj; break; }
                        }
                        if (g != null) break;
                    }
                }
            }
            catch (System.Exception) { g = null; }
#endif
            if (g == null) g = Resources.Load<GameObject>("Esyalar/" + anahtar);
            propCache[anahtar] = g;
            return g;
        }

        // Prefab varsa yerlestirir (hedef yukseklige olceklenir, zemine oturtulur); yoksa false doner
        bool Prop(string anahtar, float x, float y, float z, float yaw, float yukseklik)
        {
            var pf = PropPrefab(anahtar);
            if (pf == null) return false;
            var go = Instantiate(pf, root);
            go.name = "Prop_" + anahtar;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one;
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) { Destroy(go); return false; }
            Bounds b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            if (b.size.y < 0.001f) { Destroy(go); return false; }
            go.transform.localScale = Vector3.one * (yukseklik / b.size.y);
            Bounds b2 = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b2.Encapsulate(rs[i].bounds);
            float hedefAlt = root.position.y + y;
            go.transform.position += Vector3.up * (hedefAlt - b2.min.y);
            return true;
        }

        // ---------- Ortak parcalar ----------
        void KapiYap(float cx, float zFace, float w, float h, Color panel, Color kasa)
        {
            B("KapiAcikligi", cx, h / 2f, zFace - 0.01f, w, h, 0.02f, K(0.02f, 0.02f, 0.03f), 0f);
            B("KapiKasaUst", cx, h + 0.04f, zFace - 0.03f, w + 0.2f, 0.1f, 0.08f, kasa);
            B("KapiKasaSol", cx - w / 2f - 0.05f, h / 2f, zFace - 0.03f, 0.1f, h, 0.08f, kasa);
            B("KapiKasaSag", cx + w / 2f + 0.05f, h / 2f, zFace - 0.03f, 0.1f, h, 0.08f, kasa);
            var hinge = new GameObject("KapiMentese").transform;
            hinge.SetParent(root, false);
            hinge.localPosition = new Vector3(cx - w / 2f, 0f, zFace - 0.05f);
            B("KapiKanat", w / 2f, h / 2f, 0f, w - 0.02f, h - 0.02f, 0.05f, panel, 0.25f, 0f, hinge);
            B("KapiTokmak", w - 0.12f, h * 0.48f, -0.04f, 0.07f, 0.07f, 0.05f, K(0.85f, 0.7f, 0.3f), 0.6f, 0f, hinge);
            mentese = hinge;
        }

        // Pencere: arkasi parlayan cam + cerceve + (istege bagli) perde
        void PencereYap(float cx, float cy, float zFace, float w, float h, Color cerceve, bool perde, Color perdeRenk)
        {
            perdeRenk = Vurgu(perdeRenk);
            pencereler.Add(new Pencere { x = cx, y = cy, z = zFace - 0.045f, w = w, h = h });
            var g = Part(PrimitiveType.Cube, "PencereCam", cx, cy, zFace - 0.02f, w, h, 0.03f, K(0.85f, 0.93f, 1f), 0.3f, 0.8f, null, true);
            var mr = g.GetComponent<MeshRenderer>();
            if (mr != null) camMat = mr.sharedMaterial;
            B("PencereUst", cx, cy + h / 2f + 0.04f, zFace - 0.04f, w + 0.16f, 0.08f, 0.07f, cerceve);
            B("PencereAlt", cx, cy - h / 2f - 0.04f, zFace - 0.06f, w + 0.24f, 0.08f, 0.1f, cerceve);
            B("PencereSol", cx - w / 2f - 0.04f, cy, zFace - 0.04f, 0.08f, h + 0.16f, 0.07f, cerceve);
            B("PencereSag", cx + w / 2f + 0.04f, cy, zFace - 0.04f, 0.08f, h + 0.16f, 0.07f, cerceve);
            B("PencereDikey", cx, cy, zFace - 0.04f, 0.05f, h, 0.05f, cerceve);
            B("PencereYatay", cx, cy, zFace - 0.04f, w, 0.05f, 0.05f, cerceve);
            if (perde)
            {
                B("PerdeSol", cx - w / 2f - 0.2f, cy - 0.1f, zFace - 0.09f, 0.3f, h + 0.5f, 0.06f, perdeRenk, 0.05f);
                B("PerdeSag", cx + w / 2f + 0.2f, cy - 0.1f, zFace - 0.09f, 0.3f, h + 0.5f, 0.06f, perdeRenk, 0.05f);
            }
        }

        void Zarf(float genislik, float yukseklik, float zArka, float zOn, Color zemin, Color duvar, Color tavan, float zeminPar)
        {
            zemin = Zemin(zemin);
            duvar = Duvar(duvar);
            float derin = zArka - zOn;
            float zc = (zArka + zOn) / 2f;
            B("Zemin", 0f, -0.05f, zc, genislik, 0.1f, derin, zemin, zeminPar);
            B("ArkaDuvar", 0f, yukseklik / 2f, zArka + 0.06f, genislik, yukseklik, 0.12f, duvar, 0.05f);
            B("SolDuvar", -genislik / 2f - 0.06f, yukseklik / 2f, zc, 0.12f, yukseklik, derin, duvar, 0.05f);
            B("SagDuvar", genislik / 2f + 0.06f, yukseklik / 2f, zc, 0.12f, yukseklik, derin, duvar, 0.05f);
            B("Tavan", 0f, yukseklik + 0.05f, zc, genislik + 0.2f, 0.1f, derin, tavan, 0.02f);
            B("SuprugeArka", 0f, 0.09f, zArka - 0.015f, genislik, 0.18f, 0.05f, Color.Lerp(duvar, Color.black, 0.35f));
        }

        // ---------- ODA / HASTANE ----------
        void KurOda(bool hastane)
        {
            Dis = false;
            Color ahsap = K(0.46f, 0.3f, 0.2f);
            Color koyuAhsap = K(0.3f, 0.19f, 0.12f);
            if (!hastane)
            {
                Zarf(5.2f, 3f, 3.14f, -8f, ahsap, K(0.82f, 0.74f, 0.62f), K(0.88f, 0.84f, 0.78f), 0.2f);
                xMin = -1.9f; xMax = 0.0f; zMin = 2.0f; zMax = 2.7f;
            }
            else
            {
                Zarf(5.2f, 3f, 3.14f, -8f, K(0.74f, 0.8f, 0.82f), K(0.9f, 0.95f, 0.95f), K(0.95f, 0.97f, 0.97f), 0.55f);
                B("DuvarBandi", 0f, 0.9f, 3.1f, 5.2f, 0.7f, 0.03f, Vurgu(K(0.35f, 0.62f, 0.62f)), 0.2f);
                B("TavanPaneli", 0f, 2.98f, 1.5f, 1.6f, 0.04f, 0.6f, Color.white, 0.2f, 1.2f);
                Lamba("HastaneIsik", 0f, 2.7f, 1.5f, K(0.95f, 1f, 1f), 1.1f, 7f, false);
                xMin = -1.9f; xMax = -0.4f; zMin = 1.7f; zMax = 2.6f;
            }

            // pencere (yatagin ustunde) ve kapi
            PencereYap(0.9f, 1.75f, 3.14f, 0.9f, 1.2f, hastane ? K(0.9f, 0.9f, 0.92f) : koyuAhsap, true,
                       hastane ? K(0.4f, 0.65f, 0.65f) : K(0.6f, 0.2f, 0.24f));
            KapiYap(hastane ? -1.45f : -1.2f, 3.14f, 0.95f, 2.1f, hastane ? K(0.8f, 0.86f, 0.88f) : koyuAhsap, hastane ? K(0.85f, 0.85f, 0.88f) : K(0.25f, 0.16f, 0.1f));
            float kx = hastane ? -1.45f : -1.2f;
            N("kapi", kx, 0f, 2.7f, 180f);
            N("pencere", 1.0f, 0f, 1.45f, 0f);

            // yatak yan durur (baslik sagda): yatan kisi kameraya boydan gorunur
            if (!Prop("yatak", 0.65f, 0f, 2.45f, 90f, 0.7f))
            {
                if (!hastane)
                {
                    B("YatakCerceve", 0.65f, 0.25f, 2.45f, 1.9f, 0.3f, 1.1f, koyuAhsap);
                    B("YatakBasliK", 1.62f, 0.65f, 2.45f, 0.1f, 1.0f, 1.1f, koyuAhsap);
                    B("Yatak", 0.65f, 0.47f, 2.45f, 1.82f, 0.18f, 1.02f, K(0.95f, 0.92f, 0.85f), 0.05f);
                    B("Battaniye", 0.3f, 0.6f, 2.45f, 1.14f, 0.1f, 1.06f, Vurgu(K(0.28f, 0.42f, 0.7f)), 0.05f);
                    B("Yastik", 1.37f, 0.6f, 2.45f, 0.3f, 0.12f, 0.55f, Color.white, 0.05f);
                }
                else
                {
                    Color demir = K(0.55f, 0.58f, 0.64f);
                    B("HYatakCerceve", 0.65f, 0.52f, 2.45f, 1.92f, 0.09f, 0.92f, demir, 0.5f);
                    B("HYatak", 0.65f, 0.64f, 2.45f, 1.82f, 0.16f, 0.86f, K(0.93f, 0.94f, 0.96f), 0.05f);
                    B("HBattaniye", 0.3f, 0.74f, 2.45f, 1.1f, 0.07f, 0.9f, Vurgu(K(0.38f, 0.66f, 0.82f)), 0.05f);
                    B("HYastik", 1.37f, 0.76f, 2.45f, 0.3f, 0.12f, 0.5f, Color.white, 0.05f);
                    B("HBasTahta", 1.63f, 0.85f, 2.45f, 0.05f, 0.6f, 0.92f, demir, 0.5f);
                    B("HAyakTahta", -0.33f, 0.8f, 2.45f, 0.05f, 0.5f, 0.92f, demir, 0.5f);
                    B("HKorkuluk", 0.65f, 0.78f, 2.9f, 1.2f, 0.18f, 0.03f, demir, 0.6f);
                    for (int i = 0; i < 4; i++)
                        C("HYatakTeker" + i, (i % 2 == 0 ? -0.25f : 1.55f), 0.25f, (i < 2 ? 2.1f : 2.8f), 0.04f, 0.5f, K(0.4f, 0.42f, 0.46f), 0.6f);
                }
            }
            N("yatak", 0.65f, hastane ? 0.72f : 0.58f, 2.45f, 90f);

            // yan komodin + lamba / monitor
            float cx = -0.55f;
            B("Komodin", cx, 0.3f, 2.9f, 0.5f, 0.6f, 0.4f, hastane ? K(0.85f, 0.87f, 0.9f) : koyuAhsap, hastane ? 0.4f : 0.15f);
            if (!hastane)
            {
                C("LambaAyak", cx, 0.68f, 2.9f, 0.05f, 0.16f, K(0.3f, 0.2f, 0.12f));
                var abajur = S("LambaAbajur", cx, 0.86f, 2.9f, 0.3f, K(1f, 0.82f, 0.55f));
                Parlak(abajur, K(1f, 0.78f, 0.45f));
                Lamba("OdaLambasi", cx, 0.95f, 2.7f, K(1f, 0.74f, 0.42f), 1.3f, 4.5f, true);
                B("Tablo", cx, 1.75f, 3.1f, 0.5f, 0.38f, 0.03f, K(0.35f, 0.5f, 0.6f), 0.3f);
                B("TabloCerceve", cx, 1.75f, 3.115f, 0.58f, 0.46f, 0.02f, koyuAhsap);
                // yer minderi
                C("Hali", 0f, 0.01f, 0.6f, 1.15f, 0.02f, K(0.55f, 0.18f, 0.16f), 0.05f);
                C("HaliIc", 0f, 0.016f, 0.6f, 0.8f, 0.02f, K(0.82f, 0.62f, 0.32f), 0.05f);
                // sofra masasi + iki sandalye
                if (!Prop("masa", -1.45f, 0f, 1.4f, 0f, 0.75f))
                {
                    B("MasaUstu", -1.45f, 0.75f, 1.4f, 1.1f, 0.06f, 0.7f, ahsap, 0.2f);
                    for (int i = 0; i < 4; i++)
                        B("MasaBacak" + i, -1.45f + (i % 2 == 0 ? -0.5f : 0.5f), 0.36f, 1.4f + (i < 2 ? -0.3f : 0.3f), 0.06f, 0.72f, 0.06f, koyuAhsap);
                    C("Tabak", -1.7f, 0.8f, 1.4f, 0.14f, 0.03f, K(0.95f, 0.93f, 0.88f), 0.5f);
                    C("Tabak2", -1.2f, 0.8f, 1.4f, 0.14f, 0.03f, K(0.95f, 0.93f, 0.88f), 0.5f);
                    S("CaydanlikGovde", -1.45f, 0.88f, 1.4f, 0.22f, K(0.7f, 0.72f, 0.75f), 0.6f);
                }
                if (!Prop("sandalye", -1.45f, 0f, 0.75f, 180f, 0.9f))
                {
                    B("Sandalye1Oturak", -1.45f, 0.45f, 0.75f, 0.4f, 0.05f, 0.4f, koyuAhsap);
                    B("Sandalye1Sirt", -1.45f, 0.75f, 0.57f, 0.4f, 0.55f, 0.05f, koyuAhsap);
                    for (int i = 0; i < 4; i++) B("Sandalye1Bacak" + i, -1.45f + (i % 2 == 0 ? -0.17f : 0.17f), 0.22f, 0.75f + (i < 2 ? -0.17f : 0.17f), 0.04f, 0.44f, 0.04f, koyuAhsap);
                }
                if (!Prop("sandalye", -1.45f, 0f, 2.05f, 0f, 0.9f))
                {
                    B("Sandalye2Oturak", -1.45f, 0.45f, 2.05f, 0.4f, 0.05f, 0.4f, koyuAhsap);
                    B("Sandalye2Sirt", -1.45f, 0.75f, 2.23f, 0.4f, 0.55f, 0.05f, koyuAhsap);
                    for (int i = 0; i < 4; i++) B("Sandalye2Bacak" + i, -1.45f + (i % 2 == 0 ? -0.17f : 0.17f), 0.22f, 2.05f + (i < 2 ? -0.17f : 0.17f), 0.04f, 0.44f, 0.04f, koyuAhsap);
                }
                N("sofra", -0.8f, 0f, 1.4f, 270f);
            }
            else
            {
                // monitor
                B("MonitorGovde", cx, 0.85f, 2.9f, 0.42f, 0.3f, 0.1f, K(0.12f, 0.13f, 0.15f), 0.4f);
                var ek = Part(PrimitiveType.Cube, "MonitorEkran", cx, 0.86f, 2.845f, 0.36f, 0.22f, 0.01f, K(0.1f, 0.9f, 0.4f), 0.2f, 1f, null, true);
                var emr = ek.GetComponent<MeshRenderer>();
                if (emr != null) ekranMat = emr.sharedMaterial;
                N("monitor", cx, 0f, 2.4f, 0f);
                // serum askisi
                C("SerumDireK", 1.5f, 1.0f, 3.02f, 0.025f, 2f, K(0.75f, 0.77f, 0.8f), 0.6f);
                B("SerumKol", 1.42f, 1.98f, 3.02f, 0.2f, 0.03f, 0.03f, K(0.75f, 0.77f, 0.8f), 0.6f);
                B("SerumTorba", 1.34f, 1.8f, 3.02f, 0.12f, 0.24f, 0.04f, K(0.85f, 0.95f, 1f), 0.5f);
                C("SerumTaban", 1.5f, 0.03f, 3.02f, 0.2f, 0.04f, K(0.5f, 0.52f, 0.55f), 0.5f);
                N("serum", 1.5f, 0f, 1.6f, 180f);
                // ziyaretci sandalyesi ve yan masa (sofra)
                if (!Prop("sandalye", -0.9f, 0f, 1.5f, 90f, 0.9f))
                {
                    B("ZSandalyeOturak", -0.9f, 0.45f, 1.5f, 0.42f, 0.05f, 0.42f, K(0.3f, 0.45f, 0.55f));
                    B("ZSandalyeSirt", -1.08f, 0.75f, 1.5f, 0.05f, 0.5f, 0.42f, K(0.3f, 0.45f, 0.55f));
                    for (int i = 0; i < 4; i++) B("ZSandalyeBacak" + i, -0.9f + (i % 2 == 0 ? -0.17f : 0.17f), 0.22f, 1.5f + (i < 2 ? -0.17f : 0.17f), 0.04f, 0.44f, 0.04f, K(0.6f, 0.62f, 0.65f), 0.5f);
                }
                B("YanMasaUstu", -0.4f, 0.72f, 1.5f, 0.6f, 0.04f, 0.5f, K(0.85f, 0.87f, 0.9f), 0.4f);
                C("YanMasaAyak", -0.4f, 0.36f, 1.5f, 0.04f, 0.72f, K(0.6f, 0.62f, 0.65f), 0.5f);
                N("sofra", -0.4f, 0f, 1.0f, 180f);
            }
        }

        // ---------- SOKAK ----------
        void KurSokak()
        {
            Dis = true;
            B("Asfalt", 0f, -0.1f, -2.5f, 24f, 0.2f, 11f, Zemin(K(0.13f, 0.13f, 0.15f)), 0.35f);
            B("Kaldirim", 0f, 0.05f, 2.6f, 24f, 0.1f, 0.8f, K(0.5f, 0.5f, 0.53f), 0.1f);
            B("Bordur", 0f, 0.05f, 2.17f, 24f, 0.1f, 0.06f, K(0.74f, 0.74f, 0.76f), 0.1f);
            for (int i = 0; i < 12; i++) B("YolCizgi" + i, -6.6f + i * 1.2f, 0.004f, 1.0f, 0.6f, 0.01f, 0.1f, K(0.95f, 0.88f, 0.4f), 0.2f);

            // yan yana binalar (kaldirimin hemen arkasinda)
            var rnd = new System.Random(21);
            float[] gen = { 2.4f, 2.0f, 2.6f, 1.8f, 2.4f, 2.2f };
            Color[] cl = { K(0.55f, 0.42f, 0.36f), K(0.6f, 0.58f, 0.52f), K(0.42f, 0.45f, 0.52f), K(0.62f, 0.5f, 0.4f), K(0.48f, 0.4f, 0.42f), K(0.55f, 0.55f, 0.58f) };
            int kapiBina = 2, vitrinBina = 4;
            float bx = -6.3f;
            for (int i = 0; i < 6; i++)
            {
                float bw = gen[i];
                float bh = 5f + (float)rnd.NextDouble() * 4f;
                float bcx = bx + bw / 2f;
                B("Bina" + i, bcx, bh / 2f, 3.6f, bw - 0.05f, bh, 1.4f, Color.Lerp(cl[i % cl.Length], Duvar(cl[i % cl.Length]), 0.65f), 0.05f);
                int kolon = Mathf.Max(2, Mathf.FloorToInt(bw / 0.7f));
                for (int r = 0; r < 4; r++)
                    for (int cc = 0; cc < kolon; cc++)
                    {
                        bool yanik = rnd.NextDouble() < 0.45;
                        float wx = bcx - (kolon - 1) * 0.35f + cc * 0.7f;
                        float wy = 1.5f + r * 1.1f;
                        if (i == kapiBina && r == 0) continue;
                        if (i == vitrinBina && r == 0) continue;
                        Part(PrimitiveType.Cube, "BinaPencere", wx, wy, 2.88f, 0.4f, 0.55f, 0.02f,
                             yanik ? K(1f, 0.82f, 0.45f) : K(0.08f, 0.1f, 0.16f), 0.3f, yanik ? 0.9f : 0f, null, false);
                    }
                if (i == kapiBina)
                {
                    KapiYap(bcx, 2.9f, 0.9f, 2.0f, K(0.25f, 0.16f, 0.1f), K(0.2f, 0.2f, 0.22f));
                    N("kapi", bcx, 0.1f, 2.35f, 180f);
                    B("DukkanTabela", bcx, 2.55f, 2.86f, 1.2f, 0.3f, 0.05f, Vurgu(K(0.7f, 0.15f, 0.12f)), 0.2f, 0.3f);
                }
                if (i == vitrinBina)
                {
                    Part(PrimitiveType.Cube, "Vitrin", bcx, 1.2f, 2.88f, 1.4f, 1.2f, 0.03f, K(1f, 0.9f, 0.65f), 0.3f, 0.8f, null, false);
                    N("pencere", bcx, 0.1f, 2.35f, 180f);
                }
                bx += bw;
            }

            // sokak lambalari (kaldirim uzerinde)
            float[] lx = { -1.55f, 1.65f };
            for (int i = 0; i < lx.Length; i++)
            {
                float yon = lx[i] < 0f ? 1f : -1f;
                if (!Prop("lamba", lx[i], 0.1f, 2.5f, 0f, 3.4f))
                {
                    C("LambaDirek" + i, lx[i], 1.8f, 2.5f, 0.05f, 3.4f, K(0.18f, 0.18f, 0.2f), 0.4f);
                    B("LambaKol" + i, lx[i] + yon * 0.25f, 3.45f, 2.5f, 0.5f, 0.05f, 0.05f, K(0.18f, 0.18f, 0.2f), 0.4f);
                    var ampul = S("LambaAmpul" + i, lx[i] + yon * 0.45f, 3.38f, 2.5f, 0.26f, K(1f, 0.9f, 0.6f));
                    Parlak(ampul, K(1f, 0.88f, 0.55f));
                }
                Lamba("SokakIsigi" + i, lx[i] + yon * 0.45f, 3.2f, 2.2f, K(1f, 0.82f, 0.5f), 1.6f, 6f, false);
            }
            N("lamba", 1.4f, 0.1f, 2.2f, 270f);

            // bank (kaldirimda) ve kafe masasi
            if (!Prop("bank", 1.3f, 0.1f, 2.0f, 180f, 0.85f))
            {
                B("BankOturak", 1.3f, 0.55f, 2.0f, 1.2f, 0.06f, 0.4f, K(0.45f, 0.3f, 0.2f));
                B("BankSirt", 1.3f, 0.85f, 2.18f, 1.2f, 0.4f, 0.05f, K(0.45f, 0.3f, 0.2f));
                B("BankAyakSol", 0.8f, 0.32f, 2.0f, 0.06f, 0.44f, 0.36f, K(0.2f, 0.2f, 0.22f));
                B("BankAyakSag", 1.8f, 0.32f, 2.0f, 0.06f, 0.44f, 0.36f, K(0.2f, 0.2f, 0.22f));
            }
            N("bank", 1.3f, 0.1f, 1.7f, 180f);
            C("KafeMasa", 0.9f, 0.4f, 1.2f, 0.32f, 0.04f, K(0.9f, 0.9f, 0.92f), 0.4f);
            C("KafeMasaDirek", 0.9f, 0.2f, 1.2f, 0.03f, 0.4f, K(0.2f, 0.2f, 0.22f), 0.4f);
            C("KafeTabure1", 0.45f, 0.22f, 1.2f, 0.17f, 0.04f, K(0.7f, 0.2f, 0.18f));
            C("KafeTabure2", 1.35f, 0.22f, 1.2f, 0.17f, 0.04f, K(0.7f, 0.2f, 0.18f));
            N("sofra", 0.9f, 0f, 0.8f, 180f);

            // park halinde araba (arkada solda)
            if (!Prop("araba", -1.5f, 0f, 1.7f, 90f, 1.4f))
            {
                B("ArabaGovde", -1.5f, 0.42f, 1.7f, 1.9f, 0.5f, 0.85f, K(0.55f, 0.1f, 0.12f), 0.5f);
                B("ArabaKabin", -1.5f, 0.85f, 1.7f, 1.0f, 0.4f, 0.75f, K(0.45f, 0.08f, 0.1f), 0.5f);
                B("ArabaCam", -1.5f, 0.86f, 1.7f, 1.02f, 0.3f, 0.77f, K(0.15f, 0.2f, 0.3f), 0.8f);
                for (int i = 0; i < 4; i++)
                {
                    var tk = C("ArabaTekerlek" + i, -1.5f + (i % 2 == 0 ? -0.6f : 0.6f), 0.2f, 1.7f + (i < 2 ? -0.42f : 0.42f), 0.2f, 0.16f, K(0.05f, 0.05f, 0.05f), 0.1f);
                    tk.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                var fa = S("ArabaFar", -0.5f, 0.5f, 1.7f, 0.15f, K(1f, 0.95f, 0.8f));
                Parlak(fa, K(1f, 0.95f, 0.8f));
            }
            xMin = -1.0f; xMax = 1.8f; zMin = 1.3f; zMax = 2.0f;
        }

        // ---------- KOY ----------
        void KurKoy()
        {
            Dis = true;
            Color cimen = Zemin(K(0.32f, 0.46f, 0.2f));
            B("Cimen", 0f, -0.1f, 8f, 40f, 0.2f, 40f, cimen, 0.05f);
            C("ToprakYol", 0f, 0.004f, 0.8f, 1.9f, 0.01f, K(0.5f, 0.38f, 0.24f), 0.05f);
            for (int i = 0; i < 8; i++)
                B("ToprakIz" + i, -0.35f + (i % 3) * 0.1f, 0.006f, 3.2f + i * 1.4f, 0.9f - i * 0.06f, 0.01f, 1.3f, K(0.5f, 0.38f, 0.24f), 0.05f);

            // evler
            KoyEvi(-1.2f, 3.2f, 1.8f, 1.5f, 1.4f, true);
            KoyEvi(1.7f, 5.4f, 1.6f, 1.4f, 1.3f, false);
            KoyEvi(-3.0f, 7.0f, 1.8f, 1.5f, 1.4f, false);

            // aglar
            var rnd = new System.Random(5);
            float[] tx = { -2.4f, 2.5f, 0.2f, -1.6f, 3.2f, -3.6f, 1.2f };
            float[] tz = { 2.6f, 3.4f, 9.0f, 11f, 8.2f, 4.5f, 12.5f };
            for (int i = 0; i < tx.Length; i++)
            {
                float hh = 1.2f + (float)rnd.NextDouble() * 0.5f;
                if (Prop("agac", tx[i], 0f, tz[i], 0f, 2.6f)) continue;
                C("AgacGovde" + i, tx[i], hh / 2f, tz[i], 0.12f, hh, K(0.38f, 0.26f, 0.16f));
                S("AgacTaji" + i, tx[i], hh + 0.8f, tz[i], 1.7f + (float)rnd.NextDouble() * 0.5f, K(0.2f, 0.42f, 0.18f), 0.05f);
                S("AgacTaji2_" + i, tx[i] + 0.3f, hh + 0.5f, tz[i], 1.1f, K(0.24f, 0.46f, 0.2f), 0.05f);
            }

            // kuyu
            C("KuyuTas", 0.95f, 0.3f, 1.8f, 0.4f, 0.6f, K(0.55f, 0.55f, 0.56f), 0.1f);
            C("KuyuSu", 0.95f, 0.58f, 1.8f, 0.3f, 0.02f, K(0.05f, 0.1f, 0.2f), 0.9f);
            B("KuyuDirekSol", 0.6f, 0.9f, 1.8f, 0.06f, 1.2f, 0.06f, K(0.36f, 0.24f, 0.15f));
            B("KuyuDirekSag", 1.3f, 0.9f, 1.8f, 0.06f, 1.2f, 0.06f, K(0.36f, 0.24f, 0.15f));
            var cati = B("KuyuCati", 0.95f, 1.55f, 1.8f, 0.9f, 0.05f, 0.7f, K(0.55f, 0.28f, 0.2f));
            N("kuyu", 0.95f, 0f, 1.25f, 180f);

            // alcak sofra (yer sofrasi) ve minderler
            C("SofraSini", -0.85f, 0.32f, 1.25f, 0.42f, 0.04f, K(0.72f, 0.58f, 0.3f), 0.3f);
            C("SofraAyak", -0.85f, 0.15f, 1.25f, 0.1f, 0.3f, K(0.4f, 0.28f, 0.16f));
            C("Corba", -0.95f, 0.38f, 1.2f, 0.1f, 0.06f, K(0.85f, 0.8f, 0.7f), 0.5f);
            C("Ekmek", -0.72f, 0.36f, 1.3f, 0.12f, 0.02f, K(0.8f, 0.6f, 0.3f), 0.2f);
            C("Minder1", -1.4f, 0.08f, 1.25f, 0.2f, 0.12f, K(0.65f, 0.2f, 0.2f), 0.05f);
            C("Minder2", -0.3f, 0.08f, 1.25f, 0.2f, 0.12f, K(0.2f, 0.35f, 0.55f), 0.05f);
            N("sofra", -0.85f, 0f, 0.7f, 180f);

            // tepeler
            S("Tepe1", -6f, -3.5f, 22f, 16f, K(0.28f, 0.4f, 0.22f), 0.02f);
            S("Tepe2", 7f, -4.5f, 26f, 20f, K(0.3f, 0.42f, 0.24f), 0.02f);
            S("Tepe3", 0f, -6.5f, 32f, 26f, K(0.35f, 0.46f, 0.3f), 0.02f);

            // alcak cit
            for (int i = 0; i < 13; i++)
            {
                float fx = -3.6f + i * 0.6f;
                B("CitDirek" + i, fx, 0.3f, 2.1f, 0.05f, 0.6f, 0.05f, K(0.4f, 0.3f, 0.2f));
            }
            B("CitTahta1", 0f, 0.4f, 2.1f, 7.8f, 0.05f, 0.03f, K(0.45f, 0.34f, 0.22f));
            B("CitTahta2", 0f, 0.2f, 2.1f, 7.8f, 0.05f, 0.03f, K(0.45f, 0.34f, 0.22f));
            xMin = -1.9f; xMax = 1.9f; zMin = 2.4f; zMax = 3.4f;
        }

        void KoyEvi(float cx, float cz, float w, float d, float h, bool kapili)
        {
            float zFace = cz - d / 2f;
            B("EvDuvar", cx, h / 2f, cz, w, h, d, Duvar(K(0.86f, 0.8f, 0.68f)), 0.05f);
            var r1 = B("EvCati1", cx - w * 0.28f, h + 0.28f, cz, w * 0.62f, 0.07f, d + 0.3f, K(0.6f, 0.28f, 0.2f), 0.1f);
            r1.transform.localRotation = Quaternion.Euler(0f, 0f, 30f);
            var r2 = B("EvCati2", cx + w * 0.28f, h + 0.28f, cz, w * 0.62f, 0.07f, d + 0.3f, K(0.6f, 0.28f, 0.2f), 0.1f);
            r2.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
            B("EvAlin", cx, h + 0.22f, zFace - 0.03f, w * 0.95f, 0.5f, 0.04f, K(0.84f, 0.78f, 0.66f), 0.05f);
            B("EvBaca", cx + w * 0.3f, h + 0.55f, cz, 0.22f, 0.5f, 0.22f, K(0.5f, 0.4f, 0.36f), 0.05f);
            B("EvPencere", cx + w * 0.28f, h * 0.55f, zFace - 0.02f, 0.4f, 0.4f, 0.03f, K(1f, 0.8f, 0.45f), 0.3f, 0.7f);
            if (kapili)
            {
                KapiYap(cx - w * 0.15f, zFace, 0.55f, 1.0f, K(0.4f, 0.26f, 0.16f), K(0.3f, 0.2f, 0.12f));
                N("kapi", cx - w * 0.15f, 0f, zFace - 0.5f, 180f);
            }
            else
            {
                B("EvKapi", cx - w * 0.15f, 0.5f, zFace - 0.03f, 0.5f, 1.0f, 0.04f, K(0.4f, 0.26f, 0.16f), 0.1f);
            }
        }
    }

    // Arka plandaki figuran: rastgele bekleme animasyonlari, hiz ve sallanma farki
    public class Figuran3D : MonoBehaviour
    {
        Karakter3D kar;
        float faz, hiz, bazYaw;

        public void Kur(Karakter3D k)
        {
            kar = k;
            faz = Random.value * 10f;
            hiz = Random.Range(0.7f, 1.3f);
            bazYaw = transform.eulerAngles.y;
        }

        IEnumerator Start()
        {
            var an = GetComponentInChildren<Animator>();
            if (an != null) an.speed = Random.Range(0.85f, 1.15f);
            yield return new WaitForSeconds(Random.Range(0.1f, 2.5f));
            string[] varyant = { "dur", "dur", "dur", "konus", "dusun", "uzgun" };
            while (true)
            {
                if (kar != null)
                {
                    string ad = varyant[Random.Range(0, varyant.Length)];
                    if (kar.Find(ad) != null) kar.Play(ad, Random.Range(0.3f, 0.7f));
                    else if (kar.Find("dur") != null) kar.Play("dur", 0.4f);
                }
                yield return new WaitForSeconds(Random.Range(3f, 8f));
            }
        }

        void Update()
        {
            transform.rotation = Quaternion.Euler(0f, bazYaw + Mathf.Sin(Time.time * 0.4f * hiz + faz) * 5f, 0f);
        }
    }
}
