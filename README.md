# Hikaye Stüdyosu (içerik uygulaması)

TikTok hikâye videoları için Unity uygulaması. Hikayeyi sen yazarsın; karakterler, mekân, kamera, altyazı, ses ve efektler otomatik kurulur.
Unity 6 (6000.x), URP projesi. Ek paket gerekmez; video kaydı için Unity Recorder'ı Stüdyo penceresi tek tıkla kurar.

## Kurulum
1. Bu repodaki `Assets` klasörünün içeriğini Unity projenin `Assets` klasörüne kopyala.
2. Unity'ye dön. Derleme bitince kurulum **kendiliğinden** yapılır:
   kütüphane (`Assets/Resources/StoryLibrary`), örnek hikâye, `HikayeSahnesi` sahnesi, dikey 1080x1920 Game penceresi ve **Hikaye Stüdyosu** penceresi.
3. Stüdyoda **▶ Önizle**'ye bas.

Menü: **Hikaye > Stüdyo** (Ctrl+Shift+H), **Hikaye > Kurulumu Yenile**.

## Stüdyo penceresi
- Hikayeyi yaz, kaydet. Hatalı satırlar, eksik karakter/mekân/efekt anında listelenir.
- **Eksik sesleri üret:** Gemini API anahtarınla her satırı karakterin sabit sesiyle ve duygusuna uygun tonla seslendirir.
  Üretilen ses saklanır, aynı satır bir daha üretilmez. Günlük kota dolarsa durur, ertesi gün kalanları üretir.
- **▶ Önizle:** sahneyi oynatır (Boşluk = duraklat).
- **● Videoya al:** 1080x1920, 30 fps MP4 kaydeder, hikâye bitince durur ve videoyu `Videolar/Hikaye` klasöründe açar.

## Hikaye yazımı
```
[Karakter|duygu|dis] Metin          dışarıdan konuşur, ağzı oynar
[Karakter|duygu|ic] Metin           iç ses: ağız kapalı, ekran hafif kararır
[Karakter|duygu|dis|hareket] Metin  hareketle konuşur
```
Duygular: `normal mutlu sevgi uzgun aglamakli korkmus sasirmis kizgin yorgun hasta dusunceli fisilti`
Hareketler: `kos yuru saril zipla egil sarsil cik`

| Komut | Ne yapar |
|---|---|
| `mekan: oda_gece` | mekânı değiştirir (oda_gece, oda_gunduz, sofra, yol_gece, siyah) |
| `poz: Arda yatiyor` | ayakta / yatiyor (yatakta, yorganla) / oturuyor |
| `kanca: Öldüm.` | siyah ekranda büyük yazı |
| `geri: 1 saat önce` | VHS geri sarma efekti ve sesi |
| `far:` | kamyon farları yaklaşır, beyaz parlama |
| `flas:` | beyaz parlama |
| `yazi: Devamı 2. Bölümde...` | üst-ortada büyük yazı |
| `efekt: carpma` | carpma korna geri whoosh kapi buyu kalp tahta gok yagmur (hazır sesler) |
| `atmosfer: mum, toz` | mum, toz, yagmur, buyu, gunes / yok |
| `kamera: yakin` | otomatik / yakin / genis / salla |
| `muzik: yok` | pad (varsayılan ambiyans) / yok / kütüphanedeki müzik |
| `gorsel: eller` | tam ekran görsel (eller = kendi gözünden eller) / yok |
| `bekle: 0.5` · `cikar: Anne` · `temizle:` | |

Türkçe harfler serbest; isimler büyük/küçük harf ve ç/ş/ğ farkına bakmaz.

## Otomatik olanlar (İçerik Üretim Defteri'ne göre)
- Karakterler sürekli canlı: nefes, göz kırpma, kafa eğme, konuşurken ağız ve kafa hareketi, ağlarken omuz sarsma ve gözyaşı, sevgide kalpler, yürüme/koşma.
- Duygusal satırlar ve iç ses yakın çekilir, yüz kadrajın ortasında; diğerlerinde geniş plan ve yavaş yaklaşma.
- Altyazı italik, siyah kenarlı, karakter renginde; TikTok'un alt %20'sine ve sağ düğmelerine girmez.
- Ses: baştaki/sondaki sessizlik atılır, 0.5 sn'den uzun boşluklar kısaltılır, ses seviyesi eşitlenir; konuşurken müzik kısılır.
- Film greni, kenar karartma, mum titremesi, ışık huzmesinde toz, yağmur, büyü ışıkları.

## 3D karakterler (CC0)
`Assets/HikayeStudyo/Karakterler` altında: 11 erkek + 10 kadın insan modeli (günlük, takım elbise, işçi, çiftçi, kral, cadı, asker...),
12 hayvan, 9 oyuncak tarzı karakter, 45 insan animasyonu (Quaternius Universal Animation Library) ve ev, köy, sokak, doğa eşyaları.
Hepsi CC0; kaynaklar `Karakterler/LISANSLAR.md` dosyasında.

Unity'yi açınca kendiliğinden:
1. `.glb` dosyaları için **glTFast** paketi kurulur (bir kez, internet gerekir).
2. İnsan modellerine Unity insan iskeleti bağlanır, 45 animasyonun hepsi her insana uygulanır.
3. Her karakter için hazır prefab yapılır: `Karakterler/Hazir`.
4. **Karakter Galerisi** sahnesi kurulur; Play'e basınca karakterler üçer üçer çıkıp yürür, dans eder, konuşur.

Menü: **Hikaye > 3D Karakterleri Hazırla**, **Hikaye > Karakter Galerisi**.
Kodda: `karakter.GetComponent<Karakter3D>().Play("yuru")` (Türkçe takma adlar: dur, konus, yuru, kos, otur, dans, ol, zipla, egil, araba, buyu...).

## Kendi görsellerin
Kütüphanede (`Hikaye Stüdyosu > Kütüphaneyi Aç`) her karakter için `sprites` listesine duygu başına görsel ekleyebilirsin
(gözler açık + isteğe bağlı ağız açık ve gözler kapalı hali). Görsel yoksa karakter düz çizgi film tarzında kodla çizilir.
Mekânlara `sprite`, tam ekran anlar için `inserts`, kendi efekt ve müziklerin için `sfx` / `music` listeleri var.

## Ses seviyesi
Videoyu paylaşmadan önce son mikste -14 LUFS hedeflenir; uygulama satırları eşitler ama LUFS ölçümü yapmaz.
