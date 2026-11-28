# D11 — Bedava gerçekçi mekân, ışık ve 2.5D arka plan (URP 17.6 + Vega 7)

Tarih: 2026-11-28 · Yöntem: yalnız web araştırması (26 arama + sayfa okumaları). **Hiçbir şey indirilmedi, kurulmadı, test edilmedi.** Lisans özetleri sayfalardan alındı; hukuki görüş değildir.
Önceden okunanlar (tekrar edilmedi): `A04_isik_goruntu.md` (HDRP'ye geçme, Kademe 0 listesi, post tablosu), `A05_ortam.md` (kaynak/lisans tablosu, "mekân = veri"), `A12_lisans.md` (Asset Store 2.2.1.1(c), Fab Standard), `D7`, `D10`.

İşaretler: **[D]** = sayfası bu oturumda okundu · **[K]** = arama sonucu/ikincil kaynak · **[B]** = doğrulanamadı · **ÇIKARIM** = benim hesabım/yorumum.

Bağlam: Unity 6 URP 17.6, tümleşik AMD Vega 7, 1080x1920 dikey video, **kare kare çevrimdışı kayıt** (yavaş render kabul). Mekânlar şu an kodla ilkel kutulardan kuruluyor (KOD: `Mekan3D.Kur`, A05). Bütçe ≤1000 TL, bedava tercih.

---

## 0. Kısa sonuç — sıralı hızlı kazançlar

| Sıra | İş | Etki | Maliyet | Lisans |
|---|---|---|---|---|
| 1 | **HDRI gökyüzü + ortam ışığını HDRI'dan al + mekân başına 1 kez çekilen reflection probe** | Çok yüksek (oyuncak görünümünü en hızlı kıran şey) | 1–2 gün kod; render maliyeti çok düşük | Poly Haven CC0 [D] |
| 2 | **Kutulara CC0 PBR malzeme** (dünya koordinatlı/triplanar; albedo+normal+roughness+AO) | Çok yüksek | 2–4 gün; 1K–2K doku ile bellek makul | Poly Haven / ambientCG CC0 [D] |
| 3 | **Gölge + SSAO + HDR/ACES + sahne LUT'u + Bokeh DoF** (A04 Kademe 0, madde 1–3, 7) | Yüksek | Yarım–2 gün; kayıt yavaşlar ama sorun değil | Unity yerleşik |
| 4 | **2.5D "fotoğraf projeksiyonu"**: mevcut kutulara fotoğraf/HDRI'yı sabit bir "projektör kameradan" yansıtmak + karakter altına gölge yakalayıcı | Geniş planlarda çok yüksek (fotogerçek) | 3–5 gün shader + araç; kamera hareketi sınırlı | CC0 HDRI/fotoğraf; gölge alıcı örneği MIT [D] |
| 5 | Kahraman eşya: Poly Haven CC0 modeller, birkaç Megascans tarama (1K–2K, LOD düşük) | Orta-yüksek (yakın planda) | Disk/bellek; elle malzeme | CC0 / Fab Standard (A12: tek başına dağıtım yok) |
| 6 | Volumetrik sis/huzme: CristianQiu MIT paketi (6000.6'ya kadar, DX11 doğrulanmış) | Sahneye göre yüksek (şato, orman, korku) | Kurulum yarım gün; kare süresi artar | MIT [D] |
| 7 | Cilt: ön-entegre (pre-integrated) cilt Shader Graph'ı; ayrılabilir SSS (bentoBAUX, Unity 6 URP) denemesi | Orta (yüz yakın planda) | 1–3 gün | Kod "kredi vererek serbest" [D], depo lisansı yazılı değil [B] |
| 8 | Pişirilmiş lightmap/APV | Yüksek ama | **Yüksek iş**: mekânlar kodla kurulduğu için önce sahne/prefab'a çevrilmeli | Unity yerleşik |

**Lisans özeti:** Dağıtılabilir ve dertsiz çekirdek = **CC0** (Poly Haven, ambientCG, Quaternius, Kenney, KayKit). **CC-BY** (Sketchfab müze taramaları, Intel Sponza, Amazon Bistro) = atıf listesiyle serbest. **Fab Standard (Megascans)** = videoda serbest, uygulamada yalnız gömülü. **Unity Asset Store** paketleri (Viking Village vb.) = Standard EULA; kendi videon için kullanılabilir, ama içerik üretme aracını başkalarına dağıtırsan A12'deki 2.2.1.1(c) riski. **Fantasy Kingdom demo = ticari değil → kullanma.** **URP Defender karakteri = ticari değil.**

---

## 1. Ücretsiz ortam varlıkları

### 1.1 Ana tablo

| Kaynak | Ne var | Lisans: ticari · uygulama içinde dağıtım · atıf | Poligon / doku | URP uyumu | Vega 7 uygunluğu | Durum |
|---|---|---|---|---|---|---|
| **Poly Haven** (polyhaven.com) | HDRI (16K+; bazıları 20K), doku (en az 8K), modeller (mobilya, eşya, bitki) | CC0 · evet · gerekmez [D] | Doku 1K–8K seçilebilir; HDRI 1K–20K. Örnek `solitude_interior`: 1K…20K, EXR/HDR, 4K EXR 83 MB, 8K tonemaplı JPG 43 MB, **backplate + izleme dosyası** dahil [D] | Doku/HDRI doğrudan; roughness→smoothness çevirisi gerekir (A05) | İyi (1K–2K kullan) | [D] Kayıt/ödeme yok |
| **ambientCG** | PBR malzeme, HDRI, az model | CC0 · "ham dosyaları oyun gibi projeye koyabilirsin" · gerekmez (önerilir) [D] | 1K–8K | Aynı | İyi | [D] |
| **Megascans (Fab)** | 23.000+ tarama: 8.000+ nesne, 11.000+ malzeme | Fab/Epic Standard; 5 Kas 2026'da tümüyle ücretsiz, 12 Kas'a kadar tamamlandı; lisans değişmemiş görünüyor; "AI ile kullanım: evet" [D cgchannel]. Unreal dışı motor serbest, tek başına dağıtım yasak (A12 [S016]) | Tarama ağları ağır olabilir; LOD'lar var [K] | FBX/glTF; URP malzemesi elle | Orta: yalnız 1K–2K + düşük LOD | [D] haber; Epic hesabı kullanıcı işi |
| **Sketchfab CC-BY taramalar** | Gerçek şato/salon/kilise taramaları | Model başına. Örn. "The King's Hall" (Skokloster Şatosu): **CC BY 4.0**, indirilebilir, **1M üçgen** [D]. "Tavern Medieval House" (Simlion): CC BY 4.0, 15,8k üçgen, **NoAI notu** [D] | Müze taramaları 0,5–1M+ üçgen | glTF (ÇIKARIM) | Tarama: ağır → sadeleştir ya da 2.5D arka plan olarak kullan | [D] iki model; NC/ND/Editorial alma (A05) |
| **Intel Graphics Research Samples** | Yeni Sponza (fotogrametri, 4K doku) + perde/sarmaşık/mum eklentileri | Creative Commons Attribution (sürüm yazmıyor) [D] | Sponza Base 3,71 GB; poligon yazmıyor | GLTF/FBX/USD | **Ağır** (iGPU'da sadeleştirmeden zor) | [D] |
| **Amazon Lumberyard Bistro (NVIDIA ORCA)** | Fransız kafesi iç + dış sokak | CC-BY 4.0; atıf: "Amazon Lumberyard Bistro, Open Research Content Archive (ORCA)" [D] | İç 1,05M, dış 2,83M üçgen [D] | FBX | Ağır ama kayıt çevrimdışı: iç mekân denenebilir (ÇIKARIM) | [D] |
| **BlenderKit / BlendKit** | 60.000+ ücretsiz varlık (Ağu 2026 haberi) [K] | "Royalty Free" ve CC0 iki lisans [K]; lisans sayfası bu oturumda **açılamadı** (yönlendirme izni yanıtlanmadı) [B] | Değişken | Blender'dan FBX/glTF dışa aktarım | Değişken | [B] — kullanmadan önce lisans sayfası okunmalı |
| **Quaternius / Kenney / KayKit** | Low-poly kitler | CC0 (A05) | Çok hafif | İyi | Çok iyi | Projede 205 Quaternius dosyası var (A05). **Stilize**, gerçekçi değil |
| **Unity URP 3D Sample** | Terminal, gece Japon bahçesi, Oasis (fotogerçekçi çöl), Cockpit | Sayfada lisans yok [D]; Hub şablonu olarak geliyor | Mobil hedefli (iPhone 6S/8 alt sınır) [D] | URP | İyi (mobil için yapılmış) | Lisans [B]. Hub'dan "3D Sample Scenes (URP)" |
| **Viking Village** (Unity) | Gerçekçi Viking köyü | Standard Asset Store EULA [D] | 1,7 GB; v2.0.0, **18 Eyl 2026**, URP 6000.3 uyumlu [D] | URP | Orta | [D]. EULA → A12 notu |
| **Fantasy Kingdom (Unity 6 demo)** | Synty stilize krallık | **"Ticari olmayan sürüm"** [D Synty blog, 18 Eki 2024] | — | URP | — | **Kullanma** |
| **DEXSOFT Medieval Castle URP** | Gerçekçi şato | 39,99 $ (≈ bütçe sınırında), Standard EULA, 5,8 GB [D] | Yazmıyor | URP 6000.1 | Ağır (5,8 GB) | Ücretli; A12 riski |
| Asset Store "free medieval" paketleri | Çoğu low-poly | Standard EULA [K] | — | URP işaretlileri var | İyi | Stil çoğunlukla gerçekçi değil |

### 1.2 Lisans pratiği (ÇIKARIM)
- **Video çıktısı** (TikTok'a yüklenen mp4) için CC0, CC-BY, Fab Standard ve Asset Store EULA'nın hepsi genelde sorunsuz; CC-BY'de video açıklamasına/kanal sayfasına atıf satırı konmalı.
- **Uygulamayı başkasına dağıtmak** (içerik üretme aracı) ayrı durum: güvenli küme CC0 (+ CC-BY atıf dosyasıyla). Megascans ve Asset Store için A12'nin "yazılı teyit/temkin" önerisi geçerli.
- Her varlık için `LISANSLAR.md`'ye: kaynak URL, lisans, tarih, atıf metni (A05/A12 kuralı).

### 1.3 Gerçekçi ama hafif tutmak (ÇIKARIM; ölçülmedi)
- Kahraman eşya ≤50k üçgen, 2K doku; arka eşya 1K; tarama mimariyi doğrudan değil **arka plan katmanı** olarak (Bölüm 3) kullan.
- 1M üçgenlik taramalar Blender "Decimate" ile 100–200k'ya indirilip normal haritası pişirilebilir; bu, Blender kullanmayı gerektirir.
- Kayıt çevrimdışı olduğu için sınır FPS değil **paylaşılan bellek** (tümleşik GPU sistem RAM'i kullanır) ve sahne yükleme süresi.

---

## 2. Zayıf GPU'da URP ışık/görünüm teknikleri — etki ve maliyet

Puanlar: Etki 1–5, Maliyet (iş + kare süresi) 1–5 (5 = pahalı). Tümü ÇIKARIM; ölçülmedi. "Kod uyumu" = kodla kurulan mekâna uyar mı.

| Teknik | Etki | Maliyet | Kod uyumu | Not / kaynak |
|---|---|---|---|---|
| **HDRI skybox + ambient (Skybox kaynaklı SH) + reflection probe** | 5 | 1 | **Tam** | Gerçek ışık ortamı ve yansıma. Realtime probe'u mekân kurulunca **bir kez** `RenderProbe` ile çek; Unity "realtime probe pahalı, scriptle yenile" diyor [D Unity high-end graphics]. HDRI: Poly Haven CC0 |
| **PBR malzeme (CC0)** | 5 | 2 | Tam (triplanar/dünya UV) | Kutuların ölçeklenince UV germesi A04/A05'te not edildi |
| **Gölgeler (yumuşak, mesafe 10–20 m, 2048–4096)** | 5 | 2 | Tam | A04 Kademe 0 madde 1 |
| **SSAO** | 4 | 2 | Tam | Köşe/temas karartması; URP'de contact shadows **yok** [A04-S005]. Yerine: kısa gölge mesafesi + yüksek çözünürlüklü gölge haritası + SSAO "Direct Lighting Strength" (ÇIKARIM) |
| **HDR + ACES/Neutral + LUT** | 4 | 1 | Tam | Sahne başına LUT; 6.7'de AgX (6.7 hâlâ beta görünüyor: 6000.7.0b7 sürüm sayfası [K]) |
| **Bokeh DoF** | 4 | 1 | Tam | Arka planın sahteliğini (ve düşük çözünürlüklü HDRI'yı) gizler. Dikey videoda yakın planlarda en ucuz "sinema" hissi |
| **Mesafe sisi (Exp²) + sis/toz kartları + huzme kartları** | 3 | 1 | Tam | Volumetrik paketin ucuz yerine geçeni |
| **Volumetrik ışık/sis paketi** | 4 (şato/orman/korku) | 3 | Tam (gerçek zamanlı ışıkla) | **CristianQiu/Unity-URP-Volumetric-Light**: MIT, 2022.3 → **6000.6'ya kadar**, Render Graph, main/spot/point + gölge + APV, Forward+ önerilir, DX11 doğrulanmış, şeffaflarla karışmaz [D]. **sinnwrig/URP-Fog-Volumes**: MIT, kutu/küre/kapsül sis hacimleri, yarım/çeyrek çözünürlük, 2022.3'te test (Unity 6 [B]) [D]. **Mirza Beig Volumetric Fog Lite**: ücretsiz, Asset Store (EULA), mobilde iyi [D 80.lv, Mayıs 2024] |
| **Süper örnekleme (2160x3840 çiz → 1080x1920)** | 3 | 4 (süre) | Tam | Saç/kenar kırpışması azalır. Kayıt 4 kat yavaşlar |
| **Film grain / vignette** | 2 | 1 | Tam | Grain az: TikTok sıkıştırması bozar (A04) |
| **Pişirilmiş lightmap** | 5 | **5** | **Zayıf** | Kodla kurulan mekân önce kayıtlı sahne/prefab olmalı; çalışma anında prefab'a lightmap geri yüklemek için ek betik gerekir (yaygın topluluk yöntemi [B]). 6.6'da Progressive CPU "deprecated"; Compute Light Baker/Progressive GPU önerilir, GPU için ≥2 GB VRAM + OpenCL (A04-S010, S013). Vega 7'de Compute baker çalışıyor mu: [B] (Unity belge sayfası bu oturumda açılamadı). 6 çekirdekte küçük oda, düşük çözünürlük (10–20 texel/m): **onlarca dakika** mertebesi tahmini (ÇIKARIM, ölçüm yok). Unity 6 LTS'te bazı sistemlerde bake donması bildirildi, güncellemeyle düzeldiği söylendi [D forum] |
| **APV (Adaptive Probe Volumes)** | 4 | 4 | Zayıf (sabit geometri) | Karakterlere de dolaylı ışık verir; yine sahne olarak pişirme ister (A04 S3) |
| **Light probe (klasik)** | 3 | 3 | Zayıf | APV'nin eski hali; lightmap ile birlikte anlamlı |
| **Surface Cache GI (6.7+)** | 4 | 3 | **Tam** (bake'siz) | 6.7 LTS çıkınca ayrı kopyada dene (A04); şu an beta |
| **Cilt SSS** | 3 (yakın plan) | 3 | — | URP'de yerleşik yok. (a) **Ön-entegre cilt** (Penner) Shader Graph ile, ekran geçişi yok (A04). (b) **bentoBAUX "Separable SSS in Unity 6"**: URP + Render Graph, Jimenez yöntemi, 28 Nis 2026 (güncelleme 29 May 2026); "kredi vererek kullan"; depo lisansı yazılı değil; `stepCount/sampleCount` derleme hatası olabilir; tüm SSS nesneleri tek profil/özel katman [D]. (c) **URP Defender Character Demo** (Unity): ön-entegre cilt, Marschner + anizotropik saç, paralaks göz shader'ı içerir; **karakter yalnız eğitim/ticari olmayan**, teknoloji Asset Store EULA altında [D] → shader fikirleri incelenebilir, karakter kullanılmaz. (d) Asset Store "Pre-Integrated SSS Skin Shader URP" (ücret [B]) |
| **Saç shader'ı** | 3 | 2 | — | Kajiya-Kay anizotropik + alpha-to-coverage/dither; Defender demo örnek (EULA). Rocketbox saç düzeltme betiği (A04-S025). Ücretsiz MIT URP saç shader'ı doğrulanmadı [B] |

**Etki/maliyet sırası (ÇIKARIM):** HDRI+probe → PBR → gölge → SSAO → LUT/DoF → sis kartları → volumetrik → cilt → süper örnekleme → (6.7) Surface Cache GI → lightmap/APV.
Not: Lightmap en kaliteli "dolaylı ışık" yolu ama bu projede **en pahalısı**, çünkü mekân üretim modelini değiştirir. HDRI ambient + SSAO + iyi key/fill/rim ile "baked" görünümün büyük bölümü yakalanabilir (ÇIKARIM).

---

## 3. Matte painting / 2.5D arka plan: ucuz fotogerçeklik

Fikir: arka plan **gerçek fotoğraf/HDRI**, ön planda yalnız 3D karakter + birkaç gerçek eşya + zemin gölgesi. Dikey video dar bir dilim gösterdiği için çok etkili (ÇIKARIM).

### 3.1 Yöntemler

| Yöntem | Nasıl | Artı | Eksi | Kaynak |
|---|---|---|---|---|
| **A. HDRI arka plan + gölge yakalayıcı zemin** | Skybox = HDRI (arka plan + ışık + yansıma); karakter altına görünmez "shadow receiver" düzlem | En kolay; ışık/arka plan otomatik uyumlu | HDRI zemini "sonsuz uzakta": karakter kayıyormuş gibi görünebilir; kamera yüksekliği HDRI çekim yüksekliğine (~1,5–2 m) yakın olmalı | Poly Haven CC0 [D]; **Cyanilux URP_ShaderGraphCustomLighting**: MIT, URP 17.1+/6000.1+, içinde **"Shadow Receiver" örneği** (şeffaf, gölge alan, renk ayarlı) [D] |
| **B. Zemine yansıtılmış HDRI (ground projection)** | Skybox shader'ı alt yarıyı sanal bir zemin düzlemine/kubbeye yansıtır | Ayak-zemin ilişkisi düzelir | Hazır ücretsiz URP sürümü doğrulanmadı [B]; yazması kolay (tek shader) (ÇIKARIM) | Babylon/Blender'da yaygın yöntem [K] |
| **C. Kamera projeksiyonu (camera projection mapping)** | Fotoğraf/boyama, sabit bir "projektör kamera" matrisiyle **mevcut kutu geometrisine** yansıtılır; UV gerekmez | **Mevcut kodla kurulan kutular "taşıyıcı geometri" olur**; az kamera hareketinde gerçek paralaks; gölge/ışık 3D'de alınabilir | Kamera projektörden çok uzaklaşınca (≈ 10–20°) gerilme/boşluk görünür (ÇIKARIM); fotoğraftaki ışık yönü key ile eşleşmeli | Unity blogu, *Warhammer 40,000: Warpforge*: Unity'de ilkel kutularla yerleşim → FBX ile Blender → üstüne boyama (katmanlı) → her katmanı basit geometriye kamera projeksiyonu → Unity'de atlas; kamera çoğunlukla sabit, açılış uçuşu projeksiyon sınırına göre seçilmiş [D]. **fSpy** (fotoğraftan kamera eşleme) + Blender projeksiyon iş akışı [K BlenderNation] |
| **D. Katmanlı kartlar (2.5D paralaks)** | Ön/orta/arka plan için alfa kesilmiş fotoğraf düzlemleri | Çok ucuz; kayan kamerada derinlik | Yanal kamera hareketi sınırlı; kenarlar dikkat ister | Genel teknik (ÇIKARIM) |
| **E. Gaussian splat ortam** | Gerçek mekân taraması nokta bulutu olarak çizilir | Gerçekten fotogerçek, 3D gezinilebilir | **aras-p UnityGaussianSplatting**: MIT, URP için Unity 6+, **yalnız D3D12/Vulkan (DX11 çalışmaz)**, ~104 bayt/splat (3M splat ≈ 300 MB) [D radiancefields]; geliştirici büyük geliştirme planlamıyor; karakterle ışık/gölge etkileşimi yok; Vega 7'de hız [B] | Deney olarak (A05 "Marble") |

### 3.2 Çözünürlük hesabı (ÇIKARIM)
Dikey kadrajda dikey FOV 60° ise yatay FOV = 2·atan(tan30°·9/16) ≈ **36°**. 1080 piksel genişlik için 360°'lik HDRI'da gereken genişlik ≈ 1080·360/36 ≈ **10.800 px**.
- 8K HDRI → kadrajda ~800 px (biraz yumuşak; DoF ile gizlenir).
- 16K HDRI → ~1.600 px (keskin). Unity doku sınırı 16384 (genel bilgi).
- Pratik: **arka plan için 16K tonemaplı JPG/PNG (LDR)**, ışık/yansıma için **1K–2K EXR**. 16K EXR'yi doğrudan yüklemek tümleşik GPU'nun paylaşımlı belleğini zorlar.
- Daha dar FOV (teleobjektif, 30–35°) daha da yüksek çözünürlük ister; kamera projeksiyonunda tek yüksek çözünürlüklü fotoğraf (ör. 6000 px) bu sorunu çözer.

### 3.3 Fotoğraf ile 3D karakteri eşleştirme kuralları (ÇIKARIM)
1. **Ufuk/kamera yüksekliği**: Unity kamerası HDRI/fotoğrafın çekim yüksekliğinde; ufuk çizgisi aynı yerde.
2. **Lens**: fSpy ile fotoğrafın FOV'u bulunur; Unity kamerasına aynı FOV.
3. **Işık yönü ve rengi**: key ışığı fotoğraftaki güneş/pencere yönünden; renk sıcaklığı HDRI'dan örneklenir.
4. **Temas gölgesi**: shadow receiver + SSAO; yoksa karakter "yapıştırılmış" görünür.
5. **Ortak son işlem**: aynı LUT, grain, hafif DoF/bloom her iki katmanı birleştirir. Arka plana hafif bulanıklık + karakterde odak = sinema hissi.
6. **Kamera hareketi**: projeksiyonlu sahnede küçük dolly/pan; büyük yörünge yok. Kurgu (A08) zaten kesmeye dayalı.

### 3.4 Arka plan kaynakları
- **Poly Haven HDRI** (CC0, 16–20K, bazılarında backplate + izleme dosyası) [D]; iç mekân, sokak, orman, gece kategorileri var [K].
- **ambientCG HDRI** (CC0) [D].
- **Sketchfab CC-BY taramalar**: ağır olsa da sabit kameradan bir kez render edilip **arka plan görüntüsü** yapılabilir (1M üçgenlik "King's Hall" bir kere render → 2.5D kart). Atıf gerekir [D].
- Fotoğraf siteleri (Unsplash, Pexels, Wikimedia Commons CC0/PD): lisanslar bu oturumda okunmadı [B]. Wikimedia'da yalnız CC0/PD olanlar seçilmeli; Unsplash/Pexels CC0 değildir (genel bilgi, [B]).

---

## 4. Önerilen sıra (ÇIKARIM; test edilmedi)

1. **Hafta 1 (paket yok):** HDRI skybox + ambient + bir kez çekilen reflection probe; gölge; SSAO; HDR/ACES; 3–4 sahne LUT'u; Bokeh DoF. Varlık: 6–8 Poly Haven HDRI (gündüz dış, gün batımı, gece, iç gündüz, iç gece, orman, şato iç).
2. **Hafta 2:** Kutulara triplanar PBR (10–15 CC0 malzeme: taş, ahşap, sıva, toprak, çim, kaldırım, metal). Quaternius eşyalarının ad eşlemesini düzelt (A05 bulgusu).
3. **Hafta 3:** 2.5D modu: "Projeksiyon arka plan" katmanı (yöntem A + C): mekân tarifinde `arka_plan: <hdri/fotoğraf>` + projektör kamera + shadow receiver. Önce 1 mekânda (sokak veya şato salonu) `onizleme_goruntu` ile karşılaştır.
4. **Hafta 4:** Volumetrik (CristianQiu, paket onayı) yalnız şato/orman/korku; cilt için ön-entegre Shader Graph.
5. **Sonra:** Unity 6.7 LTS → Surface Cache GI/GTAO/AgX denemesi. Lightmap'i yalnız sabit, sık kullanılan 2–3 mekân için düşün.

---

## 5. Riskler ve açık sorular
- **Işık uyumsuzluğu** (fotoğraf arka plan ↔ 3D karakter) en büyük 2.5D riski; kural listesi 3.3.
- **Bellek**: 16K arka plan + 2K PBR + Rocketbox; tümleşik GPU paylaşımlı RAM. Ölçülmeli.
- **Volumetrik paketin 6.6/URP 17.6 ile çalışması** geliştirici beyanı; 6.7'ye geçince destek belirsiz.
- **Gaussian splat** projenin DX11 kullandığı durumda çalışmaz (D3D12/Vulkan şart) [D].
- Doğrulanamayanlar: BlenderKit lisans sayfası (açılamadı), Unity 6.6 lightmapper belge sayfaları (yönlendirme açılamadı), URP 3D Sample varlık lisansı, Vega 7'de Compute Light Baker, URP için hazır ücretsiz HDRI ground-projection shader'ı, Unsplash/Pexels lisansları.

---

## 6. Kaynaklar (erişim: 2026-11-28)
- Poly Haven ana sayfa (CC0, 16K+ HDRI, 8K+ doku) — https://polyhaven.com [D]
- Poly Haven `solitude_interior` (20K, backplate) — https://polyhaven.com/a/solitude_interior [D]
- ambientCG lisans — https://docs.ambientcg.com/license [D]
- Megascans ücretsiz (5 Kas 2026) — https://cgchannel.com/2026/11/epic-has-made-megascans-free-again-for-good-this-time [D]; https://digitalproduction.com/2026/11/13/quixel-megascans-free-again [K]
- BlenderKit lisans — https://www.blenderkit.com/docs/licenses/ → blendkit.com (açılamadı) [B]; 60.000 ücretsiz varlık haberi https://www.cgchannel.com/2026/08/get-60000-free-3d-assets-for-maya-godot-and-rhino-from-blendkit/ [K]
- Sketchfab "The King's Hall" — https://sketchfab.com/3d-models/the-king-s-hall-d18155613363445b9b68c0c67196d98d [D]
- Sketchfab "Tavern Medieval House" — https://sketchfab.com/3d-models/tavern-medieval-house-eda03fe47db1450a93a4e2102e4caf65 [D]
- Intel Graphics Research Samples — https://www.intel.com/content/www/us/en/developer/topic-technology/graphics-research/samples.html [D]
- NVIDIA ORCA Amazon Lumberyard Bistro — https://developer.nvidia.com/orca/amazon-lumberyard-bistro [D]
- Unity URP 3D Sample — https://unity.com/demos/urp-3d-sample [D]
- Viking Village — https://assetstore.unity.com/packages/essentials/tutorialprojects/viking-village-29140 [D]
- Fantasy Kingdom (ticari olmayan) — https://syntystore.com/blogs/blog/fantasy-kingdom-demo-in-unity-6 [D]
- DEXSOFT Medieval Castle URP — https://assetstore-fallback.unity.com/packages/3d/environments/historic/medieval-castle-urp-308772 [D]
- Unity high-end graphics optimizasyon rehberi — https://unity.com/how-to/performance-optimization-high-end-graphics [D]
- Unity 6 LTS bake donması forumu — https://discussions.unity.com/t/baking-global-illumination-went-from-2-minutes-to-5-estimated-hours-when-updating-to-unity-6-lts/1541553 [D]
- Unity 6.7 beta sürüm sayfası — https://unity.com/releases/editor/beta/6000.7.0b7 [K]
- CristianQiu URP Volumetric Light — https://github.com/CristianQiu/Unity-URP-Volumetric-Light [D]
- sinnwrig URP Fog Volumes — https://github.com/sinnwrig/URP-Fog-Volumes [D]
- Mirza Beig Volumetric Fog (ücretsiz) — https://80.lv/articles/grab-mirza-beig-s-free-volumetric-fog-asset-for-unity-urp/ [D]
- bentoBAUX Separable SSS Unity 6 — https://bentobaux.github.io/posts/separable-subsurface-scattering-in-unity-6/ [D]
- URP Defender Character Demo — https://github.com/UnityTechnologies/URP-Defender-Character-Demo [D]
- Johannes Deml Human Skin Shader (lisans/URP yazmıyor) — https://github.com/JohannesDeml/Unity-Human-Skin-Shader-PC [D]
- Cyanilux Shader Graph Custom Lighting (Shadow Receiver örneği) — https://github.com/Cyanilux/URP_ShaderGraphCustomLighting [D]
- Warhammer 40,000: Warpforge kamera projeksiyonu — https://unity.com/en/blog/games/stunning-scenarios-in-warhammer-40000-warpforge [D]
- fSpy projeksiyon iş akışı — https://bazaar.blendernation.com/listing/projection-mapping-with-fspy/ [K]
- Unity Gaussian Splatting (aras-p) — https://radiancefields.com/platforms/unity-gaussian-splatting [D]
- Shadow Catcher and Skybox for URP (25 $, ücretli alternatif) — https://assetstore.unity.com/packages/3d/environments/shadow-catcher-and-skybox-for-urp-376420 [D]
