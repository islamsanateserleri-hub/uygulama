using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 2: Unity Editör'den bağımsız uygulama ana ekranı (IMGUI, kodla çizilir).
    // Bölümler: Yeni hikâye, Hikâyelerim, Karakterlerim, Mekânlar, Ses ayarları, Video ayarları, Projeler, Ayarlar.
    // Yalnızca derlenmiş uygulamada (EXE) ve "-kayit/-hikaye" argümanı yokken açılır.
    public class AnaEkran : MonoBehaviour
    {
        enum Bolum { Ana, YeniHikaye, Hikayelerim, Karakterlerim, Mekanlar, Ses, Video, Projeler, Ayarlar }

        static readonly string[] BolumAdi =
        {
            "Ana ekran", "Yeni hikâye", "Hikâyelerim", "Karakterlerim", "Mekânlar", "Ses ayarları", "Video ayarları", "Projeler", "Ayarlar"
        };
        static readonly string[] BolumAciklama =
        {
            "", "Boş bir hikâye başlat", "Yazdıkların ve örnek hikâyeler", "3D ve çizgi karakterler", "Hazır 3D mekânlar ve arka planlar",
            "Ses seviyeleri", "1080x1920 dikey video, kare hızı", "Kaydedilen videolar", "Klasörler, ffmpeg, hakkında"
        };
        static readonly Color32[] BolumRenk =
        {
            new Color32(90, 140, 255, 255), new Color32(90, 140, 255, 255), new Color32(120, 120, 255, 255), new Color32(190, 110, 240, 255),
            new Color32(240, 120, 170, 255), new Color32(250, 150, 90, 255), new Color32(240, 190, 80, 255), new Color32(90, 200, 150, 255), new Color32(120, 200, 220, 255)
        };

        // ---- klasörler ----
        public static string Kok { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HikayeStudyosu"); } }
        public static string HikayeKlasoru { get { return Path.Combine(Kok, "Hikayeler"); } }
        public static string VideoKlasoru { get { return Path.Combine(Kok, "Videolar"); } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Basla()
        {
#if !UNITY_EDITOR
            foreach (var a in Environment.GetCommandLineArgs())
                if (a == "-hikaye" || a == "-kayit") return;
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "ffkayit.txt"))) return;
            var go = new GameObject("AnaEkran");
            DontDestroyOnLoad(go);
            go.AddComponent<AnaEkran>();
#endif
        }

        // ---- durum ----
        class HikayeBilgi { public string ad, metin, yol; public bool yerlesik; public int konusma = -1; public TextAsset asset; }
        class VideoBilgi { public string ad, yol; public long boyut; public DateTime zaman; }

        Bolum bolum = Bolum.Ana;
        float kaydirma;  // liste kaydırma (kendi kaydırma çubuğumuz; GUI.BeginScrollView ölçekli matrisle tıklamayı bozuyordu)
        string mesaj = ""; bool mesajHata;
        bool oynuyor, bitti;
        string yeniAd = "";
        string sonOlusturulan;
        string karakterArama = "";
        int karakterSekme;        // 0: 3D, 1: çizgi
        List<HikayeBilgi> hikayeler;
        List<VideoBilgi> videolar;
        KarakterKatalogu katalog; StoryLibrary kutuphane;
        string kodlayiciSonuc = "";
        volatile bool kodlayiciBekle;
        bool sifirlaOnay;

        // ---- stil önbelleği ----
        Font font;
        readonly Dictionary<string, GUIStyle> stiller = new Dictionary<string, GUIStyle>();
        readonly Dictionary<string, Texture2D> dokular = new Dictionary<string, Texture2D>();

        static readonly Color Zemin = new Color32(17, 20, 28, 255);
        static readonly Color Panel = new Color32(24, 28, 40, 255);
        static readonly Color Kart = new Color32(34, 40, 57, 255);
        static readonly Color Vurgu = new Color32(90, 140, 255, 255);
        static readonly Color Yazi = new Color32(236, 240, 248, 255);
        static readonly Color Sönük = new Color32(150, 160, 182, 255);
        static readonly Color Tehlike = new Color32(230, 90, 90, 255);
        static readonly Color Basari = new Color32(90, 200, 150, 255);

        float olcek = 1f, gw = 1280, gh = 720;

        void Awake()
        {
            try { Directory.CreateDirectory(HikayeKlasoru); Directory.CreateDirectory(VideoKlasoru); } catch (Exception e) { Debug.LogWarning("[AnaEkran] klasör: " + e.Message); }
            try { font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Liberation Sans" }, 32); } catch { font = null; }
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            katalog = KarakterKatalogu.Yukle();
            kutuphane = Resources.Load<StoryLibrary>("StoryLibrary");
            AudioListener.volume = PlayerPrefs.GetFloat("HS_SesGenel", 1f);
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            HikayeleriYukle(); VideolariYukle();
        }

        // ---------- veri ----------
        void HikayeleriYukle()
        {
            hikayeler = new List<HikayeBilgi>();
            try
            {
                foreach (var f in Directory.GetFiles(HikayeKlasoru, "*.txt"))
                    hikayeler.Add(new HikayeBilgi { ad = Path.GetFileNameWithoutExtension(f), yol = f, metin = File.ReadAllText(f, Encoding.UTF8), yerlesik = false });
            }
            catch (Exception e) { Debug.LogWarning("[AnaEkran] hikâyeler: " + e.Message); }
            hikayeler.Sort((a, b) => string.Compare(a.ad, b.ad, StringComparison.OrdinalIgnoreCase));
            var yerlesik = new List<HikayeBilgi>();
            foreach (var t in Resources.LoadAll<TextAsset>("Hikayeler"))
                yerlesik.Add(new HikayeBilgi { ad = t.name, metin = t.text, asset = t, yerlesik = true });
            yerlesik.Sort((a, b) => string.Compare(a.ad, b.ad, StringComparison.OrdinalIgnoreCase));
            hikayeler.AddRange(yerlesik);
            foreach (var h in hikayeler)
            {
                try { h.konusma = StoryParser.Parse(h.metin).DialogueCount; } catch { h.konusma = -1; }
            }
        }

        void VideolariYukle()
        {
            videolar = new List<VideoBilgi>();
            try
            {
                foreach (var f in Directory.GetFiles(VideoKlasoru, "*.mp4"))
                {
                    var fi = new FileInfo(f);
                    videolar.Add(new VideoBilgi { ad = fi.Name, yol = f, boyut = fi.Length, zaman = fi.LastWriteTime });
                }
            }
            catch (Exception e) { Debug.LogWarning("[AnaEkran] videolar: " + e.Message); }
            videolar.Sort((a, b) => b.zaman.CompareTo(a.zaman));
        }

        // ---------- çizim yardımcıları ----------
        Texture2D Yuvarlak(Color c)
        {
            string key = ((Color32)c).r + "," + ((Color32)c).g + "," + ((Color32)c).b + "," + ((Color32)c).a;
            Texture2D t;
            if (dokular.TryGetValue(key, out t) && t != null) return t;
            const int n = 32; const float r = 11f;
            t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Max(r - (x + 0.5f), (x + 0.5f) - (n - r), 0f);
                    float dy = Mathf.Max(r - (y + 0.5f), (y + 0.5f) - (n - r), 0f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float a = Mathf.Clamp01(0.5f - d);
                    px[y * n + x] = new Color(c.r, c.g, c.b, c.a * a);
                }
            t.SetPixels(px); t.Apply(false, false);
            dokular[key] = t;
            return t;
        }

        GUIStyle KutuStil(Color c)
        {
            string key = "kutu" + c.ToString();
            GUIStyle s;
            if (stiller.TryGetValue(key, out s)) return s;
            s = new GUIStyle { border = new RectOffset(12, 12, 12, 12) };
            s.normal.background = Yuvarlak(c);
            stiller[key] = s;
            return s;
        }

        GUIStyle YaziStil(int boyut, TextAnchor hiza, Color renk, bool kalin, bool sar)
        {
            string key = "yazi" + boyut + hiza + renk + kalin + sar;
            GUIStyle s;
            if (stiller.TryGetValue(key, out s)) return s;
            s = new GUIStyle
            {
                font = font, fontSize = boyut, alignment = hiza, wordWrap = sar, clipping = TextClipping.Clip,
                fontStyle = kalin ? FontStyle.Bold : FontStyle.Normal, richText = false
            };
            s.normal.textColor = renk;
            stiller[key] = s;
            return s;
        }

        void Kutu(Rect r, Color c) { GUI.Box(r, GUIContent.none, KutuStil(c)); }
        void Yazdir(Rect r, string t, int boyut, Color renk, TextAnchor hiza = TextAnchor.MiddleLeft, bool kalin = false, bool sar = false)
        { GUI.Label(r, t, YaziStil(boyut, hiza, renk, kalin, sar)); }

        bool Dugme(Rect r, string t, Color c, int boyut = 16, bool etkin = true)
        {
            bool ustte = etkin && r.Contains(Event.current.mousePosition);
            var cc = etkin ? c : new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, 0.6f);
            if (ustte) cc = Color.Lerp(cc, Color.white, 0.16f);
            Kutu(r, cc);
            Yazdir(r, t, boyut, etkin ? Color.white : Sönük, TextAnchor.MiddleCenter, true);
            return etkin && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        float Kaydirgac(Rect r, float v)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            var iz = new Rect(r.x, r.y + r.height * 0.5f - 4, r.width, 8);
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition)) { GUIUtility.hotControl = id; v = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width); e.Use(); }
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id) { v = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width); e.Use(); }
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == id) { GUIUtility.hotControl = 0; e.Use(); }
            Kutu(iz, new Color32(52, 60, 84, 255));
            if (v > 0.01f) Kutu(new Rect(iz.x, iz.y, Mathf.Max(8, iz.width * v), iz.height), Vurgu);
            Kutu(new Rect(r.x + r.width * v - 10, r.y + r.height * 0.5f - 10, 20, 20), Color.white);
            return v;
        }

        bool Anahtar(Rect r, bool v)
        {
            Kutu(r, v ? Vurgu : new Color32(52, 60, 84, 255));
            float k = r.height - 6;
            Kutu(new Rect(v ? r.xMax - k - 3 : r.x + 3, r.y + 3, k, k), Color.white);
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) v = !v;
            return v;
        }


        // ---- kendi kaydırma alanımız ----
        float listeIcerik;
        void ListeBasla(Rect gorunum, float icerikYuksekligi)
        {
            listeIcerik = icerikYuksekligi;
            var e = Event.current;
            float azami = Mathf.Max(0f, icerikYuksekligi - gorunum.height);
            if (e.type == EventType.ScrollWheel && gorunum.Contains(e.mousePosition)) { kaydirma += e.delta.y * 30f; e.Use(); }
            kaydirma = Mathf.Clamp(kaydirma, 0f, azami);
            GUI.BeginGroup(gorunum);
            GUI.BeginGroup(new Rect(0, -kaydirma, gorunum.width, Mathf.Max(icerikYuksekligi, gorunum.height)));
        }

        void ListeBitir(Rect gorunum)
        {
            GUI.EndGroup();
            GUI.EndGroup();
            float azami = Mathf.Max(0f, listeIcerik - gorunum.height);
            if (azami <= 0f) return;
            var iz = new Rect(gorunum.xMax - 10, gorunum.y, 8, gorunum.height);
            float oran = gorunum.height / listeIcerik;
            float tutH = Mathf.Max(30f, iz.height * oran);
            float tutY = iz.y + (iz.height - tutH) * (kaydirma / azami);
            Kutu(iz, new Color32(30, 36, 52, 255));
            Kutu(new Rect(iz.x, tutY, iz.width, tutH), new Color32(90, 100, 130, 255));
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            var alan = new Rect(iz.x - 6, iz.y, iz.width + 12, iz.height);
            if (e.type == EventType.MouseDown && alan.Contains(e.mousePosition)) { GUIUtility.hotControl = id; e.Use(); }
            if (GUIUtility.hotControl == id && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
            {
                float t = Mathf.Clamp01((e.mousePosition.y - iz.y - tutH * 0.5f) / Mathf.Max(1f, iz.height - tutH));
                kaydirma = t * azami; e.Use();
            }
            if (e.type == EventType.MouseUp && GUIUtility.hotControl == id) { GUIUtility.hotControl = 0; e.Use(); }
        }

        // ---------- ana çizim ----------
        void OnGUI()
        {
            olcek = Mathf.Max(0.6f, Mathf.Min(Screen.width / 1280f, Screen.height / 720f));
            gw = Screen.width / olcek; gh = Screen.height / olcek;
            GUI.matrix = Matrix4x4.Scale(new Vector3(olcek, olcek, 1f));
            GUI.depth = -1000;

            if (oynuyor) { OynarkenCiz(); return; }

            Kutu(new Rect(-4, -4, gw + 8, gh + 8), Zemin);
            KenarCubugu();
            var icerik = new Rect(244, 20, gw - 244 - 24, gh - 40);
            Kutu(icerik, Panel);
            var ic = new Rect(icerik.x + 24, icerik.y + 20, icerik.width - 48, icerik.height - 40);

            switch (bolum)
            {
                case Bolum.Ana: AnaCiz(ic); break;
                case Bolum.YeniHikaye: YeniHikayeCiz(ic); break;
                case Bolum.Hikayelerim: HikayelerimCiz(ic); break;
                case Bolum.Karakterlerim: KarakterlerimCiz(ic); break;
                case Bolum.Mekanlar: MekanlarCiz(ic); break;
                case Bolum.Ses: SesCiz(ic); break;
                case Bolum.Video: VideoCiz(ic); break;
                case Bolum.Projeler: ProjelerCiz(ic); break;
                case Bolum.Ayarlar: AyarlarCiz(ic); break;
            }
        }

        void Git(Bolum b)
        {
            bolum = b; kaydirma = 0f; mesaj = ""; sifirlaOnay = false;
            if (b == Bolum.Hikayelerim) HikayeleriYukle();
            if (b == Bolum.Projeler) VideolariYukle();
        }

        void KenarCubugu()
        {
            Kutu(new Rect(12, 20, 220, gh - 40), Panel);
            Yazdir(new Rect(30, 34, 190, 30), "Hikâye Stüdyosu", 20, Yazi, TextAnchor.MiddleLeft, true);
            Yazdir(new Rect(30, 62, 190, 20), "TikTok video aracı", 12, Sönük);
            float y = 100;
            for (int i = 0; i < BolumAdi.Length; i++)
            {
                var b = (Bolum)i;
                var r = new Rect(24, y, 196, 42);
                bool secili = bolum == b;
                bool ustte = r.Contains(Event.current.mousePosition);
                if (secili) Kutu(r, new Color32(46, 60, 100, 255));
                else if (ustte) Kutu(r, new Color32(34, 40, 57, 255));
                if (secili) Kutu(new Rect(r.x, r.y + 8, 4, r.height - 16), BolumRenk[i]);
                Yazdir(new Rect(r.x + 18, r.y, r.width - 20, r.height), BolumAdi[i], 16, secili ? Color.white : Yazi, TextAnchor.MiddleLeft, secili);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) Git(b);
                y += i == 0 ? 52 : 46;
            }
            if (Dugme(new Rect(24, gh - 84, 196, 38), "Çıkış", new Color32(60, 66, 88, 255), 15)) Application.Quit();
        }

        void Baslik(Rect ic, string baslik, string alt)
        {
            Yazdir(new Rect(ic.x, ic.y, ic.width, 40), baslik, 28, Yazi, TextAnchor.MiddleLeft, true);
            if (!string.IsNullOrEmpty(alt)) Yazdir(new Rect(ic.x, ic.y + 40, ic.width, 24), alt, 14, Sönük);
        }

        void MesajCiz(Rect r)
        {
            if (string.IsNullOrEmpty(mesaj)) return;
            Kutu(r, mesajHata ? new Color(Tehlike.r * 0.35f, Tehlike.g * 0.25f, Tehlike.b * 0.25f, 1f) : new Color32(30, 70, 60, 255));
            Yazdir(new Rect(r.x + 14, r.y, r.width - 28, r.height), mesaj, 14, mesajHata ? new Color32(255, 170, 170, 255) : new Color32(170, 240, 210, 255), TextAnchor.MiddleLeft, false, true);
        }

        // ---------- bölümler ----------
        void AnaCiz(Rect ic)
        {
            Yazdir(new Rect(ic.x, ic.y + 4, ic.width, 44), "Hoş geldin", 34, Yazi, TextAnchor.MiddleLeft, true);
            Yazdir(new Rect(ic.x, ic.y + 50, ic.width, 26), "Hikâyeni yaz, karakterlerini seç, 1080x1920 TikTok videosuna çevir.", 16, Sönük);
            float top = ic.y + 100, gap = 16, w = (ic.width - gap * 3) / 4f, h = 150;
            for (int i = 1; i < BolumAdi.Length; i++)
            {
                int k = i - 1, sat = k / 4, sut = k % 4;
                var r = new Rect(ic.x + sut * (w + gap), top + sat * (h + gap), w, h);
                bool ustte = r.Contains(Event.current.mousePosition);
                Kutu(r, ustte ? new Color32(44, 52, 74, 255) : Kart);
                Kutu(new Rect(r.x + 16, r.y + 18, 36, 6), BolumRenk[i]);
                Yazdir(new Rect(r.x + 16, r.y + 36, r.width - 32, 30), BolumAdi[i], 20, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(r.x + 16, r.y + 68, r.width - 32, 60), BolumAciklama[i], 13, Sönük, TextAnchor.UpperLeft, false, true);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) Git((Bolum)i);
            }
            float sy = top + 2 * (h + gap) + 6;
            Yazdir(new Rect(ic.x, sy, ic.width, 24),
                "Hikâyelerin: " + HikayeKlasoru + "      Videolar: " + VideoKlasoru, 12, Sönük);
        }

        static string Sadelestir(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.Trim())
            {
                switch (c)
                {
                    case 'ı': case 'İ': sb.Append('i'); break;
                    case 'ğ': case 'Ğ': sb.Append('g'); break;
                    case 'ü': case 'Ü': sb.Append('u'); break;
                    case 'ş': case 'Ş': sb.Append('s'); break;
                    case 'ö': case 'Ö': sb.Append('o'); break;
                    case 'ç': case 'Ç': sb.Append('c'); break;
                    case ' ': sb.Append('_'); break;
                    default:
                        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-') sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        void YeniHikayeCiz(Rect ic)
        {
            Baslik(ic, "Yeni hikâye", "Hikâyene bir ad ver, ben boş bir şablon hazırlayayım.");
            float y = ic.y + 90;
            Yazdir(new Rect(ic.x, y, 300, 24), "Hikâye adı", 14, Sönük);
            var alan = new Rect(ic.x, y + 26, 520, 44);
            Kutu(alan, Kart);
            var ts = YaziStil(18, TextAnchor.MiddleLeft, Yazi, false, false);
            var tf = new GUIStyle(ts) { padding = new RectOffset(14, 14, 0, 0), clipping = TextClipping.Clip };
            tf.focused.textColor = Yazi; tf.hover.textColor = Yazi; tf.active.textColor = Yazi;
            yeniAd = GUI.TextField(alan, yeniAd, 40, tf);
            string dosyaAdi = Sadelestir(yeniAd);
            Yazdir(new Rect(ic.x, y + 76, 700, 22), dosyaAdi.Length == 0 ? "Dosya adı: (bir ad yaz)" : "Dosya adı: " + dosyaAdi + ".txt", 13, Sönük);
            if (Dugme(new Rect(ic.x, y + 112, 200, 46), "Oluştur", Vurgu, 17, dosyaAdi.Length > 0)) YeniHikayeOlustur(dosyaAdi);
            MesajCiz(new Rect(ic.x, y + 176, Mathf.Min(ic.width, 760), 56));
            if (!string.IsNullOrEmpty(sonOlusturulan))
            {
                float by = y + 250;
                if (Dugme(new Rect(ic.x, by, 240, 42), "Not Defteri'nde aç", new Color32(60, 66, 88, 255), 15)) Ac("notepad.exe", "\"" + sonOlusturulan + "\"");
                if (Dugme(new Rect(ic.x + 256, by, 220, 42), "Hikâyelerim'e git", new Color32(60, 66, 88, 255), 15)) Git(Bolum.Hikayelerim);
            }
            Yazdir(new Rect(ic.x, ic.yMax - 60, ic.width, 50),
                "Hikâyeni yazma ekranı bir sonraki aşamada (Aşama 3) gelecek; şimdilik oluşan dosyayı Not Defteri ile düzenleyebilirsin.", 13, Sönük, TextAnchor.UpperLeft, false, true);
        }

        void YeniHikayeOlustur(string ad)
        {
            try
            {
                Directory.CreateDirectory(HikayeKlasoru);
                string yol = Path.Combine(HikayeKlasoru, ad + ".txt");
                if (File.Exists(yol)) { mesaj = "Bu adda bir hikâye zaten var. Başka bir ad dene."; mesajHata = true; return; }
                string sablon = "// " + ad + "\r\nkanca: Buraya ilk cümleni yaz.\r\n[Anlatici|normal|dis] Buraya anlatıcı metnini yaz.\r\nson: sert\r\n";
                File.WriteAllText(yol, sablon, new UTF8Encoding(false));
                sonOlusturulan = yol; yeniAd = "";
                mesaj = "Hikâye oluşturuldu: " + yol; mesajHata = false;
                HikayeleriYukle();
            }
            catch (Exception e) { mesaj = "Hikâye oluşturulamadı: " + e.Message; mesajHata = true; }
        }

        void Ac(string dosya, string arg)
        {
            try { Process.Start(new ProcessStartInfo(dosya, arg) { UseShellExecute = true }); }
            catch (Exception e) { mesaj = "Açılamadı: " + e.Message; mesajHata = true; }
        }

        void HikayelerimCiz(Rect ic)
        {
            Baslik(ic, "Hikâyelerim", hikayeler.Count + " hikâye (örnekler ve senin yazdıkların)");
            if (Dugme(new Rect(ic.xMax - 330, ic.y + 4, 150, 38), "Yenile", new Color32(60, 66, 88, 255), 14)) HikayeleriYukle();
            if (Dugme(new Rect(ic.xMax - 170, ic.y + 4, 170, 38), "Klasörü aç", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "\"" + HikayeKlasoru + "\"");
            var gorunum = new Rect(ic.x, ic.y + 80, ic.width, ic.height - 130);
            const float sat = 64;
            ListeBasla(gorunum, Mathf.Max(gorunum.height, hikayeler.Count * (sat + 8)));
            for (int i = 0; i < hikayeler.Count; i++)
            {
                var h = hikayeler[i];
                var r = new Rect(0, i * (sat + 8), gorunum.width - 18, sat);
                Kutu(r, Kart);
                Yazdir(new Rect(r.x + 18, r.y + 8, r.width - 360, 28), h.ad, 18, Yazi, TextAnchor.MiddleLeft, true);
                string alt = (h.yerlesik ? "Örnek hikâye" : "Benim hikâyem") + (h.konusma >= 0 ? "  ·  " + h.konusma + " konuşma" : "");
                Yazdir(new Rect(r.x + 18, r.y + 34, r.width - 360, 22), alt, 13, Sönük);
                if (!h.yerlesik && Dugme(new Rect(r.xMax - 330, r.y + 12, 120, 40), "Düzenle", new Color32(60, 66, 88, 255), 14)) Ac("notepad.exe", "\"" + h.yol + "\"");
                if (Dugme(new Rect(r.xMax - 196, r.y + 12, 180, 40), "Oyna", Vurgu, 16)) Oyna(h);
            }
            ListeBitir(gorunum);
            MesajCiz(new Rect(ic.x, ic.yMax - 44, ic.width, 40));
        }

        void KarakterlerimCiz(Rect ic)
        {
            int n3d = katalog != null ? katalog.karakterler.Count : 0;
            int n2d = kutuphane != null ? kutuphane.characters.Count : 0;
            Baslik(ic, "Karakterlerim", "Hikâyende kullanabileceğin karakterler");
            if (Dugme(new Rect(ic.x, ic.y + 76, 190, 38), "3D karakterler (" + n3d + ")", karakterSekme == 0 ? Vurgu : new Color32(60, 66, 88, 255), 14)) { karakterSekme = 0; kaydirma = 0f; }
            if (Dugme(new Rect(ic.x + 200, ic.y + 76, 200, 38), "Çizgi karakterler (" + n2d + ")", karakterSekme == 1 ? Vurgu : new Color32(60, 66, 88, 255), 14)) { karakterSekme = 1; kaydirma = 0f; }
            var alan = new Rect(ic.xMax - 280, ic.y + 76, 280, 38);
            Kutu(alan, Kart);
            var tf = new GUIStyle(YaziStil(15, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(12, 12, 0, 0) };
            karakterArama = GUI.TextField(alan, karakterArama, 30, tf);
            if (karakterArama.Length == 0) Yazdir(new Rect(alan.x + 12, alan.y, alan.width - 24, alan.height), "Ara...", 15, Sönük);

            var liste = new List<string[]>();   // ad, alt yazı
            string q = karakterArama.Trim().ToLowerInvariant();
            if (karakterSekme == 0 && katalog != null)
            {
                foreach (var k in katalog.karakterler)
                {
                    if (k == null || string.IsNullOrEmpty(k.kimlik)) continue;
                    string ad = string.IsNullOrEmpty(k.gorunenAd) ? k.kimlik : k.gorunenAd;
                    int u = k.kimlik.IndexOf('_');
                    string grup = u > 0 ? k.kimlik.Substring(0, u) : "3D";
                    if (q.Length > 0 && ad.ToLowerInvariant().IndexOf(q, StringComparison.Ordinal) < 0 && k.kimlik.ToLowerInvariant().IndexOf(q, StringComparison.Ordinal) < 0) continue;
                    liste.Add(new[] { ad, grup });
                }
            }
            else if (karakterSekme == 1 && kutuphane != null)
            {
                foreach (var c in kutuphane.characters)
                {
                    if (c == null) continue;
                    if (q.Length > 0 && c.id.ToLowerInvariant().IndexOf(q, StringComparison.Ordinal) < 0) continue;
                    liste.Add(new[] { c.id, "Ses: " + c.voice });
                }
            }
            KartIzgara(new Rect(ic.x, ic.y + 126, ic.width, ic.height - 126), liste, 4, 64);
        }

        void KartIzgara(Rect gorunum, List<string[]> liste, int sutun, float yukseklik)
        {
            if (liste.Count == 0) { Yazdir(new Rect(gorunum.x, gorunum.y + 10, gorunum.width, 30), "Gösterilecek kayıt yok.", 15, Sönük); return; }
            const float aralik = 12;
            float w = (gorunum.width - 18 - aralik * (sutun - 1)) / sutun;
            int satir = (liste.Count + sutun - 1) / sutun;
            ListeBasla(gorunum, Mathf.Max(gorunum.height, satir * (yukseklik + aralik)));
            for (int i = 0; i < liste.Count; i++)
            {
                var r = new Rect((i % sutun) * (w + aralik), (i / sutun) * (yukseklik + aralik), w, yukseklik);
                Kutu(r, Kart);
                Yazdir(new Rect(r.x + 14, r.y + 8, r.width - 28, 28), liste[i][0], 16, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(r.x + 14, r.y + 36, r.width - 28, 20), liste[i][1], 12, Sönük);
            }
            ListeBitir(gorunum);
        }

        void MekanlarCiz(Rect ic)
        {
            var turler = Mekan3D.TurAdlari;
            Baslik(ic, "Mekânlar", "Hikâyende kullanabileceğin hazır 3D mekânlar ve arka planlar");
            var liste = new List<string[]>();
            foreach (var t in turler) liste.Add(new[] { t, "3D mekân" });
            if (kutuphane != null) foreach (var b in kutuphane.backgrounds) if (b != null) liste.Add(new[] { b.id, "Çizgi arka plan" });
            KartIzgara(new Rect(ic.x, ic.y + 84, ic.width, ic.height - 84), liste, 4, 64);
        }

        void SesSatiri(Rect ic, float y, string ad, string anahtar, float varsayilan, bool genel)
        {
            Yazdir(new Rect(ic.x, y, 220, 36), ad, 17, Yazi);
            float v = PlayerPrefs.GetFloat(anahtar, varsayilan);
            float yeni = Kaydirgac(new Rect(ic.x + 230, y, 420, 36), v);
            Yazdir(new Rect(ic.x + 670, y, 80, 36), Mathf.RoundToInt(yeni * 100f) + "%", 16, Sönük);
            if (Mathf.Abs(yeni - v) > 0.001f)
            {
                PlayerPrefs.SetFloat(anahtar, yeni);
                if (genel) AudioListener.volume = yeni;
            }
        }

        void SesCiz(Rect ic)
        {
            Baslik(ic, "Ses ayarları", "Seviyeler otomatik kaydedilir ve hikâye oynarken uygulanır.");
            SesSatiri(ic, ic.y + 100, "Genel ses", "HS_SesGenel", 1f, true);
            SesSatiri(ic, ic.y + 156, "Konuşma sesi", "HS_SesKonusma", 1f, false);
            SesSatiri(ic, ic.y + 212, "Müzik ve ortam sesi", "HS_SesMuzik", 0.09f, false);
            SesSatiri(ic, ic.y + 268, "Efekt sesleri", "HS_SesEfekt", 0.85f, false);
            Yazdir(new Rect(ic.x, ic.y + 340, ic.width, 50),
                "Karakter sesleri ve Gemini seslendirme ayarları Aşama 4'te bu ekrana eklenecek.", 13, Sönük, TextAnchor.UpperLeft, false, true);
        }

        void VideoCiz(Rect ic)
        {
            Baslik(ic, "Video ayarları", "TikTok için dikey video biçimi");
            float y = ic.y + 100;
            Kutu(new Rect(ic.x, y, 560, 60), Kart);
            Yazdir(new Rect(ic.x + 18, y, 300, 60), "Çözünürlük", 16, Sönük);
            Yazdir(new Rect(ic.x + 200, y, 340, 60), "1080 x 1920 (dikey)", 20, Yazi, TextAnchor.MiddleLeft, true);
            y += 76;
            Yazdir(new Rect(ic.x, y, 200, 44), "Kare hızı", 17, Yazi);
            int fps = PlayerPrefs.GetInt("HS_Fps", 30);
            if (Dugme(new Rect(ic.x + 230, y, 100, 44), "30 FPS", fps == 30 ? Vurgu : new Color32(60, 66, 88, 255), 15)) PlayerPrefs.SetInt("HS_Fps", 30);
            if (Dugme(new Rect(ic.x + 340, y, 100, 44), "60 FPS", fps == 60 ? Vurgu : new Color32(60, 66, 88, 255), 15)) PlayerPrefs.SetInt("HS_Fps", 60);
            y += 64;
            Yazdir(new Rect(ic.x, y, 420, 40), "TikTok güvenli alan rehberi (yalnız önizleme)", 17, Yazi);
            bool g = PlayerPrefs.GetInt("HS_SafeArea", 0) == 1;
            bool g2 = Anahtar(new Rect(ic.x + 470, y + 6, 56, 28), g);
            if (g2 != g) PlayerPrefs.SetInt("HS_SafeArea", g2 ? 1 : 0);
            y += 56;
            Yazdir(new Rect(ic.x, y, 200, 40), "Video klasörü", 17, Yazi);
            Yazdir(new Rect(ic.x + 230, y, ic.width - 430, 40), VideoKlasoru, 13, Sönük);
            if (Dugme(new Rect(ic.xMax - 180, y, 180, 40), "Klasörü aç", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "\"" + VideoKlasoru + "\"");
            y += 60;
            Yazdir(new Rect(ic.x, y, 200, 40), "Video kodlayıcı", 17, Yazi);
            if (Dugme(new Rect(ic.x + 230, y, 200, 40), kodlayiciBekle ? "Denetleniyor..." : "Denetle", new Color32(60, 66, 88, 255), 14, !kodlayiciBekle)) KodlayiciDenetle();
            Yazdir(new Rect(ic.x + 446, y, ic.width - 450, 40), kodlayiciSonuc, 14, kodlayiciSonuc.StartsWith("Kullanılacak") ? Basari : Tehlike);
            Yazdir(new Rect(ic.x, y + 60, ic.width, 40),
                "Bu ayarlar videoya alırken kullanılır. Güvenli alan rehberi hiçbir zaman videoya girmez.", 13, Sönük);
        }

        void KodlayiciDenetle()
        {
            string ff = FfmpegPipeRecorder.FfmpegBul();   // ana iş parçacığında (StreamingAssets yolu)
            if (ff == null) { kodlayiciSonuc = "ffmpeg bulunamadı (StreamingAssets/ffmpeg)."; return; }
            kodlayiciBekle = true; kodlayiciSonuc = "";
            var t = new Thread(() =>
            {
                string enc = null;
                try { enc = FfmpegPipeRecorder.KodlayiciSec(ff); } catch { }
                kodlayiciSonuc = enc != null ? "Kullanılacak kodlayıcı: " + enc : "Çalışan bir H.264 kodlayıcı bulunamadı.";
                kodlayiciBekle = false;
            }) { IsBackground = true };
            t.Start();
        }

        void ProjelerCiz(Rect ic)
        {
            Baslik(ic, "Projeler", videolar.Count + " kayıtlı video");
            if (Dugme(new Rect(ic.xMax - 330, ic.y + 4, 150, 38), "Yenile", new Color32(60, 66, 88, 255), 14)) VideolariYukle();
            if (Dugme(new Rect(ic.xMax - 170, ic.y + 4, 170, 38), "Klasörü aç", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "\"" + VideoKlasoru + "\"");
            var gorunum = new Rect(ic.x, ic.y + 80, ic.width, ic.height - 90);
            if (videolar.Count == 0)
            {
                Yazdir(new Rect(gorunum.x, gorunum.y + 10, gorunum.width, 60),
                    "Henüz kaydedilmiş video yok. Videolar şu klasöre kaydedilir:\n" + VideoKlasoru, 15, Sönük, TextAnchor.UpperLeft, false, true);
                return;
            }
            const float sat = 64;
            ListeBasla(gorunum, Mathf.Max(gorunum.height, videolar.Count * (sat + 8)));
            for (int i = 0; i < videolar.Count; i++)
            {
                var v = videolar[i];
                var r = new Rect(0, i * (sat + 8), gorunum.width - 18, sat);
                Kutu(r, Kart);
                Yazdir(new Rect(r.x + 18, r.y + 8, r.width - 400, 28), v.ad, 17, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(r.x + 18, r.y + 34, r.width - 400, 22), (v.boyut / 1048576f).ToString("0.0") + " MB  ·  " + v.zaman.ToString("dd.MM.yyyy HH:mm"), 13, Sönük);
                if (Dugme(new Rect(r.xMax - 360, r.y + 12, 170, 40), "Klasörde göster", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "/select,\"" + v.yol + "\"");
                if (Dugme(new Rect(r.xMax - 180, r.y + 12, 164, 40), "Videoyu aç", Vurgu, 15)) Ac(v.yol, "");
            }
            ListeBitir(gorunum);
        }

        void AyarlarCiz(Rect ic)
        {
            Baslik(ic, "Ayarlar", "Klasörler, video aracı ve uygulama bilgisi");
            float y = ic.y + 100;
            Yazdir(new Rect(ic.x, y, 220, 36), "Hikâye klasörü", 16, Yazi);
            Yazdir(new Rect(ic.x + 230, y, ic.width - 450, 36), HikayeKlasoru, 13, Sönük);
            if (Dugme(new Rect(ic.xMax - 180, y - 2, 180, 38), "Klasörü aç", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "\"" + HikayeKlasoru + "\"");
            y += 54;
            Yazdir(new Rect(ic.x, y, 220, 36), "Video klasörü", 16, Yazi);
            Yazdir(new Rect(ic.x + 230, y, ic.width - 450, 36), VideoKlasoru, 13, Sönük);
            if (Dugme(new Rect(ic.xMax - 180, y - 2, 180, 38), "Klasörü aç", new Color32(60, 66, 88, 255), 14)) Ac("explorer.exe", "\"" + VideoKlasoru + "\"");
            y += 54;
            string ff = FfmpegPipeRecorder.FfmpegBul();
            Yazdir(new Rect(ic.x, y, 220, 36), "Video aracı (ffmpeg)", 16, Yazi);
            Yazdir(new Rect(ic.x + 230, y, ic.width - 240, 36), ff != null ? "Hazır" : "Bulunamadı: video kaydı çalışmaz", 14, ff != null ? Basari : Tehlike);
            y += 54;
            Yazdir(new Rect(ic.x, y, 220, 36), "Tam ekran", 16, Yazi);
            bool tam = Screen.fullScreen;
            if (Anahtar(new Rect(ic.x + 230, y + 4, 56, 28), tam)) Screen.fullScreen = !tam;
            y += 54;
            Yazdir(new Rect(ic.x, y, 220, 36), "Sürüm", 16, Yazi);
            Yazdir(new Rect(ic.x + 230, y, ic.width - 240, 36), "Hikâye Stüdyosu " + Application.version + "  ·  Unity " + Application.unityVersion, 14, Sönük);
            y += 64;
            if (!sifirlaOnay)
            {
                if (Dugme(new Rect(ic.x, y, 240, 42), "Ayarları sıfırla", new Color32(60, 66, 88, 255), 15)) sifirlaOnay = true;
            }
            else
            {
                Yazdir(new Rect(ic.x, y, 360, 42), "Ses ve video ayarları varsayılana dönsün mü?", 14, Yazi);
                if (Dugme(new Rect(ic.x + 380, y, 120, 42), "Evet", Tehlike, 15)) { SesVideoSifirla(); sifirlaOnay = false; mesaj = "Ayarlar varsayılana döndü."; mesajHata = false; }
                if (Dugme(new Rect(ic.x + 510, y, 120, 42), "Vazgeç", new Color32(60, 66, 88, 255), 15)) sifirlaOnay = false;
            }
            MesajCiz(new Rect(ic.x, y + 60, Mathf.Min(ic.width, 560), 44));
        }

        static void SesVideoSifirla()
        {
            foreach (var k in new[] { "HS_SesGenel", "HS_SesKonusma", "HS_SesMuzik", "HS_SesEfekt", "HS_Fps", "HS_SafeArea" }) PlayerPrefs.DeleteKey(k);
            AudioListener.volume = 1f;
        }

        // ---------- hikâye oynatma ----------
        static void Gunluk(string m)
        {
            try { File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "arayuz_log.txt"), DateTime.Now.ToString("HH:mm:ss") + " " + m + "\r\n"); } catch { }
        }

        void Oyna(HikayeBilgi h)
        {
            Gunluk("Oyna: " + h.ad + " olcek=" + olcek.ToString("0.00") + " pencere=" + Screen.width + "x" + Screen.height);
            try
            {
                PlayerPrefs.SetInt("HS_SafeArea", PlayerPrefs.GetInt("HS_SafeArea", 0));
                var ta = h.asset != null ? h.asset : new TextAsset(h.metin);
                if (h.asset == null) ta.name = h.ad;
                var go = new GameObject("StoryPlayer");
                var sp = go.AddComponent<StoryPlayer>();
                sp.story = ta;
                sp.autoPlay = true;
                sp.voiceVolume = PlayerPrefs.GetFloat("HS_SesKonusma", 1f);
                sp.ambientVolume = PlayerPrefs.GetFloat("HS_SesMuzik", 0.09f);
                sp.sfxVolume = PlayerPrefs.GetFloat("HS_SesEfekt", 0.85f);
                var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
                if (ol != null) foreach (var pf in ol.prefablar) if (pf != null && !sp.oyuncular.Contains(pf)) sp.oyuncular.Add(pf);
                bitti = false; oynuyor = true;
                StoryPlayer.Finished -= Bitis; StoryPlayer.Finished += Bitis;
            }
            catch (Exception e) { Gunluk("Oyna HATA: " + e); mesaj = "Hikâye başlatılamadı: " + e.Message; mesajHata = true; oynuyor = false; }
        }

        void Bitis() { bitti = true; Gunluk("Hikaye bitti"); }

        void OynarkenCiz()
        {
            var ev = Event.current;
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape) { AnaEkranaDon(); ev.Use(); return; }
            if (Dugme(new Rect(14, 14, 190, 40), bitti ? "Ana ekrana dön" : "Durdur ve dön (Esc)", new Color(0.1f, 0.12f, 0.18f, 0.85f), 14)) AnaEkranaDon();
        }

        void AnaEkranaDon()
        {
            StoryPlayer.Finished -= Bitis;
            oynuyor = false; bitti = false;
            Time.timeScale = 1f;
            Application.targetFrameRate = 60;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            bolum = Bolum.Hikayelerim;
        }
    }
}
