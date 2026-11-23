using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Game penceresini 1080x1920 (TikTok dikey) yapar. Unity'nin ic API'sini kullanir; olmazsa sessizce gecer.
    public static class GameViewUtil
    {
        const int Wd = 1080, Ht = 1920;
        const string Label = "TikTok 1080x1920";

        public static void Portrait()
        {
            try
            {
                var asm = typeof(Editor).Assembly;
                var sizesType = asm.GetType("UnityEditor.GameViewSizes");
                var single = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var inst = single.GetProperty("instance").GetValue(null, null);
                var groupType = sizesType.GetProperty("currentGroupType").GetValue(inst, null);
                var group = sizesType.GetMethod("GetGroup").Invoke(inst, new[] { groupType });
                var gType = group.GetType();
                int count = (int)gType.GetMethod("GetTotalCount").Invoke(group, null);
                int index = -1;
                for (int i = 0; i < count; i++)
                {
                    var size = gType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                    var st = size.GetType();
                    int w = (int)st.GetProperty("width").GetValue(size, null);
                    int h = (int)st.GetProperty("height").GetValue(size, null);
                    if (w == Wd && h == Ht) { index = i; break; }
                }
                if (index < 0)
                {
                    var sizeType = asm.GetType("UnityEditor.GameViewSize");
                    var kindType = asm.GetType("UnityEditor.GameViewSizeType");
                    var fixedRes = System.Enum.Parse(kindType, "FixedResolution");
                    var ctor = sizeType.GetConstructor(new[] { kindType, typeof(int), typeof(int), typeof(string) });
                    var newSize = ctor.Invoke(new object[] { fixedRes, Wd, Ht, Label });
                    gType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
                    index = (int)gType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
                }
                var gvType = asm.GetType("UnityEditor.GameView");
                var gv = EditorWindow.GetWindow(gvType, false, null, false);
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var prop = gvType.GetProperty("selectedSizeIndex", flags);
                if (prop != null && prop.CanWrite) prop.SetValue(gv, index, null);
                else
                {
                    var m = gvType.GetMethod("SizeSelectionCallback", flags);
                    if (m != null) m.Invoke(gv, new object[] { index, null });
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("[Hikaye Stüdyosu] Game penceresi otomatik dikey yapılamadı (" + e.GetType().Name + "). Game penceresinin üstündeki boyut menüsünden 1080x1920 seç.");
            }
        }
    }
}
