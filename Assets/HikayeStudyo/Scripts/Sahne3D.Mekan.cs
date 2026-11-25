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
        // herhangi bir konusma satiri (anlatici dahil): onceki hava: artik "hemen once" sayilmaz
        public void KonusmaOldu() { havaTaze = false; }

        bool havaTaze;     // son konusmadan sonra 'hava:' yazildi

        // 10: 3D mekandaki yagmurun da sesi olsun
        public bool YagmurVar { get { return OdaAktif && havaYagmur; } }
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
            havaTaze = true;
            if (!OdaAktif && (havaYagmur || havaKar || havaSis) && !havaUyarildi)
            {
                havaUyarildi = true;
                Debug.Log("[Hikaye] hava: 3D mekanda görünür (mekan3d:). Çizgi mekanda yağmur için 'atmosfer: yagmur' yaz.");
            }
            HavaKur();
        }

        public void HavaSifirla()
        {
            havaYagmur = havaKar = havaSis = havaTaze = false;
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

        // cocuk boyu: ayaktaki (yaratilis anindaki) kafa kemigi yuksekligi
        const float CocukKafaTavani = 1.2f;
        static bool KisaBoylu(Karakter3D a) { return a != null && a.KafaYuksekligi > 0.01f && a.KafaYuksekligi < CocukKafaTavani; }

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
                // once yetiskinler: surucu koltugu onlarin
                bool cx = KisaBoylu(actors[x]), cy = KisaBoylu(actors[y]);
                if (cx != cy) return cx ? 1 : -1;
                float a = slots.ContainsKey(x) ? slots[x] : 0f, b = slots.ContainsKey(y) ? slots[y] : 0f;
                return a.CompareTo(b);
            });
            foreach (var k in bekleyen)
            {
                if (koltuklar.Count >= ArabaKoltuklari.Length)
                {
                    // koltuksuz kisi arabada ayakta (konsolun ustunde) gorunmesin: sesi duyulur, kendisi gizlenir
                    Debug.LogWarning("[Hikaye] arabada en fazla 4 kişi oturabilir: " + k + " için koltuk yok, görünmeyecek.");
                    ArabadaGizle(actors[k]);
                    continue;
                }
                string koltuk = BosKoltuk(k);
                // cocuk surucu olmaz: once on yolcu, doluysa arka koltuklar (yer: ile elle verilebilir)
                if (koltuk == "surucu" && KisaBoylu(actors[k]))
                    foreach (var ad in ArabaKoltuklari) if (ad != "surucu" && !koltuklar.ContainsValue(ad)) { koltuk = ad; break; }
                if (Application.isEditor) Debug.Log("[Araba] " + k + " -> " + koltuk + " (kafa yuksekligi " + actors[k].KafaYuksekligi.ToString("F2") + ")");
                KoltugaOturt(k, actors[k], koltuk);
            }
        }

        readonly List<Renderer> arabadaGizlenen = new List<Renderer>();

        // arabada koltuksuz kalip gizlenen oyuncu: kamera, tepki cekimi ve yuz efektleri onu saymaz
        bool ArabadaGizli(Karakter3D a)
        {
            foreach (var r in arabadaGizlenen) if (r != null && a != null && r.transform.IsChildOf(a.transform)) return true;
            return false;
        }

        void ArabadaGizle(Karakter3D a)
        {
            foreach (var r in a.GetComponentsInChildren<Renderer>())
                if (r.enabled) { r.enabled = false; arabadaGizlenen.Add(r); }
        }

        void ArabadaGoster(Karakter3D a)
        {
            for (int i = arabadaGizlenen.Count - 1; i >= 0; i--)
            {
                var r = arabadaGizlenen[i];
                if (r != null && a != null && !r.transform.IsChildOf(a.transform)) continue;
                if (r != null) r.enabled = true;
                arabadaGizlenen.RemoveAt(i);
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
                else if (actors.TryGetValue(kv.Key, out o) && o != null)
                    ArabadaGizle(o);     // bos koltuk kalmadi: ayni koltukta ic ice iki govde olmasin
            }
            Oturt(k, a, ad, w, yaw);
            return true;
        }

        void Oturt(string k, Karakter3D a, string koltuk, Vector3 nokta, float yaw)
        {
            BagBitir(k);
            YatakBirak(k, a);
            walking.Remove(k);
            ArabadaGoster(a);
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
            // yavas: / dondur: sirasinda oturma pozu da yavaslar; poz yerlesmeden olcme
            for (float t = 0f; t < 0.45f; t += Time.deltaTime * Karakter3D.Hiz) yield return null;
            string s;
            if (a == null || !koltuklar.TryGetValue(k, out s) || s != koltuk) yield break;
            var kalca = Kemik(a, HumanBodyBones.Hips);
            if (kalca == null) yield break;
            float fark = Mathf.Clamp(hedefY - kalca.position.y, -0.5f, 0.5f);
            a.transform.position += Vector3.up * fark;

            // kisa boylu (cocuk): yuz on konsolun arkasinda kalmasin diye yukseltilir (yukseltici koltuk gibi).
            // Kalca en cok 0,80'e cikar, boylece kucak yine konsol ust kenarinin (0,92) altinda kalir.
            Vector3 solGoz, sagGoz, agiz, ileri, yukari;
            float boy;
            if (!YuzNoktalari(k, out solGoz, out sagGoz, out agiz, out ileri, out yukari, out boy)) yield break;
            float eksik = ArabaAgizTabani - (agiz.y - Origin.y);
            float pay = ArabaKalcaTavani - (kalca.position.y - Origin.y);
            float ek = Mathf.Min(eksik, pay);
            if (ek > 0f) a.transform.position += Vector3.up * ek;
        }

        const float ArabaAgizTabani = 1.08f;     // agiz en az bu yukseklikte olmali (konsolun arkasindan gorunsun)
        const float ArabaKalcaTavani = 0.8f;     // kalca bundan yukari cikmaz (kucak gizli kalsin)

        // yumusak gecis YOK: yari oturur pozda dizler acik gorunuyordu (dondur: ile saniyelerce kaliyordu)
        static void AnindaDur(Karakter3D a)
        {
            foreach (var n in Hareket3D.KlipDur) if (a.Find(n) != null) { a.Play(n, 0f); break; }
        }

        // arabadan baska mekana gecerken herkes ayaga kalkar
        void ArabadanKaldir()
        {
            ArabadaGoster(null);
            // koltugu elinden alinan (gizlenen) kisi koltuklar listesinde yoktur: herkes uzerinden gidilir
            foreach (var kv in actors)
            {
                if (kv.Value == null) continue;
                YatakBirak(kv.Key, kv.Value);
                AnindaDur(kv.Value);
            }
            koltuklar.Clear();
        }
    }
}
