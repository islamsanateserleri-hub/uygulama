using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;

namespace HikayeStudyo
{
    // Paket tiplerini (Recorder, glTFast) yansima ile bulmak icin yardimci.
    // AppDomain.GetAssemblies() yerine once CompilationPipeline'in derleme listesi kullanilir (UAC0005 uyarisi cikmaz);
    // o yolla bulunamazsa eski yontem (uyari bastirilmis olarak) son care olarak denenir.
    public static class AsmYardim
    {
        static readonly Dictionary<string, System.Type> bulunan = new Dictionary<string, System.Type>();
        static readonly Dictionary<string, double> yok = new Dictionary<string, double>();

        public static System.Type TipBul(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;
            System.Type t;
            if (bulunan.TryGetValue(fullName, out t)) return t;
            double now = EditorApplication.timeSinceStartup, until;
            if (yok.TryGetValue(fullName, out until) && now < until) return null;   // yakin zamanda arandi, yok

            t = System.Type.GetType(fullName, false);
            if (t == null) t = DerlemeListesindenBul(fullName);
            if (t == null) t = EskiYontem(fullName);
            if (t != null) { bulunan[fullName] = t; return t; }
            yok[fullName] = now + 5.0;   // paket sonradan kurulabilir: 5 sn sonra yeniden ara
            return null;
        }

        public static bool TipVar(string fullName) { return TipBul(fullName) != null; }

        static System.Type DerlemeListesindenBul(string fullName)
        {
            try
            {
                var asms = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
                foreach (var a in asms)
                {
                    System.Reflection.Assembly loaded = null;
                    try { loaded = System.Reflection.Assembly.Load(a.name); }
                    catch { loaded = null; }
                    if (loaded == null) continue;
                    System.Type t = null;
                    try { t = loaded.GetType(fullName, false); }
                    catch { t = null; }
                    if (t != null) return t;
                }
            }
            catch { }
            return null;
        }

#pragma warning disable UAC0005
        static System.Type EskiYontem(string fullName)
        {
            try
            {
                foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    System.Type t = null;
                    try { t = a.GetType(fullName, false); }
                    catch { t = null; }
                    if (t != null) return t;
                }
            }
            catch { }
            return null;
        }
#pragma warning restore UAC0005
    }
}
