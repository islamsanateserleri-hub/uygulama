# R07 — Test planı (okunaklı özet)

Tarih: 2026-11-27. **Hiçbir test yapılmadı.** Ayrıntının ve ölçütlerin asıl yeri: `../12_TEST_PLAN.md`. Bu rapor onunla çelişirse 12 geçerlidir.

> **Unity uyarısı (K-004, Q-016):** köprüden komut gönderilmez; Unity işlemlerini kullanıcı elle başlatır ya da onay verirse Unity CLI kurulur. **Bütçe (K-020):** kart ya da para isteyen testler kapsam dışı.

## 1. Sıra
1. **Ön teşhis (T0-1…T0-14):** kod okumasıyla bulunan kusurlar gerçek mi? Çoğu ucuz ve hızlı.
2. **Beş test sahnesi (TS-1…TS-5):** önce mevcut hâl (A), sonra aday iyileştirme (B).
3. **İkincil testler**, gerekirse.
4. Para/kart isteyenler: ~~en sona ve ayrı onayla~~ kapsam dışı (K-020).

## 2. İlk yapılacak teşhisler
| Soru | Test |
|---|---|
| Susan karakter neden el kol oynatıyor? | T0-1 |
| Yürüme neden görünmüyor? | T0-2 |
| Ağızda yalnız çene mi oynuyor; diş görünüyor mu? | T0-3 |
| EXE'nin içinde lisansı sorunlu dosya var mı? | T0-4 |
| Reddedilen filmde hangi satırların sesi yanlış? | T0-5 |
| Bugün 30 sn video kaç dakikada kaydediliyor? | T0-6 |
| 45 GB disk yeter mi? | T0-8 |

## 3. Beş sahne
| Kod | Sahne | Asıl soru | Ana ölçüt | Gerekenler |
|---|---|---|---|---|
| TS-1 | Yüz yakın planı | Yüz duyguyu anlatıyor mu? | sesi kapalı izleyenler duyguyu bilsin | kod onayı |
| TS-2 | İki kişi konuşuyor | Ses-altyazı ve kurgu doğru mu? | uyumsuz satır 0; kullanıcı yeni sürümü seçsin | kod + hizalayıcı indirme |
| TS-3 | Yürüme, koşma, takip | Kaymadan yürüyor mu? | ayak kayması ölçümü + video izleme | kod; belki animasyon indirme |
| TS-4 | Atmosfer, ışık, efekt | Görüntü "düz" olmaktan çıkıyor mu; bilgisayar kaldırıyor mu? | her adımda kare + süre | kod + CC0 indirme |
| TS-5 | Kalabalık, ejderha, çevre | Sınır nerede? | çökme yok, süre kabul edilebilir | kod; ejderha modeli |

Her sahne için teknoloji, varlık, yöntem, render ayarı, beklenen sonuç, başarı ölçütü, performans ölçümü, maliyet (hedef 0 TL) ve tekrar edilebilirlik 12 numaralı dosyada yazılıdır.

**Eklenen dört test (12 §3.1; yapılmadı):** G-1 görünüm karşılaştırması (bugünkü taban + iyileştirilmiş gerçekçi + stilize) · AS-1 asistan sınaması (aynı 10 kısa hikâye) · 4K-1 dikey 4K kodlayıcı kabulü · DY-1 düz yazıdan taslak doğruluğu.

## 4. Ölçüm kuralları
Aynı hikâye ve ayar; 3 ölçümün ortancası; kayıt süresi, kare süresi, tepe bellek, atlanan kare, disk. Aynı hikâye bugün iki kayıtta aynı çıkmadığı için (tohumsuz rastgelelik) birebir kare karşılaştırması henüz güvenilir değil.

## 5. Onaylar
- Kod değişikliği gerektiren her test **ayrı kopya projede** ve kullanıcının onayıyla.
- İndirme/kurulum, hesap, para: tek tek sorulur.
- Ses üretimi günlük ücretsiz kotayı harcar; aynı ses yeniden üretilmez.
- "Hazır" demek için sayılar yetmez; kullanıcı sesli videoyu izler.

## 6. Planın bilinen eksikleri
Kullanıcının kabul edeceği en uzun kayıt süresi bilinmiyor; eşiklerin çoğu başlangıç tahmini; izleyici sayısı küçük olacağı için sonuçlar istatistiksel kanıt sayılmaz.
