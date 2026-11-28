# D9: MetaHuman'ı UE 5.8'den Unity 6 URP'ye taşıma hattı (Vega iGPU)

Tarih: 2026-11-28. Yöntem: yalnızca web araştırması (yaklaşık 26 arama ve 35 sayfa okuması). **Hiçbir şey indirilmedi, kurulmadı ya da denenmedi.**
Ön okuma: D4 §3 (lisans özeti, Character DNA, MPFB), D6 (UE'nin bu PC'de açılmaması), D7 (bedava karakterler, bentoBAUX SSS). Bunlar burada tekrarlanmadı, yalnızca atıf yapıldı.

Etiketler:
- **[D]**: resmi sayfada ya da güncel kaynakta açıkça yazıyor.
- **[KD]**: kısmen doğrulandı (benzer sürüm, topluluk raporu ya da sayfanın bir kısmı okunabildi).
- **[DY]**: doğrulanmadı. Çıkarım, bilinen genel UE/Unity davranışı ya da test gerektiren bir konu.

Bağlam: MetaHuman bir kez kiralık GPU'lu bir makinede (UE 5.8) oluşturulacak. Sonra Blender'a, oradan da kiralık Vega PC'deki Unity 6 URP'ye (Hikaye Stüdyosu) taşınacak. Amaç 1080x1920 dikey, konuşan karakterli video. Kayıt **çevrimdışı** yapılıyor (Recorder, sabit kare hızı), yani gerçek zamanlı 30 FPS şart değil.

---

## 0. Kısa sonuç

| Soru | Cevap |
|---|---|
| Mümkün mü? | **Evet, teknik olarak mümkün. Lisans da izin veriyor.** Yine de her parçası hazır ve tek tıklık bir yol değil. En zor iki iş: **saç kartlarını UE'den çıkarmak** ve **52 ARKit + 5 visem shape key'ini pişirmek**. İkisi için de ücretsiz, belgelenmiş ve tek tıklık bir araç bulunamadı. |
| Aşama sayısı | 3 aşama, **18 adım**: kiralık GPU'da UE'de 7, Blender'da 6, Unity'de 5. |
| iGPU'da 2–3 karakter | Kolayca sığar. LOD1 ile karakter başına yaklaşık 50–60 bin köşe ve 2K dokularla yaklaşık 100 MB doku belleği gerekir. Çevrimdışı kayıt olduğu için FPS sorun değil [DY: hesap, test yok]. |
| Gerçekçilik | URP'de UE'deki görüntünün altında kalır: SSS sınırlı, saç kart, kırışıklık haritaları yok. Yine de Rocketbox'tan belirgin şekilde iyi olması beklenir [DY]. |
| Süre | İlk karakter için yaklaşık 4–7 iş günü (öğrenme dahil). Sonraki her karakter için yaklaşık yarım gün [DY]. |
| Maliyet | Yazılımların hepsi ücretsiz: UE, Blender, Character DNA temel sürümü, Unity Personal. Tek masraf **GPU'lu makineye birkaç saatlik erişim** (D5: Vast.ai'de RTX 3060 yaklaşık $0,13/sa, ya da arkadaş/okul PC'si). |

---

## 1. Hattın şeması

```
[Kiralık GPU PC, UE 5.8]                       [Vega PC]
MetaHuman Creator ─► Export > DCC Export ──► head.dna, body.dna, *.png, *.json ─┐
        │                                                                      ├─► Blender + Character DNA
        └─► Assemble (UE Optimized/High) ─► saç kartı Static Mesh FBX + doku ──┘      │ ARKit/visem shape key pişir
                                                                                      │ yüz eklemlerini sil
                                                                                      ▼
                                                                    tek FBX (gövde + kafa + göz/diş/kirpik)
                                                                                      ▼
                                                              Unity 6 URP: Humanoid + YuzCanlilik (ARKit adları)
```

---

## 2. A aşaması: kiralık GPU'lu PC'de UE 5.8 (7 adım)

### A1. Proje ve eklentiler
- Boş bir proje açın. MetaHuman Creator, MetaHuman Core Tech eklentilerini açın, Epic hesabıyla giriş yapın. Autorig ve doku sentezi bulutta çalışır (D4 §2.4) [D].
- Donanım: RTX 3070 / RX 6800 XT sınıfı, 8 GB VRAM önerilir (D4 §2.1) [D].

### A2. Karakteri oluşturun
- Hazır ön ayardan (preset) başlayıp düzenleyin. **Saç seçerken kart LOD'u olan bir groom seçin.** Bilinen sorun [D, 5.8 Known Issues]: "Long Messy Hair", Medium kalite Optimized'da kafanın içinden geçiyor. Bu groom'dan kaçının.
- Giysi: Unity'de kumaş simülasyonu olmayacak. Vücuda yapışık, basit giysiler seçin [DY].

### A3. Doku kaynakları (Texture Sources)
- Bulut doku sentezi için çözünürlük seçilir. **Bilinen sorun [D]: DCC Export, Creator'daki doku çözünürlüğü ayarını yok sayar.** Forumda (2026-03, 5.7.3) yüz dokularının 8K seçilse bile **en fazla 2K** çıktığı raporlandı [KD].
  - Bizim için bu bir avantaj: iGPU'da zaten 2K kullanacağız.
  - 4K gerekirse çözüm, Assembly'nin "Textures" bölümünden dokuları almak (A5) ya da Epic'in önerdiği gibi doku grafiklerini (Texture Graph) elle değiştirmek [D].
  - https://dev.epicgames.com/documentation/unreal-engine/metahuman-known-issues-5-8-in-unreal-engine
  - https://forums.unrealengine.com/t/mhc-ddc-export-wrong-texture-maps-resolution/2703076

### A4. Export > DCC Export (asıl paket) [D]
- 5.8'de DCC Export, Assembly'den ayrılıp yeni **Export** aracına taşındı. Menü: **Export > DCC Export**.
- Ayarlar:
  - **External Path**: hedef klasör.
  - **Bake Makeup**: **açın**. Makyaj base color'a pişer; Unity'de ayrı makyaj katmanı uğraşı kalmaz.
  - **Compress in Zip File**: isteğe bağlı.
- Çıktı: **head .dna + body .dna + .png dokular + .json**. **FBX yok.**
- 5.8 yenilikleri: paket artık head/body **SMRF** (specular, roughness, metallic, fuzz) dokularını ve animated map packed mask'leri içeriyor. Yanlış göz/diş dokusu hatası düzeltildi. Kullanılmayan iç çamaşırı/göğüs dokuları çıkarıldı.
- Aynı araçta başka seçenekler de var:
  - **DNA Export**: yalnız .dna.
  - **Geometry Export**: projede Head / Body / **Full Body** Skeletal Mesh oluşturur.
  - **Materials Export**: Material Instance ve dokuları kopyalar.
- Kaynaklar:
  - https://dev.epicgames.com/documentation/metahuman/metahuman-creator-export-tool-in-unreal-engine
  - https://dev.epicgames.com/documentation/metahuman/metahuman-5-8-release-notes-in-unreal-engine
- **DNA içinde neler var?** [D]: Kafa için LOD0–LOD7, gövde için LOD0–LOD3 geometrisi, eklemler, blendshape'ler (yalnız LOD0) ve RigLogic kuralları. Yani **LOD seçimini Blender'da yaparsınız.** UE'de ayrıca LOD dışa aktarmak gerekmez.
  https://docs.polyhammer.com/character-dna-addon/terminology/

### A5. Assemble (yalnız saç kartları ve yedek için)
- Assembly hatları [D]:
  - **UE Cine**: LOD0'da strand saç, 1–2 GB.
  - **UE Optimized**: High/Medium/Low, sıkıştırılmış doku, 100 MB altı.
  - **UEFN Export**: Low seçenekte yalnız kart saç.
- **Optimized – High** seçin. Kart LOD'ları ve dokuları projeye düşer.
- Assembly'nin **Textures** bölümünde her çıktı için ayrı çözünürlük verilebilir [D].
  https://dev.epicgames.com/documentation/metahuman/assembly?lang=en-US
- Kendi FBX'inizi üretmek için ham "ExportCombinedSkelMesh" seçeneğini **kullanmayın**. 5.6–5.7'de siyah/mermer cilt ve yanlış malzeme raporlandı. Topluluk "Assemble edin" diyor [KD].
  https://forums.unrealengine.com/t/created-a-metahuman-in-5-6-looks-great-while-editing-but-horrible-when-exported/2544330

### A6. Saç: strand'ten karta, sonra FBX
- **Strand groom'lar Unity'ye gelmez.** Unity'de hazır bir groom renderer yok. Tek örnek MetahumanURP'nin "deferred groom" renderer'ı ve o da **yalnız Metal/macOS** (§5.2).
- MetaHuman saç LOD tablosu [D]:
  - Kart köşe sayısı: LOD0 30.000, LOD1 15.000, LOD2 10.000, LOD3 3.000, LOD4 1.500.
  - LOD5–7'de "mesh" saç kullanılır (500/250/100 köşe).
  - Kaş/sakal kartları: 15.000 / 7.000 / 3.000 / 1.000 / 500.
  - https://dev.epicgames.com/documentation/metahuman/platform-support-and-lod-specifications-for-metahumans?lang=en-US
- Groom Asset Editor'da **Cards** paneli, her grup ve LOD için kart mesh'ini ve dokuları gösterir [D]:
  - Depth, Coverage, Tangent dokuları
  - Attributes (RootUV / CoordU / Seed)
  - Auxiliary
  - Color/Roughness (yalnız Hair Card Generator'ın ürettiği kartlarda)
  - Kaynak: https://dev.epicgames.com/documentation/unreal-engine/setting-up-cards-and-meshes-for-grooms-in-unreal-engine
- **Önerilen yol [DY: hiçbir resmi belgede adım adım yok]:**
  1. Groom'u açın, Cards panelinde **LOD1 ya da LOD2 kart mesh'ini** bulun ("Browse to asset").
  2. Content Browser'da Static Mesh'e sağ tıklayın: **Asset Actions > Export > FBX**.
  3. Bağlı Coverage (alfa), Tangent ve Attributes dokularını **Export** ile TGA/PNG olarak alın.
  4. Kaşlar için aynısını yapın. Kirpikler zaten kafa DNA'sında ayrı bir mesh.
- **Uyarı:** MetaHuman saç rengi dokuda değil, malzemede melanin/kökten uca parametrelerle hesaplanır. Bu yüzden Unity'de renk için RootUV/CoordU ile gradyan yapmak ya da Hair Card Generator ile Color dokusu üretmek gerekir [DY].
- **Bu konudaki 3 forum başlığı yanıtsız** (2024-03, 2025-10, 2026-03). Yani topluluk da kolay bir yol bulmuş değil [D: başlıklar yanıtsız]:
  - https://forums.unrealengine.com/t/metahuman-hair-to-static-mesh-as-cards/1737489
  - https://forums.unrealengine.com/t/cant-make-metahuman-static-mesh-grooms-disappear-or-how-do-i-export-a-hair-card-with-pbr-materials-in-it/2668973
  - https://forums.unrealengine.com/t/how-can-i-export-a-groom-asset-or-a-metahuman-hair-and-import-into-blender-with-textures/2707161
- Yedek plan: saçı Blender'da kendiniz kart olarak yapın (D7 §5), ya da kısa, dokuya yakın saç ve şapka/başörtüsü kullanın.

### A7. Eve götürülecekler (USB / bulut sürücü)
- DCC klasörü: head.dna, body.dna, PNG'ler, JSON.
- Saç ve kaş kartı FBX'leri ve dokuları.
- (İsteğe bağlı) Geometry Export ile alınmış Full Body FBX. Blender aracı çalışmazsa yedek olur.
- (İsteğe bağlı) ARKit eşleme verisi (§4.3, Yol 2).
- Boyut: karakter başına birkaç yüz MB [DY].

---

## 3. Lisans (Haziran 2025 değişikliği ve sonrası)

| Konu | Durum |
|---|---|
| Unity'de kullanmak | **Serbest.** MetaHuman, standart UE EULA'sına girdi. "MetaHumans can be used with any engine or creative software" [D, metahuman.com/license]. Başka motordaki ürün "non-engine product" sayılır, %5 pay yok [D, CG Channel 2025-06-04; Digital Production 2025-06-05]. |
| Gelir sınırı | Yıllık gelir $1M altındaysa ücretsiz. Üstünde UE seat lisansı gerekir (CG Channel: $1.850/yıl) [D]. |
| TikTok videosu (render çıktısı) | Serbest [D]. |
| Yapay zeka | EULA metni [KD, conductatlas, 2026-05 alıntısı]: MetaHuman karakterleri ve animasyon eğrileri "to build or enhance any database or training…" için **kullanılamaz**. Change log Güncelleme 21: "restriction on use of MetaHuman digital characters and animation curves with AI or similar technologies". **Gemini TTS sesiyle MetaHuman konuşturmak eğitim değil**, CG Channel'a göre "AI içeren iş akışlarında kullanım serbest". Videoları ya da karakteri bir AI modeli eğitmek için kullanmayın. Not: Tim Sweeney 2026-10-27'de bu yasağın kaldırılacağını söyledi, ama yürürlük tarihi yok [D, Game Developer]. |
| Uygulama/EXE içinde dağıtım | Diğer motorlarda "oyunlarda kullanılabilir" [D]. Ham dosyaların (FBX/DNA/PNG) **kaynak biçimde** başkalarına dağıtılması konusu açık değil. EULA'nın ilgili maddesi okunamadı (sayfa açılmadı). Royalty muafiyeti maddesinde "Starter Content in source format" içermeme şartı geçiyor [KD]. **Öneri:** Hikaye Stüdyosu'nu ya da Unity projesini başkasıyla paylaşırsanız MetaHuman FBX/DNA dosyalarını içine koymayın. Yalnız videoları yayımlayın [DY]. |
| Atıf (credit) | MetaHuman sayfasında ve haber kaynaklarında **atıf şartı geçmiyor** [D: yok]. EULA metni okunamadığı için kesin değil [DY]. |
| Satış | Fab'de MetaHuman ve giysi satmak serbest [D]. |

Kaynaklar:
- https://www.metahuman.com/en-US/license
- https://www.cgchannel.com/2025/06/you-can-now-sell-metahumans-or-use-them-in-unity-or-godot/
- https://digitalproduction.com/2025/06/05/metahumans-graduate-ready-for-unity-godot-and-the-fab-cash-register/
- https://www.unrealengine.com/eula-change-log/unreal
- https://conductatlas.com/platform/unreal-engine/unreal-engine-eula/provision/CA-P-059499/prohibition-on-using-metahuman-for-ai-database-building/
- https://www.gamedeveloper.com/programming/epic-is-updating-the-eula-to-allow-ai-training-on-outputs-from-unreal-engine-and-metahuman

Okunamayan: https://www.unrealengine.com/en-US/eula/unreal (izin zaman aşımı) ve https://www.unrealengine.com/en-US/eula (site hatası). Kesin dağıtım hükmü için bu metin elle okunmalı.

---

## 4. B aşaması: Blender (Vega PC'de, 6 adım)

### 4.1 Araç seçimi

| Araç | Ücret | 5.6+ DNA (v2.5) | Ne yapar | Durum |
|---|---|---|---|---|
| **Poly Hammer Character DNA** | Temel sürüm **ücretsiz** (2026-07-12 blog, 2026-07-22 haber). Pro tek seferlik ücretli. | Evet (UE 5.6–5.8, D4) | head/body .dna içe aktarır. OpenRigLogic ile UE'ye 1:1 yüz paneli (face board). Ücretsiz özellikler: **pose kütüphanesi (duygular, visemler)**, face board ve RBF verisini sahneye **pişirme**, DNA'ya geri yazma. Pro: Shape Key Editor, LOD kalibrasyonu, Bone Matching. Dokümanda sürüm 0.15.1. | **Önerilen** [D]. Beta. Poly Hammer Portal hesabı gerekir. FBX/Unity'den söz edilmiyor. |
| Skalin MetaMorph / "DNA Studio" | Ücretsiz beta + ücretli tam sürüm | UE 5.7'de test edildi | DNA içe/dışa aktarma, LOD, blendshape yazma, face board | Blender 5.2+, yalnız Windows. 2026-11-17'de "tamamlandı" duyurusu yapıldı [KD]. |
| MetaHuman DNA Importer (Ninia Souhail) | Gumroad, fiyat yazmıyor | ? | DNA içe aktarma, Rigify, isteğe bağlı shape key'ler | v0.5.2, 2026-06 [KD] |
| Liyou: BlendShapeExporterV2 / MetahumanBSrigerV3 | Ücret ve lisans yazmıyor | ? | "DNA'dan belirtilen LOD'da ARKit 52 ya da MetaHuman blendshape dışa aktarımı", "DNA'yı doğrudan Blender'da blendshape rig'e çevirme" (2025-12, 2026-10) | Tam olarak istediğimiz şey, ama erişim/ücret **doğrulanamadı** [KD]. Öğretici sayfası açılamadı. |
| Accent – MetaHuman Optimizer (Fab) | **Ücretli** | UE 5.7+ | 52 ARKit pozunu **OBJ** olarak verir. LOD0/1/2+ | Bütçe dışı [D] |
| **MetaHuman DNA Calibration** (Epic, GitHub) | Ücretsiz | **Hayır.** 5.6 DNA'sı v2.5, kütüphane v2.1 bekliyor. Epic güncellemeyeceğini söyledi (2025-07-24). | — | **Kullanmayın** [D] |
| **OpenRigLogic** (Epic, MIT) | Ücretsiz | 5.8 dalı "production-ready" | C++ DNA ve RigLogic kütüphaneleri, SWIG ile Python | Yalnız kendi aracınızı yazacaksanız. CMake ile derleme gerekir [D] |

Kaynaklar:
- https://www.polyhammer.com/character-dna-addon
- https://www.polyhammer.com/blog/free-character-dna-addon-openriglogic
- https://digitalproduction.com/2026/07/22/character-dna-goes-free/
- https://forums.unrealengine.com/t/customize-metahumans-entirely-in-blender/2740080
- https://blenderartists.org/t/addon-metahuman-dna-importer-for-blender/1640070
- https://forums.unrealengine.com/t/community-tutorial-blendshapeexporterv2-convert-dna-to-arkit-52-or-metahuman-blendshapes/2688321
- https://forums.unrealengine.com/t/community-tutorial-metahumanbsrigerv3-convert-metahuman-dna-to-blendshape-rig-directly-in-blender/2845154
- https://forums.unrealengine.com/t/accent-metahuman-optimizer-arkit52-export-batch-tools/2715387
- https://forums.unrealengine.com/t/ue5-6-metahuman-dna-github-dnacalibration/2650233
- https://github.com/EpicGames/openriglogic

### 4.2 Adımlar

**B1. Kurulum.** Blender 4.5 LTS ya da 5.1 kurun (Character DNA'nın desteklediği sürüm, D4). Character DNA'yı Poly Hammer Portal ve eklenti sunucusu üzerinden kurun. Blender bu iGPU'da çalışır. RigLogic hesabı CPU'da yapılır [DY].

**B2. İçe aktarma ve LOD seçimi.** head.dna ile body.dna'yı aynı "Rig Instance" altında içe aktarın (bir instance kafa ve gövdeyi birlikte tutabilir [D]).
- Hangi LOD? MetaHuman LOD tablosu [D]:

| | LOD0 | LOD1 | LOD2 | LOD3 |
|---|---|---|---|---|
| Kafa köşe | 24.000 | 12.000 | 6.000 | 2.500 |
| Kafa eklem | 713 | 529 | 397 | 283 |
| Kafa blendshape | 669 | – | – | – |
| Kafa etki/köşe | 12 | 12 | 12 | 8 |
| Animated maps (kırışıklık) | var | var | yok | yok |
| Gövde köşe | 30.500 | 7.600 | 3.350 | 1.507 |
| Gövde düzeltici (corrective) | var | var | yok | yok |

- **Öneri: kafa LOD1 + gövde LOD1.** Dikey videoda yüz yakın çekimde LOD0 da kullanılabilir. iGPU için bile 24 bin köşe sorun değil, çünkü kayıt çevrimdışı.
  - LOD1'de yüz **yalnız eklemlerle** deforme olur (blendshape yalnız LOD0'da). Pişirme, RigLogic'in o LOD için verdiği eklem pozlarından yapılır [D: tablo; DY: Character DNA'nın LOD1'i ayrı içe aktarıp aktarmadığı belgelenmemiş].
  - Araç yalnız LOD0'ı alıyorsa: **LOD0 kullanın.** Pişirmeden önce Decimate yapmayın, shape key'ler bozulur [DY].
- Kafa DNA'sı tek mesh değildir: kafa, dişler, tükürük (saliva), sol/sağ göz, göz kabuğu (eyeshell), kirpik, göz kenarı (eyeEdge), gözyaşı kanalı (cartilage) [DY: genel MetaHuman yapısı]. Unity'de **göz kabuğu, eyeEdge ve cartilage'ı silmeyi ya da gizlemeyi** düşünün. Bunlar UE'nin özel saydam/kırılma malzemeleriyle çalışır.

**B3. Yüz pozlarını shape key olarak pişirmek (asıl iş).**
- Projemizin `YuzCanlilik.cs` dosyası ad ile eşleştirme yapıyor:
  - ARKit adları: `jawOpen`, `eyeBlink*`, `mouthSmile*`, `browDown*`, `browInnerUp`, `browOuterUp*`, `mouthFrown*`, `eyeWide*`, `eyeSquint*`, `eyeLookDown*`.
  - Visemler: `aa`, `ih`, `ou`, `ee`, `oh` (ya da `viseme_aa` …).
  - Tüm SkinnedMeshRenderer'ları tarıyor, Humanoid `LeftEye/RightEye` kemiklerini kullanıyor.
  - Bu yüzden **ARKit 52'nin hepsi şart değil.** En az şu **~17 shape key** yeter: jawOpen, eyeBlinkLeft/Right, mouthSmileLeft/Right, browDownLeft/Right, browInnerUp, browOuterUpLeft/Right, mouthFrownLeft/Right, eyeWideLeft/Right, eyeSquintLeft/Right, aa, ih, ou, ee, oh.
  - Kaynak: `/home/claude/uygulama/Assets/HikayeStudyo/Scripts/YuzCanlilik.cs` satır 440–470 (kod okundu).
- Yöntem [DY: adımlar genel Blender bilgisi, bu araçla denenmedi]:
  1. Character DNA face board'unda her poz için kontrol değerlerini ayarlayın. Visemler ve duygular ücretsiz **pose kütüphanesinde** hazır [D]. ARKit pozları için iki yol var:
     - **Yol 1 (öneri):** face board'da elle ayarlayın. Kontrol değerlerini (CTRL_… adları) bir kez JSON'a yazın. **Kontrol adları tüm MetaHuman'larda aynı** olduğu için aynı JSON her karakterde tekrar kullanılır.
     - **Yol 2:** UE'deki `PA_MetaHuman_ARKit_Mapping` pose asset'i, ARKit pozlarını MetaHuman kontrol/eğri bileşimleri olarak tanımlıyor [KD: forum, 2025-11]. Bu değerler GPU'lu PC'de UE Python ile JSON'a dökülebilir [DY: API denenmedi].
  2. Her poz için: RigLogic'in değerlendirdiği sahneyi alın (depsgraph'ın değerlendirilmiş mesh'i). Köşe konumlarını kafa mesh'inde yeni bir shape key'e kopyalayın. Aynı adla **dişler, kirpik ve tükürük mesh'lerine de** shape key ekleyin. Çene açılınca alt dişler de hareket etmeli.
  3. Bunu ~20 satırlık bir Python betiğiyle otomatikleştirin. Biz yazabiliriz.
  - Character DNA'nın ücretsiz "bake to scene data" özelliği **animasyonu** (kemik ve shape key keyframe'lerini) pişiriyor. Doğrudan "shape key olarak kaydet" özelliği belgelenmemiş. Shape Key Editor Pro'da [D]. Bu yüzden betik gerekebilir.
- Yüz pişirme LOD1 eklemlerinden yapılırsa, eklem deformasyonu shape key'e "donar". Unity'de 529 yüz eklemine gerek kalmaz.

**B4. Yüz eklemlerini temizlemek.**
- Tüm pozlar pişince yüz kemiklerini (FACIAL_*) silin. Bu kemiklerin vertex group ağırlıklarını `head` grubuna birleştirin.
- **Göz kemiklerini (FACIAL_L_Eye, FACIAL_R_Eye) bırakın.** Unity Humanoid'de LeftEye/RightEye olarak eşlenir. Projede göz bakışı bu kemiklerle yapılıyor.
- Çene kemiği (FACIAL_C_Jaw): jawOpen shape key'e pişirildiyse silinebilir.
- Sonuç: gövde iskeleti (~UE5 Manny adları), iki göz kemiği ve ~17–60 shape key [DY].

**B5. Saç ve kaş kartları.** A6'daki FBX'leri içe aktarın, kafaya hizalayın. Ağırlığı %100 `head` kemiğine verin (ya da Unity'de `head` kemiğine çocuk nesne yapın, bu daha basit). Kaşlar kaş kalkınca hareket etmez. Gerekirse kaş kartlarına da browInnerUp/browDown shape key'i pişirin (kafa yüzeyine "shrinkwrap" ile) [DY].

**B6. FBX dışa aktarma** [DY: standart Blender→Unity ayarları]:
- Scale 1.0, **Apply Scalings: FBX All**, Forward **-Z**, Up **Y**, **Apply Transform** açık.
- Armature: **Add Leaf Bones kapalı**, Only Deform Bones açık.
- Geometry: **Shape Keys açık**, Smoothing **Face**, Apply Modifiers kapalı (Armature modifier kalsın).
- Animasyon yok (yalnız model).
- Tek FBX: gövde, kafa, göz, diş, kirpik ve kartlar.
- Dokuları ayrı klasöre koyun.

---

## 5. C aşaması: Unity 6 URP (5 adım)

### C1. Model içe aktarma [DY: Unity'nin genel ayarları]
- Model sekmesi:
  - **Blend Shapes açık**.
  - Blend Shape Normals: **Calculate** (ya da Import). Legacy Blend Shape Normals kapalı.
  - Mesh Compression **Off**.
- Rig sekmesi: **Humanoid**, Avatar: Create From This Model.
- Skin Weights: gövde LOD1 köşe başına 8 etki kullanıyor [D]. Proje ayarı 4 ise küçük bozulmalar olabilir. Model içe aktarmada "Custom" ile 8 seçin ve Quality > Skin Weights'i **Unlimited** yapın [DY].

### C2. Humanoid eşleme
MetaHuman gövde iskeleti UE5 Manny adlarını kullanır. Otomatik eşleme çoğunlukla tutar, ama elle kontrol edin [DY: bu sürümde denenmedi, Unity'ye özgü bir MetaHuman raporu bulunamadı].

| Unity | MetaHuman |
|---|---|
| Hips | pelvis |
| Spine / Chest / UpperChest | spine_01 / spine_03 / spine_05 |
| Neck / Head | neck_01 / head |
| Left Shoulder / UpperArm / LowerArm / Hand | clavicle_l / upperarm_l / lowerarm_l / hand_l |
| Left UpperLeg / LowerLeg / Foot / Toes | thigh_l / calf_l / foot_l / ball_l |
| Parmaklar | thumb_01..03_l, index_01..03_l … (metacarpal kemiklerini boş bırakın) |
| LeftEye / RightEye | FACIAL_L_Eye / FACIAL_R_Eye |
| Jaw | boş (çene shape key'de) |

- Twist ve düzeltici kemikler (…_twist_01, …_correctiveRoot) Humanoid'e eşlenmez. Mixamo veya Rocketbox animasyonlarında bu kemikler hareketsiz kalır. Omuz/dirsekte hafif kayma beklenir [DY].
- Mevcut `Karakter3DKurulum.cs` ve `karakter_listele` akışıyla kütüphaneye ekleyin. `YuzTesti` ve `AgizSenkronTesti` ile shape key eşleşmesini kontrol edin.

### C3. Malzemeler (ücretsiz seçenekler)

| Parça | Ücretsiz seçenek | Not |
|---|---|---|
| Cilt | (a) URP Lit + normal + SMRF'den smoothness. (b) **Unity "URP Defender Character Demo"** pre-integrated skin Shader Graph'ı. (c) bentoBAUX Separable SSS (D7). | Defender: teknoloji Asset Store EULA'sı altında, "use or modify it" serbest. **Karakterin kendisi ticari kullanıma kapalı** (yalnız shader'ları alın) [KD]. URP 12–14 için yapılmış, Unity 6 / URP 17'ye taşınması gerekebilir [DY]. |
| Göz | Defender **parallax eye** shader'ı ya da URP Lit + ayrı parlak katman | Göz kabuğunu (eyeshell) gizleyin [DY] |
| Kirpik ve saç kartı | Alpha Clip + iki taraflı + (MSAA açıksa) alpha-to-coverage. Defender **anisotropic** shader'ı vurgu için. | Kart saç rengi için RootUV gradyanı (A6) [DY] |
| Diş, tükürük | URP Lit. Tükürüğü gizleyebilirsiniz. | — |

- SMRF dokusunun kanal sırasını (S/M/R/F) Unity'nin metallic(R) + smoothness(A) düzenine çevirmek gerekir. Smoothness = 1 − roughness. Kanal sırası belgede yazmıyor, PNG'ye bakarak doğrulanmalı [DY].
- Doku içe aktarma: Max Size **2048**, base color/SMRF için BC7, normal için BC5 (Normal map tipi).
- Kaynaklar:
  - https://github.com/unitytechnologies/urp-defender-character-demo
  - https://bentobaux.github.io/posts/separable-subsurface-scattering-in-unity-6/

### C4. Hazır topluluk araçları (Unity)
- **lofihashbloom/MetahumanURP** (MIT, Unity 6000.6 / URP 17.6) [D: README sayfası okundu]:
  - İçerik: MetaHuman importer (UE'deki `Tools~/Unreal` betikleriyle dışa aktarım ve **Tools > MetaHuman > Import Selected Manifest**), cilt/göz/saç shader'ları, DNA yüz sürücüsü (`DnaRigDriver`, CTRL_expressions), ARKit→DNA eşleme (`ArkitToDna`).
  - **Önemli sınırlar:**
    - Yalnız Apple silicon / Metal'de test edilmiş.
    - Yüz rigi native eklentisi **yalnız macOS arm64**.
    - Groom renderer yalnız Metal.
    - "Unreal/DNA parity yok".
  - **Windows ve Vega'da yüz sürücüsü çalışmaz.** Shader'ları ve importer'ın malzeme kurulumu fikir verebilir. Unity sürümü 6000.6 istiyor, projemiz Unity 6.0 olabilir [DY].
  - Paket README'si okunamadı (izin zaman aşımı).
  - https://github.com/lofihashbloom/MetahumanURP
- **wenlek/MetaHumanMeshTools** (gitee, MIT, 2024-12): UE eklentisi + Blender eklentisi. Yalnız temel LOD. Eski DNA dönemi; Unity'ye yönelik değil [D].
- erosy/Pitch-LipSync-Metahuman-Unity ve HatoProject/UnityMetaHuman: **okunamadı** (hız sınırı / robots). Ne olduklarını doğrulayamadım.
- Unity Asset Store'da ücretsiz "MetaHuman importer" **bulunamadı**. PIDI Skin & Eye ücretli (sürüm 3 kullanımdan kalktı, 4'e geçti).
- Japonca, Çince, Rusça ve Türkçe aramalarda Unity'ye özgü MetaHuman aktarma rehberi çıkmadı. Sonuçlar Epic belgelerinin çevirilerine ve aynı Epic forum başlıklarına gidiyor.

### C5. Projeye bağlama ve deneme
1. `karakter_listele` ile karakterin 3D oyuncu adı olarak göründüğünü kontrol edin.
2. Kısa bir test hikayesiyle `onizleme_goruntu: <ad> <satir>` çalıştırın. `HikayeKomut/ekran.png` dosyasına bakın.
3. Bir satırı seslendirip `kaydet` ile 10 saniyelik deneme kaydı alın. Ağız (jawOpen/visem), göz kırpma ve bakışı kontrol edin.

---

## 6. Performans (Vega iGPU, 2–3 karakter, çevrimdışı 30 fps)

**Karakter başına köşe sayısı** (LOD tablosundan [D], toplam [DY]):

| Parça | LOD1 yolu | LOD0 yolu |
|---|---|---|
| Kafa | 12.000 | 24.000 |
| Gövde | 7.600 | 30.500 |
| Saç kartı | 15.000 (LOD1) / 10.000 (LOD2) | 30.000 |
| Kaş/sakal kartı | 3.000–7.000 | 15.000 |
| Giysi (tahmin) | ~10–20 bin | ~20–40 bin |
| **Toplam** | **~50–60 bin** | **~120–140 bin** |

- 3 karakter: LOD1 ile ~180 bin, LOD0 ile ~400 bin köşe. Vega 7 bu sayıları sorunsuz çizer. Kayıt çevrimdışı olduğu için FPS sınır değil [DY].
- **Doku belleği** (hesap [DY]):
  - 2048² BC7 doku mip'lerle ≈ 5,3 MB. 4096² ≈ 21 MB.
  - Karakter başına ~18 doku (kafa 3–4, gövde 3, göz 2, diş 2, kirpik 1, saç 3, giysi 3).
  - 2K'da ≈ 95 MB, 4K'da ≈ 380 MB.
  - 3 karakter 2K'da ≈ 0,3 GB. iGPU sistem RAM'ini (64 GB) paylaştığı için sorun yok.
- **Shape key:** ~17–60 şekil × 12–24 bin köşe. Unity seyrek (sparse) delta tutar ve GPU'da işler. Etkisi ihmal edilebilir [DY].
- **Pahalı olanlar:** SSS ekran-uzayı geçişleri (bentoBAUX: birkaç tam ekran geçiş + 32 örnek), gölge çözünürlüğü, alan derinliği. 1080x1920'de iGPU'da kayıt yavaşlar ama çıktı bozulmaz (D7).
- Sonuç: **2–3 MetaHuman LOD1 ile bu PC'de 30 fps çevrimdışı kayıt makul görünüyor** [DY: test yok]. İlk ölçüm için Unity'de Stats/Profiler ile tek karakterle deneyin.

---

## 7. Süre tahmini [DY]

| İş | İlk sefer | Sonraki karakter |
|---|---|---|
| GPU'lu PC: UE 5.8 kurulumu (yoksa) | 2–4 sa (indirme 50–100 GB) | – |
| Karakter oluşturma + doku/autorig + DCC Export + Assemble + saç dışa aktarma | 2–3 sa | 1 sa |
| Blender: araç kurulumu, içe aktarma, poz JSON'u, pişirme betiği | 1–2 gün | 1–2 sa |
| Blender: eklem temizliği, saç bağlama, FBX | 0,5 gün | 30 dk |
| Unity: shader'ları taşıma (Defender → URP 17), malzeme kurulumu | 1–2 gün | 30–60 dk |
| Unity: Humanoid, YuzCanlilik testi, deneme kaydı | 0,5 gün | 30 dk |
| **Toplam** | **~4–7 iş günü** | **~4–5 saat** |

**Öneri:** GPU'lu makineye bir kez çıkıldığında **4–6 karakteri birden** oluşturup hepsini dışa aktarın. Kiralık saati boşa harcamayın. Blender ve Unity işleri Vega PC'de, zaman sınırı olmadan yapılır.

---

## 8. Başarısızlık noktaları (en riskliden aza)

1. **Saç kartlarını dokusuyla çıkarmak.** Resmi, adım adım bir yol yok. 3 forum başlığı yanıtsız. Saç rengi malzemede hesaplanıyor. → Yedek plan: Blender'da kendi kart saçınız ya da kısa saç/şapka.
2. **ARKit/visem pişirme.** Ücretsiz ve tek tıklık bir araç doğrulanamadı (Liyou'nun araçlarının ücreti belirsiz, Accent ücretli). Kendi betiğimiz gerekecek. Character DNA'nın shape key kaydetme özelliği Pro'da. → Betikle "değerlendirilmiş mesh → shape key" yolu, teorik olarak ücretsiz sürümle yapılabilir [DY].
3. **Character DNA beta hataları / LOD1 içe aktarılamaması.** → LOD0 ile devam edin. Yedek araçlar: Skalin ücretsiz beta, Geometry Export FBX'i.
4. **FBX ile yüz animasyonunu Blender'a taşımak bozuk.** Forumda (2025-08, UE 5.6 + Blender 4.5.1) sekans FBX'i Blender'da yüzü bozuyordu ve çözümsüz kaldı [D]. → Animasyonu FBX ile taşımayın, yalnız DNA ile çalışın.
   https://forums.unrealengine.com/t/exporting-levelsequence-with-metahuman-characer-metahuman-performance-results-on-deformed-mesh-on-blender/2639203
5. **DNA koordinat sistemi hataları (5.8 bilinen sorun).** DNA asset'leri "Left, Down, Front" ile "Left, Up, Front" arasında karışabilir. Blender'da kafa ters ya da dönük gelirse buna bakın [D].
6. **Cilt plastik görünür, göz cansız durur.** URP'de SSS sınırlı, kırışıklık haritaları yok. → Işık, kamera, post-process ve pre-integrated skin. Beklentiyi "UE kalitesinin %60–70'i" olarak tutun [DY].
7. **Baş-gövde dikiş çizgisi.** 5.8 bilinen sorunu (boyun/omuz), çözümü yok [D]. Unity'de normal farkı da eklenebilir. → Yaka/giysiyle örtün.
8. **Humanoid retarget kayması.** Twist kemikleri eşlenmez. Rocketbox/Mixamo animasyonlarında omuz ve dirsek hafif bozulabilir [DY].
9. **Lisans yanlış adımları.** Ham MetaHuman dosyalarını paylaşılan projeye/EXE'ye koymak (belirsiz). Videoları ya da karakteri AI eğitiminde kullanmak (yasak).
10. **GPU'lu makine erişimi ve Epic bulut servisleri.** Autorig ve doku sentezi internet ve Epic hesabı ister. Kiralık makinede UE kurulumu saatlerce sürebilir (D5).

---

## 9. Adım listesi (özet, 18 adım)

**A. GPU'lu PC (UE 5.8)**
1. Boş proje + MetaHuman eklentileri + Epic girişi.
2. Karakter oluştur (kart LOD'lu saç, basit giysi).
3. Texture Sources (2K yeter) + autorig.
4. **Export > DCC Export** (Bake Makeup açık) → head.dna, body.dna, PNG, JSON.
5. Assemble: **UE Optimized – High**.
6. Saç ve kaş kartı Static Mesh'lerini (LOD1/2) FBX'e, dokularını PNG/TGA'ya aktar. (İsteğe bağlı: ARKit eşleme JSON'u.)
7. Hepsini Vega PC'ye taşı (4–6 karakter birden).

**B. Blender (Vega PC)**
8. Blender 4.5/5.1 + Character DNA (ücretsiz, Portal hesabı).
9. head + body DNA'yı içe aktar, LOD1 seç (olmazsa LOD0).
10. Betikle ~17 (ya da 52+5) yüz pozunu shape key olarak pişir (kafa, diş, kirpik).
11. FACIAL_* kemiklerini sil, ağırlıkları head'e birleştir. Göz kemiklerini bırak.
12. Saç ve kaş kartlarını head'e bağla.
13. Tek FBX dışa aktar (shape key'ler açık, leaf bone kapalı).

**C. Unity 6 URP**
14. FBX içe aktar: Humanoid, blend shape açık, sıkıştırma kapalı, skin weights 8/Unlimited.
15. Avatar eşlemesini kontrol et (göz kemikleri dahil).
16. Malzemeler: cilt (pre-integrated/SSS), göz, kart saç (alpha clip + anizotropik), dokular 2K.
17. `Karakter3DKurulum` → `karakter_listele` → `YuzTesti` / `AgizSenkronTesti`.
18. `onizleme_goruntu` + kısa `kaydet` denemesi.

---

## 10. Okunamayan / doğrulanamayan kaynaklar
- https://www.unrealengine.com/en-US/eula/unreal (izin zaman aşımı) ve https://www.unrealengine.com/en-US/eula (site hatası). Dağıtım ve atıf maddeleri buradan doğrulanmalı.
- https://dev.epicgames.com/community/learning/tutorials/3XBJ/… (Liyou'nun ARKit 52 dışa aktarma öğreticisi): izin zaman aşımı.
- https://github.com/lofihashbloom/MetahumanURP/tree/main/MetahumanURP (paket README'si): izin zaman aşımı.
- https://github.com/erosy/Pitch-LipSync-Metahuman-Unity (hız sınırı), https://github.com/HatoProject/UnityMetaHuman (robots).
- CG Channel'ın 2026-07 Character DNA haberi (robots). Bilgi Poly Hammer blogu ve Digital Production'dan alındı.
