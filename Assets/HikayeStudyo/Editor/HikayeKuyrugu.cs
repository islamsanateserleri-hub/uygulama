using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // "Hepsini üret ve kaydet": hikaye kuyrugu. Her hikaye icin sirayla
    //   1) eksik sesler Gemini ile uretilir (anahtar varsa; kota biterse o hikaye atlanir),
    //   2) hikaye videoya alinir (StudyoWindow.PlayByName ... record = true),
    //   3) kayit denetimi (StudyoKayit.SonRapor) toplanir.
    // Durum EditorPrefs'te tutulur: Play moduna girip cikarken yapilan domain reload'u atlatir.
    // Ilerleme UnityEditor.Progress ile gosterilir (Window > General > Background Tasks).
    [InitializeOnLoad]
    public static class HikayeKuyrugu
    {
        const string KAktif = "HS_KuyrukAktif", KListe = "HS_KuyrukListe", KToplam = "HS_KuyrukToplam",
                     KSimdiki = "HS_KuyrukSimdiki", KAsama = "HS_KuyrukAsama", KSes = "HS_KuyrukSesUret",
                     KOynadi = "HS_KuyrukOynadi", KRapor = "HS_KuyrukRapor", KSesCagri = "HS_KuyrukSesCagrildi",
                     KBasla = "HS_KuyrukBaslangic", KProg = "HS_KuyrukProgress";
        static double sonraki;

        static HikayeKuyrugu()
        {
            EditorApplication.update += Tick;
        }

        public static bool Aktif { get { return EditorPrefs.GetBool(KAktif, false); } }
        public static string Durum
        {
            get
            {
                if (!Aktif) return "kuyruk boş";
                return "Kuyruk: " + (Toplam - Kalan.Length) + "/" + Toplam + " — " + EditorPrefs.GetString(KSimdiki, "") + " (" + EditorPrefs.GetString(KAsama, "") + ")";
            }
        }
        public static string SonRapor { get { return EditorPrefs.GetString(KRapor, ""); } }

        static int Toplam { get { return EditorPrefs.GetInt(KToplam, 0); } }
        static string[] Kalan
        {
            get
            {
                string s = EditorPrefs.GetString(KListe, "");
                return s.Length == 0 ? new string[0] : s.Split('|');
            }
        }

        // ---------- Menu ----------
        [MenuItem("Hikaye/Hepsini üret ve kaydet", priority = 60)]
        static void MenuBaslat()
        {
            if (Aktif) { EditorUtility.DisplayDialog("Hikaye kuyruğu", "Kuyruk zaten çalışıyor.\n" + Durum, "Tamam"); return; }
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog("Hikaye kuyruğu", "Önce oynatmayı durdur.", "Tamam"); return; }
            if (!StudyoKayit.RecorderInstalled) { EditorUtility.DisplayDialog("Hikaye kuyruğu", "Recorder paketi kurulu değil.", "Tamam"); return; }
            var all = TumHikayeler();
            if (all.Length == 0) { EditorUtility.DisplayDialog("Hikaye kuyruğu", "Hikaye bulunamadı: " + OtomatikKurulum.StoryFolder, "Tamam"); return; }
            if (!EditorUtility.DisplayDialog("Hikaye kuyruğu",
                    all.Length + " hikaye sırayla seslendirilip videoya alınacak:\n" + string.Join(", ", all) +
                    "\n\nBu sırada Unity'yi kullanma. Durdurmak için: Hikaye > Kuyruğu durdur.", "Başlat", "Vazgeç")) return;
            Baslat(all, true);
        }

        [MenuItem("Hikaye/Kuyruğu durdur", priority = 61)]
        static void MenuDurdur() { Durdur("kullanıcı durdurdu"); }

        [MenuItem("Hikaye/Kuyruğu durdur", true)]
        static bool MenuDurdurAktif() { return Aktif; }

        static string[] TumHikayeler()
        {
            var l = new List<string>();
            try
            {
                if (Directory.Exists(OtomatikKurulum.StoryFolder))
                    foreach (var f in Directory.GetFiles(OtomatikKurulum.StoryFolder, "*.txt")) l.Add(Path.GetFileNameWithoutExtension(f));
            }
            catch (System.Exception) { }
            l.Sort();
            return l.ToArray();
        }

        // ---------- Genel API ----------
        public static void Baslat(string[] hikayeler, bool sesUret)
        {
            if (Aktif || hikayeler == null || hikayeler.Length == 0) return;
            EditorPrefs.SetBool(KAktif, true);
            EditorPrefs.SetString(KListe, string.Join("|", hikayeler));
            EditorPrefs.SetInt(KToplam, hikayeler.Length);
            EditorPrefs.SetString(KSimdiki, "");
            EditorPrefs.SetString(KAsama, "");
            EditorPrefs.SetBool(KSes, sesUret);
            EditorPrefs.SetString(KRapor, "");
            ProgBaslat();
            Debug.Log("[Hikaye Kuyruğu] Başladı: " + hikayeler.Length + " hikaye.");
        }

        public static void Durdur(string neden)
        {
            if (!Aktif) return;
            string rapor = EditorPrefs.GetString(KRapor, "") + "DURDURULDU: " + neden + "\n";
            Temizle();
            EditorPrefs.SetString(KRapor, rapor);
            ProgBitir(false);
            Debug.LogWarning("[Hikaye Kuyruğu] Durduruldu: " + neden);
        }

        static void Temizle()
        {
            EditorPrefs.SetBool(KAktif, false);
            EditorPrefs.DeleteKey(KListe);
            EditorPrefs.DeleteKey(KSimdiki);
            EditorPrefs.DeleteKey(KAsama);
            EditorPrefs.DeleteKey(KOynadi);
            EditorPrefs.DeleteKey(KSesCagri);
        }

        // ---------- Progress ----------
        static int ProgId { get { return SessionState.GetInt(KProg, -1); } }

        static void ProgBaslat()
        {
            int id = Progress.Start("Hikaye kuyruğu", "Hazırlanıyor");
            SessionState.SetInt(KProg, id);
        }

        static void ProgRapor(string aciklama)
        {
            int id = ProgId;
            if (id < 0 || !Progress.Exists(id)) { ProgBaslat(); id = ProgId; }
            float f = Toplam > 0 ? Mathf.Clamp01((Toplam - Kalan.Length - (EditorPrefs.GetString(KSimdiki, "").Length > 0 ? 1 : 0)) / (float)Toplam) : 0f;
            Progress.Report(id, f, aciklama);
        }

        static void ProgBitir(bool basari)
        {
            int id = ProgId;
            if (id >= 0 && Progress.Exists(id))
                Progress.Finish(id, basari ? Progress.Status.Succeeded : Progress.Status.Failed);
            SessionState.SetInt(KProg, -1);
        }

        // ---------- Durum makinesi ----------
        static void Tick()
        {
            if (!Aktif) return;
            if (EditorApplication.timeSinceStartup < sonraki) return;
            sonraki = EditorApplication.timeSinceStartup + 0.7;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            string asama = EditorPrefs.GetString(KAsama, "");
            bool oynuyor = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
            if (oynuyor)
            {
                if (asama == "bekle") EditorPrefs.SetBool(KOynadi, true);
                return;
            }
            try
            {
                switch (asama)
                {
                    case "": Siradaki(); break;
                    case "ses": SesAsamasi(); break;
                    case "kayit": KayitAsamasi(); break;
                    case "bekle": BeklemeAsamasi(); break;
                    default: EditorPrefs.SetString(KAsama, ""); break;
                }
            }
            catch (System.Exception e)
            {
                Rapor(EditorPrefs.GetString(KSimdiki, "?") + ": HATA " + e.Message);
                EditorPrefs.SetString(KAsama, "");
            }
        }

        static void Rapor(string satir)
        {
            EditorPrefs.SetString(KRapor, EditorPrefs.GetString(KRapor, "") + satir + "\n");
            Debug.Log("[Hikaye Kuyruğu] " + satir);
        }

        static void Siradaki()
        {
            var kalan = new List<string>(Kalan);
            if (kalan.Count == 0) { Bitir(); return; }
            string ad = kalan[0];
            kalan.RemoveAt(0);
            EditorPrefs.SetString(KListe, string.Join("|", kalan.ToArray()));
            EditorPrefs.SetString(KSimdiki, ad);
            EditorPrefs.SetBool(KSesCagri, false);
            EditorPrefs.SetBool(KOynadi, false);
            EditorPrefs.SetString(KAsama, EditorPrefs.GetBool(KSes, true) ? "ses" : "kayit");
            ProgRapor(ad + " hazırlanıyor");
        }

        static StoryLibrary Lib()
        {
            var lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
            if (lib == null) { OtomatikKurulum.Ensure(); lib = AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath); }
            return lib;
        }

        static int EksikSes(StoryLibrary lib, string ad, List<StoryItem> items)
        {
            int n = 0;
            foreach (var it in items) if (it.Type == ItemType.Dialogue && !GeminiSes.HasClip(ad, lib, it)) n++;
            return n;
        }

        static List<StoryItem> Oku(string ad, out bool mevcut)
        {
            string path = OtomatikKurulum.StoryFolder + "/" + ad + ".txt";
            mevcut = File.Exists(path);
            return mevcut ? StoryParser.Parse(File.ReadAllText(path)).Items : new List<StoryItem>();
        }

        static void SesAsamasi()
        {
            if (GeminiSes.Busy) return;
            string ad = EditorPrefs.GetString(KSimdiki, "");
            bool mevcut;
            var items = Oku(ad, out mevcut);
            if (!mevcut) { Rapor(ad + ": hikaye dosyası yok, atlandı"); EditorPrefs.SetString(KAsama, ""); return; }
            var lib = Lib();
            int eksik = EksikSes(lib, ad, items);
            if (eksik == 0) { EditorPrefs.SetString(KAsama, "kayit"); return; }
            if (!EditorPrefs.GetBool(KSesCagri, false))
            {
                if (EditorPrefs.GetString(GeminiSes.KeyPref, "").Trim().Length == 0)
                {
                    Rapor(ad + ": " + eksik + " ses eksik ve Gemini anahtarı yok, atlandı");
                    EditorPrefs.SetString(KAsama, "");
                    return;
                }
                EditorPrefs.SetBool(KSesCagri, true);
                ProgRapor(ad + ": " + eksik + " ses üretiliyor");
                GeminiSes.Generate(lib, ad, items);
                return;
            }
            // uretim bitti ama hala eksik var (gunluk kota vb.): sessiz video kaydetme
            Rapor(ad + ": " + eksik + " satırın sesi hâlâ eksik (kota?), kayıt atlandı");
            EditorPrefs.SetString(KAsama, "");
        }

        static System.Type Tip(string full)
        {
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try { var t = a.GetType(full, false); if (t != null) return t; }
                catch (System.Exception) { }
            }
            return null;
        }

        static void KayitAsamasi()
        {
            string ad = EditorPrefs.GetString(KSimdiki, "");
            var t = Tip("HikayeStudyo.StudyoWindow");
            var m = t != null ? t.GetMethod("PlayByName", BindingFlags.Public | BindingFlags.Static) : null;
            if (m == null) { Rapor(ad + ": StudyoWindow.PlayByName bulunamadı"); EditorPrefs.SetString(KAsama, ""); return; }
            ProgRapor(ad + " kaydediliyor");
            string res = m.Invoke(null, new object[] { ad, 0, true }) as string;
            if (res != null && res.StartsWith("kayıt"))
            {
                EditorPrefs.SetString(KAsama, "bekle");
                EditorPrefs.SetBool(KOynadi, false);
                EditorPrefs.SetString(KBasla, EditorApplication.timeSinceStartup.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                Rapor(ad + ": kayıt başlamadı — " + res);
                EditorPrefs.SetString(KAsama, "");
            }
        }

        static void BeklemeAsamasi()
        {
            string ad = EditorPrefs.GetString(KSimdiki, "");
            bool oynadi = EditorPrefs.GetBool(KOynadi, false);
            double t0;
            if (!double.TryParse(EditorPrefs.GetString(KBasla, "0"), System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture, out t0)) t0 = 0;
            double gecen = EditorApplication.timeSinceStartup - t0;
            if (!oynadi)
            {
                // Play modu hic gorulmediyse bir sure bekle (baslama gecikmesi), sonra vazgec
                if (gecen > 120.0) { Rapor(ad + ": oynatma başlamadı"); EditorPrefs.SetString(KAsama, ""); }
                return;
            }
            // Play bitti: kayit denetimi (StudyoKayit) ve KayitTakip (bitis_kontrol) tamamlanana kadar bekle
            if (SessionState.GetBool("HS_KayitDenetim", false)) return;
            if (SessionState.GetString("HS_KT_Durum", "") == "bitis_kontrol") return;
            bool ok = StudyoKayit.SonKayitTamam;
            string r = StudyoKayit.SonRapor;
            Rapor(ad + ": " + (ok ? "TAMAM" : "SORUN") + " — " + (r.Length > 0 ? r : "kayıt denetimi yok"));
            EditorPrefs.SetString(KAsama, "");
        }

        static void Bitir()
        {
            string rapor = EditorPrefs.GetString(KRapor, "");
            bool sorunsuz = rapor.IndexOf("SORUN", System.StringComparison.Ordinal) < 0 &&
                            rapor.IndexOf("atlandı", System.StringComparison.Ordinal) < 0 &&
                            rapor.IndexOf("HATA", System.StringComparison.Ordinal) < 0 &&
                            rapor.IndexOf("başlamadı", System.StringComparison.Ordinal) < 0;
            Temizle();
            EditorPrefs.SetString(KRapor, rapor);
            ProgBitir(sorunsuz);
            var sb = new StringBuilder("[Hikaye Kuyruğu] Bitti.\n");
            sb.Append(rapor);
            if (sorunsuz) Debug.Log(sb.ToString()); else Debug.LogWarning(sb.ToString());
        }
    }
}
