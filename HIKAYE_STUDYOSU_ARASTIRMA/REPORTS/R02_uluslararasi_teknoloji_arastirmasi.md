# R02 — Uluslararası teknoloji araştırması

Tarih: 2026-11-27. **Hiçbir teknoloji denenmedi.** Kaynak numaraları `SOURCES/` dosyalarındadır (toplam 534 kayıt; A 324, B 94, C 79, D 26, sınıfsız 11 — `09_SOURCE_DATABASE.md`, otomatik sayım).

## 1. Nasıl araştırıldı, sınırları
- 12 uzman ajan + 1 takip ajanı + 2 çapraz denetim. Ağırlık İngilizce kaynaklarda.
- Sayfalar çoğunlukla özetleyen bir araçla okundu; birebir metin garantisi yok.
- SIGGRAPH taraması sistematik değil; SIGGRAPH Asia programı taranmadı; NeRF ve virtual production yalnız yüzeysel (A00 §S5, A11 öz denetim).
- Web arama hakkı ilk dalgada tükendi; bazı konular takipte kapandı, bazıları açık (Q-012).

## 2. Motorlar ve render
| Bulgu | Kaynak |
|---|---|
| HDRP Unity 7.0'da "deprecated", ilk 7 LTS sonrası kaldırılacak; destek en az 2029 sonu | [A04-S001][A04-S002], C-003 |
| URP yol haritası: hacimsel sis ve ışın izlemeli aydınlatma 2027 (7.x) döneminde; cilt/saç anılmıyor | [A04-S002][A00-S045] |
| Unity 6.7 (beta): Surface Cache GI, SSR, GTAO, AgX | [A04-S007][A04-S008] |
| 6.7 LTS 27 Kas itibarıyla çıkmış görünmüyor (yokluk kanıtı değil) | [A00-S044] |
| Unity 7 önizleme Aralık 2026, tam sürüm hedefi 2027 1. çeyrek | A09 §S4, [A11-S004] |
| UE5 Movie Render Queue: zaman örnekleme, HDR görüntü dizisi; Lumen güçlü kart ister | [A09-S029][A09-S030] |
| Blender Cycles: Vega tümleşik kartta GPU desteği yok | [A04-S030] |

## 3. Dijital insan
- Rocketbox: MIT, 115 karakter, 15 viseme ve ARKit 52 uyumlu şekiller [A02-S020][A02-S024].
- MetaHuman Haziran 2025'ten beri başka motorlarda kullanılabiliyor; 1 M$ gelir altı ücretsiz [A02-S001]; Rig Logic/DNA MIT ile açıldı (Tem 2026) [A02-S012]; saç ve kıyafet aktarım paketinde yok.
- Unity araçları yeni: MHBridge (Kas 2026, yorumsuz) [A02-S015]; MetahumanURP yalnız Apple'da denenmiş [A02-S017].
- Character Creator 5: HD yüz profili, Unity aracı resmî listede ≤6000.3 [A02-S031][A02-S061].
- MPFB 2 (Blender): çıktı CC0 [A02-S062].
- Google GNM Head (Apache-2.0 parametrik kafa), Gaussian avatarlar (LAM, yalnız kafa, Unity yok) [A02-S045][A11-S019].
- Algı çalışmaları: üst yüz hareketi önemli [A02-S051]; gerçekçi görünümde kötü hareket daha ürkütücü [A02-S052].

## 4. Hareket
- Mixamo: ücretsiz, ham dosya dağıtımı yasak [A03-S015][A03-S016].
- CC0 paketler (UAL 1–2) [A03-S018][A03-S019]; CMU, 100STYLE (CC BY 4.0).
- Videodan hareket: Rokoko, QuickMagic (bulut, küçük kota; çıktı hakkı belirsiz); FreeMoCap (AGPL); GVHMR ve SMPL tabanlılar ticari değil [A03-S028][A03-S029].
- Metinden harekete: Kimodo, HY-Motion güçlü NVIDIA ister; HY-Motion bölge ve çıktı kısıtlı [A03-S034][A12-S041].
- SIGGRAPH: bozuk mocap temizleme (StableMotion) [A00-S029]; jest üretmek yerine etiketli jest seçme fikri (Sony posteri) [A00-S030].
- Motion matching (MIT proje var) bu iş için şimdilik gerekmiyor [A00-S011; ÇIKARIM].

## 5. Ses, hizalama, dudak
- Gemini 3.8 Flash TTS: Türkçe, duygu stili; zaman damgası belgede yok [A07-S001].
- ElevenLabs: karakter düzeyinde zaman damgası var [A07-S014].
- Türkçe hizalama adayları: MFA 3.0 (≈128 saat veri) [A07-S008], parseh [A07-S010], wav2vec2-tr [A07-S039]. Türkçe doğruluk karşılaştırması yok; İngilizcede MFA en iyi [A07-S009].
- Açık Türkçe sesler hızla artıyor (Chatterbox, VoxCPM2, EMA, FreyaTTS); kalite ve veri hakkı doğrulanmadı (C-008).
- Dudak: OVR LipSync arşivlenmiş [A00-S016]; Audio2Face-3D açık ama NVIDIA ister [A07-S055].

## 6. Otomatik sinematografi ve kurgu (akademik)
| Çalışma | Fikir | Kaynak |
|---|---|---|
| He, Cohen, Salesin 1996 | film kuralları durum makinesi olarak | [A01-S019] |
| Galvane vd. 2015 | atış + geçiş + ritim maliyeti, dinamik programlama (önerilen çekirdek) | [A01-S017][A08-S023] |
| Leake vd. 2017 | diyalog kurgu deyimleri | [A08-S011] |
| FilmAgent 2025, Mind-of-Director 2026 | Unity'de yapay zekâ "yönetmen" ajanları + motorla doğrulama | [A01-S015][A08-S013] |
| FilmFlow, CinemaTraj 2026 | küçük hareket sözlüğü + denetçi döngüsü (ön baskı) | [A01-S016][A01-S038] |
| Micro-Edits 2026 | adayları otomatik bul, kararı kullanıcıya bırak | [A08-S012] |

Sonuç [ÇIKARIM]: büyük dil modeli zorunlu değil; hikâye zaten etiketli, kural + puanlama CPU'da yeter (A01, A08).

## 7. Ortam ve efekt
- CC0: Poly Haven, ambientCG [A05-S017][A05-S020]; Megascans Kasım 2026'da yeniden ücretsiz (Fab Standard) [A05-S006][A12-S017].
- Prosedürel: Infinigen Indoors (BSD; NVIDIA, Windows deneysel) [A05-S015].
- Görselden 3D: TRELLIS.2 (24 GB NVIDIA), Hunyuan3D (bölge kısıtlı) [A05-S026][A05-S027].
- Gaussian splat dünyaları: Unity eklentisi DX12/Vulkan ister [A11-S013].
- Efekt: CC0 Unity Labs flipbook'ları [A06-S007]; VFX Graph compute ister; yapay zekâ video difüzyonlu efekt araştırmaları kullanıcının şartına aykırı [A06-S027].

## 8. Neural rendering ve video yapay zekâsı
- DLSS 5 (Eyl 2026): yalnız RTX 50; Unity desteği duyurusu görülmedi [A11-S001][A11-S032]; Mart tanıtımında yüz değiştirme eleştirisi [A11-S003] (açık çelişki).
- Motor koşullu "gerçekçileştirme" araştırmaları (NeuralSRender, DiffusionRenderer): büyük bellek, insan tutarlılığı kanıtsız [A11-S005][A11-S007].
- Açık video modelleri (LTX, Wan, VACE): güçlü NVIDIA ya da bulut; kullanıcının "video üretici yok" şartıyla çelişir [A11-S008–S010].
- Ayrım önerisi: "Model bizim 3D sahnemiz olmadan da video üretebiliyorsa video üreticisidir" (A11 §S2; kullanıcı karar vermeli, Q-015).

## 9. Platform ve hukuk çerçevesi
TikTok gerçekçi yapay ses/görüntüde etiket istiyor [A12-S022]; okunan resmî sayfalar eski, güncel ayrıntı doğrulanamadı. AB AI Act şeffaflık maddesi 2 Ağu 2026'dan beri [A12-S025, ikincil]. Türkiye'de etiket zorunluluğu taslak [A12-S026].

## 10. Hikâye Stüdyosu için çıkarım
Bugünkü kaliteyi belirleyen büyük işler yeni teknoloji değil, bilinen yöntemlerin uygulanması [ÇIKARIM]. Gelecek teknolojiler izlenmeli (R11) ama bugünkü kusurları ertelemek için gerekçe olmamalı (A11 Riskler).

## 11. Açık kalanlar
MetaHuman'ın uygulama içi kütüphane olarak dağıtımı (Q-011); Türkçe TTS ve hizalama için bağımsız ölçüm; Vega'da URP/EEVEE hızları; Kasım 2026 TL fiyatları; dikey dramada izleyici kaybı verisi; sistematik SIGGRAPH/NeRF taraması.
