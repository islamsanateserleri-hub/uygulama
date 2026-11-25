#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Asama 7 testi: sifreleme, tasima ve gizleme. Yalniz SAHTE degerler ve test ayar adlari kullanir.
    public static class KurulumTest
    {
        [MenuItem("Hikaye/Kurulum Testi")]
        public static void Calistir()
        {
            const string D = "HS_Test_Duz", K = "HS_Test_Korumali";
            var sb = new StringBuilder();
            int gecen = 0, toplam = 0;
            void Kontrol(string ad, bool ok) { toplam++; if (ok) gecen++; sb.AppendLine((ok ? "GEÇTİ " : "KALDI ") + ad); }

            PlayerPrefs.DeleteKey(D); PlayerPrefs.DeleteKey(K);
            const string sahte = "sahte-deneme-degeri_ÇĞİÖŞÜ-1234567890";
            string k = SirKoruma.Koru(sahte);
            Kontrol("şifreleme sonuç veriyor", !string.IsNullOrEmpty(k));
            Kontrol("şifreli metin düz değeri içermiyor", k != null && !k.Contains("sahte"));
            Kontrol("çözülen değer aynı", SirKoruma.Coz(k) == sahte);
            Kontrol("bozuk veri null döner", SirKoruma.Coz("AAAA") == null && SirKoruma.Coz("bu base64 değil!") == null);

            PlayerPrefs.SetString(D, sahte);
            bool sifreli;
            string okunan = SirKoruma.Oku(D, K, out sifreli);
            Kontrol("taşıma: değer korunuyor", okunan == sahte && sifreli);
            Kontrol("taşıma: düz kayıt silindi", !PlayerPrefs.HasKey(D));
            Kontrol("taşıma: şifreli kayıt var", PlayerPrefs.HasKey(K) && PlayerPrefs.GetString(K) != sahte);
            Kontrol("ikinci okuma aynı", SirKoruma.Oku(D, K, out sifreli) == sahte && sifreli);

            PlayerPrefs.SetString(D, "daha-yeni-sahte-deger");
            Kontrol("eski sürümün yazdığı yeni değer öncelikli", SirKoruma.Oku(D, K, out sifreli) == "daha-yeni-sahte-deger" && !PlayerPrefs.HasKey(D));

            SirKoruma.Yaz(D, K, "   ");
            Kontrol("boş değer iki kaydı da siler", !PlayerPrefs.HasKey(D) && !PlayerPrefs.HasKey(K));
            Kontrol("kayıt yoksa boş döner", SirKoruma.Oku(D, K, out sifreli) == "" && !sifreli);

            PlayerPrefs.SetString(K, "AAAA");
            Kontrol("çözülemeyen kayıt boş döner ve silinmez", SirKoruma.Oku(D, K, out sifreli) == "" && PlayerPrefs.HasKey(K));
            PlayerPrefs.DeleteKey(D); PlayerPrefs.DeleteKey(K); PlayerPrefs.Save();

            string kirli = "hata AIzaSyFAKEFAKEFAKEFAKEFAKEFAKE123 ve x-goog-api-key: abcdef123456 ve ?key=zzz999 ve " + sahte + " yol C:\\Users\\Deneme\\a.txt c:/users/deneme/b";
            string temiz = HataRaporu.Gizle(kirli, sahte, "C:\\Users\\Deneme");
            Kontrol("rapor: AIza anahtarı gizlendi", !temiz.Contains("AIzaSyFAKE"));
            Kontrol("rapor: başlık ve adres anahtarı gizlendi", !temiz.Contains("abcdef123456") && !temiz.Contains("zzz999"));
            Kontrol("rapor: kayıtlı değer gizlendi", !temiz.Contains(sahte));
            Kontrol("rapor: kullanıcı klasörü gizlendi", temiz.ToLowerInvariant().IndexOf("users\\deneme") < 0 && temiz.ToLowerInvariant().IndexOf("users/deneme") < 0);

            sb.Insert(0, System.DateTime.Now.ToString("HH:mm:ss") + " KURULUM TESTİ " + gecen + "/" + toplam + "\n");
            Directory.CreateDirectory("HikayeKomut");
            File.WriteAllText("HikayeKomut/kurulum_test.txt", sb.ToString(), new UTF8Encoding(false));
            Debug.Log("[Hikaye Stüdyosu] Kurulum testi " + gecen + "/" + toplam);
        }
    }
}
#endif
