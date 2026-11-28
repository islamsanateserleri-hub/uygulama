# D4 — Sıfır maliyetle gerçekçi (MetaHuman kalitesinde) karakter: ücretsiz yollar

Tarih: 2026-11-28 · Yöntem: yalnızca web araştırması (WebSearch/WebFetch). **Hiçbir şey test edilmedi.** İndirme yapılmadı, hesap açılmadı, cihaza dokunulmadı.

Bağlam: Kullanıcı Türkiye'de, bütçesi yok. Kiralık uzak Windows 10 PC: Ryzen 5 5600GT, yalnız tümleşik Radeon Vega, 64 GB RAM, ~40 GB boş disk, uzak masaüstüyle erişim. UE 5.8.4 kurulu. Açılışta iki kez D3D12 "GPU hung / device removed" çöktü (1. deneme kare 0'da, Lumen açıkken; 2. deneme Lumen kapalıyken PC dondu). İstek: AI video üretici kullanmadan, 0 TL ile, MetaHuman kalitesinde gerçekçi karakterli dikey hikaye videoları.

---

## 0. Kısa sonuç

| Sıra | Yol | Gerçekçilik | Bu PC'de olur mu | Kullanıcıdan ne gerekir |
|---|---|---|---|---|
| 1 | Hazır MetaHuman'ı **başka bir yerde** (GPU'lu bir PC'de bir kerelik) oluşturup DCC/DNA olarak dışa aktarmak, sonra Blender (Character DNA, ücretsiz) → FBX → Unity | Yüksek (yüz rigi kısmen) | Unity tarafı evet (düşük kalite ayarıyla) | GPU'lu bir bilgisayara **bir kere** erişim (arkadaş, internet kafe, okul labı) + Epic hesabı |
| 2 | UE 5.8'i bu PC'de **-dx11 / düşük ayar** ile açmayı denemek, sonra Python ile MetaHuman oluştur/dışa aktar | Yüksek | **Belirsiz, büyük olasılıkla hayır** (MetaHuman Creator resmi olarak RTX 3070 / RX 6800 XT + 8 GB VRAM + DX12 istiyor) | Kiralık PC'de deneme izni; TDR kayıt defteri değişikliği riskini kabul |
| 3 | **MPFB 2** (Blender, açık kaynak, çıktısı CC0) ile karakter üretip Unity'ye almak | Orta ("MetaHuman" değil, ama gerçekçi insan) | **Evet**, iGPU'da çalışır (Blender + Unity hafif) | Blender kurulumu (~1 GB) |
| 4 | Unity'nin ücretsiz **Digital Human** paketi (Enemies/Heretic) | Çok yüksek (ama tek hazır kafa, HDRP ağır) | iGPU'da büyük olasılıkla çok yavaş / pratik değil | — |
| 5 | Ücretsiz bulut GPU (Azure/GCP/AWS/Oracle deneme kredileri) | — | **Pratikte hayır**: deneme hesaplarında GPU kapalı ve hepsi kredi kartı istiyor | Kredi kartı (yok) |

**İmkânsız olanlar (resmi kaynaklara göre):** Google Cloud $300 denemesinde GPU ve Windows Server yasak; Azure ücretsiz hesapta GPU VM fiilen verilmiyor; Colab/Kaggle/Lightning/Paperspace ücretsiz planlarında Windows masaüstü yok, Colab'da uzak masaüstü açıkça yasak; Shadow'un ücretsiz denemesi yok; Character Creator denemesi filigranlı ve ticari kullanım yok; MetaHuman Creator'ın çalışması için internet/bulut (Epic hesabı) şart.

---

## 1. Ücretsiz bulut GPU (Windows masaüstü veya Unreal Editor çalıştırabilecek)

### 1.1 Google Cloud — $300 / 90 gün deneme
- Resmi sayfa, Free Trial sınırları: **"Add GPUs to your VM instances"** ve **"Create VM instances that are based on Windows Server images"** deneme hesabında *yapılamaz*. Kredi kartı veya geçerli ödeme yöntemi **zorunlu**. Kripto madenciliği yasak.
- Sonuç: **GPU yok, Windows yok → Unreal için işe yaramaz.** Topluluk şikâyetleri de aynı: "$300 Free Trial is useless without GPU Quota".
- Kaynak: https://docs.cloud.google.com/free (2026-11 itibarıyla erişildi) · https://discuss.google.dev/t/300-free-trial-is-useless-without-gpu-quota-my-trial-period-is-wasting-away/290091

### 1.2 Microsoft Azure — $200 / 30 gün ücretsiz hesap
- Resmi SSS: Kayıt için **kredi veya banka kartı (ön ödemeli olmayan)** + telefon gerekir. Ücretsiz VM saatleri yalnız B1s / B2pts v2 / B2ats v2 (GPU'suz, küçük) boyutlar. $200 kredi Spot VM'lerde ve Marketplace tekliflerinde kullanılamaz. 30 gün sonunda ödemeli plana geçilmezse hesap kapanır.
- GPU: SSS açıkça "GPU yasak" demiyor, ama Microsoft Q&A moderatör yanıtı (2024-03-14): ücretsiz hesapta "yalnızca sınırlı sayıda çekirdek" kullanılabildiğini ve ücretsiz listede GPU olmadığını söylüyor. Ücretsiz/öğrenci hesaplarında GPU (NC/NV) kotası 0 geliyor ve artış talepleri genelde reddediliyor (çok sayıda Q&A başlığı).
- Azure for Students ($100, kartsız): öğrenci doğrulaması ister; GPU kotası yine verilmiyor (Q&A başlıkları).
- Sonuç: **Kart yoksa hesap bile açılamaz; açılsa da GPU kotası pratikte yok.**
- Kaynaklar: https://azure.microsoft.com/free/free-account-faq · https://learn.microsoft.com/en-us/answers/questions/1616602/free-credits-do-not-allow-launching-gpu-accelerate · https://learn.microsoft.com/en-us/answers/questions/1352279/using-gpu-compute-resources-in-a-student-account

### 1.3 AWS — Free plan (Temmuz 2025 sonrası yeni hesaplar)
- 15 Temmuz 2025'ten beri: $100 kayıt kredisi + $100'e kadar ek kredi, 6 ay. Free plan "tüm krediyi hemen tüketecek" hizmetlere erişemez. EC2 belgesinde ücretsiz uygun tipler: t3/t4g/t8i/t9g micro-small, c7i-flex.large, m7i-flex.large — **hiçbiri GPU değil**. GPU (g4dn vb.) için ödemeli plana geçmek gerekir (o zaman kredi biter, fatura başlar).
- SSS: çoğu yeni müşteri için kart şart değil, ama AWS kimlik doğrulama için isteyebilir.
- Sonuç: **Ücretsiz planda GPU yok.**
- Kaynaklar: https://aws.amazon.com/about-aws/whats-new/2025/07/aws-free-tier-credits-month-free-plan · https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/ec2-free-tier-usage.html · https://aws.amazon.com/free/faqs/

### 1.4 Oracle Cloud — $300 / 30 gün deneme
- Resmi SSS: kart **zorunlu**; 30 gün $300; "kapasite sınırlarına tabi". GPU'dan hiç söz edilmiyor; Always Free yalnız AMD ve Ampere A1 (ARM) CPU. Forumlarda deneme hesabında şekil (shape) bulunamama şikâyetleri var.
- Sonuç: **Kart yok → yok. GPU garantisi yok.**
- Kaynak: https://www.oracle.com/cloud/free/faq/ · https://forums.oracle.com/ords/apexds/post/no-shapes-available-at-my-free-tier-account-5188

### 1.5 Shadow (bulut Windows PC)
- Ücretsiz deneme **yok** (cloudbase.gg, 2026-04-15 güncel): "does not offer a free tier, a free trial". Neo ~€32,99/ay. Türkiye desteği doğrulanamadı.
- Kaynak: https://cloudbase.gg/shadow-pc-free/

### 1.6 Notebook tabanlı ücretsiz GPU'lar (Colab, Kaggle, Lightning AI, Paperspace)
- **Colab ücretsiz:** Resmi SSS'de ücretsiz çalışma zamanlarında yasaklananlar: **"remote control such as SSH shells, remote desktops"**. Yani Unreal Editor'ı uzak masaüstüyle açmak ToS ihlali.
- **Kaggle:** T4/P100, oturum başına 12 saat, haftalık ~30 saat (gayriresmî); yalnız notebook, Linux, masaüstü yok.
- **Lightning AI:** Ücretsiz planda 1 ücretsiz CPU Studio; kredi miktarı resmi belgede yazmıyor, "her an değiştirilebilir". Linux Studio, Windows yok.
- **Paperspace Gradient Free:** M4000, en fazla 6 saat, 5 GB depolama — Unreal kurulumu (onlarca GB) sığmaz.
- Hiçbiri Windows masaüstü sunmuyor (gpuperhour.com, 2026-09-21). SageMaker Studio Lab yeni kullanıcıya kapalı.
- Teorik not: Linux UE + GPU ile ekransız render mümkün olsa da disk/oturum sınırları ve ToS nedeniyle **gerçekçi değil**.
- Kaynaklar: https://research.google.com/colaboratory/faq.html · https://lightning.ai/docs/platform/overview/faq/billing.md · https://gpuperhour.com/blog/free-cloud-gpus-and-credits

### 1.7 Programlar (girişim/öğrenci)
- **Microsoft for Startups** (2 Temmuz 2025 değişikliği): yatırımcısız "açık" yol $1.000 (90 gün) + iş doğrulamasıyla $4.000 (180 gün). Azure'da yeni olmak ve **iş doğrulaması** gerekir; bireysel kullanıcının uygun olup olmadığı açık değil; GPU kotası yine ayrı onay ister. Kart gereksinimi bu sayfada yazmıyor (Azure hesabı genelde kart ister).
  Kaynak: https://learn.microsoft.com/en-us/startups/changes-microsoft-for-startups
- **GitHub Student Pack / Azure for Students:** Öğrenci değilse uygulanamaz; öğrenciyse bile Azure'da GPU kotası verilmiyor (yukarı bkz.).
- **Epic MegaGrants, NVIDIA Inception:** Bu araştırmada doğrulanmadı; MegaGrants başvuru/proje değerlendirmesi ister, hızlı veya garantili değil; Inception doğrudan ücretsiz GPU vermez (doğrulanmadı — bilgi notu).

**Bölüm 1 sonucu:** Kredi kartı olmadan ücretsiz, Windows'lu, GPU'lu bulut makine **bulunamadı**. Kart olsa bile Google açıkça yasaklıyor, Azure/AWS fiilen vermiyor.

---

## 2. UE5'i Vega iGPU'da açmak

### 2.1 Resmi gereksinimler
- UE belgesi (5.9 sayfası): önerilen DX12 kart, **8 GB+ VRAM**, 32 GB RAM. Lumen ve MegaLights için **AMD RX 6000+ / NVIDIA RTX 2000+ / Intel Arc**. Lumen, MegaLights, Nanite **SM6** ister. Bazı özellikler en az DX11 ile çalışır.
  https://dev.epicgames.com/documentation/unreal-engine/hardware-and-software-specifications-for-unreal-engine
- **MetaHuman Creator / Animator** (5.8 sayfası): "At least nVIDIA RTX 3070, AMD RX 6800 XT … with 8GB VRAM", 16 fiziksel çekirdek + 32 GB RAM önerisi, Windows'ta **DX12 ve varsayılan RHI = DX12** (Animator için), saç teli (strands) için SM6. Tümleşik GPU veya DX11 desteğinden söz edilmiyor.
  https://dev.epicgames.com/documentation/metahuman/metahuman-hardware-requirements-in-unreal-engine
- Yani Vega iGPU, MetaHuman Creator için resmi gereksinimin **çok altında**.

### 2.2 Topluluk raporları
- "Ryzen 5 5600G iGPU ile UE5 çalışır mı?" (2023): yanıtlar "muhtemelen yetersiz, ayrık GPU önerilir, bellek çökmeleri olur"; somut FPS yok. https://forums.unrealengine.com/t/amd-ryzen-5-5600g-no-gpu/759533
- AMD + UE 5.6.1 DX12 çökmesi: `-dx11` ile açılıyor, sürücü geri alınınca DX12 düzeldi (RX 6800 XT, 2025-10). Bu, **sürücü sürümünün** de çökme nedeni olabileceğini gösteriyor. https://forums.unrealengine.com/t/ue-5-6-1-crash-at-87-when-running-dx12-works-only-with-dx11/2661920
- UE 5.8'de AMD'de DX12 donma regresyonu raporu (RX 9070 XT). https://forums.unrealengine.com/t/ue-5-8-dx12-editor-hard-freezes-at-enddrawingviewport-slate-on-rx-9070-xt-regression-from-5-6/2735270
- **Uzak masaüstü:** Windows RDP oturumunda GPU yerine "Microsoft Basic Render Driver" kullanılabilir; Grup İlkesi "Use the hardware default graphics adapter for all Remote Desktop Services sessions" açılmalı (Win10). Fare/kamera döndürme sorunları da bilinen konu. https://forums.unrealengine.com/t/work-from-home-how-to-use-unreal-through-remote-desktop/141157

### 2.3 Denenebilecek bayraklar / ayarlar (test edilmedi)
- Kısayola `-dx11` (veya `-d3d11`): DX12 yerine DX11 → SM5. Lumen/Nanite/VSM DX11'de devre dışı kalır; UE 5.5+'ta DX11'de Lumen çalışmadığı raporlanmış. https://forums.unrealengine.com/t/lumen-no-longer-working-in-ue5-5-dx11-sm5/2126402
- Proje ayarlarını editörü açmadan düşürmek: proje `Config/DefaultEngine.ini` içinde Lumen/Nanite/VSM kapatma, `Saved/Config/.../EditorPerProjectUserSettings` ve Scalability "Low". (Doğrulanmadı; değerlerin adları sürüme göre değişebilir.)
- Yeni boş proje "Blank, Scalable/Maximum Quality = Scalable, Raytracing kapalı" ile açmak, ilk açılışta shader derlemesi uzun sürer.
- `-nullrhi`: GPU'suz çalıştırır, ama **görüntü yok**; yalnız komutlet/otomasyon işleri için (ör. `UnrealEditor-Cmd.exe Proje.uproject -run=pythonscript -script=... -nullrhi`). MetaHuman montajı/doku pişirmenin `-nullrhi` altında çalışıp çalışmadığı **belgelenmemiş**.
- **TdrDelay** (HKLM\System\CurrentControlSet\Control\GraphicsDrivers, REG_DWORD, varsayılan 2 sn): Microsoft resmi belgesi: bu anahtarlar **"yalnızca sürücü geliştirme sırasında test/hata ayıklama içindir; son kullanıcılar değiştirmemelidir."** Artırmak "GPU hung" zaman aşımını geciktirir ama gerçekten takılan GPU'da PC'nin uzun süre donmasına yol açar (2. denemedeki donma buna benzer). Kiralık uzak PC'de donma = bağlantı kaybı, yeniden başlatma için sağlayıcıya muhtaç kalma riski. https://learn.microsoft.com/en-us/windows-hardware/drivers/display/tdr-registry-keys

### 2.4 MetaHuman Creator iGPU / DX11'de çalışır mı?
- Resmi cevap yok; gereksinim sayfası DX12 + 8 GB VRAM + üst segment GPU diyor. Strands saçlar SM6 ister. **Bu PC'de çalışacağına dair hiçbir kanıt bulunamadı.**
- MetaHuman Creator ayrıca **internet + Epic hesabı** ister: otomatik rig (autorig) ve doku sentezi bulut hizmetidir (metahuman.com, 2025-06-03).
  https://www.metahuman.com/news/metahuman-leaves-early-access-with-a-feature-packed-new-release

### 2.5 GPU'suz (headless) MetaHuman oluşturma/dışa aktarma
- UE **5.8+** MetaHuman Creator Python API'si: "create, edit, conform, sculpt, assemble, and export MetaHuman Character assets from scripts". Belge, karakteri editörde açmadan "headless or batch scripts" için doğrudan özellik yazmayı anlatıyor ve bulut çağrılarının "batch and commandlet contexts" içinde engelleyici (blocking) kullanılabileceğini söylüyor. Bulut adımları (doku kaynakları, autorig) Epic hesabıyla oturum açmayı gerektiriyor; ilk autorig çağrısı oturum açma akışını tetikliyor.
  https://dev.epicgames.com/documentation/metahuman/metahuman-creator-python-scripting-in-unreal-engine
- `-nullrhi` veya `-run=pythonscript` ile çalıştığı **yazmıyor**. Montaj (assemble) sırasında doku pişirme GPU isteyebilir — **belirsiz, test gerekli**. Bu, bu PC için en ilginç ama kanıtsız yoldur.
- Dışa aktarma seçenekleri (resmi): **DCC Export** (head/body .dna + .png doku + .json, zip), Geometry Export (UE Skeletal Mesh), DNA Export, Materials Export. **Doğrudan FBX seçeneği yok**; FBX için UE'nin genel Skeletal Mesh FBX dışa aktarımı veya Blender gerekir.
  https://dev.epicgames.com/documentation/metahuman/metahuman-creator-export-tool-in-unreal-engine

---

## 3. MetaHuman'ı Unreal dışında kullanmak

### 3.1 Lisans (Haziran 2025'ten beri)
- MetaHuman artık standart **Unreal Engine EULA** altında; "MetaHumans can be used with any engine or creative software" (metahuman.com, 2025-06-03). CG Channel (2025-06-04): Unity ve Godot'da kullanılabilir, Fab'de satılabilir; MetaHuman karakterleri "non-engine product" sayılır, başka motordaki projelerde Epic'in %5 payı yok; yıllık geliri $1M altı için ücretsiz. AI iş akışında kullanılabilir ama **AI modeli eğitmek/geliştirmek için kullanılamaz**.
  https://www.cgchannel.com/2025/06/you-can-now-sell-metahumans-or-use-them-in-unity-or-godot/ · https://www.unrealengine.com/eula/mhc
- Yani **Unity'de video üretmek lisans açısından serbest** (gelir < $1M).

### 3.2 Resmi DCC yolları
- **MetaHuman for Maya** (Maya 2022–2025) ve **MetaHuman for Houdini** (Houdini 20.5 + SideFX Labs): DCC Export klasörünü kullanır; saçlar Alembic. **Maya ve Houdini ücretli** (Houdini Apprentice ücretsiz ama ticari değil — doğrulanmadı).
- **OpenRigLogic** (Epic, MIT lisansı): RigLogic + DNA kütüphaneleri, C++ ve Python; "third-party content creation tools" içine entegre edilebilir. Unity için hazır eklenti **yok**; teorik olarak C# sarmalayıcı yazılabilir (büyük iş). https://github.com/EpicGames/OpenRigLogic

### 3.3 Ücretsiz Blender köprüsü
- **Poly Hammer — Character DNA** (temel sürüm 2026-07-22'den beri **ücretsiz**, GPLv3): Blender 4.5/5.1/5.2, UE 5.6–5.8 DNA'ları; head.dna/body.dna içe aktarır, yüz paneli (face board) ve iskeleti canlandırır, veriyi pişirir, DNA'ya geri yazar. Windows/Linux/macOS. Poly Hammer Portal hesabı gerekir. Hâlâ açık beta.
  https://digitalproduction.com/2026/07/22/character-dna-goes-free/ · https://digitalproduction.com/2026/05/12/poly-hammer-brings-metahumans-to-blender-rigify/
- Topluluk eğitimi "Custom MetaHuman to Blender – Full FREE Workflow" (2025-09-20): UE'de oluştur → Blender → materyal + groom → **ARKit shape key** ekle → animasyon aktar.
  https://forums.unrealengine.com/t/community-tutorial-custom-metahuman-to-blender-full-free-workflow/2658456
- Blender'dan FBX → Unity: standart yol. Unity'de yüz için ARKit tarzı 52 blendshape kullanılabilir (projede dudak senkronu bunlara bağlanır).

### 3.4 Hazır MetaHuman'lar (Fab)
- Fab'de "MetaHuman" kanalında karakter, groom, kıyafet ürünleri var; bazıları ücretsiz olabilir. Resmi sayfalar fiyat listesi vermiyor. **Fab'den indirilen MetaHuman'ı açmak/montajlamak yine UE + MetaHuman Creator ister.** Ücretsiz örnek sayısı doğrulanamadı.
  https://dev.epicgames.com/documentation/metahuman/metahumans-on-fab · https://dev.epicgames.com/documentation/metahuman/buying-metahumans-from-fab

### 3.5 Unity URP + tümleşik GPU'da kalite/performans (çıkarım, test yok)
- MetaHuman kafası LOD0'da on binlerce poligon + 8K'ya varan dokular + ~800+ eklem ve yüzlerce blendshape. Unity'de RigLogic yok → yalnız pişirilmiş blendshape/kemik animasyonu.
- Saç: UE "strands" Unity'ye gelmez; **kart (card) saç** LOD'ları kullanılmalı (Blender'da groom → card dönüşümü gerekebilir).
- Cilt: URP Lit ile MetaHuman cildi plastik görünür; alt yüzey saçılması (SSS) HDRP'de var, URP'de yok/sınırlı. Gerçekçilik UE'deki seviyenin altında kalır.
- iGPU'da: dokuları 2K'ya düşürmek, düşük LOD (LOD1–LOD3) kullanmak gerekir. Video **çevrimdışı kayıt** (Recorder, sabit kare hızı) olduğundan gerçek zamanlı FPS şart değil; yavaş da olsa kayıt alınabilir — bu iGPU için önemli bir avantaj.

---

## 4. Diğer ücretsiz gerçekçi karakter seçenekleri

| Seçenek | Ücret / lisans | Bu PC'de | Not | Kaynak |
|---|---|---|---|---|
| **MPFB 2** (MakeHuman Blender eklentisi) | Ücretsiz, kod GPLv3, **üretilen içerik CC0** (ticari serbest) | Evet (Blender iGPU'da çalışır) | Yaş, kilo, cinsiyet, etnik köken, cilt dokusu, kıyafet; otomatik rig (Rigify dahil); Blender'ın tüm formatlarına dışa aktarım. Gerçekçilik MetaHuman'dan düşük; yüz ifadeleri için ek iş gerekir. | https://www.cgchannel.com/2025/03/check-out-open-source-blender-character-generation-plugin-mpfb-2 · https://extensions.blender.org/add-ons/mpfb/ |
| **Character Creator 5 denemesi** | 30 gün; video/hareket dışa aktarım 30 sn, video 720p, **filigranlı**, **ticari kullanım yok** | Resmi asgari: GTX 10 / RX 5500 XT + **8 GB VRAM** → iGPU altında | Uygun değil | https://reallusion.com/character-creator/download.html |
| **iClone Personal** (2026-06'dan beri ücretsiz) | Süresiz; dışarı aktarım toplam **30 kez**; yerel render çıktısı **yalnız kişisel kullanım** | Belirsiz | Ticari video renderı ücretli lisans ister; CC karakterleri dahil mi belirsiz | https://www.cgchannel.com/2026/06/reallusion-releases-free-animation-tool-iclone-person/ |
| **Daz Studio + ücretsiz Genesis** | Standart lisans render edilmiş **görüntü/video** çıktısına izin verir; oyun/uygulama içinde 3D dağıtımı için Interactive License gerekir | Daz iGPU'da yavaş (Iray CPU'ya düşer) | Unity'ye dışa aktarıp kendi videomuzu render etmek gri bölge (forum, 2022); en temiz yol Daz/Blender'da render. | https://www.daz3d.com/interactive-license-info · https://www.daz3d.com/forums/discussion/comment/7278761 |
| **Unity Digital Human paketi** (Heretic/Enemies teknolojisi) | Ücretsiz, Unity Companion License (ticari serbest) | Muhtemelen çok yavaş | HDRP odaklı; yeni cilt/göz shader'ı Unity 2022.2+; tek hazır kafa, kendi karakterini yapmak zor | https://www.cgchannel.com/2022/11/download-unitys-updated-free-digital-human-package/ |

Human Generator (Blender) ücretlidir; ücretsiz sürümü bu araştırmada doğrulanmadı.

---

## 5. Önerilen sıra ve kullanıcıdan gerekenler

1. **"Bir kerelik GPU erişimi" yolu (en yüksek kalite, 0 TL):** Arkadaşın/okulun/internet kafenin RTX 3060+ (tercihen 8 GB VRAM) bilgisayarında UE 5.8 (veya 5.6+) + Epic hesabı → MetaHuman Creator'da 4–6 karakter yap → **DCC Export** (.dna + doku) ve Skeletal Mesh FBX dışa aktar → USB/bulut sürücüyle bu PC'ye taşı → Blender + Character DNA (ücretsiz) → ARKit shape key → FBX → Unity URP. Gerekenler: GPU'lu PC'ye birkaç saat erişim, Epic hesabı, internet. Not: internet kafede UE kurmak (~50 GB+) genelde mümkün değil; arkadaş/okul daha gerçekçi.
2. **MPFB 2 yolu (bugün, bu PC'de, garantili çalışır):** Blender + MPFB 2 → karakter → Rigify → Unity. Gerçekçilik orta. Gerekenler: Blender kurulumu ve birkaç saat öğrenme.
3. **UE'yi bu PC'de deneme (düşük olasılık):** Önce GPU sürücüsünü güncelle/geri al, RDP Grup İlkesi'nde donanım GPU'sunu aç, `-dx11` ile yeni Scalable boş proje. Açılırsa: MetaHuman Python API ile karakter oluştur/montajla/dışa aktar dene (görüntüye bakmadan). TdrDelay değiştirme **önerilmez** (Microsoft uyarısı + kiralık PC donma riski). Gerekenler: kiralık PC sağlayıcısının izni; donarsa yeniden başlatma imkânı.
4. **Bulut kredileri:** Kredi/banka kartı olmadan hiçbiri açılmıyor; kart olsa bile GPU fiilen verilmiyor. Bu yolu bırak.

## 6. Açıkça imkânsız / önerilmeyen

- Google Cloud deneme hesabında GPU ve Windows Server — **resmi olarak yasak**.
- Azure ücretsiz/öğrenci hesabında GPU VM — **fiilen kota yok**; kart şart.
- AWS ücretsiz planda GPU örneği — ücretsiz uygun tipler arasında yok.
- Oracle, Azure, Google denemeleri — **kart zorunlu**.
- Colab ücretsizde uzak masaüstü — **ToS yasağı**. Kaggle/Lightning/Paperspace ücretsizde Windows masaüstü yok.
- Shadow — ücretsiz deneme yok.
- MetaHuman Creator'ı internetsiz/Epic hesapsız çalıştırmak — bulut autorig/doku sentezi gerekli.
- MetaHuman Creator'ın Vega iGPU'da resmi desteği — yok (RTX 3070 / RX 6800 XT + 8 GB VRAM, DX12).
- UE strands saç ve RigLogic yüz rigini Unity'ye birebir taşımak — hazır yol yok (kart saç + pişirilmiş blendshape gerekir).
- Character Creator denemesiyle ticari/filigransız video — yok.

## 7. Belirsizlikler (test gerekir)

- UE 5.8.4'ün bu Vega'da `-dx11` ile açılıp açılmadığı.
- MetaHuman Python montaj/dışa aktarımının `-nullrhi` veya komutlet altında çalışıp çalışmadığı.
- Kiralık PC'nin RDP ayarlarında iGPU'nun gerçekten kullanılıp kullanılmadığı (Basic Render Driver'a düşüyor olabilir — bu da çökmelerin nedeni olabilir).
- Fab'de ücretsiz MetaHuman sayısı ve lisansı.
- Microsoft for Startups açık yolunun bireylere ve Türkiye'ye açık olup olmadığı.
