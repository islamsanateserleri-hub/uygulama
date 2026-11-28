using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HikayeStudyo
{
    // Aşama 1 testi: komut satırı  "-kayit <cikis.mp4> -hikaye <ad> [-klaket]"  ya da çalışma klasöründeki ffkayit.txt
    // (satırlar: hikaye=<ad>, cikis=<yol>, klaket=1). Sonuç: ffkayit_sonuc.txt
    // Aşama 5 (arayüzden "Videoya al"): "-hikayedosya <txt yolu>" kullanıcı hikâyesini dosyadan kurar (ad = dosya adı;
    // kalıcı ses klasörü bu adla eşleşir), "-yayin <klasör>" yayın klasörünü oluşturur; oraya kayit_ilerleme.txt ve kayit_sonuc.txt yazılır.
    public static class KayitBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Basla()
        {
            string hikaye = null, cikis = null, hikayeDosya = null, yayin = null; bool klaket = false;
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == "-kayit" && i + 1 < a.Length) cikis = a[i + 1];
                else if (a[i] == "-hikaye" && i + 1 < a.Length) hikaye = a[i + 1];
                else if (a[i] == "-klaket") klaket = true;
                else if (a[i] == "-hikayedosya" && i + 1 < a.Length) hikayeDosya = a[i + 1];
                else if (a[i] == "-yayin" && i + 1 < a.Length) yayin = a[i + 1];
            }
            string dosya = Path.Combine(Directory.GetCurrentDirectory(), "ffkayit.txt");
            if (hikaye == null && File.Exists(dosya))
            {
                try
                {
                    foreach (var satir in File.ReadAllLines(dosya, Encoding.UTF8))
                    {
                        int e = satir.IndexOf('='); if (e < 0) continue;
                        string k = satir.Substring(0, e).Trim().ToLowerInvariant(), v = satir.Substring(e + 1).Trim();
                        if (k == "hikaye") hikaye = v; else if (k == "cikis") cikis = v; else if (k == "klaket") klaket = v == "1";
                    }
                    File.Delete(dosya);
                }
                catch (Exception ex) { Debug.LogWarning("[Kayıt] ffkayit.txt: " + ex.Message); }
            }
            if (string.IsNullOrEmpty(hikaye) && !string.IsNullOrEmpty(hikayeDosya)) hikaye = Path.GetFileNameWithoutExtension(hikayeDosya);
            if (string.IsNullOrEmpty(hikaye)) return;
            // Pencere gizlenince/odak kaybolunca oynatıcı durmasın (kayıt ve ffmpeg birleştirme ana döngüye bağlı)
            Application.runInBackground = true;
            if (!string.IsNullOrEmpty(yayin))
            {
                try { Directory.CreateDirectory(yayin); } catch (Exception ex) { Debug.LogWarning("[Kayıt] yayın klasörü: " + ex.Message); }
                if (string.IsNullOrEmpty(cikis)) cikis = Path.Combine(yayin, "video.mp4");
            }
            if (string.IsNullOrEmpty(cikis)) cikis = Path.Combine(Directory.GetCurrentDirectory(), "Kayitlar", hikaye + ".mp4");
            // Sahnedeki hazır StoryPlayer (autoPlay açık) kayıtla çakışmasın: Start() çalışmadan kapat
            foreach (var eski in UnityEngine.Object.FindObjectsByType<StoryPlayer>(FindObjectsInactive.Include))
                eski.gameObject.SetActive(false);
            var go = new GameObject("KayitIsleyici");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var k2 = go.AddComponent<KayitIsleyici>();
            k2.hikaye = hikaye; k2.cikis = cikis; k2.klaket = klaket; k2.hikayeDosya = hikayeDosya; k2.yayin = yayin;
        }
    }

    public class KayitIsleyici : MonoBehaviour
    {
        public string hikaye, cikis; public bool klaket;
        public string hikayeDosya, yayin;      // Aşama 5: arayüzden "Videoya al" (ikisi de boş olabilir)
        FfmpegPipeRecorder rec; bool bitti; bool sonuc; string mesaj = "";
        float toplamSure; string ilerlemeDurum = "hazirlaniyor"; float sonIlerleme = -10f; bool yazildi;
        int eskiGuvenli = -1;
        StoryPlayer sp;                         // Aşama 5 (P2): yayın paketi için
        // Kapak RT'si (kayıt durduktan sonra kapak ekranı 1080x1920 çizilsin diye; videoya girmez)
        RenderTexture kapakRt; GameObject kapakKam; Canvas kapakCanvas; RenderMode kapakEskiMod; Camera kapakEskiKam;

        IEnumerator Start()
        {
            TextAsset ta = null;
            if (!string.IsNullOrEmpty(hikayeDosya))
            {
                string metin = null, hata0 = null;
                try { metin = File.ReadAllText(hikayeDosya, Encoding.UTF8); } catch (Exception ex) { hata0 = ex.Message; }
                if (metin == null) { Yaz(false, "hikâye dosyası okunamadı: " + hikayeDosya + (hata0 != null ? " (" + hata0 + ")" : "")); yield break; }
                ta = new TextAsset(metin);
                ta.name = hikaye;   // kalıcı ses klasörü (Documents\HikayeStudyosu\Ses\<ad>) bu adla eşleşir
            }
            else ta = Resources.Load<TextAsset>("Hikayeler/" + hikaye);
            if (ta == null) { Yaz(false, "hikaye bulunamadı: Resources/Hikayeler/" + hikaye); yield break; }
            try { toplamSure = HikayeDenetci.ToplamSure(StoryParser.Parse(ta.text)); } catch { toplamSure = 0f; }

            // TikTok güvenli alan rehberi yalnız önizleme içindir, videoya girmesin (1. kayıt denemesinde kırmızı bantlar videoya girmişti)
            // Aşama 5: ayar ana uygulamayla paylaşılıyor (PlayerPrefs); kayıt bitince eski değer geri konur.
            if (!string.IsNullOrEmpty(yayin)) eskiGuvenli = PlayerPrefs.GetInt("HS_SafeArea", 0);
            PlayerPrefs.SetInt("HS_SafeArea", 0);
            // StoryPlayer Start() içinde arayüzü kurar; kayıt ondan sonra başlamalı
            var go = new GameObject("StoryPlayer");
            sp = go.AddComponent<StoryPlayer>();
            sp.story = ta; sp.autoPlay = false;
            // EXE'de sahnedeki oyuncu listesi yok: Windows derlemesinin yazdığı Resources/OyuncuListesi'ni kullan
            var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
            if (ol != null) { foreach (var pf in ol.prefablar) if (pf != null && !sp.oyuncular.Contains(pf)) sp.oyuncular.Add(pf); }
            else Debug.LogWarning("[Kayıt] Resources/OyuncuListesi yok: 3D oyuncular bulunamaz");
            yield return null; yield return null;

            rec = go.AddComponent<FfmpegPipeRecorder>();
            rec.Klaket = klaket;
            string hata;
            if (!rec.Baslat(cikis, 1080, 1920, (PlayerPrefs.GetInt("HS_Fps", 30) == 60 ? 60 : 30), out hata)) { Yaz(false, hata); yield break; }
            rec.Bitti += (ok, m) => { sonuc = ok; mesaj = m; bitti = true; };
            if (!string.IsNullOrEmpty(yayin))
            {
                // Aşama 5 (P2): kapak ekranı videoya girmesin (Bitir() yayını yazmaz, Kapat() kayıt durduktan sonra yazar);
                // kapak karesi pencereden değil 1080x1920 kayıt RT'sinden alınsın.
                StoryPlayer.KayitModu = true;
                StoryPlayer.KareYakalayici = RtKaresi;
            }
            StoryPlayer.Finished += Bitis;
            ilerlemeDurum = "kayit";
            sp.Baslat();
        }

        // Aşama 5: arayüz ilerlemeyi bu dosyadan okur (gerçek zamanla 0,5 sn'de bir)
        void Update()
        {
            if (string.IsNullOrEmpty(yayin) || yazildi) return;
            float now = Time.realtimeSinceStartup;
            if (now - sonIlerleme < 0.5f) return;
            sonIlerleme = now;
            IlerlemeYaz(ilerlemeDurum, "");
        }

        void IlerlemeYaz(string durum, string not)
        {
            if (string.IsNullOrEmpty(yayin)) return;
            try
            {
                int kare = rec != null ? rec.KareSayisi : 0;
                float gecen = rec != null && rec.Fps > 0 ? kare / (float)rec.Fps : 0f;
                var sb = new StringBuilder();
                sb.Append("durum=").Append(durum).Append('\n');
                sb.Append("sure=").Append(gecen.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
                sb.Append("toplam=").Append(toplamSure.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)).Append('\n');
                sb.Append("kare=").Append(kare).Append('\n');
                if (!string.IsNullOrEmpty(not)) sb.Append("not=").Append(not.Replace("\r", " ").Replace("\n", " ")).Append('\n');
                string hedef = Path.Combine(yayin, "kayit_ilerleme.txt"), tmp = hedef + ".tmp";
                File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
                if (File.Exists(hedef)) File.Delete(hedef);
                File.Move(tmp, hedef);
            }
            catch { }
        }

        void Bitis() { StartCoroutine(Kapat()); }

        IEnumerator Kapat()
        {
            StoryPlayer.Finished -= Bitis;
            ilerlemeDurum = "kodlaniyor";
            yield return new WaitForSeconds(1f);
            rec.Durdur();
            float t = 0; while (!bitti && t < 240f) { t += Time.unscaledDeltaTime; yield return null; }
            string rapor = "";
            if (bitti && sonuc) rapor = Bilgi(cikis);
            string yayinNotu = "";
            if (!string.IsNullOrEmpty(yayin))
            {
                // Video dosyası kapandı (bitti) → artık kapak ekranı çizilebilir, videoya girmez.
                if (bitti && sonuc)
                {
                    ilerlemeDurum = "yayin";
                    IlerlemeYaz("yayin", "kapak ve açıklama hazırlanıyor");
                    bool kur = false;
                    try { kur = KapakRtKur(); } catch (Exception ex) { Debug.LogWarning("[Kayıt] kapak RT: " + ex.Message); }
                    try
                    {
                        yield return null;   // kanvas yeni kameraya bir kare çizsin
                        if (sp != null) yield return sp.YayinUret(yayin);
                    }
                    finally
                    {
                        KapakRtBirak();
                        StoryPlayer.KayitModu = false;
                        StoryPlayer.KareYakalayici = null;
                    }
                    try
                    {
                        float sn = rec != null && rec.Fps > 0 ? rec.KareSayisi / (float)rec.Fps : 0f;
                        yayinNotu = YayinPaketi.Yaz(yayin, sp, hikaye, Path.GetFileName(cikis), sn, rec != null ? rec.Kodlayici : "", kur);
                    }
                    catch (Exception ex) { yayinNotu = "yayın paketi: " + ex.Message; Debug.LogWarning("[Kayıt] " + yayinNotu); }
                }
                StoryPlayer.KayitModu = false;
                StoryPlayer.KareYakalayici = null;
            }
            Yaz(bitti && sonuc, (bitti ? mesaj : "zaman aşımı") + (yayinNotu.Length > 0 ? " · " + yayinNotu : "") + "\n" + rapor);
        }

        void OnDestroy() { StoryPlayer.KayitModu = false; StoryPlayer.KareYakalayici = null; KapakRtBirak(); }

        // Kayıt sırasında: FfmpegPipeRecorder'ın kamerasının RT'si; kayıt sonrası: kapak RT'si. Yoksa null (StoryPlayer ScreenCapture'a düşer).
        Texture2D RtKaresi()
        {
            RenderTexture r = kapakRt;
            if (r == null)
            {
                var kg = GameObject.Find("KayitKamerasi");
                var c = kg != null ? kg.GetComponent<Camera>() : null;
                r = c != null ? c.targetTexture : null;
            }
            if (r == null || !r.IsCreated()) return null;
            var onceki = RenderTexture.active;
            RenderTexture.active = r;
            var t = new Texture2D(r.width, r.height, TextureFormat.RGB24, false);
            t.ReadPixels(new Rect(0, 0, r.width, r.height), 0, 0);
            t.Apply();
            RenderTexture.active = onceki;
            return t;
        }

        // FfmpegPipeRecorder ile aynı kurulum: HikayeCanvas -> uzak ortografik kamera -> 1080x1920 RT
        bool KapakRtKur()
        {
            kapakCanvas = null;
            foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
                if (c.name == "HikayeCanvas") { kapakCanvas = c; break; }
            if (kapakCanvas == null) return false;
            int w = rec != null && rec.Genislik > 0 ? rec.Genislik : 1080, h = rec != null && rec.Yukseklik > 0 ? rec.Yukseklik : 1920;
            kapakRt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { name = "KapakRT", antiAliasing = 1 };
            kapakRt.Create();
            kapakKam = new GameObject("KapakKamerasi");
            kapakKam.transform.position = new Vector3(0f, 3000f, 0f);
            var cam = kapakKam.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 30f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.allowHDR = false; cam.allowMSAA = false; cam.depth = 1000; cam.targetTexture = kapakRt;
            kapakEskiMod = kapakCanvas.renderMode; kapakEskiKam = kapakCanvas.worldCamera;
            kapakCanvas.renderMode = RenderMode.ScreenSpaceCamera; kapakCanvas.worldCamera = cam; kapakCanvas.planeDistance = 10f;
            return true;
        }

        void KapakRtBirak()
        {
            if (kapakCanvas != null) { kapakCanvas.renderMode = kapakEskiMod; kapakCanvas.worldCamera = kapakEskiKam; kapakCanvas = null; }
            if (kapakKam != null) { Destroy(kapakKam); kapakKam = null; }
            if (kapakRt != null) { kapakRt.Release(); Destroy(kapakRt); kapakRt = null; }
        }

        static string Bilgi(string yol)
        {
            try
            {
                var psi = new ProcessStartInfo(FfmpegPipeRecorder.FfmpegBul(), "-hide_banner -i \"" + yol + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
                using (var p = Process.Start(psi)) { string e = p.StandardError.ReadToEnd(); p.WaitForExit(20000); return e; }
            }
            catch (Exception ex) { return "ffmpeg bilgi: " + ex.Message; }
        }

        void Yaz(bool ok, string m)
        {
            string icerik = DateTime.Now.ToString("HH:mm:ss") + " " + (ok ? "TAMAM" : "HATA") + "\n" + cikis + "\n" + m;
            try
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "ffkayit_sonuc.txt"), icerik, Encoding.UTF8);
            }
            catch { }
            if (!string.IsNullOrEmpty(yayin))
            {
                try { File.WriteAllText(Path.Combine(yayin, "kayit_sonuc.txt"), icerik, new UTF8Encoding(false)); } catch { }
                string ilk = (m ?? "").Split('\n')[0];
                IlerlemeYaz(ok ? "bitti" : "hata", ilk);
                yazildi = true;
            }
            if (eskiGuvenli >= 0) { PlayerPrefs.SetInt("HS_SafeArea", eskiGuvenli); PlayerPrefs.Save(); }
            Debug.Log("[Kayıt] " + (ok ? "TAMAM " : "HATA ") + m);
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }
}
