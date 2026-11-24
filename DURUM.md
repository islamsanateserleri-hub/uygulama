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
| 1 Unity'siz video kaydı | Kod yazıldı, doğrulanmadı. "Başla" bekleniyor |
| 2–7 | Başlanmadı |
| 8–16 | Başlanmadı (bkz. plan dışı önceden yazılanlar) |

### Aşama 0 (tamam)
Dört derleme birimi, arayüzler IVarlikKatalogu/IVideoKaydedici, KarakterKatalogu (68 karakter), "Hikaye/Windows Derlemesi (Deneme)". Test: 0 hata, örnek hikâye oynadı, Windows derlemesi başarılı (529 MB, 3 dk 17 sn, 5 uyarı); exe şimdilik arayüzsüz.

### Aşama 1 (kod var, test yok)
Yazılanlar: `Uygulama/FfmpegPipeRecorder.cs` (HikayeCanvas ScreenSpaceCamera, 1080x1920 RenderTexture, AsyncGPUReadback ile ffmpeg borusu, 6 karelik kuyruk, captureDeltaTime=1/fps, ses AudioRenderer→WAV→ffmpeg aac 192k, kodlayıcı sırası h264_nvenc, h264_mf, libx264), `Uygulama/KayitBootstrap.cs` (`-kayit <mp4> -hikaye <ad> -klaket` veya `ffkayit.txt`, sonuç `ffkayit_sonuc.txt`), `Editor/FfmpegIndir.cs` (menü "Hikaye/ffmpeg İndir", BtbN LGPL → StreamingAssets/ffmpeg), `StoryPlayer.Baslat()`.
Plan sırası (hepsi yapılacak): 1 derleme raporu (`HikayeKomut/derleme.txt`; son bilinen 2 hata Camera nearClip/farClip, düzeltildi, doğrulanmadı) · 2 FfmpegPipeRecorder derle · 3 ffmpeg/ffprobe indir · 4 lisans dosyaları · 5 KayitBootstrap çalıştır · 6 deneme_3d kaydet · 7 1080x1920, 30 FPS, MP4 doğrula · 8 ses aktarımı · 9 klaket (30. karede beyaz flaş + 1 kHz bip) · 10 aynı test Windows exe'de · 11 ağız senkronu · 12 kodlayıcı alternatifleri.
Başarı: Unity'siz video, exe tek başına, senkron, 1080x1920 30 FPS, anlaşılır kayıt hatası. Bitmeden Aşama 2'ye geçme.

## Plan dışı önceden yazılanlar (test edilmemiş = tamam sayılmaz)
3D karakterler hikâyeye bağlı, karaoke altyazı, bak/dinle, yüz canlılığı; 3D mekân, kamera yönetmeni, atmosfer, geçiş, ışık; Stüdyo köprüsü (komut.txt), doğrulayıcı, ritim denetçisi, toplu kayıt kuyruğu, Yapay Zekâ Kılavuzu. Unity'de derlendi ve oynadı; görsel kontrol yok.

## Açık sorunlar
- Mixamo/Quaternius'ta çene kemiği ve blendshape yok, ağız görünür açılmıyor (Aşama 9, VRoid).
- James.fbx "RightLowerArm" avatar uyarısı.
- Doğrulayıcı/Stüdyo penceresi `ItemType.Direct` ve yeni kamera değerlerini tanımıyor; `Docs/HikayeSozdizimi.md` 3D mekân komutlarını içermiyor.
- Derleme uyarıları: AppDomain.GetAssemblies (StudyoKayit, HikayeKuyrugu, AtmosferVolume), FindObjectsByType (YuzSahne), Sahne3D.Harita.

## KAYIT (yeni giriş en alta)
- 24 Kas 21:41: Plan PDF'i okundu, eski notlar `_Arsiv/`'e alındı, DURUM.md yazıldı. Aşama 1 başlamadı.
- 24 Kas 21:50: Kullanıcı sormadan GitHub'a göndermemi istedi. Klon alındı, cihazdaki kod/sahne/Resources dosyaları repoya eşitlendi.
- 24 Kas 21:55: GitHub push tamam: commit 22ebf5c (dal claude/hikaye-studyosu): 110 yeni, 11 değişen dosya. Sonraki: kullanıcı "başla" deyince Aşama 1 madde 1.
- 24 Kas 21:58: Telefondaki yeni sohbet DURUM.md'yi okuyamadı (klasör bağlı değildi). "Yeni sohbet nasıl başlatılır" bölümü eklendi.
