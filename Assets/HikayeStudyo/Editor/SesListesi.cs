using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Gemini'de elle ses uretmek icin liste: dosya adi, karakter, ses, ton, okunacak metin.
    // Ayrica Gemini TTS hazir ses katalogu (ses_listele komutu ve yapay zeka kilavuzu icin).
    public static class SesListesi
    {
        // Gemini TTS hazir sesleri ve karakterleri (Google dokumantasyonu)
        public static readonly string[,] Katalog =
        {
            { "Zephyr", "Parlak" }, { "Puck", "Neşeli" }, { "Charon", "Bilgilendirici" }, { "Kore", "Kararlı" },
            { "Fenrir", "Heyecanlı" }, { "Leda", "Genç" }, { "Orus", "Kararlı" }, { "Aoede", "Esintili" },
            { "Callirrhoe", "Rahat" }, { "Autonoe", "Parlak" }, { "Enceladus", "Nefesli" }, { "Iapetus", "Berrak" },
            { "Umbriel", "Rahat" }, { "Algieba", "Pürüzsüz" }, { "Despina", "Pürüzsüz" }, { "Erinome", "Berrak" },
            { "Algenib", "Kalın/Çatlak" }, { "Rasalgethi", "Bilgilendirici" }, { "Laomedeia", "Neşeli" }, { "Achernar", "Yumuşak" },
            { "Alnilam", "Kararlı" }, { "Schedar", "Dengeli" }, { "Gacrux", "Olgun" }, { "Pulcherrima", "Dik/İleri" },
            { "Achird", "Dostça" }, { "Zubenelgenubi", "Gündelik" }, { "Vindemiatrix", "Nazik" }, { "Sadachbia", "Canlı" },
            { "Sadaltager", "Bilgili" }, { "Sulafat", "Sıcak" }
        };

        public static bool SesVar(string ad)
        {
            for (int i = 0; i < Katalog.GetLength(0); i++)
                if (string.Equals(Katalog[i, 0], ad, System.StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static string Ozellik(string ad)
        {
            for (int i = 0; i < Katalog.GetLength(0); i++)
                if (string.Equals(Katalog[i, 0], ad, System.StringComparison.OrdinalIgnoreCase)) return Katalog[i, 1];
            return "";
        }

        public static List<string> SesAdlari()
        {
            var l = new List<string>();
            for (int i = 0; i < Katalog.GetLength(0); i++) l.Add(Katalog[i, 0]);
            return l;
        }

        public static string Metin(StoryLibrary lib, ParseResult parsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Dosya adı\tKarakter\tGemini sesi\tTon\tOkunacak metin\tAltyazı");
            int count = 0;
            foreach (var it in parsed.Items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                string raw = string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech;
                sb.AppendLine(lib.ClipKey(it) + ".wav\t" + it.Character + "\t" + lib.VoiceFor(it) + "\t" + GeminiSes.StilKisalt(Util.ToneFor(it)) +
                              "\t" + TurkceMetin.ForSpeech(raw) + "\t" + it.Text);
                count++;
            }
            sb.AppendLine();
            sb.AppendLine("Toplam satır: " + count + " (Gemini ücretsiz kota günde ~10-11 satır; yaklaşık " + Mathf.CeilToInt(count / 10f) + " gün)");
            return sb.ToString();
        }

        public static void Export(StoryLibrary lib, TextAsset story)
        {
            if (lib == null || story == null) return;
            var parsed = StoryParser.Parse(story.text);
            var sb = new StringBuilder(Metin(lib, parsed));
            sb.AppendLine("Üretilen dosyaları bu adlarla " + GeminiSes.Dir(story.name) + "/ klasörüne koy.");
            var path = EditorUtility.SaveFilePanel("Ses listesi", Application.dataPath, story.name + "_ses_listesi", "txt");
            if (!string.IsNullOrEmpty(path)) System.IO.File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }
    }
}
