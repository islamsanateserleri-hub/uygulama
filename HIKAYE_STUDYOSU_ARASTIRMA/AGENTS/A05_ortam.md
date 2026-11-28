# A05 — 3D ortam, mimari ve çevre

> Tarih: 2026-11-27. Kaynaklar: `SOURCES/A05_kaynaklar.md` (metinde `[S0xx]` = `A05-S0xx`).
> Kod yazılmadı, hiçbir şey indirilmedi, Unity çalıştırılmadı, hiçbir test yapılmadı. WebSearch: 28/28.
> "KOD:" = kaynak kod/dosya okuması (görselde doğrulanmadı). "ÇIKARIM:" = benim yorumum ya da hesabım.

## 0. Kısa sonuç
1. Görüntüyü "oyuncak" yapan iki ortam sorunu: bütün mekânlar ilkel küp/silindir/küreden, dokusuz kuruluyor; set küçük ve tek yönlü (KOD).
2. Projede **205 adet CC0 Quaternius GLB eşyası duruyor ama mekân kodu bunları bulamıyor** gibi görünüyor: kod Türkçe ad arıyor ("yatak", "sandalye"), dosyalar İngilizce ("Bed_Single", "Chair_1") (KOD; Unity'de denenmedi).
3. Gerçekçi karakter (Rocketbox) ile low-poly eşya (Quaternius, Kenney, KayKit) aynı karede büyük olasılıkla uyumsuz durur (ÇIKARIM). Gerçekçilik için CC0 PBR malzeme (Poly Haven, ambientCG) ve taranmış varlık (Megascans, Kasım 2026'da yeniden ücretsiz [S006, S007]) gerekir.
4. Öneri: **"mekân = veri"**: az sayıda modüler kit + PBR malzeme kütüphanesi + sabit adlı noktalar + tohum/durum kaydı + 4 derinlik katmanlı dikey set. Yeni paket gerekmeden başlanabilir.
5. Lisans uyarısı: Hikâye Stüdyosu bir içerik üretme aracı. Unity Asset Store EULA 2.2.1.1(c) [S002] ve Synty EULA [S024] bu tür ürünleri kısıtlıyor; A12 incelemeden bu iki kaynaktan varlık alınmamalı.

## Görev tanımı
Protokol "ALT AJAN 5" ve §9.5: gerçekçi iç/dış mekân, mimari, orman, şehir, köy, saray, sokak, oda; materyal, terrain, bitki, atmosfer, hava; modüler kit ve procedural üretim; varlık yönetimi; mekân devamlılığı; optimizasyon; ücretsiz/ücretli varlık kaynakları ve lisansları; dikey kadraj için set tasarımı. Teslim: ekonomik, gerçekçi, yeniden kullanılabilir, genişletilebilir ortam üretim sistemi önerisi.

## Araştırma alanı ve soruları
- S1: Mevcut mekân sistemi neyi yapıyor, nerede sınırlı?
- S2: Hangi varlık kaynağı hangi lisansla, URP'de kullanılabilir mi?
- S3: Modüler kit ve procedural üretim için hangi araçlar?
- S4: Terrain, bitki, atmosfer, hava?
- S5: Varlıklar adla ve gerektiğinde nasıl yüklenir?
- S6: Tümleşik GPU'da ortam optimizasyonu?
- S7: Dikey kadraj set tasarımı ve mekân devamlılığı?
- S8: Yapay zekâ destekli 3D/ortam araçları (video üretici DEĞİL)?

## Bulgular

### S1 — Mevcut sistem (KOD)
Yollar `Assets/HikayeStudyo/Scripts/`.

17 mekân: oda, ev, mutfak, yatak_odasi, hastane, sokak, koy, park, araba, kopru, koridor, hava_kilidi, zindan, sato, magara, mezarlik, orman.

| # | Bulgu | Yer | Etki (ÇIKARIM) |
|---|---|---|---|
| M1 | Her parça `GameObject.CreatePrimitive` (küp, silindir, küre) + ölçek | `Mekan3D.cs:223-261` | Pahsız kenar, kutu mimari; ağaç = silindir + küre |
| M2 | Malzeme yalnız renk + smoothness; doku yok, metallic 0 | `Mekan3D.cs:199-221` | Taş, ahşap, kumaş, altın aynı görünür |
| M3 | Gölge atma/alma kapalı | `Mekan3D.cs:237-238` (A04 K1) | Derinlik yok |
| M4 | İç mekân `Zarf`: 5,2–6 m en; arka duvar z≈3,1–3,6; yan duvarlar z=-8'e uzanır; **ön duvar yok** | `Mekan3D.cs` (Zarf), `Mekan3D.Masal.cs` | Tek yönlü "tiyatro dekoru"; gerçek ters açı (180°) çekilemez |
| M5 | Kamera x sınırı sabit ±2,35 m (5,2 m'lik odaya göre) | `KameraYonetmen.cs:370-377` | Büyük set eklenirse sınır mekân verisinden gelmeli |
| M6 | Dış mekân: 40x40 m düz levha; tepe = dev küre; gökyüzü düz renk | `Mekan3D*.cs`; A04 K9 | Ufuk ve uzak katman zayıf |
| M7 | `PropPrefab` Türkçe anahtarla (`yatak`, `masa`, `sandalye`, `lamba`, `bank`, `araba`, `agac`) `FindAssets` + ad `Contains` arıyor. `Karakterler/Esyalar` altında 205 GLB (Ev 123, Koy 44, Sokak 25, Doga 13) İngilizce adlı; eşleşen dosya yok | `Mekan3D.cs:286-324` | Hazır CC0 eşyalar kullanılmıyor, ilkel şekle düşülüyor |
| M8 | Editörde `AssetDatabase`, EXE'de `Resources.Load("Esyalar/..")`; depoda `Assets/Resources/Esyalar` yok | `Mekan3D.cs:294-319` | Editör ve EXE farklı görüntü verebilir |
| M9 | `NesneKatalogu` (Resources'ta ScriptableObject) modellere doğrudan referans tutuyor | `NesneKatalogu.cs` | Katalog yüklenince referanslı modeller de yüklenir; tek tek boşaltılamaz [S012] |
| M10 | Mekân her değişimde yok edilip yeniden kuruluyor; `nesne:` öğeleri temizleniyor; kapı vb. durum saklanmıyor | `Sahne3D.cs:446-470` | Mekâna dönünce durum kaybolur |
| M11 | Mekân dizilimi sabit tohumlu (iyi); figüran yeri, modeli ve ölçeği tohumsuz `UnityEngine.Random` | `Mekan3D.cs:121-127`; `Sahne3D.cs` figüran kurulumu | Aynı hikâye her kayıtta farklı kalabalık verir |
| M12 | `MekanTarifi` yalnız tür + 3 renk + zaman/hava/ışık; `Belgeler\HikayeStudyosu\Mekanlar`'da | `MekanTarifi.cs` | Tarif projeyle taşınmaz; yerleşim değiştirilemez |
| M13 | Projede PBR doku bulunmadı (yalnız oyuncak palet PNG'leri); instancing/batching/LOD kurulumu aramada görülmedi | proje geneli | - |

İyi taraf: **adlı noktalar** (`N("kapi", konum, yaw)`) ve figüran bölgesi, korunması gereken doğru soyutlama (KOD + ÇIKARIM). NesneKur'un beklediği `Assets/HikayeStudyo/Kutuphane` (KayKit, NASA) depoda görülmedi; cihazdaki içeriği doğrulanmadı.

Gözlem: D001 karelerinde (`EXPERIMENTS/D001_ornek30_kareler.jpg`) `sato` arka planı tek düz tuğla ızgarası, tek tonlu turuncu; arka planda derinlik katmanı yok.

### S2 — Varlık kaynakları ve lisanslar
Hukuk görüşü değildir; indirirken her paketin lisans dosyası ayrıca saklanmalı.

| Kaynak | İçerik / stil | Lisans | URP uyumu | Ücret | Dikkat |
|---|---|---|---|---|---|
| Poly Haven | HDRI, PBR doku, model; gerçekçi | CC0 [S020; A04-S022] | Doku/HDRI doğrudan; roughness → smoothness dönüştürülmeli (ÇIKARIM) | ücretsiz | **ilk tercih** |
| ambientCG | PBR doku, HDRI | CC0; yazılım içinde dağıtım serbest [S017] | aynı | ücretsiz | **ilk tercih** |
| Megascans (Fab) | taranmış kaya, bitki, mimari parça, yüzey (23.000+) [S006] | Fab Standard; Unreal ile sınırlı değil [S009]; tek başına dağıtım yasak [S007, S009] | FBX/glTF [S007]; malzeme URP'ye elle kurulur, ağ sadeleştirme gerekebilir (ÇIKARIM) | Kas 2026'dan beri ücretsiz [S006, S007] | Epic hesabı (kullanıcı işi); Megaplants yalnız UE [S007]; Personal/Professional eşiği 100 bin USD [S001] |
| Fab (diğer) | çok çeşitli | Standard ya da CC-BY [S001] | ürüne göre | ücretsiz/ücretli | "yalnız UE formatı" ürünler olabilir: formatı kontrol et |
| Quaternius | modüler köy, ev, sokak, doğa; low-poly/stilize | CC0 [S005] | glTF/FBX; hazır URP sürümü ücretli "Source" [S005] | temel ücretsiz | projede 205 dosya var; depo kaydı üçüncü taraf GitHub'dan alındığını yazıyor (`LISANSLAR.md`) |
| Kenney | low-poly kitler, parçacık | CC0 [S018] | FBX/glTF (ÇIKARIM) | ücretsiz | stil gerçekçi değil |
| KayKit | low-poly zindan, şehir vb. | CC0 (City Builder Bits doğrulandı [S010]) | FBX/glTF/OBJ [S010] | ücretsiz + ek sürüm | stil gerçekçi değil |
| Sketchfab | her şey; kalite çok değişken | modele göre: CC BY (atıf), NC (ticari yok), ND (değiştirme yok), SA [S008]; Standard/Editorial [S004]; Editorial ticari kullanılamaz [S004] | glTF (ÇIKARIM) | ücretsiz indirmeler sürüyor; mağaza kapandı [S003] | yalnız CC0/CC-BY al; atıf listesi tut; gerçek marka/fan modellerinden kaçın (ÇIKARIM) |
| Unity Asset Store | URP paketleri; ör. Terrain Sample Asset Pack ücretsiz 1,6 GB [S019]; DEXSOFT Medieval Castle URP 39,99 $, 5,8 GB [S025] | Asset Store EULA [S002]: ürüne gömülü kullanım; **2.2.1.1(c)** kullanıcı-içeriği ürünü kısıtı; AI eğitimi yasak | en kolay (URP işaretli paketler) | ücretsiz/ücretli | A12 onayı olmadan uygulama kütüphanesine koyma |
| Synty | stilize modüler | Synty EULA: "Game Creation Software" için içerik üretimi yasak [S024] | iyi (ÇIKARIM) | ücretli | **kullanma** (stil ve lisans) |

- Kullanıcının kendi videosunda kullanması ile varlığın **uygulama içinde başkalarına sahne kurma kütüphanesi olarak dağıtılması** farklı durumlar (ÇIKARIM). İkincisi için en güvenli küme CC0'dır.
- ÇIKARIM: ücretsiz ve gerçekçi **modüler mimari kit** (birbirine oturan duvar/kemer/zemin) bu araştırmada bulunamadı. En ekonomik yol: mimari kabuğu kendimiz üretmek (basit geometri + pah + doğru UV) + CC0 PBR malzeme + az sayıda taranmış "kahraman" eşya.

### S3 — Modüler kit ve procedural üretim
| Yol | Ne sağlar | Koşul | Değerlendirme (ÇIKARIM) |
|---|---|---|---|
| Mevcut C# + kendi ağ üreticisi (pahlı kutu, kemer, sütun, merdiven, tonoz; metre ölçekli UV) | tam otomatik, tohumlu, paket yok | kod işi | **Ana yol (başlangıç)** |
| Aynı yerleşim kodu + prefab parçalar | küp yerine kit parçası dizer | kit | **Ana yol (2. adım)** |
| ProBuilder 6.1.4 [S014] | editörde elle modelleme | paket onayı | kit parçası hazırlamak için yardımcı; otomatik üretim aracı değil |
| Splines `SplineInstantiate` (tohumlu) [S021] | yol, çit, duvar, ağaç sırası | Splines kurulu mu `packages-lock.json`'dan bakılmalı (Cinemachine bağımlılığı olabilir; doğrulanmadı) | köy/sokak/orman için iyi |
| Blender + Python/geometry nodes → GLB | pahlı, UV'li kit parçaları; toplu üretim | Blender kurulumu; dışa aktarımda instance'ların gerçekleştirilmesi gerekebilir (DOĞRULANAMADI) | kit **üretim atölyesi** olarak değerli; uygulamanın çalışma anı bağımlılığı olmasın |
| Infinigen Indoors (BSD) [S015] | tam prosedürel iç mekân | GPU yolu yalnız NVIDIA; Windows deneysel; USD/FBX | bu PC'de uygun değil; ileride dış kaynak sahne üretimi için izle |
| WFC / Tessera / Edgar | karo tabanlı yerleşim | Edgar yalnız 2D ve lisans metni çelişkili [S031]; diğerleri doğrulanmadı | gerekmez: hikâye mekânı küçük ve elle tasarlanmış şablon daha kontrollü |

### S4 — Terrain, bitki, atmosfer, hava
- **Zemin:** dikey kadraj dar bir dilim gösterdiği için (S7) çoğu planda hafif eğimli tek ağ + instanced çim/taş yeterli; Unity Terrain yalnız çok geniş planlarda (ÇIKARIM).
- Unity'nin URP terrain demosu instanced çim + SpeedTree + 50M+ üçgen kullanıyor ve zayıf donanım için yoğunluk/mesafe düşürmeyi öneriyor [S016]; bu PC için doğrudan hedef değil (ÇIKARIM). Terrain Sample Asset Pack ücretsiz ama 1,6 GB [S019].
- **Bitki:** yaprak = alpha-clip; üst üste binme tümleşik GPU'da pahalıdır (ÇIKARIM). Yakında 3–6 kaliteli bitki, ortada instanced basit ağaç, uzakta kart/HDRI.
- **Atmosfer:** URP 6.6'da yerleşik volumetric yok; yalnız klasik sis [A04-S005]; kod `RenderSettings.fog` kullanmıyor (A04 K18). Mekân tarifine mesafe sisi rengi/yoğunluğu + birkaç yumuşak sis/toz kartı + ışık huzmesi kartı (ÇIKARIM).
- **Hava:** mevcut `HavaDurumu3D` (tek ağda yağmur/kar dörtgenleri; iç mekânda yalnız pencere önü) korunabilir (KOD). Eksik olan ıslak zemin/birikinti/kar örtüsü: Shader Graph "Production Ready Weather" örneklerinde yağmur, birikinti, ıslaklık ve kar alt grafikleri var; tam yağmur pahalı [S028]. Sayfa URP desteğini açıkça yazmıyor: denenmeli.
- Parçacık/VFX ayrıntısı A06'nın konusu.

### S5 — Varlık yönetimi (adla, gerektiğinde yükleme)
- Doğrudan referans: dinamik yükleme/boşaltma yok. Resources: yalnız prototip/küçük proje için önerilir, tek tek boşaltma yok. Addressables: bellek ve bağımlılık yönetimi, adresle erişim; "content directories" yerel ek içerik için yeni seçenek [S012].
- Öneri (ÇIKARIM):
  - **Aşama 1 (paket yok):** `VarlikKatalogu` = yalnız veri: `id`, Türkçe takma adlar, tür, **yol (string)**, boyut (m), pivot, yön, kit, lisans, kaynak URL, atıf metni. Model `Resources/Ortam/<kit>/<id>` altından adla yüklenir; mekân değişince kullanılmayanlar boşaltılır. Editör ve EXE aynı yolu kullanır (M8'i kapatır).
  - **Aşama 2 (kütüphane büyüyünce; paket onayı):** Addressables (ya da content directories). Katalog arayüzü aynı kalır, yalnız yükleyici değişir.
  - Kullanıcının kendi GLB'si: glTFast zaten kurulu; build'de shader/variant eklenmezse magenta olur [S013].
- İçe aktarma denetimi: ölçek (1 birim = 1 m), pivot tabanda, +Z yönü, doku ≤2K, tek malzeme/az malzeme, çarpışma yok, lisans kaydı zorunlu (lisansı boş varlık kütüphaneye giremez).
- Disk (ÇIKARIM): C: 45 GB boş. İlk kütüphane için üst sınır ~5–8 GB; tek bir ticari kale paketi 5,8 GB [S025]. Megascans'ten yalnız 1K–2K sürümler. Unity `Library` klasörü ayrıca büyür.

### S6 — Optimizasyon (tümleşik GPU)
- Kayıt kare kare olduğu için akıcılık değil **kare süresi ve bellek** önemli (A04 S8).
- GPU Resident Drawer: Forward+/Deferred+ ve compute ister; MaterialPropertyBlock'lu ve Animator altındaki nesneler yararlanmaz [S011]. Daha çok CPU çizim yükünü azaltır; tümleşik GPU'da darboğaz büyük olasılıkla GPU olduğundan kazanç ölçülmeden varsayılmamalı (ÇIKARIM).
- Mesh LOD: import'ta otomatik LOD, LOD Group'tan az bellek; static batching ile LOD0'da kalır; çalışma anında kodla üretilen ağlar için değil, import edilen modeller için [S022] (son kısım ÇIKARIM).
- Runtime static batching mümkün ama birleşik ağlar için ek bellek ister [S023]. Mesh LOD'u etkisizleştirir [S022]; 6.7 Surface Cache GI de static batching'in kapalı olmasını ister [A04-S008]. Bu yüzden varsayılan: **SRP Batcher + az sayıda paylaşılan malzeme + instancing**, static batching yalnız ölçülürse (ÇIKARIM).
- 6.6'da dynamic batching kaldırıldı (A09); yüzlerce ayrı küp bugün ayrı çizim.
- Başlangıç bütçesi (ÇIKARIM, ölçülecek hipotez): mekân başına görünür ≤500 bin üçgen; ≤30 malzeme; kahraman parçalarda 2K, diğerlerinde 1K; gölge atan ek ışık ≤2; 1 reflection probe (A04); mekân kurulum ≤5 sn.
- Kamera yayına girmeyen duvar/eşyayı plan başına kapatmak (A01 "sanal set hilesi") hem kompozisyon hem hız kazandırır.

### S7 — Dikey kadraj set tasarımı ve devamlılık

**Görünen alan (hesap, ÇIKARIM):**
- Mevcut kamera (dikey FOV 30°, 6,2 m): oyuncu düzleminde yalnız **3,3 m yüksek x 1,9 m geniş**; ~9,3 m'deki arka duvarda 5,0 x 2,8 m (3 m'lik duvarın üstü/tavan kadraja girebilir).
- A01 mercekleriyle (`d = H·f/36`), 10 m uzaktaki görünen genişlik: 24 mm → 8,4 m; 35 mm → 5,8 m; 65 mm → 3,1 m; 100 mm → 2,0 m. Yükseklik: 15,0 / 10,3 / 5,5 / 3,6 m.
- Sonuç: **genişlik değil derinlik ve yükseklik** gerekir. Yakın/orta planda arka planda 2–4 m'lik bir dilim görünür; ayrıntı o dilime harcanmalı. Geniş planda ise 10–15 m'lik düşey alan dolmalı (yüksek tavan, kule, gökyüzü).

**Set kuralları (ÇIKARIM; dikey katman fikri için zayıf destek [S030, D]):**
1. 4 katman: ön plan (kenarda siluet: sütun, perde, dal, mum) → oyuncu → orta (kit, eşya) → uzak (HDRI/kart/gökyüzü). Her planda en az 2 katman.
2. Dikey çizgiler: kapı, kemer, sütun, uzun pencere, sancak, ağaç gövdesi, merdiven (A01'in A05'ten isteği).
3. Derinlik ekseninde yönlendirici çizgi: halı, koridor, yol, sıra sütun.
4. Oyuncu–arka plan mesafesi mümkünse ≥3–5 m (DoF ayrımı).
5. Işık kaynağı arkada görünür olsun (pencere, meşale), yüzün arkasında düz parlak leke olmasın (A04).
6. Alt şerit altyazı ve arayüzle örtülür: zemin ayrıntısına az yatırım. Üst bölge sakin ama boş değil.
7. Kaçan duvarlar ("wild wall"): dört yönü kurulu set; kamera hangi duvarın arkasındaysa o gizlenir. Ters açı için şart.
8. Duvar karosu tek örnek tekrar etmesin: 2–3 varyant + leke/decal + payanda/niş ile bölme.
9. Her mekâna 1 "imza kadraj" (kanca/geniş plan) tasarla.

**Devamlılık modeli (ÇIKARIM):**
- Mekân kimliği + tohum + sürüm → her zaman aynı yerleşim. Tüm rastgelelik (figüran dahil, M11) tohumlu.
- Durum farkı hikâye boyunca saklanır: kapı açık, kırık fincan, sönmüş meşale, eklenen nesneler. Mekâna dönüşte uygulanır.
- Güneş/ay yönü ve pencere ışığı mekân verisinde; zaman/hava değişimi yalnız ön ayar değiştirir.
- Adlı noktalar kit güncellense de aynı adla kalır (`kapi`, `taht`, `pencere` ...).
- İç–dış ilişkisi: ör. `sato_avlu` ile `sato_taht` aynı kit ve palet; pencereden görünen uzak katman dış mekânla uyumlu.
- Referans kare testi: mekân başına 3 sabit kamera görüntüsü; değişiklikten sonra karşılaştır.

### S8 — Yapay zekâ destekli araçlar (video üretici değil)
| Araç | Durum |
|---|---|
| TRELLIS.2 (görselden PBR GLB) | MIT; NVIDIA 24 GB+, Linux [S026]. Bu PC'de çalışmaz; bulutta olur (ücret/hesap kullanıcıya). |
| Hunyuan3D-2 | Lisans AB, İngiltere, G. Kore'yi kapsamıyor [S027]; dağıtılacak ürün için risk (A12). |
| World Labs Marble + Gaussian splat | metin/görselden 3D dünya; Unity'de üçüncü taraf eklenti (fork öneriliyor), çarpışma ağı sorunları [S029]. DX11 desteği ve ücret DOĞRULANAMADI. ÇIKARIM: splat sahneyi ışık ve gölgeyle karakterlere uydurmak zor olur; şimdilik yalnız uzak arka plan deneyi. |

Bunlar tek tek eşya ya da arka plan **ham maddesi** üretebilir; temel sistem bunlara bağımlı olmamalı (ÇIKARIM).

## Teslim: ortam üretim sistemi önerisi (ÇIKARIM; test edilmedi)

### 1. Katmanlar
```
Hikâye: mekan3d: sato [zaman= hava= isik= ...]
  -> MekanVerisi (id, sürüm, tohum, şablon)
       kabuk: kit parçaları (grid)          malzeme: PBR kütüphanesi (adla)
       eşya: kahraman + dolgu (adla)        uzak: gökyüzü/HDRI/kart
       noktalar + kamera yayları + figüran bölgesi + yürüme yolları
       ışık/atmosfer ön ayarı (A04)         durum değişkenleri
  -> Kurucu (yerleşim kodu, tohumlu) -> yükleyici (katalog) -> sahne
  -> (isteğe bağlı) Editörde "prefab olarak dondur" -> elle rötuş
```

### 2. Veri alanları
- `MekanVerisi`: ad, temel şablon, tohum, ölçü (en/boy/yükseklik), kit, malzeme eşlemesi (duvar/zemin/tavan/vurgu), eşya listesi (kalıcı kimlik + varlık adı + konum/yön), noktalar, kameranın girebileceği hacim (M5 yerine), gizlenebilir duvarlar, uzak katman, güneş yönü, sis, hava uyumu.
- Proje klasöründe metin dosyası olarak saklanır (sürümlenir, taşınır); mevcut `MekanTarifi` geriye dönük okunur. `mekan3d:` sözdizimi değişmez.
- Genişletme: yeni mekân = yeni veri dosyası; yeni kit = yeni klasör + katalog satırları; kod değişmez.

### 3. Dört temel kit (17 mekânı kapsar)
| Kit | Mekânlar | İlk parçalar |
|---|---|---|
| Taş (saray) | sato, zindan, magara | duvar 2 varyant, sütun, kemer, merdiven, zemin, pencere, sancak, meşale, taht, kaya |
| Ev (ahşap-sıva) | oda, ev, mutfak, yatak_odasi, koy | duvar, kapı, pencere, kiriş, çatı, mobilya |
| Modern | hastane, sokak, araba çevresi, kopru, koridor, hava_kilidi | panel, cephe modülü, kaldırım, lamba |
| Doğa | orman, park, mezarlik | zemin, 3 ağaç, çalı, çim, kaya, kütük, yol |

Grid: 2 m modül; kat yüksekliği 3 m (ev) / 4–6 m (saray); pivot tabanda; tek metre ölçekli UV ya da triplanar (A04); kenar pahı.

### 4. Sıra
| Aşama | İş | Yeni paket/indirme | Beklenen kazanç |
|---|---|---|---|
| 0 | Mevcut 205 eşyayı ad eşleme tablosuyla bağla ve karşılaştır (stil uyumunu gör) | yok | en ucuz deney |
| 1 | Gölge/ışık temelleri (A04) | yok | yüksek |
| 2 | `sato` pilotu: pahlı kit parçaları (kodla) + 6–8 CC0 PBR malzeme + dikey set kuralları + 4. duvar | CC0 doku indirme (onay) | "düz tuğla kutu" görünümü gider |
| 3 | Uzak katman: gökyüzü/HDRI + sis | CC0 HDRI (onay) | dış mekân derinliği |
| 4 | Veri + katalog + durum/tohum devamlılığı | yok | yeniden kullanım |
| 5 | Kahraman eşyalar: Poly Haven / Megascans; gerekirse Blender'da sadeleştirme | indirme, Epic hesabı (kullanıcı) | gerçekçilik |
| 6 | Diğer 3 kit; ıslaklık/kar; bitki | örnek içerik (onay) | genişlik |
| 7 | Kütüphane büyürse Addressables | paket (onay) | bellek/disk yönetimi |

### 5. Bütçe
Zorunlu para harcaması yok (CC0 + Megascans). İsteğe bağlı ücretli paket yalnız A12 lisans onayıyla. Asıl maliyet **emek**: bir kitin ilk hâli tahminen birkaç gün; ölçülmedi.

## Teknoloji kayıtları
Ortak: doğrulama tarihi 2026-11-27; **test durumu: hiçbiri denenmedi**.

| Ad · kategori | Ne yapar / nasıl | Katkı | Unity uyumu | Donanım | Maliyet · lisans | Avantaj / dezavantaj | Alternatif | Kaynak · güven |
|---|---|---|---|---|---|---|---|---|
| Kodla modüler kit · üretim | tohumlu yerleşim + parça | ana sistem | tam | düşük | emek | otomatik / sanat kalitesi emek ister | hazır paket | KOD · orta |
| ProBuilder 6.1.4 · modelleme | editörde geometri | kit parçası | 6.6 yayınlanmış | düşük | Unity paketi | kolay / elle | Blender | S014 · yüksek (belge) |
| Splines SplineInstantiate · dizme | eğri boyunca prefab, tohum | yol/çit/orman | UPM | düşük | Unity paketi | tekrarlanabilir / yalnız dizme | kendi döngü | S021 · yüksek |
| Blender · harici üretim | script/geometry nodes → GLB | kit atölyesi | glTF/FBX | CPU yeterli (ÇIKARIM) | ücretsiz | güçlü / öğrenme | ProBuilder | DOĞRULANAMADI (aktarım) · düşük |
| Poly Haven, ambientCG · varlık | PBR/HDRI | gerçekçi yüzey | her sürüm | bellek/disk | ücretsiz · CC0 | serbest / mimari kit az | Megascans | S017,S020 · yüksek |
| Megascans · varlık | fotogrametri | en gerçekçi ücretsiz küme | elle malzeme | ağır olabilir | ücretsiz · Fab Standard | kalite / dağıtım kısıtı, hesap | Poly Haven | S006,S007,S009 · orta-yüksek |
| Quaternius/Kenney/KayKit · varlık | low-poly kit | yerleşim, stilize hikâye | iyi | çok hafif | ücretsiz · CC0 | hafif / gerçekçi değil | Megascans | S005,S010,S018 · yüksek |
| Addressables · yükleme | adresle yükle/boşalt | büyük kütüphane | UPM | - | Unity paketi | bellek / karmaşıklık | Resources | S012 · yüksek |
| glTFast 6.21 (kurulu) · yükleme | çalışma anında GLB | kullanıcı modeli | URP | - | Unity paketi | hazır / shader variant tuzağı | editör import | S013 · yüksek |
| GPU Resident Drawer · optimizasyon | BRG instancing | çok parçalı set | 6.6, Forward+ | compute | yerleşik | CPU rahatlar / build süresi | SRP Batcher | S011 · yüksek |
| Mesh LOD · optimizasyon | import'ta LOD | uzak modeller | 6.2+ | düşük | yerleşik | az bellek / static batching ile çalışmaz | LOD Group | S022 · yüksek |
| Shader Graph Weather örnekleri · materyal | ıslak/kar | hava | URP'de denenmeli | tam yağmur pahalı | Unity örneği (lisans A12) | hazır / uyum belirsiz | basit smoothness artırma | S028 · orta |
| Infinigen Indoors · prosedürel | tam oda üretimi | ileride | USD/FBX dönüşümü | NVIDIA | BSD-3 | zengin / bu PC'de uygun değil | kendi şablon | S015 · orta |
| TRELLIS.2 / Hunyuan3D · AI 3D | görsel → model | tekil eşya | GLB | NVIDIA yüksek VRAM | MIT / kısıtlı | hızlı varlık / kalite ve hak denetimi | hazır CC0 | S026,S027 · orta |
| Gaussian splat (Marble) · AI ortam | fotogerçek arka plan | uzak katman deneyi | üçüncü taraf eklenti | bilinmiyor | bilinmiyor | görünüm / ışık uyumu, olgunluk | HDRI | S029 · düşük |

## Karşılaştırma ve çelişkiler
- **Ç1:** `Docs/VideoKalitesi.md` "Fab ücretsiz varlıkları Unreal dışında kullanılabilir (özet)": Fab EULA sayfasıyla destekleniyor [S009]; ama Megaplants gibi UE'ye özgü içerik var [S007].
- **Ç2:** `Docs/Kaynak_Arastirmasi.md` yalnız low-poly paketler öneriyor ↔ hedef "gerçekçi ortam". Low-poly paketler yedek/stilize seçenek olmalı (ÇIKARIM).
- **Ç3:** Optimizasyon araçları birbirini dışlayabiliyor: static batching ↔ Mesh LOD [S022] ↔ Surface Cache GI [A04-S008].
- **Ç4:** Unity, Resources'ı yalnız küçük projeler için öneriyor [S012] ↔ projenin kataloğu Resources + doğrudan referans. Küçük kütüphanede geçici kabul.
- **Ç5:** Edgar-Unity MIT diyor ama "yeniden dağıtılamaz" şartı da yazıyor [S031].
- **Ç6:** A04 pişirilmiş ışık için sabit prefab ister ↔ bu sistem tohumlu kurulum. Çözüm: "prefab olarak dondur" adımı; pişirme yalnız dondurulmuş mekânda (iGPU'da pişirme DOĞRULANAMADI, A04).

## Riskler
| ID | Risk | Etki | Önlem |
|---|---|---|---|
| A05-R1 | Gerçekçi karakter + stilize ortam uyumsuzluğu | yüksek | tek stil kuralı; pilotta A/B |
| A05-R2 | Asset Store/Synty varlığının uygulama kütüphanesinde lisans ihlali | yüksek | A12 onayı; öncelik CC0 |
| A05-R3 | Disk dolması (45 GB) | yüksek | boyut bütçesi, 1K–2K, harici disk |
| A05-R4 | Doku/yaprak yükü tümleşik GPU'da kare süresini patlatır | orta | bütçe + ölçüm; offline tolerans |
| A05-R5 | Elle sanat emeği hafife alınır | yüksek | önce tek mekân pilotu |
| A05-R6 | Ortam iyileşse de ışık düzeltilmeden sonuç zayıf kalır | yüksek | A04 aşama 1–5 önce/birlikte |
| A05-R7 | CC0 dosyaların üçüncü taraf aynadan alınmış olması (köken kaydı) | düşük-orta | lisans dosyası ve kaynak URL kaydı |
| A05-R8 | Megascans koşulları değişebilir | düşük | indirilen sürümün lisansını sakla |

## Test önerileri
**Bu bilgisayarda yapılabilir:**
- T1: `mekan3d: oda/koy/park` çalıştırıp `Prop_` nesnesi oluşuyor mu bak (M7 doğrulaması).
- T2: eşleme tablosuyla Quaternius eşyaları; aynı kare önce/sonra; kullanıcı stil kararı.
- T3: `sato` tek duvara PBR doku + pah + gölge; aynı kamera A/B.
- T4: 4. duvarlı setle ters açı karesi.
- T5: 35/65/100 mm ve EWS'de görünen alan tahmini ile gerçek kare karşılaştırması.
- T6: pilot mekânda 1080x1920 kare süresi, çizim sayısı, bellek, doku belleği (gölge açık/kapalı; GPU Resident Drawer açık/kapalı).
- T7: aynı hikâyeyi iki kez kaydet; ortam piksel farkı (M11).
- T8: EXE ile editör görüntüsü karşılaştırması (M8, glTF shader).
- T9: ıslak zemin örneği URP 17.6'da açılıyor mu.

**GPU isteyen:** TRELLIS.2 ile 5 eşya denemesi (bulut olabilir); yoğun orman/volumetric; büyük splat sahnesi.

## Öz denetim
- Unity çalıştırılmadı; M7/M8 kod ve dosya adı okumasına dayanıyor. Cihazdaki klasörler depodakinden farklı olabilir.
- Gerçekçi ücretsiz modüler mimari kit için kapsamlı katalog taraması yapamadım; "bulunamadı" "yoktur" demek değil.
- Megascans'in Unity'ye aktarımı ve URP'de görünümü için kullanıcı deneyimi/bağımsız test okumadım. Üçlü doğrulama (belge + test + deneyim) çoğu konuda tamamlanmadı.
- Tümleşik Radeon'da URP ortam performansı için hiç benchmark bulmadım/aramadım; bütçe sayıları hipotez.
- Dikey set tasarımı için güçlü kaynak yok; kurallar hesap + genel sinema pratiği + ÇIKARIM.
- Lisans okumaları özetleyici araçla; Asset Store EULA'nın birebir metni alınamadı. Fab tam EULA okunmadı.
- Orman, şehir, köy için ayrı ayrı üretim teknikleri (SpeedTree lisansı, şehir üreticileri, decal, vertex blend) yüzeysel kaldı.

## Eksik alanlar / açık sorular
1. Kullanıcı kararı: hedef stil tam gerçekçi mi, stilize de kabul mü?
2. Hikâyelerde en çok kullanılacak 3–5 mekân hangileri? (pilot sırası)
3. Epic hesabı açılıp Megascans kullanılacak mı?
4. Uygulama başkalarına dağıtılacak/satılacak mı? (lisans kümesini belirler)
5. Blender kurulumu kabul mü, disk için harici SSD olacak mı?
6. Forward+ ve GPU Resident Drawer bu PC'de (D3D11) sorunsuz mu?
7. URP'de zemine yansıtmalı HDRI için güvenilir çözüm?
8. Gerçekçi bitki/ağaç için CC0 kaynak yeterli mi?

## Diğer ajanlardan beklenen / ortak konular
- **A01:** mercek/plan listesi set ölçülerini belirliyor; kamera hacmi ve gizlenebilir duvar bilgisi mekân verisinden okunmalı (M5).
- **A03:** yürüme yolları, oturma/yatak noktaları, merdiven; kit ölçüleri animasyonla uyumlu olmalı.
- **A04:** PBR içe aktarma, triplanar, reflection probe, sis, ışık ön ayarı mekân verisine bağlanacak.
- **A06:** hava, sis kartı, toz, meşale alevi, kalabalık yerleşimi (tohumlu).
- **A08:** mekân/durum devamlılığı hikâye planlayıcısında; "ilk kez girilen mekân → tanıtım planı".
- **A09:** yükleyici mimarisi, GPU Resident Drawer/Forward+, EXE shader stripping, deterministik kayıt.
- **A10:** disk (45 GB), doku belleği, harici SSD, GPU'lu kademe.
- **A12:** Asset Store 2.2.1.1(c) [S002], Synty [S024], Fab dağıtım şartı [S009], Hunyuan bölge kısıtı [S027], Quaternius köken kaydı.

## Öneriler (kanıtla)
1. **Önce hazır olanı bağla ve ölç:** eşya ad eşlemesi (M7), tohumlu figüran (M11), editör/EXE aynı yükleme (M8) [KOD].
2. **"Mekân = veri" sistemine geç;** adlı noktaları koru; 17 mekânı 4 kite indir (ÇIKARIM).
3. **Tek pilot: `sato`** (D001'de reddedilen görüntü): pahlı kit + CC0 PBR + 4 katman + kaçan duvar + A04 ışığı [S017, S020; A04-S022].
4. **Lisans önceliği:** CC0 → Megascans (Fab Standard) → CC-BY (atıflı) → Asset Store yalnız A12 onayıyla; NC, ND, Editorial, Synty yok [S002, S004, S008, S009, S024].
5. **Adla yükleme:** doğrudan referans yerine yol tutan katalog; büyüyünce Addressables [S012].
6. **Optimizasyonu ölçerek aç:** paylaşılan malzeme + instancing; static batching/GPU Resident Drawer/Mesh LOD'u birlikte körlemesine açma [S011, S022, S023].
7. Infinigen, AI 3D ve Gaussian splat ana yol olmasın; izleme listesi (A11) [S015, S026, S027, S029].
