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
            // Asama 14: bolumler arasi devamlilik (videoyu etkilemez)
            Kaydet("ozet", "ozet: Arda annesinin mektubunu buldu", "Bu bölümün özeti (sonraki bölümde kullanılır)");
            Kaydet("durum", "durum: Dede = olu", "Karakter durumu: hayatta, olu, kayip, hastanede, uzakta (o satırdan sonrası için)");
            Kaydet("olay", "olay: Ev satıldı", "Geçmiş olay kaydı");
            Kaydet("gecmis", "gecmis: Anne = yıllarca yurt dışında çalıştı", "Karakter geçmişi");
            Kaydet("iliski", "iliski: Anne - Arda = anne ve oğul", "İki karakter arasındaki ilişki");
            Kaydet("soru", "soru: Dördüncü tabak kimin?", "Açık kalan soru");
            Kaydet("cozuldu", "cozuldu: Dördüncü tabak kimin?", "Daha önce sorulan sorunun yanıtlandığını işaretler");
            Kaydet("plan", "plan: 3 = Mektup bulunur", "Sezon planı: bölüm numarası = ne olacak");
            // Asama 15: etkilesimli seri
            Kaydet("devami", "devami: Yarın aynı saatte", "Bölüm sonu 'devamı N. bölümde' kartı");
            Kaydet("oylama", "oylama: Arda ne yapmalı?", "Sonraki 'secim:' kartının sorusu");
            Kaydet("mekan", "mekan: oda_gece", "Mekanı değiştirir, karakterleri temizler");
            Kaydet("mekan3d", "mekan3d: mutfak", "3D mekan: oda, ev, mutfak, yatak_odasi, hastane, sokak, koy, park, araba, kopru, koridor, hava_kilidi, yok ya da kaydedilen ad; renk ayarı: duvar= zemin= vurgu=");
            Kaydet("hava", "hava: yagmur", "3D mekanda hava: yagmur, kar, sis, yok (virgülle birlikte)");
            Kaydet("istasyon", "istasyon: sinyal", "İstasyon mekânında: sinyal, gurultu, kamera, mavi, mor, bozuk, kirag, izler, karart, yak");
            Kaydet("gorunum", "gorunum: Kopya golge", "3D karakterin rengi: tulum, golge, soguk, gizli, normal (birlikte yazılabilir)");
            // Sinematik efektler (SinematikEfekt.cs)
            Kaydet("gren", "gren: cok", "Film greni: yok, az, normal, cok");
            Kaydet("vinyet", "vinyet: cok", "Kenar karartma: yok, az, normal, cok");
            Kaydet("filtre", "filtre: soguk", "Renk filtresi: sicak, soguk, siyah_beyaz, pembe, yesil, mavi, yok");
            Kaydet("yavas", "yavas: 3", "Yavaş çekim: verilen saniye boyunca hareketler yavaşlar (yavas: yok bitirir)");
            Kaydet("dondur", "dondur: 1.5", "Zaman dondurma: flaş + görüntü verilen saniye boyunca durur");
            Kaydet("gozyasi", "gozyasi: Anne", "3D karakterin gözünden yaş süzülür (gozyasi: yok bitirir)");
            Kaydet("nefes", "nefes: Anne", "3D karakterin ağzından nefes buharı çıkar (nefes: yok bitirir)");
            Kaydet("zaman", "zaman: gun_batimi", "Zaman: sabah, gun, gun_batimi, gece");
            Kaydet("isik", "isik: huzunlu", "Işık: normal, gunduz, dramatik, yumusak, soguk, sicak, karanlik, mum, romantik, umut, korku, huzunlu");
            Kaydet("kanca", "kanca: Öldüm.", "Siyah ekranda büyük kanca yazısı (ilk saniyeler)");
            Kaydet("geri", "geri: 1 saat önce", "Geri sarma (VHS) efekti ve yazısı");
            Kaydet("yazi", "yazi: Devamı 2. Bölümde...", "Üst-ortada büyük yazı");
            Kaydet("bekle", "bekle: 0.5", "Saniye bekler");
            Kaydet("efekt", "efekt: kapi", "Ses efekti çalar");
            Kaydet("cikar", "cikar: Anne", "Karakteri sahneden çıkarır");
            Kaydet("temizle", "temizle:", "Bütün karakterleri sahneden çıkarır");
            Kaydet("kamera", "kamera: yakin", "Kamera: otomatik (yönetmen), genis, orta, omuz, ters, iki, yakin; tek seferlik: tepki, yaklas, uzaklas, salla");
            Kaydet("atmosfer", "atmosfer: mum, toz", "Atmosfer: mum, toz, yagmur, buyu, gunes (ışık huzmesi), sis, yok");
            Kaydet("muzik", "muzik: pad", "Fon müziği: pad, yok ya da kütüphanedeki müzik");
            Kaydet("far", "far:", "Kamyon farları ve beyaz parlama");
            Kaydet("flas", "flas:", "Beyaz parlama");
            Kaydet("gorsel", "gorsel: eller", "Tam ekran görsel (eller = kendi gözünden eller, yok = kapat)");
            Kaydet("secim", "secim: A) Söylesin / B) Saklasın", "İzleyici seçim kartı (iki seçenek)");
            Kaydet("sonuc", "sonuc: %69 A", "Önceki bölümün oylama sonucu kartı");
            Kaydet("oyuncu", "oyuncu: Arda = Timmy", "Karakteri 3D modelle oynatır (yok = çizgi karakter)");
            Kaydet("son", "son: yumusak Devamı yarın...", "Bölüm sonu: yumusak ya da sert");
            Kaydet("poz", "poz: Arda yatiyor", "Karakter pozu: ayakta, yatiyor, oturuyor");
            Kaydet("vfx", "vfx: flas", "Görsel efekt: flas, far, vhs (geri ile aynı; vfx: vhs 1 saat önce)");
            Kaydet("gecis", "gecis: karar", "Sahne geçişi: karar (siyah), capraz (çapraz geçiş), beyaz; 2D ve 3D sahnede");
            Kaydet("kapak", "kapak: 24", "Kapak karesi bu satırdan (ya da sonraki ilk konuşmadan) alınır; boş = sonraki konuşma");
            Kaydet("etiket", "etiket: aile, dram, duygusal", "Açıklamaya eklenecek hashtag'ler (# yazmak gerekmez)");
            // Hareket ve etkilesim (3D karakterler; Hareket3D.cs)
            Kaydet("sarilma", "sarilma: Anne Baba", "İki 3D karakter yaklaşıp sarılır (ayril: ile biter)");
            Kaydet("el_tut", "el_tut: Anne Baba", "El ele tutuşurlar; biri oturuyor/yatıyorsa diğeri yanına gelip elini tutar");
            Kaydet("yuzyuze", "yuzyuze: Anne Baba", "İki karakter birbirine döner ve bakar");
            Kaydet("ayril", "ayril:", "Sarılma / el tutma / yüz yüze durumunu bitirir (ayril: Anne = yalnız onunkini)");
            Kaydet("tepki_ver", "tepki_ver: Baba sasir", "Tepki: sasir, kork, uzul, agla, sevin, kiz, dusun, onayla, reddet, el_salla");
            Kaydet("git", "git: Anne pencere", "Karakter bir noktaya ya da karakterin yanına yürür (sonuna 'kos' eklenirse koşar)");
            Kaydet("tut", "tut: Anne bardak", "Eşya tutar: bardak, telefon, mektup, kitap, cicek, ilac, tabak, anahtar (sag/sol)");
            Kaydet("birak", "birak: Anne", "Tutulan eşyayı bırakır");
            Kaydet("kapi", "kapi: Anne ac", "3D mekanın kapısını açar/kapatır (kapi: ac, kapi: kapat, kapi: Anne ac)");
            Kaydet("kalk", "kalk: Anne", "Oturduğu ya da yattığı yerden ayağa kalkar");
            Kaydet("yatak", "yatak: Anne yat", "Yatağa yatırır ya da oturtur (yat/otur); konuşurken kalkmaz");
        }
    }
}
