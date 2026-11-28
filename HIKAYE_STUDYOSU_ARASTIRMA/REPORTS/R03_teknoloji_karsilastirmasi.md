# R03 — Teknoloji karşılaştırması (araçlar, alternatifler, lisanslar)

Tarih: 2026-11-27. **Hiçbiri bu projede karşılaştırmalı denenmedi.** Lisans sınıfları A12 ana tablosundandır; **hukuki görüş değildir**.
Sınıflar: G güvenli · K koşullu · R riskli · D doğrulanmamış. Sütunlar: K1 kendi videon (ticari değil) · K2 para kazanan video · K3 uygulamayı başkasına vermek.
X2 L4 notu: A12'de bazı "G" satırları lisans metni yeniden okunmadan ajan raporundan devralındı; bunlar "(devralınan)" diye işaretlendi.

## 1. Motor / render yolu (A09 §S9, A04; ÇIKARIM: zahmet süreleri tahmin)
| Seçenek | Kod korunumu | Bu bilgisayarda | Görüntü tavanı | Zahmet | Yargı |
|---|---|---|---|---|---|
| **Unity URP (mevcut, geliştirilmiş)** | tamamı | uygun | iyi oyun görüntüsü | gün–hafta | **önerilen** |
| Unity HDRP | kısmen | belirsiz | insan/ışık daha iyi | 4–8+ hafta | hayır: kaldırılacak (K-003) |
| Unreal 5 + MRQ | yalnız hikâye sözdizimi | uygun değil (GPU, disk) | en yüksek (güçlü kartla) | aylar | şimdi hayır (Q-013) |
| Blender tam | sözdizimi | CPU'da çok yavaş | yüksek | aylar | hayır; en çok kıyas karesi |
| Hibrit Unity→Blender | yönetmen korunur | aynı sınır | yüksek, aktarım kayıplı | 1–3 ay | ileride "kahraman plan" için düşünülebilir |

Kullanıcının getirdiği araştırma UE5 öneriyor (K01). Karşılığı: UE5'e geçmek sahne tarafının yeniden yazılması demek; bu bilgisayarda ekran kartı yok; 2,39:1 siyah bar hedef 9:16 ile çelişiyor (C-011). K01 içeriği doğrulanmamış (D).

## 2. Karakter (A02; görsel sıralama karşılaştırmalı görüntüye dayanmıyor, X1 §7)
| | Rocketbox | MPFB | CC5 | MetaHuman | VRoid |
|---|---|---|---|---|---|
| Görünüm tavanı (ÇIKARIM) | orta-düşük | orta | yüksek | en yüksek | anime |
| Unity yolu | çalışıyor | FBX, elle | araç ≤6000.3 listeli | araçlar yeni/kırılgan | VRM |
| Bu PC'de üretim | hazır | olası | hayır | hayır | evet |
| Para | 0 | 0 | 299 ya da 498 $ (kaynaklar farklı) | 0 (1 M$ altı) + güçlü PC; MHBridge 79,99 $ | 0 |
| K1/K2/K3 | G/G/G | G/G/G–K | K/K/R | K/K/R | kendi model G/G/K; örnek model K/K/R |

Yan not: "CC5 daha az riskli" yalnız teknik anlamdadır; kullanıcıya karakter özelleştirme sunan uygulamada Enterprise lisans gerekebilir (A12 §S3, forum düzeyi; X1 §2.3). Hiçbir yeni kaynakta yüz+gövde+EXE uyumu doğrulanmadı; uygulamada karakter için ara katman (bağdaştırıcı) da yok (X1 U1).

## 3. Hareket (A03)
| Kaynak | Ne verir | Lisans (K1/K2/K3) | Not |
|---|---|---|---|
| Rocketbox animasyonları | yürüme, koşma, oturma, jest | G/G/G (MIT) | projede yalnız yerinde duran küme var [KOD] |
| UAL (CC0) | oyun hareketleri | G/G/G | projede |
| Mixamo | çok çeşit | G/K/R | 36 Mixamo karakteri katalogda; klasör doluysa animasyonları bütün insan karakterlere de ekleniyor (C-021) |
| Rokoko / QuickMagic | videodan hareket | D | kota ve temizlik |
| Cascadeur ücretsiz, DeepMotion ücretsiz | — | K/R/R | ticari yasak |
| GVHMR/SMPL | — | K/R/R | ticari yasak |
| HY-Motion | metinden hareket | R/R/R | kullanma |

X2 §2.3: Mixamo'suz dağıtım profilinde bazı duygu klipleri düşer; yerine CC0/MIT karşılık listesi gerekir (var olup olmadığı doğrulanmadı).

## 4. Ses, hizalama, dudak (A07, A00, A12)
| Araç | Güçlü yan | Zayıf yan | Lisans K1/K2/K3 |
|---|---|---|---|
| Gemini TTS | mevcut, ucuz, duygu stili | zaman damgası yok; söz atlama/ekleme şikâyetleri (C) | K/K/K (18+, etiket) |
| ElevenLabs | zaman damgalı | kredi; ücretsizde ticari yok | ücretsiz K/R/R; ücretli G/K/D |
| Azure/OpenAI | alternatif | koşullar okunmadı | D |
| Kullanıcının kendi sesi | en temiz hak (ÇIKARIM) | emek | — |
| Piper (güncel motor GPLv3) | yerel | dfki sesi ticari değil | G/D/R |
| MFA Türkçe | fonem de verir | kurulum ağır; yazılımın dağıtımı incelenmedi | model G (devralınan)/…/K |
| parseh, wav2vec2-tr | hafif | doğruluk ölçülmemiş | G (devralınan)/K |
| whisper.cpp, uLipSync (MIT) | denetim/yedek | hizalamada kaba | G/G/G |
| Kendi viseme sürücüsü (Rocketbox) | paket gerekmez | eşleme ayarı | G |
| OVR LipSync | — | arşivlenmiş | R: kullanma |
| Audio2Face-3D | üst kalite | NVIDIA gerekir | D |

## 5. Ortam ve efekt varlıkları (A05, A06, A12)
| Kaynak | K1/K2/K3 | Not |
|---|---|---|
| Poly Haven, ambientCG (CC0) | G/G/G | ilk tercih |
| Quaternius, Kenney, KayKit, Unity Labs flipbook (CC0 beyanı) | G/G/G | low-poly, gerçekçi karakterle stil uyumsuzluğu riski |
| Megascans (Fab Standard, ücretsiz) | G/G/K | tek başına dağıtım yok; Epic hesabı |
| Sketchfab / Fab diğer | K/K/K–R | model model denetle |
| Unity Asset Store | G/K/R | kullanıcı içeriği üreten üründe yayıncı izni (C-016) |
| Synty | K/K/R | oyun yaratma yazılımı yasağı (A05) |
| CristianQiu volumetric (MIT) | G | paket onayı |
| NASA içerikleri (projede) | G/K/K | amblem kontrolü |

## 6. Efekt teknolojisi (A06)
| | Shuriken (yerleşik parçacık) | VFX Graph | CC0 flipbook | 2D katman (mevcut) |
|---|---|---|---|---|
| Donanım | compute istemez | compute ister; tümleşik kartta kararlılık belirsiz | her yerde | her yerde |
| Görünüm | iyi | en iyi | iyi | ekran üstü, derinliksiz |
| Yargı | ana yol (denenecek) | koşullu | ana yol | anlatım ikonlarında kalsın |

Not: mevcut 3D hava efekti Particle System değil, kendi mesh çizimidir (`HavaDurumu3D.cs`; C-019). Shuriken'e geçmek yeni iştir.

## 7. Kamera ve kurgu
| | Mevcut KameraYonetmen genişletme | Cinemachine 6.6 (kurulu) | Timeline | Kendi kurgu dosyası |
|---|---|---|---|---|
| Risk | düşük | orta (öğrenme, ikinci mantık) | ek katman | araç yazmak gerek |
| Yargı | ilk adım | tek sahnede dene | gerekirse | önerilen |

(Cinemachine'in kurulu olduğu TOPICS/00 beyanıdır; bu depo kopyasında doğrulanamadı, X1 D16.)

## 8. Video kodlama ve dağıtım
FFmpeg LGPL derlemesi kullanılıyor (x264/x265 yok) [KOD, A09/A12]. Uygulama dağıtılırsa lisans metni, tam kaynak, "Hakkında" bildirimi ve sabit sürüm gerekir; bugün yok (A12 §S6). H.264/AAC patent durumu doğrulanamadı. Wwise/FMOD gerekli görünmüyor (A12).

## 9. Kaçınılacaklar (A12, A05, A07)
HY-Motion/Hunyuan · OVR LipSync · ticari olmayan ses modelleri (XTTS-v2, MMS, Piper dfki) · doğrulanmadan MetaHuman'ı uygulama kütüphanesi yapmak · izinsiz Asset Store/Reallusion/Synty varlığını dağıtmak · bulut avatar servisine bağımlılık.

## 10. Ek: D1–D3 karşılaştırmaları
- **Görünüm yolları (gerçekçi 3D / stilize 3D / 2,5D / hibrit):** `../AGENTS/D1_duzeltme_arastirmasi.md` §2. İlk ikisi denenecek (K-019).
- **2D paketler ve çizer ücreti:** `../AGENTS/D2_acik_konular.md` §3.
- **Büyütme (doğal 4K, FSR 1, STP, dış araçlar):** D2 §4; yapay zekâlı büyütme ayrı onay ister.
- **Asistan seçenekleri (Claude dışarıdan / Gemini / yerel / ücretli):** `../AGENTS/D3_yapay_zeka_asistani.md` §3-§4. Gemini fiyat/katman satırlarından 3.5 Flash-Lite fiyatı ve 3.8 TTS ücretsiz katmanı **doğrulanmadı**.
- K-020: bu rapordaki ücretli ya da kart isteyen seçenekler bütçe dışı. Hiçbiri denenmedi.
