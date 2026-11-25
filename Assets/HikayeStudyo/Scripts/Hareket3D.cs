using System.Collections.Generic;

namespace HikayeStudyo
{
    // Asama 10: karakter hareketi ve etkilesim komutlarinin adlari, gecerli degerleri ve klip adaylari.
    // Komutlar parser'da ItemType.Direct olur (Character = komut adi, Arg = deger); calistirma Sahne3D.Hareket.cs'dedir.
    // Editor testi (Hikaye > Hareket Testi) ayni klip listelerini kullanir.
    public static class Hareket3D
    {
        public static readonly string[] Komutlar =
            { "sarilma", "el_tut", "yuzyuze", "ayril", "tepki_ver", "git", "tut", "birak", "kapi", "kalk", "yatak" };
        public static readonly string[] Tepkiler =
            { "sasir", "kork", "uzul", "agla", "sevin", "kiz", "dusun", "onayla", "reddet", "el_salla" };
        public static readonly string[] Esyalar =
            { "bardak", "telefon", "mektup", "kitap", "cicek", "ilac", "tabak", "anahtar" };

        // Karakter3D.Find adaylari (sirayla denenir)
        public static readonly string[] KlipYuru = { "yuru" };
        public static readonly string[] KlipKos = { "kos" };
        public static readonly string[] KlipDur = { "dur" };
        public static readonly string[] KlipOtur = { "otur", "oturarak_konus" };
        public static readonly string[] KlipOturKonus = { "oturarak_konus", "otur" };
        public static readonly string[] KlipYat = { "yat", "uyu", "yatarak_uyu" };
        public static readonly string[] KlipKalkOturma = { "sitting_exit", "ayaga_kalk" };
        public static readonly string[] KlipKalkYatma = { "ayaga_kalk" };

        // Bos dizi = klip aranmaz, insan iskeletiyle (kol/bas pozu) yapilir
        public static string[] TepkiKlipleri(string tepki)
        {
            switch (tepki)
            {
                case "sasir": return new[] { "sasir", "tepki" };
                case "kork": return new[] { "kork", "tepki" };
                case "uzul": return new[] { "uzgun" };
                case "agla": return new[] { "agla", "uzgun" };
                case "kiz": return new[] { "tepki" };
                case "dusun": return new[] { "dusun" };
                case "el_salla": return new[] { "el_salla" };
            }
            return new string[0];
        }

        public static bool Tanir(string key)
        {
            string k = Util.Norm(key);
            foreach (var x in Komutlar) if (x == k) return true;
            return false;
        }

        // "Anne, Baba" / "Anne Baba" -> ["Anne", "Baba"]
        public static string[] Parcala(string val)
        {
            return (val ?? "").Split(new[] { ' ', ',', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        // "ayril:" disindaki komutlarda komutun ilgilendirdigi ilk karakter
        public static string IlkKarakter(string val)
        {
            var p = Parcala(val);
            return p.Length > 0 ? p[0] : "";
        }

        // Gecersizse Turkce uyari metni, gecerliyse null (StoryParser cagirir)
        public static string Dogrula(string key, string val)
        {
            var p = Parcala(val);
            switch (key)
            {
                case "sarilma":
                case "el_tut":
                case "yuzyuze":
                    return p.Length == 2 && Util.Norm(p[0]) != Util.Norm(p[1])
                        ? null : "'" + key + ": Karakter1 Karakter2' biçiminde iki farklı karakter yaz (örn. " + key + ": Anne Baba).";
                case "ayril":
                    return p.Length <= 1 ? null : "'ayril:' ya da 'ayril: Karakter' biçiminde yaz.";
                case "tepki_ver":
                    if (p.Length < 2 || p.Length > 3) return "'tepki_ver: Karakter tepki [Diger]' biçiminde yaz (örn. tepki_ver: Baba sasir).";
                    return Util.In(p[1], Tepkiler) ? null : "'tepki_ver:' tepkisi '" + p[1] + "' bilinmiyor (" + string.Join(", ", Tepkiler) + ").";
                case "git":
                    if (p.Length < 2 || p.Length > 3) return "'git: Karakter hedef [kos]' biçiminde yaz (örn. git: Anne pencere).";
                    return p.Length == 2 || Util.Norm(p[2]) == "kos" || Util.Norm(p[2]) == "yuru"
                        ? null : "'git:' üçüncü değeri 'kos' ya da 'yuru' olmalı.";
                case "tut":
                    if (p.Length < 2 || p.Length > 3) return "'tut: Karakter esya [sag|sol]' biçiminde yaz (örn. tut: Anne bardak).";
                    if (!Util.In(p[1], Esyalar)) return "'tut:' eşyası '" + p[1] + "' bilinmiyor (" + string.Join(", ", Esyalar) + ").";
                    return p.Length == 2 || Util.Norm(p[2]) == "sag" || Util.Norm(p[2]) == "sol" ? null : "'tut:' eli 'sag' ya da 'sol' olmalı.";
                case "birak":
                    return p.Length == 1 ? null : "'birak: Karakter' biçiminde yaz.";
                case "kalk":
                    return p.Length == 1 ? null : "'kalk: Karakter' biçiminde yaz.";
                case "kapi":
                    {
                        string son = p.Length > 0 ? Util.Norm(p[p.Length - 1]) : "";
                        return p.Length >= 1 && p.Length <= 2 && (son == "ac" || son == "kapat")
                            ? null : "'kapi: ac', 'kapi: kapat' ya da 'kapi: Karakter ac' biçiminde yaz.";
                    }
                case "yatak":
                    return p.Length == 2 && (Util.Norm(p[1]) == "yat" || Util.Norm(p[1]) == "otur")
                        ? null : "'yatak: Karakter yat' ya da 'yatak: Karakter otur' biçiminde yaz.";
            }
            return null;
        }
    }
}
