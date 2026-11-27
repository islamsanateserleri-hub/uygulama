# 00 MASTER INDEX — Hikâye Stüdyosu Sinematik Teknoloji Araştırması

**Her yeni oturum buradan başlar.** Protokol: `00_PROTOKOL.pdf` (düz metin: `00_PROTOKOL.txt`). Protokol 27 Kas 2026'da kullanıcı tarafından verildi.

## Amaç
Hikâye Stüdyosu'nun 1080x1920, 30 FPS videolarda mümkün olan en yüksek uygulanabilir sinematik kaliteyi, en düşük sürdürülebilir maliyetle nasıl üreteceğini kanıta dayalı olarak belirlemek.

## ANA KURAL (protokol §19)
Araştırma tamamlanana kadar: yeni özellik yok, kod değişikliği yok, HDRP geçişi yok, yeni render sistemi yok, büyük mimari değişiklik yok, donanım satın alma kararı yok.
Araştırma bitince de geliştirmeye KENDİLİĞİNDEN geçilmez; sonuçlar sunulur, geçiş kullanıcının ayrı kararıdır.
Sıra: ÖĞREN → KAYDET → DOĞRULA → GENİŞLET → DENETLE → TAMAMLA → TEST ET → KARAR VER → EN SON GELİŞTİR.

## Durum (son güncelleme: 27 Kas 2026)
**AŞAMA 1: Kurulum ve 1. dalga alt ajan araştırması.** Araştırma TAMAMLANMADI.

| Aşama | Durum |
|---|---|
| Mevcut proje incelemesi | yapıldı (TOPICS/00_mevcut_proje.md) |
| Dosya sistemi | kuruldu |
| 1. dalga (A01, A02, A03, A04, A07, A09) | başlıyor |
| 2. dalga (A05, A06, A08, A10, A11, A12) | bekliyor |
| Çapraz denetim + çelişkiler | bekliyor |
| Test planı, 13 rapor | bekliyor |
| Öz denetim (§16) + bağımsız denetçi (§17) | bekliyor |

Ayrıntılı ilerleme: `02_RESEARCH_PROGRESS.md`.

## Alt ajanlar
Her ajanın görev tanımı ve raporu: `AGENTS/Axx_*.md`; kaynakları: `SOURCES/Axx_kaynaklar.md`.

| Kod | Uzmanlık |
|---|---|
| A01 | Sinematografi ve görüntü yönetmenliği |
| A02 | Fotogerçekçi karakter / dijital insan |
| A03 | Animasyon, motion capture, hareket |
| A04 | Işık, materyal, görüntü kalitesi |
| A05 | 3D ortam, mimari, çevre |
| A06 | VFX ve sinematik efekt |
| A07 | Ses, TTS, diyalog, dudak senkronu |
| A08 | Sinematik kurgu ve hikâye anlatımı |
| A09 | Unity, render, performans mimarisi |
| A10 | Donanım ve ekonomik optimizasyon |
| A11 | Uluslararası teknoloji ve gelecek |
| A12 | Lisans, ticari kullanım, risk denetçisi |
| DEN | Bağımsız denetçi (en sonda) |

## Dosyalar
- `01_MASTER_RESEARCH_PLAN.md` — kapsam haritası, öncelikler, ajan kuralları
- `02_RESEARCH_PROGRESS.md` · `03_RESEARCH_LOG.md` · `04_DECISION_LOG.md`
- `05_OPEN_QUESTIONS.md` · `06_CONTRADICTIONS.md` · `07_RISK_REGISTER.md`
- `08_TECHNOLOGY_DATABASE.md` · `09_SOURCE_DATABASE.md` · `10_PERFORMANCE_DATABASE.md` · `11_COST_DATABASE.md`
- `12_TEST_PLAN.md` · `13_TEST_RESULTS.md` · `14_FINAL_ARCHITECTURE.md` · `15_FINAL_ROADMAP.md` · `16_RESEARCH_AUDIT.md`
- `TOPICS/` konu notları · `EXPERIMENTS/` gerçek denemeler · `REPORTS/` 13 rapor

## Önemli kararlar
Henüz teknik karar YOK (bkz. `04_DECISION_LOG.md`).

## Önceki araştırmalar (silinmedi)
`Docs/VideoKalitesi.md`, `Docs/Kaynak_Arastirmasi.md`, `Docs/Asama16_Degerlendirme.md`. İçlerindeki bilgiler "ön araştırma"dır; yeniden doğrulanacak.
