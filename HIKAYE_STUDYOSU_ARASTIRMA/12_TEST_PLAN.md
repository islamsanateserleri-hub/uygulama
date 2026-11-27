# 12 TEST PLANI

Tarih: 2026-11-27. **Durum: PLAN HAZIR, HİÇBİR TEST YAPILMADI.** Bu plandaki hiçbir sayı ölçüm sonucu değildir. Tek gerçek gözlem protokol öncesi D001 sessiz önizlemesidir (`13_TEST_RESULTS.md`).

> **Unity koşulu uyarısı (K-004, Q-016):** netleşene ve kullanıcı karar verene kadar [SERBEST] testlerdeki "köprü komutu" yolu KULLANILMAZ. Unity işlemlerini kullanıcı menüden elle başlatır ya da onay verirse Unity CLI yolu kurulur (`AGENTS/D1_duzeltme_arastirmasi.md` §3). [SERBEST] işareti Unity koşulu açısından serbestlik anlamına gelmez.
>
> **Eklenen deneme (GÖRÜNÜM, Q-018) [KOD][İNDİRME olabilir][KULLANICI]:** aynı 10-20 sn'lik sahne üç görünümle (bugünkü; ışık/gölge iyileştirilmiş gerçekçi; VRoid + MToon + boyama tarzı mekân). Ölçüt: kullanıcının kör seçimi, kayıt süresi, tutarlılık. Yapılmadı.

## 0. Okuma anahtarı

**Kanıt türü:** KOD = statik kod okuması (çalıştırılmadı) · KAYNAK = belge/yayın · ÇIKARIM = yorum ya da başlangıç değeri · TEST EDİLMEDİ.

**İzin işaretleri (her testin yanında):**
| İşaret | Anlamı |
|---|---|
| [SERBEST] | Mevcut uygulama ve mevcut menü/köprü komutlarıyla; indirme ve kod değişikliği yok |
| [KOD] | Ayrı kopya projede küçük kod/ayar değişikliği ister. Kullanıcının "geliştirmeye geç/prototip yap" onayı gerekir (protokol §19) |
| [İNDİRME] | Dosya, araç ya da paket indirme/kurma ister; kullanıcı onayı gerekir |
| [KULLANICI] | Kullanıcının izlemesi, dinlemesi, hesabı ya da donanım bilgisi gerekir |
| [PARA] | Ücret doğurabilir |
| [KOTA] | Gemini TTS kotası harcar (projede gözlenen ~10 istek/gün; resmî olarak doğrulanamadı, A00 §S6) |

## 1. Ortak kurallar (bütün testler)

1. **Ayrı kopya.** Kod ya da ayar isteyen her deney projenin kopyasında yapılır; asıl proje değişmez (bkz. §6).
2. **Kayıt altına al:** tarih, git sürümü, Unity sürümü, hikâye dosyası, ayarlar, donanım, sürücü sürümü, kare sayısı, süre. Sonuç `13_TEST_RESULTS.md`'ye yalnız gerçekten yapılınca yazılır.
3. **Ortak performans ölçümleri (P):** toplam kayıt süresi; "1 saniye video kaç saniyede kaydediliyor"; ortalama ve en kötü kare süresi; tepe RAM; atlanan/hatalı kare sayısı (`HataliKare`); disk kullanımı; işlemci sıcaklığı (A10 §Test önerileri T1, A09 §Test 1, A06 §Karşılaştırma 4).
4. **Tekrar edilebilirlik uyarısı:** bugün aynı hikâye iki kayıtta aynı çıkmaz (tohumsuz rastgelelik; KOD: A09 K8, A02 §S1, A05 M11). Piksel piksel karşılaştırma ancak bu giderilince anlamlıdır. O zamana kadar her ölçüm 3 kez alınır, ortanca yazılır (ÇIKARIM).
5. **Kullanıcı kabulü şart.** Sayısal ölçüt geçse bile video sesli ve hareketli izlenmeden "hazır" denmez (R-005; A03 T8, A08 §9). Uzun video önce 20–30 sn örnek onayı almadan üretilmez.
6. **Güvenli alan rehberi** renk karşılaştırmalarında kapatılır (A04 §S1 notu).

## 2. Ön teşhis testleri (önce bunlar)

Amaç: kod okumasıyla bulunan kusurlar gerçekten var mı? Sıra X2 §7 ve X1 §8 önceliklerine göredir.

| No | Test | Neyi doğrular | Yöntem | Başarı / karar ölçütü | İzin | Kaynak |
|---|---|---|---|---|---|---|
| T0-1 | Durgun duruş klibi | `dur` konuşma jestine gidiyor mu (A03 H1; X2 D1) | Bir `Gercek_*` karakter konuştuktan sonra çalan klip adını (`CurrentClip`) günlüğe yaz; `Find()` sonuç tablosu (A03 T1, T3) | `Idle_Talking_Loop` çıkarsa H1 doğrulanır | [KOD] (günlük satırı; elde araç yoksa) | A03, X2 |
| T0-2 | Yürüme ezilmesi | Konuşma satırı yürümeyi eziyor mu (yalnız satır içi `yuru`/`kos`; X2 D2) | Satır içi yürüme etiketli satır + ayrı `git:` komutu; 5 kare/sn seri (A03 T2) | Satır içinde kayma görülürse H2 doğrulanır; `git:` etkilenmemeli | [SERBEST] (köprü önizleme) | A03, X2 |
| T0-3 | Yüz Testi | Rocketbox'ta yalnız çene mi oynuyor (A02, A07; X2 D7) | `menu: Hikaye/Yuz Testi`, `menu: Hikaye/Cene Gorselleri`; gerçek şekil adlarını listele; diş görünürlüğüne de bak (X1 U9) | Her `Gercek_*` için şekil sayısı, kanal listesi, çene kemiği, diş durumu kayıtlı | [SERBEST] | A02 T-A02-1, X1 |
| T0-4 | Derleme raporu | EXE'ye Mixamo/VRM/NASA ne giriyor (X2 D6, L1–L3) | Mevcut derleme varsa `Editor.log` varlık listesi; `ffmpeg.exe -version` ve `-L` (A12 T1, T2) | Katalogdaki 36 Mixamo kaydı ve Mixamo animasyon klipleri pakette mi: evet/hayır | [SERBEST]; yeni derleme gerekirse ~10,8 GB disk | A12, X2 |
| T0-5 | Ses döküm denetimi | Reddedilen filmde kaç satır uyumsuz (A07 T1) | 68 `NN.wav` dinlenir (kullanıcı) ya da yerel Whisper ile döküp metinle karşılaştırılır | Uyumsuz satır listesi | Dinleme: [KULLANICI]; Whisper: [İNDİRME] | A07 |
| T0-6 | Temel ölçüm | Bugünkü hız ve bellek (taban çizgisi) | 30 sn referans hikâye kaydı; P ölçümleri; aynı hikâye iki kez (tekrar farkı) | Taban tablosu dolu | [SERBEST] (sesler hazırsa; değilse [KOTA]) | A09 T1, T9; A10 T1 |
| T0-7 | A/V senkron | Ses-görüntü kayması (ms) | Klaket (flaş+bip) ile 30 sn ve 5 dk; hem Recorder hem EXE yolu | Baş ve sonda fark ≤2 kare (≈67 ms, A00 §S1 eşiği; ÇIKARIM) | [SERBEST]/[KULLANICI] | A07 T3, A09 T3 |
| T0-8 | Disk bütçesi | 45 GB yeter mi (X1 §3, §8) | Klasör boyutlarını listele (`Build`, `Library`, `Kayitlar`, `KutuphaneHam`); tek tablo | Kalan boş alan ve her sonraki adımın ihtiyacı yazılı | [SERBEST] (yalnız okuma; silme yok) | A10 T5, X1 |
| T0-9 | Proje ayarları | Color space, URP Asset HDR/gölge/SSAO (Q-008) | Ayarlar okunur; değiştirilmez | Değerler kayıtlı | [SERBEST] | A04 §S1 |
| T0-10 | HDR + alfa | HDR açılınca saydam bindirme bozuluyor mu (Ç-A, C-024) | İki durum: (a) 3D mekân tam kare, (b) 2D arka plan üstünde 3D karakter; 32-bit ve 64-bit/Alpha Processing | Kenarda hale/siyah kenar yok; süre farkı ölçülü | [KOD] | X1, A06 |
| T0-11 | 60 m sınırı | Uzak kırpma düzlemi geniş planı kesiyor mu | 60 m ötesine büyük öğe (ejderha/şato) koy | Görünüp görünmediği | [KOD] | X1 §0.3 |
| T0-12 | DX11 ↔ DX12 | Araçların istediği DX12 bu PC'de çalışıyor mu (C-028) | Aynı sahne iki API ile, EXE dahil; kare, süre, kararlılık | Sonuç tablosu; sonuçtan önce geçiş yok | [KOD] | X1 G4 |
| T0-13 | EXE'de DoF/bloom/tonemapping | Shader'lar derlemede siliniyor mu (Q-010) | Editör karesi ↔ EXE karesi | Aynı etki iki tarafta da görülür | [KOD] (kalıcı Volume Profile denemesi) | A09 K3, A04 T6 |
| T0-14 | Yarıda kesilen kayıt | Dosya okunabiliyor mu | Kayıt sırasında süreci kapat; normal MP4 ve parçalı MP4 | Kurtarılabilen süre | normal: [SERBEST]; parçalı: [KOD] | A09 T8 |

## 3. Beş test sahnesi (protokol §14)

Her sahne **A (mevcut hâl)** ve **B (aday iyileştirme)** olarak iki kez kaydedilir. A sürümü çoğunlukla [SERBEST]; B sürümü [KOD] ister. Süreler 10–30 sn tutulur.

### TS-1 — Gerçekçi karakter yakın planı ve yüz ifadeleri

| Alan | İçerik |
|---|---|
| Teknoloji | URP; Rocketbox ARKit 52 + 15 viseme (genişletilmiş eşleme, B); duygu tablosu; cilt/göz shader denemesi (B2); mercek/kadraj ayarı |
| Varlık | Projede olan bir kadın ve bir erkek `Gercek_*` karakter; mevcut bir iç mekân; yeni indirme yok |
| Yöntem | Tek karakter; 6 duygu (mutlu, üzgün, şaşırmış, korkmuş, kızgın, nötr), her biri kısa bir replik; b/p/m içeren bir cümle. A: bugünkü eşleme. B1: genişletilmiş ARKit + viseme, çene tek yoldan. B2: B1 + göz parlaması/cilt-göz shader. En yakın plan MCU/CU; **ECU yüz testi geçene kadar kapalı** (X2 G3). Normal yüz planında kamera uzaklığı 1 m altına inmez (A01 kural 9; C-022: 100 mm/0,7 m ECU tablosu bu kuralla çeliştiği için 135 mm ≈0,98 m denenir, X2 G1) |
| Render ayarı | 1080x1920, 30 fps; MSAA 4x; gölge ve HDR: A'da kapalı (mevcut), B'de açık; DoF Editör'de, ayrıca EXE karesi |
| Beklenen sonuç | ÇIKARIM: B1'de dudaklar kapanıp açılır, duygular daha okunur; B2'de göz "canlı" görünür. Fotogerçek yüz beklenmez (A02 "dürüst sınır") |
| Başarı ölçütü | (1) Sesi kapalı izleyen en az 3 kişide duyguyu doğru bilme oranı A'dan yüksek (A02 T-A02-2); (2) b/p/m'de tam dudak kapanışı görülür (A07); (3) diş/ağızda göze batan kusur yok; (4) yüzde çift çene hareketi yok; (5) kullanıcı B'yi tercih eder |
| Performans ölçümü | P + shader açık/kapalı kare süresi farkı + 10 sn kayıt süresi |
| Maliyet | 0 TL. Replik sesleri [KOTA]. Mağaza shader denenmez |
| Tekrar edilebilirlik | Aynı hikâye dosyası, aynı karakter, sabit kamera; rastgele göz kırpma/bakış tohumlanana kadar kareler birebir aynı olmaz |
| İzin | A: [SERBEST] · B: [KOD] · kör izleme: [KULLANICI] |

### TS-2 — İki karakterli diyalog sahnesi

| Alan | İçerik |
|---|---|
| Teknoloji | Mevcut `KameraYonetmen` + elle `kamera:` komutları (A); göz çapası, bakış boşluğu, 180°/30° kuralı, tepki planı, satır içi kesme (B); gerçek kelime zamanlı altyazı (B, A07); durgun duruş düzeltmesi (H1) |
| Varlık | İki `Gercek_*` karakter, bir iç mekân, mevcut müzik/ambiyans |
| Yöntem | 8–10 replikli, 20–30 sn tartışma sahnesi. A: bugünkü otomatik kurgu. B1: elle yazılmış 10–14 planlık liste, değişken boşluk, 2–3 J/L (A08 T1; yeni ölçekler ve J/L prototip ister). B2: altyazı zamanı hizalama dosyasından (MFA / parseh / faster-whisper karşılaştırması). Ön koşul: H1 düzelmeden tepki planı denenmez (X2 §0.5: dinleyen konuşuyor gibi görünür) |
| Render ayarı | 1080x1920/30; karaoke ve vurgusuz altyazı iki sürüm; alt güvenli alan yasak bölge (C-007, C-029) |
| Beklenen sonuç | ÇIKARIM: B'de konuşan/dinleyen anlaşılır, ritim daha canlı, altyazı sesle birlikte yürür |
| Başarı ölçütü | İçerik uyumsuz satır = 0; kelime başlangıcı hatası medyan ≤50 ms, %95 ≤120 ms (A07 hedefi; Türkçede ulaşılabilirliği bilinmiyor, X2 §5); medyan plan süresi 2,5–3 sn (A08; ifade/jest yoksa 3,5–4 sn'ye gevşer, X2 G6); istemsiz jump cut = 0; yüzü kapalı plan = 0; altyazı ≤2 satır, ≤17 karakter/sn (A00-S001); −14±1 LUFS, tepe ≤−1 dB (tercih, platform kuralı değil); kullanıcı kör seçimde B'yi seçer |
| Performans ölçümü | P + ses işleme/hizalama süresi (CPU) + A/V farkı |
| Maliyet | 0 TL; [KOTA]; ElevenLabs kullanılmaz |
| Tekrar edilebilirlik | Aynı wav dosyaları saklanır (yeniden seslendirme yok); hizalama dosyası ve wav özeti kaydedilir |
| İzin | A: [SERBEST] · B: [KOD] · hizalayıcı: [İNDİRME] · dinleme/izleme: [KULLANICI] |
| Ek | Altyazı A/B kullanıcıya telefonda izletilir; güvenli alan gerçek telefonda kontrol edilir (A01 T6) |

### TS-3 — Yürüme, koşma ve kamera takibi

| Alan | İçerik |
|---|---|
| Teknoloji | Playables; UAL yürüme (CC0, projede); Rocketbox ilerleyen (`xy`) yürüme/koşma klipleri (varsa); root motion ya da hız eşleme; takip kamerası (mevcut ↔ Cinemachine, isteğe bağlı) |
| Varlık | Bir kadın, bir erkek `Gercek_*`; `KutuphaneHam/Rocketbox/Animations` (önce yalnız listeleme, A03 T4); derin bir mekân (sokak/koridor) |
| Yöntem | 30 sn: gir → yürü → dur → dön → otur → kalk → koşarak çık (A03 T8). A: bugünkü. B1: H1–H3 düzeltmeleri. B2: Rocketbox `xy` klibi, XZ bake açık/kapalı iki ayar (A03 T5). B3: takip planı |
| Render ayarı | 1080x1920/30, tam boy plan en az bir kez; gölge açık sürüm (ayak teması görünsün) |
| Beklenen sonuç | ÇIKARIM: karakter kaymadan yürür; durunca sakin durur |
| Başarı ölçütü | Temas anında ayağın yatay kayması ölçülür (cm) ve B'de A'dan belirgin az; döngü sonunda geri sıçrama yok; yürürken idle klibi çalmaz; eşya/duvar içinden geçme yok; video olarak izleyen kullanıcı "yürüyor" der (kare görüntüsü yetmez) |
| Performans ölçümü | P + animasyon güncelleme CPU süresi |
| Maliyet | 0 TL (Mixamo indirilmez; Cascadeur/mocap yok) |
| Tekrar edilebilirlik | Sabit başlangıç noktaları ve hedefler; klip adları günlüğe yazılır |
| İzin | A: [SERBEST] · B: [KOD] · Rocketbox animasyon paketi diskte yoksa [İNDİRME] (9,4 GB notu, A03 §S1; disk testi T0-8 önce) |

### TS-4 — Atmosfer, ışık ve efekt sahnesi

| Alan | İçerik |
|---|---|
| Teknoloji | URP gölge, HDR + tonemapping (ACES/Neutral), SSAO, plana göre key/fill/rim, reflection probe; Shuriken + CC0 flipbook ateş/duman; göz parlaması (hale kartı ↔ HDR + emissive + bloom) |
| Varlık | `sato` mekânı, bir karakter; B2 için CC0 PBR doku (Poly Haven/ambientCG) ve CC0 flipbook |
| Yöntem | 10–15 sn gece/meşale sahnesi, bir "güç patlaması" anı. Adımlar tek tek açılır: taban → gölge → +HDR/ton → +SSAO → +karakter ışıkları → +PBR doku → +3D efekt (A04 T1, A06 VFX-T1–T3). Her adımda aynı kare ve süre |
| Render ayarı | 1080x1920/30; gölge haritası 2048, 1–2 gölgeli ışık; 4x MSAA **ya da** 1,5x ölçek (ikisi birlikte ve 8x ölçmeden açılmaz, A10 §S9); 2160x3840 ve 8192 gölge bu PC'de başlangıç değeri değildir |
| Beklenen sonuç | ÇIKARIM: karakter zemine oturur, arka plandan ayrılır, ışık tek turuncu tondan çıkar |
| Başarı ölçütü | Yüz her planda okunur; zemin teması görünür; parlak alanlarda kırpılma az; 2D efektle yan yana karşılaştırmada kullanıcı 3D sürümü seçer; sürücü çökmesi yok; kayıt süresi kullanıcının kabul ettiği sınır içinde (sınır henüz belirlenmedi) |
| Performans ölçümü | Her adımın ayrı kare süresi ve belleği |
| Maliyet | 0 TL; CC0 indirmeler |
| Tekrar edilebilirlik | Efekt tohumu yoksa kare farkı beklenir; ayar listesi kayıtlı |
| İzin | Taban: [SERBEST] (`onizleme_goruntu`) · B: [KOD] · doku/flipbook: [İNDİRME] · volumetric paket bu testte yok (ayrı onay) |

### TS-5 — Kalabalık, yaratık ve çevre içeren karmaşık sahne

| Alan | İçerik |
|---|---|
| Teknoloji | Skinned karakter kalabalığı (3 halka fikri, A06 §S6); iskeletli yaratık ↔ kodla ejderha; uzak plan için kart/sis; kamera uzak düzlemi mekândan; modüler/PBR çevre |
| Varlık | Rocketbox karakterler; varsa lisansı temiz iskeletli ejderha adayı (bulunamazsa kodla ejderha ile yalnız süre testi) |
| Yöntem | 8–15 sn: meydan, ejderha geçişi, diz çöken kalabalık, bir tepki planı. 10 / 30 / 60 karakter, gölgeli ve gölgesiz (A06 VFX-T4, T5). Ayrıca efektlerle birlikte en ağır durum (X1 §8.6) |
| Render ayarı | 1080x1920/30; düşük ve yüksek iki profil |
| Beklenen sonuç | ÇIKARIM: 10–30 karakter kayıt süresi pahasına mümkün olabilir; 60 belirsiz (X1 §3). Örnek videodaki ejderha/ordu görünümü beklenmez |
| Başarı ölçütü | Çökme yok; tepe RAM güvenli; ejderha 60 m sınırında kesilmez; herkes aynı anda aynı hareketi yapmaz; kayıt süresi kabul sınırında; kullanıcı görüntüyü "kullanılabilir" bulur |
| Performans ölçümü | P + karakter sayısına göre süre eğrisi |
| Maliyet | 0 TL hedef; ücretli ejderha satın alınmaz (A06: gerçekçi, ücretsiz, lisansı net aday doğrulanamadı) |
| Tekrar edilebilirlik | Figüran yeri/modeli bugün tohumsuz (A05 M11): sayı ve ayar sabit tutulur, 3 ölçüm |
| İzin | [KOD] · ejderha modeli [İNDİRME] + mağaza hesabı [KULLANICI] |

## 4. İkincil testler (beş sahneden sonra, gerekirse)

| Test | İçerik | İzin | Kaynak |
|---|---|---|---|
| Kamera ızgarası | Aynı karakter ECU→FS, 35/65/100 mm; yüz bozulması | [KOD] | A01 T1 |
| Açılış | Siyah kanca ↔ görüntü üstü kanca | [KOD] | A08 T3 |
| Ses düzeyi | İki geçişli loudnorm; seçilen mod, Hz, LUFS/tepe; bu sırada "öğrenen kazanç" kapalı | [KOD] | A00-S008, X2 S8 |
| Karakter uyum kabulü | Yeni karakter kaynağı için yüz + gövde + malzeme + EXE | [İNDİRME] | X1 §8.1 |
| Mekân eşyaları | 205 CC0 eşyanın ad eşlemeyle bağlanması; stil uyumu | [KOD] | A05 T1, T2 |
| Compute Light Baker + APV | iGPU'da pişirme çalışıyor mu | [KOD] | A04 T7 |
| VFX Graph / Volumetric / splat | Paket denemeleri; çökme günlüğü | [İNDİRME] | A06, A11 |
| MPFB ↔ Rocketbox | Tek karakter karşılaştırması | [İNDİRME] | A02 T-A02-7 |
| Blender kare süresi | EEVEE ve Cycles CPU | [İNDİRME] disk | A04 T8 |
| HDRP prototipi | **Önerilmiyor** (HDRP kaldırılacak, K-003); yalnız karşılaştırma merakı için | — | A04, A09 |

## 5. Ekran kartı ya da para isteyen testler (şimdi yapılmayacak)

CC5 / MetaHuman denemesi; bulut ya da kiralık GPU'da kayıt (Unity Editor bulutta koşulları nedeniyle derlenmiş EXE; A10, A12 incelemesi); Cycles GPU; yoğun VFX Graph; Audio2Face. Video-to-video (D4) testi kullanıcı Q-015'e "evet" demedikçe yapılmaz (A11 T4). Hepsi [PARA] ve/veya [KULLANICI].

## 6. Prototip planı (ayrı)

**Amaç:** asıl uygulamaya dokunmadan B sürümlerini denemek. Başlaması kullanıcının ayrı onayına bağlıdır.

1. **Kopya:** projenin ayrı klasörde/ayrı dalda kopyası. Önce T0-8 disk bütçesi (kopya + `Library` yer ister).
2. **Aşama P0 — yalnız ölçüm:** T0-1…T0-9. Kod değişikliği yalnız günlük satırları.
3. **Aşama P1 — tek tek düzeltme:** her düzeltme ayrı adım; önce/sonra aynı sahne: H1/H2 (hareket), yüz eşlemesi, ses kapısı, gölge/HDR, DoF derleme.
4. **Aşama P2 — beş sahne B sürümleri.**
5. **Aşama P3 — karar toplantısı:** sonuçlar `13_TEST_RESULTS.md`'ye; ölçüte göre "ana projeye alınsın / alınmasın".

**Geçiş kuralı:** bir değişiklik ancak (a) ölçütü geçerse, (b) kayıt süresi kabul edilirse, (c) eski hikâyeler bozulmazsa (referans kare karşılaştırması, A09 Riskler), (d) lisansı temizse, (e) kullanıcı onaylarsa ana projeye taşınır.

## 7. Bu planın eksikleri
- "Kabul edilen en uzun kayıt süresi" bilinmiyor; kullanıcıdan alınmalı.
- Eşiklerin çoğu ÇIKARIM; ilk ölçümlerden sonra güncellenecek.
- Küçük izleyici sayısı (1–3 kişi) kesin istatistik vermez.
- Editör ve EXE yollarının aynı kurguyu üretip üretmediği testi ancak ortak veri yapısı olunca tam yapılabilir (X2 §6.3).
