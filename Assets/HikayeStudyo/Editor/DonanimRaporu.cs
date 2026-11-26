using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Yalniz OKUR: islemci, bellek, ekran kartlari, disk bos alani -> HikayeKomut/donanim.txt
    public static class DonanimRaporu
    {
        [MenuItem("Hikaye/Donanim Raporu")]
        public static void Yaz()
        {
            var s = new StringBuilder();
            s.AppendLine("Unity GPU: " + SystemInfo.graphicsDeviceName + " | " + SystemInfo.graphicsMemorySize + " MB | " + SystemInfo.graphicsDeviceType);
            s.AppendLine("CPU: " + SystemInfo.processorType + " | cekirdek(thread) " + SystemInfo.processorCount);
            s.AppendLine("RAM MB: " + SystemInfo.systemMemorySize);
            s.AppendLine("Isletim: " + SystemInfo.operatingSystem + " | tur " + SystemInfo.deviceType + " | model " + SystemInfo.deviceModel);
            s.AppendLine("Ray tracing: " + SystemInfo.supportsRayTracing + " | compute: " + SystemInfo.supportsComputeShaders);
            try
            {
                var psi = new ProcessStartInfo("powershell",
                    "-NoProfile -Command \"Get-CimInstance Win32_VideoController | ForEach-Object { 'KART: ' + $_.Name + ' | surucu ' + $_.DriverVersion + ' | durum ' + $_.Status }; Get-PSDrive -PSProvider FileSystem | ForEach-Object { 'DISK ' + $_.Name + ': bos GB ' + [math]::Round($_.Free/1GB) }\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true };
                using (var p = Process.Start(psi))
                {
                    s.AppendLine(p.StandardOutput.ReadToEnd());
                    p.WaitForExit(20000);
                }
            }
            catch (System.Exception e) { s.AppendLine("kart listesi alinamadi: " + e.Message); }
            File.WriteAllText("HikayeKomut/donanim.txt", s.ToString(), Encoding.UTF8);
        }
    }
}
