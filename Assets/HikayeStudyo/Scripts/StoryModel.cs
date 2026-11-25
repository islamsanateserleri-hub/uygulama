using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HikayeStudyo
{
    public enum ItemType
    {
        Bak, Dinle, AltyaziStil,   // bak: / dinle: / altyazi: komutlari (YuzSahne.cs, KaraokeAltyazi.cs)
        Dialogue, Background, Hook, Rewind, Title, Wait, Sfx, Remove, Clear,
        Camera, Atmosphere, Music, Pose, Headlights, Flash, Insert, End, Cast, Choice, Result,
        DevamKarti,   // devami: "devami gelecek bolumde" karti (Asama 15)
        Direct   // yonetmen komutlari: mekan3d, yer, gir, cikar (kapi), figuran, atmosfer (huzun...), ani, zaman, gecis, sessizlik, isik
    }

    public class StoryItem
    {
        public ItemType Type;
        public int Line;                 // hikaye dosyasindaki satir numarasi
        public string Character = "";
        public string Emotion = "normal";
        public bool Inner;               // ic ses: agiz oynamaz
        public string Motion = "";       // kos, yuru, saril, zipla, egil, sarsil, cik
        public string Text = "";         // ekranda gorunen metin (ses etiketleri ve *vurgu* isaretleri temizlenmis)
        public string Speech = "";       // seslendirmeye giden metin (<sigh>, <sob> gibi etiketlerle)
        public string Vurgulu = "";      // altyazi metni, *vurgu* isaretleri korunmus (karaoke altyazi icin)
        public string Arg = "";
        public string Soru = "";          // secim karti sorusu (oylama:)
        public float Seconds;
        public int VoiceIndex;           // 1'den baslayan konusma sirasi
    }

    public class ParseResult
    {
        public readonly List<StoryItem> Items = new List<StoryItem>();
        public readonly List<string> Warnings = new List<string>();
        public int DialogueCount;
        public string Seri = "", Baslik = "";   // seri: / baslik: komutlari (kapak ve aciklama icin)
        public int Bolum;                        // bolum: 3
        public int KapakSatir;                   // kapak: 24  (kapak karesi bu dosya satirindan/sonraki ilk konusmadan alinir; 0 = otomatik)
        public readonly List<string> Etiketler = new List<string>();   // etiket: a, b, c  (aciklamaya #a #b #c olarak eklenir)
        // Asama 14 (devamlilik): ozet/durum/olay/gecmis/iliski/soru/cozuldu/plan. Videoyu etkilemez.
        public readonly List<SeriNotu> SeriNotlari = new List<SeriNotu>();
    }

    public class SeriNotu
    {
        public int Satir;
        public string Tur = "";                 // normalize komut adi
        public string Kim = "", Kim2 = "";      // karakter(ler); plan icin Kim = bolum numarasi
        public string Deger = "";

        public static readonly string[] Turler = { "ozet", "durum", "olay", "gecmis", "iliski", "soru", "cozuldu", "plan" };
        public static readonly string[] Durumlar = { "hayatta", "olu", "kayip", "hastanede", "uzakta" };

        // Hatali yazimda aciklama doner (null = tamam)
        public static string Coz(string tur, string val, int satir, out SeriNotu not)
        {
            not = new SeriNotu { Satir = satir, Tur = tur, Deger = (val ?? "").Trim() };
            if (not.Deger.Length == 0) return "'" + tur + ":' boş olamaz.";
            if (tur == "ozet" || tur == "olay" || tur == "soru" || tur == "cozuldu") return null;
            int eq = not.Deger.IndexOf('=');
            string ornek = tur == "durum" ? "durum: Dede = olu" : tur == "gecmis" ? "gecmis: Anne = yıllarca yurt dışında çalıştı"
                : tur == "iliski" ? "iliski: Anne - Arda = anne ve oğul" : "plan: 3 = mektup bulunur";
            if (eq < 0) return "'" + ornek + "' biçiminde yaz.";
            string sol = not.Deger.Substring(0, eq).Trim();
            not.Deger = not.Deger.Substring(eq + 1).Trim();
            if (sol.Length == 0 || not.Deger.Length == 0) return "'" + ornek + "' biçiminde yaz.";
            if (tur == "iliski")
            {
                int t = sol.IndexOf('-');
                if (t < 0) return "'" + ornek + "' biçiminde yaz.";
                not.Kim = sol.Substring(0, t).Trim(); not.Kim2 = sol.Substring(t + 1).Trim();
                return not.Kim.Length > 0 && not.Kim2.Length > 0 ? null : "'" + ornek + "' biçiminde yaz.";
            }
            not.Kim = sol;
            if (tur == "plan") { int n; return int.TryParse(sol, out n) && n > 0 ? null : "'" + ornek + "' biçiminde yaz (bölüm numarası)."; }
            if (tur == "durum")
            {
                not.Deger = Util.Norm(not.Deger);
                if (!Util.In(not.Deger, Durumlar)) return "durum '" + not.Deger + "' bilinmiyor (" + string.Join(", ", Durumlar) + ").";
            }
            return null;
        }
    }

    public static class Util
    {
        public static readonly string[] Emotions =
            { "normal", "mutlu", "sevgi", "uzgun", "aglamakli", "korkmus", "sasirmis", "kizgin", "yorgun", "hasta", "dusunceli", "fisilti" };
        public static readonly string[] Motions = { "kos", "yuru", "saril", "zipla", "egil", "sarsil", "cik" };
        public static readonly string[] Poses = { "ayakta", "yatiyor", "oturuyor" };

        // "Ağlamaklı" -> "aglamakli". Isimler buyuk/kucuk harf ve Turkce harf farkina bakmaz.
        // Seri sablonlarindaki "kendin yaz" yerleri; videoya gitmemeli
        // Yalnizca sablonlarin yazdigi metinler birebir yakalanir; "numarani buraya yaz" gibi gercek replikler serbest.
        static readonly string[] YerTutucular =
        {
            "Bu bölümde ne olduğunu bir cümleyle yaz", "Bu sonda neler olduğunu buraya yaz", "Buraya ilk cümleni yaz",
            "Buraya anlatıcı metnini yaz", "Metni buraya yaz"
        };

        public static bool SablonYerTutucu(string satir)
        {
            var v = (satir ?? "").Trim();
            int i = v.StartsWith("[") ? v.IndexOf(']') : v.IndexOf(':');
            if (i >= 0) v = v.Substring(i + 1);
            v = v.Trim().TrimEnd('.', '!', ' ');
            if (v.Length == 0) return false;
            var n = Norm(v);
            if (char.IsDigit(v[0]) && n.EndsWith(Norm("bölümün başlığını yaz"))) return true;
            foreach (var y in YerTutucular) if (n == Norm(y)) return true;
            return false;
        }

        public static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s.Trim())
            {
                switch (ch)
                {
                    case 'İ': case 'I': case 'ı': case 'i': sb.Append('i'); break;
                    case 'Ğ': case 'ğ': sb.Append('g'); break;
                    case 'Ü': case 'ü': sb.Append('u'); break;
                    case 'Ş': case 'ş': sb.Append('s'); break;
                    case 'Ö': case 'ö': sb.Append('o'); break;
                    case 'Ç': case 'ç': sb.Append('c'); break;
                    case ' ': case '-': sb.Append('_'); break;
                    default: sb.Append(char.ToLowerInvariant(ch)); break;
                }
            }
            return sb.ToString();
        }

        public static bool In(string value, string[] list)
        {
            var n = Norm(value);
            foreach (var x in list) if (x == n) return true;
            return false;
        }

        // Sabit (FNV-1a) ozet: ayni ses + ton + metin her zaman ayni dosya adini verir
        public static string Hash(string s)
        {
            uint h = 2166136261;
            foreach (byte b in Encoding.UTF8.GetBytes(s)) { h ^= b; h *= 16777619; }
            return h.ToString("x8");
        }

        public static float ParseFloat(string s, float def)
        {
            if (float.TryParse((s ?? "").Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return v;
            return def;
        }

        // Duygu -> Gemini'ye verilen ses tonu
        public static string ToneFor(StoryItem it)
        {
            string t;
            switch (Norm(it.Emotion))
            {
                case "mutlu": t = "sevinçli"; break;
                case "sevgi": t = "sevgi dolu, sıcak"; break;
                case "uzgun": t = "üzgün"; break;
                case "aglamakli": t = "ağlamaklı, titrek"; break;
                case "korkmus": t = "korkmuş, nefes nefese"; break;
                case "sasirmis": t = "şaşkın"; break;
                case "kizgin": t = "öfkeli"; break;
                case "yorgun": t = "yorgun, halsiz"; break;
                case "hasta": t = "hasta, bitkin"; break;
                case "dusunceli": t = "düşünceli"; break;
                case "fisilti": t = "fısıltıyla"; break;
                default: t = "doğal"; break;
            }
            return it.Inner ? "iç ses gibi sakin ve derin, " + t : t;
        }
    }

    // Satir ici ses etiketleri: (iç çek) -> <sigh>, (ağla) -> <sob> ... Altyazida gorunmezler.
    public static class SesEtiketleri
    {
        // Turkce kisa yazim -> Gemini etiketi (Gemini etiketleri Ingilizce ister)
        public static readonly string[,] Kisayol =
        {
            { "iç çek", "sigh" }, { "ic cek", "sigh" }, { "of", "sigh" },
            { "ağla", "sob" }, { "agla", "sob" }, { "hıçkır", "sob" }, { "hickir", "sob" },
            { "gül", "laugh" }, { "gul", "laugh" }, { "kıkırda", "giggle" }, { "kikirda", "giggle" },
            { "nefes", "breath" }, { "nefesi kesil", "gasp" }, { "irkil", "gasp" }, { "şaşır", "gasp" }, { "sasir", "gasp" },
            { "fısılda", "whispering" }, { "fisilda", "whispering" }, { "öksür", "cough" }, { "oksur", "cough" },
            { "kısa ara", "short pause" }, { "kisa ara", "short pause" }, { "duraklama", "short pause" },
            { "uzun ara", "long pause" }, { "bekle", "long pause" },
        };

        static string Map(string inner)
        {
            var n = inner.Trim().ToLowerInvariant();
            for (int i = 0; i < Kisayol.GetLength(0); i++)
                if (n == Kisayol[i, 0]) return Kisayol[i, 1];
            return null;
        }

        // (iç çek) ve <hickir> gibi Turkce etiketleri Gemini etiketine cevirir; *vurgu* isaretlerini atar
        public static string ToSpeech(string s)
        {
            var sb = new StringBuilder(s.Length + 16);
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '(' || c == '<')
                {
                    int j = s.IndexOf(c == '(' ? ')' : '>', i + 1);
                    if (j > i)
                    {
                        var inner = s.Substring(i + 1, j - i - 1);
                        var m = Map(inner);
                        if (m != null) { sb.Append('<').Append(m).Append('>'); i = j + 1; continue; }
                        if (c == '<') { sb.Append('<').Append(inner.Trim()).Append('>'); i = j + 1; continue; }   // zaten Ingilizce etiket
                    }
                }
                if (c == '*') { i++; continue; }
                sb.Append(c); i++;
            }
            return Clean(sb.ToString());
        }

        // Altyazi: etiketler ve *isaretler* gider, cift bosluk temizlenir
        public static string ToDisplay(string s)
        {
            var sb = new StringBuilder(s.Length);
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '<') { int j = s.IndexOf('>', i + 1); if (j > i) { i = j + 1; continue; } }
                if (c == '(') { int j = s.IndexOf(')', i + 1); if (j > i && Map(s.Substring(i + 1, j - i - 1)) != null) { i = j + 1; continue; } }
                if (c == '*') { i++; continue; }
                sb.Append(c); i++;
            }
            return Clean(sb.ToString());
        }

        static string Clean(string s)
        {
            var t = s.Replace("  ", " ").Replace("  ", " ").Replace(" ,", ",").Replace(" .", ".").Replace(" !", "!").Replace(" ?", "?");
            return t.Trim();
        }

        public static bool HasTags(string speech) { return speech.IndexOf('<') >= 0; }
    }

    public static class StoryParser
    {
        // 'Direct' (yonetmen) komutlari: Character = komut adi, Arg = geri kalan metin.
        // Baska komutlar da eklenecekse (bak, dinle, altyazi ...) adini bu listeye yazmak yeterlidir.
        public static readonly string[] DirectKeys =
            { "mekan3d", "yer", "gir", "figuran", "ani", "zaman", "gecis", "sessizlik", "isik", "hava",
              "gren", "vinyet", "filtre", "yavas", "dondur", "gozyasi", "nefes", "istasyon", "gorunum" };

        // vfx: gorsel efekt komutu (B4: efekt = yalniz ses efekti, vfx = gorsel efekt, gecis = yalniz sahne gecisi).
        // Mevcut efektlere eslenir: flas -> Flash, far -> Headlights, vhs/geri -> Rewind (eski flas:/far:/geri: aynen calisir).
        public static readonly string[] VfxListesi = { "flas", "far", "vhs", "geri" };

        static bool IsDirectKey(string key)
        {
            foreach (var k in DirectKeys) if (k == key) return true;
            return false;
        }

        static void WarnArg(ParseResult r, int lineNo, string key, string val, string[] ok)
        {
            if (!Util.In(val, ok))
                r.Warnings.Add(lineNo + ". satır: '" + key + ": " + val + "' bilinmiyor (" + string.Join(", ", ok) + ").");
        }

        public static ParseResult Parse(string text)
        {
            var r = new ParseResult();
            int voice = 0, lineNo = 0;
            string oylamaSorusu = "";
            foreach (var raw in (text ?? "").Replace("\r", "").Split('\n'))
            {
                lineNo++;
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;

                if (line[0] == '[')
                {
                    int close = line.IndexOf(']');
                    if (close < 0) { r.Warnings.Add(lineNo + ". satır: ']' eksik."); continue; }
                    var parts = line.Substring(1, close - 1).Split('|');
                    var it = new StoryItem
                    {
                        Type = ItemType.Dialogue,
                        Line = lineNo,
                        Character = parts[0].Trim(),
                        Emotion = parts.Length > 1 && parts[1].Trim().Length > 0 ? parts[1].Trim() : "normal",
                        Text = line.Substring(close + 1).Trim(),
                    };
                    it.Speech = SesEtiketleri.ToSpeech(it.Text);
                    it.Vurgulu = KaraokeAltyazi.VurguluMetin(it.Text);
                    it.Text = SesEtiketleri.ToDisplay(it.Text);
                    if (parts.Length > 2) it.Inner = Util.Norm(parts[2]).StartsWith("i");
                    if (parts.Length > 3) it.Motion = Util.Norm(parts[3]);
                    if (it.Text.Length == 0) { r.Warnings.Add(lineNo + ". satır: konuşma metni boş."); continue; }
                    if (!Util.In(it.Emotion, Util.Emotions)) r.Warnings.Add(lineNo + ". satır: '" + it.Emotion + "' duygusu bilinmiyor, 'normal' kullanılacak.");
                    if (it.Motion.Length > 0 && !Util.In(it.Motion, Util.Motions)) r.Warnings.Add(lineNo + ". satır: '" + it.Motion + "' hareketi bilinmiyor.");
                    it.VoiceIndex = ++voice;
                    r.Items.Add(it);
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon < 0) { r.Warnings.Add(lineNo + ". satır anlaşılmadı: " + line); continue; }
                var key = Util.Norm(line.Substring(0, colon));
                var val = line.Substring(colon + 1).Trim();
                var item = new StoryItem { Line = lineNo, Text = val, Arg = val };
                switch (key)
                {
                    case "mekan": item.Type = ItemType.Background; break;
                    case "kanca": item.Type = ItemType.Hook; break;
                    case "geri": item.Type = ItemType.Rewind; break;
                    case "yazi": item.Type = ItemType.Title; break;
                    case "bekle": item.Type = ItemType.Wait; item.Seconds = Util.ParseFloat(val, 0.5f); break;
                    case "efekt": item.Type = ItemType.Sfx; break;
                    case "cikar":
                        {
                            // cikar: Anne          -> eskisi gibi (yana yuruyerek / kenardan)
                            // cikar: Anne kapi     -> kapidan cikar (3D mekan)
                            var cp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            if (cp.Length == 2 && YonetmenTablo.KapiMi(cp[1]))
                            {
                                item.Type = ItemType.Direct; item.Character = "cikar"; item.Arg = val;
                            }
                            else { item.Type = ItemType.Remove; item.Character = val; }
                        }
                        break;
                    case "temizle": item.Type = ItemType.Clear; break;
                    case "kamera":
                        item.Type = ItemType.Camera;
                        if (!YonetmenTablo.KameraGecerli(val))
                            r.Warnings.Add(lineNo + ". satır: 'kamera: " + val + "' bilinmiyor (" + string.Join(", ", YonetmenTablo.Kameralar) + ").");
                        break;
                    case "atmosfer":
                        {
                            // eski: atmosfer: mum, toz, yagmur, buyu, gunes, yok   (2D efekt katmani)
                            // yeni: atmosfer: huzun | sicak_ani | kabus            (renk ayari; eskilerle birlikte yazilabilir)
                            var oldTokens = new List<string>();
                            var newTokens = new List<string>();
                            foreach (var tk in val.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                            {
                                if (Util.In(tk, YonetmenTablo.Atmosferler)) newTokens.Add(Util.Norm(tk));
                                else oldTokens.Add(tk);
                            }
                            if (newTokens.Count > 0)
                            {
                                r.Items.Add(new StoryItem { Line = lineNo, Type = ItemType.Direct, Character = "atmosfer", Arg = string.Join(",", newTokens.ToArray()), Text = val });
                                if (oldTokens.Count == 0) continue;
                                item.Text = string.Join(", ", oldTokens.ToArray()); item.Arg = item.Text;
                            }
                            item.Type = ItemType.Atmosphere;
                        }
                        break;
                    case "muzik": item.Type = ItemType.Music; break;
                    case "far": item.Type = ItemType.Headlights; break;
                    case "flas": item.Type = ItemType.Flash; break;
                    case "gorsel": item.Type = ItemType.Insert; break;
                    case "vfx":
                        {
                            // vfx: flas | vfx: far | vfx: vhs 1 saat önce (geri: ile ayni)
                            var vp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            string vk = vp.Length > 0 ? Util.Norm(vp[0]) : "";
                            string vt = vp.Length > 1 ? vp[1].Trim() : "";
                            item.Text = vt; item.Arg = vt;
                            if (vk == "flas") item.Type = ItemType.Flash;
                            else if (vk == "far") item.Type = ItemType.Headlights;
                            else if (vk == "vhs" || vk == "geri") item.Type = ItemType.Rewind;
                            else { r.Warnings.Add(lineNo + ". satır: 'vfx: " + val + "' bilinmiyor (" + string.Join(", ", VfxListesi) + ")."); continue; }
                        }
                        break;
                    case "kapak":
                        {
                            // kapak: 24  -> 24. satirdan (ya da sonraki ilk konusmadan) kapak karesi; kapak: (bos) -> bu satirdan sonraki ilk konusma
                            int ks;
                            if (val.Length == 0) r.KapakSatir = lineNo;
                            else if (int.TryParse(val, out ks) && ks > 0) r.KapakSatir = ks;
                            else r.Warnings.Add(lineNo + ". satır: 'kapak:' satır numarası ister (örn. kapak: 24) ya da boş bırak (sonraki konuşma).");
                        }
                        continue;
                    case "etiket":
                        // etiket: aile, dram, #duygusal  -> aciklamada #aile #dram #duygusal
                        foreach (var et in val.Split(new[] { ',', ' ', ';' }, System.StringSplitOptions.RemoveEmptyEntries))
                        {
                            string tg = et.Trim().TrimStart('#');
                            if (tg.Length > 0 && !r.Etiketler.Contains(tg)) r.Etiketler.Add(tg);
                        }
                        continue;
                    case "bak": item.Type = ItemType.Bak; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "dinle": item.Type = ItemType.Dinle; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "altyazi": item.Type = ItemType.AltyaziStil; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "ozet": case "durum": case "olay": case "gecmis": case "iliski": case "soru": case "cozuldu": case "plan":
                        {
                            SeriNotu sn;
                            string snHata = SeriNotu.Coz(key, val, lineNo, out sn);
                            if (snHata != null) r.Warnings.Add(lineNo + ". satır: " + snHata);
                            else r.SeriNotlari.Add(sn);
                        }
                        continue;
                    case "oylama": oylamaSorusu = val; continue;
                    case "devami": item.Type = ItemType.DevamKarti; break;
                    case "seri": r.Seri = val; continue;
                    case "baslik": r.Baslik = val; continue;
                    case "bolum":
                        { int b; if (int.TryParse(val, out b)) r.Bolum = b; else r.Warnings.Add(lineNo + ". satır: bölüm numarası sayı olmalı."); }
                        continue;
                    case "secim":
                        {
                            // secim: A) Babasına söylesin / B) Sır saklasın
                            item.Type = ItemType.Choice;
                            item.Soru = oylamaSorusu; oylamaSorusu = "";
                            var opts = val.Split('/');
                            if (opts.Length < 2) { r.Warnings.Add(lineNo + ". satır: 'secim: A) ... / B) ...' biçiminde iki seçenek yaz."); continue; }
                            item.Text = opts[0].Trim();
                            item.Arg = opts[1].Trim();
                        }
                        break;
                    case "sonuc":
                        // sonuc: %69 A   (onceki bolumun oylama sonucu)
                        item.Type = ItemType.Result;
                        break;
                    case "oyuncu":
                        {
                            // oyuncu: Arda = Timmy   (3D model; "yok" = cizgi karakter)
                            item.Type = ItemType.Cast;
                            int eq = val.IndexOf('=');
                            if (eq < 0) { r.Warnings.Add(lineNo + ". satır: 'oyuncu: Arda = Timmy' biçiminde yaz."); continue; }
                            item.Character = val.Substring(0, eq).Trim();
                            item.Arg = val.Substring(eq + 1).Trim();
                        }
                        break;
                    case "son":
                        {
                            // son: yumusak Metin  |  son: sert [Metin]
                            item.Type = ItemType.End;
                            var sp2 = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            string mode = sp2.Length > 0 ? Util.Norm(sp2[0]) : "";
                            if (mode == "sert" || mode == "yumusak") { item.Arg = mode; item.Text = sp2.Length > 1 ? sp2[1].Trim() : ""; }
                            else { item.Arg = "yumusak"; item.Text = val; }
                        }
                        break;
                    case "poz":
                        item.Type = ItemType.Pose;
                        var sp = val.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                        item.Character = sp.Length > 0 ? sp[0] : "";
                        item.Arg = sp.Length > 1 ? Util.Norm(sp[1]) : "ayakta";
                        if (!Util.In(item.Arg, Util.Poses)) r.Warnings.Add(lineNo + ". satır: '" + item.Arg + "' pozu bilinmiyor (ayakta, yatiyor, oturuyor).");
                        break;
                    default:
                        if (Hareket3D.Tanir(key))
                        {
                            // Asama 10 hareket/etkilesim komutlari (Hareket3D.cs): Direct olarak tasinir
                            item.Type = ItemType.Direct; item.Character = key; item.Arg = val;
                            string hatali = Hareket3D.Dogrula(key, val);
                            if (hatali != null) r.Warnings.Add(lineNo + ". satır: " + hatali);
                            break;
                        }
                        if (IsDirectKey(key))
                        {
                            item.Type = ItemType.Direct; item.Character = key; item.Arg = val;
                            string bad = YonetmenTablo.Dogrula(key, val);
                            if (bad != null) r.Warnings.Add(lineNo + ". satır: " + bad);
                            break;
                        }
                        r.Warnings.Add(lineNo + ". satır: '" + key + ":' komutu bilinmiyor.");
                        continue;
                }
                r.Items.Add(item);
            }
            r.DialogueCount = voice;
            return r;
        }
    }
}
