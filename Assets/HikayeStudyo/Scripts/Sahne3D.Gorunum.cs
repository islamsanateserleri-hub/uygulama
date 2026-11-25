using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 'gorunum: Ad tulum soguk' : 3D karakterin renklerini degistirir. Yalniz o karakterin malzeme kopyalari degisir.
    //   tulum  = kiyafetler koyu celik grisi
    //   soguk  = buz yanigi gibi grimsi-mor
    //   golge  = simsiyah siluet
    //   gizli  = aninda gorunmez (isiklar sonukken kaybolma icin)
    //   normal = eski haline doner
    public partial class Sahne3D
    {
        public static readonly string[] Gorunumler = { "tulum", "soguk", "golge", "normal", "gizli" };

        readonly Dictionary<string, string> gorunumler = new Dictionary<string, string>();
        struct RenkKaydi { public Material m; public string ozellik; public Color asil; }
        readonly Dictionary<Karakter3D, List<RenkKaydi>> asilRenkler = new Dictionary<Karakter3D, List<RenkKaydi>>();
        static readonly string[] RenkOzellikleri = { "_Color", "_BaseColor", "_ShadeColor", "_EmissionColor", "_RimColor", "_OutlineColor", "_MatcapColor" };

        public static string GorunumDogrula(string v)
        {
            var p = (v ?? "").Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (p.Length < 2) return "'gorunum: Karakter " + string.Join("|", Gorunumler) + "' biçiminde yaz.";
            for (int i = 1; i < p.Length; i++)
                if (!Util.In(p[i], Gorunumler)) return "'gorunum: " + p[i] + "' bilinmiyor (" + string.Join(", ", Gorunumler) + ").";
            return null;
        }

        public void Gorunum(string id, string ne)
        {
            var k = Util.Norm(id);
            gorunumler[k] = ne ?? "";
            Karakter3D a;
            if (actors.TryGetValue(k, out a) && a != null) GorunumUygula(k, a);
        }

        // Ensure() yeni oyuncu yarattiginda da cagirir
        void GorunumUygula(string k, Karakter3D a)
        {
            string ne;
            if (!gorunumler.TryGetValue(k, out ne) || a == null) return;
            List<RenkKaydi> kayit;
            if (!asilRenkler.TryGetValue(a, out kayit))
            {
                kayit = new List<RenkKaydi>();
                foreach (var r in a.GetComponentsInChildren<Renderer>(true))
                    foreach (var m in r.materials)
                    {
                        if (m == null) continue;
                        foreach (var oz in RenkOzellikleri)
                            if (m.HasProperty(oz)) kayit.Add(new RenkKaydi { m = m, ozellik = oz, asil = m.GetColor(oz) });
                    }
                asilRenkler[a] = kayit;
                if (Application.isEditor)
                {
                    var adlar = new HashSet<string>();
                    foreach (var x in kayit) adlar.Add(x.m.name);
                    Debug.Log("[Gorunum] " + k + " malzemeler: " + string.Join("; ", new List<string>(adlar).ToArray()));
                }
            }
            bool tulum = false, soguk = false, golge = false, gizli = false;
            foreach (var p in ne.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var n = Util.Norm(p);
                if (n == "tulum") tulum = true;
                else if (n == "soguk") soguk = true;
                else if (n == "golge") golge = true;
                else if (n == "gizli") gizli = true;
            }
            foreach (var r in a.GetComponentsInChildren<Renderer>(true)) r.enabled = !gizli;
            foreach (var x in kayit)
            {
                if (x.m == null) continue;
                Color c = x.asil;
                string ad = x.m.name.ToUpperInvariant();
                bool kiyafet = ad.Contains("CLOTH") || ad.Contains("TOPS") || ad.Contains("BOTTOMS") || ad.Contains("SHOES");
                // tulum: kiyafet koyu celik grisi; govde derisi (kol, bacak, boyun) de ayni renge boyanir ki
                // kisa kollu tisort/sort kapali bir tulum gibi gorunsun. Yuz derisine dokunulmaz.
                bool govdeDerisi = ad.Contains("SKIN") && ad.Contains("BODY") && !ad.Contains("FACE");
                bool sac = ad.Contains("HAIR");
                if (tulum && (kiyafet || govdeDerisi))
                {
                    // dokudaki renk (mavi tisort, kirmizi ayakkabi) kalmasin: duz celik grisi
                    if (x.m.HasProperty("_MainTex")) x.m.SetTexture("_MainTex", Texture2D.whiteTexture);
                    if (x.m.HasProperty("_ShadeTexture")) x.m.SetTexture("_ShadeTexture", Texture2D.whiteTexture);
                    if (x.ozellik == "_Color" || x.ozellik == "_BaseColor") c = new Color(0.16f, 0.18f, 0.22f, c.a);
                    else if (x.ozellik == "_ShadeColor") c = new Color(0.09f, 0.1f, 0.13f, c.a);
                }
                if (tulum && sac) c = Carp(c, 0.6f, 0.62f, 0.68f);
                if (soguk) c = Carp(c, 0.62f, 0.6f, 0.86f);
                if (golge) c = Carp(c, 0.025f, 0.025f, 0.04f);
                x.m.SetColor(x.ozellik, c);
            }
        }

        static Color Carp(Color c, float r, float g, float b) { return new Color(c.r * r, c.g * g, c.b * b, c.a); }
    }
}
