using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // HikayeKomut/indir_listesi.txt icindeki dosyalari <proje>/KutuphaneHam/ altina indirir.
    // Assets disina indirilir: Unity ice aktarmaz, editor kilitlenmez. Yarim kalirsa kaldigi yerden surer.
    // Durum: HikayeKomut/indirme_durumu.txt   Menu: Hikaye/Kutuphane Indir, Hikaye/Kutuphane Indirmeyi Durdur
    public static class KutuphaneIndir
    {
        const string Kaynak = "https://raw.githubusercontent.com/";
        const long EnAzBos = 20L * 1024 * 1024 * 1024; // diskte en az 20 GB kalsin
        class Is { public string hedef, url; public int deneme; public UnityWebRequest istek; }
        static readonly Queue<Is> sira = new Queue<Is>();
        static readonly List<Is> etkin = new List<Is>();
        static int toplam, bitti, hata; static long bayt; static double sonRapor;

        static string Proje => Directory.GetParent(Application.dataPath).FullName;
        static string Bayrak => Path.Combine(Proje, "HikayeKomut", "indirme_aktif.txt");
        static string Durum => Path.Combine(Proje, "HikayeKomut", "indirme_durumu.txt");

        [InitializeOnLoadMethod]
        static void Surdur() { EditorApplication.delayCall += () => { if (File.Exists(Bayrak)) Baslat(); }; }

        [MenuItem("Hikaye/Kutuphane Indir")]
        public static void Baslat()
        {
            Durdur(false);
            string liste = Path.Combine(Proje, "HikayeKomut", "indir_listesi.txt");
            if (!File.Exists(liste)) { Debug.Log("[Hikaye] indir_listesi.txt yok."); return; }
            string kok = Path.GetFullPath(Path.Combine(Proje, "KutuphaneHam"));
            toplam = bitti = hata = 0; bayt = 0;
            foreach (var satir in File.ReadAllLines(liste))
            {
                var p = satir.Split('\t');
                if (p.Length != 2 || !p[1].StartsWith(Kaynak, StringComparison.Ordinal)) continue;
                string hedef = Path.GetFullPath(Path.Combine(kok, p[0]));
                if (!hedef.StartsWith(kok + Path.DirectorySeparatorChar, StringComparison.Ordinal)) continue;
                string uz = Path.GetExtension(hedef).ToLowerInvariant();
                if (uz != ".fbx" && uz != ".tga" && uz != ".png" && uz != ".jpg" && uz != ".glb" && uz != ".md" && uz != ".txt") continue;
                toplam++;
                if (File.Exists(hedef)) { bitti++; continue; }
                sira.Enqueue(new Is { hedef = hedef, url = p[1] });
            }
            File.WriteAllText(Bayrak, "1");
            EditorApplication.update += Adim;
            Yaz("basladi");
        }

        [MenuItem("Hikaye/Kutuphane Indirmeyi Durdur")]
        public static void DurdurMenu() { Durdur(true); Yaz("durduruldu"); }

        static void Durdur(bool bayragiKaldir)
        {
            EditorApplication.update -= Adim;
            foreach (var i in etkin) { i.istek.Abort(); i.istek.Dispose(); }
            etkin.Clear(); sira.Clear();
            if (bayragiKaldir && File.Exists(Bayrak)) File.Delete(Bayrak);
        }

        static void Adim()
        {
            try
            {
                for (int n = etkin.Count - 1; n >= 0; n--)
                {
                    var i = etkin[n];
                    if (!i.istek.isDone) continue;
                    bool tamam = i.istek.result == UnityWebRequest.Result.Success;
                    string mesaj = i.istek.error;
                    i.istek.Dispose();
                    etkin.RemoveAt(n);
                    string yarim = i.hedef + ".yarim";
                    if (tamam) { bayt += new FileInfo(yarim).Length; File.Move(yarim, i.hedef); bitti++; }
                    else if (++i.deneme < 3) sira.Enqueue(i);
                    else { hata++; File.AppendAllText(Path.Combine(Proje, "HikayeKomut", "indirme_hatalari.txt"), i.url + " : " + mesaj + "\n"); }
                }
                while (etkin.Count < 4 && sira.Count > 0)
                {
                    var i = sira.Dequeue();
                    Directory.CreateDirectory(Path.GetDirectoryName(i.hedef));
                    i.istek = new UnityWebRequest(i.url, UnityWebRequest.kHttpVerbGET);
                    i.istek.downloadHandler = new DownloadHandlerFile(i.hedef + ".yarim") { removeFileOnAbort = true };
                    i.istek.SendWebRequest();
                    etkin.Add(i);
                }
                if (EditorApplication.timeSinceStartup - sonRapor > 15)
                {
                    sonRapor = EditorApplication.timeSinceStartup;
                    var surucu = new DriveInfo(Path.GetPathRoot(Proje));
                    if (surucu.AvailableFreeSpace < EnAzBos) { Durdur(true); Yaz("DISK AZ: durduruldu"); return; }
                    Yaz("suruyor");
                }
                if (etkin.Count == 0 && sira.Count == 0) { Durdur(true); Yaz("BITTI"); }
            }
            catch (Exception e) { Durdur(true); Yaz("HATA: " + e.Message); }
        }

        static void Yaz(string s)
        {
            File.WriteAllText(Durum, DateTime.Now.ToString("HH:mm:ss ") + s + " | " + bitti + "/" + toplam + " dosya, hata " + hata + ", bu oturumda " + (bayt / (1024 * 1024)) + " MB\n");
        }
    }
}
