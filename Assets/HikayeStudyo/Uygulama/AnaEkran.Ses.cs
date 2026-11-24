using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 4: Gemini ses sistemi arayüzü (Ses ayarları sayfası + yazma ekranındaki "Ses" sekmesi)
    public partial class AnaEkran
    {
        static readonly string[,] GeminiSesleri =
        {
            { "Zephyr", "Parlak" }, { "Puck", "Neşeli" }, { "Charon", "Bilgilendirici" }, { "Kore", "Kararlı" },
            { "Fenrir", "Heyecanlı" }, { "Leda", "Genç" }, { "Orus", "Kararlı" }, { "Aoede", "Esintili" },
            { "Callirrhoe", "Rahat" }, { "Autonoe", "Parlak" }, { "Enceladus", "Nefesli" }, { "Iapetus", "Berrak" },
            { "Umbriel", "Rahat" }, { "Algieba", "Pürüzsüz" }, { "Despina", "Pürüzsüz" }, { "Erinome", "Berrak" },
            { "Algenib", "Kalın/Çatlak" }, { "Rasalgethi", "Bilgilendirici" }, { "Laomedeia", "Neşeli" }, { "Achernar", "Yumuşak" },
            { "Alnilam", "Kararlı" }, { "Schedar", "Dengeli" }, { "Gacrux", "Olgun" }, { "Pulcherrima", "Dik/İleri" },
            { "Achird", "Dostça" }, { "Zubenelgenubi", "Gündelik" }, { "Vindemiatrix", "Nazik" }, { "Sadachbia", "Canlı" },
            { "Sadaltager", "Bilgili" }, { "Sulafat", "Sıcak" }
        };

        float sesKaydirma, ySesListeKaydirma;
        bool ySes;
        string sesAnahtarGirdi;
        AudioSource dinleSrc;
        string sesMesaj = "";
        bool sesGitIstek;

        // ---- ses etiketleri ----
        static string SesEtiketi(string ad)
        {
            if (string.IsNullOrEmpty(ad)) return "";
            for (int i = 0; i < GeminiSesleri.GetLength(0); i++)
                if (string.Equals(GeminiSesleri[i, 0], ad, StringComparison.OrdinalIgnoreCase)) return GeminiSesleri[i, 0] + " · " + GeminiSesleri[i, 1];
            return ad;
        }

        static string SesAdi(string etiket)
        {
            if (string.IsNullOrEmpty(etiket)) return "";
            int k = etiket.IndexOf(" · ", StringComparison.Ordinal);
            return k > 0 ? etiket.Substring(0, k) : etiket;
        }

        static List<string> SesSecenekleri(string otomatikEtiket)
        {
            var l = new List<string> { otomatikEtiket };
            for (int i = 0; i < GeminiSesleri.GetLength(0); i++) l.Add(GeminiSesleri[i, 0] + " · " + GeminiSesleri[i, 1]);
            return l;
        }

        // Açılır listeyi kaydırma grubunun içinde kullanırken açılan kutunun konumu ekran koordinatına çevrilir
        string SecG(Rect r, Vector2 kayma, string id, IList<string> secenekler, string varsayilan, int boyut = 13)
        {
            string v = Sec(r, id, "", secenekler, varsayilan, boyut);
            if (acikId == id && acikRect == r) acikRect = new Rect(r.x + kayma.x, r.y + kayma.y, r.width, r.height);
            return v;
        }

        // =====================================================================
        // Ses ayarları sayfası
        // =====================================================================
        void SesCiz(Rect ic)
        {
            Baslik(ic, "Ses ayarları", "Seviyeler otomatik kaydedilir. Gemini seslendirme ve karakter sesleri burada.");
            GUI.enabled = acikId == null;
            var gorunum = new Rect(ic.x, ic.y + 72, ic.width, ic.height - 72);
            var adlar = SesKarakterleri();
            float icH = 560f + adlar.Count * 50f + 60f;
            ListeAl(ref sesKaydirma, gorunum, Mathf.Max(gorunum.height, icH));
            var kayma = new Vector2(gorunum.x, gorunum.y - sesKaydirma);
            float w = gorunum.width - 24f;
            var g = new Rect(0, 0, w, icH);
            float y = 0;

            Yazdir(new Rect(0, y, w, 28), "Ses seviyeleri", 18, Yazi, TextAnchor.MiddleLeft, true); y += 34;
            SesSatiriG(g, y, "Genel ses", "HS_SesGenel", 1f, true); y += 48;
            SesSatiriG(g, y, "Konuşma sesi", "HS_SesKonusma", 1f, false); y += 48;
            SesSatiriG(g, y, "Müzik ve ortam sesi", "HS_SesMuzik", 0.09f, false); y += 48;
            SesSatiriG(g, y, "Efekt sesleri", "HS_SesEfekt", 0.85f, false); y += 62;

            Yazdir(new Rect(0, y, w, 28), "Gemini seslendirme", 18, Yazi, TextAnchor.MiddleLeft, true); y += 34;
            // API anahtarı (gizli girdi)
            Yazdir(new Rect(0, y, 200, 38), "API anahtarı", 16, Yazi);
            if (sesAnahtarGirdi == null) sesAnahtarGirdi = GeminiUyg.Anahtar;
            var tf = new GUIStyle(YaziStil(15, TextAnchor.MiddleLeft, Yazi, false, false)) { padding = new RectOffset(12, 12, 0, 0) };
            var tr = new Rect(210, y, 420, 38);
            Kutu(tr, new Color32(34, 40, 58, 255));
            string yeniAnahtar = GUI.PasswordField(tr, sesAnahtarGirdi, '*', 120, tf);
            if (yeniAnahtar != sesAnahtarGirdi)
            {
                sesAnahtarGirdi = yeniAnahtar;
                GeminiUyg.Anahtar = yeniAnahtar;
                PlayerPrefs.Save();
            }
            bool var_ = GeminiUyg.Anahtar.Length > 0;
            Yazdir(new Rect(642, y, w - 642, 38), var_ ? "Kayıtlı (" + GeminiUyg.Anahtar.Length + " karakter)" : "Anahtar yok", 14, var_ ? Basari : Tehlike);
            y += 50;

            // model
            Yazdir(new Rect(0, y, 200, 34), "Ses modeli", 16, Yazi);
            int mi = Array.IndexOf(GeminiUyg.Modeller, GeminiUyg.Model);
            var etiketler = new List<string>(GeminiUyg.ModelEtiketleri);
            string mevcutEtiket = mi >= 0 ? GeminiUyg.ModelEtiketleri[mi] : GeminiUyg.Model;
            if (!yDeger.ContainsKey("ses_model")) yDeger["ses_model"] = mevcutEtiket;
            string sm = SecG(new Rect(210, y, 420, 34), kayma, "ses_model", etiketler, mevcutEtiket, 14);
            if (sm != mevcutEtiket)
            {
                int k = etiketler.IndexOf(sm);
                if (k >= 0) { GeminiUyg.Model = GeminiUyg.Modeller[k]; PlayerPrefs.Save(); }
            }
            y += 46;

            Yazdir(new Rect(0, y, w, 24), GeminiUyg.KotaMetni(), 13, GeminiUyg.KotaDolduMu() ? Tehlike : Sönük); y += 28;
            Yazdir(new Rect(0, y, w, 40), "Ses dosyaları: " + DisSes.Kok + "  (aynı cümle bir kez üretilir; metni değişen satır yeniden üretilir)", 12, Sönük, TextAnchor.UpperLeft, false, true);
            if (Dugme(new Rect(w - 160, y - 4, 160, 32), "Klasörü aç", new Color32(60, 66, 88, 255), 13)) { try { Directory.CreateDirectory(DisSes.Kok); } catch { } Ac("explorer.exe", "\"" + DisSes.Kok + "\""); }
            y += 46;

            bool deneme = PlayerPrefs.GetInt("HS_DenemeSes", 0) == 1;
            Yazdir(new Rect(0, y, 520, 34), "Deneme sesi kullan (Gemini yerine sentetik ses; yalnız süre/miks denemesi)", 14, Yazi);
            bool d2 = Anahtar(new Rect(w - 70, y + 3, 56, 28), deneme);
            if (d2 != deneme) { PlayerPrefs.SetInt("HS_DenemeSes", d2 ? 1 : 0); PlayerPrefs.Save(); }
            y += 56;

            // karakter sesleri
            Yazdir(new Rect(0, y, w, 28), "Karakter sesleri", 18, Yazi, TextAnchor.MiddleLeft, true); y += 30;
            Yazdir(new Rect(0, y, w, 22), "Bir karakterin sesini değiştirirsen yalnız onun satırları yeniden üretilir.", 13, Sönük); y += 30;
            Yazdir(new Rect(0, y, 200, 24), "Karakter", 12, Sönük);
            Yazdir(new Rect(210, y, 200, 24), "Ses", 12, Sönük);
            Yazdir(new Rect(w * 0.5f + 30, y, 200, 24), "İç ses", 12, Sönük); y += 26;
            foreach (var d in adlar)
            {
                Kutu(new Rect(0, y, w, 44), Kart);
                Yazdir(new Rect(12, y, 190, 44), d.id, 15, Yazi, TextAnchor.MiddleLeft, true);
                SesAtamaSecici(new Rect(210, y + 7, w * 0.5f - 220, 30), kayma, d, false);
                SesAtamaSecici(new Rect(w * 0.5f + 30, y + 7, w * 0.5f - 50, 30), kayma, d, true);
                y += 50;
            }
            Yazdir(new Rect(0, y + 6, w, 24), "Son ses raporu: " + (string.IsNullOrEmpty(StoryPlayer.SonSesRaporu) ? "(henüz oynatılmadı)" : StoryPlayer.SonSesRaporu), 12, Sönük, TextAnchor.UpperLeft, false, true);
            ListeVer(ref sesKaydirma, gorunum);
            GUI.enabled = true;
            AcilirCiz();
        }

        void SesSatiriG(Rect g, float y, string ad, string anahtar, float varsayilan, bool genel)
        {
            Yazdir(new Rect(0, y, 220, 36), ad, 16, Yazi);
            float v = PlayerPrefs.GetFloat(anahtar, varsayilan);
            float yeni = Kaydirgac(new Rect(230, y, 420, 36), v);
            Yazdir(new Rect(670, y, 80, 36), Mathf.RoundToInt(yeni * 100f) + "%", 15, Sönük);
            if (Mathf.Abs(yeni - v) > 0.001f)
            {
                PlayerPrefs.SetFloat(anahtar, yeni);
                if (genel) AudioListener.volume = yeni;
            }
        }

        List<CharacterDef> SesKarakterleri()
        {
            var l = new List<CharacterDef>();
            if (kutuphane == null) return l;
            l.Add(kutuphane.narrator);
            foreach (var c in kutuphane.characters) if (c != null) l.Add(c);
            return l;
        }

        void SesAtamaSecici(Rect r, Vector2 kayma, CharacterDef d, bool ic)
        {
            string varsayilanSes = ic ? (!string.IsNullOrEmpty(d.innerVoice) ? d.innerVoice : d.voice) : d.voice;
            string otomatik = "Otomatik (" + varsayilanSes + ")";
            string at = DisSes.Atama(d.id, ic);
            string mevcut = string.IsNullOrEmpty(at) ? otomatik : SesEtiketi(at);
            string id = "sesat_" + (ic ? "i_" : "d_") + d.id;
            if (!yDeger.ContainsKey(id)) yDeger[id] = mevcut;
            string sec = SecG(r, kayma, id, SesSecenekleri(otomatik), mevcut, 13);
            if (sec != mevcut)
            {
                DisSes.AtamaYaz(d.id, ic, sec == otomatik || sec.StartsWith("Otomatik (") ? "" : SesAdi(sec));
                PlayerPrefs.Save();
            }
        }

        // =====================================================================
        // Yazma ekranı: "Ses" sekmesi
        // =====================================================================
        string ySesMetin;
        List<StoryItem> ySesSatirlar = new List<StoryItem>();
        float ySesYenile = -10f;
        readonly Dictionary<string, float> ySesDurum = new Dictionary<string, float>();
        readonly HashSet<string> ySesDeneme = new HashSet<string>();
        bool ySesOncekiMesgul;

        void SesVerisiniYenile(bool zorla)
        {
            if (!ReferenceEquals(ySesMetin, yMetin) && ySesMetin != yMetin)
            {
                ySesMetin = yMetin;
                ySesSatirlar.Clear();
                try
                {
                    var pr = StoryParser.Parse(yMetin);
                    foreach (var it in pr.Items) if (it.Type == ItemType.Dialogue) ySesSatirlar.Add(it);
                }
                catch (Exception e) { Gunluk("Ses sekmesi ayrıştırma HATA: " + e.Message); }
                zorla = true;
            }
            float now = Time.realtimeSinceStartup;
            if (!zorla && now - ySesYenile < 0.6f && ySesOncekiMesgul == GeminiUyg.Busy) return;
            ySesOncekiMesgul = GeminiUyg.Busy;
            ySesYenile = now;
            ySesDurum.Clear(); ySesDeneme.Clear();
            if (kutuphane == null) return;
            foreach (var it in ySesSatirlar)
            {
                string k = kutuphane.ClipKey(it);
                if (ySesDurum.ContainsKey(k)) continue;
                string f = DisSes.Dosya(yH.ad, k);
                try
                {
                    if (File.Exists(f)) ySesDurum[k] = DisSes.WavSuresi(f);
                    else if (File.Exists(DisSes.DenemeDosya(yH.ad, k))) { ySesDeneme.Add(k); ySesDurum[k] = -2f; }
                    else ySesDurum[k] = -1f;
                }
                catch { ySesDurum[k] = -1f; }
            }
        }

        void SesSekmesiCiz(Rect a)
        {
            SesVerisiniYenile(false);
            int toplam = 0, hazir = 0;
            var gorulen = new HashSet<string>();
            foreach (var it in ySesSatirlar)
            {
                string k = kutuphane.ClipKey(it);
                if (!gorulen.Add(k)) continue;
                toplam++;
                if (ySesDurum.ContainsKey(k) && ySesDurum[k] >= 0f) hazir++;
            }
            int eksik = toplam - hazir;
            bool mesgul = GeminiUyg.Busy;
            bool anahtarVar = GeminiUyg.Anahtar.Length > 0;

            Yazdir(new Rect(a.x, a.y, 150, 32), hazir + " / " + toplam + " hazır", 15, eksik == 0 && toplam > 0 ? Basari : Yazi, TextAnchor.MiddleLeft, true);
            if (!mesgul)
            {
                if (Dugme(new Rect(a.x + 154, a.y, 190, 32), "Eksikleri üret (" + eksik + ")", Vurgu, 13, eksik > 0 && anahtarVar)) { sesMesaj = ""; GeminiUyg.EksikleriUret(yH.ad, kutuphane, ySesSatirlar); }
                if (Dugme(new Rect(a.x + 348, a.y, 150, 32), "Deneme sesi üret", new Color32(60, 66, 88, 255), 13, eksik > 0))
                {
                    int n = GeminiUyg.DenemeUret(yH.ad, kutuphane, ySesSatirlar);
                    sesMesaj = n + " deneme sesi yazıldı. Kullanmak için Ses ayarlarında 'Deneme sesi kullan'ı aç.";
                    ySesVerisiniZorla();
                }
            }
            else if (Dugme(new Rect(a.x + 154, a.y, 190, 32), "Durdur", Tehlike, 13)) GeminiUyg.Durdur();
            if (Dugme(new Rect(a.xMax - 120, a.y, 120, 32), "Ses ayarları", new Color32(60, 66, 88, 255), 13)) sesGitIstek = true;

            string durum = mesgul ? GeminiUyg.Status : (!string.IsNullOrEmpty(sesMesaj) ? sesMesaj : (!string.IsNullOrEmpty(GeminiUyg.Status) ? GeminiUyg.Status : ""));
            if (!anahtarVar && !mesgul) durum = "Gemini API anahtarı yok: sol menüde Ses > Gemini seslendirme bölümüne gir. " + durum;
            Yazdir(new Rect(a.x, a.y + 34, a.width, 40), GeminiUyg.KotaMetni() + (durum.Length > 0 ? "\n" + durum : ""), 12,
                mesgul ? new Color(0.94f, 0.75f, 0.31f) : Sönük, TextAnchor.UpperLeft, false, true);

            // liste
            const float sat = 58f;
            float ustte = 82f;
            var gorunum = new Rect(a.x, a.y + ustte, a.width, a.height - ustte - (GeminiUyg.Rapor.Count > 0 ? 54f : 0f));
            ListeAl(ref ySesListeKaydirma, gorunum, Mathf.Max(gorunum.height, ySesSatirlar.Count * sat));
            var kayma = new Vector2(gorunum.x, gorunum.y - ySesListeKaydirma);
            float w = gorunum.width - 20f;
            for (int i = 0; i < ySesSatirlar.Count; i++)
            {
                var it = ySesSatirlar[i];
                string k = kutuphane.ClipKey(it);
                float sure = ySesDurum.ContainsKey(k) ? ySesDurum[k] : -1f;
                var r = new Rect(0, i * sat, w, sat - 4f);
                Kutu(r, Kart);
                string kim = string.IsNullOrEmpty(it.Character) ? "Anlatıcı" : it.Character;
                Yazdir(new Rect(r.x + 10, r.y + 4, 56, 22), it.Line + ".", 13, Sönük, TextAnchor.MiddleLeft);
                Yazdir(new Rect(r.x + 56, r.y + 4, 250, 22), kim + (it.Inner ? " (iç ses)" : ""), 14, Yazi, TextAnchor.MiddleLeft, true);
                Yazdir(new Rect(r.x + 56, r.y + 28, 250, 22), kutuphane.VoiceFor(it) + " · " + Util.ToneFor(it), 11, Sönük, TextAnchor.MiddleLeft, false, false);
                string metin = it.Text ?? "";
                if (metin.Length > 70) metin = metin.Substring(0, 70) + "…";
                Yazdir(new Rect(r.x + 312, r.y + 4, w - 312 - 330, 46), "“" + metin + "”", 12, Yazi, TextAnchor.MiddleLeft, false, true);

                string etiket; Color renk;
                if (sure >= 0f) { etiket = "Hazır " + sure.ToString("0.0") + " sn"; renk = Basari; }
                else if (sure <= -1.5f) { etiket = "Deneme sesi"; renk = new Color(0.94f, 0.75f, 0.31f); }
                else { etiket = "Eksik"; renk = new Color(1f, 0.59f, 0.59f); }
                Yazdir(new Rect(w - 322, r.y + 4, 110, 46), etiket, 13, renk, TextAnchor.MiddleLeft, true);
                bool dinlenir = sure >= 0f || sure <= -1.5f;
                if (Dugme(new Rect(w - 206, r.y + 10, 62, 34), "Dinle", new Color32(60, 66, 88, 255), 12, dinlenir)) Dinle(k);
                if (Dugme(new Rect(w - 138, r.y + 10, 128, 34), sure >= 0f ? "Yeniden üret" : "Üret", Vurgu, 12, !mesgul && anahtarVar))
                {
                    sesMesaj = "";
                    GeminiUyg.YenidenUret(yH.ad, kutuphane, it);
                }
            }
            ListeVer(ref ySesListeKaydirma, gorunum);

            if (GeminiUyg.Rapor.Count > 0)
            {
                var rr = new Rect(a.x, gorunum.yMax + 4, a.width, 50);
                Kutu(rr, Kart);
                Yazdir(new Rect(rr.x + 10, rr.y, rr.width - 20, rr.height), string.Join("\n", GeminiUyg.Rapor.GetRange(0, Mathf.Min(2, GeminiUyg.Rapor.Count)).ToArray()), 12, new Color(0.94f, 0.75f, 0.31f), TextAnchor.MiddleLeft, false, true);
            }
        }

        void ySesVerisiniZorla() { SesVerisiniYenile(true); }

        void Dinle(string anahtar)
        {
            try
            {
                var clip = DisSes.Yukle(yH.ad, anahtar) ?? DisSes.YukleDeneme(yH.ad, anahtar);
                if (clip == null) { sesMesaj = "Ses dosyası okunamadı."; return; }
                if (dinleSrc == null) { dinleSrc = gameObject.AddComponent<AudioSource>(); dinleSrc.playOnAwake = false; }
                dinleSrc.Stop();
                dinleSrc.PlayOneShot(clip, PlayerPrefs.GetFloat("HS_SesKonusma", 1f));
            }
            catch (Exception e) { sesMesaj = "Dinleme hatası: " + e.Message; }
        }
    }
}
