# DEN2 — İkinci bağımsız denetim (protokol §16–18)

Tarih: 2026-11-27. Araştırmayı yürütmedim. Kod, git, Unity, remote-devices kullanmadım; test yapmadım. Yalnız bu dosyayı yazdım.
**Okuma kapsamı:** 00_PROTOKOL.txt, DEN, D1, D2, D3, 00–16, REPORTS (00, R01–R13), D1/D3 kaynak kayıtları ve D2'den seçili satırlar. A00–A12, X1, X2 raporları yeniden okunmadı. Web: 10 arama/okuma; sayfalar özetleyiciyle okundu. Hukuki görüş değildir.

## 0. Karar
**Araştırma bugün "tamamlandı" olarak işaretlenemez.** Ana sebep yeni araştırma eksiği değil, **birleştirme eksiği**: kullanıcı yanıtları (K-017..K-020) ve D2/D3, kullanıcı özetine, mimariye, yol haritasına, maliyet/donanım raporlarına ve karar raporuna işlenmemiş. Kalan engellerin çoğu masa başında, yeni web araştırması olmadan kapatılabilir. Testler §18'in şartı değildir (test **planı** şarttır); testlerin yokluğu tek başına araştırmayı açık tutmaz, ama mimari ve karar "testsiz taslak" kalır. **Geliştirmeye geçilmemeli.**

Tür kısaltması: **M** = masa başında kapanır · **T** = test gerekir · **K** = kullanıcı yanıtı/onayı gerekir.

## 1. İlk denetimin bulguları kapandı mı?
| DEN | Durum | Açık kalan |
|---|---|---|
| K1 Unity koşulu | **Büyük ölçüde kapandı** (C-030, K-004, R-010, Q-016, kullanıcı özeti) | B3 |
| K2 sıra ve hafıza güncelliği | **Kısmen**; 16 yazıldı, sonra dosyalar yeniden bayatladı | B1, B4, B5 |
| K3 2D/2,5D/stilize | **Masa başında kapandı** (D1 §2); K-019 ile iki 3D görünüm seçildi | deneme (T) |
| O1 kapsam | **Kısmen**; D1/D2 çoğunu ele aldı | bulgular mimariye girmedi (B2); Reddit, reklam, mimari görselleştirme açık kısıt olarak kayıtlı |
| O2 risk/karar/veritabanı | Risk ve karar kayıtları eklendi | O3, O4 |
| O3 toplam maliyet | ABD göstergesi eklendi | K-020 sonrası bayat (B1) |
| O4 erken mimari | Kapandı ("testsiz taslak") | — |
| O5 kullanıcı özeti | O günkü maddeler kapandı | yeni yanıtlar yok (B1) |
| O6 kısmi ajanlar | 16'da kayıtlı | — |
| C1–C6 | Örneklemde işlenmiş (R04, R02, R01, C-004) | — |

## 2. Bulgular

### B1. KRİTİK — K-017..K-020 yalnız 03, 04 ve D3'te (M)
14, 15 ve REPORTS dosyalarında K-017..K-020 hiç geçmiyor; 14, 15 ve REPORTS'ta "asistan" sözcüğü yok. D3 "birleştirme ana araştırmacıda" diyor; yapılmamış.

| Dosya | Sorun | Somut düzeltme |
|---|---|---|
| REPORTS/00 | Görünüm (soru 4) ve bütçe (soru 5) yeniden soruluyor; asistan yok; kart alımlı fiyatlar duruyor | "Verdiğiniz yanıtlar" bölümü aç (dört yanıt, kendi sözleri). Soru 4'ü ve 5'in para kısmını kaldır (bekleme süresi kalsın). Para: "yalnız ek disk"; kart satırlarını "kapsam dışı" yap. D3 §7'nin üç sorusunu ekle (içeride/dışarıda; metnin Google'a gidip insanlarca okunabilmesi; 18+ ve kullanıcı kitlesi). Soru 2'yi daralt: asistan istendi; kalan belirsizlik ses/hizalama/görüntü iyileştirme. D3 §4–5'i sade dille özetle (Claude aboneliği uygulamanın içine konamaz; programdaki Gemini düğmeleri zaten var) |
| 14 | Asistan bileşeni yok | "Yapay zekâ asistanı" alt bölümü: yalnız taslak metin üretir; asıl hikâyenin üstüne kendiliğinden yazmaz; doğrulayıcı en çok 2–3 tur; kullanıcı farkı onaylar; Unity'de eylem yapmaz (durum 2'den 3'e kaymasın); sağlayıcı arayüzü (Gemini / yerel / dışarıdan); gizlilik ve anahtar saklama; içeride/dışarıda açık soru. İki görünüm profilini (gerçekçi, stilize) yaz |
| 15, R09 | Asistan aşaması ve görünüm karşılaştırması yok; Aşama 11 kart içeriyor | Asistan aşaması ekle (ön koşul, ölçüt: doğrulayıcı hatası, doğru konuşan/mekân oranı, Türkçe beğenisi). K-019 denemesini Aşama 4'e çıkış ölçütü olarak yaz. Aşama 11'i "yalnız disk; kart kapsam dışı" yap |
| R13, R08 | "En yüksek kalite / en düşük maliyet" yanıtı yeni bütçe ve asistanı bilmiyor | Öneriye "bütçe: yalnız ek disk; ekran kartı yok; iki görünüm denenecek; asistan olacak (yeri belirsiz)" ekle; "kart ancak ölçümden sonra" ifadesini düzelt; Aşama 6–7'nin tümleşik kartta riskli olduğunu kalite sınırına yaz; §6 tablosunu güncelle |
| R04, R05, R10, R11, 12 | Plan B2/C geçerli seçenek gibi; R11 asistanı yapay zekâ düzeylerine yerleştirmiyor | Başlarına "K-020: kart gerektiren planlar kapsam dışı, bilgi için saklandı" notu; R11 §4'e metin asistanı satırı; 12'ye asistan testi (aynı 10 kısa hikâye; D3 §4 ölçütü) |
| 04 | K-014 ("sonra kart"), K-006, K-015 eski hâlde; K-017..20 §7.4 alanları olmadan madde | Eskiyi silmeden yeni kararlar yaz: kart alımı kapsam dışı (K-014 yerine); UE5 yeniden açılma koşulu bu bütçede sağlanamaz; asistan kararı (gerekçe, alternatif, risk, doğrulama, geçerlilik) |
| 05 | Q-015, Q-018 "açık"; **Q-019 iki ayrı soruda**; 2D kaynak sorusu "araştırılmadı" (D2 §3 araştırdı) | Q-015 "kısmen", Q-018 "deneme kapsamı yanıtlandı"; asistan sorusunu Q-020 yap; 2D satırını güncelle |

### B2. ÖNEMLİ — D2 bulguları mimariye ve yol haritasına girmemiş (M; ölçümler T)
14, 15, R08, R09'da 4K, düz yazıdan sahne ve çocuk karakter yok. 4K, düz yazı ve çocuk karakter yalnız kullanıcı özetinde ve 16'da anılıyor.
**Düzeltme:** 14'e: (a) 1080x1920 varsayılan, 4K yalnız ölçümden sonra, koddaki sabit boyutlar; (b) mevcut `DuzMetniCevir` genişletilecek, güven işareti ve kullanıcı düzeltmesi, asistanla ilişkisi; (c) çocuk karakter kıyafet/kamera kuralları. 15'e ilgili işler; 12'ye dikey 4K kodlayıcı kabul testi ve düz yazı doğruluk testi. R03/R06'ya D1/D2 karşılaştırmaları (stilize, 2D paket, STP/upscaling) için kısa ek ya da açık gönderme.

### B3. ÖNEMLİ — Köprü yasağı proje talimatlarına yansımamış (K, sonra M)
CLAUDE.md ve GEMINI.md hâlâ yapay zekâya Unity'yi dosya köprüsüyle sürmesini söylüyor; K-004 yalnız araştırma klasöründe. Yeni bir oturum yasağı görmeden köprüyü kullanabilir. D3 §1 GEMINI.md riskini söylüyor; 07'de kayıt yok.
**Düzeltme:** 07'ye risk kaydı (M). Kullanıcı onayıyla iki dosyanın başına geçici uyarı (K). R08 §4 ve 14'teki "köprü komutları korunur" ifadesine "ajan kullanımı Q-016'ya bağlı" ekle (M). Unity'ye yazılı soru ya da CLI kararı (K).

### B4. ÖNEMLİ — Rapor 12 (araştırma hafızası) çok bayat (M)
R12: 534 kaynak, K-001..K-003, Q-001..017, C-001..029, R-001..011; "öz denetim ve bağımsız denetçi yapılmadı". Gerçek: 619 kaynak, K-020'ye, C-031'e, R-041'e kadar; DEN, D1–D3, 16 var. §15'in zorunlu raporu yeni oturumu yanlış yönlendirir.
**Düzeltme:** §2, §4, §6, §7'yi güncelle; D1–D3, DEN, DEN2'yi haritaya ekle. R02 §toplamındaki 534'ü de güncelle.

### B5. ÖNEMLİ — 00, 02, 16 tutarsız (M)
00: D2, D3, K-017..20 yok; "kalan: kullanıcı yanıtları". 02: "Bekleyen: 6 soru" derken altta yanıtlar sonrası D3 var; açık araştırma için hâlâ D1 §5. 16 §1: D2'nin kapattığı konuları madde 1'de "eksik" sayıyor; madde 12 "stilize test eklenmeli" (12'ye eklendi); §3 "görünüm, yapay zekâ ve bütçe yanıtı bekliyor"; D3 hiç yok.
**Düzeltme:** üçünü güncelle; 16'ya DEN2 sonrası yeni bir öz denetim turu ve §18 tablosu ekle (eski metin silinmeden).

### O1. ORTA — Risk kaydında D2/D3 riskleri eksik (M)
07'ye ekle: asistanın biçimce geçerli ama yanlış konuşan/mekân üretmesi; uygulama içi LLM ve Unity koşulu belirsizliği (D3 §5 durum 2); TikTok 18 yaş altı benzerliği ve çocuk karakter; yerel modelin hızı/diski; Gemini "18 altının erişmesi muhtemel uygulama" kısıtı (R-038 yalnız kısmen). R10 buna göre güncellensin.

### O2. ORTA — R07 ile 12 uyumsuz (M)
R07'de Unity uyarısı (K-004) ve görünüm karşılaştırma testi yok. İkisini ekle. 12'deki görünüm denemesine test numarası, kayıt ayarı ve tekrar yöntemi ver (K-019 sonrası iki görünüm; üçüncü "bugünkü" taban).

### O3. KÜÇÜK-ORTA — Teknoloji, maliyet, performans veritabanları (M)
08 yalnız satır dizini; D1–D3 teknolojileri (Unity CLI, Gemini metin, yerel modeller, toon, STP, 2D paketler) yok; §7.5 alanları tek yerde değil. 11 D2/D3 fiyatlarını içermiyor, "A10 araştıracak" türü eski cümleler 10'da duruyor.
**Düzeltme:** 08'e teknoloji adı başına tek satırlık tablo (lisans, donanım, maliyet, test durumu, güven, dosya göndermesi); 11 ve 10'u güncelle.

### O4. KÜÇÜK — 11 sınıfsız kaynak (M)
09'da hâlâ açık. Elle sınıflandır.

### O5. KÜÇÜK — Adlandırma ve tek ayrıntı (M)
R06/R11'deki "D2, D3" yapay zekâ düzeyleri, araştırma dosyaları D2/D3 ile karışıyor; düzeyleri "YZ-2" gibi adlandır. D3 §3(a): Anthropic sayfası, son kullanıcının değiştirilmemiş Claude Code'a kendi aboneliğiyle girmesini engellemediğini de yazıyor (üründe çalıştırma Ticari Şartlar ve koşullarla); "belirsiz" cümlesine bu eklensin.

## 3. Örneklem doğrulamaları (10 web çağrısı)
| İddia | Sonuç |
|---|---|
| Unity Terms §17.2(ff) ajan yasağı, 16 Kas 2026 | **Doğrulandı** |
| Yetkili erişim tanımı (Unity arayüzü, yetki, koşul; geri alınabilir); "Offerings" | **Doğrulandı**; sayfa güncelleme notu kendi işlettiğiniz ajanların CLI üzerinden yetkili sayıldığını aktarıyor |
| Gemini: 18+, ücretsizde veri kullanımı ve insan incelemesi, AEA/CH/BK yalnız ücretli | **Doğrulandı** https://ai.google.dev/gemini-api/terms |
| Claude aboneliği üçüncü taraf uygulamaya konamaz | **Doğrulandı** (O5 nüansı) https://code.claude.com/docs/en/legal-and-compliance |
| TikTok 18 yaş altı benzerliği | **Doğrulandı**; yardım sayfası bunu yasak yapay zekâ içeriği altında sayıyor. Kurgusal karakter kapsamı yine belirsiz. https://support.tiktok.com/en/using-tiktok/creating-videos/ai-generated-content |
| TikTok gerçekçi yapay içerik etiketi | **Doğrulandı** (şeffaflık sayfası) |
| ElevenLabs ücretsiz: ticari yok, başlıkta "elevenlabs.io", sonradan plan değiştirmez | **Doğrulandı** https://elevenlabs.io/docs/help-center/legal/can-i-publish-the-content-i-generate-on-the-platform |
| Gemini 3.5 Flash-Lite fiyatı / 3.8 TTS ücretsiz katmanı | **Doğrulanamadı** (okunan bölümde yok); 3.1 Flash TTS Preview ücretsiz katmanı görüldü. D3'ün "doğrulanamadı" notu yerinde |

Uydurma ya da tersine dönmüş bir iddia görmedim. HDRP duyurusunu ilk denetim doğruladığı için yinelemedim.

## 4. §18 ölçütleri
| # | Ölçüt | Durum | Kapatma |
|---|---|---|---|
| 1 | Ana alanlar incelendi | sağlandı, açık kısıtlarla (Reddit, bazı sektörler, çocuk sesi) | kısıtları 16'da tek listede topla (M) |
| 2 | Ajan görevleri / kısıtlar | sağlandı (A05/A10 kesintisi kayıtlı) | — |
| 3 | Uluslararası araştırma | sağlandı | — |
| 4 | Teknolojiler karşılaştırıldı | kısmen | B2, O3 (M) |
| 5 | Alternatifler | sağlandı (masa başı; D1–D3) | rapora bağla (M) |
| 6 | Donanım analizi | sağlandı, ölçümsüz | K-020 işlenmeli (M); ölçüm (T), güç/disk yuvası (K) |
| 7 | Maliyet analizi | kısmen: yeni bütçeye göre plan yok | B1 (M); TL fiyatı (K) |
| 8 | Lisans analizi | sağlandı, sınırlarıyla (bazı asıl metinler) | Unity netliği (K) |
| 9 | Teknik uyumluluk | sağlandı, yalnız belge/kod okuması | deneme (T) |
| 10 | Kaynaklar kaydedildi | sağlandı (619; 11 sınıfsız) | O4 (M) |
| 11 | Çelişkiler değerlendirildi | sağlandı; bir kısmı teste bağlı | C-014'ü K-020 ile güncelle (M) |
| 12 | Riskler | kısmen | B3, O1 (M) |
| 13 | Test planı | sağlandı | asistan, 4K, düz yazı testleri (M) |
| 14 | Nihai mimari önerisi | **kısmen**: asistan yok | B1, B2 (M) |
| 15 | Yol haritası | **kısmen**: asistan, görünüm denemesi, bütçe yok | B1, B2 (M) |
| 16 | Kalıcı hafıza | kısmen | B4, B5 (M) |
| 17 | Dosyalar güncel | **sağlanmadı** | B1, B4, B5 (M) |
| 18 | Araştırma denetimi | kısmen: 16 bayat, D3'ten önce | B5 (M) |
| 19 | Bağımsız denetçi | sağlandı (DEN + DEN2) | — |
| 20 | Geçiş gerekçesi | sağlandı, güncellenmeli | B1 (M) |

## 5. Kapatma sırası
1. **Masa başı:** B1 → B2 → O1/O2 → B4/B5 → O3–O5. Ardından 16'da §18 yeniden değerlendirilir; yalnız dosya tutarlılığı için kısa bir kontrol yeter. Bunlar yapılırsa, açık kısıtlar yazılı kalmak şartıyla araştırma "masa başı kapsamında tamamlandı, testler yapılmadı" diye işaretlenebilir.
2. **Kullanıcı:** Unity çalışma yolu ve proje talimatı uyarısı; asistanın yeri; veri/18+; ses ve görüntü iyileştirmede yapay zekâ sınırı; kabul edilen kayıt süresi; güç kaynağı, boş disk yuvası.
3. **Test (ayrı onayla):** T0 teşhis, iki görünüm, asistan sınaması, 4K kabulü. Bunlar araştırmayı kapatma şartı değil, geliştirme kararının şartıdır.
