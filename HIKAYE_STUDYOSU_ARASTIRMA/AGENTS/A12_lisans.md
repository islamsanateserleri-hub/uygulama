# A12 — Lisans, ticari kullanım ve risk denetçisi

> **BU HUKUKİ TAVSİYE DEĞİLDİR.** Teknik lisans taramasıdır. Sayfaların çoğu özetleyici araçla okundu; birebir metin garantisi yok. Uygulama dağıtılacak/satılacaksa asıl sözleşmeler bir hukukçuya okutulmalı. Tarih: 2026-11-27. Kod yazılmadı, indirme/sözleşme kabulü yapılmadı.

## Görev tanımı
Protokol ALT AJAN 12: A01–A11 ve K01'in önerdiği araç, model, varlık, karakter, ses ve teknolojilerin ticari kullanım, yeniden dağıtım, atıf, kısıt, bakım, güvenlik ve uyumluluk denetimi.

## Araştırma alanı ve soruları
- S1: Üç ayrı kullanım için ne değişir? **K1** kendi videon (ticari değil, bugün) · **K2** para kazanan video · **K3** uygulamayı (EXE) başkalarına dağıtmak: kullanıcıların içerik ürettiği ürün.
- S2: Unity lisansı. S3: Karakter/hareket. S4: Ortam/VFX/ses varlıkları. S5: Yapay zekâ ses/model ve platform kuralları. S6: FFmpeg ve açık kaynak bağımlılıklar. S7: Projede bugün var olan riskler. S8: Diğer ajanların hataları.

## Sınıflar
**GÜVENLİ** = okunan metne göre K1–K3'te, olağan bildirimlerle kullanılabilir · **KOŞULLU** = yerine getirilmesi gereken şart var · **RİSKLİ** = K2/K3'te ihlal olasılığı yüksek ya da yazılı izin gerek · **DOĞRULANMAMIŞ** = asıl metin okunmadı/belirsiz.

---

## En önemli sonuç (kısa)
1. **K1 (bugünkü durum: kendi videon, ticari değil):** büyük bir lisans engeli bulunmadı. Yapılacaklar: TikTok'ta yapay zekâ etiketini aç; ücretsiz ElevenLabs sesi kullanılan videoda başlığa "elevenlabs.io" yaz ve o videodan para kazanma [S040].
2. **K3 (EXE'yi başkasına vermek, ücretsiz olsa bile): mevcut kurulum paketi bu hâliyle dağıtılmamalı** (ÇIKARIM): Mixamo karakterleri büyük olasılıkla pakette; FFmpeg ve açık kaynak bildirimleri eksik; VRoid/NASA içerikleri denetlenmemiş. Aşağıda P1–P6.
3. "Ücretsiz" olmak lisans şartlarını kaldırmaz: LGPL, MIT bildirimi, Mixamo ham dosya yasağı, Reallusion/VRoid kısıtları ücretsiz dağıtımda da geçerli.

---

## Bulgular

### S2 — Unity
- Personal: son 12 ay geliri + yatırımı **200.000 $'a kadar**; üstünde Pro gerekir (Pro 2.310 $/yıl/koltuk, 12 Oca 2026'dan beri). Unity 6'da açılış ekranı isteğe bağlı [S003][S005].
- **Runtime Fee 12 Eyl 2024 itibarıyla iptal** [S004].
- **Yeni (16 Kas 2026) Editor koşulları** [S005]: 2.5 maddesi Unity Build Server kapsamına "Authorized Agentic Access" ile çalışan **AI ajanlarının yaptığı işleri** alıyor ve bunun grafik arayüzsüz çalışmasını istiyor; **Personal, Build Server kullanamaz**. Koltuk başına tek bilgisayarda en çok 3 Editor [S005][S006].
  - Bu proje Editor'ü bir AI ajanıyla dosya köprüsünden sürüyor. Açık arayüzlü, yerel Editor'de bunun serbest olup olmadığı okunan metinde **ne yasaklanmış ne tanımlanmış** → **DOĞRULANMAMIŞ**. Kullanıcı/ana araştırmacı metni okumalı, gerekirse Unity'ye sormalı. Headless/bulut ajan kullanımı Personal ile **yapılmamalı** (ÇIKARIM).
- Bulut/uzak sunucuda Editor kısıtı sürüyor (2.1, 2.8) → A10'un "Editor yerine derlenmiş EXE ile bulutta render" önerisi yönü doğru; EXE için de kiralama sağlayıcısı koşulları ayrıca bakılmalı.
- **Güvenlik:** CVE-2025-59489 düzeltmesi eski Unity sürümleri için yeniden derleme istiyor [S007]. Proje 6000.6.9f1 düzeltilmiş sürümlerden yeni (ÇIKARIM); eskiden başka sürümle dağıtılmış EXE varsa yeniden derlenmeli.

### S3 — Karakter ve hareket
**Rocketbox:** MIT, ek koşul görülmedi, depo arşivli değil (son kayıtlı güncelleme 2022-06) [S008]. GÜVENLİ — şart: **telif + MIT metni dağıtımla birlikte gitmeli**. `RocketboxKur.cs` LICENSE.md'yi Assets'e kopyalıyor, ama `.md` dosyası Unity derlemesine girmez ve uygulamada bildirim ekranı yok (ÇIKARIM, kod okuması).

**MetaHuman:** UE dışında (Unity dahil) kullanım serbest; yıllık gelir <1 M$ ücretsiz, üstü koltuk lisansı [S009][S010]. Ancak:
- Uygulama içinde "karakter seç" kütüphanesi olarak dağıtım: **hâlâ DOĞRULANAMADI** (Q-011 kapanmadı). Güncel MetaHuman koşul metni açılamadı. Okunan (eski olabilen) UE EULA: son kullanıcı yalnız ürünün ayrılmaz parçası olarak, derlenmiş biçimde kullanabilir [S011]; forumda varlıkların dışa aktarılamaması gerektiği yorumu var, Epic cevabı yok [S014][S015].
- İkinci el, doğrulanmamış (D) iddia: Epic'in hazır preset, kıyafet ve saçları **"yalnız Unreal" içeriği** sayılıyor [S013]; eski Content lisansıyla alınanlar da öyle [S010]. Doğruysa Unity planını doğrudan etkiler. **Denenmeden önce teyit şart.**
- AI eğitimi kısıtı: change log'da var [S012]; 29 Eki 2026'da kaldırıldığı iddiası yalnız ikinci elden [S013] → belirsiz.
- Sınıf: K1/K2 **KOŞULLU**, K3 **RİSKLİ**.

**Character Creator / Reallusion (C-009 netleşti, forum düzeyinde):** personel yanıtları (2025 Ağu–Ara): hazır, seçilebilir karakter **Extended** ile olabilir; oyuncu/kullanıcı karakteri veya kıyafeti özelleştirebiliyorsa **Enterprise** (özel pazarlık) [S047]. Standard'da uygulama içi karakter üretimi yasak sayılıyor [S048]. Hikâye Stüdyosu kullanıcıya kıyafet/renk çeşidi sunuyor → K3 **RİSKLİ**. Sözleşme metni okunamadı; yazılı teyit gerekir.

**VRoid / VRM:** lisans **model başına** değişir. pixiv örnek modelleri: ticari serbest, atıf yok; ama CC0 değil, **model verisiyle karakter üretme hizmeti kurmak yasak**, VRM'i ücretli dağıtmak yasak [S039]. Projedeki 3 VRoid karakterinin kaynağı (kendi yapımı mı, örnek model mi) kayıtlı değil → K3 **KOŞULLU/DOĞRULANMAMIŞ**.

**Mixamo:** kişisel/ticari projede telifsiz; Enterprise/Federated ID ve Çin hariç [S018]. Ham dosya dağıtımı ücretsiz bile yasak, motor şablonlarına konamaz (forum SSS, 2022) [S019]. K1/K2 **KOŞULLU** (oyuna/videoya gömülü); K3 içerik üretme aracında seçilebilir kütüphane = **RİSKLİ**.

**Diğer:** MPFB çıktısı CC0 (A02; yeniden okunmadı) · Daz Interactive License (A02; okunmadı) · UAL **CC0 doğrulandı** [S049] · CMU, 100STYLE (CC BY 4.0), FreeMoCap (AGPL; ayrı araç olarak kalmalı, koda gömülmemeli), Cascadeur ücretsiz sürüm ticari yasak, GVHMR/SMPL ticari yasak: A03'ten, yeniden okunmadı.

**HY-Motion (ve aynı ailede Hunyuan3D, ayrıca okunmadı):** lisans AB, İngiltere, G. Kore'yi dışlıyor ve **çıktıların bu bölgelerde kullanılmasını/dağıtılmasını da** yasaklıyor; 1 M MAU eşiği; makine üretimi beyanı [S041]. Dünyaya açık TikTok videosunda **RİSKLİ → kullanma**.

### S4 — Ortam, VFX, ses varlıkları
**Unity Asset Store (A06 iddiası):** **doğru**, iki katmanlı:
- Sözleşme 2.2.1.1(c): asıl amacı kullanıcı içeriği üretmek olan üründe Asset ile **izinsiz para kazanılamaz** [S001].
- Resmî SSS daha geniş: son kullanıcılarına kendi dijital ürününü yaptıran ürün için **yayıncıdan ayrı izin gerekir**; varlık ayrıca çıkarılıp indirilememeli; SDK türü varlık çalışma anında izinsiz konamaz [S002].
- Karar: K1/K2 (yalnız senin videoların, uygulama dağıtılmıyor) düşük risk, **KOŞULLU**; K3 **RİSKLİ** (ücretsiz olsa da SSS'ye göre izin iste). Particle Pack, Terrain Sample, DEXSOFT, ejderha paketleri, Magica Cloth, MHBridge bu sınıfta.

**Megascans / Fab:** Kasım 2026'da yeniden **0 $**; Epic hesabı gerekir; Fab Standard; 1 Oca 2025 sonrası satın alımlar bakiye olarak iade; AI eğitimine artık izin [S017]. Fab Standard: varlık projeye gömülü olarak ticari dağıtılabilir, **tek başına (bedava bile) dağıtılamaz**, Unreal ile sınırlı değil; gelir eşiği yüzünden hak farkı yok [S016]. Kullanıcı-içeriği aracı konusu metinde ele alınmıyor. K1/K2 **GÜVENLİ**; K3 **KOŞULLU** (kütüphane çıkarılamaz biçimde paketlenmeli; yazılı teyit önerilir). Megaplants UE'ye özgü. Depoya (GitHub) ham Megascans dosyası **konmamalı**.

**Poly Haven / ambientCG:** CC0 [S034][S035]. GÜVENLİ. Tek tuzak: uygulama Poly Haven **canlı API'sini** kullanırsa "Powered by Poly Haven" bildirimi ve uygulamaya özgü User-Agent şart [S034].

**Sonniss GDC:** ticari serbest, atıf yok, ham hâliyle satış yasak [S030]. Okunan resmî metinde AI yasağı **görülmedi** (A07 çelişkisi: resmî sayfada yok; "previous versions" sayfaları ayrıca var, indirilen yılın lisansı saklanmalı). Resmî sayfanın özeti "serbestçe dağıtılabilir" diyor, üçüncü taraf "ses kütüphanesi olarak dağıtılamaz" diyor [S031] → videoda GÜVENLİ; uygulama içi seçilebilir SFX kütüphanesi **KOŞULLU** (gömülü, tek tek dışa aktarılamaz; yazılı teyit).

**Wwise / FMOD (K01):** Wwise indie: üretim bütçesi <250.000 $ ücretsiz, onay ve anahtar ister [S032]. FMOD: yıllık gelir <200.000 $ ve bütçe <500.000 $ (2020 haberi; güncel sayfa açılamadı) [S033] → FMOD **DOĞRULANMAMIŞ**. İkisi de kapalı kaynak ara katman; bu uygulamada gereklilik yok (ÇIKARIM) → ekleme.

**NASA (projede, `NesneKur.cs`):** ABD'de genelde telifsiz; **NASA logosu/amblemi kamu malı değil**; NASA onayı izlenimi yasak; bazı öğeler üçüncü taraf telifli; AI üretimi görselde amblem yasak [S045]. KOŞULLU (amblemli model/dokuları kontrol et).

**CC0 kitler:** UAL doğrulandı [S049]. Quaternius (diğer), Kenney, KayKit, Vuena, Unity Labs flipbook: CC0 beyanı (A05/A06/proje kaydı; yeniden okunmadı) → GÜVENLİ kabul edilebilir, her paketin lisans dosyasını sakla.

### S5 — Yapay zekâ sesi, modeller, platform
**Gemini TTS** [S020][S021]:
- Google çıktı üzerinde sahiplik iddia etmiyor; sorumluluk kullanıcıda; benzer çıktı başkasına da üretilebilir.
- API kullanıcısı **18+**; üründe API istemcisi 18 yaş altına yönelik ya da onlarca kullanılması olası olamaz.
- Uygulama **AEA/İsviçre/İngiltere** kullanıcılarına sunulacaksa yalnız **ücretli** hizmet (kullanıcı kendi anahtarını girse de dikkat).
- Ücretsiz katmanda girdi/çıktı ürün geliştirmede kullanılabilir ve insan inceleyebilir → hikâyelere kişisel/gizli bilgi yazma.
- 3.8 TTS ve Gemini Audio sesleri **SynthID filigranlı** (duyulmaz).
- Ücretsiz katman ticari kullanımı açıkça yasaklayan bir madde okunan özette görülmedi. Sınıf: **KOŞULLU**.

**TikTok etiketi:** gerçekçi yapay/değiştirilmiş görüntü, ses ve video açıkça belirtilmeli [S022][S023]; etiketsiz içerik kısıtlanabilir/kaldırılabilir (üçüncü taraf, D) [S024]. Okunan resmî sayfalar 2023 tarihli; güncel yardım sayfaları robots engeliyle açılamadı → **2026 ayrıntısı DOĞRULANMAMIŞ**. Pratik karar: Gemini/ElevenLabs sesli her videoda **"AI-generated content" etiketini aç**.

**Yasa:** AB AI Act md.50 şeffaflık 2 Ağu 2026'dan beri uygulanıyor; makine-okur işaretleme için eski sistemlere geçiş 2 Ara 2026; salt kişisel, mesleki olmayan kullanım kapsam dışı sayılıyor [S025] (ikincil kaynak). Ürün AB'ye açılırsa değerlendirilmeli. Türkiye'de etiket zorunluluğu **taslak** (13. Yargı Paketi haberi, 17 Kas 2026), yasalaşmadı [S026].

**ElevenLabs:** ücretsiz plan ticari değil + başlığa "elevenlabs.io"; Beta çıktıları ücretlide de ticari değil [S040]. `DURUM.md` "Üçüncü Kişi" için ElevenLabs sesi üretildiğini (10.000 kredilik hesap) kaydediyor → o ses kullanılırsa kural geçerli. (`DURUM.md`'deki "ElevenLabs yazmak gerekmiyor" notu başka, Gemini sesli videoya aittir.)

**Piper:** motorun güncel deposu (piper1-gpl) **v1.3.0'dan beri GPLv3** [S043] → `Docs/Asama16_Degerlendirme.md`'deki "Piper motoru MIT" **güncel değil**. EXE'ye gömülürse GPL yükümlülüğü doğar. dfki sesi: depo etiketi "mit" [S044] ↔ A07: eğitim verisi CC BY-NC-SA → kısıtlayıcıyı esas al.

**C-008 (yerel Türkçe ses):** Apache/MIT kod/ağırlık etiketi, eğitim verisi ve ses sahibinin rızasını kanıtlamaz. Chatterbox/VoxCPM2/EMA/FreyaTTS bu oturumda okunmadı → **DOĞRULANMAMIŞ**. Gerçek kişinin sesini/yüzünü izinsiz kopyalama = **kullanma** (Google da rıza kaydı istiyor [S021]; TikTok kısıtlıyor [S022]). C-008 açık kalır.

**OVR LipSync:** Meta belgesi arşivlenmiş, "güncellenmiyor, kaldırılabilir", EOL Audio SDK bölümünde [S042]; lisans metni okunmadı → **RİSKLİ (terk edilme), yeni bağımlılık yapma**. Q-014 için: A07'nin kendi viseme sürücüsü yolu lisansça en temizi.

**Patent (A01 sorusu):** US6040841 "virtual cinematography" (Microsoft) **süresi dolmuş** görünüyor (2016) [S046]. Başka patent taraması yapılmadı; A01/A08 yöntemlerini kendi kodunla yazmak düşük risk (ÇIKARIM).

### S6 — FFmpeg ve açık kaynak bağımlılıklar
**Hangi derleme?** `Assets/HikayeStudyo/Editor/FfmpegIndir.cs`: BtbN `releases/download/latest/ffmpeg-master-latest-win64-lgpl.zip`; depodaki `LICENSE.txt` **LGPL v3** başlıklı; `Installer/HikayeStudyosu.iss` derleme klasörünü (StreamingAssets/ffmpeg dahil) tümüyle paketliyor. LGPL varyantında x264/x265 yok [S028]. **GPL derlemesi değil: iyi.** Statik `ffmpeg.exe` ayrı süreç olarak çağrılıyor; uygulama kodunun LGPL olması gerekmez (ÇIKARIM), ama **ikiliyi dağıtmanın yükümlülükleri var**:
| Gereken (FFmpeg listesi [S027]) | Projede |
|---|---|
| Lisans metni | LICENSE.txt (165 satır) yalnız LGPLv3 ekini içeriyor; temel GPLv3 metni dosyada **yok** (LGPLv3 onun da verilmesini ister; genel bilgi, hukukçu teyidi) |
| Tam eşleşen kaynak kodun sunulması/barındırılması | **yok** |
| "Hakkında" ekranında FFmpeg/LGPL bildirimi, indirme sayfasında kaynak bağlantısı | **yok** (Ayarlar yalnız "Video aracı (ffmpeg): Hazır" ve sürüm gösteriyor) |
| Derlemeye giren diğer kütüphanelerin bildirimleri | **yok** |
| Sürümün sabitlenmesi | **yok**: "latest" kayan; günlük derlemeler 14 gün, ay sonları 2 yıl saklanıyor [S028] → hangi kaynağa karşılık geldiği kaybolabilir |
| Bütünlük denetimi (hash) | **yok** (tedarik zinciri riski; kod okuması) |

**Patent:** LGPL patent lisansı vermez; FFmpeg H.264 için patent uyarısı yapıyor [S027]. 2026 Via LA zamları büyük yayın platformlarını hedefliyor [S029]; küçük ücretsiz araç için ne gerektiği **DOĞRULANAMADI**. ÇIKARIM: donanım/işletim sistemi kodlayıcısını (`h264_amf`, `h264_mf`) öncelemek (kod zaten öyle) riski azaltır, sıfırlamaz; `libopenh264` yedeği için Cisco'nun patent güvencesi kendi dağıttığı ikililere özgüdür (genel bilgi, doğrulanmadı). AAC için ayrıca bakılmadı.

**UniVRM:** MIT [S036], bakımlı, Unity 2022.3+ [S037]; proje v0.131.4 sabitliyor (sayfa özeti son sürümü v0.131.2 gösterdi: tutarsız, elle bak). `UniVrmKur.cs` yol denetimi yapıyor (`..`, `:` reddi) — iyi; indirilen paketin hash'i denetlenmiyor. GÜVENLİ (bildirimle).
**Draco:** Apache-2.0; dağıtımda lisans kopyası zorunlu [S038]. glTFast (Apache-2.0, okunmadı), Cinemachine/Recorder/Timeline (Unity paket lisansı, okunmadı). GÜVENLİ/KOŞULLU.
**Inno Setup** (kurulum aracı): güncel ticari kullanım koşulu bu oturumda bakılmadı → DOĞRULANMAMIŞ.
**Yazı tipi:** altyazı işletim sistemi fontuyla çiziliyor (`CreateDynamicFontFromOSFont`); Windows font koşulu bakılmadı; EXE'ye font dosyası gömülmüyor görünüyor → düşük risk (ÇIKARIM). Ürünleşmede OFL font tercih edilebilir.
**Gizlilik:** Gemini anahtarı DPAPI ile şifreli saklanıyor (kod yorumu/okuması) — iyi; izlenen dosyalarda `AIza…` kalıbında anahtar bulunmadı (0 eşleşme).

### S7 — Projede bugün görülen somut sorunlar (K3 öncesi)
- **P1 Mixamo pakette olabilir:** `Resources/KarakterKatalogu.asset` **35 Mixamo karakterine** başvuruyor. Unity `Resources` altındaki başvuruları derlemeye alır → dosyalar kullanıcının diskindeyse EXE'ye giriyor (ÇIKARIM; derleme içeriği incelenmedi). `.gitignore` yalnız depoyu korur, **kurulum paketini korumaz** (A03'ün "proje bunu zaten koruyor" ifadesi eksik).
- **P2 Açık kaynak bildirimi yok:** uygulamada "Lisanslar/Üçüncü taraf" ekranı ve kurulumda NOTICE dosyası yok (Rocketbox MIT, UniVRM MIT, Draco/glTFast Apache, FFmpeg LGPL).
- **P3 FFmpeg kaynak/sürüm yükümlülüğü** (yukarıdaki tablo).
- **P4 VRoid karakterlerinin köken kaydı yok.**
- **P5 NASA amblemli içerik kontrol edilmemiş.**
- **P6 Varlık manifesti yok** (yalnız `Karakterler/LISANSLAR.md`; Rocketbox, Mixamo, VRoid, NASA, KayKit, ses dosyaları kapsam dışı).

---

## S8 — Diğer ajanların lisans hataları / eksikleri
| Ajan | İfade | Denetim sonucu |
|---|---|---|
| A02 | "Reallusion — koşullu"; Extended şartı kaldırılmış olabilir (Ç2) | **Eksik/yanıltıcı:** 2025 sonu personel yanıtlarına göre karakter özelleştirme olan üründe Enterprise gerekir [S047] |
| A02 | MetaHuman: AI eğitimi yasak; Unity yolu "lisans: evet" | Unity kullanımı doğru; ancak preset/kıyafet/saç "yalnız UE" olabilir (D, A02'de yok) ve AI yasağının güncelliği belirsiz [S012][S013] |
| A02 | VRoid: yalnız "karakter üretici için ayrı lisans" | Örnek modellerde ayrıca ücretli dağıtım ve CC0 işaretleme yasağı; model başına lisans [S039] |
| A03 | Mixamo için "proje bunu zaten .gitignore ile koruyor" | **Eksik:** kurulum paketi korunmuyor (P1) |
| A03 | HY-Motion: bölge + MAU + AI beyanı | Eksik: **çıktı** da bölge dışında kullanılamıyor [S041] (A11 doğru söylemiş) |
| A05 | Asset Store 2.2.1.1(c) bu ürünleri "kısıtlıyor" | Yön doğru; hassaslaştırma: maddenin metni **para kazanmayı** kısıtlıyor, geniş yasak SSS'de [S001][S002] |
| A05 | Fab Personal/Professional eşiği 100 bin $ | Hak farkı yok; alımdan sonra yükseltme gerekmiyor [S016]. Ücretsiz Megascans için pratik önemi yok. (Eşik rakamı yeniden doğrulanmadı) |
| A06 | Asset Store: kullanıcı-üretimi uygulama için yayıncı izni | **Doğrulandı** [S002] |
| A07 | Sonniss AI yasağı çelişkisi | Resmî GDC lisans sayfasında görülmedi [S030] |
| A07 | Piper (motor lisansından söz yok); Asama16 "MIT" | Güncel motor GPLv3 [S043] |
| A01 | US6040841 patent durumu | Süresi dolmuş [S046] |
| A09 | ffmpeg LGPL | Doğru; ama dağıtım yükümlülükleri hiç yazılmamış |
| A10 | Editor bulut kısıtı | Doğru; 16 Kas 2026 güncellemesiyle AI ajan / Build Server maddesi de var [S005] |
| K01 | OVR LipSync; Wwise/FMOD; Megascans; 4 MetaHuman'lı ürün | OVR arşivde; Wwise/FMOD gereksiz ve koşullu; MetaHuman kütüphanesi doğrulanmadı |

---

## Teknoloji kayıtları — ANA SINIFLANDIRMA TABLOSU
Doğrulama 2026-11-27; hiçbiri hukukçu onaylı değil. K1 kendi videon (ticari değil) · K2 para kazanan video · K3 uygulama dağıtımı. G=GÜVENLİ, K=KOŞULLU, R=RİSKLİ, D=DOĞRULANMAMIŞ.

| Kalem | K1 | K2 | K3 | Şart / gerekçe | Kaynak |
|---|---|---|---|---|---|
| Unity Personal + URP | G | K | K | ≤200k $; Runtime Fee yok | S003–S005 |
| Editor'ü AI ajanıyla sürme (dosya köprüsü) | D | D | D | yeni 2.5 maddesi; tanım yok | S005 |
| Cinemachine, Recorder, Timeline, Shader/VFX Graph | G | G | D | Unity paket lisansı okunmadı | – |
| Unity Asset Store varlıkları | G | K | R | UGC aracında yayıncı izni | S001, S002 |
| MHBridge, Magica Cloth, mağaza shader/eklenti | K | K | R | aynı; tek geliştirici/bakım riski | S002 |
| Microsoft Rocketbox (karakter+animasyon) | G | G | G | MIT bildirimi zorunlu | S008 |
| MetaHuman | K | K (<1M $) | R | kütüphane dağıtımı doğrulanmadı; UE-only parçalar? | S009–S015 |
| Character Creator / Reallusion içerik | K | K | R | özelleştirme = Enterprise | S047, S048 |
| Daz Genesis | K | K | R | Interactive License (A02) | A02 |
| MPFB çıktısı (CC-BY paketlerde atıf) | G | G | G/K | eklenti GPL: gömme | A02 |
| VRoid: kendi yaptığın model | G | G | K | parça/preset koşulları | S039 |
| VRoid örnek modeller / indirilen VRM | K | K | R | model başına lisans | S039 |
| Mixamo karakter/animasyon | G | K | R | ham/kütüphane dağıtımı yasak | S018, S019 |
| UAL (CC0) | G | G | G | – | S049 |
| Quaternius/Kenney/KayKit/Vuena (CC0 beyanı) | G | G | G | lisans dosyasını sakla | proje, A05 |
| 100STYLE (CC BY) / CMU | K | K | K | atıf; satılmaz (A03) | A03 |
| FreeMoCap (AGPL) | G | G | R(gömülürse) | ayrı araç | A03 |
| Cascadeur ücretsiz / DeepMotion ücretsiz | K | R | R | ticari yasak (A03) | A03 |
| Rokoko, QuickMagic çıktıları | D | D | D | çıktı hakkı yazmıyor | A03 |
| GVHMR / SMPL tabanlı | K | R | R | ticari yasak (A03) | A03 |
| HY-Motion / Hunyuan3D | R | R | R | bölge+çıktı kısıtı | S041 |
| Kimodo, TRELLIS.2, UniRig, LAM, LTX, Wan, Marble | D | D | D | ağırlık/bağımlılık lisansı okunmadı; Marble ticari hak ücretli (A11) | A03, A11 |
| Megascans (Fab Standard) | G | G | K | tek başına dağıtım yok | S016, S017 |
| Fab diğer / Sketchfab | K | K | K/R | model başına; NC/ND/Editorial alma | S016, A05 |
| Synty | K | K | R | "game creation software" yasağı (A05) | A05 |
| Poly Haven, ambientCG | G | G | G | canlı API ise bildirim | S034, S035 |
| Unity Labs flipbook, CC0 VFX | G | G | G | beyan (A06) | A06 |
| OpenVAT | D | D | D | lisans adı | A06 |
| CristianQiu Volumetric (MIT) | G | G | G | MIT bildirimi (A04) | A04 |
| IllusionRP (Unity Companion) | G | G | K | yalnız Unity ile (A04; okunmadı) | A04 |
| Blender, Cell Fracture (GPL araç) | G | G | G | GPL kodu gömme; çıktı genelde serbest (doğrulanmadı) | A04, A06 |
| NASA model/görselleri | G | K | K | amblem/onay izlenimi yok | S045 |
| Sonniss GDC | G | G | K | ham kütüphane gibi sunma | S030, S031 |
| Freesound/Pixabay/Mixkit | G | K | K/R | yalnız CC0; Content ID; tek başına dağıtım yok (A07) | A07 |
| TikTok Commercial Music Library | K | K | R | TikTok içinde ekle (A07) | A07 |
| Gemini TTS | K | K | K | 18+, AEA ücretli, etiket | S020, S021 |
| Gemini görsel/Lyria | D | D | D | fiyat Asama16; koşul S020 genel | Asama16 |
| ElevenLabs ücretsiz | K | R | R | atıf; ticari yok | S040 |
| ElevenLabs ücretli | G | K | D | Beta hariç; uygulama entegrasyonu koşulu okunmadı | S040 |
| Azure/OpenAI/Polly | D | D | D | okunmadı (A07) | – |
| Piper (piper1-gpl) | G | D | R | GPLv3 motor; ses başına lisans | S043, S044 |
| Piper dfki, XTTS-v2, MMS(-FA), NC modeller | K | R | R | NC | A07 |
| Chatterbox, VoxCPM2, EMA, FreyaTTS | D | D | D | veri/ses rızası | A07 |
| MFA Türkçe, parseh, mpoyraz (CC BY 4.0) | G | G | K | atıf (A07; okunmadı) | A07 |
| whisper.cpp / whisper.unity / uLipSync (MIT), WhisperX (BSD) | G | G | G | bildirim (A07) | A07 |
| faster-whisper, Rhubarb, MediaPipe model, OpenTimelineIO | D | D | D | okunmadı | – |
| Audio2Face-3D, NeuroSync | D | D | D | NVIDIA OML / gelir eşiği (A07) | A07 |
| OVR LipSync | R | R | R | arşivlenmiş | S042 |
| Wwise | K | K | K | bütçe <250k $ + anahtar | S032 |
| FMOD | D | D | D | eski kaynak | S033 |
| FFmpeg BtbN LGPL | G | G | K | kaynak + bildirim + sabit sürüm | S027, S028 |
| H.264/AAC patentleri | G | D | D | belirsiz | S027, S029 |
| UniVRM, Draco, glTFast | G | G | G | lisans bildirimi | S036–S038 |
| Unreal 5 (K01) | D | D | D | UE EULA ayrı incelenmeli; Non-Engine çıktılar telifsiz görünüyor | S011 |

---

## Karşılaştırma ve çelişkiler
- Asset Store: sözleşme (para kazanma) ↔ SSS (geniş yasak). Temkinli olanı uygula.
- Sonniss: resmî özet "serbest dağıtım" ↔ üçüncü taraf "kütüphane olarak dağıtılamaz".
- Piper dfki: depo "mit" ↔ veri CC BY-NC-SA.
- MetaHuman AI yasağı: change log var ↔ ikinci el "kaldırıldı".
- UniVRM sürümü: sayfa özeti 0.131.2 ↔ kod 0.131.4.

## Riskler
| ID | Risk | Olasılık / Etki | Önlem |
|---|---|---|---|
| A12-R1 | Mixamo içeren EXE'nin dağıtılması | yüksek / yüksek | dağıtım profilinde Mixamo'yu katalogdan çıkar; yerine Rocketbox/CC0 |
| A12-R2 | FFmpeg LGPL ihlali | yüksek (dağıtımda) / orta | sürüm sabitle, kaynağı arşivle ve sun, Hakkında bildirimi; ya da ffmpeg'i kullanıcıya ayrı kurdur |
| A12-R3 | MIT/Apache bildirimleri eksik | kesin (dağıtımda) / orta | THIRD_PARTY_NOTICES + uygulama ekranı |
| A12-R4 | Asset Store/Reallusion/MetaHuman varlığının kütüphaneye girmesi | orta / yüksek | dağıtımda yalnız MIT/CC0 (+Fab teyitli) |
| A12-R5 | TikTok'ta etiketsiz yapay ses | orta / orta | yayın paketine "AI etiketi aç" hatırlatması |
| A12-R6 | Editor koşullarında AI ajan maddesi | belirsiz / orta-yüksek | metni oku, gerekirse Unity'ye sor; headless ajan kullanma |
| A12-R7 | Doğrulamasız indirme (ffmpeg "latest", UniVRM) | düşük / yüksek | sürüm + SHA-256 sabitle |
| A12-R8 | Terk edilen/arşivlenen bağımlılık (OVR, MoveBox arşivli (A03), Rocketbox güncellenmiyor) | orta / orta | yeni bağımlılık yapma; yerel kopya + lisans arşivi |
| A12-R9 | Gemini koşulları: 18 yaş altı, AEA, veri kullanımı | K3'te orta / orta | uygulamada koşul bildirimi; çocuklara yönelik pazarlama yok |
| A12-R10 | Koşulların değişmesi (Fab, Mixamo, Unity, TikTok) | yüksek / orta | her varlıkla lisans anlık kopyası + tarih; 6 ayda bir gözden geçir |
| A12-R11 | Gerçek kişi yüz/ses benzetimi, telifli dizi karakteri/müziği | – / yüksek | kural: kullanma (projede zaten "telifli karakter eklenmez") |

## Test önerileri (bu bilgisayarda, ücretsiz)
- T1: Build raporundan (Editor.log varlık listesi) EXE'ye giren Mixamo/VRM/NASA/Rocketbox dosyalarını listele.
- T2: `ffmpeg.exe -version` ve `-L` çıktısını kaydet (kesin commit, `--enable-gpl/nonfree` olmadığını doğrula, lisans v3 mü).
- T3: NASA modellerinde amblem gözle kontrol.
- T4: VRoid karakterlerinin VRM meta lisans alanlarını oku ve manifeste yaz.
- GPU isteyen test yok.

## Öz denetim
- Okunamayan: güncel MetaHuman/UE EULA metni, TikTok güncel yardım sayfaları, FMOD ve Wwise fiyat sayfaları, Reallusion sözleşme metni, Fab'ın tam metni (yalnız özet), Adobe ek koşulları.
- Yalnız ajan raporundan devralınan (yeniden okunmayan) satırlar tabloda "A0x" ile işaretli; bunları kesin sayma.
- WebFetch özetleyici: alıntılar birebir olmayabilir.
- Türk hukuku (FSEK, KVKK) incelenmedi.
- Patent taraması yalnız tek patent.
- 40/40 arama kullanıldı.

## Eksik alanlar / açık sorular
1. Uygulama gerçekten dağıtılacak mı? (K3 kararı; Q-002 ile.)
2. Unity "Authorized Agentic Access" tanımı.
3. MetaHuman kütüphane dağıtımı (Q-011 açık) ve UE-only parçalar.
4. H.264/AAC cihaz/kodlayıcı patent yükümlülüğü.
5. Projedeki VRoid karakterlerinin ve varsa dış müzik/ses dosyalarının kökeni.
6. Inno Setup ve Windows font koşulları.

## Diğer ajanlardan beklenen / ortak konular
- A09: ffmpeg paketleme/sürüm sabitleme, bildirim ekranı, dağıtım profili. A02/A03: dağıtım profilinde Mixamo'suz katalog. A05/A06: varlık manifestinde "K3 uygun" alanı. A07: TTS veri-rıza denetimi. A08: yayın paketine AI etiketi hatırlatması. A10: bulut render koşulları.

## Öneriler (kanıtla)
1. **Şimdi (ücretsiz):** her videoda TikTok AI etiketini aç [S022][S023]; ElevenLabs ücretsiz sesli videoda atıf [S040]; Gemini'ye kişisel veri yazma [S020].
2. **Varlık manifesti** (kaynak, URL, lisans, sürüm, tarih, atıf, "dağıtılabilir mi") — lisansı boş varlık kütüphaneye girmesin (A03/A05/A06 ile aynı yönde).
3. **Dağıtımdan önce engelleyici liste:** P1 Mixamo'yu çıkar [S019] · P2 bildirim dosyası/ekranı [S008][S036][S038] · P3 FFmpeg kaynak+sabit sürüm [S027][S028] · P4/P5 köken ve amblem denetimi [S039][S045].
4. **Dağıtılabilir çekirdek küme:** Rocketbox (MIT) + CC0 (UAL, Poly Haven, ambientCG, Quaternius/Kenney/KayKit) + kendi kodla üretilen mekân/efekt/ses; Megascans ancak teyitle.
5. **Uzak dur:** HY-Motion/Hunyuan, OVR LipSync, NC lisanslı sesler, izinsiz Asset Store/Reallusion/Synty, doğrulanmadan MetaHuman kütüphanesi.
6. `Docs/Asama16_Degerlendirme.md` "Piper motoru MIT" notu ana araştırmacı tarafından düzeltme olarak kaydedilsin.

Kaynaklar: `SOURCES/A12_kaynaklar.md` (metindeki [S0xx] = A12-S0xx).
