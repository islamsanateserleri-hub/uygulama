using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Unity Recorder'sız video kaydı (bağımsız uygulama için).
    // Kanvas, 1080x1920 bir RenderTexture'a çizilir; her karede AsyncGPUReadback ile kare ffmpeg'in stdin'ine gider.
    // Sabit kare hızı: Time.captureDeltaTime. Ses: AudioRenderer -> WAV -> ffmpeg ile birleştirilir.
    public class FfmpegPipeRecorder : MonoBehaviour, IVideoKaydedici
    {
        public int Genislik = 1080, Yukseklik = 1920, Fps = 30;
        public bool Klaket;                      // test: 30. karede beyaz flaş + aynı karede 1 kHz bip
        public int KlaketKare = 30;

        public bool Kaydediyor { get; private set; }
        public string Durum = "hazir";           // hazir | kayit | kodlaniyor | bitti | hata
        public string Kodlayici = "";
        public string SonDosya = "";
        public string SonMesaj = "";
        public int KareSayisi;
        public int HataliKare;
        public event Action<bool, string> Bitti;  // (basarili, mesaj) - ana iş parçacığında

        // ---- ffmpeg bulma ----
        public static string FfmpegBul()
        {
            try
            {
                string p = Path.Combine(Application.streamingAssetsPath, "ffmpeg", "ffmpeg.exe");
                if (File.Exists(p)) return p;
                var path = Environment.GetEnvironmentVariable("PATH") ?? "";
                foreach (var d in path.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrWhiteSpace(d)) continue;
                    try { var f = Path.Combine(d.Trim().Trim('"'), "ffmpeg.exe"); if (File.Exists(f)) return f; } catch { }
                }
            }
            catch { }
            return null;
        }

        static string seciliKodlayici;
        static bool Dene(string ffmpeg, string enc)
        {
            try
            {
                var psi = new ProcessStartInfo(ffmpeg, "-hide_banner -loglevel error -f lavfi -i color=c=black:s=256x256:d=0.3:r=30 -c:v " + enc + " -pix_fmt yuv420p -f null -")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
                using (var pr = Process.Start(psi))
                {
                    pr.StandardError.ReadToEnd();
                    if (!pr.WaitForExit(15000)) { try { pr.Kill(); } catch { } return false; }
                    return pr.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        public static string KodlayiciSec(string ffmpeg)
        {
            if (seciliKodlayici != null) return seciliKodlayici;
            foreach (var enc in new[] { "h264_nvenc", "h264_amf", "h264_qsv", "h264_mf", "libx264", "libopenh264" })
                if (Dene(ffmpeg, enc)) { seciliKodlayici = enc; return enc; }
            return null;
        }

        static string KodlayiciSecenekleri(string enc)
        {
            switch (enc)
            {
                case "h264_nvenc": return "-preset p5 -rc vbr -cq 21 -b:v 0";
                case "h264_amf": return "-quality balanced -b:v 12M";
                case "h264_qsv": return "-preset medium -b:v 12M";
                case "h264_mf": return "-b:v 12M";
                case "libopenh264": return "-b:v 12M";
                default: return "-preset veryfast -crf 20";
            }
        }

        // ---- iç durum ----
        string ffmpegYol, videoYol, sesYol, cikisYol;
        Process proc;
        Stream stdin;
        Thread yazici, bitirici;
        BlockingCollection<byte[]> kuyruk;
        readonly ConcurrentBag<byte[]> havuz = new ConcurrentBag<byte[]>();
        readonly Queue<AsyncGPUReadbackRequest> bekleyen = new Queue<AsyncGPUReadbackRequest>();
        readonly StringBuilder ffErr = new StringBuilder();
        RenderTexture rt;
        GameObject kamGo;
        Canvas canvas;
        RenderMode eskiMod;
        Camera eskiKamera;
        UnityEngine.UI.Image flas;
        FileStream sesDosya;
        bool sesAcik;
        int kanal = 2, ornekHizi = 48000;
        long sesOrnekSayisi;
        byte[] sesBayt = new byte[0];
        volatile bool bitirBitti;
        volatile bool bitirBasarili;
        volatile string bitirMesaj = "";
        bool bildirildi;

        public bool Baslat(string cikisYolu, int genislik, int yukseklik, int fps, out string hata)
        {
            hata = null;
            if (Kaydediyor) { hata = "zaten kaydediyor"; return false; }
            Genislik = genislik; Yukseklik = yukseklik; Fps = fps;
            ffmpegYol = FfmpegBul();
            if (ffmpegYol == null) { hata = "ffmpeg.exe bulunamadı (Assets/StreamingAssets/ffmpeg/). Menü: Hikaye > ffmpeg İndir"; return Basarisiz(hata); }
            Kodlayici = KodlayiciSec(ffmpegYol);
            if (Kodlayici == null) { hata = "uygun H.264 kodlayıcı bulunamadı"; return Basarisiz(hata); }

            canvas = null;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
                if (c.name == "HikayeCanvas") { canvas = c; break; }
            if (canvas == null) { hata = "HikayeCanvas yok (hikâye başlamadan kayıt başlatıldı)"; return Basarisiz(hata); }

            try
            {
                cikisYol = Path.GetFullPath(cikisYolu);
                Directory.CreateDirectory(Path.GetDirectoryName(cikisYol));
                videoYol = cikisYol + ".video.mp4";
                sesYol = cikisYol + ".ses.wav";
                if (File.Exists(videoYol)) File.Delete(videoYol);
                if (File.Exists(sesYol)) File.Delete(sesYol);
            }
            catch (Exception e) { hata = "çıkış yolu: " + e.Message; return Basarisiz(hata); }

            rt = new RenderTexture(Genislik, Yukseklik, 24, RenderTextureFormat.ARGB32) { name = "KayitRT", antiAliasing = 1 };
            rt.Create();

            // kanvas -> kayıt kamerası (uzak bir konumda, başka hiçbir şey görünmesin diye)
            kamGo = new GameObject("KayitKamerasi");
            kamGo.transform.position = new Vector3(0f, 3000f, 0f);
            var cam = kamGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 30f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.allowHDR = false; cam.allowMSAA = false; cam.depth = 1000; cam.targetTexture = rt;
            eskiMod = canvas.renderMode; eskiKamera = canvas.worldCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 10f;

            if (Klaket)
            {
                var fgo = new GameObject("KlaketFlas", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                fgo.transform.SetParent(canvas.transform, false);
                var rtr = (RectTransform)fgo.transform; rtr.anchorMin = Vector2.zero; rtr.anchorMax = Vector2.one; rtr.offsetMin = rtr.offsetMax = Vector2.zero;
                flas = fgo.GetComponent<UnityEngine.UI.Image>(); flas.color = Color.white; flas.raycastTarget = false;
                fgo.SetActive(false);
            }

            try
            {
                string args = "-y -loglevel warning -f rawvideo -pix_fmt rgba -s " + Genislik + "x" + Yukseklik + " -r " + Fps +
                    " -i - -vf vflip -c:v " + Kodlayici + " " + KodlayiciSecenekleri(Kodlayici) + " -pix_fmt yuv420p -movflags +faststart \"" + videoYol + "\"";
                var psi = new ProcessStartInfo(ffmpegYol, args)
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardError = true, RedirectStandardOutput = true };
                proc = Process.Start(psi);
                proc.ErrorDataReceived += (s, e) => { if (e.Data != null) lock (ffErr) { if (ffErr.Length < 8000) ffErr.AppendLine(e.Data); } };
                proc.OutputDataReceived += (s, e) => { };
                proc.BeginErrorReadLine(); proc.BeginOutputReadLine();
                stdin = proc.StandardInput.BaseStream;
            }
            catch (Exception e) { hata = "ffmpeg başlatılamadı: " + e.Message; Temizle(); return Basarisiz(hata); }

            kuyruk = new BlockingCollection<byte[]>(6);
            yazici = new Thread(YaziciDongu) { IsBackground = true, Name = "FfmpegYazici" };
            yazici.Start();

            // ses
            sesAcik = false;
            try
            {
                ornekHizi = AudioSettings.outputSampleRate;
                switch (AudioSettings.speakerMode)
                {
                    case AudioSpeakerMode.Mono: kanal = 1; break;
                    case AudioSpeakerMode.Quad: kanal = 4; break;
                    case AudioSpeakerMode.Surround: kanal = 5; break;
                    case AudioSpeakerMode.Mode5point1: kanal = 6; break;
                    case AudioSpeakerMode.Mode7point1: kanal = 8; break;
                    default: kanal = 2; break;
                }
                sesDosya = new FileStream(sesYol, FileMode.Create, FileAccess.Write);
                sesDosya.Write(new byte[44], 0, 44);
                sesOrnekSayisi = 0;
                sesAcik = AudioRenderer.Start();
            }
            catch (Exception e) { Debug.LogWarning("[Kayıt] ses başlatılamadı: " + e.Message); sesAcik = false; }

            KareSayisi = 0; HataliKare = 0; bildirildi = false; bitirBitti = false; ffErr.Length = 0;
            Time.captureDeltaTime = 1f / Fps;
            Kaydediyor = true; Durum = "kayit";
            StartCoroutine(Dongu());
            return true;
        }

        bool Basarisiz(string mesaj) { Durum = "hata"; SonMesaj = mesaj; Debug.LogWarning("[Kayıt] " + mesaj); return false; }

        void YaziciDongu()
        {
            try
            {
                foreach (var b in kuyruk.GetConsumingEnumerable())
                {
                    stdin.Write(b, 0, b.Length);
                    havuz.Add(b);
                }
                stdin.Flush(); stdin.Close();
            }
            catch (Exception) { /* ffmpeg erken kapandı; Bitir raporlar */ try { foreach (var _ in kuyruk.GetConsumingEnumerable()) { } } catch { } }
        }

        IEnumerator Dongu()
        {
            var son = new WaitForEndOfFrame();
            while (Kaydediyor)
            {
                yield return son;
                if (!Kaydediyor) break;
                KareAl();
            }
        }

        void LateUpdate()
        {
            if (Kaydediyor && flas != null) flas.gameObject.SetActive(KareSayisi == KlaketKare);
            if (bitirBitti && !bildirildi)
            {
                bildirildi = true;
                Durum = bitirBasarili ? "bitti" : "hata"; SonMesaj = bitirMesaj; if (bitirBasarili) SonDosya = cikisYol;
                var h = Bitti; if (h != null) h(bitirBasarili, bitirMesaj);
            }
        }

        void KareAl()
        {
            if (sesAcik) SesYaz();
            bekleyen.Enqueue(AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32));
            KareSayisi++;
            Bosalt(false);
        }

        void Bosalt(bool hepsi)
        {
            while (bekleyen.Count > 0)
            {
                var r = bekleyen.Peek();
                if (!r.done)
                {
                    if (!hepsi && bekleyen.Count < 4) break;
                    r.WaitForCompletion();
                }
                bekleyen.Dequeue();
                if (r.hasError) { HataliKare++; continue; }
                var data = r.GetData<byte>();
                byte[] buf;
                if (!havuz.TryTake(out buf) || buf.Length != data.Length) buf = new byte[data.Length];
                data.CopyTo(buf);
                kuyruk.Add(buf);
            }
        }

        void SesYaz()
        {
            int n = AudioRenderer.GetSampleCountForCaptureFrame();
            var buf = new NativeArray<float>(n * kanal, Allocator.Temp);
            try
            {
                if (!AudioRenderer.Render(buf)) return;
                if (Klaket && KareSayisi >= KlaketKare && KareSayisi < KlaketKare + 3)
                {
                    // 1 kHz bip, kare başlangıcından itibaren
                    for (int i = 0; i < n; i++)
                    {
                        float v = Mathf.Sin(2f * Mathf.PI * 1000f * (i / (float)ornekHizi)) * 0.8f;
                        for (int c = 0; c < kanal; c++) buf[i * kanal + c] = v;
                    }
                }
                if (sesBayt.Length < buf.Length * 2) sesBayt = new byte[buf.Length * 2];
                for (int i = 0; i < buf.Length; i++)
                {
                    float f = Mathf.Clamp(buf[i], -1f, 1f);
                    short s = (short)Mathf.RoundToInt(f * 32767f);
                    sesBayt[i * 2] = (byte)(s & 0xFF); sesBayt[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
                }
                sesDosya.Write(sesBayt, 0, buf.Length * 2);
                sesOrnekSayisi += n;
            }
            finally { buf.Dispose(); }
        }

        void WavKapat()
        {
            try
            {
                int veri = (int)(sesOrnekSayisi * kanal * 2);
                sesDosya.Seek(0, SeekOrigin.Begin);
                var w = new BinaryWriter(sesDosya);
                w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + veri); w.Write(Encoding.ASCII.GetBytes("WAVE"));
                w.Write(Encoding.ASCII.GetBytes("fmt ")); w.Write(16); w.Write((short)1); w.Write((short)kanal);
                w.Write(ornekHizi); w.Write(ornekHizi * kanal * 2); w.Write((short)(kanal * 2)); w.Write((short)16);
                w.Write(Encoding.ASCII.GetBytes("data")); w.Write(veri);
                w.Flush(); sesDosya.Close();
            }
            catch (Exception e) { Debug.LogWarning("[Kayıt] WAV kapatılamadı: " + e.Message); }
        }

        public void Durdur()
        {
            if (!Kaydediyor) return;
            Kaydediyor = false;
            Time.captureDeltaTime = 0f;
            try { Bosalt(true); } catch (Exception e) { Debug.LogWarning("[Kayıt] kareler boşaltılırken: " + e.Message); }
            try { kuyruk.CompleteAdding(); } catch { }
            if (sesAcik) { try { AudioRenderer.Stop(); } catch { } WavKapat(); }
            if (canvas != null) { canvas.renderMode = eskiMod; canvas.worldCamera = eskiKamera; }
            if (kamGo != null) Destroy(kamGo);
            if (flas != null) Destroy(flas.gameObject);
            Durum = "kodlaniyor";
            bitirici = new Thread(Bitir) { IsBackground = true, Name = "FfmpegBitir" };
            bitirici.Start();
        }

        void Bitir()
        {
            try
            {
                yazici.Join(60000);
                if (!proc.WaitForExit(180000)) { try { proc.Kill(); } catch { } bitirMesaj = "ffmpeg zaman aşımı"; bitirBasarili = false; bitirBitti = true; return; }
                if (proc.ExitCode != 0 || !File.Exists(videoYol)) { bitirMesaj = "ffmpeg hata kodu " + proc.ExitCode + ": " + Son(); bitirBasarili = false; bitirBitti = true; return; }

                string args;
                if (sesAcik && File.Exists(sesYol) && sesOrnekSayisi > 0)
                    args = "-y -loglevel warning -i \"" + videoYol + "\" -i \"" + sesYol + "\" -map 0:v:0 -map 1:a:0 -c:v copy -c:a aac -b:a 192k -shortest -movflags +faststart \"" + cikisYol + "\"";
                else
                    args = "-y -loglevel warning -i \"" + videoYol + "\" -c:v copy -movflags +faststart \"" + cikisYol + "\"";
                var psi = new ProcessStartInfo(ffmpegYol, args) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
                using (var m = Process.Start(psi))
                {
                    string err = m.StandardError.ReadToEnd();
                    m.WaitForExit(180000);
                    if (m.ExitCode != 0 || !File.Exists(cikisYol)) { bitirMesaj = "birleştirme hatası: " + err; bitirBasarili = false; bitirBitti = true; return; }
                }
                try { File.Delete(videoYol); } catch { }
                try { File.Delete(sesYol); } catch { }
                bitirMesaj = "tamam: " + KareSayisi + " kare, kodlayıcı " + Kodlayici + (HataliKare > 0 ? ", hatalı kare " + HataliKare : "");
                bitirBasarili = true; bitirBitti = true;
            }
            catch (Exception e) { bitirMesaj = "istisna: " + e.Message; bitirBasarili = false; bitirBitti = true; }
        }

        string Son() { lock (ffErr) return ffErr.ToString(); }

        void Temizle()
        {
            if (rt != null) { rt.Release(); Destroy(rt); rt = null; }
            if (canvas != null) { canvas.renderMode = eskiMod; canvas.worldCamera = eskiKamera; }
            if (kamGo != null) Destroy(kamGo);
            Time.captureDeltaTime = 0f;
        }

        void OnGUI()
        {
            if (!Kaydediyor || rt == null || Event.current.type != EventType.Repaint) return;
            float s = Mathf.Min(Screen.width / (float)Genislik, Screen.height / (float)Yukseklik);
            float w = Genislik * s, h = Yukseklik * s;
            GUI.DrawTexture(new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h), rt, ScaleMode.StretchToFill, false);
        }

        void OnDestroy()
        {
            if (Kaydediyor) { try { Durdur(); } catch { } }
            Time.captureDeltaTime = 0f;
        }
    }
}
