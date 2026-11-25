#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Aşama 8: karakter kimlik kartlarının Editör denemesi. Sonuç: HikayeKomut/kimlik_test.txt
    // Köprüden: menu: Hikaye/Kimlik Testi   (kullanıcının kart klasörüne dokunmaz)
    public static class KimlikTest
    {
        static StringBuilder rapor;
        static int gecen, kalan;

        static void Bekle(string ad, bool kosul)
        {
            if (kosul) gecen++; else kalan++;
            rapor.AppendLine((kosul ? "GECTI  " : "KALDI  ") + ad);
        }

        [MenuItem("Hikaye/Kimlik Testi")]
        public static void Calistir()
        {
            rapor = new StringBuilder(); gecen = kalan = 0;
            bool eskiEtkin = KimlikDeposu.Etkin;
            string eskiKlasor = KimlikDeposu.KlasorDegistir;
            string gecici = Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/kimlik_test_gecici");
            try
            {
                if (Directory.Exists(gecici)) Directory.Delete(gecici, true);
                KimlikDeposu.KlasorDegistir = gecici;
                KimlikDeposu.Etkin = true;
                Denemeler();
            }
            catch (System.Exception e) { kalan++; rapor.AppendLine("KALDI  istisna: " + e); }
            finally
            {
                KimlikDeposu.Etkin = eskiEtkin;
                KimlikDeposu.KlasorDegistir = eskiKlasor;
                try { if (Directory.Exists(gecici)) Directory.Delete(gecici, true); } catch { }
            }
            rapor.Insert(0, "Kimlik testi: " + gecen + " gecti, " + kalan + " kaldi\n");
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "HikayeKomut/kimlik_test.txt"), rapor.ToString(), new UTF8Encoding(false));
            Debug.Log("Kimlik testi: " + gecen + " gecti, " + kalan + " kaldi");
        }

        static void Denemeler()
        {
            var asil = Resources.Load<StoryLibrary>("StoryLibrary");
            var katalog = KarakterKatalogu.Yukle();
            Bekle("kutuphane ve katalog yuklendi", asil != null && katalog != null && katalog.karakterler.Count > 0);
            var arda = asil.FindCharacter("Arda");
            Bekle("Arda kutuphanede", arda != null);
            Bekle("kart yokken asil kutuphane doner", KimlikDeposu.Uygula(asil) == asil);

            var pr = StoryParser.Parse("[Arda|uzgun|dis] Merhaba.\n[Anne|normal|dis] Gel oğlum.\n[Arda|normal|ic] Neden?");
            StoryItem sArda = pr.Items[0], sAnne = pr.Items[1], sIc = pr.Items[2];
            string kArda = asil.ClipKey(sArda), kAnne = asil.ClipKey(sAnne), kIc = asil.ClipKey(sIc);

            // 1) hiç değiştirmeden kaydetmek görünümü ve ses anahtarını değiştirmez
            Bekle("kart kaydedildi", KimlikDeposu.Kaydet(KarakterKimligi.Karakterden(arda, katalog)) == null);
            var l1 = KimlikDeposu.Uygula(asil);
            var a1 = l1.FindCharacter("Arda");
            Bekle("kopya olusturuldu", l1 != asil && a1 != arda);
            Bekle("olcek ayni", Mathf.Approximately(a1.scale, arda.scale));
            Bekle("model, sac, sakal, yer ayni", a1.model3D == arda.model3D && a1.hairStyle == arda.hairStyle && a1.beard == arda.beard && a1.side == arda.side);
            Bekle("renkler ayni", KarakterKimligi.Renk(a1.hair) == KarakterKimligi.Renk(arda.hair) && KarakterKimligi.Renk(a1.shirt) == KarakterKimligi.Renk(arda.shirt)
                && KarakterKimligi.Renk(a1.skin) == KarakterKimligi.Renk(arda.skin) && KarakterKimligi.Renk(a1.pants) == KarakterKimligi.Renk(arda.pants));
            Bekle("ses anahtarlari ayni", l1.ClipKey(sArda) == kArda && l1.ClipKey(sAnne) == kAnne && l1.ClipKey(sIc) == kIc);
            Object.DestroyImmediate(l1);

            // 2) değişiklikler yalnız o karaktere işler, asıl kütüphane değişmez
            float eskiOlcek = arda.scale; string eskiSes = arda.voice; var eskiModel = arda.model3D; int eskiSayi = asil.characters.Count;
            string model = katalog.karakterler[0].kimlik;
            var k = KimlikDeposu.Oku("arda");   // büyük/küçük harf fark etmez
            Bekle("kart okundu", k != null && k.ad == arda.id);
            k.boyCm = 150; k.gorunum = model; k.ses = eskiSes == "Zephyr" ? "Puck" : "Zephyr"; k.icSes = ""; k.konusmaTarzi = "yavaş ve kısık"; k.sacRengi = "#B23A1E";
            Bekle("degisiklik kaydedildi", KimlikDeposu.Kaydet(k) == null);
            var l2 = KimlikDeposu.Uygula(asil);
            var a2 = l2.FindCharacter("Arda");
            Bekle("boy 150 cm", Mathf.Abs(a2.scale - 150f / 175f) < 0.001f);
            Bekle("3D model atandi (" + model + ")", a2.model3D != null && a2.model3D == katalog.KarakterPrefab(model));
            Bekle("sac rengi", KarakterKimligi.Renk(a2.hair) == "#B23A1E");
            Bekle("ses karttan", l2.VoiceFor(sArda) == k.ses && l2.VoiceFor(sIc) == k.ses);
            Bekle("tarz tona eklendi (" + l2.ToneFor(sArda) + ")", l2.ToneFor(sArda).EndsWith(", yavaş ve kısık"));
            Bekle("Arda ses anahtari degisti", l2.ClipKey(sArda) != kArda);
            Bekle("Anne ses anahtari ayni", l2.ClipKey(sAnne) == kAnne);
            Bekle("asil kutuphane degismedi", arda.scale == eskiOlcek && arda.voice == eskiSes && arda.model3D == eskiModel && asil.characters.Count == eskiSayi && string.IsNullOrEmpty(arda.konusmaTarzi));
            Object.DestroyImmediate(l2);

            // 3) yeni karakter
            Bekle("bosluklu ad reddedilir", KimlikDeposu.Kaydet(new KarakterKimligi { ad = "Ali Can" }) != null);
            Bekle("anlatici reddedilir", KimlikDeposu.Kaydet(new KarakterKimligi { ad = "Anlatıcı" }) != null);
            Bekle("yeni kart", KimlikDeposu.Kaydet(new KarakterKimligi { ad = "Şeyma", boyCm = 125, ses = "Leda", yer = 1 }) == null);
            var l3 = KimlikDeposu.Uygula(asil);
            var s = l3.FindCharacter("Şeyma");
            Bekle("yeni karakter eklendi", s != null && !l3.IsNarrator(s) && s.voice == "Leda" && s.side == 1 && s.model3D == null);
            Bekle("denetci yeni karakteri tanir", HikayeDenetci.KarakterAdlari(l3).Contains("Şeyma"));
            Bekle("denetci hatasi yok", HikayeDenetci.HataSayisi(HikayeDenetci.Denetle("[Şeyma|mutlu|dis] Selam!", l3, new string[0])) == 0);
            Object.DestroyImmediate(l3);

            // 4) kaldırma: kart _silinen klasörüne taşınır, varsayılana dönülür
            Bekle("kart kaldirildi", KimlikDeposu.Kaldir("Arda") == null && !KimlikDeposu.Var("Arda"));
            Bekle("kaldirilan kart saklandi", Directory.GetFiles(Path.Combine(KimlikDeposu.Klasor, "_silinen"), "*.json").Length == 1);
            var l4 = KimlikDeposu.Uygula(asil);
            Bekle("Arda varsayilana dondu", l4.ClipKey(sArda) == kArda && l4.FindCharacter("Arda").model3D == eskiModel);
            if (l4 != asil) Object.DestroyImmediate(l4);

            // 5) bozuk dosya diğer kartları engellemez
            File.WriteAllText(Path.Combine(KimlikDeposu.Klasor, "bozuk.json"), "{ bozuk");
            Bekle("bozuk kart atlanir", KimlikDeposu.Hepsi().Count == 1);

            KimlikDeposu.Etkin = false;
            Bekle("etkin degilken asil doner", KimlikDeposu.Uygula(asil) == asil);
        }
    }
}
#endif
