# A11 — Uluslararası teknoloji ve gelecek araştırmacısı

> Tarih: 2026-11-27. Kod yazılmadı, üretim aracı çalıştırılmadı, **hiçbir test yapılmadı**.
> Kaynaklar: `SOURCES/A11_kaynaklar.md`; `[S001]` = A11-S001. "ÇIKARIM:" = benim yorumum.
> Okunan sayfalar WebFetch süzgecinden okundu; "(özet)" = yalnız arama sonucu. 30/30 arama kullanıldı.

## Kısa sonuç
1. **Q-004:** İyi oyun sinematiği 3D yolla hedeflenebilir. Örnek videodaki "gerçek insan filmi" görünümünün otomatik, ucuz, tek kişiyle 3D'den çıktığına dair **kanıt bulamadım**; ÇIKARIM: bugün büyük olasılıkla hayır (güven: orta; test yok).
2. 1–3 yılda farkı en çok daraltacak aday **neural rendering** (motor karesini üretici modelle "gerçekçileştirme"). Bu da üretici yapay zekâdır; kullanıcı şartıyla ilişkisi kullanıcıya sorulmalı.
3. Bugün hazır ürün DLSS 5: yalnız RTX 50; NVIDIA sayfaları UE5 eklentisi ve Streamline diyor, **Unity anılmıyor** [S001, S032].
4. Şimdiden yapılacak: motoru değiştirmek değil, mimariyi "plan verisi + ek geçişler (derinlik, maske...) + takılabilir son işlemci" biçiminde açık tutmak.

## Görev tanımı
SIGGRAPH 2025–2026 ve akademik yönler; neural rendering; Gaussian splatting ve avatarlar; NeRF; image/video-to-3D; üretici 3D; AI animasyon/sinematografi/sahne; dijital insan; hibrit yol; açık ağırlıklı video modelleri (kıyas); procedural; diğer sektörler. Teslim: BUGÜN / DENEYSEL / İZLENECEK, mimari açıklığı, takip düzeni.

## Araştırma soruları
- S1 Neural rendering nerede? (DLSS 5, Unity Neural, G-buffer koşullu difüzyon)
- S2 Hibrit "3D → video-to-video" mümkün mü, "AI video üretici" sayılır mı?
- S3 Açık ağırlıklı video modelleri: donanım, lisans, maliyet
- S4 Üretici 3D / otomatik rig / Gaussian splat / avatar
- S5 AI animasyon, sinematografi, sahne üretimi, procedural
- S6 Diğer sektörler
- S7 Mimari açıklığı ve takip

---

## Bulgular

### S1 Neural rendering
- **DLSS 5 "3D-Guided Neural Rendering"** (Mart 2026 duyuru, Eylül 2026 çıkış): motorun karesi üstünde son aşama; girdi renk + hareket vektörü; ten altı ışık, saç, temas gölgesi, GI ekliyor; NVIDIA "deterministik, zamansal olarak kararlı" diyor; yoğunluk ve maske kontrolleri var; yerelde tek RTX 50, 4K'ya kadar [S001, S032]. Genel SDK takvimi duyurulmamış [S002].
- **Çelişen gözlem:** Mart tanıtımında karakter yüzünün (dudak, elmacık) değiştiği eleştirildi; NVIDIA sanatçı kontrolü olduğunu söyledi [S003]. Eylül sürümünde durum bağımsız testle doğrulanmadı. Karakter tutarlılığı için ölçülmesi gereken risk budur.
- Zengin girdi (ışın/yol izlemeli) daha doğru sonuç veriyor [S002]. ÇIKARIM: neural katman kötü sahneyi kurtarmaz; gölge, HDR, animasyon düzeltmeleri (A04, A09) yine gerekli.
- **Unity Neural:** URP'de sinirsel çıkarım, yükseltici çatısı, doku sıkıştırma, Sentis; 6.7'de başlıyor. Okuduğum duyuruda DLSS 5 benzeri bir "gerçekçileştirme" özelliği geçmiyor [S014]. Unity 7 önizleme Aralık 2026, tam sürüm hedefi 2027 Q1 [S004].
- **Akademi:** NVIDIA DiffusionRenderer [S007], FrameDiffuser [S006] (özet); NeuralSRender (Ekim 2026) G-buffer + metinle Wan2.2-5B üstünde sim-to-real; 13,67 GB tepe VRAM (RTX Pro 6000), 1280x704x81; sürüş sahneleri, **insan/kimlik tutarlılığı incelenmemiş**, lisans yazmıyor [S005].
- **SIGGRAPH:** 2026 ödül listesinde ve okuduğum tanıtım sayfasında dijital insan/üretici video öne çıkmıyor [S028]. Bu, programda yok demek değildir: bildiri listesini taramadım (zayıf nokta).

### S2 Hibrit yol: 3D önizleme/render → "neural renderer"
**Kanıt:**
- Ticari örnek: Reallusion AI Studio 3D görünümden video + oyuncu referansı alıp bulutta Seedance/MiniMax modelleriyle üretiyor [S017].
- Akademik: PrevizWhiz kaba 3D sahne → Wan/VACE (RTX 5090 24 GB, klip ~1 dk, stilize, zamansal tutarlılık sorunu sürüyor) [S016]. ANI3DHUMAN mesh animasyonu → fotogerçekçi insan; 48 GB kart, ~19 dk/animasyon; difüzyonun görünüm "uydurmasına" karşı özel önlem gerekiyor [S027].
- Açık modeller: VACE referans + video-to-video (derinlik/poz ön işlemeli, 1.3B 480p) [S010]; Wan-Animate-2 sürücü videodan karakter canlandırma [S009]; LTX-2.5 v2v etiketli [S008].
- Mimari görselleştirme aynı yolu parça parça kullanıyor: Chaos AI Enhancer render'daki insan ve bitkiyi gerçekçileştiriyor [S031].

**"AI video üretici" sayılır mı? (ÇIKARIM: benim önerdiğim ayrım)**

| Düzey | Ne yapıyor | Örnek | Son pikseli kim üretir | Kullanıcı şartıyla |
|---|---|---|---|---|
| D0 | klasik render | URP | motor | uyumlu |
| D1 | gürültü giderme / yükseltme | DLSS SR, Unity yükseltici | motor (model onarır) | uyumlu sayılır |
| D2 | yapay zekâ ARACI: model, hareket, yüz, ses üretir | TRELLIS.2, Kimodo, Audio2Face, TTS | motor | plan §1'e göre araştırma kapsamında; son kare Unity'den |
| D3 | geometriye bağlı kare iyileştirici (üretici) | DLSS 5 | üretici model, motor karesine bağlı | **gri alan: kullanıcıya sor** |
| D4 | genel video modeliyle video-to-video | VACE, LTX, Wan, Seedance | video üretici model | **ÇIKARIM: şarta aykırı** |
| D5 | metinden/görselden video | Kling, Veo vb. | video üretici | açıkça dışarıda |

- Sınama sorusu: "Model bizim 3D sahnemiz olmadan da video üretebiliyor mu?" Evet ise video üreticisidir. Uygulamanın içinden çağrılması, yerelde çalışması ya da "neural renderer" denmesi bunu değiştirmez.
- D4 "videolar uygulamamızdan çıkıyor" şartının harfini karşılayabilir ama "video üreticisi olmadan" kısmını karşılamaz. Kullanıcı açıkça onaylamadan yol haritasına girmemeli.
- D4'ün bedeli: yüz/kıyafet kayması (3D'nin en büyük üstünlüğü olan tutarlılık zayıflar, krş. A02), tekrar deneme maliyeti, güçlü kart ya da bulut.
- **TikTok etiketi:** gerçekçi görüntü, **ses** ya da video içeren yapay zekâ içeriği etiketlenmeli [S018]. ÇIKARIM: Gemini TTS sesli bir video, görüntü saf 3D olsa bile büyük olasılıkla etiket ister (A12 netleştirsin; A07 de etiket kuralını not etti).

### S3 Açık ağırlıklı video modelleri (yalnız kıyas)
| Model | Lisans | Donanım (kaynakta yazan) | Not |
|---|---|---|---|
| LTX-2.5 | LTX-2.x Community; yıllık gelir <10 M$ ücretsiz ticari [S008] | VRAM yazmıyor; CUDA; fp8/offload; NVFP4 yalnız Blackwell | senkron ses; 1920x1088 örneği; Türkçe ses desteği DOĞRULANAMADI |
| Wan-Animate-2 14B | Apache-2.0 [S009] | varsayılan 8x A800; 480p 2x A800'de denenmiş | tüketici kartı gereksinimi yazmıyor |
| Wan2.1-VACE 1.3B | Apache-2.0 [S010] | VACE VRAM yok (T2V-1.3B 8,19 GB) | 480p |
| Cosmos-Transfer2.5 | kod Apache-2.0, ağırlık NVIDIA OML [S021] | sayfada yok | derinlik/seg/kenar kontrolü; geliştirme bitti |
| Cosmos 3 Nano | OpenMDW1.1 [S022] | Linux; Ampere+; VRAM yok | sayfada kontrollü v2v belgelenmemiş |

- RTX 3060 12 GB: tek kullanıcı anlatımı ~720p ~5 sn için ~2,5 saat (Wan 2.1) [S025, C/D]. **Karar dayanağı olamaz**; yeni küçük/damıtılmış modeller çok daha hızlı olabilir, ölçmedim.
- Bu bilgisayar (Vega tümleşik Radeon): AMD'nin Windows ComfyUI/ROCm anlatımında 5000G anılmıyor [S030]. ÇIKARIM: yerel video modeli pratik değil (kesin uyumluluk DOĞRULANAMADI).
- **Bulut maliyeti (liste fiyatı):** LTX görselden videoya 1080x1920: 0,06 $/sn (2.3-fast) – 0,17 $/sn (2.5-pro) [S023]. ÇIKARIM (hesap): 90 sn tek geçiş ≈ 5–15 $; plan başı 3 deneme ≈ 16–46 $. Bu i2v fiyatıdır, genel v2v fiyatı listelenmemiş. fal Wan VACE: video başı 0,20 $, en çok 720p [S024]; ÇIKARIM: 30 plan x 3 deneme ≈ 18 $.

### S4 Üretici 3D, otomatik rig, splat, avatar
- **TRELLIS.2** (Microsoft): tek görselden PBR mesh/GLB; MIT; en az 24 GB NVIDIA; yalnız Linux test; küçük delikler olabilir [S011]. ÇIKARIM: aksesuar/dekor için; yüzü canlanacak karakter için yeterli değil. Girdi görseli üretici ile yapılırsa onun lisansı da gerekir.
- **Hunyuan3D lisans tuzağı:** bölge AB, Birleşik Krallık, Güney Kore'yi dışlıyor ve **çıktıların** bölge dışında gösterilmesi de yasak [S012, 2.0 metni]. ÇIKARIM: TikTok videosu dünyaya açık olduğu için bu aileden varlık kullanmayın; A12 denetlesin (HY-Motion için de, bkz. A03-S031).
- **UniRig:** otomatik iskelet + ağırlık, ≥8 GB VRAM, glb/fbx/vrm; yüz rig'i yok; lisansı README'de yazmıyor [S020]. Otomatik yüz rig'i araştırması yeni (OmniFaceRig, özet) [S034].
- **Gaussian splatting (sahne):** Unity eklentisi URP'de Unity 6+ ile çalışıyor ama Windows'ta **DX12 ya da Vulkan** istiyor, DX11 yok [S013, ayna]. Proje şu an Direct3D11. ÇIKARIM: sabit arka plan için denenebilir; API değişikliği + tümleşik kartta hız testi gerekir; splat içine pişmiş ışık karakter ışığıyla uyuşmayabilir, gölge/yeniden ışıklandırma zor.
- **Marble (World Labs):** metin/görselden 3D dünya; aktarım ücretli planda; dokulu GLB mesh ve ticari hak Pro'da [S026]. Bulut, D2 aracı.
- **Gaussian avatar:** LAM (SIGGRAPH 2025) tek görselden canlandırılabilir kafa, 1,4 sn kurulum, mobilde 110+ FPS (depo iddiası); kod Apache-2.0, ağırlık lisansı ayrı ve okunmadı; Unity desteği yok; yalnız kafa [S019]. Tam gövde, ışığa uyum, kıyafet, etkileşim hâlâ araştırma (ÇIKARIM). Girdi gerçek bir kişinin fotoğrafıysa rıza sorunu (A12).
- **NeRF:** ayrıca aranmadı. ÇIKARIM: gerçek zamanlı motor entegrasyonu açısından splatting daha uygulanabilir aday; bu iddiayı kaynakla doğrulamadım.
- **Video-to-3D / mocap:** A03 inceledi (GVHMR ticari değil, SMPL lisans riski); tekrar aranmadı.

### S5 AI animasyon, sinematografi, sahne, procedural
- Animasyon/yüz: Kimodo, HY-Motion, Audio2Face-3D ayrıntıları A02/A03/A07'de; hepsi NVIDIA/yüksek VRAM, bu PC'de değil.
- Sinematografi: **FilmFlow** (Ekim 2026) ajan sistemi metinden/referans videodan 3D sahnede kamera yolu üretiyor, render'a bakıp düzeltiyor; Blender; plan başı ~2,2 dk ve 69,5 bin token; tek plan; kod "yayımlanacak" [S015]. ÇIKARIM: fikir bugün bize uyarlanabilir: **kural tabanlı plan yöneticisi + önizleme karesine bakan denetçi** (`onizleme_goruntu` komutu zaten var). Bu video üretici değildir. A01/A08 ile ortak.
- Dil güdümlü kadraj araştırması da var (Auteur, özet) [S033]; video üretim odaklı.
- Procedural: **Infinigen Indoors** BSD-3, Blender tabanlı iç mekân, FBX/USD; GPU hızlandırma yalnız NVIDIA, Windows deneysel [S029]. ÇIKARIM: Unity'ye elle aktarım ve malzeme düzeltme ister; mevcut kodla üretilen mekânlara alternatif aday (A05).

### S6 Diğer sektörler
- Mimari görselleştirme: "klasik render + yalnız zayıf öğeleri (insan, bitki) AI ile iyileştir" [S031]. DLSS 5'in maskeleri de aynı mantık [S001]. ÇIKARIM: bizim için en az riskli hibrit biçimi **maskeli ve düşük yoğunluklu** iyileştirme olurdu; yine D3/D4 sorusu geçerli.
- Virtual production: ayrıca aranmadı (arama hakkı). ÇIKARIM: LED sahne bizi ilgilendirmez; "previz → son görüntü aynı sahne verisinden" disiplini ilgilendirir. Kanıtsız.

---

## Q-004: bu görünüm 3D yolla mümkün mü?
| Hedef | 3D (yapay zekâ video üreticisiz) | Dayanak |
|---|---|---|
| Daha iyi kurgu, doğru altyazı, yürüyen karakter, gölge/ışık | evet (bu PC'de büyük ölçüde) | A01/A03/A04/A07/A09; test gerek |
| İyi oyun sinematiği | evet, güçlü kart + iyi varlık + emekle | VideoKalitesi, A09; ÇIKARIM |
| Örnek videodaki fotogerçekçi insan, ejderha, kalabalık; otomatik ve ucuz | bugün **büyük olasılıkla hayır** | ÇIKARIM; örnek AI üretimi; aksini gösteren kaynak bulmadım |
Film efekt sektörünün 3D ile fotogerçeklik üretmesi bu hedefin ilkece imkânsız olmadığını düşündürür, ama orada ekip, varlık ve render bütçesi vardır (ÇIKARIM; bu oturumda kaynakla araştırılmadı).

**1–3 yıl senaryoları (hepsi ÇIKARIM, güven düşük–orta)**
- 2027: Unity 7 / URP'de GI ve sinirsel yükseltme; görüntü iyileşir, "gerçek film" olmaz.
- 2027–2028: DLSS 5 benzeri kare iyileştiriciler başka motorlara/açık modellere yayılabilir; RTX 50+ sınıfı kartla D3 yolu açılabilir. Unity desteği için duyuru görmedim.
- 2028–2029: motor koşullu (G-buffer) video modelleri tüketici kartında kimlik tutarlılığıyla çalışabilir; bu, 3D kontrol ile fotogerçekliği birleştiren en güçlü aday. Gecikebilir ya da lisansla kısıtlanabilir.
- Her senaryoda değerini koruyan: hikâye sözdizimi, plan listesi, kamera/oyuncu yönetimi, ses-zaman verisi, temiz lisanslı 3D varlık.

---

## Teslim: üç kategori
"Bugün" = ürün/kod yayımlanmış; **bizde denendiği anlamına gelmez.**

### 1) BUGÜN KULLANILABİLİR
| Teknoloji | Kanıt | Donanım | Maliyet | Lisans | Şart / not |
|---|---|---|---|---|---|
| URP'nin kullanılmayan temel özellikleri (gölge, HDR, post) | A04, A09 | mevcut PC (hız testi gerek) | 0 | Unity | ilk iş; yapay zekâ değil |
| Kural tabanlı otomatik yönetmen + önizleme denetimi | S015'ten fikir | mevcut PC | 0 | kendi kodumuz | D0 |
| Infinigen Indoors | S029 | CPU/NVIDIA; Windows deneysel | 0 | BSD-3 | aktarım emeği |
| TRELLIS.2 (aksesuar/dekor) | S011 | ≥24 GB NVIDIA, Linux (kiralık) | kiralama (A10) | MIT; bağımlılıklar denetlenmedi | D2 |
| UniRig | S020 | ≥8 GB NVIDIA | 0 | DOĞRULANAMADI | lisans netleşmeden kullanma |
| Marble dünyaları | S026 | bulut | ücretli (tutar görülmedi) | ticari hak Pro | D2 |
| Splat arka plan (Unity) | S013 | DX12/Vulkan | 0 | eklenti lisansı bu oturumda doğrulanmadı; eğitim kodu lisansı ayrı | paket için kullanıcı onayı |
| Bulut V2V (LTX, fal VACE) | S023, S024 | bulut | yukarıdaki fiyatlar | sağlayıcı/model | **teknik olarak hazır ama D4: şarta aykırı (ÇIKARIM)** |

### 2) DENEYSEL
| Teknoloji | Kanıt | Donanım | Maliyet | Lisans | Engel |
|---|---|---|---|---|---|
| Unity 6.7 Surface Cache GI, SSR, Unity Neural | S014 | URP; resmî gereksinim yok | 0 (Unity) | Unity | alpha/önizleme; proje 6000.6 |
| G-buffer koşullu "neural renderer" (NeuralSRender, DiffusionRenderer) | S005–S007 | 13,67 GB+ / 24 GB sınıfı | kart ya da bulut | NeuralSRender belirtmemiş | insan tutarlılığı kanıtsız; D4 |
| Yerel açık video modelleri (LTX, Wan, VACE) | S008–S010, S025 | NVIDIA 12–24 GB+; bazıları çoklu GPU | kart + süre | LTX Community / Apache | D4; yavaşlık, kayma |
| Gaussian avatar (LAM, ANI3DHUMAN) | S019, S027 | CUDA; 48 GB (ANI3DHUMAN) | kart | LAM ağırlık lisansı okunmadı | Unity yolu yok |
| Ajan tabanlı sinematografi (FilmFlow) | S015 | Blender + büyük LLM | token | kod yok/belirsiz | tek plan |
| Metinden harekete, ses→yüz | A03, A07 | NVIDIA | 0 + donanım | karışık | temizlik ister |

### 3) İZLENECEK
| Konu | Neden | Harekete geçme koşulu |
|---|---|---|
| DLSS 5 SDK / Unity eklentisi [S001, S002] | D3'ün en olgun adayı; RTX 50 | resmî Unity ya da genel SDK + kare yakalamada kullanım izni + yüz tutarlılığı testi |
| Unity 7 (Aralık 2026 önizleme) [S004] | render yönü | tam sürüm + örnek sahnede ölçüm; "kırılma yok" iddiasını kopya projede sına |
| Cosmos 3 aktarım yetenekleri [S022] | kontrollü sim-to-real | belgelenmiş derinlik/seg v2v + tek tüketici kartı |
| Wan-Animate-2-Lite [S009] | gerçek zamanlı iddiası | ağırlıkların yayımlanması |
| Canlandırılabilir, yeniden ışıklandırılabilir splat avatar | dijital insan | Unity çalışma zamanı + ticari lisans |
| Otomatik yüz rig'i [S034] | yeni karakterde dudak senkronu | açık kod + lisans |
| Platform kuralları ve model lisansları | hızlı değişiyor | her gözden geçirmede |

---

## Mimari açıklığı (öneri; kod yazılmadı)
Mevcut güçlü yan: metin tek doğruluk kaynağı, `Core/Arayuzler.cs` (IVideoKaydedici, IVarlikKatalogu), glTFast kurulu (A09, proje dosyaları). Eklenmesi gerekenler (ÇIKARIM):
1. **Plan manifesti** (JSON): her plan için süre, kamera (konum, lens, odak), oyuncular, animasyon klibi, ses zaman damgaları, tohum (seed). Hikâye metninden üretilir; motor ya da araç değişse korunur. En değerli adım.
2. **Kare kesin, tekrarlanabilir kayıt:** aynı manifest → aynı kareler (A09'daki kare atlama düzeltilmeli; sabit tohum).
3. **İsteğe bağlı ek geçiş (AOV) dışa aktarımı:** derinlik, normal, albedo, hareket vektörü, karakter başı kimlik maskesi. Neural renderer girdileri bunlar [S001, S005, S021]. Hemen yazmak şart değil; render yolunda yeri bırakılmalı.
4. **Takılabilir kare son işlemcisi** (`IKareIslemci`, varsayılan: hiçbiri): kareler diske → harici adım → ffmpeg. Harici model Unity sürecine gömülmez.
5. **Sağlayıcı arayüzleri:** hareket, yüz/dudak (A07 önerisiyle aynı), varlık içe aktarma (glTF/FBX).
6. **HDR/linear görüntü hattı** (A09 K2): hem klasik post hem sonraki işlemler için.
7. **Köken kaydı:** her varlık ve plan için kaynak, lisans, tarih, "yapay zekâ ile üretildi/değiştirildi" alanı (etiket ve lisans denetimi için).
8. Sürüm sabitleme; Unity yükseltmesi yalnız kopya projede.

## Takip düzeni (protokol: gelecek gelişmeleri izleme)
- Dosya önerisi: `TOPICS/TAKIP_teknoloji.md` (ana araştırmacı açsın). Alanlar: ad · kategori (İzle/Deneysel/Kullan/Elendi) · son kontrol · sonraki kontrol · resmî kaynak · tetikleyici · D düzeyi · donanım · lisans · karar.
- Sıklık: 3 ayda bir (en çok 10 arama), artı tetikleyiciyle. İlk kontrol **2027 Ocak** (Unity 7 önizleme), sonra Nisan (GTC sonrası), Eylül (SIGGRAPH sonrası; bildiri listesi de taransın), Aralık/Ocak.
- İzlenecek yerler: Unity blog/roadmap, NVIDIA developer blog, Hugging Face model kartları ve LICENSE dosyaları, arXiv cs.GR/cs.CV, SIGGRAPH ve SIGGRAPH Asia programları, TikTok yardım sayfası.
- Yükseltme kapısı (hepsi şart): resmî kaynak · lisans (A12) · eldeki donanımda çalışma · "altın sahne" testi · kullanıcı onayı (D3/D4 için ayrıca açık onay).
- **Altın sahne** (her seferinde aynı 5 plan): yüz yakın plan, iki kişilik diyalog, yürüme, geniş kalabalık, gece/ateş. Ölçüm: video saniyesi başı süre, tepe VRAM, maliyet, planlar arası yüz/kıyafet tutarlılığı, titreme, kullanıcının kör karşılaştırma puanı.
- Zamanlanmış görev kurulmadı; kullanıcı isterse kurulabilir.

## Teknoloji kayıtları (kısa; doğrulama tarihi 2026-11-27, test durumu hepsinde: yapılmadı)
1. **DLSS 5** · neural rendering · ışık/malzeme ekler · renk+hareket vektöründen model · katkı: fotogerçekliğe yaklaştırma · Unity: duyuru yok · RTX 50 · SDK lisansı sayfada yok · artı: motor karesine bağlı · eksi: yüz değiştirme riski · alternatif: G-buffer difüzyonu · S001–S003, S032 · güven A (satıcı)/B.
2. **Unity Neural + Surface Cache GI** · render · sinirsel yükseltme/dinamik GI · Sentis + URP · dolaylı ışık · 6.7+ · resmî gereksinim yok · Unity lisansı · artı: yerleşik · eksi: önizleme · alternatif: pişmiş ışık · S014 · A.
3. **LTX-2.5** · video üretici · t2v/i2v/v2v+ses · difüzyon · kıyas · Unity: harici · CUDA · <10 M$ ücretsiz · artı: açık ağırlık · eksi: D4 · alternatif: Wan · S008, S023 · A.
4. **Wan VACE / Animate-2** · video üretici/düzenleyici · referans+hareketle video · difüzyon · kıyas · harici · güçlü NVIDIA · Apache-2.0 · eksi: D4, 480–720p · S009, S010 · A.
5. **TRELLIS.2** · üretici 3D · görselden PBR mesh · aksesuar · GLB → glTFast · 24 GB · MIT · eksi: delik, rig yok · alternatif: CC0 varlık · S011 · A.
6. **3DGS Unity eklentisi** · sahne temsili · fotoğraf gerçekçi sabit mekân · nokta bulutu rasterleştirme · arka plan · URP, DX12/Vulkan · GPU compute · lisans doğrulanmadı · eksi: ışık uyumu · S013 · C.
7. **LAM** · avatar · tek görselden kafa · Gaussian · deneysel yüz · Unity yok · CUDA · Apache-2.0 kod · eksi: yalnız kafa · S019 · A.
8. **FilmFlow** · AI sinematografi · kamera yolu · LLM ajanları · fikir uyarlama · Blender · LLM maliyeti · kod yok · S015 · A (ön baskı).
9. **Infinigen Indoors** · procedural · iç mekân · Blender · mekân çeşitliliği · FBX/USD · NVIDIA/CPU · BSD-3 · S029 · B.

## Karşılaştırma ve çelişkiler
- Ç1: DLSS 5 "sanatçı niyetini korur, deterministik" (satıcı) ↔ yüz değiştirme gözlemi (Mart) [S001 ↔ S003]. AÇIK. Not: "aynı girdi aynı çıktı" ile "yüz aslına benziyor" farklı özelliklerdir.
- Ç2: 26 Kas "UE5 en yakın yol": DLSS 5'in UE5 eklentisi olması UE yönünü güçlendiriyor, ama ÇIKARIM: tek başına motor değiştirme gerekçesi değil (yeniden yazım büyük; Unity'de karelerin harici işlenmesi mümkün).
- Ç3: Donanım: VideoKalitesi "en az RTX 3060 12GB". 3060, DLSS 5'i çalıştırmaz (yalnız RTX 50) [S001]. **A10'a:** kart alınacaksa RTX 50 serisi 16 GB sınıfı geleceğe daha açık; 24 GB isteyen araçlar yine bulut ister. (Fiyat araştırmadım.)

## Riskler
- Neural katman karakter kimliğini bozabilir.
- "Neural renderer" adıyla video üreticinin fark edilmeden şarta aykırı biçimde girmesi.
- Model/depo hızla terk ediliyor (Cosmos 2.5 örneği [S021]); lisanslar değişebilir; bölge kısıtları [S012].
- Satıcı iddialarına dayanma; ön baskılar hakemsiz olabilir.
- Gelecek teknolojiyi beklerken bugünkü P1 kusurların (altyazı, hareket) ertelenmesi.

## Test önerileri
- Bu bilgisayarda: (T1) plan manifesti taslağı + aynı manifestten iki kayıt karşılaştırması; (T2) tek planda derinlik + karakter maskesi dışa aktarma denemesi; (T3) küçük bir splat sahnesi DX12'de (kullanıcı onayıyla paket).
- GPU/bulut (kullanıcı onayı ve ödeme): (T4) altın sahnenin Unity render'ı vs maskeli D4 iyileştirme, kör karşılaştırma. Önce kullanıcı D3/D4'ü kabul ediyor mu diye sorulmalı; etmiyorsa T4 yapılmaz.

## Öz denetim
- SIGGRAPH 2025/2026 bildiri listelerini sistematik taramadım; SIGGRAPH Asia hiç aranmadı.
- NeRF, virtual production ve motion capture araştırmaları ayrıca aranmadı.
- DLSS 5'in oyun dışı/çevrim dışı kayıtta kullanımı ve lisansı belirsiz; Unity desteği "yok" değil "görmedim".
- Video modellerinin tüketici kartı hız/VRAM verisi zayıf (tek C/D anlatım).
- 3DGS Unity eklentisi ayna sayfadan okundu; Inria 3DGS eğitim kodu lisansı bu oturumda doğrulanmadı.
- UniRig, LAM ağırlıkları, TRELLIS.2 bağımlılık lisansları denetlenmedi.
- Örnek videonun hangi araçla yapıldığı bilinmiyor.

## Eksik alanlar / açık sorular
- **Kullanıcıya (yeni soru önerisi Q-A11-1):** "Unity'nin ürettiği kareyi bir yapay zekâ modeliyle gerçekçileştirmek (D3 kare iyileştirici / D4 video-to-video) kabul mü, yoksa bu da 'yapay zekâ video üreticisi' mi?" Q-004'ün cevabı buna bağlı.
- Q-A11-2: Daha stilize (çizgi/boyama benzeri) tutarlı bir görünüm kabul edilebilir mi? ÇIKARIM: fotogerçeklik hedefinden daha ulaşılabilir olabilir; araştırılmadı.
- DLSS 5'in Unity'ye gelip gelmeyeceği.

## Diğer ajanlardan beklenen / ortak konular
- A10: RTX 50 vs 30 (Ç3), bulut V2V maliyeti. A12: Hunyuan bölge/çıktı kısıtı, TTS+TikTok etiketi, splat/LAM/UniRig lisansları. A09: manifest/AOV/kare işlemci uygunluğu. A01/A08: FilmFlow benzeri denetçi döngüsü. A02: kimlik tutarlılığı ölçütü. A05: Infinigen/Marble/splat.

## Öneriler
1. Önce 3D yolun bedava kazanımları (P1 kusurlar, gölge/HDR, kurgu); gelecek teknolojisi bunları ikame etmez (ÇIKARIM; dayanak S002, A09).
2. Kullanıcıya dürüst söyle: örnek videodaki görünüm 3D ile garanti edilemez; yaklaştıracak yolların güçlüleri üretici yapay zekâdır.
3. Q-A11-1 sorulmadan D3/D4'e yatırım yapma.
4. Mimaride 1, 2, 7 numaralı maddeleri erken al; 3–4 için yer bırak.
5. Kart alımında DLSS 5 / yeni model desteğini ölçüt yap; yalnız bunun için kart alma.
6. Üç aylık takip düzenini kur; ilk kontrol Ocak 2027.
