# 03 ARAŞTIRMA GÜNLÜĞÜ

## 2026-11-27 — Oturum 1: kurulum
- Konu: protokolün okunması, mevcut proje incelemesi, dosya sistemi, ajan organizasyonu.
- Kaynaklar: 00_PROTOKOL.pdf, Docs/*.md, DURUM.md, README, cihazdan manifest.json, ProjectVersion.txt, donanim.txt.
- Öğrenilen: Cinemachine 6.6.0 projede zaten kurulu (26 Kas raporunda "ekleme için onay gerekir" denmişti → 06_CONTRADICTIONS C-001).
- Kararlar: teknik karar yok. Çalışma düzeni: iki dalga (01 §4).
- Not: protokolden önce yapılan tek kod değişikliği `Editor/KareSerisi.cs` ondalık okuma düzeltmesi (0 hata derlendi) ve `ornek30.txt` deneme hikâyesi. Protokolden sonra kod değiştirilmedi.
- Sonraki: 1. dalga.

## 2026-11-27 — Oturum 1: 1. dalga sonucu
- Ajanlar: A01, A02, A03, A04, A07, A09 (gerçek alt ajanlar). Kaynak: 273 (09_SOURCE_DATABASE).
- Başlıca bulgular (kod bulguları = statik okuma, test edilmedi):
  - HDRP Unity 7'de kaldırılma yolunda → URP'de kalınmalı; eski HDRP önerisi yanlıştı (C-003).
  - 3D sahnede gölge ve HDR kapalı, tonemapping/AO yok (A04, A09; gölge/HDR satırları ana araştırmacıca doğrulandı).
  - Hareket: `dur` konuşma jesti klibine gidebilir, konuşma satırı yürümeyi eziyor, yalnız yerinde duran Rocketbox klipleri alınmış (A03).
  - Altyazı: tahmini kesim doğrulamasız, satır numaralı ses yedeği kayabilir, karaoke süresi harf sayısından (A07).
  - Yüz: viseme/ARKit şekillerinin çoğu eşlenmiyor; ağız büyük olasılıkla yalnız çene (A02, A07 bağımsız aynı sonuç).
  - Kamera: gerçek yakın plan yok, bütün planlar 59–72 mm, kesme satıra bağlı (A01).
- Kısıt: ortak 200 web arama hakkı doldu (R-006).
- Kural sapması: A07 ajanı yasak olmasına rağmen salt okunur `git log`/`git status` çalıştırdı; depo değişmedi (git durumu kontrol edildi).
- Karar: K-003 (HDRP önerisi geri çekildi).
- Sonraki: 2. dalga.

## 2026-11-27 — Oturum 2: 2. dalga, lisans, takip
- A05, A06, A08, A10, A11 (A05 ve A10 oturum sınırında kesildi, dosyaları yazılmış), ardından A12 ve A00 takip. Kaynak toplamı 534.
- Kullanıcı kendi araştırmasını gönderdi (K01).
- Başlıca: örnek videodaki fotogerçek görünüm otomatik/ucuz 3D ile bugün büyük olasılıkla çıkmaz (A11, çıkarım, güven orta); EXE bu hâliyle dağıtılmamalı (A12); Netflix TR altyazı sınırları (A00).
- Unity §2.5 maddesi ayrıca okundu (Q-016).

## 2026-11-27 — Oturum 3: rapor, denetim, düzeltme
- Çapraz denetim (X1, X2), test planı, 13 rapor, mimari ve yol haritası taslağı yazıldı.
- Bağımsız denetçi: tamamlanmadı. Ana bulgu: Unity ana kullanım koşulları ajan erişimini sınırlıyor (C-030, K-004).
- D1: görünüm karşılaştırması, Unity CLI yolu, iClone/FK/kompozit/kamera (34 arama). Test yok, kod değişikliği yok.
- Öz denetim 16'ya yazıldı. Toplam kaynak kaydı 562.

## 2026-11-27 — D2: açık konular
- 4K, düz yazıdan sahne, 2D paket/çizer, upscaling, diğer sektörler, çocuk karakter (35 arama). Test yok, kod değişikliği yok.

## 2026-11-27 — Kullanıcı yanıtları ve D3 (yapay zekâ asistanı)
- K-017..K-020 kaydedildi. D3: asistan seçenekleri, bütçe etkisi (24 arama). Test yok.
