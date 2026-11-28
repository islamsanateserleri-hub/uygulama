using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // 3D kamera yonetmeni (Sahne3D'nin Kamera3D nesnesinde calisir). Duz Transform hareketi kullanir.
    // Kamera bir "yorunge" ile tarif edilir: pivot (bakilan nokta) + mesafe + yaw + pitch + FOV.
    //  - 3D mekan (mekan3d:) acikken: genis, orta, omuz ustu, yakin, tepki ve ikili planlar.
    //    Konusan karakter kendiliginden bulunur; guclu duyguda / uzun satirda dinleyenin tepkisine kesilir;
    //    sarilma / el tutma ikili planla, yurume ve kapi genis planla gosterilir. Karakterler hareket ettikce
    //    kamera onlari her karede yeniden kadrajlar (kadraj disina tasmazlar).
    //  - 180 derece kurali: kamera her zaman karakterlerin on tarafinda (-z) kalir; sol/sag yer degistirmez.
    //  - Elle mudahale: kamera: genis / orta / omuz / ters / yakin / iki (kalici), otomatik (geri don),
    //    tepki / yaklas / uzaklas / goz / yukaridan / asagidan / bulanik / net (tek seferlik).
    //  - 2D mekanda (karakterler 2D arka plan uzerinde): sadece cok hafif yorunge cesitliligi ve el titremesi.
    //  - El titremesi (Perlin) her zaman acik, panik aninda guclenir.
    public class KameraYonetmen : MonoBehaviour
    {
        public enum Plan { Genis = 0, Orta = 1, Omuz = 2, Yakin = 3, Tepki = 4, Iki = 5 }

        const float Oran = 1080f / 1920f;     // dikey video en/boy
        const float YakinFov = 28f;

        Sahne3D owner;
        Camera cam;
        float baseY = 1.1f;
        bool oda;
        string mod = "otomatik";
        float aciPitch;            // goz = 0, yukaridan = +, asagidan = -
        float yaw2d;

        Plan plan = Plan.Genis;
        string planS = "", planL = "";     // planin konusani (ozne) ve diger karakteri
        bool planIc;
        int p1 = -1, p2 = -1;
        int genistenBeri;
        int satirSayisi, sonTepkiSatiri = -10;
        string sonKonusan = "", sonDinleyen = "";
        string tepkiHedef = "";
        bool tepkiBekliyor;
        float yon = 1f, planYon = 1f;

        // satir ortasinda dinleyene kesme (otomatik tepki cekimi)
        bool gucluSatir;
        float tepkiSayac = -1f;
        string tepkiKisi = "";

        // hedef yorunge
        Vector3 pivot;
        float dist = Sahne3D.CamDist, yaw, pitch, fov = Sahne3D.CamFov;
        float pushRate, itme, minDist = 0.9f, maxDist = 9f;
        // gecerli yorunge (yumusatilmis)
        Vector3 cPivot;
        float cDist, cYaw, cPitch, cFov;
        bool snap = true;
        Plan sonEtkinPlan = Plan.Genis;

        float panik, seed;
        readonly List<Vector3> noktalar = new List<Vector3>();
        readonly List<Vector3> digerleri = new List<Vector3>();

        public Plan SonPlan { get { return plan; } }

        public void Kur(Sahne3D sahne, Camera kamera)
        {
            owner = sahne; cam = kamera;
            seed = Random.value * 100f;
            pivot = Sahne3D.Origin + new Vector3(0f, baseY, 0f);
            snap = true;
        }

        // Sahne3D.SetFloor: zemin cizgisine gore temel kamera yuksekligi
        public void TemelKamera(float camY)
        {
            baseY = camY;
        }

        public void OdaDegisti(bool aktif)
        {
            oda = aktif;
            Sifirla();
        }

        public void Sifirla()
        {
            p1 = -1; p2 = -1; genistenBeri = 0;
            satirSayisi = 0; sonTepkiSatiri = -10;
            sonKonusan = ""; sonDinleyen = "";
            tepkiBekliyor = false; tepkiHedef = "";
            tepkiSayac = -1f;
            pushRate = 0f;
            if (oda) PlanKur(Plan.Genis, "", "", false);
            snap = true;
        }

        public void Panik(float s) { panik = Mathf.Max(panik, Mathf.Clamp01(s)); }

        // ---------- Plan secimi (saf fonksiyon: Ritim Denetcisi de kullanir) ----------
        public static Plan PlanSec(int son1, int son2, int genistenBeri, bool inner, bool strong, bool dinleyenVar, string mod)
        {
            if (mod == "yakin") return Plan.Yakin;
            if (mod == "genis") return Plan.Genis;
            if (mod == "orta") return Plan.Orta;
            if (mod == "omuz" || mod == "ters") return dinleyenVar ? Plan.Omuz : Plan.Orta;
            if (mod == "iki") return dinleyenVar ? Plan.Iki : Plan.Orta;
            Plan c;
            if (genistenBeri >= 6) c = Plan.Genis;
            else if (inner) c = Plan.Yakin;
            else if (strong) c = son1 == (int)Plan.Yakin ? (dinleyenVar ? Plan.Omuz : Plan.Orta) : Plan.Yakin;
            else if (dinleyenVar) c = Plan.Omuz;
            else c = Plan.Orta;
            if (son1 == (int)c && son2 == (int)c) c = Alternatif(c, dinleyenVar);
            return c;
        }

        static Plan Alternatif(Plan c, bool dinleyenVar)
        {
            switch (c)
            {
                case Plan.Omuz: return Plan.Yakin;
                case Plan.Yakin: return dinleyenVar ? Plan.Omuz : Plan.Orta;
                case Plan.Orta: return Plan.Yakin;
                case Plan.Tepki: return Plan.Yakin;
                default: return Plan.Orta;
            }
        }

        static bool GucluDuygu(string e)
        {
            return e == "aglamakli" || e == "korkmus" || e == "sasirmis" || e == "sevgi" || e == "kizgin" || e == "uzgun";
        }

        // ---------- Konusma satiri ----------
        // true: 3D mekan kamerayi yonetti (StoryPlayer 2D yakinlastirmayi birakmali)
        public bool Satir(string speakerId, string emotion, bool inner)
        {
            if (owner == null) return false;
            string sid = Util.Norm(speakerId);
            string e = Util.Norm(emotion);
            if (e == "korkmus") Panik(0.8f);
            else if (e == "kizgin") Panik(0.45f);
            else if (e == "sasirmis") Panik(0.4f);
            else if (e == "aglamakli") Panik(0.3f);

            string lid = owner.DigerOyuncu(sid);
            sonKonusan = sid;
            yon = -yon;
            tepkiSayac = -1f;
            gucluSatir = GucluDuygu(e) && !inner;

            if (!oda)
            {
                yaw2d = mod == "otomatik" ? yon * 5f : 0f;
                return false;
            }

            satirSayisi++;
            Plan p = Plan.Genis;
            bool secildi = false;
            if (tepkiBekliyor)
            {
                tepkiBekliyor = false;
                string hid = tepkiHedef.Length > 0 ? tepkiHedef : lid;
                if (hid.Length > 0 && hid != sid && owner.HasActor(hid))
                {
                    lid = hid;
                    p = Plan.Tepki;
                    sonTepkiSatiri = satirSayisi;
                    secildi = true;
                }
            }
            if (!secildi)
            {
                // sarilma / el tutma suruyorsa ikisi birlikte gosterilir
                string es = mod == "otomatik" && !inner ? owner.EtkilesimEsi(sid) : "";
                if (es.Length > 0 && owner.HasActor(es)) { lid = es; p = Plan.Iki; }
                else p = PlanSec(p1, p2, genistenBeri, inner, GucluDuygu(e), lid.Length > 0, mod);
            }

            p2 = p1; p1 = (int)p;
            if (p == Plan.Genis) genistenBeri = 0; else genistenBeri++;
            sonDinleyen = lid;
            // ters cekim: dinleyenin yuzu, konusanin omzu ustunden
            if (mod == "ters" && p == Plan.Omuz) PlanKur(p, lid, sid, inner);
            else PlanKur(p, sid, lid, inner);
            if (Application.isEditor) Debug.Log("[Kamera] " + sid + " -> " + plan + (lid.Length > 0 ? " (diger: " + lid + ")" : ""));
            return true;
        }

        // StoryPlayer, satirin suresi belli olunca cagirir: guclu ya da uzun satirda dinleyene tepki kesmesi planlanir
        public void SatirSuresi(float sure)
        {
            tepkiSayac = -1f;
            if (!oda || mod != "otomatik" || planIc || plan == Plan.Tepki || plan == Plan.Iki) return;
            if (sonDinleyen.Length == 0 || sure < 3f) return;
            if (satirSayisi - sonTepkiSatiri < 3) return;
            if (!gucluSatir && sure < 4.5f) return;
            tepkiKisi = sonDinleyen;
            tepkiSayac = Mathf.Clamp(sure * 0.62f, 1.6f, sure - 1.1f);
        }

        // Hikaye bir hareket komutu calistiriyor: kamera harekete uygun plana gecer (elle mod seciliyse dokunmaz)
        public void Hareket(string komut, string a, string b)
        {
            tepkiSayac = -1f;
            if (!oda || mod != "otomatik") return;
            a = Util.Norm(a); b = Util.Norm(b);
            Plan p;
            string s = a, l = b;
            switch (komut)
            {
                case "sarilma":
                case "el_tut":
                case "yuzyuze": p = Plan.Iki; break;
                case "tepki_ver": p = Plan.Yakin; l = ""; break;
                case "tut": p = Plan.Orta; l = ""; break;
                case "birak": return;
                default: p = Plan.Genis; s = ""; l = ""; break;     // git, kapi, gir, cikar, yatak, kalk, ayril
            }
            if (p == plan && s == planS && l == planL) return;
            p2 = p1; p1 = (int)p;
            if (p == Plan.Genis) genistenBeri = 0;
            PlanKur(p, s, l, false);
        }

        // ---------- Plan geometrisi ----------
        static void Yorunge(Vector3 camPos, Vector3 hedef, out float d, out float y, out float p)
        {
            Vector3 v = camPos - hedef;
            d = Mathf.Max(0.3f, v.magnitude);
            y = Mathf.Atan2(-v.x, -v.z) * Mathf.Rad2Deg;
            p = Mathf.Asin(Mathf.Clamp(v.y / d, -1f, 1f)) * Mathf.Rad2Deg;
        }

        // Verilen kafalari dikey karede (TikTok guvenli alanlari birakarak) icine alan mesafe ve bakis noktasi
        static void Cerceve(List<Vector3> kafalar, float altPay, float gorusAcisi, out Vector3 merkez, out float d)
        {
            float minX = float.MaxValue, maxX = float.MinValue, ust = float.MinValue, alt = float.MaxValue, z = 0f, minZ = float.MaxValue;
            foreach (var k in kafalar)
            {
                minX = Mathf.Min(minX, k.x); maxX = Mathf.Max(maxX, k.x);
                ust = Mathf.Max(ust, k.y); alt = Mathf.Min(alt, k.y);
                z += k.z; minZ = Mathf.Min(minZ, k.z);
            }
            z /= kafalar.Count;
            minX -= 0.34f; maxX += 0.34f; ust += 0.32f; alt -= altPay;
            float t = Mathf.Tan(gorusAcisi * 0.5f * Mathf.Deg2Rad);
            float yariEn = (maxX - minX) * 0.5f, yariBoy = (ust - alt) * 0.5f;
            d = Mathf.Max(yariEn / (t * Oran * 0.88f), yariBoy / (t * 0.74f)) + Mathf.Max(0f, z - minZ);
            merkez = new Vector3((minX + maxX) * 0.5f, (ust + alt) * 0.5f, z);
            // sagdaki ve alttaki arayuz bolgesi bos kalsin
            merkez.x += 0.04f * 2f * d * t * Oran;
            merkez.y -= 0.05f * 2f * d * t;
        }

        // Karakterin yuzunu gosterecek yorunge acisi (180 derece kurali: kamera on tarafta kalir)
        float YuzAcisi(string id, float yedek)
        {
            Vector3 f;
            if (owner == null || !owner.IleriYon(id, out f) || f.z > 0.2f) return yedek;
            return Mathf.Clamp(-Mathf.Atan2(f.x, -f.z) * Mathf.Rad2Deg * 0.75f, -48f, 48f);
        }

        // Secili plani karakterlerin su anki yerine gore hesaplar
        void Hesapla()
        {
            Vector3 S = Vector3.zero, L = Vector3.zero;
            bool hs = planS.Length > 0 && owner.HeadPos(planS, out S) && S.y > Sahne3D.Origin.y - 5f;
            bool hl = planL.Length > 0 && owner.HeadPos(planL, out L) && L.y > Sahne3D.Origin.y - 5f;

            Plan p = plan;
            if (p == Plan.Tepki) { if (hl) { S = L; hs = true; } p = Plan.Yakin; }
            if (p == Plan.Omuz && hs && (!hl || Mathf.Abs(L.x - S.x) + Mathf.Abs(L.z - S.z) > 2.2f)) p = hl ? Plan.Orta : Plan.Yakin;
            if (p == Plan.Iki && !hl) p = Plan.Orta;
            if (p != Plan.Genis && !hs) p = Plan.Genis;
            bool araba = owner.Mekan != null && owner.Mekan.Oturmali;
            // omuz ustu: iki kisi yan yana, benzer boyda ve yakinsa kamera dinleyenin omzunun arkasina gecer;
            // yoksa (biri ayakta biri oturuyor, uzaklar, ust uste, araba) konusana yakin plan yapilir
            if (p == Plan.Omuz && (araba || !hl || Mathf.Abs(L.x - S.x) < 0.5f || Mathf.Abs(L.x - S.x) > 2.2f ||
                                   Mathf.Abs(L.y - S.y) > 0.35f || Mathf.Abs(L.z - S.z) > 0.8f || owner.YatiyorMu(planS) || owner.YatiyorMu(planL) ||
                                   owner.Yuruyor(planS) || owner.Yuruyor(planL))) p = Plan.Yakin;
            // plan sahne degisince (or. yuruyen kisi yerine varinca) kamera savrulmaz, kesme yapar;
            // yuruyen birine yakin plandayken de geride kalmaz
            if (p != sonEtkinPlan || (p == Plan.Yakin && planS.Length > 0 && owner.Yuruyor(planS))) { snap = true; sonEtkinPlan = p; }

            fov = Sahne3D.CamFov;
            bool yatik = hs && owner.YatiyorMu(plan == Plan.Tepki ? planL : planS);
            string yuzu = plan == Plan.Tepki ? planL : planS;
            float d;
            switch (p)
            {
                case Plan.Orta:
                    pivot = S + Vector3.down * 0.34f;
                    dist = 3.1f; yaw = planYon * 12f; pitch = 3f; minDist = 2.2f; maxDist = 6f;
                    break;

                case Plan.Omuz:
                    {
                        // gercek omuz ustu: kamera dinleyenin arkasinda ve biraz onde (180 derece kurali),
                        // dinleyenin basi/omzu on planda bulanik, konusanin yuzu karsida
                        Vector3 yonL = S - L; yonL.y = 0f; yonL.Normalize();
                        Vector3 kam = L - yonL * 0.6f + Vector3.back * 0.36f + Vector3.up * 0.1f;
                        fov = YakinFov + 6f;
                        pivot = S + Vector3.down * 0.1f + (L - S) * 0.08f;
                        Yorunge(kam, pivot, out dist, out yaw, out pitch);
                        minDist = dist - 0.15f; maxDist = dist + 0.6f;
                    }
                    break;

                case Plan.Yakin:
                    fov = YakinFov;
                    pivot = S + Vector3.down * 0.08f;
                    dist = 2.15f; yaw = YuzAcisi(yuzu, planYon * 10f); pitch = 2f; minDist = 1.6f; maxDist = 4f;
                    break;

                case Plan.Iki:
                    {
                        noktalar.Clear(); noktalar.Add(S); noktalar.Add(L);
                        Vector3 m;
                        Cerceve(noktalar, 0.75f, Sahne3D.CamFov, out m, out d);
                        pivot = m; dist = Mathf.Clamp(d, 2.6f, 7f); yaw = planYon * 5f; pitch = 3f;
                        minDist = 2.2f; maxDist = 8f;
                    }
                    break;

                default:
                    {
                        pivot = Sahne3D.Origin + new Vector3(0f, baseY, 0f);
                        dist = Sahne3D.CamDist; yaw = planYon * 3f; pitch = 0f; minDist = 4f; maxDist = 8.5f;
                        noktalar.Clear();
                        owner.KafaKonumlari(noktalar);
                        if (noktalar.Count > 0)
                        {
                            Vector3 m;
                            Cerceve(noktalar, 1.4f, Sahne3D.CamFov, out m, out d);
                            pivot.x = Mathf.Clamp(m.x, Sahne3D.Origin.x - 1.2f, Sahne3D.Origin.x + 1.2f);
                            dist = Mathf.Clamp(Mathf.Max(Sahne3D.CamDist, d), Sahne3D.CamDist, 8.5f);
                        }
                    }
                    break;
            }
            if (araba)
            {
                // araba: kamera on camdan, neredeyse tam karsidan bakar (direkler ve kapilar araya girmesin)
                yaw = Mathf.Clamp(yaw, -6f, 6f);
                pitch = 2f;
                if (p == Plan.Genis && noktalar.Count > 0)
                {
                    float minX = float.MaxValue, maxX = float.MinValue, y2 = 0f, z2 = 0f;
                    foreach (var n in noktalar) { minX = Mathf.Min(minX, n.x); maxX = Mathf.Max(maxX, n.x); y2 += n.y; z2 += n.z; }
                    y2 /= noktalar.Count; z2 /= noktalar.Count;
                    float t = Mathf.Tan(Sahne3D.CamFov * 0.5f * Mathf.Deg2Rad);
                    dist = Mathf.Clamp(((maxX - minX) * 0.5f + 0.34f) / (t * Oran * 0.95f), 3.2f, 6f);
                    pivot = new Vector3((minX + maxX) * 0.5f, y2 - 0.22f, z2);
                    minDist = 3f; maxDist = 6.5f; yaw = 0f;
                }
            }
            else if ((p == Plan.Yakin || p == Plan.Orta) && !yatik) EngelsizAci(S);
            if (yatik && p != Plan.Genis)
            {
                // yatan karaktere yukaridan bakilir
                pitch = 30f; yaw *= 0.4f; dist = Mathf.Max(dist, 2.1f);
            }
            pitch += aciPitch * (p == Plan.Genis ? 0.6f : 1f);

            // kamera oda duvarinin icine girmesin (ic mekan genisligi 5,2 m)
            if (araba) return;
            float kameraX = pivot.x - Sahne3D.Origin.x - Mathf.Sin(yaw * Mathf.Deg2Rad) * dist;
            if (Mathf.Abs(kameraX) > 2.35f)
            {
                float sinus = (pivot.x - Sahne3D.Origin.x - Mathf.Sign(kameraX) * 2.35f) / dist;
                yaw = Mathf.Asin(Mathf.Clamp(sinus, -1f, 1f)) * Mathf.Rad2Deg;
            }
        }

        // Tek kisilik planda baska bir govde araya girmesin / kadraj kenarinda yarim kalmasin:
        // kamera acisini (gerekirse bakis noktasini) bos kalan tarafa kaydirir.
        void EngelsizAci(Vector3 ozne)
        {
            digerleri.Clear();
            owner.KafaKonumlari(digerleri);
            float istenen = yaw;
            // istenen aci kucukse eski adaylarin hepsi ~0 oluyordu ve diger kisi kameranin onunde kaliyordu:
            // genis yan acilar ve daha yakin kamera da denenir
            float[] adaylar = { istenen, istenen * 0.6f, istenen * 0.3f, 0f, -istenen * 0.3f, -istenen * 0.6f, 25f, -25f, 40f, -40f };
            float enIyi = float.MinValue, enIyiAci = istenen, ilkDist = dist;
            for (int tur = 0; tur < 2; tur++)
            {
                foreach (var aci in adaylar)
                {
                    float bosluk = Bosluk(ozne, pivot, aci);
                    if (bosluk >= 0f) { yaw = aci; return; }
                    if (bosluk > enIyi) { enIyi = bosluk; enIyiAci = aci; }
                }
                dist = Mathf.Max(minDist, dist * 0.75f);
            }
            dist = ilkDist;
            yaw = enIyiAci;
            if (Sahne3D.KameraGunluk)
            {
                var sb = new System.Text.StringBuilder("[KameraEngel] ozne=" + ozne.ToString("F2") + " pivot=" + pivot.ToString("F2") + " dist=" + dist.ToString("F2") + " yaw=" + yaw.ToString("F0") + " bosluk=" + enIyi.ToString("F2") + " digerleri=");
                foreach (var n in digerleri) sb.Append(n.ToString("F2")).Append(' ');
                Debug.Log(sb.ToString());
            }
            // hicbir aci temiz degilse bakis noktasi diger kisiden uzaga kaydirilir
            foreach (var n in digerleri)
            {
                if ((n - ozne).sqrMagnitude < 0.01f || Mathf.Abs(n.x - ozne.x) > 1f) continue;
                pivot.x -= Mathf.Sign(n.x - ozne.x) * 0.12f;
                break;
            }
        }

        // >= 0: diger govdeler kadraj disinda
        float Bosluk(Vector3 ozne, Vector3 bakis, float aci)
        {
            float s = Mathf.Sin(aci * Mathf.Deg2Rad), c = Mathf.Cos(aci * Mathf.Deg2Rad);
            float kx = bakis.x - s * dist, kz = bakis.z - c * dist;
            float t = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * Oran;
            float en = 1f;
            foreach (var n in digerleri)
            {
                if ((n - ozne).sqrMagnitude < 0.01f) continue;
                float rx = n.x - kx, rz = n.z - kz;
                float derin = rx * s + rz * c;
                if (derin < 0.1f || derin > dist + 0.25f) continue;     // kameranin arkasinda ya da arka planda
                float yan = Mathf.Abs(rx * c - rz * s);
                en = Mathf.Min(en, yan - 0.34f - derin * t);
            }
            return en;
        }

        void PlanKur(Plan p, string sid, string lid, bool inner)
        {
            plan = p;
            planS = sid ?? "";
            planL = lid ?? "";
            planIc = inner;
            planYon = yon;
            itme = 0f;
            Vector3 t;
            bool hs = planS.Length > 0 && owner != null && owner.HeadPos(planS, out t);
            bool hl = planL.Length > 0 && owner != null && owner.HeadPos(planL, out t);
            if (p == Plan.Omuz && !hl) plan = Plan.Yakin;
            if (p == Plan.Tepki && !hl) plan = Plan.Yakin;
            if (plan != Plan.Genis && !hs && !(plan == Plan.Tepki && hl)) plan = Plan.Genis;
            switch (plan)
            {
                case Plan.Omuz: pushRate = -0.02f; break;
                case Plan.Yakin: pushRate = inner ? -0.06f : -0.025f; break;
                case Plan.Tepki: pushRate = -0.04f; break;
                case Plan.Iki: pushRate = -0.03f; break;
                default: pushRate = 0f; break;
            }
            if (owner != null) Hesapla();
            snap = true;
        }

        // ---------- Komutlar ----------
        // Donus: true = komut tamamen islendi (StoryPlayer eski islemi yapmasin).
        // eski != null ise 2D mekanda eski sisteme verilecek karsilik ("yakin" / "genis").
        public static bool Komut(Sahne3D s, string arg, out string eski)
        {
            eski = null;
            if (s == null || s.Yonetmen == null) return false;
            return s.Yonetmen.KomutIsle(arg, out eski);
        }

        public bool KomutIsle(string arg, out string eski)
        {
            eski = null;
            string raw = (arg ?? "").Trim();
            var t = raw.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
            string n = t.Length > 0 ? Util.Norm(t[0]) : "";
            string rest = t.Length > 1 ? Util.Norm(t[1]) : "";
            switch (n)
            {
                case "tepki":
                    if (!oda) { Debug.Log("[Hikaye] kamera: tepki sadece mekan3d icinde calisir (2D mekanda atlandi)."); return true; }
                    tepkiBekliyor = true; tepkiHedef = rest;
                    return true;
                case "yaklas":
                    if (!oda) { eski = "yakin"; return false; }
                    pushRate = -0.2f;
                    return true;
                case "uzaklas":
                    if (!oda) { eski = "genis"; return false; }
                    pushRate = 0.2f;
                    return true;
                case "goz": SetAci(0f); return true;
                case "yukaridan": SetAci(14f); return true;
                case "asagidan": SetAci(-12f); return true;
                case "bulanik":
                    if (owner != null && owner.Atmosfer != null) owner.Atmosfer.Bulaniklik(true);
                    return true;
                case "net":
                    if (owner != null && owner.Atmosfer != null) owner.Atmosfer.Bulaniklik(false);
                    return true;
                case "salla":
                    Panik(1f);
                    return false;
                case "orta":
                case "omuz":
                case "ters":
                case "iki":
                    if (!oda) { Debug.Log("[Hikaye] kamera: " + n + " sadece mekan3d icinde calisir (2D mekanda atlandi)."); return true; }
                    mod = n; tepkiSayac = -1f;
                    if (sonKonusan.Length > 0)
                    {
                        Plan p = PlanSec(-1, -1, 0, false, false, sonDinleyen.Length > 0, mod);
                        if (n == "ters" && p == Plan.Omuz) PlanKur(p, sonDinleyen, sonKonusan, false);
                        else PlanKur(p, sonKonusan, sonDinleyen, false);
                    }
                    return true;
                case "yakin":
                case "genis":
                case "otomatik":
                    mod = n; tepkiSayac = -1f;
                    if (oda && n != "otomatik") PlanKur(n == "genis" ? Plan.Genis : Plan.Yakin, sonKonusan, sonDinleyen, false);
                    return false;
            }
            return false;
        }

        void SetAci(float a)
        {
            aciPitch = a;
            if (oda) PlanKur(plan, planS, planL, planIc);
        }

        const float ArabaKameraTavani = 0.89f;     // Mekan3D.Ek.cs KurAraba: konsol ust kenari 0,92
        const float ArabaKameraOnu = -1.25f;       // konsolun on yuzu z=-0,78; kamera bundan en az 0,47 geride

        // ---------- Her kare ----------
        void LateUpdate()
        {
            if (cam == null) return;
            float dt = Time.deltaTime;
            if (!oda)
            {
                pivot = Sahne3D.Origin + new Vector3(0f, baseY, 0f);
                dist = Sahne3D.CamDist; fov = Sahne3D.CamFov;
                pitch = aciPitch * 0.4f; yaw = yaw2d; pushRate = 0f; itme = 0f;
            }
            else if (owner != null)
            {
                if (tepkiSayac > 0f)
                {
                    tepkiSayac -= dt;
                    if (tepkiSayac <= 0f && owner.HasActor(tepkiKisi))
                    {
                        sonTepkiSatiri = satirSayisi;
                        PlanKur(Plan.Tepki, sonKonusan, tepkiKisi, false);
                        if (Application.isEditor) Debug.Log("[Kamera] tepki cekimi: " + tepkiKisi);
                    }
                }
                Hesapla();
                if (pushRate != 0f)
                {
                    float yeni = itme + pushRate * dt;
                    if (dist + yeni > minDist && dist + yeni < maxDist) itme = yeni;
                    else pushRate = 0f;
                }
            }

            float hedefDist = dist + itme;
            if (snap)
            {
                cPivot = pivot; cDist = hedefDist; cYaw = yaw; cPitch = pitch; cFov = fov;
                snap = false;
            }
            else
            {
                float k = 1f - Mathf.Exp(-dt * 3.5f);
                cPivot = Vector3.Lerp(cPivot, pivot, k);
                cDist = Mathf.Lerp(cDist, hedefDist, k);
                cYaw = Mathf.LerpAngle(cYaw, yaw, k);
                cPitch = Mathf.LerpAngle(cPitch, pitch, k);
                cFov = Mathf.Lerp(cFov, fov, k);
            }

            // el titremesi: Perlin, kucuk genlik; panikte guclenir
            panik = Mathf.MoveTowards(panik, 0f, dt * 0.35f);
            float amp = (oda ? 0.2f : 0.08f) + panik * (oda ? 1.4f : 1.0f);
            float tt = Time.time * (0.55f + panik * 4f) + seed;
            float hx = (Mathf.PerlinNoise(tt, 0.13f) - 0.5f) * 2f * amp;
            float hy = (Mathf.PerlinNoise(tt, 7.7f) - 0.5f) * 2f * amp;
            float hr = (Mathf.PerlinNoise(tt, 15.3f) - 0.5f) * 2f * amp * 0.6f;

            Quaternion yor = Quaternion.Euler(cPitch, cYaw, 0f);
            Vector3 pos = cPivot + yor * new Vector3(0f, 0f, -cDist);
            float bakisPitch = cPitch, bakisYaw = cYaw;
            float bakisFov = cFov;
            // ARABA: kamera hep on konsolun ust kenarinin (0,92) hemen altinda durur. Boylece konsolun arkasinda
            // bu yukseklikten asagidaki her sey (oturanlarin kucagi, bacaklari) plan, mesafe ve boy ne olursa
            // olsun gorunmez. Konsola fazla yaklasirsa ya da altina inerse yuzler kayboldugu icin kamera geride
            // tutulur; yakin plan hissi gorus acisi daraltilarak korunur.
            if (owner != null && owner.Mekan != null && owner.Mekan.Oturmali)
            {
                float eskiMesafe = Vector3.Distance(pos, cPivot);
                pos.y = Sahne3D.Origin.y + ArabaKameraTavani;
                pos.z = Mathf.Min(pos.z, Sahne3D.Origin.z + ArabaKameraOnu);
                float yeniMesafe = Vector3.Distance(pos, cPivot);
                if (yeniMesafe > eskiMesafe && eskiMesafe > 0.01f)
                    bakisFov = 2f * Mathf.Rad2Deg * Mathf.Atan(Mathf.Tan(cFov * 0.5f * Mathf.Deg2Rad) * eskiMesafe / yeniMesafe);
                Vector3 e = Quaternion.LookRotation(cPivot - pos).eulerAngles;
                bakisPitch = e.x; bakisYaw = e.y;
            }
            cam.transform.SetPositionAndRotation(pos, Quaternion.Euler(bakisPitch + hy, bakisYaw + hx, hr));
            cam.fieldOfView = bakisFov;

            if (owner != null && owner.Atmosfer != null)
            {
                // odak degisimi: yakin ve tepki cekiminde arka plan yumusar, odak o karaktere gecer
                owner.Atmosfer.OdakMesafe(cDist);
                owner.Atmosfer.OtoBulaniklik(oda && (plan == Plan.Yakin || plan == Plan.Tepki || plan == Plan.Omuz));
            }
        }
    }
}
