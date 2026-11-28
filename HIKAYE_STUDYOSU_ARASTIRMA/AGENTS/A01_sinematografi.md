# A01 — Sinematografi ve Sinema Görüntü Yönetmeni

> Tarih: 2026-11-27. Kaynaklar: `SOURCES/A01_kaynaklar.md` (S001–S043; metinde `[A01-S0xx]`).
> Kod yazılmadı, değiştirilmedi; proje kodu yalnız okundu. Unity çalıştırılmadı, hiçbir test yapılmadı.
> Kısıt: araştırmanın sonunda ortak WebSearch hakkı (200 arama) tükendi; kalan iş yalnız daha önce bulunan sayfalar okunarak tamamlandı.
> "ÇIKARIM:" = benim yorumum ya da hesabım. "KOD:" = kaynak kod okuması (görselde ayrıca doğrulanmadı).

## 0. En önemli sonuçlar (kısa)
1. **Sistemde gerçek yakın plan yok.** KOD: "Yakın" planda kadraj yüksekliği 1,07 m; baş kadrajın ~%22'si. Bu, bel plan (MS) civarıdır. "Tepki" planı da aynı ölçüdedir. Aşırı yakın, yakın ve nesne detayı hiç yok.
2. **Her plan aynı "mercekle" çekiliyor** (dikey FOV 28–34°, yaklaşık 59–72 mm karşılığı). Geniş plan aslında boydan plan (kişi kadrajın ~%53'ü).
3. **Kesme ritmi satıra bağlı.** Plan süresi ≈ replik süresi; yalnız bir tür satır içi kesme (tepki) var ve seyrek.
4. **Sistemleştirme yolu:** hikâye → vuruş (beat) listesi → plan adayları → maliyet fonksiyonu + dinamik programlama ile seçim (akademik olarak kurulmuş yöntem [A01-S017]). Kayıt öncesi bütün ses süreleri bilindiği için planlama **önceden, bütün sahne için** yapılabilir; LLM ve ekran kartı gerektirmez.
5. **Sinema ile aramızdaki ana fark yalnız render değil, "plan başına hazırlık":** her plan için mercek, ışık, oyuncu yerleşimi ve odak ayrı kurulur. Kayıt çevrim dışı olduğu için bunu yapabiliriz.

## Görev tanımı
Protokol §2 "ALT AJAN 1", §9.1, §9.9: gerçek sinema görüntüsü nasıl elde edilir; Hikâye Stüdyosu'nda nasıl sistemleştirilir. Teslim: plan sözlüğü, sayılı mercek/kadraj kuralları, kural tabanlı kamera seçimi, mevcut kodla karşılaştırma.

## Araştırma alanı ve soruları
S1 plan ölçekleri · S2 mercek/perspektif · S3 açı ve hareket · S4 kompozisyon ve süreklilik · S5 dikey 9:16 · S6 süre ve ritim · S7 diyalog/tepki/aksiyon · S8 alan derinliği ve film karakteri · S9 storyboard ve otomatik sinematografi · S10 sinema–oyun sinematiği farkı · S11 Unity araçları · S12 mevcut kod.

---

## Bulgular

### S1 — Plan ölçekleri
- Yaygın ölçekler ve gövde kesme yeri: çok geniş (kişi mekânda küçük), boydan (tüm gövde), orta-uzak (diz üstü), cowboy (uyluk ortası), orta (bel üstü), orta-yakın (göğüs; göğüs kemiği altı), yakın (omuz üstü/yüz), aşırı yakın (göz, ağız, nesne ayrıntısı) [A01-S024][A01-S025].
- Eklem yerlerinden (diz, dirsek) kesmek "kopmuş uzuv" hissi verir [A01-S025].
- Sinemada ortalama plan ölçeği yıllar içinde orta-uzaktan orta-yakına yaklaşmıştır (1940'lar → 2015) [A01-S035].
- Yakın plan duygusal etkiyi her zaman artırmaz: bir deneyde (n=136) yakın plan, yalnız karakterin ifadesi **üzgünken** izleyicinin zihin durumu atfetme eğilimini artırdı [A01-S034]. ÇIKARIM: yakın plan, yüz gerçekten bir şey anlatıyorsa işe yarar; ifadesiz 3D yüze yakınlaşmak kusuru büyütür.

### S2 — Mercek, mesafe, perspektif
- Perspektifi belirleyen kamera-özne **mesafesidir**; odak uzaklığı kadraj genişliğini belirler (genel optik bilgisi; ÇIKARIM). Yaklaşık 60 cm'den çekilen yüzler, ~2,1 m'den çekilenlere göre daha az güvenilir, yetkin ve çekici bulundu [A01-S033].
- Dikey çekim için bir stüdyo yazısı 85 mm (diyalog yakın planı), 50 mm (iki kişi, derinlik), 24–28 mm (mimari, derinlik hareketi) öneriyor [A01-S005] (D sınıfı; yalnız destekleyici).
- Unity'de Physical Camera: sensör boyutu, focal length, shift, gate fit; FOV bunlardan hesaplanır; Depth of Field ile birlikte mercek değerleri bulanıklığı etkiler [A01-S002]. Cinemachine de Physical mercek modunu destekler [A01-S003].

**Ölçü sözleşmesi (öneri):** "dikey tam kare" = sensör 20,25 × 36 mm (yükseklik 36). Bu sözleşmede:
`dikey FOV = 2·atan(18/f)` ve `mesafe d = H · f / 36` (H = öznenin bulunduğu düzlemde kadraj yüksekliği, metre).

| f (mm) | 24 | 28 | 35 | 50 | 65 | 75 | 85 | 100 | 135 |
|---|---|---|---|---|---|---|---|---|---|
| dikey FOV (°) | 73,7 | 65,5 | 54,4 | 39,6 | 31,0 | 27,0 | 23,9 | 20,4 | 15,2 |
| yatay FOV (°) | 45,7 | 39,8 | 32,3 | 22,9 | 17,7 | 15,4 | 13,6 | 11,6 | 8,6 |

(Hesap; ÇIKARIM.) Dikey karede yatay açı çok dardır: 85 mm'de 1,5 m uzakta kadraj eni yalnız ~36 cm.

### S3 — Kamera açıları ve hareketleri
- Göz hizası diyalogda varsayılandır; hafif alttan = güç, hafif üstten = kırılganlık; aşırı açılar dar karede rahatsız edici olabilir [A01-S005] (D). Güç ilişkisini alt/üst açılı karşılıklı omuz üstü planlarla vermek klasik örneklerde görülür [A01-S022]; dövüşte yüksek/alçak açı güç değişimini, eğik (Dutch) açı dengesizliği anlatır [A01-S030].
- Dikeyde yatay pan yerine dikey tilt, vinç/jib, derinlik ekseninde hareket önerilir [A01-S005][A01-S007] (D/C). Oyuncular kadrajı yanlamasına değil kameraya doğru/kameradan uzağa hareket ettirilmeli [A01-S005].
- Push-in yakınlığı tırmandırır, pull-out açığa çıkarma/sonuç verir; belgesel havası veren sürekli el kamerasından kaçınılmalı [A01-S005] (D). El kamerası aksiyonda kaos katar ama zayıf koreografiyi örten koltuk değneğine dönüşebilir [A01-S030].
- Sinemada görüntüdeki hareket artışı esas olarak kamera hareketinden değil, kadraj içi hareketten gelir (sayfa özetine göre hareketli kamera oranı 1959'da %16, 1999'da %6; asıl metinden denetlenmeli) [A01-S011]. ÇIKARIM: "sinematik" olmak için kamerayı sürekli oynatmak gerekmez; oyuncu ve ortam hareketi (A03, A06) daha önemli.
- DOĞRULANAMADI: push-in/dolly'nin izleyici duygusuna etkisini ölçen deneysel çalışma okunmadı (arama hakkı bitti).

### S4 — Kompozisyon ve süreklilik
- **Headroom:** küçük bir boşluk; başı kenara dayamak sıkışık, çok boşluk kişiyi önemsiz gösterir [A01-S026][A01-S027].
- **Bakış/burun boşluğu (lead/nose room):** yüzün baktığı yönde daha fazla boşluk [A01-S026][A01-S027].
- **Göz çizgisi:** üçte bir kuralı; gözler üst üçte birde [A01-S026]; dikeyde de aynı öneri [A01-S005].
- **180° kuralı:** iki kişi arasındaki eksenin tek tarafında kalınır [A01-S023][A01-S027]. Geçiş yolları: eksen üstünde nötr plan, kesintisiz kamera hareketi, yönsüz cutaway; bilinçli kırma [A01-S023].
- **30° kuralı:** aynı öznenin ardışık planları ≥30° farklı olmalı **ya da** ölçek belirgin değişmeli; yoksa jump cut görünür [A01-S021][A01-S022][A01-S027].
- **Omuz üstü (OTS):** ön planda dinleyenin omzu ve başının arkası; üç derinlik katmanı; klasik biçimde sığ alan derinliği (özne net, ön ve arka bulanık) [A01-S022].
- **Kurgu önceliği (Murch):** duygu (%51) > hikâye > ritim > göz izi > 2D düzlem > 3D süreklilik [A01-S031].

### S5 — Dikey 9:16 ve dikey kısa dramalar
Bulunan kaynakların ortak noktaları (çoğu C/D; **güçlü akademik/ölçümlü kaynak bulunamadı**):
- Yakın ve orta plan baskın; geniş plan varsayılan değil [A01-S005][A01-S007][A01-S040][A01-S041].
- İki kişi yan yana yerine **derinlikte** dizilir (biri önde biri arkada), dikey istiflenir ya da tekli planlara bölünür; ön planda omuz kullanılır [A01-S005][A01-S006][A01-S007].
- Dikey çizgiler (kapı, pencere, merdiven, yüksek tavan) çerçeve elemanı olarak kullanılır [A01-S005][A01-S006].
- Ölü kare ve ara eylemler atılır (arabadan iniş → doğrudan ofis kapısı); ilk 3 sn'de kanca; bölüm sonu askıda [A01-S007][A01-S043].
- Storyboard ve plan listesi baştan 9:16 tasarlanır; 16:9'dan kırpma yapılmaz [A01-S042][A01-S006].
- Bölümler 1–2 dk; seri çok hızlı üretilir [A01-S009][A01-S043].
- Bir dergi makalesi örnek bir yapımda yakın planların %70'i aştığını yazıyor [A01-S008] (D; yöntem zayıf, **karar dayanağı sayılmamalı**).

**Güvenli alan:** TikTok yardım metni sayı vermiyor; üçüncü taraf **reklam** şablonu ölçümü: üst 240 px (%12,5), yanlar 120 px, alt 660 px (%34), sağ alt düğme sütunu 180 px [A01-S010]. KOD: uygulamanın önizlemesi üst %8, sağ %15, alt %20 varsayıyor (`StoryPlayer.BuildSafeArea`). → Çelişki Ç2.

### S6 — Plan süresi ve ritim
- Hollywood ortalama plan süresi ~10 sn (1930–40) → <4 sn (2000 sonrası); kısa planlar daha çok kadraj içi hareket taşıyor [A01-S011][A01-S035].
- Plan süreleri istatistiksel olarak log-normal dağılıma uyar; otomatik kurguda hedef ortalama süre (ASL) parametre yapılabilir [A01-S017][A01-S018].
- Hedef örnek video: ~40 kesme / 117 sn, ortalama ~3 sn, 0,7–6 sn (`Docs/VideoKalitesi.md`; proje içi gözlem).
- DOĞRULANAMADI: dikey kısa dramalar için ölçülmüş ortalama plan süresi bulunamadı.

### S7 — Diyalog, tepki, aksiyon
**Diyalog** [A01-S028]: önce mekânı gösteren ana plan → yakın planlar en duygulu anlara saklanır → tepki planları → cutaway/insert ile tekrar kırılır → duraklamalarla tempo. J-cut (ses önce), L-cut (ses taşar; tepki görürken konuşma sürer). Not: dikeyde "açılışta geniş" şart değil; kanca ilk planı yakın olabilir (örnek video böyle açılıyor). HMM tabanlı sistemde deyimler: geniş başla, konuşanı göster, duygu yükselince yakınlaş, kısa repliklerde asıl karakterde kal [A01-S012].
**Aksiyon** [A01-S029][A01-S030]: açılışta geniş ile coğrafya; her planda hareket **tamamlanır** (aşırı parçalama zararlı); darbe sonrası yüz tepkisi; yön kaybolunca yeniden geniş; yumruk için profil açı, tekme için karşıdan/omuz üstü; derin alan derinliği; hep orta plan monotonlaşır.
**Tepki:** tepki yüzü duyguyu taşır [A01-S028][A01-S029]; örnek videoda her büyük olaydan sonra var (VideoKalitesi.md).

### S8 — Alan derinliği ve film görüntü karakteri
- URP Depth of Field: **Gaussian** hızlı ama sınırlı, parametreleri yalnız uzak alan (Start/End/Max Radius); **Bokeh** gerçek kamera parametreleri (focus distance, focal length, aperture, bıçak) [A01-S001]. ÇIKARIM: Gaussian ön plandaki omzu bulanıklaştırmaz.
- Dikeyde arka planı tamamen silmek dar karede boğucu olabilir [A01-S005] (D).
- Hareket bulanıklığı: klasik sinema görünümü ~180° obtüratör (24 fps'te 1/48 sn) [A01-S032]. ÇIKARIM: 30 fps'te 1/60 sn karşılığı. URP motion blur'ün nesne hareketini kapsayıp kapsamadığı bu araştırmada doğrulanmadı (A04/A09).
- Gerçek mercek benzeri optik hesap (ÇIKARIM, ince mercek formülü, c=0,03 mm): 85 mm, 1,2 m'de net alan f/2,8'de ~3,4 cm, f/5,6'da ~6,7 cm, f/8'de ~9,6 cm. Yakın planda "f/2" taklidi yüzün yarısını bulanıklaştırır; **CU/ECU için f/5,6–8, MCU/OTS için f/4** ile başlanmalı. URP Bokeh'nin sensör boyutunu nasıl hesaba kattığı doğrulanmadı; değerler gözle ayarlanmalı.
- Film karakteri sırası (ÇIKARIM): ışık ve renk (A04) > mercek/kompozisyon > alan derinliği > hareket bulanıklığı > hafif gren/vinyet. Lens distortion ve chromatic aberration yalnız özel anlarda.

### S9 — Storyboard, shot planning, otomatik sinematografi
| Çalışma | Yöntem | Bize yarayan |
|---|---|---|
| He, Cohen, Salesin 1996 [A01-S019] | Film deyimleri = hiyerarşik durum makinesi; ortak kamera modülleri; kadraj için oyuncuyu hafif kaydırma | Sahne türüne göre deyim; "cheat" (oyuncu kaydırma) meşru |
| Galvane vd. 2015 [A01-S017][A01-S018] | Atış maliyeti (görünürlük, doluluk, anlatı önemi) + geçiş maliyeti (jump cut, 180°, bakış) + ritim maliyeti (log-normal) → dinamik programlama | **Önerilen çekirdek** |
| Leake vd. 2017 [A01-S012] | Diyalog kurgu deyimleri HMM; birleştirilebilir | Kullanıcıya "stil" seçimi |
| FilmAgent 2025 [A01-S015] | Unity'de hazır plan seti; LLM ajanları satır başına seçiyor; kamera uygunluğu 1,67→3,53/5 | Tek LLM geçişi zayıf; satır düzeyi kaba kalıyor (yazarların kendi sınırlaması) |
| CinemaTraj 2026 [A01-S038] | Atomik hareketler + SDF çarpışma/oklüzyon optimizasyonu | Çarpışma kontrolü kaldırılınca çarpışma oranı 0,056→0,557 |
| FilmFlow 2026 [A01-S016] | Atomik kamera işlemleri, sert kontroller, eleştirmen döngüsü | Sert kontrol + puanlama ayrımı; görev başına ~2,2 dk LLM |
| PACE 2026 [A01-S039] | Birimli plan alanları; başın ekran konumunu hedefleyen çözücü; her karede tek ana odak | **Plan tarifi veri yapısı** modeli |

ÇIKARIM: Bu işte LLM zorunlu değil. Hikâye metni zaten yapılı (karakter, duygu, hareket, komut). Deterministik kurallar + maliyet optimizasyonu ücretsiz, tekrar üretilebilir ve CPU'da anlıktır (≈40 vuruş × ≈10 aday).

### S10 — Sinema ile oyun sinematiği farkı
- Sinema kamerasında mercek, film arkası, diyafram vardır; oyun motorlarının sanal kameraları bunları sonradan kazandı [A01-S036].
- Gerçek zamanda çekim, kurgu ve ses eşzamanlıdır; hareket bulanıklığı pahalıdır; nesne bazında ışık hilesi yapılabilir; istenmeyen nesne gizlenebilir [A01-S037].
- Gerçek zamanlı dolaylı ışık, görüntü yönetmeninin ışığı sette gibi denemesine imkân verir [A01-S036].
- ÇIKARIM (uygulamamız için "oyun gibi görünme" nedenleri): tek ışık düzeni bütün planlarda; tek mercek; oyuncular planlar için yeniden yerleştirilmiyor; arka plan derinliksiz; kamera kemiği her karede takip ediyor; replik dışında kimse bir iş yapmıyor.

### S11 — Unity araçları
- Cinemachine bileşenleri: Position/Rotation Composer (ekran konumu, ölü bölge, sönümleme, lookahead [A01-S004]), Target Group + Group Framing, Deoccluder/Decollider, Clear Shot + Shot Quality Evaluator, Sequencer, Mixing Camera, Impulse, Auto Focus, Storyboard [A01-S020]; Physical mercek [A01-S003].
- Projede cinemachine 6.6.0 kurulu (TOPICS/00) ama KOD: `Assets/HikayeStudyo` içinde kullanılmıyor.

### S12 — Mevcut kamera kodu: ne var, ne yok

**Var (korunmalı):** 180° kuralı (kamera hep −z tarafında) · sert kesme · yavaş push-in · otomatik tepki kesmesi · ikili plan (sarılma/el tutma) · araya giren gövdeden kaçma (`EngelsizAci`) · duvar sınırı · elle `kamera:` komutları · `RitimDenetci` · güvenli alan payı (`Cerceve`) · yatan/oturan özel durumları · göz/ağız noktaları hesabı (`Sahne3D.Sinema.cs: YuzNoktalari`, kamerada henüz kullanılmıyor).

**Bugünkü gerçek ölçüler** (KOD'dan hesap; `Sahne3D.CamFov=30`, `CamDist=6.2`; baş yüksekliği 0,24 m varsayımı):
| Koddaki ad | FOV / mesafe | Kadraj H × W (m) | Baş / kadraj | Gerçek ölçek | ≈ mercek |
|---|---|---|---|---|---|
| Geniş | 30° / 6,2 m (en çok 8,5) | 3,32 × 1,87 | %7 | boydan | 67 mm |
| Orta | 30° / 3,1 m | 1,66 × 0,93 | %14 | diz / orta-uzak | 67 mm |
| Yakın, Tepki | 28° / 2,15 m (en az 1,6) | 1,07 × 0,60 (min 0,80 × 0,45) | %22 (en çok %30) | bel – cowboy arası | 72 mm |
| Omuz | 34° / konuma bağlı | ≈1 (tahmin) | — | bel | 59 mm |

D001 kareleri de bunu gösteriyor: "yakın" karelerde göğüs ve gövde görünüyor.

**Eksik / sorunlu:**
| # | Konu | Ayrıntı | Doğrulama |
|---|---|---|---|
| K1 | CU, ECU, detay/insert, POV, arkadan, üstten, alçak kahraman, çok geniş, kalabalık planı yok | `enum Plan` 6 değer | KOD |
| K2 | Mercek çeşidi yok | FOV 28–34 | KOD |
| K3 | Kesme yalnız satır sınırında + tek tepki | tepki: satır ≥3 sn, 3 satır bekleme, güçlü değilse ≥4,5 sn | KOD |
| K4 | Plan seçimi açgözlü; sahne bütününü görmüyor | `PlanSec(son1, son2, …)` | KOD |
| K5 | Kadraj çapası baş **kemiği**; bakış boşluğu yok | D001 4/9/10. karelerde yüz sol kenarda ve sola bakıyor. Neden hipotezi: profilde yüz kemiğin ~10–15 cm önünde, 60 cm'lik ende bu büyük kayma | gözlem + hipotez |
| K6 | Jump cut riski | Tepki(dinleyen) ardından aynı kişi konuşup "Yakın" seçilirse neredeyse aynı kadraja kesilir; aynı konuşanın iki "Orta"sı yaw ±12° = 24° (<30°), aynı ölçek | KOD; videoda doğrulanmadı |
| K7 | Yaw her satırda `yon=-yon` ile taraf değiştiriyor; konuşana bağlı değil | karşılıklı planların tutarlılığını bozabilir | KOD |
| K8 | Kamera her karede baş kemiğini izliyor (ölü bölge yok); `EngelsizAci` her karede açı değiştirebiliyor | "yüzen kamera" ve yerinde hareketlerin arka planda büyümesi olası | KOD + hipotez |
| K9 | El titremesi her zaman açık (roll dahil) | dikey kaynak bundan kaçınmayı öneriyor (D) | KOD |
| K10 | DoF Gaussian (yalnız uzak); kod yorumu "omuz bulanık" diyor ama beklenen etki oluşmaz; odak+0,9 m'den önce arka plan net | `AtmosferVolume.ApplyVolume` | KOD + [A01-S001]; görselde doğrulanmalı |
| K11 | 3D mekânda da 2D dijital zoom: her satırda 1,00→1,05, satır başında geri açılıyor | `StoryPlayer.UpdateCamera` (`pushIn=0.05`, `world.localScale`); 1080p render büyütülüyor | KOD; videoda doğrulanmalı |
| K12 | `RitimDenetci` eşiği 5/8 sn (hedef ~3 sn); satır içi tepki kesmesini simüle etmiyor | | KOD |
| K13 | Plan, ışık, oyuncu yerleşimi birbirinden habersiz | anahtar ışık sabit yönlü | KOD |
| K14 | `VideoKalitesi.md`'nin önerdiği `plan:` komut adı **zaten dolu** (sezon planı) | `KomutKayit.cs:108` | KOD |

---

## Teslim: Hikâye Stüdyosu için sinematografi sistemi (öneri; test edilmedi)

### 1. Plan sözlüğü ve başlangıç sayıları
Hepsi ÇIKARIM: ölçek tanımları [S024][S025]'ten, metre/mercek değerleri hesap ve deneyle ayarlanacak başlangıçtır (1,75 m yetişkin; karakter boyuyla orantılanmalı). `d = H·f/36`. "Göz y" = karenin **üstünden** oran.

| Kod | Ad | H (m) | f (mm) | d (m) | Göz y | Diyafram | Tipik süre | Kullanım |
|---|---|---|---|---|---|---|---|---|
| ECU | aşırı yakın | 0,22–0,30 | 100 | ~0,7 | 0,42 | f/8 | 0,8–2 sn | kanca, göz, güç uyanışı |
| CU | yakın | 0,38–0,48 | 100 | ~1,2 | 0,36 | f/5,6 | 1,5–4 | doruk, ağlama, korku, tepki |
| MCU | göğüs | 0,60–0,70 | 85 | ~1,5 | 0,31 | f/4 | 2–4 | **diyalog varsayılanı** |
| MS | bel | 0,85–0,95 | 65 | ~1,65 | 0,28 | f/4 | 2–4 | jest, el hareketi |
| OTS | omuz üstü | ~0,8 | 65 | ~1,45 | 0,31 | f/4 | 2–4 | karşılıklı konuşma |
| COW | uyluk | 1,15–1,25 | 50 | ~1,7 | 0,24 | f/5,6 | 2–4 | meydan okuma, silah/kılıç |
| FS | boydan | 2,0–2,2 | 40 | ~2,3 | baş üstü 0,12–0,18; ayak ≤0,82 | derin | 2–3 | yürüme, giriş, diz çökme |
| DEPTH2 | derinlikte ikili | 1,0–2,0 | 35–50 | — | öndeki 0,30 | f/4 | 3–5 | iki kişi tek karede |
| WS | geniş | 3,5–5 | 28 | ~3 | ufuk 0,65–0,75 | derin | 2–3 | mekân, güç dengesi |
| EWS | çok geniş | ≥8 | 24 | ≥6 | ufuk düşük | derin | 2–4 | şato, ejderha, ordu |
| INS | nesne detayı | 0,15–0,5 | 85–100 | — | nesne merkez-üst | f/4–5,6 | 0,8–2 | mektup, çatlayan fincan, el |
| POV | öznel | olaya göre | 35–50 | göz noktası | — | — | 1–2 | bakış → gördüğü |
| BACK | arkadan | 0,9–2 | 35–50 | — | — | — | 1,5–3 | tehditle yüzleşme, merak |
| TOP | üstten (60–90°) | 3–8 | 28–35 | — | — | derin | 1,5–3 | kalabalık, yalnızlık |

Oda sınırı: iç mekân 5,2 m (kod) olduğundan EWS ve uzun mercekli geniş planlar büyük set ister (A05).

### 2. Kadraj kuralları (sayılı; ÇIKARIM)
1. Çapa = iki gözün orta noktası (mevcut `YuzNoktalari`), baş kemiği değil.
2. Göz y: tablodaki gibi (üstten %28–42). CU/ECU'da saç tepesi kesilebilir; MCU'da headroom %4–8.
3. Bakış boşluğu: yüz sağa bakıyorsa göz ortası x = 0,40–0,45; sola bakıyorsa 0,55–0,60; kameraya bakıyorsa 0,50. Güvenli alanın sağ düğme payıyla birlikte hesaplanır.
4. Yüz ve olayın ana noktası x ∈ [0,11 ; 0,83], y ∈ [0,12 ; 0,66] içinde kalır [S010 temelli; Ç2 çözülene kadar].
5. Açı: varsayılan göz hizası (±5 cm). Güçlü/tehditkâr: kamera aşağıdan 8–15°. Zayıf/yalnız: yukarıdan 10–20°. Eğik ufuk yalnız kâbus/panik.
6. Yüz yönü: kameraya 15–45° (3/4). Tam profil ve ensenin görünmesi yalnız bilinçli seçim (BACK).
7. OTS: ön plandaki omuz/baş kadraj eninin %20–30'u, bulanık, konuşanın yüzünü kapatmaz.
8. İki kişi: yan yana yerine derinlikte; yatay mesafe sığmıyorsa tekli planlara böl.
9. Perspektif: normal yüz planında d < 1,0 m yok. Bilinçli bozma (korku, kâbus): 24–35 mm, 0,5 m.
10. Sabitleme: plan başında kadraj kurulur; ölü bölge (ekranın ~%8–10'u) aşılmadıkça kamera kıpırdamaz; yürümede yumuşak takip.
11. Hareket bütçesi: planların çoğu sabit ya da çok yavaş push-in (kadraj yüksekliği plan boyunca %3–8 değişir). Pan/orbit seyrek. Sarsıntı yalnız darbe/panik (Impulse benzeri), sönerek.

### 3. Süreklilik ve kurgu kuralları
- 180°: eksen **çift başına** tutulur; yalnız nötr plan, INS ya da hareketli planla değiştirilir [S023].
- 30° / ölçek: aynı özneye ardışık iki plan → yaw farkı ≥30° **veya** H oranı ≥1,5 [S021]. Aksi yasak (sert kısıt).
- Konuşan/dinleyen karşılıklı planları aynı ölçek ve simetrik açı (eşlenik).
- Süre (ÇIKARIM; örnek video ve [S017] modeli): hedef medyan 2,8 sn (log-normal), diyalog 2–4, aksiyon 1–2,5, duygusal doruk 3–5 (push-in'li); en az 0,7, en çok 6 sn.
- Uzun replik (>4 sn) duraklama/kelime sınırında bölünür: konuşan → dinleyen tepkisi / el / nesne → konuşan (başka ölçek). Kesme zamanı gerçek ses dosyasından alınır (A07 kelime zaman damgası).
- L-cut: dinleyenin yüzüne replik bitmeden 0,3–1,0 sn önce geçilebilir.

### 4. Hikâyeden plana: deyim tablosu (varsayılanlar; kullanıcı ezebilir)
| Durum (metinden algılama) | Plan dizisi |
|---|---|
| Kanca (`kanca:` / ilk satır) | ECU/CU (0,8–2 sn) → mekân WS/FS |
| Yeni mekân | WS ya da FS (kanca değilse) → MCU |
| Normal diyalog | OTS/MCU A ↔ OTS/MCU B; her 4–6 planda bir DEPTH2/MS |
| kizgin | MCU→CU, hafif alttan, kesmeler hızlanır; karşı taraf hafif üstten |
| korkmus | CU, hafif üstten, tepki + gördüğü şey (POV) |
| sasirmis | olay (INS/WS) → CU tepki → ikinci kişinin tepkisi |
| uzgun / aglamakli | MCU → yavaş push-in CU; süre uzun |
| sevgi / sarılma | DEPTH2 → CU karşılıklı |
| dusunceli / iç ses | CU 3/4, çok yavaş push-in, f daha açık |
| fisilti | CU, OTS çok yakın |
| Güç uyanışı / büyü | INS (el/göz) → CU alttan → WS → 2–3 tepki |
| Yaratık / ordu belirmesi | tepki CU (bakış yukarı) → EWS/alçak açı → kalabalık TOP/FS |
| Yürüme / koşma (`git:`) | FS ya da derinlikten kameraya yürüyüş; takipli MS; varışta MCU |
| Dövüş | WS coğrafya → profil MS (darbe tam görünür) → CU tepki → WS |
| Nesne verme/alma (`tut:`) | MS → INS → tepki |
| Bölüm sonu | CU/ECU push-in → sert kesme |

### 5. Kural tabanlı kamera seçimi mimarisi
```
Hikâye ayrıştırıcı + ses süreleri
 → Vuruş listesi (tür, katılımcılar, duygu, yoğunluk, hareket, süre, önemli nokta)
 → Deyim tablosu: her vuruş için 4–10 plan adayı (PlanTarifi)
 → Geometri çözücü: aday kamera konumu; sert eleme (çarpışma, yüz kapalı, güvenli alan, 180°)
 → Puanlama: atış (görünürlük, ölçek ↔ önem, kompozisyon) + geçiş (30°, bakış, ölçek farkı)
             + ritim (log-normal süre) + çeşitlilik (son N planla benzerlik)
 → Dinamik programlama (bütün sahne) → Plan listesi (storyboard dosyası)
 → Kullanıcı önizler/değiştirir (kare kare storyboard) → Oynatıcı listeyi uygular
```
- **PlanTarifi alanları:** tür, özne, ikinci özne, f, H, göz hedefi (x,y), yaw, pitch, hareket (tür+miktar), odak hedefi, diyafram, başlangıç/bitiş (ses zamanı), ışık ön ayarı, oyuncu "cheat" kaydırması, gerekçe metni.
- Plan listesi deterministik olmalı (rastgelelik tohumlu) ki aynı hikâye aynı videoyu versin.
- Elle komut önerisi: `cekim: cu Ayse`, `cekim: detay mektup`, `cekim: genis` (Türkçe harf yok; `plan:` kullanılamaz — K14). Mevcut `kamera:` komutları geriye dönük çalışmalı.
- Uygulayıcı katman iki seçenek: (a) mevcut `KameraYonetmen`'i PlanTarifi okuyacak hale getirmek; (b) Cinemachine kameraları. ÇIKARIM: veri katmanı uygulayıcıdan bağımsız tutulsun; ilk adım (a) en az risk; Cinemachine'in Composer/Deoccluder/Group Framing'i küçük bir prototiple karşılaştırılsın (A09).
- "Sanal set hilesi": plan başına oyuncu birkaç on cm kaydırılabilir, engel gizlenebilir, ışık kameraya göre yeniden kurulabilir [S019][S037]; süreklilik ekranda bozulmadıkça.
- **Aşamalar:** (1) göz çapası + bakış boşluğu + ölü bölge + 2D zoom/titreme düzeltmesi; (2) CU/ECU/INS ve mercek tablosu; (3) satır içi bölme + yeni `RitimDenetci` eşikleri; (4) vuruş listesi + puanlama + DP; (5) storyboard arayüzü.
- **Denetçi ölçütleri:** ortalama plan süresi, ölçek dağılımı (başlangıç hedefi yakın %40–55, orta %30–40, geniş %10–20 — ÇIKARIM), jump cut sayısı (0), güvenli alan ihlali (0), yüzü kapalı plan (0), tepki planı sayısı.

---

## Teknoloji kayıtları
| Ad | Kategori | Ne yapar / nasıl | Katkı | Unity uyumu | Donanım | Maliyet / lisans | Avantaj | Dezavantaj | Alternatif | Kaynak | Test | Tarih / güven |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Unity Physical Camera | kamera | sensör+focal → FOV, DoF bağı | mercek dili | yerleşik (URP) | ek yük yok (ÇIKARIM) | Unity içinde | gerçek sinema ölçüleri | gate fit dikkat ister | yalnız FOV hesabı | S002 | yapılmadı | 27.11.2026 / A |
| Cinemachine 6.6 | kamera çatısı | Composer, Deoccluder, Group Framing, ClearShot, Impulse | kadraj ve engel çözümü | projede kurulu | düşük (ÇIKARIM) | Unity paketi (lisans ayrıntısı A12) | hazır, belgeli | mevcut kodla ikinci kamera mantığı; sönümleme davranışı öğrenilmeli | özel kod | S003,S004,S020 | yapılmadı | A (belge), uygulama D |
| URP DoF Bokeh | post | fiziksel parametreli bulanıklık | yakın plan ayrımı | yerleşik | Gaussian'dan pahalı (ÇIKARIM); tümleşik GPU'da ölçülmeli | Unity içinde | ön+arka bulanıklık beklenir | saç/kenar kusuru olası | Gaussian | S001 | yapılmadı | A (parametre) |
| Maliyet + DP otomatik kurgu | algoritma | atış/geçiş/ritim maliyeti | sahne bütününde plan seçimi | saf C# | CPU, önemsiz (ÇIKARIM) | yayımlanmış yöntem; kod alınmayacak (lisans/patent denetimi A12: aynı konulu bir patent kaydı (US6040841, "virtual cinematography") arama sonucunda görüldü; He vd. ile ilişkisi, içeriği ve durumu doğrulanmadı) | deterministik, ücretsiz | ağırlık ayarı emek ister | durum makinesi; LLM | S017–S019 | yapılmadı | A |
| LLM kamera ajanı | AI araç (video üretici DEĞİL) | metinden plan seçimi | öneri kalitesi | dış servis | bulut | ücretli/kotalı | esnek | tekrar üretilemez, maliyet | kurallar | S015,S016,S038 | yapılmadı | B |

## Karşılaştırma ve çelişkiler
- **Ç1 Geniş plan:** dikey kaynakları "geniş plandan kaçın, yakın baskın" diyor [S005,S007,S040] (C/D); hedef örnek video çok geniş şato/ejderha planları kullanıyor. Öneri: yakın/orta varsayılan, geniş plan seyrek ama "gösteri" ve coğrafya için zorunlu.
- **Ç2 Güvenli alan:** reklam şablonu ölçümü alt %34 [S010], uygulama %20. Organik video için doğrulanmış sayı yok → gerçek telefonda kontrol gerekli (A07, A08).
- **Ç3 Alan derinliği:** OTS için sığ [S022], dövüş için derin [S030], dikeyde arka plan okunmalı [S005]. Çelişki değil, plan türüne bağlı (tabloya işlendi).
- **Ç4 El kamerası:** kodda sürekli; dikey kaynak kaçınmayı öneriyor (D). A/B karşılaştırmasıyla karar.
- **Ç5 Açılış:** klasik diyalog kuralı "geniş/master ile aç" [S028]; dikey/kanca pratiği yakın planla açıyor. Kanca varsa yakın; 2.–4. planda coğrafya ver.

## Riskler
| Risk | Etki | Önlem |
|---|---|---|
| CU/ECU karakter yüz, cilt, göz, dudak kusurlarını büyütür (Rocketbox için ölçülmedi) | yüksek | A02/A07 ile yakın plan testi; kötüyse CU'yu kısa ve az kullan |
| Daha çok yakın plan + göz takibi → "ölü bakış" görünür | yüksek | bakış/göz kırpma testi (A02, A03) |
| Hızlı kesme, kötü animasyonu gizlemez; kesmede poz süreksizliği | orta | plan başına poz sürekliliği kontrolü (A03) |
| Mekânlar dar ve düz; mercek/DoF derinlik yaratamaz | yüksek | ön plan nesneleri, derin set (A05) |
| Bokeh DoF tümleşik GPU'da yavaş | orta | yalnız kayıtta yüksek kalite (A09) |
| Alt kaynaklar zayıf (dikey dil C/D) | orta | kullanıcıyla A/B önizleme; yeni örnek videolarda plan sayımı |
| Aşırı otomasyon kullanıcı kontrolünü alır | orta | storyboard listesi düzenlenebilir olsun |

## Test önerileri
**Bu bilgisayarda yapılabilir (tümleşik GPU; yalnız görüntü karşılaştırma):**
- T1 Mercek/ölçek ızgarası: aynı karakterle ECU→FS, f=35/65/100; kareleri yan yana incele. Başarı: tabloyla ±%10 H; yüz bozulması yok.
- T2 Göz çapası + bakış boşluğu: 10 farklı baş yönünde yüz güvenli alanda mı.
- T3 K11 ve K6 doğrulaması: 30 sn kayıttan kare farkı ile satır başı zoom ve jump cut ara.
- T4 DoF Gaussian ↔ Bokeh: OTS'de omuz bulanıklığı ve kare süresi.
- T5 Aynı 30 sn sahne: mevcut yönetmen ↔ elle yazılmış 12 planlık liste (kod yazmadan önce `kamera:` komutlarıyla yalnız kısmen yapılabilir; yeni ölçekler için prototip gerekir). Kullanıcı kör karşılaştırması.
- T6 Telefonda güvenli alan kontrolü.
**GPU isteyen:** HDRP fiziksel kamera + Bokeh + hareket bulanıklığı + yüksek örnekli yakın plan kalitesi.

## Öz denetim
- Dikey sinematografi için güçlü (A/B, ölçümlü) kaynak bulamadım; plan oranı ve süre hedefleri büyük ölçüde örnek video + genel sinema verisi + ÇIKARIM.
- Metre/mercek/göz-y/diyafram sayıları denenmedi; başlangıç değeridir.
- Kod bulguları okuma ve hesapla; Unity çalıştırılmadı. D001 dışında görüntü incelenmedi; ses/video izlenmedi.
- WebFetch sayfaları küçük bir modelle özetlenerek döndürür; kritik alıntılar ana araştırmacı/denetçi tarafından asıl sayfadan kontrol edilebilir.
- Okunamayan: Toric Space, Christie vd. kamera kontrol survey'i, Tandfonline mikro drama makalesi.
- Klasik ders kitapları (Mascelli "Five C's", Katz "Shot by Shot", Brown "Cinematography: Theory and Practice", Murch'un asıl kitabı) okunmadı.

## Eksik alanlar / açık sorular
1. Dikey kısa dramalarda ölçülmüş plan süresi ve ölçek dağılımı (öneri: kullanıcının yükleyeceği 2–3 örnek videoda kare kare sayım).
2. Kamera hareketi ve kesme algısı üzerine deneysel çalışmalar (Smith & Henderson tipi) okunmadı.
3. URP motion blur nesne desteği; URP Bokeh'nin sensör ilişkisi; tümleşik GPU maliyeti.
4. Organik TikTok güvenli alanı.
5. Kullanıcı kararı: tam otomatik mi, storyboard onaylı mı? (Öneri: onaylı.)

## Diğer ajanlardan beklenen / ortak konular
- **A02:** CU/ECU'ya dayanacak yüz, göz, cilt, saç kalitesi nedir? Karakter boylarına göre H ölçekleme.
- **A03:** yürüme/koşma/diz çökme klipleri; kesmeler arası poz sürekliliği; bakış (look-at) hedefleri; kameranın izlemeyeceği sakin duruş.
- **A04:** plan başına kameraya göre anahtar/kenar ışık; tonemapping/HDR (KOD: `Cam.allowHDR=false`); motion blur; gren/vinyet ölçüsü.
- **A05:** derinlikli, dikey çizgili, ön plan nesneli setler; EWS için büyük sahne.
- **A07:** kelime/duraklama zaman damgası (satır içi kesme ve altyazı); altyazı y konumu güvenli alanla birlikte.
- **A08:** vuruş listesi, deyim tablosu, DP mimarisi ortak; "cekim:" komut adı.
- **A09:** Cinemachine ↔ özel kamera; Bokeh/AA maliyeti; K11 (2D büyütme).
- **A12:** "virtual cinematography" patent kaydı (US6040841) ve otomatik kurgu yöntemleri için patent durumu.

## Öneriler (kanıtla)
1. Önce **ucuz düzeltmeler**: göz çapası + bakış boşluğu, ölü bölge, 30° sert kuralı, satır başı 2D zoom'un 3D mekânda kapatılması, titremenin duruma bağlanması [S021,S026; K5–K11].
2. **Plan sözlüğünü genişlet**; en başta CU, ECU, INS, DEPTH2, EWS [S024,S025; K1; VideoKalitesi].
3. **Mercek dili** ekle (24–100 mm; `d=H·f/36`) [S002,S033].
4. **Satır içi kesme** ve ~3 sn medyan; tepki planlarını deyim haline getir [S011,S017,S028].
5. **Önceden planlayan yönetmen** (maliyet + DP), düzenlenebilir storyboard çıktısı; LLM yalnız isteğe bağlı öneri [S015,S017,S019].
6. Kamera tek başına örnek video kalitesini getirmez; A02–A05 ile birlikte test edilmeli (ÇIKARIM).
