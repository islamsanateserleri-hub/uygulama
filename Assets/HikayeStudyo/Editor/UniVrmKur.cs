using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // UniVRM (VRoid karakterleri için resmi paket) indirir ve içe aktarır. Kullanıcı onayı: 25 Kas 2026.
    // Köprüden: menu: Hikaye/UniVRM Kur  ->  HikayeKomut/univrm_kur.txt
    // İlk çalıştırma yalnız derleme tanımlarını ekler (glTFast ile .glb çakışmasını önler),
    // derleme bitince aynı menü ikinci kez çalıştırılır.
    // Geri alma: menu: Hikaye/UniVRM Kaldir (eklenen klasörleri HikayeKomut/univrm_geri/ altına taşır)
    public static class UniVrmKur
    {
        const string Surum = "v0.131.4";
        const string Dosya = "VRM-0.131.4_efd2.unitypackage";
        const string Adres = "https://github.com/vrm-c/UniVRM/releases/download/" + Surum + "/" + Dosya;

        static readonly string[] Tanimlar = { "UNIGLTF_DISABLE_DEFAULT_GLB_IMPORTER", "UNIGLTF_DISABLE_DEFAULT_GLTF_IMPORTER" };

        static UnityWebRequest istek;

        // Eksik derleme tanımlarını ekler; bir şey eklediyse true döner (yeniden derleme gerekir)
        static bool TanimEkle()
        {
            var hedef = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            var l = new List<string>(PlayerSettings.GetScriptingDefineSymbols(hedef).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            bool degisti = false;
            foreach (var t in Tanimlar)
                if (!l.Contains(t)) { l.Add(t); degisti = true; }
            if (!degisti) return false;
            PlayerSettings.SetScriptingDefineSymbols(hedef, string.Join(";", l.ToArray()));
            AssetDatabase.SaveAssets();
            CompilationPipeline.RequestScriptCompilation();
            return true;
        }

        static string Kok { get { return Directory.GetCurrentDirectory(); } }
        static string Durum { get { return Path.Combine(Kok, "HikayeKomut/univrm_kur.txt"); } }
        static string Liste { get { return Path.Combine(Kok, "HikayeKomut/univrm_onceki.txt"); } }

        static void Yaz(string s)
        {
            File.AppendAllText(Durum, System.DateTime.Now.ToString("HH:mm:ss") + " " + s + "\n", new UTF8Encoding(false));
            Debug.Log("[UniVRM] " + s);
        }

        // Assets ve Packages altındaki üst klasör/dosyalar
        static List<string> UstKlasorler()
        {
            var l = new List<string>();
            foreach (var kok in new[] { "Assets", "Packages" })
                foreach (var d in Directory.GetDirectories(Path.Combine(Kok, kok)))
                    l.Add(kok + "/" + Path.GetFileName(d));
            return l;
        }

        [MenuItem("Hikaye/UniVRM Kur")]
        public static void Kur()
        {
            if (istek != null) { Yaz("zaten indiriliyor"); return; }
            File.WriteAllText(Durum, "", new UTF8Encoding(false));
            if (TanimEkle()) { Yaz("derleme tanımları eklendi; derleme bitince menüyü tekrar çalıştır"); return; }
            if (!File.Exists(Liste)) File.WriteAllText(Liste, string.Join("\n", UstKlasorler().ToArray()), new UTF8Encoding(false));
            var klasor = Path.Combine(Kok, "HikayeKomut/indirme");
            Directory.CreateDirectory(klasor);
            var hedef = Path.Combine(klasor, Dosya);
            if (File.Exists(hedef) && new FileInfo(hedef).Length > 100000) { Yaz("dosya zaten indirilmiş"); Aktar(hedef); return; }
            Yaz("indiriliyor: " + Adres);
            istek = UnityWebRequest.Get(Adres);
            istek.downloadHandler = new DownloadHandlerFile(hedef) { removeFileOnAbort = true };
            istek.timeout = 600;
            istek.SendWebRequest();
            EditorApplication.update += Izle;
        }

        static void Izle()
        {
            if (istek == null) { EditorApplication.update -= Izle; return; }
            if (!istek.isDone) return;
            EditorApplication.update -= Izle;
            var hedef = Path.Combine(Kok, "HikayeKomut/indirme/" + Dosya);
            bool tamam = istek.result == UnityWebRequest.Result.Success;
            string hata = tamam ? "" : istek.error + " (HTTP " + istek.responseCode + ")";
            istek.Dispose();
            istek = null;
            if (!tamam) { Yaz("HATA indirme: " + hata); if (File.Exists(hedef)) File.Delete(hedef); return; }
            Aktar(hedef);
        }

        static void Aktar(string yol)
        {
            var bayt = new FileInfo(yol).Length;
            // unitypackage = gzip; ilk iki bayt 1F 8B olmalı
            using (var f = File.OpenRead(yol))
                if (f.ReadByte() != 0x1f || f.ReadByte() != 0x8b) { Yaz("HATA: inen dosya paket değil (" + bayt + " bayt)"); return; }
            Yaz("indi: " + bayt + " bayt; açılıyor");
            // AssetDatabase.ImportPackage bu Unity sürümünde sonuç vermedi; paket doğrudan açılıyor
            try { Ac(yol); }
            catch (System.Exception e) { Yaz("HATA açma: " + e.Message); return; }
            Coz();
        }

        // Packages/ altına yeni eklenen paketleri Paket Yöneticisine tanıtır
        [MenuItem("Hikaye/UniVRM Paketleri Tani")]
        public static void Coz()
        {
            UnityEditor.PackageManager.Client.Resolve();
            AssetDatabase.Refresh();
            Yaz("paket tanıma ve yenileme çağrıldı; derleme sonucu derleme.txt dosyasında");
        }

        class Girdi { public byte[] icerik; public byte[] meta; public string yol; }

        // .unitypackage = tar.gz; her varlık <guid>/asset, <guid>/asset.meta, <guid>/pathname
        static void Ac(string paket)
        {
            var girdiler = new Dictionary<string, Girdi>();
            using (var f = File.OpenRead(paket))
            using (var gz = new GZipStream(f, CompressionMode.Decompress))
            {
                var bas = new byte[512];
                while (Oku(gz, bas, 512))
                {
                    var ad = Metin(bas, 0, 100);
                    if (ad.Length == 0) break;
                    var boyMetin = Metin(bas, 124, 12).Trim();
                    long boy = boyMetin.Length == 0 ? 0 : System.Convert.ToInt64(boyMetin, 8);
                    var veri = new byte[boy];
                    if (!Oku(gz, veri, (int)boy)) throw new IOException("paket yarım: " + ad);
                    int dolgu = (int)((512 - boy % 512) % 512);
                    if (!Oku(gz, new byte[dolgu], dolgu)) throw new IOException("paket yarım: " + ad);
                    char tur = (char)bas[156];
                    if (tur != '0' && tur != '\0') continue;
                    if (ad.StartsWith("./")) ad = ad.Substring(2);
                    int k = ad.IndexOf('/');
                    if (k < 0) continue;
                    var guid = ad.Substring(0, k);
                    var parca = ad.Substring(k + 1);
                    Girdi g;
                    if (!girdiler.TryGetValue(guid, out g)) { g = new Girdi(); girdiler[guid] = g; }
                    if (parca == "asset") g.icerik = veri;
                    else if (parca == "asset.meta") g.meta = veri;
                    else if (parca == "pathname") g.yol = Encoding.UTF8.GetString(veri).Split('\n')[0].Trim();
                }
            }

            var yazilan = new List<string>();
            var ustler = new SortedDictionary<string, int>();
            int atlanan = 0;
            foreach (var g in girdiler.Values)
            {
                var y = g.yol;
                if (string.IsNullOrEmpty(y) || !(y.StartsWith("Assets/") || y.StartsWith("Packages/")) || y.Contains("..") || y.Contains(":") || y.Contains("\\")) { atlanan++; continue; }
                var tam = Path.Combine(Kok, y);
                if (g.icerik != null)
                {
                    if (File.Exists(tam)) { atlanan++; continue; } // var olan dosyanın üstüne yazılmaz
                    Directory.CreateDirectory(Path.GetDirectoryName(tam));
                    File.WriteAllBytes(tam, g.icerik);
                    yazilan.Add(y);
                }
                else Directory.CreateDirectory(tam);
                if (g.meta != null && !File.Exists(tam + ".meta"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tam + ".meta"));
                    File.WriteAllBytes(tam + ".meta", g.meta);
                }
                var bol = y.Split('/');
                var ust = bol.Length > 1 ? bol[0] + "/" + bol[1] : y;
                if (!ustler.ContainsKey(ust)) ustler[ust] = 0;
                ustler[ust]++;
            }
            File.WriteAllText(Path.Combine(Kok, "HikayeKomut/univrm_dosyalar.txt"), string.Join("\n", yazilan.ToArray()), new UTF8Encoding(false));
            Yaz("yazılan dosya=" + yazilan.Count + " atlanan=" + atlanan);
            foreach (var u in ustler) Yaz("  " + u.Key + " (" + u.Value + ")");
        }

        static bool Oku(Stream s, byte[] hedef, int n)
        {
            int o = 0;
            while (o < n)
            {
                int r = s.Read(hedef, o, n - o);
                if (r <= 0) return false;
                o += r;
            }
            return true;
        }

        static string Metin(byte[] b, int bas, int boy)
        {
            int n = 0;
            while (n < boy && b[bas + n] != 0) n++;
            return Encoding.UTF8.GetString(b, bas, n);
        }

        [MenuItem("Hikaye/UniVRM Kaldir")]
        public static void Kaldir()
        {
            if (!File.Exists(Liste)) { Yaz("kaldırılacak kayıt yok"); return; }
            var once = new HashSet<string>(File.ReadAllLines(Liste));
            var geri = Path.Combine(Kok, "HikayeKomut/univrm_geri/" + System.DateTime.Now.ToString("HHmmss"));
            int n = 0;
            foreach (var k in UstKlasorler())
            {
                if (once.Contains(k)) continue;
                var ad = k.ToLowerInvariant();
                if (!(ad.Contains("vrm") || ad.Contains("gltf"))) continue;
                var hedef = Path.Combine(geri, k);
                Directory.CreateDirectory(Path.GetDirectoryName(hedef));
                Directory.Move(Path.Combine(Kok, k), hedef);
                var meta = Path.Combine(Kok, k + ".meta");
                if (File.Exists(meta)) File.Move(meta, hedef + ".meta");
                Yaz("taşındı: " + k);
                n++;
            }
            Yaz("kaldırma bitti, klasör sayısı=" + n);
            AssetDatabase.Refresh();
        }
    }
}
