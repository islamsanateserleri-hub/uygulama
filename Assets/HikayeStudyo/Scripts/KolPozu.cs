using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Animasyon klibi olmayan hareketler (sarilma, el tutma, esya tutma, kapi koluna uzanma, basit tepkiler) icin
    // insan iskeletinin uzerine LateUpdate'te basit iki kemikli IK, govde egimi ve bas hareketi ekler.
    // Yalnizca Sahne3D bir hareket komutu calistirinca eklenir; eklenmemis karakterlerde hicbir etkisi yoktur.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(600)]   // YuzCanlilik (500) govdeyi oynattiktan sonra
    public class KolPozu : MonoBehaviour
    {
        // Ayni kolu isteyen katmanlardan buyuk olan kazanir
        public enum Katman { Esya = 0, Etkilesim = 1, Gecici = 2 }
        const int KatmanSayisi = 3;

        class Hedef { public System.Func<Vector3> el, dirsek; }

        class Kol
        {
            public Transform ust, alt, el;
            public readonly Hedef[] hedef = new Hedef[KatmanSayisi];
            public float w;
            public Vector3 yEl, yDirsek;   // karakterin yerel uzayinda yumusatilmis hedef

            public Hedef Aktif()
            {
                for (int i = KatmanSayisi - 1; i >= 0; i--) if (hedef[i] != null) return hedef[i];
                return null;
            }
        }

        Animator anim;
        Karakter3D karakter;
        readonly Kol sag = new Kol(), sol = new Kol();
        Transform omurga, bas;
        readonly float[] egim = new float[KatmanSayisi];
        float egimS;
        int basKip;            // 0 yok, 1 onaylama, 2 reddetme
        float basZaman;
        GameObject esya;
        bool esyaSag;
        readonly List<Material> esyaMat = new List<Material>();

        // Insan iskeleti ve kol kemikleri varsa bileseni dondurur, yoksa null
        public static KolPozu Al(Karakter3D k)
        {
            if (k == null) return null;
            var an = InsanAnimator(k.gameObject);
            if (an == null) return null;
            var p = k.GetComponent<KolPozu>();
            if (p == null) p = k.gameObject.AddComponent<KolPozu>();
            p.Kur(k, an);
            return p;
        }

        // Humanoid ve iki kolun uc kemigi de bulunuyorsa Animator, yoksa null
        public static Animator InsanAnimator(GameObject go)
        {
            if (go == null) return null;
            var an = go.GetComponent<Animator>();
            if (an == null) an = go.GetComponentInChildren<Animator>();
            if (an == null || an.avatar == null || !an.isHuman) return null;
            var kemikler = new[]
            {
                HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand
            };
            foreach (var b in kemikler) if (an.GetBoneTransform(b) == null) return null;
            return an;
        }

        void Kur(Karakter3D k, Animator an)
        {
            if (anim != null) return;
            karakter = k;
            anim = an;
            sag.ust = an.GetBoneTransform(HumanBodyBones.RightUpperArm);
            sag.alt = an.GetBoneTransform(HumanBodyBones.RightLowerArm);
            sag.el = an.GetBoneTransform(HumanBodyBones.RightHand);
            sol.ust = an.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            sol.alt = an.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            sol.el = an.GetBoneTransform(HumanBodyBones.LeftHand);
            omurga = an.GetBoneTransform(HumanBodyBones.Spine);
            bas = an.GetBoneTransform(HumanBodyBones.Head);
        }

        // Elin gidecegi dunya noktasi ve dirsegin bakacagi nokta (her karede yeniden sorulur)
        public void El(bool sagEl, Katman k, System.Func<Vector3> el, System.Func<Vector3> dirsek)
        {
            (sagEl ? sag : sol).hedef[(int)k] = new Hedef { el = el, dirsek = dirsek };
        }

        // Govde one egilme (derece; eksi = geriye)
        public void Egim(Katman k, float derece) { egim[(int)k] = derece; }

        public void BasHareketi(int kip) { basKip = kip; basZaman = 0f; }

        public void Birak(Katman k)
        {
            sag.hedef[(int)k] = null;
            sol.hedef[(int)k] = null;
            egim[(int)k] = 0f;
            if (k == Katman.Gecici) basKip = 0;
        }

        // ---------- esya ----------
        public void EsyaTak(string tur, bool sagEl, Transform kok)
        {
            EsyaSil();
            esya = EsyaOlustur(Util.Norm(tur), kok);
            esyaSag = sagEl;
        }

        public void EsyaSil()
        {
            if (esya != null) Destroy(esya);
            esya = null;
            foreach (var m in esyaMat) if (m != null) Destroy(m);
            esyaMat.Clear();
        }

        GameObject EsyaOlustur(string tur, Transform kok)
        {
            var go = new GameObject("Esya_" + tur);
            go.transform.SetParent(kok, false);
            var t = go.transform;
            switch (tur)
            {
                case "bardak":
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.02f, 0f), new Vector3(0.07f, 0.055f, 0.07f), new Color(0.75f, 0.88f, 0.95f));
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.076f, 0f), new Vector3(0.058f, 0.002f, 0.058f), new Color(0.35f, 0.55f, 0.75f));
                    break;
                case "telefon":
                    Parca(PrimitiveType.Cube, t, new Vector3(0f, 0.04f, 0f), new Vector3(0.075f, 0.15f, 0.012f), new Color(0.08f, 0.08f, 0.1f));
                    Parca(PrimitiveType.Cube, t, new Vector3(0f, 0.04f, -0.0065f), new Vector3(0.063f, 0.128f, 0.002f), new Color(0.35f, 0.6f, 0.9f));
                    break;
                case "mektup":
                    Parca(PrimitiveType.Cube, t, new Vector3(0f, 0.03f, 0f), new Vector3(0.17f, 0.115f, 0.006f), new Color(0.96f, 0.92f, 0.8f));
                    Parca(PrimitiveType.Sphere, t, new Vector3(0f, 0.03f, -0.005f), new Vector3(0.025f, 0.025f, 0.008f), new Color(0.7f, 0.1f, 0.1f));
                    break;
                case "kitap":
                    Parca(PrimitiveType.Cube, t, new Vector3(0f, 0.05f, 0f), new Vector3(0.15f, 0.2f, 0.04f), new Color(0.45f, 0.18f, 0.14f));
                    Parca(PrimitiveType.Cube, t, new Vector3(0.006f, 0.05f, 0f), new Vector3(0.146f, 0.188f, 0.032f), new Color(0.95f, 0.93f, 0.86f));
                    break;
                case "cicek":
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.05f, 0f), new Vector3(0.015f, 0.1f, 0.015f), new Color(0.2f, 0.5f, 0.2f));
                    for (int i = 0; i < 3; i++)
                        Parca(PrimitiveType.Sphere, t, new Vector3((i - 1) * 0.04f, 0.16f + (i == 1 ? 0.025f : 0f), 0f), Vector3.one * 0.05f, new Color(0.92f, 0.35f, 0.5f));
                    break;
                case "ilac":
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.025f, 0f), new Vector3(0.045f, 0.04f, 0.045f), new Color(0.95f, 0.95f, 0.95f));
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.075f, 0f), new Vector3(0.05f, 0.01f, 0.05f), new Color(0.8f, 0.2f, 0.2f));
                    break;
                case "tabak":
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.02f, 0f), new Vector3(0.22f, 0.012f, 0.22f), new Color(0.95f, 0.93f, 0.88f));
                    Parca(PrimitiveType.Sphere, t, new Vector3(0f, 0.04f, 0f), new Vector3(0.12f, 0.05f, 0.12f), new Color(0.85f, 0.6f, 0.3f));
                    break;
                case "fener":   // aluminyum el feneri
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.04f, 0f), new Vector3(0.038f, 0.085f, 0.038f), new Color(0.62f, 0.64f, 0.68f));
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.135f, 0f), new Vector3(0.062f, 0.018f, 0.062f), new Color(0.4f, 0.42f, 0.46f));
                    Parca(PrimitiveType.Cylinder, t, new Vector3(0f, 0.156f, 0f), new Vector3(0.05f, 0.003f, 0.05f), new Color(0.95f, 0.93f, 0.7f));
                    break;
                default:   // anahtar
                    Parca(PrimitiveType.Sphere, t, new Vector3(0f, 0.07f, 0f), new Vector3(0.045f, 0.045f, 0.012f), new Color(0.85f, 0.68f, 0.25f));
                    Parca(PrimitiveType.Cube, t, new Vector3(0f, 0.02f, 0f), new Vector3(0.012f, 0.08f, 0.008f), new Color(0.85f, 0.68f, 0.25f));
                    Parca(PrimitiveType.Cube, t, new Vector3(0.014f, -0.005f, 0f), new Vector3(0.022f, 0.012f, 0.008f), new Color(0.85f, 0.68f, 0.25f));
                    break;
            }
            return go;
        }

        void Parca(PrimitiveType tip, Transform parent, Vector3 pos, Vector3 olcek, Color renk)
        {
            var go = GameObject.CreatePrimitive(tip);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = olcek;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) return;
            Shader sh = null;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null) sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", renk);
            if (m.HasProperty("_Color")) m.SetColor("_Color", renk);
            mr.sharedMaterial = m;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            esyaMat.Add(m);
        }

        // ---------- her kare ----------
        void LateUpdate()
        {
            if (anim == null) return;
            // Animator kemikleri her karede yeniden yazmiyorsa eklemeler birikir: klip oynamiyorsa dokunma
            if (karakter == null || string.IsNullOrEmpty(karakter.CurrentClip)) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            if (dt <= 0f) return;

            float istenenEgim = 0f;
            for (int i = 0; i < KatmanSayisi; i++) if (Mathf.Abs(egim[i]) > Mathf.Abs(istenenEgim)) istenenEgim = egim[i];
            egimS = Mathf.Lerp(egimS, istenenEgim, 1f - Mathf.Exp(-dt * 4f));
            if (omurga != null && Mathf.Abs(egimS) > 0.05f)
                omurga.rotation = Quaternion.AngleAxis(egimS, transform.right) * omurga.rotation;

            if (basKip != 0 && bas != null)
            {
                basZaman += dt;
                if (basZaman > 1.5f) basKip = 0;
                else
                {
                    float aci = Mathf.Sin(basZaman * 10f) * 11f * (1f - basZaman / 1.5f);
                    bas.rotation = Quaternion.AngleAxis(aci, basKip == 1 ? transform.right : transform.up) * bas.rotation;
                }
            }

            Coz(sag, dt);
            Coz(sol, dt);

            if (esya != null)
            {
                var k = esyaSag ? sag : sol;
                if (k.el != null && k.alt != null)
                {
                    Vector3 yon = (k.el.position - k.alt.position).normalized;
                    esya.transform.position = k.el.position + yon * 0.06f;
                    esya.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                }
            }
        }

        void Coz(Kol k, float dt)
        {
            if (k.ust == null || k.alt == null || k.el == null) return;
            var h = k.Aktif();
            if (h != null)
            {
                Vector3 el = transform.InverseTransformPoint(h.el());
                Vector3 dirsek = transform.InverseTransformPoint(h.dirsek());
                if (k.w <= 0.001f) { k.yEl = el; k.yDirsek = dirsek; }
                else
                {
                    float t = 1f - Mathf.Exp(-dt * 10f);
                    k.yEl = Vector3.Lerp(k.yEl, el, t);
                    k.yDirsek = Vector3.Lerp(k.yDirsek, dirsek, t);
                }
            }
            k.w = Mathf.MoveTowards(k.w, h != null ? 1f : 0f, dt * 2.2f);
            if (k.w <= 0.001f) return;
            Quaternion u0 = k.ust.localRotation, a0 = k.alt.localRotation;
            IkiKemik(k.ust, k.alt, k.el, transform.TransformPoint(k.yEl), transform.TransformPoint(k.yDirsek));
            float s = k.w * k.w * (3f - 2f * k.w);
            k.ust.localRotation = Quaternion.Slerp(u0, k.ust.localRotation, s);
            k.alt.localRotation = Quaternion.Slerp(a0, k.alt.localRotation, s);
        }

        // Analitik iki kemik IK: omuz-dirsek-el. Hedef kolun boyundan uzaksa kol o yone uzanir.
        static void IkiKemik(Transform ust, Transform alt, Transform uc, Vector3 hedef, Vector3 dirsekIpucu)
        {
            Vector3 s = ust.position;
            float a = Vector3.Distance(s, alt.position), b = Vector3.Distance(alt.position, uc.position);
            if (a < 1e-4f || b < 1e-4f) return;
            Vector3 d = hedef - s;
            float c = d.magnitude;
            if (c < 1e-4f) return;
            d /= c;
            c = Mathf.Clamp(c, Mathf.Abs(a - b) + 0.001f, (a + b) * 0.999f);
            Vector3 p = Vector3.ProjectOnPlane(dirsekIpucu - s, d);
            if (p.sqrMagnitude < 1e-8f) p = Vector3.ProjectOnPlane(Vector3.down, d);
            if (p.sqrMagnitude < 1e-8f) return;
            p.Normalize();
            float cosA = Mathf.Clamp((a * a + c * c - b * b) / (2f * a * c), -1f, 1f);
            float sinA = Mathf.Sqrt(1f - cosA * cosA);
            Vector3 yeniDirsek = s + (d * cosA + p * sinA) * a;
            ust.rotation = Quaternion.FromToRotation(alt.position - s, yeniDirsek - s) * ust.rotation;
            Vector3 hedefUc = s + d * c;
            alt.rotation = Quaternion.FromToRotation(uc.position - alt.position, hedefUc - alt.position) * alt.rotation;
        }

        void OnDestroy() { EsyaSil(); }
    }
}
