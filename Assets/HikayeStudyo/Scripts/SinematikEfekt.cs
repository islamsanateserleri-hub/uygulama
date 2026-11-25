using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Asama 13: sinematik efekt komutlari.
    //   gren / vinyet: yok|az|normal|cok      filtre: yok|sicak|soguk|siyah_beyaz|pembe|yesil|mavi
    //   yavas: saniye|yok                     dondur: saniye
    //   gozyasi: Ad|yok                       nefes: Ad|yok      (gozyasi ve nefes yalniz 3D karakterde)
    [DefaultExecutionOrder(700)]   // kamera yonetmeni kamerayi yerlestirdikten sonra
    public class SinematikEfekt : MonoBehaviour
    {
        public static readonly string[] Komutlar = { "gren", "vinyet", "filtre", "yavas", "dondur", "gozyasi", "nefes" };
        public static readonly string[] Dereceler = { "yok", "az", "normal", "cok" };
        public static readonly string[] Filtreler = { "yok", "sicak", "soguk", "siyah_beyaz", "pembe", "yesil", "mavi" };

        public static bool Tanir(string key) { return Util.In(Util.Norm(key), Komutlar); }

        static bool Sayi(string v, out float f)
        {
            return float.TryParse((v ?? "").Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float,
                                  System.Globalization.CultureInfo.InvariantCulture, out f);
        }

        // Gecersizse Turkce uyari, gecerliyse null
        public static string Dogrula(string key, string val)
        {
            string v = (val ?? "").Trim();
            float f;
            switch (key)
            {
                case "gren":
                case "vinyet":
                    return Util.In(v, Dereceler) ? null : "'" + key + ": " + v + "' bilinmiyor (" + string.Join(", ", Dereceler) + ").";
                case "filtre":
                    return Util.In(v, Filtreler) ? null : "'filtre: " + v + "' bilinmiyor (" + string.Join(", ", Filtreler) + ").";
                case "yavas":
                    return Util.Norm(v) == "yok" || (Sayi(v, out f) && f > 0f) ? null : "'yavas: saniye' ya da 'yavas: yok' yaz (örn. yavas: 3).";
                case "dondur":
                    return Sayi(v, out f) && f > 0f ? null : "'dondur: saniye' yaz (örn. dondur: 1.5).";
                case "gozyasi":
                case "nefes":
                    return v.Length > 0 ? null : "'" + key + ": Karakter' ya da '" + key + ": yok' yaz.";
            }
            return null;
        }

        class Yuz
        {
            public string id;
            public float baslangic;
            public readonly List<Image> parcalar = new List<Image>();
        }

        RectTransform katman;
        FxLayer fx;
        System.Func<Sahne3D> sahne;
        System.Action<string> ses;
        Image tint;
        Coroutine yavasCo;
        readonly Dictionary<string, Yuz> yaslar = new Dictionary<string, Yuz>();
        readonly Dictionary<string, Yuz> nefesler = new Dictionary<string, Yuz>();
        bool uyarildi;

        public static SinematikEfekt Kur(RectTransform root, FxLayer fx, System.Func<Sahne3D> sahne, System.Action<string> ses)
        {
            var r = Gfx.Rect("Sinema", root);
            Gfx.Stretch(r);
            r.SetSiblingIndex(fx.transform.GetSiblingIndex());   // gren ve kenar karartmanin altinda kalsin
            var s = r.gameObject.AddComponent<SinematikEfekt>();
            s.katman = r; s.fx = fx; s.sahne = sahne; s.ses = ses;
            s.tint = Gfx.Img("Filtre2D", r, null, new Color(0, 0, 0, 0), Vector2.zero, Vector2.zero);
            Gfx.Stretch(s.tint.rectTransform);
            Karakter3D.Hiz = 1f;
            return s;
        }

        public IEnumerator Calistir(StoryItem it, bool hizli)
        {
            string key = Util.Norm(it.Character);
            string arg = (it.Arg ?? "").Trim(), n = Util.Norm(arg);
            if (Dogrula(key, arg) != null) { Debug.LogWarning("[Hikaye] " + it.Line + ". satır: " + Dogrula(key, arg)); yield break; }
            float sn;
            switch (key)
            {
                case "gren": fx.Gren(n); break;
                case "vinyet": fx.Vinyet(n); break;
                case "filtre": Filtre(n); break;
                case "yavas":
                    if (hizli) break;
                    YavasBitir();
                    if (n != "yok" && Sayi(arg, out sn)) yavasCo = StartCoroutine(Yavas(Mathf.Clamp(sn, 0.3f, 20f)));
                    break;
                case "dondur":
                    if (hizli || !Sayi(arg, out sn)) break;
                    yield return Dondur(Mathf.Clamp(sn, 0.2f, 10f));
                    break;
                case "gozyasi": YuzEfekti(yaslar, arg, it.Line, 6); break;
                case "nefes": YuzEfekti(nefesler, arg, it.Line, 6); break;
            }
        }

        void Filtre(string n)
        {
            var s = sahne();
            if (s != null && s.Atmosfer != null) { s.Atmosfer.Filtre(n); tint.color = new Color(0, 0, 0, 0); return; }
            // 3D katman yoksa yalniz renk ortusu (siyah beyaz yapilamaz)
            switch (n)
            {
                case "sicak": tint.color = new Color(1f, 0.6f, 0.3f, 0.14f); break;
                case "soguk": tint.color = new Color(0.3f, 0.5f, 1f, 0.14f); break;
                case "siyah_beyaz": tint.color = new Color(0.5f, 0.5f, 0.5f, 0.35f); break;
                case "pembe": tint.color = new Color(1f, 0.5f, 0.7f, 0.14f); break;
                case "yesil": tint.color = new Color(0.3f, 0.8f, 0.4f, 0.14f); break;
                case "mavi": tint.color = new Color(0.2f, 0.3f, 0.9f, 0.18f); break;
                default: tint.color = new Color(0, 0, 0, 0); break;
            }
        }

        // karakter animasyonlari ve yurume yavaslar; konusma sesi normal akar
        IEnumerator Yavas(float sure)
        {
            Karakter3D.Hiz = 0.35f;
            if (ses != null) ses("whoosh");
            yield return new WaitForSeconds(sure);
            Karakter3D.Hiz = 1f;
            yavasCo = null;
        }

        void YavasBitir()
        {
            if (yavasCo != null) StopCoroutine(yavasCo);
            yavasCo = null;
            Karakter3D.Hiz = 1f;
        }

        IEnumerator Dondur(float sure)
        {
            YavasBitir();
            var s = sahne();
            Karakter3D.Hiz = 0f;
            fx.Donuk = true;
            fx.Flash(0.55f);
            if (ses != null) ses("flas");
            if (s != null && s.Atmosfer != null) s.Atmosfer.DondurGorunum(true);
            yield return new WaitForSeconds(sure);
            Karakter3D.Hiz = 1f;
            fx.Donuk = false;
            if (s != null && s.Atmosfer != null) s.Atmosfer.DondurGorunum(false);
        }

        void YuzEfekti(Dictionary<string, Yuz> liste, string arg, int satir, int adet)
        {
            string k = Util.Norm(arg);
            if (k == "yok") { Temizle(liste); return; }
            var s = sahne();
            if (s == null)
            {
                if (!uyarildi) Debug.LogWarning("[Hikaye] " + satir + ". satır: gözyaşı ve nefes buharı yalnız 3D karakterde görünür.");
                uyarildi = true;
                return;
            }
            if (liste.ContainsKey(k)) return;
            var y = new Yuz { id = k, baslangic = Time.time };
            for (int i = 0; i < adet; i++)
            {
                bool yas = liste == yaslar;
                var img = Gfx.Img(yas ? "Yas" : "Buhar", katman, yas ? Gfx.Circle : Gfx.SoftDot, new Color(1, 1, 1, 0), new Vector2(10, 10), Vector2.zero);
                img.raycastTarget = false;
                y.parcalar.Add(img);
            }
            liste[k] = y;
        }

        static void Temizle(Dictionary<string, Yuz> liste)
        {
            foreach (var y in liste.Values) foreach (var p in y.parcalar) if (p != null) Destroy(p.gameObject);
            liste.Clear();
        }

        // Mekan degisince kisiye bagli efektler biter (renk filtresi, gren, vinyet kalir)
        public void MekanDegisti()
        {
            Temizle(yaslar);
            Temizle(nefesler);
            YavasBitir();
        }

        void LateUpdate()
        {
            if (yaslar.Count == 0 && nefesler.Count == 0) return;
            var s = sahne();
            foreach (var y in yaslar.Values) YasCiz(s, y);
            foreach (var y in nefesler.Values) NefesCiz(s, y);
        }

        // dunya noktasini efekt katmanina izdusurur; olcek = 1 metrenin piksel karsiligi
        bool Izdusum(Sahne3D s, Vector3 w, out Vector2 p, out float olcek)
        {
            p = Vector2.zero; olcek = 0f;
            Vector3 a, b;
            if (!s.EkranNoktasi(w, out a) || !s.EkranNoktasi(w + s.Cam.transform.up * 0.1f, out b)) return false;
            p = katman.InverseTransformPoint(a);
            olcek = Vector2.Distance(p, katman.InverseTransformPoint(b)) * 10f;
            return olcek > 1f;
        }

        static void Gizle(Yuz y) { foreach (var p in y.parcalar) if (p != null) p.color = new Color(1, 1, 1, 0); }

        static void Koy(Image img, Vector2 yer, float en, float boy, Color c)
        {
            var r = img.rectTransform;
            r.anchoredPosition = yer;
            r.sizeDelta = new Vector2(en, boy);
            img.color = c;
        }

        // iki gozden sirayla suzulen damlalar (her goz: damla + parilti + iz)
        void YasCiz(Sahne3D s, Yuz y)
        {
            Vector3 sol, sag, agiz, ileri, yukari;
            float boy;
            if (s == null || !s.YuzNoktalari(y.id, out sol, out sag, out agiz, out ileri, out yukari, out boy)) { Gizle(y); return; }
            if (Vector3.Dot(ileri, (s.Cam.transform.position - sol).normalized) < 0.35f) { Gizle(y); return; }
            for (int g = 0; g < 2; g++)
            {
                float t = Mathf.Repeat(Time.time - y.baslangic - g * 1.1f, 2.4f) / 1.8f;   // 0..1 akar, sonra bekler
                bool var1 = Time.time - y.baslangic >= g * 1.1f && t <= 1f;
                Vector3 goz = g == 0 ? sol : sag;
                Vector3 bas = goz - yukari * 0.022f * boy;
                Vector3 w = bas - yukari * (0.085f * boy * t * t);
                Vector2 p, p0; float o, o0;
                var damla = y.parcalar[g * 3]; var parilti = y.parcalar[g * 3 + 1]; var iz = y.parcalar[g * 3 + 2];
                if (!var1 || !Izdusum(s, w, out p, out o) || !Izdusum(s, bas, out p0, out o0))
                {
                    damla.color = parilti.color = iz.color = new Color(1, 1, 1, 0);
                    continue;
                }
                float a = Mathf.Clamp01(t * 8f) * Mathf.Clamp01((1f - t) * 5f);
                float en = 0.011f * boy * o;
                Koy(iz, (p + p0) * 0.5f, en * 0.35f, Vector2.Distance(p, p0), new Color(0.82f, 0.92f, 1f, 0.35f * a));
                iz.rectTransform.localRotation = Quaternion.FromToRotation(Vector3.up, (Vector3)(p0 - p));
                Koy(damla, p, en, en * 1.4f, new Color(0.75f, 0.88f, 1f, 0.85f * a));
                Koy(parilti, p + new Vector2(-en * 0.18f, en * 0.25f), en * 0.35f, en * 0.35f, new Color(1f, 1f, 1f, a));
            }
        }

        // agizdan duzenli araliklarla cikan buhar bulutlari
        void NefesCiz(Sahne3D s, Yuz y)
        {
            Vector3 sol, sag, agiz, ileri, yukari;
            float boy;
            if (s == null || !s.YuzNoktalari(y.id, out sol, out sag, out agiz, out ileri, out yukari, out boy)) { Gizle(y); return; }
            for (int i = 0; i < y.parcalar.Count; i++)
            {
                float gecen = Time.time - y.baslangic - i * 0.09f;
                float t = Mathf.Repeat(gecen, 2.6f) / 1.5f;
                var img = y.parcalar[i];
                Vector3 w = agiz + ileri * ((0.03f + 0.16f * t) * boy) + Vector3.up * (0.05f * t * boy)
                          + s.Cam.transform.right * (((i % 3) - 1) * 0.02f * t * boy);
                Vector2 p; float o;
                if (gecen < 0f || t > 1f || !Izdusum(s, w, out p, out o)) { img.color = new Color(1, 1, 1, 0); continue; }
                float cap = (0.05f + 0.13f * t) * boy * o;
                Koy(img, p, cap, cap * 0.8f, new Color(0.95f, 0.97f, 1f, 0.5f * Mathf.Clamp01(t * 5f) * (1f - t)));
            }
        }

        void OnDestroy() { Karakter3D.Hiz = 1f; }
    }
}
