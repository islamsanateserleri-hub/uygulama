# PATCH_C: Agent C'nin sahip olmadigi dosyalar icin yamalar

Her BUL metni orijinal yuklenen dosyada **tam bir kez** gecer. Sirayla uygula. Hepsi OPSIYONELDIR: uygulanmazsa derleme bozulmaz, yalniz belirtilen ozellik eksik kalir.

Yeni dosyalar (yamasiz derlenir): `Scripts/KomutKayit.cs`, `Scripts/TurkceMetin.cs`, `Editor/AsmYardim.cs`, `Editor/StudyoDogrulama.cs`, `Editor/StudyoKilavuz.cs`.

---

## 1) StoryLibrary.cs  (Assets/HikayeStudyo/Scripts/StoryLibrary.cs)  — onbellek anahtari normalize metne gore

Etki: ses dosyasi adi (`ClipKey`) Turkce okunusa cevrilmis metinden (sayilar, kisaltmalar, telaffuz sozlugu) uretilir; sozluk/normalizasyon degisince ilgili satir yeniden uretilir. Uygulanmazsa ses yine normalize metinle uretilir ama dosya adi eski metinden hesaplanir (sozluk degisince eski ses "hazir" sayilir).
UYARI: sayi/kisaltma/telaffuz iceren satirlarin anahtari degisir; o satirlarin mevcut sesleri yeniden uretilmeli (diger satirlar etkilenmez, cunku normalize metin ayniysa anahtar ayni kalir).

BUL:
```csharp
            return Util.Hash(VoiceFor(it) + "|" + Util.ToneFor(it) + "|" + (string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech));
```

DEGISTIR:
```csharp
            return Util.Hash(VoiceFor(it) + "|" + Util.ToneFor(it) + "|" + TurkceMetin.ForSpeech(string.IsNullOrEmpty(it.Speech) ? it.Text : it.Speech));
```

---

## 2) StudyoKayit.cs  (Assets/HikayeStudyo/Editor/StudyoKayit.cs)  — UAC0005 uyarisi (derleme.txt) giderilir

BUL:
```csharp
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(fullName, false);
                if (t != null) return t;
            }
            return null;
```

DEGISTIR:
```csharp
            return AsmYardim.TipBul(fullName);
```

(Agent B bu metodu degistirdiyse: metnin govdesini sadece `return AsmYardim.TipBul(fullName);` yap.)

---

## 3) (Gerekmez, bilgi) StoryModel.cs — yeni komutlarin kaydi

Dogrulayici, "Ekle" menusu ve kilavuz yeni komutlari su sirayla bulur: (a) `KomutKayit.Kaydet(...)` ile kayitli olanlar, (b) `StoryParser.DirectKeys` (yansima), (c) bilinen aday adlar (bak, dinle, altyazi, mekan3d, yer, gir, ...) parser'a yoklanarak, (d) bilinmeyen komut adi icin `StoryParser.Parse("ad: 1")` uyarisina bakilarak. Yani yama olmadan da calisir. Yine de aciklamali kayit istenirse StoryParser'a statik kurucu ekle:

BUL (StoryParser sinifinin basi):
```csharp
    public static class StoryParser
    {
```

DEGISTIR:
```csharp
    public static class StoryParser
    {
        static StoryParser()
        {
            KomutKayit.Kaydet("mekan3d", "mekan3d: <ad>", "3D mekan kurar");
            // her yeni komut icin bir satir: KomutKayit.Kaydet("ad", "ornek: deger", "aciklama");
        }

```

Not: Dogrulayici kamera/atmosfer degerlerini kendisi denetlemez; bu degerlerin hatasi StoryParser'in `Warnings` listesinden `PARSER_UYARI` olarak gelir. Parser'a yeni komut eklerken yanlis degerde `r.Warnings.Add(lineNo + ". satır: ...")` yazmak yeterlidir (satir numarasi ile baslayan uyarilar sorun listesinde satira baglanir).
