# X2 — Çapraz denetim 2 (ses, lisans, sinematografi rolleri)
Tarih: 2026-11-27. Okunan: 01 §5, 05, 06, 07, A00, A01, A03, A07, A08, A09, A12; A02 ve A05/A06 yalnız ilgili satırlar. Kod yalnız okundu, Unity çalıştırılmadı, ses/video izlenmedi, git komutu çalıştırılmadı. Web: 3 arama + 1 sayfa. Yollar `Assets/HikayeStudyo/` köküne göredir. "ÇIKARIM" = benim yorumum.

## 0. En önemli bulgular
1. **A08 yanlış:** "TTS dosyasının baş/son sessizliği boşluğa eklenir" ve "K0: sessizliği kırp". Kod bunu zaten yapıyor (`Scripts/ProcAudio.cs:14-99`, çağrı `StoryPlayer.cs:311`).
2. **A12 P1 eksik ve sayısı yanlış:** katalogda 35 değil **36** Mixamo karakteri var; aynı 36'sı `Resources/OyuncuListesi.asset` içinde de var; ayrıca Mixamo **animasyonları** bütün insan karakterlere ekleniyor (`Editor/Karakter3DKurulum.cs:119-127`). "Mixamo'yu katalogdan çıkar" yetmez.
3. **A01 kendi kuralıyla çelişiyor:** ECU mesafesi ~0,7 m; aynı belgenin 9. kuralı normal yüz planında d < 1,0 m yasak diyor.
4. **Tek veri modeli mümkün**, ama kimlik, zaman birimi, ses miksi ve parçalı render konularında A07, A08, A09 bugün uyuşmuyor (§2).
5. **Tepki planı bugün ters çalışır** (H1 doğruysa): dinleyen "dur" ile konuşma jesti oynatır; kurgu eklenirse dinleyen konuşuyormuş gibi görünür.

## 1. Doğrulanan iddialar
| # | İddia (kaynak) | Sonuç | Kanıt / sınır |
|---|---|---|---|
| D1 | `dur` konuşma jesti klibine eşleniyor (A03 H1) | **DOĞRU (kod okuması, koşullu)** | `Karakter3D.cs:106-109, 123`: dur→idle; tam ad, sonra "başlar". `Karakter3DKurulum.cs:133-144`: büyük harfle başlayanlar dosya sırasıyla önde; `NesneKur.cs:913`: gestic_talk_neutral_01→`Idle_Talking_Loop`, idle_neutral_01→`Idle_Loop`; "g" < "i". `Karakter3DKurulum.cs:132` yorumu yanlış. Koşullar: RbAnim'de bu dosya var; Mixamo'da adı tam `Idle` olan klip yok. RbAnim ve Hazir depoda yok; Unity testi gerekir. |
| D2 | Konuşma satırı yürümeyi eziyor (A03 H2, H2b) | **DOĞRU (yalnız satır içi `yuru`/`kos` etiketi)** | `StoryPlayer.cs:1003-1004`: `Ensure3D` hemen ardından `Act`; `Sahne3D.cs:310-329` `walking`e bakmıyor; `Idle` de bakmıyor (351-359). Ek sınır: `git:` komutu etkilenmez, çünkü satır onu bekler (`StoryPlayer.cs:426-431`, `Sahne3D.Hareket.cs:609`). A03 bunu ayırmalı. |
| D3 | Yuvasındaki karaktere `yuru` etkisiz (A03 H3) | **DOĞRU** | `Sahne3D.cs:245` 5 cm altında çıkıyor. |
| D4 | `NN.wav` yedeği sessizce kabul ediliyor (A07 K2) | **DOĞRU** | `StoryPlayer.cs:308-309`, `Editor/GeminiSes.cs:61-67`, `DisSes.cs:67-68`, `StoryModel.cs:319`. `TopluSes.cs:62` yalnız NN adıyla yazıyor. Ek: yedek yalnız anahtar dosyası yoksa devreye girer. |
| D5 | Karaoke süresi karakter orantılı (A07 K3) | **DOĞRU, eksik anlatılmış** | `KaraokeAltyazi.cs:170-184` ağırlık = max(2, uzunluk) + noktalama; 233-242 dağıtım. Ama bütün klibe değil, ses zarfından bulunan konuşma aralığına dağıtılıyor (`AgizSenkron.cs:98-111`). İç duraklamalar yine görülmüyor. |
| D6 | Mixamo karakterleri katalogda (A12 P1) | **DOĞRU; sayı YANLIŞ (36)** | `Resources/KarakterKatalogu.asset`: 36 `Mixamo_*` kaydı, hepsinde dolu prefab başvurusu; 36/36 guid `OyuncuListesi.asset` içinde. EXE içeriği incelenmedi (DOĞRULANAMADI). |
| D7 | Rocketbox'ta viseme kullanılmıyor, yalnız çene (A07, A02) | **Kod kısmı DOĞRU; model kısmı DOĞRULANAMADI** | `YuzCanlilik.cs:407-413, 450-456`: jawopen öncelik 0, `_aa` eleniyor; `AA_VI_11_E` kalıbı eşleşmiyor. Gerçek şekil adları depoda yok (`Hikaye/Yuz Testi`). |
| D8 | Baş/son sessizlik gecikme yaratıyor (A08 S6, E2) | **YANLIŞ** | `ProcAudio.cs:52-76`: baş/son atılıyor (~50 ms pay), 0,5 sn üstü iç boşluk 0,32 sn'ye iniyor. Yalnız DecompressOnLoad kliplerde (17). `lineGap` 0,22 doğru (`StoryPlayer.cs:19, 477`). |
| D9 | Açılış: 0,35 sn siyah + 1,7 sn siyah kanca (A08 E3) | **DOĞRU** | `StoryPlayer.cs:343, 1426-1445`. 343 yorumu "kayıt başlasın diye"; kaldırılırsa Recorder yolunda ilk kare kaybı test edilmeli. |
| D10 | Tepki kesmesi koşulları ve her satırda taraf değişimi (A01 K3/K7, A08 E6) | **DOĞRU** | `KameraYonetmen.cs:147, 191-199`. |
| D11 | 3D mekânda 2D yakınlaştırma sürüyor (A01 K11) | **DOĞRU (kod)** | `StoryPlayer.cs:1023` camScaleT=1 yapıyor ama `:1225` `pushIn*lineProgress` ekliyor, `:1246` ölçekliyor. |
| D12 | 3D'de gölge yok, kamera LDR (A09 K1/K2) | **DOĞRU** | `Sahne3D.cs:91, 100, 112`; `Mekan3D.cs:271`; `Mekan3D.Istasyon.cs:176, 187`. |
| D13 | FFmpeg "latest" LGPL; lisans dosyası yalnız LGPLv3 (A12) | **DOĞRU** | `FfmpegIndir.cs:13`; `StreamingAssets/ffmpeg/LICENSE.txt` 165 satır; `Installer/HikayeStudyosu.iss:49` derleme klasörünün tümünü paketliyor. İkili depoda yok. |

## 2. Ses uzmanı rolü: A03, A02/A07, A08, A09, A01 uyumu

### 2.1 Birleşir mi? Evet, üç katmanla (ÇIKARIM; öneri, denenmedi)
| Katman | Sahibi | İçerik | Zaman birimi |
|---|---|---|---|
| `ses/<ClipKey>.json` (ses varlığı, değişmez) | A07 | wav özeti, metin özeti, sağlayıcı/style, süre, kelime/fonem, güven, onay | saniye, **son (kırpılmış) wav'a göre** |
| `kurgu.json` (= A09 "çekim listesi") | A08 kabı | izler, çapa, kilit; `goruntu` klibi A01 PlanTarifi'ni taşır; A09'dan tohum, ışık ön ayarı, efekt | 30 fps tam kare (tek yuvarlama kuralı) |
| `render_manifest.json` | A09 | girdi özetleri, ayarlar, parçalar, sonuç | dosya/parça |

### 2.2 Çakışmalar ve çözümler
| ID | Çakışma | Çözüm |
|---|---|---|
| S1 | **Kimlik üç ayrı:** A07 dosya anahtarı = ClipKey (ses+ton+okunan metin, `StoryLibrary.cs:200-202`); A08 "kalıcı satir_id" istiyor ama metin biçiminde kimlik yok; A09 hikâye özeti. İki özdeş replik aynı ClipKey; metin/ses/ton değişince anahtar değişir, A08'in "kullanıcı düzenlemesi kaybolmaz" vaadi bozulur. | Varlık = ClipKey; tekrar = (ClipKey, kaçıncı geçiş); metin değişince eski-yeni satır farkıyla çapa taşıma, taşınamayanı listele. Yalnız ekran metni değişirse ClipKey aynı kalabilir: ayrıca metin özeti şart. Kısmen çözülür; algoritma tasarlanmalı. |
| S2 | **Kırpma/iç boşluk:** A07 "çalışma anında ses değiştirilmesin"; A08 "boşluk tek yerden"; kod 0,5 sn üstü duygusal duraklamaları her oynatmada kısaltıyor. | İşlem üretim anında bir kez (EXE'de "içe aktarma" yok; ses `Documents`'a yazılıyor), iç duraklama korunabilir seçenekle. Sonra hizalama. |
| S3 | **Kimsenin yazmadığı sorun:** `Clean` her repliği aynı RMS'e (0,12; kazanç 0,5-4x) getiriyor (`ProcAudio.cs:78-80`); fısıltı bağırışla yakın düzeye çıkar. A07 "replikler eşit düzeye" önerisi de aynı riski taşır. | Satır kazancı duyguya göre hedeflensin; `kurgu.json` diyalog klibinde `kazanc` (A08'de alan var). Dinleme testi. |
| S4 | **NN yedeği:** A07 "kaldır". `TopluSes` yalnız NN yazdığı için mevcut sesler bir anda "eksik" olur; ücretsiz kotayla yeniden üretim gerekir. | Önce geçiş: dinle-onayla, NN→ClipKey adlandır; sonra yedeği kapat. |
| S5 | **J/L ve bindirme:** A08 ister; bugün tek `voiceSrc`, `Say` bitince altyazı ve bütün yüzler susturuluyor (`StoryPlayer.cs:1063-1069`), `YuzSahne.Konus` tek konuşan varsayıyor (`YuzSahne.cs:59-87`). A07 bunu yalnız not ediyor. | En az iki ses kaynağı, oyuncu başına ağız izi, altyazı izi bağımsız. K2'den önce yapılmalı. |
| S6 | **Parçalı render (A09) ↔ kesintisiz müzik/J-cut (A08):** mekân sınırında yeni ambiyans 0,3-0,8 sn önden giriyor; parça sınırını aşar. | Görüntü parça parça sessiz render edilsin; ses `kurgu.json`'dan tek parça ayrıca mikslensin. Bu, `AudioRenderer` yolundan farklı bir mimari karar: **çözülemedi, prototip gerekir.** |
| S7 | **Saat:** A07 altyazı için `timeSamples`; A08/A09 kare tabanlı belirlenimci saat. | Kayıtta kare saati esas; `timeSamples` yalnız sapma ölçümü. A00 da uyumu doğrulanmadı diyor. |
| S8 | **Ses düzeyi çift işlem:** A07/A09/A00 ffmpeg `loudnorm`; mevcut `SesOlcer` önceki kayıttan öğrenen kazanç uyguluyor (`SesOlcer.cs:45, 143`). | loudnorm gelirse öğrenen kazanç kapatılsın. |
| S9 | **Fonem → viseme boşluğu:** A07 tablosu **harf** tabanlı; MFA Türkçe sözlüğü IPA fonem verir (d̪, c, ɟ, ʎ, ɫ, ɾ, ɲ, ŋ, ʒ, ɯ, ɨ, ø, ʏ, uzun biçimler; CC BY 4.0) [W1]. parseh/whisper fonem vermez. | IPA→viseme tablosu ayrıca yazılmalı; kelime düzeyi araçta harf dağılımı yedek (hata ölçülmedi). |
| S10 | **Olay işaretleri:** A08 R12 ve A07 foley "adım, darbe" olayı istiyor; A03 Hareket Kataloğu alanlarında **yok**. | Kataloğa `olaylar[]` (ad, klip içi zaman) alanı. |
| S11 | **Üst gövde jesti:** A03 yürürken konuşma için katman şart diyor; A08 `oyuncu` izi bunu varsayıyor; yüz (viseme+duygu) A02/A07 ile uyumlu. | Sıra: H1/H2 düzeltme → katman → tepki/J-L. A08 K0-K2 bu bağımlılığı yazmalı. |
| S12 | **Kelime zamanı bağımlılığı:** A08 R4/K2 kelime zamanı ister; A07 bunu B aşamasına (kurulum onayı) koyuyor ve güven düşükse vurguyu kapatıyor. | Hizalama yokken kesme yalnız satır sınırı ve zarf sessizliğinde; açıkça yazılsın. |
| S13 | **Sahneleme önceden bilinmiyor:** A01 geometri çözücüsü ve A08 DP, plan anındaki oyuncu konumunu ister; konumlar bugün coroutine yürürken oluşuyor. A09 "atlayarak durum kurma" doğrulanmadı. | Kayıt öncesi sessiz simülasyon geçişi ya da A03 planlayıcısının konum çizelgesi. Çözülene kadar semi-Markov DP erken. |

### 2.3 A03 için ek lisans-ses-animasyon bağı
`Karakter3D.cs:141-158` takma adlarının önemli kısmı (agla→crying, uzgun→sad_idle, yat, uyu, dusun, kork, sasir...) Mixamo klibi adlarıdır (`Karakter3DKurulum.cs:123` yorumu). ÇIKARIM: dağıtım profilinde Mixamo çıkarılırsa bu duygular "dur"a, yani (H1) konuşma jestine düşer. A12 çözümü, A03 kataloğunda CC0/MIT karşılık listesi olmadan uygulanamaz. UAL/Rocketbox'ta karşılık olup olmadığı DOĞRULANAMADI.

## 3. Lisans uzmanı rolü

### 3.1 A12'deki hata ve eksikler
| # | Konu | Sonuç / düzeltme |
|---|---|---|
| L1 | P1 "35 Mixamo" | 36 (D6). |
| L2 | P1 kapsamı yalnız `KarakterKatalogu` | `OyuncuListesi` de başvuruyor; üstelik derleme `Karakterler/Hazir`'daki **tüm** prefabları bu listeye yazıyor (`UygulamaDerle.cs:146-163`). Katalogdan silmek yetmez. |
| L3 | Mixamo animasyonları hiç anılmıyor (P1) | `MixamoAnimasyon` klasörü doluysa klipler Rocketbox/Quaternius/VRM insan prefablarına da girer. A12'nin "dağıtılabilir çekirdek" kümesi bu yüzden bugünkü prefablarla temiz sayılmaz. Klasörün diskte dolu olup olmadığı DOĞRULANAMADI. A12 T1 testine animasyon klipleri eklenmeli. |
| L4 | Sınıf tutarlılığı | Tanım: GÜVENLİ = "okunan metne göre". Tabloda "okunmadı/beyan" olan satırlar (MFA-parseh-mpoyraz, whisper.cpp/uLipSync/WhisperX, CC0 beyanları, CC0 flipbook) G işaretli. Bunlar "G (devralınan)" ya da D olmalı. uLipSync MIT'i sonradan A00-S019 okudu. |
| L5 | MFA | Satır yalnız model lisansı (CC BY 4.0); MFA yazılımı ve bağımlılıklarının EXE'de dağıtımı incelenmedi (A07 de belirtiyor). K3 = D olmalı. |
| L6 | Tabloda olmayan ama önerilen/anılanlar | Animation Rigging, BONES-SEED, Asset Store Basic Motions (A03); Google Chirp 3 HD, SALSA, Incompetech, Kenney ses, Gemini Transcribe (A07); JLPM22/MotionMatching, itsFulcrum URP Hair, Defender demo (karakteri ticari olmayan), video2mocap, RGB MoCap (modelleri ticari olmayan) (A00); Faceit, MetahumanURP, Rig Logic, GNM Head (A02); TrueTrace (A04). Bazıları genel satırlarla kısmen kapsanabilir; manifeste tek tek girmeli. |
| L7 | AI etiketi hatırlatması "eklensin" | Gemini için kodda zaten var (`StoryPlayer.cs:837`). Eksik olan ElevenLabs atfı ve dağıtım bildirimleri. |

### 3.2 Ajan önerileri ↔ A12 tablosu
| Öneri | A12 | Çelişki ve çözüm |
|---|---|---|
| A03: "lisansı temiz yol: Rocketbox + UAL + gerektiğinde Mixamo"; Aşama 3 eşleşmiş etkileşim Mixamo/UAL2 | Mixamo K1 G, K2 K, K3 R | "Temiz" ifadesi yalnız kendi videosu için doğru. Katalogda `lisans` + `dagitilabilir` alanı; Mixamo yalnız yerel profil. |
| A02: GPU alınırsa CC5 daha az riskli | CC5 K3 R (özelleştirme = Enterprise, forum düzeyi) | "Daha az riskli" teknik anlamda; lisansça ürün için değil. Yazılı teyit şart. |
| A02: MetaHuman için "HDRP daha uygun" | (A04/A09/C-003) | HDRP geri çekildi; kayıt güncel değil. |
| A07: Chatterbox/VoxCPM2/EMA "ticari aday" | D | Uyumlu sayılır ancak A07 tablosundaki MIT/Apache, eğitim verisi hakkı göstermez (C-008 açık). |
| A05/A06: Megascans, Particle Pack, mağaza ejderhası | Fab K3 K; Asset Store K3 R | Uyumlu; manifestte profil alanı. |
| A00: OVR kullanma, uLipSync yedek | R / G | Uyumlu. |

Mixamo için yaptığım arama yalnız A12'nin zaten kullandığı Adobe forum SSS'sini buldu [W2]; yeni sayfa okumadım, A12 yargısını bağımsız doğrulamış sayılmam.

## 4. Sinematografi rolü: A08 (ve A01) görsel uygunluk
| # | Sorun | Değerlendirme / öneri |
|---|---|---|
| G1 | ECU d≈0,7 m ↔ "d<1,0 yok" (A01 tablo ↔ kural 9) | Düzelt: ECU 135 mm (H 0,26 → d≈0,98 m, HESAP) ya da ECU'ya açık istisna. |
| G2 | ECU f/8 "güvenli" varsayımı | ÇIKARIM (A01 formülüyle): 100 mm, 0,7 m, f/8 net alan ≈2,4 cm; göz-burun birlikte net olmayabilir. |
| G3 | CU/ECU ve tepki planları için varlık kapısı yok | A08 riski yazıyor ama çözücü kural setinde "bu karakter hangi plana dayanır" alanı yok. Rocketbox ~10 bin üçgen, 2K doku, yalnız çene (A02, D7). Öneri: karakter başına `en_yakin_plan` ve `ifade_var` sert eleme; yüz testi geçene kadar ECU kapalı, CU kısa. |
| G4 | Alan derinliği varsayımı | A01 tablosu diyafram veriyor ve OTS'de bulanık omuz istiyor; EXE'de DoF shader'ı derlemeden atılıyor (A09 K3; `DURUM.md` gözlemi) ve mevcut Gaussian ön planı bulanıklaştırmaz (A01-S001). EXE'de plan dili uygulanamaz; DoF düzeltmesi PlanTarifi'den önce. |
| G5 | Yakın planda çözünürlük | 2D arka planda 1,6x dijital büyütme (A09 K5) ve D11. Yeni ölçekler gerçek kamerayla yapılmalı, kanvas ölçeğiyle değil. |
| G6 | Ritim ↔ içerik | HESAP: 61-75 sn, ASL 2,3-3 sn → yaklaşık 20-33 plan; 10-16 replikle çoğu replik bölünür. Bölme tepki/el/nesne planı ister; tepki animasyonu ve nesne detayı yoksa aynı iki yüz arasında gidip gelir. Öneri: `ifade/jest yok` durumunda ASL hedefi gevşesin (3,5-4 sn, A08 aralığının üst ucu). |
| G7 | Dar mekân | A01: iç mekân 5,2 m; EWS d≥6 m. "Gösteri/coğrafya için zorunlu geniş plan" (A01 Ç1, C-006) bu setlerde yapılamaz: A05'e bağımlı. |
| G8 | Güvenli alan ↔ altyazı ↔ yüz | A01 yüz y≤0,66; reklam ölçümü alttan %34'ü kapalı sayıyor (yani 0,66 altı yok); uygulama %20 (`StoryPlayer.cs:967-969`); A00/BBC altyazıyı alt üçte bir civarına koyuyor. %34 geçerliyse altyazıya yüz bölgesi dışında yer kalmıyor. **Çözülemedi:** telefonda test (C-007); o zamana kadar altyazı bandı plan çözücüsünde sert yasak bölge olsun. |
| G9 | Süre sınırı | A01 "en çok 6", A08 "6 uyarı, 8 hareketle". A08'inki kabul edilebilir; "hareket" ölçütü tanımlanmalı (kamera push-in sayılmasın, ÇIKARIM). |
| G10 | Sert kural "ses ≤0,3 sn" | Bugün 0,35+1,7 sn (D9). Uyumlu bir hedef, K0'da yapılabilir; süre ölçülmeli. |
| G11 | "Cheat" oyuncu kaydırma (A01) ↔ süreklilik ve ayak kayması (A08 R6, A03) | Kaydırma yalnız kesme anında ve ayak görünmeyen planda. |

Genel yargı: A08 mimarisi (kurgu dosyası, oynatıcı yalnız uygular) sağlam bir ÇIKARIM; fakat kural ağırlıkları, J/L süreleri, tempo formülü kanıtsız (A08 de söylüyor). Semi-Markov DP, hareket/yüz/DoF/sahneleme ön koşulları olmadan görsel kazanç garantisi vermez. Önce T1 (elle kurgu A/B).

## 5. Zayıf veya kanıtsız kalan iddialar
- A08 "ortalama 2,31 sn" yalnız özetten; örneklem ilk bölümler.
- A07 "68 satır ≈ 7 gün": günlük ~10 kota resmî değil (A00-S6).
- A07 "−14 LUFS" tercih; platform kuralı değil (doğru işaretlenmiş).
- A07 hedefi "kelime başlangıcı medyan ≤50 ms, %95 ≤120 ms": Türkçe için hiçbir araçta ölçüm yok; ulaşılabilirliği bilinmiyor.
- A03 "Aşama 0 şikâyetin büyük kısmını çözer": test yok.
- A09 K3 kök nedeni hipotez.
- A12 EXE'ye Mixamo girdiği: derleme raporu incelenmedi.

## 6. Eksikler (kimse ele almamış ya da yarım)
1. Mevcut ses ve hikâye dosyalarının yeni modele geçiş (migrasyon) planı.
2. `kurgu.json` şema sürümü, deterministik yuvarlama, bozuk dosyada davranış.
3. Editör (Recorder) ve EXE yollarının aynı kurguyu aynı üretip üretmediği testi (DoF, ses, saat).
4. Mixamo'suz profil için animasyon karşılık listesi (§2.3).
5. Aynı anda iki konuşan / dinleyen sesleri için ağız ve altyazı davranışı.

## 7. Ana araştırmacıya öneri: 06'ya eklenecek çelişkiler
S1 kimlik · S6 parçalı render ↔ ses miksi (çözülemedi) · D8 A08 sessizlik hatası (çözüldü: kod) · L1-L3 A12 Mixamo (çözüldü: kod) · G1 ECU mesafesi · G4 DoF · G8 güvenli alan/altyazı (açık).
Öncelikli Unity testleri: D1 (`CurrentClip` günlüğü), D7 (Yüz Testi), build raporu (D6, L3).

## Kaynaklar (web)
- W1 [Turkish MFA dictionary v3.0.0](https://mfa-models.readthedocs.io/en/latest/dictionary/Turkish/Turkish%20MFA%20dictionary%20v3_0_0.html) — resmî model belgesi (A), erişim 2026-11-27, özet araçla okundu.
- W2 [Adobe Community: Mixamo FAQ licensing](https://community.adobe.com/t5/mixamo-discussions/mixamo-faq-licensing-royalties-ownership-eula-and-tos/td-p/13234775) — yalnız arama sonucunda görüldü, okunmadı.
- Rocketbox şekil adları için arama yaptım; yeni doğrulayıcı bilgi çıkmadı.

## Öz denetim
Kod bulguları statik okuma; hiçbir test yok. A04, A05, A06, A10, A11 tam okunmadı. A02'den yalnız arama satırları okundu. Lisans sayfaları yeniden okunmadı; lisans denetimi tablo ve proje dosyası tutarlılığıyla sınırlı. Hukuk görüşü değildir.
