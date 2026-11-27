# 04 KARAR GÜNLÜĞÜ
Biçim: Karar ID · tarih · karar · gerekçe · kaynaklar · alternatifler · riskler · doğrulama durumu · geçerlilik koşulu. Eski karar silinmez.

| ID | Tarih | Karar | Gerekçe | Kaynak | Alternatif | Risk | Doğrulama | Geçerlilik |
|---|---|---|---|---|---|---|---|---|
| K-001 | 2026-11-27 | Araştırma bitene kadar kod/özellik/geçiş yok | Kullanıcı protokolü §19 | 00_PROTOKOL | — | Geliştirme gecikir | kullanıcı talimatı | kullanıcı değiştirene kadar |
| K-002 | 2026-11-27 | Alt ajanlar iki dalgada, her ajan yalnız kendi dosyasına yazar | Kullanım sınırı ve dosya çakışması | 01 §4 | 12'si birden | Süre uzar | — | araştırma boyunca |
| K-003 | 2026-11-27 | "HDRP'ye geçiş / HDRP için ekran kartı" önerisi geri çekildi (bu bir geliştirme kararı değil, yanlış önerinin düzeltilmesi) | Unity HDRP'yi kaldıracağını açıkladı | A04-S001..S003, A09-S001 | URP (6.7/7.x yol haritası), Blender, Unreal | Yol haritası değişebilir | iki bağımsız okuma | Unity duyurusu değişene kadar |
| K-004 | 2026-11-27 | Q-016 netleşene kadar Unity'ye dosya köprüsüyle komut gönderilmeyecek | Unity koşulları ajan erişimini yetkili arayüzle sınırlıyor görünüyor | Terms of Service §17.2, §26; Unity forum | Unity CLI; kullanıcının elle çalıştırması | testler yavaşlar | metin kısmen özetle okundu | kullanıcı karar verene / Unity netleştirene kadar |

## Öneri statüsündeki kararlar (2026-11-27; hiçbiri uygulanmadı, hiçbiri sınanmadı)
"ÖNERİ" = varsayılan hipotez. Kesinleşmesi test ve kullanıcı onayı ister (protokol §19). Yalnız teşhis aşaması (15 numaralı dosya, Aşama 1) bağlayıcı plandır; o da kullanıcı onayıyla başlar.

| ID | Tarih | Karar | Gerekçe | Kaynak | Alternatif | Risk | Doğrulama | Geçerlilik |
|---|---|---|---|---|---|---|---|---|
| K-005 | 2026-11-27 | ÖNERİ (varsayılan hipotez): Unity + URP'de kal | kod korunur, bu PC'de çalışıyor, HDRP kaldırılacak | A04-S002, A09 §S9 | UE5, Blender ana yol, HDRP | URP kalitesi/hızı yetmeyebilir | TEST EDİLMEDİ | T0 ve TS-4'te gölge+HDR+SSAO kabul edilebilir sürede ve kalitede kalırsa |
| K-006 | 2026-11-27 | ÖNERİ: şimdi UE5'e geçme | bu PC'de ayrı kart yok; sahne tarafı yeniden yazılır | A09 §S9, A09-S029, A10, K01 | UE5 + MetaHuman (K01) | kalite tavanı daha düşük kalabilir | prototip yapılmadı; K01 sınanmadı | **Yeniden açılır, eğer (hepsi):** UE5 gereksinimini karşılayan kart, güç kaynağı ve disk alınabiliyor/alındı; URP denemesi kullanıcıyı tatmin etmedi; kullanıcı yeniden yazımın süresini kabul ediyor. O zaman aynı 20-30 sn sahneyle karşılaştırmalı prototip |
| K-007 | 2026-11-27 | ÖNERİ: Blender ana yol değil; isteğe bağlı varlık ve arka plan aracı | Vega'da Cycles GPU yok; aktarım işi | A04-S030, D1-S019 | tam Blender hattı; hibrit önceden render | hibritte ışık uyuşmazlığı | TEST EDİLMEDİ | güçlü kart alınırsa yeniden bakılır |
| K-008 | 2026-11-27 | ÖNERİ: mimari = ses varlığı + tek çekim listesi + render manifesti, aşamalı geçiş | ayrı şemalar birleşsin (C-027) | 14 §3, X1, X2 | bugünkü yapıya küçük yamalar | erken tasarım; büyük sınıfı bozma | taslak | yalnız teşhis sonrası yeniden yazılır |
| K-009 | 2026-11-27 | ÖNERİ: ses satır satır; gerçek kelime zamanı; hizalayıcı seçimi açık | altyazı uyumsuzluğu reddin ana nedeni | A07 | tahminle bölme; elle zamanlama | kota; Türkçe hizalama bilinmiyor | TEST EDİLMEDİ (Q-007) | Q-015 yanıtı yerel modele izin verirse |
| K-010 | 2026-11-27 | ÖNERİ: kamerada önce mevcut kod; Cinemachine tek sahnede denenir | kodda Cinemachine kullanımı görülmedi | A01, A09, D1 §4.3 | tümüyle Cinemachine | iki sistemin çakışması | TEST EDİLMEDİ | deneme sonucu üstün çıkarsa yeni karar |
| K-011 | 2026-11-27 | ÖNERİ: animasyon = hazır klip (FK) + açık hareket kataloğu + sınırlı IK; motion matching yok | yol ve zaman önceden belli | A03, A00 §S2 | motion matching; videodan hareket | klip çeşitliliği az | TEST EDİLMEDİ | hareket testleri yetersiz derse |
| K-012 | 2026-11-27 | ÖNERİ: karakter çekirdeği Rocketbox (+VRoid stilize için); CC5/MetaHuman şimdilik yok | MIT, elde; donanım ve dağıtım koşulu | A02, A12 | MetaHuman, CC5, iClone | görünüm tavanı düşük | TEST EDİLMEDİ | yüz testi, donanım, lisans teyidi |
| K-013 | 2026-11-27 | ÖNERİ: varlıklarda MIT/CC0 çekirdek ve tek varlık manifesti | dağıtım riski | A12, C-016, C-021 | mağaza/Mixamo ağırlıklı | seçenek azalır | lisans metinleri kısmen okundu | dağıtım olmayacaksa gevşetilebilir |
| K-014 | 2026-11-27 | ÖNERİ: ekran kartı/disk alımı ölçümden önce yok; önce disk, sonra güç kaynağı/kasa, sonra kart | ölçüm yok | A10 | hemen kart | beklerken fiyat artışı | — | T0-6, T0-8 ve TS ölçümleri |
| K-015 | 2026-11-27 | ÖNERİ: görünüm kararı açık; "iyileştirilmiş gerçekçi" ile "3D stilize" aynı kısa sahnede kullanıcıya karşılaştırtılır; 2,5D ana yol değil | stilize mevcut 3D hattı kullanır; 2,5D'de görsel kaynağı yok | D1 §2 | 2,5D illüstrasyon; hibrit pilot | iki görünümü sürdürme emeği | TEST EDİLMEDİ | kullanıcı yanıtı ve deneme |
| K-016 | 2026-11-27 | ÖNERİ: ajan erişimi için Unity CLI yolu kullanıcıya sunulur; onaylanana kadar K-004 sürer | koşul metni kendi işlettiğiniz ajanı Unity CLI üzerinden yetkili sayıyor görünüyor | D1-S001..S007 | elle çalıştırma; Unity'ye yazılı soru | yeni paket, geniş yetki (eval), Unity yetkiyi geri alabilir | metin özet araçla okundu; kurulmadı | kullanıcı onayı; koşullar değişmezse |
