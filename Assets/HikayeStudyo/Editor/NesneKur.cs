using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

namespace HikayeStudyo
{
    // Indirilen her seyi (hayvanlar, NASA model/gorselleri, KayKit paketleri) projeye alir:
    //   nesne katalogu ('nesne:' komutu) + uzay arka planlari ('mekan: uzay_mars') + masal karakterleri.
    // Menu: Hikaye/Her Seyi Ekle  ->  HikayeKomut/nesne_rapor.txt, nesne_galeri_N.png
    public static class NesneKur
    {
        const string Kok = "Assets/HikayeStudyo/Kutuphane";
        const string Katalog = "Assets/Resources/NesneKatalogu.asset";
        const string Hazir = "Assets/HikayeStudyo/Karakterler/Hazir";

        static StringBuilder rapor;
        static readonly HashSet<string> idler = new HashSet<string>();

        // Packages/manifest.json disaridan degisince Unity pencereye tiklanmadan cozsun
        [MenuItem("Hikaye/Paketleri Yenile", priority = 46)]
        public static void PaketleriYenile() { UnityEditor.PackageManager.Client.Resolve(); }

        [MenuItem("Hikaye/Her Seyi Ekle", priority = 44)]
        public static void Ekle()
        {
            rapor = new StringBuilder();
            idler.Clear();
            string proje = Directory.GetParent(Application.dataPath).FullName;
            string ham = Path.Combine(proje, "KutuphaneHam");
            var yol = Path.Combine(UzaktanKomut.Dir, "nesne_rapor.txt");
            try
            {
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Dosyalar kopyalanıyor...", 0.05f);
                Kopyala(ham);
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Modeller içe aktarılıyor (uzun sürebilir)...", 0.2f);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var kat = ScriptableObject.CreateInstance<NesneKatalogu>();
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Hayvanlar...", 0.4f);
                Dene("Hayvanlar", () => Hayvanlar(kat));
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Eşyalar...", 0.5f);
                Dene("KayKit eşyaları", () => KayKitEsyalar(kat));
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Uzay modelleri...", 0.6f);
                Dene("NASA modelleri", () => NasaModelleri(kat));
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Gezegenler ve uzay mekânları...", 0.7f);
                Dene("Gezegenler", () => GokCisimleri(kat));
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Masal karakterleri...", 0.8f);
                Dene("Masal karakterleri", MasalKarakterleri);

                Klasor("Assets/Resources");
                AssetDatabase.DeleteAsset(Katalog);
                AssetDatabase.CreateAsset(kat, Katalog);
                AssetDatabase.SaveAssets();

                rapor.Insert(0, "TOPLAM: " + kat.nesneler.Count + " nesne, " + kat.gokler.Count + " uzay mekânı\n");
                foreach (var g in kat.nesneler.GroupBy(n => n.kategori))
                    rapor.AppendLine("  " + g.Key + ": " + g.Count());
                rapor.AppendLine("UZAY MEKANLARI: " + string.Join(", ", kat.gokler.Select(g => g.id).ToArray()));
                File.WriteAllText(yol, rapor.ToString());

                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Galeri resimleri...", 0.9f);
                Dene("Galeri", () => Galeri(kat));
            }
            catch (System.Exception e)
            {
                rapor.AppendLine("HATA: " + e);
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                File.WriteAllText(yol, rapor.ToString());
            }
        }

        static void Dene(string ad, System.Action is_)
        {
            try { is_(); }
            catch (System.Exception e) { rapor.AppendLine("HATA (" + ad + "): " + e.Message); Debug.LogException(e); }
        }

        // ---------------- ortak ----------------
        static string Slug(string s)
        {
            var n = Util.Norm(s);
            var sb = new StringBuilder();
            foreach (var c in n)
            {
                if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9') sb.Append(c);
                else if (sb.Length > 0 && sb[sb.Length - 1] != '_') sb.Append('_');
            }
            return sb.ToString().Trim('_');
        }

        static string Benzersiz(string id)
        {
            var a = id; int i = 2;
            while (!idler.Add(a)) a = id + "_" + i++;
            return a;
        }

        static void Klasor(string p)
        {
            if (AssetDatabase.IsValidFolder(p)) return;
            Directory.CreateDirectory(p);
            AssetDatabase.Refresh();
        }

        static string Posix(string p) { return p.Replace('\\', '/'); }

        static int Al(string kaynak, string hedef)
        {
            if (File.Exists(hedef)) return 0;
            Directory.CreateDirectory(Path.GetDirectoryName(hedef));
            File.Copy(kaynak, hedef);
            return 1;
        }

        static void Kopyala(string ham)
        {
            int n = 0;
            var m = Path.Combine(ham, "NASA", "3D Models");
            if (Directory.Exists(m))
                foreach (var f in Directory.GetFiles(m, "*.glb", SearchOption.AllDirectories))
                    n += Al(f, Kok + "/NASA_Model/" + Slug(Path.GetFileNameWithoutExtension(f)) + ".glb");
            var g = Path.Combine(ham, "NASA", "Images and Textures");
            if (Directory.Exists(g))
                foreach (var f in Directory.GetFiles(g, "*.jpg", SearchOption.AllDirectories))
                    n += Al(f, Kok + "/Gok/Harita/" + Slug(Path.GetFileNameWithoutExtension(f)) + ".jpg");
            var h = Path.Combine(ham, "Rocketbox", "Animals");
            if (Directory.Exists(h))
                foreach (var d in Directory.GetDirectories(h))
                {
                    var ad = Path.GetFileName(d);
                    foreach (var alt in new[] { "Export", "Textures" })
                    {
                        var k = Path.Combine(d, alt);
                        if (!Directory.Exists(k)) continue;
                        foreach (var f in Directory.GetFiles(k))
                        {
                            var u = Path.GetExtension(f).ToLowerInvariant();
                            if (u == ".fbx" || u == ".tga" || u == ".png") n += Al(f, Kok + "/Hayvan/" + ad + "/" + Path.GetFileName(f));
                        }
                    }
                }
            rapor.AppendLine("Kopyalanan dosya: " + n);
        }

        static NesneKatalogu.Nesne Ekle(NesneKatalogu kat, string id, string kategori, GameObject model, float boy, params string[] takma)
        {
            var e = new NesneKatalogu.Nesne
            {
                id = Benzersiz(id), kategori = kategori, model = model, boy = boy,
                takma = takma.Where(t => !string.IsNullOrEmpty(t) && t != id).Distinct().ToArray()
            };
            kat.nesneler.Add(e);
            return e;
        }

        static bool Sinir(GameObject go, out Bounds b)
        {
            b = new Bounds();
            bool var1 = false;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (!var1) { b = r.bounds; var1 = true; }
                else b.Encapsulate(r.bounds);
            }
            return var1;
        }

        static float DogalBoy(GameObject prefab)
        {
            var go = Object.Instantiate(prefab);
            try
            {
                Bounds b;
                return Sinir(go, out b) ? Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)) : 0f;
            }
            finally { Object.DestroyImmediate(go); }
        }

        static Material Malzeme(string yol, string shader, Texture2D doku, Color renk)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(yol);
            if (m == null)
            {
                m = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(m, yol);
            }
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", doku);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", renk);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.08f);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------------- hayvanlar ----------------
        // klasor adi -> kimlik, boy (m), takma adlar
        static readonly string[][] HayvanTablo =
        {
            new[] { "Horse_Brown_01", "at", "2.3", "at_kahve", "beygir" },
            new[] { "Horse_Brown_Saddle_01", "at_eyerli", "2.3" },
            new[] { "Horse_LightBrown_01", "at_acik", "2.3" },
            new[] { "Horse_LightBrown_Saddle_01", "at_acik_eyerli", "2.3" },
            new[] { "Cow_Brown_01", "inek", "2.2" }, new[] { "Cow_Black_01", "inek_siyah", "2.2" },
            new[] { "Bull_Brown_01", "boga", "2.4", "okuz" },
            new[] { "Camel_OneHump_01", "deve", "2.7" }, new[] { "Camel_TwoHump_01", "deve_cift_horguclu", "2.7" },
            new[] { "Sheep_White_01", "koyun", "1.2", "kuzu" },
            new[] { "Goat_Brown_01", "keci", "1.2" }, new[] { "Goat_White_01", "keci_beyaz", "1.2" },
            new[] { "Dog_Beagle_01", "kopek", "0.85", "kopek_beagle" },
            new[] { "Dog_GermanShepard_01", "kopek_kurt", "1.15", "kurt_kopegi", "coban_kopegi" },
            new[] { "Donkey_Grey_01", "esek", "1.8" }, new[] { "Pig_Pink_01", "domuz", "1.35" },
            new[] { "Deer_Roe_Tabby_01", "geyik", "1.5", "karaca" },
            new[] { "Bird_Chicken_Brown_01", "tavuk", "0.45" }, new[] { "Bird_Chicken_White_01", "tavuk_beyaz", "0.45" },
            new[] { "Bird_Rooster_Brown_01", "horoz", "0.55" },
            new[] { "Bird_Duck_Female_01", "ordek", "0.5" }, new[] { "Bird_Duck_Male_01", "ordek_erkek", "0.5" },
            new[] { "Bird_Goose_Grey_01", "kaz", "0.75" }, new[] { "Bird_Goose_White_01", "kaz_beyaz", "0.75" },
            new[] { "Bird_Sparrow_01", "serce", "0.25", "kus" },
        };

        static void Hayvanlar(NesneKatalogu kat)
        {
            var kok = Kok + "/Hayvan";
            if (!Directory.Exists(kok)) { rapor.AppendLine("Hayvan klasörü yok"); return; }
            foreach (var d in Directory.GetDirectories(kok).OrderBy(x => x))
            {
                var ad = Path.GetFileName(d);
                var klasor = kok + "/" + ad;
                var fbx = Directory.GetFiles(klasor, "*.fbx").Select(Posix).FirstOrDefault();
                var model = fbx != null ? AssetDatabase.LoadAssetAtPath<GameObject>(fbx) : null;
                if (model == null) { rapor.AppendLine("Hayvan içe aktarılamadı: " + ad); continue; }

                var renkler = Directory.GetFiles(klasor).Select(Posix)
                    .Where(f => !f.EndsWith(".meta") && Path.GetFileNameWithoutExtension(f).ToLowerInvariant().Contains("color")).ToList();
                var go = Object.Instantiate(model);
                go.name = ad;
                try
                {
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    {
                        var mats = r.sharedMaterials;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            string mn = Slug((mats[i] != null ? mats[i].name : "") + " " + r.name);
                            // malzeme adiyla ortak parcasi en cok olan renk dokusu; esitse en buyuk dosya (govde)
                            var parcalar = mn.Split('_').Where(p => p.Length > 2).ToArray();
                            var doku = renkler.OrderByDescending(f =>
                                    Slug(Path.GetFileNameWithoutExtension(f)).Split('_').Where(p => p != "color").Count(p => parcalar.Contains(p)))
                                .ThenByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                            if (doku == null) continue;
                            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(doku);
                            mats[i] = Malzeme(klasor + "/" + Path.GetFileNameWithoutExtension(doku) + ".mat",
                                              "Universal Render Pipeline/Lit", tex, Color.white);
                        }
                        r.sharedMaterials = mats;
                    }
                    var prefab = PrefabUtility.SaveAsPrefabAsset(go, klasor + "/" + ad + ".prefab");
                    var t = HayvanTablo.FirstOrDefault(x => x[0] == ad);
                    var e = t != null
                        ? Ekle(kat, t[1], "hayvan", prefab, float.Parse(t[2], System.Globalization.CultureInfo.InvariantCulture),
                               t.Skip(3).Concat(new[] { Slug(ad) }).ToArray())
                        : Ekle(kat, Slug(ad), "hayvan", prefab, 1.2f);
                    e.donus = 140f;
                }
                finally { Object.DestroyImmediate(go); }
            }
        }

        // ---------------- KayKit esyalari ----------------
        static readonly Dictionary<string, string> Soz = new Dictionary<string, string>
        {
            { "sword", "kilic" }, { "shield", "kalkan" }, { "axe", "balta" }, { "crossbow", "arbalet" }, { "arrow", "ok" },
            { "bow", "yay" }, { "dagger", "hancer" }, { "staff", "asa" }, { "wand", "degnek" }, { "mug", "kupa" },
            { "spellbook", "buyu_kitabi" }, { "banner", "sancak" }, { "barrel", "fici" }, { "bed", "yatak" },
            { "bottle", "sise" }, { "box", "kutu" }, { "candle", "mum" }, { "chair", "sandalye" }, { "chest", "sandik" },
            { "coin", "altin" }, { "coins", "altinlar" }, { "column", "sutun" }, { "key", "anahtar" }, { "keyring", "anahtarlik" },
            { "pillar", "direk" }, { "plate", "tabak" }, { "shelf", "raf" }, { "shelves", "raflar" }, { "stairs", "merdiven" },
            { "stool", "tabure" }, { "table", "masa" }, { "torch", "mesale" }, { "wall", "duvar" }, { "floor", "zemin" },
            { "car", "araba" }, { "police", "polis" }, { "taxi", "taksi" }, { "building", "bina" }, { "bench", "bank" },
            { "bush", "cali" }, { "streetlight", "sokak_lambasi" }, { "trafficlight", "trafik_lambasi" }, { "road", "yol" },
            { "watertower", "su_kulesi" }, { "dumpster", "cop_konteyneri" }, { "firehydrant", "yangin_muslugu" },
            { "lander", "inis_araci" }, { "rock", "kaya" }, { "rocks", "kayalar" }, { "spacetruck", "uzay_kamyonu" },
            { "windturbine", "ruzgar_turbini" }, { "solarpanel", "gunes_paneli" }, { "basemodule", "us_modulu" },
            { "cargo", "yuk" }, { "terrain", "arazi" }, { "tunnel", "tunel" }, { "dome", "kubbe" },
            { "small", "kucuk" }, { "large", "buyuk" }, { "big", "buyuk" }, { "medium", "orta_boy" }, { "tall", "uzun" },
            { "broken", "kirik" }, { "open", "acik" }, { "closed", "kapali" }, { "empty", "bos" }, { "full", "dolu" },
            { "gold", "altin" }, { "color", "renkli" }, { "red", "kirmizi" }, { "blue", "mavi" }, { "green", "yesil" },
            { "yellow", "sari" }, { "white", "beyaz" }, { "brown", "kahve" }, { "1handed", "tek_el" }, { "2handed", "cift_el" },
            { "round", "yuvarlak" }, { "square", "kare" }, { "spikes", "dikenli" }, { "lit", "yanik" }, { "stack", "yigin" },
            { "decorated", "suslu" }, { "wood", "ahsap" }, { "stone", "tas" }, { "corner", "kose" }, { "doorway", "kapi" },
            { "door", "kapi" }, { "window", "pencere" },
        };

        static string Cevir(string dosyaAdi)
        {
            var p = Slug(dosyaAdi).Split('_');
            for (int i = 0; i < p.Length; i++) { string t; if (Soz.TryGetValue(p[i], out t)) p[i] = t; }
            return string.Join("_", p);
        }

        static readonly string[] Masallar = { "barbarian", "knight", "mage", "rogue", "roguehooded",
            "skeleton_mage", "skeleton_minion", "skeleton_rogue", "skeleton_warrior" };

        static IEnumerable<string> Fbxler(string paket)
        {
            var d = Kok + "/" + paket;
            if (!Directory.Exists(d)) return Enumerable.Empty<string>();
            return Directory.GetFiles(d, "*.fbx", SearchOption.AllDirectories).Select(Posix).OrderBy(x => x);
        }

        // Paketin birim olcegi: bilinen bir parcanin gercek boyundan
        static float Olcek(string paket, string parca, float metre, float yoksa)
        {
            var f = Fbxler(paket).FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).ToLowerInvariant().Contains(parca));
            var m = f != null ? AssetDatabase.LoadAssetAtPath<GameObject>(f) : null;
            var boy = m != null ? DogalBoy(m) : 0f;
            return boy > 0.001f ? metre / boy : yoksa;
        }

        static void KayKitEsyalar(NesneKatalogu kat)
        {
            var paketler = new[]
            {
                new { ad = "KayKit_Macera", kategori = "silah", olcek = Olcek("KayKit_Macera", "sword_1handed", 1.05f, 0.7f) },
                new { ad = "KayKit_Iskelet", kategori = "silah", olcek = Olcek("KayKit_Macera", "sword_1handed", 1.05f, 0.7f) },
                new { ad = "KayKit_Zindan", kategori = "zindan", olcek = 0.72f },
                new { ad = "KayKit_Sehir", kategori = "sehir", olcek = Olcek("KayKit_Sehir", "car_sedan", 4.2f, 4f) },
                new { ad = "KayKit_Uzay", kategori = "uzay_us", olcek = Olcek("KayKit_Uzay", "spacetruck", 4.5f, 2f) },
            };
            foreach (var p in paketler)
            {
                int n = 0;
                foreach (var f in Fbxler(p.ad))
                {
                    var dosya = Path.GetFileNameWithoutExtension(f);
                    if (Masallar.Contains(dosya.ToLowerInvariant())) continue;
                    var m = AssetDatabase.LoadAssetAtPath<GameObject>(f);
                    if (m == null) { rapor.AppendLine("İçe aktarılamadı: " + f); continue; }
                    var boy = DogalBoy(m) * p.olcek;
                    if (boy <= 0.001f) continue;
                    var id = Cevir(dosya);
                    if (p.ad == "KayKit_Iskelet") id = "iskelet_" + id.Replace("skeleton_", "");
                    if (p.ad == "KayKit_Zindan") m = ZindanKapla(m, dosya);   // FBX'te doku bagli degil
                    // cok kucuk parcalar gorunsun; KayKit onu +Z'ye bakar, kameraya cevrilir
                    Ekle(kat, id, p.kategori, m, Mathf.Max(0.22f, boy), Slug(dosya)).donus = id.StartsWith("araba") ? 110f : 160f;
                    n++;
                }
                rapor.AppendLine(p.ad + ": " + n + " parça (ölçek " + p.olcek.ToString("0.00") + ")");
            }
        }

        static GameObject ZindanKapla(GameObject model, string ad)
        {
            var doku = AssetDatabase.LoadAssetAtPath<Texture2D>(Kok + "/KayKit_Zindan/texture/dungeon_texture.png");
            if (doku == null) return model;
            Klasor(Kok + "/KayKit_Zindan/prefab");
            var mat = Malzeme(Kok + "/KayKit_Zindan/prefab/zindan.mat", "Universal Render Pipeline/Lit", doku, Color.white);
            var go = Object.Instantiate(model);
            go.name = model.name;
            try
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();
                return PrefabUtility.SaveAsPrefabAsset(go, Kok + "/KayKit_Zindan/prefab/" + ad + ".prefab");
            }
            finally { Object.DestroyImmediate(go); }
        }

        // ---------------- NASA modelleri ----------------
        static void NasaModelleri(NesneKatalogu kat)
        {
            var d = Kok + "/NASA_Model";
            if (!Directory.Exists(d)) { rapor.AppendLine("NASA model klasörü yok"); return; }
            int n = 0, hata = 0;
            foreach (var f in Directory.GetFiles(d, "*.glb").Select(Posix).OrderBy(x => x))
            {
                var ad = Path.GetFileNameWithoutExtension(f);
                var m = AssetDatabase.LoadAssetAtPath<GameObject>(f);
                if (m == null)   // onceki denemede Draco eklentisi yoktu: yeniden dene
                {
                    AssetDatabase.ImportAsset(f, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                    m = AssetDatabase.LoadAssetAtPath<GameObject>(f);
                }
                if (m == null || DogalBoy(m) <= 0.00001f) { rapor.AppendLine("NASA açılamadı: " + ad); hata++; continue; }
                float boy = 2.2f;
                bool yerde = ad.Contains("rover") || ad.Contains("lander") || ad.Contains("lunar_module") || ad.Contains("astronaut") ||
                             ad.Contains("suit") || ad.Contains("glove") || ad.Contains("helmet") || ad.Contains("crawler") ||
                             ad.Contains("vehicle") || ad.Contains("tool") || ad.Contains("rock") || ad.Contains("curiosity") ||
                             ad.Contains("perseverance");
                var takma = new List<string>();
                if (ad.Contains("astronaut")) { boy = 1.8f; takma.Add("astronot"); }
                else if (ad.Contains("suit")) { boy = 1.8f; takma.Add("uzay_kiyafeti"); }
                else if (ad.Contains("glove")) { boy = 0.4f; takma.Add("uzay_eldiveni"); }
                else if (ad.Contains("helmet")) { boy = 0.45f; takma.Add("uzay_kaski"); }
                if (ad.Contains("lunar_module")) takma.Add("ay_modulu");
                if (ad.Contains("international_space_station") || ad == "iss" || ad.StartsWith("iss_")) takma.Add("uzay_istasyonu");
                if (ad.Contains("shuttle")) takma.Add("uzay_mekigi");
                if (ad.Contains("saturn_v") || ad.Contains("rocket") || ad.Contains("space_launch_system")) takma.Add("roket");
                if (ad.Contains("rover") || ad.Contains("curiosity") || ad.Contains("perseverance")) takma.Add("mars_araci");
                if (ad.Contains("satellite")) takma.Add("uydu");
                if (ad.Contains("telescope") || ad.Contains("hubble")) takma.Add("uzay_teleskobu");
                var e = Ekle(kat, ad, "uzay", m, boy, takma.ToArray());
                e.havada = !yerde;
                e.donus = ad.Contains("astronaut") || ad.Contains("suit") ? 200f : 30f;
                n++;
            }
            rapor.AppendLine("NASA: " + n + " model, " + hata + " açılamadı");
        }

        // ---------------- gezegenler + uzay arka planlari ----------------
        static readonly Dictionary<string, string> GokAdlari = new Dictionary<string, string>
        {
            { "earth_a", "dunya" }, { "earth_b", "dunya_2" }, { "moon", "ay" }, { "venus", "venus" }, { "neptune", "neptun" },
            { "pluto", "pluton" }, { "jupiter", "jupiter" }, { "saturn", "saturn" }, { "mars", "mars" },
            { "moon_lunar_color", "ay_renkli" }, { "mars_cambridge_bay", "mars_yuzeyi" },
            { "moon_apollo_astronaut_bootprint", "ay_ayak_izi" },
        };

        static string GokAdi(string slug)
        {
            string t;
            if (GokAdlari.TryGetValue(slug, out t)) return t;
            foreach (var on in new[] { "jupiter_", "saturn_", "uranus_", "neptune_", "pluto_", "mars_" })
                if (slug.StartsWith(on)) return slug.Substring(on.Length).Replace("_a", "").Replace("_b", "_2");
            if (slug.StartsWith("moon_")) return "ay_" + slug.Substring(5);
            if (slug.StartsWith("discovery")) return "mekik_kokpiti";
            if (slug.StartsWith("iss")) return "istasyon_paneli";
            return slug;
        }

        static void GokCisimleri(NesneKatalogu kat)
        {
            var harita = Kok + "/Gok/Harita";
            if (!Directory.Exists(harita)) { rapor.AppendLine("Uzay görseli klasörü yok"); return; }
            Klasor(Kok + "/Gok/Arka");
            Klasor(Kok + "/Gok/Cisim");
            var dosyalar = Directory.GetFiles(harita, "*.jpg").Select(Posix).OrderBy(x => x).ToList();

            // yildiz gokyuzleri
            var yildizlar = new List<Texture2D>();
            var yildizAdlari = new[] { new[] { "tycho", "uzay" }, new[] { "yale", "uzay_yildizlar" }, new[] { "hipparcos", "uzay_samanyolu" } };
            Texture2D gokyuzu = null;
            foreach (var y in yildizAdlari)
            {
                var f = dosyalar.FirstOrDefault(x => Path.GetFileName(x).StartsWith(y[0]));
                if (f == null) continue;
                var tex = Oku(f);
                var kirp = Kirp(tex);
                Kaydet(kat, y[1], kirp);
                if (gokyuzu == null) gokyuzu = kirp; else Object.DestroyImmediate(kirp);
                Object.DestroyImmediate(tex);
            }
            if (gokyuzu == null) { gokyuzu = new Texture2D(2, 2); gokyuzu.SetPixels(new[] { Color.black, Color.black, Color.black, Color.black }); gokyuzu.Apply(); }

            foreach (var f in dosyalar)
            {
                var slug = Path.GetFileNameWithoutExtension(f);
                if (slug.Contains("star_map")) continue;
                var ham = Oku(f);
                float oran = ham.width / (float)Mathf.Max(1, ham.height);
                var ad = GokAdi(slug);
                bool harita2 = oran > 1.9f && oran < 2.1f && !slug.StartsWith("discovery") && !slug.StartsWith("iss") && !slug.Contains("cambridge") && !slug.Contains("bootprint");
                Object.DestroyImmediate(ham);
                if (harita2)
                {
                    // gezegen: kure nesnesi + yildizli arka plan
                    var doku = AssetDatabase.LoadAssetAtPath<Texture2D>(f);
                    var prefab = Kure(ad, Malzeme(Kok + "/Gok/Cisim/" + ad + ".mat", "Universal Render Pipeline/Lit", doku, Color.white), ad == "saturn");
                    var e = Ekle(kat, ad, "gok", prefab, ad == "saturn" ? 1.5f : 1.1f, slug);
                    e.havada = true;
                    var arka = GezegenResmi(prefab, gokyuzu);
                    Kaydet(kat, "uzay_" + e.id, arka);
                    Object.DestroyImmediate(arka);
                }
                else
                {
                    var tex = Oku(f);
                    var kirp = Kirp(tex);
                    Kaydet(kat, "uzay_" + ad, kirp);
                    Object.DestroyImmediate(kirp);
                    Object.DestroyImmediate(tex);
                }
            }

            // gunes: dokusu yok, uretilir
            var gt = new Texture2D(512, 256, TextureFormat.RGB24, false);
            for (int y = 0; y < 256; y++)
                for (int x = 0; x < 512; x++)
                {
                    float a = x / 512f * Mathf.PI * 2f;
                    float g = Mathf.PerlinNoise(Mathf.Cos(a) * 9f + 20f + y * 0.07f, Mathf.Sin(a) * 9f + 20f) * 0.6f +
                              Mathf.PerlinNoise(x * 0.09f, y * 0.09f) * 0.4f;
                    gt.SetPixel(x, y, Color.Lerp(new Color(0.95f, 0.38f, 0.02f), new Color(1f, 0.93f, 0.45f), Mathf.Clamp01((g - 0.25f) * 1.9f)));
                }
            var gunesYol = Kok + "/Gok/Cisim/gunes_doku.png";
            File.WriteAllBytes(gunesYol, gt.EncodeToPNG());
            Object.DestroyImmediate(gt);
            AssetDatabase.ImportAsset(gunesYol);
            var gunes = Kure("gunes", Malzeme(Kok + "/Gok/Cisim/gunes.mat", "Universal Render Pipeline/Unlit",
                                              AssetDatabase.LoadAssetAtPath<Texture2D>(gunesYol), Color.white), false);
            Ekle(kat, "gunes", "gok", gunes, 1.1f, "sun").havada = true;
            var ga = GezegenResmi(gunes, gokyuzu);
            Kaydet(kat, "uzay_gunes", ga);
            Object.DestroyImmediate(ga);
            Object.DestroyImmediate(gokyuzu);
            rapor.AppendLine("Uzay mekânı: " + kat.gokler.Count);
        }

        static Texture2D Oku(string yol)
        {
            var t = new Texture2D(2, 2, TextureFormat.RGB24, false);
            t.LoadImage(File.ReadAllBytes(yol));
            return t;
        }

        // Ortadan 9:16 kirpip 1080x1920 yapar
        static Texture2D Kirp(Texture2D kaynak)
        {
            const float Hedef = 1080f / 1920f;
            float oran = kaynak.width / (float)kaynak.height;
            Vector2 olcek = oran > Hedef ? new Vector2(Hedef / oran, 1f) : new Vector2(1f, oran / Hedef);
            var kayma = (Vector2.one - olcek) * 0.5f;
            var rt = RenderTexture.GetTemporary(1080, 1920, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var onceki = RenderTexture.active;
            Graphics.Blit(kaynak, rt, olcek, kayma);
            RenderTexture.active = rt;
            var t = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
            t.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
            t.Apply();
            RenderTexture.active = onceki;
            RenderTexture.ReleaseTemporary(rt);
            return t;
        }

        static void Kaydet(NesneKatalogu kat, string id, Texture2D t)
        {
            var temel = id; int i = 2;
            while (kat.gokler.Any(g => g.id == id)) id = temel + "_" + i++;
            var yol = Kok + "/Gok/Arka/" + id + ".png";
            File.WriteAllBytes(yol, t.EncodeToPNG());
            AssetDatabase.ImportAsset(yol, ImportAssetOptions.ForceSynchronousImport);
            var ti = AssetImporter.GetAtPath(yol) as TextureImporter;
            if (ti != null && (ti.textureType != TextureImporterType.Sprite || ti.spriteImportMode != SpriteImportMode.Single))
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.mipmapEnabled = false;
                ti.maxTextureSize = 2048;
                ti.SaveAndReimport();
            }
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(yol);
            if (s == null) { rapor.AppendLine("Arka plan sprite olmadı: " + id); return; }
            kat.gokler.Add(new NesneKatalogu.Gok { id = id, resim = s });
        }

        static GameObject Kure(string ad, Material m, bool halka)
        {
            var kok = new GameObject(ad);
            var k = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(k.GetComponent<Collider>());
            k.transform.SetParent(kok.transform, false);
            k.GetComponent<MeshRenderer>().sharedMaterial = m;
            if (halka)
            {
                var h = new GameObject("Halka");
                h.transform.SetParent(kok.transform, false);
                h.transform.localRotation = Quaternion.Euler(-22f, 0f, 12f);
                h.AddComponent<MeshFilter>().sharedMesh = HalkaMesh();
                h.AddComponent<MeshRenderer>().sharedMaterial = Malzeme(Kok + "/Gok/Cisim/halka.mat",
                    "Universal Render Pipeline/Unlit", null, new Color(0.76f, 0.68f, 0.52f));
            }
            var p = PrefabUtility.SaveAsPrefabAsset(kok, Kok + "/Gok/Cisim/" + ad + ".prefab");
            Object.DestroyImmediate(kok);
            return p;
        }

        static Mesh HalkaMesh()
        {
            var yol = Kok + "/Gok/Cisim/halka_mesh.asset";
            var eski = AssetDatabase.LoadAssetAtPath<Mesh>(yol);
            if (eski != null) return eski;
            var v = new List<Vector3>(); var t = new List<int>();
            var bantlar = new[] { new Vector2(0.66f, 0.76f), new Vector2(0.78f, 0.92f), new Vector2(0.95f, 1.02f) };
            const int N = 96;
            foreach (var b in bantlar)
            {
                int bas = v.Count;
                for (int i = 0; i <= N; i++)
                {
                    float a = i * Mathf.PI * 2f / N;
                    var yon = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    v.Add(yon * b.x); v.Add(yon * b.y);
                }
                for (int i = 0; i < N; i++)
                {
                    int q = bas + i * 2;
                    t.AddRange(new[] { q, q + 2, q + 1, q + 1, q + 2, q + 3 });   // ust
                    t.AddRange(new[] { q, q + 1, q + 2, q + 1, q + 3, q + 2 });   // alt
                }
            }
            var m = new Mesh { name = "Halka" };
            m.SetVertices(v); m.SetTriangles(t, 0); m.RecalculateNormals(); m.RecalculateBounds();
            AssetDatabase.CreateAsset(m, yol);
            return m;
        }

        // Yildizli gok onunde, ust yarida gezegen
        static Texture2D GezegenResmi(GameObject prefab, Texture2D gokyuzu)
        {
            var pru = new PreviewRenderUtility();
            var zemin = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            zemin.SetTexture("_BaseMap", gokyuzu);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.GetComponent<MeshRenderer>().sharedMaterial = zemin;
            GameObject go = null;
            try
            {
                pru.BeginStaticPreview(new Rect(0, 0, 1080, 1920));
                pru.AddSingleGO(quad);
                quad.transform.position = new Vector3(0f, 0f, 5f);
                quad.transform.localScale = new Vector3(4.55f, 8.08f, 1f);
                go = pru.InstantiatePrefabInScene(prefab);
                bool halkali = go.transform.childCount > 1;
                go.transform.position = new Vector3(0f, 0.9f, 0f);
                go.transform.localScale = Vector3.one * (halkali ? 1.35f : 2.1f);
                pru.camera.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
                pru.camera.fieldOfView = 30f;
                pru.camera.nearClipPlane = 0.1f; pru.camera.farClipPlane = 40f;
                pru.camera.clearFlags = CameraClearFlags.SolidColor;
                pru.camera.backgroundColor = Color.black;
                pru.lights[0].intensity = 1.35f; pru.lights[0].transform.rotation = Quaternion.Euler(15f, 35f, 0f);
                pru.lights[1].intensity = 0f;
                pru.Render(true);
                var kare = pru.EndStaticPreview();
                return kare;
            }
            finally
            {
                if (go != null) Object.DestroyImmediate(go);
                Object.DestroyImmediate(quad);
                Object.DestroyImmediate(zemin);
                pru.Cleanup();
            }
        }

        // ---------------- masal karakterleri (KayKit) ----------------
        static void MasalKarakterleri()
        {
            var tablo = new[]
            {
                new[] { "KayKit_Macera", "Barbarian", "Barbar" }, new[] { "KayKit_Macera", "Knight", "Sovalye" },
                new[] { "KayKit_Macera", "Mage", "Buyucu" }, new[] { "KayKit_Macera", "Rogue", "Hirsiz" },
                new[] { "KayKit_Macera", "RogueHooded", "Kapusonlu" },
                new[] { "KayKit_Iskelet", "Skeleton_Warrior", "IskeletSavasci" }, new[] { "KayKit_Iskelet", "Skeleton_Mage", "IskeletBuyucu" },
                new[] { "KayKit_Iskelet", "Skeleton_Minion", "IskeletUsak" }, new[] { "KayKit_Iskelet", "Skeleton_Rogue", "IskeletHirsiz" },
            };
            Klasor(Hazir);
            int n = 0;
            foreach (var s in tablo)
            {
                var yol = Fbxler(s[0]).FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), s[1], System.StringComparison.OrdinalIgnoreCase));
                if (yol == null) { rapor.AppendLine("Masal karakteri yok: " + s[1]); continue; }
                var mi = AssetImporter.GetAtPath(yol) as ModelImporter;
                if (mi != null)
                {
                    bool degisti = false;
                    var m0 = AssetDatabase.LoadAssetAtPath<GameObject>(yol);
                    float h = GovdeBoyu(m0);
                    // kafalari buyuk (cizgi) modeller: kask haric govde 1.45 m yeterli
                    if (h > 0.01f && Mathf.Abs(h - 1.45f) > 0.1f) { mi.globalScale *= 1.45f / h; degisti = true; }
                    var klipler = mi.clipAnimations.Length > 0 ? mi.clipAnimations : mi.defaultClipAnimations;
                    foreach (var c in klipler)
                    {
                        var a = c.name.ToLowerInvariant();
                        bool dongu = a.Contains("idle") || a.Contains("walk") || a.Contains("running") || a.Contains("cheer");
                        if (dongu && !c.loopTime) { c.loopTime = true; degisti = true; }
                    }
                    if (degisti) { mi.clipAnimations = klipler; mi.SaveAndReimport(); }
                }

                var src = AssetDatabase.LoadAssetAtPath<GameObject>(yol);
                var clips = AssetDatabase.LoadAllAssetsAtPath(yol).OfType<AnimationClip>()
                    .Where(c => !c.name.StartsWith("__preview__")).OrderBy(c => c.name, System.StringComparer.Ordinal).ToList();
                var ad = "Masal_" + s[2];
                var go = (GameObject)Object.Instantiate(src);
                go.name = ad;
                try
                {
                    // elde ust uste duran butun silah/kalkanlar gizlenir; sapka, kask, pelerin kalir
                    foreach (var r in go.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var a = r.name.ToLowerInvariant();
                        if (!(a.Contains("helmet") || a.Contains("hat") || a.Contains("cape") || a.Contains("hood") || a.Contains("head") || a.Contains("eyes") || a.Contains("jaw")))
                            r.gameObject.SetActive(false);
                    }
                    var an = go.GetComponentInChildren<Animator>();
                    if (an == null) an = go.AddComponent<Animator>();
                    an.applyRootMotion = false;
                    an.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    var k = go.AddComponent<Karakter3D>();
                    k.id = ad; k.kategori = "Masal"; k.clips = clips; k.startClip = "Idle";
                    PrefabUtility.SaveAsPrefabAsset(go, Hazir + "/" + ad + ".prefab");
                    n++;
                }
                finally { Object.DestroyImmediate(go); }
            }
            rapor.AppendLine("Masal karakteri: " + n);
        }

        static float GovdeBoyu(GameObject prefab)
        {
            if (prefab == null) return 0f;
            var go = Object.Instantiate(prefab);
            try
            {
                var rs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (rs.Length == 0) return 0f;
                var b = rs[0].bounds;
                foreach (var r in rs) b.Encapsulate(r.bounds);
                return b.size.y;
            }
            finally { Object.DestroyImmediate(go); }
        }

        // ---------------- gercekci karakter hareketleri ----------------
        [MenuItem("Hikaye/Gercek Hareketleri Ekle", priority = 45)]
        public static void Hareketler()
        {
            var yol = Path.Combine(UzaktanKomut.Dir, "hareket_rapor.txt");
            var sb = new StringBuilder();
            try
            {
                string proje = Directory.GetParent(Application.dataPath).FullName;
                var kaynak = Path.Combine(proje, "KutuphaneHam", "Rocketbox", "Animations", "all_animations_max_motextr_static");
                if (!Directory.Exists(kaynak)) { sb.AppendLine("Klasör yok: " + kaynak); return; }
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Hareketler kopyalanıyor...", 0.1f);
                int n = 0, kopya = 0;
                foreach (var f in Directory.GetFiles(kaynak, "*.fbx"))
                {
                    var ad = Path.GetFileName(f);
                    if (!ad.StartsWith("f_") && !ad.StartsWith("m_")) continue;
                    var kisa = ad.Substring(2).Replace(".max.fbx", ".fbx");
                    kopya += Al(f, Karakter3DKurulum.RbAnimDir + "/" + ad[0] + "/" + kisa);
                    n++;
                }
                sb.AppendLine("Hareket dosyası: " + n + " (yeni kopya " + kopya + ")");
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Hareketler içe aktarılıyor (uzun sürer)...", 0.3f);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                int insan = 0, degil = 0;
                foreach (var f in Directory.GetFiles(Karakter3DKurulum.RbAnimDir, "*.fbx", SearchOption.AllDirectories))
                    foreach (var c in AssetDatabase.LoadAllAssetsAtPath(Posix(f)).OfType<AnimationClip>())
                    {
                        if (c.name.StartsWith("__preview__")) continue;
                        if (c.humanMotion) insan++;
                        else { degil++; if (degil <= 10) sb.AppendLine("Humanoid olmadı: " + f); }
                    }
                sb.AppendLine("Humanoid klip: " + insan + ", olmayan: " + degil);
                EditorUtility.DisplayProgressBar("Hikaye Stüdyosu", "Gerçekçi karakterler güncelleniyor...", 0.7f);
                sb.AppendLine("Güncellenen karakter: " + Karakter3DKurulum.GercekKur(true));
                HareketGalerisi();
            }
            catch (System.Exception e) { sb.AppendLine("HATA: " + e); Debug.LogException(e); }
            finally
            {
                EditorUtility.ClearProgressBar();
                File.WriteAllText(yol, sb.ToString());
            }
        }

        // Iki karakter x secme hareketler: poz kontrol resmi
        static void HareketGalerisi()
        {
            var adlar = new[] { "idle", "konus", "otur", "el_salla", "dans", "sevin", "alkis", "kahkaha", "telefon", "etrafa_bak" };
            var kisiler = new[] { "Gercek_Kadin01", "Gercek_Erkek01" };
            const int G = 220, Y = 360;
            var tablo = new Texture2D(G * adlar.Length, Y * kisiler.Length, TextureFormat.RGB24, false);
            var pru = new PreviewRenderUtility();
            try
            {
                for (int s = 0; s < kisiler.Length; s++)
                {
                    var p = AssetDatabase.LoadAssetAtPath<GameObject>(Hazir + "/" + kisiler[s] + ".prefab");
                    if (p == null) continue;
                    for (int i = 0; i < adlar.Length; i++)
                    {
                        pru.BeginStaticPreview(new Rect(0, 0, G, Y));
                        var go = pru.InstantiatePrefabInScene(p);
                        try
                        {
                            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                            var clip = go.GetComponent<Karakter3D>().Find(adlar[i]);
                            if (clip != null) clip.SampleAnimation(go, 0f);
                            var an = go.GetComponent<Animator>();
                            var graph = UnityEngine.Playables.PlayableGraph.Create("HareketGaleri");
                            try
                            {
                                if (clip != null)
                                {
                                    graph.SetTimeUpdateMode(UnityEngine.Playables.DirectorUpdateMode.Manual);
                                    var cp = UnityEngine.Animations.AnimationClipPlayable.Create(graph, clip);
                                    UnityEngine.Animations.AnimationPlayableOutput.Create(graph, "o", an).SetSourcePlayable(cp);
                                    UnityEngine.Playables.PlayableExtensions.SetTime(cp, clip.length * 0.4f);
                                    graph.Evaluate(0f);
                                }
                                pru.camera.transform.SetPositionAndRotation(new Vector3(0f, 0.95f, 4.2f), Quaternion.Euler(0f, 180f, 0f));
                                pru.camera.fieldOfView = 30f;
                                pru.camera.nearClipPlane = 0.05f; pru.camera.farClipPlane = 20f;
                                pru.camera.clearFlags = CameraClearFlags.SolidColor;
                                pru.camera.backgroundColor = new Color(0.45f, 0.45f, 0.5f);
                                pru.lights[0].intensity = 1.1f; pru.lights[0].transform.rotation = Quaternion.Euler(30f, 160f, 0f);
                                pru.lights[1].intensity = 0.6f; pru.lights[1].transform.rotation = Quaternion.Euler(10f, 220f, 0f);
                                pru.Render(true);
                                var kare = pru.EndStaticPreview();
                                tablo.SetPixels(i * G, (kisiler.Length - 1 - s) * Y, G, Y, kare.GetPixels(0, 0, G, Y));
                                Object.DestroyImmediate(kare);
                            }
                            finally { graph.Destroy(); }
                        }
                        finally { Object.DestroyImmediate(go); }
                    }
                }
                tablo.Apply();
                File.WriteAllBytes(Path.Combine(UzaktanKomut.Dir, "hareket_galeri.png"), tablo.EncodeToPNG());
            }
            finally { pru.Cleanup(); Object.DestroyImmediate(tablo); }
        }

        // ---------------- kontrol galerisi ----------------
        static void Galeri(NesneKatalogu kat)
        {
            const int G = 200, Sutun = 10, Sira = 8, Sayfa = Sutun * Sira;
            var sira = new StringBuilder();
            var pru = new PreviewRenderUtility();
            try
            {
                for (int s = 0; s * Sayfa < kat.nesneler.Count; s++)
                {
                    var tablo = new Texture2D(G * Sutun, G * Sira, TextureFormat.RGB24, false);
                    tablo.SetPixels(Enumerable.Repeat(Color.black, tablo.width * tablo.height).ToArray());
                    for (int i = 0; i < Sayfa && s * Sayfa + i < kat.nesneler.Count; i++)
                    {
                        var n = kat.nesneler[s * Sayfa + i];
                        sira.AppendLine((s + 1) + " " + (i / Sutun + 1) + "." + (i % Sutun + 1) + " " + n.id);
                        if (n.model == null) continue;
                        pru.BeginStaticPreview(new Rect(0, 0, G, G));
                        var go = pru.InstantiatePrefabInScene(n.model);
                        try
                        {
                            go.transform.position = Vector3.zero;
                            go.transform.rotation = Quaternion.Euler(0f, n.donus, 0f) * go.transform.rotation;
                            Bounds b;
                            if (!Sinir(go, out b)) continue;
                            float r = Mathf.Max(0.0001f, b.extents.magnitude);
                            go.transform.localScale *= 1f / r;
                            Sinir(go, out b);
                            go.transform.position -= b.center;
                            pru.camera.transform.SetPositionAndRotation(new Vector3(0f, 0.9f, -3.9f), Quaternion.Euler(13f, 0f, 0f));
                            pru.camera.fieldOfView = 30f;
                            pru.camera.nearClipPlane = 0.05f; pru.camera.farClipPlane = 30f;
                            pru.camera.clearFlags = CameraClearFlags.SolidColor;
                            pru.camera.backgroundColor = new Color(0.4f, 0.4f, 0.46f);
                            pru.lights[0].intensity = 1.15f; pru.lights[0].transform.rotation = Quaternion.Euler(35f, -25f, 0f);
                            pru.lights[1].intensity = 0.5f; pru.lights[1].transform.rotation = Quaternion.Euler(20f, 150f, 0f);
                            pru.Render(true);
                            var kare = pru.EndStaticPreview();
                            tablo.SetPixels((i % Sutun) * G, (Sira - 1 - i / Sutun) * G, G, G, kare.GetPixels(0, 0, G, G));
                            Object.DestroyImmediate(kare);
                        }
                        finally { Object.DestroyImmediate(go); }
                    }
                    tablo.Apply();
                    File.WriteAllBytes(Path.Combine(UzaktanKomut.Dir, "nesne_galeri_" + (s + 1) + ".png"), tablo.EncodeToPNG());
                    Object.DestroyImmediate(tablo);
                }
            }
            finally { pru.Cleanup(); }
            File.WriteAllText(Path.Combine(UzaktanKomut.Dir, "nesne_sira.txt"), sira.ToString());
        }
    }

    // Rocketbox hareket dosyalari: Humanoid, klip adi = dosya adi (ana hareketler ortak adlarla)
    class NesneAnimIceAktarma : AssetPostprocessor
    {
        bool Bizim { get { return assetPath.Replace('\\', '/').StartsWith(Karakter3DKurulum.RbAnimDir + "/"); } }

        void OnPreprocessModel()
        {
            if (!Bizim) return;
            var mi = (ModelImporter)assetImporter;
            mi.animationType = ModelImporterAnimationType.Human;
            mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.animationCompression = ModelImporterAnimationCompression.Optimal;
            mi.importAnimation = true;
        }

        void OnPreprocessAnimation()
        {
            if (!Bizim) return;
            var mi = (ModelImporter)assetImporter;
            var k = mi.defaultClipAnimations;
            if (k.Length == 0) return;
            var ad = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var c = k[0];
            c.name = ad == "idle_neutral_01" ? "Idle_Loop" : ad == "gestic_talk_neutral_01" ? "Idle_Talking_Loop"
                   : ad == "sit_chair_idle_neutral_01" ? "Sitting_Idle_Loop" : ad == "wave_01" ? "Wave"
                   : ad == "dancing_neutral_01" ? "Dance_Loop" : ad == "crouch_idle_01" ? "Crouch_Idle_Loop" : ad;
            c.loopTime = !(ad.Contains("_in") || ad.Contains("_out") || ad.StartsWith("try_door"));
            c.lockRootRotation = true; c.lockRootHeightY = true; c.lockRootPositionXZ = true;
            c.keepOriginalOrientation = true; c.keepOriginalPositionY = true; c.keepOriginalPositionXZ = true;
            mi.clipAnimations = new[] { c };
        }
    }
}
