# DEN — Bağımsız denetçi raporu (protokol §17)

Tarih: 2026-11-27. Araştırmayı yürütmedim. Git komutu çalıştırmadım, kod değiştirmedim, Unity çalıştırmadım, test yapmadım; yalnız bu dosyayı yazdım.

**Sınırlar:** 00–16 numaralı dosyalar, REPORTS (hepsi), TOPICS, X1 ve X2 okundu; A00–A12 raporları anahtar kelime ve seçili bölümlerle tarandı (tam okunmadı). SOURCES kayıtlarından örnek alındı. **Süreç sapmam:** bana verilen web sınırı 20 arama/okumaydı; 24 çağrı yaptım (biri başarısız). Aşırı kısım çoğunlukla Unity koşulları bulgusuna (K1) gitti. Web sayfaları özetleyici araçla okundu. Hukuki görüş değildir.

---

## A. KRİTİK

### K1. Unity koşullarındaki yapay zekâ ajanı riski yanlış/eksik değerlendirilmiş
- Araştırma yalnız **Editor Software Terms §2.5**'i okumuş [A12-S005] ve "bizim durum metinde geçmiyor, §2.5 bizi kapsamıyor" demiş (Q-016, R-010 "belirsiz", R10 §4).
- **Ana Unity Terms of Service** (son güncelleme 16 Kas 2026) hiç okunmamış. §17.2(ff), herhangi bir "Offering" ile yapay zekâ ajanı, büyük dil modeli, komut satırı vb. yoluyla etkileşimi, "Authorized Agentic Access" dışında yasaklıyor; bölüm ihlalinde hakların kendiliğinden sona ereceği yazıyor. "Authorized Agentic Access" tanımı Unity'nin işlettiği/belirlediği bir geçit (Unity CLI) ve yetkilendirilmiş ajan istiyor. "Offerings" tanımı "Software, Online Services and Entitlements" içeriyor. Yani A12'nin "tanım bulunamadı" notu da geçersiz: tanım bu belgede var. https://unity.com/legal/terms-of-service
- Unity çalışanı (6 Tem 2026): yalnız kendi dosyalarına dokunan araç sorun değil, ama araç **Unity Editor'ü sürüyorsa koşullar devreye girer**; yaptırım, üçüncü taraflar incelenene kadar başlamayacak. https://discussions.unity.com/t/new-terms-of-service-is-unity-restricting-local-ai-tools-and-ai-training/1724661/21
- Karşı yönde (ikincil, C): Unity destek hesabının "yerel Editor entegrasyonları serbest" dediği aktarılıyor; aynı yazı koşul metninin bunu açıkça söylemediğini not ediyor. https://roboin.io/article/en/2026/07/02/unity-denies-concerns-over-ban-on-third-party-ai/
- **Etki:** Projenin çalışma biçimi (Claude → `HikayeKomut/komut.txt` → Unity Editor) tam bu alanda. Test planındaki birçok [SERBEST] test de köprüye dayanıyor. Sonuç "yasak" diye kesinleştirilemez; "kapsamıyor" da denemez. **Kullanıcı özetinde bu risk hiç yok.**
- **Düzeltme:** C-030 çelişki kaydı aç; Q-016 ve R-010'u yeniden yaz (seviye yüksek); kullanıcı özetine sade bir uyarı ve karar sorusu ekle; seçenekler: (a) testleri kullanıcı Unity menüsünden elle başlatsın, (b) Unity'nin yetkili yolu (Unity CLI) ve ajan yetkisi araştırılsın, (c) Unity'ye yazılı sorulsun. Netleşene kadar köprüyle Editor sürmenin riski kullanıcıya bildirilsin.

### K2. Protokolün zorunlu denetim sırası ve kalıcı hafıza güncel değil
- `16_RESEARCH_AUDIT.md` boş ("HENÜZ YAPILMADI"): §16 öz denetim yapılmadan bağımsız denetçiye gelinmiş. Bu rapor §16'nın yerine geçmez.
- `00_MASTER_INDEX`: "Çapraz denetim bekliyor; test planı, 13 rapor bekliyor". `02`: "test planı ve raporlar yazımı sürüyor". `03`: çapraz denetim, test planı ve rapor oturumunun kaydı yok. §7.10 ve §20 ("dosyaları güncellemeden oturumu bitirme") ihlali riski.
- Bayat kayıtlar: Q-012 (A00'ın kapattığı konular hâlâ "aranamadı"), Q-014 (OVR arşivi A00'da bulundu, "açık"), Q-004 (R13 yanıtlıyor, "açık"), C-002 ("yeniden değerlendirecek" denmiş, yapılmamış), Q-013 "açık" iken R13 UE5'i reddediyor. `03` git sapmasını yalnız A07 için yazıyor, `07` R-011 A07 ve A12 diyor.
- **Düzeltme:** 16'yı §16'nın 18 maddesiyle doldur; 00, 02, 03, 05, 06, 07'yi güncelle.

### K3. Karar için en önemli alternatiflerden biri hiç araştırılmamış: 2D / 2,5D / stilize yol
- Araştırmanın ana sonucu "örnek videodaki fotogerçek görünüm 3D ile büyük olasılıkla çıkmaz". Buna karşın stilize/illüstrasyon, 2,5D (katmanlı paralaks), önceden hazırlanmış arka plan (matte/önceden render) yolları karşılaştırılmamış. AGENTS dosyalarında "2.5D" hiç geçmiyor; A11 Q-A11-2 stilize için açıkça "araştırılmadı" diyor.
- Projede 2D oynatma zaten var. Kullanıcıya "çizgi/boyama tarzı kabul eder misiniz?" (özet, soru 4) soruluyor ama bu seçeneğin maliyeti, mevcut projeye uyumu, kalitesi ve lisansı yok. Kullanıcı bilgisiz karar vermek zorunda kalır.
- **Düzeltme:** kısa ek araştırma: tam 3D gerçekçi / 3D stilize (toon, boyama) / 2,5D illüstrasyon / hibrit (Blender'da önceden render arka plan + Unity karakter) karşılaştırma tablosu; sonra soru 4.

---

## B. ÖNEMLİ

### O1. Kapsam eksikleri (protokol §2, §4, §6, §9, §10)
| Eksik / çok zayıf | Kanıt |
|---|---|
| iClone (§10 listesi) | yalnız A10'da tek "genel bilgi" cümlesi; teknoloji kaydı, fiyat, lisans yok |
| FK (§2-A3, §9.4) | ajan dosyalarında geçmiyor |
| Render sonrası işleme / kompozit (§9.7) | DaVinci/Natron/kompozit hiç yok; upscaling ve image reconstruction ince; lens distortion tek anma |
| Pan, tilt, orbit, crane, takip (§9.1) | birkaç anma; "nasıl uygulanır" yok |
| Sektörler (§4): reklam, robotik, simülasyon, bilgisayarlı görü, virtual production, mimari görselleştirme | yok ya da bir iki cümle; A11 "virtual production, NeRF ayrıca aranmadı" (A00 yüzeysel kapattı) |
| Reddit, Stack Overflow, video demonstrasyonu (§6) | SOURCES'ta sıfır kayıt; "resmî + bağımsız test + kullanıcı deneyimi" üçlüsü çoğu araçta yok (ajanlar da söylüyor) |
| §10'un "her teknoloji için" soruları | 01 §6 şablonu render performansı, gerçek benchmark, mevcut projeye uygulama, geliştirici/topluluk aktifliği, sürdürülebilirlik, terk edilirse alternatif alanlarını sistematik istemiyor; `08` yalnız satır numarası dizini (§7.5 tek kayıt yok) |
| Ücretli gerçekçi karakter/mekân paketleri, gerçekçi ejderha | A00, A02, A05, A06 "taranmadı/bulunamadı" |
| Düz yazıdan otomatik sahne kurma | `DURUM.md` "KULLANICININ HEDEFİ"nde var (ayrıca elle ya da yapay zekâya söyleyerek düzeltme, 4K, çocuk karakter); plan §1 ve mimari bunu ele almıyor. 4K ile 1080x1920 hedefi arasındaki fark kullanıcıya açıklanmalı |

§11 donanım listesi büyük ölçüde karşılanmış (A10); eksik olan ölçüm ve güncel TL fiyatı.

**Düzeltme:** her satır için ya kısa takip araştırması ya da `02` ve `16`'da "araştırılmadı: neden, etki, alternatif" kaydı (§20).

### O2. Risk, karar ve veritabanı dosyaları protokol biçimine uymuyor
- `07`: 11 kayıt; ses, karakter, maliyet, geliştirme süresi, proje mimarisi, teknoloji bağımlılığı riskleri yalnız R10'da, kimliksiz; R10 "ana kaydı değiştirmez" diyor. §7.9 sağlanmıyor.
- `04`: 3 kayıt. URP'de kal, UE5'e geçme, mimari, ses, kamera, karakter, varlık önerileri karar kimliği/alternatif/geçerlilik koşuluyla yok ("ÖNERİ" statüsünde yazılabilir) (§7.4).
- `09`: 11 kaynak sınıfsız. `10`, `11` çok ince.

### O3. Plan B ve C'nin toplam maliyeti yok (§12 "hesaplanmalı")
R05 bunu dürüstçe söylüyor, ama kriter sağlanmış sayılamaz. TL fiyatı tek, 4 ay eski, C sınıfı kaynak. RTX 3060 stoğu konusunda da güncellik sorunu var (Ara 2025'te tükeniyor haberi, 2026'da yeniden satışa sunulduğu haber başlıkları; yalnız başlık gördüm). **Düzeltme:** kullanıcıdan satıcı fiyatı istenerek ya da tarihli ABD doları toplamı "gösterge" diye verilerek aralık hesapla.

### O4. Mimari, yol haritası ve "nihai" karar erken
- Protokol Rapor 8'i "araştırma **ve testler** sonucunda" ister; sıfır test var. 14 numaralı dosya üç katmanlı veri modeli, render manifesti, dinamik programlama gibi ayrıntılı tasarım içeriyor; 11 aşamalı yol haritası doğrulanmamış varsayımlara (H1–H3, yüz, iGPU hızı) dayanıyor.
- R13 "geliştirmeye geçilmemeli" diyor: **bu yargıyı onaylıyorum.**
- "URP'de kal" yönünü makul buluyorum (geri dönüşü kolay, HDRP duyurusu doğrulandı). Ama "iyi oyun sinematiği" tavanı ve Aşama 4'ün "büyük fark" beklentisi **ölçülmemiş**; "karar" değil "varsayılan hipotez" diye yazılmalı. Vega'da gölge+HDR+SSAO süresi kabul edilemez çıkarsa plan değişir.
- UE5 reddi "bu bilgisayar" kısıtına dayanıyor; kullanıcı donanım alabilirse hangi koşulda yeniden açılacağı 04'e yazılmalı. K01 UE5 önerisi bağımsız bir prototiple sınanmadı.
- **Düzeltme:** R08/R09/R13 ve 14/15'e "yalnız Aşama 1 (teşhis) bağlayıcı; sonrası T0 sonuçlarına göre yeniden yazılır" notu.

### O5. Kullanıcı özeti: genel olarak sade ve dürüst, ama eksik
Olumlu: test yapılmadığını, fotogerçeklik sınırını, belirsiz fiyatları açık söylüyor.
Eksik/yanıltıcı olabilecekler:
1. Unity koşulu riski yok (K1).
2. Kullanıcının getirdiği araştırma (Unreal 5, MetaHuman, siyah bar) hiç yanıtlanmıyor; neden benimsenmediği 3–4 cümleyle yazılmalı.
3. Araştırmanın **tamamlanmadığı** ve kapsam eksikleri açıkça söylenmiyor ("denetim henüz yapılmadı" deniyor).
4. "Aşağıdakiler güçlü tahminlerdir": bir kısmı güçlü kod okuması, bir kısmı (ışık etkisi, kalite hedefi, ağız modeli) zayıf; ayrım yapılmalı.
5. "Programın temelini değiştirmeye gerek yok" testsiz kesin dil; "şimdilik öneri" denmeli.
6. ElevenLabs ücretsiz sesli videolar için "başlığa elevenlabs.io yaz, para kazanma" uyarısı yok (A12 K1 sonucu; `DURUM.md` kullanıcı kararı).
7. "Terimler": "yakın planı", "kurgu" tamam; "HDRP" parantezi sade kullanıcı için gereksiz ayrıntı sayılabilir.

### O6. Çapraz denetim ve bazı ajanlar kısmi
A05 ve A10 kullanım sınırında kesildi, kapanış özeti yok (`02`). X1 dört, X2 beş raporu tam okumadı; §3'teki "donanım uzmanı render önerisini denetler" türü denetimler rol canlandırmasıyla ve kısmen yapıldı (X1: A10, A06 kalabalığını denetlememiş). Raporlarda kısmen yazılı; `16`'da da kayda girmeli.

---

## C. KÜÇÜK
1. R04 §2 "CC5 yapılamaz (8 GB ayrı kart)" kesin; kaynak kaydı sayfanın CC5'e ait olduğunun kesin olmadığını söylüyor [A02-S037] (X1 D17). Çekince ekle.
2. TikTok: güncel yardım sayfası gerçekçi yapay görüntü **ve ses** için etiketi zorunlu sayıyor; "güncel ayrıntı doğrulanamadı" notu kapatılabilir. https://support.tiktok.com/en/using-tiktok/creating-videos/ai-generated-content
3. Gemini "günde ~10 istek" resmî değil (doğru işaretlenmiş). Resmî sayfa sınırın AI Studio'dan bakılmasını söylüyor; kullanıcıdan ekran görüntüsü istenebilir.
4. Mixamo "ham dosya dağıtılamaz" resmî SSS sayfasında geçmiyor; Adobe topluluk SSS'sine dayanıyor [A03-S016, B]. R02 §4'te sınıf belirtilsin; asıl Adobe koşulları okunmadı.
5. "AO yok" (R01 §2): SSAO bir renderer ayarı ve URP ayarları depoda yok (Q-008). "Kodda açılmıyor; proje ayarı bilinmiyor" denmeli. "3D mekânlar dokusuz" kod mekânı için doğru; katalog eşyalarını kapsamamalı.
6. C-004: resmî Rocketbox deposu 417 animasyon diyor; "326 static" projedeki alt küme olarak ayrılabilir.
7. Ayrı süreç notu: protokolden önce `KareSerisi.cs` değişikliği ve `ornek30.txt` günlükte açıkça yazılı (ihlal sayılmaz, protokol sonradan geldi). A07/A12 salt okunur git komutu iç kural sapması, kayıtlı. Test yapılmaması protokole aykırı değil (§14 test **planı** ister; §19 kodu yasaklar), ama "TEST ET → KARAR VER" sırası gereği hiçbir karar kesin sayılamaz.

---

## D. Örneklem doğrulamaları

### D.1 Kaynak iddiaları
| # | İddia | Kontrol | Sonuç |
|---|---|---|---|
| 1 | HDRP 7.0'da deprecated, sonra kaldırılacak, destek ≥2029 [A04-S002] | web | destekliyor |
| 2 | URP hacimsel sis ve RT ışık 7.x/2027 | web | destekliyor |
| 3 | Rocketbox MIT, 115, 15 viseme, ARKit [A02-S020] | web | destekliyor |
| 4 | Netflix TR ≤2 satır, 17 karakter/sn [A00-S001] | web | destekliyor |
| 5 | MetaHuman her motorda, 1 M$ altı [A02-S001] | web | destekliyor; kütüphane dağıtımı belirsiz (doğru işaretli) |
| 6 | DLSS 5 yalnız RTX 50, Unity anılmıyor [A11-S001] | web | destekliyor |
| 7 | TikTok etiket [A12-S022] | web (güncel sayfa) | destekliyor, daha güçlü |
| 8 | Mixamo telifsiz / ham dağıtım yasak | web | ilk kısım resmî; yasak yalnız forum |
| 9 | Unity §2.5 Build Server, Personal kullanamaz [A12-S005] | web | metin doğru; **çıkarım eksik (K1)** |
| 10 | Vega HIP desteği kaldırıldı [A04-S030] | kayıt | destekliyor; tümleşik ayrıca anılmıyor |
| 11 | TL kart fiyatları [A10-S017] | kayıt | destekliyor; zayıf (C) |
| 12 | Vega 7 ↔ 3060 puanı [A10-S028] | kayıt | destekliyor; çekince yazılı |
| 13 | ASL 2,31 / ≈4 sn [A08-S001, S002] | kayıt | destekliyor; ilki özet |
| 14 | Cliffhanger devam niyetini artırmadı [A08-S008] | kayıt | destekliyor; farklı bağlam |
| 15 | 60 cm yüz çekimi olumsuz [A01-S033] | kayıt | destekliyor; durağan fotoğraf |
| 16 | Yakın plan yalnız üzgün ifadede [A01-S034] | kayıt | destekliyor |
| 17 | Alt %34 güvenli alan [A01-S010] | kayıt | destekliyor; yalnız reklam |
| 18 | Megascans ücretsiz [A05-S006, A12-S017] | kayıt | destekliyor (B) |
| 19 | İngilizcede MFA en iyi [A07-S009] | kayıt | destekliyor; ön baskı |
| 20 | Splat eklentisi DX12/Vulkan [A11-S013] | kayıt | destekliyor; ayna, C |

Kayıt kontrolü = kaynak kaydının ifadesiyle uyum; sayfa yeniden açılmadı. Uydurma kaynak bulmadım.

### D.2 Kod iddiaları (statik okuma, `Assets/`)
| İddia | Sonuç |
|---|---|
| 3D kamera `allowHDR=false`, ışıklar gölgesiz, `farClipPlane=60` (`Sahne3D.cs:98-112`) | doğru |
| `ProcAudio.Clean` baş/son sessizliği kırpıyor, RMS 0,12'ye getiriyor | doğru |
| Katalogda 36 farklı `Mixamo_*` | doğru (ad sayımı) |
| Mixamo animasyonları ortak klip listesine ekleniyor (`Karakter3DKurulum.OrtakKlipler`) | doğru |
| `dur`→`idle`, `Find` ilk uyanı alıyor; `RbListe` sırası | mantık doğru; hangi klibin çaldığı diske bağlı, test gerek |
| Ağız: `jawopen` öncelikli, `aa` eleniyor (`YuzCanlilik.cs`) | doğru |
| Hava efekti kendi mesh'i (`HavaDurumu3D.cs`) | doğru |
| GPU okuma hatasında kare atlanıyor (`FfmpegPipeRecorder.cs:266`) | doğru |
| `targetFrameRate=30` ve `captureDeltaTime` | satırlar var; süreye etkisi ölçülmedi |
| Mekânlar ilkel şekillerden, gölge kapalı (`Mekan3D.cs`, `Mekan3D.Masal.cs`) | doğru |
| `AtmosferVolume` bileşen listesinde Tonemapping yok | doğru |
| Yalnız `_static` animasyon klasörü (`NesneKur.cs:742`) | doğru |
| EXE'de DoF/Panini atılıyor (`DURUM.md:83`) | kayıt var; ben doğrulamadım |

---

## E. §16 denetim maddeleri (benim yargım)
| Madde | Yargı |
|---|---|
| Kapsam eksik mi | evet (K3, O1) |
| Sorular cevaplandı mı | kısmen (Q-001…Q-017'nin çoğu açık) |
| Ajan görevleri | kısmen (O6) |
| Kaynak doğruluğu / iddia desteği | örneklemde büyük ölçüde iyi; K1 ve C1, C4 istisna |
| Güncellik | kısmen (Unity ToS, GPU stoğu/fiyatı) |
| Lisans | **eksik** (Unity ana ToS; Adobe asıl koşulları, MetaHuman EULA okunmamış) |
| Teknik uyumluluk | kısmen (hiç denenmedi; C-028) |
| Donanım / maliyet | kısmen (ölçüm ve TL toplamı yok) |
| Çelişkiler | kayıtlı, çoğu açık; yeni: Unity koşulu |
| Eksik testler | plan iyi; Unity koşulu köprü testlerini etkiliyor; stilize görünüm testi yok |
| Gereksiz teknolojiler | belirgin sorun yok; öneriler tutumlu |
| Alternatifler | 2D/2,5D/stilize eksik |
| Yanlış varsayımlar | "köprü koşul dışı"; "URP iyileştirmesi bu PC'de yeterli olur" (sınanmadı) |
| Gelecek teknolojiler | yeterli ama sistematik değil (SIGGRAPH Asia, NeRF) |
| Hafıza güncel mi / tutarlı mı | hayır (K2) |

## F. §18 tamamlanma ölçütleri
| # | Ölçüt | Durum | Kanıt |
|---|---|---|---|
| 1 | Bütün ana alanlar incelendi | **kısmen** | O1, K3 |
| 2 | Ajan görevleri tamamlandı ya da kısıt açıklandı | **kısmen** | 13 rapor var; A05/A10 kesildi, kısıt `02`'de |
| 3 | Uluslararası araştırma | **sağlandı** | ~534 kaynak, çoğu İngilizce; derinlik sınırlı |
| 4 | Teknolojiler karşılaştırıldı | **kısmen** | R03; §10 soruları eksik, iClone yok |
| 5 | Alternatifler incelendi | **kısmen** | UE5/Blender/HDRP var; 2D/2,5D/stilize yok |
| 6 | Donanım analizi | **kısmen** | R04; ölçüm yok, güç kaynağı bilinmiyor |
| 7 | Maliyet analizi | **kısmen** | B ve C toplamı yok |
| 8 | Lisans analizi | **kısmen** | A12 geniş; Unity ana ToS atlanmış |
| 9 | Teknik uyumluluk | **kısmen** | X1 §2; denemesiz |
| 10 | Kaynaklar kaydedildi | **sağlandı** | SOURCES; 11 kaynak sınıfsız |
| 11 | Çelişkiler değerlendirildi | **kısmen** | C-001…C-029; çoğu açık, K1 eksik |
| 12 | Riskler kaydedildi | **kısmen** | O2 |
| 13 | Test planı | **sağlandı** | 12; beş sahne ve §14 alanları tam |
| 14 | Nihai mimari önerisi | **sağlandı (taslak)** | 14, R08; testsiz |
| 15 | Yol haritası | **sağlandı (taslak)** | 15, R09 |
| 16 | Kalıcı hafıza oluşturuldu | **sağlandı** | klasör yapısı tam |
| 17 | Dosyalar güncellendi | **sağlanmadı** | K2 |
| 18 | Araştırma denetimi | **sağlanmadı** | 16 boş |
| 19 | Bağımsız denetçi raporu | **sağlandı** | bu dosya (kısmi okuma sınırıyla) |
| 20 | Geliştirmeye geçiş gerekçesi | **sağlandı** | R13 §6 |

## Net hüküm
**Araştırma tamamlandı sayılamaz.** İki ölçüt sağlanmamış (17, 18), büyük bölümü kısmi. Temel iş (kaynak kaydı, test planı, dürüst "test yok" dili, HDRP düzeltmesi) sağlam; yön önerisi ("URP'de kal, önce kusurları teşhis et, satın alma yapma") taslak olarak makul. **Geliştirmeye geçilmemeli.**

Kapatmak için sırayla: (1) K1 Unity koşulu: kayıt, kullanıcıya bildirim, netleştirme; (2) §16 öz denetim ve dosya güncellemesi (K2, O2); (3) 2D/2,5D/stilize karşılaştırması (K3); (4) O1 eksikleri için araştırma ya da açık kısıt kaydı; (5) kullanıcı özetinin düzeltilmesi (O5); (6) 14/15/R13'e "testsiz taslak" bağlayıcılık notu.
