using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HikayeStudyo
{
    // Hikaye > Studyo (Ctrl+Shift+H): hikayeyi yaz, kontrol et, seslendir, onizle, videoya al.
    public class StudyoWindow : EditorWindow
    {
        string[] names = new string[0];
        int sel;
        string text = "";
        string loadedPath = "";
        bool dirty;
        Vector2 scroll, warnScroll;
        ParseResult parsed;
        bool showVoice = true, showHelp;
        GUIStyle editorStyle;
        int cursorLine = 1;          // imlecin bulundugu satir (1'den)
        int cursorIdx = -1;          // imlecin metindeki yeri
        int qChar, qEmo, qMot, qCmd;
        bool qInner;

        [MenuItem("Hikaye/Stüdyo %#h", priority = 0)]
        public static void Open()
        {
            var w = GetWindow<StudyoWindow>("Hikaye Stüdyosu");
            w.minSize = new Vector2(440, 560);
            w.Show();
        }

        void OnEnable()
        {
            GeminiSes.Changed += Repaint;
            RefreshList();
            if (names.Length > 0) Load(Mathf.Clamp(sel, 0, names.Length - 1));
        }

        void OnDisable() { GeminiSes.Changed -= Repaint; }

        void RefreshList()
        {
            Directory.CreateDirectory(OtomatikKurulum.StoryFolder);
            var files = Directory.GetFiles(OtomatikKurulum.StoryFolder, "*.txt");
            var list = new List<string>();
            foreach (var f in files) list.Add(Path.GetFileNameWithoutExtension(f));
            list.Sort();
            names = list.ToArray();
        }

        string PathOf(int i) { return OtomatikKurulum.StoryFolder + "/" + names[i] + ".txt"; }

        void Load(int i)
        {
            if (i < 0 || i >= names.Length) return;
            sel = i;
            loadedPath = PathOf(i);
            text = File.ReadAllText(loadedPath);
            dirty = false;
            parsed = null;
            GUI.FocusControl(null);
        }

        void Save()
        {
            if (string.IsNullOrEmpty(loadedPath)) return;
            File.WriteAllText(loadedPath, text, new System.Text.UTF8Encoding(false));
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
                "mekan: oda_gece\nkanca: \n[Arda|normal|ic] ...\nson: yumusak Devamı yarın...\n", new System.Text.UTF8Encoding(false));
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

        // Kutuphaneye gore ek kontroller
        List<string> Check(StoryLibrary lib)
        {
            var p = Parsed();
            var w = new List<string>(p.Warnings);
            var missingChars = new HashSet<string>();
            foreach (var it in p.Items)
            {
                if ((it.Type == ItemType.Dialogue || it.Type == ItemType.Pose) && lib.FindCharacter(it.Character) == null && missingChars.Add(it.Character))
                    w.Add(it.Line + ". satır: '" + it.Character + "' kütüphanede yok (Kütüphaneyi Aç > characters).");
                if (it.Type == ItemType.Background && lib.FindBackground(it.Arg) == null)
                    w.Add(it.Line + ". satır: '" + it.Arg + "' mekanı kütüphanede yok.");
                if (it.Type == ItemType.Sfx && lib.FindSfx(it.Arg) == null && !ProcAudio.Has(it.Arg))
                    w.Add(it.Line + ". satır: '" + it.Arg + "' efekti yok (hazır olanlar: carpma, korna, geri, whoosh, kapi, buyu, kalp, tahta, gok, yagmur).");
            }
            return w;
        }

        void OnGUI()
        {
            if (editorStyle == null)
            {
                editorStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true, fontSize = 13 };
                editorStyle.richText = false;
            }
            var lib = Lib();

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
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Kütüphaneyi Aç", EditorStyles.toolbarButton)) { Selection.activeObject = lib; EditorGUIUtility.PingObject(lib); }
            EditorGUILayout.EndHorizontal();

            if (names.Length == 0)
            {
                EditorGUILayout.HelpBox("Hikaye yok. 'Yeni bölüm'e bas.", MessageType.Info);
                return;
            }

            // --- hizli ekleme (otomatik tamamlama yerine: sec ve imlecin oldugu satirin altina ekle) ---
            QuickBar(lib);

            // --- yazma alani ---
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            // GUILayout.TextArea: imlec konumu TextEditor durumundan okunabilsin diye (EditorGUILayout'un editoru gizli)
            var nt = GUILayout.TextArea(text, editorStyle, GUILayout.ExpandHeight(true));
            if (GUIUtility.keyboardControl != 0)
            {
                var te = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                if (te != null && te.text == nt)
                {
                    int ci = Mathf.Clamp(te.cursorIndex, 0, nt.Length), ln = 1;
                    cursorIdx = ci;
                    for (int q = 0; q < ci; q++) if (nt[q] == '\n') ln++;
                    if (ln != cursorLine) { cursorLine = ln; Repaint(); }
                }
            }
            EditorGUILayout.EndScrollView();
            if (nt != text) { text = nt; dirty = true; parsed = null; }

            // --- durum ---
            var p = Parsed();
            int ready = 0;
            string storyName = names[sel];
            if (lib != null)
                foreach (var it in p.Items) if (it.Type == ItemType.Dialogue && GeminiSes.HasClip(storyName, lib, it)) ready++;
            int secs = EstimateSeconds(p), words = WordCount(p);
            EditorGUILayout.LabelField("Konuşma: " + p.DialogueCount + "  ·  Ses: " + ready + "/" + p.DialogueCount +
                "  ·  Kelime: " + words + "  ·  Süre: ~" + (secs / 60) + ":" + (secs % 60).ToString("00") +
                "  ·  İmleç: " + cursorLine + ". satır", EditorStyles.miniBoldLabel);

            var warns = lib != null ? Check(lib) : new List<string>(p.Warnings);
            warns.AddRange(Checks(p, secs));
            if (warns.Count > 0)
            {
                warnScroll = EditorGUILayout.BeginScrollView(warnScroll, GUILayout.MaxHeight(80));
                EditorGUILayout.HelpBox(string.Join("\n", warns.ToArray()), MessageType.Warning);
                EditorGUILayout.EndScrollView();
            }

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

            // --- ses ---
            showVoice = EditorGUILayout.Foldout(showVoice, "Seslendirme (Gemini)", true);
            if (showVoice)
            {
                EditorGUI.indentLevel++;
                var key = EditorPrefs.GetString(GeminiSes.KeyPref, "");
                var nk = EditorGUILayout.PasswordField("API anahtarı", key);
                if (nk != key) EditorPrefs.SetString(GeminiSes.KeyPref, nk);
                var model = GeminiSes.Model;
                int mi = System.Array.IndexOf(GeminiSes.Models, model);
                int nmi = EditorGUILayout.Popup("Model", mi < 0 ? 0 : mi, GeminiSes.ModelLabels);
                if (nmi != mi) GeminiSes.Model = GeminiSes.Models[nmi];
                EditorGUI.indentLevel--;

                int missing = p.DialogueCount - ready;
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
                    GUI.enabled = !GeminiSes.Busy && GeminiSes.HasBackup(lib, storyName, cur);
                    if (GUILayout.Button("Eski sesi geri al", GUILayout.Width(120)))
                    { if (GeminiSes.RestoreBackup(lib, storyName, cur)) GeminiSes.Status = cursorLine + ". satırın eski sesi geri alındı."; }
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
                if (missing > 10)
                    EditorGUILayout.HelpBox("Ücretsiz kota günde ~10-11 satır: bu bölüm ~" + Mathf.CeilToInt(missing / 10f) +
                        " güne bölünür. Üretilen sesler saklanır; ertesi gün kalanlar üretilir.", MessageType.Info);
                if (!string.IsNullOrEmpty(GeminiSes.Status)) EditorGUILayout.HelpBox(GeminiSes.Status, MessageType.None);
            }

            // --- yardim ---
            showHelp = EditorGUILayout.Foldout(showHelp, "Yazım kılavuzu", true);
            if (showHelp) EditorGUILayout.HelpBox(Help, MessageType.None);
        }

        int EstimateSeconds(ParseResult p)
        {
            float s = 0;
            foreach (var it in p.Items)
            {
                switch (it.Type)
                {
                    case ItemType.Dialogue: s += Mathf.Max(1.2f, it.Text.Length / 14f + 0.3f) + 0.22f; break;
                    case ItemType.Hook: s += 1.8f; break;
                    case ItemType.Rewind: s += 1.6f; break;
                    case ItemType.Title: s += 2.5f; break;
                    case ItemType.Wait: s += it.Seconds; break;
                    case ItemType.Background: s += 0.6f; break;
                    case ItemType.Headlights: s += 1.5f; break;
                    case ItemType.End: s += it.Arg == "sert" ? 1.2f : 2.8f; break;
                    case ItemType.Choice: s += 4f; break;
                    case ItemType.Result: s += 2.2f; break;
                }
            }
            return Mathf.RoundToInt(s);
        }

        static readonly string[] Komutlar =
        {
            "Komut ekle…", "mekan: oda_gece", "poz: Arda yatiyor", "kanca: ", "geri: 1 saat önce", "efekt: kapi", "atmosfer: mum",
            "kamera: yakin", "muzik: pad", "bekle: 0.5", "cikar: ", "gorsel: eller", "yazi: ", "oyuncu: Arda = Timmy",
            "secim: A) ... / B) ...", "sonuc: %60 A", "son: yumusak Devamı yarın...", "son: sert", "seri: ", "bolum: 1", "baslik: "
        };

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
            int c2 = EditorGUILayout.Popup(0, Komutlar, EditorStyles.toolbarPopup, GUILayout.Width(110));
            if (c2 > 0) InsertLine(Komutlar[c2]);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // Imlecin bulundugu satirin sonuna yeni satir ekler
        void InsertLine(string line)
        {
            int at = cursorIdx < 0 || cursorIdx > text.Length ? text.Length : text.IndexOf('\n', cursorIdx);
            if (at < 0) at = text.Length;
            string prefix = text.Length == 0 ? "" : "\n";
            text = text.Insert(at, prefix + line);
            cursorIdx = at + prefix.Length + line.Length;
            dirty = true; parsed = null;
            GUI.FocusControl(null);
            Repaint();
        }

        static StoryItem LineItem(ParseResult p, int line)
        {
            foreach (var it in p.Items) if (it.Type == ItemType.Dialogue && it.Line == line) return it;
            return null;
        }

        static int WordCount(ParseResult p)
        {
            int n = 0;
            foreach (var it in p.Items)
                if (it.Type == ItemType.Dialogue || it.Type == ItemType.Hook)
                    n += it.Text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
            return n;
        }

        // Defterdeki kurallar: kanca ilk 1,5 sn'de, en az 1 dakika (Creator Rewards), bolum sonu
        static List<string> Checks(ParseResult p, int secs)
        {
            var w = new List<string>();
            float t = 0.35f; bool hookOk = false; bool any = false;
            foreach (var it in p.Items)
            {
                if (it.Type == ItemType.Background || it.Type == ItemType.Music || it.Type == ItemType.Atmosphere ||
                    it.Type == ItemType.Camera || it.Type == ItemType.Pose || it.Type == ItemType.Cast) { t += it.Type == ItemType.Background ? 0.3f : 0f; continue; }
                any = true;
                if (it.Type == ItemType.Hook || it.Type == ItemType.Dialogue || it.Type == ItemType.Sfx ||
                    it.Type == ItemType.Headlights || it.Type == ItemType.Flash || it.Type == ItemType.Result) hookOk = t <= 1.5f;
                break;
            }
            if (any && !hookOk) w.Add("Kanca: ilk 1,5 saniyede ekranda yazı, ses ya da hareket yok. En başa 'kanca:' ya da çarpıcı bir replik koy.");
            if (secs > 0 && secs < 60) w.Add("Süre ~" + secs + " sn: TikTok Creator Rewards en az 1 dakikalık videoya ödeme yapar.");
            bool hasEnd = false;
            foreach (var it in p.Items) if (it.Type == ItemType.End || it.Type == ItemType.Title) hasEnd = true;
            if (p.DialogueCount > 3 && !hasEnd) w.Add("Bölüm sonu yok: 'son: yumusak Devamı yarın...' ya da gerilimde kesmek için 'son: sert' ekle.");
            return w;
        }

        void PlayStory(bool record, int fromLine)
        {
            Save();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Launch(loadedPath, names[sel], record, fromLine);
        }

        // Uzaktan komut icin: hikaye adiyla oynat
        public static string PlayByName(string name, int fromLine, bool record)
        {
            if (EditorApplication.isPlaying) return "zaten oynuyor";
            string path = OtomatikKurulum.StoryFolder + "/" + name + ".txt";
            if (!File.Exists(path)) return "hikaye yok: " + path;
            if (record && !StudyoKayit.RecorderInstalled) return "Recorder kurulu değil";
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
            EditorApplication.isPlaying = true;
        }

        const string Help =
@"KONUŞMA
[Karakter|duygu|dis] Metin      dışarıdan konuşur, ağzı oynar
[Karakter|duygu|ic] Metin       iç ses: ağız kapalı, ekran hafif kararır
[Karakter|duygu|dis|hareket] … hareketle konuşur

Duygular: normal mutlu sevgi uzgun aglamakli korkmus sasirmis kizgin yorgun hasta dusunceli fisilti
Hareketler: kos yuru saril zipla egil sarsil cik

KOMUTLAR
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
                               Duygu/hareket → 3D animasyon tablosu: Kütüphane > animEsleme

SES ETİKETLERİ (altyazıda görünmez)
(iç çek) (ağla) (hıçkır) (gül) (kıkırda) (nefes) (nefesi kesil) (fısılda) (öksür) (kısa ara) (uzun ara)
ya da Gemini etiketleri: <sigh> <sob> <laugh> <gasp> <breath> <whispering> <short pause> <long pause>
*kelime*  vurgu işareti (altyazıda yıldız görünmez)
Örnek: [Anne|aglamakli|dis] Oğlum (hıçkır) gözlerini açtın!

VİDEOYA AL: Videolar/Hikaye/<bölüm>_<tarih>/ klasörüne video, kapak.png ve aciklama.txt çıkar;
kayıt sonunda ses yüksekliği (LUFS) ölçülür ve açıklamadaki notlara yazılır.

İPUCU: Duygusal satırlar ve iç ses otomatik yakın çekilir. Boşluk tuşu önizlemeyi duraklatır.";
    }
}
