# D3 — Yapay zekâ asistanı seçenekleri ve "yalnız ek disk" bütçesi
Tarih: 2026-11-27. **Test yapılmadı.** Kod değiştirilmedi, indirme/kurulum/hesap yok, Unity'ye dokunulmadı; proje yalnız okundu. WebSearch: 24 (sınır 30). Sayfalar özetleyici araçla okundu. Hukuki görüş değildir. Kaynaklar: `../SOURCES/D3_kaynaklar.md`.

## Görev tanımı
Kullanıcı (bugün): "önce araştırmaları tamamla"; "Yapay zeka asistanı olacak"; gerçekçiye yakın 3D ve stilize 3D birlikte gösterilsin; bütçe en çok bir ek disk (~150 $, ABD göstergesi).
**Anlam belirsiz, ikisi de kapsandı:** (Y1, ana araştırmacının yorumu; kullanıcı onaylamadı) yapay zekâ hikâye/sahne hazırlamaya yardım eder, görüntü üretmez. (Y2) uygulamanın içinde bir yapay zekâ asistanı bulunur.
Projede Y2 yönünde eski kayıt var: `DURUM.md` "yapay zekâya söyleyerek değiştirsin" ve "Tek tuş düz hikâye → sahne (yapay zekâ ile)" diyor. Bu, bugünkü cümlenin anlamını kanıtlamaz; **kullanıcıya sorulmalı**.

## 1. Projede bugün ne var (KOD, statik okuma)
- Uygulama içinde metin yardımcısı **zaten var**: `Assets/HikayeStudyo/Uygulama/YardimciGemini.cs`, model `gemini-3.5-flash-lite`, iki düğme: "öneri al" ve "biçime çevir". Yalnız düğmeyle çalışır, hikâye metnini (yorum satırları boşaltılmış) Google'a gönderir, sınır 20.000 karakter. `DURUM.md` kaydına göre EXE'de denenmiş (bu oturumda denenmedi).
- "Biçime çevir" istemine yalnız karakter, mekân ve duygu listesi giriyor; **hareket, kamera, efekt listesi ve sözdizimi belgesi girmiyor**. Yanıt kendiliğinden doğrulanıp yeniden istenmiyor; kullanıcı "Metne uygula" deyip Hata paneline bakıyor.
- İnternetsiz, yapay zekâsız yardımcı da var (`YardimciMotor`; D2 §2.1).
- Anahtar: uygulama tarafında Windows DPAPI ile şifreli saklama (`GeminiUygulama.cs`, `SirKoruma`). Editor tarafı (`Editor/TopluSes.cs`, `GeminiSes.cs`) `EditorPrefs`'ten düz okuyor görünüyor (ÇIKARIM; kayıt defteri incelenmedi). Anahtar değeri okunmadı.
- `GEMINI.md` dış bir Gemini aracına Unity'yi dosya köprüsüyle sürmesini söylüyor: Q-016 / K-004 riski bu belge için de geçerli (ÇIKARIM). Belgedeki talimat uygulanmadı.

## 2. Görevler: asistan ne yapabilir?
Tamamı ÇIKARIM; hiçbiri denenmedi.
| Görev | Girdi → çıktı | Denetleyen | Beklenen güvenilirlik |
|---|---|---|---|
| Hikâye taslağı | fikir → etiketli metin | doğrulayıcı (biçim, ad) + kullanıcı | biçim denetlenebilir; hikâyenin iyiliği denetlenemez |
| Düz yazı → sahne etiketi | düz metin → mekân, konuşan, duygu, hareket | doğrulayıcı + kullanıcı | yanlış konuşan/mekân geçerli biçimde olabilir; doğrulayıcı yakalamaz |
| Kamera önerisi | hikâye → desteklenen plan etiketleri | doğrulayıcı + önizleme | yalnız kayıt defterindeki komutlar verilirse |
| Kontrol | hikâye → madde madde öneri | kullanıcı | yardımcı; hakem değil |
| Tek parçayı değiştirme | "3. satırda kız otursun" → yalnız o satır | fark gösterimi + doğrulayıcı | kullanıcının eski isteğine en yakın |

### Doğrulayıcıyla çalışma (öneri)
1. Asistana güncel listeler verilir (`karakter_listele`, `mekan_listele`, `komut_listele` çıktıları + kısaltılmış sözdizimi).
2. Asistan yalnız **taslak metin** üretir; asıl hikâyenin üstüne kendiliğinden yazmaz.
3. Doğrulayıcı `satir/kod/mesaj/oneri` verir; asistana en çok 2-3 tur geri beslenir.
4. Kullanıcı farkı görüp onaylar. Ses üretimi (kota) onaydan sonra.
- Dışarıdan çalışmada (Claude) doğrulayıcıyı **kullanıcı Unity/uygulamada elle** çalıştırır; ajan `komut.txt` ile `dogrula` göndermez (K-004).
- Uygulama içinde otomatik doğrula-düzelt döngüsü **kod değişikliğidir**; yapılmadı.
- Yerel modelde Ollama JSON şemasıyla çıktıyı biçime zorlayabiliyor [D3-S022]; llama.cpp GBNF dilbilgisi sunuyor [D3-S023]. Projenin biçimi düz metin etiket; JSON ara biçim istenirse dönüştürücü gerekir.

## 3. Seçenekler
### (a) Kullanıcının mevcut Claude aboneliği, dışarıdan
- Nasıl: Claude (sohbet ya da Claude Code) taslağı metin/dosya olarak yazar; Unity'yi sürmez; kullanıcı uygulamaya alır ve doğrular.
- Ek para yok (abonelik kullanımda; kullanım sınırı var). Kurulum ve kod gerekmez.
- Koşullar: 18+ (ya da yerel rıza yaşı daha yüksekse o); otomatik/insan dışı erişim, API anahtarı ya da açık izin dışında yasak [D3-S011]. Claude Code Anthropic'in kendi aracıdır; Pro/Max için "olağan bireysel kullanım" varsayılır [D3-S009]. Okunan küresel metinde genel ticari yasak görülmedi [D3-S011]; AEA/İsviçre sürümünde "yalnız ticari olmayan" cümlesi var [D3-S012]. Türkiye'deki kullanıcıya hangisinin uygulandığı: küresel sürüm olmalı (ÇIKARIM; kullanıcı kendi hesabında bakmalı). Gelir düşünülürse netleştirilmeli (Q-002).
- Çıktılar: şartlara uyulursa Anthropic haklarını (varsa) kullanıcıya devrediyor [D3-S012].
- Gizlilik: model eğitimi ayarı açıksa veri eğitimde kullanılabilir, saklama 5 yıl; kapalıysa 30 gün; Claude Code oturum kayıtları bilgisayarda düz metin (varsayılan 30 gün) [D3-S010]. Geri bildirim ve güvenlik işareti ayrı istisna [D3-S011].
- **Uygulamanın içine abonelikle koyulamaz:** üçüncü taraf geliştirici claude.ai girişi sunamaz, isteği abonelik kimliğiyle yönlendiremez; ürün API anahtarı kullanmalı. Claude Code'u ürün içinde çalıştırmak Ticari Şartlar ister [D3-S009]. Kullanıcının kendi kişisel aracının `claude` komutunu alt süreç olarak çağırması: açık cümle görmedim, **belirsiz**.
- Anlam: Y1'e uyar; Y2'yi karşılamaz.

### (b) Gemini API ücretsiz katman
- Uygulamada hazır (§1); Y1 ve Y2'yi karşılar.
- Ücret sayfası: 3.5 Flash-Lite ücretsiz katmanda girdi/çıktı bedava; ücretli 0,30 $ / 2,50 $ (1M belirteç) [D3-S004]. TTS modellerinden 3.1 Flash TTS Preview ve 2.5 Flash Preview TTS ücretsiz katmanda görünüyor [D3-S003]; projedeki `gemini-3.8-flash-tts` satırı okunan bölümde yoktu (doğrulanamadı).
- Sayısal günlük sınır resmî sayfada **yok**; AI Studio'da hesaba göre görünüyor [D3-S005]. Metin isteklerinin ses kotasını (projede gözlenen ~10/gün) etkileyip etkilemediği **doğrulanamadı**.
- Koşullar (23 Mar 2026) [D3-S001]: **18+**; 18 yaş altına yönelik ya da onların erişmesi muhtemel uygulamada kullanılamaz. Yalnız desteklenen bölgeler; **Türkiye listede** [D3-S002]. AEA, İsviçre, Birleşik Krallık kullanıcılarına sunulan uygulamada yalnız ücretli hizmet.
- **Veri:** ücretsiz katmanda gönderilen metin ve yanıt Google ürünlerini geliştirmek için kullanılır, **insan inceleyiciler okuyabilir**; hassas, gizli, kişisel bilgi gönderilmemesi isteniyor [D3-S001]. Ücretli katmanda "ürün geliştirmede kullanılmıyor" [D3-S003][D3-S004].
- Anahtar [D3-S006]: parola gibi saklanmalı, Git'e konmamalı, uygulamaya gömülmemeli. Kısıtlanmamış "standard" anahtarlar reddediliyor; 28 May 2026'dan beri yeni anahtarlar "auth" türü. Kullanıcının anahtarı eskiyse AI Studio'da bakmalı. Program başkasına verilirse **herkes kendi anahtarını** girmeli (ÇIKARIM; mevcut tasarım zaten böyle görünüyor).
- İnternet gerekir; model adları ve koşullar değişebilir.

### (c) Bu bilgisayarda yerel model (Ollama / llama.cpp)
- Yazılım: llama.cpp MIT [D3-S023]. Ollama yerel modelde istemleri görmez; bulut özellikleri `OLLAMA_NO_CLOUD=1` ile kapatılır; Windows'ta modeller `C:\Users\...\.ollama\models`, `OLLAMA_MODELS` ile **yeni diske** alınabilir [D3-S021].
| Aday | İndirme boyutu | Lisans | Türkçe kanıtı |
|---|---|---|---|
| Gemma 4 26B (MoE; etkin ~3,8B) | 16-19 GB [D3-S017][D3-S018] | Apache 2.0 [D3-S018][D3-S019] | **bu sürüm için bulunamadı** |
| Gemma 4 12B / E4B | 7,7-8 GB / 6,6-9,5 GB [D3-S017] | Apache 2.0 [D3-S019] | bulunamadı |
| Qwen3 30B (MoE) | 19 GB [D3-S020] | Apache 2.0 [D3-S020] | TurkBench ort. 73,4 [D3-S013] |
| Qwen3.6 35B (MoE) | ~23 GB [D3-S024] | Apache 2.0 [D3-S024] | bulunamadı |
| Gemma 3 12B | boyut bakılmadı | Gemma Terms (açık kaynak lisansı değil) [D3-S019] | TurkBench 71,0; TR-ince ayarlı sürüm 71,2 [D3-S013] |
| Trendyol-LLM-8B-T1 | bakılmadı | **okunmadı** | bir RAG çalışmasında 8B'ler içinde en iyi (%75), ama en yavaş [D3-S014] |
- TurkBench'te küçük modeller belirgin zayıf (ör. Qwen3-14B 43,0; Qwen2.5-7B 54,9); deyim/atasözü zayıf. Okunan bölümde yaratıcı Türkçe hikâye yazımı puanı **yok** [D3-S013]. Yani Türkçe hikâye kalitesi bilinmiyor.
- Hız: 5600GT için ölçüm **bulunamadı**. Aynı mimarili 5600X'te 7B model: üretim ~7,5-9 belirteç/sn, istem okuma ~24-62 belirteç/sn (vekil ölçüm, C sınıfı) [D3-S015].
- ÇIKARIM: `Docs/HikayeSozdizimi.md` ~25,6 KB (tahminen 7-10 bin belirteç). Tamamı her istekte verilirse 7B'de bile yalnız okuma birkaç dakika; üretimle birlikte hikâye başına ~3-10 dk olabilir. Büyük MoE modeller için hız bilinmiyor. Kısaltılmış sözdizimi şart.
- RAM (64 GB) 16-23 GB modelleri almaya yeter görünür (ÇIKARIM); Unity ile aynı anda çalışması denenmedi. Tümleşik GPU hızlandırması araştırılmadı.
- Disk: C: 45 GB boş; tek büyük model bunun yarısını yiyebilir. Ek disk bunu çözer, **hızı çözmez** (disk yalnız yüklemeyi etkiler).
- Gizlilik en iyi; ücret 0; internet yalnız indirmede. Uygulamaya bağlamak (yerel HTTP) kod değişikliği.

### (d) Ücretli API (gösterge, 1M belirteç, girdi/çıktı)
| Model | Fiyat | Örnek tek taslak (10 bin girdi + 2 bin çıktı; hesap) |
|---|---|---|
| Gemini 3.5 Flash-Lite [D3-S004] | 0,30 / 2,50 $ | ~0,008 $ |
| Gemini 3.5 Flash [D3-S003] | 1,50 / 9,00 $ | ~0,033 $ |
| Claude Haiku 5.5 (≤100 bin istem) [D3-S016] | 0,10 / 0,50 $ | ~0,002 $ |
| Claude Sonnet 5.5 [D3-S016] | 2 / 10 $ | ~0,04 $ |
| Claude Opus 5.5 [D3-S016] | 4 / 20 $ | ~0,08 $ |
Düşünme belirteçleri, yeniden denemeler, vergi, kur hariç. Küçük tutar olsa da **bütçe "yalnız disk" ise ücretli API plana girmez**. Claude API Ticari Şartlara tabidir [D3-S009]; abonelikten ayrı faturalanır.

## 4. Özet karşılaştırma (ÇIKARIM)
| | (a) Claude abonelik | (b) Gemini ücretsiz | (c) Yerel | (d) Ücretli API |
|---|---|---|---|---|
| Ek para | yok | yok | yok (disk) | var |
| Uygulama içinde | hayır | evet (var) | evet (kod ister) | evet |
| Metin dışarı gider | evet | evet, insan okuyabilir | hayır | evet |
| Türkçe | iyi olması beklenir (ölçülmedi) | iyi olması beklenir (ölçülmedi) | belirsiz | iyi olması beklenir |
| Hız | hızlı | hızlı, kotalı | dakikalar | hızlı |
| Hazırlık | yok | büyük ölçüde hazır | kurulum + indirme | anahtar + ödeme |

**Öneri (ÇIKARIM; karar değil):** Y1 ise (a) ile hemen, kod olmadan; Y2 ise mevcut Gemini düğmeleri temel alınıp doğrulayıcı döngüsü ve eksik listeler eklenir (kullanıcı onayı + kod). (c) gizlilik ya da internetsizlik istenirse, aynı 10 hikâyelik sınamayla. Sınama ölçütü: doğrulayıcı hatası sayısı, doğru konuşan/mekân/hareket oranı, süre, kullanıcının Türkçe beğenisi.
Not: Bu seçeneklerin hiçbiri video/görüntü üretmez; yine de yapay zekâ kullanımıdır (Q-015 ile birlikte onaylatılmalı). TikTok etiketi konusu ayrıca REPORTS/00'da.

## 5. Unity koşulları: üç ayrı durum
Resmî metin (16 Kas 2026) §17.2(ff): "Offerings"e yapay zekâ ajanı, otonom yazılım, büyük dil modeli vb. yoluyla erişim, yetkili ajan erişimi dışında yasak [D3-S007]; yetkili erişim Unity'nin belirlediği arayüz (Unity CLI dahil) ister. "Offerings" yazılım, çevrim içi hizmet ve hakları kapsıyor.
| Durum | Metin ne diyor | Okuma (ÇIKARIM) |
|---|---|---|
| 1. Asistan Unity dışında yalnız **kendi metin dosyanı** yazar; Unity'de her şeyi sen yaparsın | Metinde açık cümle yok. Unity personeli (resmî olmayan görüş): yerel otomasyon yalnız kendi dosyalarınla etkileşiyorsa sorun yok [D3-S008] | en düşük riskli yol |
| 2. **Uygulamanın/aracın içinde** düğmeyle LLM çağrılır, metin gelir; model Unity'yi yönetmez | **Doğrudan ele alan cümle görmedim**; personel yanıtlarında da yok [D3-S008] | düşük risk gibi görünür ama **belirsiz**; Editor penceresindeki araç ile derlenmiş EXE ayrımı da yazılı değil. Unity'ye sorulmalı |
| 3. Ajan **Unity'yi sürer** (komut verir, kaydeder, sahne değiştirir) | yalnız yetkili arayüz [D3-S007] | dosya köprüsü yetkili görünmüyor; Unity CLI yolu (D1 §3) |
- Uygulama içi asistan "doğrula, kur, kaydet" gibi **eylem** yapmaya başlarsa durum 2'den 3'e yaklaşabilir (ÇIKARIM).
- **Yeni çelişki:** bir ikincil habere göre Unity destek hesabı Reddit'te yerel AI/MCP'nin serbest olduğunu, kısıtın Unity'nin bulut/çevrim içi hizmetleri için olduğunu söylemiş (2 Tem 2026) [D3-S025]. Bu, koşul metninin D1'deki okumasıyla ve 16 Kas güncellemesinden önceki tarihle uyuşmuyor; resmî açıklama değil. **K-004 bununla kaldırılmamalı.**
- §17.2(dd) Offerings'in model eğitiminde kullanılmasını da yasaklıyor [D3-S007]. Ücretsiz Gemini gibi "girdiyle ürün geliştiren" servise **Unity'ye ait** içerik/çıktı gönderilmemeli; kullanıcının kendi hikâye metni bundan farklıdır (ÇIKARIM).

## 6. Bütçe sonucu: yalnız bir ek disk (~150 $ ABD göstergesi)
Türkiye fiyatı bilinmiyor; 150 $ karşılığı aynı kapasiteyi alamayabilir. Boş M.2/SATA yuvası alım öncesi kontrol edilmeli (A10). Gösterge: 1 TB SSD ~144 $ (R05).

**Düşen (para yok):**
- R05 **Plan B2 ve Plan C** (RTX 3060 / 5060 Ti / 5070 / 9060 XT, güç kaynağı, fan).
- 15 yol haritası **Aşama 11**'in güç kaynağı ve kart adımları (yalnız disk kalır).
- 12 test planı §5'ten para/kart isteyenler: bulut/kiralık GPU, Cycles GPU, CC5/MetaHuman denemesi, Audio2Face, yoğun VFX Graph denemeleri (sonuncusu tümleşikte ancak sınırlı; ÇIKARIM).
- **UE5 + MetaHuman yolunun yeniden açılması** (K-006 koşulu kart ister).
- Ücretli kalemler: CC5, MHBridge, ElevenLabs aboneliği, Cascadeur, ücretli hareket paketleri, mağaza ejderhası (42-230 $), video-to-video (zaten önerilmiyor), ücretli LLM API.
- DLSS 5 / kart seçimi tartışması (C-014, Q-017) şimdilik konusuz.

**Kalan (ekran kartı istemez; hızları ölçülmedi):** Aşama 1-5, 8, 9 (teşhis, hareket-ses-altyazı, yüz, ışık/gölge temelleri, kamera, yönetmen, dayanıklı kayıt); iki görünüm karşılaştırması (gerçekçi + stilize 3D, K-015).
**Riskli kalan:** Aşama 6-7 (zengin mekân, efekt, kalabalık, ejderha) tümleşik Radeon'da ayar düşürmek zorunda bırakabilir (ÇIKARIM). Aşama 4 süre sınırını aşarsa çözüm kart değil, **ayar/içerik azaltma**.
**Diskin yararı:** Unity kopya proje, Library, kayıt ara dosyaları, animasyon indirme ve istenirse yerel model. **Yararı olmayan:** görüntü kalitesi ve kare hızı.

## 7. Kullanıcıya sorulacak (kısa)
1. Asistan nerede olsun: uygulamanın içinde mi, yoksa Claude ile dışarıda mı hazırlansın?
2. Hikâye metninin Google'a gidip insanlarca okunabilmesi kabul mü? Metinlerde gerçek kişi bilgisi var mı?
3. 18 yaş ve üstü şartı ve uygulamayı kimlerin kullanacağı (Gemini koşulu).

## Öz denetim
Hiçbir model denenmedi; Türkçe kalite ve hız satırları tahmin/vekil. Unity ve Anthropic/Google koşulları özet araçla okundu; Unity ve Gemini ücret sayfalarının tamamı okunmadı. Gemma 4 / Qwen3.6 için Türkçe ölçüm, 5600GT ölçümü, ücretsiz katman sayısal sınırı, Trendyol lisansı bulunamadı/okunmadı. Unity forum başlığının tamamı (özellikle atıf yapılan bir personel iletisi) görülmedi. Ortak dosyalara (04, 05, 07, 11, REPORTS) dokunulmadı; birleştirme ana araştırmacıda.
