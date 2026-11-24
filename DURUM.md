# HİKÂYE STÜDYOSU: DURUM

Tek kayıt dosyası. Her işlem, test sonucu ve hata yalnızca buraya eklenir (en fazla 1-2 sayfa; eskiyi kısalt). Yeni sohbet bu dosyayı okuyarak eksiksiz devam eder.
Eski defterler/notlar `_Arsiv/` altında, okunmaz. Plan: `Hikaye_Studyo_Gelistirme_Plani.pdf` (kullanıcıdan alınır, sıra değişmez).

## KULLANICI KURALLARI (sırayla uy)
1. PDF planına harfiyen uy; sıra değişmez; aşamayı bitirmeden diğerine geçme; test edilmemişi "tamam" yazma.
2. Aşama aşama ilerle; kullanıcı onayı olmadan sonraki aşamaya geçme. Takılırsan dur ve kullanıcıya bildir.
3. Kullanıcıya soru sorma. Her aşama sonunda GitHub'a gönder (dal claude/hikaye-studyosu), sonucu buraya yaz, kullanıcıya kısa özet ver.
4. Çalışan özellikleri bozma; değişiklikten önce yedek; her değişiklikten sonra derlemeyi kontrol et; hataları gizleme.
5. Her aşama sonu raporu: yapılanlar, değişen dosyalar, eklenenler, düzeltilen hatalar, derleme sonucu, gerçek test, açık sorunlar, commit, sonraki aşama.
6. Hedef: Unity bilmeyen kullanıcı hikâyeyi yazıp karakter seçerek tek tuşla 1080x1920 sesli, animasyonlu film üretsin (bağımsız Windows uygulaması).

## YENİ SOHBET NASIL BAŞLATILIR
1. Bilgisayardaki Claude masaüstü uygulamasında yeni sohbet aç, + menüsünden **Klasör ekle** ile `C:\Users\Teknodc\My project` klasörünü bağla (bağlanmazsa Claude cihaz dosyalarını göremez ve DURUM.md'yi okuyamaz).
2. İlk mesaj: "DURUM.md dosyasını oku ve oradaki kurallara göre devam et. güncel aşamadan devam et." (Klasör bağlanamazsa: aynı DURUM.md GitHub'da, islamsanateserleri-hub/uygulama, dal claude/hikaye-studyosu, kök dizin.)

## ÇALIŞMA ORTAMI (yeni sohbet için)
- Proje: `C:\Users\Teknodc\My project` (Unity 6000.6.9f1, URP). Bilgisayar DESKTOP-49TN47M. Claude'un konsolu yok (device_bash yok).
- Dosya aktarımı: `device_stage_files` (cihaz → /mnt/user-data/uploads/My project/...), `device_commit_files` (/mnt/user-data/outputs/My project/... → cihaz, force:true).
- Unity'yi sürmek için `HikayeKomut/komut.txt`'ye tek satır komut yaz: `yenile`, `menu: <yol>`, `oynat: <hikâye>`, `kaydet:`, `seslendir:`, `dur`, `ekran`. Sonuçlar: `HikayeKomut/sonuc.txt`, `son_sonuc.json`, `konsol.txt`, `derleme.txt`. Unity arka plandayken 30–60 sn geç yanıtlar; komut tüketilince dosya silinir; "zaten oynuyor" = önce `dur`.
- Bash'te paket kurulumu kapalı. Foreground `sleep` ≤100 sn.
- GitHub: islamsanateserleri-hub/uygulama, dal claude/hikaye-studyosu (PR #1). Repo yalnızca `Assets/`, README, CLAUDE.md, GEMINI.md, Docs içerir; .gitignore Library/Temp/Build/*.wav/*.mp4 ve Mixamo klasörlerini dışlar. Push: klonla, cihazdan stage edilen dosyaları kopyala, commit, push.
- Yedek: `Yedek_2026-11-24/`. Yapı: Hikaye.Core (Core/), Hikaye.Runtime (Scripts/), Hikaye.Editor (Editor/), Hikaye.App (Uygulama/).

## AŞAMALAR
| Aşama | Durum |
|---|---|
| 0 Hazırlık | TAMAM |
| 1 Unity'siz video kaydı | TAMAM, onaylandı (12/12 madde denendi) |
| 2 Bağımsız uygulama arayüzü | TAMAM, onaylandı (commit d7e8e23) |
| 3 Hikâye yazma ekranı + otomatik sahne | TAMAM, onaylandı (commit 9082643) |
| 4 Gemini ses sistemi | **YAZILDI, derlendi (0 hata), EXE'de KISMEN test edildi; gerçek Gemini isteği denenmedi** (Aşama 5'e onaysız geçme) |
| 5–16 | Başlanmadı (5 = video düzenleme/dışa aktarma) |

## Özet: Aşama 0-2 (tamam)
- Aşama 0: dört derleme birimi (Core/Runtime/Editor/App), KarakterKatalogu (68 karakter), menü "Hikaye/Windows Derlemesi (Deneme)".
- Aşama 1: `Uygulama/FfmpegPipeRecorder.cs` (HikayeCanvas → ScreenSpaceCamera → 1080x1920 RenderTexture → ffmpeg borusu, ses WAV→aac), `KayitBootstrap.cs` (`-kayit <mp4> -hikaye <ad>`), Editör ve EXE'de deneme_3d kaydedildi (30 fps, h264+AAC, klaket). Kodlayıcı sırası nvenc → amf → qsv → mf → libx264; bu PC'de amf.
- Aşama 2: `Uygulama/AnaEkran.cs` (IMGUI; 8 bölüm: Yeni hikâye, Hikâyelerim, Karakterlerim, Mekânlar, Ses, Video, Projeler, Ayarlar), "Hikâye Stüdyosu" kimliği, masaüstü kısayolu; Unity Editör kapalıyken test edildi. GUI.BeginScrollView ölçek≠1'de tıklamayı bozduğu için kendi kaydırma alanı (ListeBasla/ListeBitir).
- Plan dışı önceden yazılanlar (3D karakter/mekân/kamera/atmosfer, Stüdyo köprüsü, doğrulayıcı vb.) Editörde oynadı, ayrıca görsel doğrulanmadı.

## Aşama 3 (tamam, onaylı; ayrıntı için git geçmişi 9082643)
`AnaEkran.Yazma.cs` (yazma ekranı + sahne panosu), `HikayeDenetci.cs`, `CanliOnizleme.cs`. Satır numaralı editör, hızlı ekleme çubuğu, hata paneli + Düzelt, Geri al/Yinele, otomatik kayıt (Documents\HikayeStudyosu\Hikayeler), canlı 9:16 önizleme, sahne panosu (küçük resim önbelleği, taşı, karakter/kamera değiştir). EXE'de gerçek test edildi.
Açık: yazarak Ctrl+Z, Ctrl+Y/Ctrl+S, "Satır N'den oynat", Türkçe karakter yazımı, 200+ satır performansı test EDİLMEDİ. Eski: Mixamo ağız hareketi yok (Aşama 9); görev çubuğu simgesi doğrulanmadı; Aşama 2 denenmeyenler (Klasörü aç, ses kaydırgacı etkisi, 60 FPS kayıt, ayarları sıfırla).

## Aşama 4: Gemini ses sistemi (yazıldı, kısmen test)
Yeni: `Scripts/DisSes.cs` (kalıcı ses klasörü Documents\HikayeStudyosu\Ses\<hikâye>\<anahtar>.wav, WAV okuyucu, karakter→ses ataması PlayerPrefs `HS_SesAta_*`, deneme sesi klasörü `_deneme`), `Uygulama/GeminiUygulama.cs` (Gemini TTS isteği, 400/429/kesik ses yeniden denemesi, kota ~10/gün, yedek `_yedek`), `Uygulama/AnaEkran.Ses.cs` (Ses ayarları sayfası + yazma ekranında "Ses" sekmesi). Değişen: `StoryLibrary.VoiceFor` (atamayı okur), `StoryPlayer.PreloadVoices` (Resources'ta yoksa kalıcı klasör), `AnaEkran.cs`/`AnaEkran.Yazma.cs`. Anahtar = ses+ton+metin → yalnız değişen satır yeniden üretilir. Mevcut miks (−14 LUFS, −1 dBFS sınırlayıcı, otomatik kazanç, konuşurken müzik kısma) AYNEN.
- Derleme: DLL'ler 0 hata; EXE Succeeded (793 MB).
- GERÇEK TEST (EXE, Editör kapalı): Ses ayarları açıldı, anahtar yokken "Anahtar yok" + üret düğmesi pasif; Arda Puck→Fenrir atandı, uygulama kapat-aç sonrası kaldı, Ses sekmesinde yalnız Arda'nın dış satırı Fenrir oldu; Ses sekmesi 8 satırı "Eksik" (0/8) listeledi; "Deneme sesi üret" 8 sentetik ses yazdı (Gemini sesi DEĞİL, kota harcamaz); deneme sesi açıkken Oyna sesli oynadı, rapor −17,1 LUFS (hedef −14), tepe −7,5 dBFS, kazanç +6,0 dB → ses→miks→ölçüm hattı çalışıyor. Deneme anahtarı testten sonra KAPATILDI.
- TEST EDİLMEDİ: gerçek Gemini isteği (API anahtarı yok; anahtar kutusuna ben anahtar yazmam), kota sayacı, 429/400 yolları, yeniden üret+yedek+geri alma, kesik ses yeniden denemesi, Dinle düğmesi (deneme sesi kapalıyken deneme dosyası yüklenmiyor: düzeltilecek; ayrıca sesi duyamıyorum).
- Bilinen kusur: dar pencerede Ses sekmesi üst düğmeleri üst üste biniyor (düzeltilecek). Aşama 5'te `kaydet` deneme sesi varken REDDETMELİ.
- Test notu: Claude penceresi ekranın sağında açılınca oradaki tıklamaları yakalıyor; uygulamayı Win+Sol ile sola yapıştırıp denedim (Düzenle "çalışmıyor" gibi göründü, uygulama hatası değildi).

## DERSLER
- `device_commit_files` önceki halini yükleyebilir: yaz, `sync; sleep 6`, commit, `device_stage_files` ile boyut doğrula.
- Unity arka plandayken derlemez: Hub'dan "My project" (530,293), 45 sn bekle, Assets (89,40) → Refresh (120,486); derleme `Library/ScriptAssemblies/Hikaye.App.dll` içinde metinle doğrulanır. Çıkış: File (16,40) → Exit (44,354). Exe: Gezgin → Masaüstü (70,134) → kısayol (240,291) → Giriş (80,35) → Aç (722,62); iki Gezgin penceresi olabilir.
- C# 9: renk üçlü ifadelerinde Color32/Color karışımı hedef türüyle çalışır; GUI grup/kaydırma arasında `return` yapma.

## Sonraki adım
1) Dinle düzeltmesi + düğme yerleşimi, yeniden derle/dene. 2) Kullanıcı API anahtarını girerse 1 satır gerçek Gemini testi. 3) Aşama 4 raporu, onay beklenir; Aşama 5'e onaysız geçme. Her adımda DURUM.md ve "DURUM.dm" Doc'u (Claude Docs, claude.ai/artifact/1RntmDHcKyA7NvjmKDoybk) birlikte güncelle.
