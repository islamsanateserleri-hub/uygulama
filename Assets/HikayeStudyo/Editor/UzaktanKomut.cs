using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace HikayeStudyo
{
    // STUDYO KOPRUSU: Proje klasorundeki HikayeKomut/ klasoru uzerinden Unity'ye komut verilir, sonuc dosyalardan okunur.
    // Yapay zeka (Claude/Gemini) ya da bir script, Unity'yi dosya yazarak surebilir. Ayrintilar: CLAUDE.md, Docs/HikayeSozdizimi.md
    //
    //   komut.txt      : her satir bir komut (1,5 sn'de bir okunur, islenince silinir). Tam dosyayi tek seferde yaz.
    //   sonuc.txt      : islenen komutlarin sonucu (zaman damgali, ekleme)
    //   son_sonuc.json : en son komutun sonucu {zaman, komut, sonuc, tamam}
    //   dogrulama.json : dogrula / hikaye_yaz sorun listesi [{satir, kod, mesaj, oneri}]
    //   hikaye_<ad>.txt: hikaye_yaz icin taslak hikaye (sen yazarsin)
    //   karakterler.json, mekanlar.json, sesler.json, komutlar.json, kayit_durumu.json, ekran.png/ekran.json, rig_raporu.txt
    //   konsol.txt     : Unity konsolunun kopyasi (uyari/hata)
    //   derleme.txt    : son derlemenin hata/uyari listesi
    //
    // Komutlar (tam liste: komut_listele):
    //   yenile | menu: <yol> | oynat: <ad> [satir] | kaydet: <ad> | dur | ekran | seslendir: <ad>
    //   hikaye_yaz: <ad> | dogrula: <ad> [proje] | karakter_listele | mekan_listele | ses_listele [: <ad>] | komut_listele
    //   onizleme_goruntu: <ad> [satir] [gecikme_sn] | kayit_durumu | kota | kilavuz_yaz
    [InitializeOnLoad]
    public static class UzaktanKomut
    {
        public static readonly string Dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "HikayeKomut"));
        static double next;
        static readonly List<string> compileLines = new List<string>();
        static bool sesIzleniyor;
        static string sonDurum = "";
        const int MaxHikayeBayt = 400000;

        // Komut belgesi: {ad, ornek, aciklama}
        public static readonly string[,] KopruKomutlari =
        {
            { "yenile", "yenile", "Dosyaları yeniden tarar / derler" },
            { "menu", "menu: Hikaye/Karakter Galerisi", "Unity menü öğesini çalıştırır" },
            { "oynat", "oynat: deneme_3d 5", "Hikayeyi önizler (isteğe bağlı satırdan)" },
            { "kaydet", "kaydet: deneme_3d", "Hikayeyi videoya alır (tüm satırların sesi olmalı)" },
            { "dur", "dur", "Oynatmayı durdurur" },
            { "ekran", "ekran", "Game görüntüsünü HikayeKomut/ekran.png olarak kaydeder (oynarken)" },
            { "seslendir", "seslendir: deneme_3d", "Eksik sesleri Gemini ile üretir" },
            { "hikaye_yaz", "hikaye_yaz: bolum_2", "HikayeKomut/hikaye_bolum_2.txt dosyasını doğrular; hata yoksa Assets/Resources/Hikayeler/bolum_2.txt olarak yazar, hata varsa dogrulama.json yazar" },
            { "dogrula", "dogrula: bolum_2 [proje]", "Taslağı (yoksa projedeki hikayeyi) doğrular; dogrulama.json yazar" },
            { "karakter_listele", "karakter_listele", "karakterler.json: kütüphane karakterleri, sesleri ve 3D oyuncular" },
            { "mekan_listele", "mekan_listele", "mekanlar.json: mekan, efekt, müzik, görsel, atmosfer, kamera, hikaye listesi" },
            { "ses_listele", "ses_listele: bolum_2", "sesler.json: Gemini sesleri, karakter sesleri, ses etiketleri, kota; hikaye adı verilirse satır satır ses durumu" },
            { "komut_listele", "komut_listele", "komutlar.json: köprü komutları + hikaye komutları (güncel, kayıt defterinden)" },
            { "onizleme_goruntu", "onizleme_goruntu: bolum_2 7 3", "Hikayeyi 7. satırdan oynatır, 3 sn sonra ekran görüntüsünü ekran.png'ye alır, ekran.json yazar" },
            { "kayit_durumu", "kayit_durumu", "kayit_durumu.json: kayıt durumu, video yolu, ses raporu" },
            { "kota", "kota", "Bugünkü Gemini ses isteği sayacı" },
            { "kilavuz_yaz", "kilavuz_yaz", "Docs/HikayeSozdizimi.md, CLAUDE.md, GEMINI.md dosyalarını yeniden yazar" },
        };

        static UzaktanKomut()
        {
            Directory.CreateDirectory(Dir);
            Application.logMessageReceivedThreaded += OnLog;
            CompilationPipeline.compilationStarted += o => { lock (compileLines) compileLines.Clear(); };
            CompilationPipeline.assemblyCompilationFinished += OnAssembly;
            CompilationPipeline.compilationFinished += o => WriteCompile();
            EditorApplication.update += Poll;
            Append("sonuc.txt", Stamp() + " Unity hazır (derleme yüklendi).");
        }

        static string Stamp() { return System.DateTime.Now.ToString("HH:mm:ss"); }

        internal static void Append(string file, string line)
        {
            try
            {
                var p = Path.Combine(Dir, file);
                if (File.Exists(p) && new FileInfo(p).Length > 400000) File.WriteAllText(p, "");
                File.AppendAllText(p, line + "\n", new UTF8Encoding(false));
            }
            catch { }
        }

        internal static void Cikti(string file, string content)
        {
            HsJson.Yaz(Path.Combine(Dir, file), content);
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            string t = type == LogType.Log ? "BİLGİ" : type == LogType.Warning ? "UYARI" : "HATA";
            string s = Stamp() + " [" + t + "] " + msg;
            if (type == LogType.Exception || type == LogType.Error)
            {
                var st = (stack ?? "").Split('\n');
                for (int i = 0; i < st.Length && i < 4; i++) if (st[i].Trim().Length > 0) s += "\n      " + st[i].Trim();
            }
            Append("konsol.txt", s);
        }

        static void OnAssembly(string asm, CompilerMessage[] msgs)
        {
            lock (compileLines)
                foreach (var m in msgs)
                    compileLines.Add((m.type == CompilerMessageType.Error ? "HATA " : "UYARI ") + m.file + "(" + m.line + "): " + m.message);
        }

        static void WriteCompile()
        {
            int err = 0;
            var sb = new StringBuilder();
            lock (compileLines)
                foreach (var l in compileLines) { if (l.StartsWith("HATA")) err++; sb.AppendLine(l); }
            try { File.WriteAllText(Path.Combine(Dir, "derleme.txt"), Stamp() + " Derleme bitti: " + err + " hata\n" + sb, new UTF8Encoding(false)); }
            catch { }
        }

        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + 1.5;
            var p = Path.Combine(Dir, "komut.txt");
            if (!File.Exists(p)) return;
            string[] lines;
            try { lines = File.ReadAllLines(p); File.Delete(p); } catch { return; }
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;
                string res;
                try { res = Run(line); }
                catch (System.Exception e) { res = "HATA: " + e.Message; }
                Append("sonuc.txt", Stamp() + " " + line + " -> " + res);
                bool tamam = !(res.StartsWith("HATA") || res.StartsWith("REDDEDİLDİ") || res.StartsWith("bilinmeyen"));
                Cikti("son_sonuc.json", "{\"zaman\": " + HsJson.Q(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) + ", \"komut\": " + HsJson.Q(line) +
                      ", \"sonuc\": " + HsJson.Q(res) + ", \"tamam\": " + HsJson.B(tamam) + "}");
            }
        }

        // ---------- Yol guvenligi (sandbox) ----------
        // Hikaye adi: yalniz harf, rakam, _ ve -  ('..', '/', '\', ':' ve mutlak yol reddedilir)
        public static bool AdGecerli(string ad, out string hata)
        {
            hata = "";
            if (string.IsNullOrWhiteSpace(ad)) { hata = "hikaye adı yok"; return false; }
            if (ad.Contains("..") || ad.IndexOf('/') >= 0 || ad.IndexOf('\\') >= 0 || ad.IndexOf(':') >= 0 || Path.IsPathRooted(ad))
            { hata = "geçersiz ad ('..', '/', '\\', ':' ve mutlak yol yasak): " + ad; return false; }
            if (ad.Length > 64) { hata = "ad çok uzun (en çok 64 karakter)"; return false; }
            foreach (char c in ad)
                if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-')) { hata = "adda yalnız harf, rakam, _ ve - olabilir: " + ad; return false; }
            return true;
        }

        static string Normalize(string path) { return Path.GetFullPath(path).Replace('\\', '/'); }

        static bool Icinde(string kok, string yol)
        {
            string k = Normalize(kok).TrimEnd('/') + "/";
            return Normalize(yol).StartsWith(k, System.StringComparison.OrdinalIgnoreCase);
        }

        // HikayeKomut/hikaye_<ad>.txt
        public static string TaslakYolu(string ad)
        {
            string p = Path.Combine(Dir, "hikaye_" + ad + ".txt");
            return Icinde(Dir, p) ? p : null;
        }

        // Assets/Resources/Hikayeler/<ad>.txt (proje yolu)
        public static string HikayeYolu(string ad)
        {
            string rel = OtomatikKurulum.StoryFolder + "/" + ad + ".txt";
            return Icinde(Path.GetFullPath(OtomatikKurulum.StoryFolder), rel) ? rel : null;
        }

        static string ReadUtf8(string path)
        {
            string t = File.ReadAllText(path, Encoding.UTF8);
            if (t.Length > 0 && t[0] == '﻿') t = t.Substring(1);
            return t;
        }

        static StoryLibrary Kutuphane()
        {
            var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
            if (lib == null) { OtomatikKurulum.Ensure(); lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath); }
            return lib;
        }

        // ---------- Komutlar ----------
        static string Run(string line)
        {
            int c = line.IndexOf(':');
            string cmd = Util.Norm(c < 0 ? line : line.Substring(0, c));
            string arg = c < 0 ? "" : line.Substring(c + 1).Trim();
            // "ses_listele bolum_2" (iki nokta yok) gibi bosluklu yazimi da kabul et
            if (c < 0)
            {
                int sp = line.IndexOf(' ');
                if (sp > 0) { cmd = Util.Norm(line.Substring(0, sp)); arg = line.Substring(sp + 1).Trim(); }
            }
            switch (cmd)
            {
                case "yenile":
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return "tamam";
                case "menu":
                    return EditorApplication.ExecuteMenuItem(arg) ? "tamam" : "HATA: menü bulunamadı";
                case "oynat":
                case "kaydet":
                    {
                        var parts = arg.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) return "HATA: hikaye adı yok";
                        string err;
                        if (!AdGecerli(parts[0], out err)) return "HATA: " + err;
                        int ln = 0;
                        if (parts.Length > 1) int.TryParse(parts[1], out ln);
                        return StudyoWindow.PlayByName(parts[0], ln, cmd == "kaydet");
                    }
                case "seslendir":
                    {
                        // seslendir: deneme_3d  -> eksik sesleri Gemini ile uretir (anahtar Studyo'da kayitli olmali)
                        string err;
                        if (!AdGecerli(arg, out err)) return "HATA: " + err;
                        string path = HikayeYolu(arg);
                        if (path == null || !File.Exists(path)) return "HATA: hikaye yok: " + arg;
                        if (EditorPrefs.GetString(GeminiSes.KeyPref, "").Trim().Length == 0) return "HATA: Gemini anahtarı girilmemiş";
                        if (GeminiSes.Busy) return "HATA: zaten seslendiriyor";
                        var lib = Kutuphane();
                        if (lib == null) return "HATA: kütüphane yok";
                        if (!sesIzleniyor) { sesIzleniyor = true; GeminiSes.Changed += () => { if (GeminiSes.Status != sonDurum) { sonDurum = GeminiSes.Status; Append("sonuc.txt", Stamp() + " ses: " + sonDurum); } }; }
                        GeminiSes.Generate(lib, arg, StoryParser.Parse(ReadUtf8(path)).Items);
                        return "başladı (" + GeminiSes.KotaMetni() + ")";
                    }
                case "dur":
                    EditorApplication.isPlaying = false;
                    return "tamam";
                case "ekran":
                    {
                        var f = Path.Combine(Dir, "ekran.png");
                        ScreenCapture.CaptureScreenshot(f);
                        return EditorApplication.isPlaying ? "kaydediliyor: " + f : "oynatma kapalı (Game görüntüsü olmayabilir)";
                    }
                case "hikaye_yaz": return HikayeYaz(arg);
                case "dogrula": return Dogrula(arg);
                case "karakter_listele": return KarakterListele();
                case "mekan_listele": return MekanListele();
                case "ses_listele": return SesListele(arg);
                case "komut_listele":
                case "yardim": return KomutListele();
                case "onizleme_goruntu": return KayitTakip.OnizlemeBaslat(arg);
                case "kayit_durumu":
                    KayitTakip.DurumYaz();
                    return "kayit_durumu.json yazıldı: " + KayitTakip.Durum;
                case "kota": return GeminiSes.KotaMetni();
                case "kilavuz_yaz": return StudyoKilavuz.Yaz();
            }
            return "bilinmeyen komut: " + cmd + " (komut_listele ile bak)";
        }

        // hikaye_yaz: <ad>  -> HikayeKomut/hikaye_<ad>.txt dogrulanir; hata yoksa projeye yazilir
        static string HikayeYaz(string arg)
        {
            string ad = arg.Trim();
            if (ad.EndsWith(".txt")) ad = ad.Substring(0, ad.Length - 4);
            string err;
            if (!AdGecerli(ad, out err)) return "HATA: " + err;
            string taslak = TaslakYolu(ad);
            if (taslak == null) return "HATA: geçersiz taslak yolu";
            if (!File.Exists(taslak)) return "HATA: taslak yok: HikayeKomut/hikaye_" + ad + ".txt (önce bu dosyayı yaz)";
            if (new FileInfo(taslak).Length > MaxHikayeBayt) return "HATA: taslak çok büyük (en çok " + MaxHikayeBayt / 1000 + " KB)";
            string hedef = HikayeYolu(ad);
            if (hedef == null) return "HATA: geçersiz hedef yolu";

            string icerik = ReadUtf8(taslak).Replace("\r\n", "\n");
            var lib = Kutuphane();
            var sorunlar = StudyoDogrulama.Dogrula(icerik, lib, null, false);
            Cikti("dogrulama.json", StudyoDogrulama.ToJson(sorunlar));
            int hata = StudyoDogrulama.HataSayisi(sorunlar);
            if (hata > 0)
                return "REDDEDİLDİ: " + hata + " hata, " + (sorunlar.Count - hata) + " uyarı; hikaye yazılmadı. Ayrıntı: HikayeKomut/dogrulama.json — düzelt ve tekrar hikaye_yaz: " + ad;

            string yedek = "";
            if (File.Exists(hedef))
            {
                string yd = Path.Combine(Dir, "yedek");
                Directory.CreateDirectory(yd);
                string yp = Path.Combine(yd, ad + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
                File.Copy(hedef, yp, true);
                yedek = " (eski sürüm yedeklendi: HikayeKomut/yedek/)";
            }
            Directory.CreateDirectory(OtomatikKurulum.StoryFolder);
            File.WriteAllText(hedef, icerik, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(hedef, ImportAssetOptions.ForceSynchronousImport);
            StudyoWindow.DisaridanDegisti(ad);
            return "tamam: " + hedef + " yazıldı, " + (sorunlar.Count) + " uyarı" + yedek;
        }

        static string Dogrula(string arg)
        {
            var parts = arg.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "HATA: hikaye adı yok";
            string ad = parts[0];
            if (ad.EndsWith(".txt")) ad = ad.Substring(0, ad.Length - 4);
            string err;
            if (!AdGecerli(ad, out err)) return "HATA: " + err;
            bool proje = parts.Length > 1 && Util.Norm(parts[1]) == "proje";
            string icerik = null, kaynak = "";
            string taslak = TaslakYolu(ad);
            if (!proje && taslak != null && File.Exists(taslak)) { icerik = ReadUtf8(taslak); kaynak = "taslak"; }
            else
            {
                string hp = HikayeYolu(ad);
                if (hp != null && File.Exists(hp)) { icerik = ReadUtf8(hp); kaynak = "proje"; }
            }
            if (icerik == null) return "HATA: ne HikayeKomut/hikaye_" + ad + ".txt ne de proje hikayesi var";
            var lib = Kutuphane();
            var sorunlar = StudyoDogrulama.Dogrula(icerik, lib, kaynak == "proje" ? ad : null, kaynak == "proje");
            Cikti("dogrulama.json", StudyoDogrulama.ToJson(sorunlar));
            int hata = StudyoDogrulama.HataSayisi(sorunlar);
            return "tamam (" + kaynak + "): " + hata + " hata, " + (sorunlar.Count - hata) + " uyarı → dogrulama.json";
        }

        static string KarakterListele()
        {
            var lib = Kutuphane();
            var sb = new StringBuilder("{\n");
            if (lib != null)
            {
                sb.Append("  \"anlatici\": {\"id\": ").Append(HsJson.Q(lib.narrator.id)).Append(", \"ses\": ").Append(HsJson.Q(lib.narrator.voice)).Append("},\n");
                sb.Append("  \"karakterler\": [");
                bool first = true;
                foreach (var ch in lib.characters)
                {
                    if (ch == null) continue;
                    sb.Append(first ? "\n    " : ",\n    ");
                    first = false;
                    sb.Append("{\"id\": ").Append(HsJson.Q(ch.id))
                      .Append(", \"ses\": ").Append(HsJson.Q(ch.voice))
                      .Append(", \"ic_ses\": ").Append(HsJson.Q(ch.innerVoice))
                      .Append(", \"taraf\": ").Append(ch.side)
                      .Append(", \"olcek\": ").Append(HsJson.F(ch.scale))
                      .Append(", \"model3D\": ").Append(HsJson.Q(ch.model3D != null ? ch.model3D.name : "")).Append("}");
                }
                sb.Append(first ? "],\n" : "\n  ],\n");
            }
            else sb.Append("  \"anlatici\": null, \"karakterler\": [],\n");
            var q = new List<string>();
            foreach (var n in StudyoDogrulama.OyuncuAdlari()) q.Add(HsJson.Q(n));
            sb.Append("  \"oyuncular_3d\": ").Append(HsJson.Dizi(q)).Append(",\n");
            sb.Append("  \"not\": ").Append(HsJson.Q("Konuşma satırında karakter 'id' ile yazılır: [Arda|normal|dis] Metin. 3D için: oyuncu: Arda = Timmy (oyuncular_3d adının kısa hali de olur). Her insan modelinin çeşitleri de oyuncu adıdır: <ad>_001, _002 ... (örn. VRM_Anne_005); toplam ~1000, uygulamada Karakterlerim > Çeşit karakterler.")).Append("\n}");
            Cikti("karakterler.json", sb.ToString());
            return "karakterler.json yazıldı";
        }

        static string MekanListele()
        {
            var lib = Kutuphane();
            var sb = new StringBuilder("{\n");
            sb.Append("  \"mekanlar\": [");
            bool first = true;
            if (lib != null)
                foreach (var b in lib.backgrounds)
                {
                    if (b == null) continue;
                    sb.Append(first ? "\n    " : ",\n    ");
                    first = false;
                    sb.Append("{\"id\": ").Append(HsJson.Q(b.id)).Append(", \"atmosfer\": ").Append(HsJson.Q(b.atmosphere)).Append("}");
                }
            sb.Append(first ? "],\n" : "\n  ],\n");

            var kat = NesneKatalogu.Al();
            if (kat != null)
            {
                var gk = new List<string>();
                foreach (var g in kat.gokler) if (g != null) gk.Add(HsJson.Q(g.id));
                sb.Append("  \"uzayMekanlari\": ").Append(HsJson.Dizi(gk)).Append(",\n");
                var gruplar = new SortedDictionary<string, List<string>>();
                foreach (var n in kat.nesneler)
                {
                    if (n == null) continue;
                    List<string> g;
                    if (!gruplar.TryGetValue(n.kategori, out g)) gruplar[n.kategori] = g = new List<string>();
                    g.Add(HsJson.Q(n.id));
                }
                sb.Append("  \"nesneler\": {");
                bool ilk = true;
                foreach (var kv in gruplar)
                {
                    sb.Append(ilk ? "\n    " : ",\n    ").Append(HsJson.Q(kv.Key)).Append(": ").Append(HsJson.Dizi(kv.Value));
                    ilk = false;
                }
                sb.Append("\n  },\n");
            }

            var ef = new List<string>();
            foreach (var e in StudyoDogrulama.EfektAdlari(lib)) ef.Add(HsJson.Q(e));
            sb.Append("  \"efektler\": ").Append(HsJson.Dizi(ef)).Append(",\n");

            var vl = new List<string>();
            foreach (var e in StoryParser.VfxListesi) vl.Add(HsJson.Q(e));
            sb.Append("  \"vfx\": ").Append(HsJson.Dizi(vl)).Append(",\n");

            var mu = new List<string> { HsJson.Q("pad"), HsJson.Q("yok") };
            var gr = new List<string> { HsJson.Q("eller"), HsJson.Q("yok") };
            if (lib != null)
            {
                foreach (var m in lib.music) if (m != null && !string.IsNullOrEmpty(m.id)) mu.Add(HsJson.Q(m.id));
                foreach (var m in lib.inserts) if (m != null && !string.IsNullOrEmpty(m.id)) gr.Add(HsJson.Q(m.id));
            }
            sb.Append("  \"muzikler\": ").Append(HsJson.Dizi(mu)).Append(",\n");
            sb.Append("  \"gorseller\": ").Append(HsJson.Dizi(gr)).Append(",\n");
            sb.Append("  \"atmosferler\": ").Append(HsJson.Dizi(new[] { HsJson.Q("mum"), HsJson.Q("toz"), HsJson.Q("yagmur"), HsJson.Q("buyu"), HsJson.Q("gunes"), HsJson.Q("sis"), HsJson.Q("yok") })).Append(",\n");
            sb.Append("  \"kameralar\": ").Append(HsJson.Dizi(new[] { HsJson.Q("otomatik"), HsJson.Q("yakin"), HsJson.Q("genis"), HsJson.Q("salla") })).Append(",\n");
            sb.Append("  \"pozlar\": ").Append(HsJson.Dizi(Quote(Util.Poses))).Append(",\n");
            sb.Append("  \"duygular\": ").Append(HsJson.Dizi(Quote(Util.Emotions))).Append(",\n");
            sb.Append("  \"hareketler\": ").Append(HsJson.Dizi(Quote(Util.Motions))).Append(",\n");

            var st = new List<string>();
            if (Directory.Exists(OtomatikKurulum.StoryFolder))
                foreach (var f in Directory.GetFiles(OtomatikKurulum.StoryFolder, "*.txt")) st.Add(HsJson.Q(Path.GetFileNameWithoutExtension(f)));
            sb.Append("  \"hikayeler\": ").Append(HsJson.Dizi(st)).Append("\n}");
            Cikti("mekanlar.json", sb.ToString());
            return "mekanlar.json yazıldı";
        }

        static List<string> Quote(IEnumerable<string> items)
        {
            var l = new List<string>();
            foreach (var s in items) l.Add(HsJson.Q(s));
            return l;
        }

        static string SesListele(string arg)
        {
            var lib = Kutuphane();
            var sb = new StringBuilder("{\n");
            sb.Append("  \"model\": ").Append(HsJson.Q(GeminiSes.Model)).Append(",\n");
            sb.Append("  \"kota\": {\"bugun\": ").Append(GeminiSes.KotaBugun()).Append(", \"limit\": ").Append(GeminiSes.KotaLimit)
              .Append(", \"doldu\": ").Append(HsJson.B(GeminiSes.KotaDolduMu())).Append(", \"metin\": ").Append(HsJson.Q(GeminiSes.KotaMetni())).Append("},\n");
            sb.Append("  \"api_anahtari_var\": ").Append(HsJson.B(EditorPrefs.GetString(GeminiSes.KeyPref, "").Trim().Length > 0)).Append(",\n");

            var g = new List<string>();
            for (int i = 0; i < SesListesi.Katalog.GetLength(0); i++)
                g.Add("{\"ad\": " + HsJson.Q(SesListesi.Katalog[i, 0]) + ", \"ozellik\": " + HsJson.Q(SesListesi.Katalog[i, 1]) + "}");
            sb.Append("  \"gemini_sesleri\": ").Append(HsJson.Dizi(g)).Append(",\n");

            var ks = new List<string>();
            if (lib != null)
            {
                ks.Add("{\"karakter\": " + HsJson.Q(lib.narrator.id) + ", \"ses\": " + HsJson.Q(lib.narrator.voice) + ", \"ic_ses\": \"\"}");
                foreach (var ch in lib.characters)
                    if (ch != null) ks.Add("{\"karakter\": " + HsJson.Q(ch.id) + ", \"ses\": " + HsJson.Q(ch.voice) + ", \"ic_ses\": " + HsJson.Q(ch.innerVoice) + "}");
            }
            sb.Append("  \"karakter_sesleri\": ").Append(HsJson.Dizi(ks)).Append(",\n");

            var et = new List<string>();
            for (int i = 0; i < SesEtiketleri.Kisayol.GetLength(0); i++)
                et.Add("{\"yazilis\": " + HsJson.Q("(" + SesEtiketleri.Kisayol[i, 0] + ")") + ", \"gemini\": " + HsJson.Q("<" + SesEtiketleri.Kisayol[i, 1] + ">") + "}");
            sb.Append("  \"ses_etiketleri\": ").Append(HsJson.Dizi(et)).Append(",\n");

            var tn = new List<string>();
            foreach (var e in Util.Emotions)
            {
                var it = new StoryItem { Type = ItemType.Dialogue, Emotion = e };
                tn.Add("{\"duygu\": " + HsJson.Q(e) + ", \"stil\": " + HsJson.Q(Util.ToneFor(it)) + "}");
            }
            sb.Append("  \"tonlar\": ").Append(HsJson.Dizi(tn));

            string ad = arg.Trim();
            if (ad.EndsWith(".txt")) ad = ad.Substring(0, ad.Length - 4);
            string durum = "";
            if (ad.Length > 0 && lib != null)
            {
                string err;
                string hp = AdGecerli(ad, out err) ? HikayeYolu(ad) : null;
                if (hp == null || !File.Exists(hp)) durum = "hikaye yok";
                else
                {
                    var p = StoryParser.Parse(ReadUtf8(hp));
                    var sat = new List<string>();
                    int sesVar = 0, toplam = 0;
                    foreach (var it in p.Items)
                    {
                        if (it.Type != ItemType.Dialogue) continue;
                        toplam++;
                        bool has = GeminiSes.HasClip(ad, lib, it);
                        if (has) sesVar++;
                        float sure = GeminiSes.ClipSeconds(ad, lib, it);
                        string raw = string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech;
                        sat.Add("{\"satir\": " + it.Line + ", \"karakter\": " + HsJson.Q(it.Character) + ", \"ses\": " + HsJson.Q(lib.VoiceFor(it)) +
                                ", \"var\": " + HsJson.B(has) + ", \"sure_sn\": " + HsJson.F(sure) + ", \"okunacak_metin\": " + HsJson.Q(TurkceMetin.ForSpeech(raw)) + "}");
                    }
                    sb.Append(",\n  \"hikaye\": {\"ad\": ").Append(HsJson.Q(ad)).Append(", \"konusma\": ").Append(toplam).Append(", \"sesi_var\": ").Append(sesVar)
                      .Append(", \"satirlar\": ").Append(HsJson.Dizi(sat)).Append("}");
                    durum = sesVar + "/" + toplam + " ses hazır";
                }
            }
            sb.Append("\n}");
            Cikti("sesler.json", sb.ToString());
            return "sesler.json yazıldı" + (durum.Length > 0 ? " (" + durum + ")" : "");
        }

        static string KomutListele()
        {
            StoryParser.Parse("");
            var sb = new StringBuilder("{\n  \"kopru_komutlari\": [");
            for (int i = 0; i < KopruKomutlari.GetLength(0); i++)
            {
                sb.Append(i == 0 ? "\n    " : ",\n    ");
                sb.Append("{\"ad\": ").Append(HsJson.Q(KopruKomutlari[i, 0])).Append(", \"ornek\": ").Append(HsJson.Q(KopruKomutlari[i, 1]))
                  .Append(", \"aciklama\": ").Append(HsJson.Q(KopruKomutlari[i, 2])).Append("}");
            }
            sb.Append("\n  ],\n  \"hikaye_komutlari\": [");
            bool first = true;
            foreach (var b in KomutKayit.Hepsi())
            {
                sb.Append(first ? "\n    " : ",\n    ");
                first = false;
                sb.Append("{\"ad\": ").Append(HsJson.Q(b.Ad)).Append(", \"ornek\": ").Append(HsJson.Q(b.Ornek)).Append(", \"aciklama\": ").Append(HsJson.Q(b.Aciklama)).Append("}");
            }
            sb.Append("\n  ],\n  \"duygular\": ").Append(HsJson.Dizi(Quote(Util.Emotions)));
            sb.Append(",\n  \"hareketler\": ").Append(HsJson.Dizi(Quote(Util.Motions)));
            sb.Append(",\n  \"pozlar\": ").Append(HsJson.Dizi(Quote(Util.Poses))).Append("\n}");
            Cikti("komutlar.json", sb.ToString());
            return "komutlar.json yazıldı";
        }
    }

    // Kayit ve onizleme-goruntu takibi (alan degerleri SessionState'te: derleme/domain reload'a dayanir)
    [InitializeOnLoad]
    public static class KayitTakip
    {
        const string KDurum = "HS_KT_Durum", KHikaye = "HS_KT_Hikaye", KMesaj = "HS_KT_Mesaj", KBitis = "HS_KT_Bitis",
                     KTamam = "HS_KT_Tamam", KSes = "HS_KT_Ses", KGosterildi = "HS_KT_Gosterildi";
        const string OAktif = "HS_OG_Aktif", OAsama = "HS_OG_Asama", OZaman = "HS_OG_Zaman", OGiris = "HS_OG_Giris",
                     OGecikme = "HS_OG_Gecikme", OSatir = "HS_OG_Satir", OHikaye = "HS_OG_Hikaye", ODosya = "HS_OG_Dosya", OYazma = "HS_OG_Yazma";

        public static event System.Action Bitti;

        static KayitTakip()
        {
            EditorApplication.playModeStateChanged += OnPlay;
            EditorApplication.update += Tick;
        }

        // yok | kaydediyor | bitis_kontrol | bitti | hata | reddedildi
        public static string Durum { get { return SessionState.GetString(KDurum, "yok"); } }
        public static string Hikaye { get { return SessionState.GetString(KHikaye, ""); } }
        public static string Mesaj { get { return SessionState.GetString(KMesaj, ""); } }
        public static bool BittiGosterilmedi { get { return Durum == "bitti" && !SessionState.GetBool(KGosterildi, false); } }
        // Pencerede bildirim sarit gosterilsin mi (bitti, hata ya da reddedildi; "Kapat"a basilana kadar)
        public static bool BildirimVar
        {
            get
            {
                string d = Durum;
                return (d == "bitti" || d == "hata" || d == "reddedildi") && !SessionState.GetBool(KGosterildi, false);
            }
        }
        public static void BittiyiKapat() { SessionState.SetBool(KGosterildi, true); }

        static string D(double v) { return v.ToString("R", CultureInfo.InvariantCulture); }
        static double P(string s) { double v; return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : 0; }

        public static void Basla(string story)
        {
            SessionState.SetString(KDurum, "kaydediyor");
            SessionState.SetString(KHikaye, story);
            SessionState.SetString(KMesaj, "");
            SessionState.SetString(KSes, "");
            SessionState.SetBool(KTamam, false);
            SessionState.SetBool(KGosterildi, false);
            DurumYaz();
        }

        public static void Reddet(string story, string mesaj)
        {
            SessionState.SetString(KDurum, "reddedildi");
            SessionState.SetString(KHikaye, story);
            SessionState.SetString(KMesaj, mesaj);
            SessionState.SetBool(KGosterildi, false);
            DurumYaz();
        }

        static void OnPlay(PlayModeStateChange s)
        {
            if (s == PlayModeStateChange.EnteredPlayMode && Durum == "kaydediyor")
            {
                StoryPlayer.Finished -= Fin;
                StoryPlayer.Finished += Fin;
            }
            if (s == PlayModeStateChange.EnteredEditMode && Durum == "kaydediyor")
            {
                SessionState.SetString(KDurum, "bitis_kontrol");
                SessionState.SetString(KBitis, D(EditorApplication.timeSinceStartup));
            }
        }

        static void Fin()
        {
            StoryPlayer.Finished -= Fin;
            SessionState.SetBool(KTamam, true);
            SessionState.SetString(KSes, StoryPlayer.SonSesRaporu ?? "");
        }

        static void Tick()
        {
            if (Durum == "bitis_kontrol" && EditorApplication.timeSinceStartup - P(SessionState.GetString(KBitis, "0")) > 3.0)
            {
                string v = StudyoKayit.LastVideo;
                bool varMi = !string.IsNullOrEmpty(v) && File.Exists(v) && new FileInfo(v).Length > 0;
                SessionState.SetString(KDurum, varMi ? "bitti" : "hata");
                SessionState.SetString(KMesaj, varMi ? "Video hazır: " + v : "Video dosyası bulunamadı: " + v);
                UzaktanKomut.Append("sonuc.txt", System.DateTime.Now.ToString("HH:mm:ss") + " kayıt " + (varMi ? "bitti: " + v : "HATA: video yok"));
                DurumYaz();
                if (varMi && Bitti != null) Bitti();
            }
            OnizlemeTick();
        }

        public static void DurumYaz()
        {
            string v = StudyoKayit.LastVideo;
            bool varMi = !string.IsNullOrEmpty(v) && File.Exists(v);
            long boyut = 0;
            try { if (varMi) boyut = new FileInfo(v).Length; } catch { }
            string ses = SessionState.GetString(KSes, "");
            if (ses.Length == 0 && !string.IsNullOrEmpty(StoryPlayer.SonSesRaporu)) ses = StoryPlayer.SonSesRaporu;
            var sb = new StringBuilder("{\n");
            sb.Append("  \"durum\": ").Append(HsJson.Q(Durum)).Append(",\n");
            sb.Append("  \"hikaye\": ").Append(HsJson.Q(Hikaye)).Append(",\n");
            sb.Append("  \"mesaj\": ").Append(HsJson.Q(Mesaj)).Append(",\n");
            sb.Append("  \"video\": ").Append(HsJson.Q(v)).Append(",\n");
            sb.Append("  \"klasor\": ").Append(HsJson.Q(string.IsNullOrEmpty(v) ? "" : Path.GetDirectoryName(v))).Append(",\n");
            sb.Append("  \"video_var\": ").Append(HsJson.B(varMi)).Append(",\n");
            sb.Append("  \"video_boyut_bayt\": ").Append(boyut).Append(",\n");
            sb.Append("  \"hikaye_sonuna_kadar_oynadi\": ").Append(HsJson.B(SessionState.GetBool(KTamam, false))).Append(",\n");
            sb.Append("  \"ses_raporu\": ").Append(HsJson.Q(ses)).Append(",\n");
            sb.Append("  \"oynuyor\": ").Append(HsJson.B(EditorApplication.isPlaying)).Append(",\n");
            sb.Append("  \"recorder_kurulu\": ").Append(HsJson.B(StudyoKayit.RecorderInstalled)).Append("\n}");
            UzaktanKomut.Cikti("kayit_durumu.json", sb.ToString());
        }

        // ---------- onizleme_goruntu: <ad> [satir] [gecikme_sn] ----------
        public static string OnizlemeBaslat(string arg)
        {
            var parts = arg.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "HATA: hikaye adı yok (onizleme_goruntu: <ad> [satir] [gecikme_sn])";
            string err;
            if (!UzaktanKomut.AdGecerli(parts[0], out err)) return "HATA: " + err;
            int satir = 0;
            float gecikme = 3f;
            if (parts.Length > 1 && !int.TryParse(parts[1], out satir)) satir = 0;
            if (parts.Length > 2) gecikme = Mathf.Clamp(Util.ParseFloat(parts[2], 3f), 0.5f, 30f);
            if (EditorApplication.isPlaying) return "HATA: zaten oynuyor (önce 'dur')";
            string dosya = Path.Combine(UzaktanKomut.Dir, "ekran.png");
            try { if (File.Exists(dosya)) File.Delete(dosya); } catch { }
            string r = StudyoWindow.PlayByName(parts[0], satir, false);
            if (!r.StartsWith("oynat")) return r;
            SessionState.SetBool(OAktif, true);
            SessionState.SetString(OAsama, "bekle");
            SessionState.SetString(OZaman, D(EditorApplication.timeSinceStartup));
            SessionState.SetFloat(OGecikme, gecikme);
            SessionState.SetInt(OSatir, satir);
            SessionState.SetString(OHikaye, parts[0]);
            SessionState.SetString(ODosya, dosya);
            return "başladı: " + parts[0] + (satir > 0 ? " " + satir + ". satır" : "") + ", ~" + gecikme.ToString("0.#", CultureInfo.InvariantCulture) + " sn sonra ekran.png + ekran.json yazılacak";
        }

        static void OnizlemeBitir(bool ok, string mesaj)
        {
            SessionState.SetBool(OAktif, false);
            string dosya = SessionState.GetString(ODosya, "");
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            UzaktanKomut.Cikti("ekran.json", "{\"tamam\": " + HsJson.B(ok) + ", \"hikaye\": " + HsJson.Q(SessionState.GetString(OHikaye, "")) +
                               ", \"satir\": " + SessionState.GetInt(OSatir, 0) + ", \"dosya\": " + HsJson.Q(dosya) + ", \"mesaj\": " + HsJson.Q(mesaj) + "}");
            UzaktanKomut.Append("sonuc.txt", System.DateTime.Now.ToString("HH:mm:ss") + " onizleme_goruntu " + (ok ? "tamam" : "HATA") + ": " + mesaj);
        }

        static void OnizlemeTick()
        {
            if (!SessionState.GetBool(OAktif, false)) return;
            double now = EditorApplication.timeSinceStartup;
            if (now - P(SessionState.GetString(OZaman, "0")) > 120.0) { OnizlemeBitir(false, "zaman aşımı: oynatma başlamadı ya da Game görünümü kapalı"); return; }
            string asama = SessionState.GetString(OAsama, "bekle");
            if (asama == "bekle")
            {
                if (!EditorApplication.isPlaying) return;
                SessionState.SetString(OGiris, D(now));
                SessionState.SetString(OAsama, "oynuyor");
                return;
            }
            if (asama == "oynuyor")
            {
                if (!EditorApplication.isPlaying) { OnizlemeBitir(false, "oynatma beklenmedik şekilde durdu"); return; }
                if (now - P(SessionState.GetString(OGiris, "0")) < SessionState.GetFloat(OGecikme, 3f)) return;
                string f = SessionState.GetString(ODosya, "");
                try { if (File.Exists(f)) File.Delete(f); } catch { }
                ScreenCapture.CaptureScreenshot(f);
                SessionState.SetString(OYazma, D(now));
                SessionState.SetString(OAsama, "yaziliyor");
                return;
            }
            if (asama == "yaziliyor")
            {
                string f = SessionState.GetString(ODosya, "");
                double t0 = P(SessionState.GetString(OYazma, "0"));
                bool yazildi = false;
                try { yazildi = File.Exists(f) && new FileInfo(f).Length > 0; } catch { }
                if (yazildi && now - t0 > 0.7) OnizlemeBitir(true, "ekran görüntüsü yazıldı: " + f);
                else if (now - t0 > 10.0) OnizlemeBitir(false, "ekran görüntüsü yazılmadı (Game görünümü açık ve görünür mü?)");
            }
        }
    }
}
