# 06 ÇELİŞKİLER (çözülmeden kesin karar yok)
| ID | Çelişki | Kaynaklar | Durum |
|---|---|---|---|
| C-001 | 26 Kas raporu: "Cinemachine gibi paketler ekleme için onay ister". Cihazdaki manifest.json: com.unity.cinemachine 6.6.0 zaten kurulu. | Docs/VideoKalitesi.md §5 · manifest.json | ÇÖZÜLDÜ (olgu: kurulu). Kodda kullanılıp kullanılmadığı A09 inceleyecek. |
| C-002 | VideoKalitesi 1. rapor "bu bilgisayarda mümkün değil" · 2. rapor "ekran kartı + disk yeter" · ayrıca "birebir aynı görünüm garanti değil". | Docs/VideoKalitesi.md | AÇIK: araştırma yeniden değerlendirecek (A09, A10). |
| C-003 | 26 Kas raporu ve sohbette "HDRP'ye geç (kart alınca)" önerildi. Unity resmî: HDRP'ye yeni özellik yok; 29 Eyl 2026 duyurusu HDRP'yi Unity 7.0'da "deprecated" sayıyor, destek 2029 sonu (uzatılmış 2030), sonra kaldırılacak. | Docs/VideoKalitesi.md §3 · A04-S001..S003 · A09-S001 | **ÇÖZÜLDÜ:** resmî duyuru geçerli; HDRP önerisi GERİ ÇEKİLDİ. 27 Kas'ta ayrı bir okuyucuyla iki sayfa yeniden doğrulandı. |
| C-004 | Rocketbox animasyon sayısı 417 (README) / 471 (üçüncü taraf) / 326 static; "klipler yerinde" ile "xy ilerleyen yürüme var". Kod yalnız `..._static` klasörünü alıyor (NesneKur.cs:742, doğrulandı). | A03 | AÇIK: KutuphaneHam dosyaları listelenecek (test). |
| C-005 | 26 Kas: "yürüme/koşma Mixamo'dan" ↔ A03: önce eldeki Rocketbox + UAL (hesap gerekmez). | VideoKalitesi · A03 | AÇIK (test T4/T5). |
| C-006 | Dikey kaynaklar "geniş plandan kaçın" (C/D) ↔ hedef örnek video çok geniş şato/ejderha planı kullanıyor. | A01 Ç1 | Geçici: yakın/orta ağırlık, geniş plan seyrek; A08 değerlendirecek. |
| C-007 | Dikey güvenli alan: reklam ölçümü alt %34 ↔ uygulama %20. | A01 Ç2 | AÇIK: gerçek telefonda kontrol; A07/A08. |
| C-008 | Asama16: "ticari kullanıma açık Türkçe ses olmadan yerel ses kullanma" ↔ A07: Apache/MIT adaylar var (Chatterbox, VoxCPM2, EMA, FreyaTTS) ama Türkçe kalite ve eğitim verisi hakkı doğrulanmadı. | Asama16 · A07 | AÇIK: A12 denetleyecek. |
| C-009 | Reallusion "Extended License" şartı: içerik sayfası var diyor, 2025 duyurusu kaldırıldı diyor, EULA'da yok. | A02 Ç2 | AÇIK: A12; satın almadan yazılı teyit. |
| C-010 | MetaHuman'ın Unity'de lisansı: bir blog "yol yok" (D) ↔ Epic 2025 "any engine". | A02 Ç1 | ÇÖZÜLDÜ: resmî kaynak; ancak uygulama İÇİNDE karakter kütüphanesi olarak dağıtma izni doğrulanamadı (A12). |
| C-011 | Kullanıcı araştırması: 2,39:1 sinema barları ↔ hedef 9:16 dikey 1080x1920. | K01 · protokol §1 | ÇÖZÜLDÜ (protokol hedefi geçerli: bar yok). |
| C-012 | ASL ölçümü 2,31 sn ↔ ≈4 sn (farklı örneklem). | A08 Ç1 | Aralık olarak kullanılacak; hedef medyan 2,5–3 sn (testle). |
| C-013 | Cliffhanger sektörce "vazgeçilmez" ↔ deneyde devam niyeti artmadı. | A08 Ç2 | Kullanılır ama tek tutma aracı sayılmaz. |
| C-014 | Kart: "en az RTX 3060 12GB" (eski belge) ↔ A11: DLSS 5 yalnız RTX 50; A10: ≥12, tercihen 16 GB. | VideoKalitesi · A10 · A11 | AÇIK: satın alma kararı yok; çapraz denetime. |
| C-015 | İkinci el RTX 3060 TL fiyatı: 15.000–37.850 ↔ 6.200–7.400. | A10 Ç1 | ÇÖZÜLEMEDİ: TL fiyatı güvenilir değil. |
| C-016 | Asset Store: sözleşme ↔ SSS (UGC aracında yayıncı izni). | A12, A06 | Temkinli olan uygulanır: dağıtılacak uygulamada Asset Store varlığı yok. |
| C-017 | Piper motoru "MIT" (Asama16) ↔ güncel depo GPLv3. | Asama16 · A12 | ÇÖZÜLDÜ: Asama16 güncel değil. |
| C-018 | Altyazı: karaoke (mevcut) ↔ hedef örnek sade beyaz; karaoke üstünlüğüne kanıt bulunamadı; Netflix TR: ≤2 satır, 17 karakter/sn. | VideoKalitesi · A00 | AÇIK: kullanıcı karşılaştırma testi. |

## Çapraz denetim sonuçları (27 Kas; ayrıntı AGENTS/X1, X2)
| ID | Çelişki / hata | Çözüm |
|---|---|---|
| C-019 | A09 hava efektine "Particle System" diyor; `HavaDurumu3D.cs` kendi mesh'ini çiziyor. | ÇÖZÜLDÜ (kod): A06 doğru. |
| C-020 | A08 "ses sessizliği boşluğa ekleniyor"; `ProcAudio.Clean` zaten kırpıyor. | ÇÖZÜLDÜ (kod): A08 maddesi geçersiz. |
| C-021 | A12 Mixamo karakter sayısı 35 ↔ 36; Mixamo animasyonları tüm insanlara da ekleniyor (klasör doluysa). | ÇÖZÜLDÜ (kod okuması): 36; dağıtım riski animasyonları da kapsar. |
| C-022 | A01 ECU ~0,7 m ↔ kendi "1 m altı yok" kuralı. | AÇIK: testte mercek/mesafe ayarlanacak (kural mercekle birlikte yazılmalı). |
| C-023 | DoF'a dayanan plan dili ↔ EXE'de DoF çalışmayabilir. | AÇIK: Q-010 testi. |
| C-024 | HDR açma ↔ saydam bindirme (alfa 0) 64-bit/Alpha Processing isteyebilir. | Test iki durumda yapılacak (3D tam kare; 2D üstünde 3D). |
| C-025 | Parçalı render ↔ kesintisiz ses miksi. | ÇÖZÜLEMEDİ: mimaride ses ayrı tek iz olarak sonradan bindirme seçeneği denenecek. |
| C-026 | A11 "kareleri diske yaz" ↔ 45 GB disk. | Ortak disk bütçesi çıkarılacak (A10 planları + test). |
| C-027 | Plan/çekim/kurgu için ayrı ayrı şemalar (A01, A07, A08, A09). | Öneri: tek "çekim manifesti", üç katman (X2 §2.1); denenmedi. |
| C-028 | Araç sürümleri ↔ Unity 6000.6 + DX11 (karakter araçları, splat eklentisi). | ÇÖZÜLEMEDİ: denemeyle; DX11/DX12 testi plana. |
| C-029 | Dikey güvenli alan: altyazı + yüz bölgesi birlikte sığmayabilir. | AÇIK: telefon testi. |
