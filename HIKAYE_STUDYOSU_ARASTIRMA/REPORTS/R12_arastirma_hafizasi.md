# R12 — Araştırma hafızasının kılavuzu

Tarih: 2026-11-27. Bütün yollar `HIKAYE_STUDYOSU_ARASTIRMA/` içindedir. Bu rapor hafızanın kendisi değil, **nerede ne olduğunu** ve nasıl devam edileceğini anlatır.

## 1. Yeni oturum nereden başlar
1. `00_PROTOKOL.txt` (kurallar; özellikle §19: araştırma bitmeden kod/geçiş/satın alma yok).
2. `00_MASTER_INDEX.md` → `02_RESEARCH_PROGRESS.md` → `05_OPEN_QUESTIONS.md` → `06_CONTRADICTIONS.md`.
3. Eksik olan ilk adımdan devam. Aynı araştırmayı tekrar yapma; önce ajan dosyasında ara.

Not: 00 ve 02 dosyaları rapor yazımından önceki durumu gösteriyor olabilir; güncellenmeleri ana araştırmacının işidir.

## 2. Dosya haritası
| Dosya | İçerik |
|---|---|
| 01_MASTER_RESEARCH_PLAN | kapsam, öncelik, ajan kuralları |
| 02_RESEARCH_PROGRESS / 03_RESEARCH_LOG | ilerleme ve günlük |
| 04_DECISION_LOG | kararlar (K-001…K-003) |
| 05_OPEN_QUESTIONS | Q-001…Q-017 |
| 06_CONTRADICTIONS | C-001…C-029 |
| 07_RISK_REGISTER | R-001…R-011 |
| 08 / 09 / 10 / 11 | teknoloji, kaynak, performans, maliyet dizinleri |
| 12_TEST_PLAN / 13_TEST_RESULTS | plan / yalnız gerçekten yapılan testler |
| 14 / 15 | mimari ve yol haritası (ÖNERİ) |
| 16_RESEARCH_AUDIT | öz denetim ve bağımsız denetim (rapor yazımı sırasında henüz boş) |
| AGENTS/ | A00–A12 uzman raporları, X1–X2 çapraz denetim |
| SOURCES/ | ajan başına kaynak kaydı |
| TOPICS/ | mevcut proje; kullanıcının kendi araştırması (K01, doğrulanmamış) |
| EXPERIMENTS/ | D001 sessiz önizleme gözlemi |
| REPORTS/ | R01–R13 ve kullanıcı özeti |

Kaynak numarası okuma: `A07-S008` = `SOURCES/A07_kaynaklar.md` içindeki S008. Güven sınıfı A resmî/akademik, B güvenilir test/uzman, C forum/geliştirici, D doğrulanmamış (D tek başına karar dayanağı olamaz).

## 3. Öğrenilen en önemli bilgiler
1. HDRP Unity'de kaldırılma yolunda; URP'de kalınmalı (C-003, K-003).
2. Görüntü, hareket, ses ve yüz kusurlarının büyük bölümü için **kodda** olası nedenler bulundu; hiçbiri testle doğrulanmadı.
3. Yeni donanım ya da paket olmadan yapılabilecek çok iş var (R06 §1).
4. Örnek videonun fotogerçek görünümü 3D ile büyük olasılıkla otomatik/ucuz çıkmaz (ÇIKARIM).
5. Uygulama bu hâliyle dağıtılmamalı (lisans).
6. Bu bilgisayarın en somut kısıtı disk; en büyük bilinmeyeni güç kaynağı/kasa.

## 4. Kararlar
Geliştirme kararı **yoktur**. Kayıtlı: K-001 (araştırma bitene kadar kod yok), K-002 (çalışma düzeni), K-003 (HDRP önerisi geri çekildi). 14 ve 15 numaralı dosyalar öneridir, karar değildir.

## 5. Yanlış çıkan iddialar (tekrar kullanma)
| İddia | Doğrusu |
|---|---|
| A09: hava efekti "Particle System" | kendi mesh çizimi (C-019) |
| A08: ses dosyasının baş/son sessizliği boşluğa ekleniyor; "sessizliği kırp" | kod zaten kırpıyor (C-020) |
| A12: katalogda 35 Mixamo karakteri | 36; Mixamo animasyonları da insanlara ekleniyor (C-021) |
| Eski belge: "HDRP'ye geç" | geri çekildi (C-003) |
| Asama16: "Piper motoru MIT" | güncel motor GPLv3 (C-017) |
| A10: "kart tek başına kayıt süresini kısaltmaz" | fazla genel; yalnız kare süresi 33 ms altındayken (X1 §3) |
| A03: ".gitignore Mixamo'yu koruyor" | kurulum paketini korumaz (A12) |
| A05: "Asset Store maddesi bu ürünü kısıtlar" | sözleşme para kazanmayı kısıtlıyor; daha geniş yasak SSS'de (A12) |
| A09/A02 sonrası karakter "daha az riskli CC5" | yalnız teknik; lisansça riskli (X1 §2.3) |

## 6. Açık çelişkiler ve sorular
Açık/çözülemeyen çelişkiler: C-002, C-004–C-009, C-012–C-016, C-018, C-022–C-029 (güncel durum 06'da). Önemli sorular: Q-001 (güç kaynağı), Q-002 (ticari/dağıtım), Q-003 (iGPU hızı), Q-005/Q-006 (hareket ve yüz testi), Q-007 (hizalama), Q-008 (proje ayarları), Q-010 (EXE shader), Q-011 (MetaHuman dağıtımı), Q-013 (UE5), Q-015 (yapay zekâ sınırı), Q-016 (Unity koşulları).

## 7. Araştırma durumu (rapor yazımı anı)
- Yapıldı: 12 uzman + takip + 2 çapraz denetim; 534 kaynak; test planı; 13 rapor; mimari ve yol haritası önerisi.
- **Yapılmadı:** hiçbir deneysel test; öz denetim (§16); bağımsız denetçi (§17); eksiklere dönüş.
- Çapraz denetimin kapsamı sınırlı: X1, A03/A07/A08/A12'yi yalnız taradı; X2, A04/A05/A06/A10/A11'i tam okumadı.
- Protokole göre araştırma henüz **TAMAMLANMADI** (§18).

## 8. Kalıcı öğrenme kuralı (protokol §7.10)
Yeni bilgi: doğrula → kaynağı kaydet → ilgili dosyaya yaz → index ve günlüğü güncelle → gerekirse karar/risk kaydı. Eski karar silinmez; yenisi yazılır. Yapılmayan test yapılmış gibi yazılmaz. Önemli bilgi yalnız sohbette bırakılmaz.

## 9. Bilinen düzen eksikleri
- 08, 09, 10, 11 dosyaları dizin düzeyinde; protokol §7.5/§7.6 alanlarının tamamı ajan dosyalarında dağınık.
- Karar kaydında yeni teknik karar yok (bilerek).
- Takip dosyası (`TOPICS/TAKIP_teknoloji.md`) açılmadı.
