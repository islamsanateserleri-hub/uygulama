# A02 — Fotogerçekçi karakter ve dijital insan uzmanı

Tarih: 2026-11-27. Kaynak numaraları: `SOURCES/A02_kaynaklar.md`. "ÇIKARIM" = benim yorumum. "DOĞRULANAMADI" = kanıt bulamadım. Hiçbir şey indirilmedi, kurulmadı, çalıştırılmadı (tek istisna: ad eşleme mantığının Python kopyasını geçici klasörde çalıştırdım; Unity testi değildir).

**Kısıt:** Oturumun ortak web arama hakkı (200) araştırma bitmeden doldu. Sonrasında yalnız daha önce karşıma çıkmış adresleri okudum. Aranamayanlar "Eksik alanlar"da.

## Kısa sonuç
1. **En ucuz ve en hızlı kazanç yeni karakter almak değil, eldeki Rocketbox yüzünü gerçekten kullanmak.** Kod, 52 ARKit şeklinin 18'ini, 15 viseme'in yalnız çene açmayı kullanıyor (ad eşleme benzetimi; Unity'de doğrulanmalı).
2. İkinci kazanç: cilt, göz ve saç gölgelendiricisi. Şu an hepsi düz URP/Lit.
3. MetaHuman'ı Unity'ye taşımak 2026'da yasal ve teknik olarak **mümkün hale geldi**, ama araçlar çok yeni ve denenmemiş; karakter üretimi bu bilgisayarda yapılamaz.
4. Character Creator 5 daha olgun bir Unity yoluna sahip ama ücretli ve ayrı ekran kartı ister.

## Görev tanımı
Gerçekçi dijital insanın üretimi, Unity'ye aktarımı, animasyonu ve tutarlılığı için sistemleri karşılaştırmak (protokol: Alt Ajan 2, §9.3, §3, §6, §10). Kod yazılmaz.

## Araştırma soruları
- S1 Mevcut Rocketbox karakterlerinin yüzü ne sunuyor, kod ne kullanıyor?
- S2 Cilt, göz, saç, kirpik/kaş, diş/ağız için URP ve HDRP'de ne var?
- S3 Yüz anatomisi, ARKit 52, mikro ifadeler
- S4 MetaHuman Unity'ye gerçekten aktarılır mı; ne kaybolur?
- S5 Character Creator 4/5
- S6 Blender/MPFB, VRoid, Daz, diğerleri
- S7 Karakter tutarlılığı, kıyafet, materyal
- S8 Donanım ve maliyet
- S9 Lisans (ticari video; uygulama içinde dağıtım)
- S10 Gelecek

---

## Bulgular

### S1. Mevcut durum: Rocketbox
**Kaynaklardan**
- MIT lisansı, 115 avatar. Şubat 2022'de 15 viseme, 48 FACS, 30 Vive şekli; Haziran 2022'de ARKit uyumlu şekiller eklendi. Depodaki son kayıtlı güncelleme 2022-06 [A02-S020].
- Gerçek adlar: `AA_VI_00_Sil … AA_VI_14_U` (15) ve `AK_01 … AK_52` (52) [A02-S024]. Vive şekilleri `SR_` önekli (bir kaynak 30, diğeri 42 diyor) [A02-S023].
- LOD yaklaşık 10.000 / 5.000 / 2.500 / 500 üçgen; dokular tipik 2048x2048, baş ve gövde ayrı [A02-S021]. 10 bin üçgen bugünün ölçüsüyle düşük (MetaHuman LOD0'da yalnız kafa 24.000 köşe [A02-S010]; köşe ve üçgen farklı birimler).
- Özgün kütüphanede kıyafet, saç, yüz değiştirme sınırlı [A02-S021].

**Proje kodundan** [A02-P1, P2, P3]
- `RocketboxKur.cs` `_facial.fbx` dosyasını tercih ediyor (doğru seçim).
- Malzeme: URP/Lit, Smoothness 0.2, Metallic 0; yalnız `color` ve `normal` dokusu aranıyor; saç/kirpik alpha-clip 0.35, çift yüzlü; en büyük doku 2048. SSS yok, göz için özel işlem yok.
- `YuzCanlilik.cs`: kırpma, çift kırpma, sakkad, nefes, bakış davranışları, duyguya göre profil var. Bu iyi bir temel.

**Ad eşleme benzetimi** (`Sinif()` mantığının Python kopyası, AK ve AA_VI adlarıyla)
- ARKit: **52'nin 18'i** eşleşiyor.
- Eşleşmeyenler arasında: cheekSquint, cheekPuff, noseSneer, mouthFunnel, mouthPucker, mouthClose, mouthPress, mouthRoll, mouthShrug, mouthStretch, mouthUpperUp, mouthLowerDown, mouthDimple, eyeLookIn/Out/Up, jawForward/Left/Right, tongueOut.
- Viseme: yalnız `AA_VI_10_aa` eşleşiyor; öncelik süzgeci jawOpen varken onu da atıyor. **Sonuç: konuşmada ağız yalnız çene açıp kapatıyor** (ÇIKARIM; Unity'de `Hikaye/Yuz Testi` ile doğrulanmalı).
- Ünlü kanalları sesten değil Perlin gürültüsünden seçiliyor (satır 855). Yani VRoid'de de ağız şekli konuşulan harfle ilişkili değil (A07'nin alanı).
- `Mutlu/Uzgun/Kizgin/Saskin` kanalları VRM adlarına göre; Rocketbox'ta karşılığı yok. Şaşkınlıkta ağız/çene hiç oynamıyor, öfkede yalnız kaş ve göz kısma var.

**Olası hatalar (ÇIKARIM, test gerekli)**
- İki kat çene: avatar'da Humanoid `Jaw` kemiği eşlenmişse kod hem kemiği döndürüyor (satır 695) hem jawOpen şeklini yazıyor.
- İki kat şekil: Rocketbox'ta `SR_..Jaw_Open` veya `SR_..Mouth_Smile` varsa `AK_` ile birlikte ikisi de sürülür. (SRanipal'de Jaw_Open adı var [A02-S053]; Rocketbox dosyasındaki tam ad DOĞRULANAMADI.)
- Tekrar üretilemezlik: `Random.value/Range` tohumlanmıyor (projede `InitState` aramada çıkmadı). Aynı hikâye iki kez kaydedilince kırpma ve bakış farklı olur.

### S2. Cilt, göz, saç, kirpik/kaş, diş/ağız
**Boru hattı durumu**
- HDRP'ye yeni özellik planlanmıyor; yalnız kararlılık bakımı [A02-S025]. Strateji sayfasında URP öncelikleri arasında cilt/saç/göz adı geçmiyor [A02-S025]. Unity 6.6 yenilikler sayfasında anahtar kelime aramasıyla URP için SSS/saç/göz yeniliği bulamadım [A02-S026].
- ÇIKARIM: HDRP'nin Skin/Hair/Eye gölgelendiricileri hazır ve güçlü ama geleceği donuk; URP'de bunlar ek gölgelendiriciyle sağlanmalı.

**URP için seçenekler**
| Seçenek | İçerik | Not |
|---|---|---|
| Unity URP Defender demo [A02-S028] | Marschner saç, pre-integrated skin, paralaks göz (Shader Graph) | Karakter ticari kullanılamaz; teknoloji Asset Store EULA. Unity 2021/2022 için; 6.6'da denenmedi |
| IllusionRP [A02-S027] | Ekran uzayı SSS, dual-lobe specular, Kajiya-Kay/Marschner saç, OIT | Unity 6.3, DX12/Vulkan, Forward/Forward+. Bu PC DX11 kullanıyor, proje 6.6. Göz yok. Riskli |
| itsFulcrum hair [A02-S054] | Sahte SSS'li saç kartı, CC0 | 2021.3 denemeli; geliştirilmeyecek; ağır |
| Asset Store "URP Skin Shaders" [A02-S030] | 12,49 $ | Özellik açıklaması boş; denenmedi |
| Reallusion gölgelendiricileri [A02-S031, S032] | CC karakterleri için | URP'de saç anizotropi ve kırışık eklendi; HDRP "Digital Human" daha kapsamlı |
| Strand saç (com.unity.demoteam.hair) [A02-S055] | URP destekli, compute shader ister | Bakım durumu belirsiz; iGPU performansı ölçülmedi |

**Bileşen bileşen (ÇIKARIM; genel grafik bilgisi, ayrıca kaynakla doğrulanmadı)**
| Bileşen | Yakın planda ne gerekir | Şu an | Öneri |
|---|---|---|---|
| Cilt | SSS (veya pre-integrated), bölgeye göre pürüzlülük, ince detay normal | düz Lit | önce kafaya pre-integrated cilt; yüz ışığında yumuşak ana ışık |
| Göz | ıslak parlama (catchlight), kornea kırılması/paralaks, göz kapağı gölgesi, göz küresi hareketi | özel işlem yok | paralaks göz gölgelendiricisi; kameraya göre küçük "göz ışığı" |
| Saç | anizotropik parlama, yumuşak kenar | sert alpha-clip | MSAA + alpha-to-coverage veya titrek (dither) geçiş, sonra saç gölgelendiricisi |
| Kirpik/kaş | ince, titremeyen alpha | alpha-clip 0.35 | eşiği karakter bazında ayarla; 2 kat çözünürlükte render alıp küçültmeyi dene |
| Diş/ağız | ağız içi karanlık, dişte aşırı beyaz/parlama olmaması, dil | ayrı işlem yok | ağız içine yapay gölge; Rocketbox'ta diş/dil ayrı mesh mi DOĞRULANAMADI |

- Gerçek kullanıcı örneği: CC5 karakteri URP'de "mumsu, gözleri cansız" göründü şikâyeti, çözüm yazılmadı [A02-S034]. ÇIKARIM: iyi model tek başına yetmiyor; gölgelendirici ve ışık şart.

### S3. Yüz anatomisi, ARKit 52, mikro ifadeler
- ARKit'in 52 şekli fiili standart; MediaPipe webcam'den aynı 52 katsayıyı üretir [A02-S049].
- Algı araştırması: üst yüz hareketi kaldırılınca karakter daha az çekici ve gerçek bulunuyor, duygu zayıflıyor; foto görünüm "sosyal mevcudiyeti" artırıyor (tek karakter, küçük etki) [A02-S051].
- Daha gerçek görünen karakter kötü harekette daha ürkütücü bulunuyor [A02-S052]. ÇIKARIM: görünüm yükseltmesi, hareket ve yüz düzelmeden yapılırsa izlenimi **kötüleştirebilir** (A03 ile ortak).
- Gerçek oyuncudan aktarılan yüz ifadeleri, sanatçının elle yaptığına göre duyguyu daha az okunur kılabiliyor [A02-S056; yalnız özet].
- Gerçekçi çizim tarzı her zaman üstün değil: aynı hareket çizgi tarzda daha doğal bulunmuş [A02-S057].

**Duygu hazır ayarı önerisi** (FACS/EMFACS genel bilgisi; bu oturumda birincil kaynaktan doğrulanmadı. Değerler başlangıç tahminidir)
| Duygu | ARKit bileşimi |
|---|---|
| mutlu (içten) | mouthSmile 0.7, cheekSquint 0.6, eyeSquint 0.25 |
| uzgun | browInnerUp 0.7, mouthFrown 0.6, mouthShrugLower 0.3, bakış aşağı |
| sasirmis | browInnerUp+browOuterUp 0.8, eyeWide 0.7, jawOpen 0.35 |
| korkmus | browInnerUp 0.8, browDown 0.3, eyeWide 0.9, mouthStretch 0.5 |
| kizgin | browDown 0.9, eyeSquint 0.4, eyeWide 0.3, mouthPress 0.5, noseSneer 0.3 |
| igrenme | noseSneer 0.8, mouthUpperUp 0.5, browDown 0.4 |

Mikro ifade için (ÇIKARIM): sol-sağ küçük asimetri; ifadeye kısa gecikmeli giriş ve yavaş çıkış; konuşmadan 0,2–0,5 sn önce kaş/nefes hazırlığı; ara sıra 0,1–0,2 sn'lik kaçamak ifade; hepsi tohumlu rastgelelikle.

**Yüz yakalama:** MediaPipe (52 katsayı) web/Python'da var [A02-S049]; dilin dışarı çıkması yakalanamayabiliyor [A02-S058]. Audio2Face-3D açık hale getirildi [A02-S059] ama NVIDIA GPU için tasarlandı [A02-S050]; bu bilgisayar için uygun görünmüyor (CPU'da çalışması DOĞRULANAMADI).

### S4. MetaHuman → Unity: gerçekten mümkün mü?
**Lisans: evet.** Haziran 2025'ten beri MetaHuman başka motorlarda ve araçlarda kullanılabiliyor; yıllık geliri 1 milyon $ altındakilere ücretsiz, üstünde koltuk lisansı (1.850 $/yıl) [A02-S001, S002, S003]. AI modeli eğitmek/geliştirmek için kullanmak yasak; AI içeren iş akışında kullanmak serbest [A02-S003, S004].

**Teknik: evet, ama zahmetli ve yeni.**
- Epic, Rig Logic ve DNA kütüphanelerini MIT ile açtı (Temmuz 2026): UE dışında aynı yüz rig'i çalıştırılabilir [A02-S012, S013, S014].
- DCC Export: kafa ve gövde DNA + dokular [A02-S009]. **Saç (groom) ve kıyafet bu pakette yok** [A02-S012]. Meshler ayrı "Geometry Export" ile [A02-S009].
- LOD: 8 seviye; LOD0 kafa 24.000 köşe, 713 eklem, 669 blendshape; blendshape yalnız LOD0'da [A02-S010].

**Ne kaybolur / yeniden kurulmalı**
| Parça | Durum |
|---|---|
| UE cilt/göz/diş malzemeleri | Taşınmaz; Unity'de yeniden kurulur (ÇIKARIM; forum da özel SSS/saç gölgelendiricisi gerektiğini söylüyor [A02-S018]) |
| Strand saç, kaş, kirpik | Ayrı aktarım (Alembic groom veya kart); Unity'de en zayıf halka |
| Yüz rig'i | Rig Logic entegrasyonu gerekir; yoksa yalnız pişirilmiş animasyon/kemik kalır |
| Kırışık haritaları | Rig Logic dış parametreyle sürülür [A02-S011]; gölgelendirici bağlanmalı |
| Kıyafet | Ayrı aktarılıp giydirilir |
| LOD | Araca bağlı; video için LOD0 sabitlenmeli |

**Unity araçları (2026-11)**
- **MHBridge Importer**: 79,99 $, URP+HDRP, Rig Logic, strand saç, LOD, malzeme üretimi iddiası [A02-S015, S016]. 16 Kasım 2026'da duyuruldu; **hiç kullanıcı yorumu yok**; mağazada sürüm 2022.3; Unity 6.6 uyumu bilinmiyor; demo yalnız Windows.
- **MetahumanURP** (açık kaynak, MIT): Unity 6000.6 + URP 17.6 hedefli ama yalnız Apple/Metal'de denenmiş; yüz eklentisi macOS için [A02-S017]. Bu PC için kullanılamaz görünüyor.
- Elle yol (Blender üzerinden): forumda önerilmiş, deneyen az; "zahmetli, güncellemelerde bozulur" uyarısı [A02-S018].

**Üretim donanımı:** MetaHuman Creator için önerilen 16 çekirdek, 32 GB, RTX 3070 / RX 6800 XT 8 GB [A02-S007]; Epic hesabı ve bulut auto-rig/doku hizmeti gerekir [A02-S008]. **Bu bilgisayar (6 çekirdek, tümleşik grafik) öneriyi karşılamıyor.** ÇIKARIM: karakter bir kez başka/kiralık makinede üretilip dosyaları Unity'ye getirilebilir; denenmedi. Web uygulamasının 5 Kasım 2026'da kapandığı yalnız zayıf bir blogda yazıyor [A02-S019, D].

### S5. Character Creator 4/5
- CC5 çıkışı 27 Ağustos 2025: HD ifade profili, kırışık yer değiştirme, 4K/8K cilt, MetaHuman ile ortak yüz kontrol yapısı [A02-S060]. HD profil: **262 blendshape + 128 düzeltici morph** [A02-S061, B].
- İnceleme puanı 4,5/5; eksiler: ek içerik maliyeti birikiyor, arayüz yeni başlayana karışık [A02-S061].
- Fiyat: 299 $ (çıkış) [A02-S036]; Mayıs 2026 incelemesinde 498 $ (Headshot 3 Creator dahil) veya yıllık faturalı 8,25 $/ay [A02-S061]. Satın almadan önce güncel fiyat kontrol edilmeli.
- Donanım: asgari **8 GB VRAM ayrı kart**, 16 GB RAM, 30 GB kurulum + 200 GB önbellek [A02-S037]. Bu PC'de 45 GB boş: **yetmez**.
- Unity: ücretsiz Auto Setup (v2.2.6), URP ve HDRP, Unity 2022.3–6000.3 [A02-S031]. Açık kaynak içe aktarıcı sürdürülüyor [A02-S032]. Proje 6000.6 → resmî listede yok.
- Kullanıcı deneyimi: URP'de eksikler/hatalar, HD karakter içe aktarmanın ~64 GB RAM isteyebileceği (geliştirici notu) [A02-S033]; URP'de göz/kaş titreşimi (çözüm doğrulanmadı) [A02-S035]; "mumsu" görünüm [A02-S034].

### S6. Diğer seçenekler
- **MPFB 2 (Blender, MakeHuman):** sürüm 2.0.16 (Haz 2026), GPLv3 eklenti, Blender 4.2+, parametrik gövde, otomatik rig [A02-S042]. Üretilen içerik CC0 [A02-S062]. ARKit "Faceunits 01" ve viseme paketleri CC0; bazı saç/kıyafet paketleri CC-BY (atıf ister) [A02-S043]. Gerçekçilik düzeyi için bağımsız test bulamadım. ÇIKARIM: cilt dokusu ve saç kalitesi CC5/MetaHuman'ın altında kalır; özgün karakter ve kontrol için değerli.
- **Faceit** (Blender): herhangi karaktere ARKit 52 üretir; 78–99 $ [A02-S063].
- **VRoid:** ticari kullanım serbest; birleştirilmiş VRoid mesh'lerinden model üreten bir uygulama yapmak ayrı pixiv lisansı ister [A02-S044]. Anime tarzı; fotogerçekçilik hedefine uymaz.
- **Daz 3D:** video/görsel render için ek lisans gerekmez; gerçek zamanlı motor/uygulama için ürün bazında Interactive License gerekir [A02-S047]. ÇIKARIM: Unity'ye alınacaksa gerekli sayılmalı. Köprü: topluluk çatalı Unity 6 iddiasında; Daz Studio 6 ile çalışmıyor [A02-S048].
- **Selfie avatar servisleri:** Ready Player Me 31 Ocak 2026'da, Union Avatars Temmuz 2026'da kapandı; MetaPerson/Avaturn 800 $/ay [A02-S046, satıcı blogu]. ÇIKARIM: bulut avatar servisine bağımlılık risk; önerilmez.
- **Mixamo:** ücretsiz, ticari projede telifsiz [A02-S064]. Yüz şekli ve yeniden dağıtım koşulu o sayfada yazmıyor.
- **Google GNM Head:** Apache 2.0 parametrik kafa, göz/diş/dil dahil (2026) [A02-S045]. Araştırma altyapısı; hazır karakter değil, ARKit uyumu belirtilmemiş.

### S7. Tutarlılık, kıyafet, materyal
ÇIKARIM (kaynak yerine mimari akıl yürütme): 3D'de aynı prefab = aynı yüz; yapay zekâ videosundaki "yüz değişiyor" sorunu yoktur. Tutarlılığı bozacaklar ve önlemleri:
- Yüz davranışı rastgele → hikâye+karakter+satırdan tohumla.
- LOD/doku kalitesi değişimi → kayıtta en yüksek LOD, doku akışı kapalı.
- Işık sahneden sahneye cildi farklı gösterir → karaktere sabit yüz ışığı düzeneği (A04).
- Varlık güncellemesi → "karakter kartı" (model sürümü, kıyafet, dokular, ses, boy, duygu ölçekleri) ve referans kare karşılaştırması.
- Kıyafet: Rocketbox'ta sınırlı [A02-S021]; renk/doku varyantı ucuz yol. MPFB giysileri CC0/CC-BY [A02-S043]. CC5 giydirme güçlü (denenmedi). Kumaş benzetimi eklentisi (ör. Magica Cloth 2) forumda saç için olumlu anılmış, ama bir kullanıcı ciddi hız düşüşü de bildirmiş [A02-S033].
- Fantastik kıyafet (zırh, pelerin) örnek video tarzı için ayrı eksik; Rocketbox meslek/günlük kıyafet ağırlıklı (ÇIKARIM, klasör adlarından).

### S8. Donanım ve maliyet
| İş | Bu PC (tümleşik Radeon) | Maliyet |
|---|---|---|
| Rocketbox yüz eşleme düzeltmesi | Yapılabilir | 0 (emek) |
| URP cilt/göz/saç gölgelendirici | Yapılabilir; kayıt kare kare olduğu için yavaşlık tolere edilir (ÇIKARIM; ölçülmedi) | 0 – ~100 $ |
| MPFB ile karakter üretimi | Büyük olasılıkla yapılabilir (ÇIKARIM; Blender gereksinimi kontrol edilmedi) | 0 |
| MetaHuman üretimi | Öneri karşılanmıyor [A02-S007] | yazılım 0 (<1M $) + güçlü PC |
| MetaHuman oynatma (Unity) | Bilinmiyor; LOD0 ağır | MHBridge 79,99 $ |
| CC5 | Asgari karşılanmıyor [A02-S037] | 299–498 $ veya abonelik + GPU + disk |
| Strand saç | Riskli; ölçülmeli | 0 |

### S9. Lisans özeti (A12 denetlemeli; hukukçu görüşü değildir)
| Kaynak | Videoda ticari | Uygulama içinde dağıtım | Sınıf |
|---|---|---|---|
| Rocketbox | Evet (MIT) | Evet, lisans metniyle [A02-S020] | Güvenli |
| MPFB çıktısı | Evet (CC0) | Evet; CC-BY paketlerde atıf [A02-S043, S062] | Güvenli / koşullu |
| VRoid | Evet | Karakter üretici yapılırsa ayrı lisans [A02-S044] | Koşullu |
| MetaHuman | Evet (<1M $ ücretsiz) | "Non-Engine Product" serbest; **kullanıcıya karakter seçtiren araçta kütüphane olarak dağıtım DOĞRULANAMADI** (EULA ana metni okunamadı) [A02-S004, S005] | Koşullu |
| Reallusion | Evet (Standard) [A02-S038] | Yalnız çıkarılamaz biçimde paketlenmişse; araç içi karakter kütüphanesi **Enterprise** [A02-S038, S039] | Koşullu |
| Daz | Render evet | Interactive License [A02-S047] | Koşullu |
| Defender demo karakteri | Hayır [A02-S028] | Hayır | Kullanma |

### S10. Gelecek (1–3 yıl; ayrıntı A11)
- Rig Logic'in açılması MetaHuman'ın UE dışı ekosistemini büyütebilir (ÇIKARIM) [A02-S012, S014].
- Açık parametrik kafa modelleri (GNM) [A02-S045].
- Gaussian splatting avatarlar araştırmada yüksek gerçeklik veriyor [A02-S052]; Unity'de üretim yolu araştırılmadı.
- Ses → yüz modelleri açılıyor [A02-S059]; GPU gerektiriyor.
- Mimari açık kalmalı: duygu/viseme'i **ARKit 52 + 15 viseme ara katmanı** olarak tutmak; Rocketbox, MPFB, CC5, MetaHuman arasında geçişi kolaylaştırır (ÇIKARIM).

---

## Teknoloji kayıtları
Doğrulama tarihi hepsi 2026-11-27. Test durumu hepsi **TEST EDİLMEDİ** (Rocketbox projede kullanımda).

**T1 Microsoft Rocketbox** · karakter kütüphanesi · hazır rigli insan + yüz şekilleri · iskelet + blendshape · Hikâye'ye katkı: bugün çalışan taban · Unity: URP/HDRP (malzeme elle) · donanım: hafif · maliyet 0 · lisans MIT · artı: serbest, 115 karakter, ARKit+viseme · eksi: düşük poligon, 2K doku, eski (2022), dar kıyafet · alternatif: MPFB, CC5 · kaynak S020, S021, S024 · güven A.

**T2 MetaHuman** · yüksek kalite dijital insan · UE içinde Creator + bulut auto-rig · DNA + Rig Logic · katkı: en yüksek yüz kalitesi potansiyeli · Unity: üçüncü taraf araçla; HDRP daha uygun (ÇIKARIM) · donanım: üretim RTX 3070 sınıfı · maliyet 0 (<1M $) · lisans UE EULA · artı: kalite, 669 şekil · eksi: aktarım kırılgan, saç/kıyafet ayrı, bulut bağımlılığı · alternatif: CC5 · kaynak S001–S019 · güven: lisans A, Unity aktarımı C/D.

**T3 MHBridge Importer** · Unity eklentisi · DCC export → prefab · OpenRigLogic · Unity URP/HDRP (2022.3 listeli) · 79,99 $ · Asset Store EULA · artı: tek adım iddiası · eksi: 2 haftalık, yorumsuz, tek geliştirici riski · alternatif: Blender yolu, MetahumanURP · S015, S016 · güven D (yetenek).

**T4 Character Creator 5** · karakter üretici · morph + HD yüz profili · FBX + Auto Setup · Unity URP/HDRP (≤6000.3 listeli) · 8 GB VRAM, 230 GB disk · 299–498 $ / abonelik · Reallusion EULA · artı: olgun üretim akışı, giydirme, MetaHuman uyumlu yüz kontrolleri · eksi: ücretli, GPU, URP'de pürüzler · alternatif: MPFB, MetaHuman · S031–S041, S060, S061 · güven A/B.

**T5 MPFB 2** · Blender eklentisi · parametrik insan · morph + rig + varlık paketleri · FBX/glTF ile URP/HDRP · düşük (ÇIKARIM) · 0 · GPLv3 eklenti, çıktı CC0 · artı: özgür, özgün karakter, ARKit paketi · eksi: gerçekçilik kanıtı zayıf, emek ister · alternatif: Rocketbox · S042, S043, S062 · güven A (lisans), kalite DOĞRULANAMADI.

**T6 VRoid** · anime karakter · VRM/UniVRM · 0 · pixiv koşulları · artı: kolay · eksi: gerçekçi değil · S044 · güven A.

**T7 Daz Genesis** · gerçekçi figür pazarı · köprü/FBX · ürün başı + Interactive License · artı: geniş içerik · eksi: lisans karmaşası, köprü kırılgan · S047, S048 · güven A/C.

**T8 Faceit** · Blender eklentisi · ARKit 52 üretimi · 78–99 $ · katkı: yüz şekli olmayan modeli konuşturur · S063 · güven B.

**T9 URP cilt/göz/saç gölgelendiricileri** (Defender demo, IllusionRP, itsFulcrum, mağaza) · katkı: görünümde büyük fark beklentisi (ÇIKARIM) · Unity 6.6 uyumu hiçbiri için doğrulanmadı · S027, S028, S030, S054 · güven C.

**T10 MediaPipe Face Landmarker** · webcam'den 52 katsayı · katkı: elle ifade/performans kaydı · 0 · model lisansı ayrıca kontrol edilmeli · S049, S058 · güven A.

---

## Karşılaştırma ve çelişkiler
| Ölçüt | Rocketbox | MPFB | CC5 | MetaHuman |
|---|---|---|---|---|
| Görsel tavan (ÇIKARIM) | orta-düşük | orta | yüksek | en yüksek |
| Yüz kontrolü | ARKit 52 + 15 viseme + FACS | ARKit + viseme paketleri | 262 + 128 | 669 şekil + eklemler |
| Unity yolu | çalışıyor | FBX, elle | olgun-orta | yeni/kırılgan |
| URP | evet | evet | evet (pürüzlü) | araç iddiası |
| HDRP | evet | evet | daha iyi | daha uygun |
| Bu PC'de üretim | hazır | evet (ÇIKARIM) | hayır | hayır |
| Maliyet | 0 | 0 | ücretli | 0 + donanım |
| Lisans rahatlığı | en rahat | rahat | koşullu | koşullu |

**Çelişkiler**
- Ç1: Bir blog "Unity/Godot için lisans yolu yok" diyor [A02-S019, D]; Epic'in 2025 değişikliği tersini söylüyor [S001–S003]. Karar: resmî/basın kaynakları geçerli.
- Ç2: Reallusion içerik sayfasında "Extended License" şartı var [S039]; Ağustos 2025 duyurusu kaldırıldığını söylüyor [S040]; EULA'da geçmiyor [S038]. Sayfa eski görünüyor (ÇIKARIM); satın almadan önce yazılı teyit.
- Ç3: CC5 fiyatı 299 $ ↔ 498 $ (farklı tarih/paket olabilir).
- Ç4: Rocketbox 2020 makalesi "yüz kemik tabanlı, blendshape yok" [S021]; 2022'de eklendi [S020]. Zaman farkı. Yerel dosyalarda gerçekten var mı: `yuz_test.txt` ile bakılmalı.
- Ç5: Vive şekil sayısı 30 ↔ 42 [S023]. Rocketbox yüz kemiği/gövde kemiği sayıları da makale içinde tutarsız [S021].

## Riskler
| ID | Risk | Olasılık/Etki | Önlem |
|---|---|---|---|
| A02-R1 | Yeni karakter sistemine geçip asıl kusurları (yüz, hareket) çözmemek | orta/yüksek | önce Rocketbox yüz testi |
| A02-R2 | Gerçekçilik artınca kötü hareket daha ürkütücü | orta/yüksek | A03 ile sıra: hareket+yüz, sonra görünüm |
| A02-R3 | MetaHuman Unity aracı terk edilir/kırılır | yüksek/orta | DNA+kaynak arşivle; ARKit ara katmanı |
| A02-R4 | Uygulama dağıtılırsa Reallusion/Daz/MetaHuman lisansı yetmez | orta/yüksek | dağıtımda yalnız MIT/CC0; A12 incelemesi |
| A02-R5 | iGPU'da SSS/strand saç kayıt süresini aşırı uzatır veya kararsız | orta/orta | ölçüm testi |
| A02-R6 | 45 GB disk yeni araçlara yetmez | yüksek/orta | A10 |
| A02-R7 | HDRP'ye yalnız cilt için geçip donmuş boru hattına bağlanmak | orta/yüksek | A09 kararı beklensin |
| A02-R8 | Bulut avatar/üretim servisinin kapanması | orta/orta | yerel dosya tabanlı karakter |

## Test önerileri
**Bu bilgisayarda (sıra önemli)**
1. T-A02-1: `menu: Hikaye/Yuz Testi` ve `menu: Hikaye/Cene Gorselleri` (araçlar projede var). Kabul: her `Gercek_*` için blendshape sayısı, kanal listesi, çene kemiği durumu kayıtlı. Ayrıca `gercek_rapor.txt` ile gerçek şekil adları; birden fazla LOD mesh'i açık mı bakılır.
2. T-A02-2: Tek karakter yakın plan, 6 duygu; mevcut eşleme ↔ genişletilmiş ARKit tablosu yan yana kare. Kabul: sessiz izleyen en az 3 kişiden duyguyu doğru bilme oranı artmalı.
3. T-A02-3: Ağız: yalnız jawOpen ↔ 15 viseme (A07 ile). İki kat çene kontrolü.
4. T-A02-4: URP/Lit ↔ cilt+göz gölgelendiricisi; aynı ışıkta 1080x1920 yakın plan. Ölçüm: kare başı süre, bellek, 10 sn kayıt süresi.
5. T-A02-5: Saç/kirpik: alpha-clip ↔ alpha-to-coverage ↔ 2x süper örnekleme; hareketli çekimde titreme.
6. T-A02-6: Tutarlılık: aynı hikâyeyi iki kez kaydet, kareleri karşılaştır.
7. T-A02-7 (kullanıcı onayıyla): 1 adet MPFB karakter ↔ 1 Rocketbox karakter.

**GPU veya ek harcama isteyen (kullanıcı kararı)**
- CC5 30 günlük deneme ile 1 karakter → Unity 6.6 URP.
- Güçlü/kiralık makinede 1 MetaHuman → DCC export → MHBridge (80 $) → Unity 6.6. Ölçüt: rig, saç, cilt çalışıyor mu; kayıt süresi.

## Öz denetim
- Hiçbir Unity testi yapılmadı; ad eşleme bulgusu kod okuması + Python kopyası.
- Yerel Rocketbox dosyaları bu ortamda yok; FACS ve SR şekillerinin tam adları, diş/dil/göz mesh yapısı, specular dokusu bilinmiyor.
- WebFetch sayfaları özetleyerek verdi; bazı sayfalar yalnız üst veri döndürdü (S008) veya cevapsız kaldı (S001).
- MetaHuman ana EULA metni okunamadı.
- Görsel kalite sıralaması karşılaştırmalı görüntüye dayanmıyor; kanıtsız ÇIKARIM.
- "Cilt/göz/saç" bileşen tablosu ve FACS tablosu genel bilgi; kaynaklandırılmalı.
- Gerçek kullanıcı deneyimi: CC için birkaç forum; MetaHuman-Unity için çok az; MPFB için yok.

## Eksik alanlar / açık sorular
Arama hakkı bittiği için aranamayanlar:
1. Asset Store/Fab hazır gerçekçi, ARKit'li karakter paketleri ve fiyatları (fantastik kıyafetli dahil).
2. Unity Shader Graph'ın hazır saç/göz örnekleri ve Unity 6.6 uyumları.
3. CC4/CC5 resmî yüz profili sayıları; Magica Cloth 2 fiyatı.
4. Rocketbox ve MPFB için gerçek kullanıcı deneyimi / kalite şikâyetleri.
5. FACS birincil kaynağı; diş/ağız gerçekçiliği çalışmaları.
Açık sorular (ana dosyaya): (a) Uygulama başkalarına dağıtılacak mı? Lisans seçimini belirler. (b) Güçlü ekran kartı alınacak/kiralanacak mı? (c) Hedef karakterler günlük mü fantastik mi? (d) MetaHuman kütüphanesini uygulama içinde sunmak serbest mi? (A12)

## Diğer ajanlardan beklenen / ortak konular
- A03: baş/boyun/göz kontrolünün animasyonla çakışması; hareket kalitesi görünümden önce.
- A07: 15 viseme + Türkçe ses-harf eşlemesi; Perlin ünlü seçimi; zaman damgalı sürüş.
- A04: yüz ışığı, göz parlaması, tonemapping'in cilde etkisi.
- A09: URP'de kalma; ek ScriptableRendererFeature (SSS) Unity 6.6 Render Graph + DX11 uyumu; kayıtta LOD/anti-aliasing ayarları.
- A10: iGPU ölçümü, disk, kiralık GPU.
- A12: MetaHuman, Reallusion, CC-BY paketleri, Asset Store EULA.
- A01/A08: yakın plan süresi; ağız zayıfken aşırı yakın plandan kaçınma, tepki planlarında yüz ifadesi.

## Öneriler (kanıtla)
**1. Karakter üretimi — kademeli**
- Şimdi: Rocketbox'ta kal (MIT, çalışıyor) [S020].
- Özgün ana karakter gerekirse: MPFB denemesi (CC0) [S043, S062].
- GPU alınırsa: önce CC5 ve MetaHuman'ı **tek karakterle yan yana** dene; sonra seç. Şimdiden birine karar verme. Eğilim (ÇIKARIM): Unity'de kalınacaksa CC5 daha az riskli; en yüksek yüz kalitesi hedefse MetaHuman, ama o noktada Unreal seçeneği de A09 ile tartılmalı.

**2. Aktarım** · Rocketbox: mevcut akış. Yeni kaynaklar: FBX + Humanoid; her karakter için otomatik kabul testi (şekil sayısı, kanal kapsamı, çene, göz kemiği, malzeme).

**3. Animasyon (yüz)** · ARKit 52 + 15 viseme ara katmanı; duygu tablosu veri dosyasında; çene tek yoldan (kemik veya şekil); FACS/SR çift sürüşünü engelle; asimetri ve zamanlama; tohumlu rastgelelik [P3, S024, S051].

**4. Görünüm** · Sıra: (a) göz parlaması ve göz gölgelendiricisi, (b) cilt, (c) saç/kirpik kenarı, (d) saç gölgelendiricisi. URP'de kal; HDRP'ye yalnız cilt için geçme [S025].

**5. Tutarlılık** · karakter kartı + sabit LOD + sabit yüz ışığı + tohum + referans kare kontrolü.

**Dürüst sınır (ÇIKARIM):** Rocketbox + iyi gölgelendirici + doğru yüz eşlemesi belirgin iyileşme sağlamalı, ama örnek videodaki fotogerçek yüz seviyesine ulaşmaz. O seviyeye en yakın 3D yol MetaHuman/CC5 sınıfı karakter + güçlü GPU'dur; bunun da aynı görünümü vereceği garanti değildir.
