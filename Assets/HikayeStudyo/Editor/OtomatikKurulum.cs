using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HikayeStudyo
{
    // Unity kodu derleyince calisir: kutuphane, ornek hikaye ve sahne yoksa kendisi olusturur,
    // ilk kurulumda sahneyi ve Studyo penceresini acar. Elle: Hikaye > Kurulumu Yenile
    public static class OtomatikKurulum
    {
        public const string LibPath = "Assets/Resources/StoryLibrary.asset";
        public const string ScenePath = "Assets/HikayeStudyo/HikayeSahnesi.unity";
        public const string StoryFolder = "Assets/Resources/Hikayeler";
        const string DoneKey = "HS_KurulumKontrol";

        [InitializeOnLoadMethod]
        static void Auto()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (SessionState.GetBool(DoneKey, false)) return;
                SessionState.SetBool(DoneKey, true);
                bool first = Ensure();
                if (first)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(ScenePath);
                    GameViewUtil.Portrait();
                    StudyoWindow.Open();
                    Debug.Log("[Hikaye Stüdyosu] Kurulum tamam. Stüdyo penceresinden hikayeni yaz ve ▶ Önizle'ye bas.");
                }
            };
        }

        [MenuItem("Hikaye/Kurulumu Yenile", priority = 20)]
        static void Manual()
        {
            Ensure();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
            GameViewUtil.Portrait();
            StudyoWindow.Open();
        }

        // Eksik olani olusturur. Yeni bir sey olustursa true doner.
        public static bool Ensure()
        {
            bool created = false;
            Folder("Assets/Resources");
            Folder(StoryFolder);
            Folder("Assets/Resources/Ses");
            Folder("Assets/HikayeStudyo");

            if (AssetDatabase.LoadAssetAtPath<StoryLibrary>(LibPath) == null) { CreateLibrary(); created = true; }
            if (!File.Exists(StoryFolder + "/ornek.txt"))
            {
                File.WriteAllText(StoryFolder + "/ornek.txt", OrnekHikaye, new System.Text.UTF8Encoding(false));
                AssetDatabase.ImportAsset(StoryFolder + "/ornek.txt");
                created = true;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) { CreateScene(); created = true; }
            return created;
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void CreateLibrary()
        {
            var lib = ScriptableObject.CreateInstance<StoryLibrary>();
            lib.narrator = new CharacterDef { id = "Anlatici", voice = "Orus", subtitleColor = new Color(0.82f, 0.9f, 1f) };

            lib.characters.Add(new CharacterDef
            {
                id = "Arda", voice = "Puck", innerVoice = "Orus", side = 0, scale = 0.72f,
                subtitleColor = Color.white,
                skin = new Color(1f, 0.84f, 0.7f), hair = new Color(0.2f, 0.12f, 0.08f),
                shirt = new Color(0.86f, 0.55f, 0.26f), pants = new Color(0.4f, 0.28f, 0.2f),
                hairStyle = HairStyle.Kase
            });
            lib.characters.Add(new CharacterDef
            {
                id = "Anne", voice = "Kore", side = -1, scale = 0.97f,
                subtitleColor = new Color(1f, 0.9f, 0.35f),
                skin = new Color(1f, 0.83f, 0.7f), hair = new Color(0.3f, 0.16f, 0.1f),
                shirt = new Color(0.56f, 0.3f, 0.55f), pants = new Color(0.45f, 0.24f, 0.44f),
                hairStyle = HairStyle.Uzun
            });
            lib.characters.Add(new CharacterDef
            {
                id = "Baba", voice = "Charon", side = 1, scale = 1.05f,
                subtitleColor = new Color(0.7f, 0.88f, 1f),
                skin = new Color(0.95f, 0.76f, 0.6f), hair = new Color(0.16f, 0.1f, 0.07f),
                shirt = new Color(0.3f, 0.46f, 0.36f), pants = new Color(0.3f, 0.24f, 0.2f),
                hairStyle = HairStyle.Kisa, beard = true
            });
            lib.characters.Add(new CharacterDef
            {
                id = "Adam", voice = "Orus", side = 0, scale = 1f,
                subtitleColor = new Color(0.85f, 0.85f, 0.85f),
                skin = new Color(0.96f, 0.78f, 0.64f), hair = new Color(0.25f, 0.22f, 0.2f),
                shirt = new Color(0.35f, 0.38f, 0.45f), pants = new Color(0.2f, 0.22f, 0.28f),
                hairStyle = HairStyle.Kisa
            });

            lib.backgrounds.Add(new BackgroundDef
            {
                id = "oda_gece", top = new Color(0.09f, 0.08f, 0.2f), bottom = new Color(0.24f, 0.17f, 0.3f),
                floor = new Color(0.33f, 0.22f, 0.17f), window = true, moon = true, bed = true, rug = true, atmosphere = "mum, toz"
            });
            lib.backgrounds.Add(new BackgroundDef
            {
                id = "oda_gunduz", top = new Color(0.96f, 0.86f, 0.7f), bottom = new Color(0.86f, 0.72f, 0.58f),
                floor = new Color(0.56f, 0.38f, 0.25f), window = true, bed = true, rug = true, atmosphere = "gunes"
            });
            lib.backgrounds.Add(new BackgroundDef
            {
                id = "sofra", top = new Color(0.35f, 0.22f, 0.14f), bottom = new Color(0.5f, 0.33f, 0.2f),
                floor = new Color(0.3f, 0.2f, 0.13f), window = true, moon = true, table = true, atmosphere = "mum"
            });
            lib.backgrounds.Add(new BackgroundDef
            {
                id = "yol_gece", top = new Color(0.02f, 0.03f, 0.08f), bottom = new Color(0.1f, 0.1f, 0.2f),
                floor = new Color(0.08f, 0.08f, 0.1f), road = true, atmosphere = "yagmur"
            });
            lib.backgrounds.Add(new BackgroundDef
            {
                id = "siyah", top = Color.black, bottom = Color.black, floor = Color.black
            });

            AssetDatabase.CreateAsset(lib, LibPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Hikaye Stüdyosu] Kütüphane oluşturuldu: " + LibPath);
        }

        static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            var player = new GameObject("StoryPlayer", typeof(StoryPlayer));
            player.GetComponent<StoryPlayer>().library = AssetDatabase.LoadAssetAtPath<StoryLibrary>(LibPath);
            player.GetComponent<StoryPlayer>().story = AssetDatabase.LoadAssetAtPath<TextAsset>(StoryFolder + "/ornek.txt");
            SceneManager.MoveGameObjectToScene(camGo, scene);
            SceneManager.MoveGameObjectToScene(player, scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            Debug.Log("[Hikaye Stüdyosu] Sahne oluşturuldu: " + ScenePath);
        }

        public const string OrnekHikaye =
@"// HİKAYE STÜDYOSU – ÖRNEK BÖLÜM
// Konuşma:  [Karakter|duygu|dis veya ic|hareket] Metin
// Komutlar: mekan: kanca: geri: yazi: bekle: efekt: kamera: atmosfer: muzik: poz: far: flas: gorsel: cikar: temizle:

// --- Kanca: ilk 2 saniye ---
mekan: yol_gece
efekt: korna
far:
efekt: carpma
kanca: Öldüm.
geri: 1 saat önce

// --- Bölüm ---
mekan: oda_gece
poz: Arda yatiyor
[Arda|sasirmis|ic] Burası neresi?
gorsel: eller
[Arda|korkmus|ic] Ellerim... Bu benim bedenim değil.
gorsel: yok
efekt: kapi
[Anne|aglamakli|dis|kos] Oğlum! Gözlerini açtın!
poz: Arda oturuyor
[Arda|normal|dis] Ağlama anne... İyiyim ben.
[Anne|sevgi|dis] Ateşin düştü... Şükürler olsun.
[Arda|dusunceli|ic] Şimdilik bir çocuk gibi davranmalıyım.
atmosfer: buyu
efekt: buyu
[Arda|sasirmis|ic] Bu dünyada... büyü gerçekten var.
yazi: Devamı 2. Bölümde...
";
    }
}
