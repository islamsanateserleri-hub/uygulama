using System.IO;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    public static class KatalogKurulum
    {
        const string Yol = "Assets/Resources/KarakterKatalogu.asset";

        [MenuItem("Hikaye/Karakter Kataloğunu Güncelle")]
        public static void Guncelle()
        {
            var kat = AssetDatabase.LoadAssetAtPath<KarakterKatalogu>(Yol);
            if (kat == null)
            {
                Directory.CreateDirectory("Assets/Resources");
                kat = ScriptableObject.CreateInstance<KarakterKatalogu>();
                AssetDatabase.CreateAsset(kat, Yol);
            }
            kat.karakterler.Clear();
            var klasorler = new[] { "Assets/HikayeStudyo/Karakterler" };
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", klasorler))
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                if (go == null) continue;
                string ad = Path.GetFileNameWithoutExtension(p);
                kat.karakterler.Add(new KarakterKatalogu.Kayit { kimlik = ad, gorunenAd = ad, prefab = go });
            }
            EditorUtility.SetDirty(kat);
            AssetDatabase.SaveAssets();
            Debug.Log("[Hikaye Stüdyosu] Karakter kataloğu güncellendi: " + kat.karakterler.Count + " karakter → " + Yol);
        }
    }
}
