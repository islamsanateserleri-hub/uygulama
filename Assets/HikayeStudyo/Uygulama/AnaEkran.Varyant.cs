using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // "Karakterlerim > Çeşit karakterler": ~1000 çeşidi arama ve sayfalama ile gösterir.
    // Küçük resim yalnız ekrandaki sayfa için, teker teker üretilir; sayfa / sekme değişince silinir.
    public partial class AnaEkran
    {
        static readonly string[] CesitGrupAdi = { "Tümü", "Erkek", "Kadın", "Anime", "Mixamo" };
        static readonly string[] CesitGrupOnek = { "", "Erkek_", "Kadin_", "VRM_", "Mixamo_" };
        static readonly Vector3 CesitMerkez = new Vector3(0f, 5000f, 0f);   // sahnelerden uzakta

        List<VaryantBilgi> cesitler;
        string[] cesitAranan;                       // sadeleştirilmiş ad + açıklama
        readonly List<VaryantBilgi> cesitSuzulen = new List<VaryantBilgi>();
        string cesitArama = "", cesitSuzAnahtar;
        int cesitGrup, cesitSayfa;
        KarakterKimligi cesitSecimKarti;            // doluysa: kimlik kartı için görünüm seçiliyor

        readonly Dictionary<string, RenderTexture> cesitResim = new Dictionary<string, RenderTexture>();
        Coroutine cesitRutin;
        GameObject cesitStudyo, cesitModel;
        Camera cesitKamera;
        Light cesitIsik;
        RenderTexture cesitCizilen;

        void CesitListeHazirla()
        {
            if (cesitler != null) return;
            try { cesitler = Varyantlar.Liste(Varyantlar.TumPrefablar(katalog)); }
            catch (Exception e) { cesitler = new List<VaryantBilgi>(); Gunluk("Çeşit listesi HATA: " + e); }
            cesitAranan = new string[cesitler.Count];
            for (int i = 0; i < cesitler.Count; i++) cesitAranan[i] = Util.Norm(cesitler[i].ad + " " + cesitler[i].aciklama);
            cesitSuzAnahtar = null;
        }

        void CesitSuz()
        {
            string anahtar = cesitGrup + "|" + cesitArama;
            if (anahtar == cesitSuzAnahtar) return;
            cesitSuzAnahtar = anahtar;
            cesitSayfa = 0;
            cesitSuzulen.Clear();
            var sozcukler = new List<string>();
            foreach (var s in cesitArama.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) sozcukler.Add(Util.Norm(s));
            for (int i = 0; i < cesitler.Count; i++)
            {
                if (cesitGrup > 0 && !cesitler[i].ad.StartsWith(CesitGrupOnek[cesitGrup], StringComparison.Ordinal)) continue;
                bool uyar = true;
                foreach (var s in sozcukler) if (cesitAranan[i].IndexOf(s, StringComparison.Ordinal) < 0) { uyar = false; break; }
                if (uyar) cesitSuzulen.Add(cesitler[i]);
            }
        }

        void CesitCiz(Rect r)
        {
            CesitListeHazirla();
            var gri = new Color32(60, 66, 88, 255);

            var alan = new Rect(r.x, r.y, 240, 36);
            Kutu(alan, Kart);
            var tf = new GUIStyle(YaziStil(15, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(12, 12, 0, 0), clipping = TextClipping.Clip };
            cesitArama = GUI.TextField(alan, cesitArama, 40, tf);
            if (cesitArama.Length == 0) Yazdir(new Rect(alan.x + 12, alan.y, alan.width - 24, alan.height), "Ara: ad ya da renk...", 14, Sönük);
            for (int i = 0; i < CesitGrupAdi.Length; i++)
                if (Dugme(new Rect(r.x + 250 + i * 92, r.y, 86, 36), CesitGrupAdi[i], cesitGrup == i ? Vurgu : (Color)gri, 13)) cesitGrup = i;
            if (cesitSecimKarti != null && Dugme(new Rect(r.xMax - 120, r.y, 120, 36), "Vazgeç", gri, 14))
            {
                kimlikForm = cesitSecimKarti; cesitSecimKarti = null; karakterSekme = 2; mesaj = "";
                return;
            }

            CesitSuz();
            if (cesitSuzulen.Count == 0)
            {
                Yazdir(new Rect(r.x, r.y + 50, r.width, 30), "Gösterilecek karakter yok.", 15, Sönük);
                return;
            }

            const float aralik = 12f;
            var g = new Rect(r.x, r.y + 46, r.width, r.height - 46 - 46);
            int sutun = Mathf.Max(2, Mathf.FloorToInt(g.width / 300f));
            int satir = Mathf.Max(1, Mathf.FloorToInt((g.height + aralik) / (170f + aralik)));
            int sayfaBoyu = sutun * satir;
            int sayfaSayisi = (cesitSuzulen.Count + sayfaBoyu - 1) / sayfaBoyu;
            cesitSayfa = Mathf.Clamp(cesitSayfa, 0, sayfaSayisi - 1);
            float w = (g.width - aralik * (sutun - 1)) / sutun;
            float h = Mathf.Min(260f, (g.height - aralik * (satir - 1)) / satir);

            var sayfa = new List<VaryantBilgi>();
            for (int i = cesitSayfa * sayfaBoyu; i < cesitSuzulen.Count && sayfa.Count < sayfaBoyu; i++) sayfa.Add(cesitSuzulen[i]);

            VaryantBilgi secilen = null;
            for (int i = 0; i < sayfa.Count; i++)
            {
                var v = sayfa[i];
                var k = new Rect(g.x + (i % sutun) * (w + aralik), g.y + (i / sutun) * (h + aralik), w, h);
                Kutu(k, Kart);
                var resim = new Rect(k.x + 8, k.y + 8, (h - 16) * 2f / 3f, h - 16);
                RenderTexture rt;
                if (cesitResim.TryGetValue(v.ad, out rt) && rt != null) GUI.DrawTexture(resim, rt, ScaleMode.ScaleToFit);
                else Yazdir(resim, cesitResim.ContainsKey(v.ad) ? "resim yok" : "...", 13, Sönük, TextAnchor.MiddleCenter);
                float x = resim.xMax + 10, tw = k.xMax - x - 8;
                Yazdir(new Rect(x, k.y + 8, tw, 22), v.ad, 12, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(x, k.y + 34, tw, h - 80), v.aciklama.Length > 0 ? v.aciklama.Replace(" · ", "\n") : "kendi renkleri", 12, Sönük, TextAnchor.UpperLeft, false, true);
                if (Dugme(new Rect(x, k.yMax - 40, Mathf.Min(tw, 140), 32), cesitSecimKarti != null ? "Bunu seç" : "Kopyala", Vurgu, 13)) secilen = v;
            }

            float py = r.yMax - 38;
            if (Dugme(new Rect(r.x, py, 44, 34), "<<", gri, 14, cesitSayfa > 0)) cesitSayfa = 0;
            if (Dugme(new Rect(r.x + 50, py, 80, 34), "Önceki", gri, 13, cesitSayfa > 0)) cesitSayfa--;
            Yazdir(new Rect(r.x + 136, py, 130, 34), (cesitSayfa + 1) + " / " + sayfaSayisi, 14, Yazi, TextAnchor.MiddleCenter, true);
            if (Dugme(new Rect(r.x + 272, py, 80, 34), "Sonraki", gri, 13, cesitSayfa < sayfaSayisi - 1)) cesitSayfa++;
            if (Dugme(new Rect(r.x + 358, py, 44, 34), ">>", gri, 14, cesitSayfa < sayfaSayisi - 1)) cesitSayfa = sayfaSayisi - 1;
            if (string.IsNullOrEmpty(mesaj)) Yazdir(new Rect(r.x + 414, py, r.width - 414, 34), cesitSuzulen.Count + " karakter", 13, Sönük);
            else MesajCiz(new Rect(r.x + 414, py - 2, r.width - 414, 38));

            if (Event.current.type == EventType.Repaint) CesitResimIste(sayfa);

            if (secilen == null) return;
            if (cesitSecimKarti != null)
            {
                kimlikForm = cesitSecimKarti; cesitSecimKarti = null;
                kimlikForm.gorunum = secilen.ad;
                yDeger.Remove("kk_gorunum");
                karakterSekme = 2;
                mesaj = "Görünüm seçildi: " + secilen.ad + ". Kaydet'e basmayı unutma."; mesajHata = false;
            }
            else
            {
                GUIUtility.systemCopyBuffer = "oyuncu: Karakter = " + secilen.ad;
                mesaj = "Kopyalandı. Hikâyene yapıştır, \"Karakter\" yerine karakterin adını yaz."; mesajHata = false;
            }
        }

        // ---------- küçük resimler ----------
        void CesitResimIste(List<VaryantBilgi> sayfa)
        {
            var eski = new List<string>();
            foreach (var ad in cesitResim.Keys) if (!sayfa.Exists(v => v.ad == ad)) eski.Add(ad);
            foreach (var ad in eski) { CesitResimSil(cesitResim[ad]); cesitResim.Remove(ad); }
            if (cesitRutin != null) return;
            foreach (var v in sayfa)
                if (!cesitResim.ContainsKey(v.ad)) { cesitRutin = StartCoroutine(CesitResimRutini(v)); return; }
        }

        IEnumerator CesitResimRutini(VaryantBilgi v)
        {
            yield return null;
            bool hazir = false;
            try
            {
                CesitStudyoKur();
                cesitModel = Instantiate(v.taban, CesitMerkez, Quaternion.identity);
                var k = cesitModel.GetComponent<Karakter3D>();
                if (k != null) k.canliLik = false;   // resimde göz kırpma olmasın
                var g = cesitModel.AddComponent<VaryantGorunum>();
                g.Kur(v.taban.name, v.no);
                hazir = true;
            }
            catch (Exception e) { Gunluk("Çeşit resmi HATA (" + v.ad + "): " + e.Message); }
            if (!hazir)
            {
                if (cesitModel != null) Destroy(cesitModel);
                cesitModel = null;
                cesitResim[v.ad] = null;
                cesitRutin = null;
                yield break;
            }

            // Start çalışsın, duruş animasyonu otursun
            yield return null;
            yield return null;

            if (cesitModel != null && cesitKamera != null)
            {
                cesitCizilen = new RenderTexture(176, 264, 24, RenderTextureFormat.ARGB32) { name = "Cesit_" + v.ad, antiAliasing = 2 };
                float boy = 1.6f;
                var an = cesitModel.GetComponentInChildren<Animator>();
                var bas = an != null && an.isHuman ? an.GetBoneTransform(HumanBodyBones.Head) : null;
                if (bas != null) boy = Mathf.Max(0.6f, bas.position.y - CesitMerkez.y);
                float gorus = boy * 1.4f;
                float uzaklik = gorus * 0.5f / Mathf.Tan(cesitKamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                cesitKamera.transform.position = CesitMerkez + new Vector3(0f, boy * 0.62f, uzaklik);
                cesitKamera.targetTexture = cesitCizilen;
                cesitKamera.enabled = true;
                cesitIsik.enabled = true;
                yield return new WaitForEndOfFrame();
                if (cesitKamera != null) { cesitKamera.enabled = false; cesitKamera.targetTexture = null; }
                if (cesitIsik != null) cesitIsik.enabled = false;
                cesitResim[v.ad] = cesitCizilen;
                cesitCizilen = null;
            }
            if (cesitModel != null) Destroy(cesitModel);
            cesitModel = null;
            cesitRutin = null;
        }

        void CesitStudyoKur()
        {
            if (cesitStudyo != null) return;
            cesitStudyo = new GameObject("CesitStudyosu");
            var kg = new GameObject("Kamera");
            kg.transform.SetParent(cesitStudyo.transform, false);
            kg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // modeller +Z'ye bakar
            cesitKamera = kg.AddComponent<Camera>();
            cesitKamera.enabled = false;
            cesitKamera.fieldOfView = 30f;
            cesitKamera.nearClipPlane = 0.1f; cesitKamera.farClipPlane = 30f;
            cesitKamera.clearFlags = CameraClearFlags.SolidColor;
            cesitKamera.backgroundColor = new Color(0.3f, 0.33f, 0.42f);
            cesitKamera.allowHDR = false;
            var ig = new GameObject("Isik");
            ig.transform.SetParent(cesitStudyo.transform, false);
            ig.transform.rotation = Quaternion.Euler(35f, 160f, 0f);
            cesitIsik = ig.AddComponent<Light>();
            cesitIsik.type = LightType.Directional;
            cesitIsik.intensity = 1.2f;
            cesitIsik.enabled = false;
        }

        void CesitResimSil(RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            Destroy(rt);
        }

        void CesitBirak()
        {
            if (cesitRutin != null) { StopCoroutine(cesitRutin); cesitRutin = null; }
            if (cesitModel != null) { Destroy(cesitModel); cesitModel = null; }
            if (cesitStudyo != null) { Destroy(cesitStudyo); cesitStudyo = null; }
            CesitResimSil(cesitCizilen); cesitCizilen = null;
            foreach (var rt in cesitResim.Values) CesitResimSil(rt);
            cesitResim.Clear();
        }

        // Sekmeden / bölümden çıkılınca (ya da hikâye oynatılınca) resim stüdyosu kapanır
        void LateUpdate()
        {
            bool acik = !oynuyor && !yazmaAcik && bolum == Bolum.Karakterlerim && karakterSekme == 3;
            if (acik) return;
            cesitSecimKarti = null;
            if (cesitStudyo != null || cesitResim.Count > 0) CesitBirak();
        }
    }
}
