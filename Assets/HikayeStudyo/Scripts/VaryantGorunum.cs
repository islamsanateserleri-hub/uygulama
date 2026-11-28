using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Bir karakter örneğine çeşit görünümünü uygular: Awake'te malzeme kopyalarını boyar,
    // Start'ta boy/en ölçeğini sahnenin verdiği ölçeğin üstüne çarpar. Kopyalar karakterle birlikte silinir.
    [DisallowMultipleComponent]
    public class VaryantGorunum : MonoBehaviour
    {
        public string taban = "";
        public int no;

        readonly List<Material> kopyalar = new List<Material>();   // serileştirilmez: her örnek kendi kopyasını yapar
        bool boyandi, olceklendi;
        int olcekNo;

        void Awake() { Boya(); }
        void Start() { Olcekle(); }
        void OnDestroy() { Temizle(); }

        // AddComponent ile sonradan eklenince (küçük resim, editör testi) çağrılır
        public void Kur(string tabanAdi, int sira)
        {
            taban = tabanAdi;
            no = sira;
            Boya();
        }

        void Boya()
        {
            if (boyandi || no < 1 || string.IsNullOrEmpty(taban)) return;
            boyandi = true;
            try { olcekNo = Varyantlar.Boya(gameObject, taban, no, kopyalar).olcek; }
            catch (Exception e) { Debug.LogWarning("[Hikaye] " + name + ": çeşit görünümü uygulanamadı: " + e.Message); }
        }

        public void Olcekle()
        {
            if (olceklendi || !boyandi) return;
            olceklendi = true;
            var o = Varyantlar.Olcek(olcekNo);
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(o.x, o.y, o.x));
        }

        // Editörde (Play dışında) OnDestroy çağrılmaz: test kodu bunu elle çağırır
        public void Temizle()
        {
            foreach (var m in kopyalar)
            {
                if (m == null) continue;
                if (Application.isPlaying) Destroy(m); else DestroyImmediate(m);
            }
            kopyalar.Clear();
        }
    }
}
