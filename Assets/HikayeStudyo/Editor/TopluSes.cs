using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace HikayeStudyo
{
    // Cok satiri TEK Gemini TTS isteginde seslendirir (gunluk ucretsiz kota az oldugu icin).
    // HikayeKomut/toplu_<n>.json : hazir istek govdesi. Cikti: toplu_<n>.wav, durum: toplu_durum.txt
    // Her calistirmada yalniz SIRADAKI bir dosya gonderilir; wav'i olan dosya yeniden istenmez.
    public static class TopluSes
    {
        const string Klasor = "HikayeKomut";
        static bool calisiyor;

        [Serializable] class Cevap { public Aday[] candidates; public Hata error; }
        [Serializable] class Aday { public Icerik content; }
        [Serializable] class Icerik { public Parca[] parts; }
        [Serializable] class Parca { public Veri inlineData; }
        [Serializable] class Veri { public string mimeType; public string data; }
        [Serializable] class Hata { public int code; public string message; }

        [MenuItem("Hikaye/Toplu Ses (siradaki istek)")]
        public static void Calistir()
        {
            if (calisiyor) { Yaz("zaten calisiyor"); return; }
            string siradaki = null;
            for (int i = 1; i <= 9; i++)
            {
                string j = Klasor + "/toplu_" + i + ".json";
                if (File.Exists(j) && !File.Exists(Klasor + "/toplu_" + i + ".wav")) { siradaki = j; break; }
            }
            if (siradaki == null) { Yaz("gonderilecek istek yok"); return; }
            Gonder(siradaki);
        }

        // HikayeKomut/kesim.txt: "hikaye" satiri, sonra her satir "dosyaNo sesNo ilkOrnek sonOrnek"
        [MenuItem("Hikaye/Toplu Ses Bol")]
        public static void Bol()
        {
            try
            {
                var satirlar = File.ReadAllLines(Klasor + "/kesim.txt");
                string hedef = GeminiSes.Dir(Path.GetFileName(satirlar[0].Trim()));
                Directory.CreateDirectory(hedef);
                var kaynak = new Dictionary<string, byte[]>();
                int n = 0;
                for (int i = 1; i < satirlar.Length; i++)
                {
                    var p = satirlar[i].Split(' ');
                    if (p.Length < 4) continue;
                    byte[] b;
                    if (!kaynak.TryGetValue(p[0], out b)) kaynak[p[0]] = b = File.ReadAllBytes(Klasor + "/toplu_" + int.Parse(p[0]) + ".wav");
                    int hiz = BitConverter.ToInt32(b, 24);
                    int bas = 44 + int.Parse(p[2]) * 2, son = Math.Min(b.Length, 44 + int.Parse(p[3]) * 2);
                    var pcm = new byte[son - bas];
                    Array.Copy(b, bas, pcm, 0, pcm.Length);
                    File.WriteAllBytes(hedef + "/" + int.Parse(p[1]).ToString("00") + ".wav", Wav(pcm, hiz));
                    n++;
                }
                AssetDatabase.Refresh();
                Yaz("bol TAMAM " + n + " dosya -> " + hedef);
            }
            catch (Exception e) { Yaz("bol HATA: " + e.Message); }
        }

        static async void Gonder(string jsonYolu)
        {
            calisiyor = true;
            string ad = Path.GetFileNameWithoutExtension(jsonYolu);
            try
            {
                string anahtar = EditorPrefs.GetString(GeminiSes.KeyPref, "").Trim();
                if (anahtar.Length == 0) { Yaz(ad + " HATA: Gemini anahtari girilmemis"); return; }
                string url = "https://generativelanguage.googleapis.com/v1beta/models/" + GeminiSes.Model + ":generateContent";
                Yaz(ad + " gonderiliyor (" + GeminiSes.Model + ")");
                using (var req = new UnityWebRequest(url, "POST"))
                {
                    req.uploadHandler = new UploadHandlerRaw(File.ReadAllBytes(jsonYolu));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.SetRequestHeader("x-goog-api-key", anahtar);
                    req.timeout = 600;
                    var op = req.SendWebRequest();
                    while (!op.isDone) await Task.Delay(200);

                    string govde = req.downloadHandler != null ? req.downloadHandler.text : "";
                    Cevap c = null;
                    try { c = JsonUtility.FromJson<Cevap>(govde); } catch { }
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        string m = c != null && c.error != null && !string.IsNullOrEmpty(c.error.message) ? c.error.message : req.error;
                        Yaz(ad + " HATA " + req.responseCode + ": " + m);
                        return;
                    }
                    GeminiSes.KotaArtir();
                    var pcm = new MemoryStream();
                    int hiz = 24000;
                    if (c != null && c.candidates != null && c.candidates.Length > 0 && c.candidates[0].content != null && c.candidates[0].content.parts != null)
                        foreach (var p in c.candidates[0].content.parts)
                        {
                            if (p == null || p.inlineData == null || string.IsNullOrEmpty(p.inlineData.data)) continue;
                            var b = Convert.FromBase64String(p.inlineData.data);
                            int bas = 0, uz = b.Length;
                            if (b.Length > 44 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F')
                            {
                                hiz = BitConverter.ToInt32(b, 24);
                                for (int k = 12; k + 8 <= b.Length;)
                                {
                                    int boy = BitConverter.ToInt32(b, k + 4);
                                    if (b[k] == 'd' && b[k + 1] == 'a' && b[k + 2] == 't' && b[k + 3] == 'a')
                                    {
                                        bas = k + 8; uz = boy > 0 && boy <= b.Length - bas ? boy : b.Length - bas;
                                        break;
                                    }
                                    if (boy <= 0) break;
                                    k += 8 + boy + (boy & 1);
                                }
                            }
                            pcm.Write(b, bas, uz);
                        }
                    if (pcm.Length == 0) { Yaz(ad + " HATA: cevapta ses yok"); return; }
                    var veri = pcm.ToArray();
                    File.WriteAllBytes(Klasor + "/" + ad + ".wav", Wav(veri, hiz));
                    Yaz(ad + " TAMAM " + (veri.Length / 2f / hiz).ToString("0.0") + " sn, " + hiz + " Hz. " + GeminiSes.KotaMetni());
                }
            }
            catch (Exception e) { Yaz(ad + " HATA: " + e.Message); }
            finally { calisiyor = false; }
        }

        static void Yaz(string s)
        {
            File.AppendAllText(Klasor + "/toplu_durum.txt", DateTime.Now.ToString("HH:mm:ss") + " " + s + "\n", Encoding.UTF8);
            Debug.Log("[Toplu Ses] " + s);
        }

        static byte[] Wav(byte[] pcm, int hiz)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + pcm.Length);
                w.Write(Encoding.ASCII.GetBytes("WAVE")); w.Write(Encoding.ASCII.GetBytes("fmt "));
                w.Write(16); w.Write((short)1); w.Write((short)1);
                w.Write(hiz); w.Write(hiz * 2); w.Write((short)2); w.Write((short)16);
                w.Write(Encoding.ASCII.GetBytes("data")); w.Write(pcm.Length); w.Write(pcm);
                w.Flush();
                return ms.ToArray();
            }
        }
    }
}
