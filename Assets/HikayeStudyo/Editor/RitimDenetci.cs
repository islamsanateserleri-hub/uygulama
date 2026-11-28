using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Ritim denetcisi: hikayedeki "planlari" (kamera kadrajlari) simule eder, her planin suresini hesaplar,
    //   > 5 sn sabit plan: SARI,  > 8 sn: KIRMIZI;  plan cesitliligi (genis / orta / yakin) yuzdelerini raporlar.
    // Studyo penceresi Rapor(p, lib) metnini (satir satir) gosterebilir; satirlar "[KIRMIZI] ", "[SARI] " ile baslar.
    // Kamera simulasyonu calisma zamani ile aynidir: 3D mekanda KameraYonetmen.PlanSec, 2D mekanda StoryPlayer kurallari.
    public static class RitimDenetci
    {
        public const float SariSn = 5f, KirmiziSn = 8f;

        public enum Seviye { Bilgi, Sari, Kirmizi }

        public class Satir
        {
            public string Metin;
            public Seviye Seviye;
            public int HikayeSatiri;
        }

        public class Sonuc
        {
            public readonly List<Satir> Satirlar = new List<Satir>();
            public float ToplamSn, GenisSn, OrtaSn, YakinSn, OrtalamaSn;
            public int PlanSayisi, SariSayisi, KirmiziSayisi;
        }

        class Plan
        {
            public string Sinif;      // genis | orta | yakin | kart
            public string Kisi = "";
            public float Sn;
            public int Bas, Son;
        }

        // ---------- Studyo penceresi icin basit metin ----------
        public static string Rapor(ParseResult p, StoryLibrary lib)
        {
            var r = Analiz(p, lib, "");
            var sb = new StringBuilder();
            foreach (var s in r.Satirlar)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(Onek(s.Seviye)).Append(s.Metin);
            }
            return sb.ToString();
        }

        static string Onek(Seviye s)
        {
            return s == Seviye.Kirmizi ? "[KIRMIZI] " : (s == Seviye.Sari ? "[SARI] " : "");
        }

        public static Color Renk(Seviye s)
        {
            if (s == Seviye.Kirmizi) return new Color(1f, 0.35f, 0.3f);
            if (s == Seviye.Sari) return new Color(1f, 0.82f, 0.2f);
            return new Color(0.85f, 0.85f, 0.85f);
        }

        // IMGUI: renkli satirlari ciz
        public static void GuiCiz(Sonuc r)
        {
            if (r == null) return;
            var st = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
            var eski = GUI.contentColor;
            foreach (var s in r.Satirlar)
            {
                GUI.contentColor = Renk(s.Seviye);
                GUILayout.Label(s.Metin, st);
            }
            GUI.contentColor = eski;
        }

        // ---------- Analiz ----------
        public static Sonuc Analiz(ParseResult p, StoryLibrary lib, string hikayeAdi)
        {
            var r = new Sonuc();
            if (p == null) return r;
            var planlar = new List<Plan>();
            Plan cur = null;
            string mod = "otomatik";
            bool oda = false;
            int p1 = -1, p2 = -1, genistenBeri = 0, wideRun = 0;
            string sonKisi = "";
            var kisiler = new HashSet<string>();
            bool tepki = false;
            float ek = 0f;

            foreach (var it in p.Items)
            {
                switch (it.Type)
                {
                    case ItemType.Background:
                        cur = null; oda = false; p1 = p2 = -1; genistenBeri = 0; wideRun = 0; kisiler.Clear(); ek += 0.6f;
                        break;
                    case ItemType.Clear:
                        cur = null; kisiler.Clear();
                        break;
                    case ItemType.Direct:
                        {
                            string key = Util.Norm(it.Character), n = Util.Norm(it.Arg);
                            if (key == "mekan3d") { oda = n != "yok" && n != "kapat" && n.Length > 0; cur = null; p1 = p2 = -1; genistenBeri = 0; kisiler.Clear(); }
                            else if (key == "gecis") { cur = null; ek += 0.4f; }
                            else if (key == "sessizlik")
                            {
                                float sn = Util.ParseFloat(it.Arg, 2f);
                                if (cur == null) cur = Yeni(planlar, "orta", sonKisi, it.Line);
                                cur.Sn += sn; cur.Son = it.Line;
                            }
                        }
                        break;
                    case ItemType.Camera:
                        {
                            var t = (it.Arg ?? "").Trim().Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            string c = t.Length > 0 ? Util.Norm(t[0]) : "";
                            if (c == "yakin" || c == "genis" || c == "otomatik") mod = c;
                            if (c == "tepki") tepki = true;
                            else if (c != "salla" && c != "bulanik" && c != "net") cur = null;   // yeni kadraj / dolly = yeni plan
                        }
                        break;
                    case ItemType.Wait:
                        if (cur == null) cur = Yeni(planlar, "genis", sonKisi, it.Line);
                        cur.Sn += it.Seconds; cur.Son = it.Line;
                        break;
                    case ItemType.Headlights:
                        if (cur != null) { cur.Sn += 1.5f; cur.Son = it.Line; }
                        break;
                    case ItemType.Hook: Kart(planlar, 1.8f, it.Line); cur = null; break;
                    case ItemType.Rewind: Kart(planlar, 1.6f, it.Line); cur = null; break;
                    case ItemType.Title: Kart(planlar, 2.2f, it.Line); cur = null; break;
                    case ItemType.Choice: Kart(planlar, 4f, it.Line); cur = null; break;
                    case ItemType.Result: Kart(planlar, 2.2f, it.Line); cur = null; break;
                    case ItemType.DevamKarti: Kart(planlar, 2.9f, it.Line); cur = null; break;
                    case ItemType.End: Kart(planlar, it.Arg == "sert" ? 1.2f : 2.8f, it.Line); cur = null; break;
                    case ItemType.Dialogue:
                        {
                            float dur = Sure(it, lib, hikayeAdi) + ek; ek = 0f;
                            string kisi = Util.Norm(it.Character);
                            bool anlatici = Anlatici(it, lib);
                            if (anlatici && cur != null) { cur.Sn += dur; cur.Son = it.Line; break; }
                            string e = Util.Norm(it.Emotion);
                            bool guclu = e == "aglamakli" || e == "korkmus" || e == "sasirmis" || e == "sevgi" || e == "kizgin" || e == "uzgun";
                            string sinif;
                            bool devam;
                            if (oda && !anlatici)
                            {
                                kisiler.Add(kisi);
                                bool dinleyen = kisiler.Count > 1;
                                KameraYonetmen.Plan pl;
                                if (tepki && dinleyen && kisi != sonKisi) pl = KameraYonetmen.Plan.Tepki;
                                else pl = KameraYonetmen.PlanSec(p1, p2, genistenBeri, it.Inner, guclu, dinleyen, mod);
                                tepki = false;
                                bool ayniPlan = p1 == (int)pl;
                                p2 = p1; p1 = (int)pl;
                                if (pl == KameraYonetmen.Plan.Genis) genistenBeri = 0; else genistenBeri++;
                                sinif = pl == KameraYonetmen.Plan.Genis ? "genis" : (pl == KameraYonetmen.Plan.Yakin || pl == KameraYonetmen.Plan.Tepki ? "yakin" : "orta");
                                devam = cur != null && ayniPlan && kisi == sonKisi && mod != "otomatik";
                            }
                            else
                            {
                                bool yakin;
                                if (mod == "yakin") yakin = true;
                                else if (mod == "genis") yakin = false;
                                else yakin = guclu || it.Inner || wideRun >= 2;
                                if (yakin) wideRun = 0; else wideRun++;
                                sinif = yakin ? "yakin" : "genis";
                                devam = cur != null && cur.Sinif == sinif && (sinif == "genis" || cur.Kisi == kisi);
                            }
                            if (devam) { cur.Sn += dur; cur.Son = it.Line; }
                            else
                            {
                                cur = Yeni(planlar, sinif, kisi, it.Line);
                                cur.Sn = dur;
                            }
                            sonKisi = kisi;
                        }
                        break;
                }
            }

            // ---- rapor ----
            float topPlan = 0f;
            foreach (var pl in planlar)
            {
                r.ToplamSn += pl.Sn;
                if (pl.Sinif == "kart") continue;
                r.PlanSayisi++; topPlan += pl.Sn;
                if (pl.Sinif == "genis") r.GenisSn += pl.Sn;
                else if (pl.Sinif == "orta") r.OrtaSn += pl.Sn;
                else r.YakinSn += pl.Sn;
            }
            r.OrtalamaSn = r.PlanSayisi > 0 ? topPlan / r.PlanSayisi : 0f;
            Ekle(r, Seviye.Bilgi, "Ritim: " + r.PlanSayisi + " plan, ortalama " + r.OrtalamaSn.ToString("0.0") + " sn, toplam ~" +
                 ((int)r.ToplamSn / 60) + ":" + ((int)r.ToplamSn % 60).ToString("00"), 0);

            foreach (var pl in planlar)
            {
                if (pl.Sinif == "kart") continue;
                string yer = pl.Bas == pl.Son ? pl.Bas + ". satır" : pl.Bas + "-" + pl.Son + ". satırlar";
                string ad = pl.Sinif + (pl.Kisi.Length > 0 ? ", " + pl.Kisi : "");
                if (pl.Sn > KirmiziSn)
                {
                    r.KirmiziSayisi++;
                    Ekle(r, Seviye.Kirmizi, pl.Sn.ToString("0.0") + " sn aynı plan (" + ad + "), " + yer + ": araya 'kamera: yakin' ya da başka karakter / bekle ekle.", pl.Bas);
                }
                else if (pl.Sn > SariSn)
                {
                    r.SariSayisi++;
                    Ekle(r, Seviye.Sari, pl.Sn.ToString("0.0") + " sn sabit plan (" + ad + "), " + yer + ".", pl.Bas);
                }
            }

            if (topPlan > 8f)
            {
                float g = r.GenisSn / topPlan, o = r.OrtaSn / topPlan, y = r.YakinSn / topPlan;
                Ekle(r, Seviye.Bilgi, "Plan dağılımı (süreye göre): geniş %" + Mathf.RoundToInt(g * 100f) + " · orta %" +
                     Mathf.RoundToInt(o * 100f) + " · yakın %" + Mathf.RoundToInt(y * 100f), 0);
                if (y < 0.15f) Ekle(r, Seviye.Sari, "Yakın plan az (%" + Mathf.RoundToInt(y * 100f) + "): duygusal satırlarda 'kamera: yakin' dene.", 0);
                if (g > 0.7f) Ekle(r, Seviye.Sari, "Geniş plan çok (%" + Mathf.RoundToInt(g * 100f) + "): görüntü durağan görünebilir.", 0);
                if (y > 0.8f) Ekle(r, Seviye.Sari, "Hep yakın plan (%" + Mathf.RoundToInt(y * 100f) + "): araya geniş plan ('kamera: genis') koy.", 0);
                if (r.OrtalamaSn > 5f) Ekle(r, Seviye.Sari, "Ortalama plan " + r.OrtalamaSn.ToString("0.0") + " sn: TikTok'ta 3-4 sn'de bir görüntü değişimi iyi çalışır.", 0);
            }
            return r;
        }

        static void Ekle(Sonuc r, Seviye s, string metin, int satir)
        {
            r.Satirlar.Add(new Satir { Metin = metin, Seviye = s, HikayeSatiri = satir });
        }

        static Plan Yeni(List<Plan> l, string sinif, string kisi, int satir)
        {
            var p = new Plan { Sinif = sinif, Kisi = kisi ?? "", Bas = satir, Son = satir };
            l.Add(p);
            return p;
        }

        static void Kart(List<Plan> l, float sn, int satir)
        {
            var p = Yeni(l, "kart", "", satir);
            p.Sn = sn;
        }

        static bool Anlatici(StoryItem it, StoryLibrary lib)
        {
            if (lib != null)
            {
                var d = lib.FindCharacter(it.Character);
                return d == null || lib.IsNarrator(d);
            }
            return Util.Norm(it.Character) == "anlatici";
        }

        // Satirin suresi: ses dosyasi varsa gercek uzunluk, yoksa metinden tahmin (+ satir arasi bosluk)
        static float Sure(StoryItem it, StoryLibrary lib, string hikayeAdi)
        {
            float tahmin = Mathf.Max(1.2f, it.Text.Length / 14f + 0.3f);
            if (lib != null && !string.IsNullOrEmpty(hikayeAdi))
            {
                try
                {
                    var clip = Resources.Load<AudioClip>("Ses/" + hikayeAdi + "/" + lib.ClipKey(it));
                    if (clip == null) clip = Resources.Load<AudioClip>("Ses/" + hikayeAdi + "/" + it.VoiceIndex.ToString("00"));
                    if (clip != null) tahmin = clip.length;
                }
                catch (System.Exception) { }
            }
            return tahmin + 0.22f;
        }

        // ---------- Menu: secili hikaye metni ----------
        [MenuItem("Hikaye/Ritim denetimi (seçili hikaye)", priority = 62)]
        static void Menu()
        {
            var ta = Selection.activeObject as TextAsset;
            if (ta == null) { Debug.LogWarning("[Hikaye] Project penceresinde bir hikaye .txt dosyası seç."); return; }
            var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
            var r = Analiz(StoryParser.Parse(ta.text), lib, ta.name);
            var sb = new StringBuilder("[Hikaye Ritim] " + ta.name + "\n");
            foreach (var s in r.Satirlar) sb.Append(Onek(s.Seviye)).Append(s.Metin).Append('\n');
            if (r.KirmiziSayisi > 0) Debug.LogError(sb.ToString());
            else if (r.SariSayisi > 0) Debug.LogWarning(sb.ToString());
            else Debug.Log(sb.ToString());
        }

        [MenuItem("Hikaye/Ritim denetimi (seçili hikaye)", true)]
        static bool MenuAktif() { return Selection.activeObject is TextAsset; }
    }
}
