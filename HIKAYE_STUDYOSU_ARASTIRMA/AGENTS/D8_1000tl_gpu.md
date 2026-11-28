# D8 — 1000 TL ile GPU'lu makinede MetaHuman üretmek (saatlik bulut karşılaştırması)

Tarih: 2026-11-28 · Yöntem: yalnızca web araştırması (WebSearch/WebFetch, 27 arama). **Hiçbir şey test edilmedi**, hesap açılmadı, ödeme yapılmadı.
D4 (ücretsiz yollar) ve D5 (dünyada ücretsiz GPU) tekrarlanmadı; burada **para ödeyerek** (en fazla 1000 TL) işi bitirme yolları var.

Etiketler: **[D]** doğrulandı (resmi/güncel sayfa okundu) · **[KD]** kısmen doğrulandı (üçüncü taraf ya da eski sayfa) · **[DY]** doğrulanmadı (çıkarım).

İş tanımı: UE 5.8 kur (~60 GB), MetaHuman Creator'da 3–6 karakter yap, DCC Export + FBX dışa aktar, birkaç GB sonucu indir. Toplam **10–15 saat** (2–3 saati kurulum + shader derleme).

---

## 0. Kur ve bütçe

- Bloomberg HT: **13 Kasım 2026 USD/TRY ≈ 50,37** (satış 50,3737); **19 Kasım 2026 ≈ 50,47** (satış 50,4695). EUR/TRY ≈ 55,5. [D]
  https://www.bloomberght.com/dolar-ne-kadar-oldu-13-kasim-2026-dolar-kurunda-son-durum-ne-pkh2-3794528 · https://www.bloomberght.com/dolar-ve-euro-kuru-bugun-ne-kadar-oldu-19-kasim-2026-doviz-fiyatlari-pkh2-3795077
- Hesapta **1 $ = 52 TL** kullanıldı (piyasa kuru + banka kur farkı/yurt dışı işlem komisyonu için ~%3 pay) [DY: bankaya göre değişir]. 1 € ≈ 57 TL.
- **1000 TL ≈ 19 $.**
- Uyarı: Bazı yabancı sağlayıcılar Türkiye'deki bireylere **%20 KDV** ekleyebilir (Türkiye'de yabancı dijital hizmetlere KDV uygulanıyor) [DY: sağlayıcıya göre]. Aşağıdaki toplamlar KDV'siz; KDV eklenirse ×1,2.

---

## 1. Önemli: MetaHuman Creator Linux'ta resmi olarak var mı?

| Sürüm | Durum | Kaynak |
|---|---|---|
| UE 5.6 | **Hayır.** Epic çalışanı (JamesPullan, 2025-06-05): "We currently only support MetaHuman on Windows." 2025-06-06: 5.6 döngüsünde Linux/Mac "unlikely". Linux'ta yalnız Capture Data seçenekleri görünüyordu; topluluk yamaları var (resmi değil). [D] | https://forums.unrealengine.com/t/ue-5-6-metahuman-character-creator-linux-support/2541069 |
| UE 5.7 | **Evet.** Resmi duyuru (Epic, 2025-11-12): "MetaHuman Creator in Unreal Engine is now available on Linux and macOS". metahuman.com (2025-12-15): "The MetaHuman Creator plugin for Unreal Engine is now available on Linux and macOS"; **Animator** Linux'u sonraya kaldı. [D] | https://forums.unrealengine.com/t/metahuman-5-7-released/2672724 · https://www.metahuman.com/releases/metahuman-5-7-is-now-available |
| UE 5.8 | **Evet** (Creator + artık Animator da). 5.8 sürüm notları: "MetaHuman Animator now supports Linux and macOS, expanding the availability of the Unreal Engine editor and MetaHuman Creator across platforms." Gövde animasyonu özellikleri şimdilik yalnız Windows. [D] | https://dev.epicgames.com/documentation/metahuman/metahuman-5-8-release-notes-in-unreal-engine?lang=en-US |

Sonuç: **UE 5.8 Linux + MetaHuman Creator resmi olarak destekleniyor.** Bu, Linux tabanlı ucuz GPU kiralamayı (Vast.ai, E2E) mümkün kılıyor. Linux'ta DCC Export'un da çalıştığı ayrıca yazılmamış [DY] (Creator'ın parçası olduğu için büyük olasılıkla çalışır). MetaHuman Creator bulut hizmeti kullanır (autorig, doku) → makinede internet + Epic hesabı şart (D4).

---

## 2. Sağlayıcı karşılaştırması

Sütunlar: Windows? · GPU/VRAM · $/saat · ~100–150 GB disk · indirme (egress) · en az yükleme · ödeme · bonus · oturum sınırı · ToS (masaüstü + oyun motoru).

### 2.1 TensorDock (pazar yeri, KVM sanal makine) — **en ucuz Windows adayı**
- **Windows:** Var. Resmi belgelerde "Windows 10'a NVIDIA sürücüsü kurma", "RDP ile bağlanma", "Parsec/Moonlight/Sunshine" rehberleri var; sayfa "Windows or Linux workloads" diyor. Windows lisans durumu yazmıyor. [D/KD] https://docs.tensordock.com/llms.txt · https://docs.tensordock.com/virtual-machines/using-parsec-moonlight-and-sunshine
- **GPU ve fiyat** (ev sahibine göre değişir, saniye bazlı): RTX 3070 ~$0,08 · RTX 3070 Ti / A4000 ~$0,13 · RTX 3080 Ti ~$0,17 · **RTX 3090 (24 GB) ~$0,18–0,20** · RTX 4090 ~$0,25–0,35 · A5000 ~$0,19. vCPU/RAM ayrıca ücretli (tutarı doğrulanmadı). [D] https://tensordock.com/cloud-gpus · https://gpuperhour.com/providers/tensordock
- **Disk:** NVMe $0,00005/GB/saat (en az 20 GB) → 150 GB ≈ $0,0075/saat (2 gün tutarsan ~$0,36). [D]
- **Egress:** "ücretsiz", 1 Gbps paylaşımlı. [D]
- **En az yükleme:** $5 [D]. Bakiye sıfırlanınca sunucu **otomatik silinir** [KD: spheron, 2026-10-19].
- **Ödeme:** Kredi kartı (tek seferlik yükleme) [D: dashboard FAQ]; PayPal/kripto doğrulanmadı.
- **Bonus:** Yok bulundu. **Oturum sınırı:** Yok (bakiye bittikçe). **ToS:** AUP sayfası açılmadı [DY]; genel IaaS, Windows+Parsec rehberleri var → masaüstü kullanım fiilen destekleniyor.
- Not: Tüketici GPU'su doğrudan atanır (passthrough) → **tam DX12** [DY ama KVM passthrough için normal]. Ev sahibi kalitesi değişken; her ev sahibinde Windows imajı olmayabilir [DY].
- **Tahmini toplam (12–15 sa, RTX 3090, 8 vCPU/32 GB):** ~$0,30–0,45/sa × 12–15 + disk ≈ **$4–7 → 2 × $5 yükleme = 260–520 TL.**

### 2.2 Vast.ai — Ubuntu Desktop VM + UE 5.8 Linux
- **Windows:** **Yok.** VM şablonları yalnız Ubuntu (Ubuntu 22.04 VM, Ubuntu Desktop VM). [D] https://docs.vast.ai/guides/instances/virtual-machines.md
- **Masaüstü:** Ubuntu Desktop (VM): Selkies WebRTC "hardware-accelerated graphics", Blender, Steam/Proton, Sunshine hazır. VM'ler **yalnız tüketici GPU'larında** (RTX 4090/5090). VM'ler beta, "Docker'dan daha kısıtlı". [D] https://docs.vast.ai/linux-virtual-desktop
- **Fiyat:** RTX 4090 kabaca $0,30–0,50/sa (pazar, sürekli değişir) [KD]. D5: RTX 3060 ~$0,13 (ama 3060'ta VM yok).
- **Disk:** GB/saat üzerinden, makine durdurulsa da **disk ücreti sürer**. [D] https://docs.vast.ai/guides/reference/billing.md
- **Egress:** Makineye göre $/TB; gelen + giden **her bayt** ücretli (UE indirmesi dahil!). [D]
- **En az yükleme:** Belgede yazmıyor [DY]. **Ödeme:** Stripe ile kart, **kripto (BitPay, Crypto.com)**. [D]
- **Bonus:** Yok (D5). **ToS:** Masaüstü şablonu resmi olarak sunuluyor → izinli.
- **UE:** Linux için UE 5.8 ikili (binary) dosyası Epic hesabıyla unrealengine.com'dan indirilir [KD]; MetaHuman Creator 5.7+'da Linux'ta resmi (bölüm 1).
- **Tahmini toplam (12–15 sa RTX 4090):** GPU $5–8 + disk ~$1–2 + trafik (~70 GB × $5–20/TB ≈ $0,4–1,4) ≈ **$7–12 → 380–620 TL.**

### 2.3 airgpu — hazır Windows bulut PC (en kolay)
- **Windows:** Var, tam masaüstü, "Windows'ta çalışan her yazılım" kurulabilir (Steam/Epic dahil). [KD: cloudbase.gg, son güncelleme 2024-06-14] https://cloudbase.gg/airgpu-cloud-gaming/
- **GPU/fiyat:** Tesla T4 (16 GB) $0,85/sa · AMD V520 $0,85 · A10G (24 GB) $1,15; yalnız K. Amerika: Quadro RTX 4000 $0,65, RTX A4000 $1,10. 8 vCPU/32 GB RAM için **+$0,30/sa**. "Spare capacity" modu ~%50 ucuz ama kesilebilir. Avrupa: Frankfurt, Dublin, Londra, Paris, Stockholm; Orta Doğu: Bahreyn. [KD] Subger (Kasım 2026 güncel, fiyat 2026-03-18 doğrulama): $0,50–2,50/sa. [KD] https://subger.com/en/service/airgpu
- **Disk:** $3,50 / 50 GB / ay → 100 GB = $7/ay. [KD]
- **Egress, en az yükleme:** Yazmıyor [DY]. **Ödeme:** Kart; bazı pazarlarda PayPal/Apple Pay/Google Pay [KD]. **Bonus/deneme:** Yok (D5). **Oturum sınırı:** Genel sınır yok; spare modunda kesilebilir. **ToS:** Masaüstü ve her yazılım serbest.
- Not: T4 performansı RTX 3070'in **altında** (Epic önerisi RTX 3070) ama 16 GB VRAM var; 4 vCPU/16 GB'lık temel paket shader derlemesinde yavaş [DY].
- **Tahmini toplam:** T4 temel 12–15 sa: $10–13 + disk $7 = **$17–20 → 890–1030 TL** (sınırda; KDV eklenirse aşar). Spare modunda **~620–730 TL**. 32 GB RAM eklenirse bütçeyi aşar.
- airgpu.com/pricing sayfası açılamadı → güncel fiyatı ödeme öncesi kontrol et.

### 2.4 Vagon (İstanbul kökenli) — bütçeyi aşıyor
- **Windows:** Var. Saatlik (Windows, dakika bazlı): **Planet T4 16 GB $0,99**, Spark A10G 24 GB $1,67, Flame $2,27. Aylık plan zorunlu: **Personal $7,99/ay (75 GB disk)**, +50 GB = $5. İndirme: ayda ilk 10 GB ücretsiz, sonra **$1,50/10 GB**. Deneme: $0,99 (1 saat Planet + 7 gün disk). [D] https://vagon.io/pricing
- Türkiye bölgesi ve TL fiyatı **yok** (19 bölge: Frankfurt, Dubai…). [D] https://vagon.io/region-prices
- **Tahmini toplam:** 12 sa × $0,99 + $7,99 + $5 = **$25 → ~1290 TL** (75 GB'ta kalırsan ~$20 → ~1030 TL ama Windows + UE 60 GB ile 75 GB çok dar). **Bütçe dışı.**

### 2.5 Shadow — uygun değil
- 2026 planları: **Neo Lite $29,99/ay** (RTX 2000 Ada 16 GB, 120 saat, oturum 4 sa), Neo $37,99, Power $54,99. [KD: tech-insider, 2026-11-13] https://tech-insider.org/shadow-pc-hour-caps-avoid-lockouts-2026/
- shadow.tech ülke seçicide **Türkiye yok** (yalnız K. Amerika + Avrupa ülkeleri). [KD] https://shadow.tech/
- $29,99 ≈ **1560 TL** → bütçe dışı + Türkiye'den abonelik belirsiz.

### 2.6 Paperspace (DigitalOcean) — Windows yok
- "We have deprecated Windows-based templates, so they are unavailable for new Paperspace users" — **1 Temmuz 2024'ten beri yeni kullanıcıya Windows yok**. GRID makineleri emekli. [D] https://docs.digitalocean.com/notes/2024/paperspace-retiring-windows/ · https://docs.digitalocean.com/products/paperspace/index.html.md

### 2.7 RunPod — önerilmez
- Pod'lar Linux konteyner; Windows yok [KD]. Vulkan desteği "very hit and miss, not all the hosts have it" (RunPod çalışanı, 2025-10-01). UE editörü Vulkan ister → riskli. [KD] https://www.answeroverflow.com/m/1422754410542530621
- Ücretsiz kredi yok (D5).

### 2.8 AWS (g4dn / g5) — kredilerle **belki 0 TL**, ama zahmetli
- **g4dn.xlarge** (T4 16 GB, 4 vCPU, 16 GB RAM), Frankfurt: isteğe bağlı **$0,658/sa**, spot ~$0,31/sa (Linux fiyatı; Windows lisansı ~+$0,18/sa [DY]). **g5.xlarge** (A10G 24 GB): $1,258 / spot ~$0,58. Spot kesilme oranı %15–20. [KD] https://www.doit.com/compute/spot/eu-central-1/g4dn.xlarge · https://www.doit.com/compute/spot/eu-central-1/g5.xlarge
- **Kota:** Yeni hesapta "Running On-Demand G and VT instances" kotası **0** → g4dn açılmaz, bölge bazında artış talebi gerekir (onay süresi/ret riski). [KD] https://www.repost.aws/questions/QU9YFKjFgOSci8-EW-uUOXrw/unable-to-create-an-windows-server-2022-instance-with-instance-type-g4dn-xlarge-in-free-tier
- **Krediler:** Free planda GPU yok (D4). Paid plana geçince kalan krediler (yeni hesapta $100–200) "automatically apply to future AWS bills". [D] https://aws.amazon.com/free/faqs/ — Kredinin G örneklerinde geçerli olduğu açıkça yazmıyor ("participating services") [DY]. Paid plan için kart gerekir [DY].
- Disk (EBS gp3) 150 GB × 2 gün ≈ $1; internet çıkışı ayda ilk 100 GB ücretsiz [DY].
- **Tahmini toplam:** krediler geçerse **0 TL**; geçmezse 12–15 sa Windows on-demand ≈ $10–13 + $1 → **570–730 TL**, spot ≈ **360–450 TL**. **Risk:** kota onayı günler sürebilir/reddedilebilir; makineyi silmeyi unutmak fatura çıkarır.

### 2.9 Azure NV-serisi — önerilmez
- NV6ads A10 v5 = A10'un 1/6'sı (~4 GB VRAM) → MetaHuman için yetersiz. NV12ads (1/3 A10, ~8 GB): Linux $0,908/sa (ABD), Windows daha pahalı; NV36ads tam A10: $3,20. [KD] https://www.azurespeed.com/AzureVmPricing/Series/NVads-A10-v5
- Ücretsiz kredi spot'ta kullanılamaz, GPU kotası 0 (D4). Kart şart. **Bütçe/kota açısından uygun değil.**

### 2.10 Hostkey (Hollanda; Türkiye konumu da var) — ucuz ama belirsiz
- Saatlik GPU sunucu: **RTX PRO 2000 Blackwell (16 GB) €0,221/saatten**, RTX 5090 €0,624/sa (sınırlı). Ödeme: **BitPay ile kripto**, EUR/USD. [KD] https://hostkey.com/gpu-dedicated-servers/vds-gpu/
- Windows lisans ücreti, saatlik kiralamada en az süre/depozito ve Türkiye'de GPU'lu konum **doğrulanmadı** (Türkçe belgeler açılamadı). Haber: "Europe, the US and Turkey" konumları var [DY]. https://hostkey.com/news/176-cost-effective-server-configurations-in-europe-the-us-and-turkey/
- **Tahmini:** 12–15 sa × €0,221 ≈ €3 + Windows lisansı + depozito → **~200–600 TL [DY]**. Fiyat çekici; önce destekle yazışıp "saatlik Windows + GPU, en az ne kadar yatırmalıyım" diye sor.

### 2.11 LumaDock — uygun değil
- GPU VPS yalnız **aylık**: T4 16 GB $129/ay, RTX PRO 2000 $169/ay; yalnız Linux/FreeBSD; konumlar Avrupa/ABD (Türkiye yok); 7 gün para iade garantisi. [D] https://lumadock.com/gpu-vps?language=turkish
- ~6700 TL/ay → bütçe dışı (iade garantisine güvenerek kiralamak önerilmez).

### 2.12 Turkcell bulut ve diğer Türk sağlayıcılar
- Bireye **saatlik GPU** veren Türk sağlayıcı bulunamadı [DY]. Cloud4U Türkiye (L40S/H100) kurumsal pilot odaklı (D5).

### 2.13 E2E Networks (Hindistan) — Linux, **$20 ücretsiz kredi**
- D5'te ayrıntılı: kimlik + özçekim + telefonla doğrulama sonrası **$20 kredi**, kart gerekmez. **T4 $0,45/sa, L4 (24 GB) $0,57/sa**, GPU'da yalnız Linux. [D: D5] https://www.e2enetworks.com/pricing.md
- Yeni bilgi: MetaHuman Creator artık Linux'ta resmi (bölüm 1) → bu yol **teknik olarak mümkün** hale geldi. Ama Linux masaüstü + sanal ekran kurulumu kullanıcıya düşer; kredinin GPU'da geçerliliği hâlâ belirsiz [DY].
- **Tahmini:** kredi geçerse **0 TL**; geçmezse 12–15 sa L4 ≈ $7–9 + KDV(%18 Hindistan GST) → **~430–550 TL**.

### 2.14 Genesis Cloud
- Eski listelerde RTX 3080/3090 (İzlanda) var; 2026'da H100/H200 odaklı. Güncel tüketici GPU ve Windows **doğrulanamadı** [DY]. https://pcr.cloud-mercato.com/providers/genesis-cloud/flavors/vcpu-4_memory-18g_disk-80g_nvidia3090-1/pricing

---

## 3. Özet tablo (12–15 saat, KDV hariç, 1 $ = 52 TL)

| Sağlayıcı | Windows | GPU (VRAM) | Tahmini toplam | Ödeme (Türkiye'den) | Durum |
|---|---|---|---|---|---|
| **TensorDock** | Evet | RTX 3090/4090 (24 GB) | **260–520 TL** | Kart | En ucuz Windows; ev sahibine bağlı [KD] |
| **Vast.ai** | Hayır (Ubuntu Desktop VM) | RTX 4090 (24 GB) | **380–620 TL** | Kart, **kripto** | MH Creator Linux resmi; kurulum biraz teknik |
| **airgpu** | Evet | T4 (16 GB) | **620–1030 TL** | Kart (+PayPal bazı pazarlarda) | En kolay, ama bütçe sınırda [KD] |
| AWS g4dn | Evet | T4 (16 GB) | 0 TL (kredi) / 360–730 TL | Kart | Kota 0 → onay gerekir |
| Hostkey | Evet (lisans?) | RTX PRO 2000 (16 GB) | ~200–600 TL [DY] | Kripto (BitPay) | Ayrıntılar belirsiz |
| E2E Networks | Hayır | L4 (24 GB) | 0 TL (kredi) / 430–550 TL | Kimlik doğrulama, ön ödeme | Kredi GPU'da geçerli mi belirsiz |
| Vagon | Evet | T4 (16 GB) | 1030–1290 TL | Kart | Bütçe dışı |
| Shadow | Evet | RTX 2000 Ada | ~1560 TL/ay | — | Bütçe dışı, Türkiye yok |
| Paperspace | Yeni kullanıcıya hayır | — | — | — | Windows 2024'te kapandı |
| RunPod | Hayır | — | — | — | Vulkan güvenilmez |
| Azure NV | Evet | 4–8 GB parça | >1000 TL | Kart | Kota/VRAM uygun değil |
| LumaDock | Hayır | T4 | ~6700 TL/ay | — | Yalnız aylık |

### Türkiye'den ödeme notları [DY]
- **Türk banka kartı (debit):** Mobil uygulamadan "internet alışverişi" ve "yurt dışı işlem" açılırsa Stripe kullanan sitelerde (Vast, TensorDock, airgpu) genelde çalışır; banka kur farkı + olası komisyon ~%1–3. Sanal kart (bankanın sanal kartı) önerilir: limiti 1000 TL'ye ayarla.
- **Kripto:** Vast (BitPay, Crypto.com) ve Hostkey (BitPay) kabul ediyor [D/KD]. Türk borsasından USDT gönderip ödemek mümkün ama ağ ücreti + yanlış ağ riski var.
- **PayPal:** Türkiye'de PayPal 2016'dan beri kapalı [DY: yaygın bilgi] → kullanılamaz say.
- **Papara:** 2025'teki soruşturma/kayyum süreci sonrası kartın yurt dışı ödemede durumu belirsiz [DY].

---

## 4. Önerilen iş planı (her sağlayıcıda aynı)

1. **Hazırlık (makine kapalıyken, ücretsiz):** Epic hesabı aç, 2 adımlı doğrulamayı ayarla. Karakterlerin tarifini (yaş, ten, saç, kıyafet) önceden yaz. Google Drive (15 GB ücretsiz) hazır olsun.
2. **Makine seç:** en az 8 vCPU, 32 GB RAM, **150–200 GB** disk, 16–24 GB VRAM.
3. **Bağlan:** Windows'ta önce RDP, sonra **Parsec** (ücretsiz; RDP GPU'yu kullanmayabilir, D4 §2.2). Linux'ta Selkies (tarayıcıdan).
4. **Kur (2–3 sa):** Epic Launcher → UE 5.8 (yalnız Win64/Linux platformu; Android/iOS gibi hedefleri kurma → disk ve süre azalır). Boş proje → MetaHuman Creator eklentisi → ilk açılışta shader derlemesini bitir.
5. **Üret (3–6 sa):** 3–6 karakter; her biri ~1 saat. Autorig + doku için internet açık olsun.
6. **Dışa aktar (1 sa):** DCC Export (.dna + doku + .json zip) + gerekirse Skeletal Mesh FBX. Hepsini zip'le → Google Drive'a yükle (makineden) → evden indir.
7. **Kapat:** Makineyi **sil** (yalnız durdurmak disk ücretini sürdürür — Vast, TensorDock, AWS). Bakiyeni ve faturayı kontrol et.

Para tasarrufu: işi tek oturumda bitir (disk günü saymasın), karakter tasarımlarını önceden planla, makineyi boşta açık bırakma.

---

## 5. Belirsizlikler (ödeme öncesi kontrol et)

- TensorDock'ta o an Windows imajlı ve 24 GB GPU'lu ev sahibi bulunup bulunmadığı; Windows'un etkin (lisanslı) olup olmadığı.
- Vast.ai VM'inde UE 5.8 Linux editörünün Vulkan'la sorunsuz açılması (Selkies "hardware-accelerated" diyor ama UE adı geçmiyor).
- MetaHuman **DCC Export**'un Linux'ta çalıştığının açıkça yazılı olmaması.
- airgpu'nun güncel fiyatı (resmi fiyat sayfası açılamadı) ve Türkiye'ye KDV ekleyip eklemediği.
- AWS kredisinin G tipi örneklerde geçerliliği ve kota onay süresi.
- Hostkey'de saatlik Windows lisans ücreti ve en az depozito.
- Türk banka kartlarının bu sitelerde çalışması (bankaya göre değişir).
