# Özellik Defteri — Güncelleme (Agent C)

Bu dosya, "Özellik Defteri"nin eskimiş bölümlerinin düzeltilmiş metnini ve yeni yapılanların listesini içerir. Defterin ilgili bölümlerini aşağıdaki metinlerle değiştir.

---

## Bölüm 4 (Seslendirme) — düzeltilmiş metin

**Model.** Seslendirme **Gemini 3.8 Flash TTS** ile yapılır (`gemini-3.8-flash-tts`, varsayılan). İsteğe bağlı: `gemini-3.8-flash-lite-tts` (ucuz/hızlı) ve eski `gemini-2.5-flash-preview-tts`. Model Stüdyo penceresinden seçilir; yeni model API'ye metni **birebir** okutur, ton `speech_metadata.style` içinde **kısa bir stil** (en çok ~6 kelime) olarak gider. Eski 2.5 modelinde ton cümlenin başına yazılır.

**Ses yüksekliği (LUFS).** Hedef **−14 LUFS** (ITU-R BS.1770 / EBU R128, K-ağırlıklı, 400 ms bloklar, −70 LUFS mutlak ve −10 LU göreli kapı). Kayıt sonunda ölçülür; rapor `aciklama.txt` notlarına ve `HikayeKomut/kayit_durumu.json` → `ses_raporu` alanına yazılır, ölçülen sapmaya göre bir sonraki kayıt için otomatik kazanç (−6…+18 dB) öğrenilir.

**Kota.** Ücretsiz kota günde ~10 ses isteğidir (Pasifik günü; Türkiye'de ~11:00'de sıfırlanır). Stüdyo bugünkü kullanımı sayar ve gösterir (yeniden denemeler ve varyantlar da sayılır). Üretilen sesler `Assets/Resources/Ses/<hikaye>/` altında saklanır, aynı satır tekrar üretilmez.

**Metin hazırlığı (yeni).** Seslendirmeden önce metin Türkçe okunuşa çevrilir: `1993` → "bin dokuz yüz doksan üç", `3.` → "üçüncü", `Dr.` → "Doktor", `%69` → "yüzde altmış dokuz", `14:30`, `15.03.1993`, `50 TL` vb. Altyazı orijinal metni gösterir. Dosya adı (önbellek anahtarı) normalize metinden üretilir (bkz. PATCH_C.md #1). Özel okunuşlar `Assets/Resources/telaffuz.txt` içinde `yazilis=okunus` biçiminde tutulur.

**Kalite koruma (yeni).**
- Kesik ses denetimi: beklenen süre = hece sayısı ÷ ~6 hece/sn (+ etiket araları). Ses beklenenin %70'inden kısaysa en çok 3 kez yeniden üretilir (ayar: 0–5), en uzunu saklanır; hâlâ kısaysa uyarı verilir.
- Stil metni denetimi: 6 kelimeyi aşan stil uyarılır ve Gemini'ye kısaltılarak gönderilir.
- Satır başına varyant (`_v1`, `_v2` …): dinle, "Bunu kullan", eski sesi geri al.
- Kayıt, **sesi eksik satır varken başlamaz**; eksik satırlar listelenir.

---

## Bölüm 9 (Durum) — düzeltilmiş metin

**Durum: kod derlendi ve çalışıyor.** Kurulum (kütüphane, örnek hikaye, sahne), 3D karakter hazırlığı, Gemini seslendirme, önizleme, videoya alma (Recorder, 1080x1920, 30 fps, MP4), kapak ve açıklama üretimi, ses yüksekliği ölçümü ve uzaktan komut köprüsü çalışır durumdadır (`HikayeKomut/derleme.txt`: "0 hata"; yalnız analizör uyarıları vardı: UAC0005 `AppDomain.GetAssemblies` ve UAC1015 sözlük alanı).

Açık kalanlar:
- Kadın_Worker ve Prisoner Humanoid avatarları: iskelet eşleme artık tolerans/yedek yollarıyla kurulur; kurulamazsa hangi kemiklerin eksik olduğu `HikayeKomut/rig_raporu.txt` ve konsola yazılır (sessiz atlama yok).
- `Sahne3D.Harita` sözlüğüne `[SerializeField]` / `[NonSerialized]` eklenmeli (UAC1015 uyarısı; Agent B alanı).
- Yeni eklenen kod parçaları (aşağıdaki liste) Unity'de derlenip gözle doğrulanmalı.

---

## Yol haritası satırı — düzeltme

Eski: "Kod hiç derlenmedi."
Yeni: "Kod Unity 6'da derlenip çalıştırıldı (son derleme: 0 hata). Bu turda eklenen kod (Stüdyo Köprüsü, doğrulayıcı, ses kalitesi, Stüdyo penceresi geliştirmeleri) henüz Unity'de derlenmedi; ilk iş `yenile` komutu ve `HikayeKomut/derleme.txt` kontrolü."

---

## Yeni yapılanlar (Agent C)

**Yapay zeka erişimi — Stüdyo Köprüsü** (paket gerekmez; `HikayeKomut/` dosya köprüsü)
- `hikaye_yaz: <ad>` (taslak `HikayeKomut/hikaye_<ad>.txt`; önce doğrulanır, hata varsa **yazılmaz**, `dogrulama.json` yazılır; eski sürüm yedeklenir)
- `dogrula: <ad> [proje]`, `karakter_listele`, `mekan_listele`, `ses_listele [: ad]`, `komut_listele`, `onizleme_goruntu: <ad> [satır] [gecikme]`, `kayit_durumu`, `kota`, `kilavuz_yaz`
- Tüm çıktılar JSON/metin: `dogrulama.json`, `karakterler.json`, `mekanlar.json`, `sesler.json`, `komutlar.json`, `ekran.png` + `ekran.json`, `kayit_durumu.json`, `son_sonuc.json`, `sonuc.txt`
- Yol güvenliği: `..`, `/`, `\`, `:` ve mutlak yol reddedilir; hikayeler yalnız `Assets/Resources/Hikayeler/` altına yazılır
- Doğrulayıcı: kodlu sorun listesi `[{satir, seviye, kod, mesaj, oneri, duzeltme}]`; bilinmeyen karakter/mekan/efekt/komut/duygu/hareket/poz için Levenshtein ile "Bunu mu demek istediniz"; komut listesi kayıt defterinden/parser yoklamasından gelir (sabit liste yok)
- Menü **Hikaye > Yapay Zeka Kılavuzu Yaz**: `Docs/HikayeSozdizimi.md`, `CLAUDE.md`, `GEMINI.md`

**Stüdyo penceresi**
- Tıklanabilir **Sorunlar** paneli ("Bunu mu demek istediniz" + tek tık "Düzelt")
- Canlı süre/kelime çubuğu (61 sn çizgisi, hedef 1:01–1:30), sahne başına süre listesi (gerçek ses süreleriyle)
- **Kayıt öncesi kontrol listesi**: eksik sesler, doğrulama, kanca, 61 sn, bölüm sonu, seçim twist'i, güvenli alan onayı, seri/başlık, Recorder, ritim denetçisi (varsa)
- "Bitti" bildirimi + **Klasörde göster** (video ve seslendirme bitişi)
- Undo/Redo (`Undo.RecordObject`, ↶ ↷), satırı yoksay (`//`, Ctrl+/, çoklu satır), aranabilir **Ekle** menüsü (AdvancedDropdown), seslendirmede `UnityEditor.Progress`

**Ses kalitesi**: Türkçe sayı/kısaltma normalizasyonu, telaffuz sözlüğü, kesik ses denetimi, stil denetimi, günlük kota sayacı, sesi eksikken kayıt engeli, satır varyantları.

**Düzeltmeler**: Humanoid avatar kurulumu üç aşamalı (Quaternius adları → yaygın adlar → iskelet hiyerarşisi), isteğe bağlı kemikleri azaltarak yeniden dener, eksik kemikleri adıyla raporlar; `AppDomain.GetAssemblies` yerine `AsmYardim` (CompilationPipeline tabanlı).
