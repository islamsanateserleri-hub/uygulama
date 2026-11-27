# R05 — Maliyet analizi (protokol §12: üç plan)

Tarih: 2026-11-27. Fiyatlar kaynaklardan aktarılmıştır, hızlı değişir. **Satın alma kararı yoktur.** Performans ve görüntü kalitesi satırlarının hepsi ÇIKARIM ve **TEST EDİLMEDİ**. Ana kaynak: A10; ek: A02, A03, A07, A12, `11_COST_DATABASE.md`.

**Önemli:** Plan B ve C için güvenilir tek bir TL toplamı **verilemiyor** (güncel TL ekran kartı fiyatı alınamadı; SSD ve güç kaynağı TL fiyatı yok). Emek (geliştirme zamanı) ve elektrik hiçbir planda hesaplanmadı.

## Plan A — Sıfır yatırım
| Alan | İçerik |
|---|---|
| İçerik | Mevcut bilgisayar; Unity URP; mevcut kusurların düzeltilmesi (hareket, ses-altyazı, yüz, gölge/ışık, kamera); Rocketbox; CC0/MIT varlıklar; isteğe bağlı Blender/MPFB (disk yeterse) |
| Toplam maliyet | 0 TL. Gemini ücretsiz kota (~10 istek/gün gözlemi; doğrulanmadı) |
| Avantaj | Risksiz; kullanıcının reddettiği ana kusurlar (altyazı, hareket, yürüme) donanım istemez; satın alma için gereken ölçümü üretir |
| Dezavantaj | Işın izleme yok; ağır efekt sınırlı; disk dar; CC5/MetaHuman/UE5 yok; ücretsiz ses kotası yavaşlatır; ücretsiz Gemini'de girdiler insan incelemesine açık olabilir |
| Görüntü kalitesi | Bugünkünden belirgin iyi olabilir; örnek yapay zekâ videosunun seviyesi beklenmemeli |
| Render performansı | Ölçülmedi. En iyi durumda gerçek zamana yakın (30 kare sınırı); ağır ayarlarda daha uzun |
| Donanım ihtiyacı | Yok (disk temizliği gerekebilir) |
| Uzun vadeli sürdürülebilirlik | URP Unity'nin ana yolu; Windows 10 uzatılmış güncellemesi Ekim 2027'de bitiyor; Vega sürücüsü sınırlı destekte |

## Plan B — Minimum yatırım
İki aşama; B2 ancak ölçüm gerektirirse.

| Alan | B1: yalnız gerekli | B2: giriş seviyesi ekran kartı |
|---|---|---|
| İçerik | 1 TB SSD; isteğe bağlı küçük bulut GPU denemesi; isteğe bağlı ücretli ses (film başına <1 $ Gemini) | B1 + 12 GB sınıfı kart (ör. RTX 3060 12 GB) + gerekirse güç kaynağı |
| Toplam maliyet | ABD göstergesi ~144 $ (SSD) [A10-S022]; **TL alınamadı** | ~144 $ + 350–460 $ [A10-S016] + güç kaynağı; **TL hesaplanamadı** |
| Avantaj | Disk kısıtı kalkar; düşük risk | 12 GB görüntü belleği; Vega 7'ye göre genel testte ~8,5 kat grafik puanı (dizüstü ağırlıklı ortalama, sahnemize çevrilemez) [A10-S028]; Blender GPU; CC5 asgarisi (8 GB) |
| Dezavantaj | Görüntüyü tek başına değiştirmez | Eski kart; ikinci el riski; DLSS 5 yok; güç kaynağı/kasa belirsiz |
| Görüntü kalitesi | A ile aynı | Gölge, süper örnekleme, sis daha rahat; kalite yine yazılım ve varlıktan gelir |
| Render performansı | A ile aynı | Gerçek zaman sınırına yaklaşması beklenir; sınır kaldırılmadan onun altına inmez |
| Donanım ihtiyacı | Boş M.2 ya da SATA | ~550 W kaliteli güç kaynağı, kasada yer |
| Uzun vadeli | SSD her sisteme taşınır | Yüksek ayar ve yeni araçlarda 2–3 yılda sınır olabilir |

## Plan C — Fiyat-performans odaklı yüksek kalite
| Alan | İçerik |
|---|---|
| İçerik | 16 GB ayrı kart + 1–2 TB SSD + gerekirse kaliteli 650 W güç kaynağı ve fan; URP, Blender, MPFB, CC0 (ücretsiz); isteğe bağlı CC5, MHBridge, hareket araçları, ElevenLabs |
| Kart adayları | C-a RTX 5060 Ti 16 GB (~31.000 TL) · C-b RTX 5070 12 GB (~42.000 TL; 16 GB hedefinin altında) · C-c RX 9060 XT 16 GB (~25.000 TL). **TL değerleri 18 Tem 2026, zayıf kaynak** [A10-S017]. Blender/karakter araçları kullanılacaksa C-a; yalnız Unity ise C-c de aday. Unity testi olmadan "en iyi" denemez |
| Toplam maliyet | Kart ~25–42 bin TL (eski veri) + SSD (TL yok) + güç kaynağı (TL yok) + yazılım 0 – yaklaşık 600 $. **Tek toplam verilemiyor** |
| Avantaj | Yüksek ayarlar, donanım ışın izleme, Blender hızlanması (NVIDIA), CC5/MetaHuman denemesi |
| Dezavantaj | Bellek krizinde pahalı; kalite otomatik gelmez (karakter, ışık, animasyon emeği); UE5'e geçiş ayrı ve büyük iş |
| Görüntü kalitesi | "İyi oyun sinematiği" için donanım engeli büyük ölçüde kalkar; "gerçek film" garantisi yok |
| Render performansı | URP 1080x1920 için bol olması beklenir; **ölçüm yok** |
| Donanım ihtiyacı | 450–650 W güç kaynağı (karta göre), ~25–30 cm yer, boş disk yuvası |
| Uzun vadeli | 16 GB daha uzun ömürlü olur (ÇIKARIM); anakart PCIe 3.0'da kalır; Windows 11 gerekecek |
| Zamanlama | Kaynaklar 2027'den önce fiyat rahatlaması beklemiyor (tahmin) [A10-S015][A10-S022] |

## İsteğe bağlı kalemler
| Kalem | Fiyat | Kaynak | Not |
|---|---|---|---|
| Gemini TTS ücretli, 5 dk ses | ~0,07 $ (2027'de ~0,14 $) | A07 | yeniden denemeler hariç |
| ElevenLabs | 6–22 $/ay | A07 | ücretsiz planda ticari yok + atıf |
| Character Creator 5 | 299 $ ya da 498 $ | A02 | çelişkili fiyat; güncel bakılmalı; dağıtımda lisans riski |
| MetaHuman | 0 (1 M$ altı) + güçlü bilgisayar; MHBridge 79,99 $ | A02 | aktarım yeni/kırılgan |
| Ek hareket paketleri / videodan hareket | 0–15 $; ~12–20 $/ay | A03 | sonraya |
| Cascadeur Indie | 96 $/yıl | A03 | gerekirse |
| Mağaza ejderhası | 42–230 $ | A06 | animasyon/lisans doğrulanmadı |
| Video-to-video bulut | 90 sn için ~5–46 $ (liste fiyatından hesap) | A11 | **şartınıza aykırı görünüyor; önerilmez** |

## Planlar arasında karar sırası (öneri)
A → ölçüm → (disk gerekirse) B1 → (ölçüm süre sınırını aşarsa ve güç kaynağı/kasa uygunsa) B2 veya C. Kartı alıp sorunları kodda bırakmak yüksek risktir (A10-R7).
