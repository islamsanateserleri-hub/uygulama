using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // 3D oyuncular: karakterler uzakta (y = -1000) ayri bir kamerayla cekilir ve 2D sahnenin karakter katmaninda
    // seffaf bir resim olarak gosterilir. Boylece 2D mekan, altyazi, efektler ve kamera yakinlasmasi aynen calisir.
    // 'mekan3d:' ile kodla kurulan 3D bir mekan (Mekan3D) acilirsa kamera opak olur, 2D mekanin yerini alir ve
    // KameraYonetmen tam serbest planlar kullanir. Atmosfer / zaman / gecis islemleri AtmosferVolume'dadir.
    public partial class Sahne3D : MonoBehaviour
    {
        const float W = 1080f, H = 1920f;
        public static readonly Vector3 Origin = new Vector3(0f, -1000f, 0f);
        public const float CamDist = 6.2f, CamFov = 30f;

        public Camera Cam { get; private set; }
        public KameraYonetmen Yonetmen { get; private set; }
        public Mekan3D Mekan { get; private set; }
        public AtmosferVolume Atmosfer { get; private set; }
        public Light KeyLight { get; private set; }
        public Light FillLight { get; private set; }
        public bool OdaAktif { get { return Mekan != null && Mekan.Aktif; } }

        // StoryPlayer verir: karakteri (slotuyla) sahneye yaratir; yer:/gir: komutlari henuz konusmamis karakter icin kullanir
        public System.Func<string, bool> OyuncuYarat;
        // figuran: icin kullanilabilecek 3D karakter prefablari (StoryPlayer.oyuncular)
        public List<GameObject> FiguranPrefablari;

        RenderTexture rt;
        RawImage image;
        float floorScreenY;   // zeminin 2D sahnedeki yuksekligi (kanvas birimi, merkez = 0)
        readonly Dictionary<string, Karakter3D> actors = new Dictionary<string, Karakter3D>();
        readonly Dictionary<string, float> slots = new Dictionary<string, float>();
        readonly Dictionary<string, float> yawTarget = new Dictionary<string, float>();
        readonly HashSet<string> walking = new HashSet<string>();
        readonly HashSet<string> lockedFacing = new HashSet<string>();
        string konusan = "";     // son konusan karakter
        bool konusanIcSes;       // son satir ic ses: yuzu kameraya daha cok doner
        readonly List<GameObject> extras = new List<GameObject>();

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

        // Hikayede yonetmen (Direct) komutu varsa 3D sahne (kamera + atmosfer katmani) karakter olmasa da kurulmali
        public static bool HikayeGerektirir(List<StoryItem> items)
        {
            if (items == null) return false;
            foreach (var it in items) if (it != null && it.Type == ItemType.Direct) return true;
            return false;
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
            Yonetmen = cgo.AddComponent<KameraYonetmen>();
            Yonetmen.Kur(this, Cam);
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
            KeyLight = l; FillLight = fill;

            var ri = Gfx.Rect("Oyuncular3D", charRoot);
            ri.anchorMin = ri.anchorMax = new Vector2(0.5f, 0.5f);
            ri.sizeDelta = new Vector2(W, H);
            ri.anchoredPosition = Vector2.zero;
            image = ri.gameObject.AddComponent<RawImage>();
            image.texture = rt;
            image.raycastTarget = false;

            Transform cvRoot = charRoot;
            var cv = charRoot.GetComponentInParent<Canvas>();
            if (cv != null) cvRoot = cv.rootCanvas.transform;
            Atmosfer = AtmosferVolume.Kur(this, Cam, l, fill, cvRoot);
        }

        // Zemin cizgisi kanvasta floorY'de dursun: kamera yuksekligini buna gore ayarla (mekan degisince cagrilir)
        public void SetFloor(float floorY)
        {
            floorScreenY = floorY;
            float floorV = 0.5f + floorY / H;                      // 0..1 (alttan)
            float halfH = CamDist * Mathf.Tan(CamFov * 0.5f * Mathf.Deg2Rad);
            float camY = (0.5f - floorV) * 2f * halfH;             // zemin (y=0) bu viewport yuksekliginde gorunur
            if (Yonetmen != null) Yonetmen.TemelKamera(camY);
            else if (Cam != null) Cam.transform.position = Origin + new Vector3(0f, camY, -CamDist);
        }

        public bool Has(string id) { return actors.ContainsKey(Util.Norm(id)); }
        public bool Yuruyor(string id) { return walking.Contains(Util.Norm(id)); }

        public bool HasActor(string normId) { Karakter3D a; return actors.TryGetValue(normId, out a) && a != null; }

        float WorldX(float slotX2D)
        {
            float halfH = CamDist * Mathf.Tan(CamFov * 0.5f * Mathf.Deg2Rad);
            float halfW = halfH * (W / H);
            return slotX2D / (W * 0.5f) * halfW;
        }

        // Karakterin duracagi yer. 2D mekanda kamera sabit oldugundan kadraj disina tasmasin diye sinirlanir
        // (sagda TikTok dugmeleri oldugu icin sag sinir daha icerde)
        float YerX(float slotX2D)
        {
            float x = WorldX(slotX2D);
            return OdaAktif ? x : Mathf.Clamp(x, -0.6f, 0.52f);
        }

        // ---------- Yonlendirme (yuz yonu) ----------
        // 2D mekanda eskisi gibi hafifce ortaya bakar; 3D mekanda yanindaki karaktere 3/4 doner
        float FacingYaw(string k, float x)
        {
            float baseYaw = 180f + Mathf.Clamp(-x * 12f, -30f, 30f);
            if (!OdaAktif) return baseYaw;
            float best = float.MaxValue, ox = 0f;
            bool found = false;
            foreach (var kv in actors)
            {
                if (kv.Key == k || kv.Value == null) continue;
                float cx = kv.Value.transform.position.x - Origin.x;
                float d = Mathf.Abs(cx - x);
                if (d < best) { best = d; ox = cx; found = true; }
            }
            if (!found) return baseYaw;
            return 180f - (ox >= x ? 1f : -1f) * 50f;
        }

        void ReFaceAll()
        {
            var keys = new List<string>(actors.Keys);
            foreach (var k in keys)
            {
                Karakter3D a = actors[k];
                if (a == null || lockedFacing.Contains(k)) continue;
                yawTarget[k] = FacingYaw(k, a.transform.position.x - Origin.x);
            }
        }

        void Update()
        {
            float kk = 1f - Mathf.Exp(-Time.deltaTime * Karakter3D.Hiz * 6f);
            foreach (var kv in actors)
            {
                var a = kv.Value;
                if (a == null || walking.Contains(kv.Key) || sirtustu.Contains(kv.Key)) continue;
                float yt;
                if (!yawTarget.TryGetValue(kv.Key, out yt)) continue;
                // konusan kameraya sirtini donmez (monitore, kapiya, pencereye bakiyor olsa da)
                if (kv.Key == konusan && Mathf.Abs(Mathf.DeltaAngle(yt, 180f)) > 100f) yt = FacingYaw(kv.Key, a.transform.position.x - Origin.x);
                // ic ses: kimseyle konusmuyor, yandan durmasin
                if (kv.Key == konusan && konusanIcSes && !lockedFacing.Contains(kv.Key) && Mathf.Abs(Mathf.DeltaAngle(yt, 180f)) > 30f)
                    yt = 180f + Mathf.Sign(Mathf.DeltaAngle(180f, yt)) * 25f;
                a.transform.rotation = Quaternion.Slerp(a.transform.rotation, Quaternion.Euler(0f, yt, 0f), kk);
            }
        }

        // ---------- Oyuncular ----------
        // Oyuncuyu sahneye koy (yoksa yarat). enterFrom: 0 = yerinde, -1 soldan, 1 sagdan yuruyerek
        public Karakter3D Ensure(string id, GameObject prefab, float slotX2D, float scale, int enterFrom, bool run)
        {
            var k = Util.Norm(id);
            Karakter3D a;
            if (actors.TryGetValue(k, out a) && a != null) return a;
            if (prefab == null) return null;
            var go = Instantiate(prefab, transform);
            go.name = "Oyuncu_" + id;
            a = go.GetComponent<Karakter3D>();
            if (a == null) a = go.AddComponent<Karakter3D>();
            float x = YerX(slotX2D);
            slots[k] = x;
            go.transform.localScale = Vector3.one * Mathf.Max(0.3f, scale);
            float startX = enterFrom == 0 ? x : WorldX(enterFrom * 820f);
            go.transform.position = Origin + new Vector3(startX, 0f, 0f);
            a.KafaEkseniKaydet(Kemik(a, HumanBodyBones.Head));
            actors[k] = a;
            yawTarget[k] = FacingYaw(k, x);
            go.transform.rotation = Quaternion.Euler(0f, yawTarget[k], 0f);
            if (Mekan != null && Mekan.Oturmali) { ReFaceAll(); ArabayaOturt(); return a; }
            if (enterFrom != 0) StartCoroutine(WalkTo(k, a, x, run));
            ReFaceAll();
            return a;
        }

        IEnumerator WalkTo(string k, Karakter3D a, float x, bool run)
        {
            if (a == null) yield break;
            float from = a.transform.position.x - Origin.x;
            if (Mathf.Abs(x - from) < 0.05f) yield break;
            var p = a.transform.position;
            yield return WalkToPos(k, a, new Vector3(Origin.x + x, p.y, p.z), run, true, float.NaN);
        }

        // Hedefe yuru; settle: varinca durgun animasyona gec ve yuzunu yeniden ayarla
        IEnumerator WalkToPos(string k, Karakter3D a, Vector3 target, bool run, bool settle, float finalYaw)
        {
            if (a == null) yield break;
            var t = a.transform;
            walking.Add(k);
            Play(a, new[] { run ? "kos" : "yuru", "yuru" });
            float speed = run ? 3.2f : 1.4f;
            while (a != null && (t.position - target).sqrMagnitude > 0.0004f)
            {
                if (Mekan != null && Mekan.Oturmali) { walking.Remove(k); yield break; }
                var d = target - t.position;
                var flat = new Vector3(d.x, 0f, d.z);
                if (flat.sqrMagnitude > 0.0004f)
                    t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(flat), 1f - Mathf.Exp(-Time.deltaTime * 12f));
                t.position = Vector3.MoveTowards(t.position, target, speed * Karakter3D.Hiz * Time.deltaTime);
                yield return null;
            }
            walking.Remove(k);
            if (a == null) yield break;
            if (settle)
            {
                if (!float.IsNaN(finalYaw)) { yawTarget[k] = finalYaw; lockedFacing.Add(k); }
                else yawTarget[k] = FacingYaw(k, t.position.x - Origin.x);
                Play(a, new[] { "dur" });
                ReFaceAll();
            }
        }

        public void MoveTo(string id, float slotX2D, bool run)
        {
            var k = Util.Norm(id);
            Karakter3D a;
            if (!actors.TryGetValue(k, out a) || a == null) return;
            if (Mekan != null && Mekan.Oturmali) return;   // araba icinde yurunmez
            float x = YerX(slotX2D);
            HareketBirak(k, a);
            slots[k] = x;
            lockedFacing.Remove(k);
            StartCoroutine(WalkTo(k, a, x, run));
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
            konusan = Util.Norm(id);     // ic ses satirinda da kameraya sirtini donmesin
            konusanIcSes = !talking;
            havaTaze = false;
            if (YatakKilidi(Util.Norm(id), a, talking)) return;
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
            string k = Util.Norm(id), yeni = Util.Norm(pose);
            if (Mekan != null && Mekan.Oturmali) return;     // arabada koltuktan kalkilmaz
            YatakBirak(k, a);
            string eskiPoz;
            if (yeni != "oturuyor" && pozDurum.TryGetValue(k, out eskiPoz) && eskiPoz == "oturuyor")
            {
                lockedFacing.Remove(k);
                yawTarget[k] = FacingYaw(k, a.transform.position.x - Origin.x);
            }
            if (yeni == "yatiyor" || yeni == "oturuyor") pozDurum[k] = yeni; else pozDurum.Remove(k);
            var list = Map(pose);
            if (list != null) Play(a, list);
            if (yeni == "oturuyor") Dondur(k, a, OturmaYonu(k, a.transform.position.x), true);
        }

        // Konusmadan sonra durgun hale don (yatan/oturan pozu bozma)
        public void Idle(string id, string pose)
        {
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return;
            if (YatakKilidi(Util.Norm(id), a, false)) return;
            var p = Util.Norm(pose);
            if (p == "yatiyor" || p == "oturuyor") return;
            Play(a, new[] { "dur" });
        }

        public IEnumerator Exit(string id)
        {
            var k = Util.Norm(id);
            Karakter3D a;
            if (!actors.TryGetValue(k, out a) || a == null) yield break;
            HareketBirak(k, a);
            actors.Remove(k);
            lockedFacing.Remove(k);
            float x = slots.ContainsKey(k) ? slots[k] : 0f;
            var p = a.transform.position;
            yield return WalkToPos(k, a, new Vector3(Origin.x + WorldX(x >= 0 ? 900f : -900f), p.y, p.z), false, false, float.NaN);
            if (a != null) Destroy(a.gameObject);
            yawTarget.Remove(k);
            ReFaceAll();
        }

        public void Clear()
        {
            foreach (var kv in actors) if (kv.Value != null) Destroy(kv.Value.gameObject);
            actors.Clear(); slots.Clear(); yawTarget.Clear(); walking.Clear(); lockedFacing.Clear();
            konusan = "";
            koltuklar.Clear();
            arabadaGizlenen.Clear();
            HareketTemizle();
            foreach (var e in extras) if (e != null) Destroy(e);
            extras.Clear();
            if (Yonetmen != null) Yonetmen.Sifirla();
        }

        // Kafanin 2D kanvastaki dunya konumu (kamera yakinlasmasi icin)
        public bool FaceWorld(string id, out Vector3 world)
        {
            world = Vector3.zero;
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null || image == null) return false;
            Vector3 hp = HeadOf(a);
            var v = Cam.WorldToViewportPoint(hp);
            var r = image.rectTransform;
            var local = new Vector2((v.x - 0.5f) * r.rect.width, (v.y - 0.5f) * r.rect.height);
            world = r.TransformPoint(local);
            return true;
        }

        static Vector3 HeadOf(Karakter3D a)
        {
            var an = a.GetComponent<Animator>();
            Transform head = an != null && an.isHuman ? an.GetBoneTransform(HumanBodyBones.Head) : null;
            return head != null ? head.position : a.transform.position + Vector3.up * 1.6f * a.transform.localScale.y;
        }

        // Kafanin 3D dunya konumu (KameraYonetmen icin). id normalize edilmis olmali.
        public bool HeadPos(string id, out Vector3 p)
        {
            p = Vector3.zero;
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null || ArabadaGizli(a)) return false;
            p = HeadOf(a);
            return true;
        }

        // Konusanin disindaki en yakin oyuncu (dinleyen); yoksa ""
        public string DigerOyuncu(string id)
        {
            string k = Util.Norm(id);
            Karakter3D me;
            if (!actors.TryGetValue(k, out me) || me == null) return "";
            string best = "";
            float bd = float.MaxValue;
            foreach (var kv in actors)
            {
                if (kv.Key == k || kv.Value == null || ArabadaGizli(kv.Value)) continue;
                float d = (kv.Value.transform.position - me.transform.position).sqrMagnitude;
                if (d < bd) { bd = d; best = kv.Key; }
            }
            return best;
        }

        // StoryPlayer.Say: true = 3D mekan kamerayi yonetti
        public bool KameraSatiri(string id, string emotion, bool inner)
        {
            return Yonetmen != null && Yonetmen.Satir(id, emotion, inner);
        }

        // ---------- 3D mekan, noktalar, giris/cikis ----------
        public void MekanKur(string arg)
        {
            string hata;
            var tarif = MekanTarifi.Coz(arg, out hata);
            // hatali satir eldeki 3D mekani bozmaz
            if (tarif == null) { if (hata != null) Debug.LogWarning("[Hikaye] " + hata); return; }
            if (Mekan != null && Mekan.Oturmali) ArabadanKaldir();
            if (Mekan != null) MekanDegisiyor();
            // hava mekana aittir; yeni mekana tasinmaz. Istisna: hemen once (konusma olmadan) yazilan 'hava:'
            if (!havaTaze) havaYagmur = havaKar = havaSis = false;
            havaTaze = false;
            if (Mekan != null) { Mekan.gameObject.SetActive(false); Destroy(Mekan.gameObject); Mekan = null; }
            if (tarif.Tur == "yok") { OdaDegisti(); HavaKur(); return; }
            var m = Mekan3D.Kur(transform, tarif.Tur, tarif);
            if (m == null) { OdaDegisti(); HavaKur(); return; }
            Mekan = m;
            if (tarif.Hava.Length > 0) HavaSec(tarif.Hava);
            OdaDegisti();
            if (tarif.Zaman.Length > 0) ZamanAyarla(tarif.Zaman);
            if (tarif.Isik.Length > 0) IsikAyarla(tarif.Isik);
            if (Mekan.Oturmali) ArabayaOturt();
            HavaKur();
        }

        // StoryPlayer 2D mekan degistirirken (SetBackground) cagirir
        public void MekanKapat()
        {
            if (Mekan != null) MekanKur("yok");
        }

        void OdaDegisti()
        {
            if (Atmosfer != null) Atmosfer.OdaDegisti(OdaAktif, Mekan);
            if (Yonetmen != null) Yonetmen.OdaDegisti(OdaAktif);
            ReFaceAll();
        }

        public bool NoktaBul(string ad, out Vector3 world, out float yaw)
        {
            string n = Util.Norm(ad);
            if (Mekan != null && Mekan.NoktaBul(n, out world, out yaw)) return true;
            yaw = 180f;
            switch (n)
            {
                case "sol": world = Origin + new Vector3(-0.5f, 0f, 0f); return true;
                case "sag": world = Origin + new Vector3(0.5f, 0f, 0f); return true;
                case "orta": world = Origin; return true;
            }
            world = Origin;
            return false;
        }

        Karakter3D Aktor(string id, bool yarat)
        {
            string k = Util.Norm(id);
            Karakter3D a;
            if (actors.TryGetValue(k, out a) && a != null) return a;
            if (yarat && OyuncuYarat != null && OyuncuYarat(id) && actors.TryGetValue(k, out a) && a != null) return a;
            return null;
        }

        void NoktaUyarisi(string nokta)
        {
            string liste = Mekan != null ? Mekan.NoktaListesi() : "sol, sag, orta (3D mekan yok)";
            Debug.LogWarning("[Hikaye] Nokta yok: '" + nokta + "'. Kullanılabilir: " + liste);
        }

        // yer: Karakter nokta  -> aninda o noktaya gider (yoksa once yaratilir)
        public bool Yer(string id, string nokta)
        {
            var a = Aktor(id, true);
            if (a == null) { Debug.LogWarning("[Hikaye] yer: karakter sahnede degil / 3D degil: " + id); return false; }
            if (Mekan != null && Mekan.Oturmali) return KoltugaOturt(Util.Norm(id), a, nokta);
            Vector3 w; float yaw;
            if (!NoktaBul(nokta, out w, out yaw)) { NoktaUyarisi(nokta); return false; }
            string k = Util.Norm(id);
            HareketBirak(k, a);
            if (konusan == k) konusan = "";
            a.transform.position = w;
            slots[k] = w.x - Origin.x;
            yawTarget[k] = yaw; lockedFacing.Add(k);
            a.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            ReFaceAll();
            return true;
        }

        // gir: Karakter kapi  -> kapi acilir, karakter kapidan girip yerine yuruyerek gelir (varana kadar bekler)
        public IEnumerator GirCo(string id, string kapi)
        {
            var a = Aktor(id, true);
            if (a == null) { Debug.LogWarning("[Hikaye] gir: karakter sahnede degil / 3D degil: " + id); yield break; }
            string k = Util.Norm(id);
            if (Mekan != null && Mekan.Oturmali) { ArabayaOturt(); yield break; }
            float tx = slots.ContainsKey(k) ? slots[k] : 0f;
            Vector3 target = Origin + new Vector3(tx, 0f, 0f);
            Vector3 dpos; float dyaw;
            bool noktaVar = NoktaBul(kapi, out dpos, out dyaw);
            bool kapiVar = noktaVar && Mekan != null && Mekan.KapiVar && Util.Norm(kapi).StartsWith("kapi");
            if (!noktaVar)
            {
                dpos = Origin + new Vector3(WorldX((tx >= 0f ? 1f : -1f) * 820f), 0f, 0f);
                dyaw = 180f;
            }
            HareketBirak(k, a);
            lockedFacing.Remove(k);
            a.transform.position = Origin + new Vector3(0f, -60f, 0f);   // kapi acilirken gorunmesin
            if (kapiVar) { Mekan.KapiAc(true); yield return new WaitForSeconds(0.45f); }
            if (a == null) yield break;
            a.transform.position = dpos;
            a.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            yield return WalkToPos(k, a, target, false, true, float.NaN);
            if (kapiVar && Mekan != null) Mekan.KapiAc(false);
        }

        // cikar: Karakter kapi  -> kapiya yuruyup kaybolur (beklemez)
        public void CikarKapi(string id, string kapi)
        {
            StartCoroutine(CikarKapiCo(id, kapi));
        }

        IEnumerator CikarKapiCo(string id, string kapi)
        {
            string k = Util.Norm(id);
            Karakter3D a;
            if (!actors.TryGetValue(k, out a) || a == null) yield break;
            Vector3 dpos; float dyaw;
            if (!NoktaBul(kapi, out dpos, out dyaw)) { NoktaUyarisi(kapi); yield return Exit(id); yield break; }
            HareketBirak(k, a);
            actors.Remove(k);
            lockedFacing.Remove(k);
            bool kapiVar = Mekan != null && Mekan.KapiVar && Util.Norm(kapi).StartsWith("kapi");
            if (kapiVar) Mekan.KapiAc(true);
            yield return WalkToPos(k, a, dpos, false, false, float.NaN);
            if (a != null) Destroy(a.gameObject);
            yawTarget.Remove(k);
            ReFaceAll();
            if (kapiVar)
            {
                yield return new WaitForSeconds(0.3f);
                if (Mekan != null) Mekan.KapiAc(false);
            }
        }

        // figuran: N -> arka planda N kisi (rastgele bekleme animasyonu, hiz ve sallanma farkiyla)
        public void Figuran(int n)
        {
            foreach (var e in extras) if (e != null) Destroy(e);
            extras.Clear();
            n = Mathf.Clamp(n, 0, 14);
            if (n == 0) return;
            var pool = new List<GameObject>();
            if (FiguranPrefablari != null)
                foreach (var g in FiguranPrefablari) if (g != null && g.GetComponent<Karakter3D>() != null) pool.Add(g);
            if (pool.Count == 0) { Debug.LogWarning("[Hikaye] figuran: 3D karakter prefabi bulunamadi (Karakterler/Hazir)."); return; }
            for (int i = 0; i < n; i++)
            {
                var pf = pool[Random.Range(0, pool.Count)];
                var go = Instantiate(pf, transform);
                go.name = "Figuran_" + i;
                go.transform.localScale = Vector3.one * Random.Range(0.88f, 1.05f);
                Vector3 pos; float yaw;
                if (Mekan == null || !Mekan.FiguranYeri(i, n, out pos, out yaw))
                {
                    float x = Mathf.Lerp(-1.4f, 1.4f, (i + 0.5f) / n) + Random.Range(-0.1f, 0.1f);
                    pos = Origin + new Vector3(x, 0f, 1.6f + (i % 3) * 0.7f);
                    yaw = 180f + Mathf.Clamp(-x * 20f, -35f, 35f);
                }
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                var kar = go.GetComponent<Karakter3D>();
                if (kar == null) kar = go.AddComponent<Karakter3D>();
                var fg = go.AddComponent<Figuran3D>();
                fg.Kur(kar);
                extras.Add(go);
            }
        }

        // ---------- Atmosfer / zaman / gecis (null guvenli sarmalayicilar) ----------
        public void AtmosferAyarla(string ad) { if (Atmosfer != null) Atmosfer.Atmosfer(ad); }
        public void AniAyarla(string ad) { if (Atmosfer != null) Atmosfer.Ani(ad); }
        public void ZamanAyarla(string ad) { if (Atmosfer != null) Atmosfer.Zaman(ad); HavaKur(); }
        public void IsikAyarla(string ad) { if (Atmosfer != null) Atmosfer.Isik(ad); }

        public IEnumerator GecisCo(string tur)
        {
            if (Atmosfer == null) yield break;
            yield return Atmosfer.Gecis(tur);
        }

        public IEnumerator SessizlikCo(float sn)
        {
            if (Atmosfer == null) { yield return new WaitForSeconds(sn); yield break; }
            yield return Atmosfer.Sessizlik(sn);
        }

        void OnDestroy()
        {
            if (rt != null) { rt.Release(); Destroy(rt); }
        }
    }
}
