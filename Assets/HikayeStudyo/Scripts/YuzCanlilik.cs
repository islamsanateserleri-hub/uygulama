using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 3D karakteri canlandirir (Karakter3D otomatik ekler). Hepsi LateUpdate'te, Animator'un uzerine eklemeli:
    //  - agiz: konusma sesinin genligine gore (jaw kemigi > blendshape > basin hafif kimildamasi)
    //  - goz kirpma (blendshape varsa), nefes, bel/kalca salinimi, goz/bas mikro hareketleri (sakkad)
    //  - bakis modlari: konusuyor / dinliyor / dusunuyor / bos; dinleyici tepkileri (baş onayi, yere bakis)
    //  - duygu -> yuz blendshape'leri (ARKit / VRM / VRoid adlari)
    // Animator ile savasmaz: her karede once kemigi animasyonun yazdigi duruma getirir, sonra kucuk bir donme ekler.
    // Hicbir sey bulunamazsa sessizce hicbir sey yapmaz; hata olursa kendini kapatir.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(500)]
    public class YuzCanlilik : MonoBehaviour
    {
        public enum Bakis { Serbest, Hedef, Asagi, Kamera }
        enum Segment { Ileri, Kisi, Asagi, YanYukari, YanAsagi }
        enum Ch
        {
            Kirp, GozGenis, GozKis, GozAsagi, Agiz, UnluI, UnluU, UnluE, UnluO,
            Gul, KasAsagi, KasIcYukari, KasDisYukari, Somurtma, Mutlu, Rahat, Kizgin, Uzgun, Saskin, Adet
        }

        [Header("Canlilik (otomatik)")]
        public bool acik = true;
        [Range(0f, 2f)] public float canlilik = 1f;
        public bool agizHareketi = true;
        public bool bakisAcik = true;
        public bool gozKirpma = true;
        [Range(0f, 30f)] public float cenAcisi = 12f;     // jaw kemigi en fazla kac derece acilir

        // ---------- ic siniflar ----------
        class Kemik
        {
            public Transform t;
            public Quaternion temel = Quaternion.identity, yazilan = Quaternion.identity;
            public bool gecerli;

            // Animator bu kemige dokunmadiysa (yerel donme hala bizim yazdigimiz) kendi temel degerimize don
            public void Basla()
            {
                if (t == null) return;
                Quaternion cur = t.localRotation;
                if (gecerli && Quaternion.Angle(cur, yazilan) < 0.02f) t.localRotation = temel;
                else temel = cur;
            }

            public void Bitir()
            {
                if (t == null) return;
                yazilan = t.localRotation;
                gecerli = true;
            }
        }

        class Profil
        {
            public float nefesHz = 0.25f, nefesDeg = 0.6f;
            public float salinim = 1.2f, salinimHiz = 0.25f;
            public float kirpMin = 2.8f, kirpMax = 5.5f;
            public float titreme = 0f, titremeHz = 6f;
            public float basAsagi = 0f, basEgim = 0f;
            public float enerji = 1f;

            public void Yaklas(Profil h, float k)
            {
                nefesHz = Mathf.Lerp(nefesHz, h.nefesHz, k);
                nefesDeg = Mathf.Lerp(nefesDeg, h.nefesDeg, k);
                salinim = Mathf.Lerp(salinim, h.salinim, k);
                salinimHiz = Mathf.Lerp(salinimHiz, h.salinimHiz, k);
                kirpMin = Mathf.Lerp(kirpMin, h.kirpMin, k);
                kirpMax = Mathf.Lerp(kirpMax, h.kirpMax, k);
                titreme = Mathf.Lerp(titreme, h.titreme, k);
                titremeHz = Mathf.Lerp(titremeHz, h.titremeHz, k);
                basAsagi = Mathf.Lerp(basAsagi, h.basAsagi, k);
                basEgim = Mathf.Lerp(basEgim, h.basEgim, k);
                enerji = Mathf.Lerp(enerji, h.enerji, k);
            }
        }

        static Profil P(float nHz, float nDeg, float sal, float salHz, float kMin, float kMax, float trem, float tremHz, float asagi, float egim, float enerji)
        {
            return new Profil
            {
                nefesHz = nHz, nefesDeg = nDeg, salinim = sal, salinimHiz = salHz, kirpMin = kMin, kirpMax = kMax,
                titreme = trem, titremeHz = tremHz, basAsagi = asagi, basEgim = egim, enerji = enerji
            };
        }

        // Duygu -> govde canliligi. Korku: hizli kucuk titreme; uzuntu: asagida, yavas; ofke: sert ve dik.
        static Profil ProfilAl(string e)
        {
            switch (e)
            {
                case "mutlu": return P(0.28f, 0.7f, 1.8f, 0.35f, 2.5f, 5f, 0f, 6f, -1.5f, 2f, 1.25f);
                case "sevgi": return P(0.22f, 0.6f, 1.4f, 0.22f, 3f, 5.5f, 0f, 6f, 0.5f, 4f, 0.9f);
                case "uzgun": return P(0.18f, 0.8f, 0.8f, 0.15f, 4f, 7f, 0f, 6f, 6f, 1f, 0.75f);
                case "aglamakli": return P(0.5f, 1.0f, 0.8f, 0.18f, 1.8f, 3.5f, 0.8f, 5f, 7f, 2f, 0.8f);
                case "korkmus": return P(0.65f, 1.0f, 0.5f, 0.3f, 1.5f, 3f, 1.2f, 11f, -2f, 0f, 1.1f);
                case "sasirmis": return P(0.45f, 0.9f, 0.8f, 0.3f, 5f, 8f, 0f, 6f, -3f, 0f, 1.2f);
                case "kizgin": return P(0.5f, 1.0f, 0.6f, 0.35f, 4f, 7f, 0.3f, 8f, 3f, 0f, 1.35f);
                case "yorgun": return P(0.15f, 0.9f, 1.0f, 0.12f, 3.5f, 6f, 0f, 6f, 5f, 3f, 0.6f);
                case "hasta": return P(0.17f, 0.9f, 1.0f, 0.12f, 3.5f, 6f, 0.3f, 4f, 5f, 3f, 0.6f);
                case "dusunceli": return P(0.2f, 0.5f, 0.8f, 0.2f, 3.5f, 6f, 0f, 6f, 2f, 4f, 0.8f);
                case "fisilti": return P(0.22f, 0.4f, 0.6f, 0.2f, 3f, 5.5f, 0f, 6f, 2f, 0f, 0.5f);
                default: return P(0.25f, 0.6f, 1.2f, 0.25f, 2.8f, 5.5f, 0f, 6f, 0f, 0f, 1f);
            }
        }

        // Duygu -> yuz blendshape hedefleri (0..1). Ch.Kirp = gozkapagi dususu taban degeri.
        static void YuzDoldur(string e, float[] a)
        {
            switch (e)
            {
                case "mutlu": a[(int)Ch.Mutlu] = 0.8f; a[(int)Ch.Gul] = 0.6f; a[(int)Ch.GozKis] = 0.2f; break;
                case "sevgi": a[(int)Ch.Rahat] = 0.6f; a[(int)Ch.Mutlu] = 0.3f; a[(int)Ch.Gul] = 0.4f; a[(int)Ch.KasIcYukari] = 0.2f; break;
                case "uzgun": a[(int)Ch.Uzgun] = 0.8f; a[(int)Ch.Somurtma] = 0.4f; a[(int)Ch.KasIcYukari] = 0.6f; break;
                case "aglamakli": a[(int)Ch.Uzgun] = 1f; a[(int)Ch.Somurtma] = 0.6f; a[(int)Ch.KasIcYukari] = 0.8f; a[(int)Ch.GozKis] = 0.3f; break;
                case "korkmus": a[(int)Ch.Saskin] = 0.5f; a[(int)Ch.GozGenis] = 0.7f; a[(int)Ch.KasIcYukari] = 0.7f; a[(int)Ch.KasDisYukari] = 0.3f; break;
                case "sasirmis": a[(int)Ch.Saskin] = 1f; a[(int)Ch.GozGenis] = 0.8f; a[(int)Ch.KasIcYukari] = 0.5f; a[(int)Ch.KasDisYukari] = 0.7f; break;
                case "kizgin": a[(int)Ch.Kizgin] = 1f; a[(int)Ch.KasAsagi] = 0.9f; a[(int)Ch.GozKis] = 0.35f; break;
                case "yorgun": a[(int)Ch.Uzgun] = 0.25f; a[(int)Ch.Rahat] = 0.3f; a[(int)Ch.Kirp] = 0.35f; break;
                case "hasta": a[(int)Ch.Uzgun] = 0.35f; a[(int)Ch.KasIcYukari] = 0.3f; a[(int)Ch.Kirp] = 0.25f; break;
                case "dusunceli": a[(int)Ch.KasAsagi] = 0.2f; a[(int)Ch.Somurtma] = 0.1f; break;
                case "fisilti": a[(int)Ch.Rahat] = 0.25f; break;
            }
        }

        class RW
        {
            public SkinnedMeshRenderer r;
            public float[] w;
            public readonly List<int> kul = new List<int>();
        }

        class Giris
        {
            public RW rw;
            public int i;
            public float olcek = 1f;
            public int oncelik;
        }

        class Kanal
        {
            public float v;
            public readonly List<Giris> g = new List<Giris>();
            public bool Var { get { return g.Count > 0; } }
        }

        static Kanal[] YeniKanallar()
        {
            var k = new Kanal[(int)Ch.Adet];
            for (int i = 0; i < k.Length; i++) k[i] = new Kanal();
            return k;
        }

        // ---------- durum ----------
        bool hazir, bozuk;
        Karakter3D karakter;
        Animator anim;
        bool insan, gozVar, yuzVar;
        float tekrarZaman;
        readonly List<Kemik> kemikler = new List<Kemik>();
        Kemik kHips, kSpine, kChest, kUpper, kNeck, kHead, kEyeL, kEyeR, kJaw;
        readonly List<RW> rws = new List<RW>();
        readonly Kanal[] kanal = YeniKanallar();
        readonly float[] tablo = new float[(int)Ch.Adet];
        readonly float[] hedefV = new float[(int)Ch.Adet];

        float tohum, egimYon = 1f;
        Profil pc = new Profil(), ph = new Profil();
        string profilAd = "normal";

        // konusma
        bool konusuyor, icSes;
        float konusZaman, konusSure;
        float[] zarf;
        string duygu = "normal";
        YuzCanlilik muhatap;
        float agiz, konusW;

        // dinleme
        bool dinliyor;
        YuzCanlilik dinlenen;
        string dinleDuygu = "normal";
        string dinleMod = "normal";   // normal, onay, supheli
        float nodT = -1f, nodSure = 0.45f, nodGenlik = 3f, nodSonraki, nodSayi = 1f;
        float sallaT = -1f, sallaSonraki;
        float jolt, supheliW;

        // bakis
        Bakis bakisKip = Bakis.Serbest;
        YuzCanlilik bakisKisi;
        Segment seg = Segment.Ileri;
        YuzCanlilik segKisi;
        float segBitis, yanYon = 1f;
        int sonMod = -1;
        float bakYaw, bakPitch, bakYawV, bakPitchV;
        float sakadSon, sakadHY, sakadHP, sakadY, sakadP;

        // kirpma, nefes
        float kirpSonraki, kirpT = -1f, kirpTabanS;
        bool ciftKirp;
        float nefesFaz;

        // hareket
        Vector3 sonPoz;
        float hizS, L = 1f;
        string sonKlip;
        bool klipHareketli, klipYatiyor;

        // ---------- genel API ----------
        public bool Konusuyor { get { return konusuyor; } }
        public bool Dinliyor { get { return dinliyor; } }

        public string Kimlik
        {
            get
            {
                string n = gameObject.name;
                if (n.StartsWith("Oyuncu_", System.StringComparison.Ordinal)) n = n.Substring(7);
                int p = n.IndexOf("(Clone)", System.StringComparison.Ordinal);
                if (p > 0) n = n.Substring(0, p);
                return Util.Norm(n);
            }
        }

        // Bu karakterin goz/bas hizasindaki dunya noktasi (digerleri buna bakar)
        public Vector3 GozNoktasi
        {
            get
            {
                Baslat();
                float s = Mathf.Max(0.3f, transform.lossyScale.y);
                if (kHead != null && kHead.t != null) return kHead.t.position + transform.up * 0.08f * s;
                return transform.position + transform.up * 1.6f * s;
            }
        }

        public static YuzCanlilik Al(GameObject go)
        {
            if (go == null) return null;
            var y = go.GetComponent<YuzCanlilik>();
            if (y != null) return y;
            var k = go.GetComponent<Karakter3D>();
            if (k != null && !k.canliLik) return null;
            try { y = go.AddComponent<YuzCanlilik>(); }
            catch (System.Exception) { y = null; }
            return y;
        }

        public void KonusBasla(AudioClip clip, float sure, string duyguAd, bool ic, YuzCanlilik muhatapKisi)
        {
            Baslat();
            konusuyor = true;
            icSes = ic;
            konusZaman = 0f;
            konusSure = Mathf.Max(0.1f, sure);
            zarf = ic ? null : AgizSenkron.Zarf(clip);
            duygu = Util.Norm(duyguAd);
            muhatap = muhatapKisi;
            dinliyor = false;
            dinlenen = null;
        }

        public void KonusBitir() { konusuyor = false; }

        public void DinleBasla(YuzCanlilik konusan, string duyguAd, bool ic)
        {
            Baslat();
            konusuyor = false;
            if (ic) { dinliyor = false; dinlenen = null; dinleDuygu = "normal"; return; }   // ic ses: kimse tepki vermez
            dinliyor = true;
            dinlenen = konusan;
            dinleDuygu = Util.Norm(duyguAd);
            if (dinleDuygu == "korkmus" || dinleDuygu == "sasirmis") jolt = -4.5f;   // basi hafif geri cekilir
            nodSonraki = Time.time + Random.Range(0.8f, 2.0f);
            sallaSonraki = Time.time + Random.Range(1.0f, 2.5f);
        }

        public void DinlemeBitir()
        {
            dinliyor = false;
            dinlenen = null;
            dinleDuygu = "normal";
        }

        // dinle: Anne | onay|supheli|normal
        public void DinleModuKur(string mod)
        {
            Baslat();
            string m = Util.Norm(mod);
            if (m == "onay" || m == "supheli") dinleMod = m; else dinleMod = "normal";
        }

        // bak: komutlari
        public void BakKur(Bakis kip, YuzCanlilik kisi)
        {
            Baslat();
            bakisKip = kip;
            bakisKisi = kisi;
            segBitis = 0f;
        }

        // ---------- kurulum ----------
        void Start() { Baslat(); }

        void Baslat()
        {
            if (hazir) return;
            hazir = true;
            try
            {
                karakter = GetComponent<Karakter3D>();
                tohum = Random.value * 100f;
                egimYon = Random.value < 0.5f ? -1f : 1f;
                float now = Time.time;
                kirpSonraki = now + Random.Range(0.8f, 2.5f);
                sakadSon = now + Random.Range(0.2f, 1f);
                nodSonraki = now + 2f;
                sallaSonraki = now + 3f;
                sonPoz = transform.position;
                pc = ProfilAl("normal");
                ph = ProfilAl("normal");
                KemikleriBul();
                SekilleriTara();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] YuzCanlilik kurulamadi (" + name + "): " + e.Message);
                bozuk = true;
            }
        }

        void KemikleriBul()
        {
            if (anim == null)
            {
                anim = GetComponent<Animator>();
                if (anim == null) anim = GetComponentInChildren<Animator>();
            }
            if (anim == null || !anim.isHuman) return;
            insan = true;
            kHips = KemikEkle(HumanBodyBones.Hips);
            kSpine = KemikEkle(HumanBodyBones.Spine);
            kChest = KemikEkle(HumanBodyBones.Chest);
            kUpper = KemikEkle(HumanBodyBones.UpperChest);
            kNeck = KemikEkle(HumanBodyBones.Neck);
            kHead = KemikEkle(HumanBodyBones.Head);
            kEyeL = KemikEkle(HumanBodyBones.LeftEye);
            kEyeR = KemikEkle(HumanBodyBones.RightEye);
            kJaw = KemikEkle(HumanBodyBones.Jaw);
            gozVar = kEyeL != null || kEyeR != null;
        }

        Kemik KemikEkle(HumanBodyBones b)
        {
            Transform t = null;
            try { t = anim.GetBoneTransform(b); }
            catch (System.Exception) { t = null; }
            if (t == null) return null;
            for (int i = 0; i < kemikler.Count; i++) if (kemikler[i].t == t) return null;
            var k = new Kemik();
            k.t = t;
            kemikler.Add(k);
            return k;
        }

        // ---------- blendshape tarama ----------
        void SekilleriTara()
        {
            var smrs = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int s = 0; s < smrs.Length; s++)
            {
                var r = smrs[s];
                if (r == null || r.sharedMesh == null) continue;
                var mesh = r.sharedMesh;
                int n = mesh.blendShapeCount;
                if (n <= 0) continue;
                RW rw = null;
                for (int i = 0; i < n; i++)
                {
                    string ad = mesh.GetBlendShapeName(i);
                    if (string.IsNullOrEmpty(ad)) continue;
                    string son = ad;
                    int dot = ad.LastIndexOf('.');
                    if (dot >= 0 && dot < ad.Length - 1) son = ad.Substring(dot + 1);
                    string ln = son.ToLowerInvariant();
                    string nn = ln.Replace("_", "").Replace(" ", "").Replace("-", "");
                    int onc;
                    int ch = Sinif(ln, nn, out onc);
                    if (ch < 0) continue;
                    if (rw == null) { rw = new RW(); rw.r = r; rw.w = new float[n]; rws.Add(rw); }
                    var g = new Giris();
                    g.rw = rw; g.i = i; g.oncelik = onc;
                    g.olcek = OlcekAl((Ch)ch);
                    kanal[ch].g.Add(g);
                    if (!rw.kul.Contains(i)) rw.kul.Add(i);
                }
            }
            // agiz: en iyi oncelikli (jawOpen > aa) girisleri tut
            var ag = kanal[(int)Ch.Agiz].g;
            if (ag.Count > 0)
            {
                int best = 99;
                for (int i = 0; i < ag.Count; i++) if (ag[i].oncelik < best) best = ag[i].oncelik;
                for (int i = ag.Count - 1; i >= 0; i--) if (ag[i].oncelik != best) ag.RemoveAt(i);
            }
            yuzVar = false;
            for (int i = 0; i < kanal.Length; i++) if (kanal[i].Var) { yuzVar = true; break; }
        }

        static float OlcekAl(Ch c)
        {
            switch (c)
            {
                case Ch.Agiz: return 0.9f;
                case Ch.UnluI: case Ch.UnluU: case Ch.UnluE: case Ch.UnluO: return 0.6f;
                default: return 1f;
            }
        }

        // Blendshape adi -> kanal (ARKit, VRM, VRoid, Oculus viseme, Mixamo/CC adlari). -1 = eslesmedi.
        static int Sinif(string ln, string n, out int oncelik)
        {
            oncelik = 0;
            if (n.Contains("wink")) return -1;
            if (n.Contains("eyewide")) return (int)Ch.GozGenis;
            if (n.Contains("eyesquint")) return (int)Ch.GozKis;
            if (n.Contains("eyelookdown")) return (int)Ch.GozAsagi;
            if (n.Contains("blink") || n.Contains("eyeclose") || n.Contains("eyesclosed")) return (int)Ch.Kirp;

            if (n == "jawopen" || n.EndsWith("jawopen") || n == "mouthopen" || n.EndsWith("mouthopen")) { oncelik = 0; return (int)Ch.Agiz; }
            if (ln == "aa" || ln == "a" || ln.EndsWith("_aa") || ln.EndsWith("viseme_aa") || n.EndsWith("visemeaa") || n.EndsWith("mtha"))
            { oncelik = 1; return (int)Ch.Agiz; }
            if (ln == "ih" || ln == "i" || n.EndsWith("visemeih") || n.EndsWith("visemei") || n.EndsWith("mthi")) return (int)Ch.UnluI;
            if (ln == "ou" || ln == "u" || n.EndsWith("visemeou") || n.EndsWith("visemeu") || n.EndsWith("mthu")) return (int)Ch.UnluU;
            if (ln == "ee" || ln == "e" || n.EndsWith("visemeee") || n.EndsWith("visemee") || n.EndsWith("mthe")) return (int)Ch.UnluE;
            if (ln == "oh" || ln == "o" || n.EndsWith("visemeoh") || n.EndsWith("visemeo") || n.EndsWith("mtho")) return (int)Ch.UnluO;

            if (n.Contains("smile") || n.EndsWith("mthjoy")) return (int)Ch.Gul;
            if (n.Contains("browdown") || n.EndsWith("brwangry")) return (int)Ch.KasAsagi;
            if (n.Contains("browinnerup") || n.EndsWith("brwsorrow")) return (int)Ch.KasIcYukari;
            if (n.Contains("browouterup") || n.EndsWith("brwsurprised")) return (int)Ch.KasDisYukari;
            if (n.Contains("mouthfrown") || n.EndsWith("mthsorrow")) return (int)Ch.Somurtma;

            if (ln == "happy" || ln == "joy" || n.EndsWith("alljoy")) return (int)Ch.Mutlu;
            if (ln == "relaxed" || ln == "fun" || ln == "calm" || n.EndsWith("allfun")) return (int)Ch.Rahat;
            if (ln == "angry" || n.EndsWith("allangry")) return (int)Ch.Kizgin;
            if (ln == "sad" || ln == "sorrow" || n.EndsWith("allsorrow")) return (int)Ch.Uzgun;
            if (ln == "surprised" || ln == "surprise" || n.EndsWith("allsurprised")) return (int)Ch.Saskin;
            return -1;
        }

        // ---------- ana dongu ----------
        void LateUpdate()
        {
            if (!acik || bozuk) return;
            try
            {
                Guncelle(Time.deltaTime);
            }
            catch (System.Exception e)
            {
                bozuk = true;
                Debug.LogWarning("[Hikaye] Yüz canlılığı kapatıldı (" + name + "): " + e.Message);
            }
        }

        static float Gur(float x, float y)
        {
            return Mathf.Clamp((Mathf.PerlinNoise(x, y) - 0.5f) * 2.4f, -1f, 1f);
        }

        static bool DusukBakis(string e)
        {
            return e == "uzgun" || e == "aglamakli" || e == "kizgin" || e == "yorgun" || e == "hasta";
        }

        static bool KlipHareketliMi(string cn)
        {
            if (string.IsNullOrEmpty(cn)) return false;
            string s = cn.ToLowerInvariant();
            return s.Contains("walk") || s.Contains("jog") || s.Contains("run") || s.Contains("sprint") || s.Contains("gallop") ||
                   s.Contains("dance") || s.Contains("swim") || s.Contains("flying") || s.Contains("jump") || s.Contains("driving");
        }

        static bool KlipYatiyorMu(string cn)
        {
            if (string.IsNullOrEmpty(cn)) return false;
            string s = cn.ToLowerInvariant();
            return s.Contains("lying") || s.Contains("laying") || s.Contains("sleep") || s.Contains("death");
        }

        void Guncelle(float dt)
        {
            Baslat();
            if (bozuk) return;
            if (dt < 0f) dt = 0f;
            if (dt > 0.1f) dt = 0.1f;
            float now = Time.time;
            if (!insan && now >= tekrarZaman) { tekrarZaman = now + 1f; KemikleriBul(); }
            if (!insan && !yuzVar) return;

            for (int i = 0; i < kemikler.Count; i++) kemikler[i].Basla();

            // ---- hareket: yururken / yatarken canlilik azalir
            Vector3 pos = transform.position;
            float hiz = dt > 0.0001f ? (pos - sonPoz).magnitude / dt : hizS;
            sonPoz = pos;
            hizS = Mathf.Lerp(hizS, hiz, 1f - Mathf.Exp(-dt * 8f));
            string cn = karakter != null ? karakter.CurrentClip : null;
            if (cn != sonKlip)
            {
                sonKlip = cn;
                klipHareketli = KlipHareketliMi(cn);
                klipYatiyor = KlipYatiyorMu(cn);
            }
            bool yuruyor = hizS > 0.35f || klipHareketli;
            float hedefL = klipYatiyor ? 0.2f : (yuruyor ? 0.25f : 1f);
            L = Mathf.Lerp(L, hedefL, 1f - Mathf.Exp(-dt * 4f));
            bool gozAktif = bakisAcik && !yuruyor && !klipYatiyor;
            float Lc = canlilik * L;

            // ---- konusma zamani, agiz
            if (konusuyor)
            {
                konusZaman += dt;
                if (konusZaman >= konusSure + 0.15f) konusuyor = false;
            }
            float seviye = 0f;
            if (konusuyor && !icSes && agizHareketi)
                seviye = zarf != null ? AgizSenkron.Seviye(zarf, konusZaman) : AgizSenkron.Sentetik(konusZaman, tohum);
            float hed = Mathf.Clamp01(seviye * (0.8f + 0.25f * pc.enerji));
            agiz = Mathf.Lerp(agiz, hed, 1f - Mathf.Exp(-dt * (hed > agiz ? 40f : 20f)));
            konusW = Mathf.Lerp(konusW, (konusuyor && !icSes) ? 1f : 0f, 1f - Mathf.Exp(-dt * 6f));

            // ---- profil (duyguya gore genlik)
            string pad = "normal";
            if (konusuyor) pad = duygu;
            else if (dinliyor && (dinleDuygu == "uzgun" || dinleDuygu == "aglamakli")) pad = "uzgun";
            if (pad != profilAd) { profilAd = pad; ph = ProfilAl(pad); }
            pc.Yaklas(ph, 1f - Mathf.Exp(-dt * 2.5f));

            // ---- bakis
            int mod = konusuyor ? (icSes ? 4 : 1) : (dinliyor ? (dinlenen != null ? 2 : 3) : 0);
            if (mod != sonMod) { sonMod = mod; segBitis = 0f; }
            float hy = 0f, hp = 0f;
            if (gozAktif) BakisHedefi(now, mod, out hy, out hp);
            bakYaw = Mathf.SmoothDamp(bakYaw, hy, ref bakYawV, 0.14f, 600f, dt);
            bakPitch = Mathf.SmoothDamp(bakPitch, hp, ref bakPitchV, 0.14f, 600f, dt);

            // sakkad: goz/bas mikro hareketi
            if (now >= sakadSon)
            {
                sakadSon = now + Random.Range(0.35f, 1.6f);
                float amp = gozVar ? 2.8f : 0.9f;
                sakadHY = Random.Range(-amp, amp);
                sakadHP = Random.Range(-amp * 0.6f, amp * 0.6f);
            }
            float ks = 1f - Mathf.Exp(-dt * 25f);
            sakadY = Mathf.Lerp(sakadY, sakadHY, ks);
            sakadP = Mathf.Lerp(sakadP, sakadHP, ks);

            // ---- goz kirpma
            float kirpV = 0f;
            if (gozKirpma && kanal[(int)Ch.Kirp].Var)
            {
                if (kirpT < 0f && now >= kirpSonraki)
                {
                    kirpT = 0f;
                    kirpSonraki = now + Random.Range(pc.kirpMin, pc.kirpMax);
                    ciftKirp = Random.value < 0.12f;
                }
                if (kirpT >= 0f)
                {
                    kirpT += dt;
                    float d = (pc.basAsagi > 4f && pc.enerji < 0.9f) ? 0.3f : 0.18f;
                    float x = kirpT / d;
                    if (x >= 1f)
                    {
                        kirpT = -1f;
                        if (ciftKirp) { ciftKirp = false; kirpSonraki = now + 0.12f; }
                    }
                    else kirpV = x < 0.4f ? x / 0.4f : 1f - (x - 0.4f) / 0.6f;
                }
            }

            // ---- bas onayi / sallama (dinleyici)
            float nod = 0f;
            if (nodT >= 0f)
            {
                nodT += dt;
                float x = nodT / nodSure;
                if (x >= 1f) nodT = -1f;
                else nod = nodGenlik * Mathf.Abs(Mathf.Sin(x * Mathf.PI * nodSayi));
            }
            else if (dinliyor && dinleMod != "supheli" && now >= nodSonraki) NodBaslat(now);

            float salla = 0f;
            if (sallaT >= 0f)
            {
                sallaT += dt;
                float x = sallaT / 0.9f;
                if (x >= 1f) sallaT = -1f;
                else salla = 3.5f * Mathf.Sin(x * Mathf.PI * 4f) * (1f - x);
            }
            else if (dinliyor && dinleMod == "supheli" && now >= sallaSonraki)
            {
                sallaT = 0f;
                sallaSonraki = now + Random.Range(3f, 5.5f);
            }
            supheliW = Mathf.Lerp(supheliW, (dinliyor && dinleMod == "supheli") ? 1f : 0f, 1f - Mathf.Exp(-dt * 3f));
            jolt = Mathf.Lerp(jolt, 0f, 1f - Mathf.Exp(-dt * 5f));

            // ---- govde: nefes, salinim, titreme
            nefesFaz = Mathf.Repeat(nefesFaz + dt * pc.nefesHz * 6.2831853f, 6.2831853f);
            float nefes = Mathf.Sin(nefesFaz) * pc.nefesDeg * Lc;
            float sw1 = Gur(now * pc.salinimHiz + tohum, 0.13f);
            float sw2 = Gur(now * pc.salinimHiz * 0.8f + tohum, 7.7f);
            float hipsRoll = sw1 * pc.salinim * 0.7f * Lc;
            float hipsYaw = sw2 * pc.salinim * 0.5f * Lc;
            float tx = Gur(now * pc.titremeHz, tohum + 1.3f) * pc.titreme * Lc;
            float ty = Gur(now * pc.titremeHz, tohum + 4.1f) * pc.titreme * Lc;
            float tz = Gur(now * pc.titremeHz, tohum + 8.9f) * pc.titreme * Lc;

            // ---- konusurken basin hareketi
            bool jawVar = kJaw != null;
            bool agizSekil = kanal[(int)Ch.Agiz].Var;
            bool yedek = !jawVar && !agizSekil;   // agiz hareketi yok: basin hafif sallanmasi
            float konYaw = Gur(now * 0.9f + tohum, 2.2f) * 1.6f * pc.enerji * konusW * Lc;
            float konRoll = Gur(now * 0.7f + tohum, 5.1f) * 1.4f * pc.enerji * konusW * Lc;
            float beat = agiz * pc.enerji * (yedek ? 2.4f : 0.8f) * Lc;

            // ---- bas / boyun dagilimi
            float basPay = gozVar ? 0.72f : 0.95f;
            float yawB = bakYaw * basPay;
            float pitchDownB = -bakPitch * basPay;
            float sY = gozVar ? 0f : sakadY * 0.5f;
            float sP = gozVar ? 0f : sakadP * 0.5f;
            float egim = pc.basEgim * egimYon * Lc + supheliW * 5f * egimYon * Lc;

            float headYaw = yawB * 0.65f + sY + konYaw + tx * 0.8f + salla * Lc;
            float headPitch = pitchDownB * 0.65f + sP + pc.basAsagi * 0.5f * Lc + beat + nod * Lc + jolt * Lc + ty * 0.6f;
            float headRoll = egim * 0.7f + konRoll + tz * 0.6f;
            float neckYaw = yawB * 0.35f;
            float neckPitch = pitchDownB * 0.35f + pc.basAsagi * 0.35f * Lc + nod * 0.2f * Lc;
            float neckRoll = egim * 0.3f;

            Vector3 up = transform.up, right = transform.right, fwd = transform.forward;
            Uygula(kHips, up, right, fwd, hipsYaw, 0f, hipsRoll);
            Uygula(kSpine, up, right, fwd, sw2 * pc.salinim * 0.4f * Lc, pc.basAsagi * 0.15f * Lc - nefes * 0.3f, -hipsRoll * 1.1f);
            Uygula(kChest, up, right, fwd, 0f, -nefes, tz * 0.5f);
            Uygula(kUpper, up, right, fwd, 0f, -nefes * 0.6f, 0f);
            Uygula(kNeck, up, right, fwd, neckYaw, neckPitch, neckRoll);
            Uygula(kHead, up, right, fwd, headYaw, headPitch, headRoll);
            if (gozVar)
            {
                float ey = Mathf.Clamp(bakYaw * 0.35f + sakadY, -14f, 14f);
                float ep = Mathf.Clamp(bakPitch * 0.3f + sakadP, -10f, 8f);
                Uygula(kEyeL, up, right, fwd, ey, -ep, 0f);
                Uygula(kEyeR, up, right, fwd, ey, -ep, 0f);
            }
            if (jawVar) Uygula(kJaw, up, right, fwd, 0f, agiz * cenAcisi, 0f);

            for (int i = 0; i < kemikler.Count; i++) kemikler[i].Bitir();

            if (yuzVar) YuzGuncelle(dt, kirpV);
        }

        static void Uygula(Kemik k, Vector3 up, Vector3 right, Vector3 fwd, float yaw, float pitchDown, float roll)
        {
            if (k == null || k.t == null) return;
            float toplam = yaw + pitchDown + roll;
            if (float.IsNaN(toplam) || float.IsInfinity(toplam)) return;
            if (Mathf.Abs(yaw) + Mathf.Abs(pitchDown) + Mathf.Abs(roll) < 0.0005f) return;
            Quaternion q = Quaternion.AngleAxis(yaw, up) * Quaternion.AngleAxis(pitchDown, right) * Quaternion.AngleAxis(roll, fwd);
            k.t.rotation = q * k.t.rotation;
        }

        void NodBaslat(float now)
        {
            if (dinleMod == "onay")
            {
                nodGenlik = Random.Range(4f, 6.5f);
                nodSayi = Random.value < 0.4f ? 2f : 1f;
                nodSure = 0.35f * nodSayi + 0.2f;
                nodSonraki = now + Random.Range(1.3f, 2.6f);
                nodT = 0f;
            }
            else if (dinleDuygu == "uzgun" || dinleDuygu == "aglamakli")
            {
                nodGenlik = 2f; nodSayi = 1f; nodSure = 0.9f;   // yavas, anlayisli onay
                nodSonraki = now + Random.Range(3.5f, 6f);
                nodT = 0f;
            }
            else if (dinleDuygu == "kizgin")
            {
                nodSonraki = now + Random.Range(2.5f, 4f);   // kipirdamadan dinler
            }
            else
            {
                nodGenlik = Random.Range(2.2f, 3.6f); nodSayi = 1f; nodSure = 0.45f;
                nodSonraki = now + Random.Range(2.4f, 5f);
                nodT = 0f;
            }
        }

        // ---------- bakis ----------
        void BakisHedefi(float now, int mod, out float hy, out float hp)
        {
            hy = 0f; hp = 0f;
            if (bakisKip == Bakis.Asagi) { hp = -24f; return; }
            if (bakisKip == Bakis.Kamera) return;
            if (bakisKip == Bakis.Hedef && bakisKisi != null)
            {
                AciHesapla(bakisKisi.GozNoktasi, out hy, out hp);
                hy = Mathf.Clamp(hy * 0.8f, -38f, 38f);
                hp = Mathf.Clamp(hp * 0.8f, -26f, 16f);
                return;
            }
            if (now >= segBitis) SegmentSec(now, mod);
            switch (seg)
            {
                case Segment.Kisi:
                    if (segKisi != null) { AciHesapla(segKisi.GozNoktasi, out hy, out hp); hy *= 0.75f; hp *= 0.75f; }
                    break;
                case Segment.Asagi: hp = -20f; break;
                case Segment.YanYukari: hy = 20f * yanYon; hp = 9f; break;
                case Segment.YanAsagi: hy = 18f * yanYon; hp = -16f; break;
            }
            hy = Mathf.Clamp(hy, -38f, 38f);
            hp = Mathf.Clamp(hp, -26f, 16f);
        }

        void AciHesapla(Vector3 hedef, out float yaw, out float pitch)
        {
            Vector3 d = transform.InverseTransformDirection(hedef - GozNoktasi);
            float yatay = Mathf.Sqrt(d.x * d.x + d.z * d.z);
            yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            pitch = Mathf.Atan2(d.y, Mathf.Max(0.0001f, yatay)) * Mathf.Rad2Deg;
        }

        // mod: 0 bos, 1 konusuyor, 2 birini dinliyor, 3 anlaticiyi dinliyor, 4 ic ses (dusunuyor)
        void SegmentSec(float now, int mod)
        {
            float r = Random.value;
            float sure = 2f;
            segKisi = null;
            yanYon = Random.value < 0.5f ? -1f : 1f;
            if (mod == 4 || (mod == 1 && duygu == "dusunceli"))
            {
                // dusunurken: yukari-yana bakar, goz temasindan kacinir
                seg = r < 0.55f ? Segment.YanYukari : (r < 0.8f ? Segment.Asagi : Segment.Ileri);
                sure = Random.Range(1.6f, 3.4f);
            }
            else if (mod == 1)
            {
                if (muhatap != null && r < 0.55f) { seg = Segment.Kisi; segKisi = muhatap; sure = Random.Range(1.5f, 3.4f); }
                else if (r < 0.8f) { seg = Segment.Ileri; sure = Random.Range(1.0f, 2.4f); }
                else if (r < 0.92f) { seg = Segment.YanYukari; sure = Random.Range(0.6f, 1.4f); }
                else { seg = Segment.YanAsagi; sure = Random.Range(0.6f, 1.3f); }
            }
            else if (mod == 2)
            {
                bool dusuk = DusukBakis(dinleDuygu);
                float pk = dusuk ? 0.5f : 0.75f;   // konusani izleme orani (~%75)
                if (r < pk) { seg = Segment.Kisi; segKisi = dinlenen; sure = Random.Range(1.8f, 4.2f); }
                else if (r < pk + (dusuk ? 0.4f : 0.13f)) { seg = Segment.Asagi; sure = dusuk ? Random.Range(1.2f, 2.6f) : Random.Range(0.8f, 1.8f); }
                else { seg = Segment.YanAsagi; sure = Random.Range(0.5f, 1.2f); }
            }
            else if (mod == 3)
            {
                if (r < 0.6f) { seg = Segment.Ileri; sure = Random.Range(1.5f, 3.5f); }
                else if (r < 0.85f) { seg = Segment.Asagi; sure = Random.Range(0.8f, 1.6f); }
                else { seg = Segment.YanAsagi; sure = Random.Range(0.5f, 1.2f); }
            }
            else
            {
                YuzCanlilik en = YuzSahne.EnYakin(this);
                if (en != null && r < 0.2f) { seg = Segment.Kisi; segKisi = en; sure = Random.Range(1.2f, 2.5f); }
                else if (r < 0.88f) { seg = Segment.Ileri; sure = Random.Range(2f, 5f); }
                else { seg = Segment.Asagi; sure = Random.Range(0.8f, 1.5f); }
            }
            segBitis = now + sure;
            if (Random.value < 0.35f && kirpT < 0f) kirpSonraki = Mathf.Min(kirpSonraki, now + 0.05f);   // bakis degisince sik sik goz kirpilir
        }

        // ---------- yuz blendshape'leri ----------
        void YuzGuncelle(float dt, float kirpV)
        {
            System.Array.Clear(tablo, 0, tablo.Length);
            string fe = "normal";
            float fo = 0f;
            if (konusuyor) { fe = duygu; fo = 1f; }
            else if (dinliyor) { fe = dinleDuygu; fo = 0.4f; }
            YuzDoldur(fe, tablo);
            float kirpTaban = tablo[(int)Ch.Kirp] * fo;
            for (int i = 0; i < hedefV.Length; i++) hedefV[i] = tablo[i] * fo;
            if (dinliyor && dinleMod == "supheli")
            {
                hedefV[(int)Ch.KasAsagi] = Mathf.Max(hedefV[(int)Ch.KasAsagi], 0.35f);
                hedefV[(int)Ch.Somurtma] = Mathf.Max(hedefV[(int)Ch.Somurtma], 0.2f);
            }
            if (dinliyor && dinleMod == "onay") hedefV[(int)Ch.Gul] = Mathf.Max(hedefV[(int)Ch.Gul], 0.25f);

            float k6 = 1f - Mathf.Exp(-dt * 5f);
            for (int i = 0; i < kanal.Length; i++)
            {
                if (Dinamik(i)) continue;
                kanal[i].v = Mathf.Lerp(kanal[i].v, hedefV[i], k6);
            }

            // goz kapagi: kirpma + yorgunluk tabani
            kirpTabanS = Mathf.Lerp(kirpTabanS, kirpTaban, k6);
            kanal[(int)Ch.Kirp].v = Mathf.Max(kirpTabanS, kirpV);

            // asagi bakis
            float gozAsagi = bakisAcik ? Mathf.Clamp01(-bakPitch / 18f) : 0f;
            kanal[(int)Ch.GozAsagi].v = Mathf.Lerp(kanal[(int)Ch.GozAsagi].v, gozAsagi, 1f - Mathf.Exp(-dt * 12f));

            // agiz: ana sekil + degisen unluler
            int vi = 0;
            if (konusuyor) vi = Mathf.Clamp((int)(Mathf.PerlinNoise(konusZaman * 6f, tohum + 9f) * 5f), 0, 4);
            float kf = 1f - Mathf.Exp(-dt * 30f);
            bool unluVar = kanal[(int)Ch.UnluI].Var || kanal[(int)Ch.UnluU].Var || kanal[(int)Ch.UnluE].Var || kanal[(int)Ch.UnluO].Var;
            float aa = unluVar ? (vi == 0 ? 1f : 0.55f) : 1f;
            kanal[(int)Ch.Agiz].v = Mathf.Lerp(kanal[(int)Ch.Agiz].v, agiz * aa, kf);
            kanal[(int)Ch.UnluI].v = Mathf.Lerp(kanal[(int)Ch.UnluI].v, vi == 1 ? agiz * 0.75f : 0f, kf);
            kanal[(int)Ch.UnluU].v = Mathf.Lerp(kanal[(int)Ch.UnluU].v, vi == 2 ? agiz * 0.75f : 0f, kf);
            kanal[(int)Ch.UnluE].v = Mathf.Lerp(kanal[(int)Ch.UnluE].v, vi == 3 ? agiz * 0.75f : 0f, kf);
            kanal[(int)Ch.UnluO].v = Mathf.Lerp(kanal[(int)Ch.UnluO].v, vi == 4 ? agiz * 0.75f : 0f, kf);

            // agirliklari yaz
            for (int a = 0; a < rws.Count; a++)
            {
                var rw = rws[a];
                if (rw.r == null) continue;
                for (int q = 0; q < rw.kul.Count; q++) rw.w[rw.kul[q]] = 0f;
            }
            for (int c = 0; c < kanal.Length; c++)
            {
                var ch = kanal[c];
                if (ch.v <= 0.0005f) continue;
                for (int q = 0; q < ch.g.Count; q++)
                {
                    var g = ch.g[q];
                    if (g.rw.r == null) continue;
                    g.rw.w[g.i] += ch.v * 100f * g.olcek;
                }
            }
            for (int a = 0; a < rws.Count; a++)
            {
                var rw = rws[a];
                if (rw.r == null) continue;
                for (int q = 0; q < rw.kul.Count; q++)
                {
                    int idx = rw.kul[q];
                    rw.r.SetBlendShapeWeight(idx, Mathf.Clamp(rw.w[idx], 0f, 100f));
                }
            }
        }

        static bool Dinamik(int i)
        {
            return i == (int)Ch.Kirp || i == (int)Ch.GozAsagi || i == (int)Ch.Agiz ||
                   i == (int)Ch.UnluI || i == (int)Ch.UnluU || i == (int)Ch.UnluE || i == (int)Ch.UnluO;
        }
    }
}
