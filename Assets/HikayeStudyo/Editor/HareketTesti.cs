using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 10: katalogdaki her karakterin hangi hareketleri yapabildigini iskeletine ve kliplerine bakarak raporlar.
    // Menu: Hikaye > Hareket Testi   (kopru: menu: Hikaye/Hareket Testi)   Sonuc: HikayeKomut/hareket_testi.txt
    // EVET = yapabilir, YEDEK = klip yok ama basit iskelet pozu / gecisle yapilir, HAYIR = yapamaz.
    // Yalniz varlik denetimidir; hareketin goruntusu ayrica oynatarak kontrol edilmelidir.
    public static class HareketTesti
    {
        [MenuItem("Hikaye/Hareket Testi")]
        public static void Calistir()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HAREKET TESTİ (EVET = yapabilir, YEDEK = klipsiz basit çözüm, HAYIR = yapamaz)");
            var kat = KarakterKatalogu.Yukle();
            int sayi = 0;
            if (kat == null) sb.AppendLine("HATA: Resources/KarakterKatalogu bulunamadı.");
            else
                foreach (var kayit in kat.karakterler)
                {
                    if (kayit == null || kayit.prefab == null) continue;
                    var go = (GameObject)Object.Instantiate(kayit.prefab);
                    try { Rapor(sb, kayit.kimlik, go); sayi++; }
                    catch (System.Exception e) { sb.AppendLine().AppendLine("== " + kayit.kimlik + "  HATA: " + e.Message); }
                    finally { Object.DestroyImmediate(go); }
                }
            sb.AppendLine().AppendLine("Toplam karakter: " + sayi);
            Directory.CreateDirectory(UzaktanKomut.Dir);
            File.WriteAllText(Path.Combine(UzaktanKomut.Dir, "hareket_testi.txt"), sb.ToString(), new UTF8Encoding(false));
            Debug.Log("[Hikaye Stüdyosu] Hareket testi bitti (" + sayi + " karakter): HikayeKomut/hareket_testi.txt");
        }

        static void Rapor(StringBuilder sb, string kimlik, GameObject go)
        {
            var k = go.GetComponent<Karakter3D>();
            var an = go.GetComponent<Animator>();
            if (an == null) an = go.GetComponentInChildren<Animator>();
            bool insan = an != null && an.avatar != null && an.isHuman;
            bool kol = KolPozu.InsanAnimator(go) != null;
            bool omurga = Var(an, insan, HumanBodyBones.Spine);
            bool kalca = Var(an, insan, HumanBodyBones.Hips);
            bool bas = Var(an, insan, HumanBodyBones.Head);

            sb.AppendLine();
            sb.Append("== ").Append(kimlik)
              .Append("  insan_iskeleti=").Append(E(insan))
              .Append("  kol_kemikleri=").Append(E(kol))
              .Append("  omurga=").Append(E(omurga))
              .Append("  kalca=").Append(E(kalca))
              .Append("  bas=").Append(E(bas))
              .Append("  klip_sayisi=").AppendLine(k != null && k.clips != null ? k.clips.Count.ToString() : "0");

            string yuru = Klip(k, Hareket3D.KlipYuru);
            string kos = Klip(k, Hareket3D.KlipKos);
            string otur = Klip(k, Hareket3D.KlipOtur);
            string yat = Klip(k, Hareket3D.KlipYat);
            string kalkO = Klip(k, Hareket3D.KlipKalkOturma);
            string kalkY = Klip(k, Hareket3D.KlipKalkYatma);
            string dur = Klip(k, Hareket3D.KlipDur);
            string eksikKol = !insan ? "insan iskeleti yok" : "kol kemikleri eksik";

            Satir(sb, "Sarılma (sarilma)", kol ? "EVET (kol pozu)" : "HAYIR (" + eksikKol + ")");
            Satir(sb, "El tutma (el_tut)", kol && kalca ? "EVET (kol pozu)" : "HAYIR (" + (kol ? "kalça kemiği yok" : eksikKol) + ")");
            Satir(sb, "Yürüme (yuru, git)", KlipDurumu(yuru));
            Satir(sb, "Koşma (kos, git ... kos)", kos.Length > 0 ? "EVET (klip: " + kos + ")" : (yuru.Length > 0 ? "YEDEK (yürüme klibi hızlı gider)" : "HAYIR (klip yok)"));
            Satir(sb, "Oturma (poz: oturuyor)", KlipDurumu(otur));
            Satir(sb, "Yatma (poz: yatiyor)", yat.Length > 0 ? "EVET (klip: " + yat + ")" : "HAYIR (klip yok; yatak: komutunu kullan)");
            Satir(sb, "Yatak (yatak: yat)", yat.Length > 0 ? "EVET (klip: " + yat + ")" : (kol ? "YEDEK (gövde sırtüstü yatırılır)" : "HAYIR (klip yok, " + eksikKol + ")"));
            Satir(sb, "Yatak (yatak: otur)", KlipDurumu(otur));
            Satir(sb, "Oturmadan kalkma (kalk)", kalkO.Length > 0 ? "EVET (klip: " + kalkO + ")" : (dur.Length > 0 ? "YEDEK (durgun duruşa yumuşak geçiş)" : "HAYIR (klip yok)"));
            Satir(sb, "Yataktan kalkma (kalk)", kalkY.Length > 0 ? "EVET (klip: " + kalkY + ")" : (dur.Length > 0 ? "YEDEK (yatağın yanında ayakta belirir)" : "HAYIR (klip yok)"));
            Satir(sb, "Eşya tutma (tut)", kol ? "EVET (kol pozu)" : "HAYIR (" + eksikKol + ")");
            Satir(sb, "Kapı açma (kapi)", kol && yuru.Length > 0 ? "EVET (yürüme + kol pozu)" : "HAYIR (" + (kol ? "yürüme klibi yok" : eksikKol) + "; 'kapi: ac' yine kapıyı açar)");
            Satir(sb, "Kapıdan girme/çıkma (gir, cikar ... kapi)", yuru.Length > 0 ? "EVET (klip: " + yuru + ")" : "HAYIR (yürüme klibi yok, kayarak girer)");
            Satir(sb, "Birbirine bakma (yuzyuze, bak)", bas ? "EVET (gövde dönüşü + baş/göz)" : "YEDEK (yalnız gövde döner)");

            var tepki = new StringBuilder();
            foreach (var t in Hareket3D.Tepkiler)
            {
                string c = Klip(k, Hareket3D.TepkiKlipleri(t));
                if (tepki.Length > 0) tepki.Append(", ");
                tepki.Append(t).Append('=').Append(c.Length > 0 ? "klip" : (kol ? "yedek" : "yok"));
            }
            Satir(sb, "Tepki verme (tepki_ver)", tepki.ToString());
            Satir(sb, "İki karakter etkileşimi", kol && kalca ? "EVET" : "HAYIR (sarılma/el tutma için insan iskeleti gerekir)");
        }

        static string E(bool b) { return b ? "EVET" : "hayir"; }

        static bool Var(Animator an, bool insan, HumanBodyBones b) { return insan && an.GetBoneTransform(b) != null; }

        static string Klip(Karakter3D k, string[] adaylar)
        {
            if (k == null) return "";
            foreach (var n in adaylar)
            {
                var c = k.Find(n);
                if (c != null) return c.name;
            }
            return "";
        }

        static string KlipDurumu(string klip) { return klip.Length > 0 ? "EVET (klip: " + klip + ")" : "HAYIR (klip yok)"; }

        static void Satir(StringBuilder sb, string ad, string durum)
        {
            sb.Append("   ").Append(ad.PadRight(44)).Append(durum).AppendLine();
        }
    }
}
