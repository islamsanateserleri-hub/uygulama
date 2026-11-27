# X1 — Çapraz denetim 1

Tarih: 2026-11-27. Okunan: plan §5, 06, 05, A01, A02, A04, A05, A06, A09, A10, A11 (A03, A07, A08, A12'de yalnız anahtar kelime taraması). Kod `Assets/` altında yalnız okundu; Unity çalıştırılmadı, test yapılmadı, web araması yapılmadı. Kod yolları `Assets/HikayeStudyo/` köküne göredir. "ÇIKARIM" = benim yorumum, "genel bilgi" = bu denetimde kaynakla doğrulanmadı.

## 0. En önemli sonuçlar
1. Mimaride **karakter için bağdaştırıcı (adapter) katmanı yok**. A02 yeni karakter yolları öneriyor, A09 bunları hiç değerlendirmiyor.
2. **HDR'yi açmak tek satırlık iş değil.** 3D görüntü saydam zeminle 2D kanvasa bindiriliyor; alfa için URP Alpha Processing ve 64-bit HDR gerekebilir (A06). A04 ve A09 bunu yazmıyor.
3. **Efekt, geniş plan ve kalabalık için ortak sert sınır:** 3D kameranın uzak kırpma düzlemi 60 m (`Scripts/Sahne3D.cs:98`). Hiçbir ajan anmıyor.
4. **A11'in "kareleri diske yaz" önerisi** A09 ve A10'un 45 GB disk uyarısıyla çelişiyor.
5. **Beş-altı ayrı veri şeması** öneriliyor; tek şema olmalı.
6. **Direct3D11 → DX12 sorusunun sahibi yok.**

## 1. Kod ve kaynak doğrulamaları
| # | İddia (ajan) | Sonuç | Kanıt |
|---|---|---|---|
| D1 | ARKit 52'nin 18'i eşleşiyor; viseme'lerden yalnız `aa`, o da eleniyor (A02, A07) | **DOĞRU** (kod mantığı; gerçek şekil adları doğrulanamadı) | `Scripts/YuzCanlilik.cs:441-470, 406-413`; elle sayım 18 |
| D2 | Ünlü seçimi sesten değil Perlin gürültüsünden (A02) | **DOĞRU** | `YuzCanlilik.cs:855` |
| D3 | Çene hem kemikle hem şekille sürülebilir (A02) | Kod yolu **DOĞRU**; Rocketbox'ta Jaw eşli mi **doğrulanamadı** | `YuzCanlilik.cs:659-660, 695, 855+` |
| D4 | 3D kamera LDR, ana ışık gölgesiz, mekân parçaları gölgesiz (A04, A06, A09) | **DOĞRU** | `Sahne3D.cs:91, 100, 112`; `Mekan3D.cs:237-238, 271` |
| D5 | Hatalı GPU okumasında kare atlanıyor (A09 K4) | **DOĞRU** | `Uygulama/FfmpegPipeRecorder.cs:266` |
| D6 | Kayıtta `targetFrameRate=30` (A09 K7) | **DOĞRU** (kayıt süresine etkisi ölçülmedi) | `Scripts/StoryPlayer.cs:116`; kaydedicide değiştiren satır aramada çıkmadı |
| D7 | OyuncuListesi ≈68 prefaba doğrudan referans (A09 K6) | **DOĞRU** (68 kayıt; bellek etkisi doğrulanamadı) | `Resources/OyuncuListesi.asset`; `Scripts/OyuncuListesi.cs:9-12` |
| D8 | Eşya araması Türkçe, dosyalar İngilizce, 205 GLB boşta (A05 M7) | **DOĞRU** (statik) | `Mekan3D.cs:286-320, 427-635`; `Karakterler/Esyalar` 205 GLB, yedi anahtarla ad eşleşmesi yok |
| D9 | Hava efekti "Particle System (mevcut)" (A09 A.4 ve kayıt 5) | **YANLIŞ** | `Scripts/HavaDurumu3D.cs:105-117` kendi Mesh + MeshRenderer'ı; dosyada ParticleSystem yok. A06 doğru yazmış |
| D10 | Plan türü 6; "Yakın" kadraj ≈1,07 m; satır içi 2D zoom 0,05 (A01) | **DOĞRU** | `Scripts/KameraYonetmen.cs:19, 22`; `Sahne3D.cs:16`; `StoryPlayer.cs:23-24, 1223-1248`; 2·2,15·tan14° = 1,07 |
| D11 | DoF Gaussian; kamera x sınırı ±2,35 m (A01, A05) | **DOĞRU** | `Scripts/AtmosferVolume.cs:700-708`; `KameraYonetmen.cs:370-376` |
| D12 | `plan:` komut adı dolu (A01 K14) | **DOĞRU** | `Scripts/KomutKayit.cs:108` |
| D13 | Figüran yeri/ölçeği tohumsuz (A05 M11, A09 K8) | **DOĞRU** | `Mekan3D.cs:124-127`; `Sahne3D.cs:603-610` |
| D14 | EXE'de DoF/Panini shader'ları atılıyor (A09 K3) | **DOĞRU** (proje günlüğündeki gözlem) | `DURUM.md:83` (A09 satır 79 diyor; dosya sonradan kaymış olabilir) |
| D15 | Rocketbox malzemesi URP/Lit, 0,2 smoothness, 0,35 clip, yalnız color+normal (A02) | **DOĞRU** | `Editor/Karakter3DKurulum.cs:488-544` |
| D16 | Cinemachine 6.6.0 kurulu (C-001) | **doğrulanamadı** (bu kopyada `Packages/` yok; TOPICS/00 beyanı). Kodda kullanım yok: doğru | arama |
| D17 | CC5 ~230 GB disk ister (A10, kesin dille) | **doğrulanamadı** | A02-S037 satırında "sayfanın CC5'e ait olduğu başlıktan kesin değil" |
| D18 | HDR kapalıyken 1 üstü yok; HDR+alfa için 64-bit (A06) | **DOĞRU** (kaynak kaydıyla uyumlu; sayfayı yeniden açmadım) | SOURCES A06-S008 |

## 2. Karakter (A02) ↔ Unity (A09)

### 2.1 A02 → A09: Unity tarafında karşılığı olmayanlar
| # | Sorun | Ayrıntı | Çözüm önerisi |
|---|---|---|---|
| U1 | **ARKit 52 + 15 viseme ara katmanı** A09'un "ÖNERİLEN" mimarisinde yok | Seçenek 1'de kamera ve render arayüzü var, karakter/yüz arayüzü yok | `IKarakterYuzu` + karakter kartı (şekil eşlemesi, çene yolu, göz kemikleri, malzeme ailesi, LOD) Seçenek 1'e eklensin |
| U2 | A09 teknoloji kayıtlarında MetaHuman, CC5, MPFB, özel cilt/göz/saç shader'ı yok | Uyumluluk matrisi hiç çıkarılmamış | Aşağıdaki matris ana mimariye girsin |
| U3 | Sürüm çatışması | Auto Setup ≤6000.3, MHBridge 2022.3, IllusionRP 6.3 + DX12/Vulkan, MetahumanURP 6000.6 ama Apple/Metal; proje 6000.6 + DX11. A09 6.7 LTS'e yükseltme öneriyor: araç listelerinden daha da uzaklaşır | Sürüm yükseltme kapısına "karakter aracı bu sürümde açılıyor mu" maddesi. Bugün **çözülemedi** |
| U4 | Rig Logic yerel (native) eklenti; Unity 7 CoreCLR, EXE derlemesi, CPU yükü | Kimse değerlendirmemiş (genel bilgi + ÇIKARIM) | MetaHuman denemesi EXE'de de yapılsın; yalnız Editor sayılmasın |
| U5 | K6 + ağır karakter | OyuncuListesi çözülmeden CC5/MetaHuman eklenirse EXE ve RAM büyür. A02 bunu anmıyor | Sıra bağımlılığı: önce K6, sonra yeni karakter kaynağı |

### 2.2 A09 → A02: A02'nin gözden kaçırdığı Unity gerçekleri
| # | Bulgu | Etki | Çözüm |
|---|---|---|---|
| U6 | **Mevcut `.mat` varsa shader yeniden atanmıyor** (`Karakter3DKurulum.cs:515-518`: yalnız yeni malzemede `new Material(lit)`), smoothness/cutoff her kurulumda ezilir (527, 538) | Yeni cilt/saç shader'ı kurulumu yeniden çalıştırınca eski karakterlere geçmez; elle ince ayar silinir (ÇIKARIM) | Kurulum shader'ı açıkça atasın; ayarlar karakter kartından okunsun |
| U7 | Yeni shader'ların EXE'de silinme riski (K3 ile aynı sınıf) | A02 testleri yalnız Editor | Her shader testi EXE karesiyle bitsin |
| U8 | Pre-integrated cilt, URP/Lit üstüne Shader Graph ayarıyla gelmez; özel aydınlatma kodu ister (genel bilgi) | A02 "0–~100 $" ve A04 "Shader Graph ile" emeği küçük gösteriyor olabilir | Süre tahmini vermeden tek karakter prototipi |
| U9 | Gözlenen kusur atlanmış: **"ağız açıkken diş görünmüyor"** ve saçta leke (`DURUM.md:55, 92`) | A02 "diş yapısı bilinmiyor" deyip "ağız içine gölge" öneriyor; bu görünürlüğü daha da azaltabilir | Önce diş mesh'i/şekli incelensin (T-A02-1'e ekle), gölge önerisi beklesin |
| U10 | Yeni karakterde **gövde animasyonu uyumu** (Humanoid retarget, twist kemikleri, düzeltici şekiller) ne A02 ne A09'da test maddesi | A02 yalnız yüz kabul testi istiyor | Kabul testine yürüme, oturma, el teması eklensin (A03) |
| U11 | İçe aktarıcı kuralları Rocketbox'a özel (`Karakter3DKurulum.cs:1205-1228`: `Rocketbox`, 2048 doku sınırı) | "FBX + Humanoid" yeni kaynakta kendiliğinden çalışmaz | Kaynak başına içe aktarma profili |
| U12 | `updateWhenOffscreen=true` tüm skinned mesh'lerde (`Scripts/Karakter3D.cs:62-64`) | MetaHuman/CC5 ve kalabalıkta CPU maliyeti büyür (ÇIKARIM) | Sınırlar yalnız VRM için ya da elle |

### 2.3 Lisans ile teknik öneri arasındaki gerilim
- A02: "Unity'de kalınacaksa CC5 daha az riskli" (teknik). A12 taraması: kullanıcıya kıyafet/renk çeşidi sunan uygulamada **Enterprise** gerekebileceği (forum düzeyi) yazıyor. Çözüm: "daha az riskli" ifadesi "yalnız teknik açıdan ve yalnız kendi videolarında" diye sınırlansın.
- C-010 "ÇÖZÜLDÜ" yalnız Unity'de kullanım için; kütüphane dağıtımı ve olası UE'ye özgü parçalar A12'ye göre açık. Durum metni böyle kalmalı.

## 3. Donanım denetimi (A10 rolü; bu PC: Vega 7, 45 GB)
Hiçbir öneride ölçüm yok. "Gerçekçi" sütunu ÇIKARIM'dır.

| Öneri | Gerçekçi mi | Not |
|---|---|---|
| A04 1–5 (gölge, HDR, SSAO, ışık, probe) | Büyük olasılıkla evet | tek tek ölç; HDR için bkz. Ç-A |
| A04 adım 8 (2160x3840 + MSAA) | Riskli | A10 itirazı hesapça doğru (≈398 MB, 4x). 8192 gölge için 268 MB 32-bit derinlik varsayar; 16-bit ise yarısı (doğrulanamadı) |
| A09 süper örnekleme + N alt kare motion blur | Birlikte hayır | 2x ölçek (4 kat piksel) × N=8 ≈ 32 kata kadar iş. A10 alt kareleri hiç denetlememiş |
| A09 EXE/parçalı kayıt, kuyruk | Evet | disk ön kontrolü şart |
| A06 Shuriken, flipbook, prosedürel şimşek | Evet (koşullu) | saydam üst üste çizim paylaşılan bellekte pahalı |
| A06 30–60 gerçek karakterlik kalabalık | Belirsiz | CPU skinning + U12 + YuzCanlilik; **A10 A06'yı hiç denetlememiş**. Rocketbox low-poly mesh'leri projeye kopyalanmıyor (yalnız `_facial.fbx` ya da `ad.fbx`, `Editor/RocketboxKur.cs:78-90`) |
| A06 VFX Graph | Deneysel | forum kaydına göre çökme D3D11'de, DX12'de yok (eski, C) |
| A05 PBR kit, 5–8 GB kütüphane | Disk koşullu | |
| A02 strand saç + SSS | Deneysel | A10'a katılıyorum |
| A11 splat arka plan | Bugün hayır | DX12/Vulkan ister |
| A11 D3/D4, TRELLIS.2 | Hayır | NVIDIA/bulut |

**Zayıf ya da yanlış:**
- A10 Kısa sonuç 6 "ekran kartı tek başına kayıt süresini kısaltmaz": **fazla genel**. Kendi formülü (S7: kare sayısı × max(1/30, kare süresi)) ağır karede kartın süreyi kısaltacağını söyler. Doğrusu: "kare süresi 33 ms altındaysa kilit kalkmadan kısaltmaz".
- **Ortak disk bütçesi yok.** EXE derlemesi 10,8 GB, kurulu ≈11 GB, kurulum ≈3,1 GB (`DURUM.md:81`) + A05 5–8 GB + A04 doku/HDRI + A06 flipbook + 6.7 LTS + olası Blender. 45 GB tek tek yeterli görünür, toplamda doğrulanmadı. Çözüm: ana araştırmacı tek tablo çıkarsın; SSD'nin tetikleyici eşiği yazılsın.
- Kart alımı çelişkisi (C-014): A10 B2 "RTX 3060 12 GB", A11 "RTX 50 16 GB geleceğe açık". A11 kendi içinde de gerilimli: Öneri 3 "D3 onayı sorulmadan yatırım yok", Öneri 5 "DLSS 5 desteğini ölçüt yap". Çözüm: kullanıcı D3'ü onaylamadıkça ve Unity desteği duyurulmadıkça DLSS 5 ölçüt olmasın; kart kararı ölçüm sonrasına.

## 4. Sinematografi denetimi (A01 rolü): ortam, efekt, otomatik yönetmen
| # | Çelişki / eksik | Çözüm |
|---|---|---|
| S1 | A01 tablosu ECU: f=100, d≈0,7 m; aynı belgenin kuralı 9: "normal yüz planında d < 1,0 m yok" ve 60 cm'lik yüz çekimi bulgusu | ECU için 135–150 mm (d≈0,9–1,1 m, hesap) ya da H büyütülsün |
| S2 | A01 sert kuralı "yaw ≥30° ya da H oranı ≥1,5" kendi sözlüğüyle zorlanıyor: MS/MCU ≈1,38, OTS/MCU ≈1,23; MCU→CU aralık uçlarında 1,25'e düşer | Kural yalnız aynı özne için, çözücü yaw'ı zorlasın ya da eşik denenerek ayarlansın |
| S3 | EWS/ejderha/ordu (A01, A06) ↔ `farClipPlane=60`, 5,2 m oda, 40×40 m dış levha | Uzak düzlem mekân verisinden gelsin; çok uzak öğe kart/gökyüzü katmanı. A05 `MekanVerisi`'ne alan eklensin |
| S4 | A01 yakın planda Bokeh ister; EXE'de DoF shader'ı atılıyor (D14), A01 bunu anmıyor | Kalıcı Volume Profile (A04) ön koşul |
| S5 | Mevcut `vfx:` 2D UI katmanı odak, derinlik ve kameraya uymaz; yakın plan/DoF artınca sırıtır (ÇIKARIM) | A06'nın T1 geçişi, A01 CU/ECU aşamasıyla aynı sıraya |
| S6 | A05 "dört duvar şart" ↔ A01 180° kuralı kamerayı tek tarafta tutuyor | Dört duvar P1 değil; POV/BACK planları eklenince gerekli. Pilot mekânda sınansın |
| S7 | A06 "plan başına 1 büyük efekt" bütçesi ayrı `EfektYonetmeni`'nde; A06 `yavas/dondur` zamanı değiştirir, A08/A09 ise süreyi önceden hesaplar | Zaman değiştiren modüller kurgu dosyasında yazılı olsun; efekt yönetmeni plan listesinden okur, süreyi sonradan değiştirmez |
| S8 | A02/A03 "önce hareket+yüz" ↔ A01 daha çok CU/ECU ↔ ağız bugün yalnız çene | CU/ECU genişletmesi viseme düzeltmesinden sonra (A01 riski zaten yazmış) |

**Ortak şema sorunu:** PlanTarifi (A01), MekanVerisi (A05), EfektTarifi (A06), çekim listesi (A09), kurgu.json (A08), plan manifesti (A11), karakter kartı (A02), ses zaman çizelgesi (A07). A08 yalnız A09 ile birleştirmeyi söylüyor. Çözüm: **tek "çekim manifesti"**; diğerleri ona kimlikle bağlanan kataloglar. `plan:` dolu olduğu için adlandırmada "çekim" kullanılsın (D12).

## 5. Gelecek denetimi (A11 rolü): açıklık
| # | Konu | Değerlendirme / çözüm |
|---|---|---|
| G1 | A11 `IKareIslemci`: "kareler diske → harici adım" ↔ A09: akış, kare dizisi saklama yok | 1080p 8-bit ham ≈15 GB/video-dk (hesap). Arayüz akış ya da çekim başına parça olsun; her parça işlenip silinsin |
| G2 | AOV (derinlik, maske, hareket vektörü) A09 Seçenek 1'de yok | Render profili arayüzünde yer bırakılsın; URP'de ayrı renderer feature ister (genel bilgi) |
| G3 | Bugün yalnız `IVideoKaydedici`, `IVarlikKatalogu` var (`Core/Arayuzler.cs:8, 14`) | Karakter/yüz, ses hizalama, hareket, kare işleyici arayüzleri eksik |
| G4 | **DX11 → DX12:** splat (A11), IllusionRP (A02/A04), VFX çökme notu (A06) buna bağlı; A09 hiç değerlendirmemiş | Ucuz test maddesi: aynı sahne DX11/DX12, kare + süre + kararlılık, EXE dâhil. Sonuçtan önce geçiş yok |
| G5 | Yansımayla (reflection) URP/Recorder çağrısı yeni geçişleri ve Unity 7'yi kırılgan yapar | A09 "güçlü yan" sayıyor; aynı zamanda risk. Kalıcı asset'ler ve açık asmdef referansı değerlendirilsin |
| G6 | Determinizm (A11 kare kesin kayıt) | D5, D6, D13 ve tohumsuz `Random` önce düzelmeli; "aynı manifest → aynı kare" GPU/fizik için garanti sayılmamalı |
| G7 | Köken/lisans manifesti: A05, A06, A11, A12 aynı şeyi ayrı istiyor | Tek varlık manifesti |

## 6. Diğer çelişkiler
| # | Çelişki | Çözüm |
|---|---|---|
| Ç-A | A04: "HDR açık; banding olursa 64-bit" ↔ A06: alfa gerekiyorsa Alpha Processing + 64-bit. 3D kamera zemini alfa 0 (`Sahne3D.cs:95-97`) | HDR testi iki durumda yapılsın: 3D mekân (tam kare) ve 2D arka plan üstünde 3D karakter. İkincisinde 64-bit/alfa varsayılan olsun |
| Ç-B | A09 K5 (yakın planda 1,6x zoom, 2D arka plan) ↔ A01 K11 (3D mekânda 0,05 itme) | Çelişki değil, iki ayrı durum; tek hatada "dijital zoom" diye birleştirilsin |
| Ç-C | A09 D9 (Particle System) ↔ A06 (özel mesh) | A06 doğru; A09 düzeltilmeli. Shuriken'e geçiş "mevcut" değil yeni iş |
| Ç-D | Blender eşiği GCN1+ ↔ GCN4+ | çözülemedi (karar etkisi düşük) |
| Ç-E | U3 sürüm/araç uyumu | çözülemedi; denemeyle |

## 7. Zayıf / kanıtsız iddialar
- A02 görsel tavan sıralaması (Rocketbox < MPFB < CC5 < MetaHuman): karşılaştırmalı görüntü yok (ajan da söylüyor).
- A02 FACS duygu tablosu: birincil kaynak yok.
- A01 hedef plan oranları, A05 üçgen/malzeme bütçesi, A06 halka mesafeleri: ölçümsüz başlangıç değerleri; kabul ölçütü gibi kullanılmasın.
- A09 zahmet süreleri tahmin.
- A10 TL fiyatları güvenilir değil (kendisi de söylüyor); maliyet planı TL toplamı veremiyor.
- "Yeni paket gerekmeyebilir" (A06) ve "Shader Graph ile" önerileri: .shadergraph varlığı projede hiç yok; üretim yöntemi (elle Editor işi) planlanmamış.

## 8. Eksikler ve önerilen testler
1. Karakter uyum kabul testi (yüz + gövde + malzeme + EXE).
2. DX11/DX12 karşılaştırması.
3. HDR + alfa, iki sahne türünde.
4. 60 m ötesi öğeyle geniş plan.
5. Ortak disk bütçesi tablosu.
6. Kalabalık ve efektin birlikte en ağır sahnede süre/bellek ölçümü (A10 denetimine A05/A06 eklensin).
7. Tek çekim manifesti taslağı (A01, A02, A05, A06, A07, A08, A09, A11 alanları).

## Öz denetim
- Hiçbir şey çalıştırılmadı; kod doğrulamaları statik okumadır, bazı satırlar yalnız aramayla görüldü.
- Dış kaynak sayfaları yeniden açılmadı; "kaynakla uyumlu" yalnız ajanın kaynak kaydıyla uyum demektir.
- A03, A07, A08, A12 tam okunmadı; onlarla ilgili cümleler tarama sonucudur.
- Donanım yargıları ölçümsüzdür.
