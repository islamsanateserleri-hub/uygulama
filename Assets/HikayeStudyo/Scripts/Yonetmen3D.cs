using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HikayeStudyo
{
    // Yonetmen komutlarinin gecerli degerleri (StoryParser uyarilari ve Studyo dogrulamasi icin)
    public static class YonetmenTablo
    {
        public static readonly string[] Mekanlar = { "oda", "ev", "mutfak", "yatak_odasi", "hastane", "sokak", "koy", "park", "araba", "yok" };
        public static readonly string[] Havalar = { "yagmur", "kar", "sis", "yok" };
        public static readonly string[] Zamanlar = { "sabah", "gun", "gun_batimi", "gece" };
        public static readonly string[] Atmosferler = { "huzun", "sicak_ani", "kabus", "normal" };
        public static readonly string[] Animlar = { "sepya", "vhs", "soluk", "bitir" };
        public static readonly string[] Gecisler = { "karar", "capraz", "beyaz" };
        public static readonly string[] Isiklar = { "normal", "gunduz", "dramatik", "yumusak", "soguk", "sicak", "karanlik", "mum", "romantik", "umut", "korku", "huzunlu" };
        public static readonly string[] Kameralar =
            { "otomatik", "yakin", "genis", "salla", "tepki", "yaklas", "uzaklas", "goz", "yukaridan", "asagidan", "bulanik", "net", "orta", "omuz", "ters", "iki" };

        public static bool KapiMi(string s)
        {
            return Util.Norm(s).StartsWith("kapi");
        }

        public static bool KameraGecerli(string val)
        {
            var t = (val ?? "").Trim().Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
            return t.Length > 0 && Util.In(t[0], Kameralar);
        }

        // Gecersizse Turkce uyari metni, gecerliyse null
        public static string Dogrula(string key, string val)
        {
            string v = (val ?? "").Trim();
            if (SinematikEfekt.Tanir(key)) return SinematikEfekt.Dogrula(key, v);
            switch (key)
            {
                case "mekan3d":
                    {
                        string hata;
                        return MekanTarifi.Coz(v, out hata) != null ? null : "'mekan3d: " + v + "': " + hata;
                    }
                case "hava":
                    foreach (var tk in v.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                        if (!Util.In(tk, Havalar)) return "'hava: " + tk + "' bilinmiyor (" + string.Join(", ", Havalar) + ").";
                    return v.Length > 0 ? null : "'hava:' boş (" + string.Join(", ", Havalar) + ").";
                case "zaman":
                    return Util.In(v, Zamanlar) ? null : "'zaman: " + v + "' bilinmiyor (" + string.Join(", ", Zamanlar) + ").";
                case "ani":
                    return Util.In(v, Animlar) ? null : "'ani: " + v + "' bilinmiyor (" + string.Join(", ", Animlar) + ").";
                case "gecis":
                    return Util.In(v, Gecisler) ? null : "'gecis: " + v + "' bilinmiyor (" + string.Join(", ", Gecisler) + ").";
                case "isik":
                    return Util.In(v, Isiklar) ? null : "'isik: " + v + "' bilinmiyor (" + string.Join(", ", Isiklar) + ").";
                case "yer":
                case "gir":
                    {
                        var p = v.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                        return p.Length == 2 ? null : "'" + key + ": Karakter nokta' biçiminde yaz (örn. " + key + ": Anne kapi).";
                    }
                case "figuran":
                    {
                        int n;
                        return int.TryParse(v, out n) && n >= 0 ? null : "'figuran: sayı' biçiminde yaz (örn. figuran: 4).";
                    }
                case "sessizlik":
                    {
                        float f;
                        return float.TryParse(v.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                                              System.Globalization.CultureInfo.InvariantCulture, out f) && f > 0f
                            ? null : "'sessizlik: saniye' biçiminde yaz (örn. sessizlik: 2).";
                    }
            }
            return null;
        }
    }

    // 'Direct' (yonetmen) komutlarini calistirir. StoryPlayer: yield return Yonetmen3D.Uygula(s3d, it);
    public static class Yonetmen3D
    {
        static readonly HashSet<string> uyarildi = new HashSet<string>();

        // "Anne kapi" -> id = "Anne", rest = "kapi" (normalize)
        static void Bol(string arg, out string id, out string rest)
        {
            string a = (arg ?? "").Trim();
            int i = a.IndexOf(' ');
            if (i < 0) { id = a; rest = ""; }
            else { id = a.Substring(0, i); rest = Util.Norm(a.Substring(i + 1)); }
        }

        static void Uyar(string mesaj)
        {
            if (uyarildi.Add(mesaj)) Debug.LogWarning("[Hikaye] " + mesaj);
        }

        // Canli oynatma (zamanli komutlar bekler)
        public static IEnumerator Uygula(Sahne3D s, StoryItem it)
        {
            if (it == null) yield break;
            if (s == null) { Uyar(it.Line + ". satır: '" + it.Character + ":' için 3D sahne yok."); yield break; }
            string key = Util.Norm(it.Character);
            string arg = it.Arg ?? "";
            string n = Util.Norm(arg);
            string id, nokta;
            switch (key)
            {
                case "mekan3d": s.MekanKur(arg); break;
                case "hava": s.HavaAyarla(arg); break;
                case "yer": Bol(arg, out id, out nokta); s.Yer(id, nokta); break;
                case "gir": Bol(arg, out id, out nokta); yield return s.GirCo(id, nokta); break;
                case "cikar": Bol(arg, out id, out nokta); s.CikarKapi(id, nokta); break;
                case "figuran": { int k; int.TryParse(arg.Trim(), out k); s.Figuran(k); } break;
                case "atmosfer":
                    foreach (var tk in arg.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)) s.AtmosferAyarla(tk);
                    break;
                case "ani": s.AniAyarla(n); break;
                case "zaman": s.ZamanAyarla(n); break;
                case "isik": s.IsikAyarla(n); break;
                case "gecis": yield return s.GecisCo(n); break;
                case "sessizlik": yield return s.SessizlikCo(Util.ParseFloat(arg, 2f)); break;
                default: Uyar(it.Line + ". satır: '" + key + ":' yönetmen komutu işlenemedi."); break;
            }
        }

        // "Bu satirdan oynat" atlamasi: bekleme yok, sadece kalici durum kurulur
        public static void Hizli(Sahne3D s, StoryItem it)
        {
            if (s == null || it == null) return;
            string key = Util.Norm(it.Character);
            string arg = it.Arg ?? "";
            string n = Util.Norm(arg);
            switch (key)
            {
                case "mekan3d": s.MekanKur(arg); break;
                case "yer": { string kim, yeri; Bol(arg, out kim, out yeri); s.Yer(kim, yeri); } break;
                case "hava": s.HavaAyarla(arg); break;
                case "figuran": { int k; int.TryParse(arg.Trim(), out k); s.Figuran(k); } break;
                case "atmosfer":
                    foreach (var tk in arg.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)) s.AtmosferAyarla(tk);
                    break;
                case "ani": s.AniAyarla(n); break;
                case "zaman": s.ZamanAyarla(n); break;
                case "isik": s.IsikAyarla(n); break;
            }
            if (s.Atmosfer != null) s.Atmosfer.Anlik();
        }
    }
}
