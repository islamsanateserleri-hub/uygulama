# HİKÂYE STÜDYOSU: DURUM

Tek güncel kayıt dosyası. Her işlem, test sonucu ve hata buraya eklenir; kısa tut, eskiyi `_Arsiv/`ye taşı.
26 Kas 03:30 öncesinin TAM günlüğü: `_Arsiv/DURUM_26Kas_0330_oncesi.md` (yalnız gerekirse bak).

## >>> YENİ SOHBET BURADAN BAŞLA (26 Kas 2026, 03:30) <<<
**Kullanıcı:** İngilizce bilmiyor; basit Türkçe yaz, düşünmeni/akıl yürütmeni de Türkçe yaz. Saat dilimi Europe/Istanbul. 03:27'de: "gereken bütün onayı veriyorum", uyuyor; soru sorma, kendi başına çalış, sabaha kısa yazılı özet bırak. 5 saatlik ve haftalık kullanım sınırı dar: tutumlu ol, sınır bitmeden bu dosyayı güncelle.
**Son söz (03:25):** "10-15 efekt az olur, 100 tane efekt ekle; söz, yeni video çıkana kadar bir şey eklemeyeceğim. Kaldığın yerden devam et, indirdiklerimizi yükle, sonra test aşamasına geç."
**SON ÇALIŞAN OTURUM:** 26 Kas 16:20 (07:00 görevinin sohbeti). Unity SERBEST. Film sesleri için kullanıcının indirme izni vermesi bekleniyor (aşağıda "FİLM ÇALIŞMASI").
**07:00 görevi:** 26 Kas 07:00'de YENİ bir sohbet açıp işe devam eden zamanlanmış görev kuruldu (klasör bağlı, otomatik onaylı; bilgisayar, Unity ve Claude uygulaması açık olmalı). Eski sohbete giden 07:00 hatırlatıcısı da AÇIK kaldı (kapatma izni verilmedi): 07:00'de ESKİ sohbet uyanırsa hiçbir işe dokunmaz, işi yalnız yeni görevin sohbeti yürütür.
**GitHub:** gece yapılan kod ve bu dosya gönderildi (42fac34 ve sonrası).
**Derleme:** 03:22:41 "0 hata". Unity açık ve köprüye yanıt veriyor.

## SIRADAKİ İŞLER (bu sırayla)
### 1) 100+ yeni efekt — YAPILDI (aşağıdaki tasarım notu eskidir)
Hepsi kodla üretilir; gerçek ses kaydı değildir. Adların tam listesi: Docs/HikayeSozdizimi.md.
### 2) İndirilenlerin yüklü olduğunu doğrula — YAPILDI
Aşağıdaki envanterle `HikayeKomut/nesne_rapor.txt`, `hareket_rapor.txt`, `karakterler.json` karşılaştır. Astronot (donus 200) ve araba (donus 110) yönü son düzeltmeden sonra GÖZLE DOĞRULANMADI. Yeni hareket takma adları ve uzay arka planları için `Assets/Resources/Hikayeler/test_hersey.txt` var.
### 3) Test aşaması — YAPILDI (Editör)
Baştan sona ~1 dk'lık yeni hikâye (yeni karakter, nesne, mekân, efekt, animasyon). Sessiz kareler; sonra `seslendir:` (Gemini anahtarı kullanıcının girdiği haliyle kayıtlıysa; günlük ~10 istek, boşa harcama; ANAHTAR YAZMA) ve `kaydet:`. Anahtar yoksa/kota dolduysa kullanıcıya yaz.
### 4) Windows derlemesi + kurulum dosyası — YAPILDI (kurulum KURULUP DENENMEDİ)
Kurulu uygulama (25 Kas) yeni içeriği İÇERMİYOR. `UygulamaDerle` Resources/OyuncuListesi.asset'e tüm Hazir prefablarını yazar; NesneKatalogu nesneleri çeker. EXE birkaç GB olacak; Inno Setup >2,1 GB için DiskSpanning isteyebilir. Yeni kurulum dosyasını çalıştırmak kullanıcıya kalır.
### 5) Sonra (yeni video çıkmadan yeni özellik isteme sözü var)
Tek tuş "düz hikâye → sahne" (yapay zekâ ile) ve arayüzü sadeleştirme (kullanıcı: "çok karmaşık, bir insan yapamaz"), 4K, Unity açılış ekranını kapatma, karakter çeşitleri, HATALI listesi, "Sıfırıncı Yankı" kalanı.

## 26 KASIM FİLM ÇALIŞMASI (15:20 sonrası)
- Eklendi (derleme 0 hata, karelerde görüldü): `mekan3d: zindan, sato, magara, mezarlik, orman` (`Scripts/Mekan3D.Masal.cs`; ayrıca kopru, koridor, hava_kilidi belgelendi), `oyuncu: X = Vampir|Vampir_Kadin`, `nesne: ejderha[_kirmizi|_siyah|_mavi]` (`Scripts/Yaratik.cs`, kodla üretilir). `Kare Serisi` ayarı `HikayeKomut/kare_ayar.txt` ("aralık_sn en_çok_kare"). "Mekânlar" sayfasında kaydırma yüksekliği artık ölçülüyor (EDİTÖRDE/EXE'DE TIKLANARAK DENENMEDİ).
- **Film "Üçüncü Kişi"** (`Resources/Hikayeler/ucuncu_kisi.txt`, 68 konuşma, tahmini ~4:50): sessiz önizlemede baştan sona kontrol edildi; ayrı bir alt ajan üç kez denetledi, bulguları uygulandı. Kadro `StoryLibrary` EkKadro: Deniz, Kerem, Kerem2 (kopya), KeremSes, Kayit.
- **SESLER YARIM, VİDEO KAYDEDİLMEDİ.** ElevenLabs eleven_v4; yalnız Mira (Nergis `hALSUJsnqdqp1bY0quM2`, 16 parça, 2 deneme) üretildi: akış `lx5j8tIUlZITS2JeccrC`, üretim `EQHgJU3ouaOtPpqiTGvP` (45,4 sn) ve `gfWEbVNkijP0gnOFdNBU` (45,5 sn); yeni indirme bağlantısı `creative_get_flow_run_status` ile kredisiz alınır (oturum `D3Le5Mtk4eqYCTZzb8Jw`, `44HD8785NDvKtEbeUtLB`). Kredi 1570/10000. **İndirme izni penceresi cevapsız kaldı**; kullanıcı onaylayınca yalnız web-fetch ile indir (curl vb. yok).
- Sıradaki: Mira'yı sessizlikten 16'ya bölmeyi sına (süre kısa, duraklar belirsiz olabilir); bölünebiliyorsa Kerem (Ayaz `i4Vkimmf9IrZ2RdG16Rv`, 25 parça) ve Deniz (Esin `vevaMEPHZ5tHlFtS3YPm`, 27 parça), ses başına tek istek (~5340 kredi). Dosyalar `Resources/Ses/ucuncu_kisi/NN.wav` (NN = konuşma sırası, 00'dan); `ses_listele: ucuncu_kisi`, sonra `kaydet: ucuncu_kisi`.
- **İndirme benim tarafımdan YAPILAMIYOR** (WebFetch: robots_disallowed; başka yolla indirme yok). Kullanıcı dosyaları kendisi indirip `HikayeKomut/gelen_sesler/` içine `mira.mp3, kerem.mp3, deniz.mp3` olarak koyacak.
- **Lisans (elevenlabs.io yardım ve fiyat sayfası, 26 Kas):** ücretsiz planın sesi yalnız ticari olmayan paylaşım, başlıkta "elevenlabs.io" şartı; sonradan plan almak eski sesi ticari yapmaz. Starter: ilk ay 1$, sonra 6$/ay, 30 bin kredi, ticari lisans (kampanya 1 Aralık'ta bitiyor). KARAR KULLANICIDA; karar gelmeden yeni ses üretme. Plan alınırsa Mira dahil üçü yeniden üretilir.
- **KARAR (kullanıcı, 26 Kas):** ücretsiz kalsın; para kazanmayacak, ticari değil. Video başlığına "elevenlabs.io" yazılacak. Sıra: kullanıcı `mira.mp3` koyar → bölme sınanır → Kerem ve Deniz üretilir → kullanıcı iki dosyayı koyar.
- Windows sürümü 4.2.0'a çekildi; EXE sesler eklendikten SONRA derlenecek.
- HATALI (küçük): vampirin soluk yüzü/kırmızı gözü uzaktan az seçiliyor; mezarlık `zaman: gece` verilmezse gündüz gökyüzlü; `vfx: vhs`/`geri:` ekranı siyah bırakıp karakterleri siliyor (filmde kullanma).
- Deneme hikâyesi `test_masal` listede görünür (köprü silemiyor).

## 26 KASIM ÖĞLEDEN SONRA (13:58–15:15)
- Not: 07:00 görevi klasör bağlı olmadan açıldı (Claude penceresi yoktu); kullanıcı 13:55'te yazıp izin verince devam edildi.
- **1. iş TAMAM (Editör):** 98 yeni ses (`Scripts/ProcAudio.Ses.cs`) + 63 yeni görsel efekt (`Scripts/EfektKatmani.cs`, `vfx: ad [saniye|surekli]`, `vfx: yok`). Derleme 0 hata. `menu: Hikaye/Ses Testi` → 112 ses, 0 sorunlu (`HikayeKomut/ses_ornekleri.wav`); KULAKLA DİNLENMEDİ. 63 görselin hepsi karelerde görüldü (test_efekt100a–d). Yedek: `HikayeKomut/yedek/efekt_oncesi/`.
- **2. iş TAMAM:** raporlar envanterle uyuyor (651 nesne, 51 uzay mekânı, NASA 256/256, 326 hareket, 115 karakter). Astronot ve araba karelerde göründü (astronot kolları iki yana açık duruyor; model pozu).
- **3. iş TAMAM (Editör):** `test_buyuk` sessiz kareler + `seslendir` (7/7 ses; kota ~7/10) + `kaydet` → `Videos\Hikaye\test_buyuk_20261126_143015\test_buyuk.mp4` (34 MB, sona kadar oynadı, -10,1 LUFS). Video İZLENMEDİ/DİNLENMEDİ (klasöre erişim yok).
- **HATALI (yeni):** Masal_Sovalye 3D'de kadraja çok yakın / başı alttan kesik; araba yol_gece'de kadraj dışında kalıyor; sandık karakterin arkasında gizleniyor; mekân değişiminde eski mekânın yağmuru ~1 sn görünüyor; `mekan3d: salon` yok (hazır: oda, ev, ...).
- Test hikâyeleri `Resources/Hikayeler/test_efekt100a–d, test_buyuk` uygulamada listede görünür (köprü silemiyor).
- **4. iş (14:37–15:06):** Windows derlemesi 4.1.0 `Succeeded, hata=0, uyari=9, 10817 MB`. `Exe Kayıt Testi` çıkış 0 → `Kayitlar/exe_test_buyuk.mp4` (1080x1920, 26,6 sn, sesli akış var); karelerde yeni karakter, nesne, efekt ve uzay arka planı göründü (kareler ilk EXE denemesinden; video baştan sona İZLENMEDİ/DİNLENMEDİ). Kurulum dosyası: `Build/Kurulum/HikayeStudyosu_Kurulum_4.1.0.exe` + `-1.bin` + `-2.bin` (toplam ~3,1 GB; ÜÇÜ AYNI KLASÖRDE durmalı). Kurulup DENENMEDİ; çalıştırmak kullanıcıya kalır (kurulu hali ~11 GB yer ister). `Installer/HikayeStudyosu.iss`: sürüm 4.1.0, lzma2/fast, DiskSpanning.
- **EXE'de 3 konuşma sessiz — nedeni bulundu:** uygulamada Arda için `Fenrir` sesi seçili (satır 15, 24, 29); deneme sesleri Editörde `Puck` ile üretilmişti. Kod hatası görünmüyor; uygulamada "Eksik sesleri üret" ile tamamlanır (DENENMEDİ). Uyarı artık eksik satırı, sesi ve anahtarı yazıyor (`StoryPlayer.PreloadVoices`).
- **HATALI (yeni):** EXE'de alan derinliği (bulanık arka plan) ve Panini gölgelendiricileri derlemeye girmiyor ("stripped from the build"); bu efektler uygulamada çalışmaz. Muhtemelen eskiden de vardı (doğrulanmadı).

## 26 KASIM GECE YAPILANLAR (02:42 sonrası; derleme 0 hata, sessiz Editör testi; sesli video ve EXE DENENMEDİ)
- Rocketbox'ın 326 doğal hareketi bağlandı (`Kutuphane/RbAnim/f|m`, menü "Hikaye/Gercek Hareketleri Ekle"); 115 Gercek_* yeniden kuruldu; bacak açık duruş sorunu bununla giderildi. Türkçe takma adlar (alkis, sevin, kahkaha, telefon, esne, kapi_cal ...).
- `nesne: <ad> [sol|sag|orta|on|arka|uzak|gok]`, `nesne: sil <ad>`, `nesne: yok`, `animasyon:` komutları. Yeni: Scripts/NesneKatalogu.cs, Sahne3D.Nesne.cs, Editor/NesneKur.cs (menü "Hikaye/Her Seyi Ekle").
- Envanter: 115 gerçekçi insan, 326 hareket, 651 nesne (25 hayvan, 40 silah, 203 zindan, 41 şehir, 57 uzay üssü, 256 NASA, 29 gezegen/güneş), 51 uzay arka planı, 9 fantastik karakter (Masal_*).
- 220 NASA modeli Draco sıkıştırmalıydı → kullanıcı onayıyla ("Evet kur") `com.unity.cloud.draco` 5.4.3 eklendi (Packages/manifest.json; Packages repoda YOK; yedek HikayeKomut/yedek/manifest_draco_oncesi.json). 256/256 aktarıldı.
- "1 cümle bile yazamadan bitti" şikâyeti: kullanıcı 40 harflik ad kutusuna yazmış → "Yeni hikâye" ekranına sınırsız çok satırlı hikâye kutusu (düz metni otomatik çevirir). Uygulamada tıklanarak DENENMEDİ.
- MetaHuman sorusu cevaplandı: Unity'de kullanılabilir (Haz 2025'ten beri, 1M$ altı ücretsiz) ama Unreal 5.6+ ve kullanıcının Epic hesabı gerekir; şimdilik önerilmedi.
- Yeni HATALI: bazı NASA modellerinde mor (eksik doku) parçalar; nesne karakterin arkasında gizlenebiliyor; saçta gri/siyah lekeler; ağız açıkken diş görünmüyor; Gercek_* için renk çeşidi yok; EXE çok büyüyecek.
- Kullanıcıya söylenmiş sınırlar: Marvel ve mevcut anime dizisi karakterleri eklenmez (özgün/mitolojik olur); ücretsiz Gemini sesi günde ~10 istek.

## KULLANICININ HEDEFİ
Düz yazılan hikâyeden sahne, mekân, karakter, eşya OTOMATİK kurulsun; yalnız beğenmediği parçayı elle ya da yapay zekâya söyleyerek değiştirsin. Çok çeşitli içerik ve karakter (çocuk dahil), keskin görüntü (4K), tamamen bağımsız uygulama. Ücretsizleri sormadan indir; ücretlide link ver.

## SINIRLAR (aşılmaz)
- API anahtarı/şifre yazma, hesap açma, sözleşme kabul etme, OAuth/Google yetkisi, ödeme: kullanıcıya bırak. Sır/anahtar commit etme.
- Erişim reddedilen klasörler: `Documents\HikayeStudyosu`, `Downloads`, `Videos\Hikaye`. Dolanma.
- Dosya Gezgini yalnız tıklama katmanında; betikle/araçla tuş-tık gönderme.
- Proje kuralı "paket eklenmez" (istisnalar kullanıcı onaylı UniVRM ve Draco).

## ÇALIŞMA ORTAMI
- Proje `C:\Users\Teknodc\My project` (Unity 6000.6.9f1, URP), bilgisayar DESKTOP-49TN47M. device_bash yok.
- Okuma: `device_stage_files` (→ /mnt/user-data/uploads/My project/..., 6-8 sn bekle). Yazma: /mnt/user-data/outputs/My project/... → `sync; sleep 8` → `device_commit_files` (force:true). ~25 MB üstü yazılamaz; cihazda dosya silinemez.
- Unity köprüsü: `HikayeKomut/komut.txt` (satırlar sırayla çalışır). Derleme: `dur` + `yenile`, 80-90 sn, `HikayeKomut/derleme.txt` ilk satır "0 hata" olmalı. `menu: Hikaye/...`, `hikaye_yaz: <ad>` (taslak HikayeKomut/hikaye_<ad>.txt). Kare testi: `menu: Hikaye/Kare Serisi` + `oynat: <ad>` → HikayeKomut/kareler/kare_NN.png (2 sn arayla, en çok 30).
- GitHub: islamsanateserleri-hub/uygulama, dal `claude/hikaye-studyosu` (PR #1; add_repo push gerekir). Repo: Assets/, Docs, DURUM.md, Installer/, _Arsiv/, README, CLAUDE.md. Push: `git -c push.negotiate=true push -q origin claude/hikaye-studyosu`.
- Not defteri (Claude Docs, kural 7): https://claude.ai/artifact/QfqgRcP5tUvdKXQ3JzP11x
- Yapı: Core/, Scripts/ (Hikaye.Runtime), Editor/, Uygulama/ (Hikaye.App). Paket kurulumu bash'te kapalı; Python `-I` ile.

## YENİ SOHBET NASIL BAŞLATILIR
1. Claude masaüstü uygulamasında yeni sohbet, + menüsünden **Klasör ekle** → `C:\Users\Teknodc\My project`.
2. İlk mesaj: "DURUM.md dosyasını oku ve oradan devam et."

## DERSLER
- `device_commit_files` önceki halini yükleyebilir: yaz, `sync; sleep 8`, commit, gerekirse boyut doğrula.
- C# 9: Color32/Color üçlü ifadede hedef türü; GUI grup/kaydırma arasında `return` yapma.
- Test görmeden "tamam" yazma; kareleri gerçekten aç ve bak.

## KULLANICI KURALLARI (sırayla uy)
1. PDF planına harfiyen uy; sıra değişmez; aşamayı bitirmeden diğerine geçme; test edilmemişi "tamam" yazma.
2. Aşama aşama ilerle. 25 Kas 01:46 kullanıcı: "bundan sonra aşamalar için benden onay alma" → aşama bitince (rapor + DURUM + not defteri + GitHub) onay beklemeden sıradakine geç. Geri alınamaz kararlar ve kullanıcı gerektiren işler (API anahtarı, satın alma, hesap) için dur ve bildir.
3. Kullanıcıya soru sorma. Her aşama sonunda GitHub'a gönder (dal claude/hikaye-studyosu), sonucu buraya yaz, kullanıcıya kısa özet ver.
4. Çalışan özellikleri bozma; değişiklikten önce yedek; her değişiklikten sonra derlemeyi kontrol et; hataları gizleme.
5. Her aşama sonu raporu: yapılanlar, değişen dosyalar, eklenenler, düzeltilen hatalar, derleme sonucu, gerçek test, açık sorunlar, commit, sonraki aşama.
6. Hedef: Unity bilmeyen kullanıcı hikâyeyi yazıp karakter seçerek tek tuşla 1080x1920 sesli, animasyonlu film üretsin (bağımsız Windows uygulaması).
7. Her işlemde hem bu dosyaya hem "DURUM.md" not defterine (Claude Docs, https://claude.ai/artifact/QfqgRcP5tUvdKXQ3JzP11x) aynı şekilde yaz (25 Kas 2026 kullanıcı isteği). Alt ajan kullanılabilir.
8. 25 Kas 17:38 kullanıcı (1 ve 4'teki "bitirmeden geçme" yerine geçer): hemen düzeltmeye kalkma; hatalı yere "HATALI – sonra düzeltilecek" yaz ve aşamalara devam et; denetçi ajan sürekli denetlesin; önce aşamalar biter, sonra hatalar tek tek düzeltilir. Test edilmemişe "tamam" yazma kuralı sürer.

## AŞAMALAR
| Aşama | Durum |
|---|---|
| 0 Hazırlık | TAMAM |
| 1 Unity'siz video kaydı | TAMAM, onaylandı (12/12 madde denendi) |
| 2 Bağımsız uygulama arayüzü | TAMAM, onaylandı (commit d7e8e23) |
| 3 Hikâye yazma ekranı + otomatik sahne | TAMAM, onaylandı (commit 9082643) |
| 4 Gemini ses sistemi | TAMAM (25 Kas 01:50; commit eed18dc + rapor). Gerçek Gemini: 6 istek, 6/8 satır üretildi, günlük 429 doğru yakalandı. Açık: Geri al, Durdur, ağ/400/kesik ses yolları gerçek testte tetiklenmedi |
| 5 Video düzenleme + dışa aktarma | ÇEKİRDEK TAMAM, EXE'de test edildi (25 Kas 02:17). Açık testler aşağıda (İŞLEM GÜNLÜĞÜ 02:17) |
| 6 Yapay zekâ yardımcıları | TAMAM (25 Kas 09:45), EXE'de test edildi; GitHub'da (cf5ca58) |
| 7 Kurulum ve son kontrol | Kurulum + üzerine güncelleme bu bilgisayarda denendi (25 Kas 11:00). AÇIK (kullanıcı): temiz bilgisayar testi, kaldırma testi |
| 8 Karakter kimlik sistemi | TAMAM, EXE'de kısmen test edildi (25 Kas 11:25). Açık testler günlükte |
| 9 VRoid + gelişmiş yüz animasyonu | TAMAM, EXE'de kısmen test edildi (25 Kas 14:10): 3 VRoid karakteri, ağız sesle uyumlu. Açık testler günlükte |
| 10 Hareketler/etkileşim | Yazıldı, test edildi; denetimde HATALI bulundu (aşağıdaki liste) |
| 11 Sahne yönetmeni | Yazıldı, test edildi; denetimde HATALI bulundu (aşağıdaki liste) |
| 12 3D mekân + atmosfer | YAZILDI – HATALI, sonra düzeltilecek |
| 13 Sinematik efektler | YAZILDI, EXE videosu alındı – HATALI, sonra düzeltilecek |
| 14 Hikâye devamlılığı | YAZILDI, mantık testi 25/25; arayüz denenmedi – HATALI, sonra düzeltilecek |
| 15 Seri + etkileşimli hikâye | YAZILDI, mantık testi 35/35 – HATALI, sonra düzeltilecek |
| 16 Gelişmiş yapay zekâ | DEĞERLENDİRİLDİ (Docs/Asama16_Degerlendirme.md); kod yok, ücret/lisans için kullanıcı kararı bekliyor |

## HATALI – SONRA DÜZELTİLECEK
**Aşama 10–12 (denetçi, 25 Kas ~17:35)**
1. AĞIR (TikTok riski): araba yakın planında iç çamaşırı göründü (kare_21). Torpido yükseltildi, orta konsol eklendi; DENENMEDİ. → DÜZELTİLDİ 25 Kas (Editör: test_araba, test_araba2).
2. AĞIR: son araba / konuşanın sırtı düzeltmeleri çalıştırılıp denenmedi; aşama 12 mekânlarının EXE videosu yok; Mekânlar sayfası ve kayıtlı mekân denenmedi; test_mekan yeniden çalıştırılmadı. → KISMEN: araba denendi; test_mekan 22:50 yeniden çalıştırıldı (Editör karelerinde mekânlar, yağmur, kar çizildi). Mekânlar sayfası, kayıtlı mekân (Belgeler klasörüne erişimim yok), EXE videosu AÇIK.
3. AĞIR: omuz üstü plan fiilen kalktı ("ters" ile neredeyse aynı) (KameraYonetmen.cs ~276-301). → KISMEN: dinleyen kenarda görünüyor (test_omuz); bazen omuz yerine saç parçası (3. denetim).
4. ORTA: `mekan3d: mutfak, duvar=mavi` / `mekan3d: duvar=mavi` 3D'yi sessizce kapatıyor; `duvar=mavi, zemin=ahsap` ve `hava=yagmur,sis` reddediliyor. → DÜZELTİLDİ (test_orta1; `hava=yagmur,sis` yalnız kodda).
5. ORTA: arabada ilk konuşan sürücü oluyor. → DÜZELTİLDİ: çocuk sürücü olmaz (test_orta1). 6. ORTA: hastanede yatan karakterin başı yatak başlığının içinde; etek çadır gibi, bacaklar açık. → DÜZELTİLDİ (22:45): 3D yatakta yatana gövdeyi izleyen yorgan serilir; baş yastıkta; kalkınca/mekân değişince kalkar (test_yatak, test_yatak2 Editör kareleri; EXE denenmedi). KÜÇÜK (açık): etek yüzünden yorgan yüksek tümsek yapıyor; 2D mekânda yatana yorgan yok.
7. ORTA: konuşanın sırtı dönük kalabiliyor; iç ses (|ic) satırında koruma yok. → DÜZELTİLDİ: iç seste kameraya döner (test_orta1). 8. ORTA: `yer:` konuşanı sıfırlamıyor. → DÜZELTİLDİ (kod; ayrıca denenmedi).
9. ORTA: her `mekan3d` havayı sıfırlıyor, zaman/ışık sıfırlanmıyor. → DÜZELTİLDİ: hemen önce yazılan `hava:` taşınır (test_orta1). zaman/ışık BİLEREK sürüyor (belgeye yazıldı).
10. ORTA: `hava: yagmur` sessiz; mekândan önceki atmosfer siliniyor. → Kod düzeltildi; SES VE ATMOSFER DENENMEDİ.
11. ORTA: `poz: X oturuyor` karakteri yana döndürüyor. → DÜZELTİLDİ (test_orta1; çizgi mekânda oturuş ayakta gibi görünüyor: KÜÇÜK).
12. KÜÇÜK: arabada 5. kişi ayakta kalıyor, figüran girebiliyor, yanlış koltuk adı sessiz; yeni nokta adları belgesiz; pencere noktaları uzakta; park duvar= / köy vurgu= etkisiz; köprü doğrulayıcısında mekan3d/hava denetimi yok.
**Aşama 13 (kendi testim + denetçi, 18:00)**
1. AĞIR: `dondur` zamanı durdurmuyor (kar yağıyor, göz kırpılıyor, saç oynuyor, gözyaşı/buhar akıyor, 2D karakter hareket ediyor). → DÜZELTİLDİ (Editör: test_dondur). EXE ve canlı önizleme denenmedi (önizlemede eski davranış).
2. AĞIR: gözyaşı yanakta değil, saçın üstünde iki mavi boncuk gibi; saç/el/diğer karakter arkasında kalmıyor. → DÜZELTİLDİ: baş dik ve eğikken yanakta (test_yas). Saç/el arkasında kalmama AÇIK.
3. AĞIR: nefes buharı ağız yerine burun/göz hizasında, yüzü kapatıyor; arkası dönükken de çiziliyor; mekân sonrası yakın planda baş kesilebiliyor. → DÜZELTİLDİ: buhar ağzın altında/önünde (test_yas); ilk satırda baş artık kadrajda (test_ilk).
4. ORTA: `yavas` kar/yağmur, kamera, 2D karakter ve bazı etkileşim kaymalarını yavaşlatmıyor. `dondur` süresi süre tahmininde yok.
5. ORTA: 2D mekânda 3D karakterle `filtre` neredeyse etkisiz. 6. ORTA: Editör önizlemesi ile EXE renkleri uyuşmuyor (sıcak filtre editörde görünmüyor).
7. ORTA: "geri sarma" gerçek geri sarma değil. 8. ORTA: otomatik efekt önerisi eksik; "döndüm"/"buzdolabı" yanlışlıkla nefes öneriyor; testte efekt önerisi sınanmıyor.
9. ORTA: flaş, sarsıntı, anı, huzme, yağmur, geri sarma, satırdan başlatma, Efekt sekmesi DENENMEDİ.
10. KÜÇÜK: satırdan başlatınca gözyaşı/buhar sonraki mekâna taşınıyor; yanlış karakter adı uyarısız; `nefes: yok` herkesi kapatıyor; 2D sis çok soluk; 2D mekânda 3D karakter ilk karede çömelmiş gibi; park başında baş kadraj dışında.
**Aşama 15 ekran kareleri (kendi bakışım, 18:10; Editör önizlemesi, test_seri)**
1. AĞIR: "devamı" kartı hâlâ ekranda duran seçim kartının üstüne biniyor ("Yarın aynı saatte" B düğmesinin üzerinde) — denetçinin 1. maddesi doğrulandı. → DÜZELTİLDİ (Editör: test_kart).
2. ORTA: 2D "sofra" mekânında 3D karakterlerle, Arda'nın "Dinliyorum!" satırında Arda görünmüyor (yalnız ayakları), Anne sol kenarda kesik. → DÜZELTİLDİ (23:02): kısa karakter masanın üstünde kalacak kadar yükseltilir, korkup çömelince de görünür (test_sofra Editör kareleri; EXE denenmedi). Açık (KÜÇÜK): yakın planda diğer kişi kenarda kesik kalır; buhar çocuğun önünden geçer.
**Aşama 14 (denetçi, 18:10; kod okuması)**
1. AĞIR: "Çözüldü" düğmesi soruyu listeden kaldırmıyor (liste açık bölümden önceki durumu gösteriyor); bu bölüme eklenen soru/olay/durum da görünmüyor. → DÜZELTİLDİ (Seri Testi 49/0; arayüzde tıklanmadı).
2. AĞIR: `geri:` / `vfx: vhs` sonrası bölüm sonuna kadar ölü karakter denetimi kapanıyor. → DÜZELTİLDİ (mantık testi).
3. ORTA: ölünün `poz:`/`sarilma:`/`cikar:` satırları yanlış çelişki veriyor. 4. ORTA: ilgisiz bir `olay:` açıklamasız dirilmeyi susturuyor.
5. ORTA: Olay/Durum düğmeleri satırı imlece değil sona koyuyor. 6. ORTA: önerilen seri adı/bölüm no ilk dosyadan alınıyor; numara kutusu Plan ile ortak.
7. ORTA: numarasız veya kopya hikâye durumları karıştırıyor. 8. ORTA: başa satır eklemek `kapak: 24` satır numarasını kaydırıyor.
9. KÜÇÜK: özet kartı sözcük ortasından kesiyor, tekrar tıklanınca ikinci kart ekliyor. 10. KÜÇÜK: dar pencerede düğme satırı taşar; kayıtlı hastane mekânında yanlış uyarı; sahneye hiç çıkmayan ölü "yazım hatası" sayılıyor.
**Aşama 15 (denetçi, 18:08; kod okuması)**
1. AĞIR: "devami:" kartı sonrasındaki her şeyi örtüyor (son yazısı, kararma, sert kesme); "secim:" ardından gelirse A/B kartı da örtülüyor. → DÜZELTİLDİ (test_kart, test_kart2).
2. AĞIR: şablon yer tutucuları ("…başlığını yaz", "…buraya yaz", uydurma "sonuc: %60 A") videoya ve yayın dosyalarına girebilir; denetim uyarmıyor. → DÜZELTİLDİ (mantık testi; yalnız uygulamanın kendi boş şablon cümleleri yakalanır).
3. ORTA: alternatif son aynı bölüm numarasıyla kırmızı çelişki veriyor; fragmanda `bolum:` yok. 4. ORTA: fragman `ani/poz/isik/zaman/hava/atmosfer` satırlarını atıyor; `mekan` ile `mekan3d` birbirini eziyor.
5. ORTA: "Son A/B" ses kopyalamıyor (Gemini hakkı harcanır); fragman Resources/Ses'tekileri almıyor. 6. ORTA: "Sezon oluştur" seçili şablonu yok sayıyor; şablonlar hep Arda/Anne.
7. ORTA: oylama sonucu elle; alternatif son boş iskelet; seri kapağı yalnız rozet rengi (eski serilerin rozet rengi de değişti).
8. KÜÇÜK: üretim düğmeleri onaysız; fragman sesi eski klasördeki dosyayı ezebilir; yersiz `oylama:` uyarısız; köprü denetleyicisi devami süresini saymıyor.
**3. denetim (25 Kas 22:03): "yeterli değil"**
1. AĞIR: karakterin ilk konuşmasında başı kadraj dışında kalabiliyor. → DÜZELTİLDİ 22:18 (ilk klipte çömelme kaldırıldı; test_ilk).
2. ORTA: omuz üstü planda omuz yerine kopuk saç parçası. 3. ORTA: omuz eşiğinde / yürürken kamera sıçrayabilir (denenmedi).
4. ORTA: `yer:` ile koltuğu alınan veya araba dışında oturan karakter 0,3 sn yumuşak kalkıyor (açık diz riski, Sahne3D.Hareket.cs ~886). → DÜZELTİLDİ (anında kalkma; test_araba2).
5. KÜÇÜK: gizli kişiye `tut:` eşyayı havada bırakır; `secim:`+`gorsel:` kartı örter; 5. kişi ve EXE'de `dondur` denenmedi; arabada asagidan/yukaridan/goz etkisiz; arkada çocuğun yalnız başı görünür.
**Eski açık maddeler:** yeni eşyalar kadraj dışında kalabiliyor; `hava` ile `atmosfer: yagmur` çakışıyor; her karede diskten okuma; aşama 4, 5, 6, 8, 9, 11'in denenmemiş parçaları; hareket/kamera/çeşit test videolarında konuşma sesi yok; aşama 13 öncesi cihazda yedek klasörü alınmadı (kural 4 atlandı; GitHub'daki 5cef8ae aşama 11 hali).
