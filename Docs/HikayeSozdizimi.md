# Hikaye Stüdyosu — Hikaye Sözdizimi Başvuru Kılavuzu

Bu belge, **Hikaye Stüdyosu**'nun (Unity 6, `namespace HikayeStudyo`) hikaye metin biçimini anlatır. Metin, `StoryModel.cs` içindeki `StoryParser` tarafından satır satır okunur ve 1080x1920 dikey TikTok videosuna çevrilir. Yapay zeka (Claude/Gemini) bu belgeye bakarak hikaye yazar; yazdığı dosyayı **Stüdyo Köprüsü** ile doğrulatıp projeye ekler (bkz. `CLAUDE.md` / `GEMINI.md`).

> Bu dosya Unity menüsünden **Hikaye > Yapay Zeka Kılavuzu Yaz** ile yeniden üretilir (kütüphanedeki güncel karakter/mekan listesi ve kayıtlı komutlarla). Kesin ve güncel komut listesi için köprüde `komut_listele` çalıştır.

---

## 1. Genel kurallar

- Hikaye bir **UTF-8 düz metin dosyasıdır**: `Assets/Resources/Hikayeler/<ad>.txt`. Türkçe karakter kullan (ğ ü ş ı ö ç İ).
- Her satır **bir öğedir**: ya bir konuşma (`[...]` ile başlar) ya da bir komut (`komut: değer`).
- Boş satırlar yok sayılır. `//` ya da `#` ile başlayan satır **yorumdur** (yok sayılır; bir satırı geçici kapatmak için başına `//` koy).
- **Komut adları** büyük/küçük harf ve Türkçe karakter duyarsızdır (`Mekan:`, `mekan:`, `MEKAN:` aynı). Komut adlarını **ASCII yaz**: `gorsel`, `yazi`, `muzik`, `cikar`, `secim`, `sonuc`, `bolum`, `baslik`.
- Karakter, mekan, efekt adları da büyük/küçük harf ve Türkçe harf duyarsızdır (`Anlatıcı` = `anlatici`), boşluk ve `-` alt çizgi sayılır.
- Komutun **değeri** iki noktadan sonraki her şeydir (başındaki boşluk kırpılır): `kanca: Öldüm.`
- Bir hikaye **kanca → gelişme → bölüm sonu** biçiminde kurulur (bkz. bölüm 9).
- Hikaye adı (dosya adı) yalnızca harf, rakam, `_` ve `-` içerir: `bolum_2`, `deneme_3d`.

---

## 2. Konuşma satırı

```
[Karakter|duygu|dis|hareket] Metin
```

| Alan | Zorunlu | Anlamı |
|---|---|---|
| `Karakter` | evet | Kütüphanedeki karakter `id`'si (ör. `Arda`, `Anne`) ya da anlatıcı `Anlatici`. |
| `duygu` | hayır (varsayılan `normal`) | Yüz/ses tonu ve 3D animasyonu belirler (bölüm 3). |
| `dis` / `ic` | hayır (varsayılan dış) | `dis` = karakter dışarıdan konuşur, ağzı oynar. `ic` = **iç ses**: ağız kapalı, ekran hafif kararır, ses sakin/derin okunur. Yalnızca `i` ile başlayan değer iç ses sayılır. |
| `hareket` | hayır | Konuşurken yapılan hareket (bölüm 4). |
| `Metin` | evet | Ekranda altyazı olarak görünür **ve** seslendirilir. Boş olamaz. |

Örnekler:

```
[Arda|sasirmis|ic] Burası neresi?
[Anne|aglamakli|dis|kos] Oğlum! Gözlerini açtın!
[Anlatici|normal|dis] O gece kimse uyumadı.
[Arda|normal|dis] Ağlama anne... İyiyim ben.
```

Notlar:
- Alanlar `|` ile ayrılır; sondaki alanlar boş bırakılabilir (`[Anne] Merhaba` geçerlidir) ama okunaklı olsun diye `[Anne|normal|dis]` biçimini yaz.
- Metinde `[`, `|` kullanma. Köşeli parantez yalnız satır başında başlık için.
- Bir satır çok uzun olmasın: **tek satır ≈ 6–14 kelime** (ekranda 2 satıra sığar, ~3–5 sn).
- Kanca ve cümle ortası ses etiketleri için bölüm 5.

---

## 3. Duygular

Geçerli duygular (başka değer **geçersizdir**):

`normal` · `mutlu` · `sevgi` · `uzgun` · `aglamakli` · `korkmus` · `sasirmis` · `kizgin` · `yorgun` · `hasta` · `dusunceli` · `fisilti`

Türkçe karakterli yazım da kabul edilir (`ağlamaklı` = `aglamakli`).

| Duygu | Gemini'ye giden ses stili | 3D animasyon (varsayılan) |
|---|---|---|
| normal | doğal | konuş |
| mutlu | sevinçli | konuş2 |
| sevgi | sevgi dolu, sıcak | şükret/konuş |
| uzgun | üzgün | üzgün |
| aglamakli | ağlamaklı, titrek | ağla |
| korkmus | korkmuş, nefes nefese | kork |
| sasirmis | şaşkın | şaşır |
| kizgin | öfkeli | tepki |
| yorgun | yorgun, halsiz | üzgün |
| hasta | hasta, bitkin | üzgün |
| dusunceli | düşünceli | düşün |
| fisilti | fısıltıyla | konuş |

`ic` (iç ses) ile birlikte stil başına "iç ses gibi sakin ve derin" eklenir. Güçlü duygular (ağlamaklı, korkmuş, şaşırmış, sevgi, kızgın, üzgün) ve iç ses satırlarında kamera otomatik yakınlaşır.

---

## 4. Hareketler ve pozlar

**Hareket** (konuşma satırında 4. alan): `kos` · `yuru` · `saril` · `zipla` · `egil` · `sarsil` · `cik`

```
[Anne|korkmus|dis|yuru] Sen de kimsin?
```

**Poz** (`poz:` komutu): `ayakta` · `yatiyor` · `oturuyor`

```
poz: Arda yatiyor
```

Poz, sonraki `poz:` komutuna kadar sürer. Yatan karakterde kamera otomatik yakın çeker.

---

## 5. Ses etiketleri ve vurgu

Konuşma metninin **içine** yazılır; altyazıda **görünmez**, seslendirmede Gemini etiketine çevrilir.

| Yazılış | Gemini etiketi |
|---|---|
| `(iç çek)` `(of)` | `<sigh>` |
| `(ağla)` `(hıçkır)` | `<sob>` |
| `(gül)` | `<laugh>` |
| `(kıkırda)` | `<giggle>` |
| `(nefes)` | `<breath>` |
| `(nefesi kesil)` `(irkil)` `(şaşır)` | `<gasp>` |
| `(fısılda)` | `<whispering>` |
| `(öksür)` | `<cough>` |
| `(kısa ara)` `(duraklama)` | `<short pause>` |
| `(uzun ara)` `(bekle)` | `<long pause>` |

İstersen doğrudan Gemini etiketini de yazabilirsin: `<sigh>`, `<sob>`, `<laugh>`, `<gasp>`, `<breath>`, `<whispering>`, `<short pause>`, `<long pause>`.

- `*kelime*` → **vurgu**: yıldızlar altyazıda görünmez, ses vurgulu okunur.
- Örnek: `[Anne|aglamakli|dis] Oğlum (hıçkır) gözlerini açtın! *Şükürler olsun.*`
- Etiketleri az kullan: satır başına en çok 1–2.

### Sayılar, kısaltmalar ve okunuş

Altyazı orijinal metni gösterir; **seslendirmeden önce** metin Türkçe okunuşa çevrilir:

| Yazılan | Okunan |
|---|---|
| `1993` | bin dokuz yüz doksan üç |
| `2. Bölüm` | ikinci Bölüm |
| `3'üncü` | üçüncü |
| `%69` / `69%` | yüzde altmış dokuz |
| `Dr.` `Prof.` `Av.` `vb.` `vs.` | Doktor, Profesör, Avukat, ve benzeri, vesaire |
| `14:30` | on dört otuz |
| `15.03.1993` | on beş mart bin dokuz yüz doksan üç |
| `50 TL` / `₺50` | elli lira |
| `3,5` | üç virgül beş |

Özel okunuş için `Assets/Resources/telaffuz.txt` dosyasına `yazilis=okunus` satırı ekle (ör. `WhatsApp=vatsap`).

Sıra sayısı sezgisi: `N.` yanında küçük harfle başlayan kelime ya da `bölüm/kat/gün/yıl/kez...` gibi bir kelime varsa sıra sayısı (`2. Bölümde` → ikinci), yoksa sayı + nokta (cümle sonu) sayılır.

---

## 6. Komutlar

Biçim: `komut: değer`. Aşağıdaki liste `StoryParser`'ın bildiği komutlardır.

### Bölüm bilgisi (görüntü üretmez; kapak ve açıklama için)

| Komut | Değer | Örnek | Açıklama |
|---|---|---|---|
| `seri` | metin | `seri: Sessiz Değişim` | Seri adı |
| `bolum` | tam sayı | `bolum: 3` | Bölüm numarası (sayı değilse uyarı) |
| `baslik` | metin | `baslik: Annem beni tanımadı` | Bölüm başlığı |
| `kapak` | satır numarası ya da boş | `kapak: 24` / `kapak:` | Kapak karesi o satırdan (ya da sonraki ilk konuşmadan) alınır; boş bırakılırsa bu komuttan sonraki ilk konuşma. Yazılmazsa ilk güçlü duygulu satır seçilir. |
| `etiket` | virgülle hashtag'ler | `etiket: aile, dram, duygusal` | Açıklamadaki hashtag satırına eklenir (`#` yazmak gerekmez, tekrar edenler atlanır). |

### Sahne ve görüntü

| Komut | Değer | Örnek | Açıklama |
|---|---|---|---|
| `mekan` | kütüphanedeki mekan | `mekan: oda_gece` | Arka planı değiştirir, **karakterleri temizler**. İlk konuşmadan önce bir `mekan:` olsun. |
| `kanca` | metin | `kanca: Öldüm.` | Siyah ekranda büyük yazı; **ilk 1,5 saniyede** olmalı. |
| `geri` | metin | `geri: 1 saat önce` | Geri sarma (VHS) efekti + yazı |
| `yazi` | metin | `yazi: Devamı 2. Bölümde...` | Üst-ortada büyük yazı (bölüm sonunda sık kullanılır) |
| `bekle` | saniye | `bekle: 0.5` | Bekler (`0,5` da olur) |
| `far` | — | `far:` | Kamyon farları + beyaz parlama + sarsıntı |
| `flas` | — | `flas:` | Beyaz parlama |
| `vfx` | `flas` `far` `vhs` (`geri`) [metin] | `vfx: flas` / `vfx: vhs 1 saat önce` | **Görsel efekt** komutu (B4 adlandırması: `efekt` = yalnız ses efekti, `vfx` = görsel efekt, `gecis` = yalnız sahne geçişi). `vfx: flas` = `flas:`, `vfx: far` = `far:`, `vfx: vhs metin` = `geri: metin`. Eski komutlar aynen çalışır. |
| `vfx` (yeni) | ad [saniye / `surekli`] | `vfx: kar`, `vfx: konfeti 4`, `vfx: yok` | Ekran efekti (63): `kar` `tipi` `yaprak` `cicek` `kul` `kor` `ates_bocegi` `baloncuk` `duman` `kum_firtinasi` `saganak` `su_alti` `meteor` `yildizlar` `konfeti` `havai_fisek` `balon` `para_yagmuru` `yildiz_yagmuru` `parilti` `spot` `disko` `kalp_yagmuru` `kalpler` `kirik_kalp` `gozyasi_yagmuru` `soru` `unlem` `uyku` `nota` `ofke` `sok` `odak` `patlama_isik` `kivilcim` `sok_dalgasi` `hiz_cizgileri` `darbe` `hasar` `cam_catlagi` `buz` `kararma` `beyazlama` `sepya` `gece` `gun_batimi` `zehir` `kirmizi` `altin` `buz_mavisi` `mor` `nostalji` `kabus` `sinema_seridi` `nabiz` `karincalanma` `glitch` `tarama` `isik_sizintisi` `portal` `hipnoz` `polis` `simsekler`. Hikâyeyi bekletmez, üst üste binebilir; `vfx: yok` ya da mekân değişimi kapatır. |
| `gecis` | `karar` `capraz` `beyaz` | `gecis: karar` | Sahne geçişi: `karar` siyaha kararıp açılır, `capraz` önceki kareden çapraz geçiş, `beyaz` beyaz parlamayla geçiş. Geçişten hemen sonra `mekan:` yaz. **2D ve 3D sahnelerde çalışır.** |
| `gorsel` | `eller` / `yok` / kütüphane görseli | `gorsel: eller` | Tam ekran görsel. `eller` = kendi gözünden eller. `gorsel: yok` kapatır. |
| `atmosfer` | `mum` `toz` `yagmur` `buyu` `gunes` `yok` (virgülle çoklu) | `atmosfer: mum, toz` | Ortam efekti; `yok` kapatır |
| `mekan3d` | `oda` `ev` `mutfak` `yatak_odasi` `hastane` `sokak` `koy` `park` `araba` `yok` ya da kayıtlı ad; ayarlar: `duvar=` `zemin=` `vurgu=` `zaman=` `hava=` `isik=` | `mekan3d: mutfak duvar=mavi vurgu=kirmizi` | 3D mekân. Renkler: beyaz krem sari turuncu kirmizi bordo pembe mor mavi lacivert turkuaz yesil kahve ahsap gri siyah. Uygulamada Mekânlar sayfasında adla kaydedilen mekân `mekan3d: ad` ile kullanılır. `araba`: karakterler oturur (sürücü koltuğu ekranda solda; `yer: Ad surucu/yolcu/arka_sol/arka_sag`). Arabada yalnız `tepki_ver`, `tut`, `birak` hareketleri çalışır; mekân değişince hava sıfırlanır (hemen önce, konuşma olmadan yazılan `hava:` yeni mekâna taşınır); `zaman` ve `isik` siz değiştirene kadar sürer. Arabada koltuk verilmezse çocuk sürücü yapılmaz |
| `hava` | `yagmur` `kar` `sis` `yok` (virgülle çoklu) | `hava: yagmur, sis` | Yalnız 3D mekânda. İç mekânda yağmur/kar pencerenin dışında görünür, sis yalnız açık mekânda |
| `gren` / `vinyet` | `yok` `az` `normal` `cok` | `gren: cok` | Film greni ve kenar karartma (2D ve 3D) |
| `filtre` | `sicak` `soguk` `siyah_beyaz` `pembe` `yesil` `mavi` `yok` | `filtre: soguk` | Renk filtresi. `siyah_beyaz` yalnız 3D mekânda gerçek siyah beyaz olur |
| `yavas` | saniye ya da `yok` | `yavas: 3` | Yavaş çekim: karakter hareketleri yavaşlar, konuşma sesi normal |
| `dondur` | saniye | `dondur: 1.5` | Zaman dondurma: flaş + hareketler durur |
| `gozyasi` / `nefes` | karakter adı ya da `yok` | `gozyasi: Anne` | Gözyaşı ve nefes buharı (yalnız 3D karakter; mekân değişince biter) |
| diğer efektler | | | Beyaz flaş `flas:`, sarsıntı `kamera: salla`, geri sarma `geri:`, anı sahnesi `ani: sepya`, ışık huzmesi `atmosfer: gunes`, yağmur/sis `atmosfer: yagmur` / `atmosfer: sis` / `hava:` |
| `zaman` | `sabah` `gun` `gun_batimi` `gece` | `zaman: gun_batimi` | Işık ve gökyüzü |
| `isik` | `normal` `gunduz` `dramatik` `yumusak` `soguk` `sicak` `karanlik` `mum` `romantik` `umut` `korku` `huzunlu` | `isik: huzunlu` | Duygusal ışıklandırma |
| `kamera` | `otomatik` `genis` `orta` `omuz` `ters` `iki` `yakin` `tepki` `yaklas` `uzaklas` `goz` `yukaridan` `asagidan` `bulanik` `net` `salla` | `kamera: omuz` | `otomatik`: yönetmen konuşana göre plan seçer, dinleyenin tepkisini gösterir, harekete göre açılır. `genis`/`orta`/`omuz`/`ters`/`iki`/`yakin` elle seçimdir, `otomatik` yazana kadar kalır. `tepki [Karakter]` sıradaki satırda dinleyeni gösterir; `salla` bir kez sarsar. Plan seçimi 3D mekanda (`mekan3d:`) çalışır |
| `muzik` | `pad` / `yok` / kütüphane müziği | `muzik: yok` | Fon müziği (varsayılan `pad`) |
| `efekt` | ses efekti | `efekt: kapi` | Hazır (112; kodla üretilen benzetim sesler, gerçek kayıt değil): `carpma` `korna` `geri` `whoosh` `gecis` `kapi` `buyu` `kalp` `tahta` `kase` `yagmur` `flas` `gok` `gokgurultusu` `patlama` `yumruk` `tokat` `dusme` `cam` `silah` `makineli` `kilic` `ok` `kirbac` `kaya` `zincir` `zil` `tiktak` `alarm` `telefon` `mesaj` `bildirim` `klavye` `dugme` `kapi_carpma` `kapi_vur` `anahtar` `para` `kagit` `deklansor` `su_dokme` `kaynama` `ruzgar` `firtina` `dalga` `damla` `ates` `simsek` `deprem` `kus` `karga` `baykus` `cirircir` `ari` `kurt` `kopek` `kedi` `aslan` `at` `nal` `inek` `koyun` `horoz` `yilan` `kurbaga` `ejderha` `canavar` `alkis` `kalabalik` `adim` `kosma` `nefes` `kalp_hizli` `zafer` `kayip` `gerilim` `korku` `surpriz` `davul` `gong` `can` `dram` `mutlu` `huzun` `romantik` `komik` `tada` `pop` `boing` `zip` `onay` `hata` `seviye` `bip` `geri_sayim` `lazer` `robot` `uzay` `isinlanma` `kalkan` `iyilesme` `sihir` `hayalet` `roket` `enerji` `siren` `motor` `tren` `ucak` `helikopter` `gemi` `fren` + kütüphane efektleri |

### Karakter yönetimi

| Komut | Değer | Örnek | Açıklama |
|---|---|---|---|
| `poz` | `Karakter poz` | `poz: Arda oturuyor` | `ayakta` `yatiyor` `oturuyor` |
| `cikar` | karakter | `cikar: Anne` | Karakteri sahneden çıkarır |
| `temizle` | — | `temizle:` | Bütün karakterleri çıkarır |
| `oyuncu` | `Karakter = 3DModel` | `oyuncu: Arda = Timmy` | Karakteri 3D modelle oynatır; `oyuncu: Arda = yok` çizgi karaktere döner. Model adı `karakter_listele` çıktısındaki `oyuncular_3d` adıdır (kısa ad yeter: `Timmy`). Hikayenin tamamı için geçerlidir; dosyanın başına yaz. Renk/boy çeşidi: model adına üç haneli numara ekle (`oyuncu: Anne = VRM_Anne_005`); yeni model değil, aynı modelin çeşididir. |

### Etkileşim ve bölüm sonu

| Komut | Değer | Örnek | Açıklama |
|---|---|---|---|
| `secim` | `A) ... / B) ...` | `secim: A) Babasına anlatsın / B) Sır saklasın` | İzleyici seçim kartı. **İki** seçenek `/` ile ayrılır. |
| `sonuc` | `%oran Harf` | `sonuc: %69 A` | Önceki bölümün oylama sonucu kartı |
| `son` | `yumusak [metin]` ya da `sert [metin]` | `son: yumusak Devamı yarın...` / `son: sert` | `yumusak`: merak bırakan bitiş. `sert`: gerilimde birden kesilir (siyah ekran), hikayeyi bitirir. Mod yazılmazsa yumuşak sayılır. |

### Karakter hareketleri ve etkileşim (3D karakterler)

Yalnız `oyuncu:` ile 3D modele bağlanan karakterlerde çalışır. Komut adlarını ve değerleri yazıldığı gibi (ASCII) yaz. Sarılma, el tutma, eşya tutma ve klipsiz tepkiler **insan iskeleti (Humanoid)** ister; iskeleti uygun olmayan karakterde hareket atlanır ve konsola bir Türkçe uyarı yazılır. Hangi karakterin neyi yapabildiğini görmek için Unity menüsü **Hikaye > Hareket Testi** (`HikayeKomut/hareket_testi.txt`).

| Komut | Örnek | Açıklama |
|---|---|---|
| `sarilma` | `sarilma: Anne Baba` | İki karakter yaklaşıp sarılır. `ayril:` yazılana kadar sürer. |
| `el_tut` | `el_tut: Anne Baba` | Yan yana gelip el ele tutuşur. Biri oturuyor/yatıyorsa diğeri yanına gelip elini tutar. |
| `yuzyuze` | `yuzyuze: Anne Baba` | Gövde ve bakışla birbirine döner. (Yalnız göz için `bak:`.) |
| `ayril` | `ayril:` / `ayril: Anne` | Sarılma, el tutma, yüz yüze durumunu bitirir. |
| `tepki_ver` | `tepki_ver: Baba sasir` | `sasir` `kork` `uzul` `agla` `sevin` `kiz` `dusun` `onayla` `reddet` `el_salla`. İsteğe bağlı üçüncü değer: kime döneceği. Yaklaşık 2 sn sürer, hikaye beklemez. |
| `git` | `git: Anne pencere` / `git: Baba Anne kos` | Noktaya (`sol` `orta` `sag` ya da 3D mekan noktası) veya bir karakterin yanına yürür; `kos` ile koşar. |
| `tut` / `birak` | `tut: Anne bardak sol` / `birak: Anne` | Eşya: `bardak` `telefon` `mektup` `kitap` `cicek` `ilac` `tabak` `anahtar`; el: `sag` (varsayılan) ya da `sol`. |
| `kapi` | `kapi: ac` / `kapi: Anne ac` / `kapi: Anne kapat` | Kapılı 3D mekanda kapıyı açar/kapatır; karakter verilirse kapıya yürüyüp koluna uzanır. Kapıdan girmek için `gir: Anne kapi`, çıkmak için `cikar: Anne kapi`. |
| `yatak` | `yatak: Anne yat` / `yatak: Anne otur` | 3D mekanın yatağına yatırır ya da oturtur (`ev` koltuğu, `park` bankı için `otur`); 2D mekanda olduğu yerde. Konuşurken kalkmaz. Yatma klibi yoksa gövde sırtüstü yatırılır. |
| `kalk` | `kalk: Anne` | Oturduğu ya da yattığı yerden (`yatak:` veya `poz:`) ayağa kalkar. |

Komut hareketin başlamasını bekler (yürüme, yaklaşma); görünmesi için ardından konuşma satırı ya da `bekle:` yaz. Karakter yürürse (`git`, `gir`, `cikar`, `yuru`/`kos` hareketi) sarılma ve el tutma kendiliğinden biter.

```
mekan3d: hastane
yatak: Anne yat
gir: Baba kapi
el_tut: Baba Anne
[Baba|uzgun|dis] Uyan artık, ne olur.
ayril:
kalk: Anne
sarilma: Baba Anne
bekle: 1.5
ayril:
```

### Ek komutlar (sürüme göre)

Stüdyonun ilerleyen sürümleri yeni komutlar ekler (karakter bakış/dinleme/altyazı komutları, kamera ve sahne komutları gibi). Bir komutun **geçerli olup olmadığını varsayma**: köprüde `komut_listele` çalıştır; `hikaye_komutlari` listesinde yoksa kullanma. Doğrulayıcı bilinmeyen komutu hata sayar ve "Bunu mu demek istediniz" önerir.

<!-- HS:KOMUTLAR_EK -->
(Kayıt defterindeki ek komutlar Unity'den "Yapay Zeka Kılavuzu Yaz" çalıştırılınca burada listelenir.)
<!-- /HS:KOMUTLAR_EK -->

---

## Seri ve devamlılık (videoda görünmez)

Aynı `seri:` adını taşıyan hikâyeler `bolum:` numarasına göre sırayla okunur. Yazma ekranındaki **Seri** sekmesi önceki bölümleri özetler ve çelişkileri listeler (ölen karakterin açıklamasız konuşması, kapanmamış sorular, bölüm numarası hataları, ilişki / oyuncu modeli değişiklikleri).

| Komut | Örnek | Anlamı |
|---|---|---|
| `ozet` | `ozet: Arda mektubu buldu` | Bölüm özeti |
| `durum` | `durum: Dede = olu` | `hayatta`, `olu`, `kayip`, `hastanede`, `uzakta`. O satırdan **sonrası** için geçerlidir (ölüm sahnesinin arkasına yaz) |
| `olay` | `olay: Ev satıldı` | Geçmiş olay; ölü karakter yeniden `hayatta` yapılacaksa önce açıklama olarak yazılır |
| `gecmis` | `gecmis: Anne = yıllarca uzaktaydı` | Karakter geçmişi |
| `iliski` | `iliski: Anne - Arda = anne ve oğul` | İlişki |
| `soru` / `cozuldu` | `soru: Tabak kimin?` | Açık soru / yanıtlandı |
| `plan` | `plan: 3 = Mektup bulunur` | Sezon planı |
| `devami` | `devami: Yarın aynı saatte` | "DEVAMI N. BÖLÜMDE" kartı (`bolum:` + 1); bölüm sonunda ekranda kalır |
| `oylama` | `oylama: Arda ne yapmalı?` | Sonraki `secim:` kartının sorusu |

Ölen karakter yalnız anı / geri dönüş içinde (`ani: sepya` … `ani: bitir`, ya da `geri:` sonrasında) çelişki sayılmadan konuşabilir.

## 7. Projedeki karakterler, mekanlar ve 3D oyuncular

Aşağıdaki liste kütüphane (`Assets/Resources/StoryLibrary.asset`) varsayılanıdır. **Güncel liste** için köprüde `karakter_listele` ve `mekan_listele` çalıştır (`HikayeKomut/karakterler.json`, `mekanlar.json`).

<!-- HS:KARAKTERLER -->
**Karakterler** (konuşma satırında `id` ile yazılır):

| id | Gemini sesi | İç ses |
|---|---|---|
| Anlatici | Orus | — |
| Arda | Puck | Orus |
| Anne | Kore | — |
| Baba | Charon | — |
| Adam | Orus | — |
<!-- /HS:KARAKTERLER -->

<!-- HS:MEKANLAR -->
**Mekanlar** (`mekan:` değeri): `oda_gece` · `oda_gunduz` · `sofra` · `yol_gece` · `siyah`

**3D oyuncular:** `karakter_listele` çıktısındaki `oyuncular_3d` alanına bak.
<!-- /HS:MEKANLAR -->

Kütüphanede olmayan karakter/mekan adı **hatadır**; doğrulayıcı en yakın adı önerir.

---

## 8. Tam örnekler

### 8.1 Örnek bölüm (kanca + geri sarma)

```
// HİKAYE STÜDYOSU – ÖRNEK BÖLÜM
mekan: yol_gece
efekt: korna
far:
efekt: carpma
kanca: Öldüm.
geri: 1 saat önce

mekan: oda_gece
poz: Arda yatiyor
[Arda|sasirmis|ic] Burası neresi?
gorsel: eller
[Arda|korkmus|ic] Ellerim... Bu benim bedenim değil.
gorsel: yok
efekt: kapi
[Anne|aglamakli|dis|kos] Oğlum! Gözlerini açtın!
poz: Arda oturuyor
[Arda|normal|dis] Ağlama anne... İyiyim ben.
[Anne|sevgi|dis] Ateşin düştü... Şükürler olsun.
[Arda|dusunceli|ic] Şimdilik bir çocuk gibi davranmalıyım.
atmosfer: buyu
efekt: buyu
[Arda|sasirmis|ic] Bu dünyada... büyü gerçekten var.
yazi: Devamı 2. Bölümde...
```

### 8.2 3D oyuncu ve ses etiketleri

```
oyuncu: Arda = Timmy
oyuncu: Anne = Martha
mekan: oda_gece
kanca: Annem beni tanımadı.
[Arda|sasirmis|dis] Anne? Benim, Arda!
[Anne|korkmus|dis|yuru] Sen de kimsin? (nefesi kesil) Evimde ne arıyorsun?
[Arda|aglamakli|dis] Anne lütfen... (hıçkır) bana bak.
[Arda|dusunceli|ic] Neden beni hatırlamıyor?
[Anne|uzgun|dis] Benim oğlum... üç yıl önce kayboldu.
son: sert
```

### 8.3 Seçim kartı, seri bilgisi, oylama sonucu

```
seri: Sessiz Değişim
bolum: 2
baslik: Babam her şeyi duydu
sonuc: %69 A
mekan: oda_gece
[Arda|korkmus|ic] Babam kapının arkasındaymış... (nefesi kesil)
[Anne|aglamakli|dis] Oğlum, *sakla* bunu. Kimse bilmesin.
[Arda|dusunceli|ic] Ama babam artık biliyor.
secim: A) Babasına anlatsın / B) Sır saklasın
son: sert
```

---

## 9. İyi bir bölüm için kurallar (TikTok)

1. **Kanca (ilk 1,5 sn):** Bölüm `kanca:` ya da çarpıcı bir replikle başlasın; sessiz/boş açılış yok. Doğrulayıcı `KANCA_YOK` uyarısı verir.
2. **Süre ≥ 61 sn:** TikTok Creator Rewards 1 dakikadan uzun videoya ödeme yapar. Kısa bölümde `SURE_KISA` uyarısı çıkar. Kabaca 61 sn ≈ 12–16 konuşma satırı + birkaç komut.
3. **Ritim:** Mekanı/kamerayı/atmosferi her ~15–20 sn'de bir değiştir; ard arda 6'dan fazla aynı karakter satırından kaçın.
4. **Bölüm sonu:** `son: yumusak Devamı yarın...` ya da gerilimde `son: sert`. Bitişsiz bölüm `SON_YOK` uyarısı verir.
5. **Seçim kartı (`secim:`):** Kartın hemen öncesinde bir **twist/soru** olsun (son replik `?`, `!` ya da `...` ile bitsin) ki izleyici oy vermek istesin. İki seçenek de net ve zıt olsun. Sonraki bölümde `sonuc:` ile sonucu göster.
6. **Güvenli alan:** TikTok arayüzü ekranın altını ve sağ kenarı kapatır. Önemli yazı/altyazı alt %20 ve sağ %15'e bağımlı olmasın. Stüdyoda "güvenli alanı göster" ile kontrol et.
7. **Her satır seslendirilmeden kayıt alınmaz.** Önce `seslendir:` (Gemini ücretsiz kota günde ~10 satır).
8. Karakter başına satır uzunluğunu dengele; iç sesi (`ic`) duygu doruklarında kullan.

---

## 10. Doğrulama kodları

`dogrula:` ve `hikaye_yaz:` komutları `HikayeKomut/dogrulama.json` yazar: `[{satir, seviye, kod, mesaj, oneri, duzeltme}]`.
`seviye: "hata"` olan sorun varsa `hikaye_yaz` hikayeyi **yazmaz**. `oneri` "Bunu mu demek istediniz: X?" içerir, `duzeltme` yerine konacak değerdir.

| Kod | Seviye | Anlamı |
|---|---|---|
| `PARANTEZ_EKSIK` | hata | `]` eksik |
| `KARAKTER_BOS` / `KARAKTER_YOK` | hata | Karakter adı boş / kütüphanede yok |
| `DUYGU_BILINMIYOR` | hata | Geçersiz duygu |
| `HAREKET_BILINMIYOR` | hata | Geçersiz hareket |
| `MOD_BILINMIYOR` | uyarı | Üçüncü alan `dis`/`ic` değil |
| `BOS_METIN` | hata | Konuşma metni boş |
| `SATIR_ANLASILMADI` | hata | Ne konuşma ne komut |
| `KOMUT_BILINMIYOR` | hata | Bilinmeyen komut |
| `MEKAN_YOK` / `EFEKT_YOK` / `MUZIK_YOK` / `GORSEL_YOK` | hata | Kütüphanede yok |
| `POZ_BILINMIYOR` / `POZ_EKSIK` | hata | Geçersiz/eksik poz |
| `OYUNCU_YOK` | hata | 3D model bulunamadı |
| `SECIM_BICIM` | hata | `secim:` iki seçenek içermiyor |
| `VFX_BILINMIYOR` / `GECIS_BILINMIYOR` | hata | `vfx:` / `gecis:` değeri geçersiz |
| `KAPAK_SATIR` / `ETIKET_BOS` | uyarı | `kapak:` satır numarası bozuk ya da hikâyeden uzun / `etiket:` boş |
| `BEKLE_SAYI` / `SONUC_BICIM` | uyarı | Değer biçimi bozuk |
| `MEKAN_ILK` | uyarı | İlk konuşmadan önce `mekan:` yok |
| `KANCA_YOK` | uyarı | İlk 1,5 sn'de içerik yok |
| `SURE_KISA` / `SURE_UZUN` | uyarı | 61 sn'den kısa / 180 sn'den uzun |
| `SON_YOK` | uyarı | Bölüm sonu yok |
| `SECIM_TWIST` | uyarı | Seçim öncesi twist yok |
| `STIL_UZUN` | uyarı | Ses stili 6 kelimeden uzun |
| `SES_EKSIK` | uyarı | Satırın sesi üretilmemiş (yalnız projedeki hikayede) |
| `PARSER_UYARI` | uyarı | Ayrıştırıcının verdiği diğer uyarılar (ör. kamera/atmosfer/yeni komutların geçersiz değeri) |

---

## 11. Stüdyo Köprüsü (kısa)

Unity açıkken `HikayeKomut/komut.txt` dosyasına komut yaz, 2–3 sn sonra `sonuc.txt` / `son_sonuc.json` oku:

```
hikaye_yaz: bolum_2        (önce HikayeKomut/hikaye_bolum_2.txt dosyasını yaz)
dogrula: bolum_2
karakter_listele | mekan_listele | ses_listele | komut_listele
onizleme_goruntu: bolum_2 7
oynat: bolum_2 | kaydet: bolum_2 | seslendir: bolum_2 | dur
kayit_durumu | kota | yenile
```

Ayrıntı ve iş akışı: `CLAUDE.md`, `GEMINI.md`.
