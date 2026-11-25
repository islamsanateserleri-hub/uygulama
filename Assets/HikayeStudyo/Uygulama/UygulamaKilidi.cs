using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HikayeStudyo
{
    // Kurulum programı (Installer/HikayeStudyosu.iss, AppMutex satırı) uygulamanın açık olduğunu bu adlı kilitten anlar
    // ve güncelleme/kaldırmadan önce kapatılmasını ister (Unity penceresi Windows'un otomatik kapatma isteğine uymuyor).
    // Kilit süreç bitene kadar tutulur; Windows süreç kapanınca kendisi bırakır. Kayıt süreci de aynı exe olduğu için alır.
    public static class UygulamaKilidi
    {
        public const string Ad = "HikayeStudyosu_Calisiyor";   // .iss içindeki AppMutex ile AYNI olmalı

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("kernel32.dll", EntryPoint = "CreateMutexW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr CreateMutex(IntPtr guvenlik, bool sahip, string ad);

        static IntPtr tutamac;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Al()
        {
            if (tutamac != IntPtr.Zero) return;
            tutamac = CreateMutex(IntPtr.Zero, false, Ad);
            if (tutamac == IntPtr.Zero) Debug.LogWarning("[Kilit] Oluşturulamadı, hata " + Marshal.GetLastWin32Error());
            else Debug.Log("[Kilit] " + Ad + " alındı");
        }
#endif
    }
}
