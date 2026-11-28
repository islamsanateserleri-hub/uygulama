# A03 — Animasyon, Motion Capture ve Karakter Hareketi

27 Kas 2026. Kaynak numaraları: `SOURCES/A03_kaynaklar.md`. Kod yazılmadı; proje kodu yalnız okundu; **Unity çalıştırılmadı, hiçbir hareket testi yapılmadı.**

## Görev tanımı
Protokol ALT AJAN 3 + §9.4. Karakter hareketinin sinematik kaliteye ulaşmasını araştırmak ve ekonomik, kaliteli, genişletilebilir bir animasyon mimarisi önermek. Kullanıcının reddi: "karakterler yerinde durup saçma hareket ediyor, yürüme/koşma yok" (en yüksek öncelik).

## Kısa sonuç
1. Sorun büyük olasılıkla önce **kodda**, sonra animasyon eksikliğinde. Yeni paket veya araç almadan düzeltilebilecek en az üç ciddi hata adayı var (ÇIKARIM).
2. Elde olan Rocketbox hareketlerinin yalnız **yerinde duran (static)** kümesi projeye alınmış; Rocketbox'ın yürüme/koşma klipleri alınmamış görünüyor (kod + [S003][S004]).
3. Ücretsiz ve lisansı temiz yol var: Rocketbox (MIT) + UAL (CC0) + gerektiğinde Mixamo. Yapay zekâ hareket üretimi bu donanımda yerelde pratik değil.

## Araştırma alanı ve soruları
S0 kod nedenleri · S1 eldeki kütüphaneler · S2 Mixamo · S3 diğer kütüphaneler · S4 mocap · S5 Unity sistemleri · S6 bakış/el/etkileşim/dövüş/oturma · S7 AI hareket üretimi · S8 otomasyon ve mimari.

## Bulgular

### S0 — Kod incelemesi (tamamı ÇIKARIM: kod okundu, çalıştırılıp doğrulanmadı)
Yollar `Assets/HikayeStudyo/` altındadır.

| # | Belirti | Dosya:satır | Neden | Güven |
|---|---|---|---|---|
| H1 | Susan karakter sürekli el kol hareketi yapıyor | `Scripts/Karakter3D.cs:103-111, 123`; `Editor/Karakter3DKurulum.cs:133-145`; `Editor/NesneKur.cs:913-915` | "dur" → "idle" → "ile başlayan ilk klip". Rocketbox listesinde `gestic_talk_neutral_01` (adı `Idle_Talking_Loop`) dosya sırasında `idle_neutral_01`'den (`Idle_Loop`) önce. Yani **"durgun duruş" = konuşma jesti döngüsü**. `Karakter3DKurulum.cs:132` yorumu tersini amaçlıyor. İstisna: MixamoAnimasyon klasöründe adı tam `Idle` olan klip varsa o seçilir (klasör repoda yok, bakılamadı) | Yüksek |
| H1b | Aynı klip konuşurken ve susarken | `Sahne3D.cs:279, 358`; `Sahne3D.Hareket.cs:562, 605, 752, 855`; `Sahne3D.cs:46, 58` | Her "durgun hale dön" çağrısı H1'e düşer. Eşleşmeyen duygu klipleri de (`uzgun`, `kork`...) yedek olarak "dur"a, yani yine jestli klibe iner | Yüksek |
| H2 | Yürüme komutu var ama yürümüyor, kayıyor | `StoryPlayer.cs:1003-1004`; `Sahne3D.cs:310-329` | `Ensure3D` yürüyüşü başlatır, hemen ardından `Act()` konuşma/duygu klibini oynatır. `Act()` `walking` kümesine bakmaz. `[Karakter|duygu|dis|yuru]` ve `kos` bu yüzden idle pozunda kayma olur | Yüksek |
| H2b | Kısa satırda yürüyüş ortasında durma pozu | `Sahne3D.cs:351-359` (`StoryPlayer.cs:1064`'ten) | `Idle()` de `walking`e bakmaz | Yüksek |
| H3 | `yuru`/`kos` yazılan yerinde duran karakter kıpırdamıyor | `Sahne3D.cs:245`; `StoryPlayer.cs:908, 1087-1091` | Hedef kendi yuvası; 5 cm'den yakınsa çıkılır | Yüksek |
| H4 | Ayak kayması | `Sahne3D.cs:257, 268-270` | Hız sabit (1,4 / 3,2 m/s); klip hızı, boy/ölçek hesaba katılmıyor; ivmelenme, başlama/durma, dönüş klibi yok; dönmeden ilerliyor | Orta-yüksek |
| H5 | Adımsız kayma, olduğu yerde dönme | `Sahne3D.Hareket.cs:152-164, 174, 216-217, 422`; `Sahne3D.cs:195-210` | `Kay()` karakteri 0,45-0,5 sn'de adımsız taşır; dönüş yalnız kök döndürme | Yüksek |
| H6 | "Tören yürüyüşü" | `Karakter3D.cs:108`; UAL FBX | UAL dosyasında hem `Walk_Formal_Loop` hem `Walk_Loop` var (dosyadan okundu, 45 take). Ada göre sıralamada `Walk_Formal_Loop` önce; `Find("walk")` onu seçer | Orta-yüksek |
| H7 | Jestler bitmeden tekrar tekrar | `NesneKur.cs:916` | `_in/_out/try_door` dışındaki her Rocketbox klibi döngülü (alkış, kahkaha, el sallama dahil) | Yüksek |
| H8 | Herkes aynı hareketi yapıyor, her satırda baştan | `Sahne3D.cs:44-69`; `Karakter3D.cs:223-233` | Duygu başına 1-3 klip adı; her `Play` aynı klip olsa da zaman 0'dan yeni playable; rastgele başlangıç yok | Yüksek |
| H8b | Geçişte sıçrama | `Karakter3D.cs:225-238, 250-256` | Karıştırıcı 2 girişli; geçiş yarıdayken yeni `Play` gelirse eski poz bir anda atılır | Orta |
| H9 | Ayaklar yerde yüzüyor | `YuzCanlilik.cs:652-656, 682` | "Canlılık" kalçayı döndürüyor (salınım); bacaklar kalçanın çocuğu, ayak IK yok. Etki küçük (birkaç cm) ama yakın zeminde görünür | Orta |
| H10 | Mankenvari sarılma/tepki | `KolPozu.cs:236-282`; `Sahne3D.Hareket.cs:301-318, 525-555` | Yalnız el hedefi + dirsek ipucu; bilek, parmak, omuz, iç içe geçme denetimi yok; iki ayrı karakterin eşleşmiş hareketi değil | Orta |
| H11 | `zipla` yalnız iniş | `Karakter3D.cs:132`; UAL | "jump" → sırada `Jump_Land` önce | Orta-yüksek |
| H12 | Eşyanın/karakterin içinden geçme | `Sahne3D.cs:251-282` | Yol bulma yok, düz çizgi | Yüksek |

Ek: `Resources/Hikayeler/ucuncu_kisi.txt` içinde yalnız 2 `git:` var (satır 81, 95) (dosyadan okundu). Yani reddedilen filmde yürüme zaten neredeyse istenmemiş; DURUM.md de bunu not etmiş.

**Kök tasarım zaafı:** klipler ada göre bulanık eşleniyor (tam → başlar → içerir). Kütüphane büyüdükçe (326 + 45 + Mixamo) sonuç sıraya bağlı ve sessizce yanlış oluyor.

### S1 — Eldeki kütüphaneler
- **Rocketbox:** MIT; README 417 animasyon diyor [S001][S002][S006]. Üçüncü taraf kaynaklar 471 klip ve `static` (yerinde) / `xy` (yerde ilerleyen) / `xyz` klasörleri bildiriyor [S003]. Static küme 326 klip, 19 grup: idle, jest (konuşma, dinleme, kahkaha, sunum), sandalyede/masada oturma, telefon, dans, belge, çömelme, içme, çanta, gazete, el arabası, şemsiye, el sallama, tezahürat, kapı çalma, fotoğraf [S004]. Yürüme (`walk_neutral_01..03`), koşma (`run_neutral_01..02`), sandalyeden kalkma klipleri de kütüphanede [S005], `m_walk_neutral_01` `xy` klasöründe [S003].
- **Projede kullanım:** `NesneKur.cs:742` yalnız `all_animations_max_motextr_static` klasörünü kopyalıyor; DURUM.md "326 hareket" diyor ([S004] sayısıyla aynı). Sonuç (ÇIKARIM): yürüme/koşma Rocketbox'tan **hiç alınmamış**; yürüme UAL'den retarget ediliyor. Kalan klipler kullanıcının `KutuphaneHam/Rocketbox/Animations` klasöründe olabilir (9,4 GB indirme notu var; DOĞRULANAMADI).
- 326 klip bağlı ama takma ad tablosu (`Karakter3D.cs:159-180`) yalnız yaklaşık 20'sine isim veriyor; dinleme (`listen`), idle çeşitleri, oturma varyantları otomatik seçilmiyor.
- **UAL (projede, CC0):** `UAL1_Standard.fbx` içinde yürüme (2), jog, sprint, çömelerek yürüme, itme, yüzme, oturma giriş/bekleme/konuşma/çıkış, yumruk (2), kılıç, tabanca, darbe alma (2), ölüm, takla (`Roll`, `Roll_RM`), yerden alma, etkileşim, büyü, zıplama (3) var (dosyadan okundu). Eksik: kalkma (yerden), dönüş, yürümeye başlama/durma, sarılma.
- Önemli: `Sitting_Enter/Exit` mevcut ama `Sahne3D.Hareket.cs:705, 721` oturmayı klipsiz **ışınlama** ile yapıyor (ÇIKARIM).

### S2 — Mixamo
- Adobe ID ile ücretsiz, abonelik gerekmez; film dahil ticari kullanım telifsiz; yalnız iki ayaklı insan; Enterprise/Federated ID ve Çin ülke kodu hariç. Resmî SSS tarihi 14 Eyl 2021 [S015].
- Ham dosyalar dağıtılamaz (uygulama/şablon içinde paketlemek dahil); ML eğitimi hariç [S016]. Proje bunu zaten `.gitignore` ile koruyor. **Uygulama EXE'si başkasına dağıtılacaksa** Mixamo klipleri sorun olabilir → A12 incelemeli.
- 2026 hizmet durumu: kapanış duyurusu bulunamadı; resmî güncel bir durum belgesi de bulunamadı (DOĞRULANAMADI) [S017]. Eski güncelleme tarihi nedeniyle tek bağımlılık yapılmamalı (ÇIKARIM).
- İndirme için kullanıcının Adobe hesabı gerekir: otomatik yapılamaz, ajan yapmamalı.
- Unity retarget: FBX "Humanoid" içe aktarılır; proje bunu yapıyor (`Karakter3DKurulum.cs:1228-1270`). Mixamo'da "In Place" seçeneği ile root'lu sürüm ayrımı indirirken yapılır (genel bilgi; bu turda kaynaktan doğrulanmadı).

### S3 — Diğer hareket kaynakları
| Kaynak | İçerik | Lisans | Not |
|---|---|---|---|
| UAL Standard/Pro/Source | 120+ (tam); root motion'lu/'suz [S018] | CC0 | Standard ücretsiz, Pro 9,99 USD, Source 14,99 USD [S020] |
| UAL 2 | 130+, dövüş komboları, parkur [S019] | CC0 | bir kısmı ücretsiz |
| CMU mocap | büyük genel mocap | ticari ürüne dahil edilebilir, satılamaz, teşekkür [S037] | BVH/FBX dönüşümü ve temizlik gerekir; kalite eski/değişken (ÇIKARIM) |
| 100STYLE | 100 yürüme/koşma stili, duygulu yürüyüş için çok uygun [S039] | CC BY 4.0 (atıf) | BVH → Blender → FBX dönüşümü gerekir |
| Basic Motions Free (Asset Store) | temel hareket | Asset Store EULA [S021] | içerik listesi okunamadı |
| BONES-SEED | 142 bin hareket, BVH [S036] | özel lisans, onay ister | ticari koşul bilinmiyor; kullanma |

### S4 — Motion capture
- **Optik/atalet donanım** (Vicon, Xsens, Rokoko Smartsuit): bu projenin bütçesi için gereksiz; fiyat araştırılmadı.
- **Video (markerless), bulut:**
  - Rokoko Vision: ücretsiz 30 sn/ay, FBX; 600 sn/ay planı yaklaşık 10-20 USD/ay (sayfada çelişki) [S022]. Çıktının ticari hakkı sayfada yazmıyor.
  - QuickMagic: ücretsiz planda video başına 30 sn, FBX; ücretli 11,90 USD/ay'dan [S025, D]. Ticari hak yazmıyor.
  - DeepMotion: ücretsiz 60 sn/ay ama **ticari kullanım yok** [S024, D] → ücretsiz planını kullanma.
  - Move AI: bu turda doğrulanamadı.
- **Yerel, açık kaynak:**
  - FreeMoCap: AGPL-3.0, CPU kurulumu var, birden çok webcam mantığı; "bozulabilir" uyarısı [S027]. Kamera sayısı/FBX aktarımı bu sayfada doğrulanmadı.
  - GVHMR: yalnız ticari olmayan araştırma [S028]; SMPL/SMPL-X model lisansı ticari kullanımı yasaklıyor [S029] (SMPL-Body için ayrı CC BY 4.0 lisansı var [S030]; karıştırma). WHAM vb. SMPL tabanlıları da aynı riske sahip say (ÇIKARIM).
  - MoveBox (Rocketbox için): Haz 2026'da arşivlendi; Kinect ya da araştırma lisanslı VIBE ister [S007] → önermiyorum.
- **Cascadeur:** ücretsiz sürüm FBX vermez ve ticari yasak; Indie 96 USD/yıl (gelir <100 bin USD) [S026]. `Docs/VideoKalitesi.md` ile uyumlu.
- Genel sınır (ÇIKARIM): tek kamera mocap'te ayak kayması, derinlik hatası ve parmak eksikliği temizlik ister; Rokoko kendi metinden-harekete çıktısı için de bunu söylüyor [S023]. Doğruluk çalışması aranamadı.

### S5 — Unity animasyon sistemleri
- **Humanoid retarget:** projede kullanılıyor; Rocketbox klipleri aynı Biped iskeletinden geldiği için UAL'den daha az bozulma beklenir ([S005] Godot deneyimi; Unity için ÇIKARIM).
- **Playables:** `Karakter3D` zaten kendi grafını kuruyor. Genişletme yolu: `AnimationLayerMixerPlayable` + AvatarMask (üst gövde jesti, alt gövde yürüme) + eklemeli katman. Animator Controller çizmeye gerek yok (ÇIKARIM; Unity API bilgisi, bu turda ayrıca kaynaklanmadı).
- **Foot IK:** `AnimationClipPlayable.SetApplyFootIK` var [S009]; Playables'ta işe yaramadığını bildiren forum kaydı var [S010] → test gerekli, güvenme.
- **Animation Rigging:** 1.4.1 Unity 6.0 için yayınlı [S011]; Two Bone IK, Multi-Aim, Override Transform vb. [S012][S013]. Yeni paket = kullanıcı onayı. Alternatif: projedeki analitik iki kemik IK (`KolPozu.IkiKemik`) bacağa da uygulanabilir (ÇIKARIM). Örnek foot IK: Grounder, MIT [S014].
- **Root motion:** projede `applyRootMotion=false`, kök hareketi kodla veriliyor. **Uyarı:** içe aktarıcılar `lockRootPositionXZ=true` yapıyor (`NesneKur.cs:917`, `Karakter3DKurulum.cs:1263`); bu "Bake Into Pose" demek, hareket gövde pozunda kalır [S008]. `xy` yürüme klipleri bu ayarla alınırsa gövde ileri gidip her döngüde geri sıçrar + kod ayrıca iter (ÇIKARIM). İlerleyen klipler için XZ bake KAPALI olmalı; sonra ya root motion kullanılmalı ya da yerinde oynatıp hız klipten ölçülmeli.
- **Ayak kayması çözümü (ÇIKARIM, öncelik sırasıyla):** (1) konumu root motion ile sür; (2) olmazsa oynatma hızı = gerçek hız / klibin doğal hızı, boy ölçeği dahil; (3) başla-dur-dön klipleri; (4) duruşta ayak kilitleme IK; (5) ölçüm testiyle denetle.
- **Blend:** 0,2-0,35 sn çapraz geçiş yeterli; ek olarak aynı klip yeniden başlatılmamalı, yürüme fazı korunmalı, yarım geçişte 3+ girişli karıştırıcı kullanılmalı (ÇIKARIM).
- **Motion matching / Kinematica:** bu tür çizgisel, önceden bilinen video için gereksiz görünüyor (ÇIKARIM; araştırılamadı).

### S6 — Bakış, el, etkileşim, dövüş, düşme, oturma, fizik
- **Bakış:** `YuzCanlilik` zaten konuşana bakma, sakkad, kırpma yapıyor; korunmalı. Eksik: gövde/boyun/göz sıralı dönüş (önce göz, sonra baş, sonra gövde) ve büyük açıda adımlı dönüş (ÇIKARIM).
- **El:** parmaklar yalnız klipten geliyor. Eşya için kavrama pozu (parmak) ve bileğe sabitlenmiş tutma noktası gerekli (`KolPozu.cs:224-232` eşya elin 6 cm ötesine konuyor, el dönüşünü izlemiyor) (ÇIKARIM).
- **Etkileşim (sarılma, el tutma, dövüş):** kaliteli sonuç için **eşleşmiş iki kişilik klip** + hizalama noktası (iki karakterin başlangıç konumu/yönü sabit) gerekir; IK yalnız küçük düzeltme yapmalı. Rocketbox/UAL listelerinde eşleşmiş sarılma görülmedi (DOĞRULANAMADI: tam liste okunamadı).
- **Dövüş/düşme:** UAL'de yumruk, darbe, ölüm; UAL2'de kombolar [S019]. Sinematik yöntem: darbe anını kesme + ses + kamera sarsıntısı ile gizle (A01/A08).
- **Oturma/kalkma:** durum makinesi: ayakta → (otur_giris) → oturuyor → (cikis) → ayakta. Koltuk noktasına önce yürü, dön, sonra giriş klibi; kök hizalaması koltuk yüksekliğine göre.
- **Fizik tabanlı:** ragdoll yalnız bayılma/düşme sonunda düşünülebilir; kayıt tekrarlanabilirliğini bozar, ilk aşamada gerek yok (ÇIKARIM).
- **Prosedürel:** nefes, mikro salınım, bakış iyi; ama **genlik az, ayaklar sabit** olmalı. Prosedürel kol pozları klip bulunamazsa son çare olmalı.

### S7 — AI tabanlı hareket üretimi (video üreticisi DEĞİL; iskelet hareketi üretir)
| Araç | Lisans | Donanım | Hükmüm |
|---|---|---|---|
| HY-Motion 1.0 (Tencent) | topluluk lisansı; AB/UK/G.Kore dışı; 1M MAU eşiği; AI açıklaması şartı [S031] | 24-26 GB VRAM [S033, D] | yerelde olmaz; parmak yok, nesne etkileşimi zayıf [S032]; lisans incelemesi ister |
| Kimodo (NVIDIA) | kod Apache-2.0; SOMA/G1 ağırlıkları NVIDIA Open Model License; SMPL-X sürümü R&D [S034] | ~17 GB VRAM ya da CPU metin kodlayıcı ile <3 GB [S034] | AMD tümleşik GPU'da desteklenmesi beklenmez (ÇIKARIM); ileride aday; BVH/FBX dönüşümü gerekir |
| Rokoko Create | lisans metni okunamadı | bulut/tarayıcı | ücretsiz üretim; Studio'dan ayda 5 FBX; blocking kalitesi [S022][S023] |
Sonuç: ana kaynak olmasın; **eksik tekil hareketi** üretip elle onaylayıp kütüphaneye ekleme aracı olabilir (ÇIKARIM). A11 takip etmeli.

### S8 — Hikâyeye göre otomasyon
Uygulama çevrimdışı video üretiyor: satırlar, komutlar ve ses süreleri önceden biliniyor. Bu, oyundan farklı büyük bir avantaj: hareket **önceden planlanabilir** (mesafe/hız = süre; gerekirse kamera kesmesi arkasında konum değiştir). Önerilen yapı aşağıda.

## Önerilen mimari ("Hareket Sistemi v2", ÇIKARIM; kod yazılmadı)

```
Hikâye satırları + ses süreleri
   → 1) Hareket Planlayıcı (her karakter için zaman çizelgesi, kayıttan önce)
   → 2) Hareket Kataloğu (açık veri; ada göre tahmin YOK)
   → 3) Katmanlı oynatıcı (Playables)
   → 4) Son düzeltme: ayak kilidi, bakış, el IK, nefes
   → 5) Hareket Denetçisi (sayısal test + uyarı)
```

1. **Planlayıcı:** durum makinesi (ayakta / yürüyor / dönüyor / oturuyor / yatıyor / etkileşimde). Kural: hareket sürerken konuşma klibi tabanı ezemez; konuşma jesti yalnız üst gövde katmanına gider. Satır başına en çok 1 belirgin jest; jestler arası sakin duruş. Duygu → (idle ailesi, jest havuzu, yürüyüş hızı, yoğunluk). Aynı tohumla aynı sonuç.
2. **Katalog (veri dosyası):** kimlik, dosya, kategori, başlangıç→bitiş duruşu, döngü mü, kök hareketi mi, doğal hız (m/s), ilk saniyedeki yön, üst gövde uyumu, duygu etiketi, yoğunluk, kaynak, **lisans**, "gözle onaylandı" bayrağı. Yeni kaynak = yeni satır; kod değişmez.
3. **Oynatıcı:** taban katman (lokomosyon/duruş) + üst gövde jest katmanı (AvatarMask) + eklemeli katman. Rastgele başlangıç zamanı ve ±%5 hız farkı.
4. **Lokomosyon:** Rocketbox `xy` yürüme/koşma (erkek/kadın ayrı) birincil, UAL yedek (`Walk_Loop`). Başla/dur/dön klipleri katalogdaki envanterden seçilir. Yol: mekân noktaları arası ara noktalar (kod mekânları için basit grafik); NavMesh (AI Navigation) ancak onayla.
5. **Sinematik hileler (ücretsiz ve etkili):** kısa konum düzeltmesini kesme arkasında yap, `Kay()` kullanma; yürüyüşte omuz/takip planı; yakın planda ayak görünmesin. Ama kullanıcı yürüme görmek istiyor: her sahnede en az bir tam boy yürüme planı gösterilebilmeli.
6. **Denetçi:** ayak yere yakınken yatay hız (kayma, cm), kök hızı ile klip hızı farkı, yürürken idle klibi, aynı jestin art arda tekrarı, karakter-karakter/eşya çakışması. Eşik aşılırsa satır numarasıyla uyarı (köprü çıktısı).

### Uygulama sırası ve maliyet
| Aşama | İş | Maliyet | Beklenen etki |
|---|---|---|---|
| 0 | H1, H2/H2b, H3, H6, H7, H9, H11 düzeltmeleri; her hareket için açık klip adı | 0 | "saçma hareket" ve "yürümüyor" şikâyetinin büyük kısmı (ÇIKARIM) |
| 1 | Rocketbox `xy` yürüme/koşma/kalkma klipleri, doğru içe aktarma, hız eşleme, `Kay()` kaldırma | 0 (kullanıcının indirdiği MIT paket) | gerçek yürüme/koşma |
| 2 | Katalog + planlayıcı + üst gövde katmanı + denetçi | 0 (geliştirme zamanı) | tutarlılık, otomasyon |
| 3 | Ayak IK, gövde-baş-göz bakış sırası, oturma geçişleri, eşleşmiş etkileşimler (Mixamo/UAL2) | 0 – 15 USD | sinematik cila |
| 4 | Eksik özel hareketler: video mocap / metinden harekete + Blender temizliği | 0 – ~12-20 USD/ay (gerekirse) | uzun kuyruk |

## Teknoloji kayıtları (doğrulama tarihi hepsi 2026-11-27; hiçbiri bu projede test edilmedi, aksi yazılmadıkça)
| Ad / kategori | Ne yapar, nasıl | Katkı | Unity / donanım | Maliyet / lisans | + / − | Alternatif | Kaynak, güven |
|---|---|---|---|---|---|---|---|
| Rocketbox animasyonları / mocap kütüphanesi | Biped iskeletli FBX; Humanoid retarget | yürüme, koşma, oturma, jest | static kısmı projede bağlı; CPU yeter | 0 / MIT | gerçekçi, aynı iskelet / dönüş-geçiş envanteri belirsiz | UAL, Mixamo | S001-S005, A (lisans), C (klasör içeriği) |
| UAL 1-2 / CC0 paket | oyun animasyonları | dövüş, oturma geçişi, yedek | projede | 0-15 USD / CC0 | temiz lisans / oyunsu duruş [S005] | Rocketbox | S018-S020, A |
| Mixamo / web kütüphane | oto-rig + klip indirme | yürüme, dövüş, duygu | Humanoid; tarayıcı | 0 / telifsiz, ham dağıtım yasak | çok çeşit / elle indirme, hesap, belirsiz gelecek | UAL2 | S015-S016, A/B |
| Playables / motor API | kodla katmanlı karışım | tüm mimari | yerleşik | 0 | paket gerekmez / Foot IK belirsiz | Animator Controller | S009-S010, A/C |
| Animation Rigging / paket | C# job IK kısıtları | ayak, el, bakış | 1.4.1 (Unity 6.0); 6000.6 uyumu test | 0 / Unity paket lisansı (okunmadı) | hazır kısıtlar / onay gerekir | kendi IK kodu | S011-S012, A |
| Rokoko Vision / video mocap | videodan iskelet | özel hareket | bulut; kamera/telefon | 30 sn/ay 0; ~10-20 USD/ay | kolay / kota, temizlik, çıktı hakkı belirsiz | QuickMagic | S022, A/D |
| FreeMoCap / yerel mocap | çok kameralı yeniden yapım | sınırsız yerel çekim | CPU mümkün (hız bilinmiyor) | 0 / AGPL-3.0 | yerel / kurulum zor, olgunluk | bulut araçlar | S027, A |
| Cascadeur / animasyon editörü | fizik destekli elle animasyon | klip düzeltme | masaüstü | Indie 96 USD/yıl | iyi düzeltme / ücretsiz sürüm kullanışsız | Blender | S026, A |
| Kimodo, HY-Motion / metinden harekete | difüzyon modeli | boşluk doldurma | NVIDIA, yüksek VRAM | 0 + donanım / özel lisanslar | esnek / bu PC'de değil | Rokoko Create | S031-S035, A/D |

## Karşılaştırma ve çelişkiler
- Rocketbox animasyon sayısı: resmî README **417** [S001], üçüncü taraf **471** [S003]; static 326 [S004]. Çözüm: `KutuphaneHam` dosyaları sayılmalı.
- Rocketbox klipleri "yerinde" [S004][S005] ile "`xy` ilerleyen yürüme" [S003]: büyük olasılıkla farklı klasörlerden söz ediliyor (ÇIKARIM); dosyada kontrol.
- Rokoko fiyat sayfasında iki farklı fiyat seti [S022].
- `Docs/VideoKalitesi.md` "yürüme/koşma (Mixamo)" öneriyor; bence önce eldeki Rocketbox + UAL kullanılmalı (hesap/indirme gerektirmez).
- `HareketTesti.cs` yalnız klip **varlığına** bakıyor; "EVET" çıktısı görüntünün doğru olduğunu göstermez (dosyanın kendi notu). H1-H3 bu testten geçer.

## Riskler
- Düzeltmeler yapılmadan yeni klip eklemek bulanık eşleme yüzünden başka yerlerde sessiz bozulma yaratır.
- `xy` kliplerinin yanlış içe aktarımı (geri sıçrama).
- Retarget: boy/oran farklı karakterlerde (çocuk) ayak kayması ve kol iç içe geçmesi; D001'de boy farkı zaten sorun.
- Mixamo ham klipleri dağıtılan uygulamaya girerse lisans riski [S016].
- SMPL tabanlı veya "ticari yok" araçların çıktısı kütüphaneye karışırsa lisans riski → katalogda lisans alanı zorunlu.
- Kayıt kare hızı dalgalanırsa `Random` ve `Time` bağımlı hareket tekrarlanamaz (A09 ile kontrol).

## Test önerileri
**Bu bilgisayarda (GPU istemez):**
1. T1 (H1): bir `Gercek_*` karakter konuştuktan sonra `CurrentClip` günlüğe yazılsın. `Idle_Talking_Loop` ise H1 doğrulanır.
2. T2 (H2): `[X|normal|dis|yuru]` satırında yürüme boyunca klip adı günlüğe; 5 fps kare serisi.
3. T3: `Find()` sonuç tablosu: her takma ad × bir kadın, bir erkek Rocketbox, bir UAL karakteri.
4. T4: `KutuphaneHam/.../Animations` klasör adları ve walk/run/turn/sit dosya listesi (yalnız listeleme).
5. T5: tek `xy` yürüme klibini XZ bake açık/kapalı iki ayarla karşılaştır.
6. T6: ayak kayması ölçümü (duruş fazında ayak kemiği yatay yolu) önce/sonra.
7. T7: Foot IK bayrağı açık/kapalı fark var mı.
8. T8: 30 sn örnek (gir → yürü → dur → dön → otur → kalk → koşarak çık) kullanıcıya gösterilip onay alınmadan uzun video yok (DURUM.md süreç kuralı). Akıcılık video olarak izlenmeli; kare görüntüsü yetmez.
**GPU isteyen (sonraya):** Kimodo / HY-Motion denemesi; yerel video mocap modelleri.

## Öz denetim
- Unity çalıştırılmadı: H1-H12 kod okuma çıkarımıdır. H1 klip dosya adlarının kullanıcının diskinde beklendiği gibi olmasına bağlı.
- `Packages/manifest.json`, `RbAnim`, `KutuphaneHam`, Mixamo klasörleri bu kopyada yok; incelenemedi.
- Arama bütçesi doldu: MediaPipe, tek kamera doğruluk çalışmaları, Unity stride/ayak kayması resmî rehberi, motion matching, dönüş teknikleri, SIGGRAPH literatürü, ücretli Unity eklentileri (Final IK, Animancer vb.), Move AI, mocap kıyafet fiyatları araştırılamadı.
- Lisans sayfaları özetleyici araçla okundu; A12 insan gözüyle doğrulatmalı.
- Unity 6000.6 root motion sayfası okunamadı; `applyRootMotion=false` davranışı kaynakla doğrulanmadı.
- Protokolün "resmî + bağımsız test + kullanıcı deneyimi" üçlüsü çoğu araçta tamamlanmadı; bağımsız kalite testi bulunamadı.

## Eksik alanlar / açık sorular
1. `xy`/`xyz` klasöründe dönüş, başla/dur, merdiven, yere düşme, yerden kalkma klipleri var mı?
2. Uygulama EXE'si üçüncü kişilere dağıtılacak mı (Mixamo ve animasyon dosyası paketleme)?
3. Kullanıcının webcam'i/telefonu ve çekim yapacak boş alanı var mı?
4. Aylık küçük ücret (video mocap) kabul edilebilir mi?
5. Parmak ve kavrama animasyonu kaynağı.
6. Çocuk karakterlere özel yürüme klibi var mı?

## Diğer ajanlardan beklenen / ortak konular
- A01/A08: yürüyen karakter için takip planı, kesme arkasında konum düzeltme, plan süresi ↔ yürüme süresi.
- A02: karakter boy/oran ve iskelet farkı; parmak kemikleri; etek/saç fiziği (oturma).
- A07: konuşma jesti ve baş vurgusunun ses zamanlamasına bağlanması.
- A09: sabit zaman adımı, Playables + Recorder, Animation Rigging 6000.6 uyumu.
- A05: mekân noktaları, engeller, oturma yüksekliği meta verisi.
- A12: Mixamo, Rokoko/QuickMagic çıktı hakkı, CMU, 100STYLE atfı, HY-Motion bölge koşulu.

## Öneriler (kanıtla)
1. **Önce hata düzelt** (H1, H2, H3): kod kanıtı yukarıda; maliyet sıfır. T1-T3 ile önce doğrula.
2. **Yürüme/koşmayı Rocketbox `xy` kümesinden al**, içe aktarma ayarını ayrı yaz (`NesneKur.cs:742, 917`; [S003][S008]).
3. **Bulanık ad eşlemeyi bırak, açık Hareket Kataloğu kullan.**
4. **Daha az hareket:** susan karakter sakin dursun; jest istisna olsun.
5. **Yerinde kayma ve klipsiz ışınlamayı kaldır**; geçiş klibi yoksa kamera kesmesi kullan.
6. **Sayısal hareket denetçisi + 30 sn onay videosu** olmadan "hazır" deme.
7. Mocap/AI araçlarına şimdi para harcama; Aşama 0-2 sonrası kalan boşluk listesine göre karar ver.
