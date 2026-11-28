# D1 — Düzeltme araştırması (denetçi K3 ve O1)
Tarih: 2026-11-27. **Test yapılmadı.** Kod değiştirilmedi, indirme yapılmadı, Unity'ye dokunulmadı. Proje yalnız okundu. Web araması: 34 (sınır 40); biri hata verdi. Sayfalar özetleyici araçla okundu. Hukuki görüş değildir. Kaynaklar: `../SOURCES/D1_kaynaklar.md`.

## Görev
A) tam 3D gerçekçi / 3D stilize / 2,5D illüstrasyon / hibrit karşılaştırması. B) O1: iClone, FK, kompozit araçları, kamera hareketleri, Unity CLI ve yetkili ajan erişimi, kullanıcı deneyimi.

## 1. Projede 2D bugün ne durumda (KOD, statik okuma)
- `mekan:` 2D arka planı değiştirir. `BackgroundDef` tek `sprite` alanı taşıyor (katman yok). `Assets/Resources/StoryLibrary.asset` içindeki 5 mekânın 5'inde `sprite` boş: arka planlar kodla (renk geçişi, kutular) çiziliyor (`StoryPlayer.BuildBackground`).
- `CharacterView`: görsel verilirse onu, yoksa kodla çizgi karakter. `EmotionSprite` duygu başına 3 görsel alır: gövde, ağız açık, göz kapalı. Yani ağız iki durumlu.
- Paralaks katmanı için kod görmedim (arama: paralaks/parallax).
- `AtmosferVolume.cs` notu: 2D mekân Canvas Overlay olduğu için post-process görmez; taklit katman kullanılıyor. Plan seçimi (`omuz`, `ters` vb.) yalnız `mekan3d` içinde.
- 3D karakter 2D arka planın üstüne saydam resim olarak bindirilebiliyor (`Sahne3D.cs` baş notu). Hibritin iskeleti bu.
- `DURUM.md` 2D+3D için sorun kayıtları içeriyor (filtre etkisiz, ilk karede çömelme gibi görünme); güncel durumları denenmedi.
- Sonuç: "projede 2D oynatma var" doğru, ama **hazır illüstrasyon görünümü yok**; görsel de, katman da, kamera dili de eklenmeli.

## 2. Dört yolun karşılaştırması
Kalite, emek ve performans satırları **ÇIKARIM**; hiçbiri sınanmadı.

| Ölçüt | A. Tam 3D gerçekçi | B. 3D stilize (toon / boyama) | C. 2,5D illüstrasyon (katmanlı paralaks) | D. Hibrit (Blender'da önceden render arka plan + Unity karakter) |
|---|---|---|---|---|
| Ulaşılabilir görünüm | en iyi ihtimalle oyun sinematiği; fotogerçek insan beklenmez (A11) | tutarlı "oyun/çizgi film" görünümü; gerçekçilik kıyası baskısı azalır | tek tek kareler çok güzel olabilir (görsele bağlı); hareket kısıtlı | arka plan derinlikli ve zengin; karakterle ışık uyuşmazlığı riski |
| Hareket, yürüme, açı değiştirme | serbest | serbest | zayıf: her açı/poz yeni çizim ya da kukla; yürüme zor | karakter serbest, **kamera her arka plan için sabit** |
| Örnek (stüdyo düzeyi; ulaşılacak vaat değil) | MetaHuman demoları | Arcane tarzı boyama 3D [S028, yalnız başlık] | Klaus tarzı 2D [S028, yalnız başlık], hareketli çizgi roman | eski sabit kameralı oyunlar; küçük ekip örneği [S020] |
| Yazılım maliyeti | 0 + (karakter araçları ücretli olabilir) | 0 (MToon/UniVRM projede; Toon Shader) | 0 (Unity 2D) ; isteğe bağlı Spine 69–379 $ [S015], Live2D [S014] | 0 (Blender) |
| Asıl maliyet | karakter, ışık, animasyon emeği | malzeme ve doku tarzını tek tek uydurma | **görsel üretimi**: her karakter x duygu x poz, her mekân x katman | mekânı Blender'da kurma + kamera eşleme |
| Bu PC (Ryzen 5600GT, tümleşik Radeon) | en riskli | büyük olasılıkla A'dan hafif | büyük olasılıkla en hafif | oynatma hafif; arka plan render'ı CPU Cycles ya da Eevee (Vega'da Cycles GPU yok [S019]); süre ölçülmedi |
| Mevcut projeye uyum | en yüksek (bugünkü 3D hat) | yüksek: aynı sahne/kamera/hareket; VRoid karakterleri zaten anime tarzı. Rocketbox gerçekçi dokulu, toon'a çevrilince sonuç bilinmiyor | orta-düşük: 2D oynatıcı var ama kamera/kurgu/post 3D hattında; katman ve kamera yeni kod | orta: 2D sprite mekân + 3D karakter bindirme var; derinlik, zemin gölgesi, kamera eşleme yok |
| Lisans | A12 tablosu | MToon/UniVRM MIT [S013]; Toon Shader lisansı okunmadı; VRoid model başına | çizim kimin ise onun koşulu; Spine/Live2D ayrı koşul | Blender çıktısı kullanıcının [S017]; kullanılan doku/model lisansları ayrı |
| "Yapay zekâsız" şartı | uyar | uyar | **elle çizim ya da lisanslı hazır görsel gerekir**; görsel üreten yapay zekâ kullanılacaksa kullanıcıya sorulmalı (Q-015) | uyar |
| Yeni karakter/mekân ekleme | kütüphane geniş (Rocketbox 115) | VRoid ile kolay; gerçekçi olmayan mekân varlıkları (low-poly) daha uyumlu olabilir | en pahalı | orta |

### Değerlendirme (ÇIKARIM)
- **B en düşük riskli ikinci görünüm yolu:** mevcut 3D hareket, kamera ve ses işi aynen kullanılır; geri dönülebilir. Önerilen sınama: aynı 3 saniyelik sahneyi (a) bugünkü, (b) ışık/gölge iyileştirilmiş gerçekçi, (c) VRoid + MToon + boyama tarzı mekân olarak üretip kullanıcıya kör seçtirmek. Test planında stilize görünüm testi yok; eklenmeli.
- **C'yi ana yol yapmak için görsel kaynağı çözülmeli.** Duygulu CC0 fantastik karakter takımı bu kısa aramada bulunamadı (kapsamlı tarama değil).
- **D, tek mekân pilotu olarak anlamlı**; hareketli kamera isteyen çekimlerde kullanılamaz.
- A ve B birlikte aynı hatta yaşayabilir (görünüm profili); C ve D ek kod ister.
- Stilizenin izleyicide daha iyi karşılandığına dair kanıt **bulmadım**. Okunan tek çalışma stilizeyi sınamamış [S021].

## 3. Unity CLI ve "Authorized Agentic Access" (Q-016)
### Resmî metin
- Terms §17.2(ff): Unity "Offerings" ile yapay zekâ ajanı, büyük dil modeli, komut satırı vb. yoluyla etkileşim, yetkili ajan erişimi dışında yasak [S001].
- Yetkili erişim üç koşul ister [S001]: (i) Unity'nin işlettiği/belirlediği geçit ya da arayüz, **Unity CLI dahil**; (ii) ajan/model/MCP o anda Unity'ce yetkili (liste, ek koşul ya da yazılı onay); metin ayrıca **sizin kendi işlettiğiniz ajanların Unity CLI üzerinden yetkili olduğunu** söylüyor; (iii) koşullara ve teknik/kimlik doğrulama sınırlarına uyum. Unity yetkiyi dilediğinde değiştirebilir/geri alabilir.
- Çıkarım: kullanıcının kendi Claude'unun **Unity CLI üzerinden** Editor sürmesi metne göre yetkili görünüyor. Bugünkü `komut.txt` köprüsü Unity'nin belirlediği bir arayüz değil; yetkili sayılacağına dair dayanak yok.
### Ücret ve Personal
- CLI ve MCP ücretsiz (personel, 15 Tem 2026) [S007]; Unity AI aboneliği gerekmez [S006].
- "Personal planda serbest" diye açık bir resmî cümle **görmedim**. Ücretsiz ve plan şartı belirtilmemiş; Personal'da çalışacağı çıkarımdır. Personal gelir eşiği 200.000 $ [S002].
- **Arayüzsüz** (batch/headless) ajan çalıştırma Build Server lisansı ister; Personal bunu kullanamaz [S002]. Yani Editor normal penceresiyle açık olmalı.
### Kurulum (yapılmadı)
1. Windows 10 21H1+ [S004] (cihazın yapı numarası kontrol edilmeli).
2. CLI: güncel Unity Hub ile gelir [S003] ya da `winget install Unity.CLI` / kurulum betiği [S004][S011].
3. `unity auth login` (tarayıcıda Unity hesabı) [S004] — **kullanıcı yapmalı**.
4. Projede `unity pipeline install` → `com.unity.pipeline` paketi eklenir (Unity 6.0+) [S005]. **Yeni paket: kullanıcı onayı gerekir** (proje kuralı).
5. `unity setup` Claude Code eklentisini kurar [S003]; `unity status`, `unity command` ile bağlanılır [S005].
6. Bağlantı yalnız aynı bilgisayardan (localhost) [S006]: komutlar kullanıcının bilgisayarında çalıştırılmalı.
- Mevcut köprü komutları Pipeline "özel komut" olarak sarılabilir görünüyor [S005][S011]; bu **kod değişikliğidir**, yapılmadı.
- `eval` rastgele C# çalıştırır; geniş yetkidir. CLI anonim hata/kullanım bildirimi gönderiyor, kapatılabiliyor [S011].
### Durum ve çelişkiler
- CLI 1.0 ve Pipeline 17 Kas 2026'da betadan çıktı [S003]; bazı yazılar hâlâ "beta/deneysel" diyor [S009][S010] (daha eski).
- Kullanıcı deneyimi: Editor bazen odak ister, açılır pencereler ajanı durdurur, çağrılar yavaş olabilir (~0,8 sn, tek ölçüm) [S009]; Codex+Rider'da klasör hatası [S007].
- Açık kalan: köprünün kendisi (Editor içinde dosya okuyan kod) CLI'siz kullanılırsa durum ne olur; kullanıcının elle tetiklediği menü komutlarında Claude'un yalnız dosya yazması nasıl sayılır. Personel "Editor'ü sürüyorsa koşullar devreye girer" demişti [S008]. Netlik için Unity'ye yazılı soru.

## 4. O1 eksikleri
### 4.1 iClone 8
- Ne: Reallusion'ın gerçek zamanlı karakter animasyon programı. En az GTX 900 / Radeon R7 300 ve 4 GB [S022]; tümleşik Radeon'un karşılayıp karşılamadığı belli değil.
- Ücretsiz Personal: en çok 30 3D dışa aktarım, her biri 30 sn; gerçek zamanlı/RTX render çıktısı yalnız kişisel/sosyal; ticari yok [S023]. Sürekli lisans fiyatı alınamadı [S024].
- Uyum: Unity'ye FBX ile; ek bir üretim adımı, uygulamanın otomatik hattına girmiyor (ÇIKARIM). Dağıtım koşulları A12'de Reallusion kısıtı (C-009). **Şimdilik önerilmez** (ÇIKARIM). Alternatif: Rocketbox + UAL + Blender.
- Yapay zekâsız: temel animasyon araçları uyar; AccuFACE gibi yüz yakalama eklentileri yapay zekâ kullanabilir (doğrulanmadı).
### 4.2 FK
- FK (ileri kinematik): eklemler tek tek döndürülür; hazır animasyon klipleri böyle oynar. IK: el/ayak hedefi verilir, eklemler hesaplanır. Bu tanım genel bilgidir; resmî Unity sayfası bu oturumda **açılamadı** (izin isteği yanıtsız kaldı).
- Projede `Scripts/` içinde `OnAnimatorIK`/`SetIKPosition`/Animation Rigging araması sonuç vermedi; klipler (FK) + kodla poz düzeltme (`KolPozu.cs`, incelenmedi) kullanılıyor görünüyor. Öneri (ÇIKARIM): temel hareket FK klip; yalnız ayak basma, uzanma, bakış için IK.
### 4.3 Kamera pan / tilt / orbit / crane / takip
Cinemachine 6.6 kurulu; kodda `Unity.Cinemachine`/`CinemachineCamera` kullanımı bulamadım. Projenin kendi kamerası var (`KameraYonetmen.cs`).
| Hareket | Kendi kodda yaklaşım | Cinemachine karşılığı [S027] |
|---|---|---|
| Pan / tilt | kamera sabit, yatay/dikey dönüş eğrisi | Pan Tilt |
| Orbit | hedef etrafında açı ve mesafe | Orbital Follow |
| Dolly | konumu eksen boyunca kaydırma | Spline Dolly |
| Crane | yüksekliği değiştirirken hedefe bakma | Spline Dolly (dikey eğri) + Rotation Composer |
| Takip | hedefe sönümlü uzaklık | Follow + Rotation Composer |
| El sarsıntısı | küçük gürültü | noise |
Öneri (ÇIKARIM): ilk aşamada mevcut kameraya süre+eğrili hareket türleri eklemek daha küçük iş; deterministik kayıt denetlenmeli. Cinemachine'e geçiş ayrı karar. Hiçbiri denenmedi.
### 4.4 Render sonrası kompozit
| Araç | Lisans/ücret | Bu PC | Not |
|---|---|---|---|
| DaVinci Resolve 21 (Fusion içinde) | ücretsiz sürüm; özel lisans, koşul metni okunmadı | tümleşik GPU kabul, en az 4 GB VRAM [S025]; yeterliliği ölçülmedi; disk dar | elle renk/kurgu için |
| Natron | GPL-2.0+ [S026] | bilinmiyor | son kararlı sürüm 2022: bakım zayıf görünüyor |
| Blender Compositor | GPL; çıktı kullanıcının [S017] | disk gerekir | komut satırıyla otomatikleşebilir (genel bilgi; kılavuz sayfası robots engeliyle okunamadı) |
| ffmpeg (projede var) | A12 | uygun | LUT, gren, vinyet gibi basit işlemler otomatik hatta (genel bilgi) |
Öneri (ÇIKARIM): şimdilik yeni araç gerekmez; görüntü işlemleri Unity post + ffmpeg. Lens distortion URP post-process bileşenidir (genel bilgi, doğrulanmadı). Upscaling/image reconstruction: **araştırılmadı** (aşağıda).
### 4.5 Kullanıcı deneyimi
- Reddit: alan adı sınırlı arama hata verdi (proxy HTTP 400); genel aramada Reddit kaydı çıkmadı. **Reddit deneyimi yok.**
- Bir Unity forum başlığı (kısa film deneyimi) izin yanıtsız kaldığı için açılamadı; tekrar denenmedi.
- Alınabilenler: [S007] [S008] [S009] (CLI), [S020] (önceden render arka plan: sanat işi beklenenden çok uzun; önce küçük dikey dilim).
- 2D'ye karşı 3D hikâye videosu için güvenilir kullanıcı deneyimi bulunamadı (çıkanlar pazarlama yazıları; kullanılmadı).

## 5. Araştırılmadı (neden · etki · alternatif)
| Konu | Neden | Etki | Alternatif |
|---|---|---|---|
| Stilize/2D görünümün bu PC'de hızı ve kalitesi | test yok | tablo tahmin | 3 görünümlü kısa deneme |
| Lisanslı hazır 2D karakter/mekân paketleri, çizer ücreti | arama zayıf döndü, bütçe | C yolunun maliyeti bilinmiyor | kullanıcı tarzı seçerse hedefli arama / teklif |
| Toon Shader ve Unity 2D paketlerinin lisans metni, Unity 6000.6 uyumu | okunmadı | dağıtım riski bilinmiyor | kurmadan önce oku |
| Reddit, Stack Overflow, video demonstrasyonları | erişim/izin, bütçe | kullanıcı deneyimi zayıf | tarayıcıyla elle bakma |
| iClone sürekli lisans fiyatı; Resolve EULA | sayfada yok / okunmadı | maliyet/lisans eksik | satın alma gündeme gelirse |
| Upscaling, image reconstruction | görev listesi dışında kaldı, bütçe | kayıt hızı seçeneği değerlendirilmedi | ölçümden sonra |
| Reklam, robotik, simülasyon, bilgisayarlı görü, mimari görselleştirme sektörleri | bu görevde yok | protokol §4 eksik | ana araştırmacı karar versin |
| Ücretli gerçekçi karakter/mekân paketleri, gerçekçi ejderha | bu görevde yok | Plan C eksik | ayrı arama |
| Düz yazıdan otomatik sahne kurma; 4K hedefi; çocuk karakter | bu görevde yok | `DURUM.md` kullanıcı hedefi karşılanmadı | mimari revizyonunda ele alınmalı |
| Kodda kare kare doğrulama (2D oynatma) | Unity'ye dokunulmadı | 2D'nin bugünkü hâli görülmedi | kullanıcı elle önizleme |

## 6. Öz denetim
Kalite/performans satırları tahmin. Stüdyo örnekleri yalnız başlık düzeyinde. Unity koşul metni özetleyiciyle okundu; hukuki netlik için asıl metin ve Unity yanıtı gerekir. VRoid/MToon uyumu `Karakter3D.cs` notuna göre ışıkta "beyaza kaçma" sorunu yaşamış; stilize yolda ayar gerekecek.

## 7. Dosya düzeltmeleri (iş B)
Aşağıdaki dosyalar güncellendi: 04, 05, 06, 07, 12, 14, 15, REPORTS/00, R01, R02, R04, R05, R08, R09, R10, R13. 00, 02, 03, 16'ya dokunulmadı (ana araştırmacı).
