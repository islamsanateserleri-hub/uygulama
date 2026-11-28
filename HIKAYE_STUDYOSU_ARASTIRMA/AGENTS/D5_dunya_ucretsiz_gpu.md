# D5 — Dünyada sıfır maliyetli GPU masaüstü: Türkiye'den erişilebilir yollar

Tarih: 2026-11-28 · Yöntem: yalnız web araştırması (WebSearch/WebFetch, 27 arama). **Hiçbir şey test edilmedi**, hesap açılmadı, indirme yapılmadı.
Hedef: Türkiye'deki, parası ve kredi kartı olmayan bir kullanıcının birkaç saatten birkaç güne kadar **RTX 3060+ sınıfı (8 GB+ VRAM) GPU'lu, UE5 editörünü açabilecek bir masaüstüne** (Windows ya da Linux) ücretsiz ulaşması.
D4'te zaten işlenenler burada tekrarlanmadı: Google Cloud, Azure, AWS ve Oracle denemeleri, Colab, Kaggle, Lightning, Paperspace, Shadow.

Etiketler: **[D]** = resmi sayfa veya güncel kaynak okunarak doğrulandı · **[KD]** = kısmen doğrulandı · **[DY]** = doğrulanmadı, çıkarım ya da topluluk bilgisi.

---

## 0. Kısa sonuç

| Sıra | Yol | GPU | Masaüstü | Kart | Türkiye | Ücretsiz süre | Gerçekçilik |
|---|---|---|---|---|---|---|---|
| 1 | **Arkadaş / okul / üniversite labı / internet kafe** (fiziksel PC) | RTX 3060–4060 (kafe) | Windows | Yok | Evet | Kafe ~40 TL/saat (2024); arkadaş/okul 0 TL | **En gerçekçi** (kafede kurulum izni yok, aşağıya bkz.) |
| 2 | **E2E Networks** (Hindistan) — kimlik doğrulamasından sonra **$20 ücretsiz altyapı kredisi** | T4 16 GB (~$0,45/sa), L4 24 GB (~$0,57/sa) | Linux VM (GPU'da Windows listelenmiyor) | **Gerekmiyor** (pasaport/kimlik + özçekim + telefon) | Uluslararası müşteri kabul ediliyor [D] | $20 ≈ T4'te ~40 saat, kredinin GPU'da geçerli olup olmadığı **[DY]** | Orta: kredi GPU'da geçerliyse tek gerçek "kartsız GPU VM" |
| 3 | **Modal** Starter — her ay **$30** | T4 / L4 / A10 (24 GB) | Linux konteyner; resmi VNC masaüstü örneği var, ama GPU'suz | Spheron'a göre gerekmiyor [KD] | Belirtilmemiş | Aylık $30 (≈ 27 sa A10) | Düşük: gVisor varsayılan olarak Vulkan/grafiği kapatıyor → UE editörü büyük olasılıkla açılmaz |
| 4 | **AutoDL** (Çin) — yeni kullanıcıya ¥10 | RTX 3090/4090 | Linux konteyner, masaüstü yok | — | Çin telefonu gerekebilir [DY] | ~6 saat 3090 | Düşük |
| 5 | **Çin bulut PC'leri** (顺网云电脑 vb.) | Oyun GPU'su | Windows | — | Çin telefonu ve gerçek ad doğrulaması büyük olasılıkla şart [DY] | 2–5 saat deneme kodu (söylenti) | Çok düşük |
| — | **Vast.ai** (ücretsiz değil, en ucuz yedek) | RTX 3060 ~$0,13/sa; VM şablonu RTX 4090/5090 | Ubuntu Desktop VM (donanım hızlandırmalı WebRTC) | Ön ödemeli kredi | Muhtemelen evet | Ücretsiz kredi yok | Para gelirse en ucuz yol (5 $ ≈ 30+ saat) |

**Sonuç:** Kartsız ve ücretsiz bir **Windows + GPU** bulut makinesi **bulunamadı**. Kartsız ve ücretsiz GPU için tek ciddi aday **E2E Networks**: $20 kredi, ama Linux, ve kredinin GPU'da harcanıp harcanamayacağı belli değil. En garantili yol hâlâ fiziksel bir GPU'lu PC'ye birkaç saat erişmek.

---

## 1. Bulut oyun servisleri (tam Windows masaüstü veriyorlar mı?)

### 1.1 NVIDIA GeForce NOW (ücretsiz katman) — **İşe yaramaz** [D]
- Resmi SSS: ücretsiz üyeler "basic rigs" alır, kuyruk ve reklam vardır. **"Games must be accessed from the digital stores"**: yalnız katalogdaki oyunlar açılır, kendi uygulamanı ya da masaüstünü açamazsın. Türkiye adı geçmiyor ("North America, Europe, Japan, India" + Alliance ortakları).
- Kaynak: https://www.nvidia.com/en-gb/geforce-now/faq.md (2026-11 erişim)

### 1.2 Boosteroid — **İşe yaramaz** [KD]
- Yalnız oyun kütüphanesi akışı. Yardım sayfasında masaüstü veya kendi yazılımını kurmaktan söz edilmiyor; ücretsiz deneme yok (D4 ve cloudbase.gg ile uyumlu). https://help.boosteroid.com/en/content/do-you-have-a-desktop-app
- Xbox Cloud, Blacknut ve benzerleri de aynı mantıkla çalışır: oyun akışı var, masaüstü yok. [DY]

### 1.3 airgpu — Windows bulut PC, **ücretsiz deneme yok** [D]
- "airgpu does not offer a free tier, free trial, or sign-up credits"; saati ~$0,65'ten başlıyor (cloudbase.gg, güncelleme 2026-04-15). https://cloudbase.gg/airgpu-free/

### 1.4 Vagon — **ücretsiz deneme yok** [KD]
- Yardım makalesi: ücretsiz deneme yok; $0,99'luk ücretli deneme (1 saat + 50 GB, 1 hafta). Makale 2021 tarihli, güncel olmayabilir. https://help.vagon.io/en/articles/3845627-do-you-have-a-free-trial-option
- Maximum Settings, Parsec tabanlı kiralamalar ve Windows 365 için ücretsiz deneme bulunamadı (Windows 365 ücretsiz denemelerinin durdurulduğuna dair Teknoblog haberi var, sayfa açılamadı) [DY].

---

## 2. Çin

### 2.1 AutoDL [KD]
- Yeni kullanıcıya **¥10** (≈ 6 saat RTX 3090). Öğrenci doğrulamasıyla %85 indirim. GPU'lar: 3090, 4090, A100, H100 (yangmao.ai, doğrulama tarihi 2026-04-21). https://yangmao.ai/zh/compute/autodl/
- Konteyner tabanlı Linux, hazır imajlar PyTorch/SD/ComfyUI; **masaüstü yok**. Kayıt için Çin (+86) telefonu ve gerçek ad doğrulaması gerektiği yaygın biliniyor ama burada doğrulanamadı [DY]. UE editörü için uygun değil.

### 2.2 Alibaba Cloud — Wuying Workspace (bulut masaüstü) denemesi [D]
- Yeni kullanıcıya bulut bilgisayar başına **120 saat / 30 gün**, hesap başına 2 adet. Ama şartı **"enterprise real-name registration"** (kurumsal gerçek ad kaydı). Teknik özellik/GPU sayfada yazmıyor. Sayfa son değişiklik: 2026-11-16.
- **Bireysel bir kullanıcı için fiilen imkânsız.** https://help.aliyun.com/en/wuying-workspace/getting-started/free-trial-for-new-users
- Aliyun ücretsiz deneme merkezi (Çin sitesi): ECS denemesi 1 ay, ama listede GPU yok (developer.aliyun.com kullanıcı yazısı). https://developer.aliyun.com/article/1748928

### 2.3 Tencent Cloud Studio, Baidu AI Studio [DY]
- Notebook/IDE tipi ücretsiz GPU; masaüstü yok; Çin telefonu gerekir (aramada resmi doğrulama bulunamadı).

### 2.4 Çin bulut PC'leri (顺网云电脑, 网易云游戏, 达龙 vb.) [DY]
- Bilibili yazısı: Shunwang yeni kullanıcıya "deneme süresi" veriyor; söylentiye göre bir kodla 2–5 saat. GPU modeli ve yazılım kurma izni belirtilmemiş. https://www.bilibili.com/read/cv50017943
- Engeller: Çin telefonu ve gerçek ad doğrulaması (büyük olasılıkla), Türkiye'den 250 ms+ gecikme, oturum sonunda sistem diskinin sıfırlanma ihtimali, Epic'ten 50 GB+ indirme. **Pratik değil.**

---

## 3. Rusya

### 3.1 Yandex Cloud hibesi [D]
- Bireysel: RF/Belarus/Kazakistan dışı için **$30**, 60 gün. **Kart bağlamak şart.** Kartla alınan hibe için: "любых сервисов, **кроме кластеров GPU**". Yalnız şirkete banka havalesiyle verilen hibede GPU istisnası yok.
- https://yandex.cloud/ru/docs/free-trial/concepts/usage-grant
- Sonuç: Kart yoksa olmaz; GPU kümeleri hariç tutuluyor (tekil GPU VM'nin dahil olup olmadığı açık değil).

### 3.2 Selectel [D]
- "GPU 1 rubleye" kampanyası (T4/A2/A2000 adanmış sunucu): GPU indirimli ama sunucunun kendisi ücretli. Bireylere ve her ülkeye açık. Kampanya **2025-05-15 → 2025-12-31** arasıydı, yani bitti. https://files.selectel.ru/docs/ru/gpu1-promotion-rules-new2.pdf
- dtf.ru sıralaması: Selectel yeni müşteriye "30 000 bonus / 30 gün" veriyor (şartlar belirsiz); Cloud4Y'de "ücretsiz GPU test dönemi" var (süre/şart yok, büyük olasılıkla şirketlere yönelik). https://dtf.ru/top-raiting/5042891-luchshie-vps-hostingi-s-gpu [KD]

---

## 4. Hindistan / Güneydoğu Asya

### 4.1 E2E Networks — **en ilginç kartsız aday** [KD]
- Uluslararası müşteri doğrulaması **Stripe üzerinden pasaport / ehliyet / kimlik kartı + özçekim** ile yapılıyor. Ön ödemeli hesapta ilk düğümü (node) açmak için "kart, e-mandate veya AutoPay gerekmez, yalnız altyapı kredisi" yeterli. Doğrulama onaylanınca USD hesabına **$20 ücretsiz altyapı kredisi** yükleniyor (telefon numarası başına bir kez). [D]
  https://docs.e2enetworks.com/docs/myaccount/GettingStarted/myaccount_faq/
- Fiyatlar (KDV hariç, saatlik): **T4 $0,45 · L4 $0,57 · A30 $1,35 · A40 $1,44**. GPU düğümlerinde Windows listelenmiyor; Windows planları yalnız CPU (E1WC). T4/L4 sayfalarında "LIMITED AVAILABILITY" yazıyor. [D] https://www.e2enetworks.com/pricing.md
- Bilinmeyenler [DY]: $20 kredinin GPU düğümünde harcanıp harcanamayacağı; Türk kimliğinin/pasaportunun kabul edilip edilmediği; ToS'un uzak masaüstüne izin verip vermediği (genel IaaS olduğu için büyük olasılıkla izin veriyor).
- UE açısından: T4 (16 GB, Turing, Vulkan destekli) veya L4 (24 GB) üzerinde **Linux UE 5 editörü** teorik olarak açılabilir. Bunun için Linux masaüstü + NVIDIA sürücüsü + sanal ekran (ör. TurboVNC/VirtualGL veya Sunshine/Moonlight) kurmak gerekir. UE Linux kurulumu 40–60 GB disk ister; disk ücreti krediden düşer. **MetaHuman Creator Linux'ta destekleniyor mu, bu araştırmada doğrulanmadı.** D4'teki MetaHuman gereksinim sayfası Windows/DX12'yi anlatıyor; Linux için risk yüksek.

### 4.2 Diğerleri [DY]
- E2E'nin NASSCOM "200 saat ücretsiz A100" sayfası ve Yotta "Shakti Studio 10.000 ücretsiz kredi" programı Hint girişimlerine yönelik görünüyor; Türkiye'deki bir birey için uygun değil.

---

## 5. Avrupa

| Sağlayıcı | Kredi | Kart | GPU | Not | Kaynak |
|---|---|---|---|---|---|
| OVHcloud Public Cloud denemesi | ~$200, 1 ay | **Ödeme yöntemi şart** | Hariç değil (örneklerde GPU var) | Türkiye listede yok, "NIC pazarına" bağlı | https://www.ovhcloud.com/en/public-cloud/free-trial/ [D] |
| Scaleway | €100, 30 gün | **Şart** | Belirtilmemiş | Yalnız "Business" müşterisi | https://scaleway.com/en/signup-trial [D] |
| OVH Startup Program, EuroHPC Playground | Değişken | — | — | Şirket/girişim (EuroHPC: AB KOBİ'si) | https://startup.ovhcloud.com/ · spheron (aşağı) [KD] |
| Genesis Cloud, Hetzner | — | — | — | Ücretsiz GPU denemesi bulunamadı | [DY] |

---

## 6. Serverless / notebook (D4'te olmayanlar)

### 6.1 Modal [KD]
- Starter planda **her ay $30** ücretsiz kredi (resmi fiyat sayfası). Spheron'a göre **kart gerekmiyor**. Saatlik: T4 ~$0,59, L4 ~$0,80, A10 ~$1,10. https://modal.com/pricing · https://www.spheron.network/blog/free-gpu-cloud-credits-2026/
- Resmi örnekte Sandbox içinde Xvfb + x11vnc + noVNC + tünel ile **masaüstü** açılıyor, ama örnek GPU kullanmıyor ve Sandbox süresi 60 dk. https://modal.com/docs/examples/computer_use_vnc.md
- Modal gVisor kullanıyor (bu bilgi [DY]). gVisor `nvproxy` varsayılan olarak yalnız `compute,utility` yeteneklerine izin veriyor; Vulkan için `graphics` yeteneği **ek ayar** ister. https://gvisor.dev/docs/user_guide/gpu
- **Sonuç:** UE editörü Vulkan ister. Modal'ın `graphics` yeteneğini açtığına dair kanıt yok → **büyük olasılıkla çalışmaz.** UE imajı da (40 GB+) konteyner için çok büyük.

### 6.2 Diğer serverless kredileri (Beam 15 saat, Inferless 10 saat/$30 kartsız, Cerebrium) [KD]
- Hepsi fonksiyon/inference odaklı. Masaüstü ve grafik API'si yok → UE için uygun değil. https://amitpandeytiktok.github.io/awesome-free-compute-2026/compute.html (Haziran 2026 anlık görüntü)
- Hugging Face ZeroGPU: günde 5 dk, yalnız Gradio. İşe yaramaz.

---

## 7. Eşler arası (P2P) GPU kiralama

### 7.1 Vast.ai [KD]
- **Ücretsiz kayıt kredisi yok** (girişim programı $2.500'e kadar, ama "mevcut iş yükünü taşıyan" şirketler için). RTX 3060 ~**$0,13/saat** (gpuperhour, 2026; fiyat 60 sn'de bir güncelleniyor). "Ön ödemeli kredi", peşin kart şartı yok.
- **Ubuntu Desktop (VM)** şablonu: Selkies WebRTC ile "hardware-accelerated graphics"; Blender, Steam/Proton listelenmiş. VM'ler yalnız tüketici GPU'larında (RTX 4090/5090) çalışıyor. UE adı geçmiyor.
  https://docs.vast.ai/linux-virtual-desktop · https://gpuperhour.com/providers/vastai/rtx-3060 · https://vast.ai/article/vast-ai-startup-program
- Para çıkarsa (~$5–10) **en ucuz gerçek GPU masaüstü** yolu bu. Ödeme yöntemleri (kart/kripto) bu araştırmada doğrulanmadı.
- TensorDock, RunPod, Clore.ai: kayıtta ücretsiz kredi bulunamadı (RunPod'un $1.000'lık "Starter"ı başvuruyla verilen girişim programı) [KD].

---

## 8. Programlar ve hibeler

| Program | Ne verir | Birey/Türkiye | Hızlı mı | Kaynak |
|---|---|---|---|---|
| **Epic MegaGrants** | Nakit hibe; donanım/bulut kredisi yok | Sayfa ayrıntı vermiyor; fundsforngos'ta 2026-09-04 son tarihli bir çağrı görünüyor | Hayır (aylar sürer) | https://www.unrealengine.com/en-US/megagrants [KD] |
| Unreal Dev Grants (eski) | $5K–50K nakit, UE4 dönemi | "Anyone making cool things with UE4" | Durumu belirsiz | https://www.unrealengine.com/en-US/unrealdevgrants [KD] |
| **NVIDIA Inception** | Doğrudan kredi **yok**; ortak kredilerinin kapısını açar (ör. Nebius AI Lift) | Tescilli şirket + web sitesi şart | Hayır | spheron [KD] |
| AWS Activate Founders | $1K–5K | Kendi kendini finanse eden **girişim**, ücretli AWS hesabı (kart) | — | spheron [KD] |
| Lambda Research Grant | $5.000'e kadar | Araştırmacı/akademisyen | Değerlendirme süreli | spheron [KD] |
| Unreal Fellowship | 2026 için güncel ücretsiz program bulunamadı | — | — | [DY] |
| GitHub Student Pack | GPU'lu bulut makinesi yok (Azure öğrenci kredisinde GPU kotası yok, bkz. D4) | Öğrenci | — | [KD] |

---

## 9. Türkiye içi

### 9.1 TÜBİTAK ULAKBİM TRUBA [KD]
- Türkiye'deki üniversite/araştırma kurumlarının akademisyen, araştırmacı ve **öğrencilerine** açık (öğrenciye danışman onay e-postası gerekir). Başvuru portal.truba.gov.tr üzerinden, kimlik e-Devlet + YÖKSİS ile doğrulanıyor. Sanayi kullanıcıları vaka bazında değerlendiriliyor.
- Kullanım **konsol + iş kuyruğu (batch)** üzerinden; grafik masaüstü yok. Amaç paralel HPC işleri. **UE editörünü etkileşimli açmak için uygun değil**, ToS'a da büyük olasılıkla aykırı.
  https://bezmialem.edu.tr/PublishingImages/Lists/Haberler/AllItems/TRUBA.pdf
- (ARF-ACC GPU kümesi TRUBA'da var, ama aynı batch modeliyle.)

### 9.2 Türk bulut / GPU firmaları [KD]
- **Cloud4U Türkiye** (2026-06-24): İstanbul'daki veri merkezinden L40S ve H100 GPU bulutu, VMware Cloud Director. "Free trial" bağlantısı var, ama pilot projeleri kurumlarla görüşerek yürütüyorlar; bireysel kullanım belirsiz. https://www.cloud4u.com/news/cloud4u-ai-infrastructure-turkey/
- Hostkey, Cloudzy, LumaDock vb.'nin Türkçe GPU sayfaları var; ücretsiz GPU denemesi bulunamadı [DY].

### 9.3 İnternet kafe / oyun salonu [KD]
- Hürriyet Daily News (2024-03-26): gelişmiş oyun salonları **saatte ~40 TL**. PC başı maliyet 80.000 TL'ye kadar çıkıyor (genelde RTX 3060–4060 sınıfı, [DY]). 2026 fiyatı enflasyonla büyük olasılıkla daha yüksek [DY].
  https://hurriyetdailynews.com/internet-cafes-running-out-of-business-amid-soaring-costs-191928
- **Yazılım kurma:** Kafeler genelde disksiz sistem veya "reboot restore" (Deep Freeze benzeri) yazılımıyla çalışır; kurulan her şey yeniden başlatmada silinir, yönetici izni yoktur [DY]. UE 5.8 + MetaHuman indirme ve kurulumu (50 GB+, saatler) pratikte mümkün değil. Ancak sahibiyle konuşup **kendi harici SSD'ne kurulu taşınabilir UE** çalıştırmak bir seçenek olabilir (izin şart) [DY].

### 9.4 Üniversite labları, teknoparklar, gençlik merkezleri [DY]
- Üniversitelerin oyun geliştirme / bilgisayar mühendisliği / mimarlık labları ve teknopark kuluçka merkezlerinde RTX'li PC'ler sık bulunur. Öğrenci değilsen bir hocadan ya da kulüpten izin almak gerekir. Belediye gençlik merkezlerinde bilgisayar labı olabilir, ama GPU'lu olma ihtimali düşük. Bu araştırmada somut ücretsiz program doğrulanmadı.

---

## 10. Açıkça imkânsız / uygun değil

- GeForce NOW, Boosteroid, Xbox Cloud: masaüstü yok, kendi uygulaman açılmaz.
- airgpu, Vagon, Shadow: ücretsiz deneme yok.
- Alibaba Wuying 120 saat: yalnız kurumsal gerçek ad kaydıyla.
- Yandex Cloud: kart şart; kartlı hibede GPU kümeleri hariç.
- OVH, Scaleway: ödeme yöntemi şart (Scaleway yalnız şirketler).
- TRUBA: batch HPC, grafik masaüstü yok.
- Çin platformları: Çin telefonu ve gerçek ad doğrulaması (büyük olasılıkla), yüksek gecikme.
- NVIDIA Inception, AWS Activate, MegaGrants: şirket şartı veya aylar süren değerlendirme; hızlı GPU vermiyorlar.
- Serverless kredileri (Modal, Beam, Inferless): grafik/Vulkan yok → UE editörü yok.
- Selectel "1 ruble GPU": kampanya 2025 sonunda bitti, sunucu zaten ücretliydi.

## 11. Kullanıcının yapması gerekenler (öncelik sırasıyla)

1. **Fiziksel GPU'lu PC bul (0 TL):** RTX 3060+ bilgisayarı olan bir arkadaş, üniversite/okul labı ya da teknopark. 3–6 saatlik tek oturum yeterli: UE + MetaHuman → DCC Export → USB (D4 §5 akışı). En az 100 GB boş disk ve iyi internet gerekir.
2. **E2E Networks'ü dene (kartsız):** pasaport veya kimlik + özçekim + telefonla kaydol, doğrulama sonrası $20 kredinin yüklenip yüklenmediğine bak. Bir T4/L4 Linux düğümü açılabiliyorsa Linux masaüstü + UE kur. MetaHuman'ın Linux'ta çalışma riski yüksek; önce E2E desteğine "kredi GPU'da geçerli mi, GPU düğümünde Windows var mı" diye sor.
3. **Birkaç dolar bulunabilirse:** Vast.ai Ubuntu Desktop VM veya Windows bulut PC (airgpu ~$0,65/sa). 5–10 $ birkaç saatlik iş için yeter.
4. **Uzun vade:** MegaGrants başvurusu (nakit; hızlı değil).

## 12. Belirsizlikler

- E2E $20 kredisinin GPU'da kullanılabilirliği, Türk vatandaşlarının doğrulanması, GPU düğümünde Windows olup olmadığı.
- Modal'da `graphics` sürücü yeteneğinin açılıp açılamadığı.
- Türkiye'de 2026 internet kafe saatlik fiyatları ve kurulum politikası.
- MetaHuman Creator'ın Linux UE editöründe çalışıp çalışmadığı.
- Çin bulut PC'lerinin yabancı telefonla kayıt kabul edip etmediği.

### Ek kaynaklar
- Spheron, "Free GPU Cloud Credits 2026": https://www.spheron.network/blog/free-gpu-cloud-credits-2026/
- Awesome Free Compute 2026: https://amitpandeytiktok.github.io/awesome-free-compute-2026/compute.html
- Vast.ai sanal hesaplama: https://vast.ai/use-cases/virtual-computing
