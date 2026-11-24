# NOTLAR_C — Agent C (köprü, pencere, ses kalitesi, belgeler)

## Ne yaptım
- **Yeni:** `Scripts/KomutKayit.cs`, `Scripts/TurkceMetin.cs`, `Editor/AsmYardim.cs`, `Editor/StudyoDogrulama.cs` (Sorun, StudyoAnaliz, HsJson), `Editor/StudyoKilavuz.cs`, `Assets/Resources/telaffuz.txt`, `Docs/HikayeSozdizimi.md`, `CLAUDE.md`, `GEMINI.md`.
- **Yeniden yazıldı:** `UzaktanKomut.cs` (+`KayitTakip`), `StudyoWindow.cs`, `GeminiSes.cs`, `SesListesi.cs`.
- **Değişti:** `Karakter3DKurulum.cs` (rig yedekleri, AsmYardim), `OtomatikKurulum.cs` (telaffuz.txt oluşturur). `GameViewUtil.cs` değişmedi.
- Yamalar: `PATCH_C.md` (StoryLibrary.ClipKey, StudyoKayit.Type). Defter: `DEFTER_GUNCELLEME.md`.

## Yeni komutlar (HikayeKomut/komut.txt)
```
hikaye_yaz: bolum_2          (taslak: HikayeKomut/hikaye_bolum_2.txt; hata varsa yazmaz -> dogrulama.json)
dogrula: bolum_2 [proje]
karakter_listele | mekan_listele | ses_listele: bolum_2 | komut_listele | kota | kilavuz_yaz
onizleme_goruntu: bolum_2 7 3   (7. satır, 3 sn sonra ekran.png + ekran.json)
kayit_durumu                 (kayit_durumu.json)
```
`kaydet:` artık sesi eksik satır varsa reddeder. Sonuçlar: `sonuc.txt`, `son_sonuc.json`.

## Bilinen riskler (derleyemedim)
1. **Hiç derlenmedi.** Elle gözden geçirdim; ilk iş `yenile` + `derleme.txt`.
2. `StudyoKayit.LastVideo/RecorderInstalled/Key/NameKey`, `StoryPlayer.Finished/SonSesRaporu`, `Karakter3D.id/kategori/clips/startClip` alanlarının adı korunmalı (B/A değiştirirse derleme kırılır).
3. `AsmYardim.TipBul`: `Assembly.Load(ad)` yüklü derlemeyi bulamazsa eski `AppDomain` yoluna (uyarısı bastırılmış) düşer; işlev bozulmaz.
4. `onizleme_goruntu`: `ScreenCapture.CaptureScreenshot` Game görünümü açık/görünür değilse dosya yazmayabilir (10 sn sonra `ekran.json` `tamam:false`).
5. Ses dinleme `UnityEditor.AudioUtil` (iç API, yansıma); olmazsa sessizce atlar.
6. Hiyerarşiden rig çıkarımı (3. aşama) sol/sağı konumdan tahmin edebilir; `rig_raporu.txt` bunu belirtir.
7. `ClipKey` yaması uygulanırsa sayı/kısaltma içeren satırların sesi yeniden üretilir.
8. Doğrulayıcı kamera/atmosfer değerlerini kendisi denetlemez (B'nin değerleri değişken); parser uyarıları `PARSER_UYARI` olarak gelir. Yeni komutlar `StoryParser.DirectKeys` + parser yoklamasıyla bulunur; sabit liste yok.
9. `STIL_UZUN` iç ses satırlarında çıkar (stil 8 kelime); Gemini'ye otomatik kısaltılarak gider, anahtar değişmez.
10. Kilavuz üreteci belgeyi Unity'den yeniden yazar; `CLAUDE.md`/`GEMINI.md` farklıysa eskisi `.bak` olarak saklanır.
