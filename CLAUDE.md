# CLAUDE.md — Hikaye Stüdyosu

Unity 6 projesi: etiketli Türkçe hikaye metnini 3D/2D karakterlerle **1080x1920 dikey TikTok videosuna** çeviren editör aracı. Tüm kod `namespace HikayeStudyo`, `Assets/HikayeStudyo/` altında. Unity'de **MCP paketi yoktur**; Unity ile **dosya köprüsü** (Stüdyo Köprüsü) üzerinden konuşursun.

## Önce oku

- **`Docs/HikayeSozdizimi.md`** — hikaye sözdiziminin tamamı (etiketler, duygular, hareketler, pozlar, komutlar, örnekler, doğrulama kodları). Hikaye yazmadan önce oku.
- Örnek hikayeler: `Assets/Resources/Hikayeler/*.txt`.

## Stüdyo Köprüsü (dosya köprüsü)

Klasör: `HikayeKomut/` (proje kökünde). Unity açıkken her **1,5 sn'de** bir `HikayeKomut/komut.txt` okunur ve **silinir**.

1. **Komut ver:** `HikayeKomut/komut.txt` dosyasına satır satır komut yaz. Dosyayı **tek seferde, bütün halinde** yaz (önce `komut.tmp`'ye yazıp `komut.txt` olarak yeniden adlandırmak en güvenlisi).
2. **2–3 sn bekle**, sonra sonucu oku:
   - `HikayeKomut/sonuc.txt` — zaman damgalı sonuç günlüğü (en son satırlar)
   - `HikayeKomut/son_sonuc.json` — son komutun sonucu `{zaman, komut, sonuc, tamam}`
3. Uzun işler (seslendirme, kayıt, önizleme görüntüsü) arka planda sürer; ilerlemeyi `sonuc.txt`, `kayit_durumu.json`, `ekran.json` dosyalarından izle.

Hata ayıklama dosyaları: `HikayeKomut/derleme.txt` (son derleme: hata/uyarı), `HikayeKomut/konsol.txt` (Unity konsolu), `HikayeKomut/rig_raporu.txt` (3D iskelet eşleme).

### Komutlar

| Komut | Ne yapar | Çıktı dosyası |
|---|---|---|
| `hikaye_yaz: <ad>` | `HikayeKomut/hikaye_<ad>.txt` taslağını **doğrular**; hata yoksa `Assets/Resources/Hikayeler/<ad>.txt` olarak yazar (eskisini `HikayeKomut/yedek/`'e yedekler). **Hata varsa yazmaz.** | `dogrulama.json` |
| `dogrula: <ad> [proje]` | Taslağı (yoksa projedeki hikayeyi; `proje` yazarsan projedekini) doğrular | `dogrulama.json` |
| `karakter_listele` | Kütüphane karakterleri + sesleri + 3D oyuncu adları | `karakterler.json` |
| `mekan_listele` | Mekan, efekt, müzik, görsel, atmosfer, kamera, duygu, hareket, poz, mevcut hikayeler | `mekanlar.json` |
| `ses_listele [: <ad>]` | Gemini sesleri, karakter sesleri, ses etiketleri, stil tablosu, kota; hikaye adı verilirse satır satır ses durumu | `sesler.json` |
| `komut_listele` | Köprü komutları + hikaye komutları (kayıt defterinden **güncel**) | `komutlar.json` |
| `onizleme_goruntu: <ad> [satir] [gecikme_sn]` | Hikayeyi o satıra kadar sessizce kurar, oynatır, ~3 sn sonra ekran görüntüsü alır | `ekran.png`, `ekran.json` |
| `oynat: <ad> [satir]` | Önizleme oynatır | — |
| `seslendir: <ad>` | Eksik sesleri Gemini ile üretir (günlük ücretsiz kota ~10 istek) | `sonuc.txt` |
| `kaydet: <ad>` | Videoya alır. **Her satırın sesi yoksa reddeder** (eksik satırları listeler) | `kayit_durumu.json` |
| `kayit_durumu` | Kayıt durumu, video yolu/boyutu, ses (LUFS) raporu | `kayit_durumu.json` |
| `dur` | Oynatmayı durdurur | — |
| `kota` | Bugünkü Gemini istek sayacı | `sonuc.txt` |
| `ekran` | Oynarken Game görüntüsünü kaydeder | `ekran.png` |
| `yenile` | Dosyaları yeniden tarar/derler | — |
| `menu: <yol>` | Unity menüsü çalıştırır (ör. `menu: Hikaye/Karakter Galerisi`) | — |
| `kilavuz_yaz` | Bu belgeleri yeniden üretir | — |

### Güvenlik kuralları (sandbox)

- Hikaye adı yalnızca **harf, rakam, `_`, `-`** içerir. `..`, `/`, `\`, `:` ve mutlak yol **reddedilir**.
- Hikayeler yalnızca `Assets/Resources/Hikayeler/` altına yazılır (`hikaye_yaz` ile). Başka klasöre yazma.
- Taslaklar yalnızca `HikayeKomut/hikaye_<ad>.txt` olur.

## Hikaye yazma iş akışı

1. `Docs/HikayeSozdizimi.md` oku. `karakter_listele` ve `mekan_listele` yaz; çıktılardaki **güncel** karakter/mekan/3D oyuncu adlarını kullan.
2. Hikayeyi `HikayeKomut/hikaye_<ad>.txt` dosyasına yaz (UTF-8). Kanca ile başla, 61 sn+ süre hedefle (≈ 12–16 konuşma satırı), bölüm sonu ekle.
3. `komut.txt`'ye `hikaye_yaz: <ad>` yaz; 3 sn bekle; `sonuc.txt` / `son_sonuc.json` oku.
   - `REDDEDİLDİ` ise `dogrulama.json`'u oku: her kayıtta `satir`, `kod`, `mesaj`, `oneri` ("Bunu mu demek istediniz: …") vardır. Taslağı düzelt, `hikaye_yaz`'ı yeniden çalıştır. `seviye: "uyari"` olanlar yazmayı engellemez ama gözden geçir (kanca, süre, bölüm sonu, seçim öncesi twist).
4. (İsteğe bağlı) `onizleme_goruntu: <ad> <satir>` ile bir sahnenin görüntüsünü al, `HikayeKomut/ekran.png` dosyasına bak (`ekran.json` `tamam: true` olunca hazırdır).
5. Kullanıcı Gemini API anahtarını Stüdyo penceresine girmiş olmalı. `seslendir: <ad>` → ilerleme `sonuc.txt`'de. Kota dolduysa kalanı ertesi gün üret (ses dosyaları saklanır, tekrar üretilmez).
6. `kaydet: <ad>` yalnızca tüm sesler hazırken çalışır. `kayit_durumu` ile bitişi izle (`durum: bitti`, `video`, `ses_raporu`).

## C# kodu değiştirirsen

- Derleme sonucu `HikayeKomut/derleme.txt` dosyasındadır; "0 hata" olmadan işi bitmiş sayma. Kod hiç derlenmemişse önce `yenile`, 10–20 sn bekle, `derleme.txt` oku.
- Dosya sahipliği: editör araçları `Assets/HikayeStudyo/Editor/`, oynatıcı/model `Assets/HikayeStudyo/Scripts/`. Yeni komut eklersen `KomutKayit.Kaydet("ad", "ornek: ...", "açıklama")` ile kaydet ki doğrulayıcı, "Ekle" menüsü ve bu kılavuz tanısın; sonra Unity'de **Hikaye > Yapay Zeka Kılavuzu Yaz**.
- Derleme güvenliği: Unity 6 (C# 9), paket ekleme yok; Recorder/glTFast tipleri yansımayla çağrılır.

## Yapma

- Hikayede kütüphanede olmayan karakter/mekan/efekt kullanma (önce `karakter_listele`, `mekan_listele`).
- Komut adlarını Türkçe harfle yazma (`gorsel`, `yazi`, `muzik`, `cikar`).
- `HikayeKomut/` dışına hikaye yazma, `..` içeren yol verme.
- Ücretsiz Gemini kotasını (günlük ~10 istek) boşa harcama: aynı satırı gereksiz yeniden seslendirme.
