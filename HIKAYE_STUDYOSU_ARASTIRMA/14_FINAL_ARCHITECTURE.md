# 14 NİHAİ MİMARİ (ÖNERİ)

Tarih: 2026-11-27. **Durum: ÖNERİ. Test edilmedi. Geliştirmeye geçiş kullanıcının ayrı kararıdır** (protokol §19). Testler yapıldıkça bu dosya değişebilir; eski karar silinmez, yenisi eklenir.
Dayanak: A09 "Seçenek 0 ve 1", A01, A02, A03, A04, A05, A06, A07, A08, A12; denetimler X1, X2. Ayrıntıların çoğu ÇIKARIM'dır.

## 1. Temel kararlar (öneri)
1. **Unity + URP'de kal.** HDRP kaldırılacak [A04-S002]; UE5/Blender'a tam geçiş büyük yeniden yazım ve donanım ister (A09 §S9).
2. **Hikâye metni tek doğruluk kaynağı olarak kalsın** (A09 §A.2).
3. **Önce kararlar, sonra çekim:** kayıttan önce her şey hesaplanır; oynatıcı yalnız uygular (A08).
4. **Kimseye bağımlı olmayan arayüzler:** ses, hizalama, yüz, hareket, kamera, render, kare işleyici (A07, A11, X1 G3).
5. Yapay zekâ **video üreticisi** mimaride yok. Yerel yardımcı modeller (TTS, hizalama) kullanıcı kararına bağlı (Q-015).

## 2. Büyük resim
```
Hikâye metni (.txt)
  → Ayrıştırıcı + doğrulama (mevcut)
  → [1] SES AŞAMASI: satır sesi → denetim kapısı → hizalama → ses varlık dosyası
  → [2] PLANLAMA: vuruşlar → hareket planı → konum çizelgesi → plan adayları → kurgu
  → [3] ÇEKİM LİSTESİ (kurgu dosyası)  ← kullanıcı düzenler ve kilitler
  → [4] DENETÇİ: hata varsa kayıt başlamaz
  → [5] YÜRÜTÜCÜLER: sahne · oyuncu · yüz · kamera · ışık · efekt · altyazı
  → [6] KAYIT: render profili → parça parça görüntü + ayrı ses miksi → birleştirme
  → [7] KALİTE RAPORU + render kaydı (manifest)
```

## 3. Tek veri modeli (X2 §2.1; C-027)
Ajanlar ayrı ayrı 5–6 şema önermişti (X1 §0.5). Öneri: **üç katman**, diğer her şey kimlikle bağlanan katalog.

| Dosya | İçerik | Zaman birimi |
|---|---|---|
| Ses varlığı `ses/<ClipKey>.json` (değişmez) | wav özeti, metin özeti, sağlayıcı/stil, süre, kelime/fonem zamanları, güven, onay | saniye, **son kırpılmış sese göre** |
| `kurgu.json` (= çekim listesi) | izler (diyalog, görüntü, oyuncu, altyazı, müzik, efekt, kart), çapa, kilit, tohum, ışık ön ayarı, şema sürümü | 30 fps tam kare, tek yuvarlama kuralı |
| `render_manifest.json` | girdi özetleri, ayarlar, parçalar, sonuç | dosya/parça |

Kataloglar: karakter kartı, hareket kataloğu (klip adı, hız, kök hareketi, olaylar, lisans), mekân verisi, efekt tarifi, plan tarifi, **tek varlık manifesti** (kaynak, lisans, tarih, yapay zekâ üretimi mi, dağıtılabilir mi).

Adlandırma: `plan:` komutu dolu; yeni komutlarda `cekim:` (X1 D12).

**Henüz çözülmemiş tasarım noktaları:**
- Satır kimliği: metin değişince kullanıcının düzenlemesi nasıl korunur (X2 S1). Algoritma tasarlanmalı.
- Parça parça render ile kesintisiz müzik/J-L kesme (C-025): öneri ses tek iz olarak ayrıca mikslensin; **prototip gerekir**.
- Planlama anında oyuncu konumu bilinmiyor (X2 S13): sessiz ön simülasyon ya da konum çizelgesi.

## 4. Alt sistemler
**4.1 Ses ve altyazı.** Satır başına üretim; tahminle bölme kaldırılır. Ses bir kez işlenir (kırpma, duygusal iç duraklama korunabilir), sonra hizalanır. Eski satır numaralı dosyalar için **geçiş planı** (dinle-onayla, yeni ada taşı, sonra yedeği kapat) şart, yoksa hepsi "eksik" olur (X2 S4). Ses düzeyi duyguya göre; her repliği aynı güce getirmek fısıltıyı bağırışa yaklaştırır (X2 S3). En az iki ses kaynağı ve oyuncu başına ağız izi (X2 S5). Güven düşükse karaoke kapanır.

**4.2 Karakter ve yüz.** Bağdaştırıcı katman (X1 U1): "karakter yüzü" arayüzü + karakter kartı (şekil eşlemesi, çene yolu tek, göz kemiği, malzeme ailesi, içe aktarma profili, en yakın izinli plan). Kurulum shader'ı açıkça atamalı; bugün mevcut malzemede yeniden atanmıyor (X1 U6). Yeni ağır karakter eklenmeden önce tüm karakterleri belleğe çeken liste yapısı çözülmeli (X1 U5; A09 K6).

**4.3 Hareket.** Planlayıcı (ayakta/yürüyor/oturuyor…) → açık katalog → katmanlı oynatma (taban + üst gövde + ek) → ayak/bakış/el düzeltmesi → sayısal hareket denetçisi (A03).

**4.4 Kamera ve kurgu.** Plan sözlüğü + mercek tablosu; vuruş analizi; sert kurallar (180°, aynı kadraja kesme yok, kelime ortasında kesme yok, yüz görünür) + puanlama; dinamik programlama **sonraki aşama** (hareket, yüz, alan derinliği ve sahneleme hazır olmadan görsel kazanç garantisi yok, X2 §4). Uygulayıcı: önce mevcut kamera kodu; Cinemachine tek sahnede denenir. Efekt yönetmeni plan listesini okur, süreyi sonradan değiştirmez (X1 S7).

**4.5 Mekân, ışık, efekt.** "Mekân = veri" (kit, malzeme, noktalar, kamera hacmi, gizlenebilir duvar, uzak katman, uzak kırpma mesafesi, tohum, durum). Üç ışık katmanı (mekân, karakter, renk görünümü). Efekt tarifi + üç arka uç (2D / parçacık+shader / VFX Graph); mevcut `vfx:` adları korunur.

**4.6 Render ve kayıt.** Profiller: Önizleme (hızlı), Kayıt (yüksek), Ağır (ileride kart). HDR hattı + alfa çözümü (C-024); kalıcı Volume Profile (EXE'de efekt silinmesin); kare atlamada yerine kopya; takılma bekçisi; disk ön kontrolü; iptalde düzgün kapatma; yarıda kalmaya dayanıklı ara dosya; iki kayıt yolunun teke indirilmesi; sürüm sabitleme (ffmpeg, paketler). Kare dizisi diskte tutulmaz.

**4.7 Geleceğe açıklık (yalnız yer bırakılır).** Ek geçişler (derinlik, maske, hareket vektörü) ve `IKareIslemci`; akış ya da parça halinde, işlenip silinen (X1 G1, G2). Belirlenimcilik hedeflenir ama GPU/fizik için kare birebirliği garanti sayılmaz (X1 G6).

**4.8 Dağıtım profili.** Uygulama başkasına verilecekse: Mixamo karakter **ve animasyonları** çıkar (katalog + oyuncu listesi + hazır prefablar; 36 karakter; C-021), yalnız MIT/CC0 (+teyitli Fab), bildirim ekranı, FFmpeg yükümlülükleri (A12; X2 L1–L3).

## 5. Sürüm
Proje 6000.6 (uzun destekli değil). 6.7 LTS çıkınca ayrı kopyada referans kare testiyle geçiş; Unity 7'ye acele yok (A09 §S4). Gerilim: karakter araçlarının listeleri daha eski sürümlerde; yükseltme onları daha da uzaklaştırabilir (X1 U3, C-028, çözülemedi). DX11→DX12 kararı ancak T0-12 testinden sonra.

## 6. Mevcut koddan geçiş ilkesi
Büyük sınıflar (StoryPlayer ~1600 satır) bir anda yeniden yazılmaz. Önce çekim listesi **mevcut davranışı aynen kaydeder**; referans kareler aynı kalınca yürütme ona taşınır (A09 Seçenek 1).

## 7. Bu mimarinin geçerli olma koşulları
URP yol haritasının sürmesi · testlerde gölge/HDR'nin kabul edilebilir sürede çalışması · kullanıcının "video üretici yapay zekâ yok" şartının korunması. Biri değişirse karar kaydı güncellenir.
