# 07 RİSK KAYDI
| ID | Risk | Olasılık | Etki | Seviye | Önlem | Alternatif | Durum |
|---|---|---|---|---|---|---|---|
| R-001 | Haftalık kullanım hakkı araştırma ortasında biter | yüksek | araştırma durur | yüksek | aşamalı dosya kaydı, dalga sonu GitHub+cihaz yedeği | hak yenilenince 00'dan devam | açık |
| R-002 | Bulut çalışma alanı silinir | orta | veri kaybı | orta | GitHub'a sık gönderim | cihaz kopyası | açık |
| R-003 | Tümleşik ekran kartı hedef kaliteyi sınırlar | yüksek | kalite | yüksek | A09/A10 araştırması | GPU yükseltme / çevrim dışı render | açık |
| R-004 | C: diskinde 45 GB boş | yüksek | büyük araç/varlık sığmaz | orta | boyut hesabı, SSD | — | açık |
| R-005 | Ses/hareket çıktıları dinlenmeden/izlenmeden "hazır" denmesi (26 Kas'ta yaşandı) | orta | kullanıcı güveni | yüksek | her testte doğrulama ölçütü; doğrulanamayanı açık yaz | — | açık |
| R-006 | Web arama hakkı (200, bütün ajanlarla ortak, kullanıcı mesajı başına) 1. dalgada doldu | gerçekleşti | 2. dalga bekler, 1. dalgada eksik konular | orta | 2. dalga yeni kullanıcı mesajından sonra; ajan başına arama bütçesi (~30) ver | yalnız bilinen adresleri okuma | açık |
| R-007 | HDRP'ye dayalı herhangi bir plan boşa gider | düşük (öneri geri çekildi) | yüksek | orta | URP | — | izleniyor |
| R-008 | Örnek videonun fotogerçek yüz seviyesine 3D yolla ulaşılamaması | yüksek | kullanıcı beklentisi | yüksek | dürüst bildirim; A11 gelecek teknolojiler | — | açık |
| R-009 | EXE bu hâliyle başkasına dağıtılırsa lisans ihlali (Mixamo karakterleri katalogda, FFmpeg bildirimi, açık kaynak bildirimleri) | yüksek (dağıtımda) | yüksek | yüksek | dağıtma; dağıtım profili (A12-R1..R3) | — | açık |
| R-010 | Dosya köprüsüyle Unity sürmek Unity koşullarına aykırı olabilir (Q-016) | orta-yüksek | yüksek (lisans hakları) | **yüksek** | köprü kullanımı durduruldu; yetkili yol (Unity CLI) araştırılacak | testleri kullanıcı menüden elle başlatır; Unity'ye yazılı sorulur | açık |
| R-011 | İki ajan (A07, A12) "git yasak" kuralına karşın salt okunur git komutu çalıştırdı | gerçekleşti | düşük | düşük | depo değişmedi; denetçiye git kullanımı açıkça kapatılacak | — | kapalı |

## R10'dan taşınan riskler (2026-11-27; olasılık/etki ÇIKARIM, ölçülmedi)
R-001…R-011 yukarıda. R10'daki satırlardan zaten kayıtlı olanlar tekrar yazılmadı: fotogerçek beklentisi = R-008, izlenmeden teslim = R-005, dağıtımda lisans = R-009, tümleşik kart = R-003, disk = R-004, kullanım sınırı = R-001/R-002, arama hakkı = R-006, git = R-011.

| ID | Risk (kategori) | Olasılık | Etki | Seviye | Önlem | Alternatif | Durum |
|---|---|---|---|---|---|---|---|
| R-012 | Kart alınır, sorun kodda kalır (maliyet) | yüksek | yüksek | yüksek | önce düzeltme ve ölçüm | alım ertelenir | açık |
| R-013 | Yetersiz güç kaynağıyla kart takmak (donanım) | bilinmiyor | çok yüksek | yüksek | etiketi öğrenmeden alma | bilgisayarcıya danışmak | açık |
| R-014 | Gölge/HDR açınca kayıt uzar ya da sürücü çöker (performans/render) | orta | orta | orta | kademeli aç; bekçi | düşük kalite profili | açık |
| R-015 | HDR açınca 2D/3D bindirme bozulur (render) | orta | orta | orta | iki durumda test | yalnız tam 3D'de HDR | açık |
| R-016 | EXE'de efekt shader'ları silinir (render) | gözlendi | orta | orta | kalıcı profil; EXE ile doğrula | Editör'den kayıt | açık |
| R-017 | Büyük oynatıcı sınıfını düzenlerken davranış bozulur (proje mimarisi) | yüksek | yüksek | yüksek | referans hikâye ve kare karşılaştırması | küçük adımlar, geri al | açık |
| R-018 | Yeni klip bulanık ad eşlemesiyle başka yeri bozar (animasyon) | orta | orta | orta | açık hareket kataloğu | klip eklememek | açık |
| R-019 | Satır numaralı eski sesler "eksik" olur (ses) | yüksek (geçişte) | orta | orta | önce geçiş planı | eski yola geri dönüş | açık |
| R-020 | Ses düzeyi iki kez işlenir (ses) | orta | orta | orta | loudnorm gelirse eski kazancı kapat | yalnız eski kazanç | açık |
| R-021 | Karakter aracı bu Unity sürümünde/DX11'de açılmaz (uyumluluk) | orta-yüksek | orta | orta | kopya projede kabul testi | mevcut karakterler | açık |
| R-022 | Unity 6000.6 uzun destekli değil; Unity 7 değişiklikleri (sürdürülebilirlik) | yüksek | orta | orta | planlı 6.7 LTS geçişi | sürümü sabit tutmak | açık |
| R-023 | Aynı hikâye her kayıtta farklı (render/test) | gözlendi (kod) | orta | orta | tohumlu rastgelelik | 3 ölçümün ortancası | açık |
| R-024 | Karakter listesi hepsini belleğe yükler (performans) | orta | orta | orta | adla yükleme | az karakter | açık |
| R-025 | VFX Graph tümleşik kartta kararsız (performans) | orta | orta | orta | ana yol parçacık sistemi | 2D efekt | açık |
| R-026 | Yakın plan yüz/ağız kusurlarını büyütür (karakter) | yüksek | yüksek | yüksek | yüz testi geçene kadar sınırlı | orta plan | açık |
| R-027 | Gerçekçi karakter + low-poly eşya uyumsuzluğu (karakter/ortam) | yüksek | yüksek | yüksek | tek stil kuralı, pilot | stilize görünüm | açık |
| R-028 | Sinirsel kare iyileştirme yüzü değiştirir (karakter tutarlılığı) | belirsiz | yüksek | orta | yalnız onay ve tutarlılık testiyle | kullanmamak | açık |
| R-029 | Fiyat artışı, stok, güncel TL fiyatı yok (maliyet) | yüksek | orta | orta | karar anında yeniden bak | Plan A | açık |
| R-030 | Elle sanat ve ayar emeği hafife alınır (geliştirme süresi) | yüksek | yüksek | yüksek | tek mekân/karakter pilotu | kapsamı kullanıcıyla küçültmek | açık |
| R-031 | Ücretsiz ses kotası yetmez ya da koşul değişir (ses/teknoloji bağımlılığı) | orta | orta | orta | sağlayıcı arayüzü, ses saklama | ücretli ses | açık |
| R-032 | Terk edilen bağımlılık (teknoloji bağımlılığı) | orta | orta | orta | yerel dosya, lisans kopyası | değiştirilebilir arayüz | açık |
| R-033 | Doğrulamasız indirme ("latest" ffmpeg) (sürdürülebilirlik/güvenlik) | düşük | yüksek | orta | sürüm ve SHA-256 sabitle | elle indirme | açık |
| R-034 | Windows 10 uzatılmış güncellemesi Ekim 2027'de biter (sürdürülebilirlik) | yüksek | orta | orta | kayıt ve Windows 11 planı | — | açık |
| R-035 | Mağaza / Reallusion / MetaHuman varlığının uygulama kütüphanesine girmesi (lisans) | orta | yüksek | yüksek | dağıtımda kullanma; yazılı teyit | MIT/CC0 | açık |
| R-036 | TikTok'ta etiketsiz yapay ses/görüntü (lisans/platform) | orta | orta | orta | yapay zekâ etiketini aç | — | açık |
| R-037 | ElevenLabs ücretsiz sesli videoda atıfsız paylaşım ya da gelir (lisans) | orta | orta | orta | başlıkta "elevenlabs.io", ticari kullanım yok | başka ses | açık |
| R-038 | Gemini koşulları (18+, AB'ye sunum, ücretsizde veri incelemesi) (lisans/veri) | orta | orta | orta | kişisel bilgi yazma | ücretli/yerel ses | açık |
| R-039 | Gerçek kişi yüzü/sesi, telifli karakter/müzik (lisans) | düşük | yüksek | orta | kullanma | özgün/CC0 | açık |
| R-040 | Bulgular statik kod okuması ve özet araçla okunan kaynaklara dayanıyor (araştırma) | yüksek | yüksek | yüksek | teşhis testleri; kritik cümleleri asıl metinden denetle | — | açık |
| R-041 | Unity CLI yolu: geniş yetki (eval), yeni paket, Unity yetkiyi geri alabilir (teknoloji bağımlılığı) | orta | orta | orta | kullanıcı onayı, kopya projede deneme | elle çalıştırma | açık |
