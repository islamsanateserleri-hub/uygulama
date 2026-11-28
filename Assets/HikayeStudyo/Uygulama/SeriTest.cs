#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 14: devamlılık motorunun Editör denemesi. Sonuç: HikayeKomut/seri_test.txt
    // Köprüden: menu: Hikaye/Seri Testi
    public static class SeriTest
    {
        static StringBuilder rapor;
        static int gecen, kalan;

        static void Bekle(string ad, bool kosul)
        {
            if (kosul) gecen++; else kalan++;
            rapor.AppendLine((kosul ? "GECTI  " : "KALDI  ") + ad);
        }

        [MenuItem("Hikaye/Seri Testi")]
        public static void Calistir()
        {
            rapor = new StringBuilder(); gecen = kalan = 0;
            try { Denemeler(); }
            catch (System.Exception e) { kalan++; rapor.AppendLine("KALDI  istisna: " + e); }
            rapor.Insert(0, "Seri testi: " + gecen + " gecti, " + kalan + " kaldi\n");
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/seri_test.txt"), rapor.ToString(), new UTF8Encoding(false));
            Debug.Log("Seri testi: " + gecen + " gecti, " + kalan + " kaldi");
        }

        static bool Var(SeriRaporu r, int bolum, string parca)
        {
            foreach (var c in r.Celiskiler) if (c.Bolum == bolum && c.Mesaj.Contains(parca)) return true;
            return false;
        }

        static void Denemeler()
        {
            string b1 = "seri: Dördüncü Tabak\nbolum: 1\nkanca: Dede sofraya oturdu.\n[Dede|normal|dis] Bu son yemeğim.\ndurum: Dede = olu\n" +
                        "durum: Baba = hastanede\nsoru: Dördüncü tabak kimin?\niliski: Anne - Arda = anne ve oğul\noyuncu: Anne = VRM_Anne\n" +
                        "ozet: Dede öldü, tabak sorusu kaldı.\nplan: 2 = Mektup\nplan: 5 = Final\nson: yumusak Devamı yarın";
            string b2 = "seri: dördüncü tabak\nbolum: 2\nmekan: sofra\noyuncu: Anne = Megan\n[Dede|normal|dis] Ben geldim.\n[Dede|normal|dis] Yine ben.\n" +
                        "[Baba|uzgun|dis] Evdeyim.\niliski: Arda - Anne = teyze ve yeğen\ncozuldu: Başka bir soru\nson: sert";
            string b3 = "seri: Dördüncü Tabak\nbolum: 4\nani: sepya\n[Dede|normal|dis] Eskiden böyleydi.\nani: bitir\ndurum: Dede = hayatta\n" +
                        "durum: Hayaletcik = olu\ncozuldu: dördüncü tabak kimin\nson: sert";
            string baska = "seri: Başka\nbolum: 1\n[Dede|normal|dis] Merhaba.";

            var l = new List<SeriBolum> { SeriMotoru.Bolum("b3", b3), SeriMotoru.Bolum("b1", b1), SeriMotoru.Bolum("baska", baska), SeriMotoru.Bolum("b2", b2) };
            Bekle("seri notlari okundu", l[1].P.SeriNotlari.Count == 7);
            Bekle("yeni komutlar uyari vermiyor", l[1].P.Warnings.Count == 0);
            Bekle("bozuk durum uyarir", StoryParser.Parse("durum: Dede = ucuyor").Warnings.Count == 1);
            Bekle("bozuk iliski uyarir", StoryParser.Parse("iliski: Anne = anne").Warnings.Count == 1);
            Bekle("seri notu videoyu etkilemez", StoryParser.Parse("olay: x\nsoru: y").Items.Count == 0);
            int hataB1 = HikayeDenetci.HataSayisi(HikayeDenetci.Denetle(b1, Resources.Load<StoryLibrary>("StoryLibrary"), new string[0]));
            foreach (var s in HikayeDenetci.Denetle(b1, Resources.Load<StoryLibrary>("StoryLibrary"), new string[0])) if (s.Hata) rapor.AppendLine("  denetci s" + s.Satir + " " + s.Mesaj);
            Bekle("denetci yeni komutlari tanir (karakter disi hata yok) " + hataB1, !rapor.ToString().Contains("komutu bilinmiyor"));

            var r = SeriMotoru.Incele(l, "Dördüncü Tabak", 2);
            foreach (var c in r.Celiskiler) rapor.AppendLine("  " + (c.Agir ? "AGIR " : "uyari ") + "b" + c.Bolum + " s" + c.Satir + " " + c.Mesaj);
            Bekle("3 bolum, baska seri disarida", r.Bolumler.Count == 3);
            Bekle("sira 1-2-4", r.Bolumler[0].No == 1 && r.Bolumler[1].No == 2 && r.Bolumler[2].No == 4);
            Bekle("olu karakter konusuyor", Var(r, 2, "Dede 1. bölümde öldü"));
            int dede = 0; foreach (var c in r.Celiskiler) if (c.Bolum == 2 && c.Mesaj.StartsWith("Dede ")) dede++;
            Bekle("ayni karakter bir kez bildirilir", dede == 1);
            Bekle("hastanede ama sofrada", Var(r, 2, "hastanede"));
            Bekle("iliski degisti", Var(r, 2, "ilişkisi değişti"));
            Bekle("model degisti", Var(r, 2, "3D model değişti"));
            Bekle("sorulmamis soru cozuldu", Var(r, 2, "Başka bir soru"));
            Bekle("bolum atlandi", Var(r, 4, "atlanmış"));
            Bekle("ani icinde olu konusabilir", !Var(r, 4, "öldü ama"));
            Bekle("aciklamasiz dirilme", Var(r, 4, "açıklamasız"));
            Bekle("gorunmeyen ad", Var(r, 4, "Hayaletcik"));
            Bekle("onceki ozet", r.OncekiNo == 1 && r.OncekiOzet.StartsWith("Dede öldü"));
            Bekle("durum fotografi (Dede olu)", r.Durumlar.Exists(d => d.Ad == "Dede" && d.Durum == "olu"));
            Bekle("acik soru 2. bolumde duruyor", r.AcikSorular.Count == 1);
            Bekle("plan 2 satir", r.Plan.Count == 2 && r.Plan[0].Contains("b2") && r.Plan[1].Contains("yazılmadı"));

            var r5 = SeriMotoru.Incele(l, "Dördüncü Tabak", 5);
            Bekle("sonraki bolumde soru kapanmis", r5.AcikSorular.Count == 0);
            Bekle("sonraki bolumde Dede hayatta", r5.Durumlar.Exists(d => d.Ad == "Dede" && d.Durum == "hayatta"));
            // acik bolume yazilan soru/cozuldu hemen listeye yansir
            var tek = new List<SeriBolum> { SeriMotoru.Bolum("k1", "seri: K\nbolum: 1\nsoru: Kapida kim var\ndurum: Dede = olu\nson: sert") };
            var once = new List<SeriBolum>(tek) { SeriMotoru.Bolum("k2", "seri: K\nbolum: 2\nmekan: sofra\nson: sert") };
            Bekle("acik bolumde soru gorunur", SeriMotoru.Incele(once, "K", 2).AcikSorular.Count == 1);
            once[1] = SeriMotoru.Bolum("k2", "seri: K\nbolum: 2\nmekan: sofra\ncozuldu: Kapida kim var\nolay: kapi acildi\nson: sert");
            var rc = SeriMotoru.Incele(once, "K", 2);
            Bekle("Cozuldu soruyu listeden kaldirir", rc.AcikSorular.Count == 0);
            Bekle("acik bolumun olayi gorunur", rc.Olaylar.Count == 1);
            once[1] = SeriMotoru.Bolum("k2", "seri: K\nbolum: 2\nmekan: sofra\ngeri: 1 saat once\n[Dede|normal|dis] Ben.\nson: sert");
            var rg = SeriMotoru.Incele(once, "K", 2);
            Bekle("geri: sonrasi olu karakter hafif uyari verir", Var(rg, 2, "öldü ama") && !rg.Celiskiler.Exists(c => c.Agir && c.Mesaj.Contains("öldü ama")));

            string olum = "seri: C\nbolum: 1\ndurum: Dede = olu\nson: sert";
            var cn = new List<SeriBolum> { SeriMotoru.Bolum("c1", olum),
                SeriMotoru.Bolum("c2", "seri: C\nbolum: 2\nmekan: sofra\npoz: Dede yatiyor\nsarilma: Anne Dede\ncikar: Dede kapi\nson: sert") };
            Bekle("olunun yatmasi/sarilmasi/cikarilmasi celiski degil", !Var(SeriMotoru.Incele(cn, "C", 2), 2, "öldü ama"));
            cn[1] = SeriMotoru.Bolum("c2", "seri: C\nbolum: 2\nolay: yagmur yagdi\ndurum: Dede = hayatta\nson: sert");
            Bekle("ilgisiz olay dirilmeyi aciklamaz", Var(SeriMotoru.Incele(cn, "C", 2), 2, "açıklamasız"));
            cn[1] = SeriMotoru.Bolum("c2", "seri: C\nbolum: 2\nolay: Dede'nin yasadigi ortaya cikti\ndurum: Dede = hayatta\nson: sert");
            Bekle("adi gecen olay dirilmeyi aciklar", !Var(SeriMotoru.Incele(cn, "C", 2), 2, "açıklamasız"));

            Bekle("seri adi bos -> bos rapor", SeriMotoru.Incele(l, "", 0).Bolumler.Count == 0);

            // ---- Asama 15 ----
            var lib = Resources.Load<StoryLibrary>("StoryLibrary");
            for (int i = 0; i < SeriUretici.SablonAdlari.Length; i++)
            {
                string m = SeriUretici.Sablon(i, "Deneme", 3, "mektup", "ozet metni");
                var p = StoryParser.Parse(m);
                var sorunlar = HikayeDenetci.Denetle(m, lib, new string[0]);
                int hata = sorunlar.FindAll(x => x.Hata && x.Kod != "SABLON_BOS").Count;
                Bekle("sablon " + i + " uyarisiz, yer tutucu disinda hatasiz (" + p.Warnings.Count + "/" + hata + ")", p.Warnings.Count == 0 && hata == 0 && p.Bolum == 3);
                Bekle("sablon " + i + " doldurulmadan videoya gidemez", sorunlar.Exists(x => x.Hata && x.Kod == "SABLON_BOS"));
                if (i == 0)
                {
                    Bekle("gercek replik sablon sayilmaz", !Util.SablonYerTutucu("[Anne|normal|dis] Numaranı buraya yaz.") && !Util.SablonYerTutucu("kanca: İsmini buraya yazdım"));
                    Bekle("uygulamanin ekledigi bos satirlar yakalanir", Util.SablonYerTutucu("kanca: Buraya ilk cümleni yaz.") && Util.SablonYerTutucu("[Arda|normal|dis] Metni buraya yaz"));
                }
                Bekle("sablon " + i + " uydurma oylama sonucu yok", !p.Items.Exists(x => x.Type == ItemType.Result));
            }
            var dk = StoryParser.Parse("devami: Yarin\noylama: Ne yapmali?\nsecim: A) x / B) y");
            Bekle("devami ve oylama okunur", dk.Warnings.Count == 0 && dk.Items.Count == 2 && dk.Items[0].Type == ItemType.DevamKarti && dk.Items[1].Soru == "Ne yapmali?");
            string asil = "seri: X\nbolum: 2\noyuncu: Anne = VRM_Anne\nmekan: sofra\nkanca: Sir\n[Anne|normal|dis] Bir.\n[Arda|korkmus|dis] Iki!\n" +
                          "mekan: oda_gece\n[Arda|normal|dis] Uc.\n[Anne|aglamakli|dis] Dort.\n[Anne|kizgin|dis] Bes!\n[Arda|sasirmis|dis] Alti?\nsecim: A) Kalsin / B) Gitsin\ndevami: yarin\nson: yumusak";
            List<StoryItem> sec;
            string fr = SeriUretici.Fragman(asil, 4, out sec);
            rapor.AppendLine("--- fragman ---\n" + fr);
            var pf = StoryParser.Parse(fr);
            Bekle("fragman 4 replik", sec.Count == 4 && pf.DialogueCount >= 0 && pf.Items.FindAll(x => x.Type == ItemType.Dialogue).Count == 4);
            Bekle("fragman en guclu replikler", fr.Contains("Iki!") && fr.Contains("Dort.") && fr.Contains("Bes!") && !fr.Contains("Uc."));
            Bekle("fragman mekan ve oyuncu korur", fr.Contains("mekan: sofra") && fr.Contains("mekan: oda_gece") && fr.Contains("oyuncu: Anne = VRM_Anne"));
            Bekle("fragman secimi icermez, sert biter", !fr.Contains("secim:") && fr.Contains("son: sert") && fr.Contains("2. BÖLÜM YAKINDA"));
            string alt = SeriUretici.AlternatifSon(asil, "B");
            Bekle("alternatif son B", alt.Contains("Alternatif son B: B) Gitsin") && !alt.Contains("secim:") && alt.Contains("Alti?") && StoryParser.Parse(alt).Warnings.Count == 0);
        }
    }
}
#endif
