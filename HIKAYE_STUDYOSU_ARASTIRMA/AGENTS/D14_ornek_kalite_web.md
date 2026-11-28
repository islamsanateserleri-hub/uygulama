# D14 — "@diziko2 kalitesi" nasıl yapılır? (Web araştırması)

Tarih: 28 Kasım 2026 · Yöntem: yalnızca web araması (WebSearch/WebFetch, 35 arama sınırı), indirme/hesap yok.
Etiketler: **[DOĞRULANDI]** = resmi sayfa ya da tarihli birincil kaynak okundu · **[KISMEN]** = üçüncü taraf blog, tarihli · **[DOĞRULANMADI]** = çıkarım ya da bulunamadı.
**[YZ]** = yapay zeka video üreticisi gerektiren yol (kullanıcı önceden bunu istememişti, açıkça işaretlendi).

---

## 0. Kısa sonuç

- @diziko2 tarzı (fotoğraf gerçekliğinde, sinematik ışık, ejderha, şato, kalabalık, altın gözlü "Owen") videolar **neredeyse kesin olarak YZ video üreticisiyle** yapılıyor. TikTok'un "AI-generated" etiketi de bunu gösteriyor. Google Veo/Gemini/Flow çıktıları **C2PA meta verisi** taşıyor, TikTok da C2PA içeren videoları **otomatik** etiketliyor (ikisi de doğrulandı; @diziko2'nin özellikle hangi aracı kullandığı **DOĞRULANMADI**).
- Tipik hat (2026): **görsel modeli ile karakter sayfası → görselden videoya (3–8 sn) → TTS seslendirme → CapCut/DaVinci ile kurgu ve altyazı**.
- **Zayıf PC ile, YZ olmadan** bu fotoğraf gerçekliğine ulaşan ücretsiz bir web servisi **bulunamadı**. Bulut Unreal/MetaHuman saat başı ücretli ve yine de büyük emek istiyor.
- **≤1000 TL ile mümkün [YZ]:** Google Flow'un ücretsiz günlük kredisi (Türkiye listede), Dreamina ve PixVerse'in günlük kredileri, ayrıca Kaggle'da açık kaynak Wan 2.2. Bir bölüm ücretsiz yolla **birkaç günde** biter. Ücretli bir aylık paket (~7–10 USD ≈ 360–510 TL) bütçeye sığıyor.

---

## 1. Bu kanallar nasıl üretiliyor? (2026)

### 1.1 Tipik araç zinciri
| Adım | Yaygın araçlar | Kaynak |
|---|---|---|
| Senaryo ve sahne listesi | ChatGPT/Gemini/DeepSeek | [KISMEN] |
| Karakter sayfası (ön, 45°, yan, arka) ve mekân görselleri | Nano Banana 2 / Pro (Gemini), Seedream (Dreamina), Midjourney, FLUX | [KISMEN] pctechmag 03.04.2026 |
| Görselden videoya (3–8 sn çekimler) | Veo 3.1 (Flow), Seedance 2.0 (Dreamina), Kling 3.0, Hailuo, Vidu | [KISMEN] pctechmag; TheWrap 11.06.2026 |
| Seslendirme | ElevenLabs, Gemini TTS, Doubao/CapCut sesleri | [KISMEN] |
| Kurgu, altyazı, müzik | CapCut, DaVinci Resolve | [KISMEN] |

- PC Tech Magazine (03.04.2026), "Nano Banana 2 + Veo 3.1 drama iş akışı": karakter sayfası ve ortamlar Nano Banana 2 ile; storyboard taslakları Flash ile, son kareler Pro ile; ardından **Veo 3.1'e anahtar kare yükleyip** kamera hareketi yazılıyor; kurgu CapCut/DaVinci'de, 9:16 dışa aktarım. https://pctechmag.com/2026/04/nano-banana-2-veo-3-1-the-ultimate-ai-drama-workflow/
- TheWrap (11.06.2026): profesyonel YZ mikrodizi stüdyoları (Dashverse 700 kişi, StoReel 60 kişi) **Seedance 2.0 ve Veo 3** kullanıyor. Bir saatlik içerik ~20–30 bin USD tutuyor (canlı çekim ~150 bin USD). Douyin'e Mart 2026'da ~50.000 YZ dizisi eklendi. https://www.thewrap.com/?p=8031164 [DOĞRULANDI: makale okundu]
- Çin topluluğu (Bilibili, 27.04.2026, "AI短剧崩脸怎么办"): ücretsiz araçlar 白日梦AI (kartsız ücretsiz kota) ve Coze iş akışı; ücretli olan 有戏AI ise saniyesi 0,1 yuan. https://www.bilibili.com/read/cv48245381 [KISMEN]

### 1.2 Karakter tutarlılığı yöntemleri (toplulukların ortak noktaları)
1. **Üç görünüşlü referans seti:** ön yakın, ön bel çekim, 45°, 90° yan ve arka (Bilibili 27.04.2026).
2. **Sabit karakter tanımı:** ~15 İngilizce anahtar kelime (saç, göz rengi, "glowing golden eyes", kıyafet, yara izi) her istemin **başına** aynen yazılır.
3. **Metinle değil, görselle başlatma:** videoya yalnızca metin değil, **onaylanmış karenin kendisi** yüklenir (image-to-video).
4. **Referans özellikleri:** Veo 3.1 "Ingredients" (karakter, nesne, sahne referansı; en açık belgelenmiş yöntem), Kling 3.0 "Omni" (Image/Video 3.0 Omni), Seedance 2.0 çoklu girdi (görsel, video, ses), Vidu referanstan videoya (ücretsizde ayda 10 kayıtlı referans). Kaynak: elser.ai 05.08.2026 [KISMEN]; Vidu: wavespeed 18.11.2026 [KISMEN].
5. **Her 5 karede bir kontrol:** saç, göz, kıyafet rengi ve yüz oranı kontrol edilir; kayan kare tek başına yeniden üretilir.
6. **Önce ses:** seslendirme önce yapılır, çekim süreleri ona göre planlanır (3 sn çekim ritmi buradan gelir).
7. **Mekân kütüphanesi:** şato, taht salonu, avlu gibi mekânlar için sabit istem ve görsel arşivi tutulur.

### 1.3 @diziko2 ve Türk kanalları
- "diziko2", "diziko" ve "yapay zeka dizi TikTok" aramalarında kanalla ilgili **haber, röportaj ya da yöntem bilgisi bulunamadı** [DOĞRULANMADI]. TikTok profil sayfaları arama motorlarında görünmüyor.
- Türkçe kaynakların çoğu Dreamina'nın (CapCut) kendi tanıtım sayfaları (ör. "AI Animasyon Serisi 2026"), bu yüzden taraflı.
- Çıkarım [DOĞRULANMADI]: "Owen" gibi İngilizce isim ve Batı fantastik estetiği, içeriğin büyük olasılıkla İngilizce ya da Çince bir YZ dizisinden Türkçe seslendirmeyle uyarlandığını **veya** kanalın İngilizce istemlerle ürettiğini düşündürüyor. Başkasının dizisini yeniden seslendirmek telif ve "özgün olmayan içerik" riski taşır (bkz. §6).

---

## 2. Ücretsiz krediyle web araçları [YZ] (Türkiye, kartsız)

> Uyarı: Ücretsiz paketler ayda bir değişiyor. Aşağıdakiler Ağustos–Kasım 2026 tarihli sayfalardan alındı. Kullanmadan önce hesabınızdaki güncel bakiyeye bakın.

| Araç | Ücretsiz kota | Çözünürlük (ücretsiz) | 9:16 | Görünür filigran | Ticari kullanım (ücretsiz) | Karakter referansı | Türkiye | Kaynak / durum |
|---|---|---|---|---|---|---|---|---|
| **Google Flow (Veo 3.1 Lite)** | **Günde 50 kredi.** Lite 4/6/8 sn = 10 kredi, yani **~5 klip/gün (≤40 sn)**. Fast = 20 kredi, Quality 8 sn = 100 kredi | 720p (Lite) | Evet | Ağustos 2026'dan beri Gemini/Flow'da görünür filigran ayarla kapatılabiliyor (ücretsiz kullanıcıda olup olmadığı net değil); **SynthID ve C2PA her zaman kalıyor** | Açıkça yazmıyor (Google Gen-AI şartları) | **Ingredients** (karakter/sahne referansı), ilk-son kare | **Evet**, Türkiye listede, 18+ | Kredi: [DOĞRULANDI] support.google.com/flow/answer/16526234 · Ülke: [DOĞRULANDI] support.google.com/labs/answer/16353544 · Filigran: [KISMEN] gigazine 17.08.2026 |
| Gemini uygulaması (Veo) | Ücretsiz planda video **yok** gibi görünüyor (Google AI planı gerekiyor) | – | – | – | – | – | – | [KISMEN] Gemini limit sayfası, vuela 10.11.2026 |
| Gemini, Nano Banana 2 (görsel) | Ücretsiz, "standart limit" (sayı verilmiyor, değişken) | – | Evet | SynthID | – | Görsel düzenleme ve referans | Evet | [DOĞRULANDI] support.google.com/gemini/answer/16275805 |
| **Dreamina / Seedance (CapCut)** | **Günde ~120 kredi** (Ağustos 2026 taban değeri). Video başına maliyet yayımlanmamış | Belirtilmemiş | Evet | Ücretsizde garanti yok | "Şartlara bağlı", garanti yok | Çoklu referans (modele bağlı) | tr-tr sayfası var (muhtemelen evet) | [KISMEN] dreamina.capcut.com (Dreamina'nın kendi sayfası, 24.10.2026) |
| **PixVerse** | Kayıtta 60, sonra **günde 30 kredi** (00:00 UTC'de sıfırlanır; başka kaynak günde 60 diyor) | ≤540p | Evet | Kaynaklar çelişiyor | Hayır ya da belirsiz | Belirtilmemiş | [DOĞRULANMADI] | [KISMEN] vuela, whichoneisreal (Kasım 2026) |
| **Kling** | Sabit günlük 66 kredi **artık garanti değil**. Abone olmayanlara aylık ya da kampanya kredisi verilebiliyor | 720p (1080p ücretli) | Evet | Evet | Hayır (ücretli pakette var) | Omni / Elements | Küresel | [KISMEN] magichour (Eyl–Eki 2026), vuela |
| **Hailuo (MiniMax)** | Bir kerelik **150 hoş geldin kredisi, 3 günde biter** | 768p | Evet | Evet | Belirtilmemiş | Konu referansı | Küresel | [KISMEN] vuela, dreamina karşılaştırması |
| **Vidu** | Kayıt ve günlük giriş kredisi (miktar yayımlanmamış). Q4 540p, saniyesi 2 kredi | 540p | Evet | Evet | **Hayır** | Referanstan videoya (ayda 10 referans) | Küresel | [KISMEN] wavespeed 18.11.2026 |
| Pika | Aylık 0 video kredisi (bir kaynak 80 diyor, çelişkili) | 480p | Evet | – | Hayır | – | – | [KISMEN] çelişkili |
| Luma | Ücretsiz plan yok, sadece deneme | Taslak | – | Olabilir | Kişisel ve ticari olmayan kullanım | – | – | [KISMEN] |
| Runway | Bir kerelik 125 kredi, yenilenmiyor | – | Evet | Evet | Bir kaynak "izinli" diyor | – | – | [KISMEN] |
| Grok Imagine | **Mart 2026'dan beri yalnızca SuperGrok** (bazı hesaplarda geri gelmiş olabilir) | – | – | – | – | – | – | [KISMEN] latestly 24.03.2026 |
| Meta AI / Vibes | Ücretsiz, kısa video. **Türkiye'de kullanıma açıldı** (teknoblog başlığı) | Düşük–orta | Evet | Meta etiketi | Belirsiz | Görselden animasyon | Evet | [DOĞRULANMADI] (sayfa açılmadı) |
| Sora | Uygulama Nisan 2026'da kapandı, API Eylül 2026'da kaldırıldı | – | – | – | – | – | – | [KISMEN] whichoneisreal |

**En işe yarar ücretsiz kombinasyon:** Flow (5 klip/gün, en iyi kalite, referans destekli) + Dreamina (günlük kredi) + PixVerse (günlük kredi). Hepsi tarayıcıda çalışır, zayıf PC sorun olmaz.

---

## 3. Açık kaynak modeller: Kaggle ve Colab ücretsiz GPU [YZ]

| Model | Lisans | T4 (15–16 GB) uyumu | 1 klip süresi (T4) | Kaynak |
|---|---|---|---|---|
| **Wan 2.2 TI2V-5B** | **Apache 2.0** (ticari serbest) | Evet, offload ile | **~2 dk / 3 sn klip** (480p, Kaggle 2×T4). 720p T4'te pratik değil (10 sn > 3 saat) | Lisans: [DOĞRULANDI] HF Wan-AI/Wan2.2-TI2V-5B · Süre: [KISMEN] github davidsantdev/wan-studio |
| **Wan 2.2 14B (GGUF Q4)** | Apache 2.0 | Evet, Q4 ve CPU offload ile | **~7 dk / 3 sn klip**, 16 fps | [KISMEN] wan-studio |
| Wan 2.1 1.3B | Apache 2.0 | Rahat sığar (832×480, 81 kare) | – | [KISMEN] ahmedfayaz.com 14.11.2026 |
| **LTX-2 / 2.3** (Lightricks) | LTX-2 Open Weights: yıllık geliri **<10 M USD olana ücretsiz**. **Çıktının "makine üretimi" olduğunu belirtmek zorunlu**, deepfake yasak | Düşük bellek (768×512) | Hızlı (rakam yok) | [DOĞRULANDI] HF Lightricks/LTX-2.3 LICENSE |
| **HunyuanVideo 1.5** (8.3B) | Tencent Hunyuan Community (eski sürümde AB/İngiltere/G. Kore hariç; 1.5 için bölge maddesi **okunamadı**) | **Min. 14 GB, offload ile**; 480p/720p, I2V var | RTX 4090'da 480p distilled ~75 sn (T4'te çok daha yavaş, ~5–10 kat tahmini) | [KISMEN] HF tencent/HunyuanVideo-1.5 |
| CogVideoX-2B / 5B | 2B Apache; 5B CogVideoX lisansı | 2B rahat, 5B offload ile (720×480, 49 kare) | – | [KISMEN] ahmedfayaz |

**Kaggle:** haftada ~30 GPU saati, oturum en fazla 12 saat, boşta 60 dk sonra kapanır, **telefon doğrulaması şart**, yoğun saatte kuyruk olabilir. Wan-studio yazarına göre Kaggle bir "veri bilimi" servisi; video üretimi **kişinin kendi riskinde**, ölçülü kullanmak gerek. Yüz değiştirme (FaceFusion) oturumları dakikalar içinde iptal edilmiş, yetişkin içerik hesap engeline yol açmış [KISMEN].

**Colab ücretsiz:** resmi SSS'ye göre ücretsiz çalışma zamanında **"notebook arayüzünü atlayıp ağırlıkla web arayüzü (ComfyUI/WebUI) kullanmak"**, uzak masaüstü/SSH ve proxy yasak; deepfake her planda yasak. **Notebook hücresinden doğrudan (Diffusers) üretim** sorun değil, ama ComfyUI arayüzü ücretsiz Colab'da **yasak**. [DOĞRULANDI] research.google.com/colaboratory/faq.html

**Gerçekçi hesap (Kaggle, Wan 2.2 5B, 480p):** 25 çekim × 3 deneme = 75 klip × 2 dk ≈ **2,5 GPU saati**. 14B ile ≈ 9 saat. Haftalık 30 saatin içinde kalıyor. Kalite ise Veo/Kling'in **altında** (480p, 16–24 fps, yüz kayması daha çok); fotoğraf gerçekliği ve kalabalık sahnelerde fark belirgin.

---

## 4. Bir bölümün maliyeti (60–90 sn, ~25 çekim)

Varsayım: 25 çekim × 3–4 sn ≈ 90 sn. Her kullanılabilir çekim için **ortalama 2–3 deneme** (topluluk deneyimi, [DOĞRULANMADI]).

| Yol | Para | Süre | Kalite | Not |
|---|---|---|---|---|
| **A. Tamamen ücretsiz, Flow** [YZ] | 0 TL | 8 sn'lik bir Lite klip 2 çekime bölünürse 25 çekim ≈ 13 klip; denemelerle 30–40 üretim ≈ **6–8 gün** (günde 5 klip) | En iyi ücretsiz kalite (720p) | Görseller Gemini/Nano Banana 2'de ücretsiz |
| **B. Ücretsiz karma: Flow + Dreamina + PixVerse** [YZ] | 0 TL | **~2–4 gün** | Karışık (araçlar arasında stil farkı olabilir) | Aynı karakter sayfasını hepsine yükleyin |
| **C. Kaggle Wan 2.2** [YZ] | 0 TL | 1 hafta sonu (2,5–9 GPU saati + kurulum) | Orta (480p) | Teknik kurulum gerekir, PC'den bağımsız |
| **D. Bir ay ücretli paket** [YZ] | Kling Standard: ilk ay **6,99 USD (≈360 TL)**, 660 kredi · Vidu Standard: **10 USD (≈510 TL)**, 800 kredi | 1–2 gün | İyi–çok iyi, filigransız, ticari hak | Bir aylık paketle ~2–4 bölüm çıkar [DOĞRULANMADI, kredi tüketimi modele göre değişir] |
| Seslendirme | 0 TL (projede zaten kullanılan Gemini TTS; ElevenLabs ücretsizde **ticari kullanım yok** ve atıf zorunlu, ayda 10k kredi ≈ 10 dk) | – | – | ElevenLabs: [KISMEN] dupple 18.10.2026 |
| Kurgu ve altyazı | 0 TL (CapCut ücretsiz / DaVinci Resolve) | – | – | – |

**≤1000 TL ile en mantıklısı:** önce A+B ile **0 TL'ye 1–2 pilot bölüm** çekin. Tutarsa tek ay Kling ya da Vidu alın (≈360–510 TL). Toplam bütçenin yarısı kalır.

---

## 5. YZ olmadan, web üzerinden fotoğraf gerçekliği?

- **Gerçekçi cevap: yok gibi.** MetaHuman Creator artık (UE 5.6+) motorun içinde çalışıyor, eski "tarayıcıda MetaHuman" bulut servisi ana yol değil [KISMEN].
- **Bulut bilgisayar (Vagon):** en ucuzu T4 16 GB ile **0,99 USD/saat**, A10G 24 GB ile 1,67 USD/saat. Ücretsiz deneme yok, deneme ücreti 0,99 USD (1 saat). [DOĞRULANDI] vagon.io/pricing. 1000 TL ≈ 19 USD ≈ **~12–19 saat** eder. Bu süre Unreal kurulumuna, ejderha, şato ve kalabalık sahne kurmaya ve render almaya **yetmez**; ayrıca hazır ejderha ve kalabalık varlıkları ile animasyon becerisi gerekir.
- Eagle 3D Streaming gibi piksel akışı servisleri, **hazır** bir Unreal projesini yayınlamak içindir, üretim aracı değil.
- Sonuç: YZ'siz yolda kalite tavanı, mevcut **Unity Hikaye Stüdyosu + iyi ışık/post-process** düzeyidir (stilize ya da yarı gerçekçi). Ejderhalı ve kalabalık fotoğraf gerçekliği YZ'siz ve 1000 TL ile **ulaşılamaz**.
- **Hibrit öneri:** karakter tutarlılığını Unity'den alın. Unity'de pozlanmış karakter ve mekân karesini **anahtar kare** olarak verip YZ ile "sinematikleştirin" (image-to-video ya da video-to-video). Bu, karakter kaymasını azaltır ama çıktı yine **YZ etiketli** olur [DOĞRULANMADI, öneri].

---

## 6. Hukuk ve platform kuralları

1. **TikTok YZ etiketi [DOĞRULANDI]:** gerçekçi görüntü, ses ya da video içeren YZ içeriğinin **etiketlenmesi zorunlu**. Etiketsiz içerik kaldırılabilir. TikTok, C2PA "Content Credentials" içeren içeriği **otomatik etiketliyor** ve bu etiket kaldırılamıyor (Veo/Gemini çıktısı C2PA taşıyor). https://support.tiktok.com/en/using-tiktok/creating-videos/ai-generated-content
2. **TikTok Creator Rewards:** YZ içeriği etiketli ve **özgün** olduğu sürece para kazanabiliyor. Toplu üretilmiş, birbirinin aynı ya da başkasından alınmış içerik elenir. Şartlar: 10k takipçi, 30 günde 100k görüntülenme, ≥1 dk video. Gelir yaklaşık 0,4–1 USD / 1000 izlenme [KISMEN] lilachbullock 15.11.2026. **Türkiye, uygun ülkeler listesinde görünmüyor** (ABD, Kanada, İngiltere, Almanya, Fransa, İtalya, İspanya, Avustralya) [KISMEN] ttcalculator 09.07.2026. VPN ile katılmak şartlara aykırı.
3. **YouTube "özgün olmayan (inauthentic) içerik" [KISMEN]** (Tubefilter 13.07.2026): kural değişmedi, açıklaması netleşti. "Seri üretim izlenimi veren", şablonlu, yaratıcının **kendi özgün katkısı olmayan** YZ içeriği para kazanamıyor. Özgün senaryo, özgün seslendirme ve kurgu katkısı risk azaltır.
4. **Telif:**
   - ABD Telif Ofisi Rapor Bölüm 2 (Ocak 2025): **yalnızca istem (prompt) yazmak yazarlık sayılmaz**. İnsan eliyle seçme, düzenleme ve değiştirme korunabilir [KISMEN] ipwatchdog 29.01.2025, copyright.gov/newsnet/2025/1060.
   - Türkiye (FSEK 5846): eser sahibi yalnızca gerçek kişi olabilir. Saf YZ çıktısının korunup korunmadığı **netleşmedi**; yeterli insan yaratıcılığı varsa karma eser korunabilir; özel YZ yasası yok [KISMEN] bilalalyar.av.tr (Mart 2026).
   - Pratik sonuç: **senaryo, karakter tasarımı, seslendirme metni ve kurgu sizin olsun**. Başkasının YZ dizisini indirip Türkçe seslendirmek **yasak ve riskli**.
5. **Araç şartları:** ücretsiz paketlerin çoğunda (Kling, Vidu, Pika, Luma, ElevenLabs) **ticari kullanım yok**. Para kazanacaksanız ücretli paket ya da Apache 2.0 açık model (Wan) kullanın. LTX-2 çıktılarında "makine üretimi" beyanı **lisans gereği** zorunlu.
6. **Gerçek kişi ve ünlü yüzü** kullanmayın: deepfake her platformda ve Colab/Kaggle'da yasak.
7. **Yaş:** Flow 18+ istiyor.

---

## 7. Kaynaklar (erişim 28.11.2026)
- Google Flow kredileri: https://support.google.com/flow/answer/16526234
- Flow ülkeleri: https://support.google.com/labs/answer/16353544
- Gemini limitleri: https://support.google.com/gemini/answer/16275805?hl=en-GB
- Google görünür filigran ayarı (17.08.2026): https://gigazine.net/gsc_news/en/20260817-google-visible-watermarks/
- Vuela ücretsiz video araçları (10.11.2026): https://vuela.ai/blog/best-free-ai-video-generators-2026
- WhichOneIsReal ücretsiz limitler (Kasım 2026): https://whichoneisreal.com/compare/best-free-ai-video/
- Dreamina yenilenen ve tek seferlik krediler (24.10.2026, taraflı): https://dreamina.capcut.com/ai-video/free-ai-video-generators-recurring-credits-vs-trials-2026
- Dreamina ücretsiz rehber: https://dreamina.capcut.com/ai-video/free-ai-video-generator-usable-clips-guide-2026
- Kling fiyatları: https://magichour.ai/blog/kling-ai-pricing
- Vidu fiyatları (18.11.2026): https://wavespeed.ai/blog/cost-and-billing/vidu-ai-pricing-2026/
- Grok Imagine ücretli oldu (24.03.2026): https://www.latestly.com/socially/technology/xai-restricts-grok-imagine-image-and-video-generation-to-supergrok-subscribers-ends-free-access-7365341.html
- Meta Vibes Türkiye: https://www.teknoblog.com/meta-ai-vibes-turkiye-kullanima-sunuldu/
- Karakter tutarlılığı karşılaştırması (05.08.2026): https://www.elser.ai/blog/kling-3-vs-seedance-2-vs-veo-3-1-character-consistency
- Nano Banana 2 + Veo 3.1 iş akışı (03.04.2026): https://pctechmag.com/2026/04/nano-banana-2-veo-3-1-the-ultimate-ai-drama-workflow/
- TheWrap YZ mikrodiziler (11.06.2026): https://www.thewrap.com/?p=8031164
- Bilibili yüz tutarlılığı (27.04.2026): https://www.bilibili.com/read/cv48245381
- Wan 2.2 5B: https://huggingface.co/Wan-AI/Wan2.2-TI2V-5B
- Kaggle Wan stüdyosu (T4 süreleri): https://github.com/davidsantdev/wan-studio
- Colab/Kaggle video kurulumu (14.11.2026): https://www.ahmedfayaz.com/blog/colab-kaggle-video-generation-setup
- Colab SSS: https://research.google.com/colaboratory/faq.html
- LTX-2.3 lisansı: https://huggingface.co/Lightricks/LTX-2.3/blob/main/LICENSE
- HunyuanVideo 1.5: https://huggingface.co/tencent/HunyuanVideo-1.5
- Vagon fiyatları: https://vagon.io/pricing
- TikTok YZ etiketi: https://support.tiktok.com/en/using-tiktok/creating-videos/ai-generated-content
- TikTok YZ ile para kazanma: https://www.lilachbullock.com/ai-generated-videos-monetised-tiktok/
- Creator Rewards ülkeleri: https://ttcalculator.net/learn/creator-fund-countries/
- YouTube inauthentic content (13.07.2026): https://www.tubefilter.com/2026/07/13/youtube-inauthentic-content-monetization-policy-update/
- ABD Telif Ofisi Bölüm 2: https://copyright.gov/newsnet/2025/1060.html · https://ipwatchdog.com/2025/01/29/part-two-copyright-office-ai-report-says-creative-prompting-doesnt-constitute-authorship/
- Türkiye YZ ve telif: https://bilalalyar.av.tr/yapay-zeka-telif-hakki/
- ElevenLabs ücretsiz plan: https://dupple.com/learn/is-elevenlabs-free
