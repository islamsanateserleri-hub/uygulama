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
            File.WriteAllText(p, "// Yeni bölüm\nmekan: oda_gece\n[Arda|normal|ic] ...\n", new System.Text.UTF8Encoding(false));
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

            // --- yazma alani ---
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            var nt = EditorGUILayout.TextArea(text, editorStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            if (nt != text) { text = nt; dirty = true; parsed = null; }

            // --- durum ---
            var p = Parsed();
            int ready = 0;
            string storyName = names[sel];
            if (lib != null)
                foreach (var it in p.Items) if (it.Type == ItemType.Dialogue && GeminiSes.HasClip(storyName, lib, it)) ready++;
            EditorGUILayout.LabelField("Konuşma satırı: " + p.DialogueCount + "   ·   Sesi hazır: " + ready + "/" + p.DialogueCount +
                "   ·   Tahmini süre: " + EstimateSeconds(p) + " sn", EditorStyles.miniBoldLabel);

            var warns = lib != null ? Check(lib) : p.Warnings;
            if (warns.Count > 0)
            {
                warnScroll = EditorGUILayout.BeginScrollView(warnScroll, GUILayout.MaxHeight(80));
                EditorGUILayout.HelpBox(string.Join("\n", warns.ToArray()), MessageType.Warning);
                EditorGUILayout.EndScrollView();
            }

            // --- oynat ---
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !EditorApplication.isPlaying;
            if (GUILayout.Button("▶  Önizle", GUILayout.Height(32))) PlayStory(false);
            if (StudyoKayit.RecorderInstalled)
            {
                if (GUILayout.Button("●  Videoya al (MP4)", GUILayout.Height(32))) PlayStory(true);
            }
            else if (GUILayout.Button("Video için Recorder'ı kur", GUILayout.Height(32))) StudyoKayit.InstallRecorder();
            GUI.enabled = true;
            if (EditorApplication.isPlaying && GUILayout.Button("■  Durdur", GUILayout.Height(32))) EditorApplication.isPlaying = false;
            EditorGUILayout.EndHorizontal();
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
                var model = EditorPrefs.GetString(GeminiSes.ModelPref, GeminiSes.DefaultModel);
                var nm = EditorGUILayout.TextField("Model", model);
                if (nm != model) EditorPrefs.SetString(GeminiSes.ModelPref, nm);
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
                }
            }
            return Mathf.RoundToInt(s);
        }

        void PlayStory(bool record)
        {
            Save();
            var lib = Lib();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            OtomatikKurulum.Ensure();
            if (SceneManager.GetActiveScene().path != OtomatikKurulum.ScenePath)
                EditorSceneManager.OpenScene(OtomatikKurulum.ScenePath);
            var player = FindAnyObjectByType<StoryPlayer>();
            if (player == null) player = new GameObject("StoryPlayer").AddComponent<StoryPlayer>();
            player.story = AssetDatabase.LoadAssetAtPath<TextAsset>(loadedPath);
            player.library = lib;
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
            EditorSceneManager.SaveScene(player.gameObject.scene);
            GameViewUtil.Portrait();
            SessionState.SetBool(StudyoKayit.Key, record);
            SessionState.SetString(StudyoKayit.NameKey, names[sel]);
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

İPUCU: Duygusal satırlar ve iç ses otomatik yakın çekilir. Boşluk tuşu önizlemeyi duraklatır.";
    }
}
