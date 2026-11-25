using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // HikayeKomut/indirilen/*.zip dosyalarini Assets/HikayeStudyo/Kutuphane/ altina acar.
    // Acilan zip silinmez, indirilen/acildi/ altina tasinir.
    // Rapor: HikayeKomut/kutuphane_rapor.txt
    public static class KutuphaneAc
    {
        [MenuItem("Hikaye/Kutuphane Ac")]
        public static void Ac()
        {
            string proje = Directory.GetParent(Application.dataPath).FullName;
            string gelen = Path.Combine(proje, "HikayeKomut", "indirilen");
            string hedef = Path.GetFullPath(Path.Combine(Application.dataPath, "HikayeStudyo", "Kutuphane"));
            string rapor = Path.Combine(proje, "HikayeKomut", "kutuphane_rapor.txt");
            if (!Directory.Exists(gelen)) { Debug.Log("[Hikaye] Kutuphane: gelen klasoru yok."); return; }
            Directory.CreateDirectory(hedef);
            string acildi = Path.Combine(gelen, "acildi");
            Directory.CreateDirectory(acildi);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var zip in Directory.GetFiles(gelen, "*.zip"))
                {
                    int sayi = 0, atlanan = 0;
                    try
                    {
                        using (var akis = File.OpenRead(zip))
                        using (var arsiv = new ZipArchive(akis, ZipArchiveMode.Read))
                        {
                            foreach (var e in arsiv.Entries)
                            {
                                if (string.IsNullOrEmpty(e.Name)) continue;
                                string yol = Path.GetFullPath(Path.Combine(hedef, e.FullName));
                                if (!yol.StartsWith(hedef + Path.DirectorySeparatorChar, StringComparison.Ordinal)) { atlanan++; continue; }
                                if (yol.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || yol.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) { atlanan++; continue; }
                                Directory.CreateDirectory(Path.GetDirectoryName(yol));
                                using (var kaynak = e.Open())
                                using (var cikti = File.Create(yol)) kaynak.CopyTo(cikti);
                                sayi++;
                            }
                        }
                        string yeni = Path.Combine(acildi, Path.GetFileName(zip));
                        if (File.Exists(yeni)) yeni = Path.Combine(acildi, DateTime.Now.Ticks + "_" + Path.GetFileName(zip));
                        File.Move(zip, yeni);
                        File.AppendAllText(rapor, DateTime.Now.ToString("HH:mm:ss ") + Path.GetFileName(zip) + ": " + sayi + " dosya acildi, " + atlanan + " atlandi\n");
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(rapor, DateTime.Now.ToString("HH:mm:ss ") + Path.GetFileName(zip) + ": HATA " + ex.Message + "\n");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            File.AppendAllText(rapor, DateTime.Now.ToString("HH:mm:ss ") + "Ice aktarma bitti\n");
        }
    }
}
