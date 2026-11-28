# D10 — Dünyada benzer uygulamalar: Hikaye Stüdyosu'nun yerine veya yanında ne kullanılabilir?

Tarih: 2026-11-28 · Yöntem: yalnızca web araştırması (30 arama + sayfa okumaları). **Hiçbir şey indirilmedi, kurulmadı, test edilmedi.** Lisans özetleri sayfalardan alındı. Bu belge hukuki görüş değildir; para kazanmaya başlamadan önce asıl metni okuyun.
Ön okuma: `D4_ucretsiz_yollar.md`, `D7_bedava_karakterler.md` (MetaHuman, MPFB, Renderpeople, Reallusion tabanları, Daz, Mixamo, CC denemesi orada; burada tekrar edilmedi).

Kullanıcı bağlamı: Türkiye, bütçe en çok 1000 TL (~19 USD), AI video üretici yok (metin/ses için AI serbest), Ryzen 5 5600GT + tümleşik Vega 7, 64 GB RAM, Windows 10, uzak masaüstü. Unity 6 çalışıyor, UE5 çöküyor. Hedef: 1080x1920 dikey hikaye videosu, gerçekçi/sinematik 3D karakter.

İşaretler: **[D]** = bu oturumda sayfadan doğrulandı · **[K]** = kısmen doğrulandı / ikincil kaynak · **[B]** = doğrulanmadı (genel bilgi veya çıkarım).
Puanlar: Gerçekçilik/sinematiklik 1–5 (5 = film/AAA). Zayıf PC: İyi / Orta / Zayıf.

---

## 0. Kısa sonuç

**Ana bulgu:** Dünyada "hikaye metni yaz → 3D sahne otomatik kurulsun → dikey video çıksın" işini **AI kullanmadan, zayıf PC'de, bedava** yapan hazır bir uygulama **bulunamadı**. Bu işi yapan eski araçlar (Plotagon Studio, Muvizu, Moviestorm) ya kapandı ya da 2013–2014'ten beri güncellenmiyor. Yeni çıkanların çoğu (Toonstar, Reallusion AI Studio, Çin'in "小说推文" araçları) AI tabanlı. Bu yüzden **Unity uygulamamız ana hat olarak kalmalı**; diğer araçlar **yanında** kullanılmalı.

### Bu kullanıcı için ilk 5 (Unity uygulamasının yanında)

| Sıra | Araç | Ne için | Gerçekçilik | Vega 7 | Fiyat | Para kazanma lisansı | Otomasyon / bizim hatla bağlantı |
|---|---|---|---|---|---|---|---|
| 1 | **Blender (Eevee) + MPFB** | Sinematik ana planlar, kapak görseli, Unity'nin zayıf kaldığı yakın çekimler | 4 | Orta (çevrimdışı render, yavaş ama olur) [B] | 0 | Çıktı tamamen sizin (GPL araç, CC0 MPFB içerik) [K] | **Yüksek**: Python (bpy) ile metinden sahne kurulabilir; aynı karakter FBX'leri Unity ile ortak |
| 2 | **The Sims 4** (temel oyun ücretsiz) + ReShade + poz/kamera modları | Ayrı bir "Sims hikaye" kanalı; TikTok'ta kanıtlanmış format | 2–3 (ReShade ile 3) | İyi [B] | 0 (ek paketler ücretli) | EA: YouTube/Twitch ortaklık geliri **serbest**; Patreon/ücretli erişim yasak [D] | **Düşük**: elle poz/kamera; seslendirmeyi bizim Gemini hattımız yapar |
| 3 | **VRoid Studio + UniVRM → Unity** | Anime tarzı ikinci karakter seti / kanal | 1–2 (anime) | İyi | 0 | Çıktı modeller ticari serbest [D] | **Yüksek**: VRM, Unity'ye doğrudan girer; 52'ye yakın yüz ifadesi/visem (VRM standart) [B] |
| 4 | **iClone Personal** (2026-06'dan beri ücretsiz) | Animasyon/dudak senkronu düzenleyip FBX olarak Unity'ye aktarmak | 4 (iClone içinde) | Belirsiz [B] | 0 | **iClone'un kendi render çıktısı yalnız kişisel**; ticari kullanım "izinli" ama ömür boyu **30 dışa aktarım** [D] | Orta: elle düzenle → FBX → Unity'de render. EULA'yı mutlaka okuyun |
| 5 | **Cascadeur** (Free öğrenmek için; Indie 1 ay = ~19 USD) | Fizik destekli, doğal karakter animasyonu üretip FBX'le Unity'ye almak | — (animasyon aracı) | Orta [B] | Free 0 / Indie $19/ay veya $96/yıl [D] | Free: **ticari değil, FBX yok**; Indie: yıllık gelir < $100k ise ticari [D] | Orta: animasyon kütüphanesini 1 ayda üretip sakla |

**Kaçınılması önerilen (para kazanacaksanız):** GTA V / Rockstar Editor / FiveM / RDR2 (Rockstar'ın güncel politikası "ticari olmayan = para kazanmayan" diyor [D]); SFM/S2FM'de Valve dışı "workshop" modelleri (çoğu başka oyunlardan sökülmüş, telif riski); MMD'de hazır modeller (çoğu ticari yasak); Koikatsu/Honey Select (yetişkin içerik, şirket kapandı).

---

## 1. Oyun tabanlı (machinima) araçlar

### 1.1 The Sims 4 ★
- **Görsel kalite:** Stilize (çizgi-gerçekçi arası) 2/5; ReShade önayarları + poz modlarıyla 3/5. Sims topluluğunda sinematik dizi çeken içerik üreticileri var: 2026-08 tarihli yazıda kullanılan araçlar: ReShade ("Pearl" önayarı), poz paketleri + poz oynatıcı, Teleport Any Sim, T.O.O.L (eşya yerleştirme), otonomiyi kapatma, plumbob gizleme [D] https://simscommunity.info/2026/08/20/the-sims-4-can-look-this-good-how-nishaimansims-does-it/ · Poz oynatıcı rehberi: https://snootysims.com/wiki/sims-4/how-to-use-the-andrew-pose-player/
- **Vega 7:** Sims 4 düşük donanım için tasarlandı; tümleşik GPU'da oynanır [B]. ReShade efektleri FPS düşürür; kayıt için kabul edilebilir.
- **Fiyat:** Temel oyun 2022 Ekim'den beri ücretsiz [B]. Genişleme paketleri ücretli (bütçeyi aşabilir).
- **Lisans (EA içerik politikası) [D]** https://help.ea.com/en/articles/security-and-rules/ea-content-policy/ :
  - "Videoların ortaklık programları (YouTube veya Twitch gibi) üzerinden para kazanması" **serbest**; pasif reklam serbest.
  - Yasak: oyun içeriğini satmak, videoları Patreon gibi ücretli duvarın arkasına koymak, EA sponsorluğu izlenimi vermek, hile göstermek, spoiler.
  - Oyundaki üçüncü taraf müzik için izin verilmez → kendi/telifsiz müziğinizi kullanın.
  - Modlar genelde yasak ama **The Sims 4 için istisna** (kendi kılavuzlarıyla).
  - TikTok adı geçmiyor; "ortaklık programı" ifadesinin TikTok Creator Rewards'ı kapsadığı varsayılıyor ama yazılı değil [K]. EA politikayı haber vermeden değiştirebilir.
- **Büyük kanallar nasıl yapıyor:** Oyunda sahneyi kur → poz modu ile karakterleri dondur → serbest kamerayla çek → kurgu programında üzerine dış ses + altyazı. Sims karakterleri "Simlish" konuştuğu için seslendirme tamamen sonradan eklenir — bu, bizim **Gemini seslendirme + altyazı** hattımızla birebir uyumlu [B].
- **Otomasyon:** Sims 4 Python ile modlanabilir bir oyun [B]; teorik olarak bir mod hikaye dosyasından poz/kamera tetikleyebilir ama hazır araç yok, emek büyük. Pratikte: **Unity uygulamamızdan yalnız ses + altyazı + kurgu kısmını** (seslendir, ses dosyaları, altyazı zamanlaması) yeniden kullanmak mümkün.

### 1.2 GTA V — Rockstar Editor, FiveM; Red Dead Redemption 2
- **Görsel kalite:** GTA V 4/5 (2013 tabanlı ama gerçekçi şehir, araçlar); RDR2 5/5.
- **Vega 7:** GTA V Legacy sürümü 720p–900p düşük ayarda oynanabilir [B]; RDR2 çok zayıf [B]. RDR2 PC'de Rockstar Editor **yok** (yalnız fotoğraf modu) [B].
- **Fiyat:** GTA V sık sık indirimde, ama 1000 TL sınırına yakın/üstünde olabilir [B].
- **Lisans — ÖNEMLİ [D]:** Rockstar'ın güncel "telifli materyal yayınlama" politikası hayranlara oyun materyalini **ticari olmayan** amaçla kullanma izni veriyor ve "ticari olmayan"ı **"paylaştığınız görüntüden para kazanmamak"** olarak tanımlıyor. Yani para kazanılan video bu iznin **dışında** kalıyor; Take-Two "her zaman kaldırtma hakkını saklı tutuyor". Ara sahneleri tek başına yayınlamak, sızıntı, hile, izinsiz mod tanıtımı kaldırılır. https://support.rockstargames.com/articles/200153756/rockstar-games-video-policy
  - Rockstar Editor'deki GTA V radyo şarkıları YouTube'da kanal ihlali (strike) getirmez ama hak sahipleri o videodan para alabilir veya ülke bazlı engelleyebilir; enstrümantal skor önerilir [D] https://support.rockstargames.com/articles/3f8XH01Kodw4zKnwbF6yTB/information-about-being-safe-from-copyright-strikes-when-uploading-rockstar-editor-videos
  - Pratikte binlerce GTA kanalı para kazanıyor ve Rockstar genelde dokunmuyor — ama bu **hoşgörü**, izin değil [B].
- **FiveM/RedM:** Rockstar 2023'te Cfx.re'yi satın aldı. 2026 Eylül'de yayımlanan mod kuralları: tek oyunculu mod serbest ama resmi hikaye/karakteri değiştirmek, başka IP ve gerçek kişi benzerliği yasak; çok oyunculu mod yalnız lisanslı platformda; modlar ticari olmayan olmalı [K] https://www.digitalcitizen.life/rockstar-mod-guidelines-limit-multiplayer-modding-creator-platform/
- **Sonuç:** Görsel en iyi seçeneklerden biri ama **para kazanılan TikTok için hukuken en riskli**. Önerilmez.

### 1.3 Source Filmmaker (SFM) ve Source 2 Filmmaker (S2FM)
- **SFM (Source 1):** Steam'de ücretsiz; eski motor, Vega 7'de rahat çalışır [B]. Görsel 2–3/5 (iyi ışıkla 3). Python betik arabirimi var [B] → teorik olarak otomasyon.
- **S2FM:** Dota 2, Counter-Strike 2, Half-Life: Alyx ve SteamVR Home "Workshop Tools" DLC'leriyle geliyor; yalnız 64-bit [D] https://developer.valvesoftware.com/wiki/Source_2_Filmmaker · Görsel 3–4/5. Vega 7'de ağır olabilir; resmi GPU gereksinimi sayfada yok [B].
- **Lisans [D]:** SFM abonelik şartları: aracın kendisi ticari veya ticari olmayan kullanılabilir; **Valve oyun varlıkları "kişisel, ticari olmayan kullanım" içindir**; jeneriğe "Created using the Source® Filmmaker. © Valve" yazılmalı. https://www.sourcefilmmaker.com/sfm_subscription_agreement
  - Valve 2013 açıklaması: SFM filmleri dahil videolardan **YouTube Partner veya diğer servislerle para kazanabilirsiniz**, ama izleyiciden ücret alınamaz, video satılamaz/lisanslanamaz; Valve müziği/sesi ayrı kullanılamaz [K] (2013-12-13 haberi) https://overclockers.ru/softnews/show/57681/Valve_My_ne_protiv_ispolzovaniya_kontenta_iz_nashih_igr_v_YouTube
  - **Asıl risk:** Workshop'taki karakterlerin çoğu başka oyunlardan (Capcom, Bethesda…) sökülmüş; onlarla para kazanmak telif ihlali [B]. Valve karakterleri (TF2, HL, CS) ise hikayemize uymaz. Kendi MPFB/Renderpeople karakterlerinizi SFM'e almak mümkün ama zahmetli — o zaman Unity zaten daha iyi.
- **Sonuç:** Ücretsiz ve iGPU'ya uygun ama karakter kaynağı sorunu yüzünden bizim için Unity'den üstün değil.

### 1.4 Garry's Mod
- Görsel 1–2/5; ~$10 [B]; Vega 7'de çalışır [B].
- **Facepunch hayran içeriği kuralları (son güncelleme 2025-07-15) [D]:** video/yayın platformlarında **standart gelirle para kazanmak serbest**; başka yollar yazılı onay ister; kurallar yalnız Facepunch'ın sahip olduğu kısımlar için geçerli; GMod'daki kullanıcı yapımı içerik ve üçüncü taraf IP **kapsam dışında**. https://facepunch.com/legal/ugc
- 2025'te Skibidi Toilet stüdyosunun GMod'a DMCA göndermesi gibi telif tartışmaları var [K] https://shacknews.com/article/140768/garrys-mod-skibidi-toilet-dmca
- Sonuç: Komik/meme kanalı için olur; gerçekçi hikaye için değil.

### 1.5 Roblox (Brookhaven hikayeleri vb.)
- Görsel 1/5 (blok/stilize); ücretsiz; iGPU'da iyi [B]. TikTok'ta "Roblox hikaye" türü çok büyük [B].
- Lisans: Roblox video içeriği "Roblox Kullanım Şartları ve Topluluk Standartlarına uygun olmalı"; Roblox Video Stars programı (büyük üreticiler için komisyon, rozet; başvurular kapalıydı) [D] https://en.help.roblox.com/hc/articles/360026092011 · Genel video para kazanma izninin metni bu oturumda bulunamadı [B].
- Otomasyon: Roblox Studio (Lua) ile kendi "deneyiminizde" betikli sahne kurulabilir [B] — ama sonuç Roblox görünümü.
- Sonuç: Gerçekçilik hedefine uymuyor.

### 1.6 Minecraft (+ Mine-imator)
- Görsel 1/5 (gölgelendirici paketleriyle "sinematik" ama bloklu). iGPU'da iyi. Mine-imator ücretsiz Minecraft tarzı animasyon programı [B].
- Lisans: Minecraft Kullanım Kılavuzu videolardan para kazanmaya izin veriyor; videoda "yeterli özgün içerik" şartı var [K] https://conductatlas.com/platform/minecraft/minecraft-usage-guidelines/ (ikincil kaynak; resmi metin minecraft.net/usage-guidelines'ta okunmalı).
- Sonuç: Gerçekçilik hedefine uymuyor.

### 1.7 Second Life, VRChat, Avakin Life (sosyal 3D dünyalar)
- Second Life: yarı gerçekçi avatarlar (3/5), ücretsiz, machinima politikası var [B]. iGPU'da orta [B]. Diğer kullanıcıların içeriklerinin izinleri karışık.
- VRChat'in "Video Content Guidelines" sayfası var [K] https://hello.vrchat.com/video-content-guidelines (okunmadı).
- Sonuç: Kontrol (kamera, ışık, tekrar çekim) zayıf; otomasyon yok.

---

## 2. Özel hikaye/animasyon araçları

| Araç | Durum (2026-11) | Gerçekçilik | Vega 7 | Fiyat | Para kazanma | Otomasyon / entegrasyon |
|---|---|---|---|---|---|---|
| **iClone Personal** | Yaşıyor; 2026-06'da ücretsiz sürüm [D] | 4 | Belirsiz [B] | 0 (tam sürüm $599, yıllık $199) [D] | Ticari kullanım izinli; **yerleşik ve RTX render çıktısı yalnız kişisel**; ömür boyu **30 dışa aktarım** [D] | Animasyonu FBX olarak Unity'ye aktar, Unity'de render et (EULA kontrol) |
| **Cartoon Animator** (Reallusion) | Yaşıyor | **2D** | İyi [B] | Ücretli [B] | — | 3D hedefe uymuyor |
| **Muvizu** | Son sürüm 2013 (Play+, £24,99) [K] | 1–2 | — | — | — | Ölü proje |
| **Moviestorm/"Filmmaker"** | Son sürüm 1.6.1, 2014-01-16 [K] https://en.wikipedia.org/wiki/Moviestorm | 1–2 | — | — | — | Ölü proje |
| **Plotagon Studio** (masaüstü) | **Kapandı**, sunucular kapatıldı; mobil "Plotagon Story" öneriliyor [K] https://forums.justuseapp.com/en/post/3ZAN5LHSZ6/shutdown-permanently | 1–2 | — | — | — | Metinden sahne fikri bizimkine en yakın ama kapandı |
| **Toonstar** | AI tabanlı stüdyo [B] | — | — | — | — | Kullanıcının "AI video yok" kuralına aykırı |
| **Adobe Character Animator** | Yaşıyor; Creative Cloud aboneliği [B] | **2D kukla** | İyi | Bütçe üstü [B] | Adobe şartları | Uymuyor |
| **Cascadeur** | Yaşıyor [D] https://cascadeur.com/plans | Animasyon aracı | Orta [B] | Free / Indie $19 ay, $96 yıl | Free: ticari değil, yalnız .casc, 300 kare, 120 eklem; Indie: FBX/DAE, gelir < $100k [D] | FBX → Unity Humanoid. AutoPosing AI destekli poz yardımcısıdır (video üretmez) [B] |
| **MikuMikuDance (MMD)** + ray-MMD | Son sürüm 9.32 (2019-12-10); ücretsiz [K] https://en.wikipedia.org/wiki/MikuMikuDance | Anime 2; ray-MMD ile 3 | MMD iyi, ray-MMD ağır [B] | 0 | Program ücretsiz ama **modellerin lisansı ayrı**; Vocaloid modelleri izinsiz ticari kullanılamaz [K] | PMX modeller Unity'ye alınabilir [B] ama lisans yüzünden önerilmez |
| **Koikatsu / Honey Select** (Illusion) | Şirket 2023-08 kapandı; ILLGAMES ayrı şirket [K] https://en.wikipedia.org/wiki/Illusion_(company) | 2–3 | Orta | — | Yetişkin (18+) oyunlar; TikTok için uygun değil | Önerilmez |
| **VRoid Studio** | Yaşıyor; ücretsiz [B] | 1–2 (anime) | İyi | 0 | Çıktı modeller ticari/ticari olmayan serbest [D] https://scrapbox.io/VRoid/VRoid_Studio%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6 | **UniVRM ile Unity'ye doğrudan** [B] |
| **Godot** | Yaşıyor; MIT | Unity'den düşük (karakter/ışık ekosistemi zayıf) [B] | İyi | 0 | Serbest | Unity zaten çalışıyor; geçmek anlamsız |
| **Blender (Eevee)** | Yaşıyor; GPL | 4 (iyi varlık + ışıkla) | Orta: Eevee OpenGL ile iGPU'da çalışır, render yavaş ama çevrimdışı olur [B] | 0 | Çıktı sizin | **Python (bpy)** ile tam otomasyon mümkün |
| **Clip Studio Paint 3D** | Çizim yardımcısı | — | — | Ücretli | — | Video aracı değil |

Not: Vega 7 için ölçülü referans: 5600G'de Battlefield 6 açık betası 1080p düşük + FSR Kalite ~45–50 FPS (Notebookcheck, 2025-08-08) [D] https://www.notebookcheck.net/Battlefield-6-performs-well-even-on-super-weak-iGPU-Ryzen-5-5600G-with-mediocre-Vega-7-runs-Battlefield-6-at-over-45-FPS.1082939.0.html — iGPU'nun düşük ayarda modern oyunları kaldırabildiğini gösteriyor; ama uzak masaüstünde GPU'nun gerçekten kullanıldığından emin olun (D4 §2.2).

---

## 3. Asya ve diğer dil toplulukları

- **Çince** (3D动画短剧 制作软件 低配 / 3D小说推文): Sonuçlar CapCut/Dreamina (AI), Tripo (AI 3D), Guru99 listeleri gibi SEO sayfalarıydı. AI'sız, zayıf PC'ye özel bir Çin 3D hikaye aracı **bulunamadı**. Genel bilgi [B]: "小说推文" (roman tanıtım) videoları çoğunlukla AI görsel + 剪映 (CapCut) kurgu; 3D kısa diziler UE/MetaHuman, iClone/CC veya C4D ile; Bilibili'de MMD yaygın. 剪映 şablonları 2D kurgu şablonudur, 3D karakter üretmez.
- **Korece:** ZEPETO (Naver Z) mobil 3D avatar uygulaması; "제페토 드라마" türü var, Fan Entertainment ile "metaverse drama" iş birliği haberi (2021-12) [K] https://view.asiae.co.kr/en/article/2021120210463932798 · Görsel 1/5 (stilize). Video para kazanma şartı bulunamadı [B].
- **Japonca:** VRoid (yukarıda), MMD, mobil "Sakura School Simulator" (anime, hikaye videolarında popüler) [B]; video kılavuzu bulunamadı. Gerçekçi 3D hikaye için Japonya'ya özgü yeni araç bulunamadı.
- **Rusça, Türkçe, İspanyolca/Portekizce:** Aramalar AI araç tanıtımlarına ve genel "en iyi 3D programı" listelerine çıktı (Blender, Cascadeur, iClone). Yeni araç bulunamadı. Bu topluluklarda pratikte Sims 4, GTA V, Roblox (Brookhaven), Sakura School, Avakin Life hikaye videoları yaygın [B].

---

## 4. Mobil 3D hikaye uygulamaları

| Uygulama | Gerçekçilik | Not |
|---|---|---|
| Plotagon Story | 1–2 | Şablonlu, insan karakterli; ücretsiz sürüm kısıtlı [K] https://freeappsforme.com/best-3d-animation-apps-android |
| AnimeTok 3D | 1 | Zaman çizelgeli poz animasyonu; yoğun reklam [K] (aynı kaynak) |
| Prisma3D | — | Modelleme/animasyon; hikaye aracı değil [K] |
| ZEPETO, Avakin Life, Sakura School Simulator | 1–2 | Sosyal/oyun; kamera ve tekrar çekim kontrolü zayıf; lisanslar belirsiz [B] |

Sonuç: Hiçbiri "gerçekçi/sinematik" hedefe yaklaşmıyor; bizim uygulamanın yerini tutmaz.

---

## 5. Bizim hatla nasıl birleştirilir (öneri)

1. **Unity uygulaması = ana fabrika** (metin → sahne → ses → kayıt otomatik; dünyada benzeri bulunamadı).
2. **Blender yan hat:** Aynı karakter FBX'lerini (MPFB/Renderpeople) Blender'da Eevee ile 3–5 sn'lik "güzel" plan veya kapak görseli olarak render et; Unity videosuna ara plan olarak ekle. İleride `HikayeKomut` benzeri bir Python betiği aynı hikaye dosyasından Blender sahnesi kurabilir (büyük iş; önce elle dene).
3. **VRoid → Unity:** Anime tarzı ikinci kanal için UniVRM paketi (proje kuralı: paket ekleme yok — kaynak kodu Assets altına almak gerekir; lisansı MIT olduğu bilinir [B]). Karakter kütüphanesine VRM türü eklenir.
4. **Animasyon kaynakları:** Mixamo (D7) + Cascadeur (1 ay Indie ile bir animasyon paketi üret) + iClone Personal (30 dışa aktarım hakkını en önemli animasyonlara harca). Hepsi FBX → Unity Humanoid → mevcut hareket/poz sistemi.
5. **Sims 4 ayrı kanal:** Unity'den bağımsız elle çekim; ama **seslendirme, altyazı ve ses seviyesi (LUFS) kısmı bizim uygulamadan** yeniden kullanılabilir (Gemini sesleri saklanıyor).

---

## 6. Hukuki uyarılar (özet)

- **Rockstar (GTA V/RDR2/FiveM):** Güncel politika para kazanılan videoyu izin dışında bırakıyor [D]. Kanal büyüyünce kaldırma riski.
- **EA (Sims 4):** Reklam geliri serbest; ücretli üyelik/Patreon arkasına koymak ve EA müziğini kullanmak sorunlu [D].
- **Valve (SFM/S2FM):** Araç ticari serbest; Valve varlıkları resmi şartta "ticari olmayan", ama Valve YouTube ortaklık gelirine izin vermişti (2013) [K]. Başka oyunlardan sökülmüş workshop modelleri kullanmayın.
- **Facepunch (GMod):** Standart platform geliri serbest; GMod içindeki üçüncü taraf içerik kapsam dışı [D].
- **iClone Personal:** Kendi render çıktısı yalnız kişisel [D] — TikTok'ta para kazanan videoyu iClone'da render etmeyin.
- **Cascadeur Free:** Ticari değil [D]. **MMD modelleri:** her modelin readme'si ayrı; çoğu ticari yasak [K].
- **TikTok:** Creator Rewards programı özgün içerik istiyor; oyun görüntüsü + özgün hikaye + özgün ses genelde kabul görür ama TikTok kurallarını ayrıca okuyun [B].

## 7. Belirsizlikler / test gerekenler
- iClone Personal'ın Vega 7'de açılıp açılmadığı; 30 dışa aktarımın Unity'de ticari render için yeterli lisans olup olmadığı (EULA okunmalı).
- Blender Eevee'nin bu iGPU'da 1080x1920 kare başına süresi.
- Sims 4 + ReShade'in uzak masaüstünde çalışması ve kayıt (OBS) performansı.
- S2FM'in Vega 7'de açılıp açılmadığı.
- ZEPETO, Sakura School Simulator, Second Life, Roblox video para kazanma metinleri bu oturumda okunamadı.

## Kaynaklar (erişim 2026-11-28)
- EA içerik politikası: https://help.ea.com/en/articles/security-and-rules/ea-content-policy/
- Rockstar politikası: https://support.rockstargames.com/articles/200153756/rockstar-games-video-policy · Rockstar Editor müzik: https://support.rockstargames.com/articles/3f8XH01Kodw4zKnwbF6yTB/information-about-being-safe-from-copyright-strikes-when-uploading-rockstar-editor-videos
- Rockstar mod kuralları haberi: https://www.digitalcitizen.life/rockstar-mod-guidelines-limit-multiplayer-modding-creator-platform/
- SFM şartları: https://www.sourcefilmmaker.com/sfm_subscription_agreement · S2FM: https://developer.valvesoftware.com/wiki/Source_2_Filmmaker · Valve 2013: https://overclockers.ru/softnews/show/57681/Valve_My_ne_protiv_ispolzovaniya_kontenta_iz_nashih_igr_v_YouTube
- Facepunch: https://facepunch.com/legal/ugc
- Roblox Video Stars: https://en.help.roblox.com/hc/articles/360026092011
- Minecraft (ikincil): https://conductatlas.com/platform/minecraft/minecraft-usage-guidelines/
- iClone Personal: https://www.cgchannel.com/2026/06/reallusion-releases-free-animation-tool-iclone-person/
- Cascadeur: https://cascadeur.com/plans
- MMD: https://en.wikipedia.org/wiki/MikuMikuDance · Moviestorm: https://en.wikipedia.org/wiki/Moviestorm · Muvizu: https://en.wikipedia.org/wiki/Muvizu
- Plotagon Studio kapanışı: https://forums.justuseapp.com/en/post/3ZAN5LHSZ6/shutdown-permanently
- Illusion: https://en.wikipedia.org/wiki/Illusion_(company)
- VRoid: https://scrapbox.io/VRoid/VRoid_Studio%E3%81%AB%E3%81%A4%E3%81%84%E3%81%A6
- Sims sinematik araçları: https://simscommunity.info/2026/08/20/the-sims-4-can-look-this-good-how-nishaimansims-does-it/
- ZEPETO drama haberi: https://view.asiae.co.kr/en/article/2021120210463932798
- Vega 7 referans: https://www.notebookcheck.net/Battlefield-6-performs-well-even-on-super-weak-iGPU-Ryzen-5-5600G-with-mediocre-Vega-7-runs-Battlefield-6-at-over-45-FPS.1082939.0.html
