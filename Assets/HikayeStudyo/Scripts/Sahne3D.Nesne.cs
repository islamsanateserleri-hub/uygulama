using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 'nesne:' komutu: katalogdaki esya / hayvan / yapi / gok cismini sahneye koyar.
    //   nesne: at            nesne: kilic sag        nesne: araba arka
    //   nesne: dunya gok     nesne: sil at           nesne: yok
    public partial class Sahne3D
    {
        readonly Dictionary<string, GameObject> nesneler = new Dictionary<string, GameObject>();

        public static readonly string[] NesneYerleri = { "sol", "sag", "orta", "on", "arka", "uzak", "gok" };

        // "kurt kopegi sag arka" -> ad = "kurt_kopegi", yerler = {arka, sag}
        public static void NesneAyir(string arg, out string ad, out List<string> yerler)
        {
            yerler = new List<string>();
            var p = new List<string>((arg ?? "").Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries));
            while (p.Count > 1)
            {
                var son = Util.Norm(p[p.Count - 1]);
                if (System.Array.IndexOf(NesneYerleri, son) < 0) break;
                yerler.Add(son);
                p.RemoveAt(p.Count - 1);
            }
            ad = Util.Norm(string.Join("_", p.ToArray()));
        }

        // animasyon: Karakter hareket
        public void Animasyon(string id, string hareket)
        {
            var a = Aktor(id, true);
            if (a == null) { Debug.LogWarning("[Hikaye] animasyon: karakter sahnede değil / 3D değil: " + id); return; }
            a.Play((hareket ?? "").Trim());
        }

        public void NesneKomutu(string arg)
        {
            var parcalar = (arg ?? "").Trim().Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
            if (parcalar.Length == 0) return;
            var ilk = Util.Norm(parcalar[0]);
            if (ilk == "yok") { NesneTemizle(); return; }
            if (ilk == "sil")
            {
                if (parcalar.Length < 2) { NesneTemizle(); return; }
                string silAd; List<string> bos;
                NesneAyir(parcalar[1], out silAd, out bos);
                NesneSil(silAd);
                return;
            }

            string ad; List<string> yerler;
            NesneAyir(arg, out ad, out yerler);
            var kat = NesneKatalogu.Al();
            var n = kat != null ? kat.Bul(ad) : null;
            if (n == null || n.model == null) { Debug.LogWarning("[Hikaye] Nesne yok: " + ad); return; }
            NesneKoy(n, yerler);
        }

        void NesneSil(string ad)
        {
            var kat = NesneKatalogu.Al();
            var n = kat != null ? kat.Bul(ad) : null;
            var on = (n != null ? n.id : ad) + "@";
            var silinecek = new List<string>();
            foreach (var kv in nesneler) if (kv.Key.StartsWith(on)) silinecek.Add(kv.Key);
            foreach (var k in silinecek)
            {
                if (nesneler[k] != null) { nesneler[k].SetActive(false); Destroy(nesneler[k]); }
                nesneler.Remove(k);
            }
        }

        public void NesneTemizle()
        {
            foreach (var kv in nesneler) if (kv.Value != null) { kv.Value.SetActive(false); Destroy(kv.Value); }
            nesneler.Clear();
        }

        static bool Sinir(GameObject go, out Bounds b)
        {
            b = new Bounds();
            bool var1 = false;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                if (!var1) { b = r.bounds; var1 = true; }
                else b.Encapsulate(r.bounds);
            }
            return var1;
        }

        void NesneKoy(NesneKatalogu.Nesne n, List<string> yerler)
        {
            yerler.Sort(System.StringComparer.Ordinal);
            string anahtar = n.id + "@" + string.Join("+", yerler.ToArray());
            GameObject eski;
            if (nesneler.TryGetValue(anahtar, out eski) && eski != null) return;

            var kok = new GameObject("Nesne_" + n.id);
            kok.transform.SetParent(transform, false);
            kok.transform.position = Origin;
            var model = Instantiate(n.model, kok.transform);
            model.transform.localPosition = Vector3.zero;
            foreach (var c in model.GetComponentsInChildren<Camera>()) c.enabled = false;
            foreach (var l in model.GetComponentsInChildren<Light>()) l.enabled = false;
            kok.transform.rotation = Quaternion.Euler(0f, n.donus, 0f);

            Bounds b;
            if (!Sinir(kok, out b)) { Debug.LogWarning("[Hikaye] Nesnenin görüntüsü yok: " + n.id); Destroy(kok); return; }
            float enBuyuk = Mathf.Max(0.0001f, Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z)));
            kok.transform.localScale = Vector3.one * (n.boy / enBuyuk);
            Sinir(kok, out b);

            bool sol = yerler.Contains("sol"), sag = yerler.Contains("sag"), orta = yerler.Contains("orta");
            bool gok = yerler.Contains("gok") || (n.havada && !yerler.Contains("on") && !yerler.Contains("arka"));
            bool kucuk = n.boy <= 1.2f;

            float yarimTan = Mathf.Tan(CamFov * 0.5f * Mathf.Deg2Rad);
            float z = yerler.Contains("uzak") ? 9f : yerler.Contains("arka") ? 2.6f : yerler.Contains("on") ? -0.5f
                    : gok ? 6f : kucuk ? 0.35f : 2.4f;
            if (OdaAktif)
            {
                // oda duvarlarinin icinde kalsin: buyukse kucult
                z = Mathf.Min(z, 1.4f);
                float s = Mathf.Min(1f, Mathf.Min(1.8f / Mathf.Max(0.01f, b.size.x), 2.0f / Mathf.Max(0.01f, b.size.y)));
                if (s < 1f) { kok.transform.localScale *= s; Sinir(kok, out b); }
            }
            else
            {
                // kadraja sigacak kadar geriye
                float zEn = b.size.x * 1.1f / (2f * yarimTan * (W / H)) - CamDist;
                float zBoy = b.size.y * 1.9f / (2f * yarimTan) - CamDist;
                z = Mathf.Min(30f, Mathf.Max(z, Mathf.Max(zEn, zBoy)));
            }

            float derinlik = CamDist + z;
            float x = sol ? -0.62f : sag ? 0.62f : (!orta && kucuk && !gok) ? 0.62f : 0f;
            x *= derinlik / CamDist;                       // ekrandaki yeri ayni kalsin

            var hedef = Origin + new Vector3(x, 0f, z);
            if (gok)
            {
                float camY = Cam.transform.position.y - Origin.y;
                float gorunen = 2f * yarimTan * derinlik;
                hedef.y = Origin.y + camY + gorunen * 0.24f;
                kok.transform.position += hedef - b.center;
            }
            else
            {
                kok.transform.position += hedef - new Vector3(b.center.x, b.min.y, b.center.z);
            }
            if (n.kategori == "gok") kok.AddComponent<NesneDondur>().merkez = hedef;
            nesneler[anahtar] = kok;
        }
    }

    // Gezegenler yavasca doner
    public class NesneDondur : MonoBehaviour
    {
        public Vector3 merkez;
        void Update() { transform.RotateAround(merkez, Vector3.up, 6f * Time.deltaTime * Karakter3D.Hiz); }
    }
}
