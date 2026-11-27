# R04 — Donanım analizi

Tarih: 2026-11-27. **Bu bilgisayarda hiçbir performans ölçümü yapılmadı** ve bu donanıma ait yayımlanmış Unity ölçümü bulunamadı. **Satın alma kararı yoktur** (protokol §11, §19). Ana kaynak: A10; denetim: X1 §3.

> **K-020 (bütçe yalnız ek disk):** ekran kartı, güç kaynağı ve ücretli bulut seçenekleri **bütçe dışı**; §6-§8 ve §9'un "yüksek kalite" sütunu yalnız bilgi için saklandı.

## 1. Mevcut bilgisayar
| Parça | Durum | Kaynak |
|---|---|---|
| İşlemci | Ryzen 5 5600GT, 6 çekirdek / 12 iş parçacığı | TOPICS/00, [A10-S001] |
| Grafik | Tümleşik Radeon "Vega 7"; ayrı belleği yok, RAM'i paylaşır; ışın izleme yok | [A10-S001][A10-S028]; resmî sayfa "Vega" demiyor, güven yüksek-orta |
| RAM | 64 GB (bol) | TOPICS/00 |
| Anakart | Gigabyte A520M K V2: PCIe 3.0 x16, 1 M.2, 4 SATA | [A10-S002] |
| Disk | C: ~45 GB boş (**en somut kısıt**) | TOPICS/00 |
| Sistem | Windows 10, Direct3D11 | TOPICS/00 |
| Güç kaynağı, kasa | **Bilinmiyor** (Q-001) | — |

## 2. Bu bilgisayarla ne yapılabilir (A10 §S3; ÇIKARIM)
| İş | Durum |
|---|---|
| Ses-altyazı, hareket, yürüme, kamera, yüz eşleme düzeltmeleri | yapılabilir |
| URP gölge, HDR, tonemapping, SSAO, plana göre ışık | denenebilir; hız bilinmiyor |
| 1080x1920/30 kayıt | zaten yapılıyor |
| Süper örnekleme, hacimsel sis, cilt SSS | yüksek risk |
| Tel tel (strand) saç, VFX Graph | deneysel |
| 30–60 gerçek karakterlik kalabalık | belirsiz (X1 §3: A10 bunu denetlememişti) |
| HDRP ışın izleme, UE5 Lumen | yapılamaz |
| Character Creator 5 | büyük olasılıkla yapılamaz: okunan sayfa 8 GB ayrı kart istiyor [A02-S037], ancak sayfanın CC5'e ait olduğu kesin değil (X1 D17) |
| MetaHuman üretimi | önerilen donanımın altında [A02-S007] |
| Yerel yapay zekâ video/3D modelleri | pratik değil (A11) |
| Blender Cycles | yalnız işlemciyle, çok yavaş [A04-S030] |

## 3. Kayıt süresi hakkında
- Kayıtta oyun döngüsü 30 kare/sn ile sınırlı [KOD: A09 K7; X1 D6]. 60 sn video en az ~60 sn'de kaydedilir. `captureDeltaTime` ile etkileşimi ölçülmeli.
- Düzeltme: "ekran kartı tek başına kayıt süresini kısaltmaz" ifadesi fazla geneldir. Doğrusu: kare süresi 33 ms altındaysa sınır kaldırılmadan kısalmaz; ağır karelerde daha güçlü kart süreyi kısaltır (X1 §3).
- Tek gözlem: 299 sn'lik bir film ~5 dk 8 sn'de kaydedilmiş (köprü günlüğü, `10_PERFORMANCE_DATABASE.md`).
- Blender "87 saat / 124 saat" hesapları planlamada kullanılmamalı (A10 §S7).

## 4. Disk bütçesi (tek tablo; X1 §8.5 isteği) — değerler tahmin/aktarım, ölçülmedi
| Kalem | Boyut | Kaynak |
|---|---|---|
| Boş alan | ~45 GB | TOPICS/00 |
| EXE derlemesi | 10,8 GB | DURUM.md (A09, X1) |
| Kurulu uygulama | ~11 GB | DURUM.md (X1) |
| Kurulum dosyası | ~3,1 GB | DURUM.md (X1) |
| Ortam varlık kütüphanesi önerisi | 5–8 GB | A05 |
| Doku/HDRI, flipbook, Unity 6.7, Blender, prototip kopyası | ölçülmedi | X1 |
| 5 dk sıkıştırmasız kare dizisi | 75–149 GB (**saklanmamalı**) | A09 hesabı |
| CC5 | 30 GB + 200 GB önbellek (sayfanın CC5'e ait olduğu kesin değil, X1 D17) | [A02-S037] |

Sonuç [ÇIKARIM]: kalemler tek tek sığar görünür, **toplamda sığacağı doğrulanmadı**. Önce envanter (test T0-8), sonra kullanıcı onayıyla temizlik ya da ek disk. A11'in "kareleri diske yazıp işle" fikri ancak parça parça ve silerek yapılabilir (C-026).

## 5. Ücretsiz kontroller (A10 §Plan A)
RAM hızı/XMP · BIOS sürümü ve paylaşılan bellek ayarı · disk temizliği · Windows 10 uzatılmış güvenlik güncellemesi kaydı (Ekim 2027'ye kadar; haberde 12 ve 14 Ekim geçiyor) [A10-S024] · güç kaynağı etiketi. BIOS değişikliği yalnız kullanıcı isterse.

## 6. Ekran kartı adayları (BÜTÇE DIŞI; bilgi için)
| Kart | Bellek | Güç / önerilen güç kaynağı | TL (18 Tem 2026, "başlangıç") | Not |
|---|---|---|---|---|
| RTX 3060 12 GB | 12 | 170 W / ~550 W (tablo başlığı doğrulanamadı) | yeni fiyat alınamadı | eski; DLSS 5 yok |
| RTX 5060 Ti 16 GB | 16 | 180 W / 600 W | ~31.000 | bu anakartta 3.0 x8 |
| RTX 5070 | 12 | 250 W / 650 W | ~42.000 | hızlı, bellek az |
| RX 9060 XT 16 GB | 16 | 160 W / 450 W | ~25.000 | Blender'da çok zayıf |
| Arc B580 | 12 | alınamadı | ~12.000 | bu tür işlemcilerde görüntü kaybı bildirimi → riskli [A10-S011] |

Kaynak: [A10-S005–S008, S010–S011, S017]. TL sayıları tek ve zayıf (C) bir kaynaktan, 4 ay eski; sonrasında dünya fiyatları arttı. İkinci el TL fiyatı güvenilir değil (C-015). 8 GB kart önerilmez [A10-S008]. Blender'da 5060 Ti, 9060 XT'den ~2,7 kat hızlı; Unreal'da yakın [A10-S026]. Unity URP için karşılaştırma **yok**.

**Kart seçiminde açık çelişki (C-014):** "RTX 3060 yeter" (eski belge) ↔ A10 "en az 12, tercihen 16 GB" ↔ A11 "RTX 50 geleceğe daha açık". Çapraz denetim yargısı: kullanıcı yapay zekâ kare iyileştirmeyi (Q-015) onaylamadıkça ve Unity desteği duyurulmadıkça DLSS 5 ölçüt olmasın; kart kararı ölçümden sonra (X1 §3).

## 7. Güç kaynağı, kasa, SSD
Güç kaynağı en büyük bilinmeyen: yetersiz/eski ünite ile kart takmak donanım hasarı riski (A10-R1). Gerekli: marka, model, watt, yaş, 8-pin kablo; kasada yaklaşık 25–30 cm kart yeri. PCIe 4.0 SSD burada 3.0 hızında çalışır; pahalı model gereksiz. ABD'de 1 TB ~144 $ [A10-S022]; güvenilir TL fiyatı yok.

## 8. Bulut GPU (bütçe dışı)
RunPod 0,27–0,89 $/saat (örnekler; Windows olduğu yazmıyor), Shadow Pro 26–41 £/ay [A10-S030][A10-S031]. Unity Editor koşulları bulutta Editor çalıştırmayı kısıtlıyor; derlenmiş EXE yönü daha güvenli, ama koşullar ayrıca okunmalı (A10, A12). Saatlik Windows kartı için doğrulanmış fiyat bulunamadı.

## 9. Asgari ve önerilen
| | Asgari (bugün) | Yüksek kalite hedefi (ÇIKARIM; bütçe dışı) |
|---|---|---|
| İşlemci/RAM/anakart | mevcut | mevcut (ölçüm darboğaz göstermedikçe) |
| Grafik | Vega 7, düşük/orta ayar | 16 GB (alt sınır 12) ayrı kart |
| Disk | ~30 GB boş tutulmalı | +1–2 TB SSD |
| Güç kaynağı | mevcut | kaliteli 550–650 W (karta göre) |
| Sistem | Windows 10 + güncelleme kaydı | 2027 içinde Windows 11 |

## 10. Öneri
1. Şimdi para harcama; önce ölç (12_TEST_PLAN T0-6, TS-4).
2. İlk ihtiyaç büyük olasılıkla disk.
3. Disk almadan önce boş M.2/SATA yuvası kontrol edilsin. (Kart konuşulmadığı için güç kaynağı bilgisi şimdilik gerekmiyor.)
4. Kullanıcı "1 dakikalık video için en çok şu kadar beklerim" desin (Q-021); ölçüm bunu aşarsa ayar/içerik azaltılır.

Disk yararı: kopya proje, Library, kayıt ara dosyaları, indirme, istenirse yerel model. Görüntü kalitesini ve kare hızını artırmaz (D3 §6).
