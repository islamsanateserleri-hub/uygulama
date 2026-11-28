# R14 — Bedava ana plan (Plan A: 0 TL · Plan B: en çok 1000 TL)

Tarih: 2026-11-28. Dayanak: D4–D12, 04_DECISION_LOG (K-017..K-027), DURUM.md (28 Kasım oturumları).
**Bu plan test edilmedi.** D dosyalarının hepsi yalnız web araştırması. Ölçülen tek şeyler DURUM.md'deki Unity düzeltmeleri ve Unreal çökmeleri.
Web araması yapılmadı. Çelişkiler dosyaların kendi kaynaklarıyla çözüldü (bölüm 4).

Sabitler:
- PC: Ryzen 5 5600GT, yalnız tümleşik Vega 7, 64 GB RAM, ~40 GB boş disk, uzak masaüstü. Unity 6000.6.9f1 URP çalışıyor. Unreal 5.8.4 iki kez çöktü.
- Bütçe en çok 1000 TL (K-027). Kur: 1 $ ≈ 52 TL (D8), yani 1000 TL ≈ 19 $. "Ne kadar ücretsiz o kadar iyi."
- Yapay zekâ ile video üretmek yok. Metin ve ses için yapay zekâ serbest (Gemini sesi zaten kullanılıyor).
- Hedef: 1080x1920 dikey, olabildiğince gerçekçi ve sinematik, Türkçe ses ve altyazı.
- Kayıt çevrimdışı (sabit kare hızı). Yavaş PC kaydı uzatır ama görüntüyü bozmaz. Asıl sınır bellek ve iş süresi.

---

## 1. Dürüst beklenti (tek paragraf)

0 TL ile ulaşılabilecek en iyi sonuç "iyi bir oyun ara sahnesi"dir, film değildir. En büyük ve en ucuz kazanç karakterden değil; doğru altyazı, gerçekten yürüyen karakter, gerçek gökyüzü ışığı (HDRI), taş/ahşap gibi gerçek malzemeler, sis ve alan derinliğinden gelir. Kullanıcının 26 Kasım'daki şikâyeti de zaten bunlardı (altyazı kayması, yerinde duran karakterler). Bedava karakterler için: Renderpeople taramaları geniş planda neredeyse fotoğraf gibi görünür ama yüzleri konuşmaz. MPFB karakterleri konuşur ama yüzleri "oyun karakteri" düzeyindedir. ~300–600 TL'ye bir kez GPU kiralayıp MetaHuman yapılırsa yüz ve cilt belirgin şekilde daha gerçekçi olur. Ama Unity URP'de bu, Unreal'daki görüntünün tahminen %60–70'i kadar kalır (D9 tahmini, test yok): saç kart olur, cilt biraz plastik kalır, kırışıklık haritası olmaz. Hiçbir yol yapay zekâ videolarındaki ya da Hollywood'daki fotogerçek insanı vermez. Bunu bugün söylemek, sonra hayal kırıklığından iyidir. Plan B'nin parası küçük, ama emeği büyük: ilk MetaHuman karakter için 4–7 iş günü (D9).

---

## 2. Planlar

Etiketler: **[K]** = kullanıcı gerekir (hesap, ödeme, indirme onayı, bilgisayar başında olmak). **[C]** = Claude köprü ve dosyalarla yapabilir.
Süreler tahmindir. "İş günü" = Claude'un çalışması + kullanıcının kısa onayları.

### Plan A — 0 TL (önerilen başlangıç)

| # | Aşama | Görünür değişiklik | Emek | Riskler | Lisans | Kullanıcıdan |
|---|---|---|---|---|---|---|
| A0 | **Temel kusurlar** (zaten açık işler): satır satır ses, gerçek kelime zamanıyla altyazı; yürüme mesafesi (şatoda ~0,7 sn kalıyor); yakın planda yana/aşağı dönük yüz; konuşanın önünü kapatan kişi | Altyazı sesle aynı anda. Karakter gerçekten yer değiştirir. Yüz kameraya bakar | 3–6 iş günü | Toplu ses tasarruflu ama bölme tahmini altyazıyı bozdu; satır satır ses Gemini kotasını (~10 istek/gün) yer | — | Yeni sesleri dinlemek |
| A1 | **Işık ve görünüm**: Poly Haven HDRI gökyüzü + ortam ışığı + mekân başına bir kez çekilen yansıma sondası (reflection probe), yumuşak gölge, SSAO, HDR + ACES, sahne LUT'u, bokeh alan derinliği, mesafe sisi | "Oyuncak kutu" görüntüsü en çok burada kırılır. Gerçek gökyüzü, doğru yansıma, köşelerde karartma, bulanık arka plan | 2–4 iş günü | Kayıt yavaşlar; EXE'de alan derinliği gölgelendiricisi derlemeye girmiyor (DURUM'daki eski hata) | Poly Haven CC0 | İndirme onayı (hesap yok) **[K]** |
| A2 | **Gerçek malzeme**: kodla kurulan kutulara CC0 PBR doku (taş, ahşap, sıva, toprak, çim, metal), dünya koordinatlı (triplanar) | Duvar taş gibi, zemin toprak gibi görünür | 2–4 iş günü | Doku belleği (1K–2K kullan); UV gerilmesi | Poly Haven / ambientCG CC0 | İndirme onayı **[K]** |
| A3 | **Daha gerçekçi karakter** (bedava): Renderpeople ücretsiz rigli taramalar (konuşmayan rol, figüran, geniş plan) + **MPFB 2.0.16** ile 2–4 konuşan karakter (ARKit uyumlu 52 yüz birimi) + Rocketbox'a daha iyi cilt/saç gölgelendiricisi | Geniş planda gerçek insan taraması. Konuşan karakterde daha düzgün yüz ve ifade | 1–2 hafta | Renderpeople yüzü hareket etmez; MPFB yüz şekillerinin FBX'e çıkıp çıkmadığı bilinmiyor; MPFB saçı zayıf; Humanoid eşleme elle kontrol | Renderpeople: video serbest, dosyaları paylaşılan projeye/EXE'ye koyma, hakaret içerikli sahnede kullanma, yapay zekâ eğitimi yok. MPFB çıktısı CC0 | **Blender kurulumu (~1 GB) onayı [K]**. İsteğe bağlı: Reallusion hesabı (ücretsiz CC tabanları) **[K]** |
| A4 | **Hareket kütüphanesi**: Quaternius UAL1+2 (CC0), Mixamo, Kevin Iglesias Basic Motions (Asset Store sürümü), Everyday Actions Pack 2, ActorCore 18 örnek; kök hareketli (root motion) yürüme + Foot IK. Özel jest için TDPT (telefon videosundan hareket) | Doğal yürüme, oturarak konuşma, telefonla yazma gibi günlük hareketler | 3–6 iş günü | İskelet eşleme; ayak kayması; TDPT tek kamerada titrer | CC0 / telifsiz; Mixamo ve diğerlerinin ham dosyası dağıtılmaz. Bandai Namco, LAFAN1, BOOTH VRMA **kullanılmaz** (ticari değil) | **Adobe ID (Mixamo) [K]**. TDPT için kısa telefon videosu çekmek (isteğe bağlı) **[K]** |
| A5 | **Türkçe dudak ve altyazı**: metin bilindiği için "hizalama" (Montreal Forced Aligner Türkçe; yedek Vosk small-tr). Türkçe harf→ağız şekli kuralı; yedek uLipSync (önceden pişirilmiş). Denetim için whisper.cpp | Ağız sesle aynı açılıp kapanır; altyazı kelime kelime | 3–5 iş günü | MFA'nın Windows'a kurulumu ağır; Vosk Türkçe doğruluğu bilinmiyor | MFA modeli CC BY 4.0, Vosk Apache 2.0, uLipSync MIT | Python/conda kurulum onayı **[K]** |
| A6 | **Ses yedeği**: Gemini ana ses kalır. Kota yetmezse yerelde Chatterbox Multilingual (MIT, Türkçe, duygu ayarı) dinleme testi. Edge TTS kullanılmaz; Piper yalnız taslak | Kota bitince iş durmaz | 1–2 iş günü | Chatterbox'un Türkçesi dinlenmedi; CPU'da çok yavaş olabilir | Chatterbox MIT (duyulmayan filigran ekler) | Kurulum onayı (birkaç GB) **[K]** |
| A7 | **Atmosfer ve isteğe bağlı 2,5D**: hacimsel ışık/sis (CristianQiu, MIT) yalnız şato/orman/korku için. Geniş planda fotoğraf/HDRI'yı kutulara yansıtma (kamera projeksiyonu) + gölge yakalayan zemin | Işık huzmeleri, sisli şato. Geniş planda fotoğraf gerçekliği | 4–8 iş günü | Proje kuralı "paket eklenmez" → kaynak kod Assets'e kopyalanır, onay gerekir. 2,5D'de ışık uyumsuzluğu, kamera az oynayabilir. K-024'e göre 2,5D deneme kapsamında değildi → en sona | MIT; CC0 HDRI | Paket/kod ekleme onayı **[K]** |
| A8 | (İsteğe bağlı) **Blender Eevee ile tek tük "güzel plan"** ve kapak görseli | Birkaç saniyelik daha sinematik ara plan | 2–4 iş günü | Vega'da Eevee hızı bilinmiyor; Unity planlarıyla ışık uyuşmazlığı | GPL araç, çıktı sizin | — |

**Plan A toplam:** kabaca 4–7 hafta, parça parça. Her aşamadan sonra kısa video çıkar ve kullanıcı bakar.
**Plan A'da para gerektiren bir şey yok.** İsteğe bağlı tek ücretli kalem Quaternius "Source" (Patreon 10 $ ≈ 520 TL). Önerilmez; ücretsiz kısım yeter.

### Plan B — en çok 1000 TL (Plan A + bir kerelik MetaHuman)

**Ne zaman başlar:** Test 2'den sonra (bölüm 3), kullanıcı "bedava karakterlerin yüzü hâlâ sahte" derse. K-012'nin "önce yüz testi, lisans teyidi" koşulu böyle karşılanır. Plan A'nın A0–A2'si (ışık, malzeme, senkron) her durumda önce yapılır; MetaHuman kötü ışıkta da kötü görünür.

| # | Aşama | Görünür değişiklik | Emek | Riskler | Lisans | Kullanıcıdan |
|---|---|---|---|---|---|---|
| B0 | **Hazırlık (ücretsiz, makine kapalıyken)**: 4–6 karakterin tarifi (yaş, ten, saç, giysi). Kısa saç / kafaya yakın saç, vücuda yapışık giysi seçilir. Kiralama adım listesi ve dışa aktarma betiği yazılır | — | 1 iş günü | Saat boşa gitmesin diye her şey önceden | — | **Epic hesabı + iki adımlı doğrulama [K]**. Google Drive (15 GB ücretsiz) **[K]** |
| B1 | **Bedava GPU denemesi önce**: RTX 3060+ (8 GB VRAM) bilgisayarı olan arkadaş, okul/üniversite labı, teknopark. 3–6 saat yeter. İnternet kafede kurulum genelde mümkün değil | — | — | Bulunamayabilir | — | Bilgisayarı bulmak ve başında olmak **[K]** |
| B2 | **Bulunamazsa kiralama (tek oturum, 10–15 saat)**. Önerilen sıra: (1) **TensorDock**, Windows, RTX 3090 24 GB: ~260–520 TL; (2) **Vast.ai**, Ubuntu masaüstü, RTX 4090: ~380–620 TL (kart veya kripto); (3) airgpu en kolay ama 620–1030 TL, sınırda → kaçın. Türkiye'deki bireye %20 KDV eklenebilir → TensorDock en kötü ~620 TL | — | 1 gün (bilgisayar başında) | Makinede Windows imajı ya da boş GPU olmayabilir. Türk banka kartı reddedilebilir. Makine **silinmezse** disk ücreti sürer (TensorDock'ta bakiye bitince makine silinir). En az yükleme 5 $ | Kiralık makine şartları (genel IaaS) | **Ödeme: banka sanal kartı, limit 1000 TL, yurt dışı/internet alışverişi açık [K]**. Kiralama boyunca bilgisayar başında olmak **[K]** |
| B3 | **Kiralık makinede**: UE 5.8 kur (yalnız Win64/Linux) → MetaHuman Creator → 4–6 karakter → **Export > DCC Export** (Bake Makeup açık) → **Assemble: UE Optimized – High** → saç/kaş kartı mesh'lerini FBX, dokularını PNG olarak al → hepsini zip'le, Drive'a yükle → makineyi **sil** | — | 10–15 saat (2–3 sa kurulum + derleme) | Autorig ve doku bulutta, internet şart. Saç kartlarını dokusuyla çıkarmanın resmi yolu yok (3 forum başlığı yanıtsız). DCC Export dokuları en çok 2K verir (bizim için yeterli). Linux'ta DCC Export ayrıca yazılı değil | MetaHuman: UE EULA; Unity'de kullanmak serbest; yıllık gelir 1 M $ altı ücretsiz | Epic girişi **[K]** |
| B4 | **Vega PC'de Blender**: Poly Hammer Character DNA (temel sürüm ücretsiz) → head/body DNA içe al → betikle ~17 yüz şeklini pişir (jawOpen, göz kırpma, gülümseme, kaşlar, 5 visem) → yüz eklemlerini sil, göz kemiklerini bırak → saç kartlarını bağla → tek FBX | — | İlk karakter 1,5–2,5 gün; sonrakiler 2–3 saat | Araç beta. Shape key kaydetme özelliği Pro'da → kendi betiğimiz gerekir. DNA eksen hatası (5.8 bilinen sorun). Boyunda dikiş çizgisi (5.8 bilinen sorun, yaka ile örtülür) | Character DNA GPLv3 | **Poly Hammer Portal hesabı [K]**. Blender kurulumu (A3'te yapıldıysa yok) |
| B5 | **Unity**: Humanoid, blend shape açık, sıkıştırma kapalı, deri ağırlığı 8/sınırsız. Cilt: pre-integrated gölgelendirici (Unity URP Defender demosunun gölgelendiricileri, karakteri değil) veya bentoBAUX SSS. Göz, kart saç (alpha clip + anizotropik). `karakter_listele`, `YuzTesti`, `AgizSenkronTesti` | Yakın planda belirgin daha gerçek yüz, göz, cilt | 1,5–3 iş günü | Cilt plastik kalabilir. Twist kemikleri eşlenmez → omuz/dirsekte hafif kayma. MetahumanURP (MIT) Unity sürümü bizimkiyle aynı (6000.6) ama yüz sürücüsü yalnız macOS | Defender karakteri ticari değil (yalnız gölgelendirici fikri alınır); bentoBAUX "kredi vererek" | — |

**Plan B toplam:** para ~260–620 TL (KDV dahil en kötü). Emek: Plan A'ya ek **1–2 hafta** + 1 kiralama günü.
**Bütçe kuralı:** Kiralama + KDV 1000 TL'yi geçecekse dur. Kalan parayı Patreon/Cascadeur gibi şeylere harcama (Cascadeur Indie 19 $ ≈ 990 TL tek başına bütçeyi bitirir).
**0 TL ihtimali:** E2E Networks (Hindistan) kimlik + özçekimle 20 $ kredi veriyor, kart istemiyor; ama kredinin GPU'da geçip geçmediği ve Türk kimliğinin kabulü bilinmiyor. Önce destek ekibine yazılı sorulur **[K]**. AWS kredileri GPU kotası 0 olduğu için pratik değil.

### Lisans özeti (videoda kullanım)
- **Serbest ve dertsiz:** CC0 (Poly Haven, ambientCG, Quaternius, MPFB çıktısı), MIT (Rocketbox, uLipSync, CristianQiu, Chatterbox).
- **Videoda serbest, ham dosya paylaşılmaz:** Renderpeople, Mixamo, MetaHuman, Megascans (Fab), Asset Store paketleri.
  → Hikaye Stüdyosu EXE'si/kurulumu başkasına verilirse bu dosyalar **içine konmaz**. Şu an kurulum kullanıcının kendisi için.
- **MetaHuman ek kural:** karakter ya da video yapay zekâ modeli eğitmek için kullanılmaz. Gemini sesiyle konuşturmak eğitim değildir (D9).
- **Kullanma:** Bandai Namco, LAFAN1, BOOTH VRMA, Piper dfki, XTTS (ticari değil); Edge TTS (resmi değil); Character Creator denemesi (filigran); iClone Personal'ın kendi render çıktısı (yalnız kişisel); Fantasy Kingdom demo; GTA/RDR2 görüntüsü (para kazanılan video izin dışı).
- Şu an paylaşım "ticari değil" (DURUM, 26 Kas kararı). Yine de plan, ileride para kazanılırsa da geçerli olan varlıkları seçer.

### Plan dışı bırakılanlar (neden)
- **Unreal'ı bu PC'de kullanmak:** iki deneme de çöktü/dondu (DURUM). `-d3d11` denemesi kullanıcı başındayken yapılacak not olarak duruyor (K-026), ama MetaHuman Creator için kanıt yok ve donma kiralık PC'de bağlantıyı koparıyor. TDR kayıt ayarı **değiştirilmez**.
- **Bedava bulut GPU:** kartsız, Windows'lu, GPU'lu ücretsiz makine bulunamadı (D4, D5).
- **Eski Quixel Bridge/web MetaHuman:** web uygulaması 5 Kasım 2026'da kapandı (D6).
- **Hazır benzer uygulama:** AI'sız, bedava, zayıf PC'de metinden 3D video yapan yaşayan araç yok (D10). Unity uygulaması ana fabrika kalır.
- **Unity Digital Human / Enemies:** yalnız HDRP, Vega'da pratik değil (D4, D7).

---

## 3. İlk 3 test (her biri kullanıcının izleyeceği kısa bir video)

Ortak kurallar:
- `kaydet` her satırın sesi olmadan reddeder. Gemini kotasını harcamamak için **zaten sesli kısa bir hikâye** (ör. 28 Kasım'daki ornek30, şato) ya da en çok 1–2 yeni satır kullanılır.
- Her test için **önce/sonra** aynı sahne, aynı kamera. Video kopyası `HikayeKomut/son_video.mp4`'e düşer. Claude kareleri (`onizleme_goruntu`, Kare Serisi) kendisi kontrol eder, ama **son kararı kullanıcı videoyu izleyerek verir**.
- Her testte ölç: kayıt süresi (10 sn video kaç dakikada çıktı), bellek hatası var mı, mor/eksik doku var mı.

### Test 1 — Şatoda ışık: HDRI + PBR + sis (Plan A1–A2)
- **Ne:** Şato mekânı, 10–15 sn. Önce bugünkü görünüm, sonra: Poly Haven HDRI (gökyüzü + ortam ışığı), bir kez çekilen yansıma sondası, yumuşak gölge, SSAO, ACES + LUT, bokeh alan derinliği, mesafe sisi, 3–4 CC0 malzeme (taş duvar, taş zemin, ahşap).
- **Çıktı:** iki kısa video (önce/sonra) + iki kare.
- **Geçme ölçütü:**
  1. Kullanıcı "sonra" videosunu açıkça daha iyi buluyor ("oyuncak gibi değil").
  2. Duvar ve zeminde doku gerilmesi ya da mor/eksik doku yok.
  3. 10 sn video en çok ~15 dakikada kaydediliyor, çökme yok.
  4. Yüzler çok karanlık değil (ışık ayarı karakteri boğmuyor).
- **Kalırsa:** ışık ayarı geri alınabilir; önce/sonra karşılaştırması K-005'in "URP yeterli mi" sorusunu yanıtlar.

### Test 2 — Karakter yan yana: Rocketbox / Renderpeople / MPFB (Plan A3; Plan B kararı)
- **Ne:** Test 1'in ışıklı şatosunda üç karakter yan yana: bugünkü Rocketbox, bir Renderpeople ücretsiz rigli tarama, bir MPFB 2.0.16 karakteri. Önce geniş plan (hepsi beklemede), sonra her birine 3–4 sn yakın plan. MPFB karakteri göz kırpar, gülümser, "a" der.
- **Çıktı:** 15–20 sn video.
- **Geçme ölçütü:**
  1. Üçü de Humanoid olarak doğru duruyor (kırık kol/bacak, ters el yok).
  2. MPFB'de en az jawOpen, göz kırpma ve gülümseme Unity'de çalışıyor (shape key'ler FBX'ten geldi).
  3. 2K dokularla bellek hatası yok.
  4. **Kullanıcı sırayla dizer:** hangisi daha gerçek? Bu yanıt kararı verir:
     - "MPFB/Renderpeople yeterli" → Plan A ile devam, para harcanmaz.
     - "Yüzler hâlâ sahte" → Plan B (MetaHuman) hazırlığı başlar.
- **Not:** Renderpeople yüzü konuşmaz; testte yalnız görünüşü değerlendirilir.

### Test 3 — Yürüme + Türkçe dudak senkronu (Plan A0, A4, A5)
- **Ne:** Bir karakter şatoda 3–4 m yürür (kök hareketli Quaternius veya Kevin Iglesias klibi), durur, kameraya döner ve bir Türkçe cümle söyler. Altyazı kelime kelime, hizalayıcının (MFA ya da Vosk) verdiği zamanla. Ağız Türkçe harf→ağız şekli kuralıyla.
- **Çıktı:** 10–15 sn video (+ kelime zamanları JSON'u ve altyazı olay günlüğü).
- **Geçme ölçütü:**
  1. Yürüyüş en az 2–3 sn sürüyor, ayak kayması gözle fark edilmiyor.
  2. Her kelimenin altyazısı, sesin içindeki kelimeden en çok ~0,15 sn sapıyor (günlükten ölçülür).
  3. "m/b/p" seslerinde ağız kapanıyor, "a"da açılıyor; konuşma bitince ağız kapalı.
  4. Kullanıcı "gerçekten konuşuyor ve yürüyor" diyor.
- **Neden önemli:** 26 Kasım reddinin iki ana nedeni (altyazı kayması, yerinde duran karakter) doğrudan bu testte ölçülür.

---

## 4. D dosyaları arasındaki çelişkiler ve çözümleri

| # | Çelişki | Çözüm |
|---|---|---|
| 1 | D5: "MetaHuman Creator Linux'ta destekleniyor mu, doğrulanmadı; risk yüksek." D8: 5.7'den beri Creator, 5.8'de Animator da Linux'ta resmi. | **D8 geçerli.** Epic duyurusu ve 5.8 sürüm notunu doğrudan aktarıyor. Kalan belirsizlik yalnız "DCC Export Linux'ta açıkça yazılı değil". Bu yüzden Windows'lu TensorDock 1. sırada, Linux'lu Vast.ai 2. sırada. |
| 2 | D9 kiralık maliyeti için "Vast.ai'de RTX 3060 ~0,13 $/sa" (D5'ten) diyor. D8: Vast.ai masaüstü sanal makineleri yalnız RTX 4090/5090'da; 3060'ta masaüstü yok. | **D8 geçerli.** Maliyet D8'in tablosundan alındı (Vast ~380–620 TL, TensorDock ~260–520 TL). |
| 3 | D12: Quaternius Patreon 10 $ ≈ 350–450 TL. D8 kuru (52 TL/$) ile ≈ 520 TL. | **D8 kuru kullanıldı** (güncel Bloomberg HT kuru + banka payı). Sonuç: Patreon, kiralamayla birlikte bütçeye sığmayabilir → önerilmedi. |
| 4 | D5 tablosunda internet kafe "en gerçekçi" yol gibi görünüyor. D4 ve D5'in kendi notu: kafelerde kurulum yasak/silinir, UE 50 GB+. | **Kafe yedek.** Arkadaş/okul PC'si 1. sırada; kafe yalnız sahibinin izniyle, kendi harici SSD'nde kurulu UE ile. |
| 5 | D4 §3.5: MetaHuman dokuları 8K'ya kadar. D9: DCC Export çözünürlük ayarını yok sayıyor, yüz en çok 2K çıkıyor (bilinen sorun). | **D9 geçerli** (5.8 bilinen sorunlar sayfası). Bizim için sorun değil; iGPU'da zaten 2K kullanılacak. |
| 6 | D4: uzak masaüstü GPU yerine "Basic Render Driver" kullanıyor olabilir, çökme nedeni bu olabilir. D6: Unreal günlüğü AMD adaptörünü seçip gerçek "GPU hung" aldı. | **D6 geçerli** (gerçek günlük). Donanım GPU'su kullanılıyordu; sorun sürücü ilkesi değil, Vega + D3D12. |
| 7 | D9: "Projemiz Unity 6.0 olabilir, MetahumanURP 6000.6 istiyor." DURUM: proje Unity 6000.6.9f1. | **Sürüm uyuyor.** Ama MetahumanURP'nin yüz sürücüsü ve saç çizicisi yalnız macOS/Metal → Windows'ta yalnız gölgelendirici fikirleri işe yarar. Engel sürüm değil, platform. |
| 8 | D11 2,5D fotoğraf projeksiyonunu 4. hızlı kazanç sayıyor. K-015/K-024: 2,5D ana yol değil, deneme kapsamında değil. | **Karar günlüğü geçerli.** 2,5D Plan A'nın sonuna (A7) isteğe bağlı kondu; ilk 3 testte yok. Kullanıcı isterse yeniden açılır. |
| 9 | K-020/K-021: ücretli her şey bütçe dışı, yalnız disk. K-027: bütçe 1000 TL. | **K-027 daha yeni → geçerli.** Plan B buna dayanıyor. K-022'nin "kart yoksa UE yok" kuralı bozulmuyor: UE yalnız kiralık makinede, bir kez. |
| 10 | K-012: "MetaHuman şimdilik yok." Plan B MetaHuman öneriyor. | Çelişki değil, koşul: K-012'nin koşulları (yüz testi, donanım, lisans) Test 2 + kiralık GPU + D9 lisans okumasıyla karşılanınca açılır. |
| 11 | DURUM: tüm film sesleri Gemini ile 2 istekte (toplu ses) üretildi; kota için iyi. Aynı DURUM: bu bölme tahminleydi ve kullanıcı altyazı kaymasından şikâyet etti. | **Satır satır ses + hizalama** (A0, A5). Kota yetmezse kalan satırlar ertesi gün ya da Chatterbox ile. Toplu sesi tahminle bölme bir daha kullanılmaz. |
| 12 | D4/D9: Character DNA ücretsiz tarihi 2026-07-22 / 2026-07-12. | Önemsiz (blog ve haber tarihi farkı). Bugün ücretsiz olduğu iki kaynakta da aynı. |

Web araması gerekmedi; çelişkilerin hepsi dosyaların içindeki kaynaklarla çözüldü.

---

## 5. Hâlâ bilinmeyenler

**Bu PC ve Unity:**
- Işık + SSAO + alan derinliği + sis açıkken 1080x1920 kaydın Vega'da ne kadar süreceği (Test 1 ölçer).
- Unity'nin DX11 mi DX12 mi çalıştığı (Gaussian splat gibi bazı şeyler DX11'de çalışmaz; ana plan buna bağlı değil).
- Hacimsel sis paketinin 6000.6.9 ile gerçekten çalışması.
- EXE'de alan derinliği gölgelendiricisinin derlemeden atılması sorununun çözülüp çözülmeyeceği.

**Bedava karakterler:**
- MPFB 52 yüz biriminin FBX'e shape key olarak çıkıp çıkmadığı ve kalitesi.
- Renderpeople Unity paketlerinin URP malzemeli mi geldiği, iskeletin Humanoid'e sorunsuz eşlenip eşlenmediği.
- HeadBox (Rocketbox yüz şekilleri) deposunun lisansı ve dosyaların gerçekten var olup olmadığı.

**Ses ve senkron:**
- Montreal Forced Aligner'ın bu Windows PC'ye kurulup kurulamayacağı; Vosk Türkçe doğruluğu.
- Chatterbox'un Türkçe telaffuzu ve CPU'da bir satırın kaç dakika sürdüğü.
- Gemini ücretsiz kotası resmi sayfada sayı olarak yok (forumdaki ~10/gün kullanıldı).

**Plan B:**
- Kiralama anında TensorDock'ta Windows'lu 24 GB GPU bulunup bulunmayacağı; Windows'un lisanslı olup olmadığı.
- Türk banka kartının bu sitelerde geçip geçmeyeceği; %20 KDV eklenip eklenmeyeceği.
- Saç kartlarının dokusuyla çıkarılabilip çıkarılamayacağı (en riskli adım; yedek: Blender'da kendi kart saçımız ya da kısa saç/şapka/başörtüsü).
- Character DNA'nın LOD1'i ayrı alıp alamadığı; yüz pişirme betiğinin ücretsiz sürümle çalışıp çalışmadığı.
- MetaHuman'ın URP'de gerçekte ne kadar iyi göründüğü (%60–70 yalnız tahmin).
- E2E Networks 20 $ kredisinin GPU'da geçerliliği.
- MetaHuman/UE EULA'sının dağıtım maddesi elle okunmadı (sayfa açılmadı). Güvenli yol: ham dosyalar hiçbir paylaşılan pakete girmez.

**Kullanıcıya bağlı:**
- Para kazanılacak mı? (Şu an "ticari değil".) Plan iki durumda da geçerli varlıkları seçti, ama Sims/GTA gibi yan kanal fikirleri buna bağlı.
- RTX'li bir bilgisayara (arkadaş, okul) bir kez erişimi var mı? Varsa Plan B 0 TL olur.
- Face ID'li iPhone var mı? Varsa ücretsiz yüz yakalama (Unity Face Capture) eklenebilir.
- Unity koşulları riski (K-025'te kullanıcı kabul etti) sürüyor.

---

## Ek: Kullanıcının yapacağı işler listesi (tek yerde)

| Ne zaman | İş |
|---|---|
| Plan A başında | Poly Haven/ambientCG indirmelerine onay; Blender kurulumuna onay |
| A4 | Adobe ID açmak (Mixamo) |
| A3 (isteğe bağlı) | Reallusion hesabı (ücretsiz CC tabanları) |
| A5–A6 | Python/conda, MFA ve Chatterbox kurulumuna onay (birkaç GB disk; boş disk ~40 GB, dikkat) |
| A7 | Hacimsel sis kodunun projeye eklenmesine onay ("paket eklenmez" kuralı) |
| Her test sonrası | Videoyu izleyip "geçti/kaldı" demek |
| Plan B (yalnız Test 2 sonrası) | Epic hesabı; Poly Hammer Portal hesabı; RTX'li bilgisayar bulmak **ya da** sanal kartla ödeme (limit 1000 TL); kiralama günü bilgisayar başında olmak; makineyi sildiğini kontrol etmek |
