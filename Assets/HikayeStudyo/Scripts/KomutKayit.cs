using System.Collections.Generic;
using System.Reflection;

namespace HikayeStudyo
{
    // Hikaye komutlarinin ortak kaydi. Dogrulayici (StudyoDogrulama), "Ekle" menusu ve yapay zeka kilavuzu
    // gecerli komut adlarini buradan okur; boylece StoryModel'e yeni komut eklenince listeler kendiliginden guncellenir.
    //
    // Yeni komut ekleyen kod (or. StoryParser) bir kez kaydeder:
    //     KomutKayit.Kaydet("kamera_hiz", "kamera_hiz: 1.5", "Kamera hizini ayarlar");
    // Kaydedilmeyen ama StoryParser'in bildigi komutlar da dogrulayicida sorun cikarmaz (parser yoklanir).
    public static class KomutKayit
    {
        public class Bilgi
        {
            public string Ad = "";        // normalize edilmis ad (kucuk harf, Turkce karaktersiz)
            public string Ornek = "";     // "mekan: oda_gece"
            public string Aciklama = "";
        }

        static readonly Dictionary<string, Bilgi> kayit = new Dictionary<string, Bilgi>();

        // Gecerli komut adlari (normalize). Disaridan da dogrudan eklenebilir.
        public static readonly HashSet<string> Komutlar = new HashSet<string>();

        static KomutKayit() { Taban(); }

        public static void Kaydet(string ad, string ornek, string aciklama)
        {
            if (string.IsNullOrEmpty(ad)) return;
            string k = Util.Norm(ad);
            Komutlar.Add(k);
            Bilgi b;
            if (!kayit.TryGetValue(k, out b)) { b = new Bilgi { Ad = k }; kayit[k] = b; }
            if (!string.IsNullOrEmpty(ornek)) b.Ornek = ornek;
            if (!string.IsNullOrEmpty(aciklama)) b.Aciklama = aciklama;
        }

        public static void Kaydet(string ad) { Kaydet(ad, "", ""); }

        public static bool Var(string ad)
        {
            return !string.IsNullOrEmpty(ad) && Komutlar.Contains(Util.Norm(ad));
        }

        // Kayitli butun komutlar (ad sirasina gore degil, eklenme sirasina yakin: once bilgisi olanlar)
        public static List<Bilgi> Hepsi()
        {
            KesfEt();
            var list = new List<Bilgi>();
            foreach (var k in Komutlar)
            {
                Bilgi b;
                if (!kayit.TryGetValue(k, out b)) b = new Bilgi { Ad = k, Ornek = k + ": " };
                list.Add(b);
            }
            list.Sort((x, y) => string.CompareOrdinal(x.Ad, y.Ad));
            return list;
        }

        // Baska ajanlarin/surumlerin parser'a ekledigi komutlari kendiliginden bulur (kayit defteri kullanilmasa bile):
        // 1) StoryParser.DirectKeys (varsa, yansima ile) 2) bilinen aday adlar parser'a yoklanir.
        static bool kesfedildi;
        static readonly string[] Adaylar =
            { "bak", "dinle", "altyazi", "mekan3d", "yer", "gir", "figuran", "ani", "zaman", "gecis", "sessizlik", "isik", "kamera_hiz", "kamera_ac", "sahne", "ses", "dolly", "odak" };

        static bool ParserBiliyor(string k)
        {
            try
            {
                var r = StoryParser.Parse(k + ": 1");
                foreach (var w in r.Warnings) if (w.IndexOf("komutu bilinmiyor", System.StringComparison.OrdinalIgnoreCase) >= 0) return false;
                return true;
            }
            catch { return false; }
        }

        public static void KesfEt()
        {
            if (kesfedildi) return;
            kesfedildi = true;
            try
            {
                var f = typeof(StoryParser).GetField("DirectKeys", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var arr = f != null ? f.GetValue(null) as string[] : null;
                if (arr != null)
                    foreach (var k in arr)
                        if (!string.IsNullOrEmpty(k) && !Komutlar.Contains(Util.Norm(k))) Kaydet(k, k + ": ", "Yönetmen/sahne komutu (StoryParser.DirectKeys)");
                foreach (var k in Adaylar)
                    if (!Komutlar.Contains(k) && ParserBiliyor(k)) Kaydet(k, k + ": ", "Parser'ın tanıdığı komut (örnek için Docs/HikayeSozdizimi.md / kodda ilgili sınıf)");
            }
            catch { }
        }

        static void Taban()
        {
            Kaydet("seri", "seri: Sessiz Değişim", "Seri adı (kapak ve açıklama için)");
            Kaydet("bolum", "bolum: 3", "Bölüm numarası");
            Kaydet("baslik", "baslik: Annem beni tanımadı", "Bölüm başlığı (kapak ve açıklama)");
            Kaydet("mekan", "mekan: oda_gece", "Mekanı değiştirir, karakterleri temizler");
            Kaydet("kanca", "kanca: Öldüm.", "Siyah ekranda büyük kanca yazısı (ilk saniyeler)");
            Kaydet("geri", "geri: 1 saat önce", "Geri sarma (VHS) efekti ve yazısı");
            Kaydet("yazi", "yazi: Devamı 2. Bölümde...", "Üst-ortada büyük yazı");
            Kaydet("bekle", "bekle: 0.5", "Saniye bekler");
            Kaydet("efekt", "efekt: kapi", "Ses efekti çalar");
            Kaydet("cikar", "cikar: Anne", "Karakteri sahneden çıkarır");
            Kaydet("temizle", "temizle:", "Bütün karakterleri sahneden çıkarır");
            Kaydet("kamera", "kamera: yakin", "Kamera modu: otomatik, yakin, genis, salla");
            Kaydet("atmosfer", "atmosfer: mum, toz", "Atmosfer: mum, toz, yagmur, buyu, gunes, yok");
            Kaydet("muzik", "muzik: pad", "Fon müziği: pad, yok ya da kütüphanedeki müzik");
            Kaydet("far", "far:", "Kamyon farları ve beyaz parlama");
            Kaydet("flas", "flas:", "Beyaz parlama");
            Kaydet("gorsel", "gorsel: eller", "Tam ekran görsel (eller = kendi gözünden eller, yok = kapat)");
            Kaydet("secim", "secim: A) Söylesin / B) Saklasın", "İzleyici seçim kartı (iki seçenek)");
            Kaydet("sonuc", "sonuc: %69 A", "Önceki bölümün oylama sonucu kartı");
            Kaydet("oyuncu", "oyuncu: Arda = Timmy", "Karakteri 3D modelle oynatır (yok = çizgi karakter)");
            Kaydet("son", "son: yumusak Devamı yarın...", "Bölüm sonu: yumusak ya da sert");
            Kaydet("poz", "poz: Arda yatiyor", "Karakter pozu: ayakta, yatiyor, oturuyor");
        }
    }
}
