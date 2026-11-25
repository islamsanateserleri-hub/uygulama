using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public const string VrmDir = Root + "/VRM";
        public const string RocketboxDir = Root + "/Rocketbox";
        const string InstallKey = "HS_GltfKuruluyor";
        const string AskedKey = "HS_GaleriSoruldu";
        static bool scheduled;

        public static bool GltfInstalled
        {
            get
            {
                // AppDomain.GetAssemblies() yerine AsmYardim (CompilationPipeline tabanli, UAC0005 uyarisi cikmaz)
                return AsmYardim.TipVar("GLTFast.GltfImport");
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
                   Files(Root + "/Oyuncak", ".fbx").Length + Files(MixamoDir, ".fbx").Length + Files(VrmDir, ".vrm").Length + RocketboxModelleri().Count;
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
        static List<AnimationClip> OrtakKlipler()
        {
            var ual = Clips(UalPath);
            if (ual.Count == 0) Debug.LogWarning("[Hikaye Stüdyosu] Animasyon kütüphanesi okunamadı: " + UalPath);
            // Mixamo'dan indirilen hikaye animasyonlari (aglama, yatma, uyuma, dusunme...) butun insanlara eklenir
            foreach (var p in Files(MixamoAnimDir, ".fbx"))
                foreach (var c in Clips(p)) if (c.name != "Take 001") ual.Add(c);
            return ual;
        }

        // Yalnizca gercekci (Rocketbox) karakterleri kurar; ayni yola yazildigi icin prefab kimlikleri degismez
        public static int GercekKur(bool rebuild)
        {
            Folder(Hazir);
            var ual = OrtakKlipler();
            int made = 0;
            try { foreach (var p in RocketboxModelleri()) if (RocketboxChar(p, ual, rebuild)) made++; }
            finally { AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); }
            return made;
        }

        public static int BuildAll(bool rebuild)
        {
            Folder(Hazir);
            var ual = OrtakKlipler();
            int made = 0;
            try
            {
                foreach (var p in Files(Root + "/Insanlar/Erkek", ".glb")) if (Human(p, "Erkek", ual, rebuild)) made++;
                foreach (var p in Files(Root + "/Insanlar/Kadin", ".glb")) if (Human(p, "Kadin", ual, rebuild)) made++;
                foreach (var p in Files(Root + "/Hayvanlar", ".glb")) if (Own(p, "Hayvan", rebuild)) made++;
                foreach (var p in Files(Root + "/Oyuncak", ".glb")) if (Own(p, "Oyuncak", rebuild)) made++;
                foreach (var p in Files(Root + "/Oyuncak", ".fbx")) if (Own(p, "Oyuncak", rebuild)) made++;
                foreach (var p in Files(MixamoDir, ".fbx")) if (MixamoChar(p, ual, rebuild)) made++;
                foreach (var p in Files(VrmDir, ".vrm")) if (VrmChar(p, ual, rebuild)) made++;
                foreach (var p in RocketboxModelleri()) if (RocketboxChar(p, ual, rebuild)) made++;
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
                if (avatar == null)
                {
                    RigRapor(name + ": Humanoid avatar kurulamadı, prefab OLUŞTURULMADI (ayrıntı yukarıdaki satırlarda / HikayeKomut/rig_raporu.txt).", true);
                    return false;
                }
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

        // VRoid Studio'dan dışa aktarılan .vrm dosyaları proje kökündeki "VRoid" klasöründen alınır.
        // Köprüden: menu: Hikaye/VRM Karakterleri Ekle  ->  HikayeKomut/vrm_ekle.txt
        [MenuItem("Hikaye/VRM Karakterleri Ekle", priority = 42)]
        public static void VrmEkle()
        {
            var rapor = Path.Combine(UzaktanKomut.Dir, "vrm_ekle.txt");
            var sb = new StringBuilder();
            Folder(VrmDir);
            int kopya = 0;
            foreach (var f in Files("VRoid", ".vrm"))
            {
                var hedef = VrmDir + "/" + Path.GetFileName(f);
                if (File.Exists(hedef) && new FileInfo(hedef).Length == new FileInfo(f).Length) continue;
                File.Copy(f, hedef, true);
                kopya++;
            }
            AssetDatabase.Refresh();
            int made = BuildAll(false);
            sb.AppendLine("kopyalanan=" + kopya + " yeni_prefab=" + made);
            foreach (var p in Files(VrmDir, ".vrm"))
            {
                var pf = AssetDatabase.LoadAssetAtPath<GameObject>(Hazir + "/" + VrmAdi(p) + ".prefab");
                if (pf == null) { sb.AppendLine(VrmAdi(p) + ": PREFAB YOK"); continue; }
                var an = pf.GetComponent<Animator>();
                int sekil = 0;
                var golgeler = new SortedDictionary<string, int>();
                foreach (var r in pf.GetComponentsInChildren<Renderer>(true))
                {
                    var smr = r as SkinnedMeshRenderer;
                    if (smr != null && smr.sharedMesh != null) sekil += smr.sharedMesh.blendShapeCount;
                    foreach (var m in r.sharedMaterials)
                    {
                        var ad = m == null || m.shader == null ? "(malzeme yok)" : m.shader.name;
                        int n; golgeler.TryGetValue(ad, out n); golgeler[ad] = n + 1;
                    }
                }
                sb.Append(pf.name).Append(": insan=").Append(an != null && an.isHuman ? "EVET" : "hayir")
                  .Append(" blendshape=").Append(sekil).Append(" golgelendirici:");
                foreach (var kv in golgeler) sb.Append(' ').Append(kv.Key).Append('=').Append(kv.Value);
                sb.AppendLine();
            }
            KatalogKurulum.Guncelle();
            Directory.CreateDirectory(UzaktanKomut.Dir);
            File.WriteAllText(rapor, sb.ToString(), new UTF8Encoding(false));
            Debug.Log("[Hikaye Stüdyosu] VRM karakterleri: " + sb);
        }

        static string VrmAdi(string path) { return "VRM_" + Path.GetFileNameWithoutExtension(path).Replace(' ', '_'); }

        static bool VrmChar(string path, List<AnimationClip> clips, bool rebuild)
        {
            string name = VrmAdi(path);
            string prefabPath = Hazir + "/" + name + ".prefab";
            if (!rebuild && File.Exists(prefabPath)) return false;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (src == null) { Debug.LogWarning("[Hikaye Stüdyosu] VRM içe aktarılmadı (UniVRM kurulu mu?): " + path); return false; }
            var go = (GameObject)Object.Instantiate(src);
            go.name = name;
            try
            {
                var an = go.GetComponent<Animator>();
                if (an == null || an.avatar == null || !an.avatar.isHuman)
                {
                    RigRapor(name + ": VRM içinde Humanoid avatar yok, prefab atlandı.", true);
                    return false;
                }
                // Vrm10Instance her karede yüz ağırlıklarını sıfırlar; yüzü YuzCanlilik oynattığı için kaldırılır
                // (bedeli: saç/etek sallanma fiziği de kapanır).
                foreach (var c in go.GetComponentsInChildren<MonoBehaviour>(true))
                    if (c != null && c.GetType().Name == "Vrm10Instance") Object.DestroyImmediate(c);
                an.applyRootMotion = false;
                an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var k = go.AddComponent<Karakter3D>();
                k.id = name; k.kategori = "VRM"; k.clips = clips; k.startClip = "Idle_Loop";
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
            if (avatar == null || !avatar.isHuman)
            {
                RigRapor(name + ": FBX içe aktarımında Humanoid avatar yok (" + (avatar == null ? "Avatar nesnesi hiç oluşmamış" : "Avatar.isHuman=false, isValid=" + avatar.isValid) +
                         "). " + path + " dosyasının Import Settings > Rig > Animation Type = Humanoid olmalı ve Configure'da kemikler yeşil olmalı. Prefab atlandı.", true);
                return false;
            }
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
                MixamoMalzemeler(go, path);
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

        // ---------- Gercekci insanlar (Microsoft Rocketbox, MIT lisansi) ----------
        // Her karakter Karakterler/Rocketbox/<ad>/ altinda: <ad>_facial.fbx + .tga dokular
        public static List<string> RocketboxModelleri()
        {
            var l = new List<string>();
            if (!Directory.Exists(RocketboxDir)) return l;
            foreach (var d in Directory.GetDirectories(RocketboxDir))
                l.AddRange(Files(d.Replace('\\', '/'), ".fbx"));
            l.Sort();
            return l;
        }

        // "Female_Adult_01" -> "Gercek_Kadin01", "Police_Male_02" -> "Gercek_PolisErkek02"
        static readonly string[,] RocketboxTr =
        {
            { "Female_Adult_", "Kadin" }, { "Male_Adult_", "Erkek" }, { "Female_Child_", "KizCocuk" }, { "Male_Child_", "ErkekCocuk" },
            { "Female_Party_", "PartiKadin" }, { "Business_", "IsInsani" }, { "Chef_", "Asci" }, { "Construction_", "Insaat" },
            { "Delivery_", "Kurye" }, { "Fire_", "Itfaiye" }, { "Gardener_", "Bahcivan" }, { "Medical_", "Saglik" },
            { "Military_", "Asker" }, { "Pilot_", "Pilot" }, { "Police_", "Polis" }, { "Security_", "Guvenlik" },
            { "Sports_", "Sporcu" }, { "Wood_", "Oduncu" }, { "Female_", "Kadin" }, { "Male_", "Erkek" },
        };

        public static string RocketboxAdi(string klasorAdi)
        {
            var n = klasorAdi;
            for (int i = 0; i < RocketboxTr.GetLength(0); i++) n = n.Replace(RocketboxTr[i, 0], RocketboxTr[i, 1]);
            return "Gercek_" + n.Replace("_", "").Replace(' ', '_');
        }

        static bool RocketboxChar(string path, List<AnimationClip> clips, bool rebuild)
        {
            string klasor = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = RocketboxAdi(Path.GetFileName(klasor));
            string prefabPath = Hazir + "/" + name + ".prefab";
            if (!rebuild && File.Exists(prefabPath)) return false;
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (src == null) return false;
            Avatar avatar = null;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Avatar) avatar = (Avatar)o;
            if (avatar == null || !avatar.isHuman || !avatar.isValid)
            {
                RigRapor(name + ": FBX içe aktarımında geçerli Humanoid avatar yok, prefab atlandı: " + path, true);
                return false;
            }
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
                k.id = name; k.kategori = "Gercek"; k.clips = clips; k.startClip = "Idle_Loop";
                RocketboxMalzemeler(go, klasor, name);
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

        // Doku adindan parcayi cikar: "f110_head_color_a" -> parca "head", tur "color", ek "_a"
        static readonly System.Text.RegularExpressions.Regex RbDoku =
            new System.Text.RegularExpressions.Regex(@"^(?:[a-z]+\d+_)?(?:(.*?)_)?(color|normal)((?:_[a-z]+)?)$");

        // FBX malzeme adi ("f110_head") ile ayni parcanin dokulari. Eksiz dosya varsa o, yoksa ekli olan (_a, _acu, _blue).
        static string RocketboxDokusu(string[] dokular, string parca, string tur)
        {
            string ekli = null, tek = null; int turSayisi = 0;
            foreach (var d in dokular)
            {
                var e = RbDoku.Match(Path.GetFileNameWithoutExtension(d).ToLowerInvariant());
                if (!e.Success || e.Groups[2].Value != tur) continue;
                turSayisi++; tek = d;
                if (e.Groups[1].Value != parca) continue;
                if (e.Groups[3].Value.Length == 0) return d;
                if (ekli == null) ekli = d;
            }
            return ekli != null ? ekli : turSayisi == 1 ? tek : null;
        }

        // FBX malzemeleri -> ayni adli dokularla URP Lit (govde, bas, sac/kirpik, kask, techizat ...)
        static void RocketboxMalzemeler(GameObject go, string klasor, string name)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;
            var dokular = Files(klasor, ".tga");
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    string mname = mats[i] != null ? mats[i].name : r.name;
                    string low = mname.ToLowerInvariant().Trim().Replace(' ', '_').Replace('/', '_').Replace(':', '_');
                    var onEk = System.Text.RegularExpressions.Regex.Match(low, @"^[a-z]+\d+(?=_|$)").Value;   // "f110"
                    string parca = System.Text.RegularExpressions.Regex.Replace(low, @"^[a-z]+\d+(_|$)([a-z]_)?", "");
                    parca = parca.Replace("equpiment", "equipment");   // kaynak dosyadaki yazim hatasi
                    // ayni klasorde iki ayri takim doku olabilir: once malzemenin on ekiyle baslayanlar
                    var kendi = System.Array.FindAll(dokular, d => onEk.Length > 0 && Path.GetFileName(d).ToLowerInvariant().StartsWith(onEk + "_"));
                    var aday = kendi.Length > 0 ? kendi : dokular;
                    string renk = RocketboxDokusu(aday, parca, "color"), normal = RocketboxDokusu(aday, parca, "normal");
                    bool camli = false;
                    if (renk == null) { renk = RocketboxDokusu(aday, parca + "_opacity", "color"); camli = renk != null; }   // gozluk
                    if (renk == null) { RigRapor(name + ": renk dokusu bulunamadı (malzeme " + mname + ")", true); continue; }
                    string matPath = klasor + "/" + name + "_" + (parca.Length > 0 ? parca : "govde") + ".mat";
                    var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, matPath); }
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(renk);
                    m.SetTexture("_BaseMap", tex); m.SetTexture("_MainTex", tex);
                    m.SetColor("_BaseColor", Color.white); m.SetColor("_Color", Color.white);
                    m.SetFloat("_Smoothness", 0.2f); m.SetFloat("_Metallic", 0f);
                    if (normal != null)
                    {
                        var ti = AssetImporter.GetAtPath(normal) as TextureImporter;
                        if (ti != null && ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport(); }
                        m.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal)); m.EnableKeyword("_NORMALMAP");
                    }
                    bool cut = camli || parca.Contains("opacity") || parca.Contains("hair") || parca.Contains("lash");   // saydam yerler kesilir, iki yuzlu
                    m.SetFloat("_AlphaClip", cut ? 1f : 0f); m.SetFloat("_Cutoff", 0.35f);
                    if (cut) m.EnableKeyword("_ALPHATEST_ON"); else m.DisableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Cull", cut ? 0f : 2f);
                    m.renderQueue = cut ? (int)RenderQueue.AlphaTest : -1;
                    EditorUtility.SetDirty(m);
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
            }
        }

        // Mixamo FBX'teki gomulu dokular cogu zaman malzemeye baglanmiyor (beyaz karakter).
        // Dokulari disari cikarir, her malzeme icin URP Lit malzeme yapip karaktere takar.
        static void MixamoMalzemeler(GameObject go, string path)
        {
            var dir = Path.GetDirectoryName(path).Replace('\\', '/') + "/Dokular/" + Path.GetFileNameWithoutExtension(path);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                Directory.CreateDirectory(dir);
                if (imp != null) { try { imp.ExtractTextures(dir); } catch (System.Exception e) { Debug.LogWarning(e.Message); } }
                AssetDatabase.Refresh();
            }
            if (!Directory.Exists(dir)) return;
            var files = Directory.GetFiles(dir);
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var src = mats[i];
                    string mname = src != null ? src.name : r.name;
                    string matPath = dir + "/" + mname.Replace(':', '_').Replace('/', '_') + "_URP.mat";
                    var m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (m == null) { m = new Material(lit); AssetDatabase.CreateAsset(m, matPath); }
                    m.shader = lit;
                    string diff = Karakter3DIceAktarma.En(files, mname, "diffuse"), nrm = Karakter3DIceAktarma.En(files, mname, "normal");
                    var tex = diff != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(diff) : null;
                    if (tex == null && src != null && src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex") as Texture2D;
                    if (tex != null) { m.SetTexture("_BaseMap", tex); m.SetTexture("_MainTex", tex); }
                    m.SetColor("_BaseColor", Color.white); m.SetColor("_Color", Color.white);
                    m.SetFloat("_Smoothness", 0.25f); m.SetFloat("_Metallic", 0f);
                    var low = (mname + " " + (diff ?? "")).ToLowerInvariant();
                    bool cut = low.Contains("hair") || low.Contains("lash") || low.Contains("brow");
                    m.SetFloat("_AlphaClip", cut ? 1f : 0f); m.SetFloat("_Cutoff", 0.5f);
                    if (cut) m.EnableKeyword("_ALPHATEST_ON"); else m.DisableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Cull", cut ? 0f : 2f);
                    var nt = nrm != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(nrm) : null;
                    if (nt != null)
                    {
                        var ti = AssetImporter.GetAtPath(nrm) as TextureImporter;
                        if (ti != null && ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport(); nt = AssetDatabase.LoadAssetAtPath<Texture2D>(nrm); }
                        m.SetTexture("_BumpMap", nt); m.EnableKeyword("_NORMALMAP");
                    }
                    EditorUtility.SetDirty(m);
                    mats[i] = m;
                }
                r.sharedMaterials = mats;
            }
            AssetDatabase.SaveAssets();
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

        // ---------- Insan iskeleti (Humanoid avatar) ----------
        // Unity'nin Humanoid avatar icin zorunlu kildigi 15 kemik
        static readonly HumanBodyBones[] Gerekli =
        {
            HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Head,
            HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
            HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
            HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
            HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
        };

        // Konsola ve HikayeKomut/rig_raporu.txt dosyasina yazar (yapay zeka da okuyabilsin)
        public static void RigRapor(string mesaj, bool uyari)
        {
            if (uyari) Debug.LogWarning("[Hikaye Stüdyosu] " + mesaj); else Debug.Log("[Hikaye Stüdyosu] " + mesaj);
            try
            {
                Directory.CreateDirectory(UzaktanKomut.Dir);
                File.AppendAllText(Path.Combine(UzaktanKomut.Dir, "rig_raporu.txt"),
                    System.DateTime.Now.ToString("HH:mm:ss") + " " + mesaj + "\n", new UTF8Encoding(false));
            }
            catch { }
        }

        // Kemik adini karsilastirma icin sadelestirir: "mixamorig:LeftUpLeg" -> "leftupleg", "UpperArm.L" -> "upperarml"
        static string Nr(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            int i = Mathf.Max(name.LastIndexOf(':'), name.LastIndexOf('|'));
            if (i >= 0) name = name.Substring(i + 1);
            var sb = new StringBuilder(name.Length);
            foreach (char ch in name) if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            string s = sb.ToString();
            string[] onekler = { "mixamorig", "bip001", "bip01", "ccbase", "armature" };
            foreach (var pre in onekler)
                if (s.StartsWith(pre) && s.Length > pre.Length + 1) { s = s.Substring(pre.Length); break; }
            return s;
        }

        static List<string> Yanli(string[] bazlar, bool sol)
        {
            var l = new List<string>();
            string[] w = sol ? new[] { "left", "l" } : new[] { "right", "r" };
            foreach (var x in bazlar)
                foreach (var yan in w) { l.Add(yan + x); l.Add(x + yan); }
            return l;
        }

        static readonly string[] BUstKol = { "upperarm", "arm", "uparm", "armupper" };
        static readonly string[] BAltKol = { "lowerarm", "forearm", "armlower", "loarm" };
        static readonly string[] BEl = { "hand", "wrist" };
        static readonly string[] BUstBacak = { "upperleg", "upleg", "thigh", "legupper" };
        static readonly string[] BAltBacak = { "lowerleg", "calf", "shin", "leglower", "leg" };
        static readonly string[] BAyak = { "foot", "ankle" };
        static readonly string[] BParmak = { "toes", "toe", "toebase", "ball" };
        static readonly string[] BOmuz = { "shoulder", "clavicle", "collar" };

        static List<string> Adaylar(HumanBodyBones b)
        {
            switch (b)
            {
                case HumanBodyBones.Hips: return new List<string> { "hips", "pelvis", "hip", "bippelvis", "cog" };
                case HumanBodyBones.Spine: return new List<string> { "spine", "spine0", "spine00", "spine1", "spine01", "abdomen", "spinelower", "lowerspine", "spinebase" };
                case HumanBodyBones.Chest: return new List<string> { "chest", "spine2", "spine02", "torso", "spinemid", "middlespine" };
                case HumanBodyBones.UpperChest: return new List<string> { "upperchest", "spine3", "spine03", "chest2", "spineupper", "upperspine" };
                case HumanBodyBones.Neck: return new List<string> { "neck", "neck1", "neck01", "necklower" };
                case HumanBodyBones.Head: return new List<string> { "head" };
                case HumanBodyBones.LeftShoulder: return Yanli(BOmuz, true);
                case HumanBodyBones.RightShoulder: return Yanli(BOmuz, false);
                case HumanBodyBones.LeftUpperArm: return Yanli(BUstKol, true);
                case HumanBodyBones.RightUpperArm: return Yanli(BUstKol, false);
                case HumanBodyBones.LeftLowerArm: return Yanli(BAltKol, true);
                case HumanBodyBones.RightLowerArm: return Yanli(BAltKol, false);
                case HumanBodyBones.LeftHand: return Yanli(BEl, true);
                case HumanBodyBones.RightHand: return Yanli(BEl, false);
                case HumanBodyBones.LeftUpperLeg: return Yanli(BUstBacak, true);
                case HumanBodyBones.RightUpperLeg: return Yanli(BUstBacak, false);
                case HumanBodyBones.LeftLowerLeg: return Yanli(BAltBacak, true);
                case HumanBodyBones.RightLowerLeg: return Yanli(BAltBacak, false);
                case HumanBodyBones.LeftFoot: return Yanli(BAyak, true);
                case HumanBodyBones.RightFoot: return Yanli(BAyak, false);
                case HumanBodyBones.LeftToes: return Yanli(BParmak, true);
                case HumanBodyBones.RightToes: return Yanli(BParmak, false);
            }
            return new List<string>();
        }

        static void Ata(Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how, HumanBodyBones b, Transform t, string yontem)
        {
            if (t == null || map.ContainsKey(b)) return;
            foreach (var v in map.Values) if (v == t) return;   // ayni kemik iki role verilmesin
            map[b] = t;
            how[b] = yontem;
        }

        static List<HumanBodyBones> EksikGerekli(Dictionary<HumanBodyBones, Transform> map)
        {
            var l = new List<HumanBodyBones>();
            foreach (var b in Gerekli) if (!map.ContainsKey(b)) l.Add(b);
            return l;
        }

        static string Adlar(List<HumanBodyBones> l)
        {
            var parts = new List<string>();
            foreach (var b in l) parts.Add(b.ToString());
            return string.Join(", ", parts.ToArray());
        }

        // 2. asama: Mixamo / Unreal / Blender gibi yaygin adlandirmalar
        static void SinonimleTamamla(Transform[] all, Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how)
        {
            var norm = new Dictionary<string, Transform>();
            foreach (var t in all)
            {
                string n = Nr(t.name);
                if (n.Length > 0 && !norm.ContainsKey(n)) norm[n] = t;
            }
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                var b = (HumanBodyBones)i;
                if (map.ContainsKey(b)) continue;
                foreach (var cand in Adaylar(b))
                {
                    Transform t;
                    if (norm.TryGetValue(cand, out t))
                    {
                        Ata(map, how, b, t, "isim:" + t.name);
                        if (map.ContainsKey(b)) break;
                    }
                }
            }
        }

        // 3. asama: isimden bulunamayanlari iskelet hiyerarsisinden (Hips -> bacaklar/omurga -> kollar/boyun) cikar
        static int Alt(Transform t, HashSet<Transform> tum)
        {
            int n = 0;
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (!tum.Contains(c)) continue;
                n += 1 + Alt(c, tum);
            }
            return n;
        }

        static int Derinlik(Transform t, HashSet<Transform> tum)
        {
            int d = 0;
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (!tum.Contains(c)) continue;
                d = Mathf.Max(d, 1 + Derinlik(c, tum));
            }
            return d;
        }

        static List<Transform> KemikCocuklari(Transform t, HashSet<Transform> tum)
        {
            var l = new List<Transform>();
            for (int i = 0; i < t.childCount; i++) if (tum.Contains(t.GetChild(i))) l.Add(t.GetChild(i));
            return l;
        }

        static Transform EnBuyuk(List<Transform> l, HashSet<Transform> tum)
        {
            Transform best = null; int bc = -1;
            foreach (var c in l) { int n = Alt(c, tum); if (n > bc) { bc = n; best = c; } }
            return best;
        }

        // Adindan sol/sag ipucu: -1 sol, +1 sag, 0 belirsiz
        static int YanIpucu(Transform t)
        {
            string r = t.name.ToLowerInvariant();
            if (r.Contains("left")) return -1;
            if (r.Contains("right")) return 1;
            if (r.EndsWith(".l") || r.EndsWith("_l") || r.Contains("_l_") || r.Contains(".l.") || r.StartsWith("l_") || r.StartsWith("l.")) return -1;
            if (r.EndsWith(".r") || r.EndsWith("_r") || r.Contains("_r_") || r.Contains(".r.") || r.StartsWith("r_") || r.StartsWith("r.")) return 1;
            return 0;
        }

        // Iki dalin (sol, sag) siralamasi: once adlardaki ipucu, yoksa konum (karakter +Z'ye bakar: sol = -X)
        static void SolSag(List<Transform> dallar, out Transform sol, out Transform sag, List<string> log, string ne)
        {
            dallar.Sort((a, b) => a.position.x.CompareTo(b.position.x));
            sol = dallar[0];
            sag = dallar[dallar.Count - 1];
            int hs = YanIpucu(sol), hg = YanIpucu(sag);
            if (hs > 0 || hg < 0)
            {
                var tmp = sol; sol = sag; sag = tmp;
                log.Add(ne + ": sol/sağ adlardan belirlendi");
            }
            else if (hs == 0 && hg == 0) log.Add(ne + ": sol/sağ KONUMDAN tahmin edildi (ad ipucu yok) — animasyonlar aynalı çıkarsa Rig'i elle kontrol et");
        }

        static void HiyerarsiTamamla(GameObject go, Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how, List<string> log)
        {
            var kemik = new HashSet<Transform>();
            foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var bs = smr.bones;
                if (bs == null) continue;
                foreach (var b in bs) if (b != null) kemik.Add(b);
            }
            if (kemik.Count < 15) { log.Add("hiyerarşi: SkinnedMeshRenderer kemik sayısı yetersiz (" + kemik.Count + "): iskelet çıkarılamadı"); return; }
            var tum = new HashSet<Transform>(kemik);
            foreach (var b in kemik) for (var t = b.parent; t != null && t != go.transform; t = t.parent) tum.Add(t);

            // Hips: kemik agacinin ilk dallanma noktasi (omurga + iki bacak)
            Transform hips;
            if (!map.TryGetValue(HumanBodyBones.Hips, out hips))
            {
                Transform kok = null; int kc = -1;
                foreach (var t in tum)
                    if (t.parent == null || !tum.Contains(t.parent)) { int n = Alt(t, tum); if (n > kc) { kc = n; kok = t; } }
                hips = kok;
                for (int g = 0; g < 20 && hips != null; g++)
                {
                    var k = KemikCocuklari(hips, tum);
                    if (k.Count >= 3 || k.Count == 0) break;
                    hips = k.Count == 1 ? k[0] : EnBuyuk(k, tum);
                }
                if (hips == null) { log.Add("hiyerarşi: Hips bulunamadı"); return; }
                Ata(map, how, HumanBodyBones.Hips, hips, "hiyerarşi:" + hips.name);
            }

            // bacaklar: Hips'in altinda kalan derin dallar
            var cocuk = KemikCocuklari(hips, tum);
            var bacaklar = new List<Transform>();
            var ustler = new List<Transform>();
            foreach (var c in cocuk)
            {
                if (c.position.y < hips.position.y - 1e-4f && Derinlik(c, tum) >= 2) bacaklar.Add(c); else ustler.Add(c);
            }
            if (bacaklar.Count >= 2)
            {
                Transform sol, sag;
                SolSag(bacaklar, out sol, out sag, log, "bacaklar");
                BacakZinciri(map, how, tum, sol, true);
                BacakZinciri(map, how, tum, sag, false);
            }
            else log.Add("hiyerarşi: Hips altında iki bacak dalı bulunamadı (" + bacaklar.Count + ")");

            // omurga: en yuksek dal; dallanma noktasina (boyun + omuzlar) kadar tek zincir
            Transform omurga = null;
            foreach (var c in ustler) if (omurga == null || c.position.y > omurga.position.y) omurga = c;
            if (omurga == null) { log.Add("hiyerarşi: Hips üstünde omurga dalı yok"); return; }
            Ata(map, how, HumanBodyBones.Spine, omurga, "hiyerarşi:" + omurga.name);
            var zincir = new List<Transform> { omurga };
            var cur = omurga;
            for (int g = 0; g < 8; g++)
            {
                var k = KemikCocuklari(cur, tum);
                if (k.Count != 1) break;
                cur = k[0];
                zincir.Add(cur);
            }
            if (zincir.Count >= 3)
            {
                Ata(map, how, HumanBodyBones.Chest, zincir[1], "hiyerarşi:" + zincir[1].name);
                Ata(map, how, HumanBodyBones.UpperChest, zincir[zincir.Count - 1], "hiyerarşi:" + zincir[zincir.Count - 1].name);
            }
            else if (zincir.Count == 2) Ata(map, how, HumanBodyBones.Chest, zincir[1], "hiyerarşi:" + zincir[1].name);

            var dallar = KemikCocuklari(cur, tum);
            if (dallar.Count < 3) { log.Add("hiyerarşi: omurga ucunda boyun + iki omuz dalı bulunamadı (" + dallar.Count + " dal)"); return; }
            // boyun: merkez cizgisine en yakin dal; kollar: kalan dallarin en uctakileri
            Transform boyun = null; float bd = float.MaxValue;
            foreach (var d in dallar)
            {
                float dx = Mathf.Abs(d.position.x - hips.position.x);
                if (dx < bd) { bd = dx; boyun = d; }
            }
            var kollar = new List<Transform>();
            foreach (var d in dallar) if (d != boyun) kollar.Add(d);
            if (boyun != null) BoyunZinciri(map, how, tum, boyun);
            if (kollar.Count >= 2)
            {
                Transform sol, sag;
                SolSag(kollar, out sol, out sag, log, "kollar");
                KolZinciri(map, how, tum, sol, true, log);
                KolZinciri(map, how, tum, sag, false, log);
            }
        }

        static void BacakZinciri(Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how, HashSet<Transform> tum, Transform ust, bool sol)
        {
            var ub = sol ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg;
            var lb = sol ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            var fb = sol ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            Ata(map, how, ub, ust, "hiyerarşi:" + ust.name);
            Transform u;
            if (!map.TryGetValue(ub, out u)) return;
            var k1 = KemikCocuklari(u, tum);
            if (k1.Count == 0) return;
            var alt = EnBuyuk(k1, tum);
            Ata(map, how, lb, alt, "hiyerarşi:" + alt.name);
            Transform a;
            if (!map.TryGetValue(lb, out a)) return;
            var k2 = KemikCocuklari(a, tum);
            if (k2.Count == 0) return;
            var ayak = EnBuyuk(k2, tum);
            Ata(map, how, fb, ayak, "hiyerarşi:" + ayak.name);
        }

        static void BoyunZinciri(Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how, HashSet<Transform> tum, Transform bas)
        {
            var z = new List<Transform> { bas };
            var cur = bas;
            for (int g = 0; g < 4; g++)
            {
                var k = KemikCocuklari(cur, tum);
                if (k.Count != 1) break;
                cur = k[0];
                z.Add(cur);
            }
            if (Nr(bas.name).Contains("head")) { Ata(map, how, HumanBodyBones.Head, bas, "hiyerarşi:" + bas.name); return; }
            if (z.Count >= 2)
            {
                Ata(map, how, HumanBodyBones.Neck, z[0], "hiyerarşi:" + z[0].name);
                Ata(map, how, HumanBodyBones.Head, z[z.Count - 1], "hiyerarşi:" + z[z.Count - 1].name);
            }
            else Ata(map, how, HumanBodyBones.Head, z[0], "hiyerarşi:" + z[0].name);
        }

        static void KolZinciri(Dictionary<HumanBodyBones, Transform> map, Dictionary<HumanBodyBones, string> how, HashSet<Transform> tum, Transform bas, bool sol, List<string> log)
        {
            var z = new List<Transform> { bas };
            var cur = bas;
            for (int g = 0; g < 10; g++)
            {
                var k = KemikCocuklari(cur, tum);
                if (k.Count != 1) break;
                cur = k[0];
                z.Add(cur);
            }
            // adinda el/bilek gecen ilk kemikte kes
            for (int i = 0; i < z.Count; i++)
            {
                string n = Nr(z[i].name);
                if ((n.Contains("hand") || n.Contains("wrist")) && !n.Contains("finger") && !n.Contains("index") && !n.Contains("thumb")) { z.RemoveRange(i + 1, z.Count - i - 1); break; }
            }
            var sb = sol ? HumanBodyBones.LeftShoulder : HumanBodyBones.RightShoulder;
            var ub = sol ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm;
            var lb = sol ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm;
            var hb = sol ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
            int c = z.Count;
            if (c >= 4)
            {
                Ata(map, how, sb, z[0], "hiyerarşi:" + z[0].name);
                Ata(map, how, ub, z[1], "hiyerarşi:" + z[1].name);
                Ata(map, how, lb, z[c - 2], "hiyerarşi:" + z[c - 2].name);
                Ata(map, how, hb, z[c - 1], "hiyerarşi:" + z[c - 1].name);
            }
            else if (c == 3)
            {
                Ata(map, how, ub, z[0], "hiyerarşi:" + z[0].name);
                Ata(map, how, lb, z[1], "hiyerarşi:" + z[1].name);
                Ata(map, how, hb, z[2], "hiyerarşi:" + z[2].name);
            }
            else log.Add("hiyerarşi: " + (sol ? "sol" : "sağ") + " kol zinciri çok kısa (" + c + " kemik)");
        }

        // Verilen eslemeyle avatar kurar; 'cikar' icindeki (istege bagli) kemikler dahil edilmez
        static Avatar Kur(GameObject go, Transform[] all, Dictionary<HumanBodyBones, Transform> map, HashSet<HumanBodyBones> cikar)
        {
            var human = new List<HumanBone>();
            foreach (var kv in map)
            {
                if (cikar != null && cikar.Contains(kv.Key)) continue;
                var lim = new HumanLimit();
                lim.useDefaultValues = true;
                var hb = new HumanBone();
                hb.humanName = HumanTrait.BoneName[(int)kv.Key];
                hb.boneName = kv.Value.name;
                hb.limit = lim;
                human.Add(hb);
            }
            var skel = new SkeletonBone[all.Length];
            for (int i = 0; i < all.Length; i++)
                skel[i] = new SkeletonBone { name = all[i].name, position = all[i].localPosition, rotation = all[i].localRotation, scale = all[i].localScale };
            var desc = new HumanDescription
            {
                human = human.ToArray(), skeleton = skel,
                upperArmTwist = 0.5f, lowerArmTwist = 0.5f, upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
                armStretch = 0.05f, legStretch = 0.05f, feetSpacing = 0f, hasTranslationDoF = false
            };
            return AvatarBuilder.BuildHumanAvatar(go, desc);
        }

        // Humanoid avatar: 1) Quaternius adlari 2) yaygin adlandirmalar (Mixamo, Unreal, Blender) 3) iskelet hiyerarsisi.
        // Kurulamazsa hangi kemiklerin eksik oldugunu tek tek raporlar (sessizce atlamaz, calismayi bozmaz).
        static Avatar BuildAvatar(GameObject go)
        {
            var all = go.GetComponentsInChildren<Transform>(true);
            var byName = new Dictionary<string, Transform>();
            int dup = 0;
            foreach (var t in all)
            {
                if (byName.ContainsKey(t.name) && t != go.transform) t.name = t.name + "_" + (++dup);   // ayni ad olmasin
                byName[t.name] = t;
            }
            var map = new Dictionary<HumanBodyBones, Transform>();
            var how = new Dictionary<HumanBodyBones, string>();
            var log = new List<string>();
            try
            {
                // 1) Quaternius adlari
                for (int i = 0; i < HB.Length; i++)
                {
                    Transform t;
                    if (byName.TryGetValue(QB[i], out t)) Ata(map, how, HB[i], t, "quaternius");
                }
                var eksik = EksikGerekli(map);
                string ilkEksik = Adlar(eksik);
                // 2) yaygin adlar
                if (eksik.Count > 0) { SinonimleTamamla(all, map, how); eksik = EksikGerekli(map); }
                // 3) hiyerarsi
                if (eksik.Count > 0)
                {
                    try { HiyerarsiTamamla(go, map, how, log); }
                    catch (System.Exception e) { log.Add("hiyerarşi hatası: " + e.Message); }
                    eksik = EksikGerekli(map);
                }
                if (eksik.Count > 0)
                {
                    var hepsi = new List<string>();
                    foreach (var kv in map) hepsi.Add(kv.Key + "=" + kv.Value.name);
                    RigRapor(go.name + ": Humanoid için ZORUNLU kemikler bulunamadı: [" + Adlar(eksik) + "]. Bulunanlar (" + map.Count + "): " + string.Join(", ", hepsi.ToArray()) +
                             (log.Count > 0 ? " | notlar: " + string.Join("; ", log.ToArray()) : "") +
                             " | Çözüm: modelin kemik adlarını Quaternius/Mixamo adlarına çevir ya da FBX olarak içe aktarıp Rig > Humanoid > Configure ile eşle.", true);
                    return null;
                }

                // hiyerarsi tutarliligi: istege bagli kemikler Spine'in altinda olmali (degilse cikar)
                var istege = new List<HumanBodyBones>
                {
                    HumanBodyBones.UpperChest, HumanBodyBones.Chest, HumanBodyBones.Neck, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder,
                    HumanBodyBones.LeftToes, HumanBodyBones.RightToes
                };
                Transform spine = map[HumanBodyBones.Spine];
                foreach (var b in istege)
                {
                    Transform t;
                    if (map.TryGetValue(b, out t) && t != spine && !t.IsChildOf(spine) && b != HumanBodyBones.LeftToes && b != HumanBodyBones.RightToes)
                    {
                        log.Add(b + " (" + t.name + ") Spine'in altında değil, çıkarıldı");
                        map.Remove(b);
                    }
                }

                // denemeler: once hepsi, sonra istege bagli kemikleri azaltarak
                var denemeler = new List<HashSet<HumanBodyBones>>
                {
                    null,
                    new HashSet<HumanBodyBones> { HumanBodyBones.UpperChest },
                    new HashSet<HumanBodyBones> { HumanBodyBones.UpperChest, HumanBodyBones.Chest },
                    new HashSet<HumanBodyBones> { HumanBodyBones.UpperChest, HumanBodyBones.Chest, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder },
                    new HashSet<HumanBodyBones> { HumanBodyBones.UpperChest, HumanBodyBones.Chest, HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder, HumanBodyBones.Neck,
                                                  HumanBodyBones.LeftToes, HumanBodyBones.RightToes },
                };
                for (int d = 0; d < denemeler.Count; d++)
                {
                    var av = Kur(go, all, map, denemeler[d]);
                    if (av != null && av.isValid && av.isHuman)
                    {
                        bool fallback = ilkEksik.Length > 0 || d > 0 || log.Count > 0;
                        if (fallback)
                        {
                            var kaynak = new List<string>();
                            foreach (var kv in how) if (kv.Value != "quaternius") kaynak.Add(kv.Key + "<-" + kv.Value);
                            RigRapor(go.name + ": Humanoid avatar fallback ile kuruldu (" + (d == 0 ? "tüm kemiklerle" : d + ". deneme, isteğe bağlı kemikler çıkarıldı") + ")." +
                                     (kaynak.Count > 0 ? " Eşlenenler: " + string.Join(", ", kaynak.ToArray()) + "." : "") +
                                     (log.Count > 0 ? " Notlar: " + string.Join("; ", log.ToArray()) : ""), false);
                        }
                        return av;
                    }
                    if (av != null) Object.DestroyImmediate(av);
                }
                RigRapor(go.name + ": 15 zorunlu kemik bulundu (" + map.Count + " kemik eşlendi) ama AvatarBuilder geçerli Humanoid avatar üretmedi (isValid/isHuman=false) — " +
                         "T-pose dışı duruş, hiyerarşide kemik atlaması ya da yanlış sol/sağ olabilir." + (log.Count > 0 ? " Notlar: " + string.Join("; ", log.ToArray()) : ""), true);
                return null;
            }
            catch (System.Exception e)
            {
                RigRapor(go.name + ": avatar kurulurken beklenmeyen hata: " + e.GetType().Name + ": " + e.Message, true);
                return null;
            }
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
        bool UalKutuphane { get { return assetPath.Replace('\\', '/').Contains("/Karakterler/Animasyonlar/"); } }
        bool Toy { get { var p = assetPath.Replace('\\', '/'); return p.Contains("/Karakterler/Oyuncak/") && p.ToLowerInvariant().EndsWith(".fbx"); } }

        bool Rocketbox { get { return assetPath.Replace('\\', '/').Contains("/Karakterler/Rocketbox/"); } }

        void OnPreprocessTexture()
        {
            if (!Rocketbox) return;
            var ti = (TextureImporter)assetImporter;
            var n = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            if (n.Contains("_normal") || n == "normal") ti.textureType = TextureImporterType.NormalMap;
            else if (n.Contains("opacity")) ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;
        }

        void OnPreprocessModel()
        {
            if (Rocketbox)
            {
                var rb = (ModelImporter)assetImporter;
                rb.animationType = ModelImporterAnimationType.Human;
                rb.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                rb.importAnimation = false;
                rb.importBlendShapes = true;
                rb.importCameras = false;
                rb.importLights = false;
                return;
            }
            if (!Ual && !Toy) return;
            var imp = (ModelImporter)assetImporter;
            imp.importAnimation = true;
            imp.animationType = Ual ? ModelImporterAnimationType.Human : ModelImporterAnimationType.Generic;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            if (assetPath.Replace('\\', '/').Contains("/Karakterler/Mixamo/"))
            {
                imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                imp.materialLocation = ModelImporterMaterialLocation.InPrefab;
                imp.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
            }
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
                // Gövde yönü sıfırlansın: Mixamo klipleri yana dönük başlamasın; UAL kütüphanesi -Z'ye bakar,
                // orijinal yön korunursa karakterlerin sırtı kameraya döner.
                c.keepOriginalOrientation = !MixAnim && !UalKutuphane;
                c.keepOriginalPositionY = true;
                c.keepOriginalPositionXZ = true;
            }
            imp.clipAnimations = clips;
        }

        // Malzeme adina en cok benzeyen dokuyu sec ("Ch28_body" -> "Ch28_1001_Diffuse.png")
        public static string En(string[] files, string matName, string kind)
        {
            string best = null; int bestScore = -1;
            var toks = matName.ToLowerInvariant().Replace(' ', '_').Replace('.', '_').Split('_');
            foreach (var f in files)
            {
                var n = Path.GetFileName(f).ToLowerInvariant();
                if (n.EndsWith(".meta") || !n.Contains(kind)) continue;
                int sc = 0;
                foreach (var t in toks) if (t.Length > 1 && n.Contains(t)) sc++;
                if (sc > bestScore) { bestScore = sc; best = f.Replace('\\', '/'); }
            }
            return best;
        }

        static readonly List<string> DokuCikarSirasi = new List<string>();

        [MenuItem("Hikaye/Hareket Kutuphanesini Yeniden Ice Aktar")]
        static void UalYenileMenu()
        {
            AssetDatabase.ImportAsset(Karakter3DKurulum.UalPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("[Hikaye Stüdyosu] Hareket kütüphanesi yeniden içe aktarıldı.");
        }

        [MenuItem("Hikaye/Mixamo Animasyonlarını Yenile")]
        static void AnimYenileMenu()
        {
            var dir = Karakter3DKurulum.MixamoAnimDir;
            if (!Directory.Exists(dir)) return;
            AssetDatabase.StartAssetEditing();
            try { foreach (var f in Directory.GetFiles(dir, "*.fbx")) AssetDatabase.ImportAsset(f.Replace('\\', '/'), ImportAssetOptions.ForceUpdate); }
            finally { AssetDatabase.StopAssetEditing(); }
            int n = Karakter3DKurulum.BuildAll(true);
            Debug.Log("[Hikaye Stüdyosu] Mixamo animasyonları yenilendi, " + n + " karakter hazır.");
        }

        [MenuItem("Hikaye/Mixamo Dokularını Düzelt")]
        static void DokulariDuzeltMenu()
        {
            var dir = Karakter3DKurulum.MixamoDir;
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.GetFiles(dir, "*.fbx")) DokuCikarSirasi.Add(f.Replace('\\', '/'));
            DokulariCikar();
            // malzemeleri yeniden bagla (prefablari bastan kur)
            int n = Karakter3DKurulum.BuildAll(true);
            Debug.Log("[Hikaye Stüdyosu] Mixamo malzemeleri yenilendi, " + n + " karakter hazır.");
        }

        static void DokulariCikar()
        {
            var list = new List<string>(DokuCikarSirasi);
            DokuCikarSirasi.Clear();
            var yeniden = new List<string>();
            foreach (var path in list)
            {
                var imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp == null) continue;
                var dir = Path.GetDirectoryName(path).Replace('\\', '/') + "/Dokular/" + Path.GetFileNameWithoutExtension(path);
                if (AssetDatabase.IsValidFolder(dir)) continue;   // daha once cikarildi
                Directory.CreateDirectory(dir);
                try { if (imp.ExtractTextures(dir)) yeniden.Add(path); }
                catch (System.Exception e) { Debug.LogWarning("[Hikaye] " + path + " dokulari cikarilamadi: " + e.Message); }
            }
            if (yeniden.Count == 0) return;
            AssetDatabase.Refresh();
            foreach (var path in yeniden) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("[Hikaye Stüdyosu] " + yeniden.Count + " Mixamo karakterinin dokulari cikarildi.");
        }

        // Yeni .glb / .fbx karakter ice aktarilinca hazirligi tekrar dene
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // Mixamo karakterlerinin dokulari FBX icine gomulu: disari cikar ki karakter beyaz gorunmesin
            foreach (var p in imported)
            {
                var q = p.Replace('\\', '/');
                if (q.Contains("/Karakterler/Mixamo/") && q.ToLowerInvariant().EndsWith(".fbx")) DokuCikarSirasi.Add(q);
            }
            if (DokuCikarSirasi.Count > 0) { EditorApplication.delayCall -= DokulariCikar; EditorApplication.delayCall += DokulariCikar; }

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
