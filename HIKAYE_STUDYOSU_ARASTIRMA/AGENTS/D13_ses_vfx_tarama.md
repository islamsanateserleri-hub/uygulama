# D13 — Bedava ses/müzik, VFX, telefonla tarama, Türkçe yazı tipi (dünya taraması)

Tarih: 2026-11-28 · Yöntem: yalnız web (25 arama sınırı içinde, ~24 arama + sayfa okumaları). Hiçbir şey indirilmedi, hesap açılmadı.
Önceki çalışmalarla çakışmayı önlemek için A07 §S6 (ses kaynak tablosu), A06 (Unity Labs CC0 flipbook, EmberGen fiyatı), D11 (splat temel bilgisi), D12 (TTS/mocap) **tekrarlanmadı**; yalnız yeni bulgu ve düzeltme var.

**İşaretler:** **[D]** = birincil/resmî sayfa bu turda okundu · **[İ]** = ikincil kaynak (haber, blog, forum) okundu · **[K]** = doğrulanmadı (bilgi/anı, sayfa okunamadı) · **ÇIKARIM** = benim yorumum.

---

## 0. Kısa sonuç

| Alan | En iyi bedava seçim | En büyük tehlike |
|---|---|---|
| Efekt sesi (SFX) | **Kenney ses paketleri (CC0)**, **Freesound yalnız CC0 filtresi**, **Sonniss GDC paketleri** | Freesound'daki CC0 ses başkasının şarkısına girip Content ID'ye kaydedilmiş olabilir → haksız talep; itiraz edilir |
| Müzik | **YouTube Ses Kitaplığı "Atıf gerekmez"** (YouTube için), **Pixabay kalkan simgesi OLMAYAN parçalar** | Pixabay kalkanlı parçalar Content ID'li; Kevin MacLeod parçaları atıf eksikse talep alıyor; YouTube kitaplığının TikTok'ta kullanımı resmî olarak **belirsiz** |
| TikTok müziği | TikTok uygulaması içinden ses eklemek (yalnız TikTok'ta) | TikTok Commercial Music Library **işletmeler için ve TikTok içi**; YouTube'a taşınamaz |
| VFX (Unity URP) | **Unity "Particle Pack" (Unity Technologies, ücretsiz, URP 6000.x uyumlu)**, **Kenney Smoke/Particle Pack (CC0)**, A06'daki Unity Labs CC0 flipbook'lar | Asset Store paketleri Standart EULA: kaynak dosyayı ayrıca dağıtamazsın (video çıktısı serbest) |
| LUT | Kendi LUT'unu Unity/ffmpeg'de üret; hazır paketlerde "yeniden dağıtma" yasağı var | Lisanssız "free LUT" sitelerinin çoğu ticari şartı açık yazmıyor |
| Telefonla tarama | **KIRI Engine Basic** (sınırsız dışa aktarım, ticari serbest yazıyor), **RealityScan Mobile** (ücretsiz, Epic) | **Scaniverse yeni sürümde ticari hak yalnız Pro planda** (ikincil kaynak); splat Unity'de DX11'de çalışmaz |
| Türkçe altyazı yazı tipi | **TikTok Sans (OFL 1.1, Türkçe destekli [D])**, Noto Sans / Montserrat / Inter (OFL) [K] | C# `ToUpper()` Türkçe `i→İ` hatası; TMP atlasına Türkçe harfleri koymayı unutmak |

---

## 1. Bedava ses efekti ve müzik (para kazanan TikTok/YouTube için)

### 1.1 Kaynak kaynak tablo

| Kaynak | Lisans / şart | Para kazanılan videoda | Atıf | Content ID riski | Kanıt |
|---|---|---|---|---|---|
| **YouTube Ses Kitaplığı** (Studio içinde) | YouTube kendi lisansı; bazı parçalar CC-BY | YouTube'da **evet**: "you can monetize videos with music and sound effects from the Audio Library" | Standart lisans: yok ("Attribution not required" filtresi). CC parçalar: açıklamaya atıf **zorunlu** | YouTube: "Copyright-safe music … downloaded from the Audio Library won't be claimed" | [D] https://support.google.com/youtube/answer/3376882 (2026-11-28 okundu) |
| ↳ YouTube dışı (TikTok) | Resmî sayfa **açıkça izin vermiyor, yasaklamıyor da**; "can't give legal guidance … off the platform" | **Belirsiz** | — | TikTok'un kendi eşleştirmesi ayrı çalışır | [D] aynı sayfa; [İ] digitalapplied 2026-09-13: "taşınabilir varsayma, parça lisansını incele" https://www.digitalapplied.com/blog/social-video-music-rights-platform-comparison |
| **Pixabay müzik/ses** | Pixabay İçerik Lisansı: ticari kullanım serbest, **müzik daha büyük bir eserin parçası olmalı**; tek başına satış/dağıtım yasak | Evet | Gerekmez | **Var.** "Some contributors register tracks with Content ID … automated claims even when the track is used legally." **Kalkan simgeli** parçalar Content ID'li. Yalnız müzik + sabit görsel videolar "daha çok işaretlenir". Reddedilen itirazı tekrar temyiz etmek **telif ihtarına** dönüşebilir | [D] https://pixabay.com/service/faq/ |
| **Freesound** | CC0 / CC-BY / CC-BY-NC (NC = ticari yasak) | Yalnız CC0 ve BY | BY'de evet | **Var (dolaylı):** CC0 ses değiştirilmeden bir şarkıya konuyor, şarkı Content ID'ye giriyor, sonra sesi kullanan herkes talep alıyor. Tavsiye: CC0 kaynağını gösterip **itiraz**; sesi değiştirmek (perde/hız/katman) eşleşmeyi azaltır. Freesound SSS'sine bu konuda madde eklendi | [İ] Freesound forumu, 2019-09-14 https://freesound.org/forum/legal-help-and-attribution-questions/41930 |
| **Sonniss GDC paketleri** | "royalty-free and commercially usable", "royalty-free for a lifetime"; **ham/değiştirilmemiş halde yeniden yayımlama ve satma yasak**; bir eserin (video, müzik) içinde serbest | Evet | Gerekmez (lisans PDF'i bu turda okunamadı) [K] | Düşük (ÇIKARIM: kaynak profesyonel kütüphane, Content ID'ye yüklenmiyor; ama kanıt yok) | [İ] BPB, GDC 2026 paketi 7,47 GB / 347+ WAV, 2026-03-16 https://bedroomproducersblog.com/2026/03/16/sonniss-gdc-2026-bundle/ ; [İ] Freesound forumu 2020-03-10 (ham hali Freesound'a yüklenemez) https://freesound.org/forum/legal-help-and-attribution-questions/42190/94872/ |
| ↳ "AI eğitimi yasak" maddesi | A07'deki çelişki **çözülemedi**: resmî lisans PDF'i yalnız güvenilmeyen bir aynada bulundu, açılmadı | — | — | — | [K] — Bizim için önemsiz: sesleri AI eğitmiyoruz, videoya koyuyoruz |
| **Kenney ses** (Sci-fi Sounds 70 dosya, Interface Sounds, UI Audio, Impact…) | **CC0** | Evet | Gerekmez | Çok düşük (ÇIKARIM) | [D] https://kenney.nl/assets/sci-fi-sounds ; https://kenney.nl/assets/interface-sounds |
| **ZapSplat** | Ücretsiz (Basic) hesap: **atıf zorunlu** ("If you're a Basic member you have to credit us"); Gold (bağış) ile atıf kalkar | YouTube'da **evet** ("Even if you monetise the video through ads"); **TikTok yazılı değil** | Basic'te evet | "content ID claims can happen, although not often"; müzikte YouTube simgeleri var | [D] https://www.zapsplat.com/faq-category/license-and-usage/ ; https://www.zapsplat.com/youtube-safe-content-faq |
| **Incompetech / Kevin MacLeod** | CC-BY (atıf) veya ücretli atıfsız lisans | Evet (atıfla) | **Zorunlu**, açıklamaya: `"Parça Adı" by Kevin MacLeod` | **Var:** "increasingly difficult to challenge all of the false claims" — talepler çoğunlukla atıf eksik ya da atıf videonun içinde olduğu için; itiraz ~72 saatte düşüyor; toplu kaldırma formu var | [D] https://incompetech.com/music/royalty-free/youtube-contentid.html |
| **TikTok Commercial Music Library** | "free for businesses to use" — **işletme hesapları** için, TikTok'taki organik içerik/reklam/markalı içerik | Yalnız **TikTok içinde** | — | — | [D] https://ads.tiktok.com/help/article/commercial-music-library ; [İ] digitalapplied: TikTok koşulları "off-platform commercial uses require separate permission" |
| **OpenGameArt** | Parça parça değişir (CC0, CC-BY, CC-BY-SA, GPL) | CC0/BY evet | Lisansa göre | Freesound ile aynı mantık (ÇIKARIM) | Site robots nedeniyle okunamadı [K] |
| **DOVA-SYNDROME** (Japon bedava BGM; korku/gerilim çok) | Ticari/YouTube gelirli kullanım serbest olarak bilinir [K] | [K] | Gerekmez olarak bilinir [K] | Bazı besteciler Content ID kullanıyor [K] | dova-s.jp lisans sayfası **opentracks.com'a yönleniyor** (2026-11-28, isim/alan değişikliği olabilir); sayfa okunamadı [K] |
| **効果音ラボ (Sound Effect Lab, Japon SFX)** | Kredi/bağlantı gerekmez | [K] (makale ticari durumu yazmıyor) | Hayır | [K] | [İ] CyberLink JP blog, 2022-04-18 https://jp.cyberlink.com/blog/youtube/1196/youtube-sound-effects-recommended-site — **eski**, site şartı ayrıca okunmalı |
| **OtoLogic (Japon)** | Kredi **zorunlu** | [K] | Evet | [K] | [İ] aynı kaynak |

Çin/Rus kaynakları: bu turda bütçe içinde güvenilir, lisansı okunabilir bir bedava ses kütüphanesi bulunamadı (aranmadı/yetişmedi) [K].

### 1.2 Content ID tehlikeleri — düz anlatım

1. **"Telifsiz" ≠ "talep almaz."** Pixabay ve Kevin MacLeod için resmî sayfalar bile yasal kullanımda otomatik talep olabileceğini söylüyor [D].
2. **Talep (claim) ≠ ihtar (strike).** Talep gelirini/görünürlüğünü etkiler; ihtar kanalı tehlikeye atar. Pixabay: reddedilen itirazı ısrarla temyiz etmek ihtara gidebilir [D].
3. **En güvenli müzik zinciri (ÇIKARIM):**
   - YouTube Shorts → YouTube Ses Kitaplığı "Atıf gerekmez" parçaları.
   - TikTok → TikTok uygulaması içinden müzik eklemek (videoyu müziksiz yükle, müziği TikTok'ta seç) ya da CC0 / kendi ürettiğin müzik.
   - İki platforma **aynı dosya** gidecekse → yalnız CC0 (Kenney, Freesound CC0) veya Pixabay'ın **kalkansız** parçaları, ya da kendi yaptığın müzik.
4. **Ses efektleri müziğe göre çok daha az risklidir** (ÇIKARIM): kısa, katmanlı, işlenmiş SFX'in eşleşmesi zor. Riski asıl **müzik yatağı** taşır.
5. **Kayıt defteri tut:** her dosya için kaynak URL, lisans, indirme tarihi, (gerekiyorsa) atıf metni, Pixabay'da "kalkan var mı". A07 de bunu istiyordu; uygulamada `lisans.json` gibi bir dosyaya otomatik yazılabilir (ÇIKARIM).
6. **Hazır atıf metni** (Kevin MacLeod örneği, açıklamaya): `"Parça" by Kevin MacLeod (incompetech.com), Licensed under Creative Commons: By Attribution 4.0` [K: tam format incompetech SSS'de; burada SSS okunmadı].

---

## 2. Unity URP için bedava sinematik VFX (zayıf ekran kartı)

| Kaynak | Ne | Lisans | URP / sürüm | Zayıf GPU | Kanıt |
|---|---|---|---|---|---|
| **Unity "Particle Pack"** (Unity Technologies) | Ateş, patlama, duman vb. Shuriken parçacıkları, 160 MB, sürüm 3.3 | **Ücretsiz**, Standart Unity Asset Store EULA ("Extension Asset") | **URP: 6000.3, 6000.0, 2022.3**; Built-in: 2021.3; HDRP yok | Shuriken (CPU) parçacıkları → iGPU'da uygun (ÇIKARIM) | [D] https://assetstore.unity.com/packages/p/particle-pack-127325 |
| Unity "Shader Graph Examples – VFX Sample Project" | Shader Graph ile VFX örnekleri | Ücretsiz [K] | [K] | VFX Graph kullanıyorsa compute ister (ÇIKARIM) | Arama sonucu, sayfa okunmadı [K] https://assetstore.unity.com/packages/essentials/tutorial-projects/shader-graph-examples-vfx-sample-project-372864 |
| **Kenney Smoke Particles** | 2D duman/patlama sprite'ları, 70 dosya | **CC0** | Her boru hattı (doku) | Çok hafif | [D] https://kenney.nl/assets/smoke-particles |
| **Kenney Particle Pack** | 80+ parçacık sprite'ı (kıvılcım, ışık, duman) | CC0 [K: sayfa açıldı ama lisans satırı alınmadı; Kenney genelde CC0] | Her boru hattı | Çok hafif | [İ] https://kenney.nl/assets/particle-pack |
| **JangaFX ücretsiz VDB'leri** | 6 adet ateş/duman/patlama **akışkan simülasyonu (VDB hacim)** | **CC0** | Doğrudan Unity'ye girmez; Blender'da flipbook'a çevrilmeli (ÇIKARIM) | Çevrildikten sonra hafif | [İ] CG Channel, 2020-03-07 https://www.cgchannel.com/?p=113054 |
| JangaFX "free VFX asset pack" (OpenGameArt'ta flipbook paketi) | Flipbook sprite'ları olduğu düşünülüyor | [K] | — | — | Sayfa robots nedeniyle okunamadı [K] https://opengameart.org/content/free-vfx-asset-pack |
| itch.io "open source VFX" / CC0 koleksiyonları (ör. Brackeys VFX Bundle) | Stilize flipbook'lar | Paket paket değişir [K] | — | Hafif | [İ] https://itch.io/c/4909438/open-source-vfx ; https://brackeysgames.itch.io/brackeys-vfx-bundle |
| Poly Haven | HDRI/doku/model; **flipbook/VFX yok** (bilgi) [K] | CC0 | — | — | — |

**Zayıf GPU için öneri (ÇIKARIM):**
- Sinematik his için **Shuriken + flipbook + yumuşak parçacık (soft particles) kapalı + az sayıda büyük parçacık** daha iyidir; VFX Graph compute ister, iGPU'da gereksiz.
- **Video kaydı sabit kare hızıyla, gerçek zamanlı olmadan alındığı** için (Recorder) kayıt sırasında FPS düşmesi videoyu bozmaz; asıl sınır **bellek** (doku çözünürlüğü). Flipbook'ları 2048 yerine 1024 tutmak yeter.
- A06'daki **Unity Labs CC0 flipbook'ları** + Kenney CC0 + Unity Particle Pack üçlüsü, lisans dertsiz başlangıç seti için yeterli.
- Asset Store EULA: paketi kullanıp video yayınlamak serbest; **paketin dosyalarını ayrıca paylaşmak** (ör. projeyi açık kaynak yapmak) yasak olabilir (A12 ile uyumlu, ÇIKARIM).

### 2.1 LUT (renk tablosu)

| Kaynak | Şart | Kanıt |
|---|---|---|
| PremiumBeat "13 FREE Cinematic LUTs" | Tek yazılı şart: "not to resell or redistribute"; ticari/YouTube kullanımı **açıkça yazmıyor**; 2021 | [İ] https://www.premiumbeat.com/blog/download-free-cinematic-luts/ |
| freevisuals.net, Filmora vb. "free LUT" sayfaları | Lisans satırı belirsiz [K] | Arama sonucu |
| **Kendi LUT'unu üret** (önerilen) | Lisans derdi yok | ÇIKARIM: Unity'de bir sahneyi Color Adjustments/Curves ile ayarla, nötr LUT şeridine uygulayıp kaydet; ya da ffmpeg `lut3d` ile (A04) |

- URP **Color Lookup** override'ı **2D şerit doku** (ör. 1024×32) bekler; `.cube` dosyası doğrudan kullanılamaz, şeride çevrilmesi gerekir [K: bilgi, bu turda doğrulanmadı]. Basit bir editör betiği `.cube → şerit PNG` çevirebilir (ÇIKARIM).
- LUT, **altyazı katmanına uygulanmamalı** (A04 ile aynı).

---

## 3. Telefonla fotogrametri / 3D tarama

| Uygulama | Fiyat | İşleme | Dışa aktarım | Çıktı lisansı / ticari | Not | Kanıt |
|---|---|---|---|---|---|---|
| **Epic RealityScan Mobile** (1.8, Kasım 2025) | Ücretsiz; Epic'in ücretsiz lisansı "<1 milyon $ gelir" | Sunucuda (bulut) işleme izlenimi; kırpma cihazda | **iOS: OBJ, Android: GLB** (1.5 belgesi); Sketchfab'a paylaşım | **Bu turda okunamadı** [K]. Epic hesabı gerekir | 1.8: AR rehberi, Object Mode (otomatik arka plan silme), mesh temizleme, su geçirmez/geçirmez mesh seçimi, odak tepe (focus peaking), zamanlayıcı, sürekli flaş. Bilinen hata: iOS'ta "Processing"de takılma | [D] https://dev.epicgames.com/documentation/realityscan-mobile/realityscan-1-5-version ; [İ] https://digitalproduction.com/2025/11/21/realityscan-mobile-1-8-with-tools-for-on-the-go-scanning/ ; [İ] https://cdm.link/epic-free-reality-scan/ (2025-07-17) |
| ↳ RealityScan masaüstü 2.0 | Ücretsiz (<1M $) | **Windows + NVIDIA CUDA şart** | — | — | Zayıf PC'de büyük olasılıkla çalışmaz (NVIDIA yoksa) | [İ] cdm.link 2025-07-17 |
| ↳ Türkiye'de çalışıyor mu? | — | — | — | — | Play Store/App Store Türkiye erişimi **doğrulanmadı** [K] | — |
| **KIRI Engine Basic** (ücretsiz) | 0 TL; Pro 17,99 $/ay veya 79,99 $/yıl | Bulut | **Sınırsız**, filigransız: OBJ, STL, GLTF, FBX, USDZ | KIRI: "You can use exported models from the Basic plan for personal or commercial projects." | Photo Scan yükleme başına **150 foto**; **3DGS (splat) yalnız Pro** | [İ/üretici blogu] 2026-06-04 https://www.kiriengine.app/blog/kiri-engine-basic-vs-pro |
| **Polycam** ücretsiz | Ücretsiz katman | Bulut (Photo mode) | **GLTF/GLB ücretsiz** (doku gömülü); iOS 360 foto filigranlı | **Yazmıyor** [K] | Photo mode'da **sınırlı sayıda** tarama (sayı yazmıyor), görevle artırılabiliyor; LiDAR/Room modu LiDAR'lı iPhone'da ücretsiz | [D] https://learn.poly.cam/product-faqs/faq/can-i-use-polycam-for-free |
| **Scaniverse Classic** (Niantic Spatial) | **Ücretsiz** | **Cihaz üstünde**, internet gerekmez, çoğu tarama <90 sn | PLY/SPZ splat, mesh (sürüme göre) | **Belirsiz** [K] | — | [İ] https://radiancefields.com/platforms/scaniverse |
| **Scaniverse (yeni, bulut)** | Free 20.000 kredi; Plus 20 $/ay, Pro 50 $/ay | Bulut (Android'de Mayıs 2026'dan beri) | FBX mesh, PLY/SPZ splat; USDZ (Temmuz 2026) Plus/Pro | **"Ticari haklar Pro planla gelir"** (ikincil kaynak) | Yeni hesap gerekir | [İ] radiancefields (aynı sayfa); https://radiancefields.com/scaniverse-cloud-comes-to-android (2026-05-12) |

**Tarama için pratik (ÇIKARIM):**
- **Prop (eşya)**: kafatası, eski telefon, oyuncak bebek, kilitli kutu… → **KIRI Basic** (ticari izin yazılı, FBX/GLTF) ya da RealityScan Mobile. Unity'ye GLB/GLTF için glTFast zaten yansımayla kullanılıyor (CLAUDE.md).
- **Oda**: fotogrametri mesh'i dağınık çıkar; dikey video için **tek açıdan arka plan** olarak splat ya da camera-projection (D11 C seçeneği) daha iyi sonuç verir.
- **İnsan**: telefonla taranan insan **rig'siz statik heykel** çıkar; konuşan karakter olmaz. Ayrıca taranan kişinin **açık izni** gerekir (kişilik hakkı) — kendin ya da yazılı izin veren biri dışında kimseyi tarama.
- Taranan mesh'ler genelde 100k–1M üçgen; Unity'ye almadan Blender'da **Decimate** ile 20–50k'ya indir (zayıf PC).
- Telif: başkasının tasarımı olan ürün/heykel/marka logosu taranırsa video yine sorun çıkarabilir; jenerik eşyalar seç.

### 3.1 Gaussian splat'ı Unity'de arka plan olarak kullanmak

| Konu | Bulgu | Kanıt |
|---|---|---|
| Paket | aras-p **UnityGaussianSplatting**, kod **MIT** | [İ: README aynası] https://awesome.ecosyste.ms/projects/github.com%2Faras-p%2FUnityGaussianSplatting |
| Grafik API | Windows'ta **D3D12 veya Vulkan şart, DX11 çalışmaz** (D11 ile aynı); OpenGL/WebGPU yok | aynı |
| Girdi | **PLY ve Scaniverse SPZ** dosyaları içe aktarılır; sıkıştırma seviyeleri seçilebilir, "very low" bile "decently usable", bir örnek <8 MB | aynı |
| Bellek | Varlığın kendisine ek **~48 bayt/splat GPU belleği** (sıralama + önbellek) | aynı (D11'deki ~104 B/splat tüm veri için) |
| Hız | 6,1 M splat, RTX 3080 Ti: 6,8 ms; M1 Max: 21,5 ms. **Entegre GPU ölçümü yok** | aynı |
| Bakım | "As of 2023 December: I'm not planning any significant further developments"; son itme 2025-02-05 | aynı |
| **Lisans tuzağı** | README: orijinal INRIA 3DGS **eğitim** yazılımı akademik/ticari olmayan lisanslı; o yazılımla eğitilmiş splat'ları ticari kullanmak için INRIA lisansı gerekir | aynı |

**Yorum (ÇIKARIM):**
- Telefon uygulamaları (Scaniverse, KIRI, Polycam) splat'ı **kendi** altyapılarında üretir; INRIA kısıtı onlara değil, **senin kendi PC'nde orijinal INRIA koduyla eğittiğin** splat'lara uygulanır. Uygulama çıktısında geçerli olan **uygulamanın kendi şartıdır** → Scaniverse yeni sürümde ticari hak Pro'da; KIRI'de splat zaten Pro. Yani **bedava + ticari + splat** üçlüsü şu an temiz değil; Scaniverse Classic'in şartı okunmalı.
- **Zayıf PC'de arka plan olarak:** kayıt sabit kare hızıyla yapıldığı için FPS önemsiz; önemli olan bellek. 300k–1M splat + "very low/low" sıkıştırma ≈ onlarca MB → iGPU'nun paylaşılan belleğinde sığması beklenir. Proje DX11'deyse önce **D3D12'ye geçiş** denenmeli (diğer shader'lar/Recorder ile uyum test edilmeli).
- Splat karakterle **ışık/gölge alışverişi yapmaz** (D11). En iyi kullanım: **uzak arka plan / pencere dışı / koridor sonu** + önde normal 3D karakter + ortak LUT/grain/DoF ile kaynaştırma.
- Daha güvenli alternatif: splat'tan ya da taramadan **tek kare render alıp** D11'deki camera-projection/katmanlı kart yöntemiyle kullanmak (DX11'de çalışır, lisansı aynı uygulama şartına bağlı).

---

## 4. Türkçe destekli bedava altyazı yazı tipleri (OFL)

| Yazı tipi | Lisans | Türkçe (ğ ş ı İ ç ö ü) | Altyazı için | Kanıt |
|---|---|---|---|---|
| **TikTok Sans** | **SIL OFL 1.1** | **Evet** — 460+ dil, Latin/Yunan/Kiril; Türkçe listede | Değişken: Weight, Width, Slant, Optical Size; TikTok'un kendi görünümü, kısa video için ideal | [D] https://github.com/tiktok/TikTokSans ; https://developers.tiktok.com/blog/tiktok-sans-open-source ; Google Fonts: https://fonts.google.com/specimen/TikTok+Sans |
| Noto Sans (Google) | OFL | Evet [K] | Çok okunaklı, nötr | [K] |
| Montserrat | OFL | Evet [K] | Kalın ağırlıkları başlık/kanca için iyi | [K] |
| Inter | OFL | Evet [K] | Ekran okunaklılığı yüksek | [K] |
| Roboto | OFL (eskiden Apache 2.0) [K] | Evet [K] | Nötr | [K] |
| Poppins | OFL | Evet [K] | Yuvarlak, "YouTube altyazı" tarzı | [K] |
| Anton / Bebas Neue / Oswald | OFL | Kısmen/sürüme göre [K] — **mutlaka İ, ı, ğ, ş deneyin** | Korku başlığı / kanca metni | [K] |
| Creepster, Nosifer vb. korku süs fontları | OFL [K] | Çoğunda Türkçe harf **eksik** olabilir [K] | Yalnız kısa başlık, altyazıda kullanma | [K] |

**OFL ne demek (ÇIKARIM, bilinen lisans):** ücretsiz, ticari videoda serbest, atıf gerekmez; yalnız **font dosyasını tek başına satamazsın**. Videoya gömülü yazı tamamen serbest.

**Unity/TMP pratik uyarıları (ÇIKARIM):**
1. TextMeshPro font atlasında **Character Set = Custom** ve şu harfleri ekle: `ÇçĞğİıÖöŞşÜüÂâÎîÛû` + tırnaklar `“ ” ‘ ’ …`. Eksikse kare (□) çıkar.
2. **`ToUpper()` / `ToLower()` tuzağı:** C#'ta kültürsüz büyütme `i → I` yapar, Türkçede `i → İ` olmalı. Kanca/başlık metnini büyük harfe çevirirken `ToUpper(new CultureInfo("tr-TR"))` kullan. (TMP'nin "UPPERCASE" stili de kültürsüz çalışabilir [K] → metni kodda büyüt.)
3. Altyazıda **kalın (700–900) + 3–6 px kontur + hafif gölge**; TikTok Sans'ın Width ekseniyle dar ağırlık uzun Türkçe kelimelere yer açar.

---

## 5. Önceki raporlara düzeltme / ek

- **A07 §S6 Pixabay**: "bazı parçalarda Content ID riski" → artık **resmî kaynakla doğrulandı**; ayırt edici işaret: **kalkan simgesi** [D].
- **A07 §S6 Incompetech**: Content ID taleplerinin varlığı ve itiraz yolu resmî sayfayla doğrulandı [D].
- **A07 §S6 TikTok CML**: "işletmeler için" doğru; ek olarak TikTok dışı ticari kullanım için ayrı izin gerekir [D+İ].
- **Yeni:** ZapSplat ücretsiz hesapta **atıf zorunlu** [D]; YouTube Ses Kitaplığı'nın TikTok'ta kullanımı **resmî olarak belirsiz** [D].
- **D11 splat**: INRIA eğitim lisansı tuzağı ve ~48 B/splat ek GPU belleği eklendi; SPZ girişi destekleniyor [İ].
- **A07 Sonniss AI maddesi çelişkisi**: hâlâ çözülmedi [K]; bizim kullanımı etkilemez.

## 6. Açık kalanlar (doğrulanmadı)

1. RealityScan Mobile **çıktı lisansı** ve Türkiye mağaza erişimi.
2. Scaniverse **Classic** dışa aktarımlarının ticari şartı.
3. Polycam ücretsiz tarama sayısı ve ticari şartı (fiyat sayfası okunmadı).
4. Sonniss GDC lisans PDF'inin tam metni (atıf, AI maddesi).
5. DOVA-SYNDROME → opentracks.com geçişi ve güncel şartlar; 効果音ラボ güncel ticari şartı.
6. aras-p splat paketinin **entegre GPU'da** gerçek hızı/bellek davranışı (yerelde test gerekir).
7. Noto/Montserrat/Inter/Bebas vb. Türkçe glif kapsamı (bilgi olarak doğru olması beklenir, bu turda okunmadı).

## 7. Okunamayan / engellenen URL'ler

- https://www.cgchannel.com/2025/11/epic-games-releases-realityscan-mobile-1-8/ (robots)
- https://opengameart.org/content/free-vfx-asset-pack (robots)
- https://opentracks.com/en/help/articles/license-usage/ (izin isteği zaman aşımı; dova-s.jp buraya yönleniyor)
- Sonniss lisans PDF'i yalnız güvenilmeyen bir aynada çıktı, açılmadı.
