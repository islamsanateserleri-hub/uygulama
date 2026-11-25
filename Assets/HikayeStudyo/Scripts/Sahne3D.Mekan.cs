using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 12: hava durumu (yagmur, kar, sis) ve araba icinde oturma.
    public partial class Sahne3D
    {
        HavaDurumu3D havaGorsel;
        bool havaYagmur, havaKar, havaSis, havaUyarildi;
        readonly Dictionary<string, string> koltuklar = new Dictionary<string, string>();   // karakter -> koltuk noktasi
        static readonly string[] ArabaKoltuklari = { "surucu", "yolcu", "arka_sol", "arka_sag" };

        // ---------- Hava ----------
        void HavaSec(string deger)
        {
            havaYagmur = havaKar = havaSis = false;
            foreach (var tk in (deger ?? "").Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string n = Util.Norm(tk);
                if (n == "yagmur") havaYagmur = true;
                else if (n == "kar") havaKar = true;
                else if (n == "sis") havaSis = true;
                else if (n != "yok") Debug.LogWarning("[Hikaye] hava bilinmiyor: " + tk + " (" + string.Join(", ", YonetmenTablo.Havalar) + ")");
            }
        }

        // hava: yagmur | kar | sis | yagmur, sis | yok
        public void HavaAyarla(string deger)
        {
            HavaSec(deger);
            if (!OdaAktif && (havaYagmur || havaKar || havaSis) && !havaUyarildi)
            {
                havaUyarildi = true;
                Debug.Log("[Hikaye] hava: 3D mekanda görünür (mekan3d:). Çizgi mekanda yağmur için 'atmosfer: yagmur' yaz.");
            }
            HavaKur();
        }

        public void HavaSifirla()
        {
            havaYagmur = havaKar = havaSis = false;
            if (havaGorsel != null) havaGorsel.Temizle();
        }

        void HavaKur()
        {
            bool istek = OdaAktif && (havaYagmur || havaKar || havaSis);
            if (!istek)
            {
                if (havaGorsel != null) havaGorsel.Temizle();
                if (Mekan == null) havaUyarildi = false;
                return;
            }
            if (havaGorsel == null) havaGorsel = gameObject.AddComponent<HavaDurumu3D>();
            bool gece = Atmosfer != null && Atmosfer.SonZaman == "gece";
            havaGorsel.Ayarla(havaYagmur, havaKar, havaSis, Mekan, gece);
        }

        // ---------- Araba ----------
        string BosKoltuk(string k)
        {
            string s;
            if (koltuklar.TryGetValue(k, out s)) return s;
            foreach (var ad in ArabaKoltuklari) if (!koltuklar.ContainsValue(ad)) return ad;
            return ArabaKoltuklari[ArabaKoltuklari.Length - 1];
        }

        // Sahnedeki herkesi oturt (soldaki surucu koltuguna)
        void ArabayaOturt()
        {
            if (Mekan == null || !Mekan.Oturmali) return;
            // sahneden cikanlarin koltugu bosalir
            foreach (var k in new List<string>(koltuklar.Keys))
            {
                Karakter3D v;
                if (!actors.TryGetValue(k, out v) || v == null) koltuklar.Remove(k);
            }
            var bekleyen = new List<string>();
            foreach (var kv in actors) if (kv.Value != null && !koltuklar.ContainsKey(kv.Key)) bekleyen.Add(kv.Key);
            bekleyen.Sort((x, y) =>
            {
                float a = slots.ContainsKey(x) ? slots[x] : 0f, b = slots.ContainsKey(y) ? slots[y] : 0f;
                return a.CompareTo(b);
            });
            foreach (var k in bekleyen)
            {
                if (koltuklar.Count >= ArabaKoltuklari.Length) { Debug.LogWarning("[Hikaye] arabada en fazla 4 kişi oturabilir: " + k + " için koltuk yok."); continue; }
                KoltugaOturt(k, actors[k], BosKoltuk(k));
            }
        }

        // yer: Karakter surucu|yolcu|arka_sol|arka_sag
        bool KoltugaOturt(string k, Karakter3D a, string koltuk)
        {
            Vector3 w; float yaw;
            if (Mekan == null || a == null) return false;
            string ad = Util.Norm(koltuk);
            if (System.Array.IndexOf(ArabaKoltuklari, ad) < 0) ad = BosKoltuk(k);
            if (!Mekan.NoktaBul(ad, out w, out yaw)) return false;
            // koltuk doluysa icindekiyle yer degistir
            string eski;
            bool eskiVar = koltuklar.TryGetValue(k, out eski);
            foreach (var kv in new List<KeyValuePair<string, string>>(koltuklar))
            {
                if (kv.Key == k || kv.Value != ad) continue;
                koltuklar.Remove(kv.Key);
                Karakter3D o;
                Vector3 ow; float oyaw;
                string yeni = eskiVar ? eski : null;
                if (yeni == null) foreach (var s in ArabaKoltuklari) if (s != ad && !koltuklar.ContainsValue(s)) { yeni = s; break; }
                if (yeni != null && actors.TryGetValue(kv.Key, out o) && o != null && Mekan.NoktaBul(yeni, out ow, out oyaw))
                    Oturt(kv.Key, o, yeni, ow, oyaw);
            }
            Oturt(k, a, ad, w, yaw);
            return true;
        }

        void Oturt(string k, Karakter3D a, string koltuk, Vector3 nokta, float yaw)
        {
            BagBitir(k);
            YatakBirak(k, a);
            walking.Remove(k);
            koltuklar[k] = koltuk;
            slots[k] = nokta.x - Origin.x;
            yatakOncesi[k] = new Vector3(nokta.x, Origin.y, nokta.z);
            if (KlipVar(a, Hareket3D.KlipOtur)) Play(a, Hareket3D.KlipOtur);
            a.transform.position = new Vector3(nokta.x, Mathf.Max(Origin.y - 0.1f, nokta.y - 0.45f * a.transform.localScale.y), nokta.z);
            Dondur(k, a, yaw, true);
            yataktaki[k] = "otur";
            pozDurum[k] = "oturuyor";
            if (isActiveAndEnabled) StartCoroutine(KalcaOturt(k, a, koltuk, nokta.y + 0.09f));
        }

        // Oturma klibi yerlesince kalcayi koltuk yuksekligine getirir (boylar farkli olabilir)
        IEnumerator KalcaOturt(string k, Karakter3D a, string koltuk, float hedefY)
        {
            yield return new WaitForSeconds(0.45f);
            string s;
            if (a == null || !koltuklar.TryGetValue(k, out s) || s != koltuk) yield break;
            var kalca = Kemik(a, HumanBodyBones.Hips);
            if (kalca == null) yield break;
            float fark = Mathf.Clamp(hedefY - kalca.position.y, -0.5f, 0.5f);
            a.transform.position += Vector3.up * fark;
        }

        // arabadan baska mekana gecerken herkes ayaga kalkar
        void ArabadanKaldir()
        {
            foreach (var k in new List<string>(koltuklar.Keys))
            {
                Karakter3D a;
                if (!actors.TryGetValue(k, out a) || a == null) continue;
                YatakBirak(k, a);
                Play(a, Hareket3D.KlipDur);
            }
            koltuklar.Clear();
        }
    }
}
