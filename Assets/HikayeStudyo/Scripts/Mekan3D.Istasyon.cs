using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Uzay istasyonu mekanlari: kopru (ana dinleme koprusu), koridor (Koridor Gamma), hava_kilidi (Sektor Sifir).
    // Hikayede 'istasyon:' komutu: sinyal | gurultu | kamera | mavi | mor | bozuk | kirag | izler | karart | yak
    public partial class Mekan3D
    {
        public static readonly string[] IstasyonDurumlari = { "sinyal", "gurultu", "kamera", "mavi", "mor", "bozuk", "kirag", "izler", "karart", "yak" };

        // Mira'nin isik halkasi
        Transform halka;
        Material halkaMat;
        Light halkaIsik;
        Color halkaRenk = new Color(0.55f, 0.85f, 1f), halkaHedef = new Color(0.55f, 0.85f, 1f);
        float halkaKonusma;
        bool halkaBozuk;

        // dalga ekrani / dis kamera
        readonly List<Transform> cubuklar = new List<Transform>();
        Material cubukMat;
        GameObject dalgaGrubu, kameraGrubu;
        bool sinyalVar;

        // surgulu basinc kapisi
        Transform surgu;
        float surguYol, surguBazX;
        Material panelMat;

        Transform damla;
        readonly List<Transform> kiriklar = new List<Transform>();   // gozlem camindaki donen geometri
        readonly List<GameObject> kiragParca = new List<GameObject>();
        float kiragT = -1f;
        readonly List<Transform> ayakIzleri = new List<Transform>();
        float izT = -1f;

        // Sahne3D: Mira konusurken halka nabiz atar
        public void HalkaKonus(float sure) { halkaKonusma = Mathf.Max(halkaKonusma, sure); }

        public bool IstasyonDurum(string durum)
        {
            switch (Util.Norm(durum))
            {
                case "sinyal": sinyalVar = true; DalgaGoster(true); return true;
                case "gurultu": sinyalVar = false; DalgaGoster(true); return true;
                case "kamera": DalgaGoster(false); return true;
                case "mavi": halkaHedef = new Color(0.55f, 0.85f, 1f); halkaBozuk = false; return true;
                case "mor": halkaHedef = new Color(0.72f, 0.5f, 0.95f); return true;
                case "bozuk": halkaBozuk = true; return true;
                case "karart": case "yak":
                    foreach (var l in root.GetComponentsInChildren<Light>(true)) l.enabled = Util.Norm(durum) == "yak";
                    return true;
                case "kirag": kiragT = 0f; return true;
                case "izler":
                    izT = 0f;
                    foreach (var t in ayakIzleri) if (t != null) { t.gameObject.SetActive(true); t.localScale = new Vector3(0.11f, 0.004f, 0.25f); }
                    return true;
            }
            return false;
        }

        void DalgaGoster(bool dalga)
        {
            if (dalgaGrubu != null) dalgaGrubu.SetActive(dalga);
            if (kameraGrubu != null) kameraGrubu.SetActive(!dalga);
        }

        static void Isima(Material m, Color c, float guc)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c * 0.5f);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c * 0.5f);
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * guc);
        }

        void IstasyonGuncelle(float dt)
        {
            if (halka != null)
            {
                halkaRenk = Color.Lerp(halkaRenk, halkaHedef, dt * 1.6f);
                float nabiz = 0f;
                if (halkaKonusma > 0f)
                {
                    halkaKonusma -= dt;
                    nabiz = Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.5f + Mathf.PerlinNoise(Time.time * 11f, 0.3f) * 0.5f;
                }
                float titreme = halkaBozuk && Mathf.PerlinNoise(Time.time * 17f, 4f) < 0.38f ? 0.25f : 1f;
                Isima(halkaMat, halkaRenk, (0.9f + nabiz * 1.4f) * titreme);
                halka.localScale = Vector3.one * (1f + nabiz * 0.09f);
                if (halkaIsik != null) { halkaIsik.color = halkaRenk; halkaIsik.intensity = (0.5f + nabiz * 0.8f) * titreme; }
            }
            if (cubuklar.Count > 0)
            {
                for (int i = 0; i < cubuklar.Count; i++)
                {
                    float h = sinyalVar
                        ? 0.05f + Mathf.Abs(Mathf.Sin(i * 0.55f - Time.time * 5f)) * (0.12f + 0.3f * Mathf.Exp(-Mathf.Pow((i - cubuklar.Count * 0.5f) / 6f, 2f)))
                        : 0.02f + Mathf.PerlinNoise(i * 1.7f, Time.time * 7f) * 0.12f;
                    var s = cubuklar[i].localScale;
                    cubuklar[i].localScale = new Vector3(s.x, h, s.z);
                }
                Isima(cubukMat, sinyalVar ? new Color(0.72f, 0.35f, 1f) : new Color(0.55f, 0.75f, 0.8f), sinyalVar ? 1.5f : 0.7f);
            }
            if (damla != null)
            {
                float f = Mathf.Repeat(Time.time, 3.4f);
                float y = f < 2.6f ? 2.75f : 2.75f - (f - 2.6f) * (f - 2.6f) * 4.6f;
                var p = damla.localPosition;
                damla.localPosition = new Vector3(p.x, Mathf.Max(0.02f, y), p.z);
            }
            for (int i = 0; i < kiriklar.Count; i++)
                if (kiriklar[i] != null) kiriklar[i].Rotate(0f, 0f, (i % 2 == 0 ? 1f : -1f) * (5f + i * 2.5f) * dt, Space.Self);
            if (kiragT >= 0f)
            {
                kiragT += dt;
                int acik = Mathf.Min(kiragParca.Count, (int)(kiragT / 3f * kiragParca.Count));
                for (int i = 0; i < acik; i++) if (!kiragParca[i].activeSelf) kiragParca[i].SetActive(true);
            }
            if (izT >= 0f)
            {
                izT += dt;
                float k = Mathf.Clamp01(1f - (izT - 1.5f) / 2.5f);
                foreach (var t in ayakIzleri)
                    if (t != null)
                    {
                        t.localScale = new Vector3(0.11f * k, 0.004f, 0.25f * k);
                        if (k <= 0f) t.gameObject.SetActive(false);
                    }
                if (k <= 0f) izT = -1f;
            }
            if (surgu != null)
            {
                kapiT = Mathf.MoveTowards(kapiT, kapiHedef ? 1f : 0f, dt * 0.45f);   // agir hidrolik kapi
                float e = kapiT * kapiT * (3f - 2f * kapiT);
                var p = surgu.localPosition;
                surgu.localPosition = new Vector3(surguBazX + surguYol * e, p.y, p.z);
                Isima(panelMat, kapiHedef ? new Color(0.2f, 1f, 0.35f) : new Color(1f, 0.12f, 0.1f), 1.4f);
            }
        }

        // ---------- Ortak parcalar ----------
        GameObject Isikli(string ad, float x, float y, float z, float sx, float sy, float sz, Color c, float guc, out Material m, Transform parent = null)
        {
            var g = Part(PrimitiveType.Cube, ad, x, y, z, sx, sy, sz, c, 0.3f, guc, parent, true);
            m = g.GetComponent<MeshRenderer>().sharedMaterial;
            return g;
        }

        Transform Grup(string ad, float x, float y, float z)
        {
            var g = new GameObject(ad).transform;
            g.SetParent(root, false);
            g.localPosition = new Vector3(x, y, z);
            return g;
        }

        void HalkaYap(float x, float y, float z, float r)
        {
            B("HalkaPanel", x, y, z + 0.03f, r * 2.7f, r * 2.7f, 0.02f, K(0.02f, 0.03f, 0.05f), 0.8f);
            halka = Grup("MiraHalkasi", x, y, z);
            var ilk = Part(PrimitiveType.Cube, "HalkaDilim0", 0f, r, 0f, r * 0.2f, r * 0.09f, 0.02f, halkaRenk, 0.3f, 1.2f, halka, true);
            halkaMat = ilk.GetComponent<MeshRenderer>().sharedMaterial;
            const int n = 40;
            for (int i = 1; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                var d = Part(PrimitiveType.Cube, "HalkaDilim" + i, Mathf.Sin(a) * r, Mathf.Cos(a) * r, 0f, r * 0.2f, r * 0.09f, 0.02f, halkaRenk, 0.3f, 1.2f, halka, false);
                d.GetComponent<MeshRenderer>().sharedMaterial = halkaMat;
                d.transform.localRotation = Quaternion.Euler(0f, 0f, -a * Mathf.Rad2Deg);
            }
            var go = new GameObject("MiraIsigi");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(x, y, z - 0.35f);
            halkaIsik = go.AddComponent<Light>();
            halkaIsik.type = LightType.Point; halkaIsik.range = 3f; halkaIsik.shadows = LightShadows.None;
            halkaIsik.color = halkaRenk; halkaIsik.intensity = 0.5f;
        }

        void SabitIsik(string ad, float x, float y, float z, Color c, float siddet, float menzil)
        {
            var go = new GameObject(ad);
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(x, y, z);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = c; l.intensity = siddet; l.range = menzil;
            l.shadows = LightShadows.None;
        }

        // ---------- ANA DINLEME KOPRUSU ----------
        void KurKopru()
        {
            Dis = false;
            Color metal = K(0.14f, 0.17f, 0.22f), koyu = K(0.07f, 0.09f, 0.12f), cizgi = K(0.24f, 0.28f, 0.34f);
            Zarf(5.2f, 3.2f, 3.14f, -8f, K(0.1f, 0.12f, 0.15f), metal, koyu, 0.55f);
            for (int i = -3; i <= 3; i++) B("DuvarEki" + i, i * 0.7f, 1.6f, 3.07f, 0.03f, 3.2f, 0.02f, cizgi, 0.4f);
            for (int i = 0; i < 4; i++) B("ZeminEki" + i, 0f, 0.005f, -0.6f + i * 0.9f, 5.2f, 0.008f, 0.03f, cizgi, 0.5f);
            C("SogutmaBorusu", 0f, 2.98f, 2.7f, 0.08f, 0.1f, cizgi, 0.6f);
            B("BoruGovde", 0f, 2.98f, 2.75f, 5.2f, 0.12f, 0.12f, cizgi, 0.6f);
            damla = S("SuDamlasi", -0.55f, 2.75f, 1.0f, 0.05f, K(0.6f, 0.85f, 1f), 0.95f, 0.5f).transform;

            // buyuk ekran: dalga formu / dis kamera
            B("EkranCerceve", 0.36f, 1.92f, 3.05f, 1.54f, 0.96f, 0.04f, K(0.3f, 0.34f, 0.4f), 0.5f);
            B("EkranZemin", 0.36f, 1.92f, 3.02f, 1.42f, 0.84f, 0.02f, K(0.01f, 0.03f, 0.06f), 0.8f);
            dalgaGrubu = Grup("DalgaFormu", 0.36f, 1.92f, 2.99f).gameObject;
            var ilk = Part(PrimitiveType.Cube, "Dalga0", -0.65f, 0f, 0f, 0.032f, 0.08f, 0.01f, K(0.6f, 0.8f, 0.85f), 0.3f, 0.8f, dalgaGrubu.transform, true);
            cubukMat = ilk.GetComponent<MeshRenderer>().sharedMaterial;
            cubuklar.Add(ilk.transform);
            for (int i = 1; i < 30; i++)
            {
                var d = Part(PrimitiveType.Cube, "Dalga" + i, -0.65f + i * 0.0448f, 0f, 0f, 0.032f, 0.08f, 0.01f, K(0.6f, 0.8f, 0.85f), 0.3f, 0.8f, dalgaGrubu.transform, false);
                d.GetComponent<MeshRenderer>().sharedMaterial = cubukMat;
                cubuklar.Add(d.transform);
            }
            // dis kamera: buzlu kayaliklar, yildizsiz bosluk
            kameraGrubu = Grup("DisKamera", 0.36f, 1.92f, 2.98f).gameObject;
            var rnd = new System.Random(4);
            for (int i = 0; i < 11; i++)
            {
                float x = -0.6f + i * 0.12f, h = 0.12f + (float)rnd.NextDouble() * 0.28f;
                var kaya = B("BuzKaya" + i, x, -0.42f + h * 0.35f, -0.002f * i, 0.2f, h, 0.01f, K(0.42f, 0.55f, 0.7f), 0.6f, 0.25f, kameraGrubu.transform);
                kaya.transform.localRotation = Quaternion.Euler(0f, 0f, (float)rnd.NextDouble() * 60f - 30f);
            }
            kameraGrubu.SetActive(false);

            // konsol, monitorler, osiloskoplar, kayit cihazi, kahve
            B("KonsolGovde", 0f, 0.45f, 2.1f, 3.4f, 0.9f, 0.6f, koyu, 0.4f);
            B("KonsolUst", 0f, 0.92f, 2.05f, 3.5f, 0.05f, 0.75f, cizgi, 0.6f);
            float[] mx = { -1.15f, -0.5f, 0.55f, 1.2f };
            for (int i = 0; i < mx.Length; i++)
            {
                Material m;
                B("MonitorKasa" + i, mx[i], 1.17f, 2.2f, 0.48f, 0.36f, 0.12f, K(0.05f, 0.06f, 0.08f), 0.5f);
                Isikli("MonitorCam" + i, mx[i], 1.17f, 2.13f, 0.42f, 0.29f, 0.01f, K(0.08f, 0.3f, 0.7f), 0.8f, out m);
                for (int k = 0; k < 3; k++)
                    B("MonitorSatir" + i + "_" + k, mx[i] - 0.05f * k, 1.25f - k * 0.08f, 2.115f, 0.26f - 0.04f * k, 0.012f, 0.005f, K(0.5f, 0.85f, 1f), 0.3f, 1f);
            }
            for (int i = 0; i < 12; i++)
                B("Tus" + i, -0.9f + i * 0.16f, 0.955f, 1.85f, 0.08f, 0.02f, 0.06f, i % 5 == 0 ? K(0.9f, 0.5f, 0.15f) : K(0.3f, 0.55f, 0.7f), 0.5f, 0.4f);
            B("KayitCihazi", -0.2f, 1.0f, 1.82f, 0.26f, 0.09f, 0.16f, K(0.35f, 0.36f, 0.38f), 0.5f);
            S("KayitDugmesi", -0.12f, 1.05f, 1.74f, 0.035f, K(1f, 0.15f, 0.1f), 0.5f, 1.2f);
            C("KahveKupa", 0.3f, 1.01f, 1.8f, 0.055f, 0.12f, K(0.75f, 0.76f, 0.78f), 0.5f);
            C("Kahve", 0.3f, 1.072f, 1.8f, 0.046f, 0.006f, K(0.12f, 0.06f, 0.03f), 0.9f);

            HalkaYap(-0.72f, 1.9f, 3.04f, 0.2f);
            SabitIsik("MaviMonitorIsigi", 0f, 1.4f, 1.4f, K(0.3f, 0.55f, 1f), 1.5f, 4.5f);
            SabitIsik("TavanIsigi", -0.6f, 2.8f, -0.8f, K(0.55f, 0.65f, 0.85f), 0.9f, 6f);
            N("kapi", 1.9f, 0f, 1.2f, 270f);
            N("konsol", 0.45f, 0f, 1.25f, 0f);
            N("arka", 0.55f, 0f, 1.3f, 180f);
        }

        // ---------- KORIDOR GAMMA ----------
        void KurKoridor()
        {
            Dis = false;
            Color metal = K(0.12f, 0.15f, 0.19f), buz = K(0.3f, 0.42f, 0.52f), kaburga = K(0.25f, 0.29f, 0.34f);
            Zarf(5.2f, 3.2f, 9.14f, -8f, buz, metal, K(0.05f, 0.06f, 0.08f), 0.85f);
            for (int i = 0; i < 5; i++)
            {
                float z = 1.5f + i * 1.6f;
                B("KaburgaSol" + i, -1.55f, 1.4f, z, 0.2f, 2.8f, 0.24f, kaburga, 0.5f);
                B("KaburgaSag" + i, 1.55f, 1.4f, z, 0.2f, 2.8f, 0.24f, kaburga, 0.5f);
                B("KaburgaUst" + i, 0f, 2.82f, z, 3.3f, 0.18f, 0.24f, kaburga, 0.5f);
                B("DuvarLambaSol" + i, -1.43f, 1.9f, z, 0.03f, 0.4f, 0.08f, K(0.6f, 0.85f, 1f), 0.3f, i % 2 == 0 ? 1.2f : 0.25f);
                B("DuvarLambaSag" + i, 1.43f, 1.9f, z, 0.03f, 0.4f, 0.08f, K(0.6f, 0.85f, 1f), 0.3f, i % 2 == 1 ? 1.2f : 0.25f);
                if (i % 2 == 0) SabitIsik("KoridorIsigi" + i, 0f, 2.4f, z - 0.4f, K(0.5f, 0.7f, 1f), 0.9f, 4f);
            }
            B("BoruSol", -1.2f, 2.65f, 3f, 0.1f, 0.1f, 14f, kaburga, 0.6f);
            B("BoruSag", 1.25f, 2.55f, 3f, 0.14f, 0.14f, 14f, kaburga, 0.6f);
            var rnd = new System.Random(9);
            for (int i = 0; i < 16; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * (0.9f + (float)rnd.NextDouble() * 0.5f);
                var p = S("BuzOdak" + i, x, 0.02f, 0.4f + i * 0.5f, 0.2f + (float)rnd.NextDouble() * 0.25f, K(0.7f, 0.85f, 0.95f), 0.9f);
                var s = p.transform.localScale;
                p.transform.localScale = new Vector3(s.x, s.y * 0.35f, s.z);
            }

            // Sektor Sifir ic kapisi (koridor sonu): surgulu
            B("IcKapiKasa", 0f, 1.2f, 9.04f, 2.0f, 2.5f, 0.1f, K(0.35f, 0.38f, 0.42f), 0.5f);
            B("IcKapiBosluk", 0f, 1.15f, 8.98f, 1.6f, 2.2f, 0.04f, K(0.01f, 0.01f, 0.03f), 0.2f);
            surgu = Grup("IcKapiKanadi", 0f, 0f, 8.93f); surguBazX = 0f; surguYol = 1.55f;
            B("IcKapiYuzu", 0f, 1.15f, 0f, 1.58f, 2.18f, 0.06f, K(0.22f, 0.24f, 0.28f), 0.55f, 0f, surgu);
            for (int i = 0; i < 4; i++) B("IcKapiSerit" + i, -0.6f + i * 0.4f, 0.35f, -0.04f, 0.18f, 0.25f, 0.01f, K(0.85f, 0.65f, 0.1f), 0.3f, 0.2f, surgu);
            var teker = C("IcKapiTeker", 0f, 1.3f, -0.07f, 0.3f, 0.07f, K(0.5f, 0.12f, 0.1f), 0.4f, 0f, surgu);
            teker.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Isikli("IcKapiPaneli", 1.15f, 1.35f, 8.96f, 0.16f, 0.22f, 0.03f, K(1f, 0.12f, 0.1f), 1.4f, out panelMat);

            // acil durum tableti (zeminde, buzun altinda kirmizi isik)
            var tab = Grup("Tablet", -0.42f, 0.03f, 0.55f);
            tab.localRotation = Quaternion.Euler(0f, 24f, 0f);
            B("TabletKasa", 0f, 0f, 0f, 0.34f, 0.035f, 0.24f, K(0.16f, 0.17f, 0.18f), 0.5f, 0f, tab);
            B("TabletEkran", 0f, 0.021f, 0f, 0.28f, 0.006f, 0.18f, K(0.9f, 0.5f, 0.2f), 0.7f, 0.9f, tab);
            S("TabletLed", 0.14f, 0.035f, 0.09f, 0.035f, K(1f, 0.1f, 0.08f), 0.5f, 2f, tab);
            SabitIsik("TabletIsigi", -0.42f, 0.25f, 0.55f, K(1f, 0.2f, 0.12f), 0.7f, 1.4f);

            // iki ciplak ayak izi ('istasyon: izler' ile gorunup buharlasir)
            for (int i = 0; i < 2; i++)
            {
                var iz = C("AyakIzi" + i, 0.35f + i * 0.18f, 0.008f, 0.75f + i * 0.22f, 0.5f, 0.008f, K(0.04f, 0.05f, 0.09f), 0.9f);
                iz.transform.localScale = new Vector3(0.11f, 0.004f, 0.25f);
                iz.SetActive(false);
                ayakIzleri.Add(iz.transform);
            }

            HalkaYap(0.78f, 2.15f, 2.6f, 0.2f);
            SabitIsik("OnDolgu", -0.4f, 2.2f, -1.2f, K(0.45f, 0.6f, 0.9f), 0.8f, 6f);
            N("kapi", 0f, 0f, 7.9f, 0f);
            N("tablet", -0.1f, 0f, 0.85f, 200f);
            N("arka", 0.5f, 0f, 2.0f, 185f);
        }

        // ---------- SEKTOR SIFIR: GOZLEM HAVA KILIDI ----------
        void KurHavaKilidi()
        {
            Dis = false;
            Color metal = K(0.17f, 0.21f, 0.27f), buz = K(0.28f, 0.38f, 0.48f), kasa = K(0.34f, 0.37f, 0.43f);
            Zarf(5.2f, 3.2f, 3.14f, -8f, buz, metal, K(0.05f, 0.06f, 0.09f), 0.85f);

            // zirhli gozlem cami ve isigi buken geometrik kirilma
            B("CamKasa", -0.6f, 1.6f, 3.05f, 1.62f, 1.46f, 0.05f, kasa, 0.5f);
            B("Cam", -0.6f, 1.6f, 3.01f, 1.44f, 1.28f, 0.02f, K(0.015f, 0.01f, 0.04f), 0.95f);
            for (int i = 0; i < 4; i++)
            {
                var g = Grup("Kirilma" + i, -0.6f, 1.62f, 2.985f - i * 0.003f);
                float r = 0.13f + i * 0.11f;
                int kenar = 3 + (i % 2);
                for (int k = 0; k < kenar; k++)
                {
                    float a = k * Mathf.PI * 2f / kenar;
                    float boy = 2f * r * Mathf.Tan(Mathf.PI / kenar);
                    float ic = r;
                    var c = B("KirikCizgi" + i + "_" + k, Mathf.Sin(a) * ic, Mathf.Cos(a) * ic, 0f, Mathf.Min(boy, 0.6f), 0.008f, 0.004f,
                              i % 2 == 0 ? K(0.7f, 0.45f, 1f) : K(0.45f, 0.65f, 1f), 0.3f, 1.3f - i * 0.2f, g);
                    c.transform.localRotation = Quaternion.Euler(0f, 0f, -a * Mathf.Rad2Deg);
                }
                g.localRotation = Quaternion.Euler(0f, 0f, i * 23f);
                kiriklar.Add(g);
            }
            for (int i = 0; i < 6; i++) B("CamPerc" + i, -1.35f + i * 0.3f, 0.93f, 3.0f, 0.04f, 0.04f, 0.02f, K(0.6f, 0.62f, 0.66f), 0.6f);
            // camda buyuyen sarmal kiragi ('istasyon: kirag')
            for (int i = 0; i < 70; i++)
            {
                float a = i * 0.42f, r = 0.012f + i * 0.0048f;
                var p = B("Kirag" + i, -0.38f + Mathf.Cos(a) * r, 1.45f + Mathf.Sin(a) * r, 2.96f, 0.05f, 0.012f, 0.004f, K(0.85f, 0.95f, 1f), 0.8f, 0.55f);
                p.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f);
                p.SetActive(false);
                kiragParca.Add(p);
            }

            // 17 tam centik + yarim 18. centik
            for (int i = 0; i < 18; i++)
            {
                float x = -1.26f + i * 0.07f + (i / 5) * 0.03f;
                bool yarim = i == 17;
                var c = B("Centik" + i, x, yarim ? 0.5f : 0.57f, 3.06f, 0.016f, yarim ? 0.12f : 0.26f, 0.01f, K(0.82f, 0.88f, 0.95f), 0.3f, 0.35f);
                c.transform.localRotation = Quaternion.Euler(0f, 0f, (i % 3 - 1) * 4f);
            }

            // dis basinc kapisi (sagda, surgulu) ve arkasindaki mor-beyaz isik
            B("DisKapiKasa", 0.82f, 1.12f, 3.05f, 0.98f, 2.3f, 0.05f, kasa, 0.5f);
            B("DisKapiIsik", 0.82f, 1.08f, 3.015f, 0.82f, 2.12f, 0.02f, K(0.85f, 0.75f, 1f), 0.3f, 1.6f);
            surgu = Grup("DisKapiKanadi", 0.82f, 0f, 2.96f); surguBazX = 0.82f; surguYol = 0.8f;
            B("DisKapiYuzu", 0f, 1.08f, 0f, 0.84f, 2.14f, 0.05f, K(0.24f, 0.26f, 0.31f), 0.6f, 0f, surgu);
            for (int i = 0; i < 3; i++) B("DisKapiKusak" + i, 0f, 0.4f + i * 0.7f, -0.035f, 0.8f, 0.07f, 0.02f, K(0.4f, 0.43f, 0.5f), 0.6f, 0f, surgu);
            B("DisKapiUyari", 0f, 0.14f, -0.035f, 0.8f, 0.12f, 0.01f, K(0.9f, 0.65f, 0.1f), 0.3f, 0.25f, surgu);
            Isikli("BasincPaneli", 0.24f, 1.3f, 2.99f, 0.13f, 0.2f, 0.03f, K(1f, 0.12f, 0.1f), 1.4f, out panelMat);

            // buzun icindeki kirik saat
            var saat = C("KirikSaat", 0.28f, 0.03f, 0.7f, 0.075f, 0.025f, K(0.55f, 0.57f, 0.6f), 0.7f);
            C("KirikSaatCam", 0.28f, 0.046f, 0.7f, 0.058f, 0.006f, K(0.15f, 0.9f, 0.6f), 0.9f, 0.5f);
            B("SaatKordon", 0.28f, 0.015f, 0.7f, 0.05f, 0.015f, 0.34f, K(0.35f, 0.36f, 0.4f), 0.6f);

            HalkaYap(-0.6f, 2.65f, 3.04f, 0.2f);
            SabitIsik("KilitIsigi", -0.5f, 2.5f, 0.4f, K(0.45f, 0.6f, 1f), 1.1f, 5f);
            SabitIsik("CamIsigi", -0.6f, 1.6f, 2.4f, K(0.6f, 0.4f, 1f), 0.9f, 2.5f);
            SabitIsik("OnDolgu", 0.4f, 1.8f, -1.4f, K(0.5f, 0.65f, 0.9f), 0.7f, 6f);
            N("kapi", 0.75f, 0f, 2.35f, 180f);
            N("esik", 0.72f, 0f, 2.3f, 195f);
            N("cam", -0.45f, 0f, 2.0f, 20f);
            N("arka", 0.5f, 0f, 1.6f, 180f);
        }
    }
}
