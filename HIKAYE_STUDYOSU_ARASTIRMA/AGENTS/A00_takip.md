# A00 — Takip ajanı (eksik kalan konular)
Tarih: 2026-11-27. Kaynaklar: `SOURCES/A00_kaynaklar.md`. "ÇIKARIM" = benim yorumum. "DOĞRULANAMADI" = kanıt bulamadım. Hiçbir test yapılmadı, kod yazılmadı, bir şey indirilmedi.

## Görev tanımı
1. ve 2. dalgada arama hakkı bittiği için kalan boşlukları kapatmak (Q-012, Q-014 ve ajanların "eksik alanlar" listeleri). Arama sınırı: 40.

## Araştırma alanı ve soruları
S1 altyazı zamanlama/okunurluk, loudnorm, TikTok ses düzeyi · S2 ayak kayması, motion matching, video mocap lisansı · S3 OVR LipSync ve alternatifleri · S4 URP saç/göz/cilt, hazır karakter · S5 SIGGRAPH, virtual production, NeRF · S6 diğer kritik "doğrulanamadı" maddeleri.

## Bulgular

### S1 — Altyazı ve ses düzeyi (kapatılan eksik: A07 §Öz denetim, A08, Q-012)

**Netflix Türkçe kılavuzu** [A00-S001]: en çok 2 satır; satır genişliği "ekrana 42 karakter sığacak" ölçüde; okuma hızı yetişkin ≤17 karakter/sn (çocuk ≤13; işitme engelli altyazısında 20/17); 1 sn alt süre teşvik edilir. Türkçe satır kırma: bağlaçtan önce; ulaç, zarf, edat ve noktalamadan sonra; sıfat-ad ve fiil-yardımcı fiil ayrılmaz; "gibi, diye, benzeri" öncesindeki sözden koparılmaz; tek kısa sözcüklü satırdan kaçınılır.

**Netflix zamanlama kuralları** [A00-S002]: giriş sesin ilk karesinde (1-2 kare sapma kabul); ardından altyazı yoksa çıkış sesten en az yarım sn sonra (30 fps'te 15 kare); altyazılar arası en az 2 kare; yarım sn'den kısa boşluklar 2 kareye kapatılır; en kısa süre 20 kare (0,8 sn); kesmeye yarım sn'den yakın giriş/çıkış kesmeye hizalanır. (Dil kılavuzundaki "1 sn teşvik" ile birlikte okunmalı.)

**BBC (ayna sayfa, resmî alan değil)** [A00-S003]: 160-180 sözcük/dk (sözcük başına ~0,33-0,375 sn); konuşandan en çok 1,5 sn erken/geç. **9:16 için:** satır en çok video genişliğinin %90'ı (kabaca ~25 karakter), en çok 3 satır, metin dikeyde orta %75 ve yatayda orta %90 içinde, alt üçte birde ama normalden biraz yukarıda (yüzler üst yarıda), satır yüksekliği video yüksekliğinin %3,9-4,5'i. ÇIKARIM: 1920 yükseklikte ~75-86 px satır yüksekliği.

**Karaoke okunurluğu:** kelime kelime vurgunun normal altyazıdan daha iyi anlaşıldığını gösteren bağımsız, hakemli kanıt **bulunamadı**. Bulunanlar dolaylı: zaman eşli anahtar sözcük vurgusu öğrenme için sevildi ama günlük izlemede fazla dikkat dağıtıcı bulundu (n=49) [A00-S004]; sözcük boyutu vurgusu yalnız anadil olmayanlarda işe yaradı [A00-S005]. "Karaoke izlenmeyi artırır" iddiaları araç satıcılarının sayfaları (D) — karar dayanağı yapılmadı.
ÇIKARIM öneri: varsayılan = 2-4 sözcüklük sabit sayfa + yalnız o an söylenen sözcükte sade renk vurgusu (zıplama/büyüme yok); kullanıcı "vurgusuz" seçebilsin; zaman güveni düşükse vurgu kapansın (A07 kuralıyla uyumlu). İki biçim kullanıcıya A/B izletilsin.

Kısa dikey video için denetçi eşikleri (ÇIKARIM, yukarıdakilerden uyarlama): CPS ≤17 · satır ≤~25 karakter · ≤3 satır (hedef 1-2) · sayfa ≥20 kare · giriş hatası ≤2 kare (≈67 ms; A07'nin medyan ≤50 ms hedefiyle uyumlu) · son sözcükten sonra ≤+0,5 sn tutma.

**ffmpeg loudnorm** [A00-S007]: I −70…−5 (varsayılan −24), TP −9…0 (varsayılan −2), LRA 1…50 (varsayılan 7). `linear=true` (varsayılan) için measured_I, measured_LRA, measured_TP, measured_thresh birlikte verilmeli (iki geçiş: ilk geçiş `print_format=json`).
**Kaynak koddan (8.0) kritik ayrıntı** [A00-S008]: doğrusal mod yalnız (ölçülen TP + gerekli kazanç) ≤ hedef TP **ve** ölçülen LRA ≤ hedef LRA ise seçilir; aksi hâlde sessizce dinamik moda geçer ve dinamik modda örnekleme 192 kHz'e zorlanır. ÇIKARIM: TTS konuşmasında tepe yüksekse −14 LUFS / −1 dBTP hedefi dinamiğe düşürebilir; (a) `LRA` hedefi ölçülenin altında bırakılmamalı, (b) çıkışa `-ar 48000` açıkça verilmeli, (c) rapordan hangi modun kullanıldığı denetlenmeli, gerekirse önce tepe sınırlanmalı.
**TikTok ses düzeyi:** resmî reklam teknik sayfasında LUFS şartı yok [A00-S009]; organik video için resmî hedef bulunamadı. A07'nin yargısı (−14 LUFS / −1 dBTP bir tercih, platform kuralı değil) değişmiyor.

### S2 — Ayak kayması, motion matching, videodan hareket (kapatılan eksik: A03 "arama bütçesi doldu", Q-012)

**Root motion (Unity 6000.5 resmî)** [A00-S010]: klibin XZ hareketi "Bake Into Pose" KAPALI ise karakteri taşır (yürüme/koşma); açık ise poza gömülür (idle için önerilir). Y için "Based Upon: Feet" yüzme sorununu azaltır; `OnAnimatorMove` ile kök hareketi kodla uygulanabilir (kod örneği araç özetinin eki, sayfada değil). ÇIKARIM (A03 sırasını doğrular): (1) yürüme/koşmada ilerlemeyi klibin kök hareketinden al; (2) kodla sürülecekse oynatma hızı = istenen hız / klibin doğal hızı; (3) kalan kayma için duruş fazında ayak IK kilidi; (4) denetçi: temas sırasında ayağın yatay hızı (cm/sn).

**Motion matching:** JLPM22/MotionMatching — MIT, Unity 6+, ayak kilidi ve inertialization ayarı var, BVH tabanlı veri ister [A00-S011]; son commit tarihi okunamadı, test edilmedi. Unity'nin yerleşik motion matching'i için güncel resmî kaynak bulunamadı (DOĞRULANAMADI). ÇIKARIM: motion matching klavye/oyun kolu gibi canlı yön girdisi için tasarlanır; bizde yol ve zaman önceden belli. Kısa vadede **gerekmez**: zengin ve tutarlı mocap veritabanı ister, kontrolü zorlaştırır. Hazır klip + root motion + ayak IK yeterli ilk adımdır.

**MediaPipe Pose:** üç model (lite/full/heavy), 33 nokta, metre cinsinden "world landmark" [A00-S012]; model kartı Apache-2.0 diyor (Google'ın paket içi model kartı PDF'ine dayanarak, topluluk aynası) [A00-S013]. Sınırlar: kare başına tek kişi; 4 m'den uzak kişi kapsam dışı; derinlik (Z) yalnız göreli, metrik değil [A00-S013]. ÇIKARIM: MediaPipe ham poz verir; iskelete dönüştürme, kök hareketi, ayak teması ve parmak düzeltmesi ayrıca yazılmalı. Doğrudan "videodan animasyon" çözümü değil.

**Hazır video→animasyon araçları (lisans tuzağı):**
- video2mocap: depo Apache-2.0, ayak kilidi var; varsayılan motor ≥8 GB NVIDIA + Linux/WSL2; GVHMR/PromptHMR arka uçları SMPL nedeniyle yalnız araştırma [A00-S014]. Bu PC'de çalışmaz.
- RGB MoCap (Windows, yerel): kod MIT ama alttaki modeller **ticari olmayan** (GVHMR vb.) [A00-S015, tek ikincil kaynak]; donanım şartı yazılmamış. ÇIKARIM: kullanılırsa çıktı hakkı riskli; A12 denetlemeli.
- ÇIKARIM: "kod MIT" ifadesi yeterli değil; model/ağırlık/vücut modeli (SMPL) lisansı ayrıca okunmalı.

### S3 — Dudak senkronu araçları (kapatılan eksik: A07 §S5, Q-014)
- **OVR LipSync:** Meta'nın Unity sayfası artık "Archive" bölümünde ve "güncellenmiyor, kaldırılabilir" uyarısı var; sayfadaki açık "end-of-life" cümlesi Spatializer eklentisi içindir, LipSync için açık EOL cümlesi **görülmedi** [A00-S016]. İndirme sayfası sürüm 1.16.0 gösteriyor; sayfadaki tarih 2017 (güvenilirliği belirsiz) [A00-S017]. Lisans metni okunamadı (DOĞRULANAMADI).
- Meta'nın yeni "Audio To Expression" özelliği eski kütüphaneden daha hafif diyor ama yalnız Quest başlıklarında (Movement SDK) [A00-S018]; bizim Windows uygulaması için alternatif değil.
- **Sonuç (Q-014):** yeni bağımlılık yapılmamalı (A07 yargısı güçlendi).
- **uLipSync:** MIT; Burst + Mathematics; BakedData + Timeline; varsayılan kalibrasyon A-I-U-E-O + gürültü, ünsüz eklenebilir ama belgede ünsüz kalibrasyon adımı yok [A00-S019]. Unity sürüm desteği ve son sürüm tarihi sayfada görülmedi.
- **Rhubarb:** lisans "other" (LICENSE.md), son itme 2024-01-11 [A00-S020, ikincil dizin]. Metnin kendisi okunmadı.
- ÇIKARIM: A07'nin birincil önerisi (hizalama fonemi → Rocketbox viseme, kendi sürücü) değişmiyor; uLipSync yedek.

### S4 — URP saç/göz/cilt ve hazır karakter (kapatılan eksik: A02 madde 1-2, A04 madde 7)
| Kaynak | Ne var | Lisans | Uyum notu |
|---|---|---|---|
| Unity URP-Defender-Character-Demo [A00-S021] | Marschner saç, pre-integrated cilt, parallax göz | Karakter yalnız eğitim/ticari olmayan; teknoloji kısmı Asset Store EULA (LICENSE.md okunmadı) | Unity 2021.2+ / URP 12-14; Unity 6.6 Render Graph ile uyum ŞÜPHELİ (ÇIKARIM) |
| AkiKurisu/IllusionRP [A00-S022] | Kajiya-Kay + Marschner saç, OIT, SSS cilt (göz yazılmamış) | Unity Companion License | Unity 6.3 / URP 17.3; Windows'ta **DX12** (bu PC D3D11); "yüksek donanım" hedefli |
| itsFulcrum URP Hair [A00-S023] | saç kartı, anizotropik parlama, sahte SSS | CC0 | Unity 2021.3; geliştirme beklenmiyor; en serbest başlangıç noktası |
| Shader Graph örnekleri 17.5 [A00-S024][A00-S025] | listede saç/göz/cilt örneği YOK (Production Ready: bitki, kaya, su, hava) | Unity | Alt sayfalar tek tek okunmadı |
| Unity 2026 render stratejisi sayfası [A00-S026] | URP için saç/göz/cilt vaadi görülmedi (SSR vb. var) | — | yokluk kanıtı değil |

ÇIKARIM: URP'de "tak-çalıştır, bakımlı, serbest lisanslı" saç+göz+cilt üçlüsü **bulunamadı**. Yol: Defender demosunu yalnız teknik referans olarak incelemek, gerekirse sade kendi göz (parallax + yansıma) ve saç kartı shader'ı.

**Mağaza karakterleri:** örnek bir insan paketi 35 $, URP uyumlu, 2022 tarihli, ARKit/blendshape bilgisi yok [A00-S027]. Gerçekçi + ARKit 52 + fantastik kıyafetli + net Unity 6 uyumlu belirli bir paket arama ile **doğrulanamadı**; sayfalar tek tek elle kontrol edilmeli.

**Asset Store EULA (Q-011 benzeri, önemli)** [A00-S028, araç özeti, kelimesi kelimesine değil]: varlık, özgün içeriği olan bir ürünün içine gömülü dağıtılabilir; ancak **birincil amacı kullanıcı içeriği üretmek olan** üründe varlıktan gelir elde etmek açık izin ister; son kullanıcının varlığı aktarmasını sağlamak sınırlıdır. ÇIKARIM: Hikâye Stüdyosu bir içerik üretme aracı sayılabilir → satılacak/ücretli olursa mağaza varlıkları için yayıncıdan yazılı izin gerekebilir. Hukuk görüşü değildir. **A12 (lisans denetimi) esas alınmalı:** A12 madde metninin para kazanmayı kısıtladığını, SSS'de ise daha geniş bir yasak olduğunu ve temkinli olanın uygulanması gerektiğini yazıyor; yani "ticari değil" olmak tek başına güvence sayılmamalı.

### S5 — SIGGRAPH 2025/2026, virtual production, NeRF (kapatılan eksik: A11 öz denetim, A03 "SIGGRAPH literatürü")
Sistematik bildiri taraması DEĞİL; hedefli birkaç arama.

| Çalışma | Ne yapar | Bizim için |
|---|---|---|
| StableMotion (SIGGRAPH Asia 2025) [A00-S029] | bozuk mocap'i temizleyen model; ayak kayması dahil, kayma ölçüsü iyileşiyor | Kod/donanım şartı okunan bölümde yok; yakın vadede kullanılmaz. "Ayak kayması ölçülür" fikri denetçiye alınır |
| Sony posteri: eş-konuşma jesti + yüz (SIGGRAPH Posters 2025) [A00-S030] | metin parçalarını 500 örneklik jest kütüphanesiyle anlam benzerliğinden eşleştirir; anime tarzı karakter; 25 kişilik testte üretici modele (EMAGE) tercih edildi (%61'e %16) | **En uygulanabilir fikir:** üretmek yerine etiketli jest klibi SEÇMEK. ÇIKARIM: biz duygu/niyet etiketi + kuralla da seçebiliriz (modelsiz). Kod yayımlanmamış görünüyor |
| FilmAgent (arXiv 2501.12909) [A00-S031][A00-S032] | Unity'de 15 mekân, hazır eylemler, 9 plan tipi; LLM "yönetmen/kameraman" + kural denetimi; kod bağlantısı var (lisans görülmedi) | Plan sözlüğü + denetçi yaklaşımı A01/A08 ile aynı yönde. LLM kullanımı Q-015'e bağlı |
| FilmFlow (arXiv, Eki 2026) [A00-S033] · CinemaTraj (arXiv, Tem 2026) [A00-S034] | kamera hareketini atomik işlemlerden (orbit, pan, dolly, crane, sabit) kurar; FilmFlow'da "eleştirmen" döngüsü; hakemli yayın bilgisi yok | ÇIKARIM: kamera planı için küçük, sabit bir hareket sözlüğü + render sonrası otomatik kontrol |

- **Virtual production / 3DGS:** yeniden ışıklandırılabilir Gaussian splatting yaklaşımı çekilmiş özel veri ister, bildirilen kullanım <5 GB VRAM [A00-S035]. ÇIKARIM: tümleşik kartlı bu PC'de ve elle çekim verisi olmayan projede anlamlı değil.
- **NeRF:** ayrı, derin okuma yapılmadı (yalnız arama başlıkları). ÇIKARIM: NeRF/3DGS "var olan gerçek yeri yeniden kurma" aracıdır; hayali fantastik mekân, ışık ve çarpışma kontrolü isteyen bu iş için temel yol değil. LED duvarlı virtual production bizim durumumuzda karşılıksız; alınacak ders "önizleme ve son görüntü aynı sahne verisinden".

### S6 — Diğer "doğrulanamadı" maddeleri

| Konu (eksik sahibi) | Bulgu | Durum |
|---|---|---|
| Kayıtta ses saati (A07 açık soru 4) | `AudioRenderer` çevrim dışı kayıt içindir; ses ana iş parçacığında, kare yakalama hızına bağlı örnek sayısıyla alınır [A00-S036]. Kod okuması: `Uygulama/FfmpegPipeRecorder.cs:198-203, 277-281` zaten `AudioRenderer` + `captureDeltaTime` kullanıyor; `Assets/HikayeStudyo` içinde `timeSamples` geçen kod bulunmadı | Yaklaşım doğru görünüyor; `timeSamples`'ın kare ile uyumu DOĞRULANAMADI → test (A07 T3). Editördeki Unity Recorder yolu ayrıca ölçülmeli |
| Türkçe hizalama doğruluğu (A07, Q-007) | MFA 3.0 Türkçe eğitim verisi ~127,8 saat; karşılaştırmalı doğruluk Türkçe için YOK. İngilizcede ortalama sınır hatası MFA ~20-25 ms, MMS ~43-50, WhisperX ~110 ms [A00-S037] | MFA'yı ilk aday yapmayı destekler; Türkçe test şart |
| Türkçe TTS kalitesi (A07) | Sestek'in nesnel karşılaştırması: açık modellerde tek kazanan yok, insan dinleme testi yok [A00-S038]. EMA Lightning (8,6M, Apache-2.0, tek ses) düşük kelime hatası iddiası model yazarının kendi ölçümü; doğallık tahmininde Gemini 3.8 ve Trendyol-TTS'in altında [A00-S039] | Bağımsız dinleme testi hâlâ yok; EMA duygulu çok karakterli iş için tek sesli olduğundan sınırlı |
| Türkçe viseme (A07 tablo) | AWS Polly resmî tr-TR fonem→viseme tablosu: b,p,m→p · f,v→f · d,t,n,l,r→t · s,z→s · c,ç,ş,j→S · k,g,h→k · y→i · ünlüler a,e/E,i,o/O,u; ı→i, ü→u [A00-S040] | A07 taslağıyla büyük ölçüde uyumlu (fark: h; Polly k sayıyor). Tablonun bazı örnek sözcükleri hatalı; rehber olarak kullan |
| Azure tr-TR viseme (A07) | Viseme sayfası dil listesini vermiyor; liste sayfasının okunan kısmında bölüm yoktu [A00-S041] | DOĞRULANAMADI |
| Gemini TTS ücretsiz kota (A07) | Resmî hız sınırı sayfası (2026-10-28) TTS için günlük sayı yayımlamıyor, AI Studio'ya yönlendiriyor [A00-S042]. Üçüncü taraf: 3.8 Flash TTS ücretsiz katmanda var, girdiler insan incelemesine açık olabilir [A00-S043] | "~10 istek/gün" doğrulanamadı; kullanıcı AI Studio'dan bakmalı |
| Unity 6.7 LTS (A04, A09) | 30 Eki 2026 resmî yazıda henüz çıkmamış, "yakında" (Q4) [A00-S044]. Unity resmî yol haritası: 7.x özellikleri 2027, ilk 7 LTS 2027 sonu tahmini; URP 7.x listesinde hacimsel sis ve ışın izlemeli aydınlatma var, saç/göz/cilt anılmıyor [A00-S045] | 27 Kas itibarıyla çıkış duyurusu görülmedi (yokluk kanıtı değil) |
| Compute Light Baker iGPU (A04 soru 3) | URP/HDRP; donanım ışın izleme şart değil (compute ile taklit, Progressive GPU'dan yavaş); sayfada en az bellek/tümleşik kart şartı yok [A00-S046] | Vega 7'de çalışır mı DOĞRULANAMADI → test |
| URP motion blur nesne desteği (A01 madde 3) | "Camera and Objects" modu GameObject hareket vektörlerini kullanıyor; aynı sayfanın girişinde "yalnız kamera" diyen çelişkili cümle var [A00-S047] | Karakterde gözle test gerekir (6000.5 belgesi) |
| Rocketbox yüz (A02, Q-006) | Microsoft HeadBox araç yazısı: 15 viseme, 48 FACS, 30 Vive yüz şekli [A00-S048] | A07'nin viseme yaklaşımını destekler; lisansı görülmedi. Kullanıcı kalite şikâyeti kaynağı bulunamadı |
| MetaHuman dağıtımı (Q-011, C-010) | Resmî sayfa: 1M $ altı ücretsiz, herhangi bir motorda kullanılabilir; oluşturma yalnız Unreal'da [A00-S049][A00-S050]. Uygulama içinde karakter kütüphanesi sunma ve yapay zekâ kısıtı SSS cevapları okunamadı | **Q-011 AÇIK** |
| Mixamo 2026 durumu (A03) | Üçüncü taraf (Eyl 2026): hâlâ çalışıyor ve ücretsiz, kapanış duyurusu yok; Adobe SSS 2021'den; ham dosya dağıtılamaz [A00-S051] | C düzeyi; A03 yargısı değişmez |
| TikTok organik güvenli alan (C-007) | Aramada yalnız üçüncü taraf ölçüm/araç sayfaları çıktı; resmî organik rehber görülmedi | DOĞRULANAMADI → gerçek telefonda kontrol |
| Blender 5.x + Vega EEVEE (A10) | Arama bu yongaya özel resmî bilgi vermedi | DOĞRULANAMADI |

## Teknoloji kayıtları
Biçim: ad · ne yapar · katkı · Unity uyumu · donanım · maliyet · lisans · +/− · alternatif · kaynak · test · güven. Doğrulama 2026-11-27.
1. **ffmpeg loudnorm** · EBU R128 düzeyleme · kayıt sonrası ses · harici (ffmpeg borusu var) · CPU · 0 · LGPL/GPL · + iki geçiş / − koşul sağlanmazsa dinamik + 192 kHz · önceden sabit kazanç · S007,S008 · test yok · yüksek.
2. **JLPM22/MotionMatching** · motion matching · şimdilik gerekmez · Unity 6+ · CPU (ölçülmedi) · 0 · MIT · + ayak kilidi / − veri hazırlığı · klip + root motion · S011 · yok · orta-düşük.
3. **MediaPipe Pose Landmarker** · videodan 33 nokta · kendi çektiği hareketten taslak · harici Python · CPU mümkün (ölçülmedi) · 0 · Apache-2.0 (model kartı) · + serbest / − iskelet dönüşümü bizde · video2mocap, RGB MoCap · S012,S013 · yok · orta.
4. **uLipSync** · ünlü tanıma ile dudak · yedek · eklenti (paket onayı) · CPU · 0 · MIT · + BakedData/Timeline / − Unity 6.6 uyumu bilinmiyor · kendi viseme sürücüsü · S019 · yok · orta.
5. **OVR LipSync** · ses→15 viseme · KULLANMA · eski Unity eklentisi · CPU · 0 · lisans okunmadı · − arşivlenmiş belge · uLipSync, hizalama · S016,S017 · yok · orta.
6. **itsFulcrum URP Hair** · saç kartı shader · saç görünümü · 2021.3 (6.6 testi gerek) · hafif (ÇIKARIM) · 0 · CC0 · − bakım yok · kendi shader · S023 · yok · düşük.
7. **IllusionRP** · URP saç/cilt · referans · URP 17.3, DX12 · yüksek · 0 · Unity Companion License · − bu PC'ye uygun değil (ÇIKARIM) · — · S022 · yok · orta.

## Karşılaştırma ve çelişkiler
- Netflix en kısa altyazı: zamanlama sayfası 4/5 sn (20 kare), Türkçe sayfası "mümkünse 1 sn" [S001,S002]. Çelişki değil: alt sınır ve tercih.
- Netflix satır sayısı 2 (yatay) ↔ BBC dikey için 3'e kadar [S001,S003]. Hedef 1-2, taşarsa 3 (ÇIKARIM). BBC'nin WPM değeri İngilizce; Türkçe eklemeli olduğu için CPS ölçütü tercih edilmeli (ÇIKARIM).
- URP motion blur belgesi kendi içinde çelişkili [S047].
- "Kod MIT/Apache" ↔ model lisansı ticari değil (RGB MoCap, video2mocap araştırma arka uçları).
- MFA İngilizce üstünlüğü Türkçeye otomatik taşınmaz.

## Riskler
| Risk | Önlem |
|---|---|
| loudnorm sessizce dinamik moda geçer, ses "pompalar" veya 192 kHz kalır | mod ve örnekleme hızını rapora yaz, `-ar 48000` |
| Eğitim/araştırma lisanslı karakter veya model (Defender karakteri, SMPL tabanlı mocap) projeye girer | lisans manifestosu; A12 denetimi |
| Mağaza varlığı + "içerik üretme aracı" EULA kısıtı (ileride satış olursa) | satış öncesi yazılı izin / hukuk görüşü |
| Karaoke vurgusu dikkat dağıtır | sade vurgu + kapatma seçeneği, kullanıcı A/B |
| Eski URP shader'ları Unity 6.6 Render Graph'ta çalışmaz (ÇIKARIM) | ayrı kopya projede dene |

## Test önerileri
Bu bilgisayarda (GPU'suz):
1. Altyazı denetçisi: bir bölümde CPS, satır uzunluğu, sayfa süresi, giriş hatası (kare) raporu.
2. A/B: sabit sayfa vurgusuz ↔ kelime vurgulu; kullanıcı telefonda izlesin.
3. loudnorm iki geçiş: seçilen mod, çıkış Hz, LUFS/TP ölçümü.
4. Yürüme klibi: root motion ↔ kodla taşıma; temas sırasında ayak yatay yolu (cm).
5. Flaş+bip ile Unity Recorder ve ffmpeg borusu A/V farkı (A07 T3 ile aynı).
6. Compute Light Baker küçük sahnede çalışıyor mu, süre; URP "Camera and Objects" motion blur karakterde görünüyor mu.
7. (İsteğe bağlı) MediaPipe ile telefon videosundan tek hareket denemesi — harici Python kurulumu onay ister.
GPU isteyen: video2mocap, 3DGS, öğrenmeli hareket temizleme.

## Öz denetim
- Hiçbir şey çalıştırılmadı, indirilmedi; tek kod işlemi `Assets/HikayeStudyo` içinde metin araması (yalnız okuma).
- 40/40 arama kullanıldı. WebFetch özet aracıdır; sayılar ve lisans cümleleri asıl metinden denetlenmeli (özellikle EULA, Netflix, ffmpeg kod koşulu).
- BBC sayfası resmî alan değil (ayna) ve yarısı okundu.
- Açılamayanlar: tandfonline altyazı çalışması, littlewolflabs mocap karşılaştırması (robots). Tekrar denenmedi.
- Yönlendirme (302) veren Netflix ve Unity adresleri sunucunun verdiği yeni adresten açıldı.
- SIGGRAPH taraması sistematik değil; NeRF derin okunmadı; mağaza karakterlerinde yalnız tek sayfa okundu; shader depolarında tarih ve Unity 6.6 uyumu doğrulanamadı.
- Karaoke okunurluğu için doğrudan kanıt yok; denetçi eşikleri uyarlamadır.

## Eksik alanlar / açık sorular
1. MetaHuman SSS ve ana EULA (Q-011) — A12 / kullanıcı tarayıcısı.
2. OVR LipSync lisans metni (kullanılmayacaksa gereksiz).
3. Gerçekçi + ARKit'li + fantastik kıyafetli belirli mağaza paketi listesi: sayfalar elle taranmalı.
4. Unity yerleşik motion matching planı; CC4/CC5 profil sayıları, Magica Cloth 2 fiyatı, UniVRM (HDRP geri çekildiği için artık düşük öncelik): ARANMADI.
5. Push-in/dolly duygu etkisi deneyi (A01), dikey drama izlenme kaybı (A08), TL fiyatları, bulut Windows GPU (A10), saatlik elektrik: ARANMADI.
6. `AGENTS/A12_lisans.md` bu çalışma sürerken oluştu; yalnız anahtar kelimeyle karşılaştırıldı (OVR arşivi, MetaHuman açık, Asset Store kısıtı: uyumlu). Tam metni okunmadı.

## Diğer ajanlardan beklenen / ortak konular
- A07: S1, S3, S6 (kayıt saati, MFA, viseme, kota). A03: S2, jest seçme fikri. A02/A04: S4. A01/A08: kamera sözlüğü, altyazı güvenli alan. A12: Asset Store EULA, MetaHuman, mocap model lisansları (RGB MoCap'in ticari olmayan modelleri A12'ye ek bilgi olabilir).

## Öneriler (kanıtla)
1. Altyazı denetçisine Netflix Türkçe + zamanlama kurallarını koy: ≤17 CPS, Türkçe satır kırma, giriş ≤2 kare, sayfa ≥0,8 sn [S001,S002]; dikey yerleşimde BBC 9:16 önerisini başlangıç al [S003].
2. Karaoke "kanıtlanmış üstünlük" diye sunulmasın; sade ve kapatılabilir olsun [S004,S005].
3. Ses düzeyi: iki geçişli loudnorm + mod/Hz denetimi [S007,S008].
4. Yürüme: önce root motion + ayak IK + ölçüm; motion matching sonraya [S010,S011].
5. Dudak: OVR LipSync alınmasın; hizalama → viseme, Polly tr-TR tablosu çapraz kontrol [S016,S018,S040].
6. Jest: üretici model yerine etiketli klip kütüphanesinden seçim [S030].
7. Saç/göz/cilt için hazır çözüm beklenmesin; küçük, kendi shader çalışması planlanabilir [S021-S026].
8. NeRF/3DGS ve virtual production bu sürümün kapsamı dışında tutulabilir (ÇIKARIM) [S035].

Not: bu dosyadaki "[S0xx]" kısaltmaları `A00-S0xx` demektir.
