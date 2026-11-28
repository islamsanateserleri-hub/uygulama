# A04 — Işık, materyal ve görüntü kalitesi

## Görev tanımı
Protokol "ALT AJAN 4": Hikâye Stüdyosu'nun 1080x1920 videolarında görüntü kalitesini yükseltecek ışık, materyal, render ve post-processing sistemini araştırmak; GPU'suz ve GPU'lu iki kademe önermek. Kod yazılmadı, proje kodu yalnız okundu. Tarih: 27 Kas 2026. Kaynaklar: `SOURCES/A04_kaynaklar.md`.

## Kısa sonuç
1. **HDRP'ye geçilmemeli.** Unity, HDRP'nin 7.0'da "deprecated" olacağını ve ilk 7 LTS sonrasında kaldırılacağını açıkladı (29 Eyl 2026) [A04-S001, S002]. Projedeki eski öneri (`Docs/VideoKalitesi.md` "HDRP'ye geçmek (ÖNERİLEN)") geçersiz.
2. Kötü görüntünün büyük bölümü URP'den değil, **kodda kapalı veya hiç kurulmamış özelliklerden** geliyor (ÇIKARIM): gölge yok, HDR yok, tonemapping yok, AO ayarlanmıyor, doku yok, düz ortam ışığı, kameraya bağlı olmayan ışık.
3. Kayıt kare kare olduğu için (sabit kare adımı) tümleşik Radeon'da **yavaş ama kaliteli** render mümkün (ÇIKARIM; ölçülmedi).

## Araştırma alanı ve soruları
- S1: Mevcut kodda görüntüyü kötü yapan teknik eksikler neler?
- S2: HDRP ile URP farkı (Unity 6.6) ve HDRP'nin geleceği?
- S3: URP'de GI, APV, volumetric, SSR durumu?
- S4: Işıklandırma tasarımı (key/fill/rim, cilt, gece/gündüz, iç/dış)?
- S5: PBR materyal, HDRI, reflection, cilt/saç?
- S6: Post-processing (tonemapping, LUT, bloom, DoF, AO, motion blur, AA, upscaling)?
- S7: Ray/path tracing ve Unity dışı render (Blender)?
- S8: GPU'suz iyileştirme yolları?

## Bulgular

### S1 — Mevcut kod (tümü ÇIKARIM: kod okundu, Unity'de çalıştırılıp denenmedi)
Yollar `Assets/HikayeStudyo/Scripts/` altındadır.

| # | Eksik | Dosya:satır | Görüntüye etkisi |
|---|---|---|---|
| K1 | Mekân parçaları gölge düşürmüyor ve almıyor | `Mekan3D.cs:237-238` | Duvar, eşya, zemin düz; derinlik yok |
| K2 | Güneş/key ışığında gölge kapalı | `Sahne3D.cs:112` | Karakter zeminde "yüzüyor", yüz hacmi zayıf |
| K3 | Tüm nokta ışıklarda gölge kapalı | `Mekan3D.cs:271`, `Mekan3D.Istasyon.cs:176,187` | Meşale/lamba ışığı duvarın ve gövdenin içinden geçiyor gibi |
| K4 | Kamera HDR kapalı | `Sahne3D.cs:100` | 1.0 üstü parlaklık kırpılır; parlak yüzeyler yumuşak sönmez, bloom zayıf |
| K5 | Volume listesinde Tonemapping, LUT, Lift/Gamma/Gain yok | `AtmosferVolume.cs:615` | Film benzeri ton eğrisi ve renk ayrımı yok |
| K6 | Bloom varsayılan 0 (yalnız "sicak_ani"/"sepya" açıyor); eşik 0.9 + LDR | `AtmosferVolume.cs:25,80,98,696-697` | Meşale, mum, büyü parlamıyor |
| K7 | Ortam ışığı tek düz renk (`AmbientMode.Flat`) | `AtmosferVolume.cs:534-539` | Üstten/alttan gelen ışık farkı yok; gölgeli taraf tek ton |
| K8 | Reflection probe, light probe/APV, gökyüzü yansıması hiç kurulmuyor (aramada bulunamadı) | proje geneli | Metal, altın, göz, cilt parlaması "ölü" |
| K9 | Gökyüzü düz renk (`CameraClearFlags.SolidColor`) | `AtmosferVolume.cs:339-340,473-477`, `Sahne3D.cs:95` | Dış mekânda gökyüzü ve gökten gelen ışık yok |
| K10 | Mekân malzemeleri dokusuz, normal map'siz, metallic hep 0 (altın taht dahil) | `Mekan3D.cs:199-221` (211); `Mekan3D.Masal.cs:90-97` | "Oyuncak/karton" görünümü |
| K11 | Geometri ilkel küp/küre/silindir | `Mekan3D.cs:223-261` | Kenar pahı yok, ışık kenarda yakalanmıyor (ortam konusu: A05) |
| K12 | Key ve fill dünyaya sabit; kamera açısına ve plana göre değişmiyor; rim/göz ışığı yok | `Sahne3D.cs:106-118`, `AtmosferVolume.cs:523-533` | Bazı planlarda yüz karanlık, karakter arka plandan ayrılmıyor |
| K13 | `isik:` rengi yalnız key'e uygulanıyor; "mum"da fill 0.15'e düşüyor, üstüne turuncu meşaleler | `AtmosferVolume.cs:312,523-529`, `Mekan3D.Masal.cs:46` | D001'deki "tek tonlu turuncu" görüntünün olası nedeni (soğuk karşı renk kalmıyor) |
| K14 | DoF yalnız Gaussian, düşük kalite örnekleme | `AtmosferVolume.cs:703-709` | Sinematik portre bulanıklığı zayıf |
| K15 | Volume/profil kodla ve reflection ile kuruluyor | `AtmosferVolume.cs:584-634` | `DURUM.md`: EXE'de DoF ve Panini shader'ları "stripped from the build". Olası neden bu; projede kalıcı Volume Profile asset yok |
| K16 | Kamera için SMAA/TAA ayarlanmıyor (yalnız `renderPostProcessing`); MSAA 4 RT'de | `AtmosferVolume.cs:636-649`, `Sahne3D.cs:91` | Saç/kirpik kenarlarında kırıklık |
| K17 | Karakter malzemesi: URP Lit, sabit smoothness 0.2, roughness/specular dokusu yok; saç alpha-clip + çift yüz | `Editor/Karakter3DKurulum.cs:515-529` | Cilt, kumaş, saç aynı türden yüzey gibi; saçta leke/kırık kenar riski |
| K18 | Atmosfer (sis) yok: `RenderSettings.fog` kullanımı aramada bulunamadı | proje geneli | Uzak/yakın ayrımı yok |

Not: D001 karelerindeki üst/alt/sağ kırmızı bantlar görüntü hatası değil, önizlemeye özel TikTok güvenli alan rehberi (`Uygulama/KayitBootstrap.cs:91` yorumu). Renk değerlendirmesi yaparken kapatılmalı.

Doğrulanamayan: `ProjectSettings` ve URP Asset repoda yok. Color space (Linear/Gamma), URP Asset HDR/shadow/SSAO ayarları cihazda okunmalı.

### S2 — HDRP mi URP mi? (en kritik bulgu)

**Unity'nin resmî açıklamaları**
- Şubat 2026: HDRP'ye yeni özellik planlanmıyor; yalnız kararlılık/hata bakımı ve Switch 2 [A04-S003, S004].
- **29 Eylül 2026** (Unity çalışanı duyurusu + resmî sayfa): HDRP **Unity 7.0'da deprecated**; ilk Unity 7 LTS (tahminen 2027 sonu) boyunca desteklenecek; "After the first 7 LTS, HDRP will be obsolete and removed." Destek en az 2029 sonu (Extended Support ile 2030). Built-In 7.0'da tamamen kaldırılıyor. Tüm çaba URP'ye [A04-S001, S002].
- URP'ye 7.x döneminde (2027) gelecekler: volumetric fog, ray tracing tabanlı direct lighting/shadow/indirect lighting. Duyuruda **skin, hair, path tracing anılmıyor** [A04-S002].

**Unity 6.6 özellik karşılaştırması** [A04-S005]

| Özellik | URP 6.6 | HDRP 6.6 |
|---|---|---|
| SSR | Hayır (6.7'de geliyor) | Evet |
| SSGI / realtime screen-space GI | Hayır | Evet |
| Adaptive Probe Volumes | Evet | Evet |
| Ambient occlusion | SSAO | GTAO + ray-traced AO |
| Volumetric fog / local volumetrics / bulut | Yok (yalnız klasik Linear/Exp sis) | Var |
| Hair, skin SSS, eye shader | **Yok** | Var |
| Physically based sky, water | Yok | Var |
| Contact shadows, planar reflection, physical light units | Yok | Var |
| Realtime area light | Yalnız baked | Var (rectangle, tube) |
| Path tracing / ray tracing | Yok | Var (DX12 + RT donanımı) |
| DoF (Bokeh/Gaussian), motion blur, bloom, color grading, lens flare | Var | Var |
| Tonemapping | Neutral, ACES [A04-S006] | Neutral, ACES, Custom |
| AA | MSAA, FXAA, SMAA, TAA | aynı + daha iyi TAA |
| Upscaling | FSR 1.0, STP | + FSR 2, DLSS, TAA upsampler |

Not: karşılaştırma tablosu özetleyici araç "volumetric fog: URP Evet" diye yazdı, ama açıklaması yalnız klasik sis modlarıydı; Unity'nin kendi yol haritası volumetric fog'u 2027'ye koyuyor [A04-S002]. Gerçek volumetric URP'de yerleşik olarak **yok** sayılmalı (ÇIKARIM).

**Değerlendirme (ÇIKARIM):** HDRP cilt, saç, göz ve volumetric için bugün daha güçlü. Ama (a) kaldırılacağı resmen açık, (b) tüm malzeme/ışık/kod geçişi haftalar alır, sonra URP'ye geri dönmek gerekir, (c) HDRP compute destekli DX11'de teknik olarak çalışabilir [A04-S011] fakat tümleşik Vega'da ağır olur ve ray/path tracing hiç çalışmaz [A04-S012]. **Karar önerisi: URP'de kal.**

### S3 — URP'de GI, APV, SSR, volumetric

- **APV** (6.6'da var): probe'ları geometri yoğunluğuna göre otomatik yerleştirir, pişirilir (bake); "Lighting Scenarios" ile gündüz/gece setleri arasında geçiş/karışım yapılır; geometri iki bake arasında değişmemeli [A04-S009].
- **Sorun:** mekânlar çalışma anında kodla üretiliyor (`Mekan3D.Kur`). Pişirilmiş GI için her mekânın Editörde sabit prefab/sahne hâline getirilip bake edilmesi gerekir (ÇIKARIM; belge çalışma anında üretilen geometriyi anlatmıyor).
- **Bake motoru:** 6.6'da Unity Compute Light Baker veya Progressive GPU önerilir; **Progressive CPU deprecated**. Progressive GPU en az 2 GB ayrılmış VRAM ve OpenCL 1.2 ister [A04-S010]. Compute Light Baker 6.6'da geldi, ray tracing donanımı varsa hızlanır [A04-S013]. Bu PC'nin tümleşik GPU'sunda çalışıp çalışmadığı DOĞRULANAMADI.
- **Unity 6.7 (21 Eyl 2026'da beta):** Surface Cache GI (bake'siz gerçek zamanlı dolaylı ışık), SSR, GTAO, **AgX tonemapping**, SSAO'nun Volume sistemine taşınması [A04-S007]. 6.7 LTS'in bugün çıkmış olup olmadığı DOĞRULANAMADI.
- **Surface Cache GI sınırları:** compute shader şart; donanım RT şart değil (yoksa compute ile, daha yavaş); yalnız opak yüzeyde dolaylı diffuse; specular yok; skinned mesh ışık alır ama yansıtmaz; çok video belleği kullanır; Static Batching kapalı olmalı [A04-S008]. Kodla üretilen mekânlara APV'den daha uygun görünüyor (ÇIKARIM; test gerekli). DX11/Vega desteği sayfada yazmıyor.
- **Volumetric (6.6):** yerleşik yok. Açık kaynak: CristianQiu "Unity-URP-Volumetric-Light" — MIT, 6000.6'ya kadar destek, render graph, main/spot/point ışık + gölge + APV, DX11'de doğrulanmış (geliştirici beyanı) [A04-S014]. Paket eklemek kullanıcı onayı ister.
- **IllusionRP** (URP için SSS, saç, SSGI, SSR): Unity 6.3 / URP 17.3 ve Windows'ta yalnız DX12; Unity Companion License [A04-S015]. 6.6/17.6 ve DX11 projeye doğrudan uygun görünmüyor (ÇIKARIM).

### S4 — Işık tasarımı

- Fill, key'in yaklaşık %20–50'si (dramatikte daha az); rim ince ve key'den güçlü olabilir (%150–250); sıcak key + soğuk fill klasik karşıtlık, abartılmamalı; düz önden ışık yüzü yassılaştırır; Rembrandt dramatik portre için [A04-S016]. Bu bir eğitim yazısı (sınıf C); oranlar başlangıç değeri, kural değil.
- URP Forward yolunda nesne başına 1 main + 8 ek ışık; daha fazlası için Forward+ [A04-S017]. Büyük duvar/zemin tek küp olduğundan meşaleli sahnelerde sınır aşılabilir (ÇIKARIM).
- Gölge ayarları: Max Distance, cascade, soft shadow kalitesi (High = 7x7), ek ışık gölge atlası; shadow map 8192'ye kadar [A04-S018, S019].

**Önerilen ışık sistemi (ÇIKARIM, test edilecek) — üç katman:**
1. **Ortam katmanı (mekâna ait):** gradient/skybox ambient + 1 reflection probe + "motivasyonlu" kaynaklar (pencere, meşale, sokak lambası). 1–2 kaynak gölgeli.
2. **Karakter katmanı (plana ait, kameraya göre):** her kesmede key (kameradan 30–60° yana, 20–45° yukarı, yumuşak gölgeli), fill (karşı taraf, key'in ~%25–40'ı, gölgesiz), rim (arkadan-üstten, ince), göz ışığı. Rendering Layers ile yalnız karakterlere [A04-S020]. Key yönü sahnedeki kaynağın tarafıyla tutarlı kalmalı; aynı sahnede plandan plana taraf değiştirmemeli (kamera ekseni: A01/A08).
3. **Look katmanı (post):** mekân+zaman+duygu başına bir Volume Profile + LUT.

| Durum | Key | Fill/ambient | Rim | Ek |
|---|---|---|---|---|
| Gündüz dış | sıcak-beyaz güneş, gölgeli | mavi gök ambient | güneş ters yönde | hafif mesafe sisi |
| Gün batımı | alçak turuncu | mor-mavi ambient | turuncu | bloom az |
| Gece dış | soğuk ay (mavi) | çok düşük koyu mavi | soğuk veya lamba sıcağı | yüz okunur kalmalı; pozlamayı değil kontrastı düşür |
| İç gündüz | pencere yönünden | oda bounce rengi | pencere kenarı | volumetric huzme (isteğe bağlı) |
| İç gece / şato | meşale-mum sıcağı | **soğuk** mavi fill (karşıtlık korunur) | soğuk ay penceresi | bloom + titreşim |
| Korku | alttan/yandan, sert | neredeyse yok | yeşil-soğuk | vignette, sis |

**Cilt ışığı:** URP'de SSS yok [A04-S005]. Yollar (ÇIKARIM): (1) yumuşak key + düşük-orta smoothness + cilde ayrı roughness; (2) Shader Graph ile wrap/pre-integrated skin (Penner, SIGGRAPH 2011 yöntemi; ekran geçişi istemez) [A04-S021]; (3) hazır URP SSS shader'ı (ücret/lisans: A12). Tam shader seçimi A02 ile ortak.

### S5 — PBR, HDRI, reflection

- **Poly Haven** (HDRI, doku, model): CC0, ticari serbest, atıf yok, paketleme serbest [A04-S022]. **ambientCG:** CC0 [A04-S023]. (İndirme yapılmadı; indirmek kullanıcı onayı ister.)
- Mekân malzemesi (ÇIKARIM): her yüzeye albedo + normal + roughness + AO; gerçek metallic; dünya koordinatlı tiling/triplanar (küpler ölçeklendiği için normal UV gerilir); kenar pahlı birkaç temel mesh.
- Reflection probe: box projection ve probe blending URP Asset'te var [A04-S019]. Realtime probe script ile `ReflectionProbe.RenderProbe` ile çekilebilir [A04-S024]; mekân kurulunca bir kez çekmek yeterli olur (ÇIKARIM).
- Rocketbox: MIT; kendi deposu Unity'de saçın "cam levha" gibi görünme ve renk çarpanı sorunlarına dikkat çekiyor ve düzeltme betiği veriyor [A04-S025]. Doku türleri/çözünürlükleri sayfada yok (cihazdaki klasörden bakılmalı).

### S6 — Post-processing

| Efekt | Öneri | Gerekçe/not |
|---|---|---|
| HDR | Açık (URP Asset + kamera); banding olursa 64-bit | [A04-S019]; kayıt çıktısı yine SDR |
| Tonemapping | 6.6'da **ACES** veya **Neutral**; 6.7'de AgX'i dene | ACES daha kontrastlı, renk tonunu/doygunluğu değiştirir; Neutral renklere az dokunur [A04-S006, S007]. Cilt için ikisi yan yana test (ÇIKARIM) |
| Grading | Grading Mode = HDR, LUT 32; look başına LUT | [A04-S019] |
| AO | SSAO renderer feature açık; 6.7'de GTAO | [A04-S005, S007] |
| Bloom | HDR ile eşik ~1, düşük şiddet; yalnız gerçek emissive | ÇIKARIM |
| DoF | Yakın/orta planda Bokeh, odak karakterin gözünde | URP'de Bokeh var [A04-S005]; EXE stripping riski (K15) |
| Motion blur | Çok az ya da kapalı | URP'de var [A04-S005]. Recorder "Accumulation" yalnız HDRP [A04-S026]; URP'de alt kare üretip ortalamak ayrı geliştirme (ÇIKARIM) |
| AA | MSAA 4–8x + süper örnekleme; ya da TAA tek başına | TAA, MSAA ile birlikte olmaz, ghosting yapar [A04-S027]. Sert kamera kesmelerinde TAA geçmişi sorun çıkarabilir (ÇIKARIM) |
| Upscaling | Offline kayıtta **gerek yok**; tersine büyük çizip küçült | FSR1/STP kalite artırmaz, hız içindir (ÇIKARIM) |
| Grain/vignette/CA | Az; chromatic aberration yalnız efekt anında | ÇIKARIM; yoğun grain TikTok sıkıştırmasında bozulur |
| Son işlem | ffmpeg `lut3d` + `scale` (lanczos) | [A04-S028 başlık/özet]; altyazı katmanı grade'den etkilenmemeli (ÇIKARIM) |

### S7 — Ray/path tracing ve Unity dışı render

- HDRP ray tracing: RTX 20/30 serisi veya AMD RX 6600–6900 XT (belge listesi; yeni kartlar listede yazılı değil), bazı GTX'ler fallback [A04-S012]. Bu PC'de yok. HDRP kaldırılacağı için yatırım gerekçesi değil.
- URP'de yerleşik path tracing yok [A04-S005]; RT aydınlatma 2027 yol haritasında [A04-S002]. TrueTrace gibi üçüncü taraf compute path tracer'lar var (yalnız arama başlığı görüldü; lisans/uyum DOĞRULANAMADI) [A04-S029].
- **Blender Cycles (bu PC):** Vega GPU desteği Cycles HIP'ten 4.3 itibarıyla kaldırıldı [A04-S030] → tümleşik Vega 7 ile yalnız **CPU** (tümleşik GPU'nun duyuruda ayrıca anılmadığına dikkat). Ryzen 5 5600G, Blender 3.4, BMW27 test sahnesi: ~175 sn/kare [A04-S031]. 60 sn video = 1800 kare; kare başına 175 sn olsaydı ~87 saat (ÇIKARIM; BMW27 bizim sahneye eşdeğer değil, yalnız büyüklük fikri).
- **Blender Eevee:** Blender gereksinim sayfası AMD GCN 1. nesil+ ve OpenGL 4.3 diyor [A04-S032] (sayfa eski sürüm bilgisi gösteriyor olabilir: macOS 11.2, Win 8.1). Vega 7'de Eevee kare süresi için güvenilir ölçüm bulunamadı: DOĞRULANAMADI.
- **Maliyet (ÇIKARIM):** Blender'a geçiş için karakter, animasyon, yüz blendshape, kamera, ışık ve efektlerin dışa aktarılıp yeniden kurulması gerekir. Kullanıcı şartı (videolar uygulamadan çıkmalı) otomatik Blender-arka-plan render'ıyla korunabilir ama büyük iştir. Şimdilik **önerilmez**; tek bir "kıyas karesi" testi yeterli.

### S8 — GPU'suz iyileştirme
Kayıt sabit kare adımıyla yapılıyor (`Uygulama/FfmpegPipeRecorder.cs:18,203`; `Editor/StudyoKayit.cs:310-311`), yani render yavaş olsa da video akıcı kalır (ÇIKARIM). Bu yüzden: gölge, SSAO, Bokeh DoF, volumetric, 2x süper örnekleme gibi "oyunda pahalı" şeyler offline kayıtta kabul edilebilir. Canlı önizleme için ayrı "hızlı" profil gerekir. Not: `EXE` yolu `AudioRenderer` ile ses de alıyor; çok yavaş karede senkron test edilmeli (A07/A09).

## Teknoloji kayıtları
Ortak: doğrulama tarihi 2026-11-27; **test durumu: hiçbiri denenmedi**.

| Ad · kategori | Ne yapar / nasıl | Katkı | Unity uyumu | Donanım | Maliyet · lisans | Avantaj / dezavantaj | Alternatif | Kaynak · güven |
|---|---|---|---|---|---|---|---|---|
| URP 17.6 · render pipeline | Ölçeklenebilir SRP | Ana taban | Mevcut | iGPU'da çalışıyor | Unity lisansı | geçiş yok, gelecek garantili / skin-hair-volumetric eksik | HDRP | S002,S005 · yüksek |
| HDRP · render pipeline | Üst düzey SRP | skin/hair/volumetric/RT | geçiş büyük | compute; RT için RTX/RX6000 | Unity lisansı | kalite / **deprecated** | URP | S001,S002,S011,S012 · yüksek |
| APV · baked GI | Otomatik probe ızgarası | bounce ışık | URP yerleşik | bake: compute/GPU | ücretsiz | senaryo karışımı / sabit geometri ister | SCGI | S009 · yüksek |
| Surface Cache GI · realtime GI | Yüzey önbelleği + (donanım/compute) RT | bake'siz bounce | **6.7+** | compute, çok bellek | ücretsiz | dinamik mekâna uygun / yeni, iGPU belirsiz | APV, elle fill | S007,S008 · orta |
| SSAO/GTAO · AO | Ekran uzayı temas gölgesi | derinlik | SSAO 6.6; GTAO 6.7 | düşük-orta | ücretsiz | ucuz / ekran dışını bilmez | baked AO | S005,S007 · yüksek |
| ACES/Neutral/AgX · tonemapping | HDR→SDR eğrisi | film tonu | AgX 6.7+ | düşük | ücretsiz | kolay / ACES renk kaydırır | özel LUT | S006,S007 · yüksek |
| URP Volumetric Light (CristianQiu) · fog | Ray-march sis ve huzme | atmosfer | 2022.3–6000.6 | orta-yüksek; DX11 doğrulanmış (beyan) | ücretsiz · MIT | hazır / üçüncü taraf, bakım riski | sis + huzme kartı; URP 7.x | S014 · orta |
| IllusionRP · URP eklentisi | SSS, saç, SSGI | cilt/saç | 6.3/17.3, DX12 | yüksek | Unity Companion | geniş / sürüm uyumsuz | özel shader | S015 · düşük-orta |
| Poly Haven, ambientCG · varlık | HDRI/PBR doku | gerçekçi yüzey ve ışık | her sürüm | RAM/disk | ücretsiz · CC0 | serbest / disk (C: 45 GB boş) | Fab (A12) | S022,S023 · yüksek |
| Blender Cycles/Eevee · harici render | Path tracing / raster | üst kalite potansiyeli | dışa aktarım gerekir | Vega'da Cycles yalnız CPU | ücretsiz · GPL (yazılım; genel bilgi) | kalite / çok yavaş, büyük entegrasyon | URP offline | S030–S032 · orta |

## Öneri: iki kademe

### Kademe 0 — GPU'suz (bu PC), sıra ile (ÇIKARIM)
| Sıra | İş | Yeni paket/varlık | Beklenen etki | Yük riski |
|---|---|---|---|---|
| 0 | Color space = Linear ve URP Asset ayarlarını doğrula | yok | temel doğruluk | yok |
| 1 | Gölgeleri aç (key + 1–2 ana kaynak, soft, mesafe ~10–20 m, mekân parçaları cast/receive) | yok | çok yüksek | orta |
| 2 | HDR + tonemapping + kalıcı Volume Profile asset'leri (stripping de çözülür) | yok | yüksek | düşük |
| 3 | SSAO | yok | yüksek | düşük-orta |
| 4 | Plana göre key/fill/rim/göz ışığı; sıcak-soğuk karşıtlık | yok | çok yüksek | düşük |
| 5 | Trilight/skybox ambient + mekân başına reflection probe | yok (HDRI isteğe bağlı) | orta-yüksek | düşük |
| 6 | PBR doku + gerçek metallic/roughness; cilt/saç/göz malzeme ayrımı | CC0 doku (onay) | yüksek | bellek |
| 7 | Sahne look LUT'ları, Bokeh DoF, kontrollü bloom | yok | orta-yüksek | düşük |
| 8 | Offline kalite profili: 2160x3840 çiz → 1080x1920'ye küçült, MSAA, High soft shadow | yok | orta (kenar, saç) | yüksek (süre) |
| 9 | Volumetric sis/huzme | MIT paket (onay) | sahneye göre yüksek | yüksek |
| 10 | Unity 6.7 LTS çıkınca ayrı kopyada AgX/GTAO/SSR/SCGI dene | sürüm yükseltme (onay) | orta-yüksek | uyum |

Beklenti (ÇIKARIM): 1–5 ile "düz oyuncak" görünümü belirgin düzelmeli; örnek AI videosu seviyesine tek başına ulaşmaz. Karakter, ortam geometrisi ve animasyon da sınırlayıcı.

### Kademe 1 — ayrı GPU ile
- Yine **URP**. Aynı sistem; 2x süper örnekleme, 4096–8192 gölge, Bokeh yüksek kalite, volumetric rahat çalışır.
- 6.7 LTS: SCGI (RT donanımıyla hızlı), SSR, GTAO, AgX.
- 2027 (Unity 7.x): URP volumetric fog ve RT aydınlatma [A04-S002] — takip et, bugünden bağımlı olma.
- Cilt/saç/göz için özel URP shader yatırımı (A02).
- İsteğe bağlı Blender Cycles GPU "hero plan" denemesi.
- Kart seçimi: URP RT yol haritası ve Blender için donanım RT'li kart (belirli model/fiyat A10'un konusu). 12–16 GB VRAM tercih (ÇIKARIM).
- **HDRP için kart almak gerekçe değildir.**

## Karşılaştırma ve çelişkiler
- **Ç1:** `Docs/VideoKalitesi.md` (26 Kas, "2. rapor" §3) HDRP'ye geçişi öneriyor ↔ Unity 29 Eyl 2026 duyurusu HDRP'yi deprecated ilan ediyor [A04-S002]. Çözüm: resmî duyuru geçerli; eski öneri geri çekilmeli. `06_CONTRADICTIONS.md` ve karar kaydına ana araştırmacı eklesin.
- **Ç2:** Aynı belge "Saç, SSS, volumetric, path tracing yalnız HDRP'de" diyor: 6.6 için doğru [A04-S005]; ama URP volumetric ve RT 2027'de planlı.
- **Ç3:** 6.6 karşılaştırma tablosu SSR için "URP: Hayır"; 6.7 duyurusu "Evet" — sürüm farkı, çelişki değil.
- **Ç4:** Pazarlama/blog yazılarındaki "HDRP is dead" başlıkları abartılı: HDRP 2029'a kadar desteklenecek; ama yeni projede seçmemek doğru.

## Riskler
| ID | Risk | Olasılık/Etki | Önlem |
|---|---|---|---|
| A04-R1 | HDRP'ye geçilirse kaldırılan pipeline'a yatırım | — / yüksek | URP'de kal |
| A04-R2 | Gölge+SSAO+süper örnekleme ile kayıt süresi veya iGPU sürücü çökmesi | orta/orta | kademeli aç, süre ölç |
| A04-R3 | EXE'de shader stripping (DoF vb.) | gözlendi/orta | kalıcı profil asset'i, EXE'de kare testi |
| A04-R4 | URP yol haritası gecikir | orta/orta | bugünkü özelliklerle tasarla |
| A04-R5 | Üçüncü taraf volumetric paket bakımsız kalır | orta/düşük | isteğe bağlı katman |
| A04-R6 | C: diskte 45 GB: doku, HDRI, Unity 6.7 | orta/orta | 2K doku sınırı, A10 |
| A04-R7 | Kapalı mekânda ışık gölgeye alınınca "Zarf" tavanı güneşi keser, sahne kararır | yüksek/orta | iç mekânda directional yerine spot/area-benzeri kaynak veya tavan gölge ayarı |

## Test önerileri
Her test aynı hikâye, aynı kare, güvenli alan rehberi kapalı; kare süresi ve tepe bellek ölçülür.

**Bu bilgisayarda**
- T1: Mevcut kare (taban) → gölge → +HDR/ACES → +SSAO → +3 nokta ışık. Her adımda aynı kare, yan yana.
- T2: ACES ve Neutral (6.7'de AgX) cilt tonu kıyası, 3 farklı ten.
- T3: 1x+MSAA4 / 1x+SMAA / TAA / 2x süper örnekleme: saç kenarı ve titreme (video olarak).
- T4: Şato sahnesi: dokusuz ↔ CC0 PBR + reflection probe.
- T5: Volumetric açık/kapalı kare süresi.
- T6: EXE derlemesinde DoF/bloom/tonemapping gerçekten çalışıyor mu.
- T7: Compute Light Baker + APV bir mekânda tümleşik GPU'da bitiyor mu, süre.
- T8: Aynı karakter portresini Blender Eevee ve Cycles CPU'da 1080x1920 render süresi (Blender kurulumu kullanıcı onayı).
- Başarı ölçütü: yüz her planda okunur; karakter arka plandan ayrılır; zemin teması görünür; parlaklık kırpılması az; 60 sn video kaydı kabul edilebilir sürede (sınırı kullanıcı belirlemeli).

**GPU isteyen:** SCGI performansı, 6.7 SSR+GTAO tam kalite, Cycles GPU kıyası, ileride URP RT.

## Öz denetim
- Hiçbir test yapılmadı; S1 maddeleri kod okumasına dayanıyor.
- URP Asset/Project Settings görülmedi; SSAO renderer feature veya Linear color space zaten açık olabilir.
- Resmî sayfalar özetleyici web aracıyla okundu; karşılaştırma tablosunun bir bölümü özetlenmiş. Kritik HDRP bulgusu iki ayrı resmî sayfayla doğrulandı.
- Bağımsız test/kullanıcı deneyimi (benchmark) zayıf: URP özelliklerinin Vega 7'deki maliyeti için kaynak bulunamadı.
- Engellenen sayfalar (tekrar denenmedi): discussions.unity.com "Render Pipelines strategy 2026" print sayfası; docs.blender.org GPU rendering 5.1; projects.blender.org (commit ve #128253); forum.blender.org 5.0 gereksinimleri; videocardz Blender 5.0 haberi.
- Işık oranları için akademik/GDC kaynağı incelenmedi; yalnız bir eğitim yazısı (C).
- "Vega 7": Ryzen 5 5600GT'nin tümleşik GPU'sunun Vega mimarisi olduğu genel bilgiye dayanıyor; proje dosyası yalnız "tümleşik AMD Radeon" diyor. Cihazdan doğrulanmalı (S030 sonucu buna bağlı).
- Yalnız arama başlığı görülen kaynaklar: S020, S021, S024, S028, S029.

## Eksik alanlar / açık sorular
1. Projenin color space'i ve URP Asset/Renderer ayarları? (cihazdan)
2. Unity 6.7 LTS yayımlandı mı; bu iGPU + DX11'de SCGI/SSR çalışıyor mu?
3. Compute Light Baker tümleşik Radeon'da çalışıyor mu?
4. Vega 7'de Eevee kare süresi?
5. Rocketbox dokularında specular/roughness var mı, çözünürlük?
6. Kullanıcı bir dakikalık video için en çok ne kadar render süresini kabul eder?
7. URP için ticari kullanılabilir, bakımlı skin/hair shader hangisi?

## Diğer ajanlardan beklenen / ortak konular
- **A09:** HDRP kararı, 6.7 yükseltme, URP Asset profilleri (önizleme/kayıt), süper örneklemeli kayıt yolu, shader stripping.
- **A02:** cilt/göz/saç shader, Rocketbox dokuları.
- **A01/A08:** plana göre ışık rig'i kamera yönetmeninden plan bilgisini almalı; DoF odak mesafesi.
- **A05:** ilkel geometri yerine modüler mekân; bake edilebilir prefab mekân.
- **A06:** parıltı/ateş emissive + HDR bloom ve sahne ışığına bağlanmalı (D001 düz yıldızlar).
- **A10:** GPU modeli, disk, kayıt süresi.
- **A12:** CC0, MIT, Unity Companion lisansları.
- **A11:** URP 2027 RT/volumetric, Surface Cache GI takibi.

## Öneriler (kanıtla)
1. URP'de kal; HDRP planını kapat [A04-S001, S002].
2. Önce kodda eksik olan temelleri aç (gölge, HDR, tonemapping, AO, plana göre ışık) — yeni donanım ve paket istemez (ÇIKARIM; K1–K7, K12).
3. Mekân malzemelerini CC0 PBR + reflection probe ile yükselt [A04-S022, S023].
4. Offline kayıt için ayrı yüksek kalite profili kur (süper örnekleme) (ÇIKARIM).
5. 6.7 LTS'i ayrı kopyada dene (AgX, GTAO, SSR, SCGI) [A04-S007, S008].
6. Blender/ray tracing'i ana yol yapma; yalnız kıyas testi [A04-S030, S031].
