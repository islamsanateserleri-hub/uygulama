# Unreal icindeki MetaHuman ve kayit araclarini listeler, sonra editoru kapatir.
import unreal, os, traceback
yol = os.path.join(os.path.dirname(os.path.abspath(__file__)), "yokla.txt")
s = []
try:
    s.append("surum: " + unreal.SystemLibrary.get_engine_version())
    s.append("rhi: " + str(unreal.SystemLibrary.get_rendering_detail_mode()) if hasattr(unreal.SystemLibrary, "get_rendering_detail_mode") else "rhi: ?")
    adlar = [a for a in dir(unreal) if "metahuman" in a.lower()]
    s.append("metahuman_adlari: " + str(len(adlar)))
    for a in adlar:
        s.append("  " + a)
        if any(k in a for k in ("Subsystem", "Library", "Character")) and len(a) < 60:
            try:
                o = getattr(unreal, a)
                s.append("     " + ", ".join(m for m in dir(o) if not m.startswith("_"))[:1500])
            except Exception as e:
                s.append("     hata " + str(e))
    s.append("moviepipeline: " + str(len([a for a in dir(unreal) if a.startswith("MoviePipeline")])))
except Exception:
    s.append(traceback.format_exc())
with open(yol, "w", encoding="utf-8") as f:
    f.write("\n".join(s))
unreal.SystemLibrary.quit_editor()
