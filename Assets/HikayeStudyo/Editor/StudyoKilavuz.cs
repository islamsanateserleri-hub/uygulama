using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HikayeStudyo
{
    // Yapay zeka kilavuzlarini projeye yazar: Docs/HikayeSozdizimi.md, CLAUDE.md, GEMINI.md
    // Menu: Hikaye > Yapay Zeka Kılavuzu Yaz   (komut: kilavuz_yaz)
    // Sozdizimi belgesindeki karakter/mekan/ek komut bolumleri kutuphaneden ve KomutKayit'ten canli doldurulur.
    public static class StudyoKilavuz
    {
        // Belgede zaten anlatilan komutlar; kayit defterindeki digerleri "Ek komutlar" tablosuna girer
        static readonly string[] TabanKomutlar =
        {
            "seri", "bolum", "baslik", "mekan", "kanca", "geri", "yazi", "bekle", "efekt", "cikar", "temizle", "kamera",
            "atmosfer", "muzik", "far", "flas", "gorsel", "secim", "sonuc", "oyuncu", "son", "poz",
            "sarilma", "el_tut", "yuzyuze", "ayril", "tepki_ver", "git", "tut", "birak", "kapi", "yatak", "kalk"
        };

        [MenuItem("Hikaye/Yapay Zeka Kılavuzu Yaz", priority = 60)]
        static void Menu()
        {
            string r = Yaz();
            EditorUtility.DisplayDialog("Hikaye Stüdyosu", r, "Tamam");
        }

        public static string Yaz()
        {
            try
            {
                string kok = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                Directory.CreateDirectory(Path.Combine(kok, "Docs"));
                var sb = new StringBuilder();
                Dosya(Path.Combine(kok, "Docs/HikayeSozdizimi.md"), Doldur(SozdizimiSablon), false, sb);
                Dosya(Path.Combine(kok, "CLAUDE.md"), ClaudeMd, true, sb);
                Dosya(Path.Combine(kok, "GEMINI.md"), GeminiMd, true, sb);
                AssetDatabase.Refresh();
                return "tamam:" + sb;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return "HATA: " + e.Message;
            }
        }

        static void Dosya(string yol, string icerik, bool yedekle, StringBuilder log)
        {
            icerik = icerik.Replace("\r\n", "\n");
            if (yedekle && File.Exists(yol))
            {
                string eski = File.ReadAllText(yol, Encoding.UTF8).Replace("\r\n", "\n");
                if (eski == icerik) { log.Append(" ").Append(Path.GetFileName(yol)).Append("(değişmedi)"); return; }
                File.WriteAllText(yol + ".bak", eski, new UTF8Encoding(false));
            }
            File.WriteAllText(yol, icerik, new UTF8Encoding(false));
            log.Append(" ").Append(Path.GetFileName(yol));
        }

        static string Doldur(string t)
        {
            t = t.Replace("\r\n", "\n");
            StoryParser.Parse("");   // StoryParser'in statik komut kayitlari hazir olsun
            t = Degistir(t, "KARAKTERLER", KarakterBlok(t));
            t = Degistir(t, "MEKANLAR", MekanBlok(t));
            t = Degistir(t, "KOMUTLAR_EK", EkKomutBlok());
            return t;
        }

        static string Bolum(string t, string ad)
        {
            string a = "<!-- HS:" + ad + " -->", b = "<!-- /HS:" + ad + " -->";
            int i = t.IndexOf(a), j = t.IndexOf(b);
            if (i < 0 || j < 0 || j < i) return "";
            return t.Substring(i + a.Length, j - i - a.Length).Trim('\n');
        }

        static string Degistir(string t, string ad, string yeni)
        {
            string a = "<!-- HS:" + ad + " -->", b = "<!-- /HS:" + ad + " -->";
            int i = t.IndexOf(a), j = t.IndexOf(b);
            if (i < 0 || j < 0 || j < i || string.IsNullOrEmpty(yeni)) return t;
            return t.Substring(0, i + a.Length) + "\n" + yeni.TrimEnd('\n') + "\n" + t.Substring(j);
        }

        static StoryLibrary Lib()
        {
            return AssetDatabase.LoadAssetAtPath<StoryLibrary>(OtomatikKurulum.LibPath);
        }

        static string KarakterBlok(string t)
        {
            var lib = Lib();
            if (lib == null) return Bolum(t, "KARAKTERLER");
            var sb = new StringBuilder();
            sb.AppendLine("**Karakterler** (konuşma satırında `id` ile yazılır):");
            sb.AppendLine();
            sb.AppendLine("| id | Gemini sesi | İç ses |");
            sb.AppendLine("|---|---|---|");
            sb.AppendLine("| " + lib.narrator.id + " | " + lib.narrator.voice + " | — |");
            foreach (var c in lib.characters)
            {
                if (c == null) continue;
                sb.AppendLine("| " + c.id + " | " + c.voice + " | " + (string.IsNullOrEmpty(c.innerVoice) ? "—" : c.innerVoice) + " |");
            }
            return sb.ToString();
        }

        static string MekanBlok(string t)
        {
            var lib = Lib();
            if (lib == null) return Bolum(t, "MEKANLAR");
            var m = new List<string>();
            foreach (var b in lib.backgrounds) if (b != null && !string.IsNullOrEmpty(b.id)) m.Add("`" + b.id + "`");
            var sb = new StringBuilder();
            sb.AppendLine("**Mekanlar** (`mekan:` değeri): " + string.Join(" · ", m.ToArray()));
            sb.AppendLine();
            var o = new List<string>();
            foreach (var n in StudyoDogrulama.OyuncuAdlari()) o.Add("`" + n + "`");
            sb.AppendLine("**3D oyuncular** (`oyuncu: Karakter = Ad`): " + (o.Count > 0 ? string.Join(" · ", o.ToArray()) : "henüz hazırlanmamış (Hikaye > 3D Karakterleri Hazırla)"));
            return sb.ToString();
        }

        static string EkKomutBlok()
        {
            var sb = new StringBuilder();
            int n = 0;
            foreach (var b in KomutKayit.Hepsi())
            {
                if (System.Array.IndexOf(TabanKomutlar, b.Ad) >= 0) continue;
                if (n == 0)
                {
                    sb.AppendLine("| Komut | Örnek | Açıklama |");
                    sb.AppendLine("|---|---|---|");
                }
                n++;
                sb.AppendLine("| `" + b.Ad + "` | `" + b.Ornek.Replace("|", "\\|") + "` | " + b.Aciklama.Replace("|", "\\|") + " |");
            }
            return n == 0 ? "(Şu an kayıt defterinde ek komut yok; güncel liste için `komut_listele`.)" : sb.ToString();
        }

        // ---------------- Gomulu sablonlar (Docs/*.md ile ayni icerik) ----------------
        const string SozdizimiSablon = @"# Hikaye Stüdyosu — Hikaye Sözdizimi Başvuru Kılavuzu

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

`ic` (iç ses) ile birlikte stil başına ""iç ses gibi sakin ve derin"" eklenir. Güçlü duygular (ağlamaklı, korkmuş, şaşırmış, sevgi, kızgın, üzgün) ve iç ses satırlarında kamera otomatik yakınlaşır.

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
| `gorsel` | `eller` / `yok` / kütüphane görseli | `gorsel: eller` | Tam ekran görsel. `eller` = kendi gözünden eller. `gorsel: yok` kapatır. |
| `atmosfer` | `mum` `toz` `yagmur` `buyu` `gunes` `yok` (virgülle çoklu) | `atmosfer: mum, toz` | Ortam efekti; `yok` kapatır |
| `mekan3d` | `oda` `ev` `mutfak` `yatak_odasi` `hastane` `sokak` `koy` `park` `araba` `yok` ya da kayıtlı ad; ayarlar: `duvar=` `zemin=` `vurgu=` `zaman=` `hava=` `isik=` | `mekan3d: mutfak duvar=mavi vurgu=kirmizi` | 3D mekân. Renkler: beyaz krem sari turuncu kirmizi bordo pembe mor mavi lacivert turkuaz yesil kahve ahsap gri siyah. Uygulamada Mekânlar sayfasında adla kaydedilen mekân `mekan3d: ad` ile kullanılır. `araba`: karakterler oturur (sürücü koltuğu ekranda solda; `yer: Ad surucu/yolcu/arka_sol/arka_sag`). Arabada yalnız `tepki_ver`, `tut`, `birak` hareketleri çalışır; mekân değişince hava sıfırlanır |
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
| `efekt` | ses efekti | `efekt: kapi` | Hazır: `carpma` `korna` `geri` `whoosh` `gecis` `kapi` `buyu` `kalp` `tahta` `kase` `yagmur` `flas` `gok` `gokgurultusu` + kütüphane efektleri |

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

Stüdyonun ilerleyen sürümleri yeni komutlar ekler (karakter bakış/dinleme/altyazı komutları, kamera ve sahne komutları gibi). Bir komutun **geçerli olup olmadığını varsayma**: köprüde `komut_listele` çalıştır; `hikaye_komutlari` listesinde yoksa kullanma. Doğrulayıcı bilinmeyen komutu hata sayar ve ""Bunu mu demek istediniz"" önerir.

<!-- HS:KOMUTLAR_EK -->
(Kayıt defterindeki ek komutlar Unity'den ""Yapay Zeka Kılavuzu Yaz"" çalıştırılınca burada listelenir.)
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
| `devami` | `devami: Yarın aynı saatte` | ""DEVAMI N. BÖLÜMDE"" kartı (`bolum:` + 1); bölüm sonunda ekranda kalır |
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
6. **Güvenli alan:** TikTok arayüzü ekranın altını ve sağ kenarı kapatır. Önemli yazı/altyazı alt %20 ve sağ %15'e bağımlı olmasın. Stüdyoda ""güvenli alanı göster"" ile kontrol et.
7. **Her satır seslendirilmeden kayıt alınmaz.** Önce `seslendir:` (Gemini ücretsiz kota günde ~10 satır).
8. Karakter başına satır uzunluğunu dengele; iç sesi (`ic`) duygu doruklarında kullan.

---

## 10. Doğrulama kodları

`dogrula:` ve `hikaye_yaz:` komutları `HikayeKomut/dogrulama.json` yazar: `[{satir, seviye, kod, mesaj, oneri, duzeltme}]`.
`seviye: ""hata""` olan sorun varsa `hikaye_yaz` hikayeyi **yazmaz**. `oneri` ""Bunu mu demek istediniz: X?"" içerir, `duzeltme` yerine konacak değerdir.

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
";

        const string ClaudeMd = @"# CLAUDE.md — Hikaye Stüdyosu

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
   - `REDDEDİLDİ` ise `dogrulama.json`'u oku: her kayıtta `satir`, `kod`, `mesaj`, `oneri` (""Bunu mu demek istediniz: …"") vardır. Taslağı düzelt, `hikaye_yaz`'ı yeniden çalıştır. `seviye: ""uyari""` olanlar yazmayı engellemez ama gözden geçir (kanca, süre, bölüm sonu, seçim öncesi twist).
4. (İsteğe bağlı) `onizleme_goruntu: <ad> <satir>` ile bir sahnenin görüntüsünü al, `HikayeKomut/ekran.png` dosyasına bak (`ekran.json` `tamam: true` olunca hazırdır).
5. Kullanıcı Gemini API anahtarını Stüdyo penceresine girmiş olmalı. `seslendir: <ad>` → ilerleme `sonuc.txt`'de. Kota dolduysa kalanı ertesi gün üret (ses dosyaları saklanır, tekrar üretilmez).
6. `kaydet: <ad>` yalnızca tüm sesler hazırken çalışır. `kayit_durumu` ile bitişi izle (`durum: bitti`, `video`, `ses_raporu`).

## C# kodu değiştirirsen

- Derleme sonucu `HikayeKomut/derleme.txt` dosyasındadır; ""0 hata"" olmadan işi bitmiş sayma. Kod hiç derlenmemişse önce `yenile`, 10–20 sn bekle, `derleme.txt` oku.
- Dosya sahipliği: editör araçları `Assets/HikayeStudyo/Editor/`, oynatıcı/model `Assets/HikayeStudyo/Scripts/`. Yeni komut eklersen `KomutKayit.Kaydet(""ad"", ""ornek: ..."", ""açıklama"")` ile kaydet ki doğrulayıcı, ""Ekle"" menüsü ve bu kılavuz tanısın; sonra Unity'de **Hikaye > Yapay Zeka Kılavuzu Yaz**.
- Derleme güvenliği: Unity 6 (C# 9), paket ekleme yok; Recorder/glTFast tipleri yansımayla çağrılır.

## Yapma

- Hikayede kütüphanede olmayan karakter/mekan/efekt kullanma (önce `karakter_listele`, `mekan_listele`).
- Komut adlarını Türkçe harfle yazma (`gorsel`, `yazi`, `muzik`, `cikar`).
- `HikayeKomut/` dışına hikaye yazma, `..` içeren yol verme.
- Ücretsiz Gemini kotasını (günlük ~10 istek) boşa harcama: aynı satırı gereksiz yeniden seslendirme.
";

        const string GeminiMd = @"# GEMINI.md — Hikaye Stüdyosu

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
HikayeKomut/son_sonuc.json-> {""zaman"",""komut"",""sonuc"",""tamam""}
HikayeKomut/dogrulama.json-> [{""satir"",""seviye"",""kod"",""mesaj"",""oneri"",""duzeltme""}]
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
   - `""sonuc""` `REDDEDİLDİ...` ile başlıyorsa `dogrulama.json` aç. Her sorunda `satir`, `kod`, `mesaj`, `oneri` var; `oneri` örn. ""Bunu mu demek istediniz: Anne?"". Taslağı düzelt, 3. adımı tekrarla.
   - `tamam: ... yazıldı` ise hikaye projede. `seviye: ""uyari""` kayıtları engel değildir ama düzeltmek iyidir.
5. Görsel kontrol: `onizleme_goruntu: <ad> 7` → birkaç saniye sonra `HikayeKomut/ekran.png` (`ekran.json` içinde `""tamam"": true`).
6. Ses: `seslendir: <ad>` (kullanıcının Gemini API anahtarı Stüdyo'da kayıtlı olmalı; günlük ücretsiz kota ~10 istek, `kota` komutuyla bak). `ses_listele: <ad>` her satırın ses durumunu verir.
7. Video: `kaydet: <ad>`. Tüm satırların sesi yoksa reddedilir ve eksik satırları söyler. Bitişi `kayit_durumu` → `kayit_durumu.json` (`durum: bitti`) ile izle.

## 4. Sorun giderme

- `HikayeKomut/derleme.txt` — C# derleme hataları (0 hata olmalı).
- `HikayeKomut/konsol.txt` — Unity konsolu.
- Komut işlenmediyse Unity kapalı ya da derliyor olabilir; `sonuc.txt` sonuna bak, 10 sn bekle.
- Sık hata: `KARAKTER_YOK`, `MEKAN_YOK`, `KOMUT_BILINMIYOR`, `DUYGU_BILINMIYOR` — hepsinde `oneri` alanındaki düzeltmeyi uygula.

Tam sözdizimi, kodlar ve kurallar: `Docs/HikayeSozdizimi.md`. Claude için ayrıntılı kılavuz: `CLAUDE.md`.
";
    }
}
