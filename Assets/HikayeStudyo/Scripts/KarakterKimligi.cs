using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 8: karakter kimlik kartı. Kart yalnız kullanıcı kaydedince oluşur ve yalnız
    // kullanıcı değiştirince değişir; böylece karakter bütün bölümlerde aynı görünür.
    [Serializable]
    public class KarakterKimligi
    {
        public const string VarsayilanGorunum = "*";   // kütüphanedeki görünüm aynen kalsın
        public const string CizgiGorunum = "";         // çizgi karakter (3D yok)
        public const float YetiskinBoyCm = 175f;       // ölçek 1.0

        public string ad = "";
        public int yas;                    // 0 = belirtilmedi
        public int boyCm = 175;
        public string gorunum = "";        // 3D katalog kimliği, "" = çizgi, "*" = varsayılan
        public int sacModeli;              // (int)HairStyle
        public bool sakal;
        public string tenRengi = "#FFD1AD", sacRengi = "#38241A", ustRengi = "#4D80CC", altRengi = "#404052";
        public string altyaziRengi = "#FFFFFF";
        public int yer;                    // -1 sol, 0 orta, 1 sağ
        public string ses = "Puck";
        public string icSes = "";          // "" = konuşma sesiyle aynı
        public string konusmaTarzi = "";   // seslendirme tonuna eklenir
        public string yuz = "", kiyafet = "", fiziksel = "", duygusal = "";   // not

        public static int Boy(float olcek) { return Mathf.RoundToInt(olcek * YetiskinBoyCm); }

        public static string Renk(Color c) { return "#" + ColorUtility.ToHtmlStringRGB(c); }

        public static Color RenkOku(string s, Color yedek)
        {
            Color c;
            return !string.IsNullOrEmpty(s) && ColorUtility.TryParseHtmlString(s, out c) ? c : yedek;
        }

        // Karakter adı hikâyede [Ad|...], "poz: Ad ..." ve "oyuncu: Ad = ..." içinde geçer: boşluksuz olmalı
        public static string AdHatasi(string ad)
        {
            if (string.IsNullOrEmpty(ad)) return "Bir ad yaz.";
            if (ad.Length > 24) return "Ad en çok 24 harf olabilir.";
            foreach (var ch in ad)
                if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '-') return "Adda yalnız harf, rakam, _ ve - olabilir (boşluk olmaz).";
            var n = Util.Norm(ad);
            if (n == "anlatici") return "Anlatıcı için kart oluşturulamaz.";
            return null;
        }

        // Mevcut bir kütüphane karakterinden kart: hiç değiştirmeden kaydedilirse görünüm ve ses aynı kalır
        public static KarakterKimligi Karakterden(CharacterDef d, KarakterKatalogu katalog)
        {
            var k = new KarakterKimligi
            {
                ad = d.id,
                boyCm = Boy(d.scale),
                gorunum = CizgiGorunum,
                sacModeli = (int)d.hairStyle,
                sakal = d.beard,
                tenRengi = Renk(d.skin), sacRengi = Renk(d.hair), ustRengi = Renk(d.shirt), altRengi = Renk(d.pants),
                altyaziRengi = Renk(d.subtitleColor),
                yer = Mathf.Clamp(d.side, -1, 1),
                ses = d.voice,
                icSes = d.innerVoice ?? "",
                konusmaTarzi = d.konusmaTarzi ?? ""
            };
            if (d.model3D != null)
            {
                k.gorunum = VarsayilanGorunum;
                if (katalog != null)
                    foreach (var kayit in katalog.karakterler)
                        if (kayit != null && kayit.prefab == d.model3D) { k.gorunum = kayit.kimlik; break; }
            }
            return k;
        }

        // Kartı karakter tanımına işler (kütüphanenin kopyası üzerinde çağrılır)
        public void Isle(CharacterDef d, KarakterKatalogu katalog)
        {
            // boy değişmediyse ölçek aynen kalır (yuvarlama görünümü oynatmasın)
            if (boyCm > 0 && boyCm != Boy(d.scale)) d.scale = Mathf.Clamp(boyCm / YetiskinBoyCm, 0.4f, 1.3f);
            if (string.IsNullOrEmpty(gorunum)) d.model3D = null;
            else if (gorunum != VarsayilanGorunum)
            {
                var pf = katalog != null ? katalog.KarakterPrefab(gorunum) : null;
                if (pf != null) d.model3D = pf;
                else Debug.LogWarning("[Kimlik] " + ad + ": '" + gorunum + "' 3D modeli bulunamadı, varsayılan görünüm kullanılıyor.");
            }
            d.hairStyle = (HairStyle)Mathf.Clamp(sacModeli, 0, (int)HairStyle.Kel);
            d.beard = sakal;
            d.skin = RenkOku(tenRengi, d.skin);
            d.hair = RenkOku(sacRengi, d.hair);
            d.shirt = RenkOku(ustRengi, d.shirt);
            d.pants = RenkOku(altRengi, d.pants);
            d.subtitleColor = RenkOku(altyaziRengi, d.subtitleColor);
            d.side = Mathf.Clamp(yer, -1, 1);
            if (!string.IsNullOrEmpty(ses)) d.voice = ses;
            d.innerVoice = icSes ?? "";
            d.konusmaTarzi = (konusmaTarzi ?? "").Trim();
        }
    }

    // Kartlar: Documents\HikayeStudyosu\Karakterler\<ad>.json
    public static class KimlikDeposu
    {
        // Kartlar uygulamanın özelliğidir. Editör araçları (köprü komutları, Stüdyo penceresi) varsayılan
        // kütüphaneyle çalışır; Editör'de yalnız uygulama ana ekranı açılınca etkinleşir.
#if UNITY_EDITOR
        public static bool Etkin = false;
#else
        public static bool Etkin = true;
#endif
        public static string KlasorDegistir;   // yalnız deneme için

        public static string Klasor
        {
            get
            {
                if (!string.IsNullOrEmpty(KlasorDegistir)) return KlasorDegistir;
                return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HikayeStudyosu"), "Karakterler");
            }
        }

        static string Yol(string ad) { return Path.Combine(Klasor, Util.Norm(ad) + ".json"); }

        public static bool Var(string ad) { return KarakterKimligi.AdHatasi(ad) == null && File.Exists(Yol(ad)); }

        public static KarakterKimligi Oku(string ad)
        {
            if (!Var(ad)) return null;
            return Dosyadan(Yol(ad));
        }

        static KarakterKimligi Dosyadan(string yol)
        {
            try
            {
                var k = JsonUtility.FromJson<KarakterKimligi>(File.ReadAllText(yol, Encoding.UTF8));
                if (k != null && KarakterKimligi.AdHatasi(k.ad) == null) return k;
                Debug.LogWarning("[Kimlik] geçersiz kart atlandı: " + Path.GetFileName(yol));
            }
            catch (Exception e) { Debug.LogWarning("[Kimlik] kart okunamadı: " + Path.GetFileName(yol) + " (" + e.Message + ")"); }
            return null;
        }

        public static List<KarakterKimligi> Hepsi()
        {
            var l = new List<KarakterKimligi>();
            try
            {
                if (!Directory.Exists(Klasor)) return l;
                var dosyalar = Directory.GetFiles(Klasor, "*.json");
                Array.Sort(dosyalar, StringComparer.OrdinalIgnoreCase);
                foreach (var f in dosyalar) { var k = Dosyadan(f); if (k != null) l.Add(k); }
            }
            catch (Exception e) { Debug.LogWarning("[Kimlik] kartlar okunamadı: " + e.Message); }
            return l;
        }

        // null = kaydedildi, aksi halde hata metni
        public static string Kaydet(KarakterKimligi k)
        {
            string h = KarakterKimligi.AdHatasi(k.ad);
            if (h != null) return h;
            try
            {
                Directory.CreateDirectory(Klasor);
                string yol = Yol(k.ad), gecici = yol + ".tmp";
                File.WriteAllText(gecici, JsonUtility.ToJson(k, true), new UTF8Encoding(false));
                if (File.Exists(yol)) File.Replace(gecici, yol, yol + ".onceki");
                else File.Move(gecici, yol);
                return null;
            }
            catch (Exception e) { return "Kart kaydedilemedi: " + e.Message; }
        }

        // Kart silinmez, _silinen klasörüne taşınır
        public static string Kaldir(string ad)
        {
            if (!Var(ad)) return "Kart bulunamadı.";
            try
            {
                string hedefKlasor = Path.Combine(Klasor, "_silinen");
                Directory.CreateDirectory(hedefKlasor);
                string hedef = Path.Combine(hedefKlasor, Util.Norm(ad) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".json");
                File.Move(Yol(ad), hedef);
                return null;
            }
            catch (Exception e) { return "Kart kaldırılamadı: " + e.Message; }
        }

        // Kartları kütüphanenin KOPYASINA işler; asıl varlık hiç değişmez. Kart yoksa aslı aynen döner.
        public static StoryLibrary Uygula(StoryLibrary asil)
        {
            if (asil == null || !Etkin) return asil;
            var kartlar = Hepsi();
            if (kartlar.Count == 0) return asil;
            var kopya = UnityEngine.Object.Instantiate(asil);
            kopya.name = asil.name + " (kimlik)";
            var katalog = KarakterKatalogu.Yukle();
            foreach (var k in kartlar)
            {
                var d = kopya.FindCharacter(k.ad);
                if (d != null && kopya.IsNarrator(d)) continue;
                if (d == null) { d = new CharacterDef { id = k.ad }; kopya.characters.Add(d); }
                k.Isle(d, katalog);
            }
            return kopya;
        }
    }
}
