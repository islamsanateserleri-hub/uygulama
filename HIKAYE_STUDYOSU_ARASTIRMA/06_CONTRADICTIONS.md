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
