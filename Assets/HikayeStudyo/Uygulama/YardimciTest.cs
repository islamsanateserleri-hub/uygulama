#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 6: internetsiz yardımcı motorunun Editör denemesi. Sonuç: HikayeKomut/yardimci_test.txt
    // Köprüden: menu: Hikaye/Yardimci Testi
    public static class YardimciTest
    {
        static StringBuilder rapor;
        static int gecen, kalan;

        static void Bekle(string ad, bool kosul)
        {
            if (kosul) gecen++; else kalan++;
            rapor.AppendLine((kosul ? "GECTI  " : "KALDI  ") + ad);
        }

        [MenuItem("Hikaye/Yardimci Testi")]
        public static void Calistir()
        {
            rapor = new StringBuilder(); gecen = kalan = 0;
            try { Denemeler(); }
            catch (System.Exception e) { kalan++; rapor.AppendLine("KALDI  istisna: " + e); }
            rapor.Insert(0, "Yardimci testi: " + gecen + " gecti, " + kalan + " kaldi\n");
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/yardimci_test.txt"), rapor.ToString(), new UTF8Encoding(false));
            Debug.Log("Yardimci testi: " + gecen + " gecti, " + kalan + " kaldi");
        }

        static void Denemeler()
        {
            var lib = Resources.Load<StoryLibrary>("StoryLibrary");
            Bekle("kutuphane yuklendi", lib != null);
            var kars = HikayeDenetci.KarakterAdlari(lib);
            var mekanlar = HikayeDenetci.MekanAdlari(lib);

            // 1) düz metin -> sahneler
            string duz = "Gece yarısı Arda odasında uyandı. Her yer karanlıktı.\nArda: Burası neresi?\n\n\"Oğlum, gözlerini açtın!\" diye bağırdı Anne.\n\nErtesi sabah sofrada herkes sessizdi.\nBaba: Yemeğini ye.";
            Bekle("duz satir sayisi 4+", YardimciMotor.DuzSatirSayisi(duz) >= 4);
            string cevrilen = YardimciMotor.DuzMetniCevir(duz, kars, mekanlar);
            rapor.AppendLine("--- cevrilen ---\n" + cevrilen + "\n----------------");
            Bekle("cevirme sonrasi duz satir kalmadi", YardimciMotor.DuzSatirSayisi(cevrilen) == 0);
            Bekle("gece odasi sahnesi", cevrilen.Contains("mekan: oda_gece"));
            Bekle("sofra sahnesi", cevrilen.Contains("mekan: sofra"));
            Bekle("Arda konusmasi", cevrilen.Contains("[Arda|") && cevrilen.Contains("] Burası neresi?"));
            Bekle("tirnakli soz Anne'ye", cevrilen.Contains("[Anne|") && cevrilen.Contains("gözlerini açtın"));
            int hata = HikayeDenetci.HataSayisi(HikayeDenetci.Denetle(cevrilen, lib, new string[0]));
            Bekle("cevrilen metinde denetci hatasi yok (" + hata + ")", hata == 0);
            Bekle("etiketli metin degismez", YardimciMotor.DuzMetniCevir(cevrilen, kars, mekanlar) == cevrilen);

            Bekle("soguk: usudum", YardimciMotor.SogukAnlatiliyor("Çok üşüdüm."));
            Bekle("soguk: dondum", YardimciMotor.SogukAnlatiliyor("Dışarıda donuyorum, dondum!"));
            Bekle("soguk degil: dondum (geri geldim)", !YardimciMotor.SogukAnlatiliyor("Eve döndüm anne."));
            Bekle("soguk degil: buzdolabi", !YardimciMotor.SogukAnlatiliyor("Buzdolabında süt var."));

            // 2) analiz: karakter, mekân, duygu, kamera, tutarlılık, yazım
            string m = "[Arda|normal|dis] herkez nerede ?\n[Ane|normal|dis] Korkuyorum, imdat!\nmekan: oda_gece\npoz: Arda yatiyor\n[Arda|normal|dis|kos] Günaydın.\nmekan: yol_gec\n[Baba|uzgun|dis] Üzgünüm.";
            var a = YardimciMotor.Incele(m, kars, mekanlar);
            foreach (var o in a.Oneriler) rapor.AppendLine("  " + o.Tur + " s" + o.Satir + " " + o.Mesaj + (o.Uygulanabilir ? "  => " + o.YeniSatir : ""));
            foreach (var s in a.Sahneler) rapor.AppendLine("  " + s.Aciklama);
            Bekle("3 sahne (" + a.Sahneler.Count + ")", a.Sahneler.Count == 3);
            Bekle("eksik karakter Ane", a.Oneriler.Exists(o => o.Tur == OneriTuru.Karakter && o.EksikAd == "Ane" && o.YeniSatir.StartsWith("[Anne|")));
            Bekle("mekansiz giris sahnesine oneri", a.Oneriler.Exists(o => o.Tur == OneriTuru.Mekan && o.Satir == 1 && o.OneEkle));
            Bekle("yanlis mekan adi duzeltmesi", a.Oneriler.Exists(o => o.Tur == OneriTuru.Mekan && o.YeniSatir == "mekan: yol_gece"));
            Bekle("duygu korkmus", a.Oneriler.Exists(o => o.Tur == OneriTuru.Duygu && o.Satir == 2 && o.YeniSatir.Contains("|korkmus|")));
            Bekle("kamera onerisi var", a.Sayi(OneriTuru.Kamera) > 0);
            Bekle("yatarken kosma", a.Oneriler.Exists(o => o.Tur == OneriTuru.Tutarlilik && o.Satir == 5 && o.YeniSatir == "poz: Arda ayakta"));
            Bekle("gece gunaydin", a.Oneriler.Exists(o => o.Tur == OneriTuru.Tutarlilik && o.Mesaj.Contains("günaydın")));
            Bekle("yazim herkez", a.Oneriler.Exists(o => o.Tur == OneriTuru.Yazim && o.Satir == 1 && o.YeniSatir.EndsWith("] Herkes nerede?")));

            // 3) uygulama
            var yazim = a.Oneriler.Find(o => o.Tur == OneriTuru.Yazim && o.Satir == 1);
            string u = YardimciMotor.Uygula(m, yazim);
            Bekle("oneri uygulandi", u != null && u.StartsWith("[Arda|normal|dis] Herkes nerede?\n"));
            Bekle("degisen satira ikinci kez uygulanmaz", YardimciMotor.Uygula(u, yazim) == null);
            int n;
            string k = YardimciMotor.KarakterDegistir(m, "Ane", "Anne", out n);
            Bekle("karakter degistirme", n == 1 && k.Contains("[Anne|normal|dis] Korkuyorum"));
            string t = YardimciMotor.TumunuUygula(m, a.Oneriler, OneriTuru.Mekan, out n);
            Bekle("tum mekan onerileri (" + n + ")", n == 2 && t.StartsWith("mekan: ") && t.Contains("mekan: yol_gece"));
            string ac = YardimciMotor.AciklamalariEkle(m, kars, mekanlar);
            string ac2 = YardimciMotor.AciklamalariEkle(ac, kars, mekanlar);
            Bekle("sahne aciklamalari eklendi", ac.Split(new[] { "// Sahne açıklaması:" }, System.StringSplitOptions.None).Length - 1 == 3);
            Bekle("aciklama tekrar eklenmez", ac == ac2);
            Bekle("aciklama oynaticiyi etkilemez", StoryParser.Parse(ac).Items.Count == StoryParser.Parse(m).Items.Count);

            // 4) örnek hikâyeler: çökme yok, temiz hikâyede yanlış uyarı sayısı
            foreach (var ta in Resources.LoadAll<TextAsset>("Hikayeler"))
            {
                var r = YardimciMotor.Incele(ta.text, kars, mekanlar);
                rapor.AppendLine("ornek " + ta.name + ": " + r.Sahneler.Count + " sahne, karakter " + r.Sayi(OneriTuru.Karakter) + ", mekan " + r.Sayi(OneriTuru.Mekan) +
                                 ", duygu " + r.Sayi(OneriTuru.Duygu) + ", kamera " + r.Sayi(OneriTuru.Kamera) + ", tutarlilik " + r.Sayi(OneriTuru.Tutarlilik) + ", yazim " + r.Sayi(OneriTuru.Yazim));
                foreach (var o in r.Oneriler) rapor.AppendLine("  " + o.Tur + " s" + o.Satir + " " + o.Mesaj);
                Bekle(ta.name + ": eksik karakter yok", r.Sayi(OneriTuru.Karakter) == 0);
            }
            Bekle("teselli cumlesi aglamakli sayilmaz", YardimciMotor.DuyguTahmin("Ağlama anne... İyiyim ben.") == "normal" && YardimciMotor.DuyguTahmin("Korkma, buradayım.") == "normal");
            Bekle("agliyorum / korktum algilanir", YardimciMotor.DuyguTahmin("Ağlıyorum.") == "aglamakli" && YardimciMotor.DuyguTahmin("Çok korktum.") == "korkmus");
            Bekle("uzak ada oneri yok", YardimciMotor.Incele("[Ayşe|normal|dis] Merhaba.", kars, mekanlar).Oneriler.Exists(o => o.Tur == OneriTuru.Karakter && o.EksikAd == "Ayşe" && o.OnerilenAd == "" && !o.Uygulanabilir));
            Bekle("yorumlar Gemini'ye gitmez", YardimciGemini.YorumlariBosalt("// not\nmekan: sofra") == "\nmekan: sofra");
            Bekle("bos metin", YardimciMotor.Incele("", kars, mekanlar).Oneriler.Count == 0);
        }
    }
}
#endif
