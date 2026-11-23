using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Gemini'de elle ses uretmek icin liste: dosya adi, karakter, ses, ton, metin.
    public static class SesListesi
    {
        public static void Export(StoryLibrary lib, TextAsset story)
        {
            if (lib == null || story == null) return;
            var parsed = StoryParser.Parse(story.text);
            var sb = new StringBuilder();
            sb.AppendLine("Dosya adı\tKarakter\tGemini sesi\tTon\tMetin");
            int count = 0;
            foreach (var it in parsed.Items)
            {
                if (it.Type != ItemType.Dialogue) continue;
                sb.AppendLine(lib.ClipKey(it) + ".wav\t" + it.Character + "\t" + lib.VoiceFor(it) + "\t" + Util.ToneFor(it) + "\t" + it.Text);
                count++;
            }
            sb.AppendLine();
            sb.AppendLine("Toplam satır: " + count + " (Gemini ücretsiz kota günde ~10-11 satır; yaklaşık " + Mathf.CeilToInt(count / 10f) + " gün)");
            sb.AppendLine("Üretilen dosyaları bu adlarla " + GeminiSes.Dir(story.name) + "/ klasörüne koy.");
            var path = EditorUtility.SaveFilePanel("Ses listesi", Application.dataPath, story.name + "_ses_listesi", "txt");
            if (!string.IsNullOrEmpty(path)) System.IO.File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        }
    }
}
