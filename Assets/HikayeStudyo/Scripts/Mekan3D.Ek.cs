using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 12: ev (salon), mutfak, yatak odasi, park ve araba ici.
    public partial class Mekan3D
    {
        public struct Pencere { public float x, y, z, w, h; }
        readonly List<Pencere> pencereler = new List<Pencere>();
        public List<Pencere> Pencereler { get { return pencereler; } }

        // Araba icinde karakterler hep oturur
        public bool Oturmali { get { return Tur == "araba"; } }

        MekanTarifi tarif;
        readonly List<Transform> akan = new List<Transform>();     // araba: geriye akan yol cizgileri ve agaclar
        float akisHizi = 6f, akisMin = -3f, akisMax = 17f;

        Color Duvar(Color varsayilan) { return Ozel(tarif != null ? tarif.Duvar : "", varsayilan); }
        Color Zemin(Color varsayilan) { return Ozel(tarif != null ? tarif.Zemin : "", varsayilan); }
        Color Vurgu(Color varsayilan) { return Ozel(tarif != null ? tarif.Vurgu : "", varsayilan); }

        static Color Ozel(string ad, Color varsayilan)
        {
            Color c;
            return !string.IsNullOrEmpty(ad) && MekanTarifi.RenkBul(ad, out c) ? c : varsayilan;
        }

        void AkisGuncelle(float dt)
        {
            for (int i = 0; i < akan.Count; i++)
            {
                var t = akan[i];
                if (t == null) continue;
                var p = t.localPosition;
                p.z += akisHizi * dt;                    // araba kameraya dogru gider: disarisi uzaklasir
                if (p.z > akisMax) p.z -= akisMax - akisMin;
                t.localPosition = p;
            }
        }

        // ---------- Ortak esyalar ----------
        void Masa(float x, float z, Color ust, Color bacak, bool tabakli)
        {
            B("MasaUstu", x, 0.75f, z, 1.1f, 0.06f, 0.7f, ust, 0.2f);
            for (int i = 0; i < 4; i++)
                B("MasaBacak" + i, x + (i % 2 == 0 ? -0.5f : 0.5f), 0.36f, z + (i < 2 ? -0.3f : 0.3f), 0.06f, 0.72f, 0.06f, bacak);
            if (!tabakli) return;
            C("Tabak", x - 0.25f, 0.8f, z, 0.14f, 0.03f, K(0.95f, 0.93f, 0.88f), 0.5f);
            C("Tabak2", x + 0.25f, 0.8f, z, 0.14f, 0.03f, K(0.95f, 0.93f, 0.88f), 0.5f);
            S("Caydanlik", x, 0.88f, z, 0.2f, K(0.7f, 0.72f, 0.75f), 0.6f);
        }

        void Sandalye(float x, float z, float sirtZ, Color c)
        {
            B("Oturak", x, 0.45f, z, 0.4f, 0.05f, 0.4f, c);
            B("Sirt", x, 0.75f, z + sirtZ, 0.4f, 0.55f, 0.05f, c);
            for (int i = 0; i < 4; i++)
                B("Bacak" + i, x + (i % 2 == 0 ? -0.17f : 0.17f), 0.22f, z + (i < 2 ? -0.17f : 0.17f), 0.04f, 0.44f, 0.04f, c);
        }

        void AbajurluLamba(float x, float tabanY, float z, float boy, bool mum)
        {
            C("LambaAyak", x, tabanY + boy * 0.5f, z, 0.035f, boy, K(0.25f, 0.2f, 0.14f), 0.3f);
            var abajur = S("LambaAbajur", x, tabanY + boy + 0.08f, z, 0.28f, K(1f, 0.82f, 0.55f));
            Parlak(abajur, K(1f, 0.78f, 0.45f));
            Lamba("OdaLambasi", x, tabanY + boy + 0.1f, z - 0.25f, K(1f, 0.74f, 0.42f), 1.2f, 4.5f, mum);
        }

        void Tablo(float x, float y, float w, float h, Color ic, Color cerceve)
        {
            B("Tablo", x, y, 3.1f, w, h, 0.03f, ic, 0.3f);
            B("TabloCerceve", x, y, 3.115f, w + 0.08f, h + 0.08f, 0.02f, cerceve);
        }

        // ---------- EV (salon) ----------
        void KurEv()
        {
            Dis = false;
            Color koyuAhsap = K(0.3f, 0.19f, 0.12f);
            Color vurgu = Vurgu(K(0.2f, 0.36f, 0.42f));
            Zarf(5.2f, 3f, 3.14f, -8f, K(0.5f, 0.34f, 0.22f), K(0.84f, 0.78f, 0.68f), K(0.9f, 0.87f, 0.82f), 0.2f);
            PencereYap(0.8f, 1.85f, 3.14f, 1.15f, 1.0f, K(0.92f, 0.9f, 0.85f), true, Vurgu(K(0.72f, 0.5f, 0.28f)));
            KapiYap(-1.45f, 3.14f, 0.95f, 2.1f, koyuAhsap, K(0.25f, 0.16f, 0.1f));
            N("kapi", -1.45f, 0f, 2.7f, 180f);
            N("pencere", 1.9f, 0f, 2.7f, 0f);

            // koltuk (pencerenin onunde)
            B("KoltukTaban", 0.8f, 0.22f, 2.65f, 1.7f, 0.34f, 0.7f, vurgu, 0.05f);
            B("KoltukMinder", 0.8f, 0.43f, 2.6f, 1.45f, 0.12f, 0.6f, Color.Lerp(vurgu, Color.white, 0.12f), 0.05f);
            B("KoltukSirt", 0.8f, 0.72f, 2.98f, 1.7f, 0.55f, 0.18f, vurgu, 0.05f);
            B("KoltukKolSol", -0.1f, 0.52f, 2.65f, 0.18f, 0.32f, 0.72f, vurgu, 0.05f);
            B("KoltukKolSag", 1.7f, 0.52f, 2.65f, 0.18f, 0.32f, 0.72f, vurgu, 0.05f);
            B("Kirlent", 0.3f, 0.64f, 2.82f, 0.34f, 0.3f, 0.1f, K(0.86f, 0.66f, 0.3f), 0.05f);
            B("Kirlent2", 1.32f, 0.64f, 2.82f, 0.34f, 0.3f, 0.1f, K(0.74f, 0.3f, 0.26f), 0.05f);
            N("koltuk", 0.8f, 0.49f, 2.45f, 180f);
            N("yatak", 0.8f, 0.55f, 2.45f, 0f);

            // sehpa + hali
            C("Hali", 0.5f, 0.008f, 1.5f, 1.25f, 0.016f, K(0.52f, 0.2f, 0.18f), 0.05f);
            C("HaliIc", 0.5f, 0.012f, 1.5f, 0.9f, 0.016f, K(0.8f, 0.66f, 0.42f), 0.05f);
            B("Sehpa", 1.45f, 0.4f, 1.65f, 0.75f, 0.05f, 0.45f, koyuAhsap, 0.35f);
            for (int i = 0; i < 4; i++)
                B("SehpaBacak" + i, 1.45f + (i % 2 == 0 ? -0.32f : 0.32f), 0.19f, 1.65f + (i < 2 ? -0.18f : 0.18f), 0.05f, 0.38f, 0.05f, koyuAhsap);
            C("Bardak", 1.35f, 0.48f, 1.65f, 0.045f, 0.11f, K(0.8f, 0.45f, 0.2f), 0.7f);
            N("sofra", 0.9f, 0f, 1.5f, 180f);

            // televizyon ve alt dolabi
            B("TvDolabi", -0.42f, 0.27f, 2.9f, 0.85f, 0.54f, 0.4f, koyuAhsap, 0.25f);
            B("TvGovde", -0.42f, 0.9f, 2.98f, 0.82f, 0.52f, 0.06f, K(0.05f, 0.05f, 0.06f), 0.6f);
            B("TvEkran", -0.42f, 0.9f, 2.94f, 0.74f, 0.44f, 0.01f, K(0.18f, 0.3f, 0.46f), 0.5f, 0.7f);
            N("televizyon", -0.42f, 0f, 2.45f, 0f);

            // kitaplik (sol duvar dibinde)
            B("Kitaplik", -2.35f, 0.95f, 2.2f, 0.35f, 1.9f, 1.0f, koyuAhsap);
            var rnd = new System.Random(8);
            Color[] kitap = { K(0.7f, 0.2f, 0.18f), K(0.2f, 0.35f, 0.55f), K(0.25f, 0.45f, 0.28f), K(0.8f, 0.66f, 0.3f), K(0.85f, 0.84f, 0.8f) };
            for (int r = 0; r < 3; r++)
            {
                float y = 0.5f + r * 0.5f;
                for (int i = 0; i < 9; i++)
                {
                    float h = 0.22f + (float)rnd.NextDouble() * 0.12f;
                    B("Kitap", -2.16f, y + h / 2f, 1.82f + i * 0.095f, 0.04f, h, 0.07f, kitap[rnd.Next(kitap.Length)], 0.1f);
                }
            }

            // ayakli lamba, tablo, saksi
            AbajurluLamba(2.15f, 0f, 2.55f, 1.35f, false);
            Tablo(-0.42f, 1.8f, 0.6f, 0.42f, K(0.32f, 0.5f, 0.45f), koyuAhsap);
            C("Saksi", 2.2f, 0.18f, 1.5f, 0.16f, 0.36f, K(0.66f, 0.34f, 0.22f), 0.1f);
            S("Yaprak", 2.2f, 0.62f, 1.5f, 0.55f, K(0.2f, 0.44f, 0.2f), 0.1f);
            xMin = -1.9f; xMax = -0.2f; zMin = 1.9f; zMax = 2.5f;
        }

        // ---------- MUTFAK ----------
        void KurMutfak()
        {
            Dis = false;
            Color koyuAhsap = K(0.3f, 0.19f, 0.12f);
            Color dolap = Vurgu(K(0.34f, 0.5f, 0.46f));
            Color tezgah = K(0.82f, 0.8f, 0.76f);
            Color metal = K(0.72f, 0.74f, 0.77f);
            Zarf(5.2f, 3f, 3.14f, -8f, K(0.68f, 0.62f, 0.54f), K(0.9f, 0.85f, 0.7f), K(0.92f, 0.9f, 0.86f), 0.35f);
            B("Fayans", 0.8f, 1.2f, 3.105f, 2.6f, 0.62f, 0.02f, K(0.9f, 0.93f, 0.94f), 0.6f);
            for (int i = 0; i < 13; i++) B("DerzD" + i, -0.4f + i * 0.2f, 1.2f, 3.092f, 0.008f, 0.62f, 0.005f, K(0.6f, 0.64f, 0.68f));
            B("DerzY", 0.8f, 1.2f, 3.092f, 2.6f, 0.008f, 0.005f, K(0.6f, 0.64f, 0.68f));
            KapiYap(-1.5f, 3.14f, 0.95f, 2.1f, koyuAhsap, K(0.25f, 0.16f, 0.1f));
            N("kapi", -1.5f, 0f, 2.7f, 180f);

            // alt dolaplar + tezgah
            B("AltDolap", 0.75f, 0.43f, 2.85f, 2.3f, 0.86f, 0.55f, dolap, 0.25f);
            B("Tezgah", 0.75f, 0.89f, 2.83f, 2.36f, 0.06f, 0.62f, tezgah, 0.55f);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.17f + i * 0.46f;
                B("Kapak" + i, x, 0.45f, 2.565f, 0.42f, 0.72f, 0.015f, Color.Lerp(dolap, Color.white, 0.1f), 0.3f);
                B("Kulp" + i, x + 0.13f, 0.7f, 2.545f, 0.1f, 0.02f, 0.02f, metal, 0.7f);
            }
            // ust dolaplar (pencerenin iki yaninda)
            B("UstDolapSol", 0.0f, 2.15f, 2.98f, 0.9f, 0.62f, 0.3f, dolap, 0.25f);
            B("UstDolapSag", 1.82f, 2.15f, 2.98f, 0.34f, 0.62f, 0.3f, dolap, 0.25f);
            PencereYap(1.1f, 1.8f, 3.14f, 0.75f, 0.7f, K(0.93f, 0.92f, 0.88f), false, Color.white);

            // ocak + tencere
            B("Ocak", 0.15f, 0.93f, 2.8f, 0.55f, 0.025f, 0.45f, K(0.07f, 0.07f, 0.08f), 0.7f);
            C("Tencere", 0.03f, 1.01f, 2.78f, 0.12f, 0.14f, metal, 0.7f);
            C("TencereKapak", 0.03f, 1.09f, 2.78f, 0.125f, 0.02f, metal, 0.7f);
            C("Caydanlik", 0.33f, 1.0f, 2.84f, 0.09f, 0.12f, K(0.75f, 0.2f, 0.16f), 0.6f);
            // lavabo + musluk
            B("Lavabo", 1.1f, 0.925f, 2.8f, 0.55f, 0.02f, 0.4f, metal, 0.8f);
            C("Musluk", 1.1f, 1.05f, 2.98f, 0.018f, 0.24f, metal, 0.8f);
            B("MuslukAgiz", 1.1f, 1.17f, 2.9f, 0.03f, 0.03f, 0.18f, metal, 0.8f);
            C("Kase", 1.65f, 0.97f, 2.75f, 0.11f, 0.09f, K(0.86f, 0.7f, 0.4f), 0.4f);

            // buzdolabi
            B("Buzdolabi", 2.27f, 0.95f, 2.78f, 0.62f, 1.9f, 0.66f, K(0.9f, 0.92f, 0.93f), 0.6f);
            B("BuzAyrim", 2.27f, 1.3f, 2.44f, 0.62f, 0.015f, 0.01f, K(0.55f, 0.57f, 0.6f));
            B("BuzKulp", 2.04f, 1.55f, 2.43f, 0.03f, 0.3f, 0.03f, metal, 0.7f);
            B("BuzKulp2", 2.04f, 1.0f, 2.43f, 0.03f, 0.3f, 0.03f, metal, 0.7f);

            // tavan lambasi
            var sapka = S("SarkitLamba", -1.0f, 2.45f, 1.5f, 0.3f, K(1f, 0.86f, 0.6f));
            Parlak(sapka, K(1f, 0.84f, 0.55f));
            C("SarkitKablo", -1.0f, 2.75f, 1.5f, 0.008f, 0.4f, K(0.1f, 0.1f, 0.1f));
            Lamba("MutfakIsik", -0.4f, 2.4f, 1.6f, K(1f, 0.86f, 0.62f), 1.2f, 6f, false);

            // yemek masasi
            Masa(-1.45f, 1.4f, K(0.6f, 0.42f, 0.26f), koyuAhsap, true);
            Sandalye(-1.45f, 0.75f, -0.18f, koyuAhsap);
            Sandalye(-1.45f, 2.05f, 0.18f, koyuAhsap);

            N("tezgah", 1.1f, 0f, 2.15f, 0f);
            N("ocak", 0.15f, 0f, 2.15f, 0f);
            N("pencere", 1.1f, 0f, 2.15f, 0f);
            N("buzdolabi", 2.05f, 0f, 2.0f, 0f);
            N("sofra", -0.8f, 0f, 1.4f, 270f);
            xMin = -0.6f; xMax = 1.4f; zMin = 1.9f; zMax = 2.2f;
        }

        // ---------- YATAK ODASI ----------
        void KurYatakOdasi()
        {
            Dis = false;
            Color koyuAhsap = K(0.3f, 0.19f, 0.12f);
            Color vurgu = Vurgu(K(0.46f, 0.3f, 0.55f));
            Zarf(5.2f, 3f, 3.14f, -8f, K(0.5f, 0.35f, 0.24f), K(0.72f, 0.7f, 0.82f), K(0.88f, 0.86f, 0.9f), 0.2f);
            PencereYap(0.9f, 1.8f, 3.14f, 0.9f, 1.05f, koyuAhsap, true, vurgu);
            KapiYap(-1.55f, 3.14f, 0.95f, 2.1f, koyuAhsap, K(0.25f, 0.16f, 0.1f));
            N("kapi", -1.55f, 0f, 2.7f, 180f);
            N("pencere", 1.0f, 0f, 1.45f, 0f);

            // yatak yan durur (baslik sagda): yatan kisi kameraya boydan gorunur
            B("YatakCerceve", 0.65f, 0.25f, 2.45f, 1.9f, 0.3f, 1.15f, koyuAhsap);
            B("YatakBaslik", 1.62f, 0.65f, 2.45f, 0.1f, 0.95f, 1.15f, koyuAhsap);
            B("Yatak", 0.65f, 0.47f, 2.45f, 1.82f, 0.18f, 1.07f, K(0.95f, 0.92f, 0.85f), 0.05f);
            B("Battaniye", 0.3f, 0.6f, 2.45f, 1.14f, 0.1f, 1.12f, vurgu, 0.05f);
            B("Yastik", 1.37f, 0.61f, 2.45f, 0.3f, 0.12f, 0.6f, Color.white, 0.05f);
            N("yatak", 0.65f, 0.58f, 2.45f, 90f);

            // komodin + lamba
            B("Komodin", 1.95f, 0.3f, 2.9f, 0.45f, 0.6f, 0.4f, koyuAhsap, 0.2f);
            AbajurluLamba(1.95f, 0.6f, 2.9f, 0.16f, true);

            // sifonyer + ayna
            B("Sifonyer", -0.72f, 0.42f, 2.9f, 0.8f, 0.84f, 0.42f, koyuAhsap, 0.25f);
            for (int i = 0; i < 3; i++)
            {
                B("Cekmece" + i, -0.72f, 0.18f + i * 0.25f, 2.685f, 0.72f, 0.21f, 0.012f, K(0.42f, 0.28f, 0.18f), 0.25f);
                B("CekmeceKulp" + i, -0.72f, 0.18f + i * 0.25f, 2.67f, 0.14f, 0.025f, 0.02f, K(0.85f, 0.7f, 0.3f), 0.6f);
            }
            B("AynaCerceve", -0.72f, 1.45f, 3.1f, 0.62f, 0.86f, 0.03f, K(0.85f, 0.7f, 0.3f), 0.5f);
            B("Ayna", -0.72f, 1.45f, 3.08f, 0.52f, 0.76f, 0.02f, K(0.62f, 0.74f, 0.82f), 0.95f);
            N("ayna", -0.72f, 0f, 2.3f, 0f);

            // gardirop (sag duvar)
            B("Gardirop", 2.37f, 1.1f, 1.55f, 0.44f, 2.2f, 1.2f, koyuAhsap, 0.2f);
            B("GardiropKapak1", 2.14f, 1.1f, 1.24f, 0.015f, 2.08f, 0.56f, K(0.42f, 0.28f, 0.18f), 0.25f);
            B("GardiropKapak2", 2.14f, 1.1f, 1.86f, 0.015f, 2.08f, 0.56f, K(0.42f, 0.28f, 0.18f), 0.25f);
            B("GardiropKulp1", 2.12f, 1.1f, 1.48f, 0.02f, 0.18f, 0.025f, K(0.85f, 0.7f, 0.3f), 0.6f);
            B("GardiropKulp2", 2.12f, 1.1f, 1.62f, 0.02f, 0.18f, 0.025f, K(0.85f, 0.7f, 0.3f), 0.6f);
            N("dolap", 1.85f, 0f, 1.4f, 90f);

            C("Hali", -0.3f, 0.008f, 1.0f, 0.95f, 0.016f, Color.Lerp(vurgu, Color.white, 0.35f), 0.05f);
            Tablo(-1.55f, 2.5f, 0.55f, 0.28f, K(0.8f, 0.6f, 0.45f), koyuAhsap);
            Lamba("YatakOdasiIsik", -0.6f, 2.5f, 1.2f, K(1f, 0.86f, 0.7f), 0.7f, 5f, false);
            xMin = -1.9f; xMax = -0.3f; zMin = 1.6f; zMax = 2.4f;
        }

        // ---------- PARK ----------
        void Agac(float x, float z, float govde, float tac, Color yaprak, Transform ebeveyn = null)
        {
            C("AgacGovde", x, govde / 2f, z, 0.11f, govde, K(0.38f, 0.26f, 0.16f), 0.05f, 0f, ebeveyn);
            S("AgacTaji", x, govde + tac * 0.3f, z, tac, yaprak, 0.05f, 0f, ebeveyn);
            S("AgacTaji2", x + tac * 0.2f, govde + tac * 0.1f, z, tac * 0.7f, Color.Lerp(yaprak, Color.white, 0.07f), 0.05f, 0f, ebeveyn);
        }

        void Bank(float x, float z, Color c)
        {
            B("BankOturak", x, 0.45f, z, 1.25f, 0.06f, 0.42f, c);
            B("BankSirt", x, 0.78f, z + 0.2f, 1.25f, 0.34f, 0.05f, c);
            B("BankAyakSol", x - 0.52f, 0.22f, z, 0.06f, 0.44f, 0.38f, K(0.18f, 0.18f, 0.2f), 0.4f);
            B("BankAyakSag", x + 0.52f, 0.22f, z, 0.06f, 0.44f, 0.38f, K(0.18f, 0.18f, 0.2f), 0.4f);
        }

        void KurPark()
        {
            Dis = true;
            Color cimen = Zemin(K(0.3f, 0.5f, 0.22f));
            Color vurgu = Vurgu(K(0.5f, 0.32f, 0.18f));
            B("Cimen", 0f, -0.1f, 8f, 40f, 0.2f, 40f, cimen, 0.05f);
            B("Yol", 0f, 0.003f, 1.0f, 7f, 0.01f, 1.8f, K(0.7f, 0.62f, 0.5f), 0.05f);
            B("YolIleri", 0.3f, 0.004f, 6f, 1.1f, 0.01f, 9f, K(0.7f, 0.62f, 0.5f), 0.05f);

            Bank(1.05f, 2.35f, vurgu);
            Bank(-2.3f, 3.4f, vurgu);
            N("bank", 1.05f, 0.49f, 2.15f, 180f);
            N("koltuk", 1.05f, 0.49f, 2.2f, 180f);
            N("yatak", 1.05f, 0.55f, 2.2f, 0f);

            Color yesil = K(0.2f, 0.42f, 0.18f);
            float[] tx = { -2.1f, 2.7f, -3.4f, 1.9f, -1.2f, 3.9f, -4.6f, 0.9f, 4.9f, -2.6f };
            float[] tz = { 2.9f, 3.6f, 6.2f, 7.5f, 9.5f, 6.0f, 9.0f, 12f, 11f, 13f };
            for (int i = 0; i < tx.Length; i++)
                Agac(tx[i], tz[i], 1.3f + (i % 3) * 0.25f, 1.6f + (i % 4) * 0.2f, i % 3 == 0 ? K(0.34f, 0.5f, 0.18f) : yesil);
            N("agac", -1.8f, 0f, 2.4f, 180f);

            // havuz + fiskiye
            C("HavuzKenar", -1.35f, 0.12f, 3.9f, 0.85f, 0.24f, K(0.62f, 0.62f, 0.64f), 0.2f);
            C("HavuzSu", -1.35f, 0.245f, 3.9f, 0.74f, 0.02f, K(0.18f, 0.45f, 0.66f), 0.9f, 0.15f);
            C("Fiskiye", -1.35f, 0.5f, 3.9f, 0.07f, 0.5f, K(0.62f, 0.62f, 0.64f), 0.3f);
            S("Su", -1.35f, 0.82f, 3.9f, 0.18f, K(0.7f, 0.86f, 1f), 0.9f, 0.4f);
            N("havuz", -1.1f, 0f, 2.6f, 0f);

            // salincak
            B("SalincakUst", 2.0f, 2.0f, 4.8f, 1.4f, 0.08f, 0.08f, K(0.75f, 0.2f, 0.18f), 0.3f);
            B("SalincakDirek1", 1.32f, 1.0f, 4.8f, 0.08f, 2.0f, 0.08f, K(0.75f, 0.2f, 0.18f), 0.3f);
            B("SalincakDirek2", 2.68f, 1.0f, 4.8f, 0.08f, 2.0f, 0.08f, K(0.75f, 0.2f, 0.18f), 0.3f);
            B("Ip1", 1.8f, 1.3f, 4.8f, 0.02f, 1.35f, 0.02f, K(0.2f, 0.2f, 0.22f));
            B("Ip2", 2.2f, 1.3f, 4.8f, 0.02f, 1.35f, 0.02f, K(0.2f, 0.2f, 0.22f));
            B("SalincakOturak", 2.0f, 0.62f, 4.8f, 0.46f, 0.05f, 0.22f, vurgu);

            // lambalar ve cicekler
            float[] lx = { -1.65f, 1.95f };
            for (int i = 0; i < lx.Length; i++)
            {
                C("ParkDirek" + i, lx[i], 1.4f, 2.75f, 0.04f, 2.8f, K(0.15f, 0.2f, 0.18f), 0.4f);
                var ampul = S("ParkAmpul" + i, lx[i], 2.9f, 2.75f, 0.24f, K(1f, 0.9f, 0.62f));
                Parlak(ampul, K(1f, 0.88f, 0.58f));
                Lamba("ParkIsigi" + i, lx[i], 2.7f, 2.3f, K(1f, 0.84f, 0.55f), 1.1f, 5f, false);
            }
            var rnd = new System.Random(31);
            Color[] cicek = { K(0.9f, 0.3f, 0.35f), K(0.95f, 0.85f, 0.3f), K(0.95f, 0.95f, 0.95f), K(0.65f, 0.4f, 0.8f) };
            for (int i = 0; i < 70; i++)
            {
                float x = -4f + (float)rnd.NextDouble() * 8f, z = 2.2f + (float)rnd.NextDouble() * 3.5f;
                if (Mathf.Abs(x - 0.3f) < 0.7f) continue;
                S("Cicek", x, 0.05f, z, 0.06f, cicek[i % cicek.Length], 0.1f);
            }
            S("Tepe1", -7f, -4f, 22f, 16f, K(0.26f, 0.42f, 0.22f), 0.02f);
            S("Tepe2", 8f, -5f, 26f, 20f, K(0.28f, 0.44f, 0.24f), 0.02f);
            xMin = -1.9f; xMax = 1.9f; zMin = 2.6f; zMax = 3.4f;
        }

        // ---------- ARABA ICI ----------
        // Kamera on camdan iceri bakar: surucu solda, yolcu sagda; arka camdan yol akar.
        void KurAraba()
        {
            Dis = true;
            Color doseme = Vurgu(K(0.24f, 0.22f, 0.24f));
            Color govde = Duvar(K(0.5f, 0.1f, 0.12f));
            Color ic = K(0.12f, 0.12f, 0.13f);

            // yol ve disarisi
            B("Cimen", 0f, -0.55f, 7f, 40f, 0.2f, 30f, K(0.28f, 0.42f, 0.2f), 0.05f);
            B("Asfalt", 0f, -0.44f, 7f, 4.4f, 0.05f, 30f, Zemin(K(0.14f, 0.14f, 0.16f)), 0.3f);
            for (int i = 0; i < 10; i++)
            {
                var c1 = B("YolCizgi", -1.6f, -0.41f, -3f + i * 2f, 0.1f, 0.01f, 0.9f, K(0.95f, 0.9f, 0.5f), 0.2f);
                var c2 = B("YolCizgi", 1.6f, -0.41f, -3f + i * 2f, 0.1f, 0.01f, 0.9f, K(0.95f, 0.9f, 0.5f), 0.2f);
                akan.Add(c1.transform); akan.Add(c2.transform);
            }
            var rnd = new System.Random(17);
            for (int i = 0; i < 14; i++)
            {
                var kok = new GameObject("YolKenari" + i).transform;
                kok.SetParent(root, false);
                float x = (i % 2 == 0 ? -1f : 1f) * (3.1f + (float)rnd.NextDouble() * 2.2f);
                kok.localPosition = new Vector3(x, -0.45f, -3f + (i / 2) * (20f / 7f) + (float)rnd.NextDouble());
                if (i % 5 == 4)
                {
                    C("Direk", 0f, 1.6f, 0f, 0.05f, 3.2f, K(0.2f, 0.2f, 0.22f), 0.3f, 0f, kok);
                    S("DirekLambasi", 0f, 3.2f, 0f, 0.22f, K(1f, 0.88f, 0.55f), 0.2f, 0.9f, kok);
                }
                else Agac(0f, 0f, 1.1f + (float)rnd.NextDouble() * 0.5f, 1.4f + (float)rnd.NextDouble() * 0.6f, K(0.2f, 0.4f, 0.18f), kok);
                akan.Add(kok);
            }

            // govde: taban, kapilar, direkler, tavan
            B("Taban", 0f, -0.05f, 0.1f, 2.3f, 0.1f, 1.9f, ic, 0.1f);
            B("AltGovde", 0f, -0.2f, 0.1f, 2.34f, 0.2f, 1.92f, govde, 0.55f);
            for (int s = -1; s <= 1; s += 2)
            {
                float x = 1.13f * s;
                B("Kapi", x, 0.42f, 0.1f, 0.09f, 0.84f, 1.8f, govde, 0.55f);
                B("KapiIc", x - 0.055f * s, 0.42f, 0.1f, 0.02f, 0.76f, 1.72f, doseme, 0.1f);
                B("OnDirek", x, 1.3f, -0.72f, 0.06f, 0.94f, 0.06f, govde, 0.5f);
                B("OrtaDirek", x, 1.3f, 0.45f, 0.07f, 0.94f, 0.09f, govde, 0.5f);
                B("ArkaDirek", x, 1.3f, 0.99f, 0.09f, 0.94f, 0.09f, govde, 0.5f);
                var teker = C("Tekerlek", 1.12f * s, -0.22f, -0.55f, 0.24f, 0.2f, K(0.04f, 0.04f, 0.04f), 0.1f);
                teker.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                var teker2 = C("Tekerlek", 1.12f * s, -0.22f, 0.75f, 0.24f, 0.2f, K(0.04f, 0.04f, 0.04f), 0.1f);
                teker2.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            B("Tavan", 0f, 1.81f, 0.15f, 2.34f, 0.08f, 1.82f, govde, 0.55f);
            B("TavanIc", 0f, 1.765f, 0.15f, 2.24f, 0.015f, 1.74f, K(0.55f, 0.53f, 0.5f), 0.05f);
            B("ArkaCamAlt", 0f, 0.92f, 1.0f, 2.24f, 0.2f, 0.06f, govde, 0.5f);
            B("ArkaCamUst", 0f, 1.69f, 1.0f, 2.24f, 0.14f, 0.06f, govde, 0.5f);
            B("DikizAynasi", 0f, 1.66f, -0.66f, 0.24f, 0.08f, 0.04f, ic, 0.7f);

            // on konsol + direksiyon (surucu)
            // konsol yuksek tutulur: oturanlarin kucagi ve bacaklari kadraja girmesin
            B("Konsol", 0f, 0.8f, -0.66f, 2.18f, 0.24f, 0.24f, ic, 0.25f);
            B("KonsolAlt", 0f, 0.34f, -0.74f, 2.18f, 0.7f, 0.06f, govde, 0.5f);
            B("Gosterge", -0.52f, 0.93f, -0.64f, 0.3f, 0.07f, 0.12f, K(0.05f, 0.05f, 0.06f), 0.5f);
            B("Ekran", 0.02f, 0.82f, -0.79f, 0.22f, 0.06f, 0.02f, K(0.2f, 0.5f, 0.7f), 0.5f, 0.8f);
            for (int i = 0; i < 16; i++)
            {
                float a = i / 16f * Mathf.PI * 2f;
                var p = B("Direksiyon", -0.52f + Mathf.Cos(a) * 0.17f, 0.85f + Mathf.Sin(a) * 0.15f, -0.46f + Mathf.Sin(a) * 0.05f,
                          0.075f, 0.032f, 0.032f, K(0.06f, 0.06f, 0.07f), 0.4f);
                p.transform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg + 90f);
            }
            B("DireksiyonKol", -0.52f, 0.85f, -0.46f, 0.32f, 0.035f, 0.03f, K(0.06f, 0.06f, 0.07f), 0.4f);

            // koltuklar
            for (int s = -1; s <= 1; s += 2)
            {
                float x = 0.52f * s;
                B("KoltukMinder", x, 0.4f, 0.12f, 0.58f, 0.14f, 0.5f, doseme, 0.05f);
                B("KoltukSirt", x, 0.82f, 0.42f, 0.56f, 0.74f, 0.12f, doseme, 0.05f);
                B("KoltukBaslik", x, 1.31f, 0.44f, 0.26f, 0.18f, 0.1f, doseme, 0.05f);
            }
            // on koltuklarin arasi: arkadakilerin kucagini kapatir
            B("OrtaKonsol", 0f, 0.46f, 0.3f, 0.46f, 0.9f, 0.3f, doseme, 0.1f);
            B("ArkaKoltuk", 0f, 0.62f, 0.9f, 2.1f, 0.5f, 0.12f, doseme, 0.05f);

            // arka koltuk yukarida ve iceride: arkadakiler on basliklarin arasindan gorunur
            B("ArkaMinder", 0f, 0.56f, 0.74f, 1.2f, 0.14f, 0.34f, doseme, 0.05f);
            N("surucu", -0.52f, 0.49f, 0.12f, 168f);
            N("yolcu", 0.52f, 0.49f, 0.12f, 192f);
            N("arka_sol", -0.19f, 0.65f, 0.72f, 175f);
            N("arka_sag", 0.19f, 0.65f, 0.72f, 185f);
            N("kapi", -0.52f, 0.49f, 0.12f, 168f);
            Lamba("ArabaIci", 0f, 1.55f, -0.5f, K(0.85f, 0.9f, 1f), 0.6f, 2.6f, false);
            xMin = -0.5f; xMax = 0.5f; zMin = 0.7f; zMax = 0.75f;
        }
    }
}
