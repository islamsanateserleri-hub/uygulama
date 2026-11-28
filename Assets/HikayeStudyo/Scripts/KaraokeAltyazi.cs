using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HikayeStudyo
{
    // Kelime kelime altyazi (karaoke / tek kelime). Mevcut Text tabanli altyaziyi (klasik) degistirmez;
    // yanina, TikTok guvenli alaninin icinde (alt 480, sag 140, ust 220, sol 60 px bos) kendi kelime yazilarini kurar.
    //  - Kelime sureleri: klip suresi kelimelere karakter sayisina gore (noktalama sonrasi ek agirlikla) bolunur.
    //  - *vurgu* isaretli kelimeler daha buyuk ve renkli cikar.
    public class KaraokeAltyazi : MonoBehaviour
    {
        public const string Klasik = "klasik";
        public const string Karaoke = "karaoke";
        public const string TekKelime = "tek_kelime";

        // Kanvas birimi (1080x1920). Alan: x 60..940, y alttan 490..760  (alt 480 ve sag 140 bos kalir)
        const float AlanGenislik = 860f;
        const float AlanYukseklik = 270f;
        const float AlanAlt = 490f;
        const float AlanMerkezX = 500f;

        class Kelime
        {
            public string metin = "";
            public bool vurgu;
            public float agirlik = 1f;
            public float basla;
            public int sayfa;
            public Text yazi;
            public RectTransform rt;
            public float genislik, boy;
            public float olcek = 1f;
        }

        class Sayfa
        {
            public int ilk, son;
            public RectTransform kok;
            public float fit = 1f;
        }

        RectTransform kap;
        Font font, fontKalin;
        readonly List<Kelime> kelimeler = new List<Kelime>();
        readonly List<Sayfa> sayfalar = new List<Sayfa>();
        bool aktif;
        float zaman, sure, sayfaT = 1f;
        int aktifSayfa = -1, aktifKelime = -2;
        string stil = Karaoke;
        Color taban = Color.white;
        Color vurguRenk = new Color(1f, 0.86f, 0.18f);
        static readonly Color VurguluKelimeRenk = new Color(1f, 0.52f, 0.2f);

        // ---------- Kurulum ----------
        public static KaraokeAltyazi Create(RectTransform parent, Font font, Font fontKalin)
        {
            if (parent == null) return null;
            var go = new GameObject("KaraokeAltyazi", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(AlanMerkezX / 1080f, AlanAlt / 1920f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(AlanGenislik, AlanYukseklik);
            rt.anchoredPosition = Vector2.zero;
            var k = go.AddComponent<KaraokeAltyazi>();
            k.kap = rt;
            k.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            k.fontKalin = fontKalin != null ? fontKalin : k.font;
            return k;
        }

        // "karaoke", "tek kelime", "klasik" -> sabit ad. Taninmazsa "".
        public static string StilAdi(string v)
        {
            switch (Util.Norm(v))
            {
                case "karaoke": case "kelime": case "kelime_kelime": return Karaoke;
                case "tek_kelime": case "tekkelime": case "tek": case "tek_kelime_modu": return TekKelime;
                case "klasik": case "normal": case "varsayilan": case "duz": case "eski": return Klasik;
            }
            return "";
        }

        // ---------- Metin yardimcilari ----------
        // Ham metinden ses etiketlerini ((iç çek), <sigh>) atar ama *vurgu* isaretlerini korur
        public static string VurguluMetin(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            var sb = new StringBuilder(raw.Length);
            int i = 0;
            while (i < raw.Length)
            {
                char c = raw[i];
                if (c == '<')
                {
                    int j = raw.IndexOf('>', i + 1);
                    if (j > i) { i = j + 1; continue; }
                }
                if (c == '(')
                {
                    int j = raw.IndexOf(')', i + 1);
                    if (j > i && EtiketMi(raw.Substring(i + 1, j - i - 1))) { i = j + 1; continue; }
                }
                sb.Append(c);
                i++;
            }
            var t = sb.ToString().Replace("  ", " ").Replace("  ", " ").Replace(" ,", ",").Replace(" .", ".").Replace(" !", "!").Replace(" ?", "?");
            return t.Trim();
        }

        static bool EtiketMi(string inner)
        {
            var n = inner.Trim().ToLowerInvariant();
            for (int i = 0; i < SesEtiketleri.Kisayol.GetLength(0); i++)
                if (n == SesEtiketleri.Kisayol[i, 0]) return true;
            return false;
        }

        // Klasik altyazi icin: *vurgu* -> <color><b> ... </b></color> (Text zengin metni acik olmali; varsayilan acik)
        public static string KlasikMetin(string isaretli, Color vurgu)
        {
            if (string.IsNullOrEmpty(isaretli)) return "";
            if (isaretli.IndexOf('*') < 0) return isaretli;
            string hex = ColorUtility.ToHtmlStringRGB(vurgu);
            var sb = new StringBuilder(isaretli.Length + 32);
            bool acik = false;
            foreach (char c in isaretli)
            {
                if (c == '*')
                {
                    if (!acik) sb.Append("<color=#").Append(hex).Append("><b>");
                    else sb.Append("</b></color>");
                    acik = !acik;
                }
                else sb.Append(c);
            }
            if (acik) sb.Append("</b></color>");
            return sb.ToString();
        }

        static List<Kelime> Ayir(string s)
        {
            var list = new List<Kelime>();
            if (string.IsNullOrEmpty(s)) return list;
            var sb = new StringBuilder();
            bool acik = false, vurgu = false;
            for (int i = 0; i <= s.Length; i++)
            {
                char c = i < s.Length ? s[i] : ' ';
                if (c == '*') { acik = !acik; continue; }
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        list.Add(new Kelime { metin = sb.ToString(), vurgu = vurgu, agirlik = Agirlik(sb.ToString()) });
                        sb.Length = 0;
                        vurgu = false;
                    }
                    continue;
                }
                sb.Append(c);
                if (acik) vurgu = true;
            }
            return list;
        }

        // Kelime suresi ~ karakter sayisi; noktalamadan sonra duraklama payi
        static float Agirlik(string w)
        {
            float a = Mathf.Max(2f, w.Length);
            char son = w[w.Length - 1];
            for (int i = w.Length - 1; i >= 0; i--)
            {
                char ch = w[i];
                if (ch == '"' || ch == '\'' || ch == ')' || ch == '»' || ch == '”' || ch == '’') continue;
                son = ch; break;
            }
            if (son == ',' || son == ';' || son == ':') a += 2.5f;
            else if (son == '.' || son == '!' || son == '?' || son == '…') a += 4f;
            else if (son == '-' || son == '—' || son == '–') a += 1.5f;
            return a;
        }

        static bool SonNoktalama(string w)
        {
            if (string.IsNullOrEmpty(w)) return false;
            return ".!?…,;:".IndexOf(w[w.Length - 1]) >= 0;
        }

        static Color VurguRengi(Color b)
        {
            var y = new Color(1f, 0.86f, 0.18f);
            float d = Mathf.Abs(b.r - y.r) + Mathf.Abs(b.g - y.g) + Mathf.Abs(b.b - y.b);
            if (d < 0.55f) y = new Color(0.35f, 1f, 0.85f);
            return y;
        }

        // ---------- Oynatma ----------
        // isaretli: *vurgu* isaretli altyazi metni; clip: konusma (olabilir null); sureSn: satir suresi.
        // true = basladi (klasik altyaziyi bos birak), false = baslayamadi (klasige don).
        public bool Baslat(string isaretli, AudioClip clip, float sureSn, string stilAdi, Color renk)
        {
            try
            {
                return BaslatIc(isaretli, clip, sureSn, stilAdi, renk);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] Karaoke altyazı başlatılamadı: " + e.Message);
                aktif = false;
                Temizle();
                return false;
            }
        }

        bool BaslatIc(string isaretli, AudioClip clip, float sureSn, string stilAdi, Color renk)
        {
            Temizle();
            aktif = false;
            if (kap == null || font == null) return false;
            stil = stilAdi == TekKelime ? TekKelime : Karaoke;
            taban = renk;
            taban.a = 1f;
            vurguRenk = VurguRengi(taban);

            var liste = Ayir(isaretli);
            if (liste.Count == 0) return false;
            sure = Mathf.Max(0.3f, sureSn);

            // kelime sureleri
            float t0, t1;
            AgizSenkron.SesAraligi(AgizSenkron.Zarf(clip), sure, out t0, out t1);
            float toplam = 0f;
            foreach (var k in liste) toplam += k.agirlik;
            float aralik = Mathf.Max(0.2f, t1 - t0);
            float acc = 0f;
            foreach (var k in liste)
            {
                k.basla = t0 + acc / toplam * aralik;
                acc += k.agirlik;
            }
            kelimeler.AddRange(liste);

            Olustur();
            aktifSayfa = -1;
            aktifKelime = -2;
            zaman = 0f;
            sayfaT = 1f;
            aktif = true;
            Guncelle(0f);
            return true;
        }

        void Olustur()
        {
            bool tek = stil == TekKelime;
            float temel = tek ? 92f : 56f;
            float bosluk = temel * 0.30f;

            // 1) kelime yazilarini gecici kokte olustur ve olc (olcum icin canvas altinda aktif olmali)
            var geciciGo = new GameObject("Gecici", typeof(RectTransform));
            var gecici = (RectTransform)geciciGo.transform;
            gecici.SetParent(kap, false);
            for (int i = 0; i < kelimeler.Count; i++) KelimeYazisi(kelimeler[i], gecici, temel);

            // 2) sayfalara bol
            int n = kelimeler.Count;
            int a = 0;
            while (a < n)
            {
                int j = a + 1;
                if (tek)
                {
                    // 1-2 kelime: kisa iki kelime birlikte, uzun ya da vurgulu kelime tek basina
                    var w0 = kelimeler[a];
                    if (j < n)
                    {
                        var w1 = kelimeler[j];
                        if (!w0.vurgu && !w1.vurgu && w0.metin.Length <= 8 && w1.metin.Length <= 8 &&
                            w0.metin.Length + w1.metin.Length <= 13 && !SonNoktalama(w0.metin)) j = a + 2;
                    }
                }
                else
                {
                    int satir = 1;
                    float x = 0f;
                    for (j = a; j < n; j++)
                    {
                        float w = kelimeler[j].genislik;
                        if (x > 0f && x + bosluk + w > AlanGenislik)
                        {
                            if (satir >= 3) break;
                            satir++;
                            x = w;
                        }
                        else x = x > 0f ? x + bosluk + w : w;
                    }
                    if (j <= a) j = a + 1;
                }
                sayfalar.Add(new Sayfa { ilk = a, son = j - 1 });
                a = j;
            }

            // 3) her sayfayi yerlestir
            for (int si = 0; si < sayfalar.Count; si++)
            {
                var s = sayfalar[si];
                var sgo = new GameObject("Sayfa", typeof(RectTransform));
                s.kok = (RectTransform)sgo.transform;
                s.kok.SetParent(kap, false);
                s.kok.anchorMin = s.kok.anchorMax = new Vector2(0.5f, 0.5f);
                s.kok.pivot = new Vector2(0.5f, 0.5f);
                s.kok.sizeDelta = Vector2.zero;
                s.kok.anchoredPosition = Vector2.zero;

                // satirlara dagit
                var satirlar = new List<List<Kelime>>();
                var gen = new List<float>();
                var yuk = new List<float>();
                var cur = new List<Kelime>();
                float cw = 0f, ch = 0f;
                for (int i = s.ilk; i <= s.son; i++)
                {
                    var k = kelimeler[i];
                    k.sayfa = si;
                    if (cur.Count > 0 && cw + bosluk + k.genislik > AlanGenislik)
                    {
                        satirlar.Add(cur); gen.Add(cw); yuk.Add(ch);
                        cur = new List<Kelime>(); cw = 0f; ch = 0f;
                    }
                    cw = cur.Count > 0 ? cw + bosluk + k.genislik : k.genislik;
                    ch = Mathf.Max(ch, k.boy);
                    cur.Add(k);
                }
                if (cur.Count > 0) { satirlar.Add(cur); gen.Add(cw); yuk.Add(ch); }

                float toplamY = 0f, maxW = 0f;
                for (int l = 0; l < yuk.Count; l++) { toplamY += yuk[l]; maxW = Mathf.Max(maxW, gen[l]); }
                float y = toplamY / 2f;
                for (int l = 0; l < satirlar.Count; l++)
                {
                    float x = -gen[l] / 2f;
                    for (int q = 0; q < satirlar[l].Count; q++)
                    {
                        var k = satirlar[l][q];
                        k.rt.SetParent(s.kok, false);
                        k.rt.anchoredPosition = new Vector2(x + k.genislik / 2f, y - yuk[l] / 2f);
                        x += k.genislik + bosluk;
                    }
                    y -= yuk[l];
                }
                s.fit = Mathf.Min(1f, Mathf.Min(AlanGenislik / Mathf.Max(1f, maxW), AlanYukseklik / Mathf.Max(1f, toplamY)));
                s.kok.localScale = new Vector3(s.fit, s.fit, 1f);
            }

            Destroy(geciciGo);
            for (int si = 0; si < sayfalar.Count; si++) sayfalar[si].kok.gameObject.SetActive(false);
        }

        void KelimeYazisi(Kelime k, Transform ebeveyn, float temel)
        {
            var go = new GameObject("K", typeof(RectTransform));
            go.transform.SetParent(ebeveyn, false);
            int boyut = Mathf.RoundToInt(k.vurgu ? temel * 1.28f : temel);
            var t = go.AddComponent<Text>();
            t.font = stil == TekKelime ? fontKalin : font;
            t.fontSize = boyut;
            t.fontStyle = k.vurgu ? FontStyle.BoldAndItalic : FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.supportRichText = false;
            t.text = k.metin;
            t.color = k.vurgu ? VurguluKelimeRenk : taban;
            float kenar = Mathf.Max(3f, boyut * 0.075f);
            var o = go.AddComponent<Outline>();
            o.effectColor = Color.black;
            o.effectDistance = new Vector2(kenar, -kenar);
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.55f);
            sh.effectDistance = new Vector2(kenar * 0.8f, -kenar * 1.4f);

            float w = t.preferredWidth;
            if (float.IsNaN(w) || w < 1f) w = k.metin.Length * boyut * 0.55f;
            k.genislik = w + 4f;
            k.boy = boyut * 1.2f;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(k.genislik, k.boy);
            k.yazi = t;
            k.rt = rt;
            k.olcek = 1f;
        }

        public void Durdur()
        {
            aktif = false;
            Temizle();
        }

        void Temizle()
        {
            for (int i = 0; i < sayfalar.Count; i++)
            {
                if (sayfalar[i].kok != null)
                {
                    sayfalar[i].kok.gameObject.SetActive(false);
                    Destroy(sayfalar[i].kok.gameObject);
                }
            }
            sayfalar.Clear();
            kelimeler.Clear();
            aktifSayfa = -1;
            aktifKelime = -2;
            if (kap != null)
            {
                for (int i = kap.childCount - 1; i >= 0; i--)
                {
                    var c = kap.GetChild(i);
                    if (c != null && c.name == "Gecici") { c.gameObject.SetActive(false); Destroy(c.gameObject); }
                }
            }
        }

        void Update()
        {
            if (!aktif) return;
            Guncelle(Time.deltaTime);
        }

        void Guncelle(float dt)
        {
            if (sayfalar.Count == 0 || kelimeler.Count == 0) { aktif = false; return; }
            zaman += dt;
            if (zaman >= sure) { Durdur(); return; }

            int c = -1;
            for (int i = 0; i < kelimeler.Count; i++)
            {
                if (zaman >= kelimeler[i].basla) c = i; else break;
            }
            int sn = c >= 0 ? kelimeler[c].sayfa : 0;
            if (sn < 0 || sn >= sayfalar.Count) sn = 0;

            if (sn != aktifSayfa)
            {
                if (aktifSayfa >= 0 && aktifSayfa < sayfalar.Count) sayfalar[aktifSayfa].kok.gameObject.SetActive(false);
                aktifSayfa = sn;
                var ns = sayfalar[sn];
                ns.kok.gameObject.SetActive(true);
                sayfaT = 0f;
                for (int i = ns.ilk; i <= ns.son; i++)
                {
                    var w = kelimeler[i];
                    w.olcek = 1f;
                    w.rt.localScale = Vector3.one;
                    w.yazi.color = w.vurgu ? VurguluKelimeRenk : taban;
                }
            }
            if (c != aktifKelime)
            {
                aktifKelime = c;
                if (c >= 0) kelimeler[c].olcek = 1.32f;   // "pop": kisa tasma, sonra yerine oturur
            }

            var sy = sayfalar[sn];
            float k = 1f - Mathf.Exp(-dt * 20f);
            Color gecmis = new Color(taban.r * 0.82f, taban.g * 0.82f, taban.b * 0.82f, 1f);
            for (int i = sy.ilk; i <= sy.son; i++)
            {
                var w = kelimeler[i];
                bool simdi = i == c;
                w.olcek = Mathf.Lerp(w.olcek, simdi ? 1.14f : 1f, k);
                w.rt.localScale = new Vector3(w.olcek, w.olcek, 1f);
                Color hedef = simdi ? vurguRenk : (w.vurgu ? VurguluKelimeRenk : (i < c ? gecmis : taban));
                w.yazi.color = Color.Lerp(w.yazi.color, hedef, k);
            }

            sayfaT += dt;
            float pk = Mathf.Clamp01(sayfaT / 0.1f);
            float ps = sy.fit * Mathf.Lerp(0.94f, 1f, pk);
            sy.kok.localScale = new Vector3(ps, ps, 1f);
        }
    }
}
