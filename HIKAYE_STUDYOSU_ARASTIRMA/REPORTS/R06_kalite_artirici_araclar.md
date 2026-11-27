# R06 — Kalite artırıcı araçlar ve kullanım yöntemleri

Tarih: 2026-11-27. **Hiçbiri denenmedi.** Etki sütunu ÇIKARIM'dır. Amaç eklenti yığmak değil, gerçekten fark yaratanı seçmek (protokol §13).

## 1. En yüksek öncelik: eldekini doğru kullanmak (ücretsiz, indirmesiz)
| # | İş | Kanıt [KOD] | Beklenen etki | Kaynak |
|---|---|---|---|---|
| 1 | Susan karakterin konuşma jesti oynatması | "dur" ilk uyan klibe gidiyor (koşullu) | yüksek | A03 H1, X2 D1 |
| 2 | Satır içi yürümenin ezilmesi; yerinde `yuru` etkisizliği | — | yüksek | A03 H2–H3, X2 D2–D3 |
| 3 | Ses kapısı: satır numaralı yedek ses, içerik denetimi yok, karaoke süresi harf sayısından | — | yüksek | A07 K2, K3, K5; X2 D4–D5 |
| 4 | Rocketbox yüz şekilleri: 52 ARKit'in 18'i, viseme'lerin neredeyse hiçbiri kullanılmıyor | model tarafı doğrulanamadı | yüksek | A02, A07, X1 D1 |
| 5 | Gölge, HDR + tonemapping, SSAO | kapalı/yok | çok yüksek | A04, A09 |
| 6 | Plana göre ışık (ana/dolgu/kenar/göz) | ışık sabit | yüksek | A04 |
| 7 | Göz hizası kadraj, bakış boşluğu, ölü bölge, gereksiz 2D yakınlaştırmanın kapatılması | — | orta-yüksek | A01 K5–K11, X2 D11 |
| 8 | Açılışta görüntü üstünde kanca; duruma göre boşluk | 0,35 + 1,7 sn siyah | orta-yüksek | A08, X2 D9 |
| 9 | 205 hazır CC0 eşyayı mekâna bağlamak (Türkçe arama, İngilizce dosya) | — | orta; stil uyumu bakılmalı | A05 M7, X1 D8 |

## 2. Alan alan araçlar
| Alan (§13) | Araç / yöntem | Nasıl kullanılır | Para / izin |
|---|---|---|---|
| Gerçekçi karakter | Rocketbox (şimdi); MPFB (özgün karakter gerekirse); CC5/MetaHuman (kart alınırsa tek karakterle yan yana) | karakter kartı: model, kıyafet, ses, boy, yüz eşlemesi | 0 / ücretli sonra |
| Yüz ifadesi | ARKit 52 + 15 viseme ara katmanı; duygu tablosu veri dosyasında; hafif asimetri, konuşma öncesi hazırlık | tohumlu rastgelelik | 0 |
| Göz | göz parlaması, paralaks göz shader'ı | önce basit "göz ışığı" | 0 |
| Cilt | yumuşak ana ışık, cilde ayrı pürüzlülük, pre-integrated cilt denemesi | URP'de hazır SSS yok; özel kod gerekebilir (X1 U8) | 0 / mağaza gerekirse |
| Saç | alpha-to-coverage / süper örnekleme; sonra saç shader'ı | "tak-çalıştır, serbest" URP üçlüsü bulunamadı (A00 §S4) | 0 |
| Hareket | Rocketbox yürüme/koşma + UAL; açık "hareket kataloğu" (ada göre tahmin yok), üst gövde katmanı, ayak kayması ölçümü | A03 Sistem v2 | 0 |
| Motion capture | boşluk kalırsa videodan hareket + temizlik | çıktı hakkı kontrol | 0–20 $/ay, sonra |
| Kamera | plan sözlüğü (ECU/CU/detay/geniş), mercek tablosu, maliyet + dinamik programlama | düzenlenebilir storyboard | 0 |
| Işık / shader | URP gölge, SSAO, reflection probe, kalıcı Volume Profile, Shader Graph | EXE'de de denenmeli (X1 U7) | 0 |
| Atmosfer | mesafe sisi, yumuşak sis kartları, ışık huzmesi; isteğe bağlı MIT volumetric paketi | — | 0; paket onayı |
| VFX | Shuriken + CC0 flipbook + her parlayan efektte gerçek ışık; prosedürel şimşek; önceden parçalanmış yıkım | "plan başına 1 büyük efekt" | 0; indirme onayı |
| Animasyon (nesne/yaratık) | iskeletli ejderha + yol + gölge | uygun model doğrulanamadı | olası ücret |
| Ses | satır başına TTS; denetim kapısı; hizalama; ayrı ses izleri; iki geçişli loudnorm (mevcut öğrenen kazanç kapatılarak, X2 S8) | — | 0 / küçük |
| Render | 3D görüntüyü büyük çizip küçültme; yüksek kalite "kayıt profili" ve hızlı "önizleme profili"; kare atlama ve takılma koruması | iki ayarı birlikte ölçmeden açma | 0 |
| Post | ACES/Neutral karşılaştırması, LUT, az bloom/vinyet/gren | — | 0 |
| Büyütme/yeniden oluşturma | kayıtta gerek yok; yapay zekâ kare iyileştirme (DLSS 5 vb.) kullanıcı kararına bağlı | Q-015 | kart |
| Prosedürel üretim | tohumlu modüler mekân kitleri | "mekân = veri" | 0 |
| Karakter araçları | Faceit (başka modele yüz şekli) | gerekirse | 78–99 $ |
| Ortam araçları | CC0 PBR; Megascans; Blender ile kit üretimi | A05 | 0; hesap/indirme |

## 3. Bunları birlikte tutan sistem (R08 ayrıntısı)
- Tek **çekim listesi** (plan, süre, kamera, oyuncu, ışık, efekt, altyazı zamanı, tohum).
- Tek **varlık manifesti** (kaynak, lisans, sürüm, "dağıtılabilir mi").
- Her aracın "kabul testi": yüz + gövde + malzeme + EXE (X1 §8.1).

## 4. Almamayı önerdiklerimiz
HDRP · OVR LipSync · Wwise/FMOD · Synty · NC lisanslı modeller · HY-Motion/Hunyuan · şimdilik motion matching, Gaussian splat ana yol olarak, yapay zekâ video-to-video.

## 5. Dürüst sınır
Bu araçlarla hedef "iyi bir oyun sinematiği"dir. Ekran kartı efekt sayısını artırır; ejderha modelini, animasyonu ve sanat ayarını kendiliğinden iyileştirmez (A06).
