# R13 — Nihai karar (ÖNERİ)

> **TESTSİZ TASLAK.** Yalnız teşhis aşaması (15 numaralı dosya Aşama 1; testler T0) bağlayıcı plandır ve o da kullanıcı onayıyla başlar. Sonrası T0 sonuçlarına göre yeniden yazılır. "URP'de kal" bir karar değil **varsayılan hipotezdir** (04: K-005).

Tarih: 2026-11-27. **Hiçbir deneysel test yapılmadı.** Bu yüzden protokolün istediği "kesin öneri", bugün **kanıta dayalı ama testle doğrulanmamış** bir yön önerisidir. Bağımsız denetçi raporu hazır (`../AGENTS/DEN_bagimsiz_denetci.md`) ve araştırmayı tamamlanmış saymadı; öz denetim için `../16_RESEARCH_AUDIT.md`'ye bakın. Bu öneri değişebilir.

## 1. Soru
En yüksek uygulanabilir sinematik kalite, en düşük sürdürülebilir maliyetle nasıl elde edilir?

## 2. Öneri
1. **Unity URP'de kal (varsayılan hipotez; test edilmedi).** HDRP'ye geçme (kaldırılacak [A04-S002]); şimdi Unreal/Blender'a taşıma (A09, A10).
2. **Önce kusurları düzelt, sonra güzelleştir:** hareket ve ses/altyazı → yüz → gölge/ışık → kamera/kurgu → mekân/efekt → büyük sahneler.
3. **Sıfır yatırımla başla (Plan A)** ve ölç. İlk gerekirse disk; ekran kartı ancak ölçüm ve güç kaynağı/kasa kontrolünden sonra.
4. **Varlıklarda MIT/CC0 çekirdeği** (Rocketbox, UAL, Poly Haven, ambientCG; lisans kaydıyla); dağıtım yapılacaksa Mixamo ve mağaza varlıkları çıkarılır.
5. **Mimari:** ses deposu + tek çekim listesi + dayanıklı kayıt (14 numaralı dosya).
6. **Yapay zekâ video üreticisi kullanılmaz.** Yardımcı araçların sınırı kullanıcıya sorulur (Q-015).

## 3. Beklenen kalite — dürüst sınır
- Hedeflenebilir: doğru altyazı, yürüyen ve konuşan karakterler, ışıklı ve derinlikli görüntü, film benzeri kurgu; iyi varlık, emek ve gerekirse kartla "iyi oyun sinematiği" (ÇIKARIM).
- Beklenmemeli: örnek videodaki fotogerçek insanlar, ejderha ve kalabalığın otomatik ve ucuz üretimi (A11 Q-004; güven orta).
- "Gerçek film kalitesine" bu yolla ulaşılabileceğini gösteren kaynak bulunamadı; bunun imkânsız olduğu da kanıtlanmadı.

## 4. Neden bu karar
| Gerekçe | Dayanak | Kanıt türü |
|---|---|---|
| Reddedilen kusurların olası nedenleri kodda | A03 H1–H3, A07 K1–K5; X2 D1–D5 | KOD |
| Görüntü temelleri kapalı | A04, A09; X1 D4 | KOD |
| HDRP'nin geleceği yok | [A04-S002][A09-S001] | KAYNAK |
| Yeniden yazım pahalı | A09 §S9 | ÇIKARIM |
| Donanım ölçümü yok | A10 | — |
| Lisans riskleri | A12; C-021 | KAYNAK + KOD |

## 5. Reddedilen seçenekler
HDRP · Unreal'a hemen geçiş (kullanıcının getirdiği araştırma öneriyor; doğrulanmamış) · Blender ana yol · yapay zekâ video-to-video · ölçmeden kart alımı · 2,39:1 siyah bar (9:16 hedefine aykırı).

## 6. Geliştirmeye geçiş kararı (protokol §18, §19)

**Bugünkü yargı: geliştirmeye geçilmemeli; yalnız aşağıdakiler tamamlanınca, kullanıcının ayrı onayıyla geçilebilir.**

Gerekçe:
- Bulguların çoğu statik kod okuması; hiçbir test yok.
- Protokolün tamamlanma ölçütlerinden bazıları eksik.

| §18 ölçütü | Durum |
|---|---|
| Ana alanlar, alt ajanlar, uluslararası araştırma, karşılaştırma, alternatifler | yapıldı (eksik alt konular var) |
| Donanım ve maliyet analizi | yapıldı; TL toplamları verilemedi, ölçüm yok |
| Lisans ve uyumluluk | tarandı; bazı asıl metinler okunmadı |
| Kaynak kaydı, çelişki ve risk kaydı | yapıldı; açık çelişkiler var |
| Test planı, mimari ve yol haritası önerisi, kalıcı hafıza | hazır |
| Araştırma dosyalarının güncelliği | index/ilerleme dosyaları güncellenmeli |
| Araştırma denetimi (§16) | durumu için `16_RESEARCH_AUDIT.md` |
| Bağımsız denetçi (§17) | yapıldı (`AGENTS/DEN_bagimsiz_denetci.md`); bulduğu eksiklerin bir kısmı açık |
| Geçiş gerekçesi | bu bölüm |

**Ön koşullar (sırayla):**
1. Öz denetim; bağımsız denetçinin bulduğu eksiklerin kapatılması; Unity koşulu için kullanıcı kararı (Q-016).
2. Kullanıcının soruları yanıtlaması (00_KULLANICI_OZETI).
3. Kullanıcının "ayrı kopya projede teşhis ve prototip başlasın" demesi.
4. Disk envanteri ve yedek.
5. Teşhis testlerinde (T0) ana varsayımların doğrulanması; doğrulanmayanlar için planın güncellenmesi.
6. Her ana projeye aktarım için: ölçüt geçildi, süre kabul edildi, eski hikâyeler bozulmadı, lisans temiz, kullanıcı onayladı.

**Bu kararın değişmesine yol açacaklar:** testlerde URP'nin bu işte yetersiz kalması; kullanıcının yapay zekâ kare iyileştirmeyi kabul etmesi (yol açılır, yine ayrı araştırma/test ister); Unity yol haritasının değişmesi; uygulamanın satılacak olması.

## 7. Bilinen eksikler
Hiç performans ölçümü yok · Türkçe ses/hizalama kalitesi ölçülmedi · MetaHuman kütüphane dağıtımı belirsiz (Q-011) · güncel TL fiyatları yok · Unity koşullarındaki yapay zekâ ajanı maddesi için hukuki netlik yok (Q-016) · karakter görünüm sıralaması görüntü karşılaştırmasına dayanmıyor.
UE5 hangi koşulda yeniden açılır: `04_DECISION_LOG.md` K-006. Görünüm seçenekleri (stilize, 2,5D, hibrit): `AGENTS/D1_duzeltme_arastirmasi.md` §2; test yok.
