using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Karakter Galerisi: butun 3D karakterleri ucer ucer sahneye cikarir, her birine farkli hareket yaptirir.
    public class GaleriOynatici : MonoBehaviour
    {
        public List<GameObject> karakterler = new List<GameObject>();
        public float sure = 5f;
        public int grup = 3;
        public float aralik = 1.3f;

        static readonly string[] insanHareketleri =
        {
            "Idle_Talking_Loop", "Walk_Loop", "Dance_Loop", "Jog_Fwd_Loop", "Sitting_Talking_Loop",
            "Interact", "Walk_Formal_Loop", "Spell_Simple_Idle_Loop", "Crouch_Idle_Loop", "Idle_Loop", "Driving_Loop",
            "Crying", "Thinking", "Scared", "Surprised", "Thankful", "Talking", "Sad_Idle"
        };
        static readonly string[] digerHareketler = { "Idle", "Walk", "Dance", "Wave", "Cheer", "Run", "Eating", "Gallop" };

        readonly List<GameObject> sahnede = new List<GameObject>();
        int sira, hareket;

        IEnumerator Start()
        {
            if (karakterler.Count == 0) { Debug.LogWarning("[Galeri] Karakter yok. Menü: Hikaye > 3D Karakterleri Hazırla"); yield break; }
            while (true)
            {
                foreach (var g in sahnede) if (g != null) Destroy(g);
                sahnede.Clear();
                int n = Mathf.Min(grup, karakterler.Count);
                for (int i = 0; i < n; i++)
                {
                    var prefab = karakterler[(sira + i) % karakterler.Count];
                    if (prefab == null) continue;
                    float x = (i - (n - 1) / 2f) * aralik;
                    var go = Instantiate(prefab, new Vector3(x, 0, 0), Quaternion.identity);   // modeller +Z yonune bakar, kamera +Z tarafinda
                    sahnede.Add(go);
                    var k = go.GetComponent<Karakter3D>();
                    string ad = prefab.name;
                    if (k != null)
                    {
                        k.startClip = "";
                        bool insan = k.kategori == "Erkek" || k.kategori == "Kadin" || k.kategori == "Mixamo" || k.kategori == "VRM";
                        var liste = insan ? insanHareketleri : digerHareketler;
                        string h = liste[(hareket + i) % liste.Length];
                        StartCoroutine(PlayNextFrame(k, h, liste));
                        ad = prefab.name.Replace('_', ' ');
                    }
                    Etiket(go.transform, ad);
                }
                sira = (sira + n) % karakterler.Count;
                hareket++;
                yield return new WaitForSeconds(sure);
            }
        }

        IEnumerator PlayNextFrame(Karakter3D k, string h, string[] yedek)
        {
            yield return null;   // Karakter3D.Start calissin
            if (k == null) yield break;
            if (!k.Play(h, 0.2f))
                foreach (var y in yedek) if (k.Find(y) != null) { k.Play(y, 0.2f); break; }
        }

        static void Etiket(Transform parent, string text)
        {
            var go = new GameObject("Etiket");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, 2.15f, 0);
            go.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.05f;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }
    }
}
