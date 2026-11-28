# A06 — VFX ve Sinematik Efekt Uzmanı

## Görev tanımı
Protokol ALT AJAN 6 ve §9.6: VFX Graph, Shader Graph, parçacık, ateş/duman/sis/yağmur/kar/patlama/yıkım/büyü/aura/şimşek, hacimli efekt, karakter efektleri (parlayan göz/damar), ejderha, kalabalık, fizik, optimizasyon, hazır paketler ve lisans, önceden render (flipbook), efektlerin hikâyeye göre otomasyonu. Teslim: efekt kataloğu + efekt yönetim mimarisi (GPU'suz / GPU'lu).
Tarih: 27 Kas 2026. Kod yazılmadı, indirme/kurulum yapılmadı, **hiçbir efekt bu bilgisayarda denenmedi**. WebSearch: 28/28. Kaynaklar: `SOURCES/A06_kaynaklar.md`.

## Araştırma alanı ve soruları
- S1 Bugün projede efekt nasıl yapılıyor, neden zayıf görünüyor?
- S2 VFX Graph / Particle System / Shader Graph: Unity 6.6 URP'de durum, donanım şartı?
- S3 Parlayan göz ve damar nasıl yapılır?
- S4 Ateş, duman, patlama, hava, sis ve hacimli ışık; önceden render (flipbook)?
- S5 Ejderha: model kaynağı, lisans, uçuş?
- S6 Kalabalık?
- S7 Yıkım, fizik, büyü, aura, şimşek?
- S8 Hazır paketler ve lisans?
- S9 Efektler hikâyeden nasıl otomatik yönetilir?

## Bulgular

### S1 — Mevcut durum (kod okuması)
- `EfektKatmani.cs`: 63 `vfx:` efekti; hepsi **ekran üstü 2D UI** parçaları (kar, duman, patlama ışığı, şimşek, portal…). Derinlik, ışık ve sahne ile ilişkisi yok. D001 gözlemi bunu doğruluyor: "`vfx: parilti` ekran üstü düz yıldızlar".
- `FxLayer.cs`: mum, toz, yağmur, sis, büyü, gren, vinyet — yine 2D. `UnityEngine.Random` tohumsuz kullanılıyor (iki kayıt birebir aynı olmaz).
- `SinematikEfekt.cs`: gözyaşı ve nefes buharı yüz noktasının ekrana izdüşümüne 2D çiziliyor; yavaşlatma ve dondurma var (işe yarar, korunmalı).
- `HavaDurumu3D.cs`: 3D yağmur/kar/sis; tek dinamik ağ, `Sprites/Default`, CPU'da her kare güncelleniyor. Dörtgenler kameraya dönmüyor (sabit XY düzlemi). ÇIKARIM: kamera yana/üste geçince yağmur ve sis incelir veya "kâğıt" gibi görünür.
- `Yaratik.cs`: ejderha küre/kapsül/koni birleşimi, iskelet yok; D001: "oyuncak gibi ve küçük". Vampir gözleri küçük emissive küre.
- **Kritik:** 3D kamera `allowHDR=false` ve ARGB32 hedefe çiziyor (`Sahne3D.cs:96-100`). HDR kapalıyken 1'in üstü parlaklık tutulmaz [A06-S008]. `Yaratik.Parlak` 1,6–2,2 kat emission veriyor ama kırpılır. ÇIKARIM: örnek videodaki "parlayan altın göz" bugünkü ayarla çıkmaz; bloom (eşik 0,9) yalnız zaten beyaza yakın her şeyi hafifçe taşırır.
- Ana ışıkta gölge kapalı (`l.shadows = None`) — efekt ve yaratığın yere oturmasını da bozar (A04 ile ortak).
- **Olumlu:** kayıt sabit kare adımıyla yapılıyor (`Time.captureDeltaTime`, Recorder "Constant"). ÇIKARIM: efektler gerçek zamanlı 30 FPS olmak zorunda değil; ağır efekt videoyu takıltmaz, yalnız kayıt süresini uzatır. Bu, tümleşik kart için en önemli avantaj.

### S2 — Araçlar ve donanım şartı
- **VFX Graph:** parçacığı GPU'da simüle eder [A06-S023]; compute shader + SSBO şart; belgeye göre URP desteği "preview dışı değil", yalnız bazı platformlar [A06-S001] (17.3; 17.6 sayfası okunamadı). Donanım raporunda compute var, D3D11. ÇIKARIM: teknik olarak çalışmalı, ama **denenmedi**.
- Risk: 2021 tarihli forum kaydında tümleşik kartlarda D3D11 altında VFX Graph yükü çökmeye yol açmış [A06-S024] (eski, C sınıfı).
- Projede VFX Graph kurulu değil (paket listesi, TOPICS/00); eklemek kullanıcı onayı ister.
- **Built-in Particle System (Shuriken):** CPU simülasyonu, compute istemez. Unity 6.6 ile Shader Graph bu sistemle URP'de tam uyumlu parçacık gölgelendiricisi yapabiliyor (mesh instancing dahil, 5 yeni düğüm) [A06-S002]. Kullanımdan kaldırma duyurusu **görmedim** (yokluk kanıtı zayıf [A06-S003][A06-S004]).
- **Shader Graph:** göz/damar, aura, kalkan, erime (dissolve), ısı titremesi için ana araç. Proje gölgelendiriciyi şu an kodla/yansımayla kullanıyor; Shader Graph paketi URP'nin bağımlılığıdır (genel bilgi, manifest okunmadı).
- Unity 7 hedefi 2027 1. çeyrek [A06-S029, yalnız başlık]; efekt tarafında duyurulmuş kırıcı değişiklik görmedim [A06-S003].

### S3 — Parlayan göz ve damar
Önerilen katmanlar (hepsi ÇIKARIM, denenmedi):
1. **HDR'siz hızlı yol:** göz kemiğine bağlı, kameraya dönük toplamalı (additive) hale kartı + küçük menzilli altın nokta ışık. Bugünkü LDR hatta çalışır.
2. **Doğru yol:** 3D kamera HDR + emissive maske (göz irisi / damar deseni ayrı doku) + HDR renk yoğunluğu + URP Bloom.
3. **Damarın "yayılması":** gürültü + eşik parametresiyle maskeyi zamanla açan Shader Graph; `guc` değeri 0→1, ardından nabız.
4. Destek: pozlama/bloom darbesi, hafif kamera sarsıntısı, ses (`efekt: enerji`), yukarı süzülen kor parçacıkları. "Güç patlaması" hissini tek efekt değil bu birleşim verir.

Engeller:
- Saydam arka planlı RT'de post-process alfayı korumak için URP **Alpha Processing** açılmalı; HDR ile alfa istenirse HDR Precision 64-bit gerekir. Bloom/DoF nesne sınırı dışına taşamayabilir (keskin kenar) [A06-S008]. 3D mekân tam kareyi doldurduğunda sorun olmayabilir — test gerekir.
- HDR açmak mevcut renk ön ayarlarının görünümünü değiştirir (A04 ile birlikte yapılmalı).
- Rocketbox MIT lisanslı, doku değiştirilebilir [A06-S019]; ancak göz/baş UV düzeni **DOĞRULANAMADI** (dosyaya bakılmalı). VRoid/VRM (MToon) ve glTF malzemelerinde özellik adları farklıdır; her aile için ayrı eşleme gerekir.

### S4 — Ateş, duman, patlama, hava, hacimli efekt
**Flipbook (önceden simüle edilmiş kare dizisi)** — bu bilgisayar için en yüksek kalite/maliyet oranı (ÇIKARIM): ağır hesap bir kez yapılmış olur, oynatmak ucuzdur.
- Unity Labs Paris'in Houdini ile ürettiği duman/ateş/patlama dizileri **CC0** (2016) [A06-S007]. Başlangıç için en temiz lisans.
- Unity "Particle Pack" ücretsiz, URP uyumlu, Asset Store EULA [A06-S018].
- Kendi üretimimiz: Blender (Mantaflow) simülasyonu → kare dizisi → sprite sheet. Cycles isteyen "Sequenced Bake" eklentisi var [A06-S026]; hacim simülasyonu için uygunluğu ve bu CPU'daki süre **DOĞRULANAMADI**.
- EmberGen: ekran kartı ister (asgari GTX 1060 / AMD ayrık), indie 210 $ ilk yıl [A06-S020] → GPU planı.
- **Six-way lighting:** dumanın sahne ışığına tepki vermesi; URP'de VFX Graph "Six-Way Smoke Lit" çıkışı [A06-S006]. Shuriken'de hazır karşılığı doğrulanamadı.

**Hacimli sis/ışık:** URP yol haritasında yerleşik volumetric fog görmedim [A06-S004]. Seçenek: CristianQiu URP Volumetric Light (MIT, 6000.6'ya kadar, DX11; saydam nesnelerle doğru karışmıyor, fog sınırlı mesafe) [A06-S005]. Yeni paket sayılır → onay. Tümleşik kartta maliyeti bilinmiyor. Ucuz karşılığı: mesafe sisi + yumuşak parçacık sis katmanları + ışık huzmesi kartları.

**Yağmur/kar:** Shuriken'e taşınıp kameraya dönük, derinliğe göre solan parçacık; yere sıçrama, ıslak zemin (pürüzlülük düşür, A04). Kar/yağmur için video içine gömülü 2D katman yalnız en ön "lens" katmanı olarak kalmalı.

### S5 — Ejderha ve yaratık
| Kaynak | Fiyat | Lisans | Uçuş anim. | URP | Not |
|---|---|---|---|---|---|
| Dungeon Mason "Dragon for Boss Monster: HP" [A06-S010] | ücretsiz | Asset Store EULA | doğrulanamadı | doğrulanamadı | 2021; ilk bakılacak aday |
| Stylized Fantasy Dragons [A06-S012] | 42,49 $ (indirimli) | Asset Store EULA | doğrulanamadı | uyumlu (6000.0) | stilize; gerçekçi insanla çelişebilir |
| 3D-Disco Realistic Dragons [A06-S011] | 229,99 $ | Asset Store EULA | doğrulanamadı | listede yok | önerilmez |
| Dragon Pack Bundle 1 [A06-S013] | 119,99 $ | Asset Store EULA | doğrulanamadı | yok | önerilmez |
| Quaternius monsters [A06-S014] | ücretsiz | CC0 | "flying" var | FBX | low-poly; ejderha olduğu doğrulanamadı |
| Sketchfab / CGTrader modelleri | değişir | model başına (CC-BY vb.) | değişir | — | yalnız arama başlığı; tek tek lisans denetimi gerekir |

- Gerçekçi + iyi uçuş animasyonlu + net lisanslı **ücretsiz** ejderha bu araştırmada bulunamadı/doğrulanamadı. Satın almadan önce sayfadan animasyon listesi ve video elle kontrol edilmeli.
- URP uyumsuz görünen paketlerde model yine kullanılabilir, malzemeler elle dönüştürülür (genel bilgi).
- Uçuş yöntemi (ÇIKARIM): (1) iskeletli `fly/glide/roar` klipleri Animator ile; (2) gövde yörüngesi ayrı: spline/Bezier yol, dönüşte yatış (bank), dalışta kanat toplama; (3) büyüklük hissi: yavaş kanat ritmi, alt açı, uzaklık sisi, zemine gölge, geçişte toz + sarsıntı + kükreme; (4) nefes ateşi: ağız kemiğine bağlı parçacık + flipbook + turuncu ışık.
- Kodla yapılan mevcut ejderha yalnız uzak silüet olarak kalabilir.

### S6 — Kalabalık
- GPU Resident Drawer yalnız Mesh Renderer'ı kapsar; Animator hiyerarşisindekiler hariç [A06-S015]. ÇIKARIM: animasyonlu karakterlere kendiliğinden hız kazandırmaz.
- Rocketbox'ta midpoly/lowpoly/ultralowpoly mesh seçenekleri var [A06-S019].
- Üç halkalı öneri (ÇIKARIM):
  - Yakın (0–8): normal karakter, farklı klip fazı/hızı.
  - Orta (10–60): lowpoly SkinnedMeshRenderer; animasyon güncellemesi seyrek.
  - Uzak (100+): VAT/animasyon dokusu + instancing, ya da karakterden uygulama içinde üretilen sprite (impostor).
- VAT: OpenVAT ücretsiz/açık kaynak, Blender→URP [A06-S016] (lisans adı doğrulanmalı). Unity'nin 2018 örneği doku-skinning ile mobilde 30 FPS karakter sayısını ~150'den 900'e çıkarmış [A06-S017]; eski ve bu donanıma aktarılamaz, yalnız yöntemin yararını gösterir.
- Çevrim dışı kayıt sayesinde 30–60 gerçek karakter bile kayıt süresi pahasına mümkün olabilir; ölçülmeli. Gölge ve skinning maliyeti en büyük kalem.
- Diz çökme/kaçışma: ortak klip + rastgele gecikme (±0,5 sn), yön ve boy çeşidi. Klipler A03'ten.

### S7 — Yıkım, fizik, büyü, aura, şimşek (ÇIKARIM; yöntem önerisi)
- **Yıkım:** çalışma anında kesme yerine Blender'da önceden parçalama (Cell Fracture, GPL, 4.2+ [A06-S021]) → Unity'de patlama kuvvetiyle Rigidbody; parçalar gizli başlar, olayda sağlam model kapanır. Tekrarlanabilirlik için sonuç animasyon olarak pişirilebilir. Açık kaynak Unity parçalama depoları da var [A06-S030, okunmadı]. Nesne detayı (çatlayan fincan) aynı yöntemle.
- **Büyü/aura:** Fresnel kenar ışıması veren ikinci kabuk + yerde dönen halka (decal/quad) + yükselen parçacık + renkli nokta ışık.
- **Şimşek:** orta-nokta kaydırma ile üretilen çizgi ağı (LineRenderer), 2–3 karelik yanıp sönme, ışık ve pozlama darbesi, gecikmeli gök gürültüsü. Tamamen prosedürel, varlık gerekmez.
- **Patlama:** flipbook + kıvılcım + duman + şok halkası + ışık darbesi + sarsıntı + 2–4 karelik donma + ses.
- **Işın/enerji topu, kalkan, portal:** Shader Graph (kaydırılan gürültü dokusu).

### S8 — Paket ve lisans
- Asset Store: varlık ürüne gömülü olmalı; **son kullanıcıların kendi dijital ürününü yaptığı uygulamalar için yayıncıdan ayrı izin** gerekir [A06-S009]. Hikâye Stüdyosu başkalarına dağıtılacaksa (EXE) bu doğrudan ilgilidir. Yalnız kullanıcının kendi videolarında kullanım için SSS açık cümle kurmuyor. → A12 kesinleştirmeli.
- Fab: Unreal dışında kullanılabilir, tek başına yeniden dağıtılamaz [A06-S022]; kullanıcı-üretimi maddesi okunmadı.
- En güvenli sıra: CC0 (Unity Labs flipbook, Quaternius) > MIT (Rocketbox, volumetric repo) > mağaza EULA > model başına CC-BY (atıf listesi tutulmalı).
- GPL araç (Cell Fracture) kullanmak üretilen mesh'i genel olarak GPL yapmaz (genel bilgi; A12 doğrulamalı). GPL kod uygulamaya gömülmemeli.

### S9 — Hikâyeye göre otomasyon
- Akademik taraf (ör. EffectMaker) efekti **video difüzyonla** üretiyor: büyük veri merkezi kartı, klip başına dakikalar, motor varlığı vermiyor [A06-S027]. Kullanıcının "yapay zekâ video üreticisi yok" şartına da aykırı → kullanılmaz.
- ÇIKARIM: doğru yol **kural tabanlı efekt tarifi**. Hikâye zaten etiketli; yapay zekâ yalnız metni yazarken komutları önerir (mevcut köprü akışı), render sırasında model gerekmez.

## Efekt kataloğu (öneri)
Öncelik: P1 = örnek videodaki hedefler; P2 = sık hikâye ihtiyacı; P3 = sonra. "T0" bugünkü 2D katman, "T1" tümleşik kartta hedef, "T2" ayrık kartta ek. Hiçbiri denenmedi.

| Aile | Tarif | Bugün | T1 (GPU'suz) | T2 (GPU'lu) | Öncelik |
|---|---|---|---|---|---|
| Karakter | goz_parlamasi | yok (vampir küresi) | hale kartı + ışık; HDR+bloom denemesi | emissive maske, ısı titremesi | P1 |
| Karakter | damar_parlamasi | yok | emissive maske + yayılma shader | + yüzeyden kıvılcım (VFX) | P1 |
| Karakter | aura / guc_patlamasi | 2D `altin`, `sok_dalgasi` | kabuk + parçacık + ışık + sarsıntı | yoğun VFX Graph, ışık kırılması | P1 |
| Karakter | gozyasi, nefes | var (2D izdüşüm) | koru; sonra 3D | ıslak göz shader | P3 |
| Yaratık | ejderha_ucus / inis / kukreme | oyuncak model | iskeletli model + yol + gölge | yüksek poli, doku | P1 |
| Yaratık | ejderha_atesi | yok | flipbook + parçacık + ışık | VFX Graph + six-way | P1 |
| Kalabalık | meydan, diz_cokme, kacisma | yok | 3 halka (skinned + sprite) | VAT/instancing yüzlerce | P1 |
| Savaş | toz, kivilcim, kilic_carpmasi | 2D `kivilcim`, `darbe` | 3D parçacık + sarsıntı | GPU yoğun | P1 |
| Duman/ateş | duman, yangin, mesale, kor | 2D `duman`, `kor` | CC0 flipbook parçacık | six-way, kendi simülasyonlar | P1 |
| Işık | isik_patlamasi, huzme, simsek | 2D `patlama_isik`, `simsekler` | 3D ışık darbesi + prosedürel şimşek + bloom | hacimli huzme | P1 |
| Hava | yagmur, kar, sis, firtina | 3D quad + 2D | Shuriken, kameraya dönük, zemin teması | VFX Graph, volumetrik | P2 |
| Yıkım | duvar_kirilmasi, fincan_catlagi, enkaz | 2D `cam_catlagi` | ön parçalanmış mesh + fizik | daha çok parça + toz | P2 |
| Büyü | portal, kalkan, enerji_topu, isinlanma | 2D `portal` | Shader Graph | VFX Graph | P2 |
| Ortam | toz_zerresi, yaprak, kul, ates_bocegi | 2D | 3D parçacık (derinlikli) | — | P3 |
| Anlatım | kalp, soru, unlem, nota, ofke, konfeti | 2D | **2D kalsın** (komik/anlatım katmanı) | — | — |
| Post/renk | filtre, gren, vinyet, yavas, dondur, glitch | var | koru | — | — |

## Efekt yönetim mimarisi (öneri; ÇIKARIM)

```
Hikaye komutu (vfx:/guc:/yaratik:/kalabalik:) + duygu etiketi + mekan/zaman
        v
EfektYonetmeni  -> tarif seçer, kalite katmanı seçer, çakışma/bütçe denetler
        v
EfektTarifi (veri): moduller[] = parçacık | flipbook | shader parametresi |
                    ışık | post darbesi | kamera sarsıntısı | ses | zaman (yavas/dondur)
        v
Bağlama noktası: Karakter(goz, el, gogus, ayak) | Yaratik(agiz) | Dunya | Zemin | Gok | Ekran
```

İlkeler:
1. **Tek kayıt defteri:** her tarif `KomutKayit.Kaydet` ile; doğrulayıcı ve kılavuz tanısın. Eski 63 `vfx:` adı çalışmaya devam etsin (ad aynı, arka uç yükselir).
2. **Üç arka uç, tek ad:** T0 UI, T1 Shuriken+Shader, T2 VFX Graph. T2 yoksa T1'e, o da yoksa T0'a düşer. Seçim: `SystemInfo.supportsComputeShaders` + `maxComputeBufferInputsVertex` [A06-S001] + kullanıcı ayarı.
3. **Bileşik tarif:** `guc_patlamasi` = göz + damar + aura + ışık + bloom darbesi + sarsıntı + ses + yavaşlatma; zaman çizelgesi (0,0 gözler → 0,4 damar → 1,0 patlama → tepki planı). Tepki planı A08'e olay olarak bildirilir.
4. **Belirlenimci:** her efekte hikâye satırından türeyen tohum; sabit zaman adımı; `dondur`/`yavas` bütün modüllere aynı saatten.
5. **Bütçe:** plan başına en çok 1 "büyük" efekt + 2 ortam; saydam üst üste çizim (overdraw) sınırı — dikey tam ekran dumanda ana maliyet budur.
6. **Işıkla bağ:** her ışıyan efekt gerçek bir ışık da yaksın; sahneye "oturmayı" bu sağlar.
7. **Renk dili:** kahraman gücü sıcak altın, dünya soğuk gri (VideoKalitesi gözlemi); renk tarif parametresi.
8. **Varlık manifesti:** her dış doku/model için kaynak, lisans, atıf alanı.
9. **Ön ısıtma:** gölgelendirici ve sürekli efektler kayıttan önce hazırlanır (ilk karede boş sis olmasın).

Örnek hikâye komutları (öneri, mevcut değil): `guc: Arda altin 3`, `yaratik: ejderha gecis sol_sag gok`, `yaratik: ejderha ates hedef=meydan`, `kalabalik: 40 diz_cok`, `yikim: duvar`.
Duygu → otomatik hafif destek (kapatılabilir): ofke → kızıl kenar + kor; korku → soğuk sis; büyük açıklama → ışık darbesi. Büyük efekt yalnız açık komutla.

### GPU'suz plan (bu bilgisayar)
HDR/bloom denemesi · Shuriken + Shader Graph · CC0 flipbook · prosedürel şimşek/aura · iskeletli ejderha (tek) · küçük-orta kalabalık · önceden parçalanmış yıkım · kayıt süresine razı olma. Yeni paket gerekmeyebilir.

### GPU'lu plan (ayrık kart)
VFX Graph (six-way duman, on binlerce parçacık) · hacimli sis/ışık · VAT ile büyük kalabalık · EmberGen veya Blender'da yüksek çözünürlüklü kendi flipbook'ları · süper örnekleme. ÇIKARIM: kart efekt **sayısını ve yoğunluğunu** artırır; ejderha modeli, animasyon klibi ve sanat ayarı kalitesini kendiliğinden getirmez.

## Teknoloji kayıtları
Test durumu hepsi için **TEST EDİLMEDİ**; doğrulama 2026-11-27.

| Ad | Kategori / iş | Katkı | Uyum / donanım | Maliyet, lisans | Artı / eksi | Alternatif | Kaynak | Güven |
|---|---|---|---|---|---|---|---|---|
| Built-in Particle System | CPU parçacık | T1 ana araç | URP; compute gerekmez | ücretsiz, Unity | basit, ışık/fizik ile bağlı / sayı sınırlı | VFX Graph | S002 | yüksek |
| VFX Graph | GPU parçacık | yoğun duman/büyü | compute+SSBO; URP preview | ücretsiz paket (onay) | güçlü / iGPU riski | Shuriken | S001,S023,S024 | orta |
| Shader Graph | gölgelendirici | göz, damar, aura | URP | ücretsiz | görsel düzenleme / .shadergraph dosyaları kodla zor üretilir (genel bilgi) | elle HLSL | S002 | yüksek |
| HDR + Bloom + emissive | post | parlama | HDR RT; hafif ek yük | ücretsiz | büyük etki / renk ayarı değişir, alfa tuzağı | hale kartı | S008 | yüksek (belge), proje için test gerek |
| CC0 Unity Labs flipbook | doku | ateş/duman/patlama | her donanım | CC0 | ücretsiz, temiz / 2016, düz görünebilir | Blender, EmberGen | S007 | yüksek |
| Particle Pack | hazır efekt | hızlı başlangıç | URP | ücretsiz, Asset Store EULA | hazır / oyun görünümü, EULA şartı | CC0 flipbook | S018,S009 | orta |
| URP Volumetric Light (repo) | hacimli sis | atmosfer | DX11; maliyet bilinmiyor | MIT | gerçek huzme / saydam sorunu, tek geliştirici | sis kartları | S005 | orta |
| OpenVAT / VAT | animasyon pişirme | kalabalık | Blender→URP | ücretsiz (lisans adı doğrulanacak) | çok ucuz çizim / geçiş yok, iş akışı | sprite impostor | S016,S017 | orta-düşük |
| Cell Fracture | önceden parçalama | yıkım | Blender 4.2+ | GPL-3.0+ | basit / elle iş | Unity açık kaynak depoları | S021 | yüksek |
| EmberGen | gerçek zamanlı simülasyon | kendi flipbook | ayrık GPU | 210 $ ilk yıl | hızlı / kart gerekir | Blender Mantaflow | S020 | orta |

## Karşılaştırma ve çelişkiler
1. `Docs/VideoKalitesi.md` "VFX Graph URP ve HDRP'de var" diyor; belge ayrıntısı: URP tarafı "preview dışı değil" [A06-S001]. İkisi de doğru olabilir; ana araştırmacı VFX Graph'ı "kesin" değil "koşullu" yazmalı.
2. Aynı belge parıltı/yıkım için "kısmen kart gerekir" diyor. ÇIKARIM: göz/damar, aura, şimşek, flipbook için kart gerekmez; engel HDR ayarı ve varlık/emek.
3. HDRP üstünlükleri (hacimli bulut/sis) artık plan dayanağı olamaz: HDRP'ye yeni özellik yok [A06-S004].
4. Kayıt çevrim dışı olduğu için oyun dünyasındaki "gerçek zaman performans" ölçütleri burada ikincil; ölçüt "1 sn video kaç sn'de kaydediliyor" olmalı (A09, A10).

## Riskler
| Risk | Etki | Önlem |
|---|---|---|
| Asset Store EULA: kullanıcı-üretimi uygulama maddesi | yüksek (dağıtımda) | CC0/MIT öncelik; yayıncı izni; A12 |
| HDR/Alpha Processing mevcut 2D birleştirmeyi bozabilir | orta | ayrı deneme, geri alınabilir ayar |
| VFX Graph tümleşik kartta kararsız | orta | T1 ana yol; VFX Graph yalnız testten sonra |
| Uygun ejderha bulunamaması / stil uyumsuzluğu | yüksek | satın almadan animasyon kontrolü; uzak plan + sis ile kullanım |
| Kalabalık + saydam efekt kayıt süresini aşırı uzatır | orta | bütçe, halka sistemi, ölçüm |
| Paylaşımlı bellek / 45 GB disk (EXR dizileri, modeller) | düşük-orta | sheet boyutu sınırı, A10 |
| Efekt çokluğu "ucuz" görünüm | orta | plan başına bir büyük efekt; A01/A08 denetimi |

## Test önerileri
**Bu bilgisayarda:**
1. VFX-T1 Göz parlaması: (a) bugünkü LDR + hale kartı, (b) HDR RT + emissive + bloom. Aynı yakın plan, kare karşılaştırması; kenarda alfa bozulması; kayıt süresi.
2. VFX-T2 Ateş/duman: CC0 flipbook + Shuriken + nokta ışık; `vfx: duman` 2D ile yan yana. Ölçü: sn/video-sn.
3. VFX-T3 Güç patlaması bileşik tarifi (6 sn): göz → damar → patlama → tepki. Başarı: kullanıcı onayı.
4. VFX-T4 Kalabalık: 10 / 30 / 60 lowpoly Rocketbox, gölgeli/gölgesiz; kayıt süresi ve bellek.
5. VFX-T5 Ejderha: ücretsiz aday modelin içe aktarımı, uçuş klibi var mı, URP malzeme, 8 sn geçiş planı.
6. VFX-T6 (paket onayıyla) VFX Graph 10k/100k parçacık, D3D11, 3 dk dayanıklılık; çökme/log.
7. VFX-T7 (onayla) Volumetric Light repo, 1080x1920 maliyeti.
**Ayrık GPU gerektiren:** EmberGen üretimi, yüzlerce VAT karakter + yoğun VFX Graph + hacimli efekt birlikte.

## Öz denetim
- Hiçbir efekt çalıştırılmadı; performans sayısı **yok** (bu donanım için güvenilir tümleşik kart ölçümü bulunamadı).
- Unity 6.6'ya ait VFX Graph 17.6 sistem gereksinimi sayfası okunamadı.
- Ejderha paketlerinin animasyon listeleri mağaza sayfalarından alınamadı.
- Shader Graph, şimşek, aura, yıkım yöntemleri büyük ölçüde genel bilgi/çıkarım; bu konularda kaynak zayıf (arama "glow shader" yararlı sonuç vermedi).
- Sketchfab/CGTrader ve ücretli VFX paketleri (ücretli patlama/büyü) tek tek incelenmedi.
- WebFetch özet araçlıdır; kritik lisans cümleleri A12 tarafından asıl metinden doğrulanmalı.
- Packages klasörü bu kopyada yok; paket listesi TOPICS/00'dan alındı.

## Eksik alanlar / açık sorular
- Q1: Rocketbox ve VRM karakterlerde göz/damar için UV ve malzeme yapısı nedir?
- Q2: Saydam RT + HDR + Bloom bu projede doğru birleşiyor mu?
- Q3: Gerçekçi, uçuş animasyonlu, dağıtıma izinli ejderha hangisi, kaça?
- Q4: Uygulama başkalarına dağıtılacak mı (lisans seçimini değiştirir)?
- Q5: Kabul edilebilir kayıt süresi nedir (ör. 1 dk video için kaç dakika)?
- Q6: Blender ile flipbook üretim süresi bu CPU'da ne kadar?
- Q7: Efektlerin ses eşleri yeterli mi (A07)?

## Diğer ajanlardan beklenen / ortak konular
- A04: HDR, bloom, gölge, emissive, ıslak zemin, sis. A09: HDR RT, alfa, paket ekleme, kayıt süresi, Unity 7. A10: kayıt süresi bütçesi, GPU seçimi. A03: yaratık klipleri, kalabalık/diz çökme. A01/A08: efekt-plan zamanlaması, tepki planları. A07: efekt sesleri. A05: mekân ölçeği (meydan + şato ejderha ve kalabalığa yetmeli). A12: Asset Store/Fab maddeleri, CC0/MIT/GPL.

## Öneriler (kanıtla)
1. Önce **HDR + bloom + göz parlaması denemesi** (kod: `allowHDR=false`; belge [A06-S008]). En görünür kazanç, en düşük maliyet.
2. 2D efektleri anlatım katmanına indir; ateş/duman/hava/patlama için **3D parçacık + CC0 flipbook + ışık** [A06-S002][A06-S007][D001].
3. **Efekt tarifi + EfektYonetmeni** mimarisini kur; mevcut `vfx:` adları korunsun.
4. VFX Graph'ı **koşullu T2** say; testten önce bağımlılık yaratma [A06-S001][A06-S024].
5. Kodla ejderhayı iskeletli modelle değiştir; satın alma öncesi animasyon ve lisans kontrolü [A06-S009–S013].
6. Kalabalıkta üç halka; GPU Resident Drawer'a güvenme [A06-S015].
7. Dış varlıklarda CC0/MIT öncelikli, lisans manifesti zorunlu [A06-S009].
8. Dürüst sınır: bu efektlerle iyi bir oyun sinematiği hedeflenebilir; örnek videodaki yapay zekâ üretimi görünümün aynısı beklenmemeli.
