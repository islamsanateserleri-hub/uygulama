# D12 — Bedava animasyon, webcam mocap, Türkçe dudak/altyazı zamanlaması ve Türkçe TTS

Tarih: 2026-11-28 · Yöntem: yalnızca web araştırması (WebSearch/WebFetch). İndirme, hesap açma ve deneme yapılmadı.
Önceden okunanlar (burada tekrar edilmedi): A03 (Mixamo şartları, CMU/100STYLE özeti, DeepMotion/Rokoko Vision/FreeMoCap), A07 (MFA Türkçe, WhisperX, Piper dfki NC, XTTS NC, viseme önerisi), D7, D9, D10.

İşaretler: **[D]** = bu turda kaynak sayfasından doğrulandı · **[K]** = kısmen doğrulandı (sayfa eksik/çelişkili) · **[Ç]** = doğrulanmadı, genel bilgi veya çıkarım.

Kısa sonuç:
1. **Animasyon:** Rocketbox (MIT) + **Quaternius UAL 1 ve 2 (CC0)** + **Mixamo** temel kalsın. Hikaye oyunculuğu için ek olarak **Anderson Rohr "Everyday Actions Pack 2"** (telefonla yazma, oturarak klavye vb., ticari serbest), **ActorCore 18 örnek hareket** (ayakta/oturarak konuşma, oturma geçişi), **Mocap Online ücretsiz sampler**, **Kevin Iglesias Basic Motions FREE** (8 yön yürüme, dönüş, konuşma). **Bandai Namco, LAFAN1, BOOTH'taki ücretsiz VRMA'lar ticari değil → kullanma.**
2. **Webcam mocap:** **TDPT (ThreeDPoseTracker)** en uygunu: ücretsiz, Windows, çıktı **ticari kullanılabilir**, GPU'suz hafif modda çalışıyor, BVH/VMD/VRMA dışa aktarım. Bulut seçenekleri (Plask 15 sn/gün, QuickMagic 30 sn video) ücretsiz planda ticari hak yazmıyor.
3. **Dudak/altyazı:** Metin zaten elimizde (TTS metni) → **zorunlu hizalama** (MFA Türkçe, A07) en doğru yol. CPU'da hafif yedek: **Vosk small-tr (35 MB, Apache 2.0)**. Denetim için **whisper.cpp (MIT)**. Ağız şekli: kendi Türkçe harf→viseme kodu; yedek **uLipSync (MIT, önceden "bake")**. Rhubarb Türkçe için zayıf.
4. **TTS:** Ücretsiz ve ticari açıdan temiz, Türkçe destekli en iyi yerel aday **Chatterbox Multilingual (MIT, Türkçe var, duygu ayarı)**; CPU'da yavaş olabilir (ölçülmedi). Gemini ücretsiz katman **~10 istek/gün**. Edge TTS resmi değil → para kazanılan video için riskli. Piper Türkçe sesler hızlı ama robotik ve "lessac" temeli yüzünden lisans gri.

---

## 1. Ücretsiz, ticari kullanılabilir insan (humanoid) hareket kütüphaneleri

Bizim kullanım: hareket Unity içinde render edilip **video** olarak yayımlanıyor. Çoğu lisansta "videoda kullanmak" serbest; asıl sınır **ham dosyayı yeniden dağıtmak** (uygulama EXE'si/şablonla birlikte vermek) ve bazen **oyun/uygulama içine gömmek**.

| Kaynak | Sayı / içerik | Lisans (ticari / uygulamada dağıtım) | Unity Humanoid notu | Durum |
|---|---|---|---|---|
| **Quaternius Universal Animation Library (UAL1)** | Kaynak sürüm 120+; ücretsiz kısmın sayısı yazmıyor. 8 yön yürüme, koşu, itme, sürünme, yüzme, **oturma, ölüm, dövüş, emote** | **CC0** — ticari serbest, dağıtım serbest | FBX/GLB/Blend; root motion'lı ve root'suz iki sürüm. "Universal humanoid rig" | [D] https://quaternius.com/packs/universalanimationlibrary.html |
| **Quaternius UAL2** | Kaynak sürüm 130+. Yakın dövüş/silahlı kombolar, parkur, tarım, balık tutma, zombi yürüyüşü | **CC0** | Aynı iskelet, Unity/Godot/Unreal; Humanoid avatar özelinde yazmıyor | [D] https://quaternius.com/packs/universalanimationlibrary2.html |
| Quaternius "Source" erişimi | Tüm klipler + .blend | Patreon kredisi: Silver 10 USD = 15 kredi (bir ay destek yeterli olabilir) | — | [D] aynı sayfa; kaç kredi gerektiği [Ç] |
| **Mixamo** | Binlerce; konuşma jestleri, duygular, oturma/kalkma, dövüş, düşme | Telifsiz, ticari serbest; **ham dosya dağıtımı yasak** | Humanoid'e kolay | A03'te [D] |
| **Anderson Rohr – Everyday Actions Pack 2** (Rokoko/CG Channel duyurusu, 2026-09-27) | 22 klip, 7+ dk: **oturarak klavye, telefonla mesaj, online toplantı**, duş, diş fırçalama, araba sürme | **Ticari serbest, atıf gerekmez** | FBX: UE5 Manny, Rokoko Default, iClone iskeleti → Unity'de Humanoid'e alınabilir [Ç] | [D] https://www.cgchannel.com/?p=178935 |
| **Rokoko ücretsiz paketler** | 263 klip "büyü temalı" (Sam Lazarus) + TV temalı vb. ayrı paketler | "Ticari kullanım dahil" her projede; dağıtım koşulu yazmıyor | FBX 30 fps; Mixamo, UE4/UE5, HumanIK iskeletleri | [K] https://www.rokoko.com/resources/download-263-rokoko-motion-capture-assets (form doldurma gerekli) |
| **ActorCore Sample Motions** (Reallusion, Fab) | **18**: ayakta ve oturarak **konuşma/bekleme**, **ayaktan oturmaya geçiş**, yürüme, dans, catwalk, büyü | Fab ücretsiz; sayfada lisans alanı boş → Fab Standart Lisans varsayılır [Ç] | Unreal formatında listelenmiş; Unity'ye taşımak için Blender/FBX gerekir [Ç] | [K] https://www.fab.com/listings/4776b9fa-7776-4f1c-be4f-39dea70d4bfc |
| ActorCore sitesi ücretsiz hareketler | 32 ücretsiz (bir kısmı kampanyalı), yürüme/durma/koşu, dans, silah | Ticari serbest; **oyun/etkileşimli uygulamaya gömmek için ayrı "mass distribution" lisansı** (videoda sorun yok) | FBX 24/30/60 fps, Unity ön ayarı var | [D] https://www.cgchannel.com/?p=121885 (2022 haberi; güncel sayı [Ç]) |
| **Mocap Online Free Sampler** | Sayı yazmıyor; yürüme, idle, dövüş | Standart Lisans; SSS "ticari serbest, ücretsizde gelir sınırı yok" ama başlık "küçük ticari" diyor (çelişki). Dağıtım yazmıyor | Unity formatı var | [K] https://mocaponline.com/pages/free-animations (0 USD ödeme, kart yok) |
| **Kevin Iglesias – Basic Motions FREE** | Idle'lar, zıplama, düşme, **8 yön yürüme/koşu**, sprint, **sağa/sola dönüş**, **konuşma**; erkek+kadın sürümü; root motion + in place | itch sayfası lisansı kendi sitesine bağlıyor, metni okunamadı. **Unity Asset Store sürümü** standart Asset Store EULA ile gelir (ürün içinde kullanım serbest, ham satış yasak) [Ç] | FBX; Asset Store sürümü Unity için hazır | [K] https://kevdev.itch.io/basic-motions-free · https://assetstore.unity.com/packages/3d/animations/basic-motions-free-154271 |
| **CMU Mocap** (2000+ BVH) | Günlük hareketler, yürüme, oturma, kavga, dans… çok çeşitli ama eski ve gürültülü | Ticari ürüne katılabilir; **verinin kendisi (dönüştürülmüş hali dahil) satılamaz**; teşekkür istenir | rancidmilk FBX dönüşümü: Quaternius CC0 karakterine Auto-Rig Pro ile aktarılmış, "henüz oyuna hazır değil, cila ister"; 4TU'da Unity için FBX dönüşümü (2024-06-12) | [D] https://rancidmilk.itch.io/free-character-animations · https://data.4tu.nl/datasets/0448aab2-3332-449f-a8e2-d208cb58c7df |
| **100STYLE** | 100 yürüme/koşu stili (yaşlı, yorgun, mutlu…) | CC BY 4.0 (atıf) | BVH → Blender → FBX | A03'te [D] |
| Truebones ücretsiz | 4 FBX/BVH (zaman zaman ücretsiz paket) | Sayfada lisans yazmıyor | — | [K] https://truebones.gumroad.com/p/free-4-mocap-files-in-fbx-bvh-and-i-clone-format |
| Unity Starter Assets (Third Person) | Idle, yürüme, koşu, zıplama, iniş | Asset Store/Unity lisansı; ürün içinde serbest [Ç] | Humanoid hazır | [Ç] |

### Kullanma (ticari değil)
| Kaynak | Neden | Durum |
|---|---|---|
| **Bandai Namco Research Motion Dataset** (2 set; 17 içerik × 15 stil, duygulu; BVH) | **CC BY-NC 4.0** → ticari yok. YouTube/TikTok'ta para kazanılan video ticari sayılır | [D] https://github.com/BandaiNamcoResearchInc/Bandai-Namco-Research-Motiondataset |
| **Ubisoft LAFAN1** | **CC BY-NC-ND 4.0** | [D] https://github.com/ubisoft/ubisoft-laforge-animation-dataset |
| BOOTH'taki "ücretsiz" VRMA/BVH paketleri (ör. Mananeko) | Ticari kullanım, AI ile kullanım, yeniden dağıtım yasak | [D] https://booth.pm/ja/items/5582912 |
| DeepMotion ücretsiz plan | Ticari kullanım yok | A03'te [D] |

Japonca/Çince aramalar ("モーションデータ 無料 商用", "免费 动作捕捉 可商用") çoğunlukla TurboSquid listelerine ve kişisel BOOTH mağazalarına çıktı; ticari serbest + toplu ücretsiz yeni bir kaynak bulunamadı. Japon tarafının en değerli bulgusu bir **araç**: TDPT (bölüm 2).

### Unity Humanoid'e aktarma notları
- FBX → Rig: **Humanoid**, "Create From This Model" → kemik eşlemesi kontrol; Rocketbox ve UAL aynı "Humanoid" kas uzayına çevrilip birbirine uygulanabilir [Ç, genel Unity bilgisi].
- **Root motion:** "Yerinde saçma yürüme" şikâyeti için kök hareketli klip seçilmeli (Quaternius ve Kevin Iglesias iki sürümü de veriyor) veya yerinde klipte karakter, adım hızına eşit hızla kod ile taşınmalı; ayak kayması için "Foot IK" açılmalı [Ç].
- BVH (CMU, 100STYLE) Unity'ye doğrudan girmez → Blender'da içe al, insan iskeletine eşle, FBX ver [Ç].
- Farklı iskeletli paketler (UE5 Manny, Rokoko, iClone) Unity'de Humanoid olarak alınabilir ama parmak/omuz eşlemesini elle kontrol etmek gerekir [Ç].
- Kötü eşleşme: Bandai/CMU gibi ham mocap'te el parmağı ve yüz yok; konuşma jestleri için Mixamo/ActorCore/Kevin "talk" klipleri daha temiz.

---

## 2. Ücretsiz webcam/video mocap (üretken video aracı DEĞİL)

Bunlar videodaki insanın iskeletini tahmin edip **hareket verisi** (BVH/FBX) çıkarır; görüntü üretmez.

| Araç | Nerede çalışır | Ücretsiz sınır | Çıktı lisansı | Kalite / not | Durum |
|---|---|---|---|---|---|
| **TDPT (ThreeDPoseTracker), Windows v0.6.3** | Yerel; normal kamera veya video dosyası | Tamamen ücretsiz (~926 MB); bahşiş isteğe bağlı | **"Üretilen hareket verisi bireyler ve şirketler tarafından ticari olarak serbestçe kullanılabilir"** (videodaki müzik/koreografi hakları ayrı) | 3D 24 nokta, yüz+el takibi, **düşük PC için hafif mod**. Ölçüm: i5 + 4 GB RAM, ekran kartı yok → ~12 fps (hafif model). **BVH kaydı, VMD, VRMA dışa aktarma**, VMC protokolü | [D] https://booth.pm/ja/items/5294636 |
| FreeMoCap 1.2.0 | Yerel, açık kaynak (AGPL-3.0 yazılım) | Sınırsız | Yazılım AGPL; senin çektiğin veriye AGPL bulaşmaz [Ç] | Çok kameralı kurulum önerilir; dışa aktarım yalnızca **Blender eklentisi** (BVH/FBX sürüm notunda yok) | [K] https://github.com/freemocap/freemocap/releases/tag/v1.2.0 |
| Plask Motion | Bulut (tarayıcı) | **15 sn/gün**, FBX/GLB/BVH, 1 GB | Ücretsiz planda ticari hak **yazmıyor** | Paid: 18 USD/ay (yıllık) | [K] https://plask.ai/pricing |
| QuickMagic | Bulut | 30 sn'lik video, 2 kuyruk, yalnız FBX, 15 gün saklama | Ücretsizde ticari hak **yazmıyor** | Basic 11,90 USD/ay (yıllık) | [K] https://www.itechguides.com/products/quickmagic/pricing/ (üçüncü taraf, 2026-11-14) |
| Rokoko Vision | Bulut | 30 sn/ay (A03) | Belirsiz (A03) | — | A03 |
| DeepMotion | Bulut | 60 sn/ay | **Ücretsizde ticari yok** | — | A03 |

Öneri: Zayıf PC'de **TDPT + telefonla çekilmiş kısa video** → BVH → Blender'da temizlik (ayak kayması, titreme) → FBX → Unity Humanoid. Tek kameralı tüm yöntemlerde derinlik hatası ve ayak kayması beklenir [Ç]; kısa "özel jest" klipleri (kapıyı çalma, telefona bakma) için uygun, uzun yürüyüş için değil (yürüyüşü kütüphaneden al).

---

## 3. Türkçe dudak senkronu ve altyazı zamanlaması (bulut AI yok)

Temel fikir: TTS metni zaten bilindiği için **"konuşmayı tanıma" değil "metni sese hizalama"** yapılmalı. Türkçe yazım sese çok yakın olduğundan harf → ses → viseme dönüşümü kural tabanlı yazılabilir [Ç].

| Araç | Ne verir | Türkçe | CPU | Lisans | Değerlendirme | Durum |
|---|---|---|---|---|---|---|
| **Montreal Forced Aligner 3 – Türkçe model** | Kelime + **fonem** zamanı | Var (128 saat) | Evet; kurulum ağır (conda) | Model CC BY 4.0 | **1. tercih**: altyazı ve viseme aynı kaynaktan | A07'de [D] |
| **Vosk vosk-model-small-tr-0.3** | Tanıma + kelime zamanları | Var; WER yayımlanmamış ("TBD") | Çok hafif (35 MB, Raspberry Pi'de bile) | **Apache 2.0** | Hafif yedek; doğruluk ölçülmeli | [D] https://alphacephei.com/vosk/models · kelime zamanı özelliği [Ç] |
| **whisper.cpp** (+ ince ayarlı Türkçe Whisper) | Döküm + kelime zamanı (deneysel) | İyi; ör. selimc large-v3-turbo-Türkçe ince ayarı **WER %18,9** (Common Voice 17) | Evet, ama large/turbo modeller zayıf CPU'da yavaş; small/base hızlı ama daha hatalı [Ç] | whisper.cpp MIT; selimc modeli **MIT** | En iyi kullanım: TTS doğru mu okudu **denetimi** (yanlış kelime/atlanmış satır yakalama). Zamanlama için hizalayıcıdan kaba | [D] https://huggingface.co/selimc/whisper-large-v3-turbo-turkish |
| **uLipSync** | Sesten ağız şekli (MFCC, ünlü tanıma) | Dil bağımsız; A-I-U-E-O profili **elle kalibre** edilir (Türkçe a/e/ı/i/o/ö/u/ü bu 5 şekle indirgenir [Ç]) | Evet; **önceden "Bake" → BakedData**, toplu "Baked Data Generator", Timeline | **MIT**; Burst + Mathematics paketleri gerekir | Hizalama yoksa iyi yedek; v3'te v2 profilleri yeniden kalibre edilmeli | [D] https://github.com/hecomi/uLipSync |
| Rhubarb Lip Sync | 6–9 ağız şekli | İngilizce dışı için yalnız "phonetic" mod: **daha az hassas** | Evet | LICENSE.md ("other" görünüyor; ana kod MIT bilinir [Ç]) | 2D çizgi karakter için; 3D Türkçe için zayıf | [K] https://awesome.ecosyste.ms/projects/github.com%2Fdanielswolf%2Frhubarb-lip-sync |

Önerilen hat (A07 ile aynı yönde): TTS sesi + metin → **MFA** (veya Vosk/whisper) → kelime/fonem zamanları JSON → (a) altyazı kelime kelime bu saatle, (b) fonem → Rocketbox viseme blendshape. Altyazının konuşmayla kaymaması için altyazı saati **ses saatine (DSP)** bağlanmalı (A07 K8).

---

## 4. Ücretsiz Türkçe TTS

| Seçenek | Kalite | Lisans / şart | CPU | Durum |
|---|---|---|---|---|
| **Chatterbox Multilingual (Resemble AI) V3, 0.5B** | Modern, **duygu abartı ayarı** (exaggeration), ses klonlama; Türkçe 23 dil içinde | **MIT**. Her çıktıya duyulmayan **PerTh filigranı** eklenir (yasak değil, bilgi) | Mümkün (topluluk sunucusunda `--cpu`, CPU Docker); hız ölçümü yok, zayıf CPU'da yavaş olabilir. ONNX sürümü var | [D] https://huggingface.co/ResembleAI/chatterbox/blob/main/README.md · https://raw.githubusercontent.com/devnen/Chatterbox-TTS-Server/main/README.md · Türkçe kalite dinlenmedi [Ç] |
| **Gemini TTS ücretsiz katman** (projede kullanılıyor) | Çok iyi, stil komutlu | Ücretsiz katman **~10 istek/gün** (gemini-3.1-flash-tts, forum, 2026-08-08). Resmi oran sayfası (2026-10-28) ücretsiz değerleri AI Studio'ya yönlendiriyor. Ücretsiz katmanda verinin ürün geliştirmede kullanılabileceği [Ç] | Bulut | [K] https://discuss.ai.google.dev/t/gemini-lies-about-free-tier-quota-limits-sudden-resource-exhausted-blockers-on-tts-models/177531 · https://ai.google.dev/gemini-api/docs/rate-limits |
| Piper tr_TR **fahrettin**, **fettah** | Hızlı, kararlı, **duygusuz/robotik** | Veri **CC0** (NabuCasa), depo MIT; ama ikisi de **ABD İngilizcesi "lessac" sesinden ince ayar** → lessac verisinin lisansı belirsiz (A07) → gri | Çok hızlı, zayıf CPU'da gerçek zamandan hızlı [Ç] | [D] https://huggingface.co/rhasspy/piper-voices/blob/6fb8245d6d27fef8988272e0c8e9f5018da1bbbf/tr/tr_TR/fettah/medium/MODEL_CARD |
| Piper tr_TR dfki | — | **CC BY-NC-SA → ticari yok** | — | A07'de [D] |
| Edge TTS (edge-tts kütüphanesi; tr-TR Emel/Ahmet) | İyi | Kütüphane GPL-3; ama Microsoft'un **resmi olmayan** okuma servisini kullanıyor. Microsoft Q&A yanıtı (2024-10-07, resmî olmayan): Azure aboneliği olmadan ticari kullanım **hizmet şartlarını ihlal edebilir** → para kazanılan videoda **kullanma** | Bulut | [D] https://learn.microsoft.com/en-us/answers/questions/2088770/are-opensource-edge-tts-free-for-commercial-use |
| Azure AI Speech F0 (resmî yol, aynı tr-TR sesler) | İyi, viseme olayı verir | Aylık ücretsiz karakter kotası var [Ç: miktar bu turda doğrulanmadı] | Bulut | [Ç] |
| Coqui XTTS-v2 | İyi, klonlama | CPML **ticari değil**; Coqui şirketi kapandı (2024 başı) → ticari lisans satın alınamıyor [Ç] | GPU önerilir | A07'de [D] (NC) |
| MMS-TTS, F5-TTS bazı Türkçe modeller | — | Ticari değil | — | A07 |

Öneri: (1) Gemini'yi ana ses olarak sürdür ama kota yetmeyince **Chatterbox Multilingual'ı yerelde dene** (önce 3–4 satırlık Türkçe dinleme testi + CPU süresi ölçümü). (2) Piper'ı yalnız taslak/önizleme sesi için kullan (hızlı; yayımda lisans gri). (3) Edge TTS'ten kaçın.

---

## 5. Sıralı öneri (bütçe ≤ 1000 TL)

1. **Rocketbox (MIT) + Quaternius UAL1+UAL2 (CC0)** — 0 TL; tüm klipler + .blend için bir ay Patreon Silver (10 USD ≈ 350–450 TL [Ç: kur]).
2. **Mixamo** — 0 TL; konuşma jestleri, duygular, oturma/kalkma, düşme. Ham FBX'i uygulamayla dağıtma.
3. **Everyday Actions Pack 2 + ActorCore 18 örnek + Mocap Online sampler + Kevin Iglesias FREE** — 0 TL; günlük hayat ve oturarak konuşma boşluklarını doldurur.
4. **TDPT** — 0 TL; özel jest gerektiğinde telefon videosundan BVH.
5. **MFA Türkçe** (yedek Vosk small-tr) + kendi viseme kodu; uLipSync yedek — 0 TL.
6. **Gemini ücretsiz + Chatterbox Multilingual yerel** — 0 TL.

## 6. Açık konular / doğrulanmayanlar
- Kevin Iglesias itch lisans metni okunamadı; Asset Store sürümünü tercih et.
- ActorCore Fab örneğinin lisans alanı boş; Fab Standart Lisans varsayıldı.
- Chatterbox'un Türkçe telaffuzu ve zayıf CPU'da hızı ölçülmedi.
- Vosk Türkçe WER ve kelime zamanı doğruluğu ölçülmedi.
- Rhubarb lisans dosyası açılmadı.
- Gemini ücretsiz kota resmi sayfada sayı olarak yok; forum verisi (10/gün) kullanıldı.
- Plask/QuickMagic/Rokoko Vision ücretsiz çıktılarının ticari hakkı sayfalarda yazmıyor.

## Kaynaklar (erişim 2026-11-28)
- Bandai Namco: https://github.com/BandaiNamcoResearchInc/Bandai-Namco-Research-Motiondataset/blob/master/README.md
- Quaternius UAL1: https://quaternius.com/packs/universalanimationlibrary.html · UAL2: https://quaternius.com/packs/universalanimationlibrary2.html
- ActorCore: https://www.cgchannel.com/?p=121885 · https://www.fab.com/listings/4776b9fa-7776-4f1c-be4f-39dea70d4bfc
- Rokoko: https://www.rokoko.com/resources/download-263-rokoko-motion-capture-assets · Anderson Rohr: https://www.cgchannel.com/?p=178935
- Kevin Iglesias: https://kevdev.itch.io/basic-motions-free
- Mocap Online: https://mocaponline.com/pages/free-animations
- CMU: https://data.4tu.nl/datasets/0448aab2-3332-449f-a8e2-d208cb58c7df · https://rancidmilk.itch.io/free-character-animations
- Truebones: https://truebones.gumroad.com/p/free-4-mocap-files-in-fbx-bvh-and-i-clone-format
- LAFAN1: https://github.com/ubisoft/ubisoft-laforge-animation-dataset · BOOTH VRMA: https://booth.pm/ja/items/5582912
- TDPT: https://booth.pm/ja/items/5294636 · FreeMoCap: https://github.com/freemocap/freemocap/releases/tag/v1.2.0 · Plask: https://plask.ai/pricing · QuickMagic: https://www.itechguides.com/products/quickmagic/pricing/
- Vosk: https://alphacephei.com/vosk/models · Whisper TR: https://huggingface.co/selimc/whisper-large-v3-turbo-turkish · uLipSync: https://github.com/hecomi/uLipSync · Rhubarb: https://awesome.ecosyste.ms/projects/github.com%2Fdanielswolf%2Frhubarb-lip-sync
- Chatterbox: https://huggingface.co/ResembleAI/chatterbox/blob/main/README.md · https://raw.githubusercontent.com/devnen/Chatterbox-TTS-Server/main/README.md
- Piper TR: https://huggingface.co/rhasspy/piper-voices/tree/v1.0.0/tr/tr_TR · fettah kartı: https://huggingface.co/rhasspy/piper-voices/blob/6fb8245d6d27fef8988272e0c8e9f5018da1bbbf/tr/tr_TR/fettah/medium/MODEL_CARD · fahrettin kartı: https://huggingface.co/rhasspy/piper-voices/blob/2083514c726ed4e54d15dbf8d86285d6059642f0/tr/tr_TR/fahrettin/medium/MODEL_CARD
- Gemini: https://ai.google.dev/gemini-api/docs/rate-limits · https://discuss.ai.google.dev/t/gemini-lies-about-free-tier-quota-limits-sudden-resource-exhausted-blockers-on-tts-models/177531
- Edge TTS: https://learn.microsoft.com/en-us/answers/questions/2088770/are-opensource-edge-tts-free-for-commercial-use
