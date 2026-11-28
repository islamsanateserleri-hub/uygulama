# GEMINI.md — Hikaye Stüdyosu

Bu proje, Türkçe etiketli hikaye metnini 3D karakterlerle 1080x1920 dikey TikTok videosuna çeviren bir Unity 6 aracıdır. Sen Unity'yi **dosya yazarak** sürersin (MCP yok). Aşağıdaki adımlar eksiksiz iş akışıdır.

## 1. Önce oku

`Docs/HikayeSozdizimi.md` — hikaye biçimi: `[Karakter|duygu|dis|hareket] Metin` konuşma satırları ve `komut: değer` komutları. Örnekler: `Assets/Resources/Hikayeler/*.txt`.

Kısa özet:

```
seri: Sessiz Değişim
bolum: 3
mekan: oda_gece
kanca: Annem beni tanımadı.
[Arda|sasirmis|dis] Anne? Benim, Arda!
[Anne|korkmus|dis|yuru] Sen de kimsin? (nefesi kesil) Evimde ne arıyorsun?
[Arda|dusunceli|ic] Neden beni hatırlamıyor?
secim: A) Söylesin / B) Saklasın
son: sert
```

## 2. Köprü: dosya ile konuş

Klasör: `HikayeKomut/` (proje kökü). Unity her 1,5 saniyede `komut.txt` okur ve siler.

```
HikayeKomut/komut.txt     <- sen yazarsın (her satır bir komut)
HikayeKomut/sonuc.txt     -> sonuç günlüğü
HikayeKomut/son_sonuc.json-> {"zaman","komut","sonuc","tamam"}
HikayeKomut/dogrulama.json-> [{"satir","seviye","kod","mesaj","oneri","duzeltme"}]
```

Komutlar: `hikaye_yaz: <ad>`, `dogrula: <ad>`, `karakter_listele`, `mekan_listele`, `ses_listele`, `komut_listele`, `onizleme_goruntu: <ad> [satir]`, `oynat: <ad>`, `seslendir: <ad>`, `kaydet: <ad>`, `kayit_durumu`, `kota`, `dur`, `yenile`.

Kural: hikaye adı yalnızca harf/rakam/`_`/`-`. `..`, `/`, `\`, `:` yasak. Hikayeler sadece `Assets/Resources/Hikayeler/` altına yazılır (bunu `hikaye_yaz` yapar).

## 3. Adım adım

1. `komut.txt` içine yaz: `karakter_listele` ve `mekan_listele`. 3 sn sonra `HikayeKomut/karakterler.json` ve `mekanlar.json` oku. **Yalnız o listelerdeki** karakter, mekan, efekt, 3D oyuncu adlarını kullan.
2. Hikayeyi `HikayeKomut/hikaye_<ad>.txt` dosyasına yaz.
   - İlk satırlar `kanca:` ya da çarpıcı bir replik olsun (ilk 1,5 sn).
   - 61 saniyeden uzun olsun (12–16 konuşma satırı civarı).
   - Sonda `son: yumusak Devamı yarın...` ya da `son: sert`.
   - `secim:` varsa hemen öncesine twist/soru koy.
3. `komut.txt` içine `hikaye_yaz: <ad>` yaz.
4. 3 sn sonra `son_sonuc.json` oku:
   - `"sonuc"` `REDDEDİLDİ...` ile başlıyorsa `dogrulama.json` aç. Her sorunda `satir`, `kod`, `mesaj`, `oneri` var; `oneri` örn. "Bunu mu demek istediniz: Anne?". Taslağı düzelt, 3. adımı tekrarla.
   - `tamam: ... yazıldı` ise hikaye projede. `seviye: "uyari"` kayıtları engel değildir ama düzeltmek iyidir.
5. Görsel kontrol: `onizleme_goruntu: <ad> 7` → birkaç saniye sonra `HikayeKomut/ekran.png` (`ekran.json` içinde `"tamam": true`).
6. Ses: `seslendir: <ad>` (kullanıcının Gemini API anahtarı Stüdyo'da kayıtlı olmalı; günlük ücretsiz kota ~10 istek, `kota` komutuyla bak). `ses_listele: <ad>` her satırın ses durumunu verir.
7. Video: `kaydet: <ad>`. Tüm satırların sesi yoksa reddedilir ve eksik satırları söyler. Bitişi `kayit_durumu` → `kayit_durumu.json` (`durum: bitti`) ile izle.

## 4. Sorun giderme

- `HikayeKomut/derleme.txt` — C# derleme hataları (0 hata olmalı).
- `HikayeKomut/konsol.txt` — Unity konsolu.
- Komut işlenmediyse Unity kapalı ya da derliyor olabilir; `sonuc.txt` sonuna bak, 10 sn bekle.
- Sık hata: `KARAKTER_YOK`, `MEKAN_YOK`, `KOMUT_BILINMIYOR`, `DUYGU_BILINMIYOR` — hepsinde `oneri` alanındaki düzeltmeyi uygula.

Tam sözdizimi, kodlar ve kurallar: `Docs/HikayeSozdizimi.md`. Claude için ayrıntılı kılavuz: `CLAUDE.md`.
