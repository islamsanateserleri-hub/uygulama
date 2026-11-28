# R08 — Nihai teknik mimari (ÖNERİ, sade anlatım)

> **TESTSİZ TASLAK.** Yalnız teşhis aşaması (15 numaralı dosya Aşama 1; testler T0) bağlayıcı plandır ve o da kullanıcı onayıyla başlar. Sonrası T0 sonuçlarına göre yeniden yazılır. "URP'de kal" bir karar değil **varsayılan hipotezdir** (04: K-005).

Tarih: 2026-11-27. **Test edilmedi.** Protokol bu raporu "araştırma ve testler sonucunda" ister; testler henüz yapılmadığı için bu bir **taslak öneridir**. Teknik ayrıntı: `../14_FINAL_ARCHITECTURE.md`.

**Yanıtlarınız (K-017..K-020):** bütçe yalnız ek disk, ekran kartı yok; iki görünüm denenecek; yapay zekâ asistanı olacak (içeride mi dışarıda mı belirsiz). Aşağıdaki "büyük sahne" hedefleri (Aşama 6-7) tümleşik kartta risklidir.

## 1. Tek cümle
Mevcut uygulama atılmaz; Unity URP üzerinde, "önce her şeyi planla, sonra çek" düzenine adım adım geçirilir.

## 2. Neden bu yol
| Seçenek | Neden seçilmedi / seçildi |
|---|---|
| URP'de geliştirmek | kod korunur, bu bilgisayarda çalışır, Unity'nin ana yolu → **varsayılan hipotez** |
| HDRP | Unity kaldıracağını açıkladı [A04-S002] |
| Unreal 5 | yeniden yazım, ekran kartı ve disk gerekir (A09, A10) |
| Blender'a taşımak | bu bilgisayarda çok yavaş, büyük aktarım işi |
| Yapay zekâ video üreticisi | kullanıcının şartına aykırı |

## 3. Altı yapı taşı
1. **Ses deposu:** her satırın sesi ve gerçek kelime zamanları; denetlenmemiş ses kayda giremez.
2. **Çekim listesi:** her plan (kim, nereden, kaç saniye, hangi ışık, hangi efekt). Kullanıcı görür, değiştirir, kilitler.
3. **Oyuncu sistemi:** açık hareket listesi, yüz eşlemesi, karakter kartı.
4. **Mekân ve efekt tarifleri:** veri dosyaları; yeni mekân için kod değişmez.
5. **Kayıt:** hızlı önizleme ve yüksek kalite profilleri; parça parça, yarıda kalırsa kaldığı yerden.
6. **Denetçi ve kayıt defteri:** hata varsa kayıt başlamaz; her varlığın lisansı yazılı.

**Eklenenler (14 §8-§11):** metin asistanı (yalnız taslak; onay sizde; Unity'de eylem yapmaz) · iki görünüm profili · varsayılan 1080x1920, 4K yalnız ölçümden sonra · mevcut düz yazı çeviricisinin güven işaretiyle genişletilmesi · çocuk karakter kıyafet/kamera kuralları.

## 4. Korunanlar
Hikâye sözdizimi ve doğrulayıcı · köprü komutları (kod korunur; **yapay zekâ ajanının kullanması Q-016'ya bağlı, bugün kapalı**) · mevcut ffmpeg ile kayıt yapısı (iyi temel, A09 §A.2) · adlı mekân noktaları · yüz canlılığı (kırpma, bakış) · mevcut `vfx:` ve `kamera:` komutları.

## 5. Çözülmemiş mimari sorular
Satır değişince düzenlemelerin korunması · parçalı kayıtta kesintisiz ses · planlama anında oyuncu konumu · yeni karakter araçlarının bu Unity sürümüyle uyumu · HDR ile saydam bindirme. Her biri için küçük prototip gerekir (12_TEST_PLAN).

## 6. Durum
Geliştirmeye başlanmadı. Başlamak için R13'teki ön koşullar ve kullanıcının ayrı onayı gerekir.
