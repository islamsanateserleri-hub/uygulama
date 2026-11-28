using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 7: API anahtarini Windows DPAPI ile (yalniz bu Windows kullanicisinin cozebilecegi sekilde) saklar.
    // Paket ya da ek referans gerektirmez (crypt32.dll). Not: ayni Windows kullanicisiyla calisan baska bir program da cozebilir.
    public static class SirKoruma
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [StructLayout(LayoutKind.Sequential)]
        struct Blob { public int uzunluk; public IntPtr veri; }

        const int ArayuzYok = 0x1;   // CRYPTPROTECT_UI_FORBIDDEN

        [DllImport("crypt32.dll", SetLastError = true)]
        static extern bool CryptProtectData(ref Blob giris, IntPtr aciklama, IntPtr entropi, IntPtr ayrilmis, IntPtr istem, int bayrak, ref Blob cikis);
        [DllImport("crypt32.dll", SetLastError = true)]
        static extern bool CryptUnprotectData(ref Blob giris, IntPtr aciklama, IntPtr entropi, IntPtr ayrilmis, IntPtr istem, int bayrak, ref Blob cikis);
        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr bellek);

        static byte[] Donustur(byte[] veri, bool sifrele)
        {
            var giris = new Blob { uzunluk = veri.Length, veri = Marshal.AllocHGlobal(veri.Length) };
            var cikis = new Blob();
            try
            {
                Marshal.Copy(veri, 0, giris.veri, veri.Length);
                bool ok = sifrele
                    ? CryptProtectData(ref giris, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ArayuzYok, ref cikis)
                    : CryptUnprotectData(ref giris, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ArayuzYok, ref cikis);
                if (!ok || cikis.veri == IntPtr.Zero) return null;
                var sonuc = new byte[cikis.uzunluk];
                Marshal.Copy(cikis.veri, sonuc, 0, cikis.uzunluk);
                return sonuc;
            }
            finally
            {
                Marshal.FreeHGlobal(giris.veri);
                if (cikis.veri != IntPtr.Zero) LocalFree(cikis.veri);
            }
        }
#else
        static byte[] Donustur(byte[] veri, bool sifrele) { return null; }
#endif

        // Basarisizsa null doner (hata gizlenmez: cagiran taraf duz metni korur ve uyarir).
        public static string Koru(string duz)
        {
            if (string.IsNullOrEmpty(duz)) return null;
            try
            {
                var s = Donustur(Encoding.UTF8.GetBytes(duz), true);
                return s == null ? null : Convert.ToBase64String(s);
            }
            catch (Exception e) { Debug.LogWarning("[Hikaye] Anahtar şifrelenemedi: " + e.GetType().Name); return null; }
        }

        public static string Coz(string korumali)
        {
            if (string.IsNullOrEmpty(korumali)) return null;
            try
            {
                var s = Donustur(Convert.FromBase64String(korumali), false);
                return s == null ? null : Encoding.UTF8.GetString(s);
            }
            catch (Exception e) { Debug.LogWarning("[Hikaye] Anahtar çözülemedi: " + e.GetType().Name); return null; }
        }

        // ---------- PlayerPrefs'te saklama ----------
        // Duz metin kayit varsa (eski surum yazmis) sifreli kayda tasinir. Duz kayit, sifreli kopya geri okunup
        // ayni oldugu dogrulanmadan SILINMEZ.
        public static string Oku(string duzPref, string korumaliPref, out bool sifreli)
        {
            sifreli = false;
            string duz = PlayerPrefs.GetString(duzPref, "").Trim();
            if (duz.Length > 0)
            {
                sifreli = Yaz(duzPref, korumaliPref, duz);
                return duz;
            }
            string cozulen = Coz(PlayerPrefs.GetString(korumaliPref, ""));
            if (cozulen == null) return "";
            sifreli = true;
            return cozulen.Trim();
        }

        // true: sifreli saklandi (ya da bos deger silindi). false: sifrelenemedi, duz metin olarak saklandi.
        public static bool Yaz(string duzPref, string korumaliPref, string deger)
        {
            deger = (deger ?? "").Trim();
            if (deger.Length == 0)
            {
                PlayerPrefs.DeleteKey(duzPref);
                PlayerPrefs.DeleteKey(korumaliPref);
                PlayerPrefs.Save();
                return true;
            }
            string k = Koru(deger);
            if (k != null && Coz(k) == deger)
            {
                PlayerPrefs.SetString(korumaliPref, k);
                PlayerPrefs.DeleteKey(duzPref);
                PlayerPrefs.Save();
                return true;
            }
            PlayerPrefs.SetString(duzPref, deger);
            PlayerPrefs.DeleteKey(korumaliPref);
            PlayerPrefs.Save();
            return false;
        }
    }
}
