using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HikayeStudyo
{
    // Veriye dayalı karakter çeşitleri: temel insan modeli + sıra numarasından hesaplanan görünüm.
    // Prefab dosyası ÜRETİLMEZ. Ad = "<temel prefab adı>_NNN" (örn. VRM_Anne_005); kısa ad da olur (Anne_005).
    // Aynı ad her çalıştırmada aynı görünümü verir (temel modelin malzemeleri ve bu dosyadaki tablolar değişmedikçe).
    // Yalnız adından güvenle tanınan parçalar boyanır; tanınmayan parçaya dokunulmaz.

    public enum VaryantParca { Yok, Sac, Ust, Alt, Ten }

    public class VaryantYuvasi
    {
        public Renderer renderer;
        public string rendererAdi = "";
        public int yuva;
        public Material malzeme;
        public string malzemeAdi = "";
        public VaryantParca parca;
    }

    public class VaryantParcalari
    {
        public bool sac, ust, alt, ten, canta;
        public bool RenkVar { get { return sac || ust || alt || ten || canta; } }

        public string Ozet()
        {
            var l = new List<string>();
            if (ust) l.Add("üst");
            if (alt) l.Add("alt");
            if (sac) l.Add("saç");
            if (ten) l.Add("ten");
            if (canta) l.Add("çanta");
            return l.Count == 0 ? "(hiçbiri: yalnız boy/en değişir)" : string.Join(", ", l.ToArray());
        }
    }

    // 0 = modelin kendi rengi
    public struct VaryantTarifi
    {
        public int ust, alt, sac, ten, olcek;
        public bool cantaGizli;

        public int Secim(VaryantParca p)
        {
            switch (p)
            {
                case VaryantParca.Ust: return ust;
                case VaryantParca.Alt: return alt;
                case VaryantParca.Sac: return sac;
                case VaryantParca.Ten: return ten;
                default: return 0;
            }
        }
    }

    public class VaryantBilgi
    {
        public string ad;
        public GameObject taban;
        public int no;
        public VaryantTarifi tarif;
        public VaryantParcalari parcalar;
        public string aciklama;
    }

    public static class Varyantlar
    {
        public const int HedefToplam = 1000;
        public const int AzamiNo = 999;

        static readonly string[] insanOnekleri = { "Erkek_", "Kadin_", "Mixamo_", "VRM_" };

        // ---------- renk tabloları ----------
        // Düz renkli (dokusuz) malzemeler: renk doğrudan verilir. Dokulu malzemeler: mevcut renk bu çarpanla çarpılır.
        static Color32 C(byte r, byte g, byte b) { return new Color32(r, g, b, 255); }
        static Color M(float r, float g, float b) { return new Color(r, g, b, 1f); }

        static readonly string[] ustAd = { "kırmızı", "mavi", "yeşil", "hardal", "mor", "pembe", "siyah", "krem", "turkuaz", "kahve" };
        static readonly Color[] ustDuz =
        {
            C(184, 59, 59), C(47, 93, 168), C(46, 125, 79), C(217, 154, 43), C(111, 74, 158),
            C(212, 102, 138), C(42, 45, 54), C(226, 220, 204), C(31, 138, 138), C(138, 90, 59)
        };
        static readonly Color[] ustCarpan =
        {
            M(1.15f, 0.42f, 0.42f), M(0.45f, 0.65f, 1.25f), M(0.48f, 0.95f, 0.55f), M(1.2f, 0.88f, 0.32f), M(0.82f, 0.5f, 1.15f),
            M(1.3f, 0.65f, 0.85f), M(0.38f, 0.38f, 0.42f), M(1.45f, 1.38f, 1.22f), M(0.42f, 1.05f, 1.05f), M(0.85f, 0.6f, 0.42f)
        };

        static readonly string[] altAd = { "siyah", "lacivert", "kot mavisi", "haki", "gri", "bej", "bordo", "koyu yeşil" };
        static readonly Color[] altDuz =
        {
            C(32, 36, 46), C(45, 62, 102), C(74, 111, 165), C(107, 91, 69), C(122, 122, 128), C(185, 169, 138), C(94, 42, 51), C(53, 82, 58)
        };
        static readonly Color[] altCarpan =
        {
            M(0.36f, 0.36f, 0.4f), M(0.45f, 0.55f, 0.95f), M(0.65f, 0.9f, 1.3f), M(0.85f, 0.75f, 0.52f),
            M(0.8f, 0.8f, 0.82f), M(1.35f, 1.2f, 0.95f), M(0.9f, 0.35f, 0.4f), M(0.45f, 0.72f, 0.5f)
        };

        static readonly string[] sacAd = { "siyah", "koyu kahve", "kahve", "kumral", "kızıl", "sarı", "platin", "gri" };
        static readonly Color[] sacDuz =
        {
            C(28, 20, 18), C(59, 36, 24), C(110, 68, 35), C(160, 112, 64), C(170, 62, 30), C(217, 174, 85), C(230, 218, 190), C(142, 142, 147)
        };
        static readonly Color[] sacCarpan =
        {
            M(0.3f, 0.28f, 0.28f), M(0.55f, 0.42f, 0.34f), M(0.85f, 0.65f, 0.48f), M(1.15f, 0.92f, 0.65f),
            M(1.25f, 0.55f, 0.35f), M(1.5f, 1.3f, 0.8f), M(1.7f, 1.6f, 1.4f), M(0.8f, 0.82f, 0.88f)
        };
        // VRoid (anime) karakterlerde canlı saç renkleri de kullanılır
        static readonly string[] sacAnimeAd = { "siyah", "kahve", "kızıl", "sarı", "gümüş", "mavi", "pembe", "mor", "yeşil", "turuncu" };
        static readonly Color[] sacAnimeCarpan =
        {
            M(0.28f, 0.27f, 0.3f), M(0.8f, 0.58f, 0.42f), M(1.3f, 0.45f, 0.35f), M(1.55f, 1.32f, 0.72f), M(1.35f, 1.42f, 1.6f),
            M(0.45f, 0.7f, 1.55f), M(1.55f, 0.72f, 1.05f), M(0.95f, 0.58f, 1.5f), M(0.5f, 1.25f, 0.8f), M(1.55f, 0.82f, 0.35f)
        };

        // Ten her zaman çarpandır (modelin kendi teni korunarak açılır / koyulaşır)
        static readonly string[] tenAd = { "açık", "buğday", "esmer", "koyu" };
        static readonly Color[] tenCarpan = { M(1.07f, 1.05f, 1.04f), M(0.9f, 0.83f, 0.78f), M(0.74f, 0.63f, 0.55f), M(0.56f, 0.45f, 0.38f) };

        // x = en, y = boy
        static readonly Vector2[] olcekler =
        {
            new Vector2(1f, 1f), new Vector2(0.96f, 0.94f), new Vector2(1f, 1.06f), new Vector2(1.06f, 1f), new Vector2(1.03f, 0.94f),
            new Vector2(0.95f, 1.05f), new Vector2(1.06f, 0.97f), new Vector2(1.05f, 1.05f), new Vector2(0.95f, 0.97f)
        };

        public static Vector2 Olcek(int i) { return olcekler[((i % olcekler.Length) + olcekler.Length) % olcekler.Length]; }

        // ---------- adlar ----------
        public static bool InsanAdi(string tabanAdi)
        {
            if (string.IsNullOrEmpty(tabanAdi)) return false;
            foreach (var o in insanOnekleri) if (tabanAdi.StartsWith(o, StringComparison.Ordinal)) return true;
            return false;
        }

        public static string Ad(string tabanAdi, int no) { return tabanAdi + "_" + no.ToString("000"); }

        // "Anne_005" -> ("anne", 5). Son parça tam üç rakam olmalı.
        public static bool AdAyir(string ad, out string normTaban, out int no)
        {
            normTaban = null; no = 0;
            string n = Util.Norm(ad);
            int i = n.LastIndexOf('_');
            if (i <= 0 || n.Length - i - 1 != 3) return false;
            int v = 0;
            for (int j = i + 1; j < n.Length; j++)
            {
                char c = n[j];
                if (c < '0' || c > '9') return false;
                v = v * 10 + (c - '0');
            }
            if (v < 1) return false;
            normTaban = n.Substring(0, i);
            no = v;
            return true;
        }

        // Önce tam ad, sonra "_kısa ad" ile biten; birden çok aday varsa ad sırasındaki ilki (her yerde aynı sonuç)
        static string TabanBul(string normTaban, IEnumerable<string> adlar)
        {
            var l = new List<string>();
            foreach (var a in adlar) if (InsanAdi(a)) l.Add(a);
            l.Sort(StringComparer.Ordinal);
            foreach (var a in l) if (Util.Norm(a) == normTaban) return a;
            foreach (var a in l) if (Util.Norm(a).EndsWith("_" + normTaban, StringComparison.Ordinal)) return a;
            return null;
        }

        // Yalnız ad listesiyle denetim (doğrulayıcılar için)
        public static bool AdGecerli(string ad, IEnumerable<string> oyuncuAdlari)
        {
            string t; int no;
            return oyuncuAdlari != null && AdAyir(ad, out t, out no) && TabanBul(t, oyuncuAdlari) != null;
        }

        // Çeşit üretilebilen temel modeller: insan, adı tekil, ad sırasında
        public static List<GameObject> Tabanlar(IEnumerable<GameObject> kaynak)
        {
            var d = new SortedDictionary<string, GameObject>(StringComparer.Ordinal);
            if (kaynak != null)
                foreach (var g in kaynak)
                    if (g != null && InsanAdi(g.name) && g.GetComponent<VaryantGorunum>() == null && !d.ContainsKey(g.name)) d[g.name] = g;
            return new List<GameObject>(d.Values);
        }

        // Katalog + derlemenin yazdığı oyuncu listesi (biri eskiyse diğeri tamamlar)
        public static List<GameObject> TumPrefablar(KarakterKatalogu katalog)
        {
            var l = new List<GameObject>();
            if (katalog != null) foreach (var k in katalog.karakterler) if (k != null && k.prefab != null) l.Add(k.prefab);
            var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
            if (ol != null) foreach (var g in ol.prefablar) if (g != null) l.Add(g);
            return l;
        }

        // ---------- çalışma anı şablonu ----------
        static readonly Dictionary<string, GameObject> sablonlar = new Dictionary<string, GameObject>();
        static GameObject depo;

        // Çeşit adını "prefab gibi kullanılabilen" bir nesneye çevirir; ad çeşit değilse null.
        public static GameObject Sablon(string ad, IEnumerable<GameObject> kaynak)
        {
            string t; int no;
            if (!AdAyir(ad, out t, out no)) return null;
            var tabanlar = Tabanlar(kaynak);
            var adlar = new List<string>();
            foreach (var g in tabanlar) adlar.Add(g.name);
            string tam = TabanBul(t, adlar);
            if (tam == null) return null;
            foreach (var g in tabanlar) if (g.name == tam) return Sablon(g, no);
            return null;
        }

        // Etkin olmayan bir nesnenin altında durur: Awake çalışmaz, boyanmaz. Instantiate edilen kopya kendini boyar.
        public static GameObject Sablon(GameObject taban, int no)
        {
            if (taban == null || no < 1) return null;
            string ad = Ad(taban.name, no);
            GameObject go;
            if (sablonlar.TryGetValue(ad, out go) && go != null) return go;
            if (depo == null)
            {
                depo = new GameObject("VaryantSablonlari");
                depo.SetActive(false);
                if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(depo);
                else depo.hideFlags = HideFlags.DontSave;
            }
            go = UnityEngine.Object.Instantiate(taban, depo.transform);
            go.name = ad;
            var v = go.AddComponent<VaryantGorunum>();
            v.taban = taban.name;
            v.no = no;
            sablonlar[ad] = go;
            return go;
        }

        // ---------- parça tanıma ----------
        public static List<VaryantYuvasi> Yuvalar(GameObject kok, string tabanAdi)
        {
            var l = new List<VaryantYuvasi>();
            if (kok == null) return l;
            var rs = kok.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rs)
            {
                if (r == null || !(r is SkinnedMeshRenderer || r is MeshRenderer)) continue;
                // kökteki renderer'ın adı örneğe göre değişir: ad kuralı uygulanmaz
                string rAd = r.transform == kok.transform ? "" : r.name;
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null) continue;
                    string mAd = ms[i].name.Replace(" (Instance)", "").Trim();
                    l.Add(new VaryantYuvasi
                    {
                        renderer = r, rendererAdi = rAd, yuva = i, malzeme = ms[i], malzemeAdi = mAd,
                        parca = Sinifla(tabanAdi, rAd, mAd, rs.Length)
                    });
                }
            }
            return l;
        }

        public static VaryantParcalari Parcalar(GameObject kok, string tabanAdi) { return Ozetle(Yuvalar(kok, tabanAdi)); }

        static VaryantParcalari Ozetle(List<VaryantYuvasi> yuvalar)
        {
            var p = new VaryantParcalari();
            foreach (var y in yuvalar)
            {
                if (y.parca == VaryantParca.Sac) p.sac = true;
                else if (y.parca == VaryantParca.Ust) p.ust = true;
                else if (y.parca == VaryantParca.Alt) p.alt = true;
                else if (y.parca == VaryantParca.Ten) p.ten = true;
                if (CantaMi(y.rendererAdi)) p.canta = true;
            }
            return p;
        }

        static bool CantaMi(string rendererAdi) { return rendererAdi.ToLowerInvariant().Contains("backpack"); }

        static bool Quaternius(string tabanAdi)
        {
            return tabanAdi.StartsWith("Erkek_", StringComparison.Ordinal) || tabanAdi.StartsWith("Kadin_", StringComparison.Ordinal);
        }

        static VaryantParca Sinifla(string taban, string rAd, string mAd, int rendererSayisi)
        {
            if (Quaternius(taban)) return SiniflaQuaternius(taban, rAd.ToLowerInvariant(), mAd.ToLowerInvariant());
            if (taban.StartsWith("VRM_", StringComparison.Ordinal)) return SiniflaVrm(mAd.ToUpperInvariant());
            return SiniflaGenel(rAd, mAd, rendererSayisi);
        }

        // Quaternius: düz renkli, adlı malzemeler; gövde bölümü renderer adından (X_Body, X_Legs, X_Head, X_Feet).
        // Saçı "Hair" adını taşımayan modeller (GLB dosyaları çizdirilerek doğrulandı):
        static readonly Dictionary<string, string[]> ozelSac = new Dictionary<string, string[]>
        {
            { "Erkek_Punk", new[] { "red", "red_dark" } },
            { "Kadin_Formal", new[] { "red" } },
            { "Kadin_Medieval", new[] { "white" } },
            { "Kadin_Punk", new[] { "pink" } },
            { "Kadin_Worker", new[] { "darkbrown" } },
        };

        static VaryantParca SiniflaQuaternius(string taban, string r, string m)
        {
            if (r.Contains("backpack")) return VaryantParca.Yok;
            if (m.StartsWith("skin", StringComparison.Ordinal)) return VaryantParca.Ten;
            if (m.StartsWith("hair", StringComparison.Ordinal) || m == "moustache" || m == "eyebrows") return VaryantParca.Sac;
            if (r.Contains("head"))
            {
                string[] s;
                if (ozelSac.TryGetValue(taban, out s) && Array.IndexOf(s, m) >= 0) return VaryantParca.Sac;
                return VaryantParca.Yok;   // göz, şapka, takı: dokunma
            }
            if (m == "gold" || m == "metal" || m == "metal_dark" || m == "earrings" || m == "visor") return VaryantParca.Yok;
            if (r.Contains("body")) return VaryantParca.Ust;
            if (r.Contains("legs") || r.Contains("pants")) return VaryantParca.Alt;
            return VaryantParca.Yok;
        }

        // VRoid: malzeme adının sonundaki tür (_SKIN, _HAIR, _CLOTH) güvenilirdir
        static VaryantParca SiniflaVrm(string m)
        {
            if (m.Contains("FACEBROW") || m.Contains("_HAIR")) return VaryantParca.Sac;
            if (m.Contains("_SKIN")) return VaryantParca.Ten;
            if (m.Contains("_CLOTH"))
            {
                if (m.Contains("TOPS") || m.Contains("ONEPIECE")) return VaryantParca.Ust;
                if (m.Contains("BOTTOMS")) return VaryantParca.Alt;
            }
            return VaryantParca.Yok;
        }

        // Mixamo ve diğerleri: yalnız açık sözcükler. "body" gibi ten + giysi karışık olabilecek adlar tanınmaz.
        static readonly string[] sDokunma = { "eye", "eyes", "eyelash", "eyelashes", "lash", "lashes", "brow", "brows", "eyebrow", "eyebrows", "teeth", "tooth", "tongue", "mouth", "cornea", "tear", "occlusion", "glass", "glasses", "lens" };
        static readonly string[] sSac = { "hair", "hairs", "beard", "moustache", "mustache", "ponytail" };
        static readonly string[] sUst = { "top", "tops", "shirt", "shirts", "tshirt", "hoodie", "hoody", "sweater", "sweatshirt", "jacket", "coat", "vest", "blouse", "cardigan", "dress", "polo" };
        static readonly string[] sAlt = { "bottom", "bottoms", "pant", "pants", "trousers", "jeans", "shorts", "skirt", "leggings" };

        static VaryantParca SiniflaGenel(string rAd, string mAd, int rendererSayisi)
        {
            var m = Sozcukler(mAd);
            // tek parça modelde renderer adı bir şey söylemez
            var r = rendererSayisi >= 2 ? Sozcukler(rAd) : new List<string>();
            if (Icerir(m, sDokunma) || Icerir(r, sDokunma)) return VaryantParca.Yok;
            if (m.Contains("skin")) return VaryantParca.Ten;
            if (Icerir(m, sSac)) return VaryantParca.Sac;
            if (Icerir(m, sUst)) return VaryantParca.Ust;
            if (Icerir(m, sAlt)) return VaryantParca.Alt;
            if (r.Contains("skin")) return VaryantParca.Ten;
            if (Icerir(r, sSac)) return VaryantParca.Sac;
            if (Icerir(r, sUst)) return VaryantParca.Ust;
            if (Icerir(r, sAlt)) return VaryantParca.Alt;
            return VaryantParca.Yok;
        }

        static bool Icerir(List<string> l, string[] aranan)
        {
            foreach (var s in l) if (Array.IndexOf(aranan, s) >= 0) return true;
            return false;
        }

        // "Ch09_Hairmat" -> ch, hair
        static List<string> Sozcukler(string s)
        {
            var l = new List<string>();
            var sb = new StringBuilder();
            char once = ' ';
            foreach (var c in s)
            {
                bool harf = char.IsLetter(c);
                if (!harf || (char.IsUpper(c) && char.IsLower(once))) SozcukEkle(l, sb);
                if (harf) sb.Append(char.ToLowerInvariant(c));
                once = c;
            }
            SozcukEkle(l, sb);
            return l;
        }

        static void SozcukEkle(List<string> l, StringBuilder sb)
        {
            if (sb.Length == 0) return;
            string s = sb.ToString();
            sb.Length = 0;
            if (s.Length > 8 && s.EndsWith("material", StringComparison.Ordinal)) s = s.Substring(0, s.Length - 8);
            else if (s.Length > 3 && s.EndsWith("mat", StringComparison.Ordinal)) s = s.Substring(0, s.Length - 3);
            l.Add(s);
        }

        // ---------- tarif ----------
        static int Karma(string s)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (var c in s) { h ^= c; h *= 16777619u; }
                return (int)(h & 0x7fffffff);
            }
        }

        static bool Anime(string tabanAdi) { return tabanAdi.StartsWith("VRM_", StringComparison.Ordinal); }

        static int[] Boyutlar(string tabanAdi, VaryantParcalari p)
        {
            return new[]
            {
                p.ust ? 1 + ustAd.Length : 1,
                p.alt ? 1 + altAd.Length : 1,
                p.sac ? 1 + (Anime(tabanAdi) ? sacAnimeAd.Length : sacAd.Length) : 1,
                p.ten ? 1 + tenAd.Length : 1,
                p.canta ? 2 : 1
            };
        }

        // Bu model için birbirinden farklı en çok kaç çeşit çıkar
        public static int Kapasite(string tabanAdi, VaryantParcalari p)
        {
            long c = 1;
            foreach (var n in Boyutlar(tabanAdi, p)) c *= n;
            return (int)Math.Min(AzamiNo, c * olcekler.Length - 1);
        }

        // Sıra numarası -> görünüm. Kapasiteye kadar her numara farklı bir birleşim verir;
        // ardışık numaralar bütün renklerde birden değişir.
        public static VaryantTarifi Tarif(string tabanAdi, VaryantParcalari p, int no)
        {
            var n = Boyutlar(tabanAdi, p);
            int toplam = 1;
            foreach (var x in n) toplam *= x;
            int kalan = no % toplam;
            var d = new int[n.Length];
            int ilk = -1;
            for (int i = 0; i < n.Length; i++)
            {
                if (n[i] <= 1) continue;
                int q = kalan % n[i];
                kalan /= n[i];
                if (ilk < 0) { ilk = q; d[i] = q; }
                else d[i] = (q + ilk) % n[i];
            }
            // her model renk tablosuna farklı yerden başlasın (0 = kendi rengi aynen kalır)
            int h = Karma(tabanAdi);
            for (int i = 0; i < 3; i++)
                if (d[i] > 0) d[i] = (d[i] - 1 + (h >> (i * 4)) % (n[i] - 1)) % (n[i] - 1) + 1;
            return new VaryantTarifi
            {
                ust = d[0], alt = d[1], sac = d[2], ten = d[3], cantaGizli = d[4] == 1,
                olcek = (no / toplam + no % toplam) % olcekler.Length
            };
        }

        public static string Aciklama(string tabanAdi, VaryantTarifi t)
        {
            var l = new List<string>();
            if (t.ust > 0) l.Add("üst " + ustAd[t.ust - 1]);
            if (t.alt > 0) l.Add("alt " + altAd[t.alt - 1]);
            if (t.sac > 0) l.Add("saç " + (Anime(tabanAdi) ? sacAnimeAd[t.sac - 1] : sacAd[t.sac - 1]));
            if (t.ten > 0) l.Add("ten " + tenAd[t.ten - 1]);
            if (t.cantaGizli) l.Add("çantasız");
            var o = Olcek(t.olcek);
            if (o.y < 0.98f) l.Add("kısa"); else if (o.y > 1.02f) l.Add("uzun");
            if (o.x < 0.98f) l.Add("ince"); else if (o.x > 1.02f) l.Add("geniş");
            return string.Join(" · ", l.ToArray());
        }

        // ---------- liste (~1000) ----------
        // Her modele sırayla birer çeşit verilir; rengi tanınmayan modeller en çok 8 (boy/en) alır,
        // gerisi rengi değişebilen modellere kalır. Nesne yaratılmaz.
        public static List<VaryantBilgi> Liste(IEnumerable<GameObject> kaynak, int hedef = HedefToplam)
        {
            var tabanlar = Tabanlar(kaynak);
            var parcalar = new VaryantParcalari[tabanlar.Count];
            var kapasite = new int[tabanlar.Count];
            var adet = new int[tabanlar.Count];
            for (int i = 0; i < tabanlar.Count; i++)
            {
                parcalar[i] = Parcalar(tabanlar[i], tabanlar[i].name);
                kapasite[i] = Kapasite(tabanlar[i].name, parcalar[i]);
            }
            int toplam = 0;
            bool ilerledi = true;
            while (toplam < hedef && ilerledi)
            {
                ilerledi = false;
                for (int i = 0; i < tabanlar.Count && toplam < hedef; i++)
                    if (adet[i] < kapasite[i]) { adet[i]++; toplam++; ilerledi = true; }
            }
            var l = new List<VaryantBilgi>(toplam);
            for (int i = 0; i < tabanlar.Count; i++)
            {
                string ad = tabanlar[i].name;
                for (int no = 1; no <= adet[i]; no++)
                {
                    var t = Tarif(ad, parcalar[i], no);
                    l.Add(new VaryantBilgi { ad = Ad(ad, no), taban = tabanlar[i], no = no, tarif = t, parcalar = parcalar[i], aciklama = Aciklama(ad, t) });
                }
            }
            return l;
        }

        // ---------- boyama ----------
        static readonly string[] renkOzellikleri = { "baseColorFactor", "_BaseColor", "_Color", "_ShadeColor" };
        static readonly string[] dokuOzellikleri = { "_BaseMap", "_MainTex", "baseColorTexture" };

        public static bool Dokulu(Material m)
        {
            foreach (var p in dokuOzellikleri) if (m.HasProperty(p) && m.GetTexture(p) != null) return true;
            return false;
        }

        static Color AnaRenk(Material m)
        {
            foreach (var p in renkOzellikleri) if (m.HasProperty(p)) return m.GetColor(p);
            return Color.white;
        }

        static float Parlaklik(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

        static uint Buyukluk(VaryantYuvasi y)
        {
            Mesh mesh = null;
            var s = y.renderer as SkinnedMeshRenderer;
            if (s != null) mesh = s.sharedMesh;
            else
            {
                var f = y.renderer.GetComponent<MeshFilter>();
                if (f != null) mesh = f.sharedMesh;
            }
            return mesh != null && y.yuva < mesh.subMeshCount ? mesh.GetIndexCount(y.yuva) : 0u;
        }

        // Örneğin malzeme KOPYALARINI boyar. Asıl malzemeler hiç değişmez; kopyalar 'kopyalar'a eklenir (silmek çağıranın işi).
        public static VaryantTarifi Boya(GameObject kok, string tabanAdi, int no, List<Material> kopyalar)
        {
            var yuvalar = Yuvalar(kok, tabanAdi);
            var t = Tarif(tabanAdi, Ozetle(yuvalar), no);

            // her parçanın en büyük malzemesi: diğerleri ona göre daha açık / koyu kalır
            var baskin = new Material[5];
            var enBuyuk = new uint[5];
            foreach (var y in yuvalar)
            {
                int p = (int)y.parca;
                uint b = Buyukluk(y);
                if (baskin[p] == null || b > enBuyuk[p]) { baskin[p] = y.malzeme; enBuyuk[p] = b; }
            }

            var onbellek = new Dictionary<Material, Material>[5];
            var degisen = new Dictionary<Renderer, Material[]>();
            foreach (var y in yuvalar)
            {
                int secim = t.Secim(y.parca);
                if (secim <= 0) continue;
                int p = (int)y.parca;
                // beyaz gömlek / şerit gibi küçük ayrıntılar beyaz kalsın
                if ((y.parca == VaryantParca.Ust || y.parca == VaryantParca.Alt) && Quaternius(tabanAdi) &&
                    y.malzeme != baskin[p] && y.malzemeAdi.StartsWith("White", StringComparison.OrdinalIgnoreCase)) continue;
                if (onbellek[p] == null) onbellek[p] = new Dictionary<Material, Material>();
                Material kopya;
                if (!onbellek[p].TryGetValue(y.malzeme, out kopya))
                {
                    kopya = new Material(y.malzeme) { name = y.malzemeAdi };
                    Renklendir(kopya, tabanAdi, y.parca, secim, baskin[p]);
                    onbellek[p][y.malzeme] = kopya;
                    kopyalar.Add(kopya);
                }
                Material[] ms;
                if (!degisen.TryGetValue(y.renderer, out ms)) degisen[y.renderer] = ms = y.renderer.sharedMaterials;
                ms[y.yuva] = kopya;
            }
            foreach (var kv in degisen) kv.Key.sharedMaterials = kv.Value;

            if (t.cantaGizli)
                foreach (var y in yuvalar) if (CantaMi(y.rendererAdi)) y.renderer.enabled = false;
            return t;
        }

        static void Renklendir(Material m, string taban, VaryantParca parca, int secim, Material baskin)
        {
            bool dokulu = Dokulu(m);
            int i = secim - 1;
            Color renk;
            bool carp = true;
            switch (parca)
            {
                case VaryantParca.Ten: renk = tenCarpan[i]; break;
                case VaryantParca.Ust: carp = dokulu; renk = dokulu ? ustCarpan[i] : ustDuz[i]; break;
                case VaryantParca.Alt: carp = dokulu; renk = dokulu ? altCarpan[i] : altDuz[i]; break;
                case VaryantParca.Sac:
                    if (Anime(taban)) renk = sacAnimeCarpan[i];
                    else { carp = dokulu; renk = dokulu ? sacCarpan[i] : sacDuz[i]; }
                    break;
                default: return;
            }
            float oran = 1f;
            if (!carp && baskin != null)
                oran = Mathf.Clamp((Parlaklik(AnaRenk(m)) + 0.02f) / (Parlaklik(AnaRenk(baskin)) + 0.02f), 0.6f, 1.4f);
            foreach (var p in renkOzellikleri)
            {
                if (!m.HasProperty(p)) continue;
                var c = m.GetColor(p);
                if (carp) m.SetColor(p, new Color(c.r * renk.r, c.g * renk.g, c.b * renk.b, c.a));
                else
                {
                    float o = p == "_ShadeColor" ? oran * 0.8f : oran;
                    m.SetColor(p, new Color(Mathf.Clamp01(renk.r * o), Mathf.Clamp01(renk.g * o), Mathf.Clamp01(renk.b * o), c.a));
                }
            }
        }
    }
}
