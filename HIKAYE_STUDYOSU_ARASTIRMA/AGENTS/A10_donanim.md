# A10 — Donanım ve ekonomik optimizasyon

> Tarih: 2026-11-27. Test çalıştırılmadı, kullanıcının bilgisayarına dokunulmadı, kod yalnız okundu.
> Kaynaklar: `SOURCES/A10_kaynaklar.md`. `[S002]` = A10-S002. "ÇIKARIM:" = benim yorumum. "GENEL BİLGİ:" = bu oturumda kaynakla doğrulanmadı.
> **Satın alma kararı verilmemiştir** (protokol §11, §19). Fiyatlar hızlı değişiyor.

## Görev tanımı
Protokol "ALT AJAN 10": mevcut bilgisayar ve en az yatırımla en yüksek kaliteyi elde etme yolları; sıfır / minimum / fiyat-performans planları; A02, A04, A09 önerilerinin performans denetimi.

Mevcut bilgisayar: Ryzen 5 5600GT, 64 GB RAM, Gigabyte A520M K V2, yalnız tümleşik Radeon, C: ~45 GB boş, Windows 10, Direct3D11. Güç kaynağı ve kasa **bilinmiyor**.

## Kısa sonuç
1. Bu bilgisayarda URP yolundaki yazılım iyileştirmelerinin hepsi **denenebilir**; hızları **ölçülmedi**. İlk iş ölçüm, sonra satın alma tartışması.
2. Q-009: evet, tümleşik grafik **Vega 7** (7 hesap birimi, 1900 MHz) [S001, S003, S028]. Blender Cycles GPU kullanamaz (A04-S030).
3. Anakartta işlemciye bağlı **PCIe 3.0 x16** yuva var [S002]. x8 kartlar (5060 Ti) burada 3.0 x8 çalışır; 16 GB sürümde VRAM taşmadıkça etkisi küçük [S008].
4. **Intel Arc bu sistem için riskli**: Ryzen APU'larda çözülmemiş görüntü kaybı hatası var [S011].
5. 2026 bellek krizi: GPU ve SSD fiyatları liste fiyatının çok üstünde [S014, S015, S022]. Güncel (Kasım) TL GPU raf fiyatı **alınamadı**; en yeni TL verisi Temmuz 2026 [S017].
6. Ekran kartı **tek başına kayıt süresini kısaltmaz ve görüntüyü güzelleştirmez**: kayıt gerçek zamana kilitli (A09 K7), gölge/HDR/malzeme kodda eksik (A04 K1–K18).

## Araştırma alanı ve soruları
- S1 Tümleşik grafik nedir, sınırı ne? (Q-009)
- S2 Anakart ve PCIe ekran kartı kısıtları?
- S3 Bu PC ile ne yapılabilir / yapılamaz?
- S4 GPU seçenekleri (NVIDIA/AMD/Intel)?
- S5 Güç kaynağı, kasa, soğutma, SSD?
- S6 Fiyatlar (TL), ikinci el riski, bulut GPU?
- S7 Render süresi tahmini yapılabilir mi?
- S8 Windows 10 ve sürücü desteği?
- S9 A02/A04/A09 önerileri performans açısından doğru mu?

## Bulgular

### S1. Tümleşik grafik
- AMD sayfası: "Radeon Graphics", 7 grafik çekirdeği, 1900 MHz; DDR4-3200, 2 kanal; 65 W (45–65 W ayarlanabilir) [S001].
- Mimari Vega [S003 başlık; S028 5600G için Vega 7 listeliyor]. GENEL BİLGİ: GCN 5 ailesi; donanım ışın izleme yok (projedeki `donanim.txt` de "ray tracing yok" diyor, TOPICS/00).
- **Ayrı video belleği yok**, sistem RAM'ini paylaşır. Anakart paylaşılan belleği en çok 16 GB gösteriyor [S002]. `Docs/VideoKalitesi.md` "32 GB ekran belleği" paylaşılan RAM diyor; bu VRAM değildir.
- Bant genişliği (hesap): DDR4-3200 çift kanal ≈ 51,2 GB/s, CPU + GPU + video kodlayıcı ortak.
- Anakartta 2 bellek yuvası, modül başına en çok 32 GB [S002]. ÇIKARIM: 64 GB takılı olduğuna göre 2x32 GB, yani **çift kanal çalışıyor olmalı**. RAM hızı bilinmiyor; XMP kapalıysa iGPU gereksiz yavaştır (ücretsiz kontrol).
- Büyüklük fikri: 3DMark Time Spy grafik ortalaması Vega 7 **991**, RTX 3060 **8426** (~8,5 kat) [S028]. Vega 7 ortalaması çoğunlukla dizüstü yongalarından; bizim sahnemizin kare süresine çevrilemez.
- Sürücü: Vega ayrı bir sürücü koluna alınmış; Mayıs 2026'da hâlâ sürücü çıktı (Windows 10 1809+) [S004]. ÇIKARIM: yeni özellik beklenmemeli.
- Blender: Cycles GPU yok (Vega HIP desteği 4.3'te kaldırıldı, A04-S030) → CPU. EEVEE'nin 5.x ile bu yongada durumu **DOĞRULANAMADI** (okunan gereksinim sayfası eski [S029]).

### S2. Anakart ve PCIe
- 1 x PCIe x16 yuva, işlemciye bağlı, "PCIe 3.0, x16 çalışır"; ek olarak 1 x PCIe 3.0 x1 [S002]. AMD işlemci sayfası da PCIe 3.0 diyor [S001].
- AMD sayfasında hat sayısı yazmıyor; "x16" bilgisi **anakart üreticisinden**. Kart takılınca GPU-Z ile bağlantı genişliği kontrol edilmeli.

| Kart | Kartın arayüzü | Bu sistemde | Kaynak |
|---|---|---|---|
| RX 9060 XT 16 GB | 5.0 x16 | 3.0 x16 | [S007] |
| RTX 3060 / 4070 / 5070 | 4.0 veya 5.0, x16 (hat sayısı GENEL BİLGİ) | 3.0 x16 | [S006] (nesil) |
| RTX 5060 Ti 16/8 GB | 5.0 x8 | **3.0 x8** (~7,9 GB/s, hesap) | [S005, S008] |
| Arc B580 | 4.0 x8 (GENEL BİLGİ) | **3.0 x8** + ReBAR ister | [S010] |

- 5060 Ti 16 GB: PCIe 3.0'da VRAM aşılmadıkça etki küçük; 8 GB sürüm VRAM taşınca çok kötü [S008, oyun testi]. **8 GB kart önerilmez.**
- ÇIKARIM: bizim kayıtta GPU'dan geri okuma 8,3 MB x 30 = ~249 MB/s; 3.0 x8 için bile küçük.
- A520 hız aşırtma sınırlıdır ve PCIe 4.0 vermez (GENEL BİLGİ); anakart değişimi bu amaç için gerekmiyor (ÇIKARIM).

### S3. Bu PC ile yapılabilen / yapılamayan

| İş | Durum | Not |
|---|---|---|
| Kod düzeltmeleri: ses-altyazı, yürüme/animasyon, kamera, yüz eşleme | **Yapılır** | CPU/RAM yeterli (ÇIKARIM) |
| URP gölge, HDR, tonemapping, SSAO, plana göre ışık | **Denenir** | maliyet ölçülmedi |
| 1080x1920/30 kayıt (mevcut) | **Yapılıyor** | proje 26 sn EXE test videosu üretmiş (DURUM.md) |
| Süper örnekleme, volumetric, SSS, strand saç | Denenebilir, **yüksek risk** | bkz. S9 |
| MPFB/Blender modelleme | Büyük olasılıkla yapılır (ÇIKARIM) | disk |
| Blender Cycles | Yalnız CPU, çok yavaş | A04/A09 süreleri |
| HDRP | Asgari şartı kâğıtta karşılıyor (A09) | zaten deprecated; yatırım yok |
| HDRP ışın/yol izleme, UE5 Lumen | **Yapılamaz** | donanım yok (A09-S029) |
| MetaHuman Creator | Öneriyi karşılamıyor | RTX 3070 sınıfı önerisi (A02-S007) |
| Character Creator 5 | **Yapılamaz** | 8 GB ayrı kart, 30 GB + 200 GB önbellek (A02-S037) |
| Yerel yapay zekâ ses/yüz modelleri | CPU'da bazıları yavaş çalışabilir | A07'nin alanı; test yok |
| UE5 kurulumu | Pratik değil | 45 GB disk, GPU yok |

- RAM avantajı (ÇIKARIM): 64 GB, büyük sahne/doku, Blender CPU render ve çok uygulama için bol. A520 kartın üst sınırı zaten 64 GB; RAM yatırımı **gereksiz**.
- CPU sınırı (ÇIKARIM): 6 çekirdek; skinned mesh, ffmpeg renk dönüşümü ve (yazılım yolunda) kodlama burada. İşlemci değişimi öncelik değil; ölçüm darboğaz gösterirse yeniden bakılır.
- Disk: 45 GB. Kurulu uygulama ~11 GB, derleme 10,8 GB (DURUM.md). 5 dk sıkıştırmasız kare dizisi 75–149 GB (A09 hesabı). **Disk bugün en somut kısıt.**

### S4. Ekran kartı seçenekleri

| Kart | VRAM | Güç / önerilen PSU | Blender skoru [S027] | ABD fiyatı (2026) | TL (18 Tem 2026) [S017] | Not |
|---|---|---|---|---|---|---|
| RTX 3060 12 GB | 12 | 170 W / 550 W* [S006] | 2287 | yeni 414–460 $, 2. el medyan 423 $ [S016] | alınamadı (2. el: aşağıda) | eski; 8 GB'lık sürümle karıştırma |
| RTX 5060 Ti 16 GB | 16 | 180 W / 600 W [S005] | 4289 | 790 $ Eki [S014]; 830 $ Kas [S015]; MSRP ~429 $ | ~31.000 | 3.0 x8 |
| RTX 5070 | 12 | 250 W / 650 W [S006] | 6079 | 900 $ Eki [S014] | ~42.000 | daha hızlı, VRAM az |
| RTX 4070 | 12 | 200 W / 650 W [S006] | 5365 | alınamadı | alınamadı | yeni stok belirsiz; büyük olasılıkla ikinci el (ÇIKARIM) |
| RX 9060 XT 16 GB | 16 | 160 W / 450 W [S007] | **1623** | 620 $ Eki [S014] | ~25.000 | x16; Blender'da çok zayıf |
| Arc B580 | 12 | alınamadı | — | 350 $ Eki [S014]; Kas'ta Amazon'da yok [S015] | ~12.000 | **APU sorunu** [S011] |

\* 170 W/550 W sütununun 3060 olduğu tabloda başlıktan doğrulanamadı.

Karşılaştırma:
- Blender 4.4: 5060 Ti 4294, 9060 XT 1611; Unreal 5.5 ortalama fps: 57,8 ve 55,5 [S026]. Yani **Blender'da NVIDIA ~2,7 kat, gerçek zamanlı raster'da yakın**. Unity URP için benchmark **yok**; raster iş Unreal örneğine daha yakın olabilir (ÇIKARIM).
- Fiyat-performans (hesap; farklı tarih ve kaynak karışık, yalnız fikir): Blender puanı / bin TL: 5070 ≈145, 5060 Ti ≈138, 9060 XT ≈65. Unreal fps / bin TL: 9060 XT ≈2,2, 5060 Ti ≈1,9. **"En iyi" kart iş yüküne bağlı.**
- Video kodlayıcı: kod `h264_nvenc → h264_amf → qsv → mf` sırasıyla dener (`FfmpegPipeRecorder.cs:72`). NVIDIA takılırsa NVENC kendiliğinden seçilmeli (ÇIKARIM; ffmpeg derlemesiyle test edilmeli).
- NVIDIA lehine ek: Cycles/OptiX, CC5/iClone, yerel yapay zekâ araçlarının çoğu (GENEL BİLGİ). AMD lehine: TL başına VRAM, düşük güç, x16.
- Arc B580: Intel ReBAR'ı gerekli sayıyor, Ryzen 5000 + 500 serisini listeliyor [S010]; ama Cezanne APU'lu sistemlerde sıcak yeniden başlatmada görüntü kaybı bildirimleri çok, 31 Ağu 2026'da hâlâ resmî çözüm yok [S011]; ayrı bir 5600G + B570 şikâyeti [S012]. Bir A520 kullanıcısında sorun yok [S011]. Karar için yeterli güven yok; **risk yüzünden eleme adayı**.
- RTX 3060'ın yeniden üretime alındığı yönünde söylentiler var [S021, D]; karar dayanağı yapılmadı.

### S5. Güç kaynağı, kasa, soğutma, SSD

**Güç kaynağı (en büyük bilinmeyen, Q-001)**
- Hesap kuralı: (CPU + GPU + ~100 W) x 1,2–1,3 [S033]. 5600GT 65 W [S001] + 180 W + 100 → ~415–450 W; 250 W'lık kartta ~500–540 W (hesap; işlemcinin anlık gücü 65 W'ı aşabilir).
- Üretici önerileri: 450 W (9060 XT) – 650 W (5070/4070) [S005–S007]. ÇIKARIM: kaliteli, güncel 550 W çoğu 160–200 W kartı; 650 W 250 W sınıfını karşılar.
- Watt etiketi yetmez: anlık güç sıçraması, yaş (5–7+ yıl riskli), markasız ürün [S033]. Gerekli PCIe 8-pin soketi var mı bakılmalı. Molex/SATA'dan 8-pin dönüştürücü kullanılmamalı (GENEL BİLGİ).

**Kasa**
- Anakart mATX [S002]; kasa bilinmiyor. Referans uzunluklar: 5070 242 mm, 4070 244 mm [S006]; üçüncü taraf kartlar farklı. İç boşluk, 2–3 yuva kalınlık ve kablo payı ölçülmeli.

**Soğutma**
- İşlemci kutu soğutucusu Wraith Stealth, Tjmax 95 °C [S001]. Uzun render'da sıcaklık izlenmeli. Kart eklenince kasaya en az bir giriş + bir çıkış fanı (GENEL BİLGİ).

**SSD**
- Anakart: 1 M.2 (SATA veya PCIe 3.0 x4/x2) + 4 SATA [S002]. M.2 dolu mu bilinmiyor.
- PCIe 4.0 SSD burada 3.0 hızında çalışır; pahalı hızlı model gereksiz (ÇIKARIM).
- Boyut: yalnız URP yolu 1 TB yeter; CC5 (230 GB) + UE5 + Blender birlikte düşünülürse 1–2 TB (ÇIKARIM).
- Fiyat: ABD'de 1 TB Gen4 ortalama ~144 $ (Eki 2026), bir yılda çok yükseldi, 2027'ye kadar rahatlama beklenmiyor [S022]. TL: güvenilir güncel fiyat **alınamadı** (tek tarihsiz 2.499 TL kaydı [S023, D] bu ABD fiyatıyla uyumsuz).
- Ücretsiz önlem: `Build/`, eski `Kayitlar/`, Unity `Library` boyutlarına bakıp kullanıcı onayıyla temizlemek; başka disk varsa kayıtları oraya almak.

### S6. TL fiyatlar, ikinci el, bulut

**TL (yeni)** — Oyungünlüğü, 18 Tem 2026, "başlangıç" fiyatı [S017]: RTX 5050 15 bin · 5060 17 bin · 5060 Ti 8 GB 20 bin · 5060 Ti 16 GB 31 bin · 5070 42 bin · 5070 Ti 54 bin · RX 9060 XT 16 GB 25 bin · RX 9070 34 bin · Arc B580 12 bin. Sayfada kendi içinde tutarsızlık var. Sonrasında dünya fiyatları yükseldi [S014, S020] → ÇIKARIM: bugünkü raf fiyatı daha yüksek olabilir. **Kasım 2026 TL fiyatı: alınamadı** (fiyat siteleri açılamadı/açılmadı).

**TL (ikinci el RTX 3060 12 GB)** — çelişkili: letgo istenen fiyat 15.000–37.850 TL [S018]; bir satıcı endeksi 6.200–7.400 TL [S019]. İkisi de tarihsiz, D sınıfı. **Güvenilir ikinci el TL fiyatı: alınamadı.**

**İkinci el riskleri**
- Isı yıpranması, fan aşınması; GDDR6X kartlar daha riskli; satıcıya kullanım geçmişi sorulmalı, teslimde stres testi ve görüntü bozulması kontrolü [S034, 2022].
- GENEL BİLGİ: garanti/fatura yokluğu, sahte/yeniden etiketli kart, 3060'ın 8 GB sürümü [S006 iki sürüm olduğunu doğruluyor]. GPU-Z ile model/VRAM/PCIe doğrulanmalı; elden, çalışırken görerek alım.
- Güç kaynağı ikinci el **alınmamalı** (GENEL BİLGİ).

**Bulut GPU**
| Hizmet | Fiyat | Windows | Not |
|---|---|---|---|
| RunPod Pod | 3090 0,50 $/sa, 4090 0,89 $/sa, A5000 0,27 $/sa; disk 0,10 $/GB/ay [S030] | sayfada yazmıyor | GENEL BİLGİ: Linux kapsayıcı; Windows EXE'miz doğrudan çalışmaz |
| Shadow Pro | 25,99–40,94 £/ay, 1 yıl taahhüt [S031] | Windows 11 | Türkiye bölge listesinde yok |
| Saatlik Windows GPU VM | alınamadı | — | arama sonuçlarında doğrulanmış fiyat yok |
- Örnek hesap: ayda 10 saat 4090 ≈ 8,9 $ + 30 GB kalıcı disk ≈ 3–6 $. Kurulum süresi, yükleme (10+ GB), kur ve ödeme yöntemi hariç.
- **Lisans uyarısı:** Unity Editor koşulları bulut/uzak sunucuda çalıştırmaya kısıt koyuyor (madde 2.1, 2.8) [S035]. ÇIKARIM: bulutta Editor yerine **derlenmiş EXE** ile render daha güvenli olabilir; A12 metni incelemeli. Epic/Reallusion araçları için de ayrı kontrol.
- ÇIKARIM: bulut, "GPU gerçekten kaliteyi artırıyor mu?" sorusunu satın almadan denemek için uygun; günlük üretim akışı için zahmetli. Hesap ve ödeme kullanıcıya aittir.

### S7. Render süresi
- **Unity/URP'de bizim sahnemize benzer, bu donanımlar için kaynaklı benchmark YOK.** Kayıt süresi tahmini verilmiyor.
- Yapısal alt sınır (A09 K7): `targetFrameRate=30` açık kaldıkça 60 sn video en az ~60 sn'de kaydedilir. ÇIKARIM: süre ≈ kare sayısı x max(1/30 sn, gerçek kare süresi). Hızlı kart bu kilit kalkmadan süreyi bunun altına indiremez.
- Blender CPU yalnız büyüklük fikri: 5600G BMW27 ~175 sn (Blender 3.4, A04-S031) ve 247,72 sn (4.2, A09-S032). Sürümler farklı, sayılar kıyaslanamaz; bunlardan türetilen "87 saat" / "124 saat" tahminleri **planlamada kullanılmamalı**. 5600GT aynı yonga ailesi, ama ayrıca ölçülmüş değeri bulunmadı.
- Gereken ölçüm: 10 sn referans sahne → toplam süre, kare süresi, tepe RAM; her kalite adımında tekrar.

### S8. Windows 10 ve sürücüler
- Tüketici güvenlik güncellemesi (ESU) **Ekim 2027'ye uzatıldı** (haberde 12 ve 14 Ekim geçiyor); kayıt gerekir: Microsoft hesabıyla eşitleme, 1000 puan veya 30 $ [S024]. Türkiye için ücretsiz yöntemin ayrıntısı doğrulanmadı. Kullanıcının kayıtlı olup olmadığı bilinmiyor → **kontrol edilmeli**.
- NVIDIA: Ekim 2026'dan itibaren Windows 10'da yalnız çeyreklik güvenlik güncellemesi, Ekim 2029'a kadar [S025]. Yeni özellik/optimizasyon sürücüsü yok.
- AMD: Vega sürücüsü Mayıs 2026'da Windows 10'u destekliyordu [S004].
- Unity 7 ve yeni araçların Windows 10 tutumu: araştırılmadı. ÇIKARIM: 2027 içinde Windows 11 planlanmalı (5600GT genel olarak uyumlu; TPM ayarı BIOS'tan açılır — GENEL BİLGİ).

### S9. Diğer ajanların önerilerine performans denetimi

**A09 (mimari)**
- Katılıyorum: önce ölç; URP'de kal; kare dizisi saklama; tek render süreci.
- İtiraz/ek 1: K1+K2 (gölge, HDR) "yapılabilir" ama Vega 7 maliyeti bilinmiyor; tek tek açılıp ölçülmeli.
- Ek 2: "yavaş GPU yalnız kaydı uzatır" doğruluk için geçerli olabilir; fakat çok ağır karelerde Windows sürücü sıfırlaması (TDR) riski var (GENEL BİLGİ; kaynak aranmadı). Watchdog (K10) önce gelmeli.
- Ek 3: ikinci GPU takılınca Unity'nin hangi karta çizdiği doğrulanmalı (monitör ayrı karta; `Donanim Raporu`). `h264_amf` ayarları yerine NVENC yolu devreye girer.
- Ek 4: Seçenek 2/3 (HDRP, UE, Blender) için disk ön şart: 45 GB yetmez.
- Blender gereksinimi: A09 "GCN4+", A04 ve [S029] "GCN1+". Vega her iki eşiği de geçiyor görünüyor; 5.x resmî sayfası doğrulanmalı.

**A04 (ışık/görüntü)**
- Katılıyorum: temeller (gölge, HDR, tonemapping, AO, ışık) donanımdan önce.
- **İtiraz 1: Adım 8'deki 2160x3840 + MSAA birlikte ağır.** Hesap: 64-bit HDR renk + derinlik, 4x MSAA ≈ 400 MB hedef; 8x ≈ 800 MB; her karede yazma/çözme trafiği ortak ~51 GB/s belleğe biner. Öneri: bu PC'de ya 1,5x ölçek ya 4x MSAA ile başla; ikisini birlikte ve 8x'i ölçmeden açma.
- **İtiraz 2: 8192 gölge haritası** (tek harita ≈ 268 MB, hesap) bu iGPU için başlangıç değeri olmamalı; 2048, 1–2 gölgeli ışık.
- İtiraz 3: volumetric ışın yürüyüşü ve Surface Cache GI ("çok video belleği") iGPU'da yüksek riskli; GPU kademesine bırakılmalı.
- Ek: Progressive GPU pişirme 2 GB ayrılmış VRAM ister (A04-S010) → BIOS "UMA Frame Buffer Size" ayarı denenebilir (ÇIKARIM; bu kartta doğrulanmadı).
- Ek: A04 "kart için 12–16 GB, donanım RT" — uygun; ancak Blender "hero plan" isteniyorsa AMD belirgin geride [S026, S027].

**A02 (karakter)**
- Katılıyorum: önce Rocketbox yüz eşlemesi (donanım maliyeti sıfır).
- **İtiraz 1:** "kayıt kare kare olduğu için yavaşlık tolere edilir" strand saç + SSS + süper örnekleme birlikte için kanıtsız; strand saç bu PC'de "deneysel" sayılmalı.
- İtiraz 2: CC5 için yalnız GPU yetmez; ~230 GB disk de gerekir. Plan C'de SSD zorunlu.
- Ek: MetaHuman LOD0 (669 blendshape) CPU/GPU yükü bilinmiyor; GPU alınsa bile 6 çekirdekte ölçülmeli.
- Ek: "kiralık makinede MetaHuman" Windows masaüstü ve ilgili lisans ister; ucuz Linux kapsayıcı yetmez (ÇIKARIM).

**`Docs/VideoKalitesi.md` (C-002)**
- "Ekran kartı + disk yeter, yeni bilgisayar gerekmez": anakart ve işlemci açısından makul [S002], ama güç kaynağı/kasa bilinmeden **koşulludur** ve kalite garantisi değildir.
- "HDRP ışın izleme RTX ister": HDRP zaten deprecated (C-003); satın alma gerekçesi olmamalı.

## Teknoloji kayıtları
Doğrulama tarihi 2026-11-27. **Hiçbiri test edilmedi** (iGPU projede kullanımda). Lisans: donanımda uygulanmaz; sürücü/hizmet koşulları A12.

| Ad · kategori | Ne yapar / nasıl | Katkı | Unity uyumu | Donanım ihtiyacı | Maliyet | Avantaj / dezavantaj | Alternatif | Kaynak · güven |
|---|---|---|---|---|---|---|---|---|
| Vega 7 iGPU · tümleşik GPU | paylaşılan bellekle raster/compute | mevcut taban | URP çalışıyor | yok | 0 | bedava / yavaş, RT yok | ayrı kart | S001, S028 · A/B |
| RTX 3060 12 GB · GPU | ayrı kart, NVENC 7. nesil | en eski/ucuz 12 GB | tam | 170 W, ~550 W PSU | ABD ~414–460 $ | VRAM / eski, hız düşük | 5060 Ti | S006, S016 · B |
| RTX 5060 Ti 16 GB · GPU | NVENC 9. nesil | 16 GB, Cycles OptiX | tam | 180 W, 600 W | ~31 bin TL (Tem) | bol VRAM, düşük güç / 3.0 x8, pahalandı | 9060 XT, 5070 | S005, S008, S017 · A/B |
| RTX 5070 / 4070 · GPU | üst orta sınıf | daha hızlı render | tam | 200–250 W, 650 W | 5070 ~42 bin TL (Tem) | hız / 12 GB, PSU | 5060 Ti | S006, S017 · A/C |
| RX 9060 XT 16 GB · GPU | x16, AMF | TL başına VRAM/raster | tam (DX11/12) | 160 W, 450 W | ~25 bin TL (Tem) | ucuz, az güç / Blender zayıf | 5060 Ti | S007, S026 · A/B |
| Arc B580 · GPU | ReBAR ister | ucuz 12 GB | bilinmiyor | 3.0 x8 | ~12 bin TL (Tem) | ucuz / APU hatası | diğerleri | S010, S011 · A/C |
| 1–2 TB SSD · depolama | M.2 veya SATA | disk kısıtını kaldırır | — | boş yuva | 1 TB ~144 $ (ABD) | gerekli / pahalandı | temizlik, harici disk | S022 · B |
| RunPod / Shadow · bulut GPU | uzak makine | satın almadan deneme | EXE için Windows şart (ÇIKARIM) | internet | 0,27–1,39 $/sa; 26–41 £/ay | yatırım yok / kurulum, lisans, TR erişimi | yerel kart | S030, S031 · A |

## Maliyet planları (§12)

### PLAN A — SIFIR YATIRIM
- **İçerik:** mevcut PC; URP; A09 "Seçenek 0"; A04 Kademe 0; Rocketbox yüz eşleme; ücretsiz/CC0/MIT varlıklar; Blender/MPFB (isteğe bağlı, disk el verirse).
- **Toplam maliyet:** 0 TL (emek ve elektrik hariç; Gemini ücretsiz kota ~10 istek/gün).
- **Avantaj:** risksiz; asıl şikâyetlerin (altyazı, hareket, yürüme) tümü donanımsız iş; ölçüm verisi üretir.
- **Dezavantaj:** RT yok; ağır efektler sınırlı; disk dar; CC5/MetaHuman/UE5 yok; ücretsiz TTS kotası yavaşlatır.
- **Görüntü kalitesi (ÇIKARIM):** bugünkünden belirgin iyi olabilir; örnek yapay zekâ videosunun seviyesi beklenmemeli.
- **Render performansı:** ölçülmedi; en iyi durumda gerçek zaman (30 fps kilidi), ağır ayarlarla daha uzun.
- **Donanım ihtiyacı:** yok.
- **Sürdürülebilirlik:** URP yolu uzun ömürlü; Windows 10 Ekim 2027'de bitiyor; Vega sürücüsü sınırlı destekte.
- **Ücretsiz kontroller:** RAM hızı/XMP; BIOS sürümü; UMA bellek ayarı; disk temizliği; ESU kaydı; güç kaynağı etiketi.

### PLAN B — MİNİMUM YATIRIM
İki aşama; ikinci aşama ancak ölçüm gerektirirse.

| | B1: en gerekli | B2: giriş seviyesi GPU |
|---|---|---|
| İçerik | 1 TB SSD (+ isteğe bağlı birkaç dolarlık bulut GPU denemesi) | B1 + 12 GB sınıfı kart (ör. RTX 3060 12 GB yeni/2. el) + gerekiyorsa PSU |
| Yazılım | ücretsiz | ücretsiz |
| Toplam maliyet | ABD göstergesi ~144 $ SSD [S022]; **TL alınamadı** | ABD göstergesi ~144 $ + 350–460 $ [S016] + PSU; **TL toplam hesaplanamadı** |
| Avantaj | disk kısıtı kalkar; düşük risk | 12 GB VRAM; Vega'ya göre ~8 kat 3DMark grafik puanı [S028]; NVENC; Cycles GPU; CC5 asgarisi (8 GB) |
| Dezavantaj | görüntüyü tek başına değiştirmez | eski kart; 2. el riski; fiyat/performans zayıflamış; PSU/kasa belirsiz |
| Görüntü kalitesi | Plan A ile aynı | gölge/süper örnekleme/volumetric daha rahat; kalite yine yazılım ve varlıktan gelir |
| Render performansı | aynı | gerçek zaman kilidine yaklaşması beklenir (ÇIKARIM) |
| Donanım ihtiyacı | boş M.2/SATA | ~550 W kaliteli PSU, kasa yeri |
| Sürdürülebilirlik | SSD her sistemde taşınır | UE5/yüksek ayarda 2–3 yıl içinde sınır (ÇIKARIM) |

### PLAN C — FİYAT-PERFORMANS ODAKLI YÜKSEK KALİTE
- **GPU adayları (karar değil):**
  - C-a **RTX 5060 Ti 16 GB**: VRAM + NVIDIA ekosistemi + 180 W. ~31 bin TL (Tem).
  - C-b **RTX 5070 12 GB**: en yüksek hız; 650 W; ~42 bin TL (Tem).
  - C-c **RX 9060 XT 16 GB**: yalnız Unity raster hedefliyse en ucuz 16 GB; ~25 bin TL (Tem). Blender/CC5 önemliyse zayıf.
  - ÇIKARIM: Blender/karakter araçları kullanılacaksa C-a dengeli aday; yalnız Unity URP ise C-c ciddi aday. Unity testi olmadan "en iyi" denemez.
- **SSD:** 1–2 TB (zorunlu). **PSU:** kaliteli 650 W (mevcut yetersizse). **Fan:** gerekiyorsa.
- **Yazılım/varlık:** URP, Blender, MPFB, CC0 doku ücretsiz. İsteğe bağlı: CC5 299–498 $ (A02), MHBridge 79,99 $ (A02), hareket araçları (A03), TTS (A07). Lisanslar A12.
- **Toplam maliyet:** kart ~25–42 bin TL (**Temmuz verisi, güncel değil**) + SSD (TL alınamadı) + PSU (TL alınamadı) + yazılım 0–600 $ civarı. **Tek bir TL toplamı verilemiyor.**
- **Avantaj:** yüksek ayarlar, Cycles/OptiX (NVIDIA), CC5 ve MetaHuman denemesi, donanım RT.
- **Dezavantaj:** bellek krizinde pahalı; kalite otomatik gelmez (karakter, ışık, animasyon emeği); UE5'e geçiş büyük yeniden yazım (A09).
- **Görüntü kalitesi (ÇIKARIM):** "iyi oyun sinematiği" için donanım engeli büyük ölçüde kalkar; "gerçek film" garantisi yok.
- **Render performansı:** URP 1080x1920 için bol olması beklenir; **benchmark yok**; kilit kalkmadan gerçek zamandan hızlı değil.
- **Donanım ihtiyacı:** PSU 450–650 W, kasada ~25–30 cm yer (modele göre ölçülmeli).
- **Sürdürülebilirlik:** 16 GB VRAM daha uzun ömürlü (ÇIKARIM); anakart/işlemci PCIe 3.0'da kalır; Windows 11'e geçiş gerekecek.
- **Zamanlama:** kaynaklar fiyat rahatlamasını 2027'den önce beklemiyor [S015, S022]; bu tahmindir.

## Minimum ve önerilen donanım

| | Minimum (Plan A, çalışır) | Önerilen (yüksek kalite hedefi) |
|---|---|---|
| CPU | mevcut 5600GT | mevcut 5600GT (ölçüm darboğaz göstermedikçe) |
| RAM | 16–32 GB yeter (ÇIKARIM); mevcut 64 GB | mevcut 64 GB |
| GPU | mevcut Vega 7 (düşük/orta ayar) | 16 GB VRAM'li, donanım RT'li ayrı kart (12 GB alt sınır) |
| Disk | en az ~30 GB boş tutulmalı (ÇIKARIM) | +1–2 TB SSD |
| PSU | mevcut | kaliteli 550–650 W (karta göre) |
| Anakart | mevcut | mevcut (PCIe 3.0 x16 yeterli) |
| İşletim sistemi | Windows 10 + ESU | Windows 11 (2027 içinde) |

## Karşılaştırma ve çelişkiler
- Ç1: İkinci el 3060 TL: 15.000–37.850 [S018] ↔ 6.200–7.400 [S019]. Çözülemedi.
- Ç2: TL SSD 2.499 [S023] ↔ ABD 1 TB ~144 $ [S022]. TL kaydı tarihsiz; güvenilmedi.
- Ç3: Blender 5600G BMW27 175 sn (3.4) ↔ 248 sn (4.2): sürüm farkı, kıyaslanamaz.
- Ç4: Blender AMD eşiği GCN1+ ↔ GCN4+ (A09). Sürüm farkı olabilir.
- Ç5: Intel resmî listede Ryzen 5000 + 500 serisi destekli [S010] ↔ APU kullanıcıları görüntü kaybı bildiriyor [S011]. Resmî destek hatanın olmadığını göstermiyor.
- Ç6: ESU bitiş 12 ↔ 14 Ekim 2027 [S024].

## Riskler
| ID | Risk | Olasılık/Etki (ÇIKARIM) | Önlem |
|---|---|---|---|
| A10-R1 | Yetersiz/eski PSU ile kart takmak | bilinmiyor / **çok yüksek** (donanım hasarı) | satın almadan PSU modeli öğren; bilgisayarcı |
| A10-R2 | Kart kasaya sığmaz | orta/orta | ölç |
| A10-R3 | Arc + APU görüntü kaybı | orta/yüksek | Arc'tan kaçın veya iade garantisi |
| A10-R4 | 45 GB disk dolar, kayıt yarıda kalır | yüksek/yüksek | disk ön kontrolü (A09), SSD |
| A10-R5 | İkinci el arızalı/sahte kart | orta/yüksek | test ederek elden alım |
| A10-R6 | iGPU'da ağır ayarla sürücü çökmesi/aşırı süre | orta/orta | kademeli aç, ölç, watchdog |
| A10-R7 | Kart alınır, kalite sorunu kodda kalır | yüksek/yüksek | önce Plan A ve ölçüm |
| A10-R8 | Fiyat artışı/stok yokluğu | yüksek/orta | karar anında güncel fiyat |
| A10-R9 | Windows 10 ESU kaydı yapılmamış | bilinmiyor/orta | kontrol |
| A10-R10 | Bulutta Unity Editor lisans ihlali | orta/orta | A12 incelemesi |

## Test önerileri
**Bu bilgisayarda (satın alma öncesi)**
1. T1: 10 sn referans sahne; taban kayıt süresi, kare süresi, tepe RAM, işlemci/GPU kullanımı ve sıcaklık.
2. T2: Aynı sahnede sırayla gölge 2048 → HDR → SSAO → 4x MSAA → 1,5x ölçek; her adımda süre.
3. T3: `targetFrameRate=-1` ile süre farkı (A09 ile aynı test).
4. T4: RAM hızı ve XMP; UMA bellek ayarı açık/oto farkı (BIOS değişikliği kullanıcı onayıyla).
5. T5: Disk envanteri (hangi klasör kaç GB).

**Kabul ölçütü önerisi:** kullanıcı "1 dakikalık video için en çok X dakika beklerim" desin; T2 sonucu X'i aşarsa GPU konuşulur.

**GPU isteyen:** 5–10 sn sahnenin kiralık Windows GPU'da EXE ile kaydı (lisans kontrolünden sonra); kart takılınca GPU-Z (PCIe genişliği), NVENC seçildi mi, Unity hangi kartı kullanıyor.

**Kullanıcıdan istenecekler (Q-001)**
- Güç kaynağı etiketinin fotoğrafı (marka, model, watt, kaç yıllık, 8-pin PCIe kablosu var mı).
- Kasa modeli veya x16 yuvadan öne boşluk (cm) ve genişlik fotoğrafı.
- C: diski M.2 mi SATA mı; boş yuva.
- Bütçe sınırı, ikinci el kabulü, kabul edilen render süresi.

## Öz denetim
- Hiçbir donanım testi yapılmadı; performans yargıları ölçümsüz.
- Unity/URP için bu kartlar veya Vega 7 ile benchmark bulunamadı. Blender/3DMark/Unreal skorları yalnız dolaylı gösterge.
- Güncel TL fiyatları zayıf: GPU 4 ay eski tek C kaynağı; SSD ve PSU TL fiyatı yok. §12'deki "toplam maliyet" B ve C için TL olarak **hesaplanamadı**.
- PCIe x16 yalnız anakart belgesinden; AMD'nin hat sayısı belgesi bulunamadı. 3060/4070/5070 x16 ve B580 x8 genel bilgi.
- RTX 3060 güç/PSU sütunu doğrulanamadı; B580 güç değeri alınamadı.
- TDR, güç dönüştürücü, bulut kapsayıcısının Linux olması gibi maddeler kaynaksız GENEL BİLGİ.
- **Kural sapmaları:** (1) www.techspot.com adresi hata verince aynı yazının www'siz adresini bir kez daha açtım; "engellenen sayfayı tekrar deneme" kuralına aykırı. (2) Dosyalar "her 3–4 aramada bir" yazılmadı, araştırma sonunda yazıldı.
- WebFetch sayfaları özetleyerek verdi; sayılar kelimesi kelimesine doğrulanmış sayılmamalı.
- Arama bütçesi (28/28) doldu.

## Eksik alanlar / açık sorular
- Kasım 2026 TL fiyatları (GPU, SSD, PSU); güvenilir ikinci el fiyatı.
- PSU ve kasa (Q-001); M.2 doluluk; RAM hızı; BIOS sürümü; ESU kaydı.
- Vega 7'de EEVEE ve URP gerçek kare süreleri (Q-003'ün hız kısmı).
- Saatlik Windows bulut GPU'su: sağlayıcı, fiyat, Türkiye'den erişim/ödeme.
- Blender 5.x resmî AMD gereksinimi.
- Elektrik tüketimi/maliyeti (araştırılmadı).
- Q-009: **yanıtlandı** (Vega 7). Güven: yüksek-orta (resmî sayfa "Vega" kelimesini kullanmıyor).

## Diğer ajanlardan beklenen / ortak konular
- A09: gerçek zaman kilidi, ölçüm çıktısı (kare süresi/RAM) kayda eklensin, NVENC ayarları, GPU seçimi.
- A04: kalite profillerine bellek bütçesi; S9'daki itirazlar.
- A02: CC5/MetaHuman denemesi GPU + SSD'ye bağlı.
- A12: Unity bulut maddesi (2.1/2.8), bulut hizmeti koşulları, CC5/MetaHuman.
- A07: yerel TTS/hizalama modellerinin CPU süresi.
- A06: VFX Graph ve volumetric için iGPU bütçesi.
- A11: 2027 bellek fiyatı ve URP RT yol haritası.

## Öneriler (kanıtla)
1. **Şimdi para harcama; Plan A + ölçüm** (kayıt kilidi A09 K7; kod eksikleri A04).
2. **İlk yatırım gerekirse disk** (45 GB; CC5 230 GB ister, A02-S037).
3. **GPU konuşulmadan önce PSU ve kasa öğrenilsin** (Q-001).
4. GPU gerekirse: **en az 12 GB, tercihen 16 GB**; 8 GB alma [S008]; Intel Arc'ı bu APU'lu sistemde riskli say [S011].
5. NVIDIA/AMD seçimi iş akışına göre: Blender/CC5 varsa NVIDIA [S026]; yalnız Unity ise AMD de aday.
6. Mümkünse satın almadan önce tek sahnelik bulut denemesi (lisans kontrolüyle).
7. Windows 10 ESU kaydını kontrol et; Windows 11'i 2027 içinde planla [S024].
8. Nihai karar kullanıcınındır; bu dosya karar değildir.
