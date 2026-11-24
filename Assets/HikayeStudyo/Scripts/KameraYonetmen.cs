using UnityEngine;

namespace HikayeStudyo
{
    // 3D kamera yonetmeni (Sahne3D'nin Kamera3D nesnesinde calisir). Duz Transform hareketi kullanir.
    // Kamera bir "yorunge" ile tarif edilir: pivot (bakilan nokta) + mesafe + yaw + pitch + FOV.
    //  - 3D mekan (mekan3d:) acikken: omuz ustu, yakin, orta, genis ve tepki planlari; diyalogda otomatik degisim,
    //    ayni plan ust uste 3 kez gelmez. ic ses = yavas yaklasma. kamera: tepki / yaklas / uzaklas / goz /
    //    yukaridan / asagidan / bulanik / net.
    //  - 2D mekanda (karakterler 2D arka plan uzerinde): sadece cok hafif yorunge cesitliligi ve el titremesi.
    //  - El titremesi (Perlin) her zaman acik, panik aninda guclenir.
    public class KameraYonetmen : MonoBehaviour
    {
        public enum Plan { Genis = 0, Orta = 1, Omuz = 2, Yakin = 3, Tepki = 4 }

        Sahne3D owner;
        Camera cam;
        float baseY = 1.1f;
        bool oda;
        string mod = "otomatik";
        float aciPitch;            // goz = 0, yukaridan = +, asagidan = -
        float yaw2d;

        Plan plan = Plan.Genis;
        int p1 = -1, p2 = -1;
        int genistenBeri;
        string sonKonusan = "", sonDinleyen = "";
        string tepkiHedef = "";
        bool tepkiBekliyor;
        float yon = 1f;

        // hedef yorunge
        Vector3 pivot;
        float dist = Sahne3D.CamDist, yaw, pitch, fov = Sahne3D.CamFov;
        float pushRate, minDist = 0.9f, maxDist = 9f;
        // gecerli yorunge (yumusatilmis)
        Vector3 cPivot;
        float cDist, cYaw, cPitch, cFov;
        bool snap = true;

        float panik, seed;

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
            sonKonusan = ""; sonDinleyen = "";
            tepkiBekliyor = false; tepkiHedef = "";
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

            if (!oda)
            {
                yaw2d = mod == "otomatik" ? yon * 5f : 0f;
                return false;
            }

            Plan p;
            if (tepkiBekliyor)
            {
                tepkiBekliyor = false;
                string hid = tepkiHedef.Length > 0 ? tepkiHedef : lid;
                if (hid.Length > 0 && hid != sid && owner.HasActor(hid))
                {
                    lid = hid;
                    p = Plan.Tepki;
                }
                else p = PlanSec(p1, p2, genistenBeri, inner, GucluDuygu(e), lid.Length > 0, mod);
            }
            else p = PlanSec(p1, p2, genistenBeri, inner, GucluDuygu(e), lid.Length > 0, mod);

            p2 = p1; p1 = (int)p;
            if (p == Plan.Genis) genistenBeri = 0; else genistenBeri++;
            sonDinleyen = lid;
            PlanKur(p, sid, lid, inner);
            return true;
        }

        // ---------- Plan geometrisi ----------
        static void Yorunge(Vector3 camPos, Vector3 hedef, out float d, out float y, out float p)
        {
            Vector3 v = camPos - hedef;
            d = Mathf.Max(0.3f, v.magnitude);
            y = Mathf.Atan2(-v.x, -v.z) * Mathf.Rad2Deg;
            p = Mathf.Asin(Mathf.Clamp(v.y / d, -1f, 1f)) * Mathf.Rad2Deg;
        }

        void PlanKur(Plan p, string sid, string lid, bool inner)
        {
            Vector3 S = Vector3.zero, L = Vector3.zero;
            bool hs = !string.IsNullOrEmpty(sid) && owner != null && owner.HeadPos(sid, out S);
            bool hl = !string.IsNullOrEmpty(lid) && owner != null && owner.HeadPos(lid, out L);
            if (p != Plan.Genis && !hs && !(p == Plan.Tepki && hl)) p = Plan.Genis;
            if (p == Plan.Omuz && !hl) p = Plan.Yakin;
            if (p == Plan.Tepki && !hl) p = Plan.Yakin;
            plan = p;
            pushRate = 0f; minDist = 0.9f; fov = Sahne3D.CamFov;
            Vector3 g = Sahne3D.Origin + new Vector3(0f, baseY, 0f);
            float d, y, pt;
            switch (p)
            {
                case Plan.Orta:
                    {
                        Vector3 c = hl ? Vector3.Lerp(S, L, 0.3f) : S;
                        pivot = c + Vector3.down * 0.3f;
                        dist = 3.1f; yaw = yon * 14f; pitch = 3f + aciPitch;
                        maxDist = 6f;
                    }
                    break;
                case Plan.Omuz:
                    {
                        Vector3 u = L - S; u.y = 0f;
                        if (u.sqrMagnitude < 0.01f) u = new Vector3(1f, 0f, 0f);
                        u.Normalize();
                        Vector3 pf = new Vector3(-u.z, 0f, u.x);
                        if (pf.z > 0f) pf = -pf;
                        Vector3 cp = L + u * 0.65f + pf * 0.18f + Vector3.up * (0.1f + Mathf.Sin(aciPitch * Mathf.Deg2Rad) * 0.5f);
                        Vector3 hedef = S + Vector3.down * 0.03f;
                        Yorunge(cp, hedef, out d, out y, out pt);
                        pivot = hedef; dist = d; yaw = y; pitch = pt + aciPitch * 0.5f;
                        pushRate = -0.015f; minDist = 1.1f; maxDist = d + 2f;
                    }
                    break;
                case Plan.Yakin:
                    pivot = S + Vector3.down * 0.04f;
                    dist = 1.35f; yaw = yon * 10f; pitch = 2f + aciPitch; fov = 28f;
                    pushRate = inner ? -0.05f : -0.015f; minDist = 0.95f; maxDist = 4f;
                    break;
                case Plan.Tepki:
                    pivot = L + Vector3.down * 0.04f;
                    dist = 1.3f; yaw = yon * 12f; pitch = 2f + aciPitch; fov = 28f;
                    pushRate = -0.03f; minDist = 0.95f; maxDist = 4f;
                    break;
                default:
                    {
                        pivot = g;
                        if (hs && hl) pivot.x += ((S.x + L.x) * 0.5f - Sahne3D.Origin.x) * 0.2f;
                        dist = Sahne3D.CamDist; yaw = yon * 3f; pitch = aciPitch * 0.6f;
                        maxDist = 8.5f;
                    }
                    break;
            }
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
                    pushRate = -0.2f; minDist = Mathf.Min(minDist, 0.95f);
                    return true;
                case "uzaklas":
                    if (!oda) { eski = "genis"; return false; }
                    pushRate = 0.2f; maxDist = Mathf.Max(maxDist, dist + 2.5f);
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
                case "yakin":
                case "genis":
                case "otomatik":
                    mod = n;
                    if (oda && n != "otomatik") PlanKur(n == "genis" ? Plan.Genis : Plan.Yakin, sonKonusan, sonDinleyen, false);
                    return false;
            }
            return false;
        }

        void SetAci(float a)
        {
            aciPitch = a;
            if (oda) PlanKur(plan, sonKonusan, sonDinleyen, false);
        }

        // ---------- Her kare ----------
        void LateUpdate()
        {
            if (cam == null) return;
            float dt = Time.deltaTime;

            if (!oda)
            {
                pivot = Sahne3D.Origin + new Vector3(0f, baseY, 0f);
                dist = Sahne3D.CamDist; fov = Sahne3D.CamFov;
                pitch = aciPitch * 0.4f; yaw = yaw2d; pushRate = 0f;
            }
            else if (pushRate != 0f)
            {
                dist = Mathf.Clamp(dist + pushRate * dt, minDist, maxDist);
                if (dist <= minDist + 0.001f || dist >= maxDist - 0.001f) pushRate = 0f;
            }

            if (snap)
            {
                cPivot = pivot; cDist = dist; cYaw = yaw; cPitch = pitch; cFov = fov;
                snap = false;
            }
            else
            {
                float k = 1f - Mathf.Exp(-dt * 7f);
                cPivot = Vector3.Lerp(cPivot, pivot, k);
                cDist = Mathf.Lerp(cDist, dist, k);
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
            cam.transform.SetPositionAndRotation(pos, Quaternion.Euler(cPitch + hy, cYaw + hx, hr));
            cam.fieldOfView = cFov;

            if (owner != null && owner.Atmosfer != null) owner.Atmosfer.OdakMesafe(cDist);
        }
    }
}
