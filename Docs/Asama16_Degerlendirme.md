# Aşama 16: Gelişmiş yapay zekâ üretimi — maliyet ve lisans değerlendirmesi

Tarih: 25 Kasım 2026. Ana plan bu özellikleri sonraki sürüme bırakıyor; proje kuralı önce maliyet ve lisans bakılmasını istiyor. **Bu aşamada kod yazılmadı.** Aşağıdaki işlerin çoğu ücretli hesap ister; karar kullanıcınındır.

Fiyatlar araştırma günündeki halidir, değişebilir. Dolar cinsindendir.

## Özellik özellik

| Plan maddesi | Seçenek | Ücret / lisans | Görüş |
|---|---|---|---|
| Arka plan üretme, görsel düzenleme | Gemini görsel modelleri | 1K görsel başı 0,034 $ (`gemini-3.1-flash-lite-image`) ile 0,067 $ (`gemini-3.1-flash-image`). Ücretsiz kullanım yok | Ucuz ve eldeki anahtarla yapılır; ödeme açmak gerekir. İlk aday |
| Müzik üretimi | Lyria (Google) | 30 sn parça 0,04 $, tam şarkı 0,08 $. Ücretsiz yok | Ucuz; ödeme gerekir |
| Müzik üretimi (yerel) | ACE-Step 1.5 | MIT lisansı, ticari kullanım serbest | Ücretsiz ama büyük model indirmek ve güçlü ekran kartı gerekir (bu bilgisayarda denenmedi) |
| Video üretimi | Gemini Omni Flash / Veo | Omni Flash 720p saniyesi yaklaşık 0,10 $; Veo 3.1 daha pahalı (yaklaşık 0,40 $/sn, üçüncü taraf kaynak). Ücretsiz yok | 60 sn'lik tek deneme yaklaşık 6 $ ve üstü. Şimdilik önerilmez |
| Çok dilli seslendirme | Eldeki Gemini TTS | `gemini-3.8-flash-tts`: ücretsiz katman var; ücretlide 10 sn ses 0,00225 $ (1 Ocak 2027'den sonra 0,0045 $) | En kolayı: metni çevirip aynı sistemle seslendirmek. Ücretsiz günlük sınır küçük |
| Yerel ses üretimi | Piper, XTTS-v2 vb. | Piper motoru MIT; **her sesin kendi lisansı var**. Türkçe `dfki` Piper sesi ve XTTS-v2 ticari kullanıma kapalı | TikTok'tan para kazanılacaksa lisansı ticari kullanıma açık bir Türkçe ses bulunmadan kullanılmamalı |
| Videodan hareket çıkarma, kamerayla poz | MediaPipe Pose | Kod Apache-2.0; model dosyasının koşulları ayrıca doğrulanmalı | Unity'ye ek eklenti ve hareket aktarma işi gerekir; büyük iş, sona bırakılmalı |
| Yapay zekâ karakter oluşturma | Araştırılmadı | — | Şimdilik VRoid Studio (elle) kullanılıyor |

## Önerilen sıra (karar verilirse)

1. Önce "HATALI" listesi düzeltilir.
2. Çok dilli seslendirme (ek ücretsiz altyapı, ek paket yok).
3. Arka plan üretme (ödeme açılırsa).
4. Müzik üretimi.
5. Hareket çıkarma ve video üretimi en sona.

## Kullanıcının karar vermesi gerekenler

- Google hesabında ücretli API açılacak mı? (Claude ödeme veya hesap işlemi yapmaz.)
- Büyük yerel model (müzik) indirilsin mi? İndirmeden önce dosya, boyut ve kaynak sorulur.

## Kaynaklar

- Gemini API fiyatları: https://ai.google.dev/gemini-api/docs/pricing
- Veo fiyatı (üçüncü taraf): https://benchlm.ai/media-pricing/veo
- ACE-Step 1.5 model kartı: https://huggingface.co/ACE-Step/acestep-v15-base/blob/main/README.md
- Piper ses lisansları: https://tts.ai/voices/piper/?lang=tr
- Türkçe ses modeli lisansları: https://huggingface.co/ahakan/okur-tts ve https://www.ssdnodes.com/learn/lang/tr/open-source-turkish-tts-models-compared
- MediaPipe Pose: https://developers.google.com/edge/mediapipe/solutions/vision/pose_landmarker
