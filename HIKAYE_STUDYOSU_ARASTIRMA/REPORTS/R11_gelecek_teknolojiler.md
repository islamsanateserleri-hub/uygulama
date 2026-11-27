# R11 — Gelecek teknolojiler

Tarih: 2026-11-27. **Hiçbiri denenmedi.** Ana kaynak: A11; ek A00, A02, A03, A06, A07. "Bugün" = ürün ya da kod yayımlanmış; **bizde çalıştığı anlamına gelmez.** Senaryolar ÇIKARIM, güven düşük–orta.

## 1. Bugün uygulanabilir
| Teknoloji | Donanım | Not |
|---|---|---|
| URP'de kullanılmayan temel özellikler (gölge, HDR, post) | mevcut (hız testi gerek) | ilk iş |
| Kural tabanlı otomatik yönetmen + önizleme karesine bakan denetçi | mevcut | fikir [A11-S015]; yapay zekâ gerekmez |
| Yerel hizalama (MFA/parseh) | işlemci | kurulum onayı; Türkçe doğruluk ölçülmedi |
| CC0 varlıklar, Megascans | mevcut | disk |
| Infinigen Indoors (hazır mekân üretimi) | NVIDIA ya da yavaş; Windows deneysel | aktarım emeği [A11-S029] |
| UniRig, TRELLIS.2 (otomatik iskelet, görselden 3D) | güçlü NVIDIA (kiralık) | lisans denetimi gerek [A11-S011][A11-S020] |

## 2. Deneysel
| Teknoloji | Engel |
|---|---|
| Unity 6.7 Surface Cache GI, SSR, Unity Neural | beta/önizleme [A11-S014] |
| Motor görüntüsünü "gerçekçileştiren" modeller (NeuralSRender, DiffusionRenderer) | büyük bellek; insan tutarlılığı kanıtsız [A11-S005] |
| Açık video modelleri (LTX, Wan, VACE) | güçlü kart/bulut; video üreticisi sayılır [A11-S008–S010] |
| Gaussian avatar ve splat sahne | Unity yolu yok ya da DX12 ister [A11-S013][A11-S019] |
| Yapay zekâ ajanlı sinematografi | token maliyeti, tek plan [A11-S015] |
| Metinden harekete, sesten yüze (Kimodo, Audio2Face) | NVIDIA, temizlik [A03-S034][A07-S055] |

## 3. İzlenecek
| Konu | Ne olursa harekete geçilir |
|---|---|
| DLSS 5 için genel SDK / Unity desteği | resmî duyuru + yüz tutarlılığı testi + kullanım izni |
| Unity 7 (Aralık 2026 önizleme; 2027 1. çeyrek hedefi) | tam sürüm + örnek sahnede ölçüm |
| URP 7.x hacimsel sis, ışın izlemeli ışık | yayımlanınca |
| Açık, yeniden ışıklandırılabilir splat avatar; otomatik yüz rig'i | Unity çalışma zamanı + ticari lisans |
| MetaHuman'ın Unity araçları ve dağıtım koşulları | olgunlaşma + yazılı netlik |
| Açık Türkçe sesler | dinleme testi + veri hakkı |
| Platform etiket kuralları ve model lisansları | her gözden geçirmede |

## 4. Yapay zekâ "çizgisi" (kullanıcı kararı, Q-015)
| Düzey | Ne yapar | Öneri |
|---|---|---|
| D0 | klasik çizim | uyumlu |
| D1 | görüntü büyütme/temizleme | uyumlu sayılır |
| D2 | yardımcı araç: ses, hareket, 3D model üretimi, hizalama | son kareyi Unity üretir; kullanıcı onaylamalı |
| D3 | motor karesini yapay zekâyla güzelleştirme | **gri alan: sorulmalı** |
| D4 | video-to-video | A11: video üreticisi sayılır |
| D5 | metinden/resimden video | kapsam dışı |

Sınama sorusu (A11): "Bu model bizim 3D sahnemiz olmadan da video üretebiliyor mu?" Evetse video üreticisidir.

## 5. 1–3 yıl (tahmin)
- 2027: URP'de daha iyi dolaylı ışık ve büyütme; görüntü iyileşir, "gerçek film" olmaz.
- 2027–2028: kare iyileştiriciler yaygınlaşabilir.
- 2028–2029: motor koşullu video modelleri tüketici kartında tutarlı hâle gelebilir; gecikebilir ya da lisansla kısıtlanabilir.
Hangi senaryo olursa olsun değerini koruyacaklar: hikâye sözdizimi, çekim listesi, kamera/oyuncu yönetimi, ses-zaman verisi, temiz lisanslı 3D varlık.

## 6. Mimaride bırakılacak yer
Çekim listesi · tohumlu tekrar üretilebilirlik · ek geçişler (derinlik, maske) · takılabilir kare işleyici (parça parça, diskte biriktirmeden) · sağlayıcı arayüzleri · köken/lisans kaydı (A11; X1 G1–G7).

## 7. Takip düzeni
Üç ayda bir (en çok 10 arama) + tetikleyiciyle. İlk kontrol Ocak 2027. Her aday için aynı beş planlık "altın sahne" (yüz yakın, iki kişi, yürüme, geniş kalabalık, gece/ateş). Geçiş için hepsi şart: resmî kaynak, lisans, donanımda çalışma, altın sahne testi, kullanıcı onayı. Önerilen takip dosyası `TOPICS/TAKIP_teknoloji.md` **henüz açılmadı**; zamanlanmış görev kurulmadı.

## 8. Eksikler
SIGGRAPH bildiri listeleri sistematik taranmadı, SIGGRAPH Asia programı taranmadı; NeRF derin okunmadı.
