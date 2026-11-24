# Aşama 5 planı: Video düzenleme ve dışa aktarma (25 Kas 2026)

Kaynak: Docs/Hikaye_Studyo_Gelistirme_Plani.pdf, sayfa 4 (Aşama 5) ve sayfa 10–11 (Bölüm 4: Teknik temizlik ve kalite güvencesi).
Docs klasörü: `Hikaye_Studyo_Gelistirme_Plani.pdf`, `HikayeSozdizimi.md` (başka belge yok).

## PDF'teki Aşama 5 maddeleri (aynen)
- Kullanıcı video kaydetmeden önce düzenleme yapabilsin.
- Özellikler: sahne sıralama, sahne süresi, kamera düzenleme, altyazı düzenleme, müzik ekleme, ses efekti ekleme, geçiş efektleri, görsel efektler, kapak oluşturma, video önizleme.
- Çıktı: 1080x1920, 30 FPS, MP4, H.264, AAC ses.
- Yayın paketi: video, kapak, başlık, açıklama, etiketler. Bütün dosyalar düzenli bir yayın klasörüne kaydedilsin.

Aşama 5'e bağlanan B4 maddeleri:
- Komut adları: efekt = yalnız ses efekti, vfx = görsel efekt, gecis = yalnız sahne geçişi, kamera, filtre, atmosfer, isik ve diğerleri. Aynı komut iki anlamda kullanılmaz.
- Editör kaydı ile EXE kaydı ayrı olmalı ve ikisi de IVideoKaydedici kullanmalı. FFmpeg konumu tek yerden gelmeli, ses karıştırma tek yöntemle yapılmalı, kare hızı sabit ve ses senkronlu olmalı.
- Her video üretiminden sonra otomatik kalite kontrolü: siyah ekran, donmuş kare, eksik ses, ses seviyesi, altyazı taşması, kadraj, ağız senkronu, bozuk dosya. Sonuç kullanıcıya rapor olarak gösterilmeli.

## Mevcut durum (kod incelemesi, depo eed18dc = cihaz)

ZATEN VAR:
- **FfmpegPipeRecorder** (IVideoKaydedici): 1080x1920 RenderTexture → ffmpeg borusu. Kodlayıcı sırası nvenc → amf → qsv → mf → libx264. Ses AudioRenderer → WAV → AAC. Sabit kare hızı (captureDeltaTime), klaket. Kodlayıcı için tek ffmpeg konumu `FfmpegBul()`: önce StreamingAssets/ffmpeg, sonra PATH.
- **KayitBootstrap/KayitIsleyici**: yalnız komut satırından çalışıyor (`-kayit <mp4> -hikaye <ad> [-klaket]` ya da ffkayit.txt). Hikâyeyi yalnız `Resources/Hikayeler/` altından yüklüyor. Bitince `ffkayit_sonuc.txt` yazıyor ve `Application.Quit()` çağırıyor. Yayın klasörü ve kapak üretmiyor.
- **EXE arayüzü (AnaEkran)**: 9 bölüm var.
  - "Video ayarları": FPS 30/60, güvenli alan, kodlayıcı denetimi.
  - "Projeler": `Documents\HikayeStudyosu\Videolar` içindeki mp4 dosyalarını listeliyor.
  - **ARAYÜZDE "VİDEOYA AL" YOK.** Kullanıcı EXE'den video üretemiyor. Aşama 5'in çekirdek eksiği bu.
- **Yazma ekranı (AnaEkran.Yazma.cs)**:
  - Sekmeler: Yaz, Sahne panosu, Ses.
  - Sahne panosu: sahneler `mekan:` satırlarıyla bölünüyor. Her sahnede küçük resim, tahmini süre (HikayeDenetci.ToplamSure), sahne taşıma (SahneTasi), karakter değiştirme ve kamera uygulama (KameraUygula) var.
  - Canlı 9:16 önizleme (CanliOnizleme), "Baştan oynat" ve "Satır N'den oynat".
  - Hikâyenin kaynağı yalnızca metin; Geri al ve Yinele de metin üzerinde çalışıyor.
- **StoryPlayer**:
  - Hikâye komutları: muzik, efekt, kamera, atmosfer, flas, far, geri (VHS), gorsel, altyazi (stil), bekle, kanca, yazi, secim, son, poz, oyuncu, seri, baslik, bolum.
  - Yönetmen komutları (DirectKeys): mekan3d, yer, gir, figuran, ani, zaman, gecis, sessizlik, isik.
  - Kapak ve yayın klasörü: `KapakKaresi()` duygusal satırdan kare alıyor. `Yayin(dir)` kapak.png (seri, büyük başlık, BÖLÜM N rozeti) ve aciklama.txt (başlık satırı, seçim soruları, hashtag'ler, notlar) yazıyor. PlayerPrefs `HS_Kapak=1` ve `HS_YayinKlasor=<dir>` ile tetikleniyor; bunları şu an yalnızca Editör StudyoKayit kullanıyor.
- **Editör kaydı (StudyoKayit)**: Unity Recorder'ı yansıma ile kullanıyor. IVideoKaydedici'yi KULLANMIYOR (B4 maddesi açık). Yalnızca Editör'de çalışıyor, EXE'yi etkilemiyor.
- **Ses**:
  - DisSes kalıcı klasörü `Documents\HikayeStudyosu\Ses\<hikaye>\<anahtar>.wav`.
  - Deneme sesleri `_deneme` altında. PreloadVoices sırası: `Resources/Ses/<hikaye>/<anahtar>` → kalıcı klasör → (HS_DenemeSes=1 ise) deneme.
  - Cihazda gerçek sesi olan tek hikâye `deneme_3d` (Assets/Resources/Ses/deneme_3d).

EKSİK:
1. EXE'den video üretme. Ön kontrol yok: eksik ses ya da deneme sesi varken kayıt reddedilmeli (DURUM.md notu). Ayrıca kullanıcı hikâyeleri (Documents) kaydedilemiyor.
2. EXE'de yayın paketi yok. Mevcut Yayin() başlık, açıklama ve etiketleri tek aciklama.txt'ye koyuyor, plan bunları ayrı istiyor. Kapak düzenlenemiyor: kare otomatik seçiliyor, başlık yalnızca metindeki `baslik:` komutundan geliyor.
3. Sahne düzenleme paneli yok. Süre, müzik, ses efekti, geçiş, görsel efekt ve altyazı stili arayüzden eklenemiyor; yalnızca elle komut yazılarak yapılabiliyor.
4. `gecis:` yalnızca 3D sahnelerde çalışıyor (`s3d != null`); 2D sahnelerde hiçbir şey yapmıyor. B4'teki `vfx:` komutu yok; görsel efektler ayrı komutlarda dağınık (flas, far, geri, gorsel).
5. Kayıt sonrası otomatik kalite kontrolü ve kullanıcıya rapor yok. Yalnızca `ffmpeg -i` bilgisi ffkayit_sonuc.txt'ye yazılıyor.
6. Editör kaydının IVideoKaydedici kullanmaması (B4). Düşük öncelik, EXE ürününü etkilemiyor.

## İş paketleri (dosya sahipliği ayrık; ortak dosyalar yalnızca sahibince değişir)

Sıra: 1. dalgada P1 ve P4 paralel yapılır. 2. dalgada P2, P3 ve P5 paralel yapılır (P2 ve P5 P1'e, P3 P4'e bağlı). En sonda tümleşik EXE testi.

### P1: Arayüzden "Videoya al" (çekirdek) ve ses ön kontrolü
- **Amaç:** Kullanıcı EXE'de tek tuşla 1080x1920, 30 fps, H.264, AAC MP4 üretsin. Eksik ya da deneme sesi varsa kayıt başlamasın.
- **PDF karşılığı:** çıktı biçimi (1080x1920/30 FPS/MP4/H.264/AAC), "video önizleme" (kayıttan önce Oyna, sonra Videoyu aç), DURUM.md notu.
- **Dosyalar:**
  - YENİ `Uygulama/AnaEkran.Video.cs` (partial AnaEkran): VideoyaAl(h) ön kontrolü, süreç başlatma, ilerleme paneli, sonuç ekranı.
  - `Uygulama/KayitBootstrap.cs` (sahibi P1):
    - `-hikayedosya <yol>` argümanı: TextAsset metinden kurulur ve `ta.name = hikâye adı` olur (kalıcı ses klasörü eşleşsin).
    - `-yayin <klasör>` argümanı.
    - İlerleme dosyası `kayit_ilerleme.txt` (kare/süre/durum).
  - `Scripts/DisSes.cs` (sahibi P1): `public static List<int> EksikGercekSesler(...)` ya da eşdeğeri. Kural: Resources veya kalıcı klasörde gerçek dosya yoksa satır eksik sayılır. HS_DenemeSes açık olsa bile deneme dosyası sayılmaz.
  - `AnaEkran.Yazma.cs`: üst çubuğa yalnızca bir "Videoya al" düğmesi çağrısı (tek satır).
  - `AnaEkran.cs`: HikayelerimCiz satırına "Videoya al" düğmesi (tek satır).
  - Bu iki dosyada başka değişiklik yapılmaz.
- **Tasarım önerisi:** Ayrı süreç. `Process.Start(<exe>, "-kayit \"...\\video.mp4\" -hikayedosya \"...\" -yayin \"...\"")`.
  - Aşama 1'de test edilmiş kayıt yolu korunur. Kayıt bitince süreç kapanır, ana arayüz donmaz.
  - Exe yolu: `Path.Combine(Path.GetDirectoryName(Application.dataPath), "HikayeStudyo.exe")`, ya da `Process.GetCurrentProcess().MainModule.FileName`.
  - Yayın klasörü: `Videolar\<hikaye>_<yyyyMMdd_HHmm>\` → `video.mp4`.
  - Kayıt sürerken Videoya al düğmesi pasif olur; ilerleme dosyası her 0,5 sn okunur.
- **Kabul:**
  1. Sesi tam olmayan hikâyede "Videoya al" → "N satırın sesi yok / deneme sesi: satır 12, 18…" mesajı çıkar, kayıt başlamaz.
  2. deneme_3d ile (Resources sesli) EXE'den kayıt → klasörde video.mp4 oluşur.
  3. `ffmpeg -i video.mp4` (StreamingAssets'teki ffmpeg; ffprobe varsa `ffprobe -v error -show_streams`) çıktısında Video: h264, 1080x1920, 30 fps; Audio: aac görünür.
  4. Süre ≈ hikâye süresi.
  5. Arayüz kayıt sırasında donmaz.
- **Riskler:**
  - Ayrı süreç ikinci bir pencere açar. KayitIsleyici `runInBackground` kullanıyor; pencereyi küçültmek ya da gizlemek kayıt üretimini bozabilir (Aşama 1 notu). Önce görünür pencereyle test et.
  - Kopyalanan hikâyenin adı değişince (örn. deneme_3d → ornek_kopya) Resources/Ses eşleşmez, sesler "eksik" görünür. Bu doğru davranış, ama test için deneme_3d'nin kendisi kullanılmalı.
  - Hikâye adı yol içerdiği için temizlenmeli (DisSes.Temiz / dosya adı güvenliği).

### P2: Yayın paketi ve kapak oluşturma
- **Amaç:** Yayın klasöründe video.mp4, kapak.png, baslik.txt, aciklama.txt ve etiketler.txt olsun. Kullanıcı kapak karesini ve başlığı seçebilsin.
- **PDF karşılığı:** kapak oluşturma, yayın paketi (video, kapak, başlık, açıklama, etiketler, düzenli klasör).
- **Dosyalar:**
  - YENİ `Uygulama/YayinPaketi.cs`:
    - Kayıttan sonra metinleri ayrı dosyalara yazar: başlık; açıklama gövdesi; hashtag satırı, varsayılanlar + `etiket:` komutu.
    - `yayin.json` özeti üretir.
  - `KayitBootstrap.cs`: kayıtta `HS_Kapak=1` ve `HS_YayinKlasor` ayarlanır. Bu P1 dosyası; P2 kendi değişikliğini P1 bittikten sonra ya da P1 sahibiyle birlikte yapar.
  - `AnaEkran.cs` ProjelerCiz: yayın klasörlerini listeler (kapak küçük resmi, Videoyu aç, Klasörü aç, "Açıklamayı kopyala" → GUIUtility.systemCopyBuffer). ProjelerCiz'in sahibi P2'dir.
  - Kapak seçimi:
    - `kapak: <satırNo>` komutu yazılırsa KapakKaresi o satırda alınır. Komut StoryModel'de, P4 sahipliğinde; P2 ihtiyacı P4'e iletilir.
    - Yazma ekranında "Bu satırı kapak yap" düğmesi P3 panelinde yer alır.
- **ÖNEMLİ HATA RİSKİ (kapak videoya girebilir):**
  - StoryPlayer.Bitir() önce `Finished` olayını tetikliyor, ardından hemen `Yayin()` başlıyor.
  - KayitIsleyici ise Finished'tan sonra 1 sn bekleyip `rec.Durdur()` çağırıyor.
  - Bu 1 sn içinde kapak ekranı videonun son karelerine kaydedilir.
  - Çözüm: Kayıt modunda Yayin, kayıt durduktan sonra çalışmalı. Örneğin KayitIsleyici `HS_YayinKlasor`'u StoryPlayer'a vermez; rec.Durdur ve Bitti'den sonra kendisi `sp.YayinUret(dir)` (public coroutine) çağırır. Bu StoryPlayer değişikliği P4 sahipliğinde, küçük ve public bir sarmalayıcıdır.
  - Kabul: videonun son 1 sn'sinde kapak karesi olmadığı ffmpeg ile son kareden PNG çıkarılarak doğrulanır.
- **Kabul:**
  1. EXE'den kayıt sonrası klasörde 5 dosya olur.
  2. kapak.png 1080x1920, başlık ve seri yazısı görünür, siyah değil.
  3. Türkçe karakterler UTF-8 olarak doğru.
  4. Projeler'de listelenir.
  5. Kopyala çalışır.
- **Riskler:** ScreenCapture boyutu pencere boyutuna bağlı. Kapak 1080x1920 olmayabilir; RenderTexture'dan almak gerekebilir (FfmpegPipeRecorder'ın RT'si). EXE'de yalnızca bir kez alınır.

### P3: Sahne düzenleme paneli (sahne panosu genişletmesi)
- **Amaç:** Kayıttan önce, seçili sahne için arayüzden düzenleme yapılabilsin: süre, kamera, müzik, ses efekti, geçiş, görsel efekt, altyazı. Sahne sıralaması da test edilsin (var olan SahneTasi).
- **PDF karşılığı:** sahne sıralama, sahne süresi, kamera düzenleme, altyazı düzenleme, müzik ekleme, ses efekti ekleme, geçiş efektleri, görsel efektler.
- **Tasarım:** Tek doğruluk kaynağı metin. Panel, sahnenin metnine komut satırı ekler ya da var olanı değiştirir, mevcut Degistir()/Geri al akışını kullanır. Böylece doğrulayıcı, önizleme ve kayıt aynı yolu izler.
  - **Süre:** Sahnenin tahmini süresi gösterilir. "+0,5 sn / −0,5 sn" sahne sonundaki `bekle:` değerini ayarlar. Konuşma hızı değişmez.
  - **Kamera:** Mevcut KameraUygula; 2D ve 3D kamera listeleri (Yonetmen3D.Kameralar).
  - **Müzik:** Sahne başındaki `muzik:` satırı. Açılır liste: pad, yok ve StoryLibrary müzikleri.
  - **Ses efekti:** Seçili satırdan sonra `efekt: <ad>`. Liste ProcAudio adlarından ve kütüphane efektlerinden gelir.
  - **Geçiş:** Sahne başında `gecis: karar|capraz|beyaz`.
  - **Görsel efekt:** `vfx: flas|far|vhs|…` (P4'ün tanımladığı liste) ve `atmosfer:`.
  - **Altyazı:** Seçili konuşma satırının metni tek satırlık alanda düzenlenir; `altyazi:` stili (KaraokeAltyazi stilleri) eklenir.
  - **Kapak:** "Bu satırı kapak yap" → `kapak: <satır>`.
- **Dosyalar:**
  - YENİ `Uygulama/AnaEkran.Duzen.cs` (partial): panel çizimi, satır ekleme yardımcıları.
  - `AnaEkran.Yazma.cs`: yalnızca PanoCiz içine panel çağrısı ve seçili satır bilgisi. Sahibi P3; P1'in tek satırlık düğmesinden sonra birleştirilir.
  - Listeler için P4'ün public sabitleri okunur (StoryModel/Yonetmen3D).
- **Kabul:**
  1. EXE'de bir sahnede müzik, geçiş, efekt ve vfx seçilir; metne doğru satırlar eklenir.
  2. Geri al bunları kaldırır.
  3. Doğrulayıcı 0 hata verir.
  4. Sahnenin canlı önizlemesi ve "Satır N'den oynat" efekti gösterir.
  5. Sahne süresi göstergesi `bekle:` ile değişir.
  6. Sahne taşıma sonrası sıra doğru olur.
  7. Dar pencerede (≈680 px) panel üst üste binmez (ListeBasla/ListeBitir; GUI grup arasında return yok).
- **Riskler:**
  - AnaEkran.Yazma.cs 1008 satır, birleştirme çakışması olabilir; yeni kodu ayrı partial dosyada tut.
  - Satır numaraları değişince seçili satır kayar.
  - Metin düzenleme alanı ile panel arasında odak ve Türkçe karakter (IME) sorunları çıkabilir.

### P4: Oynatıcı komutları (gecis 2D, vfx, kapak, etiket) ve doğrulama
- **Amaç:** Panelin eklediği komutlar EXE'de gerçekten çalışsın; B4 adlandırmasına uyulsun.
- **PDF karşılığı:** geçiş efektleri, görsel efektler; B4 komut sistemi (efekt = ses, vfx = görsel, gecis = yalnız geçiş).
- **Dosyalar (sahibi P4):**
  - `Scripts/StoryModel.cs`: `vfx:` (ItemType.Vfx ya da mevcut Flash/Headlights/Rewind'e eşleme), `kapak:` (r.KapakSatir), `etiket:` (r.Etiketler) ayrıştırma. `gecis:` mevcut DirectKeys'te kalır.
  - `Scripts/StoryPlayer.cs`:
    - 2D sahnelerde `gecis` için fade katmanıyla karar (siyah), capraz (çapraz geçiş) ve beyaz.
    - `vfx` case'leri.
    - Kapak satırı seçimi (kapakSatir = KapakSatir).
    - P2 için public `IEnumerator YayinUret(string dir)` sarmalayıcısı ve kayıt modunda Bitir() içinden Yayin çağrılmaması.
  - `Scripts/KomutKayit.cs`: `vfx`, `kapak`, `etiket` kayıtları.
  - `Uygulama/HikayeDenetci.cs` ve `Editor/StudyoDogrulama.cs`: yeni komutların doğrulaması ve "Bunu mu demek istediniz" önerileri.
  - `Docs/HikayeSozdizimi.md`: yeni komutlar.
  - Public sabit listeler: `StoryModel.VfxListesi`, `Yonetmen3D.Gecisler` (zaten public).
- **Kabul:**
  1. Unity derlemesi 0 hata.
  2. Editör'de ve EXE'de `gecis: beyaz` ile 2D mekân değişimi görülür; `onizleme_goruntu` ile ekran görüntüsü alınır.
  3. `vfx: flas` beyaz parlama verir.
  4. Eski komutlar (flas:, far:, geri:) aynen çalışır; geriye uyumluluk korunur.
  5. Doğrulayıcı yanlış değerde öneri gösterir.
- **Riskler:**
  - StoryPlayer 1312 satır ve "çalışanı bozma" kuralı geçerli; değişiklikler yalnızca yeni case'ler ve küçük sarmalayıcılar olmalı.
  - Atlama (Hizli/Satır N'den oynat) yolunda yeni komutların kalıcı durumu da kurulmalı.

### P5: Otomatik kalite kontrolü ve rapor (B4)
- **Amaç:** Her kayıttan sonra video otomatik denetlensin, `kalite_raporu.txt` yayın klasörüne yazılsın ve arayüzde özet gösterilsin.
- **PDF karşılığı:** B4 "Otomatik kalite kontrolü": siyah ekran, donmuş kare, eksik ses, ses seviyesi, bozuk dosya. Altyazı taşması, kadraj ve ağız senkronu bu aşamada "kontrol edilmedi" olarak raporlanır, uydurma sonuç yazılmaz. Ayrıca çıktı biçim doğrulaması (1080x1920/30/h264/aac).
- **Dosyalar:**
  - YENİ `Uygulama/KaliteKontrol.cs`. Statik ve iş parçacığında çalışır; FfmpegBul() kullanır. Denetimler:
    - Biçim: `ffmpeg -hide_banner -i` çıktısını ayrıştır (ffprobe StreamingAssets'te varsa onu kullan).
    - Siyah: `-vf blackdetect=d=0.5:pix_th=0.10 -an -f null -`.
    - Donmuş: `-vf freezedetect=n=-60dB:d=2 -an -f null -`.
    - Ses: `-af ebur128=peak=true -vn -f null -`; integrated LUFS, true peak, sessizlik `silencedetect=n=-50dB:d=3`.
    - Bozuk dosya: `-v error -i x -f null -`.
  - Çağrı: KayitIsleyici, kayıt bittikten sonra çağırır (P1 dosyası, tek satır; P1 sahibi ekler). Ya da AnaEkran.Video sonuç ekranı çağırır (P1).
- **Kabul:**
  1. deneme_3d kaydında rapor biçimi "1080x1920 30 fps h264 aac: GEÇTİ" diye yazar.
  2. Siyah ve donmuş aralıklar listelenir.
  3. LUFS değeri miks hedefi (−14) ±2 aralığında olur ya da uyarı verir.
  4. Sessiz bir test videosunda "eksik ses" bulunur. Test videosu ffmpeg lavfi ile üretilip KaliteKontrol'e verilerek denenir (bu doğrulama container'da da yapılabilir).
  5. Rapor arayüzde görünür.
- **Riskler:**
  - Hikâyede bilerek kullanılan siyah ekran (kanca: siyah ekranda kanca yazısı) ve `bekle:` blackdetect ile freezedetect'te yanlış alarm verir. Eşikler ve "kanca süresi hariç" kuralı gerekir; raporda bunlar "uyarı" olarak geçmeli, "hata" değil.
  - ffmpeg süresi uzun videolarda dakika sürebilir; iş parçacığında çalıştırılmalı.

## Ortak kurallar
- Unity 6 (C# 9), paket ekleme yok, namespace HikayeStudyo.
- IMGUI ve kendi ListeBasla/ListeBitir kaydırması. Türkçe arayüz metinleri.
- Değişiklikten önce yedek. Her değişiklikten sonra derleme 0 hata.
- Unity arka plandayken derlemez: Hub → My project → Assets/Refresh. Derleme DLL içinde metin aranarak doğrulanır.
- EXE: menü `Hikaye/Windows Derlemesi (Deneme)`; EXE açıkken derleme yapılmaz.
- Test edilmeyen hiçbir şey "tamam" yazılmaz.
- Commit dalı `claude/hikaye-studyosu`.

## En önemli riskler (özet)
1. **Kapak videoya girebilir:** Yayin(), Finished ile rec.Durdur arasındaki 1 sn içinde çalışıyor. P2 ve P4 birlikte çözmeli.
2. **Test sesi kısıtı:** Gerçek sesi olan tek hikâye deneme_3d. Gemini kotası (~10/gün) yeni bir hikâyeyi tam seslendirmeye yetmeyebilir. Uçtan uca kayıt testi deneme_3d ile yapılmalı. Kopya hikâyenin sesi "eksik" görünür; bu doğru davranış.
3. **Ayrı kayıt süreci:** ikinci pencere ve odak sorunu, kullanıcı hikâyesini (Documents) yükleme ve ses klasörü adı eşleşmesi.
4. **Büyük dosyalarda birleştirme çakışması:** AnaEkran.Yazma.cs (1008 satır) ve StoryPlayer.cs (1312 satır). Yeni kod partial ya da yeni dosyalarda tutulmalı; ortak dosyalara yalnızca sahibi dokunmalı.
5. **Kalite kontrolünde yanlış alarm:** Kanca ve bekle sahnelerindeki bilinçli siyah veya donuk kareler. Ağız senkronu, kadraj ve altyazı taşması bu aşamada otomatik ölçülemez; raporda "kontrol edilmedi" olarak yazılmalı.
6. **Editör kaydı hâlâ IVideoKaydedici kullanmıyor (B4).** EXE ürününü etkilemiyor; ayrı küçük iş olarak sona bırakılabilir.
