using System.Collections.Generic;
using System.Text;

namespace HikayeStudyo
{
    // Aşama 15: bölüm şablonları, sezon, alternatif son ve otomatik fragman metinleri. Yalnız metin üretir.
    public static class SeriUretici
    {
        public static readonly string[] SablonAdlari = { "Klasik: seçimli", "Oylama sonucu ile devam", "Gerilim: sert son", "Duygusal: anı" };

        public static string Sablon(int tur, string seri, int bolum, string plan, string oncekiOzet)
        {
            var s = new StringBuilder();
            s.Append("seri: ").Append(seri).Append('\n');
            s.Append("bolum: ").Append(bolum).Append('\n');
            s.Append("baslik: ").Append(bolum).Append(". bölümün başlığını yaz\n");
            if (!string.IsNullOrEmpty(plan)) s.Append("# Sezon planı: ").Append(plan).Append('\n');
            if (!string.IsNullOrEmpty(oncekiOzet)) s.Append("# Önceki bölüm: ").Append(oncekiOzet.Replace("\n", " ")).Append('\n');
            s.Append('\n');
            switch (tur)
            {
                case 1:
                    s.Append("sonuc: %60 A\n");
                    s.Append("mekan: oda_gunduz\n");
                    s.Append("kanca: Kararını verdi ama bunu beklemiyordu.\n");
                    s.Append("[Anlatici|normal|dis] Arda sonunda seçimini yaptı.\n");
                    s.Append("[Arda|dusunceli|dis] Artık geri dönemem.\n");
                    s.Append("[Anne|sasirmis|dis] Sen bunu nereden biliyorsun?\n");
                    s.Append("devami: Yarın aynı saatte\n");
                    s.Append("son: yumusak\n");
                    break;
                case 2:
                    s.Append("mekan: oda_gece\n");
                    s.Append("kanca: Kapı gece üçte çaldı.\n");
                    s.Append("[Anlatici|normal|dis] Evde herkes uyuyordu.\n");
                    s.Append("efekt: kapi\n");
                    s.Append("[Arda|korkmus|dis] Kim var orada?\n");
                    s.Append("[Anne|korkmus|dis] Sakın açma!\n");
                    s.Append("son: sert\n");
                    break;
                case 3:
                    s.Append("mekan: sofra\n");
                    s.Append("kanca: O fotoğrafı yıllardır saklamıştı.\n");
                    s.Append("[Anne|uzgun|dis] Bunu sana hiç anlatmadım.\n");
                    s.Append("ani: sepya\n");
                    s.Append("[Anlatici|normal|dis] Yıllar önce bu sofrada dört tabak vardı.\n");
                    s.Append("ani: bitir\n");
                    s.Append("[Arda|aglamakli|dis] Neden şimdi söylüyorsun?\n");
                    s.Append("devami: Gerçek gelecek bölümde\n");
                    s.Append("son: yumusak\n");
                    break;
                default:
                    s.Append("mekan: oda_gunduz\n");
                    s.Append("kanca: Bu sabah her şey değişti.\n");
                    s.Append("[Anlatici|normal|dis] Arda uyandığında ev sessizdi.\n");
                    s.Append("[Arda|sasirmis|dis] Herkes nerede?\n");
                    s.Append("[Anne|uzgun|dis] Sana bir şey söylemem gerekiyor.\n");
                    s.Append("oylama: Arda ne yapmalı?\n");
                    s.Append("secim: A) Dinlesin / B) Çekip gitsin\n");
                    s.Append("devami: Sonucu yorumlar belirleyecek\n");
                    s.Append("son: yumusak\n");
                    break;
            }
            s.Append("ozet: Bu bölümde ne olduğunu bir cümleyle yaz\n");
            return s.ToString();
        }

        // Alternatif son: seçim (yoksa son) satırından öncesi aynen kalır, sonrası yeniden yazılır
        public static string AlternatifSon(string metin, string harf)
        {
            var l = new List<string>(metin.Replace("\r", "").Split('\n'));
            int kes = -1; string secenek = "";
            for (int i = 0; i < l.Count; i++)
            {
                string n = Util.Norm(l[i]);
                if (n.StartsWith("secim:"))
                {
                    var o = l[i].Substring(l[i].IndexOf(':') + 1).Split('/');
                    int j = harf == "B" ? 1 : 0;
                    if (j < o.Length) secenek = o[j].Trim();
                    kes = i; break;
                }
            }
            if (kes < 0)
                for (int i = 0; i < l.Count; i++)
                {
                    string n = Util.Norm(l[i]);
                    if (n.StartsWith("son:") || n.StartsWith("devami:")) { kes = i; break; }
                }
            if (kes < 0) kes = l.Count;
            var s = new StringBuilder();
            for (int i = 0; i < kes; i++)
            {
                string n = Util.Norm(l[i]);
                if (n.StartsWith("oylama:")) continue;
                s.Append(l[i]).Append('\n');
            }
            s.Append("# ---- Alternatif son ").Append(harf).Append(secenek.Length > 0 ? ": " + secenek : "").Append(" ----\n");
            s.Append("[Anlatici|normal|dis] Bu sonda neler olduğunu buraya yaz.\n");
            s.Append("son: yumusak Hangi sonu seçerdin?\n");
            return s.ToString();
        }

        static int Guc(StoryItem it)
        {
            int g = 0;
            switch (Util.Norm(it.Emotion))
            {
                case "aglamakli": case "korkmus": case "kizgin": g = 3; break;
                case "sasirmis": case "uzgun": g = 2; break;
            }
            if (it.Text.EndsWith("!") || it.Text.EndsWith("?")) g += 1;
            return g;
        }

        // Fragman: kanca + en güçlü en fazla 'adet' replik (özgün sırayla, mekânlarıyla) + "yakında" kartı.
        // Replik satırları aynen kopyalanır; böylece hazır sesler yeniden üretilmeden kullanılabilir.
        public static string Fragman(string metin, int adet, out List<StoryItem> secilen)
        {
            secilen = new List<StoryItem>();
            var satirlar = metin.Replace("\r", "").Split('\n');
            var p = StoryParser.Parse(metin);
            var aday = new List<StoryItem>();
            foreach (var it in p.Items) if (it.Type == ItemType.Dialogue) aday.Add(it);
            var sirali = new List<StoryItem>(aday);
            sirali.Sort((a, b) => Guc(a) != Guc(b) ? Guc(b).CompareTo(Guc(a)) : a.Line.CompareTo(b.Line));
            for (int i = 0; i < sirali.Count && i < adet; i++) secilen.Add(sirali[i]);
            secilen.Sort((a, b) => a.Line.CompareTo(b.Line));

            var s = new StringBuilder();
            foreach (var satir in satirlar)
            {
                string n = Util.Norm(satir);
                if (n.StartsWith("seri:") || n.StartsWith("baslik:") || n.StartsWith("oyuncu:") || n.StartsWith("etiket:")) s.Append(satir.Trim()).Append('\n');
            }
            s.Append("# Fragman (otomatik)\n");
            string mekan = "", yazilan = "";
            int sonraki = 0;
            bool kanca = false;
            for (int i = 0; i < satirlar.Length && sonraki < secilen.Count; i++)
            {
                string n = Util.Norm(satirlar[i]);
                if (n.StartsWith("mekan:") || n.StartsWith("mekan3d:")) mekan = satirlar[i].Trim();
                if (n.StartsWith("kanca:") && !kanca) { s.Append(satirlar[i].Trim()).Append('\n'); kanca = true; }
                if (i + 1 != secilen[sonraki].Line) continue;
                if (mekan.Length > 0 && mekan != yazilan) { s.Append(mekan).Append('\n'); yazilan = mekan; }
                s.Append(satirlar[i].Trim()).Append('\n');
                sonraki++;
            }
            s.Append("yazi: ").Append(p.Bolum > 0 ? p.Bolum + ". BÖLÜM YAKINDA" : "YAKINDA").Append('\n');
            s.Append("son: sert\n");
            return s.ToString();
        }
    }
}
