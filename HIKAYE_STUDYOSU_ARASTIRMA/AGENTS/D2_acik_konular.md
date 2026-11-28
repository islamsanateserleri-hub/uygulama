# D2 — Açık konular araştırması
Tarih: 2026-11-27. **Test yapılmadı.** Kod değiştirilmedi, indirme yok, Unity'ye dokunulmadı. Web yalnız WebSearch/WebFetch; sayfalar özetleyici araçla okundu. Hukuki görüş değildir. Kaynaklar: `../SOURCES/D2_kaynaklar.md`.

## Görev
D1 §5'te açık kalanlar: (1) 4K dikey çıktı, (2) düz Türkçe yazıdan yapay zekâsız sahne/çekim listesi, (3) lisanslı hazır 2D paketler ve çizer ücreti, (4) upscaling, (5) başka sektörlerden aktarılabilecek yöntem, (6) çocuk karakter kaynakları.

## 1. 4K dikey çıktı (2160x3840)

### 1.1 TikTok 4K'yı nasıl işliyor
- Geliştirici (Content Posting) API sınırı: her kenar 360–4096 piksel, 23–60 FPS, en çok 4 GB, MP4/WebM/MOV, H.264 (önerilen) veya H.265 [D2-S001]. 2160x3840 bu sınırın içinde. Bu **yükleme kabul** sınırıdır; izleyiciye 4K gösterileceği anlamına gelmez.
- TikTok bir "Upload HD" anahtarıyla **1080p** yükleme seçeneği duyurmuştu (basın aktarımı, tarih sayfada yok) [D2-S002].
- Reklam özellik sayfası dikeyde en az 540x960 istiyor; 500 MB, en az 2500 kbps [D2-S003]. Reklam kuralıdır, normal paylaşım için değil.
- **DOĞRULANAMADI:** TikTok'un 4K yüklemeyi hangi çözünürlüğe indirdiğini ya da izleyiciye 4K sunduğunu söyleyen resmî sayfa bulunamadı. Dolaşan "TikTok 1080p'ye indirir" bilgisi yalnız ticari bloglarda görüldü; dayanak sayılmadı.
- ÇIKARIM: 4K'nın TikTok izleyicisine görünür fayda sağlayacağına dair kanıt yok. Sınama: aynı sahne 1080p ve 4K yüklenip telefonda kör karşılaştırılır (hesap işi kullanıcının).

### 1.2 Hesaplanan etkiler (aritmetik; ölçüm değil)
| Büyüklük | 1080x1920 | 2160x3840 |
|---|---|---|
| Piksel / kare | 2,07 milyon | 8,29 milyon (4 kat) |
| Ham RGBA kare | 8,3 MB | 33,2 MB |
| Ham veri akışı, 30 fps | ~249 MB/sn | ~995 MB/sn |
| 1 dk ham kare (diske yazılırsa) | ~15 GB | ~60 GB (**45 GB boş diske sığmaz**) |
| 1 dk MP4 | 12 Mbps ile ~90 MB | 35–45 Mbps ile ~260–340 MB |
| 5 dk MP4 | ~450 MB | ~1,3–1,7 GB |

- 12 Mbps projedeki `h264_amf` ayarıdır (`Uygulama/FfmpegPipeRecorder.cs:82`, yalnız okundu). 8 Mbps (1080p) ve 35–45 Mbps (2160p) YouTube'un yükleme önerisidir; TikTok önerisi değildir [D2-S004]. 12 Mbps ayarı 4K'da değiştirilmezse piksel başına veri dörtte bire iner.
- Projede genişlik/yükseklik 1080x1920 sabit yazılmış yerler var: `Editor/StudyoKayit.cs:168,286-287`, `Uygulama/KayitBootstrap.cs:73,108,220-227`. 4K "tek ayar" değil, küçük bir kod işi (ÇIKARIM).

### 1.3 Bu PC (5600GT, tümleşik Vega, 64 GB)
- Kodlayıcı: AMD'nin tablosunda Ryzen 5000 APU (Cezanne) H.264 8 bit ve HEVC 8/10 bit "4K'ya kadar" [D2-S005]. VCN sürümü kaynaklarda farklı (AMD: 2.0; Wikipedia: 2.2) [D2-S006]; fps değeri ikisinde de yok.
- **Belirsiz:** "4K'ya kadar" yatay 3840x2160 için olabilir; **dikey 2160x3840'ı `h264_amf`'in kabul ettiği doğrulanamadı.** İlk yapılacak sınama budur (kısa ffmpeg denemesi). Olmazsa HEVC, ya da yazılım kodlayıcı (projedeki LGPL ffmpeg'de libx264 yok [A09]).
- Unity Recorder özel genişlik/yükseklik ve 2160 hazır ayarı sunuyor; en büyük H.264 boyutu belgede yazmıyor [D2-S007]. Bilinen sorunlar sayfası yüksek çözünürlüklü eşzamanlı kayıtta Editor'ün belirgin yavaşladığını söylüyor [D2-S008].
- Render süresi: piksel yükü 4 kat; ekran kartına bağlı işler (ışık, post, saydamlık) kabaca buna yakın artabilir, CPU işleri (iskelet, mantık) artmaz (ÇIKARIM). Tümleşik GPU sistem belleğini paylaşır; A10 hesabı: 2160x3840 HDR + 4x MSAA hedefi ≈ 400 MB, bellek bant genişliği darboğaz adayı. 64 GB **kapasite** sorunu çözer, **hız** sorununu çözmez.
- Süre, fps, kare düşmesi: **ölçülmedi**. Benzer APU için güvenilir 4K kodlama benchmark'ı bulunamadı.
- Depolama: sonuç MP4 sorun değil; ham/PNG/ProRes ara kare sorun. Kare doğrudan ffmpeg'e borulanmalı (EXE yolu bunu yapıyor).
- Varlık sınırı (ÇIKARIM): düşük çözünürlüklü doku, 2D görsel ve yazı 4K'da keskinleşmez; 2D mekân görselleri en az 2160x3840 (kamera yakınlaşacaksa daha büyük) gerekir.

### 1.4 Öneri (ÇIKARIM)
1) Varsayılan 1080x1920 kalsın. 2) İsteğe bağlı "2x çiz, 1080p'ye küçült" (kenar/saç temizliği; A04 adım 8, A10 itirazıyla). 3) "4K dışa aktar" yalnız ölçümden sonra: 10 sn örnek, süre + dosya boyutu + dikey kodlayıcı kabulü. Kullanıcıya açıkça söylenmeli: 4K dosya üretmek, TikTok'ta 4K izlenmesi demek değil.

## 2. Düz Türkçe yazıdan sahne ve çekim listesi

### 2.1 Projede bugün ne var (KOD, statik okuma; çalıştırılmadı)
`Uygulama/YardimciMotor.cs` içinde **yapay zekâsız bir düz metin çevirici zaten var** (`DuzMetniCevir`, satır 570-672):
- Konuşan: `Ad: söz` kalıbı; tırnaklı sözde tırnak dışındaki karakter adı ya da `dedi|sordu|... + Ad`; bulunamazsa Anlatıcı.
- Mekân: kelime sayımıyla yalnız `yol_gece`, `sofra`, `oda_gunduz`, `oda_gece` (satır 123-133). Orman, zindan, uzay vb. mekânlar tahmin edilmiyor.
- Duygu: kelime sözlüğü. Uzun cümle virgülden bölünüyor.
- Koddan görülen sınırlar (ÇIKARIM; sınanmadı): (a) `Anne, Arda'ya "..." dedi` cümlesinde konuşan, ada göre ilk eşleşendir; kesme işaretli ek yüzünden hitap edilen kişi de eşleşir, konuşan yanlış seçilebilir (satır 628-629). (b) Konuşma çizgili (`— Gel, dedi.`) yazım için kalıp görmedim. (c) Hareket, eşya, kadro, çekim üretmiyor.

Yani iş "sıfırdan yazmak" değil, bu çeviriciyi genişletmek.

### 2.2 Akademik örnekler
| Sistem | Ne yapar | Yapay zekâ? | Sınır |
|---|---|---|---|
| WordsEye (2001) [D2-S009] | metinden **durağan** 3D sahne: ayrıştırıcı → anlam → elle yazılmış resmetme kuralları | istatistiksel ayrıştırıcı + elle kural | ~2000 model, ~1300 isim, ~2300 fiil elle işaretli; ortak akıl ve mekân çıkarımı zayıf; hareket yok |
| Text-to-scene taraması [D2-S010] | CarSim, CONFUCIS, SceneMaker vb. | karışık | örtük bilgi çıkarılamıyor; dar alan; CONFUCIS 20'den az görsel fiil |
| Virtual Cinematographer (1996) [D2-S011] | olaylara göre kamera: film deyimleri = durum makineleri | **hayır** | her sahne türü için ayrı deyim yazılır; girdi metin değil yapılandırılmış olaylar |
| Leake vd., diyalog sahnesi kurgusu [D2-S012] | senaryo + çok kamera kaydı → deyimlere göre otomatik kurgu (HMM) | istatistiksel model; duygu etiketi | girdi düz yazı değil **senaryo**; yalnız diyalog |
| İngilizce romanda konuşan bulma, iki aşamalı kural eleği [D2-S013] | sözü kişiye bağlar | **hayır** (deterministik) | üç İngilizce romanda ortalama F 87,5; hassas ayarda doğruluk 90,4 / kapsama 65,1. Türkçe değil |

- Türkçe: düşen özne (pro-drop) yüzünden "kim" sorusu zorlaşır; Türkçe eşgönderim çalışması düşen zamirleri ayrıca etiketlemek zorunda kalmış ve çözümü **sinir ağıyla** yapmış [D2-S014; yalnız özet, başarı sayısı okunmadı]. Türkçe hikâyede konuşan atfı için hazır, kural tabanlı bir araç **bulunamadı** (kapsamlı tarama değil).
- Kural tabanlı yardımcı: ZemberekDotNet (NuGet; MIT; .NET Standard 2.1; Nis 2026) biçimbilim/sözcük ayırma sunuyor [D2-S015]. Kök bulma ("ormana" → "orman") için işe yarayabilir; Unity 6000.6 uyumu denenmedi, **yeni paket = kullanıcı onayı**. İstatistiksel bileşen içerip içermediği ayrıca bakılmalı.

### 2.3 Ürün örnekleri
- Xtranormal "Text-To-Movie" (kapandı, 2013): kullanıcı karakter ve seti menüden seçiyor, **diyaloğu kendisi yazıyordu**; düz yazıdan sahne çıkarmıyordu [D2-S016].
- Plotagon Studio: sahne, karakter, yer, etkileşim tek tek elle seçiliyor; belgede düz yazı çözümleme yok [D2-S017].
- Düz anlatıdan yapay zekâsız tam sahne kuran güncel bir ürün **bulunamadı**.

### 2.4 Gerçekçi sınır (ÇIKARIM)
Yapay zekâsız yapılabilir: açık yazılmış konuşan; sözlükteki mekân/zaman/hava/eşya adları; sözlükteki fiilden hareket ("yürüdü", "oturdu", "koştu"); deyim kuralıyla çekim listesi (yeni mekân → geniş plan; ilk kez konuşan → yakın; karşılıklı söz → omuz/ters; güçlü duygu → yakın).
Güvenilir yapılamaz: adı geçmeyen özne, "o/kadın/çocuk" göndermesi, mecaz, ima, olumsuzluk ("kapıyı açmadı"), sahne değişimini anlatımdan anlama, sözlükte olmayan şey.
Önerilen tasarım: otomatik **taslak** + her tahminde güven işareti + belirsizde kullanıcıya açılır liste ("Bunu kim söylüyor?") + kullanıcının düzeltmesini ad sözlüğüne kaydetme. Bu, kullanıcı hedefindeki "yalnız beğenmediğini değiştir" ile uyumlu. "Hatasız otomatik" sözü verilmemeli. Başarı ölçüsü: kullanıcının 10 kısa hikâyesinde konuşan/mekân/hareket doğru oranı (test yapılmadı).

### 2.5 Yapay zekâlı seçenekler (AYRI; kullanıcı onayı ister, Q-015)
- Büyük dil modeline (projede zaten Gemini bağlantısı var: `YardimciGemini.cs`) düz yazıyı etiketli sözdizimine çevirtmek, ardından köprü doğrulayıcısıyla denetlemek. Video/görsel üretmez, yalnız metin düzenler. `DURUM.md` "Sonra" listesinde "yapay zekâ ile" diye geçiyor. Veri dışarı gider; kota ve koşullar bu görevde okunmadı.
- Yerel Türkçe NLP / eşgönderim modelleri (sinir ağı): [D2-S014] türü; hazır ürün olarak doğrulanmadı.
- Ticari "senaryodan storyboard" araçları yapay zekâ görsel üretiyor görünüyor (yalnız arama başlıkları); uygulama dışı olduğundan incelenmedi.

## 3. Lisanslı hazır 2D paketler ve çizer ücreti
Hiçbir paket indirilmedi, görüntü kalitesi ve duygu sayısı gözle denetlenmedi. Fiyatlar 27 Kas 2026 sayfa gösterimi (USD).

### 3.1 Örnekler
| Paket | İçerik | Fiyat | Koşul | Eksik |
|---|---|---|---|---|
| Unity Asset Store "5 Characters – 36 Expressions" [D2-S019] | 5 portre | 25 $ | standart EULA; sayfada tür "Extension Asset" (koltuk başı olabilir) | içerik listesi boş; yapay zekâ üretimi mi bilinmiyor |
| itch "Luna Emotional Sprite Collection" [D2-S020] | 33 ifade, tek karakter (başlık) | en az 2,99 $ (atıflı) / 3,99 $ (atıfsız) | ticari/video koşulu özetle görünmedi | yapay zekâ beyanı yok |
| itch "VN & RPG Character Pack 1 – taster" [D2-S021] | 5 gövde üstü 500x450, 1 arka plan 2560x1440 | ücretsiz deneme; ücretli fiyatı yazmıyor | ticari ve değiştirme serbest; yeniden satış yok; yapay zekâya verilmez | "oyun ve benzeri"; çözünürlük 4K için küçük |
| OpenGameArt "VN Characters" [D2-S022] | küçük paket, çoğunda farklı duygu | 0 | CC0 | karakter/duygu sayısı yazmıyor; kalite bilinmiyor |
| itch "Annako's Backgrounds" [D2-S023] | arka plan | görünmedi | atıf; ticari koşul **belirsiz** (fırça lisanslarına bağlı) | önerilmez |

### 3.2 Lisans uyarısı: Unity Asset Store
EULA'ya göre varlık, kendi amacı ve özgün içeriği olan bir "uygulama ya da dijital medya" içine gömülerek kullanılır; **ana amacı kullanıcı içeriği üretmek olan bir üründe varlıktan gelir elde etmek açık izin ister**; videolar adıyla anılmıyor [D2-S018]. Hikâye Stüdyosu bir içerik üretme aracı: Asset Store görsellerini uygulamanın kütüphanesine koyup başkasına dağıtmak riskli (ÇIKARIM; hukuki görüş değil). Güvenli sıra: CC0 → yazılı izni net sanatçı paketi → sözleşmeli çizim.

### 3.3 Projeye uyum (ÇIKARIM)
`EmotionSprite` duygu başına 3 görsel ister (gövde, ağız açık, göz kapalı; D1 §1). Hazır "görsel roman" paketleri çoğunlukla yüz ifadesi verir; ağız-açık ve göz-kapalı eşleri, yürüme/oturma pozu genelde ayrıca gerekir. Paketlerin aynı çizim tarzında hem karakter hem mekân sunması beklenmemeli. 4K için 500 px sprite yetmez.

### 3.4 Çizer ücreti göstergesi
- Fiverr rehberi (anime; platform pazarlama yazısı, sınıf C): büst 30–75 $, tam boy standart tasarım 100–200 $ (ticari hakla), ayrıntılı arka plan 100–300 $; ticari kullanım çoğu zaman +%50–100 [D2-S024]. İfade başı fiyat yazmıyor.
- Birleşik Krallık illüstratör anketi: ortalama gün ücreti 350 £ (orta yarı 280–450 £; 90 yanıt, gönüllü); tam telif devri ücreti yaklaşık üçe katlıyor deniyor [D2-S025].
- ÇIKARIM (kaba): 4 karakter + 5 mekân, Fiverr aralıklarıyla ≈ 900–2300 $; ifade/ağız/göz çeşitleri ve ek açılar **hariç**. Teklif alınmadan bütçe sayılmamalı.
- **Türkiye çizer ücreti bulunamadı** (arama yalnız maaş sayfaları döndürdü).

## 4. Upscaling (büyütme)
| Yöntem | Ne | Yapay zekâ? | Bu projeye (ÇIKARIM) |
|---|---|---|---|
| Doğal 4K render | her piksel hesaplanır | hayır | kayıt gerçek zamanlı olmak zorunda değilse en temiz seçenek; süresi ölçülmeli |
| Fazla örnekleme (büyük çiz, küçült) | kalite artırır | hayır | 1080p teslimde tercih (A04 adım 8) |
| FSR 1 (URP) | tek kare uzamsal büyütme [A04] | hayır | az maliyetli; ayrıntı üretmez |
| STP (URP, Unity 6) | uzamsal + zamansal; TAA ister; compute shader / SM 5.0; belge makine öğrenmesinden söz etmiyor [D2-S026] | belgede yok | bu PC kâğıt üzerinde karşılıyor (compute var, D3D11); saç/ince çizgide gölgelenme ve kayıt belirlenimliliği sınanmalı |
| ffmpeg ölçekleme (lanczos vb.) | bitmiş videoyu büyütür | hayır | yalnız boyut büyür, ayrıntı eklenmez (genel bilgi, doğrulanmadı) |
| Real-ESRGAN / Real-CUGAN / Anime4K (Video2X) | video sonrası büyütme; Vulkan, AVX2; en az AMD GCN 1.0 [D2-S027] | Real-ESRGAN/CUGAN **evet** (sinir ağı; genel bilgi) | **ayrı onay** (Q-015); Video2X AGPL-3.0 [D2-S027]: ayrı araç olarak kalmalı; tümleşik Vega'da hız ve titreme bilinmiyor |

Öneri (ÇIKARIM): önce doğal 4K'nın 10 sn'lik süresini ölç; kabul edilemezse 1440x2560 çiz → FSR1/STP karşılaştır. Dışarıdan yapay zekâ büyütme şimdilik önerilmez. Hiçbiri denenmedi.

## 5. Başka sektörlerden aktarılabilecek yöntem (kısa)
| Sektör | Sonuç |
|---|---|
| Simülasyon / robotik testi | **Var (fikir düzeyinde).** Sürüş simülasyonunda senaryo üç düzeyde yazılıyor: sözlü anlatım → parametre ve aralıkları → tek somut örnek; denetimli sözlük ve bağımlılık kuralları geçersiz birleşimi engelliyor [D2-S028]. Uyarlama (ÇIKARIM): düz yazı → "mantıksal sahne" (mekân, kim, nerede, belirsiz alanlar) → somut hikâye dosyası; kurallar ("yatan yürüyemez", "araba sahnesinde çocuk sürücü olmaz"). Makalenin değerlendirmesi küçük ve gayriresmî. |
| Bilgisayarlı görü | Kaynaklı yöntem aranmadı. ÇIKARIM (kaynaksız): yapay zekâsız denetim görü yerine sahne geometrisiyle yapılabilir: konuşanın baş noktası kadrajda mı, önünde nesne var mı (kayıtlı "baş kadraj dışında" kusurları için). |
| Mimari görselleştirme | Güvenilir kaynak **bulunamadı** (arama pazarlama/forum sayfası döndürdü). "Düşük kalite taslak, sonra son render" yaygın bilgi; proje zaten "önce kısa örnek" kuralını koyuyor. |
| Reklam | **Araştırılmadı** (arama bütçesi). Yalnız reklam teknik özelliği okundu [D2-S003]. |

## 6. Çocuk karakter
### 6.1 Kaynaklar
| Kaynak | Durum | Lisans / koşul |
|---|---|---|
| Microsoft Rocketbox (projede) | kodda `Female_Child_` → `Gercek_KizCocuk`, `Male_Child_` → `Gercek_ErkekCocuk` eşlemesi ve `_01` örnekleri var (`Editor/RocketboxKur.cs:17`, `Karakter3DKurulum.cs:417`); kaç çocuk modeli olduğu doğrulanmadı | MIT; depo sayfasında ek kullanım kısıtı görünmedi, çocuk modelinden de söz edilmiyor [D2-S029] |
| MPFB / MakeHuman | yaş parametresiyle çocuk gövde üretilebildiği genel bilgi, **bu görevde doğrulanmadı** | çekirdek varlıklar CC0; kaynak kod GPL/AGPL [D2-S030]; sayfada çocuk içeriğine özel koşul yok |
| VRoid | anime tarzı çocuk oranı yapılabilir mi doğrulanmadı | model başına; karakter üretici yapılırsa ayrı pixiv izni (A02-S044) |
| Hazır 2D paketler | çocuk karakter içeren lisansı net paket **bulunamadı** (hedefli tarama yapılmadı) | - |
| Çizer | ısmarlama; §3.4 | sözleşmede video + dağıtım hakkı yazılmalı |

İlk adım (ÇIKARIM): yeni indirme gerekmeden projedeki Rocketbox çocukları sayılıp önizlenmeli.

### 6.2 Platform koşulu — dikkat
- TikTok'un yapay zekâ içeriği sayfası: "18 yaş altındaki kişilerin benzerliğini (likeness) içeren içeriğe izin vermiyoruz" anlamında bir cümle içeriyor; aynı sayfa gerçekçi görünen yapay zekâ içeriğinin etiketlenmesini istiyor [D2-S031]. **Kurgusal, elle hazırlanmış 3D/2D çocuk karakteri kapsayıp kapsamadığı belli değil.** Gerçekçi 3D çocuk + yapay zekâ sesi birleşimi için yayından önce güncel kurallar ve itiraz yolu elle okunmalı.
- Topluluk Kuralları genç kişilerin her türlü cinselleştirilmesini ve onlara zarar veren içeriği yasaklıyor; "yüksek gerçekçilikte dijital/yapay zekâ üretimi" açıkça dahil [D2-S032].
- ÇIKARIM (uygulama kuralları): çocuk karakterde kıyafet her açıdan denetlenmeli (kayıtlı kusur: araba planında iç çamaşırı görünmesi, `DURUM.md`; yetişkin/çocuk ayrımı fark etmeksizin kamera denetimi); çocuk için alçaktan açı, yakın beden planı, yatak/kıyafet değiştirme sahneleri kapatılmalı; şiddet/tehlike sahneleri sınırlanmalı. Gerçek bir çocuğun yüzü ya da sesi kullanılmamalı.
- Çocuk sesi (Türkçe TTS) ve seslendirme koşulları **araştırılmadı**.

## 7. Araştırılamayan / doğrulanamayan
- TikTok'un 4K yüklemeyi yeniden kodlama davranışı (resmî kaynak bulunamadı).
- 5600GT'de dikey 4K donanım kodlama kabulü, render/kodlama süresi (test yok).
- Türkçe konuşan atfı için kural tabanlı hazır araç ve başarı oranı.
- WordsEye ve Türkçe eşgönderim için ResearchGate sayfaları, bir arka plan paketi sayfası açılamadı; taramanın son bölümü okunmadı.
- Türkiye çizer ücreti; paketlerin görsel kalitesi; ifade başı fiyat.
- Reklam sektörü; mimari görselleştirme için güvenilir kaynak; çocuk sesi; MPFB/VRoid çocuk oluşturma ayrıntısı.

## 8. Öz denetim
Sayfaların hepsi özetleyiciyle okundu; sayılar asıl sayfada yeniden denetlenmeli. 1.2 tablosu aritmetik, süre/kalite yargıları tahmin. Projedeki kod bulguları statik okumadır. **Hiçbir test yapılmadı.** Web araması: 35 (sınır 35); ayrıca sayfa açmaları.
