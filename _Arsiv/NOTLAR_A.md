# NOTLAR_A: Karakter canlılığı + altyazı (Agent A)

## Ne yapıldı (yeni/değişen dosyalar, hepsi `Assets/HikayeStudyo/Scripts/`)
- **AgizSenkron.cs** (yeni): konuşma klibinden 50 Hz genlik zarfı (RMS, gürültü kapısı, yüzdelik normalleme) çıkarır, klip başına önbellekler. Klip okunamazsa `null` döner ve yapay (Perlin) ağız hareketine geçilir.
- **YuzCanlilik.cs** (yeni): her 3D karaktere otomatik eklenen LateUpdate bileşeni. Animator'u bozmaz: her karede kemiği önce animasyonun yazdığı duruma getirir, sonra dünya eksenlerinde küçük bir dönme ekler.
  - Ağız: Humanoid `Jaw` kemiği varsa onu açar. Yoksa `jawOpen/mouthOpen/aa/A/Fcl_MTH_A` blendshape'i ve değişen ünlüler (ih/ou/ee/oh). İkisi de yoksa baş hafifçe sallanır.
  - Göz kırpma (yalnızca blendshape varsa), nefes (göğüs/omurga), kalça-omurga ağırlık kayması (Perlin, 1-3°), göz/baş sakkadları (göz kemiği varsa gözle, yoksa kafayla).
  - Bakış modları: konuşuyor (muhataba ~%55, kameraya, yana), dinliyor (konuşana ~%75; üzgün/öfkeli konuşanda ~%50 ve %40 yere), düşünüyor (iç ses veya `dusunceli`: yukarı-yana bakar), boş (çoğunlukla kameraya).
  - Duyguya göre genlik: korku = hızlı küçük titreme, üzüntü/ağlama = aşağıda ve yavaş, öfke = sert ve dik, yorgun/hasta = yavaş nefes ve ağır göz kapağı.
  - Yüz blendshape'leri (ARKit/VRM/VRoid adları: happy, angry, sad, surprised, relaxed, blink, aa/ih/ou/ee/oh, mouthSmile*, browDown*, browInnerUp, eyeWide*, eyeBlink*, jawOpen...) duygudan yumuşak geçişle sürülür. Eşleşen yoksa hiçbir şey yapmaz.
  - Yürürken, koşarken, yatarken canlılık otomatik düşer (kayma/bozulma olmasın).
- **YuzSahne.cs** (yeni): statik koordinatör. `Konus / Sustur / Bak / Dinle / KomutCoz`. Sahnedeki 3D karakterleri `FindObjectsByType<Karakter3D>` ile bulur, kimliği `Oyuncu_<id>` adından çıkarır (Sahne3D'ye dokunmaya gerek yok). Konuşmayan karakterler konuşanın duygusuna göre baş onayı / yere bakış yapar; korku/şaşkınlıkta baş hafif geri çekilir; iç sesde kimse tepki vermez.
- **KaraokeAltyazi.cs** (yeni): kelime kelime altyazı (UI `Text`, kelime başına bir Text; legacy uGUI, mevcut altyazı da böyle). Güvenli alan: x 60..940, y 490..760 (alt 480, sağ 140, sol 60, üst 220 boş). Kelime süreleri: klip süresi karakter sayısına bölünür (virgül +2,5 / nokta-soru +4 karakter ağırlığı), baştaki/sondaki sessizlik zarftan atılır. Uzun cümleler 3 satırlık sayfalara bölünür.
- **Karakter3D.cs** (değişti): `canliLik` (varsayılan açık) alanı ve Start sonunda `YuzCanlilik` otomatik ekleme. Başka bir şey değişmedi.
- **PATCH_A.md**: StoryModel.cs ve StoryPlayer.cs için 10 küçük yama (sırayla uygula).

## Yeni hikâye komutları
```
altyazi: karaoke        // aktif kelime sarı + hafif büyür; tüm cümle görünür (3 satırlık sayfalar)
altyazi: tek_kelime     // ekranda 1-2 kelime, büyük puntolu
altyazi: klasik         // eski davranış (varsayılan)

bak: Anne               // son konuşan karakter Anne'ye bakar
bak: Arda > Anne        // Arda, Anne'ye bakar
bak: yere               // son konuşan yere bakar (başı eğik, göz aşağı blendshape'i varsa)
bak: Arda > yere
bak: kamera             // doğrudan ileri
bak: serbest            // otomatik bakışa dön ("bak: Arda > serbest" da olur)

dinle: Anne | onay      // Anne dinlerken sık sık başını sallar
dinle: Anne | supheli   // Anne dinlerken başı yana eğik, kaş çatık, ara sıra "hayır" sallaması
dinle: Anne | normal    // varsayılan
```
Metinde `*vurgu*`: karaoke/tek_kelime'de kelime %28 büyür ve turuncu olur; klasikte kalın + sarı olur. Örnek: `[Arda|korkmus|dis] Sen *asla* gitmeyecektin!`

`bak:` ve `dinle:` kalıcıdır (`bak: serbest` / `dinle: X | normal` ya da `mekan:` ile karakterler silinene kadar). `altyazi:` hikâyenin sonuna kadar geçerlidir. "Bu satırdan oynat" atlamasında altyazı stili korunur.

## Bilinen riskler / dikkat
1. **Derleme**: Unity yokken yazıldı, elle iki kez tarandı. Yeni dosyalar yamasız da derlenir (yeni enum değerlerine yalnızca yamalar başvurur). Yama 1-4 (StoryModel) ve 5-10 (StoryPlayer) birlikte uygulanmalı, yoksa `ItemType.Bak` / `Vurgulu` bulunamaz hatası çıkar. Başka ajanların yamaları aynı satırlara (enum, Say, Run switch) dokunuyorsa elle birleştir.
2. **Yön varsayımı**: Dünya eksenleri karakter kökünün `forward/up/right`'ına göre alınır; model +Z'ye bakıyor varsayılır (Sahne3D zaten böyle döndürüyor). Ters yüzlü bir model baş çevirmeyi ters yapar.
3. **Mixamo/UAL modellerinde** genelde ne `Jaw` kemiği ne blendshape vardır: ağız görünür açılmaz, sadece baş/boyun ses ritmiyle hafifçe sallanır. Gerçek ağız için blendshape'li (ARKit/VRM) model gerekir; kod onları adından bulur.
4. Ses klipleri `DecompressOnLoad` değilse (`SesIceAktarma` ayarlıyor) zarf okunamaz; yapay ağız ritmi kullanılır ve karaoke süreleri tüm klibe yayılır.
5. Kelime zamanları tahmindir (karakter sayısı), gerçek hizalama değildir; uzun duraklamalarda ±0,2 sn kayabilir. Kelime-ses senkronu önemliyse Gemini'den kelime zaman damgası alınmalı.
6. Karaoke Text'leri Canvas altında kelime başına Outline+Shadow içerir; 3 satır / ~25 kelimede maliyet önemsiz, çok uzun satırlar sayfalara bölünür.
7. Hata durumunda `YuzCanlilik` kendini kapatır (tek uyarı loglar); karaoke başlatılamazsa klasik altyazıya düşer.
8. Stüdyo penceresindeki "Komut ekle" listesine yeni komutlar eklenmedi (StudyoWindow.cs benim değil). İstenirse `altyazi: karaoke`, `bak: yere`, `dinle: Anne | onay` satırları eklenebilir.
