using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 10: sarilma, el tutma, yuz yuze durma, tepki, sahne icinde yurume, esya tutma, kapi, yatak ve ayaga kalkma.
    // Komut adlari ve gecerli degerler Hareket3D'de; kol/govde pozlari KolPozu'ndadir.
    // Bu komutlar hikayede yoksa asagidaki listeler bos kalir ve Sahne3D eskisi gibi davranir.
    public partial class Sahne3D
    {
        // Iki karakter arasinda suren etkilesim
        class Bag
        {
            public string tur, a, b;
            public Vector3 eskiA, eskiB;
            public bool tasindiA, tasindiB;
        }

        readonly List<Bag> baglar = new List<Bag>();
        readonly Dictionary<string, string> pozDurum = new Dictionary<string, string>();   // "oturuyor" / "yatiyor"
        readonly Dictionary<string, string> yataktaki = new Dictionary<string, string>();  // yatak: komutu -> "yat" / "otur"
        readonly Dictionary<string, Vector3> yatakOncesi = new Dictionary<string, Vector3>();
        readonly HashSet<string> sirtustu = new HashSet<string>();                          // yatma klibi yok: govde dondurulmus
        readonly HashSet<string> hareketUyarilari = new HashSet<string>();

        void HareketUyar(string mesaj)
        {
            if (hareketUyarilari.Add(mesaj)) Debug.LogWarning("[Hikaye] " + mesaj);
        }

        // StoryPlayer cagirir. aninda = "bu satirdan oynat" atlamasi: bekleme ve gecici hareket yok, yalniz kalici durum
        public IEnumerator HareketCo(StoryItem it, bool aninda)
        {
            if (it == null) yield break;
            string key = Util.Norm(it.Character);
            var p = Hareket3D.Parcala(it.Arg);
            string satir = it.Line + ". satır '" + key + ":' ";
            if (Hareket3D.Dogrula(key, it.Arg) != null) { HareketUyar(satir + "değeri geçersiz, atlandı: " + it.Arg); yield break; }
            if (Mekan != null && Mekan.Oturmali && key != "tepki_ver" && key != "tut" && key != "birak")
            {
                HareketUyar(satir + "arabada herkes oturur; bu komut atlandı.");
                yield break;
            }
            switch (key)
            {
                case "sarilma":
                case "el_tut":
                case "yuzyuze":
                    yield return IkiliCo(key, p[0], p[1], satir, aninda);
                    break;
                case "ayril":
                    yield return AyrilCo(p.Length > 0 ? Util.Norm(p[0]) : "", aninda);
                    break;
                case "tepki_ver":
                    if (!aninda) TepkiVer(p[0], Util.Norm(p[1]), p.Length > 2 ? p[2] : "", satir);
                    break;
                case "git":
                    yield return GitCo(p[0], p[1], p.Length > 2 && Util.Norm(p[2]) == "kos", satir, aninda);
                    break;
                case "tut":
                    Tut(p[0], p[1], p.Length < 3 || Util.Norm(p[2]) == "sag", satir);
                    if (!aninda) yield return new WaitForSeconds(0.45f);
                    break;
                case "birak":
                    EsyaBirak(p[0], satir);
                    break;
                case "kapi":
                    yield return KapiCo(p.Length > 1 ? p[0] : "", Util.Norm(p[p.Length - 1]) == "ac", satir, aninda);
                    break;
                case "yatak":
                    Yatak(p[0], Util.Norm(p[1]) == "yat", satir);
                    break;
                case "kalk":
                    yield return KalkCo(p[0], satir, aninda);
                    break;
            }
        }

        // ---------- yardimcilar ----------
        Karakter3D HareketAktor(string id, string satir)
        {
            var a = Aktor(id, true);
            if (a == null) HareketUyar(satir + "karakter sahnede değil ya da 3D değil: " + id);
            return a;
        }

        KolPozu Kollar(Karakter3D a, string id, string satir)
        {
            var k = KolPozu.Al(a);
            if (k == null) HareketUyar(satir + "'" + id + "' insan iskeletli (Humanoid) değil; bu hareket atlandı.");
            return k;
        }

        static Transform Kemik(Karakter3D a, HumanBodyBones b)
        {
            if (a == null) return null;
            var an = a.GetComponent<Animator>();
            if (an == null) an = a.GetComponentInChildren<Animator>();
            return an != null && an.isHuman ? an.GetBoneTransform(b) : null;
        }

        static Vector3 KemikPos(Karakter3D a, HumanBodyBones b, float yedekYukseklik)
        {
            var t = Kemik(a, b);
            if (t != null) return t.position;
            return a != null ? a.transform.position + a.transform.up * yedekYukseklik * a.transform.localScale.y : Vector3.zero;
        }

        static Vector3 Gogus(Karakter3D a)
        {
            var t = Kemik(a, HumanBodyBones.UpperChest);
            if (t == null) t = Kemik(a, HumanBodyBones.Chest);
            if (t == null) return KemikPos(a, HumanBodyBones.Spine, 1.2f);
            return t.position;
        }

        static float OmuzGenisligi(Karakter3D a)
        {
            var l = Kemik(a, HumanBodyBones.LeftUpperArm);
            var r = Kemik(a, HumanBodyBones.RightUpperArm);
            return l != null && r != null ? Mathf.Clamp(Vector3.Distance(l.position, r.position), 0.12f, 0.6f) : 0.3f;
        }

        static Vector3 Omuz(Karakter3D a, bool sagEl)
        {
            return KemikPos(a, sagEl ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm, 1.4f);
        }

        // Dirsek disari ve asagi baksin
        static Vector3 Dirsek(Karakter3D a, bool sagEl)
        {
            var t = a.transform;
            return Omuz(a, sagEl) + t.right * (sagEl ? 0.4f : -0.4f) - t.up * 0.3f;
        }

        bool Sabit(string k) { return pozDurum.ContainsKey(k) || yataktaki.ContainsKey(k); }

        bool Yatiyor(string k)
        {
            string d;
            return pozDurum.TryGetValue(k, out d) && d == "yatiyor";
        }

        void Dondur(string k, Karakter3D a, float yaw, bool anindaDon)
        {
            yawTarget[k] = yaw;
            lockedFacing.Add(k);
            if (anindaDon) a.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        IEnumerator Kay(Karakter3D a, Vector3 hedef, float sure)
        {
            if (a == null) yield break;
            Vector3 bas = a.transform.position;
            float t = 0f;
            while (a != null && t < sure)
            {
                t += Time.deltaTime;
                float x = Mathf.Clamp01(t / sure);
                a.transform.position = Vector3.Lerp(bas, hedef, x * x * (3f - 2f * x));
                yield return null;
            }
        }

        // Kisa mesafede kayarak, uzun mesafede yuruyerek hedefe gider ve yaw yonune doner
        IEnumerator YaklasCo(string k, Karakter3D a, Vector3 hedef, float yaw, bool aninda)
        {
            if (a == null) yield break;
            if (aninda) { a.transform.position = hedef; Dondur(k, a, yaw, true); yield break; }
            Vector3 d = hedef - a.transform.position;
            d.y = 0f;
            if (d.magnitude > 0.6f) yield return WalkToPos(k, a, hedef, false, true, yaw);
            else { Dondur(k, a, yaw, false); yield return Kay(a, hedef, 0.45f); }
        }

        // ---------- iki karakter: sarilma, el_tut, yuzyuze ----------
        IEnumerator IkiliCo(string tur, string idA, string idB, string satir, bool aninda)
        {
            var a = HareketAktor(idA, satir);
            var b = HareketAktor(idB, satir);
            if (a == null || b == null) yield break;
            string ka = Util.Norm(idA), kb = Util.Norm(idB);
            bool kolGerekli = tur != "yuzyuze";
            KolPozu pa = null, pb = null;
            if (kolGerekli)
            {
                pa = Kollar(a, idA, satir);
                pb = Kollar(b, idB, satir);
                if (pa == null || pb == null) yield break;
            }
            BagBitir(ka);
            BagBitir(kb);

            bool sabitA = Sabit(ka), sabitB = Sabit(kb);
            var bag = new Bag { tur = tur, a = ka, b = kb, eskiA = a.transform.position, eskiB = b.transform.position };
            baglar.Add(bag);

            if (tur == "yuzyuze")
            {
                float yon = a.transform.position.x <= b.transform.position.x ? 1f : -1f;
                // ic ice durmasinlar: ikisi de ayaktaysa ayni hizada, aralarinda bosluk
                if (!sabitA && !sabitB)
                {
                    Vector3 hA = a.transform.position, hB = b.transform.position;
                    float aralik = Aralik(a, b);
                    if (Mathf.Abs(hA.x - hB.x) < aralik || Mathf.Abs(hA.z - hB.z) > 0.25f)
                    {
                        AraAc(ref hA, ref hB, aralik, yon);
                        bag.tasindiA = bag.tasindiB = true;
                        slots[ka] = hA.x - Origin.x;
                        slots[kb] = hB.x - Origin.x;
                        if (aninda) { a.transform.position = hA; b.transform.position = hB; }
                        else
                        {
                            var k1 = StartCoroutine(Kay(a, hA, 0.45f));
                            var k2 = StartCoroutine(Kay(b, hB, 0.45f));
                            yield return k1;
                            yield return k2;
                        }
                        if (a == null || b == null) yield break;
                    }
                }
                if (!sabitA) Dondur(ka, a, 180f - yon * 80f, aninda);
                if (!sabitB) Dondur(kb, b, 180f + yon * 80f, aninda);
                YuzSahne.Bak(ka, kb);
                YuzSahne.Bak(kb, ka);
                if (!aninda) yield return new WaitForSeconds(0.5f);
                yield break;
            }

            float swA = OmuzGenisligi(a), swB = OmuzGenisligi(b), sw = (swA + swB) * 0.5f;
            if (!sabitA && !sabitB)
            {
                // ikisi de ayakta: ortada bulusur
                Vector3 p1 = a.transform.position, p2 = b.transform.position;
                float yon = p1.x <= p2.x ? 1f : -1f;
                Vector3 m = (p1 + p2) * 0.5f;
                m.y = Mathf.Min(p1.y, p2.y);
                if (!OdaAktif) m.x = Origin.x + Mathf.Clamp(m.x - Origin.x, -0.4f, 0.4f);
                float mesafe = tur == "sarilma" ? sw : sw * 1.9f;
                float yawA = tur == "sarilma" ? 180f - yon * 90f : 180f;
                float yawB = tur == "sarilma" ? 180f + yon * 90f : 180f;
                Vector3 hA = m + new Vector3(-yon * mesafe * 0.5f, 0f, 0f);
                Vector3 hB = m + new Vector3(yon * mesafe * 0.5f, 0f, 0f);
                if (tur == "sarilma")
                {
                    // baslar ust uste binmesin: ikisi de kendi soluna kayar
                    hA -= Quaternion.Euler(0f, yawA, 0f) * Vector3.right * sw * 0.2f;
                    hB -= Quaternion.Euler(0f, yawB, 0f) * Vector3.right * sw * 0.2f;
                }
                bag.tasindiA = bag.tasindiB = true;
                if (aninda)
                {
                    yield return YaklasCo(ka, a, hA, yawA, true);
                    yield return YaklasCo(kb, b, hB, yawB, true);
                }
                else
                {
                    var c1 = StartCoroutine(YaklasCo(ka, a, hA, yawA, false));
                    var c2 = StartCoroutine(YaklasCo(kb, b, hB, yawB, false));
                    yield return c1;
                    yield return c2;
                }
            }
            else if (sabitA != sabitB)
            {
                // biri oturuyor/yatiyor: digeri onun yanina gelir
                var s = sabitA ? a : b;
                var h = sabitA ? b : a;
                string ks = sabitA ? ka : kb, kh = sabitA ? kb : ka;
                Vector3 g = Gogus(s);
                float taraf = h.transform.position.x <= g.x ? -1f : 1f;
                float uzak = Yatiyor(ks) ? 0.55f : sw * 1.4f;
                Vector3 hedef = new Vector3(g.x + taraf * uzak, Mathf.Min(h.transform.position.y, Origin.y + 0.15f), g.z);
                if (sabitA) bag.tasindiB = true; else bag.tasindiA = true;
                yield return YaklasCo(kh, h, hedef, 180f + taraf * 90f, aninda);
            }
            if (a == null || b == null) yield break;

            if (tur == "sarilma")
            {
                if (!Yatiyor(ka)) SarilKollari(a, b, pa);
                if (!Yatiyor(kb)) SarilKollari(b, a, pb);
                pa.Egim(KolPozu.Katman.Etkilesim, SarilmaEgimi(ka, kb));
                pb.Egim(KolPozu.Katman.Etkilesim, SarilmaEgimi(kb, ka));
            }
            else ElTut(a, b, pa, pb, ka, kb);

            if (!aninda) yield return new WaitForSeconds(0.6f);
        }

        float SarilmaEgimi(string ben, string diger)
        {
            if (Sabit(ben)) return 0f;
            if (Yatiyor(diger)) return 35f;
            return Sabit(diger) ? 22f : 4f;
        }

        // Kollar karsidakinin sirtina dolanir
        void SarilKollari(Karakter3D ben, Karakter3D o, KolPozu p)
        {
            for (int i = 0; i < 2; i++)
            {
                bool sagEl = i == 0;
                float yan = sagEl ? -1f : 1f;            // benim sag elim karsidakinin sol tarafina
                float yuk = sagEl ? 0.05f : -0.12f;      // bir el omuzda, biri belde
                p.El(sagEl, KolPozu.Katman.Etkilesim,
                    () =>
                    {
                        if (o == null) return Gogus(ben);
                        float sw = OmuzGenisligi(o);
                        var t = o.transform;
                        return Gogus(o) - t.forward * sw * 0.45f + t.right * yan * sw * 0.28f + Vector3.up * yuk;
                    },
                    () => Dirsek(ben, sagEl) + ben.transform.forward * 0.15f);
            }
        }

        // Karakterin donecegi yonde sag tarafi (henuz o yone donmemis olabilir)
        Vector3 HedefSag(string k, Karakter3D a)
        {
            float yaw;
            return yawTarget.TryGetValue(k, out yaw) ? Quaternion.Euler(0f, yaw, 0f) * Vector3.right : a.transform.right;
        }

        // Yan yana: iki ic el ortada bulusur. Biri oturuyor/yatiyorsa yalniz ayaktaki onun elini tutar.
        void ElTut(Karakter3D a, Karakter3D b, KolPozu pa, KolPozu pb, string ka, string kb)
        {
            bool sabitA = Sabit(ka), sabitB = Sabit(kb);
            if (sabitA != sabitB)
            {
                var s = sabitA ? a : b;
                var h = sabitA ? b : a;
                var ph = sabitA ? pb : pa;
                // sabit karakterin yaklasana yakin eli ve yaklasanin o ele yakin kolu
                Vector3 sl = KemikPos(s, HumanBodyBones.LeftHand, 0.9f), sr = KemikPos(s, HumanBodyBones.RightHand, 0.9f);
                bool sSag = (sr - h.transform.position).sqrMagnitude < (sl - h.transform.position).sqrMagnitude;
                var hedefKemik = sSag ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
                Vector3 el = sSag ? sr : sl;
                bool hSag = Vector3.Dot(el - h.transform.position, HedefSag(sabitA ? kb : ka, h)) >= 0f;
                ph.El(hSag, KolPozu.Katman.Etkilesim,
                    () => s != null ? KemikPos(s, hedefKemik, 0.9f) + Vector3.up * 0.03f : Gogus(h),
                    () => Dirsek(h, hSag));
                ph.Egim(KolPozu.Katman.Etkilesim, Yatiyor(sabitA ? ka : kb) ? 28f : 8f);
                return;
            }

            bool aSag = Vector3.Dot(b.transform.position - a.transform.position, HedefSag(ka, a)) > 0f;
            bool bSag = Vector3.Dot(a.transform.position - b.transform.position, HedefSag(kb, b)) > 0f;
            System.Func<Vector3> orta = () =>
            {
                if (a == null || b == null) return Vector3.zero;
                Vector3 m = (Omuz(a, aSag) + Omuz(b, bSag)) * 0.5f;
                m.y = (KemikPos(a, HumanBodyBones.Hips, 0.9f).y + KemikPos(b, HumanBodyBones.Hips, 0.9f).y) * 0.5f;
                m.z -= 0.05f;
                return m;
            };
            pa.El(aSag, KolPozu.Katman.Etkilesim,
                () => b != null ? orta() + (a.transform.position - b.transform.position).normalized * 0.025f : Gogus(a),
                () => Dirsek(a, aSag) - a.transform.forward * 0.2f);
            pb.El(bSag, KolPozu.Katman.Etkilesim,
                () => a != null ? orta() + (b.transform.position - a.transform.position).normalized * 0.025f : Gogus(b),
                () => Dirsek(b, bSag) - b.transform.forward * 0.2f);
        }

        // Etkilesimi yerinde birakir (karakter baska bir yere gidiyor ya da sahneden cikiyor)
        void BagBitir(string k)
        {
            for (int i = baglar.Count - 1; i >= 0; i--)
            {
                var g = baglar[i];
                if (g.a != k && g.b != k) continue;
                baglar.RemoveAt(i);
                BagCoz(g);
            }
        }

        void BagCoz(Bag g)
        {
            foreach (var k in new[] { g.a, g.b })
            {
                Karakter3D a;
                if (!actors.TryGetValue(k, out a) || a == null) continue;
                var p = a.GetComponent<KolPozu>();
                if (p != null) p.Birak(KolPozu.Katman.Etkilesim);
                if (g.tur == "yuzyuze") YuzSahne.Bak(k, "serbest");
                if (!Sabit(k)) lockedFacing.Remove(k);
            }
        }

        // ayril: [Karakter] -> kollar iner, karakterler eski yerlerine doner
        IEnumerator AyrilCo(string k, bool aninda)
        {
            var bitecek = new List<Bag>();
            for (int i = baglar.Count - 1; i >= 0; i--)
            {
                var g = baglar[i];
                if (k.Length > 0 && g.a != k && g.b != k) continue;
                bitecek.Add(g);
                baglar.RemoveAt(i);
            }
            foreach (var g in bitecek)
            {
                BagCoz(g);
                // eski yerler birbirine cok yakinsa oraya donunce ic ice kalirlar: araliklari acilir
                Vector3 eA = g.eskiA, eB = g.eskiB;
                Karakter3D oa, ob;
                if (actors.TryGetValue(g.a, out oa) && oa != null && actors.TryGetValue(g.b, out ob) && ob != null
                    && !Sabit(g.a) && !Sabit(g.b) && g.tasindiA && g.tasindiB && Mathf.Abs(eA.x - eB.x) < Aralik(oa, ob))
                    AraAc(ref eA, ref eB, Aralik(oa, ob), oa.transform.position.x <= ob.transform.position.x ? 1f : -1f);
                for (int i = 0; i < 2; i++)
                {
                    string id = i == 0 ? g.a : g.b;
                    if (!(i == 0 ? g.tasindiA : g.tasindiB)) continue;
                    Karakter3D a;
                    if (!actors.TryGetValue(id, out a) || a == null || Sabit(id)) continue;
                    Vector3 eski = i == 0 ? eA : eB;
                    slots[id] = eski.x - Origin.x;
                    if (aninda) a.transform.position = eski;
                    else if ((eski - a.transform.position).magnitude > 0.8f) StartCoroutine(WalkToPos(id, a, eski, false, true, float.NaN));
                    else StartCoroutine(Kay(a, eski, 0.5f));
                }
            }
            if (bitecek.Count == 0) yield break;
            ReFaceAll();
            if (!aninda) yield return new WaitForSeconds(0.55f);
        }

        // Ayakta duran iki karakter arasinda birakilacak en az yatay mesafe
        static float Aralik(Karakter3D a, Karakter3D b)
        {
            return (OmuzGenisligi(a) + OmuzGenisligi(b)) * 1.5f;
        }

        // Iki noktayi ortalari sabit kalacak sekilde yan yana (ayni derinlikte) ayirir; yon: a soldaysa 1
        void AraAc(ref Vector3 a, ref Vector3 b, float aralik, float yon)
        {
            float mx = (a.x + b.x) * 0.5f, z = (a.z + b.z) * 0.5f;
            if (!OdaAktif)
            {
                // 2D mekanda kamera sabit: ikisi de kadrajda kalsin
                float sol = -0.62f + aralik * 0.5f, sag = 0.54f - aralik * 0.5f;
                mx = Origin.x + (sol < sag ? Mathf.Clamp(mx - Origin.x, sol, sag) : -0.04f);
            }
            a = new Vector3(mx - yon * aralik * 0.5f, a.y, z);
            b = new Vector3(mx + yon * aralik * 0.5f, b.y, z);
        }

        // ---------- kamera yonetmeni icin ----------
        // Sarilan / el tutan karakterin esi; yoksa ""
        public string EtkilesimEsi(string k)
        {
            foreach (var g in baglar)
            {
                if (g.tur == "yuzyuze") continue;
                if (g.a == k) return g.b;
                if (g.b == k) return g.a;
            }
            return "";
        }

        public bool YatiyorMu(string id)
        {
            string k = Util.Norm(id), d;
            return Yatiyor(k) || (yataktaki.TryGetValue(k, out d) && d == "yat");
        }

        // Karakterin onunun baktigi yon
        public bool IleriYon(string id, out Vector3 f)
        {
            f = Vector3.back;
            Karakter3D a;
            if (!actors.TryGetValue(Util.Norm(id), out a) || a == null) return false;
            f = a.transform.forward;
            return true;
        }

        // Sahnedeki oyuncularin kafa konumlari (kapi acilirken gizlenenler sayilmaz)
        public void KafaKonumlari(List<Vector3> liste)
        {
            foreach (var kv in actors)
                if (kv.Value != null && kv.Value.transform.position.y > Origin.y - 5f && !ArabadaGizli(kv.Value)) liste.Add(HeadOf(kv.Value));
        }

        // ---------- tepki ----------
        void TepkiVer(string id, string tepki, string digerId, string satir)
        {
            var a = HareketAktor(id, satir);
            if (a == null) return;
            string k = Util.Norm(id);
            string diger = digerId.Length > 0 ? Util.Norm(digerId) : DigerOyuncu(id);
            Karakter3D o;
            bool eskiKilit = lockedFacing.Contains(k);
            float eskiYaw;
            if (!yawTarget.TryGetValue(k, out eskiYaw)) eskiYaw = 180f;
            bool donduruldu = false;
            if (!Sabit(k) && !BagliMi(k) && diger.Length > 0 && diger != k && actors.TryGetValue(diger, out o) && o != null)
            {
                Dondur(k, a, 180f - (o.transform.position.x >= a.transform.position.x ? 1f : -1f) * 55f, false);
                donduruldu = true;
            }

            string klip = "";
            if (!Sabit(k))
                foreach (var n in Hareket3D.TepkiKlipleri(tepki))
                    if (a.Find(n) != null) { a.Play(n, 0.2f); klip = a.CurrentClip; break; }

            KolPozu p = null;
            if (klip.Length == 0)
            {
                p = Kollar(a, id, satir);
                if (p != null) TepkiPozu(a, p, tepki);
            }
            StartCoroutine(TepkiBitirCo(k, a, p, klip, donduruldu, eskiKilit, eskiYaw));
        }

        bool BagliMi(string k)
        {
            foreach (var g in baglar) if (g.a == k || g.b == k) return true;
            return false;
        }

        // Klibi olmayan tepkiler: kol, govde ve bas pozu
        void TepkiPozu(Karakter3D a, KolPozu p, string tepki)
        {
            p.Birak(KolPozu.Katman.Gecici);
            float o = OmuzGenisligi(a) / 0.3f;
            var t = a.transform;
            // (ileri, yana, yukari): gogus ya da basa gore
            System.Action<bool, bool, float, float, float> el = (sagEl, basaGore, ileri, yana, yukari) =>
            {
                p.El(sagEl, KolPozu.Katman.Gecici,
                    () => (basaGore ? KemikPos(a, HumanBodyBones.Head, 1.6f) : Gogus(a)) +
                          (t.forward * ileri + t.right * (sagEl ? yana : -yana) + t.up * yukari) * o,
                    () => Dirsek(a, sagEl));
            };
            switch (tepki)
            {
                case "sasir": el(true, false, 0.2f, 0.18f, 0.05f); el(false, false, 0.2f, 0.18f, 0.05f); p.Egim(KolPozu.Katman.Gecici, -8f); break;
                case "kork": el(true, false, 0.17f, 0.05f, 0.1f); el(false, false, 0.17f, 0.05f, 0.1f); p.Egim(KolPozu.Katman.Gecici, -12f); break;
                case "uzul": p.Egim(KolPozu.Katman.Gecici, 12f); break;
                case "agla": el(true, true, 0.13f, 0.045f, 0.03f); el(false, true, 0.13f, 0.045f, 0.03f); p.Egim(KolPozu.Katman.Gecici, 14f); break;
                case "sevin": el(true, true, 0.05f, 0.3f, 0.18f); el(false, true, 0.05f, 0.3f, 0.18f); p.Egim(KolPozu.Katman.Gecici, -4f); break;
                case "kiz": el(true, false, 0.14f, 0.2f, -0.18f); el(false, false, 0.14f, 0.2f, -0.18f); p.Egim(KolPozu.Katman.Gecici, 9f); break;
                case "dusun": el(true, true, 0.11f, 0.03f, -0.03f); p.Egim(KolPozu.Katman.Gecici, 4f); break;
                case "onayla": p.BasHareketi(1); break;
                case "reddet": p.BasHareketi(2); break;
                case "el_salla":
                    p.El(true, KolPozu.Katman.Gecici,
                        () => Omuz(a, true) + (t.right * (0.22f + Mathf.Sin(Time.time * 9f) * 0.08f) + t.up * 0.2f + t.forward * 0.08f) * o,
                        () => Dirsek(a, true));
                    break;
            }
        }

        IEnumerator TepkiBitirCo(string k, Karakter3D a, KolPozu p, string klip, bool donduruldu, bool eskiKilit, float eskiYaw)
        {
            yield return new WaitForSeconds(1.8f);
            if (a == null) yield break;
            if (p != null) p.Birak(KolPozu.Katman.Gecici);
            if (klip.Length > 0 && a.CurrentClip == klip && !Sabit(k)) Play(a, Hareket3D.KlipDur);
            if (donduruldu && !walking.Contains(k) && !BagliMi(k) && !Sabit(k))
            {
                yawTarget[k] = eskiYaw;
                if (!eskiKilit) { lockedFacing.Remove(k); ReFaceAll(); }
            }
        }

        // ---------- sahne icinde yurume / kosma ----------
        IEnumerator GitCo(string id, string hedefAdi, bool kos, string satir, bool aninda)
        {
            var a = HareketAktor(id, satir);
            if (a == null) yield break;
            string k = Util.Norm(id), h = Util.Norm(hedefAdi);
            Vector3 hedef;
            float yaw;
            Karakter3D o;
            if (NoktaBul(h, out hedef, out yaw))
            {
                if (h == "orta" || h == "sol" || h == "sag") yaw = float.NaN;
                // yatak gibi yuksek noktalara yurunerek cikilmaz: ayak ucunda durulur
                if (hedef.y > Origin.y + 0.2f) { hedef.y = Origin.y; hedef.z -= 0.8f; yaw = float.NaN; }
            }
            else if (actors.TryGetValue(h, out o) && o != null && h != k)
            {
                float taraf = a.transform.position.x <= o.transform.position.x ? -1f : 1f;
                hedef = o.transform.position + new Vector3(taraf * OmuzGenisligi(o) * 2.2f, 0f, 0f);
                hedef.y = Mathf.Min(hedef.y, Origin.y + 0.15f);
                yaw = 180f + taraf * 55f;
            }
            else { NoktaUyarisi(hedefAdi); yield break; }

            BagBitir(k);
            YatakBirak(k, a);
            pozDurum.Remove(k);
            lockedFacing.Remove(k);
            if (konusan == k) konusan = "";
            slots[k] = hedef.x - Origin.x;
            if (aninda)
            {
                a.transform.position = hedef;
                if (float.IsNaN(yaw)) { yawTarget[k] = FacingYaw(k, slots[k]); a.transform.rotation = Quaternion.Euler(0f, yawTarget[k], 0f); }
                else Dondur(k, a, yaw, true);
                Play(a, Hareket3D.KlipDur);
                ReFaceAll();
                yield break;
            }
            yield return WalkToPos(k, a, hedef, kos, true, yaw);
        }

        // ---------- esya ----------
        void Tut(string id, string esya, bool sagEl, string satir)
        {
            var a = HareketAktor(id, satir);
            if (a == null) return;
            var p = Kollar(a, id, satir);
            if (p == null) return;
            p.Birak(KolPozu.Katman.Esya);
            p.EsyaTak(esya, sagEl, transform);
            float o = OmuzGenisligi(a) / 0.3f;
            var t = a.transform;
            p.El(sagEl, KolPozu.Katman.Esya,
                () => Gogus(a) + (t.forward * 0.27f + t.right * (sagEl ? 0.11f : -0.11f) - t.up * 0.12f) * o,
                () => Dirsek(a, sagEl));
        }

        void EsyaBirak(string id, string satir)
        {
            var a = Aktor(id, false);
            if (a == null) { HareketUyar(satir + "karakter sahnede değil: " + id); return; }
            var p = a.GetComponent<KolPozu>();
            if (p == null) return;
            p.EsyaSil();
            p.Birak(KolPozu.Katman.Esya);
        }

        // ---------- kapi ----------
        // kapi: ac | kapat | Karakter ac  (3D mekanin kapisi)
        IEnumerator KapiCo(string id, bool ac, string satir, bool aninda)
        {
            if (Mekan == null || !Mekan.KapiVar) { HareketUyar(satir + "kapılı bir 3D mekan yok (mekan3d: oda, ev, mutfak, yatak_odasi, hastane, sokak, koy)."); yield break; }
            var mentese = Mekan.transform.Find("KapiMentese");
            var tokmak = mentese != null ? mentese.Find("KapiTokmak") : null;
            var a = id.Length > 0 ? HareketAktor(id, satir) : null;
            if (a == null || tokmak == null) { Mekan.KapiAc(ac); if (!aninda) yield return new WaitForSeconds(0.45f); yield break; }

            string k = Util.Norm(id);
            if (konusan == k) konusan = "";
            Vector3 kol = mentese.position + tokmak.localPosition;     // kapi kapaliyken tokmagin yeri
            Vector3 dur;
            float yaw;
            float zemin = NoktaBul("kapi", out dur, out yaw) ? dur.y : Origin.y;
            // kanat acilirken carpmasin diye tokmagin hemen disinda durulur
            Vector3 hedef = new Vector3(kol.x + 0.12f, zemin, kol.z - 0.5f);
            BagBitir(k);
            YatakBirak(k, a);
            pozDurum.Remove(k);
            slots[k] = hedef.x - Origin.x;
            if (aninda) { a.transform.position = hedef; Dondur(k, a, 0f, true); Mekan.KapiAc(ac); yield break; }
            yield return WalkToPos(k, a, hedef, false, true, 0f);
            if (a == null) yield break;
            var p = Kollar(a, id, satir);
            if (p != null)
                p.El(false, KolPozu.Katman.Gecici, () => kol, () => Dirsek(a, false));
            yield return new WaitForSeconds(0.55f);
            if (Mekan != null) Mekan.KapiAc(ac);
            yield return new WaitForSeconds(0.45f);
            if (p != null) p.Birak(KolPozu.Katman.Gecici);
        }

        // ---------- yatak (hastane / oda) ve ayaga kalkma ----------
        static bool KlipVar(Karakter3D a, string[] adaylar)
        {
            foreach (var n in adaylar) if (a.Find(n) != null) return true;
            return false;
        }

        // yatak: Karakter yat|otur  -> 3D mekanda yatak noktasina, 2D mekanda oldugu yerde
        void Yatak(string id, bool yat, string satir)
        {
            var a = HareketAktor(id, satir);
            if (a == null) return;
            string k = Util.Norm(id);
            BagBitir(k);
            YatakBirak(k, a);

            Vector3 nokta = a.transform.position;
            float yaw = 180f;
            bool yatakVar = Mekan != null && Mekan.NoktaBul("yatak", out nokta, out yaw);
            if (!yatakVar) { nokta = a.transform.position; yaw = 180f; }

            bool klipVar = KlipVar(a, yat ? Hareket3D.KlipYat : Hareket3D.KlipOtur);
            if (!klipVar && (!yat || KolPozu.InsanAnimator(a.gameObject) == null))
            {
                HareketUyar(satir + "'" + id + "' için " + (yat ? "yatma" : "oturma") + " animasyonu yok; atlandı.");
                return;
            }

            yatakOncesi[k] = a.transform.position;
            walking.Remove(k);
            Vector3 koltuk; float koltukYaw;
            if (!yat && Mekan != null && Mekan.NoktaBul("koltuk", out koltuk, out koltukYaw))
            {
                Play(a, Hareket3D.KlipOtur);
                // koltukta zaten oturan varsa yanina oturulur (ust uste binmesin)
                float yerX = koltuk.x;
                foreach (var aday in new[] { 0f, -0.55f, 0.55f })
                {
                    yerX = koltuk.x + aday;
                    bool dolu = false;
                    foreach (var kv in actors)
                        if (kv.Key != k && kv.Value != null && yataktaki.ContainsKey(kv.Key) && Mathf.Abs(kv.Value.transform.position.x - yerX) < 0.4f) { dolu = true; break; }
                    if (!dolu) break;
                }
                a.transform.position = new Vector3(yerX, Mathf.Max(Origin.y, koltuk.y - 0.45f * a.transform.localScale.y), koltuk.z - 0.12f);
                Dondur(k, a, OturmaYonu(k, yerX), true);
            }
            else if (!yat)
            {
                Play(a, Hareket3D.KlipOtur);
                // yatagin on kenarinda, kameraya donuk oturur (oturma klibinde kalca yerden ~0,45 m yukarida);
                // onunde duran biri varsa kenarda ondan en uzak yere
                if (yatakVar)
                {
                    float yerX = nokta.x, enUzak = -1f;
                    foreach (var aday in new[] { 0f, 0.55f, -0.55f })
                    {
                        float uzak = 9f;
                        foreach (var kv in actors)
                            if (kv.Key != k && kv.Value != null) uzak = Mathf.Min(uzak, Mathf.Abs(kv.Value.transform.position.x - (nokta.x + aday)));
                        if (uzak > enUzak + 0.05f) { enUzak = uzak; yerX = nokta.x + aday; }
                        if (uzak >= 0.6f) break;
                    }
                    a.transform.position = new Vector3(yerX, Mathf.Max(Origin.y, nokta.y - 0.45f * a.transform.localScale.y), nokta.z - 0.45f);
                }
                Dondur(k, a, OturmaYonu(k, a.transform.position.x), true);
            }
            else if (klipVar && KolPozu.InsanAnimator(a.gameObject) == null)
            {
                // insan modellerinde yatma klibi kullanilmaz: kok hareketi karakteri havada birakiyor
                Play(a, Hareket3D.KlipYat);
                if (yatakVar) a.transform.position = nokta;
                Dondur(k, a, yatakVar ? yaw : 180f, true);
            }
            else
            {
                // durgun pozdaki govde sirtustu yatirilir
                var bas = Kemik(a, HumanBodyBones.Head);
                float boy = bas != null ? Mathf.Clamp(Vector3.Distance(bas.position, a.transform.position) + 0.15f, 0.8f, 2.2f)
                                        : 1.65f * a.transform.localScale.y;
                Play(a, Hareket3D.KlipDur);
                sirtustu.Add(k);
                lockedFacing.Add(k);
                if (yatakVar)
                {
                    // bas yastikta (sagda), yuz yukari ve hafif kameraya
                    a.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0.85f, -0.5f), Vector3.right);
                    a.transform.position = new Vector3(nokta.x + 0.82f - boy, nokta.y + 0.12f, nokta.z);
                }
                else
                {
                    // 2D mekan: basi sola, yuzu hafif kameraya
                    Vector3 yuz = new Vector3(0f, 0.8f, -0.6f);
                    a.transform.rotation = Quaternion.LookRotation(yuz, Vector3.left);
                    a.transform.position = new Vector3(a.transform.position.x + boy * 0.5f, Origin.y + 0.3f, a.transform.position.z);
                }
            }
            yataktaki[k] = yat ? "yat" : "otur";
            pozDurum[k] = yat ? "yatiyor" : "oturuyor";
        }

        // Oturan karakter kameraya dumduz bakmaz: digerine dogru yan oturur
        // (etek fizigi olmadigi icin onden gorunum etegin altini gosteriyordu)
        float OturmaYonu(string k, float x)
        {
            float digerX = Origin.x;
            float enYakin = float.MaxValue;
            foreach (var kv in actors)
            {
                if (kv.Key == k || kv.Value == null) continue;
                float d = Mathf.Abs(kv.Value.transform.position.x - x);
                if (d < enYakin) { enYakin = d; digerX = kv.Value.transform.position.x; }
            }
            // cizgi mekanda ortada masa/koltuk yok: yandan degil hafif acili, yuzu gorunur
            if (!OdaAktif) return digerX <= x ? 200f : 160f;
            return digerX <= x ? 250f : 110f;
        }

        // yatak: kilidini ve sirtustu dondurmeyi kaldirir
        void YatakBirak(string k, Karakter3D a)
        {
            bool dondurulmus = sirtustu.Remove(k);
            bool vardi = yataktaki.Remove(k);
            if (vardi) pozDurum.Remove(k);
            Vector3 eski;
            bool eskiVar = yatakOncesi.TryGetValue(k, out eski);
            yatakOncesi.Remove(k);
            if (!vardi || a == null) return;
            if (eskiVar) a.transform.position = eski;
            if (dondurulmus) a.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            lockedFacing.Remove(k);
            yawTarget[k] = FacingYaw(k, a.transform.position.x - Origin.x);
        }

        // Konusma satiri yataktaki karakteri ayaga kaldirmasin
        bool YatakKilidi(string k, Karakter3D a, bool konusuyor)
        {
            string d;
            if (!yataktaki.TryGetValue(k, out d)) return false;
            if (d == "otur") Play(a, konusuyor ? Hareket3D.KlipOturKonus : Hareket3D.KlipOtur);
            return true;
        }

        // kalk: Karakter  -> oturdugu / yattigi yerden ayaga kalkar
        IEnumerator KalkCo(string id, string satir, bool aninda)
        {
            var a = HareketAktor(id, satir);
            if (a == null) yield break;
            string k = Util.Norm(id);
            BagBitir(k);
            bool yatiyordu = Yatiyor(k);
            bool yataktaydi = yataktaki.ContainsKey(k);
            bool dondurulmustu = sirtustu.Contains(k);

            Vector3 yanNokta = Vector3.zero;
            float yaw;
            bool yanVar = false;
            if (yataktaydi && Mekan != null && Mekan.NoktaBul("yatak", out yanNokta, out yaw))
            {
                yanNokta = new Vector3(yanNokta.x - 0.4f, Origin.y, yanNokta.z - 0.95f);
                yanVar = true;
            }
            // yatarken kalkis: once yatagin yanina alinir, sonra kalkma klibi oynar
            if (yatiyordu && yanVar) { yataktaki.Remove(k); yatakOncesi.Remove(k); sirtustu.Remove(k); a.transform.position = yanNokta; a.transform.rotation = Quaternion.Euler(0f, 180f, 0f); }
            else if (dondurulmustu) YatakBirak(k, a);

            if (!aninda && !dondurulmustu)
            {
                foreach (var n in yatiyordu ? Hareket3D.KlipKalkYatma : Hareket3D.KlipKalkOturma)
                {
                    var klip = a.Find(n);
                    if (klip == null) continue;
                    a.Play(klip, 0.25f);
                    yield return new WaitForSeconds(Mathf.Clamp(klip.length, 0.4f, 2.5f));
                    break;
                }
            }
            if (a == null) yield break;
            yataktaki.Remove(k);
            yatakOncesi.Remove(k);
            sirtustu.Remove(k);
            pozDurum.Remove(k);
            lockedFacing.Remove(k);
            Play(a, Hareket3D.KlipDur);
            if (yanVar && !yatiyordu)
            {
                if (aninda) a.transform.position = yanNokta;
                else yield return WalkToPos(k, a, yanNokta, false, false, float.NaN);
                if (a == null) yield break;
                Play(a, Hareket3D.KlipDur);
            }
            slots[k] = a.transform.position.x - Origin.x;
            yawTarget[k] = FacingYaw(k, slots[k]);
            ReFaceAll();
            if (!aninda) yield return new WaitForSeconds(0.35f);
        }

        // Karakter yer degistirirken / sahneden cikarken suren hareketleri bitirir
        void HareketBirak(string k, Karakter3D a)
        {
            BagBitir(k);
            YatakBirak(k, a);
            pozDurum.Remove(k);
        }

        // Yeni mekan: yatma/oturma, dokunma ve yurunen derinlik eski mekandan tasinmaz
        void MekanDegisiyor()
        {
            var sira = new List<KeyValuePair<string, Karakter3D>>();
            foreach (var kv in actors)
            {
                if (kv.Value == null) continue;
                BagBitir(kv.Key);
                if (yataktaki.ContainsKey(kv.Key))
                {
                    YatakBirak(kv.Key, kv.Value);
                    AnindaDur(kv.Value);
                }
                walking.Remove(kv.Key);
                if (!pozDurum.ContainsKey(kv.Key)) lockedFacing.Remove(kv.Key);
                sira.Add(kv);
            }
            // herkes on cizgiye, birbirine en az 0,7 m arayla
            sira.Sort((x, y) => x.Value.transform.position.x.CompareTo(y.Value.transform.position.x));
            var yerler = new float[sira.Count];
            for (int i = 0; i < sira.Count; i++)
            {
                yerler[i] = sira[i].Value.transform.position.x - Origin.x;
                if (i > 0) yerler[i] = Mathf.Max(yerler[i], yerler[i - 1] + 0.7f);
            }
            float tasma = sira.Count > 0 ? Mathf.Max(0f, yerler[sira.Count - 1] - 0.9f) : 0f;
            for (int i = 0; i < sira.Count; i++)
            {
                float x = yerler[i] - tasma;
                slots[sira[i].Key] = x;
                sira[i].Value.transform.position = new Vector3(Origin.x + x, Origin.y, Origin.z);
            }
        }

        // ---------- yatakta yatanin yorgani ----------
        // Sirtustu yatirilan govdede etek cadir gibi acik kaliyor: ayaktan goguse kadar govdeyi izleyen ortu serilir.
        // sirtustu listesi her karede izlenir; yatis nasil biterse bitsin (kalk, mekan degisimi, cikis) ortu kalkar.
        class Ortu { public GameObject obj; public Mesh mesh; public Karakter3D a; public Vector3 konum; public float basla, sonraki; }
        readonly Dictionary<string, Ortu> ortuler = new Dictionary<string, Ortu>();
        const int OrtuDilim = 28;
        Mesh ortuPisirme;
        Material cizgiOrtuMalzemesi;

        // ---------- cizgi mekandaki sofra ----------
        // Sofra 3D oyuncularin onune ciziliyor: kisa karakter (cocuk) masanin arkasinda kayboluyordu.
        // Basi masanin ustunde kalacak kadar yukseltilir (sandalyede oturuyormus gibi); ayaklar masanin arkasinda kalir.
        public float MasaUstu;                       // cizgi mekanda masanin ust yuzu (metre); 0 = masa yok
        readonly Dictionary<string, float> yukseltilen = new Dictionary<string, float>();
        const float MasaUstuBas = 0.45f;             // bas kemigi masanin en az bu kadar ustunde
        const float KisaKarakter = 1.15f;            // yalniz bundan kisa (bas kemigi) oyuncular yukseltilir

        void MasaYukseltme()
        {
            foreach (var kv in actors)
            {
                var a = kv.Value;
                if (a == null) continue;
                float eski;
                bool vardi = yukseltilen.TryGetValue(kv.Key, out eski);
                var p = a.transform.position;
                float fark = p.y - Origin.y;
                bool sahnede = Mathf.Abs(fark) < 1f;
                float yuk = 0f;
                string yd;
                bool yatiyor = yataktaki.TryGetValue(kv.Key, out yd) && yd == "yat";
                if (!OdaAktif && MasaUstu > 0f && sahnede && !yatiyor && !sirtustu.Contains(kv.Key)
                    && a.KafaYuksekligi > 0.1f && a.KafaYuksekligi < KisaKarakter)
                {
                    // o anki durusa gore: korkup comelince de basi masanin ustunde kalir
                    var bas = Kemik(a, HumanBodyBones.Head);
                    float basY = bas != null ? bas.position.y - p.y : a.KafaYuksekligi;
                    yuk = Mathf.Clamp(MasaUstu + MasaUstuBas - basY, 0.001f, 0.55f);
                    yuk = Mathf.MoveTowards(vardi ? eski : yuk, yuk, Time.deltaTime * 0.8f);
                }
                if (yuk > 0.001f)
                {
                    yukseltilen[kv.Key] = yuk;
                    if (Mathf.Abs(fark - yuk) > 0.0005f) a.transform.position = new Vector3(p.x, Origin.y + yuk, p.z);
                }
                else if (vardi)
                {
                    yukseltilen.Remove(kv.Key);
                    // baska bir komut konumu degistirdiyse dokunulmaz
                    if (Mathf.Abs(fark - eski) < 0.01f) a.transform.position = new Vector3(p.x, Origin.y, p.z);
                }
                // yukseltilmis cocuga yaklasan yetiskin onun yuksekligini devralmis olmasin
                else if (!OdaAktif && MasaUstu > 0f && !yataktaki.ContainsKey(kv.Key) && !sirtustu.Contains(kv.Key)
                         && fark > 0.001f && fark < 0.6f)
                    a.transform.position = new Vector3(p.x, Origin.y, p.z);
            }
        }

        void LateUpdate()
        {
            MasaYukseltme();
            Vector3 yatak = Vector3.zero;
            float yaw;
            bool yatakVar = Mekan != null && Mekan.Aktif && Mekan.NoktaBul("yatak", out yatak, out yaw);
            // cizgi mekanda (ve yatagi olmayan 3D mekanda) yatak noktasi yok: ortu yatanin altindan olculur

            List<string> sil = null;
            foreach (var kv in ortuler)
            {
                Karakter3D a;
                if (!sirtustu.Contains(kv.Key) || !actors.TryGetValue(kv.Key, out a) || a == null || a != kv.Value.a
                    || !a.gameObject.activeInHierarchy || kv.Value.obj == null)
                {
                    if (sil == null) sil = new List<string>();
                    sil.Add(kv.Key);
                }
            }
            if (sil != null) foreach (var k in sil) OrtuSil(k);
            foreach (var k in sirtustu)
            {
                Karakter3D a;
                if (!actors.TryGetValue(k, out a) || a == null || !a.gameObject.activeInHierarchy) continue;
                Ortu o;
                if (!ortuler.TryGetValue(k, out o))
                {
                    o = new Ortu { a = a, konum = a.transform.position, basla = Time.time };
                    o.obj = new GameObject("YatakOrtusu_" + k);
                    o.obj.transform.SetParent(transform, true);
                    o.mesh = new Mesh();
                    o.obj.AddComponent<MeshFilter>().sharedMesh = o.mesh;
                    var mr = o.obj.AddComponent<MeshRenderer>();
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    ortuler[k] = o;
                }
                else if (Vector3.Distance(o.konum, a.transform.position) > 0.01f)
                {
                    o.konum = a.transform.position;
                    o.basla = Time.time;
                    o.sonraki = 0f;
                }
                // durus ve etek kemikleri ilk saniyelerde oturur: o surede bicim yenilenir
                if (Time.time >= o.sonraki && Time.time - o.basla < 3f)
                {
                    // yatis konumu: kok, yatak yuzeyinin 0,12 m ustunde (Yatak komutu)
                    OrtuBicimle(o, yatakVar ? yatak : a.transform.position + new Vector3(0f, -0.12f, 0f));
                    o.sonraki = Time.time + 0.2f;
                }
            }
        }

        void OrtuSil(string k)
        {
            Ortu o;
            if (!ortuler.TryGetValue(k, out o)) return;
            ortuler.Remove(k);
            if (o.obj != null) Destroy(o.obj);
            if (o.mesh != null) Destroy(o.mesh);
        }

        void OrtuBicimle(Ortu o, Vector3 yatak)
        {
            var solAyak = Kemik(o.a, HumanBodyBones.LeftFoot);
            var sagAyak = Kemik(o.a, HumanBodyBones.RightFoot);
            var gogus = Kemik(o.a, HumanBodyBones.Chest);
            if (gogus == null) gogus = Kemik(o.a, HumanBodyBones.Spine);
            if (solAyak == null || sagAyak == null || gogus == null) return;
            // 3D yatakta bas sagda, cizgi mekanda solda
            float yon = gogus.position.x >= solAyak.position.x ? 1f : -1f;
            float ayak = yon > 0f ? Mathf.Min(solAyak.position.x, sagAyak.position.x) : Mathf.Max(solAyak.position.x, sagAyak.position.x);
            float uc1 = ayak - yon * 0.16f, uc2 = gogus.position.x + yon * 0.06f;
            float x0 = Mathf.Min(uc1, uc2), x1 = Mathf.Max(uc1, uc2);
            if (x1 - x0 < 0.3f) return;

            float taban = yatak.y + 0.08f;        // yataktaki sabit battaniyenin hemen ustu
            float tavan = yatak.y + 0.6f;
            float adim = (x1 - x0) / OrtuDilim;
            var ham = new float[OrtuDilim + 1];
            for (int i = 0; i <= OrtuDilim; i++) ham[i] = taban;

            if (ortuPisirme == null) ortuPisirme = new Mesh();
            int olculen = 0;
            foreach (var r in o.a.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!r.enabled || r.sharedMesh == null) continue;
                r.BakeMesh(ortuPisirme);
                var m = Matrix4x4.TRS(r.transform.position, r.transform.rotation, Vector3.one);
                foreach (var v in ortuPisirme.vertices)
                {
                    var d = m.MultiplyPoint3x4(v);
                    if (d.x < x0 - adim || d.x > x1 + adim || Mathf.Abs(d.z - yatak.z) > 0.36f) continue;
                    int i = Mathf.Clamp(Mathf.RoundToInt((d.x - x0) / adim), 0, OrtuDilim);
                    olculen++;
                    if (d.y + 0.03f > ham[i]) ham[i] = Mathf.Min(tavan, d.y + 0.03f);
                }
            }
            // govde olculemezse (mesh okunamiyorsa) yuksek duz ortu: hicbir sey acikta kalmasin
            if (olculen < 50)
                for (int i = 0; i <= OrtuDilim; i++) ham[i] = yatak.y + 0.45f;
            // komsunun en yuksegi + yumusatma: govde ortuden disari tasmasin
            var genis = new float[OrtuDilim + 1];
            var h = new float[OrtuDilim + 1];
            for (int i = 0; i <= OrtuDilim; i++)
                genis[i] = Mathf.Max(ham[i], Mathf.Max(ham[Mathf.Max(0, i - 1)], ham[Mathf.Min(OrtuDilim, i + 1)]));
            for (int i = 0; i <= OrtuDilim; i++)
                h[i] = Mathf.Max(ham[i], (genis[Mathf.Max(0, i - 1)] + genis[i] + genis[Mathf.Min(OrtuDilim, i + 1)]) / 3f);

            // kesit: ortada govde yuksekligi, kenarlarda yataga iner, en kenarda asagi sarkar
            Material malzeme;
            float yari;
            if (OdaAktif) Mekan.OrtuBilgisi(out malzeme, out yari);
            else
            {
                if (cizgiOrtuMalzemesi == null) cizgiOrtuMalzemesi = Mekan3D.DuzMalzeme(new Color(0.28f, 0.42f, 0.7f));
                malzeme = cizgiOrtuMalzemesi;
                yari = 0.46f;
            }
            o.obj.GetComponent<MeshRenderer>().sharedMaterial = malzeme;
            float[] kz = { -1f, -0.98f, -0.8f, -0.56f, -0.28f, 0f, 0.28f, 0.56f, 0.8f, 0.98f, 1f };
            float[] kw = { 0f, 0f, 0.45f, 1f, 1f, 1f, 1f, 1f, 0.45f, 0f, 0f };
            int n = kz.Length;

            var v3 = new List<Vector3>();
            var t = new List<int>();
            for (int i = 0; i <= OrtuDilim; i++)
                for (int j = 0; j < n; j++)
                {
                    float y = (j == 0 || j == n - 1) ? yatak.y - 0.1f : taban + (h[i] - taban) * kw[j];
                    v3.Add(o.obj.transform.InverseTransformPoint(new Vector3(x0 + adim * i, y, yatak.z + kz[j] * yari)));
                }
            for (int i = 0; i < OrtuDilim; i++)
                for (int j = 0; j < n - 1; j++)
                {
                    int p = i * n + j;
                    t.Add(p); t.Add(p + 1); t.Add(p + n);
                    t.Add(p + 1); t.Add(p + n + 1); t.Add(p + n);
                }
            // ayak ve gogus uclari kapali
            foreach (int i in new[] { 0, OrtuDilim })
            {
                int bas = v3.Count;
                for (int j = 1; j < n - 1; j++)
                {
                    v3.Add(v3[i * n + j]);
                    v3.Add(o.obj.transform.InverseTransformPoint(new Vector3(x0 + adim * i, yatak.y - 0.02f, yatak.z + kz[j] * yari)));
                }
                for (int j = 0; j < n - 3; j++)
                {
                    int p = bas + j * 2;
                    t.Add(p); t.Add(p + 1); t.Add(p + 2);
                    t.Add(p + 2); t.Add(p + 1); t.Add(p + 3);
                }
            }
            // iki yuzlu: ters sarimli kopya
            int adet = v3.Count, ucgen = t.Count;
            for (int i = 0; i < adet; i++) v3.Add(v3[i]);
            for (int i = 0; i < ucgen; i += 3) { t.Add(t[i] + adet); t.Add(t[i + 2] + adet); t.Add(t[i + 1] + adet); }

            o.mesh.Clear();
            o.mesh.SetVertices(v3);
            o.mesh.SetTriangles(t, 0);
            o.mesh.RecalculateNormals();
            o.mesh.RecalculateBounds();
        }

        void HareketTemizle()
        {
            baglar.Clear();
            pozDurum.Clear();
            yataktaki.Clear();
            yatakOncesi.Clear();
            sirtustu.Clear();
            yukseltilen.Clear();
        }
    }
}
