using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HikayeStudyo
{
    // 3D karakterleri kendiliginden hazirlar:
    //  1) .glb dosyalarini acmak icin glTFast paketini kurar
    //  2) Insan modellerine Unity insan iskeleti (Humanoid avatar) baglar; ortak 45 animasyonu hepsine verir
    //  3) Her karakter icin hazir prefab yapar (Assets/HikayeStudyo/Karakterler/Hazir)
    //  4) Karakter Galerisi sahnesini kurar
    // Menu: Hikaye > 3D Karakterleri Hazirla, Hikaye > Karakter Galerisi
    public static class Karakter3DKurulum
    {
        public const string Root = "Assets/HikayeStudyo/Karakterler";
        public const string Hazir = Root + "/Hazir";
        public const string UalPath = Root + "/Animasyonlar/UAL1_Standard.fbx";
        public const string GaleriPath = "Assets/HikayeStudyo/KarakterGalerisi.unity";
        public const string MixamoDir = Root + "/Mixamo";
        public const string MixamoAnimDir = Root + "/MixamoAnimasyon";
        const string InstallKey = "HS_GltfKuruluyor";
        const string AskedKey = "HS_GaleriSoruldu";
        static bool scheduled;

        public static bool GltfInstalled
        {
            get
            {
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
                    if (a.GetType("GLTFast.GltfImport", false) != null) return true;
                return false;
            }
        }

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            if (Application.isBatchMode) return;
            Schedule();
        }

        public static void Schedule()
        {
            if (scheduled) return;
            scheduled = true;
            EditorApplication.delayCall += () => { scheduled = false; Auto(); };
        }

        static void Auto()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!AssetDatabase.IsValidFolder(Root)) return;
            if (!GltfInstalled)
            {
                if (SessionState.GetBool(InstallKey, false)) return;
                SessionState.SetBool(InstallKey, true);
                Debug.Log("[Hikaye Stüdyosu] 3D karakterler için glTFast paketi kuruluyor (bir kez, internet gerekir)...");
                UnityEditor.PackageManager.Client.Add("com.unity.cloud.gltfast");
                return;
            }
            if (CountPrefabs() >= ExpectedCount()) return;
            int made = BuildAll(false);
            if (made > 0 && !SessionState.GetBool(AskedKey, false))
            {
                SessionState.SetBool(AskedKey, true);
                if (EditorUtility.DisplayDialog("Hikaye Stüdyosu",
                        made + " karakter hazır. Karakter Galerisi sahnesi açılsın mı? (Play'e basınca hepsi sırayla hareket eder)", "Aç", "Sonra"))
                    OpenGallery();
            }
        }

        static string[] Files(string folder, string ext)
        {
            if (!Directory.Exists(folder)) return new string[0];
            var list = new List<string>();
            foreach (var f in Directory.GetFiles(folder, "*" + ext)) list.Add(f.Replace('\\', '/'));
            list.Sort();
            return list.ToArray();
        }

        static int ExpectedCount()
        {
            return Files(Root + "/Insanlar/Erkek", ".glb").Length + Files(Root + "/Insanlar/Kadin", ".glb").Length +
                   Files(Root + "/Hayvanlar", ".glb").Length + Files(Root + "/Oyuncak", ".glb").Length +
                   Files(Root + "/Oyuncak", ".fbx").Length + Files(MixamoDir, ".fbx").Length;
        }

        static int CountPrefabs() { return Files(Hazir, ".prefab").Length; }

        [MenuItem("Hikaye/3D Karakterleri Hazırla", priority = 40)]
        static void Menu()
        {
            if (!GltfInstalled)
            {
                UnityEditor.PackageManager.Client.Add("com.unity.cloud.gltfast");
                EditorUtility.DisplayDialog("Hikaye Stüdyosu", "glTFast paketi kuruluyor. Kurulum bitince karakterler kendiliğinden hazırlanacak.", "Tamam");
                return;
            }
            int n = BuildAll(true);
            EditorUtility.DisplayDialog("Hikaye Stüdyosu", n + " karakter hazırlandı:\n" + Hazir, "Tamam");
        }

        [MenuItem("Hikaye/Karakter Galerisi", priority = 41)]
        public static void OpenGallery()
        {
            if (CountPrefabs() == 0) { Menu(); if (CountPrefabs() == 0) return; }
            BuildGalleryScene();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(GaleriPath);
            GameViewUtil.Portrait();
        }

        // ---------- Karakterler ----------
        public static int BuildAll(bool rebuild)
        {
            Folder(Hazir);
            var ual = Clips(UalPath);
            if (ual.Count == 0) Debug.LogWarning("[Hikaye Stüdyosu] Animasyon kütüphanesi okunamadı: " + UalPath);
            // Mixamo'dan indirilen hikaye animasyonlari (aglama, yatma, uyuma, dusunme...) butun insanlara eklenir
            foreach (var p in Files(MixamoAnimDir, ".fbx"))
                foreach (var c in Clips(p)) if (c.name != "Take 001") ual.Add(c);
            int made = 0;
            try
            {
                foreach (var p in Files(Root + "/Insanlar/Erkek", ".glb")) if (Human(p, "Erkek", ual, rebuild)) made++;
                foreach (var p in Files(Root + "/Insanlar/Kadin", ".glb")) if (Human(p, "Kadin", ual, rebuild)) made++;
                foreach (var p in Files(Root + "/Hayvanlar", ".glb")) if (Own(p, "Hayvan", rebuild)) made++;
                foreach (var p in Files(Root + "/Oyuncak", ".glb")) if (Own(p, "Oyuncak", rebuild)) made++;
                foreach (var p in Files(Root + "/Oyuncak", ".fbx")) if (Own(p, "Oyuncak", rebuild)) made++;
                foreach (var p in Files(MixamoDir, ".fbx")) if (MixamoChar(p, ual, rebuild)) made++;
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (made > 0) Debug.Log("[Hikaye Stüdyosu] " + made + " 3D karakter hazırlandı: " + Hazir);
            return made;
        }

        static List<AnimationClip> Clips(string path)
        {
            var list = new List<AnimationClip>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var c = o as AnimationClip;
                if (c != null && !c.name.StartsWith("__preview__")) list.Add(c);
            }
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list;
        }

        static string PrefabName(string kat, string path)
        {
            var n = Path.GetFileNameWithoutExtension(path).Replace("blocky_", "");
            if (n.Length > 3 && char.IsDigit(n[0]) && char.IsDigit(n[1]) && n[2] == '_') n = n.Substring(3);
            return kat + "_" + char.ToUpperInvariant(n[0]) + n.Substring(1);
        }

        static bool Human(string path, string kat, List<AnimationClip> ual, bool rebuild)
        {
            string name = PrefabName(kat, path);
            string prefabPath = Hazir + "/" + name + ".prefab";
            if (!rebuild && File.Exists(prefabPath)) return false;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (src == null) { Debug.LogWarning("[Hikaye Stüdyosu] Henüz içe aktarılmadı: " + path); return false; }
            var go = (GameObject)Object.Instantiate(src);
            go.name = name;
            try
            {
                var avatar = BuildAvatar(go);
                if (avatar == null) return false;
                avatar.name = name + "_Avatar";
                string avPath = Hazir + "/" + name + "_Avatar.asset";
                AssetDatabase.DeleteAsset(avPath);
                AssetDatabase.CreateAsset(avatar, avPath);
                foreach (var a in go.GetComponentsInChildren<Animation>(true)) Object.DestroyImmediate(a);
                foreach (var a in go.GetComponentsInChildren<Animator>(true)) if (a.gameObject != go) Object.DestroyImmediate(a);
                var an = go.GetComponent<Animator>();
                if (an == null) an = go.AddComponent<Animator>();
                an.avatar = avatar;
                an.applyRootMotion = false;
                an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var k = go.GetComponent<Karakter3D>();
                if (k == null) k = go.AddComponent<Karakter3D>();
                k.id = name; k.kategori = kat; k.clips = ual; k.startClip = "Idle_Loop";
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Hikaye Stüdyosu] " + name + " hazırlanamadı: " + e.Message);
                return false;
            }
            finally { Object.DestroyImmediate(go); }
        }

        static bool Own(string path, string kat, bool rebuild)
        {
            string name = PrefabName(kat, path);
            string prefabPath = Hazir + "/" + name + ".prefab";
            if (!rebuild && File.Exists(prefabPath)) return false;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (src == null) { Debug.LogWarning("[Hikaye Stüdyosu] Henüz içe aktarılmadı: " + path); return false; }
            var go = (GameObject)Object.Instantiate(src);
            go.name = name;
            try
            {
                var clips = Clips(path);
                bool legacy = clips.Count > 0 && clips[0].legacy;
                if (!legacy)
                {
                    var an = go.GetComponentInChildren<Animator>();
                    if (an == null) an = go.AddComponent<Animator>();
                    an.applyRootMotion = false;
                    an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
                var k = go.AddComponent<Karakter3D>();
                k.id = name; k.kategori = kat; k.clips = clips;
                k.startClip = "Idle";
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Hikaye Stüdyosu] " + name + " hazırlanamadı: " + e.Message);
                return false;
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Mixamo karakteri: FBX zaten insan iskeletiyle ice aktarilir (Mixamo kemik adlarini Unity kendisi tanir)
        static bool MixamoChar(string path, List<AnimationClip> clips, bool rebuild)
        {
            string name = "Mixamo_" + Path.GetFileNameWithoutExtension(path).Replace(' ', '_');
            string prefabPath = Hazir + "/" + name + ".prefab";
            if (!rebuild && File.Exists(prefabPath)) return false;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (src == null) return false;
            Avatar avatar = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Avatar) avatar = (Avatar)o;
            if (avatar == null || !avatar.isHuman) { Debug.LogWarning("[Hikaye Stüdyosu] " + name + ": insan iskeleti yok, atlandı."); return false; }
            var go = (GameObject)Object.Instantiate(src);
            go.name = name;
            try
            {
                var an = go.GetComponent<Animator>();
                if (an == null) an = go.AddComponent<Animator>();
                an.avatar = avatar;
                an.applyRootMotion = false;
                an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var k = go.AddComponent<Karakter3D>();
                k.id = name; k.kategori = "Mixamo"; k.clips = clips; k.startClip = "Idle_Loop";
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Hikaye Stüdyosu] " + name + " hazırlanamadı: " + e.Message);
                return false;
            }
            finally { Object.DestroyImmediate(go); }
        }

        // Quaternius insan iskeleti -> Unity insan kemikleri
        static readonly HumanBodyBones[] HB =
        {
            HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest,
            HumanBodyBones.Neck, HumanBodyBones.Head,
            HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
            HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
            HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
        };
        static readonly string[] QB =
        {
            "Hips", "Abdomen", "Torso", "Chest",
            "Neck", "Head",
            "Shoulder.L", "UpperArm.L", "LowerArm.L", "Hand.L",
            "Shoulder.R", "UpperArm.R", "LowerArm.R", "Hand.R",
            "UpperLeg.L", "LowerLeg.L", "Foot.L",
            "UpperLeg.R", "LowerLeg.R", "Foot.R",
        };

        static Avatar BuildAvatar(GameObject go)
        {
            var all = go.GetComponentsInChildren<Transform>(true);
            var byName = new Dictionary<string, Transform>();
            foreach (var t in all)
            {
                if (byName.ContainsKey(t.name) && t != go.transform) t.name = t.name + "_" + t.GetInstanceID();   // ayni ad olmasin
                byName[t.name] = t;
            }
            var human = new List<HumanBone>();
            for (int i = 0; i < HB.Length; i++)
            {
                if (!byName.ContainsKey(QB[i])) continue;
                var lim = new HumanLimit();
                lim.useDefaultValues = true;
                var hb = new HumanBone();
                hb.humanName = HumanTrait.BoneName[(int)HB[i]];
                hb.boneName = QB[i];
                hb.limit = lim;
                human.Add(hb);
            }
            if (human.Count < 15) { Debug.LogWarning("[Hikaye Stüdyosu] " + go.name + ": insan iskeleti bulunamadı (" + human.Count + " kemik)."); return null; }

            var skel = new SkeletonBone[all.Length];
            for (int i = 0; i < all.Length; i++)
                skel[i] = new SkeletonBone { name = all[i].name, position = all[i].localPosition, rotation = all[i].localRotation, scale = all[i].localScale };

            var desc = new HumanDescription
            {
                human = human.ToArray(), skeleton = skel,
                upperArmTwist = 0.5f, lowerArmTwist = 0.5f, upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
                armStretch = 0.05f, legStretch = 0.05f, feetSpacing = 0f, hasTranslationDoF = false
            };
            var avatar = AvatarBuilder.BuildHumanAvatar(go, desc);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                Debug.LogWarning("[Hikaye Stüdyosu] " + go.name + ": insan iskeleti kurulamadı.");
                return null;
            }
            return avatar;
        }

        // ---------- Galeri sahnesi ----------
        static void BuildGalleryScene()
        {
            var before = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);   // isik ayarlari galeri sahnesine yazilsin

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.2f, 0.27f);
            cam.fieldOfView = 40f;
            camGo.transform.position = new Vector3(0, 1.35f, 6.8f);
            camGo.transform.LookAt(new Vector3(0, 0.95f, 0));

            var lightGo = new GameObject("Gunes", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.shadows = LightShadows.Soft;
            light.color = new Color(1f, 0.95f, 0.88f);
            lightGo.transform.rotation = Quaternion.Euler(45f, 200f, 0f);

            var fillGo = new GameObject("Dolgu", typeof(Light));
            var fill = fillGo.GetComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.45f;
            fill.color = new Color(0.7f, 0.8f, 1f);
            fillGo.transform.rotation = Quaternion.Euler(20f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.18f, 0.16f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Zemin";
            ground.transform.localScale = new Vector3(3, 1, 3);
            var mat = ZeminMateryali();
            if (mat != null) ground.GetComponent<Renderer>().sharedMaterial = mat;

            var g = new GameObject("Galeri", typeof(GaleriOynatici));
            var gal = g.GetComponent<GaleriOynatici>();
            gal.aralik = 0.95f;
            gal.karakterler.Clear();
            foreach (var p in Files(Hazir, ".prefab"))
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (pf != null) gal.karakterler.Add(pf);
            }

            foreach (var o in new[] { camGo, lightGo, fillGo, ground, g }) SceneManager.MoveGameObjectToScene(o, scene);
            AssetDatabase.DeleteAsset(GaleriPath);
            EditorSceneManager.SaveScene(scene, GaleriPath);
            if (before.IsValid()) SceneManager.SetActiveScene(before);
            EditorSceneManager.CloseScene(scene, true);
        }

        static Material ZeminMateryali()
        {
            string p = "Assets/HikayeStudyo/Zemin.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null) return m;
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            if (sh == null) return null;
            m = new Material(sh);
            var c = new Color(0.42f, 0.38f, 0.34f);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            AssetDatabase.CreateAsset(m, p);
            return m;
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }

    // Animasyon kutuphanesini insan iskeletiyle, oyuncak karakterleri kendi animasyonlariyla ice aktarir.
    public class Karakter3DIceAktarma : AssetPostprocessor
    {
        bool Ual
        {
            get
            {
                var p = assetPath.Replace('\\', '/');
                return p.Contains("/Karakterler/Animasyonlar/") || p.Contains("/Karakterler/Mixamo/") || p.Contains("/Karakterler/MixamoAnimasyon/");
            }
        }
        bool MixAnim { get { return assetPath.Replace('\\', '/').Contains("/Karakterler/MixamoAnimasyon/"); } }
        bool Toy { get { var p = assetPath.Replace('\\', '/'); return p.Contains("/Karakterler/Oyuncak/") && p.ToLowerInvariant().EndsWith(".fbx"); } }

        void OnPreprocessModel()
        {
            if (!Ual && !Toy) return;
            var imp = (ModelImporter)assetImporter;
            imp.importAnimation = true;
            imp.animationType = Ual ? ModelImporterAnimationType.Human : ModelImporterAnimationType.Generic;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }

        void OnPreprocessAnimation()
        {
            if (!Ual && !Toy) return;
            var imp = (ModelImporter)assetImporter;
            var clips = imp.defaultClipAnimations;
            if (MixAnim)
            {
                // Mixamo her dosyada klibi "mixamo.com" diye adlandirir: dosya adini ver ("Sad Idle" -> "Sad_Idle")
                var keep = new List<ModelImporterClipAnimation>();
                foreach (var c in clips) if (c.name != "Take 001") keep.Add(c);
                if (keep.Count == 0 && clips.Length > 0) keep.Add(clips[0]);
                if (keep.Count > 0) keep[0].name = Path.GetFileNameWithoutExtension(assetPath).Replace(' ', '_').Replace("(", "").Replace(")", "");
                clips = keep.ToArray();
            }
            foreach (var c in clips)
            {
                var n = c.name.ToLowerInvariant();
                c.loopTime = n.Contains("loop") || n == "idle" || n == "walk" || n == "run" || n == "dance" ||
                             (MixAnim && (n.Contains("idle") || n.StartsWith("walking") || n == "running" || n.Contains("talking") ||
                                          n.Contains("sleeping") || n.Contains("crying") || n.Contains("driving") || n.Contains("pole")));
                c.lockRootRotation = true;
                c.lockRootHeightY = true;
                c.lockRootPositionXZ = true;
                c.keepOriginalOrientation = true;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            imp.clipAnimations = clips;
        }

        // Yeni .glb / .fbx karakter ice aktarilinca hazirligi tekrar dene
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var p in imported)
            {
                var q = p.Replace('\\', '/');
                if (q.StartsWith(Karakter3DKurulum.Root) && !q.StartsWith(Karakter3DKurulum.Hazir) &&
                    (q.EndsWith(".glb") || q.EndsWith(".fbx")))
                {
                    Karakter3DKurulum.Schedule();
                    return;
                }
            }
        }
    }
}
