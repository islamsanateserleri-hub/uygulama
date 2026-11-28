using UnityEngine;

namespace HikayeStudyo
{
    // Masal / korku mekanlari: zindan, sato, magara, mezarlik, orman.
    // Zindan esyalari indirilen KayKit paketinden (NesneKatalogu) gelir.
    public partial class Mekan3D
    {
        // Katalogdaki esyayi hedef yukseklikte, tabani y'de olacak sekilde koyar
        void Esya(string id, float x, float y, float z, float yaw, float boy)
        {
            var kat = NesneKatalogu.Al();
            var n = kat != null ? kat.Bul(id) : null;
            if (n == null || n.model == null) return;
            var go = Instantiate(n.model, root);
            go.name = "Esya_" + id;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Destroy(c);
            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) { Destroy(go); return; }
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            if (b.size.y < 0.001f) { Destroy(go); return; }
            go.transform.localScale *= boy / b.size.y;
            b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            go.transform.position += Vector3.up * (root.position.y + y - b.min.y);
        }

        void TasDuvar(float genislik, float yukseklik, float z, Color tas)
        {
            var rnd = new System.Random(7);
            const float eb = 0.62f, ey = 0.3f;
            for (int s = 0; s * ey < yukseklik; s++)
                for (float x = -genislik / 2f + (s % 2) * eb * 0.5f; x < genislik / 2f; x += eb)
                    B("Tas", x + eb * 0.25f, s * ey + ey / 2f, z - (float)rnd.NextDouble() * 0.03f, eb - 0.03f, ey - 0.03f, 0.06f,
                      tas * (0.82f + (float)rnd.NextDouble() * 0.36f), 0.04f);
        }

        void Alev(float x, float y, float z)
        {
            var a = S("Alev", x, y, z, 0.11f, K(1f, 0.55f, 0.1f));
            a.transform.localScale = new Vector3(0.1f, 0.17f, 0.1f);
            Parlak(a, K(1f, 0.55f, 0.12f));
            Lamba("MesaleIsigi", x, y + 0.05f, z - 0.35f, K(1f, 0.62f, 0.28f), 1.5f, 5f, true);
        }

        void Mesale(float x, float z)
        {
            B("MesaleSap", x, 1.7f, z, 0.07f, 0.34f, 0.07f, K(0.3f, 0.18f, 0.08f));
            B("MesaleKafa", x, 1.9f, z, 0.13f, 0.08f, 0.13f, K(0.12f, 0.12f, 0.13f), 0.4f);
            Alev(x, 2.02f, z);
        }

        void KurZindan()
        {
            Dis = false;
            Color tas = Duvar(K(0.3f, 0.29f, 0.31f));
            Zarf(5.4f, 3.1f, 3.2f, -8f, K(0.17f, 0.16f, 0.17f), K(0.12f, 0.11f, 0.13f), K(0.08f, 0.07f, 0.08f), 0.1f);
            TasDuvar(5.4f, 3.1f, 3.12f, tas);
            // demir parmakli hucre
            B("HucreIci", 1.35f, 1.05f, 3.07f, 1.15f, 2.1f, 0.03f, K(0.03f, 0.03f, 0.04f), 0f);
            for (int i = 0; i <= 6; i++) C("Parmaklik", 0.8f + i * 0.183f, 1.05f, 3.0f, 0.022f, 2.1f, K(0.2f, 0.2f, 0.22f), 0.55f);
            B("ParmaklikUst", 1.35f, 1.9f, 3.0f, 1.2f, 0.07f, 0.06f, K(0.2f, 0.2f, 0.22f), 0.55f);
            B("ParmaklikAlt", 1.35f, 0.5f, 3.0f, 1.2f, 0.07f, 0.06f, K(0.2f, 0.2f, 0.22f), 0.55f);
            Mesale(-0.35f, 3.0f);
            Mesale(2.25f, 3.0f);
            Esya("fici_buyuk", -2.05f, 0f, 2.7f, 20f, 0.9f);
            Esya("fici_kucuk_yigin", -1.45f, 0f, 2.85f, -10f, 0.65f);
            Esya("kutu_stacked", 2.25f, 0f, 2.6f, 15f, 0.8f);
            Esya("sandik_altin", -0.55f, 0f, 2.75f, 175f, 0.55f);
            Esya("rubble_half", 2.1f, 0f, 1.7f, 40f, 0.25f);
            N("hucre", 1.35f, 0f, 2.5f, 180f);
            N("kapi", 1.35f, 0f, 2.5f, 180f);
            N("sandik", -0.55f, 0f, 2.2f, 0f);
            xMin = -1.9f; xMax = 0.4f; zMin = 1.9f; zMax = 2.5f;
        }

        void KurSato()
        {
            Dis = false;
            Color tas = Duvar(K(0.55f, 0.5f, 0.44f));
            Color kirmizi = Vurgu(K(0.55f, 0.08f, 0.1f));
            Zarf(6f, 3.8f, 3.6f, -8f, Zemin(K(0.3f, 0.27f, 0.25f)), K(0.3f, 0.27f, 0.24f), K(0.2f, 0.17f, 0.15f), 0.35f);
            TasDuvar(6f, 3.8f, 3.52f, tas);
            B("Hali", 0f, 0.006f, 0.8f, 1.5f, 0.012f, 5f, kirmizi, 0.02f);
            // taht
            B("Basamak", 0f, 0.08f, 3.0f, 1.7f, 0.16f, 0.9f, K(0.4f, 0.36f, 0.32f), 0.2f);
            B("TahtOturak", 0f, 0.55f, 3.1f, 0.74f, 0.14f, 0.55f, kirmizi, 0.15f);
            B("TahtAyak", 0f, 0.32f, 3.12f, 0.7f, 0.34f, 0.5f, K(0.75f, 0.58f, 0.2f), 0.6f);
            B("TahtSirt", 0f, 1.22f, 3.36f, 0.74f, 1.2f, 0.1f, K(0.75f, 0.58f, 0.2f), 0.6f);
            B("TahtMinder", 0f, 1.15f, 3.29f, 0.52f, 0.92f, 0.05f, kirmizi, 0.15f);
            foreach (var s in new[] { -1f, 1f })
            {
                B("TahtKol", s * 0.43f, 0.74f, 3.1f, 0.12f, 0.12f, 0.55f, K(0.75f, 0.58f, 0.2f), 0.6f);
                S("TahtTopuz", s * 0.37f, 1.9f, 3.36f, 0.14f, K(0.9f, 0.72f, 0.25f), 0.7f);
                C("Sutun", s * 2.2f, 1.9f, 3.0f, 0.2f, 3.8f, tas * 1.1f, 0.2f);
                B("SutunBaslik", s * 2.2f, 3.2f, 3.0f, 0.55f, 0.18f, 0.55f, tas * 0.85f, 0.2f);
                Esya("sancak_kirmizi", s * 1.15f, 1.25f, 3.38f, 0f, 1.6f);
                Mesale(s * 1.7f, 3.35f);
                Esya("mum_triple", s * 1.0f, 0.16f, 2.85f, 0f, 0.35f);
            }
            Esya("altin_yigin_buyuk", -2.1f, 0f, 2.2f, 30f, 0.45f);
            Esya("sandik_altin", 2.1f, 0f, 2.25f, 200f, 0.6f);
            N("taht", 0f, 0.6f, 2.95f, 180f);
            N("koltuk", 0f, 0.6f, 2.95f, 180f);
            N("kapi", -2.4f, 0f, 1.8f, 90f);
            xMin = -2.0f; xMax = -0.9f; zMin = 1.8f; zMax = 2.5f;
        }

        void Kaya(float x, float y, float z, float en, float boy, float derin, Color c)
        {
            var k = S("Kaya", x, y, z, 1f, c, 0.03f);
            k.transform.localScale = new Vector3(en, boy, derin);
            k.transform.localRotation = Quaternion.Euler((x * 37f) % 25f, (z * 53f) % 180f, (x * z * 29f) % 25f);
        }

        void KurMagara()
        {
            Dis = false;
            Color tas = Duvar(K(0.24f, 0.2f, 0.19f));
            Zarf(5.6f, 3.2f, 3.4f, -8f, Zemin(K(0.16f, 0.13f, 0.11f)), K(0.06f, 0.05f, 0.05f), K(0.07f, 0.06f, 0.06f), 0.05f);
            var rnd = new System.Random(11);
            for (int i = 0; i < 46; i++)
            {
                float x = -2.8f + (float)rnd.NextDouble() * 5.6f, y = (float)rnd.NextDouble() * 3.1f;
                float d = 0.6f + (float)rnd.NextDouble() * 0.7f;
                Kaya(x, y, 3.35f, d, d * 0.8f, d * 0.55f, tas * (0.7f + (float)rnd.NextDouble() * 0.6f));
            }
            foreach (var s in new[] { -1f, 1f })
                for (int i = 0; i < 5; i++) Kaya(s * 2.75f, 0.3f + i * 0.65f, 2.6f - (i % 2) * 0.5f, 0.9f, 1f, 1.2f, tas * (0.8f + i * 0.07f));
            // sarkit ve dikitler
            for (int i = 0; i < 11; i++)
            {
                float x = -2.4f + i * 0.48f, boy = 0.3f + (float)rnd.NextDouble() * 0.55f;
                var k = Yaratik.Koni(root, new Vector3(x, 3.2f, 2.6f + (i % 3) * 0.25f), 0.2f, boy, new Vector3(180f, 0f, 0f), Mat(tas * 1.15f, 0.1f, 0f, false));
                k.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach (var x in new[] { -2.2f, -1.9f, 2.25f })
                Yaratik.Koni(root, new Vector3(x, 0f, 2.5f), 0.28f, 0.35f + Mathf.Abs(x) * 0.12f, Vector3.zero, Mat(tas * 1.1f, 0.1f, 0f, false));
            // parlayan kristaller
            Color kristal = Vurgu(K(0.25f, 0.75f, 1f));
            foreach (var p in new[] { new Vector3(-1.55f, 0f, 2.8f), new Vector3(1.7f, 0f, 2.75f), new Vector3(2.35f, 1.3f, 2.9f) })
            {
                for (int i = -1; i <= 1; i++)
                {
                    var m = Mat(kristal, 0.8f, 1.3f, false);
                    Yaratik.Koni(root, p + new Vector3(i * 0.13f, 0f, 0f), 0.13f, 0.5f - Mathf.Abs(i) * 0.18f, new Vector3(0f, 0f, -i * 28f), m);
                }
                Lamba("KristalIsigi", p.x, p.y + 0.4f, p.z - 0.4f, kristal, 1.1f, 3.5f, false);
            }
            Lamba("MagaraIsigi", 0f, 2.2f, 0.4f, K(1f, 0.72f, 0.45f), 0.9f, 7f, false);
            N("kristal", -1.4f, 0f, 2.3f, 0f);
            xMin = -1.8f; xMax = 1.8f; zMin = 1.9f; zMax = 2.5f;
        }

        void MezarTasi(float x, float z, float egim, bool hac)
        {
            Color tas = K(0.46f, 0.48f, 0.5f);
            var g = new GameObject("Mezar").transform;
            g.SetParent(root, false);
            g.localPosition = new Vector3(x, 0f, z);
            g.localRotation = Quaternion.Euler(0f, 0f, egim);
            if (hac)
            {
                B("HacDikey", 0f, 0.5f, 0f, 0.13f, 1f, 0.1f, tas, 0.1f, 0f, g);
                B("HacYatay", 0f, 0.72f, 0f, 0.5f, 0.13f, 0.1f, tas, 0.1f, 0f, g);
            }
            else
            {
                B("Tas", 0f, 0.32f, 0f, 0.52f, 0.64f, 0.13f, tas, 0.1f, 0f, g);
                var ust = C("TasUst", 0f, 0.64f, 0f, 0.26f, 0.13f, tas, 0.1f, 0f, g);
                ust.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                B("Yazi", 0f, 0.5f, -0.07f, 0.3f, 0.035f, 0.01f, K(0.2f, 0.2f, 0.22f), 0f, 0f, g);
                B("Yazi2", 0f, 0.4f, -0.07f, 0.22f, 0.03f, 0.01f, K(0.2f, 0.2f, 0.22f), 0f, 0f, g);
            }
            B("Toprak", x, 0.03f, z - 0.55f, 0.6f, 0.1f, 0.95f, K(0.2f, 0.15f, 0.11f), 0.02f);
        }

        void KuruAgac(float x, float z, float boy)
        {
            Color k = K(0.13f, 0.09f, 0.07f);
            C("KuruGovde", x, boy / 2f, z, 0.1f, boy, k, 0.03f);
            for (int i = 0; i < 5; i++)
            {
                float yon = i % 2 == 0 ? 1f : -1f, y = boy * (0.55f + i * 0.1f);
                var d = C("Dal", x + yon * 0.28f, y + 0.18f, z, 0.035f, 0.7f, k, 0.03f);
                d.transform.localRotation = Quaternion.Euler(0f, 0f, -yon * (38f + i * 6f));
            }
        }

        void KurMezarlik()
        {
            Dis = true;
            B("Zemin", 0f, -0.1f, 8f, 40f, 0.2f, 40f, Zemin(K(0.14f, 0.19f, 0.13f)), 0.03f);
            B("Patika", 0f, 0.004f, 3f, 1.2f, 0.01f, 12f, K(0.26f, 0.22f, 0.18f), 0.02f);
            float[] x = { -1.5f, 1.45f, -2.5f, 2.5f, -1.2f, 1.1f, -3.4f, 3.2f, -2.0f, 2.1f };
            float[] z = { 2.7f, 2.9f, 4.0f, 4.3f, 5.6f, 5.9f, 6.4f, 6.8f, 8.2f, 8.6f };
            for (int i = 0; i < x.Length; i++) MezarTasi(x[i], z[i], (i % 3 - 1) * 7f, i % 3 == 1);
            KuruAgac(-2.6f, 5.0f, 2.4f);
            KuruAgac(2.9f, 7.5f, 3.0f);
            KuruAgac(0.8f, 11f, 3.4f);
            for (int i = 0; i < 40; i++)
            {
                float px = -6f + i * 0.31f;
                if (Mathf.Abs(px) < 0.7f) continue;
                B("Citdemir", px, 0.55f, 10f, 0.04f, 1.1f, 0.04f, K(0.08f, 0.08f, 0.09f), 0.4f);
            }
            B("CitUst", 0f, 0.95f, 10f, 12.4f, 0.05f, 0.04f, K(0.08f, 0.08f, 0.09f), 0.4f);
            var ay = S("Ay", 2.2f, 4.6f, 14f, 1.3f, K(0.92f, 0.94f, 1f));
            Parlak(ay, K(0.85f, 0.9f, 1f));
            Lamba("AyIsigi", 1f, 3f, 1f, K(0.6f, 0.7f, 1f), 0.9f, 9f, false);
            Esya("mum_yanik", -1.5f, 0.08f, 2.2f, 0f, 0.2f);
            Lamba("MumIsigi", -1.5f, 0.35f, 2.0f, K(1f, 0.65f, 0.3f), 0.8f, 2f, true);
            N("mezar", -1.1f, 0f, 2.2f, 0f);
            xMin = -1.8f; xMax = 1.8f; zMin = 2.2f; zMax = 3.2f;
        }

        void KurOrman()
        {
            Dis = true;
            B("Zemin", 0f, -0.1f, 8f, 40f, 0.2f, 40f, Zemin(K(0.2f, 0.34f, 0.14f)), 0.03f);
            B("Patika", 0.1f, 0.004f, 4f, 1.1f, 0.01f, 14f, K(0.4f, 0.3f, 0.19f), 0.02f);
            var rnd = new System.Random(5);
            for (int i = 0; i < 46; i++)
            {
                float z = 2.7f + (float)rnd.NextDouble() * 12f;
                float x = -6f + (float)rnd.NextDouble() * 12f;
                if (Mathf.Abs(x) < 0.9f + z * 0.03f) x += x < 0f ? -1.2f : 1.2f;
                Color yaprak = K(0.1f, 0.3f, 0.13f) * (0.8f + (float)rnd.NextDouble() * 0.6f);
                if (i % 3 == 0) Agac(x, z, 1.4f, 1.7f, yaprak);
                else
                {
                    float g = 0.5f + (float)rnd.NextDouble() * 0.4f;
                    C("CamGovde", x, g / 2f, z, 0.09f, g, K(0.28f, 0.18f, 0.1f), 0.03f);
                    var m = Mat(yaprak, 0.03f, 0f, false);
                    for (int k = 0; k < 3; k++) Yaratik.Koni(root, new Vector3(x, g + k * 0.45f, z), 1.25f - k * 0.3f, 0.85f, Vector3.zero, m);
                }
            }
            for (int i = 0; i < 30; i++)
            {
                float x = -3.5f + (float)rnd.NextDouble() * 7f, z = 2.0f + (float)rnd.NextDouble() * 4f;
                if (Mathf.Abs(x) < 0.7f) continue;
                S("Cali", x, 0.08f, z, 0.2f + (float)rnd.NextDouble() * 0.2f, K(0.16f, 0.36f, 0.12f), 0.03f);
            }
            Kaya(-1.7f, 0.15f, 2.6f, 0.8f, 0.45f, 0.6f, K(0.4f, 0.4f, 0.38f));
            // mantarlar
            foreach (var p in new[] { new Vector2(1.45f, 2.3f), new Vector2(1.65f, 2.45f) })
            {
                C("MantarSap", p.x, 0.09f, p.y, 0.035f, 0.18f, K(0.93f, 0.9f, 0.8f));
                var s = S("MantarSapka", p.x, 0.19f, p.y, 0.18f, K(0.8f, 0.1f, 0.08f), 0.3f);
                s.transform.localScale = new Vector3(0.2f, 0.11f, 0.2f);
            }
            N("agac", -1.8f, 0f, 2.4f, 180f);
            N("kaya", -1.5f, 0f, 2.1f, 0f);
            xMin = -1.8f; xMax = 1.8f; zMin = 2.4f; zMax = 3.2f;
        }
    }
}
