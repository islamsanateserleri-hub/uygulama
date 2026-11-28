using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace HikayeStudyo
{
    // Unreal Engine denemesi: Unreal'i bir Python betigiyle baslatir (uzaktan pencere kontrolu olmadigi icin).
    // HikayeKomut/unreal_arg.txt: UnrealEditor.exe'ye verilecek arguman satiri. Kopru: menu: Hikaye/Unreal Calistir
    public static class UnrealKopru
    {
        const string Exe = @"C:\Program Files\Epic Games\UE_5.8\Engine\Binaries\Win64\UnrealEditor.exe";

        [MenuItem("Hikaye/Unreal Calistir")]
        public static void Calistir()
        {
            string argDosya = Path.Combine(UzaktanKomut.Dir, "unreal_arg.txt");
            if (!File.Exists(Exe)) { UzaktanKomut.Append("sonuc.txt", "Unreal bulunamadı: " + Exe); return; }
            if (!File.Exists(argDosya)) { UzaktanKomut.Append("sonuc.txt", "unreal_arg.txt yok"); return; }
            string arg = File.ReadAllText(argDosya).Trim();
            var p = Process.Start(new ProcessStartInfo(Exe, arg) { UseShellExecute = false });
            UzaktanKomut.Append("sonuc.txt", "Unreal başlatıldı (pid " + (p != null ? p.Id.ToString() : "?") + "): " + arg);
        }
    }
}
