using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Oynatma sırasında her karede çalan sesin şiddetini ve karakterlerin ağız açıklığını yazar (Aşama 9).
    // Köprüden: menu: Hikaye/Agiz Senkron Testi  ->  oynat: <hikaye>  ->  (bitince) dur
    // Çıktı: HikayeKomut/agiz_senkron.csv  (kare;zaman;klip;satir_basindan_gecen;ses_siddeti;karakter=agiz(0-100)...)
    // Ses konumu oyun saatine göre hesaplanır: ağız da aynı saatle sürülür. (Ses kartı gecikmesini ölçmez.)
    [InitializeOnLoad]
    public static class AgizSenkronTesti
    {
        const string Bayrak = "HS_AgizSenkronTesti";
        static StreamWriter yazici;
        static int sonKare = -1;
        static AudioClip sonKlip;
        static float klipBasi;
        static readonly Dictionary<AudioClip, float[]> veriler = new Dictionary<AudioClip, float[]>();

        static AgizSenkronTesti()
        {
            EditorApplication.update += Ornekle;
            EditorApplication.playModeStateChanged += Durum;
        }

        static string Yol { get { return Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/agiz_senkron.csv"); } }

        [MenuItem("Hikaye/Agiz Senkron Testi")]
        public static void Kur()
        {
            Kapat();
            if (File.Exists(Yol)) File.Delete(Yol);
            SessionState.SetBool(Bayrak, true);
            Debug.Log("Ağız senkron testi hazır: şimdi hikayeyi oynatın.");
        }

        static void Durum(PlayModeStateChange d)
        {
            if (d != PlayModeStateChange.ExitingPlayMode || yazici == null) return;
            Kapat();
            SessionState.SetBool(Bayrak, false);
        }

        static void Kapat()
        {
            if (yazici != null) { yazici.Dispose(); yazici = null; }
            veriler.Clear();
            sonKare = -1;
            sonKlip = null;
        }

        static void Ornekle()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(Bayrak, false) || Time.frameCount == sonKare) return;
            sonKare = Time.frameCount;

            var ses = SesKaynagi();
            var kisiler = YuzSahne.Hepsi();
            if (kisiler.Count == 0) return;
            if (yazici == null) yazici = new StreamWriter(Yol, false, new UTF8Encoding(false)) { AutoFlush = true };

            var c = CultureInfo.InvariantCulture;
            var s = new StringBuilder();
            s.Append(Time.frameCount).Append(';').Append(Time.time.ToString("F3", c)).Append(';');
            if (ses == null) s.Append("-;0;0");
            else
            {
                if (ses.clip != sonKlip) { sonKlip = ses.clip; klipBasi = Time.time; }
                float gecen = Time.time - klipBasi;
                s.Append(ses.clip.length.ToString("F3", c)).Append(';')
                 .Append(gecen.ToString("F3", c)).Append(';')
                 .Append(Siddet(ses.clip, gecen).ToString("F4", c));
            }
            foreach (var k in kisiler)
                if (k != null) s.Append(';').Append(k.Kimlik).Append('=').Append(Agiz(k.gameObject).ToString("F1", c));
            yazici.WriteLine(s.ToString());
        }

        // Konuşma sesi: çalan, döngüsüz klip (ortam/yağmur döngülüdür)
        static AudioSource SesKaynagi()
        {
            foreach (var a in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude))
                if (a.isPlaying && !a.loop && a.clip != null) return a;
            return null;
        }

        // Verilen andaki 20 ms'lik parçanın şiddeti (ağız hesabından bağımsız ölçüm)
        static float Siddet(AudioClip clip, float an)
        {
            float[] veri;
            if (!veriler.TryGetValue(clip, out veri))
            {
                veri = new float[clip.samples * clip.channels];
                if (!clip.GetData(veri, 0)) veri = new float[0];
                veriler[clip] = veri;
            }
            int bas = (int)(an * clip.frequency) * clip.channels;
            int son = Mathf.Min(veri.Length, bas + clip.frequency / 50 * clip.channels);
            if (son <= bas) return 0f;
            double toplam = 0;
            for (int i = bas; i < son; i++) toplam += veri[i] * veri[i];
            return Mathf.Sqrt((float)(toplam / (son - bas)));
        }

        // Ağız şekillerinin en büyüğü (0-100)
        static float Agiz(GameObject go)
        {
            float en = 0f;
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>(false))
            {
                if (r.sharedMesh == null) continue;
                for (int i = 0; i < r.sharedMesh.blendShapeCount; i++)
                {
                    var k = YuzCanlilik.KanalAdi(r.sharedMesh.GetBlendShapeName(i));
                    if (k == null || (k != "Agiz" && !k.StartsWith("Unlu"))) continue;
                    en = Mathf.Max(en, r.GetBlendShapeWeight(i));
                }
            }
            return en;
        }
    }
}
