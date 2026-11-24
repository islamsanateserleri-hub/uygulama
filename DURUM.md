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
| 3 Hikâye yazma ekranı + otomatik sahne | **TAMAM (EXE'de test edildi), kullanıcı onayı bekleniyor** (Aşama 4'e onaysız geçme) |
| 4–16 | Başlanmadı (4 = Gemini ses, 5 = video düzenleme/dışa aktarma) |

## Özet: Aşama 0-2 (tamam)
- Aşama 0: dört derleme birimi (Core/Runtime/Editor/App), KarakterKatalogu (68 karakter), menü "Hikaye/Windows Derlemesi (Deneme)".
- Aşama 1: `Uygulama/FfmpegPipeRecorder.cs` (HikayeCanvas → ScreenSpaceCamera → 1080x1920 RenderTexture → ffmpeg borusu, ses WAV→aac), `KayitBootstrap.cs` (`-kayit <mp4> -hikaye <ad>`), Editör ve EXE'de deneme_3d kaydedildi (30 fps, h264+AAC, klaket). Kodlayıcı sırası nvenc → amf → qsv → mf → libx264; bu PC'de amf.
- Aşama 2: `Uygulama/AnaEkran.cs` (IMGUI; 8 bölüm: Yeni hikâye, Hikâyelerim, Karakterlerim, Mekânlar, Ses, Video, Projeler, Ayarlar), "Hikâye Stüdyosu" kimliği, masaüstü kısayolu; Unity Editör kapalıyken test edildi. GUI.BeginScrollView ölçek≠1'de tıklamayı bozduğu için kendi kaydırma alanı (ListeBasla/ListeBitir).
- Plan dışı önceden yazılanlar (3D karakter/mekân/kamera/atmosfer, Stüdyo köprüsü, doğrulayıcı vb.) Editörde oynadı, ayrıca görsel doğrulanmadı.

## Aşama 3: Hikâye yazma ekranı ve otomatik sahne
Yeni/değişen dosyalar (`Assets/HikayeStudyo/Uygulama/`): `AnaEkran.Yazma.cs` (yazma ekranı + sahne panosu), `HikayeDenetci.cs` (Editör'süz doğrulayıcı), `CanliOnizleme.cs` (canlı önizleme), `AnaEkran.cs` (partial; Düzenle/Yeni hikâye yazma ekranını açar). Komut sistemi (StoryParser/KomutKayit) AYNEN korundu; kullanıcı komut yazabilir ya da menülerden seçebilir.
- Yazma ekranı: satır numaralı metin alanı + imleç satırı vurgusu; hızlı ekleme çubuğu (konuşma: karakter/duygu/dış-iç/hareket menüleri; komut: tüm komutlar + değer menüleri: mekân, mekan3d, kamera, efekt, atmosfer, müzik, poz, oyuncu, bekle, son…); hata/uyarı paneli (satıra tıkla → atlar, "Düzelt" önerisi uygular, hatalar önce); Geri al/Yinele (düğme + Ctrl+Z/Y); otomatik kayıt (1,5 sn boşta, `Documents\HikayeStudyosu\Hikayeler\<ad>.txt`, ilk açılışta `Yedek\`e kopya); örnek hikâyeler "Kopyala+düzenle" ile kopyası açılır; yan panelde 9:16 canlı önizleme (RenderTexture; Baştan / imleç satırından oynat, Durdur → sahne yeniden yüklenir); sahne ekranda doğru oranda, kırpılmıyor.
- Sahne panosu: `mekan:` satırlarında sahnelere böler (başlık satırları seri/bolum/baslik/kanca/oyuncu/geri ve sondaki son/secim/sonuc sabit kalır); kart: gerçek ekran görüntüsü, mekân, karakterler, konuşma sayısı, tahmini süre, ilk replik. İşlemler: küçük resmi yenile, eksikleri/tümünü üret, bu sahneyi oynat, yukarı/aşağı taşı, karakter değiştir (sahne içinde), kamera uygula. Küçük resim önbelleği `Documents\HikayeStudyosu\Onbellek\<hikâye>\<özet>.png`; özet = başlık + sahne metni (boş satır/yorum yok sayılır) → bir sahne değişince yalnız o sahne eskir.
- Derleme: Hikaye.App.dll yenilendi, 0 hata (DLL'de metinler doğrulandı); EXE derlemesi Succeeded (iki kez, 793 MB). Unity Editör KAPALI iken test.
- GERÇEK TEST (EXE, ekran görüntüleriyle): Hikâyelerim → ornek "Kopyala+düzenle" → ornek_kopya açıldı; "Satır ekle" satır ekledi, yazı yerine geçti, "Kaydedildi 23:50:05"; komut menüsü (kamera) → Ekle; hatalı "kamra: yakin" → hata paneli + "Düzelt" → "kamera: yakin"; Baştan oynat → önizleme 9:16 dikey ve doğru yönde (yağmur, "1 saat önce"); Durdur; Sahne panosu → 2 kart → "Eksikleri üret (2)" → 2 gerçek küçük resim; kamera=genis uygula → yalnız o sahne eskidi (diğeri kaldı) → "Eksikleri üret (1)" yalnız onu üretti; Aşağı/Yukarı taşıma metni değiştirdi, taşınan sahnelerin küçük resimleri kaldı (özet düzeltmesinden sonra yeniden doğrulandı); karakter değiştir Arda→Baba (7 satır), Ctrl+Z geri aldı; "Bu sahneyi oynat" sahneyi önizlemede oynattı; Yeni hikâye (ad yazıldı) → şablon + yazma ekranı açıldı; kapat-aç sonrası otomatik kayıt kalıcı (8 konuşma); pencere büyütme çalıştı. EXE günlüğünde hata yok.
- Düzeltilen hatalar: sahne taşınınca küçük resim eskiyordu (özet boş satır/yorum içeriyordu → normalleştirildi); hata paneli uyarıları arasında kayboluyordu (hatalar öne alındı, panel büyütüldü).
- Aşama 3 AÇIK / test EDİLMEDİ: yazarak geri alma (Ctrl+Z yalnız sahne işleminde denendi) ve Ctrl+Y/Ctrl+S; "Satır N'den oynat" düğmesi (aynı yol "Bu sahneyi oynat" ile denendi); önizlemede ses (Aşama 4'e kadar hikâye sesi yok); çok uzun hikâyede (200+ satır) satır numarası/performans; Türkçe karakter yazımı (Segoe UI; yalnız ASCII yazıldı); sahne içi karakter değiştirme `oyuncu:` satırlarına dokunmaz; sahne panosunda küçük resim üretimi sahne başına ~7 sn (her biri için sahne yeniden yüklenir); küçük resim 3,2 sn sonra alınır (ilk replikten önce olabilir); doğrulayıcı süre tahmini yaklaşık.
- Eski açık sorunlar: Mixamo karakterlerde ağız hareketi yok (Aşama 9); doğrulayıcı/`Docs/HikayeSozdizimi.md` 3D mekân komutlarını tam içermiyor (uygulama içi doğrulayıcı mekan3d/kamera değerlerini biliyor); görev çubuğu simgesi doğrulanmadı; Aşama 2 listesindeki test edilmemişler (Klasörü aç/Not Defteri, ses kaydırgacı etkisi, 60 FPS kayıt, ayarları sıfırla).

## DERSLER
- `device_commit_files` önceki halini yükleyebilir: yaz, `sync; sleep 6`, commit, `device_stage_files` ile boyut doğrula.
- Unity arka plandayken derlemez: Hub'dan "My project" (530,293), 45 sn bekle, Assets (89,40) → Refresh (120,486); derleme `Library/ScriptAssemblies/Hikaye.App.dll` içinde metinle doğrulanır. Çıkış: File (16,40) → Exit (44,354). Exe: Gezgin → Masaüstü (70,134) → kısayol (240,291) → Giriş (80,35) → Aç (722,62); iki Gezgin penceresi olabilir.
- C# 9: renk üçlü ifadelerinde Color32/Color karışımı hedef türüyle çalışır; GUI grup/kaydırma arasında `return` yapma.

## Sonraki adım
Kullanıcı Aşama 3'ü onaylarsa Aşama 4 (Gemini ses). Onaysız başlama.
