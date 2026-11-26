using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 'nesne:' komutunun esya/hayvan/yapi/gok cismi listesi + uzay arka planlari.
    // Dosya: Assets/Resources/NesneKatalogu.asset (menu: Hikaye/Her Seyi Ekle uretir)
    public class NesneKatalogu : ScriptableObject
    {
        [System.Serializable]
        public class Nesne
        {
            public string id = "";
            public string kategori = "";      // hayvan, silah, esya, zindan, sehir, uzay, gok
            public string[] takma = new string[0];
            public GameObject model;
            public float boy = 1f;            // en buyuk kenarin hedef boyu (metre)
            public float donus;               // y ekseninde donus (derece)
            public bool havada;               // varsayilan yeri gokyuzu (gezegen, uydu)
        }

        [System.Serializable]
        public class Gok
        {
            public string id = "";
            public Sprite resim;
        }

        public List<Nesne> nesneler = new List<Nesne>();
        public List<Gok> gokler = new List<Gok>();

        static NesneKatalogu onbellek;

        public static NesneKatalogu Al()
        {
            if (onbellek == null) onbellek = Resources.Load<NesneKatalogu>("NesneKatalogu");
            return onbellek;
        }

        // Sirasiyla: tam ad, takma ad, "ad_..." ile baslayan, icinde gecen
        public Nesne Bul(string ad)
        {
            var k = Util.Norm(ad);
            if (k.Length == 0) return null;
            foreach (var n in nesneler) if (n != null && n.id == k) return n;
            foreach (var n in nesneler)
                if (n != null && n.takma != null) foreach (var t in n.takma) if (t == k) return n;
            foreach (var n in nesneler) if (n != null && n.id.StartsWith(k + "_")) return n;
            foreach (var n in nesneler) if (n != null && k.Length > 3 && n.id.Contains(k)) return n;
            return null;
        }

        public Sprite GokBul(string ad)
        {
            var k = Util.Norm(ad);
            foreach (var g in gokler) if (g != null && g.id == k) return g.resim;
            return null;
        }

        public List<string> Benzer(string ad, int sayi)
        {
            var k = Util.Norm(ad);
            var on = k.Length > 3 ? k.Substring(0, 3) : k;
            var l = new List<string>();
            foreach (var n in nesneler)
            {
                if (l.Count >= sayi) break;
                if (n != null && on.Length > 0 && n.id.Contains(on)) l.Add(n.id);
            }
            return l;
        }
    }
}
