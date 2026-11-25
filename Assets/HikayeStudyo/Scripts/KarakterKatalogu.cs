using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Karakter listesi. Editörde "Hikaye/Karakter Kataloğunu Güncelle" menüsü doldurur;
    // derlenmiş uygulamada Resources/KarakterKatalogu olarak yüklenir.
    [CreateAssetMenu(menuName = "Hikaye Stüdyosu/Karakter Kataloğu", fileName = "KarakterKatalogu")]
    public class KarakterKatalogu : ScriptableObject, IVarlikKatalogu
    {
        [Serializable]
        public class Kayit
        {
            public string kimlik;      // örn. "Timmy"
            public string gorunenAd;
            public GameObject prefab;
        }

        public List<Kayit> karakterler = new List<Kayit>();

        List<string> kimlikler;

        public IReadOnlyList<string> KarakterKimlikleri
        {
            get
            {
                if (kimlikler == null || kimlikler.Count != karakterler.Count)
                {
                    kimlikler = new List<string>();
                    foreach (var k in karakterler) if (k != null && !string.IsNullOrEmpty(k.kimlik)) kimlikler.Add(k.kimlik);
                }
                return kimlikler;
            }
        }

        public GameObject KarakterPrefab(string kimlik)
        {
            if (string.IsNullOrEmpty(kimlik)) return null;
            foreach (var k in karakterler)
                if (k != null && k.prefab != null && string.Equals(k.kimlik, kimlik, StringComparison.OrdinalIgnoreCase)) return k.prefab;
            // çeşit adı ("VRM_Anne_005"): temel modelden çalışma anında üretilir
            string taban; int no;
            return Varyantlar.AdAyir(kimlik, out taban, out no) ? Varyantlar.Sablon(kimlik, Varyantlar.TumPrefablar(this)) : null;
        }

        void OnValidate() { kimlikler = null; }

        public static KarakterKatalogu Yukle() { return Resources.Load<KarakterKatalogu>("KarakterKatalogu"); }
    }
}
