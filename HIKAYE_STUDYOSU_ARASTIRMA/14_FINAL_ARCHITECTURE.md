# 14 NİHAİ MİMARİ (ÖNERİ)

> **TESTSİZ TASLAK.** Yalnız teşhis aşaması (15 numaralı dosya Aşama 1; testler T0) bağlayıcı plandır ve o da kullanıcı onayıyla başlar. Sonrası T0 sonuçlarına göre yeniden yazılır. "URP'de kal" bir karar değil **varsayılan hipotezdir** (04: K-005).

> **Kullanıcı yanıtları (K-017..K-020) işlendi:** önce araştırma; yapay zekâ asistanı olacak (yeri belirsiz); gerçekçiye yakın 3D ve stilize 3D birlikte denenecek; bütçe yalnız ek disk (ekran kartı yok). Ayrıntı §8-§11.

Tarih: 2026-11-27. **Durum: ÖNERİ. Test edilmedi. Geliştirmeye geçiş kullanıcının ayrı kararıdır** (protokol §19). Testler yapıldıkça bu dosya değişebilir; eski karar silinmez, yenisi eklenir.
Dayanak: A09 "Seçenek 0 ve 1", A01, A02, A03, A04, A05, A06, A07, A08, A12; denetimler X1, X2. Ayrıntıların çoğu ÇIKARIM'dır.

## 1. Temel kararlar (öneri)
1. **Unity + URP'de kal (varsayılan hipotez).** HDRP kaldırılacak [A04-S002]; UE5/Blender'a tam geçiş büyük yeniden yazım ve donanım ister (A09 §S9).
2. **Hikâye metni tek doğruluk kaynağı olarak kalsın** (A09 §A.2).
3. **Önce kararlar, sonra çekim:** kayıttan önce her şey hesaplanır; oynatıcı yalnız uygular (A08).
4. **Kimseye bağımlı olmayan arayüzler:** ses, hizalama, yüz, hareket, kamera, render, kare işleyici (A07, A11, X1 G3).
5. Yapay zekâ **video üreticisi** mimaride yok. Metin asistanı olacak (K-018; §8). Yerel yardımcı modeller (TTS, hizalama) ve görüntü iyileştirme kullanıcı kararına bağlı (Q-015).

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

**4.6 Render ve kayıt.** Profiller: Önizleme (hızlı), Kayıt (yüksek), Ağır (kart gerektirir; **bütçe dışı**, K-020; yalnız yer bırakılır). HDR hattı + alfa çözümü (C-024); kalıcı Volume Profile (EXE'de efekt silinmesin); kare atlamada yerine kopya; takılma bekçisi; disk ön kontrolü; iptalde düzgün kapatma; yarıda kalmaya dayanıklı ara dosya; iki kayıt yolunun teke indirilmesi; sürüm sabitleme (ffmpeg, paketler). Kare dizisi diskte tutulmaz.

**4.7 Geleceğe açıklık (yalnız yer bırakılır).** Ek geçişler (derinlik, maske, hareket vektörü) ve `IKareIslemci`; akış ya da parça halinde, işlenip silinen (X1 G1, G2). Belirlenimcilik hedeflenir ama GPU/fizik için kare birebirliği garanti sayılmaz (X1 G6).

**4.8 Dağıtım profili.** Uygulama başkasına verilecekse: Mixamo karakter **ve animasyonları** çıkar (katalog + oyuncu listesi + hazır prefablar; 36 karakter; C-021), yalnız MIT/CC0 (+teyitli Fab), bildirim ekranı, FFmpeg yükümlülükleri (A12; X2 L1–L3).

## 5. Sürüm
Proje 6000.6 (uzun destekli değil). 6.7 LTS çıkınca ayrı kopyada referans kare testiyle geçiş; Unity 7'ye acele yok (A09 §S4). Gerilim: karakter araçlarının listeleri daha eski sürümlerde; yükseltme onları daha da uzaklaştırabilir (X1 U3, C-028, çözülemedi). DX11→DX12 kararı ancak T0-12 testinden sonra.

## 6. Mevcut koddan geçiş ilkesi
Büyük sınıflar (StoryPlayer ~1600 satır) bir anda yeniden yazılmaz. Önce çekim listesi **mevcut davranışı aynen kaydeder**; referans kareler aynı kalınca yürütme ona taşınır (A09 Seçenek 1).

## 7. Bu mimarinin geçerli olma koşulları
URP yol haritasının sürmesi · testlerde gölge/HDR'nin kabul edilebilir sürede çalışması · kullanıcının "video üretici yapay zekâ yok" şartının korunması. Biri değişirse karar kaydı güncellenir.

## 8. Yapay zekâ asistanı (K-018, K-023; ÖNERİ, denenmedi)
- Görev: yalnız **taslak metin** (hikâye taslağı, düz yazıdan etiket, kamera önerisi, tek satır değiştirme, kontrol önerisi). Görüntü/video üretmez (ana araştırmacının yorumu; kullanıcı onaylamadı).
- Asıl hikâyenin üstüne kendiliğinden yazmaz. Doğrulayıcının `satir/kod/mesaj/oneri` çıktısı en çok 2-3 tur geri verilir. Kullanıcı farkı görüp onaylar; ses üretimi (kota) onaydan sonra.
- **Unity'de eylem yapmaz** (doğrula-kur-kaydet komutu vermez): D3 §5'teki durum 2 (yalnız metin) durum 3'e (Unity'yi sürme) kaymasın. Durum 2 için bile Unity metni belirsiz (R-044).
- Sağlayıcı arayüzü: Gemini / yerel model / dışarıdan (metin dosyası). Ücretli API bütçe dışı.
- Bugün (statik kod okuması, D3 §1): `YardimciGemini.cs` iki düğme (öneri al, biçime çevir); istemde hareket, kamera, efekt listesi ve sözdizimi yok; otomatik doğrula-düzelt döngüsü yok. Eklemek kod değişikliğidir.
- Gizlilik: ücretsiz Gemini'de metin ürün geliştirmede kullanılır, insanlar okuyabilir; 18+ ve kullanıcı kitlesi koşulu (Q-022). Anahtar şifreli saklanır, Git'e ve uygulamaya gömülmez; Editor tarafı `EditorPrefs`'ten düz okuyor görünüyor (ÇIKARIM).
- Claude aboneliği uygulamanın içine konamaz. Aynı Anthropic sayfası, son kullanıcının değiştirilmemiş Claude Code'a kendi aboneliğiyle girmesini engellemediğini de yazıyor (üründe çalıştırma Ticari Şartlar ve koşullarla) (DEN2 O5).
- **Açık soru:** içeride mi dışarıda mı (Q-020). Model fiyatı/ücretsiz katman bilgileri doğrulanmadı (Q-023).

## 9. İki görünüm profili (K-019, K-024)
| Profil | İçerik | Not |
|---|---|---|
| Gerçekçiye yakın | mevcut Rocketbox hattı + gölge, HDR/ton, SSAO, plana göre ışık | fotogerçek beklenmez |
| Stilize | VRoid + MToon + boyama tarzı mekân | aynı hareket, kamera, ses hattı; ayar emeği ve sonuç bilinmiyor |
Görünüm profili veridir (malzeme, ışık, renk, karakter kümesi); hikâye ve çekim listesi ortaktır. Seçim G-1 denemesinden sonra. 2,5D ana yol değil.

## 10. 4K ve düz yazıdan sahne (D2 §1-§2)
- **Varsayılan 1080x1920.** 4K (2160x3840) yalnız ölçümden (4K-1) sonra, isteğe bağlı. `h264_amf`'in dikey 4K'yı kabul ettiği doğrulanmadı. Boyutlar kodda sabit (`Editor/StudyoKayit.cs:168,286-287`, `Uygulama/KayitBootstrap.cs:73,108,220-227`); bit hızı ayarı `Uygulama/FfmpegPipeRecorder.cs:82` (12 Mbps). 2D görsel ve yazı 4K'ya uygun boyutta olmalı. TikTok'un izleyiciye 4K gösterdiğine dair resmî bilgi yok.
- **Düz yazı:** sıfırdan yazılmaz; mevcut `DuzMetniCevir` (`Uygulama/YardimciMotor.cs`) genişletilir. Her tahminde güven işareti; belirsizde açılır liste ("Bunu kim söylüyor?"); kullanıcı düzeltmesi ad sözlüğüne kaydedilir. "Hatasız otomatik" sözü verilmez. Asistanla ilişkisi: yapay zekâsız çevirici her zaman çalışan temel yoldur; asistan isteğe bağlı ikinci taslak kaynağıdır; ikisi de aynı doğrulayıcı ve onaydan geçer.

## 11. Çocuk karakter kuralları (D2 §6; ÇIKARIM)
Kıyafet her açıdan denetlenir; çocukta alçaktan açı, yakın beden planı, yatak/kıyafet değiştirme sahnesi kapalı; şiddet/tehlike sınırlı; gerçek çocuk yüzü ya da sesi yok. Yayından önce TikTok kuralı elle okunur (R-045). İlk adım: projedeki Rocketbox çocukları sayılıp önizlenir. Çocuk sesi araştırılmadı.

## 12. Köprü komutları
§6 ve R08'deki "mevcut komutlar korunur" ifadesi kodun korunması içindir. **Yapay zekâ ajanının bu köprüyü kullanması Q-016'ya bağlıdır; bugün kapalı (K-004).**
