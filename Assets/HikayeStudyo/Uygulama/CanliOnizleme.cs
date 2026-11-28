using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HikayeStudyo
{
    // Aşama 3: uygulama içi canlı önizleme.
    // Hikâye oynatıcısının "HikayeCanvas" kanvasını bir RenderTexture'a (9:16) yönlendirir; yazma ekranı bu dokuyu
    // yan panelde çizer. Aynı doku sahne küçük resimleri (PNG) için de yakalanır. Durdurunca sahne yeniden yüklenir
    // (oynatıcının kurduğu her şeyi temizlemenin en güvenli yolu; AnaEkran ve bu nesne DontDestroyOnLoad'dır).
    public class CanliOnizleme : MonoBehaviour
    {
        static CanliOnizleme ornek;

        public static CanliOnizleme Al()
        {
            if (ornek == null)
            {
                var go = new GameObject("CanliOnizleme");
                DontDestroyOnLoad(go);
                ornek = go.AddComponent<CanliOnizleme>();
            }
            return ornek;
        }

        public RenderTexture RT { get; private set; }
        public bool Aktif { get; private set; }        // önizleme kuruldu / oynuyor (Bitti olsa da son kare görünür)
        public bool Hazirlaniyor { get; private set; } // başlatma sürüyor
        public bool Bitti { get; private set; }
        public string Hata = "";
        public int BaslangicSatiri { get; private set; }
        public float Gecen { get { return Aktif ? Time.realtimeSinceStartup - baslamaZamani : 0f; } }

        GameObject playerGo, kamGo;
        Canvas canvas;
        float baslamaZamani;
        Coroutine rutin;

        // sessiz: küçük resim üretimi için tüm sesler kapalı
        public void Baslat(string ad, string metin, int satir, bool sessiz, int genislik = 540, int yukseklik = 960)
        {
            if (rutin != null) { StopCoroutine(rutin); rutin = null; }
            rutin = StartCoroutine(BaslatRutini(ad, metin, satir, sessiz, genislik, yukseklik));
        }

        IEnumerator BaslatRutini(string ad, string metin, int satir, bool sessiz, int genislik, int yukseklik)
        {
            if (Aktif || playerGo != null)
            {
                yield return DurdurRutini();
            }
            Hazirlaniyor = true; Bitti = false; Hata = ""; BaslangicSatiri = satir;
            try
            {
                if (satir > 0) PlayerPrefs.SetInt("HS_StartLine", satir); else PlayerPrefs.DeleteKey("HS_StartLine");
                var ta = new TextAsset(metin ?? ""); ta.name = ad;
                playerGo = new GameObject("StoryPlayer");
                var sp = playerGo.AddComponent<StoryPlayer>();
                sp.story = ta; sp.autoPlay = false;
                sp.voiceVolume = sessiz ? 0f : PlayerPrefs.GetFloat("HS_SesKonusma", 1f);
                sp.ambientVolume = sessiz ? 0f : PlayerPrefs.GetFloat("HS_SesMuzik", 0.09f);
                sp.sfxVolume = sessiz ? 0f : PlayerPrefs.GetFloat("HS_SesEfekt", 0.85f);
                var ol = Resources.Load<OyuncuListesi>("OyuncuListesi");
                if (ol != null) foreach (var pf in ol.prefablar) if (pf != null && !sp.oyuncular.Contains(pf)) sp.oyuncular.Add(pf);
            }
            catch (Exception e) { Hata = "Önizleme kurulamadı: " + e.Message; Hazirlaniyor = false; yield break; }

            yield return null; yield return null;

            canvas = null;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
                if (c.name == "HikayeCanvas") { canvas = c; break; }
            if (canvas == null) { Hata = "Önizleme kanvası bulunamadı."; Hazirlaniyor = false; yield break; }

            RT = new RenderTexture(genislik, yukseklik, 24, RenderTextureFormat.ARGB32) { name = "OnizlemeRT", antiAliasing = 1 };
            RT.Create();
            kamGo = new GameObject("OnizlemeKamerasi");
            kamGo.transform.position = new Vector3(0f, 3000f, 0f);
            var cam = kamGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f; cam.nearClipPlane = 0.1f; cam.farClipPlane = 30f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.allowHDR = false; cam.allowMSAA = false; cam.depth = 1000; cam.targetTexture = RT;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 10f;

            StoryPlayer.Finished -= BitisOlayi; StoryPlayer.Finished += BitisOlayi;
            var player = playerGo.GetComponent<StoryPlayer>();
            baslamaZamani = Time.realtimeSinceStartup;
            Aktif = true; Hazirlaniyor = false;
            player.Baslat();
            rutin = null;
        }

        void BitisOlayi() { Bitti = true; }

        public void Durdur()
        {
            if (rutin != null) { StopCoroutine(rutin); rutin = null; }
            rutin = StartCoroutine(DurdurRutini());
        }

        public IEnumerator DurdurRutini()
        {
            StoryPlayer.Finished -= BitisOlayi;
            bool temizlik = Aktif || playerGo != null || RT != null;
            Aktif = false; Hazirlaniyor = false; Bitti = false;
            if (canvas != null) { try { canvas.worldCamera = null; } catch { } }
            if (kamGo != null) { Destroy(kamGo); kamGo = null; }
            if (RT != null) { RT.Release(); Destroy(RT); RT = null; }
            playerGo = null; canvas = null;
            Time.timeScale = 1f;
            if (temizlik)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                yield return null; yield return null;
            }
            Application.targetFrameRate = 60;
            rutin = null;
        }

        public bool Calisiyor { get { return Aktif || Hazirlaniyor || rutin != null; } }

        // Küçük resim: yalnızca WaitForEndOfFrame sonrasında çağır. Çıktı: genislik x yukseklik RGB doku (ya da null).
        public Texture2D Yakala(int genislik, int yukseklik)
        {
            if (RT == null) return null;
            var onceki = RenderTexture.active;
            var gecici = RenderTexture.GetTemporary(genislik, yukseklik, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(RT, gecici);
            RenderTexture.active = gecici;
            var t = new Texture2D(genislik, yukseklik, TextureFormat.RGB24, false);
            t.ReadPixels(new Rect(0, 0, genislik, yukseklik), 0, 0);
            t.Apply(false, false);
            RenderTexture.active = onceki;
            RenderTexture.ReleaseTemporary(gecici);
            return t;
        }

        public static bool PngKaydet(Texture2D t, string yol)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(yol));
                File.WriteAllBytes(yol, t.EncodeToPNG());
                return true;
            }
            catch { return false; }
        }
    }
}
