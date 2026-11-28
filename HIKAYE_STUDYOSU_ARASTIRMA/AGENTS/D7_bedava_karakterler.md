# D7 — Bedava gerçekçi 3D insan kaynakları (Unity URP + Vega iGPU için)

Tarih: 2026-11-28 · Yöntem: yalnızca web araştırması (~28 arama + sayfa okumaları). **Hiçbir şey indirilmedi veya test edilmedi.** Lisans özetleri sayfalardan alındı. Kullanmadan önce asıl lisans metnini mutlaka okuyun; bu belge hukuki görüş değildir.
Ön okuma: `D4_ucretsiz_yollar.md` (MetaHuman dışa aktarma, MPFB 2, CC denemesi, Daz zaten orada; burada tekrar edilmedi, yalnız yeni bilgi eklendi).

Bağlam: Unity 6 URP, tümleşik AMD Vega, 1080x1920 dikey video, şu an Microsoft Rocketbox (MIT) karakterleri. Bütçe 0. AI video üretici yok.
**Önemli avantaj:** Video çevrimdışı kaydediliyor (Recorder, sabit kare hızı). Gerçek zamanlı 30 FPS şart değil; ağır karakter de kayıtta yavaş ama düzgün çıkar. Asıl sınır **VRAM/RAM ve yükleme**, FPS değil.

---

## 0. Kısa sonuç: ilk 5 aday

| Sıra | Kaynak | Gerçekçilik | Ticari / sosyal video | Uygulamada dağıtım | Humanoid | Yüz blendshape | iGPU |
|---|---|---|---|---|---|---|---|
| 1 | **Renderpeople ücretsiz "Rigged People"** (taranmış, 4–6 kişi) | Çok yüksek (gerçek insan taraması, kıyafet dokuda) | **Evet** (video/animasyon açıkça serbest) | Oyunlarda serbest, ama modelin **dışarı çıkarılabilir** halde dağıtılması yasak | Evet (iskelet; Unity formatı var) | **Yok** (yüz sabit) | İyi: 10–15k poligon; 8K doku → 2K'ya indir |
| 2 | **MPFB 2.0.16** (Blender, çıktısı CC0) + yeni **ARKit uyumlu 52 "face unit"** | Orta–iyi | **Evet, CC0** | **Evet, CC0** | Evet (Rigify/standart rig → FBX) | **Var** (2026-06'dan beri, ARKit uyumlu, "cilalanması gerekebilir") | İyi (poligon ayarlanabilir) |
| 3 | **Reallusion ücretsiz CC karakter tabanları** (5 adet, 3'ü gerçekçi) | Orta–iyi (çıplak taban, saçsız, kıyafetsiz) | **Evet** (film, animasyon, oyun) | Evet; **türev karakteri mağazada satmak yasak** | Evet (CC iskeleti, Unity'de Humanoid'e eşlenir) | **Var: 150+ yüz morph'u** (ARKit adları değil) | Orta |
| 4 | **Mixamo karakterleri** (Adobe, ücretsiz hesap) | Orta (oyun tarzı, Rocketbox'a yakın/biraz iyi) | **Evet** (telifsiz) | Evet; ham dosyayı tek başına dağıtmak yasak | Evet | Çoğunda yok | Çok iyi |
| 5 | **Rocketbox'u iyileştirmek**: HeadBox blendshape'leri + daha iyi URP cilt/saç/ışık | Mevcut + | MIT | MIT | Zaten var | HeadBox: 15 visem + 48 FACS | Çok iyi |

**Önce dene:** Renderpeople ücretsiz rigli birkaç kişi (en gerçekçi, hemen Unity'ye girer, yüz konuşmaz ama arka plan / geniş plan / sırt çekimleri için mükemmel) **+** konuşan karakterler için MPFB 2.0.16 (CC0, ARKit yüz). Hiçbiri MetaHuman seviyesinde değil; MetaHuman için D4 §5.1 (bir kerelik GPU'lu PC) hâlâ en yüksek kalite yolu.

---

## 1. Taranmış (scan) gerçek insanlar

### 1.1 Renderpeople — ücretsiz modeller ★
- URL: https://renderpeople.com/free-3d-model · Lisans: https://renderpeople.com/?p=3072 (erişim 2026-11-28)
- Ücretsiz türler: Posed (pozlu, rigsiz), **Rigged People** (10–15k poligon, retopolu dörtgen, 8K diffuse/normal/gloss/alpha, iskelet + kontrol rigi; formatlar: 3ds Max, C4D, Maya, UE4, **Unity**, FBX), **Animated People** (temizlenmiş mocap iskelete pişirilmiş, Unity formatı var), 4D People (hacimsel sekans, Unity'ye FBX olarak gelmez).
- Sayfa: "Free Download, Free Commercial Use, No Registration Required"; ücretsizler ücretli modellerle **aynı şartlara** tabi (§2.5).
- **Lisans özeti:**
  - §4.1a: hareketsiz görüntü ve **animasyon/video üretimi** ticari olarak serbest → TikTok videosu uygun.
  - §4.1b: gerçek zamanlı render (AR/VR/XR) serbest; §4.2c: bilgisayar/video oyunları gömme kısıtından **muaf**.
  - §4.2b / §4.3b: modeli yeniden dağıtmak, satmak, **üçüncü kişilerin kolayca indirip çıkarabileceği şekilde** sunmak yasak. → Hikaye Stüdyosu'nu (Unity projesini) başkalarıyla paylaşırsanız Renderpeople dosyalarını içine koymayın.
  - §4.2a: veri seti / yapay zeka eğitimi yasak. §4.2d: içinde Renderpeople olan video/görüntüyü **stok sitelerinde satmak** yasak (izinsiz).
  - §4.3a: pornografik, dolandırıcı, **karalayıcı/aşağılayıcı** bağlamda kullanım yasak → hikayede "kötü adam" yapmak sorun değil gibi görünür ama gerçek kişinin taraması olduğu için hakaret içerikli sahnelerden kaçının.
- Rig: gövde + parmaklar; Unity'de Humanoid'e eşlenmeli (Renderpeople iskeleti genelde 3ds Max Biped tarzı; avatar eşleme elle kontrol edilmeli — doğrulanmadı).
- **Yüz: blendshape yok** (sayfada yüz rigi geçmiyor). Ağız/göz konuşmaz → sırt, geniş plan, figüran, "dinleyen" karakter için.
- iGPU: 10–15k poligon çok hafif; 8K dokuları Unity'de Max Size 2048 yapın.
- Gerçekçilik: listedeki **en yüksek** (fotogrametri). Kıyafet ve saç dokuya gömülü (ayrı değiştirilemez).
- Not: Eski ücretsiz rigli isimler (Eric, Carla, Claudia, Mei, Rosy vb.) CG Channel'da duyurulmuş (2017–2019): https://www.cgchannel.com/2019/07/download-four-new-free-renderpeople-stock-3d-characters/

### 1.2 AXYZ design (anima / Metropoly)
- URL: https://secure.axyz-design.com/en/faq
- "Free demos" filtresiyle örnek modeller var; **lisansı sayfada yazmıyor**.
- Standart lisans: render görüntü/animasyon, video, reklam serbest. **Etkileşimli oyun/uygulama için "Extended"**, yazılımınıza gömmek için "Distribution" lisansı gerekir. Unity'de rigli modeller FBX ile içe alınır.
- Sonuç: Video için muhtemelen uygun ama ücretsiz demoların lisansı belirsiz → **ikinci tercih**, önce lisansı yazılı teyit edin.

### 1.3 3D Scan Store (Ten24) ücretsiz kafa / ifade seti
- URL: https://www.3dscanstore.com/blog/multi-expression-basemesh
- Ücretsiz "multi expression base mesh": ZTL (ZBrush 2022+) + OBJ ifade setleri (FACS benzeri). **Lisans: yalnız kişisel / ticari olmayan**; ticari için e-postayla izin gerekir; indirme için e-posta listesine katılmak şart.
- Sonuç: Para kazanan TikTok hesabı için **uygun değil**. Ayrıca ZBrush ister.

### 1.4 Sketchfab / Free3D / CGTrader "ücretsiz scan"
- Sketchfab'deki "human man scan …MBody" serisi (Scanlab) **ücretli**, ücretsiz değil (https://sketchfab.com/3d-models/human-man-scan-270mbody-b54de36e085345d6b0ed51390fed5e77).
- Sketchfab CC0 filtresiyle tek tük taranmış insan bulunur ama genelde rigsiz, dağınık topolojili, yüksek poligon. Her model için lisans ayrı (CC-BY atıf ister, **CC-BY-NC ticari değil**).
- CGTrader'da "unity digital human rh011/rh013" gibi ücretsiz ilanlar görüldü (https://www.cgtrader.com/free-3d-models/character/woman/unity-digital-human-rh013) ama sayfa içeriği okunamadı → lisans/kalite **doğrulanmadı**.
- **Poly Haven / ambientCG:** insan modeli yok (doku/HDRI/obje odaklı). Twinbru kumaş sitesidir, insan modeli yok.

---

## 2. Karakter üreticiler (kendi karakterini yap)

### 2.1 MPFB 2 (MakeHuman Blender eklentisi) — 2.0.16 yenilikleri ★
- URL: https://static.makehumancommunity.org/mpfb/releases/release_2016.html (2026-06-13) · https://extensions.blender.org/add-ons/mpfb/
- Lisans: kod GPLv3, **üretilen içerik CC0** → ticari, uygulamada dağıtım, her şey serbest (CG Channel 2025-03).
- **Yeni (2.0.16):** "Expressions" araç seti — **ARKit uyumlu 52 face unit** (faceunits01 varlık paketi gerekir); MakeExpression ile ifade üretip JSON kaydetme; ifade kütüphanesi. Notlar: "yüz şekilleri hâlâ cilalanmaya ihtiyaç duyabilir." Rigify yeniden düzenlendi, "full rig" tek adımda.
- Rig → FBX → Unity Humanoid: standart. 52 face unit'in shape key olarak FBX'e çıkıp çıkmadığı **test edilmeli** (sürümde "shape keys ile ifade" deniyor, bu umut verici).
- Gerçekçilik: orta–iyi; cilt dokuları ve saç MakeHuman varlıkları (saçlar zayıf). Saç için ayrı kart saç gerekir (bkz. §5).
- iGPU: çok iyi (proxy/düşük poligon seçenekleri).

### 2.2 Reallusion ücretsiz CC karakter tabanları ★
- URL: https://www.reallusion.com/character-creator/free-3d-character-base.html · CG Channel 2024-03-20: https://www.cgchannel.com/2024/03/get-reallusions-five-great-free-animation-ready-3d-characters
- 5 karakter: gerçekçi erkek, kadın, cinsiyetsiz + 2 çizgi film. FBX (tam iskelet + **150+ yüz morph'u**, dudak senkronu için), OBJ, ZTL. Ücretsiz Reallusion hesabı gerekir.
- Lisans: "kişisel ve ticari amaçlar" (film, animasyon, **oyun**, eğitim). EULA: bu tabanlardan türetilmiş karakterleri **online mağazalarda satmak yasak**. CC programına sahip olmak gerekmiyor görünüyor. (EULA §5 ayrıntısı okunmadı.)
- Eksik: saç, kıyafet, (muhtemelen) iyi göz/kaş. Taban model → Blender'da kıyafet/saç eklemek gerekir.
- Morph adları ARKit değil; Unity'de projenin dudak senkronuna eşleme tablosu gerekir.
- **Uyarı:** CC5 denemesiyle düzenleyip dışa aktarmak filigran/ticari yasak getirir (D4). Yalnız ücretsiz FBX'i doğrudan kullanın.

### 2.3 Mixamo (Adobe)
- URL: https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html
- Karakter ve animasyonlar "kişisel, ticari ve kâr amacı gütmeyen projelerde telifsiz". Ücretsiz Adobe ID. Ham dosyaları tek başına (paket olarak) dağıtmak yasak (Adobe forum SSS).
- Gerçekçilik: orta (2015 dönemi oyun karakterleri). Rocketbox'tan çok farklı değil. **Asıl değeri: ücretsiz animasyonlar** + kendi modelinize otomatik rig.

### 2.4 Selfie'den avatar servisleri (durum 2026)
- **Ready Player Me: KAPANDI** — Netflix Aralık 2025'te satın aldı, hizmet 31 Ocak 2026'da kapandı; yalnız önceden indirilmiş GLB'ler çalışır (avatarsdk.com, 2026-08-31 — rakip firmanın yazısı).
- **Union Avatars:** 2026 Temmuz'dan beri çevrimdışı (aynı kaynak).
- **Avaturn:** yaşıyor, stilize görünüm; Pro $800/ay. Ücretsiz katman doğrulanmadı.
- **MetaPerson (Avatar SDK):** gerçekçi, tek selfie'den; "ilk avatar ücretsiz", sonrası kredi; API denemesi 7 gün; GLB/glTF/FBX; ticari kullanım şartı sayfada yazmıyor. Ayrıca **yapay zeka ile 3D üretimi** — kullanıcının "AI video yok" kuralının dışında ama yine de AI üretim; dikkat. https://avatarsdk.com/pricing-cloud/
- **Genies:** ücretsiz Unity SDK ama stilize. **VRoid:** ücretsiz, anime — gerçekçi değil.
- **Hyper3D / benzeri "ücretsiz AI 3D insan" siteleri:** AI üretim, gerçekçi insan kalitesi zayıf, lisans karışık → önerilmez.

### 2.5 Anny (NAVER LABS Europe) — bilgi notu
- https://europe.naverlabs.com/blog/anny-a-free-to-use-3d-human-parametric-model-for-all-ages/ (2025-11-06)
- **Apache 2.0**, MakeHuman tabanlı parametrik gövde (163 kemik, 564 şekil). Araştırma/sentetik veri için; doku ve yüz ifadesi yok. Video karakteri olarak doğrudan işe yaramaz; MPFB daha pratik.

---

## 3. Unity'nin kendi dijital insanları

### 3.1 Enemies (2023) örnek projesi
- https://assetstore.unity.com/packages/essentials/tutorial-projects/enemies-248975 — 3,3 GB, **yalnız HDRP** (URP ve Built-in "Not compatible"), Unity 2023.1/2023.2, **"Non standard EULA"**. Sürüm 1.0, 2026-09-01.
- Vega iGPU + HDRP + 4D yüz verisi → pratik değil. Karakteri URP'ye taşımak: cilt/göz/saç shader'ları HDRP'ye özgü; yeniden yazmak gerekir. Lisans metni okunmadı → karakteri kendi videolarınızda kullanmak **belirsiz**.

### 3.2 Digital Human paketi (com.unity.demoteam.digital-human, Heretic teknolojisi)
- README (ayna): HDRP 10.9+ (yeni cilt/göz için HDRP 14 + Unity 2022.2), skin attachment, tension (kırışıklık), 4D klip. **URP desteği yok.** https://github.com/ZorroYanBo/com.unity.demoteam.digital-human
- Sonuç: Bizim proje (URP + iGPU) için **uygun değil**.

---

## 4. Yüz rigi / ARKit 52 blendshape kaynakları (AI'sız)

| Kaynak | Ne | Lisans | Not |
|---|---|---|---|
| **MPFB 2.0.16 face units** | 52 ARKit uyumlu yüz birimi | CC0 içerik | En temiz yol (bkz. §2.1) |
| **ARKit Blendshape Tools** (Blender eklentisi, v0.7.5, 2026-03-14) https://extensions.blender.org/add-ons/arkit-blendshape-tools/ | 52 ARKit + 18 ek shape key **yuvası** oluşturur, sol/sağ böler, düzenler | GPLv3 (araç; çıktınız sizin) | Şekilleri **otomatik heykellemez**; elle yapılır |
| **HeadBox** (Microsoft Research / UCL, IEEE VR 2022) https://www.microsoft.com/en-us/research/?p=843265 · repo: github.com/openVRlab/Headbox | Rocketbox kafaları için 15 visem + 48 FACS + 30 Vive blendshape; Maya'da kemikten blendshape üretip diğer avatarlara aktarma aracı | Repo lisansı okunamadı (Rocketbox MIT) | **Mevcut Rocketbox karakterlerinize en ucuz yükseltme** — yüz ifadesi ve dudak. Maya gerekir (araç kısmı için); blendshape'ler "orijinal kütüphaneyle yayımlandı" deniyor → Rocketbox deposunda kontrol edin |
| **ICT Face Model Light** (USC) https://vgl.ict.usc.edu/Data/ICTFaceModel | 53 ifade blendshape'i + 100 kimlik modu | Ticari yüksek çözünürlük için satın alma; Light lisansı okunmadı | Dokusuz; araştırma odaklı |
| "MetaHuman Head 52 blendshapes (Free)" (Gumroad) | MetaHuman'dan çıkarılmış ARKit kafa | "Yalnızca eğitim amaçlı" | **Videoda kullanmayın** (lisans belirsiz) |
| SideFX Facial Capture örneği | Polywink örnek karakteri | **CC BY-ND 4.0** (türev yasak) | Uygun değil |
| Ücretli (bilgi): Faceit, FaceKeys, RealFace Rig (Superhive) | Otomatik ARKit shape key | Ücretli | Bütçe dışı |

**Yüz animasyonu verisi (AI'sız):**
- iPhone (Face ID'li) varsa: **Unity Face Capture** uygulaması (ücretsiz, Apple App Store) veya Epic Live Link Face → ARKit 52 eğrisi kaydı. iPhone yoksa bu yol kapalı.
- Ücretsiz, ticari kullanılabilir hazır "ARKit 52 animasyon veri seti" **bulunamadı** (bulunanlar araştırma lisanslı).
- Pratik: Mevcut dudak senkronu (sese göre ağız) + elle hazırlanmış 10–15 ifade pozu (gülme, şaşırma, öfke…) + rastgele göz kırpma/bakış. Bu, AI'sız ve iGPU'da bedava.

---

## 5. Saç ve cilt (URP, bedava)

### Saç kartları
- Ücretsiz, ticari, kaliteli **kart saç** kaynağı bu araştırmada **bulunamadı**; CGTrader/TurboSquid'deki "realistic hair cards" ilanları çoğunlukla ücretli. MakeHuman/MPFB saçları CC0 ama zayıf.
- Yol: Blender'da (ücretsiz) saç kartı yapmak veya Blender "hair curves → mesh" ile kart üretmek; URP'de alfa-test + iki taraflı + basit anizotropik vurgulu Shader Graph. (Test edilmedi.)

### Cilt
- **bentoBAUX — Separable Subsurface Scattering in Unity 6** (URP Renderer Feature, RenderGraph; yazı 2026-04-28, güncelleme 2026-05-29): https://bentobaux.github.io/posts/separable-subsurface-scattering-in-unity-6/ · repo: https://github.com/bentoBAUX/Separable-Subsurface-Scattering-in-Unity-6 — yazı CC BY 4.0, "kodumu atıfla kullanın"; repo lisansı ayrıca kontrol edilmeli. Birkaç tam ekran geçiş + 32 örnek bulanıklık → iGPU'da pahalı ama **çevrimdışı kayıtta sorun değil**. Sınırlar: tek SSS profili, ayrı katman, sis yok.
- JohannesDeml/Unity-Human-Skin-Shader-PC: **Built-in forward** (URP değil), Unity 2021 test → bizim için uygun değil.
- Ücretli (bilgi): Lux URP Essentials (~$12,5–25), Pre-Integrated SSS Skin URP ($5,49), PIDI Skin & Eye.
- Ucuz alternatif: URP Lit + iyi normal/roughness haritası + Shader Graph'ta "wrap lighting" / ön-entegre (pre-integrated) eğri dokusu + hafif kırmızı geçirgenlik; post-process'te ACES tonemap, hafif film grain, alan derinliği. Gerçekçiliği shader'dan çok **ışık ve kamera** artırır.

---

## 6. Ülke/dil taramaları
- **Çince** (免费 写实 人物 模型 商用 / 扫描): sonuçlar TurboSquid Çince sayfaları, AI üretimli Hyper3D ve ücretli UE forumu ilanları. Ücretsiz-ticari tarama kaynağı çıkmadı.
- **Japonca** (無料 リアル 人物 3Dモデル 商用可): CGTrader "unity humanoid model / unity digital human" serisi (bazıları ücretsiz) — lisans doğrulanmadı.
- **Korece, Rusça, Türkçe:** TurboSquid/CGTrader yerelleştirilmiş sayfaları, Free3D (genelde kişisel kullanım), Cinevva rehberi (CC0 ama stilize: KayKit, Quaternius, 100 Avatars, Kenney). Gerçekçi + ücretsiz + ticari yeni kaynak çıkmadı.
- Sonuç: Dünyada gerçekçi + bedava + ticari kaynak az; ana adaylar yukarıdakiler.

---

## 7. Fab (Epic) notu
- Fab'da ücretsiz içerik ya **CC-BY** ya da **Standard License**. Standard: geliri $100k altı için Personal katman (https://dev.epicgames.com/documentation/fab/licenses-and-pricing-in-fab). Unity'de kullanım ve Megascans istisnası bu sayfada yazmıyor → fab.com/eula okunmalı. İki haftada bir ücretsiz paketler veriliyor (80.lv); arada gerçekçi karakter çıkarsa değerlendirilebilir. **Doğrulanmadı.**

---

## 8. Önerilen deneme sırası
1. **Renderpeople Rigged + Animated ücretsiz modelleri** indir (kayıtsız). Unity paketi → URP materyale çevir → Humanoid eşle → bir sahnede Rocketbox'la yan yana `onizleme_goruntu` al. Konuşmayan rollerde kullan. Proje paylaşılacaksa bu dosyaları dışarıda tut.
2. **MPFB 2.0.16 + faceunits01**: 2 karakter (kadın/erkek) yap, 52 face unit'i shape key olarak FBX'e çıkar, Unity'de projenin dudak senkronuna bağla. CC0 → hiçbir lisans endişesi yok.
3. **Reallusion ücretsiz taban**: gerçekçi yüz + 150 morph; Blender'da saç/kıyafet ekle. Türev karakteri satmayın.
4. **Rocketbox + HeadBox + bentoBAUX SSS + ışık/post-process**: mevcut karakterleri en az işle tazeleme.
5. MetaHuman (D4 §5.1) — tek seferlik GPU'lu PC bulunursa en yüksek kalite.

## 9. Belirsizlikler
- Renderpeople Unity paketlerinin URP'de hazır mı yoksa Built-in materyalli mi geldiği; iskeletin Humanoid'e sorunsuz eşlenip eşlenmediği.
- MPFB face units'in FBX dışa aktarımında shape key olarak korunup korunmadığı ve kalitesi.
- Reallusion EULA §5 ayrıntıları (uygulamada dağıtım).
- HeadBox repo lisansı ve blendshape'li FBX'lerin Rocketbox deposunda olup olmadığı.
- AXYZ ücretsiz demolarının lisansı; Fab Standard License'ın Unity'de kullanımı.
