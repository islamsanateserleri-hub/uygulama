using System;
using System.IO;
using UnityEngine;

namespace HikayeStudyo
{
    // Uygulama (exe) icin kalici ses klasoru: Documents\HikayeStudyosu\Ses\<hikaye>\<anahtar>.wav
    // Editor'de Resources/Ses kullanilir; exe'de burasi. Ayrica karakter -> Gemini sesi atamasi.
    public static class DisSes
    {
        public const string AtamaOn = "HS_SesAta_";

        static string kok;
        public static string Kok
        {
            get
            {
                if (string.IsNullOrEmpty(kok))
                {
                    try
                    {
                        kok = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HikayeStudyosu"), "Ses");
                    }
                    catch { kok = Path.Combine(Application.persistentDataPath, "Ses"); }
                }
                return kok;
            }
            set { kok = value; }
        }

        public static string Klasor(string hikaye)
        {
            return Path.Combine(Kok, Temiz(hikaye));
        }

        public static string Dosya(string hikaye, string anahtar)
        {
            return Path.Combine(Klasor(hikaye), anahtar + ".wav");
        }

        public static string DenemeDosya(string hikaye, string anahtar)
        {
            return Path.Combine(Path.Combine(Path.Combine(Kok, "_deneme"), Temiz(hikaye)), anahtar + ".wav");
        }

        public static bool Var(string hikaye, string anahtar)
        {
            try { return File.Exists(Dosya(hikaye, anahtar)); } catch { return false; }
        }

        static string Temiz(string s)
        {
            if (string.IsNullOrEmpty(s)) return "_";
            var sb = new System.Text.StringBuilder();
            foreach (char c in s) sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_');
            return sb.ToString();
        }

        // ---- Karakter -> ses atamasi (bos = karakterin kendi sesi) ----
        public static string Atama(string karakter, bool ic)
        {
            if (string.IsNullOrEmpty(karakter)) karakter = "_anlatici";
            return PlayerPrefs.GetString(AtamaOn + (ic ? "ic_" : "") + Util.Norm(karakter), "");
        }

        public static void AtamaYaz(string karakter, bool ic, string ses)
        {
            if (string.IsNullOrEmpty(karakter)) karakter = "_anlatici";
            string k = AtamaOn + (ic ? "ic_" : "") + Util.Norm(karakter);
            if (string.IsNullOrEmpty(ses)) PlayerPrefs.DeleteKey(k); else PlayerPrefs.SetString(k, ses);
        }

        // ---- WAV yukleme ----
        public static AudioClip Yukle(string hikaye, string anahtar)
        {
            string yol = Dosya(hikaye, anahtar);
            try
            {
                if (!File.Exists(yol))
                {
                    // Yalnız kullanıcı "deneme sesi" açtıysa: sahte ses (süre/miks denemesi için)
                    if (PlayerPrefs.GetInt("HS_DenemeSes", 0) != 1) return null;
                    yol = DenemeDosya(hikaye, anahtar);
                    if (!File.Exists(yol)) return null;
                }
                return WavCoz(File.ReadAllBytes(yol), anahtar);
            }
            catch (Exception e) { Debug.LogWarning("[Hikaye] Ses dosyası okunamadı: " + yol + " (" + e.Message + ")"); return null; }
        }

        // Yalnız "Dinle" düğmesi için: deneme (sentetik) dosyasını ayardan bağımsız yükler
        public static AudioClip YukleDeneme(string hikaye, string anahtar)
        {
            string yol = DenemeDosya(hikaye, anahtar);
            try { return File.Exists(yol) ? WavCoz(File.ReadAllBytes(yol), anahtar) : null; }
            catch (Exception e) { Debug.LogWarning("[Hikaye] Deneme sesi okunamadı: " + yol + " (" + e.Message + ")"); return null; }
        }

        // PCM16 / PCM float32 WAV -> AudioClip
        public static AudioClip WavCoz(byte[] b, string ad)
        {
            if (b == null || b.Length < 44) return null;
            if (b[0] != 'R' || b[1] != 'I' || b[2] != 'F' || b[3] != 'F') return null;
            int pos = 12, kanal = 1, hz = 24000, bit = 16, fmt = 1;
            int veriBas = -1, veriLen = 0;
            while (pos + 8 <= b.Length)
            {
                string id = "" + (char)b[pos] + (char)b[pos + 1] + (char)b[pos + 2] + (char)b[pos + 3];
                int len = BitConverter.ToInt32(b, pos + 4);
                int govde = pos + 8;
                if (id == "fmt ")
                {
                    fmt = BitConverter.ToInt16(b, govde);
                    kanal = BitConverter.ToInt16(b, govde + 2);
                    hz = BitConverter.ToInt32(b, govde + 4);
                    bit = BitConverter.ToInt16(b, govde + 14);
                }
                else if (id == "data")
                {
                    veriBas = govde;
                    veriLen = len;
                    if (veriLen < 0 || veriBas + veriLen > b.Length) veriLen = b.Length - veriBas;
                    break;
                }
                if (len < 0) break;
                pos = govde + len + (len & 1);
            }
            if (veriBas < 0 || kanal < 1 || hz < 8000) return null;
            int bytePerSample = bit / 8;
            if (bytePerSample < 2) return null;
            int adet = veriLen / bytePerSample;
            int kare = adet / kanal;
            if (kare <= 0) return null;
            var f = new float[kare * kanal];
            if (fmt == 3 && bit == 32)
                for (int i = 0; i < f.Length; i++) f[i] = BitConverter.ToSingle(b, veriBas + i * 4);
            else if (bit == 16)
                for (int i = 0; i < f.Length; i++) f[i] = BitConverter.ToInt16(b, veriBas + i * 2) / 32768f;
            else return null;
            var clip = AudioClip.Create(ad, kare, kanal, hz, false);
            clip.SetData(f, 0);
            return clip;
        }

        public static float WavSuresi(string yol)
        {
            try
            {
                using (var fs = new FileStream(yol, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var h = new byte[Math.Min(256, (int)fs.Length)];
                    fs.Read(h, 0, h.Length);
                    int pos = 12, hz = 24000, kanal = 1, bit = 16;
                    while (pos + 8 <= h.Length)
                    {
                        string id = "" + (char)h[pos] + (char)h[pos + 1] + (char)h[pos + 2] + (char)h[pos + 3];
                        int len = BitConverter.ToInt32(h, pos + 4);
                        if (id == "fmt ") { kanal = BitConverter.ToInt16(h, pos + 10); hz = BitConverter.ToInt32(h, pos + 12); bit = BitConverter.ToInt16(h, pos + 22); }
                        else if (id == "data") { int n = len; if (n < 0 || pos + 8 + n > fs.Length) n = (int)fs.Length - pos - 8; return n / Mathf.Max(1f, kanal * bit / 8f) / hz; }
                        pos += 8 + len + (len & 1);
                    }
                }
            }
            catch { }
            return 0f;
        }

        // PCM16 mono WAV baytlari
        public static byte[] WavYaz(byte[] pcm, int hz)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
                w.Write(36 + pcm.Length);
                w.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E', (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
                w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(hz); w.Write(hz * 2); w.Write((short)2); w.Write((short)16);
                w.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
                w.Write(pcm.Length); w.Write(pcm);
                return ms.ToArray();
            }
        }
    }
}
