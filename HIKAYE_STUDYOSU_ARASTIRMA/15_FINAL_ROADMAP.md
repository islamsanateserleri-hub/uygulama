# 15 YOL HARİTASI (ÖNERİ)

Tarih: 2026-11-27. **Durum: ÖNERİ. Test edilmedi. Hiçbir aşama kendiliğinden başlamaz; geliştirmeye geçiş kullanıcının ayrı kararıdır.**
Süre tahmini verilmiyor: ajanlardaki süreler ölçümsüz tahmindi. Her aşamanın sonunda kullanıcı sesli ve hareketli örneği izleyip onaylar.

## Aşama listesi

| Aşama | Amaç | İşler | Ön koşul | Çıkış ölçütü | Para / indirme |
|---|---|---|---|---|---|
| **0. Araştırmayı kapatmak** | protokol §16–18 | öz denetim, bağımsız denetçi, eksiklere dönüş, kullanıcı soruları | — | 16_RESEARCH_AUDIT dolu | 0 |
| **1. Teşhis ve ölçüm** | varsayımları sınamak | 12_TEST_PLAN T0-1…T0-14; disk envanteri | kullanıcı onayı | taban ölçümleri 13'te | 0 |
| **2. Hareket ve ses düzeltmeleri** (reddin ana nedenleri) | doğru davranış | durgun duruş, yürüme ezilmesi, yürüme klipleri; ses kapısı, satır başı ses, geçiş planı, hizalama, gerçek altyazı zamanı; kare atlama ve takılma bekçisi | Aşama 1 | TS-2 ve TS-3 ölçütleri; 30 sn onay videosu | hizalayıcı ve belki animasyon indirme |
| **3. Yüz** | konuşan ve duygulu yüz | viseme ve ARKit eşleme, duygu tablosu, karakter kartı, tohumlu rastgelelik | 1 | TS-1 | 0 |
| **4. Görüntü temelleri** | "düz oyuncak" görünümünden çıkış | proje ayarları, gölge, HDR+tonemapping (alfa testiyle), SSAO, plana göre ışık, EXE'de efekt silinmesi, kayıt profili | 1 | TS-4'ün ışık adımları; süre kabulü | 0 |
| **5. Kamera ve kurgu 1** | gerçek yakın planlar ve ritim | göz hizası kadraj, açılış, değişken boşluk; **ses izlerinin ayrılması** (J-L için); çekim listesi mevcut davranışla | 2; yakın planlar için 3; sığ odak için 4 | TS-2 B; kullanıcı kör seçimi | 0 |
| **6. Mekân pilotu ve efekt** | tek mekân (şato) örnek kalite | pahlı kit, CC0 PBR, 4 derinlik katmanı, uzak katman; 3D ateş/duman, göz parlaması, güç patlaması tarifi | 4 | TS-4 tam; stil kararı | CC0 indirme |
| **7. Büyük sahneler** | kalabalık ve yaratık | iskeletli ejderha, 3 halkalı kalabalık, uzak düzlem | 4, 6 | TS-5 | olası ücretli model |
| **8. Otomatik yönetmen** | elle emeği azaltmak | vuruş analizi, puanlama, dinamik programlama, storyboard ve kilit | 2–5 | denetçi hatasız; kullanıcı tercih eder | 0 |
| **9. Dayanıklı üretim** | uzun video, toplu üretim | parçalı ve kurtarılabilir kayıt, kuyruk, tek kayıt yolu, belleği boşaltan yükleme | 5 | çökme sonrası devam testi | 0 |
| **10. Dağıtım hazırlığı** (yalnız istenirse) | lisans temizliği | Mixamo çıkarma + yerine CC0/MIT hareket listesi, bildirim ekranı, FFmpeg yükümlülükleri, manifest, hukukçu | karar | A12 T1–T4 | hukuk ücreti olabilir |
| **11. Donanım değerlendirmesi** (her an, ölçüme bağlı) | gerekirse yatırım | disk → güç kaynağı/kasa kontrolü → kart | 1 ve 4 ölçümleri | süre sınırı aşılıyorsa | para |

## Sıra bağımlılıkları (atlanmamalı)
- Hareket düzelmeden tepki planı eklenmez (dinleyen konuşuyor gibi görünür) (X2 §0.5, S11).
- Yüz testi geçmeden aşırı yakın plan kapalı (X2 G3, X1 S8).
- Alan derinliği EXE'de çalışmadan sığ odaklı plan dili yok (X2 G4).
- Karakter bellek yapısı çözülmeden ağır yeni karakter yok (X1 U5).
- Tek veri modeli netleşmeden ayrı ayrı şema yazılmaz (C-027).

## Takip (her aşamadan bağımsız)
Ocak 2027: Unity 7 önizleme ve 6.7 LTS durumu; Nisan: GTC sonrası; Eylül: SIGGRAPH sonrası; 6 ayda bir lisans gözden geçirme (A11 takip düzeni, A12-R10). Zamanlanmış görev kurulmadı.

## Her aşamada sabit kural
Önce ölç → küçük değiştir → aynı sahneyle karşılaştır → sesli izle → onay → kaydet. Başarısız aşama geri alınır.
