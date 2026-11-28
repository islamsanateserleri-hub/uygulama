using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HikayeStudyo
{
    // "Ekle" acilir menusunun ogesi (arama kutulu AdvancedDropdown)
    class EkleOgesi : AdvancedDropdownItem
    {
        public string Metin;
        public bool SatirIci;
        public EkleOgesi(string ad, string metin, bool satirIci) : base(ad) { Metin = metin; SatirIci = satirIci; }
    }

    class EkleMenusu : AdvancedDropdown
    {
        readonly StoryLibrary lib;
        readonly System.Action<EkleOgesi> sec;

        public EkleMenusu(AdvancedDropdownState state, StoryLibrary lib, System.Action<EkleOgesi> sec) : base(state)
        {
            this.lib = lib;
            this.sec = sec;
            minimumSize = new Vector2(300, 380);
        }

        static AdvancedDropdownItem Grup(string ad) { return new AdvancedDropdownItem(ad); }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Ekle");
            var chars = new List<string>();
            if (lib != null)
            {
                if (lib.narrator != null && !string.IsNullOrEmpty(lib.narrator.id)) chars.Add(lib.narrator.id);
                foreach (var c in lib.characters) if (c != null && !string.IsNullOrEmpty(c.id)) chars.Add(c.id);
            }
            if (chars.Count == 0) chars.Add("Arda");

            var konus = Grup("Konuşma");
            foreach (var ch in chars)
            {
                var g = Grup(ch);
                foreach (var e in Util.Emotions) g.AddChild(new EkleOgesi(ch + " · " + e + " (dış)", "[" + ch + "|" + e + "|dis] ", false));
                foreach (var e in Util.Emotions) g.AddChild(new EkleOgesi(ch + " · " + e + " (iç ses)", "[" + ch + "|" + e + "|ic] ", false));
                konus.AddChild(g);
            }
            root.AddChild(konus);

            var mekan = Grup("Mekan");
            foreach (var m in StudyoDogrulama.MekanAdlari(lib)) mekan.AddChild(new EkleOgesi("mekan: " + m, "mekan: " + m, false));
            root.AddChild(mekan);

            var poz = Grup("Poz");
            foreach (var ch in chars)
                foreach (var p in Util.Poses) poz.AddChild(new EkleOgesi("poz: " + ch + " " + p, "poz: " + ch + " " + p, false));
            root.AddChild(poz);

            var efekt = Grup("Efekt");
            foreach (var e in StudyoDogrulama.EfektAdlari(lib)) efekt.AddChild(new EkleOgesi("efekt: " + e, "efekt: " + e, false));
            root.AddChild(efekt);

            var atm = Grup("Atmosfer");
            foreach (var a in new[] { "mum", "toz", "yagmur", "buyu", "gunes", "sis", "mum, toz", "yok" }) atm.AddChild(new EkleOgesi("atmosfer: " + a, "atmosfer: " + a, false));
            root.AddChild(atm);

            var kam = Grup("Kamera");
            foreach (var k in new[] { "otomatik", "yakin", "genis", "salla" }) kam.AddChild(new EkleOgesi("kamera: " + k, "kamera: " + k, false));
            root.AddChild(kam);

            var muz = Grup("Müzik");
            muz.AddChild(new EkleOgesi("muzik: pad", "muzik: pad", false));
            muz.AddChild(new EkleOgesi("muzik: yok", "muzik: yok", false));
            if (lib != null) foreach (var m in lib.music) if (m != null && !string.IsNullOrEmpty(m.id)) muz.AddChild(new EkleOgesi("muzik: " + m.id, "muzik: " + m.id, false));
            root.AddChild(muz);

            var gor = Grup("Görsel");
            gor.AddChild(new EkleOgesi("gorsel: eller", "gorsel: eller", false));
            gor.AddChild(new EkleOgesi("gorsel: yok", "gorsel: yok", false));
            if (lib != null) foreach (var m in lib.inserts) if (m != null && !string.IsNullOrEmpty(m.id)) gor.AddChild(new EkleOgesi("gorsel: " + m.id, "gorsel: " + m.id, false));
            root.AddChild(gor);

            var son = Grup("Bölüm sonu / seçim");
            son.AddChild(new EkleOgesi("son: yumusak Devamı yarın...", "son: yumusak Devamı yarın...", false));
            son.AddChild(new EkleOgesi("son: sert", "son: sert", false));
            son.AddChild(new EkleOgesi("secim: A) ... / B) ...", "secim: A) ... / B) ...", false));
            son.AddChild(new EkleOgesi("sonuc: %60 A", "sonuc: %60 A", false));
            root.AddChild(son);

            var etiket = Grup("Ses etiketi (satır içi)");
            foreach (var t in new[] { "(iç çek)", "(ağla)", "(hıçkır)", "(gül)", "(kıkırda)", "(nefes)", "(nefesi kesil)", "(fısılda)", "(öksür)", "(kısa ara)", "(uzun ara)", "*vurgu*" })
                etiket.AddChild(new EkleOgesi(t, t, true));
            root.AddChild(etiket);

            var diger = Grup("Tüm komutlar");
            StoryParser.Parse("");
            foreach (var b in KomutKayit.Hepsi())
                diger.AddChild(new EkleOgesi(b.Ad + (b.Aciklama.Length > 0 ? "  —  " + b.Aciklama : ""), b.Ornek.Length > 0 ? b.Ornek : b.Ad + ": ", false));
            root.AddChild(diger);
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var e = item as EkleOgesi;
            if (e != null && sec != null) sec(e);
        }
    }

    class KontrolMaddesi
    {
        public int Durum;      // 0 tamam, 1 uyari, 2 hata, 3 bilgi
        public string Metin = "";
        public int Satir;
    }

    // Hikaye > Studyo (Ctrl+Shift+H): hikayeyi yaz, kontrol et, seslendir, onizle, videoya al.
    public class StudyoWindow : EditorWindow
    {
        const string TextCtl = "HikayeMetni";

        string[] names = new string[0];
        [SerializeField] int sel;
        [SerializeField] string text = "";
        [SerializeField] string loadedPath = "";
        [SerializeField] bool dirty;
        Vector2 scroll, bottomScroll, problemScroll;
        ParseResult parsed;
        bool showVoice = true, showHelp, showProblems = true, showCheck = true, showScenes;
        [System.NonSerialized] GUIStyle editorStyle, linkStyle, barStyle, rowStyle;
        int cursorLine = 1;          // imlecin bulundugu satir (1'den)
        int cursorIdx = -1;          // imlecin metindeki yeri
        int selA = -1, selB = -1;    // secim sinirlari (metin indeksi)
        int qChar, qEmo, qMot;
        bool qInner;

        // onbellek (metin degisince/ses degisince yeniden hesaplanir)
        bool gecerli;
        List<Sorun> sorunlar;
        SureOzeti ozet;
        List<KontrolMaddesi> kontrol = new List<KontrolMaddesi>();
        List<StoryItem> eksikSes = new List<StoryItem>();
        int hataSayisi;

        int jumpLine, jumpTry;
        double lastTypeTime;
        readonly AdvancedDropdownState ddState = new AdvancedDropdownState();
        string bildirim = "";

        [MenuItem("Hikaye/Stüdyo %#h", priority = 0)]
        public static void Open()
        {
            var w = GetWindow<StudyoWindow>("Hikaye Stüdyosu");
            w.minSize = new Vector2(460, 600);
            w.Show();
        }

        void OnEnable()
        {
            GeminiSes.Changed += OnSesDegisti;
            GeminiSes.Bitti += OnSesBitti;
            KayitTakip.Bitti += OnKayitBitti;
            Undo.undoRedoPerformed += OnUndoRedo;
            RefreshList();
            int i = string.IsNullOrEmpty(loadedPath) ? -1 : System.Array.IndexOf(names, Path.GetFileNameWithoutExtension(loadedPath));
            if (i >= 0 && dirty && text.Length > 0) sel = i;           // domain reload: kaydedilmemis yazi korunur
            else if (names.Length > 0) Load(Mathf.Clamp(sel, 0, names.Length - 1));
            Gecersiz();
        }

        void OnDisable()
        {
            GeminiSes.Changed -= OnSesDegisti;
            GeminiSes.Bitti -= OnSesBitti;
            KayitTakip.Bitti -= OnKayitBitti;
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        void OnSesDegisti() { gecerli = false; Repaint(); }

        void OnSesBitti(string durum)
        {
            bildirim = "Seslendirme bitti: " + durum;
            ShowNotification(new GUIContent("Bitti: " + durum));
            Repaint();
        }

        void OnKayitBitti()
        {
            ShowNotification(new GUIContent("Bitti: video hazır"));
            Repaint();
        }

        void OnUndoRedo()
        {
            dirty = true;
            Gecersiz();
            GUI.FocusControl(null);
            Repaint();
        }

        void Gecersiz() { gecerli = false; parsed = null; }

        // Uzaktan komut (hikaye_yaz) bir hikayeyi yazinca cagrilir
        public static void DisaridanDegisti(string ad)
        {
            foreach (var w in Resources.FindObjectsOfTypeAll<StudyoWindow>()) w.DisaridaGuncellendi(ad);
        }

        void DisaridaGuncellendi(string ad)
        {
            RefreshList();
            int i = System.Array.IndexOf(names, ad);
            if (i >= 0 && i == sel)
            {
                if (!dirty) Load(i);
                else bildirim = "'" + ad + "' dışarıdan değişti; senin kaydedilmemiş yazın duruyor (Kaydet dosyanın üzerine yazar).";
            }
            ShowNotification(new GUIContent("Yeni hikaye yazıldı: " + ad));
            Gecersiz();
            Repaint();
        }

        void RefreshList()
        {
            Directory.CreateDirectory(OtomatikKurulum.StoryFolder);
            var files = Directory.GetFiles(OtomatikKurulum.StoryFolder, "*.txt");
            var list = new List<string>();
            foreach (var f in files) list.Add(Path.GetFileNameWithoutExtension(f));
            list.Sort();
            names = list.ToArray();
            if (sel >= names.Length) sel = Mathf.Max(0, names.Length - 1);
        }

        string PathOf(int i) { return OtomatikKurulum.StoryFolder + "/" + names[i] + ".txt"; }

        void Load(int i)
        {
            if (i < 0 || i >= names.Length) return;
            sel = i;
            loadedPath = PathOf(i);
            text = File.ReadAllText(loadedPath);
            dirty = false;
            Undo.ClearUndo(this);
            Gecersiz();
            GUI.FocusControl(null);
        }

        void Save()
        {
            if (string.IsNullOrEmpty(loadedPath)) return;
            File.WriteAllText(loadedPath, text, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(loadedPath);
            dirty = false;
        }

        void NewStory()
        {
            if (dirty && EditorUtility.DisplayDialog("Hikaye", "Değişiklikler kaydedilsin mi?", "Kaydet", "Hayır")) Save();
            int n = 1;
            while (File.Exists(OtomatikKurulum.StoryFolder + "/bolum_" + n + ".txt")) n++;
            var p = OtomatikKurulum.StoryFolder + "/bolum_" + n + ".txt";
            // onceki bolumun seri adini al, bolum numarasini bir artir
            string seri = "";
            if (names.Length > 0)
            {
                var prev = StoryParser.Parse(File.ReadAllText(PathOf(names.Length - 1)));
                seri = prev.Seri;
            }
            File.WriteAllText(p, "// Yeni bölüm\n" + (seri.Length > 0 ? "seri: " + seri + "\n" : "seri: \n") + "bolum: " + n + "\nbaslik: \n" +
                "mekan: oda_gece\nkanca: \n[Arda|normal|ic] ...\nson: yumusak Devamı yarın...\n", new UTF8Encoding(false));
            AssetDatabase.ImportAsset(p);
            RefreshList();
            Load(System.Array.IndexOf(names, "bolum_" + n));
        }

        StoryLibrary Lib()
        {
            var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
            if (lib == null) { OtomatikKurulum.Ensure(); lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath); }
            return lib;
        }

        ParseResult Parsed()
        {
            if (parsed == null) parsed = StoryParser.Parse(text);
            return parsed;
        }

        // ---------- metin duzenleme (hepsi Undo'ya kaydedilir) ----------
        void SetText(string nt, string undoAd)
        {
            if (nt == text) return;
            Undo.RecordObject(this, undoAd);
            text = nt;
            dirty = true;
            Gecersiz();
        }

        int LineStart(int line)
        {
            int pos = 0;
            for (int i = 1; i < line; i++)
            {
                int n = text.IndexOf('\n', pos);
                if (n < 0) return text.Length;
                pos = n + 1;
            }
            return pos;
        }

        int LineEnd(int start)
        {
            int n = text.IndexOf('\n', Mathf.Clamp(start, 0, text.Length));
            return n < 0 ? text.Length : n;
        }

        // Sorun listesinden satira git: yazi alani odaklanir, satir secilir
        void JumpToLine(int line)
        {
            if (line <= 0) return;
            jumpLine = line;
            jumpTry = 0;
            float w = Mathf.Max(100f, position.width - 30f);
            float h = editorStyle != null ? editorStyle.CalcHeight(new GUIContent(text.Substring(0, LineStart(line))), w) : line * 18f;
            scroll.y = Mathf.Max(0f, h - 40f);
            GUI.FocusControl(TextCtl);
            Repaint();
        }

        void ApplyJump()
        {
            if (jumpLine <= 0) return;
            if (GUI.GetNameOfFocusedControl() != TextCtl || GUIUtility.keyboardControl == 0)
            {
                if (++jumpTry > 40) jumpLine = 0;
                Repaint();
                return;
            }
            var te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
            if (te == null) { jumpLine = 0; return; }
            int start = LineStart(jumpLine), end = LineEnd(start);
            te.selectIndex = start;
            te.cursorIndex = end;
            selA = start; selB = end;
            cursorIdx = end; cursorLine = jumpLine;
            jumpLine = 0;
            Repaint();
        }

        // Imlecin bulundugu satirin (ya da secili satirlarin) basina // koyar / kaldirir
        void ToggleComment()
        {
            int a = selA >= 0 ? Mathf.Min(selA, selB) : (cursorIdx >= 0 ? cursorIdx : 0);
            int b = selA >= 0 ? Mathf.Max(selA, selB) : a;
            a = Mathf.Clamp(a, 0, text.Length);
            b = Mathf.Clamp(b, 0, text.Length);
            int ls = a == 0 ? 0 : text.LastIndexOf('\n', a - 1) + 1;
            int bb = (b > a && text[b - 1] == '\n') ? b - 1 : b;
            int le = text.IndexOf('\n', bb);
            if (le < 0) le = text.Length;
            var lines = text.Substring(ls, le - ls).Split('\n');
            bool hepsi = true;
            foreach (var l in lines)
                if (l.Trim().Length > 0 && !l.TrimStart().StartsWith("//")) { hepsi = false; break; }
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (l.Trim().Length == 0) continue;
                if (hepsi)
                {
                    int at = l.IndexOf("//");
                    string rest = l.Substring(at + 2);
                    if (rest.StartsWith(" ")) rest = rest.Substring(1);
                    lines[i] = l.Substring(0, at) + rest;
                }
                else lines[i] = "// " + l;
            }
            SetText(text.Substring(0, ls) + string.Join("\n", lines) + text.Substring(le), hepsi ? "Yorumu kaldır" : "Satırı yoksay");
            GUI.FocusControl(null);
            Repaint();
        }

        // Imlecin bulundugu satirin sonuna yeni satir ekler
        void InsertLine(string line)
        {
            int at = cursorIdx < 0 || cursorIdx > text.Length ? text.Length : text.IndexOf('\n', cursorIdx);
            if (at < 0) at = text.Length;
            string prefix = text.Length == 0 ? "" : "\n";
            SetText(text.Insert(at, prefix + line), "Satır ekle");
            cursorIdx = at + prefix.Length + line.Length;
            GUI.FocusControl(null);
            Repaint();
        }

        void InsertInline(string s)
        {
            int at = cursorIdx < 0 || cursorIdx > text.Length ? text.Length : cursorIdx;
            SetText(text.Insert(at, s), "Etiket ekle");
            cursorIdx = at + s.Length;
            GUI.FocusControl(null);
            Repaint();
        }

        void EkleSecildi(EkleOgesi e)
        {
            if (e == null) return;
            if (e.SatirIci) InsertInline(e.Metin); else InsertLine(e.Metin);
        }

        // ---------- hesaplar ----------
        void Yenile(StoryLibrary lib)
        {
            string story = names.Length > 0 ? names[sel] : null;
            parsed = StoryParser.Parse(text);
            sorunlar = StudyoDogrulama.Dogrula(text, lib, story, false);
            hataSayisi = StudyoDogrulama.HataSayisi(sorunlar);
            ozet = StudyoAnaliz.Hesapla(parsed, lib, story);
            eksikSes = (lib != null && story != null) ? GeminiSes.Eksikler(lib, story, parsed.Items) : new List<StoryItem>();
            kontrol = KontrolListesi(lib, story);
            gecerli = true;
        }

        static string Mmss(float s)
        {
            int n = Mathf.RoundToInt(s);
            return (n / 60) + ":" + (n % 60).ToString("00");
        }

        bool Kodu(string kod)
        {
            foreach (var s in sorunlar) if (s.Kod == kod) return true;
            return false;
        }

        List<KontrolMaddesi> KontrolListesi(StoryLibrary lib, string story)
        {
            var l = new List<KontrolMaddesi>();
            var p = parsed;
            // 1) sesler
            if (p.DialogueCount == 0) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Hiç konuşma satırı yok." });
            else if (eksikSes.Count == 0) l.Add(new KontrolMaddesi { Durum = 0, Metin = "Bütün konuşmaların sesi hazır (" + p.DialogueCount + "/" + p.DialogueCount + ")." });
            else
            {
                var sat = new List<string>();
                for (int i = 0; i < eksikSes.Count && i < 12; i++) sat.Add(eksikSes[i].Line.ToString());
                l.Add(new KontrolMaddesi
                {
                    Durum = 2,
                    Satir = eksikSes[0].Line,
                    Metin = "Sesi eksik " + eksikSes.Count + " satır: " + string.Join(", ", sat.ToArray()) + (eksikSes.Count > 12 ? "…" : "") + ". Kayıt bunlar üretilmeden başlamaz."
                });
            }
            foreach (var r in GeminiSes.Rapor) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Ses: " + r });
            // 2) doğrulama
            if (hataSayisi > 0) l.Add(new KontrolMaddesi { Durum = 2, Metin = hataSayisi + " hata var (Sorunlar panelinde); önce düzelt." });
            else if (sorunlar.Count > 0) l.Add(new KontrolMaddesi { Durum = 1, Metin = (sorunlar.Count) + " uyarı var (Sorunlar paneline bak)." });
            else l.Add(new KontrolMaddesi { Durum = 0, Metin = "Doğrulama temiz." });
            // 3) kanca
            if (Kodu("KANCA_YOK")) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Kanca: ilk 1,5 saniyede ekranda yazı/ses/hareket yok." });
            else l.Add(new KontrolMaddesi { Durum = 0, Metin = "Kanca ilk 1,5 saniyede." });
            // 4) süre
            int secs = Mathf.RoundToInt(ozet.Sure);
            if (secs < 61) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Süre ~" + Mmss(ozet.Sure) + ": TikTok Creator Rewards için 61 sn'den uzun olmalı (hedef 1:01–1:30)." });
            else l.Add(new KontrolMaddesi { Durum = 0, Metin = "Süre ~" + Mmss(ozet.Sure) + " (61 sn üstü)." });
            // 5) bölüm sonu
            if (Kodu("SON_YOK")) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Bölüm sonu yok (son: yumusak ... / son: sert)." });
            else l.Add(new KontrolMaddesi { Durum = 0, Metin = "Bölüm sonu var." });
            // 6) seçim kartı
            StoryItem choice = null;
            foreach (var it in p.Items) if (it.Type == ItemType.Choice) { choice = it; break; }
            if (choice != null)
            {
                if (Kodu("SECIM_TWIST")) l.Add(new KontrolMaddesi { Durum = 1, Satir = choice.Line, Metin = "Seçim kartından önce twist yok: kartın hemen öncesindeki replik soru/şok ('?', '!', '...') bırakmalı." });
                else l.Add(new KontrolMaddesi { Durum = 3, Satir = choice.Line, Metin = "Hatırlatma: seçim kartından önceki replik gerçekten bir twist bırakıyor mu? İzleyici oy vermek için merak etmeli." });
            }
            // 7) seri bilgisi
            var eks = new List<string>();
            if (string.IsNullOrEmpty(p.Seri)) eks.Add("seri:");
            if (p.Bolum <= 0) eks.Add("bolum:");
            if (string.IsNullOrEmpty(p.Baslik)) eks.Add("baslik:");
            if (eks.Count > 0) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Kapak/açıklama için eksik: " + string.Join(" ", eks.ToArray()) });
            else l.Add(new KontrolMaddesi { Durum = 0, Metin = "Seri, bölüm ve başlık dolu." });
            // 8) recorder
            if (!StudyoKayit.RecorderInstalled) l.Add(new KontrolMaddesi { Durum = 2, Metin = "Recorder paketi kurulu değil (Video için Recorder'ı kur)." });
            // 9) ritim denetçisi (varsa)
            var ritim = StudyoAnaliz.RitimRaporu(p, lib);
            for (int i = 0; i < ritim.Count && i < 6; i++) l.Add(new KontrolMaddesi { Durum = 1, Metin = "Ritim: " + ritim[i] });
            return l;
        }

        bool GuvenliAlanTamam() { return names.Length > 0 && EditorPrefs.GetBool("HS_GA_" + names[sel], false); }

        // ---------- cizim ----------
        void OnGUI()
        {
            if (editorStyle == null || barStyle == null || linkStyle == null || rowStyle == null)
            {
                editorStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true, fontSize = 13 };
                editorStyle.richText = false;
                linkStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, alignment = TextAnchor.MiddleLeft };
                linkStyle.hover.textColor = new Color(0.45f, 0.75f, 1f);
                barStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };
                barStyle.normal.textColor = Color.white;
                rowStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            }
            var lib = Lib();
            var ev = Event.current;

            // --- ust cubuk ---
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (names.Length > 0)
            {
                int ns = EditorGUILayout.Popup(sel, names, EditorStyles.toolbarPopup, GUILayout.Width(170));
                if (ns != sel)
                {
                    if (dirty && EditorUtility.DisplayDialog("Hikaye", "Değişiklikler kaydedilsin mi?", "Kaydet", "Hayır")) Save();
                    Load(ns);
                }
            }
            if (GUILayout.Button("Yeni bölüm", EditorStyles.toolbarButton)) NewStory();
            GUI.enabled = dirty;
            if (GUILayout.Button(dirty ? "Kaydet *" : "Kaydet", EditorStyles.toolbarButton)) Save();
            GUI.enabled = true;
            if (GUILayout.Button("↶", EditorStyles.toolbarButton, GUILayout.Width(24))) Undo.PerformUndo();
            if (GUILayout.Button("↷", EditorStyles.toolbarButton, GUILayout.Width(24))) Undo.PerformRedo();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Kütüphaneyi Aç", EditorStyles.toolbarButton)) { Selection.activeObject = lib; EditorGUIUtility.PingObject(lib); }
            EditorGUILayout.EndHorizontal();

            if (names.Length == 0)
            {
                EditorGUILayout.HelpBox("Hikaye yok. 'Yeni bölüm'e bas.", MessageType.Info);
                return;
            }

            if (sorunlar == null || ozet == null || (!gecerli && ev.type == EventType.Layout)) Yenile(lib);

            // --- bitti bildirimi ---
            if (KayitTakip.BildirimVar)
            {
                bool ok = KayitTakip.Durum == "bitti";
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(ok ? "Bitti: video hazır — " + Path.GetFileName(StudyoKayit.LastVideo) : (KayitTakip.Durum == "reddedildi" ? "Kayıt reddedildi" : "Kayıt sorunu") + ": " + KayitTakip.Mesaj,
                    EditorStyles.wordWrappedLabel);
                if (ok && GUILayout.Button("Klasörde göster", GUILayout.Width(110))) EditorUtility.RevealInFinder(StudyoKayit.LastVideo);
                if (GUILayout.Button("Kapat", GUILayout.Width(50))) KayitTakip.BittiyiKapat();
                EditorGUILayout.EndHorizontal();
            }
            if (bildirim.Length > 0)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(bildirim, EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Kapat", GUILayout.Width(50))) bildirim = "";
                EditorGUILayout.EndHorizontal();
            }

            // --- hizli ekleme ---
            QuickBar(lib);

            // --- yazma alani ---
            if (ev.type == EventType.KeyDown && (ev.control || ev.command) && ev.keyCode == KeyCode.Slash)
            {
                ToggleComment();
                ev.Use();
            }
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(150), GUILayout.ExpandHeight(true));
            // GUILayout.TextArea: imlec konumu TextEditor durumundan okunabilsin diye (EditorGUILayout'un editoru gizli)
            GUI.SetNextControlName(TextCtl);
            var nt = GUILayout.TextArea(text, editorStyle, GUILayout.ExpandHeight(true));
            if (GUIUtility.keyboardControl != 0)
            {
                var te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                if (te != null && te.text == nt)
                {
                    int ci = Mathf.Clamp(te.cursorIndex, 0, nt.Length), ln = 1;
                    cursorIdx = ci;
                    selA = Mathf.Clamp(te.selectIndex, 0, nt.Length);
                    selB = ci;
                    for (int q = 0; q < ci; q++) if (nt[q] == '\n') ln++;
                    if (ln != cursorLine) { cursorLine = ln; Repaint(); }
                }
            }
            ApplyJump();
            EditorGUILayout.EndScrollView();
            if (nt != text)
            {
                double now = EditorApplication.timeSinceStartup;
                if (now - lastTypeTime > 1.0) Undo.IncrementCurrentGroup();
                lastTypeTime = now;
                Undo.RecordObject(this, "Hikaye yazısı");
                text = nt; dirty = true; Gecersiz();
            }

            // --- sayac cubugu ---
            var p = parsed != null ? parsed : Parsed();
            string storyName = names[sel];
            int ready = p.DialogueCount - eksikSes.Count;
            SayacCubugu();
            EditorGUILayout.LabelField("Konuşma: " + p.DialogueCount + "  ·  Ses: " + ready + "/" + p.DialogueCount +
                "  ·  Kelime: " + ozet.Kelime + "  ·  Süre: ~" + Mmss(ozet.Sure) + "  ·  İmleç: " + cursorLine + ". satır", EditorStyles.miniBoldLabel);

            // --- oynat ---
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !EditorApplication.isPlaying;
            if (GUILayout.Button("▶  Önizle", GUILayout.Height(32))) PlayStory(false, 0);
            if (GUILayout.Button("▶ " + cursorLine + ". satırdan", GUILayout.Height(32), GUILayout.Width(110))) PlayStory(false, cursorLine);
            if (StudyoKayit.RecorderInstalled)
            {
                if (GUILayout.Button("●  Videoya al (MP4)", GUILayout.Height(32))) PlayStory(true, 0);
            }
            else if (GUILayout.Button("Video için Recorder'ı kur", GUILayout.Height(32))) StudyoKayit.InstallRecorder();
            GUI.enabled = true;
            if (EditorApplication.isPlaying && GUILayout.Button("■  Durdur", GUILayout.Height(32))) EditorApplication.isPlaying = false;
            EditorGUILayout.EndHorizontal();
            bool safe = EditorPrefs.GetBool("HS_SafeArea", false);
            bool ns2 = EditorGUILayout.ToggleLeft("Önizlemede TikTok güvenli alanını göster (videoya girmez)", safe);
            if (ns2 != safe) EditorPrefs.SetBool("HS_SafeArea", ns2);
            if (!string.IsNullOrEmpty(StudyoKayit.LastVideo) && File.Exists(StudyoKayit.LastVideo))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Son video: " + Path.GetFileName(StudyoKayit.LastVideo), EditorStyles.miniLabel);
                if (GUILayout.Button("Klasörde göster", EditorStyles.miniButton, GUILayout.Width(110))) EditorUtility.RevealInFinder(StudyoKayit.LastVideo);
                EditorGUILayout.EndHorizontal();
            }

            // --- alt paneller (kaydirilabilir) ---
            bottomScroll = EditorGUILayout.BeginScrollView(bottomScroll, GUILayout.MaxHeight(Mathf.Max(160f, position.height * 0.5f)));
            SorunPaneli();
            KontrolPaneli();
            SahnePaneli();
            SesPaneli(lib, storyName, p);
            showHelp = EditorGUILayout.Foldout(showHelp, "Yazım kılavuzu", true);
            if (showHelp) EditorGUILayout.HelpBox(Help, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        void SayacCubugu()
        {
            Rect r = GUILayoutUtility.GetRect(1, 20, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, new Color(0.14f, 0.14f, 0.14f));
            const float max = 100f;
            float f = Mathf.Clamp01(ozet.Sure / max);
            int secs = Mathf.RoundToInt(ozet.Sure);
            Color c = secs < 61 ? new Color(0.85f, 0.6f, 0.1f) : (secs <= 90 ? new Color(0.2f, 0.62f, 0.3f) : new Color(0.8f, 0.4f, 0.15f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * f, r.height), c);
            EditorGUI.DrawRect(new Rect(r.x + r.width * (61f / max) - 1f, r.y, 2f, r.height), Color.white);
            GUI.Label(r, "~" + Mmss(ozet.Sure) + "  ·  hedef 1:01–1:30 (beyaz çizgi 61 sn)  ·  " + (ozet.GercekSesVar ? "gerçek ses süreleriyle" : "tahmini"), barStyle);
        }

        void SorunPaneli()
        {
            string baslik = "Sorunlar (" + hataSayisi + " hata, " + (sorunlar.Count - hataSayisi) + " uyarı)";
            showProblems = EditorGUILayout.Foldout(showProblems, baslik, true);
            if (!showProblems) return;
            if (sorunlar.Count == 0) { EditorGUILayout.LabelField("Sorun yok.", EditorStyles.miniLabel); return; }
            var errIcon = EditorGUIUtility.IconContent("console.erroricon.sml");
            var warnIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
            problemScroll = EditorGUILayout.BeginScrollView(problemScroll, GUILayout.MaxHeight(Mathf.Min(190f, 24f * sorunlar.Count + 8f)));
            for (int i = 0; i < sorunlar.Count; i++)
            {
                var s = sorunlar[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(s.Hata ? errIcon : warnIcon, GUILayout.Width(20), GUILayout.Height(18));
                string etiket = (s.Satir > 0 ? s.Satir + ". satır · " : "") + "[" + s.Kod + "] " + s.Mesaj + (s.Oneri.Length > 0 ? "  →  " + s.Oneri : "");
                if (GUILayout.Button(new GUIContent(etiket, "Tıkla: satıra git"), linkStyle, GUILayout.ExpandWidth(true)) && s.Satir > 0) JumpToLine(s.Satir);
                if (s.Duzeltme.Length > 0 && s.Yanlis.Length > 0 && GUILayout.Button("Düzelt: " + Kisalt(s.Duzeltme, 14), EditorStyles.miniButton, GUILayout.Width(120)))
                {
                    string yeni = StudyoDogrulama.Duzelt(text, s);
                    if (yeni != null) SetText(yeni, "Öneriyi uygula");
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        static string Kisalt(string s, int n) { return s.Length > n ? s.Substring(0, n) + "…" : s; }

        void KontrolPaneli()
        {
            int tamam = 0;
            foreach (var k in kontrol) if (k.Durum == 0) tamam++;
            bool guvenli = GuvenliAlanTamam();
            bool hazir = guvenli;
            foreach (var k in kontrol) if (k.Durum == 2) hazir = false;
            showCheck = EditorGUILayout.Foldout(showCheck, "Kayıt öncesi kontrol listesi (" + tamam + "/" + (kontrol.Count) + " tamam" + (hazir ? " · kayda hazır" : "") + ")", true);
            if (!showCheck) return;
            foreach (var k in kontrol)
            {
                EditorGUILayout.BeginHorizontal();
                var old = GUI.contentColor;
                string tag;
                switch (k.Durum)
                {
                    case 0: GUI.contentColor = new Color(0.4f, 0.85f, 0.45f); tag = "TAMAM"; break;
                    case 1: GUI.contentColor = new Color(1f, 0.8f, 0.3f); tag = "UYARI"; break;
                    case 2: GUI.contentColor = new Color(1f, 0.45f, 0.4f); tag = "HATA"; break;
                    default: GUI.contentColor = new Color(0.6f, 0.8f, 1f); tag = "NOT"; break;
                }
                GUILayout.Label(tag, rowStyle, GUILayout.Width(48));
                GUI.contentColor = old;
                if (GUILayout.Button(k.Metin, linkStyle, GUILayout.ExpandWidth(true)) && k.Satir > 0) JumpToLine(k.Satir);
                EditorGUILayout.EndHorizontal();
            }
            bool g2 = EditorGUILayout.ToggleLeft("Güvenli alan önizlemesine baktım: altyazı ve kartlar ekranın altından ~%20 ve sağ kenardan uzak (TikTok arayüzü orayı kapatır)", guvenli);
            if (g2 != guvenli) EditorPrefs.SetBool("HS_GA_" + names[sel], g2);
        }

        void SahnePaneli()
        {
            showScenes = EditorGUILayout.Foldout(showScenes, "Sahne süreleri (" + ozet.Sahneler.Count + " sahne)", true);
            if (!showScenes) return;
            float maxS = 1f;
            foreach (var s in ozet.Sahneler) maxS = Mathf.Max(maxS, s.Sure);
            for (int i = 0; i < ozet.Sahneler.Count; i++)
            {
                var s = ozet.Sahneler[i];
                Rect r = GUILayoutUtility.GetRect(1, 18, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(r, new Color(0.17f, 0.17f, 0.17f));
                Color c = s.Sure > 25f ? new Color(0.8f, 0.4f, 0.15f, 0.9f) : new Color(0.25f, 0.5f, 0.75f, 0.9f);
                EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * (s.Sure / maxS), r.height), c);
                GUI.Label(r, (i + 1) + ". " + s.Ad + "  ·  " + Mathf.RoundToInt(s.Sure) + " sn  ·  " + s.Konusma + " konuşma  ·  " + s.Kelime + " kelime  ·  " + s.Satir + ". satır" +
                          (s.Sure > 25f ? "  (uzun sahne)" : ""), rowStyle);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) JumpToLine(s.Satir);
            }
        }

        void SesPaneli(StoryLibrary lib, string storyName, ParseResult p)
        {
            showVoice = EditorGUILayout.Foldout(showVoice, "Seslendirme (Gemini)", true);
            if (!showVoice) return;
            EditorGUI.indentLevel++;
            var key = EditorPrefs.GetString(GeminiSes.KeyPref, "");
            var nk = EditorGUILayout.PasswordField("API anahtarı", key);
            if (nk != key) EditorPrefs.SetString(GeminiSes.KeyPref, nk);
            var model = GeminiSes.Model;
            int mi = System.Array.IndexOf(GeminiSes.Models, model);
            int nmi = EditorGUILayout.Popup("Model", mi < 0 ? 0 : mi, GeminiSes.ModelLabels);
            if (nmi != mi) GeminiSes.Model = GeminiSes.Models[nmi];
            int kt = GeminiSes.KisaTekrar;
            int nkt = EditorGUILayout.IntSlider(new GUIContent("Kesik ses yeniden deneme", "Ses beklenen sürenin %70'inden kısaysa en çok bu kadar yeniden üretilir (her deneme kotadan yer)"), kt, 0, 5);
            if (nkt != kt) GeminiSes.KisaTekrar = nkt;
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(GeminiSes.KotaMetni(), EditorStyles.miniLabel);

            int missing = eksikSes.Count;
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !GeminiSes.Busy && missing > 0 && lib != null && nk.Trim().Length > 0;
            if (GUILayout.Button("Eksik sesleri üret (" + missing + ")"))
            {
                if (dirty) Save();
                GeminiSes.Generate(lib, storyName, StoryParser.Parse(text).Items);
            }
            GUI.enabled = GeminiSes.Busy;
            if (GUILayout.Button("Durdur", GUILayout.Width(70))) GeminiSes.Stop();
            GUI.enabled = lib != null;
            if (GUILayout.Button("Listeyi dışa aktar", GUILayout.Width(130)))
            {
                if (dirty) Save();
                SesListesi.Export(lib, AssetDatabase.LoadAssetAtPath<TextAsset>(loadedPath));
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            var cur = LineItem(p, cursorLine);
            if (cur != null && lib != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !GeminiSes.Busy && nk.Trim().Length > 0;
                if (GUILayout.Button("Bu satırı yeniden seslendir (" + cursorLine + ")"))
                {
                    if (dirty) Save();
                    GeminiSes.Regenerate(lib, storyName, LineItem(StoryParser.Parse(text), cursorLine));
                }
                if (GUILayout.Button("Varyant üret", GUILayout.Width(90)))
                {
                    if (dirty) Save();
                    GeminiSes.GenerateVariant(lib, storyName, LineItem(StoryParser.Parse(text), cursorLine));
                }
                GUI.enabled = !GeminiSes.Busy && GeminiSes.HasBackup(lib, storyName, cur);
                if (GUILayout.Button("Eski sesi geri al", GUILayout.Width(120)))
                { if (GeminiSes.RestoreBackup(lib, storyName, cur)) GeminiSes.Status = cursorLine + ". satırın eski sesi geri alındı."; gecerli = false; }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                string active = GeminiSes.Dir(storyName) + "/" + lib.ClipKey(cur) + ".wav";
                var vars = GeminiSes.Variants(lib, storyName, cur);
                if (File.Exists(active) || vars.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Etkin ses", EditorStyles.miniLabel, GUILayout.Width(70));
                    if (File.Exists(active))
                    {
                        float sn = GeminiSes.ClipSeconds(storyName, lib, cur);
                        GUILayout.Label(sn > 0 ? sn.ToString("0.0") + " sn" : "", EditorStyles.miniLabel, GUILayout.Width(50));
                        if (GUILayout.Button("Dinle", EditorStyles.miniButton, GUILayout.Width(50))) GeminiSes.Dinle(active);
                    }
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < vars.Count; i++)
                    {
                        string vp = vars[i];
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Varyant v" + (i + 1), EditorStyles.miniLabel, GUILayout.Width(70));
                        if (GUILayout.Button("Dinle", EditorStyles.miniButton, GUILayout.Width(50))) GeminiSes.Dinle(vp);
                        if (GUILayout.Button("Bunu kullan", EditorStyles.miniButton, GUILayout.Width(80)))
                        {
                            if (GeminiSes.UseVariant(lib, storyName, cur, vp)) GeminiSes.Status = cursorLine + ". satır: " + Path.GetFileName(vp) + " etkin yapıldı (eskisi yedeğe alındı; 'Eski sesi geri al').";
                            gecerli = false;
                        }
                        if (GUILayout.Button("Sil", EditorStyles.miniButton, GUILayout.Width(36))) { GeminiSes.DeleteVariant(vp); gecerli = false; }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            if (missing > 10)
                EditorGUILayout.HelpBox("Ücretsiz kota günde ~10-11 satır: bu bölüm ~" + Mathf.CeilToInt(missing / 10f) +
                    " güne bölünür. Üretilen sesler saklanır; ertesi gün kalanlar üretilir.", MessageType.Info);
            if (GeminiSes.Rapor.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", GeminiSes.Rapor.ToArray()), MessageType.Warning);
            if (!string.IsNullOrEmpty(GeminiSes.Status)) EditorGUILayout.HelpBox(GeminiSes.Status, MessageType.None);
        }

        void QuickBar(StoryLibrary lib)
        {
            var chars = new List<string>();
            if (lib != null) foreach (var c in lib.characters) if (c != null && !string.IsNullOrEmpty(c.id)) chars.Add(c.id);
            if (chars.Count == 0) chars.Add("Arda");
            var mots = new List<string> { "(hareket yok)" }; mots.AddRange(Util.Motions);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            qChar = EditorGUILayout.Popup(Mathf.Clamp(qChar, 0, chars.Count - 1), chars.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(80));
            qEmo = EditorGUILayout.Popup(Mathf.Clamp(qEmo, 0, Util.Emotions.Length - 1), Util.Emotions, EditorStyles.toolbarPopup, GUILayout.Width(80));
            qMot = EditorGUILayout.Popup(Mathf.Clamp(qMot, 0, mots.Count - 1), mots.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(90));
            qInner = GUILayout.Toggle(qInner, "iç ses", EditorStyles.toolbarButton, GUILayout.Width(50));
            if (GUILayout.Button("+ Konuşma", EditorStyles.toolbarButton))
                InsertLine("[" + chars[qChar] + "|" + Util.Emotions[qEmo] + "|" + (qInner ? "ic" : "dis") + (qMot > 0 ? "|" + mots[qMot] : "") + "] ");
            if (GUILayout.Button("Ekle ▾", EditorStyles.toolbarDropDown, GUILayout.Width(60)))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                var dd = new EkleMenusu(ddState, lib, EkleSecildi);
                dd.Show(r);
            }
            if (GUILayout.Button(new GUIContent("// Yoksay", "Satırı yoksay / geri al (Ctrl+/). Seçili birden çok satırda hepsi."), EditorStyles.toolbarButton, GUILayout.Width(70))) ToggleComment();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        static StoryItem LineItem(ParseResult p, int line)
        {
            foreach (var it in p.Items) if (it.Type == ItemType.Dialogue && it.Line == line) return it;
            return null;
        }

        void PlayStory(bool record, int fromLine)
        {
            if (record)
            {
                var lib = Lib();
                var miss = GeminiSes.Eksikler(lib, names[sel], StoryParser.Parse(text).Items);
                if (miss.Count > 0)
                {
                    var sat = new List<string>();
                    for (int i = 0; i < miss.Count && i < 25; i++) sat.Add(miss[i].Line + ". satır: " + miss[i].Character + " — " + Kisalt(miss[i].Text, 40));
                    string m = "Şu " + miss.Count + " satırın sesi yok:\n\n" + string.Join("\n", sat.ToArray()) + (miss.Count > 25 ? "\n…" : "") +
                               "\n\nÖnce 'Eksik sesleri üret'. (Sessiz video kaydı engellendi.)";
                    KayitTakip.Reddet(names[sel], "Sesi olmayan satırlar: " + miss.Count);
                    EditorUtility.DisplayDialog("Kayıt başlatılamadı", m, "Tamam");
                    return;
                }
            }
            Save();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Launch(loadedPath, names[sel], record, fromLine);
        }

        // Uzaktan komut icin: hikaye adiyla oynat
        public static string PlayByName(string name, int fromLine, bool record)
        {
            if (EditorApplication.isPlaying) return "HATA: zaten oynuyor";
            string err;
            if (!UzaktanKomut.AdGecerli(name, out err)) return "HATA: " + err;
            string path = UzaktanKomut.HikayeYolu(name);
            if (path == null || !File.Exists(path)) return "HATA: hikaye yok: " + name;
            if (record)
            {
                if (!StudyoKayit.RecorderInstalled) return "HATA: Recorder kurulu değil";
                var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
                if (lib == null) { OtomatikKurulum.Ensure(); lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath); }
                var miss = GeminiSes.Eksikler(lib, name, StoryParser.Parse(File.ReadAllText(path)).Items);
                if (miss.Count > 0)
                {
                    var sat = new List<string>();
                    foreach (var it in miss) sat.Add(it.Line.ToString());
                    string m = "REDDEDİLDİ: " + miss.Count + " satırın sesi yok (satır: " + string.Join(", ", sat.ToArray()) + "). Önce seslendir: " + name;
                    KayitTakip.Reddet(name, m);
                    return m;
                }
            }
            Launch(path, name, record, fromLine);
            return record ? "kayıt başlıyor" : "oynatılıyor";
        }

        static void Launch(string storyPath, string storyName, bool record, int fromLine)
        {
            PlayerPrefs.SetInt("HS_StartLine", record ? 0 : fromLine);
            PlayerPrefs.SetInt("HS_SafeArea", !record && EditorPrefs.GetBool("HS_SafeArea", false) ? 1 : 0);
            PlayerPrefs.Save();
            var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
            if (lib == null) { OtomatikKurulum.Ensure(); lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath); }
            OtomatikKurulum.Ensure();
            if (SceneManager.GetActiveScene().path != OtomatikKurulum.ScenePath)
                EditorSceneManager.OpenScene(OtomatikKurulum.ScenePath);
            var player = Object.FindAnyObjectByType<StoryPlayer>();
            if (player == null) player = new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
            player.story = AssetDatabase.LoadAssetAtPath<TextAsset>(storyPath);
            player.library = lib;
            // 3D oyuncu listesi: Karakterler/Hazir klasorundeki butun prefablar
            player.oyuncular = new List<GameObject>();
            if (AssetDatabase.IsValidFolder(Karakter3DKurulum.Hazir))
                foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { Karakter3DKurulum.Hazir }))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g));
                    if (go != null && go.GetComponent<Karakter3D>() != null) player.oyuncular.Add(go);
                }
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            EditorSceneManager.SaveScene(player.gameObject.scene);
            GameViewUtil.Portrait();
            SessionState.SetBool(StudyoKayit.Key, record);
            SessionState.SetString(StudyoKayit.NameKey, storyName);
            if (record) KayitTakip.Basla(storyName);
            EditorApplication.isPlaying = true;
        }

        const string Help =
@"KONUŞMA
[Karakter|duygu|dis] Metin      dışarıdan konuşur, ağzı oynar
[Karakter|duygu|ic] Metin       iç ses: ağız kapalı, ekran hafif kararır
[Karakter|duygu|dis|hareket] … hareketle konuşur

Duygular: normal mutlu sevgi uzgun aglamakli korkmus sasirmis kizgin yorgun hasta dusunceli fisilti
Hareketler: kos yuru saril zipla egil sarsil cik

KOMUTLAR (tam liste: 'Ekle ▾' menüsü ya da Docs/HikayeSozdizimi.md)
mekan: oda_gece        mekân değiştirir (karakterler temizlenir)
poz: Arda yatiyor      ayakta / yatiyor / oturuyor
kanca: Öldüm.          siyah ekranda büyük yazı (ilk saniyeler)
geri: 1 saat önce      geri sarma (VHS) efekti
far:                   kamyon farları + beyaz parlama
flas:                  beyaz parlama
yazi: Devamı 2. Bölümde...   üst-ortada büyük yazı
efekt: carpma          carpma korna geri whoosh kapi buyu kalp tahta gok yagmur
atmosfer: mum, toz     mum toz yagmur buyu gunes / yok
kamera: yakin          otomatik / yakin / genis / salla
muzik: yok             pad (varsayılan) / yok / kütüphanedeki müzik
gorsel: eller          tam ekran görsel (eller = kendi gözünden eller) / yok
bekle: 0.5   cikar: Anne   temizle:
son: yumusak Devamı yarın...   merak bırakan yumuşak bitiş
son: sert                      gerilimde bir anda kesilir (siyah ekran)
secim: A) Söylesin / B) Saklasın   izleyici seçim kartı (açıklamaya da yazılır)
sonuc: %69 A                   yeni bölüm başında oylama sonucu kartı
seri: Sessiz Değişim   bolum: 3   baslik: Annem beni tanımadı   (kapak ve açıklama)
oyuncu: Arda = Timmy           Arda'yı 3D karakterle oynat (Karakterler/Hazir; 'yok' = çizgi karakter)

SES ETİKETLERİ (altyazıda görünmez)
(iç çek) (ağla) (hıçkır) (gül) (kıkırda) (nefes) (nefesi kesil) (fısılda) (öksür) (kısa ara) (uzun ara)
*kelime*  vurgu işareti (altyazıda yıldız görünmez)
Sayılar/kısaltmalar seslendirmeden önce Türkçe okunuşa çevrilir (1993, 3., Dr., %, 14:30); özel okunuş: Resources/telaffuz.txt (yazilis=okunus)

STÜDYO
Sorunlar paneli: tıkla = satıra git, 'Düzelt' = 'Bunu mu demek istediniz' önerisini uygula.
// Yoksay (Ctrl+/): satırı geçici kapatır. ↶ ↷: geri al / ileri al.
Kayıt öncesi kontrol listesi: ses, kanca, 61 sn, bölüm sonu, seçim twist'i, güvenli alan. Sesi eksik satır varsa video kaydı başlamaz.
Varyant: bir satır için alternatif ses üret, dinle, 'Bunu kullan' ile değiştir; eski ses 'Eski sesi geri al' ile döner.

YAPAY ZEKA KÖPRÜSÜ
Menü: Hikaye > Yapay Zeka Kılavuzu Yaz  →  Docs/HikayeSozdizimi.md, CLAUDE.md, GEMINI.md
HikayeKomut/komut.txt'ye komut yazılır (hikaye_yaz: ad, dogrula: ad, karakter_listele, onizleme_goruntu: ad 5 ...), sonuç sonuc.txt ve json dosyalarında.

VİDEOYA AL: Videolar/Hikaye/<bölüm>_<tarih>/ klasörüne video, kapak.png ve aciklama.txt çıkar;
kayıt sonunda ses yüksekliği (LUFS) ölçülür ve açıklamadaki notlara yazılır. Bitince pencerede 'Klasörde göster' çıkar.

İPUCU: Duygusal satırlar ve iç ses otomatik yakın çekilir. Boşluk tuşu önizlemeyi duraklatır.";
    }
}
