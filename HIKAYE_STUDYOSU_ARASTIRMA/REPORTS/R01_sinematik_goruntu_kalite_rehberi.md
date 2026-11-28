# R01 — Sinematik görüntü kalite rehberi

Tarih: 2026-11-27. **Hiçbir deneysel test yapılmadı.** Buradaki sayıların çoğu başlangıç değeridir.
Anahtar: [KOD] statik kod okuması · [KAYNAK] belge/yayın · [ÇIKARIM] yorum · [TEST EDİLMEDİ].

## 1. Temel fikir
Sinematik görüntü tek bir ayar değildir. Birlikte çalışması gereken katmanlar [ÇIKARIM; A01, A04, A08]:

1. **Doğru hareket ve doğru ses** (bunlar bozuksa güzel ışık işe yaramaz; gerçekçilik artınca kötü hareket daha rahatsız edici bulunuyor [A02-S052]).
2. **Plan başına hazırlık:** her plan için mercek, ışık, oyuncu yeri ve odak ayrı kurulur. Kayıt kare kare (çevrim dışı) olduğu için bunu yapabiliriz (A01 §0.5).
3. **Işık ve renk** > mercek/kompozisyon > alan derinliği > hareket bulanıklığı > gren/vinyet (A01 §S8, ÇIKARIM).
4. **Kurgu ve ritim.**
5. **Karakter, ortam, efekt kalitesi** (tavanı bunlar belirler).

## 2. Bugün neden sinematik görünmüyor [KOD, TEST EDİLMEDİ]
- 3D sahnede gölge yok, kamera HDR kapalı, tonemapping yok (A04 K1–K5; A09 K1–K2; X1 D4, X2 D12 doğruladı). Ortam gölgelemesi (AO/SSAO) kodda açılmıyor; bu bir proje (renderer) ayarı olduğu ve URP ayarları depoda olmadığı için açık olup olmadığı **bilinmiyor** (Q-008).
- Kodla kurulan 3D mekânlar dokusuz küp/silindir/küre (katalog eşyaları bu tespitin dışında); metal ile ahşap aynı görünür (A05 M1–M2).
- Işık dünyaya sabit; plana göre değişmiyor, kenar ve göz ışığı yok (A04 K12).
- Gerçek yakın plan yok: "yakın" planda baş kadrajın ~%22'si (bel planı gibi); bütün planlar yaklaşık 59–72 mm (A01 §S12; X1 D10).
- Plan süresi neredeyse replik süresi kadar (A01 K3; A08 E1–E2).
- Açılışta ~2 sn siyah ekran (A08 E3; X2 D9).
- Efektlerin çoğu ekran üstüne çizilen 2D resimler (A06 §S1).
- Ağız büyük olasılıkla yalnız çene açıp kapatıyor (A02, A07; X2 D7: model tarafı doğrulanamadı).
- Gözlem (D001): düz tuğla duvar, tek turuncu ton, oyuncak gibi ejderha, ekran üstü yıldızlar.

## 3. Kamera ve kadraj
**Mercek ve uzaklık** (A01 §S2; ÇIKARIM): "dikey tam kare" kabulünde dikey açı = 2·atan(18/f), uzaklık d = H·f/36 (H: kadraj yüksekliği, m). Yaklaşık 60 cm'den çekilen yüz, ~2,1 m'den çekilene göre daha olumsuz algılanıyor [A01-S033]; bu yüzden uzun mercek + biraz uzaktan.

| Plan | H (m) | mercek | uzaklık | kullanım |
|---|---|---|---|---|
| Aşırı yakın (ECU) | 0,22–0,30 | **135 mm denenmeli** | ≈1 m | göz, güç anı |
| Yakın (CU) | 0,38–0,48 | 100 mm | ~1,2 m | doruk, ağlama, korku |
| Göğüs (MCU) | 0,60–0,70 | 85 mm | ~1,5 m | diyalog varsayılanı |
| Omuz üstü | ~0,8 | 65 mm | ~1,45 m | karşılıklı konuşma |
| Boydan | 2,0–2,2 | 40 mm | ~2,3 m | yürüme, giriş |
| Geniş / çok geniş | 3,5–8+ | 28–24 mm | 3–6+ m | mekân, ejderha, ordu |

Not: A01'in ilk tablosu ECU için 100 mm/0,7 m diyordu; kendi "1 m altı yok" kuralıyla çeliştiği için düzeltme önerildi (X2 G1; C-022 açık, testte ayarlanacak). Kamera uzak sınırı bugün 60 m; çok geniş planlar bunu aşarsa kesilir (X1 §0.3). İç mekân 5,2 m olduğu için çok geniş planlar büyük set ister (A01, A05).

**Kadraj kuralları** (A01 §2; ÇIKARIM): iki gözün ortası hedef alınır; gözler üstten %28–42; yüzün baktığı yönde boşluk; normal diyalog göz hizası, güçlü karakter hafif alttan, zayıf hafif üstten; yüz 3/4; iki kişi yan yana değil derinlikte; kamera sürekli kıpırdamaz (küçük ölü bölge).

**Süreklilik:** iki kişi arasındaki eksenin aynı tarafında kal (180° kuralı) [A01-S023]; aynı kişiye art arda planlarda ≥30° açı farkı ya da belirgin ölçek farkı [A01-S021]. Not: ikinci kuralın sayısal eşiği kendi plan sözlüğüyle zorlanabiliyor (X1 S2); testte ayarlanmalı.

**Yakın planın şartı:** yüz gerçekten bir şey anlatmalı. Yakın plan yalnız üzgün ifadede etkiyi artırdı, ifadesizde artırmadı [A01-S034][A08-S018]. Karakter başına "en yakın izinli plan" kuralı önerilir (X2 G3).

## 4. Dikey 9:16
- Yakın ve orta plan ağırlık; geniş plan seyrek ama gösteri ve coğrafya için gerekli (A01 Ç1, C-006; kaynaklar zayıf: C/D).
- Genişlik değil derinlik ve yükseklik: 4 katman (ön plan silueti → oyuncu → orta → uzak) (A05 §S7).
- Güvenli alan: reklam ölçümü alttan %34 [A01-S010], uygulama %20. Organik video için doğrulanmış sayı yok; telefonda denenmeli (C-007, C-029). %34 doğruysa altyazıya yer çok daralır (X2 G8).

## 5. Işık ve renk
Üç katman (A04 §S4; ÇIKARIM): (1) mekân ışığı (pencere, meşale; 1–2'si gölgeli) + ortam rengi + yansıma; (2) plana göre karakter ışığı: ana (key), dolgu (key'in ~%25–40'ı), kenar, göz parlaması; (3) renk görünümü (tonemapping + LUT). Sıcak ana ışık + soğuk dolgu karşıtlığı; oranlar bir eğitim yazısından (C) başlangıç değeri [A04-S016].

Sıra önerisi (donanımsız): gölge → HDR + tonemapping → SSAO → plana göre ışık → yansıma/ortam → PBR doku (A04 §Kademe 0). HDR "tek satır" iş değil: 3D görüntü saydam zeminle 2D üstüne bindiriliyor; ek ayar (Alpha Processing, 64-bit) gerekebilir (X1 §0.2, Ç-A; [A06-S008]).

## 6. Alan derinliği ve film karakteri
- URP'de Gaussian yalnız uzağı bulanıklaştırır; ön plandaki omuz için Bokeh gerekir [A01-S001].
- Bugün EXE derlemesinde alan derinliği shader'ı atılıyor (proje günlüğü gözlemi; A09 K3, X1 D14). Bu çözülmeden sığ alan derinliğine dayanan plan dili kurulmamalı (X2 G4).
- Yakın planda yüzün yarısı bulanık olmasın: CU/ECU için f/5,6–8 ile başla (A01 hesabı). ECU'da f/8'de bile net alan çok dar olabilir (X2 G2).
- Hareket bulanıklığı, gren, vinyet az tutulur; yoğun gren TikTok sıkıştırmasında bozulur [A04 ÇIKARIM].

## 7. Kurgu ve ritim
- Dikey dramada ölçülmüş ortalama plan süresi: 2,31 sn [A08-S001] (yalnız özet okundu) ve ≈4 sn [A08-S002]. Öneri: medyan 2,5–3 sn, tekdüze değil (C-012).
- İlk karede yüz ya da hareket olsun; kanca yazısı görüntünün üstüne (A08 R5). [A08-S004] ilk 3 sn önerisi yalnız reklam içindir.
- Kesme kelime ortasına düşmesin; hareket üstünde kesme daha az fark edilir [A08-S010] (7 kişilik küçük çalışma).
- Boşluklar sabit olmasın: itiraf öncesi uzun, kavgada kısa (A08 §4; ÇIKARIM). Not: A08'in "ses sessizliği boşluğa ekleniyor" iddiası yanlıştır; kod sessizliği zaten kırpıyor (C-020). Kalan sorun boşluğun sabit olmasıdır.
- Her bölüm küçük bir soruyu kapatıp büyüğünü açsın; yalnız yarıda kesmek devam niyetini artırmayabilir [A08-S008].

## 8. Ses ve altyazı
Altyazı zamanı gerçek sesten gelmeli (A07). Netflix Türkçe: en çok 2 satır, ≤17 karakter/sn [A00-S001]. Karaoke'nin daha iyi anlaşıldığına dair bağımsız kanıt bulunamadı (A00 §S1). Ses düzeyi −14 LUFS bir tercihtir, TikTok kuralı değildir [A07-S063].

## 9. Oyun sinematiği ile sinema farkı ve "gerçek film" sorusu
- Fark yalnız render değil: tek ışık düzeni, tek mercek, planlar için yerleştirilmeyen oyuncu, derinliksiz arka plan, replik dışında iş yapmayan karakter (A01 §S10; ÇIKARIM).
- İyi oyun sinematiği 3D yolla hedeflenebilir. Örnek videodaki fotogerçek insan filmi görünümünün otomatik ve ucuz biçimde 3D'den çıktığına dair kanıt bulunamadı; bugün büyük olasılıkla çıkmaz (A11 Q-004; ÇIKARIM, güven orta, test yok).

## 10. Kontrol listesi (her video için)
Ses-altyazı uyumu · yürüme kaymıyor · susan karakter sakin · ilk karede görüntü · yüz güvenli alanda · zemin teması/gölge · karakter arka plandan ayrılıyor · aynı kadraja kesme yok · tepki planında yüz gerçekten değişiyor · sesli izlenip onaylandı.
