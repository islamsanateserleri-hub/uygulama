using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Sahnedeki 3D karakterlerin ortak koordinatoru: kim konusuyor, kim dinliyor, kim kime bakiyor.
    // StoryPlayer sadece birkac satirla bunu cagirir:
    //   YuzSahne.Konus(def.id, clip, dur, it.Emotion, it.Inner);   // satir basinda
    //   YuzSahne.Sustur();                                        // satir sonunda
    //   YuzSahne.Bak(kim, hedef) / YuzSahne.Dinle(kim, mod)       // bak: ve dinle: komutlari
    // Hicbiri hata firlatmaz; 3D karakter yoksa hicbir sey yapmaz.
    public static class YuzSahne
    {
        public static string SonKonusan = "";

        // Sahnedeki tum (aktif) 3D karakterler. Her cagrida taze liste (domain reload kapali olsa da eskimez).
        public static List<YuzCanlilik> Hepsi()
        {
            var list = new List<YuzCanlilik>();
            try
            {
                var ks = Object.FindObjectsByType<Karakter3D>(FindObjectsSortMode.None);
                for (int i = 0; i < ks.Length; i++)
                {
                    if (ks[i] == null) continue;
                    var y = YuzCanlilik.Al(ks[i].gameObject);
                    if (y != null) list.Add(y);
                }
            }
            catch (System.Exception) { }
            return list;
        }

        public static YuzCanlilik Bul(string id)
        {
            string k = Util.Norm(id);
            if (k.Length == 0) return null;
            var hepsi = Hepsi();
            for (int i = 0; i < hepsi.Count; i++) if (hepsi[i].Kimlik == k) return hepsi[i];
            return null;
        }

        public static YuzCanlilik EnYakin(YuzCanlilik ben)
        {
            if (ben == null) return null;
            YuzCanlilik en = null;
            float best = float.MaxValue;
            var hepsi = Hepsi();
            for (int i = 0; i < hepsi.Count; i++)
            {
                if (hepsi[i] == ben) continue;
                float d = (hepsi[i].transform.position - ben.transform.position).sqrMagnitude;
                if (d < best) { best = d; en = hepsi[i]; }
            }
            return en;
        }

        // Satir basladi: konusan agiz oynatir, digerleri dinler (duyguya gore basla onay / yere bakis)
        public static void Konus(string id, AudioClip clip, float sure, string duygu, bool icSes)
        {
            try
            {
                string key = Util.Norm(id);
                SonKonusan = key;
                var hepsi = Hepsi();
                if (hepsi.Count == 0) return;
                YuzCanlilik konusan = null;
                for (int i = 0; i < hepsi.Count; i++) if (hepsi[i].Kimlik == key) { konusan = hepsi[i]; break; }

                // muhatap: konusana en yakin diger karakter
                YuzCanlilik muhatap = null;
                float best = float.MaxValue;
                for (int i = 0; i < hepsi.Count; i++)
                {
                    if (hepsi[i] == konusan) continue;
                    float d = konusan != null ? (hepsi[i].transform.position - konusan.transform.position).sqrMagnitude : 0f;
                    if (d < best) { best = d; muhatap = hepsi[i]; }
                }
                if (konusan != null) konusan.KonusBasla(clip, sure, duygu, icSes, muhatap);
                for (int i = 0; i < hepsi.Count; i++)
                {
                    if (hepsi[i] == konusan) continue;
                    hepsi[i].DinleBasla(konusan, duygu, icSes);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] YuzSahne.Konus: " + e.Message);
            }
        }

        // Satir bitti
        public static void Sustur()
        {
            try
            {
                var hepsi = Hepsi();
                for (int i = 0; i < hepsi.Count; i++) { hepsi[i].KonusBitir(); hepsi[i].DinlemeBitir(); }
            }
            catch (System.Exception) { }
        }

        // bak: Anne  |  bak: yere  |  bak: Arda > Anne  |  bak: Arda > yere  |  bak: serbest
        // kim bos ise son konusan karakter bakar.
        public static void Bak(string kim, string hedef)
        {
            try
            {
                string k = string.IsNullOrEmpty(kim) ? SonKonusan : kim;
                var ozne = Bul(k);
                if (ozne == null) { Debug.LogWarning("[Hikaye] bak: 3D karakter bulunamadı: " + (string.IsNullOrEmpty(k) ? "(son konuşan yok)" : k)); return; }
                string h = Util.Norm(hedef);
                if (h == "serbest" || h == "bitir" || h == "sifirla" || h == "otomatik") { ozne.BakKur(YuzCanlilik.Bakis.Serbest, null); return; }
                if (h == "yere" || h == "asagi" || h == "yer") { ozne.BakKur(YuzCanlilik.Bakis.Asagi, null); return; }
                if (h == "kamera" || h == "ileri" || h == "bize") { ozne.BakKur(YuzCanlilik.Bakis.Kamera, null); return; }
                var h2 = Bul(h);
                if (h2 == null || h2 == ozne) { Debug.LogWarning("[Hikaye] bak: hedef karakter bulunamadı: " + hedef); return; }
                ozne.BakKur(YuzCanlilik.Bakis.Hedef, h2);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] YuzSahne.Bak: " + e.Message);
            }
        }

        // dinle: Anne | onay   (mod: onay, supheli, normal). kim bos ise sahnedeki herkes.
        public static void Dinle(string kim, string mod)
        {
            try
            {
                if (string.IsNullOrEmpty(kim))
                {
                    var hepsi = Hepsi();
                    for (int i = 0; i < hepsi.Count; i++) hepsi[i].DinleModuKur(mod);
                    return;
                }
                var y = Bul(kim);
                if (y == null) { Debug.LogWarning("[Hikaye] dinle: 3D karakter bulunamadı: " + kim); return; }
                y.DinleModuKur(mod);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Hikaye] YuzSahne.Dinle: " + e.Message);
            }
        }

        // StoryParser'dan cagrilir: bak: / dinle: / altyazi: komutlarini cozer.
        // item.Type'i parser (patch) ayarlar; burada Character ve Arg doldurulur. false = satiri atla.
        public static bool KomutCoz(string key, StoryItem item, List<string> warnings, int satir)
        {
            string val = (item.Arg ?? "").Trim().Replace("->", ">").Replace("→", ">");
            if (key == "bak")
            {
                string kim = "", hedef = val;
                int gt = val.IndexOf('>');
                if (gt >= 0) { kim = val.Substring(0, gt).Trim(); hedef = val.Substring(gt + 1).Trim(); }
                if (hedef.Length == 0)
                {
                    warnings.Add(satir + ". satır: 'bak: Karakter', 'bak: yere' ya da 'bak: Kim > Hedef' biçiminde yaz.");
                    return false;
                }
                item.Character = kim;
                item.Arg = hedef;
                item.Text = val;
                return true;
            }
            if (key == "dinle")
            {
                var parts = val.Split('|');
                string kim = parts[0].Trim();
                string mod = parts.Length > 1 ? Util.Norm(parts[1]) : "normal";
                if (mod.Length == 0 || mod == "yok" || mod == "dogal") mod = "normal";
                if (mod != "normal" && mod != "onay" && mod != "supheli")
                {
                    warnings.Add(satir + ". satır: dinleme tarzı '" + parts[1].Trim() + "' bilinmiyor (onay, supheli, normal).");
                    mod = "normal";
                }
                item.Character = kim;
                item.Arg = mod;
                return true;
            }
            if (key == "altyazi")
            {
                string stil = KaraokeAltyazi.StilAdi(val);
                if (stil.Length == 0)
                {
                    warnings.Add(satir + ". satır: altyazı stili '" + val + "' bilinmiyor (karaoke, tek_kelime, klasik).");
                    return false;
                }
                item.Arg = stil;
                return true;
            }
            return false;
        }
    }
}
