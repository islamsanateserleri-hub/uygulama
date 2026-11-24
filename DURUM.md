# HİKÂYE STÜDYOSU: DURUM

Tek kayıt dosyası. Her işlem, test sonucu ve hata yalnızca buraya eklenir (en fazla 1-2 sayfa; eskiyi kısalt). Yeni sohbet bu dosyayı okuyarak eksiksiz devam eder.
Eski defterler/notlar `_Arsiv/` altında, okunmaz. Plan: `Hikaye_Studyo_Gelistirme_Plani.pdf` (kullanıcıdan alınır, sıra değişmez).

## KULLANICI KURALLARI (sırayla uy)
1. PDF planına harfiyen uy; sıra değişmez; aşamayı bitirmeden diğerine geçme; test edilmemişi "tamam" yazma.
2. Şimdi yalnızca **Aşama 1**. Kullanıcı **"başla"** demeden başlama. Takılırsan dur ve kullanıcıya bildir.
3. Kullanıcıya soru sorma. Her aşama sonunda GitHub'a gönder (dal claude/hikaye-studyosu), sonucu buraya yaz, kullanıcıya kısa özet ver.
4. Çalışan özellikleri bozma; değişiklikten önce yedek; her değişiklikten sonra derlemeyi kontrol et; hataları gizleme.
5. Her aşama sonu raporu: yapılanlar, değişen dosyalar, eklenenler, düzeltilen hatalar, derleme sonucu, gerçek test, açık sorunlar, commit, sonraki aşama.
6. Hedef: Unity bilmeyen kullanıcı hikâyeyi yazıp karakter seçerek tek tuşla 1080x1920 sesli, animasyonlu film üretsin (bağımsız Windows uygulaması).

## YENİ SOHBET NASIL BAŞLATILIR
1. Bilgisayardaki Claude masaüstü uygulamasında yeni sohbet aç, + menüsünden **Klasör ekle** ile `C:\Users\Teknodc\My project` klasörünü bağla (bağlanmazsa Claude cihaz dosyalarını göremez ve DURUM.md'yi okuyamaz).
2. İlk mesaj: "DURUM.md dosyasını oku ve oradaki kurallara göre devam et. Aşama 1'e başla." (Klasör bağlanamazsa: aynı DURUM.md GitHub'da, islamsanateserleri-hub/uygulama, dal claude/hikaye-studyosu, kök dizin.)

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
| 0 Hazırlık | TAMAM, denendi |
| 1 Unity'siz video kaydı | **TAMAM, kullanıcı onayı bekleniyor** (12/12 madde denendi; Aşama 2'ye onaysız geçme) |
| 2–7 | Başlanmadı |
| 8–16 | Başlanmadı (bkz. plan dışı önceden yazılanlar) |

### Aşama 0 (tamam)
Dört derleme birimi, arayüzler IVarlikKatalogu/IVideoKaydedici, KarakterKatalogu (68 karakter), "Hikaye/Windows Derlemesi (Deneme)". Test: 0 hata, örnek hikâye oynadı, Windows derlemesi başarılı (529 MB, 3 dk 17 sn, 5 uyarı); exe şimdilik arayüzsüz.

### Aşama 1 (tamam: Editörde ve EXE'de doğrulandı)
Yazılanlar: `Uygulama/FfmpegPipeRecorder.cs` (HikayeCanvas ScreenSpaceCamera, 1080x1920 RenderTexture, AsyncGPUReadback ile ffmpeg borusu, 6 karelik kuyruk, captureDeltaTime=1/fps, ses AudioRenderer→WAV→ffmpeg aac 192k, kodlayıcı sırası h264_nvenc, h264_mf, libx264), `Uygulama/KayitBootstrap.cs` (`-kayit <mp4> -hikaye <ad> -klaket` veya `ffkayit.txt`, sonuç `ffkayit_sonuc.txt`), `Editor/FfmpegIndir.cs` (menü "Hikaye/ffmpeg İndir", BtbN LGPL → StreamingAssets/ffmpeg), `StoryPlayer.Baslat()`.
Plan sırası (hepsi yapılacak): 1 derleme raporu (0 hata, doğrulandı) · 2 FfmpegPipeRecorder derle · 3 ffmpeg/ffprobe indir · 4 lisans dosyaları · 5 KayitBootstrap çalıştır · 6 deneme_3d kaydet · 7 1080x1920, 30 FPS, MP4 doğrula · 8 ses aktarımı · 9 klaket (30. karede beyaz flaş + 1 kHz bip) · 10 aynı test Windows exe'de · 11 ağız senkronu · 12 kodlayıcı alternatifleri.
Başarı: Unity'siz video, exe tek başına, senkron, 1080x1920 30 FPS, anlaşılır kayıt hatası. Bitmeden Aşama 2'ye geçme.

## Plan dışı önceden yazılanlar (test edilmemiş = tamam sayılmaz)
3D karakterler hikâyeye bağlı, karaoke altyazı, bak/dinle, yüz canlılığı; 3D mekân, kamera yönetmeni, atmosfer, geçiş, ışık; Stüdyo köprüsü (komut.txt), doğrulayıcı, ritim denetçisi, toplu kayıt kuyruğu, Yapay Zekâ Kılavuzu. Unity'de derlendi ve oynadı; görsel kontrol yok.

## Açık sorunlar
- Mixamo/Quaternius'ta çene kemiği ve blendshape yok, ağız görünür açılmıyor (Aşama 9, VRoid).
- James.fbx "RightLowerArm" avatar uyarısı.
- Doğrulayıcı/Stüdyo penceresi `ItemType.Direct` ve yeni kamera değerlerini tanımıyor; `Docs/HikayeSozdizimi.md` 3D mekân komutlarını içermiyor.
- Derleme uyarıları: AppDomain.GetAssemblies (StudyoKayit, HikayeKuyrugu, AtmosferVolume), FindObjectsByType (YuzSahne), Sahne3D.Harita.

## KAYIT (yeni giriş en alta)
- 21:41-21:58: PDF okundu, eski notlar `_Arsiv/`'e alındı, DURUM.md yazıldı; GitHub push 22ebf5c (bu commit hatalı nearClip sürümünü içeriyor, sonraki push düzeltir); yeni sohbet kılavuzu eklendi.
- 22:20 AŞAMA 1 BAŞLADI. Madde 1-4: cihazdaki FfmpegPipeRecorder.cs'te `cam.nearClip/farClip` düzeltmesi hiç uygulanmamıştı → düzeltildi, derleme **0 hata**. ffmpeg.exe+ffprobe.exe (138 MB) + LICENSE.txt (LGPL v3) Assets/StreamingAssets/ffmpeg/ altına indi (menüden; köprü komutu Türkçe İ yüzünden çalışmadı). libx264 LGPL sürümde yok → kodlayıcı nvenc/mf; .exe'ler GitHub'a girmeyecek.
- 22:28 Madde 5-9 (Editör Play, `ffkayit.txt` ile): deneme_3d.mp4 TAMAM: 488 kare, h264_mf, **1080x1920, 30 fps, MP4**, h264+AAC 48 kHz stereo, 16,23 sn video/16,26 sn ses, -14 LUFS. **Klaket TAMAM**: beyaz kare yalnız 30. karede (parlaklık 255), 1 kHz bip 0,98 sn'de; siyah kare yok; ses var. Bulunan/düzeltilen hatalar: (a) TikTok güvenli alan bantları videoya giriyordu → KayitIsleyici'de `HS_SafeArea=0`; (b) sahnedeki StoryPlayer(autoPlay) çakışıyordu → bootstrap kapatıyor; (c) render-graph "derinlik tamponu" uyarısı → RenderTexture derinlik 24.
- 22:30 Madde 11 (ağız): ağız Arda yüzünde görünür açılıyor; ağız açıklığı ile ses zarfı korelasyonu 0,53 (en iyi gecikme: ağız sesten 2 kare ≈ 67 ms geride). Kabul edilebilir, 3D Mixamo'da ağız yok (açık sorun).
- DERS: `device_commit_files` dosyanın BİR ÖNCEKİ halini yükleyebiliyor (yazdıktan hemen sonra commit edince eski içerik gidiyor; `sed -i`, Edit ve komut.txt'de görüldü). Yazdıktan sonra `sync; sleep 6` bekle, commit et, `device_stage_files` ile geri çekip boyutu kontrol et. Unity arka plandayken derlemeyi kendiliğinden başlatmaz: Assets > Refresh (menü (89,32) → Refresh (120,478)) ile derlet; derlemeden sonra Unity Hub penceresi öne gelip tıklamayı engelliyor → Hub'ı küçült (sağ üst (1155,37), Hub izni verildi), sonra köprü komutu yaz. Derleme gerçekten oldu mu: `Library/ScriptAssemblies/*.dll` mtime'ına bak.
- 22:37-22:46 Madde 10 (EXE) ilk denemeler: (1) pencere gizlenince (ekran görüntüsü alınca) oynatıcı duruyor, ffmpeg birleştirme takıldı → `Application.runInBackground=true`; (2) EXE'de "'Timmy' 3D karakter yok" (build sahnesi GetStarted_Scene, oyuncu listesi sahnede) → `Scripts/OyuncuListesi.cs` + `UygulamaDerle.OyuncuListesiYaz()` ile `Resources/OyuncuListesi.asset`, KayitBootstrap okuyor; (3) takılı eski ffmpeg dosyayı kilitledi → `OncekileriKapat()` (HikayeStudyo+ffmpeg süreçlerini kapatır); yeni menüler: "Hikaye/Exe Kayıt Testi (derle + kaydet)" ve "(yalnız çalıştır)" (sonuç `Build/exe_test_sonuc.txt`, günlük `Build/exe_log.txt`).
- 22:55 **Madde 10 TAMAM**: derlenen `HikayeStudyo.exe` (Unity Editör kapalı mantığıyla ayrı süreç, `-kayit <mp4> -hikaye deneme_3d -klaket`) 488 kare, 1080x1920, 30 fps, h264+AAC, 16,27 sn video üretti; beyaz flaş yalnız 30. karede, 1 kHz bip 0,98 sn, siyah kare yok, 3D karakterler + altyazı görünüyor (görsel kontrol edildi). 23:00 aynı test yeni kodlayıcı sırasıyla (h264_amf) tekrarlandı: aynı sonuç, 489 kare.
- 23:00 **Madde 12 TAMAM** (`Hikaye/Kodlayıcı Raporu` → `HikayeKomut/kodlayici_raporu.txt`): bu bilgisayarda (AMD Radeon iGPU) ÇALIŞIYOR: h264_amf, h264_mf, libopenh264. ÇALIŞMIYOR: h264_nvenc (nvcuda.dll yok), h264_qsv (MFX oturumu yok), libx264 (LGPL sürümde yok). Kayıt sırası: nvenc → amf → qsv → mf → libx264 → libopenh264 (her biri 0,3 sn deneme kodlamasıyla seçilir; hiçbiri çalışmazsa anlaşılır hata yazılır).
- Madde 11 (ağız senkronu) ve "98 birebir aynı kare": birebir aynı kareler siyah kanca/başlık kartında (sabit görüntü, beklenen); ağız ses zarfıyla 2 kare (≈67 ms) gecikmeli eşleşiyor, korelasyon 0,53; kabul, kayma yok.
- Aşama 1 AÇIK/NOT: EXE derlemesi 793 MB (ffmpeg 276 MB + 68 karakter prefabı); build sahnesi `GetStarted_Scene` (HikayeSahnesi değil, bootstrap kendi StoryPlayer'ını kurar); EXE günlüğünde URP DoF/Panini shader "stripped" uyarısı (post-process geçişleri çalışmıyor, video etkilenmedi) ve "SesOlcer aynı GameObject'te birden çok AudioSource" uyarısı; EXE'nin arayüzü yok (Aşama 2). Sonraki aşama: **Aşama 2, yalnız kullanıcı onayıyla**.
- 23:05 GitHub: Aşama 1 push tamam, commit 9dc451d (dal claude/hikaye-studyosu): 8 değişen/yeni kod dosyası, OyuncuListesi varlığı, ffmpeg LICENSE.txt + .meta, .gitignore (ffmpeg/ffprobe .exe ve Kayitlar/ hariç). Sonraki adım: kullanıcı onayı → Aşama 2.
