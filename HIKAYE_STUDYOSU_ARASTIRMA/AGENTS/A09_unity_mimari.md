# A09 — Unity, render ve performans mimarisi

> Tarih: 2026-11-27. Kod değiştirilmedi, test çalıştırılmadı. Proje bulguları **kod okumasıdır**.
> Kaynaklar: `SOURCES/A09_kaynaklar.md`. `[S007]` = A09-S007. "ÇIKARIM:" = benim yorumum.
> Not: web arama bütçesi (tüm ajanlarla ortak) araştırma sonunda doldu; açık kalanlar "Eksik alanlar"da.

## Görev tanımı
- Görev A: mevcut projeyi inceleme (mimari harita, güçlü/zayıf yanlar, darboğazlar, Cinemachine/Timeline, kayıt yolları, kare hızı/ses senkronu).
- Görev B: Unity render teknolojileri, offline render, FFmpeg, kuyruk/iptal/kurtarma, uyumluluk, alternatif mimariler.
- Teslim: mevcut projeyi koruyan veya kontrollü değiştiren mimari seçenekler.

## Araştırma alanı ve soruları
- S1 Mevcut mimari nasıl? Zayıf noktalar neler?
- S2 Kayıt yolları kare hızını ve ses senkronunu garanti ediyor mu?
- S3 URP mi HDRP mi? Unity'nin resmî yönü ne?
- S4 Unity sürümü ve uyumluluk riski ne?
- S5 Cinemachine, Timeline, VFX Graph, Shader Graph ne katar?
- S6 Offline kalite (süper örnekleme, motion blur) nasıl alınır?
- S7 FFmpeg, render kuyruğu, iptal, kurtarma nasıl olmalı?
- S8 CPU/GPU/bellek darboğazları neler?
- S9 UE5, Blender, hibrit yolların maliyeti ne?

---

# GÖREV A — PROJE İNCELEMESİ

Yollar `Assets/HikayeStudyo/` köküne göredir.

## A.1 Mimari harita

```
Hikâye metni (.txt)
  -> AYRIŞTIRMA   Scripts/StoryModel.cs (StoryParser), KomutKayit.cs; denetim: Uygulama/HikayeDenetci.cs, Editor/StudyoDogrulama.cs
  -> OYNATMA      Scripts/StoryPlayer.cs : Run() tek coroutine, switch(it.Type)  (337-506)
                  Say() = konuşma satırı (988-1070): süre = klip uzunluğu, yoksa harf/14 (1029)
  -> SAHNE        2D: uGUI "HikayeCanvas" (166-258) + CharacterView
                  3D: Scripts/Sahne3D*.cs, Karakter3D.cs (PlayableGraph), Mekan3D*.cs (kodla mekân), Yonetmen3D.cs
                  3D kamera -> "Sahne3D_RT" 1080x1920 MSAA4 (Sahne3D.cs:91) -> kanvasta RawImage (120-126)
  -> KAMERA       3D: Scripts/KameraYonetmen.cs (kendi yörünge modeli; Satir() 135)
                  2D: StoryPlayer.UpdateCamera (1223-1248) tüm "Dunya" RectTransform'unu ölçekler
  -> IŞIK/POST    Scripts/AtmosferVolume.cs: URP Volume'u YANSIMA ile kurar (584-634), yalnız 3D mekân açıkken (347, 503)
  -> SES          AudioSource x4 (240-246), ProcAudio*.cs, SesOlcer.cs (OnAudioFilterRead: kazanç + sınırlayıcı + LUFS)
  -> KAYIT        (1) Editör: Editor/StudyoKayit.cs -> Unity Recorder (yansıma), GameView 1080x1920, 30 fps sabit (256-356)
                  (2) EXE: Uygulama/KayitBootstrap.cs + FfmpegPipeRecorder.cs (ayrı süreç; AnaEkran.Video.cs:123-135)
  -> SONRASI      Uygulama/KaliteKontrol.cs (ffmpeg ile denetim), YayinPaketi.cs
  KUYRUK          Editor/HikayeKuyrugu.cs (yalnız Editör)
  Derleme         asmdef: Hikaye.Core / Runtime / App / Editor. Arayüz: Core/Arayuzler.cs (IVideoKaydedici, IVarlikKatalogu)
```

## A.2 Güçlü yanlar
- **Metin tek doğruluk kaynağı.** Aynı ayrıştırıcı önizleme, denetim ve kayıtta kullanılıyor. Motor değişse bile sözdizimi korunabilir.
- **Zaman tabanlı mantık ölçekli zamanı kullanıyor** (`Time.deltaTime` / `Time.time`); `captureDeltaTime` ile kare kare tutarlı ilerler [S023]. Aramada animasyonu süren `unscaledDeltaTime` bulunmadı (yalnız KayitBootstrap.cs:161 zaman aşımı).
- **EXE kaydı doğru yapıda:** sabit `captureDeltaTime` (FfmpegPipeRecorder.cs:203), `AudioRenderer` ile offline ses (198, 275-301) [S009], `AsyncGPUReadback` + ayrı yazıcı iş parçacığı + sınırlı kuyruk (177, 250-272), klaket testi (154-161, 282-290).
- Kayıt ayrı süreçte; arayüz donmuyor. Kayıt sonrası otomatik kalite denetimi var.
- Düşük bağımlılık: URP ve Recorder tipleri yansımayla çağrılıyor. HDRP'ye veya başka sürüme geçişte derleme kırılması az olur.
- Prosedürel mekânlar çoğunlukla sabit tohumlu (`new System.Random(n)`, Mekan3D*.cs).

## A.3 Zayıf yanlar ve bulunan sorunlar (önem sırasıyla)

| # | Bulgu | Kanıt | Etki (ÇIKARIM) |
|---|---|---|---|
| K1 | **3D hikâye sahnesinde gerçek zamanlı gölge yok.** Ana ışık ve mekân ışıkları `LightShadows.None` | Sahne3D.cs:112; Mekan3D.cs:271; Mekan3D.Istasyon.cs:176,187. `LightShadows.Soft` yalnız Editor/Karakter3DKurulum.cs:1124 | Karakterler "havada" ve düz görünür; zayıf görüntünün büyük nedeni olabilir. A04 ile doğrulanmalı |
| K2 | **3D kamera LDR:** `allowHDR = false`, RT 8-bit ARGB32 | Sahne3D.cs:91,100 | Bloom eşiği 0.9 (AtmosferVolume.cs:697) kırpılmış renkte çalışır; sinematik tonemapping yapılamaz [S021] |
| K3 | **EXE'de alan derinliği ve Panini shader'ları derlemeden atılıyor** | DURUM.md:79 (gözlem); Volume profili çalışma anında yaratılıyor (AtmosferVolume.cs:608-626) | Olası neden: "Strip Unused Post Processing Variants" projede varlık olarak görmediği override'ları atıyor [S014]. Çözüm adayı: DoF içeren VolumeProfile varlığını projeye eklemek veya ayarı kapatmak. Denenmedi |
| K4 | **Hatalı GPU okumasında kare atlanıyor, yerine kopya yazılmıyor** | FfmpegPipeRecorder.cs:266 | Her atlanan kare görüntüyü sese göre 1 kare (33 ms) kaydırır ve `-shortest` (346) sonu keser. `HataliKare` mesaja yazılıyor (358) ama düzeltilmiyor |
| K5 | **2D arka plan + 3D karakterde yakın plan dijital büyütme:** `camScaleT = closeZoom (1.6)` 3D RT'yi gösteren kanvası ölçekliyor; 3D yönetmen yalnız 3D mekânda (`oda`) devralıyor | StoryPlayer.cs:1213-1221, 1023, 1246; KameraYonetmen.cs:151-155; Sahne3D.cs:91 | Yakın planda etkin çözünürlük yaklaşık 675x1200'e düşer; bulanıklık |
| K6 | **`Resources/OyuncuListesi` tüm karakter prefab'larına doğrudan referans veriyor** ve yalnız ad almak için bile yükleniyor | OyuncuListesi.cs:9-12; UygulamaDerle.cs:147-163; AnaEkran.Video.cs:70; AnaEkran.cs:745; AnaEkran.Yazma.cs:109; CanliOnizleme.cs:65; KayitBootstrap.cs:100; Varyantlar.cs:200 | Büyük olasılıkla listedeki bütün karakterlerin mesh ve dokuları RAM/VRAM'e geliyor (depodaki listede yaklaşık 68 giriş; derleme `Karakterler/Hazir`'ın hepsini yazar, envanterde 115 gerçekçi insan); açılış yavaş, EXE 10.8 GB (DURUM.md:77). Resources'ta tek varlık boşaltılamaz [S022]. Bellek ÖLÇÜLMEDİ |
| K7 | **Kayıt en fazla gerçek zaman hızında:** `targetFrameRate = 30` kayıtta da geçerli | StoryPlayer.cs:116; KayitBootstrap/FfmpegPipeRecorder bunu değiştirmiyor | targetFrameRate oyun döngüsünü sınırlar [S010]; 30 fps video en az kendi süresi kadar, 60 fps en az 2 katı sürer. captureDeltaTime ile etkileşim belgede yazmıyor: ölçülmeli |
| K8 | **Render tekrarlanabilir değil** | `Random.value/Range` (YuzCanlilik.cs ~36 yer, KameraYonetmen.cs:67, StoryPlayer.cs:1014, 1244, 1440, Sahne3D.cs:603-610); `Random.InitState` yok; `Time.time` fazı (KameraYonetmen.cs:572); ses kazancı önceki kayıttan öğreniliyor (SesOlcer.cs:45, 143-149) | Aynı hikâye her kayıtta farklı; tek sahneyi yeniden render edip ekleme güvenli değil |
| K9 | **Ses seviyesi tek geçişte tutmuyor** | SesOlcer.cs:35-36, 143-149; DURUM.md (film -16,5, test -10,1 LUFS; hedef -14) | Kayıt sonrası ffmpeg `loudnorm` geçişi [S026] daha kararlı olur |
| K10 | **Takılan kayıt için bekçi (watchdog) yok** | `Finished` yalnız `Run` sonuna ulaşınca (StoryPlayer.cs:505-514); AnaEkran.Video.cs:197 yalnız süreç ölürse bitiriyor | Coroutine içinde hata olursa kayıt sonsuza kadar sürebilir, diski doldurur |
| K11 | **İptal ve kurtarma yok:** iptal = `Process.Kill`; ara dosyalar kalıyor; sürdürme yok | AnaEkran.Video.cs:283-289; FfmpegPipeRecorder.cs:134-135, 165-166, 356-357 | Ara video normal MP4: ffmpeg düzgün kapanmazsa okunamayabilir [S024]. 5 dk filmde çökme = baştan |
| K12 | **EXE'de kuyruk yok;** Editör kuyruğu Play modu + domain reload'a bağlı | HikayeKuyrugu.cs:10-16; StudyoKayit.cs:368 | Toplu üretim kullanıcı uygulamasında yok; Unity 7'de domain reload kalkacak [S016] |
| K13 | İki ayrı kayıt yolu; Editör yolu `IVideoKaydedici` kullanmıyor | StudyoKayit.cs; Asama5_Plani.md | Kapak, kalite, ses davranışı farklılaşır. Recorder zaten EXE'de çalışmaz [S008] |
| K14 | Büyük, merkezî sınıflar | StoryPlayer.cs 1609 satır; Sahne3D.Hareket.cs 1146; statik olaylar ve PlayerPrefs ile iletişim (StoryPlayer.cs:31-38, 141, 516) | Yeni sahne/kamera özelliği eklemek riskli; birim testi zor |

## A.4 Cinemachine / Timeline / VFX Graph / Shader Graph
- Kodda `Cinemachine`, `PlayableDirector`, `UnityEngine.Timeline`, `VisualEffect`, Shader Graph **hiç kullanılmıyor** (arama sonucu 0). `.shader` / `.shadergraph` dosyası da yok.
- Yalnız Playables API var: `Karakter3D.cs:214-222` (iki girişli `AnimationMixerPlayable`) ve Editör test araçları.
- Görsel efektler uGUI katmanı (`EfektKatmani.cs`, `FxLayer.cs`); hava durumu Particle System (`HavaDurumu3D.cs`).
- Cinemachine 6.6.0 manifestte kurulu (TOPICS/00), ama `Packages/` depoda yok; manifest ve URP varlığı ayarları (HDR, MSAA, gölge, render scale) **depodan doğrulanamadı**.

## A.5 Kare hızı ve ses senkronu garantisi

| Yol | Kare hızı | Ses | Garanti düzeyi |
|---|---|---|---|
| EXE (ffmpeg borusu) | `captureDeltaTime = 1/fps`; her `WaitForEndOfFrame`'de 1 kare | `AudioRenderer`, kare başına örnek | Yapı olarak iyi. Açıklar: K4 (atlanan kare), `-shortest`. Klaket ölçüm sonucu (ms kayma) kayıtlarda bulunmadı |
| Editör (Recorder) | `FrameRate=30`, `Constant`, `CapFrameRate` (StudyoKayit.cs:308-311) | Recorder; yalnız mono/stereo [S008] | Recorder'a emanet; Game View görünür olmalı |

- Altyazı-ses uyumsuzluğu (kullanıcı şikâyeti): kayıt yolundan değil, büyük olasılıkla toplu sesin tahminle bölünmesinden (DURUM.md). Oynatıcı satır süresini klipten alıyor (StoryPlayer.cs:1029). Ayrıntı A07'nin alanı.
- Kare atlama dışında 30 fps garantisi gerçek render hızına bağlı değil: yavaş GPU yalnız kaydı uzatır [S023].

## A.6 Genişletilebilirlik
- İyi: yeni hikâye komutu `KomutKayit` + `ItemType`/`Direct` ile eklenebiliyor; mekân ve efektler veri/isim tabanlı.
- Zayıf: "satır -> kamera/animasyon/ışık" kararları yürütme anında, dağınık coroutine'lerde veriliyor. Önceden hesaplanmış bir **çekim listesi (shot list)** yok. Bu olmadan sahne bazlı render, kurgu (A08) ve otomatik yönetmen (A01/A08) zorlaşır.

## A.7 Render darboğazları (ÖLÇÜLMEDİ; yapıdan çıkarım)
1. Tümleşik Vega GPU: paylaşılan bellek bant genişliği. Zincir: 3D (MSAA4 1080x1920) -> post -> kanvas -> kayıt RT -> GPU'dan okuma 8,3 MB/kare -> boru -> ffmpeg `vflip` + RGBA->YUV420 (CPU) -> AMF kodlayıcı (aynı yonga).
2. CPU: `updateWhenOffscreen = true` tüm skinned mesh'lerde (Karakter3D.cs:62-64); yansıma ile Volume parametreleri her karede (AtmosferVolume.cs:503, 685-710); yorgan için `BakeMesh` + `vertices` kopyası (Sahne3D.Hareket.cs:1053-1060, her 0,2 sn, 3 sn boyunca).
3. Bellek/yükleme: K6.
4. Gerçek zaman kilidi: K7.
- Kare süresi, GPU süresi ve RAM için **hiç profil verisi yok**. İlk iş ölçmek olmalı.

---

# GÖREV B — BULGULAR

## S3: URP mi HDRP mi?
- Unity 2026 stratejisi: **HDRP'ye yeni özellik planlanmıyor**; yalnız kararlılık ve kritik hata bakımı. Öncelik URP. Built-in 6.5'te kullanımdan kaldırıldı [S001, S002 (25 Şub 2026)].
- HDRP'de olup URP'de olmayanlar (6000.6 karşılaştırma): ekran uzayı yansıma ve GI, ışın/yol izleme, contact shadow, PCSS, gerçek zamanlı alan ışığı, diffusion profile (ten altı), fiziksel kamera pozu [S013]. `Docs/VideoKalitesi.md` saç shader'ı ve hacimsel sisi de HDRP'ye özgü sayıyor (bu oturumda ayrıca doğrulanmadı).
- URP'ye SSR 6.7'de hedefleniyor (önizleme, ışın izleme istemez) [S015]; Surface Cache GI ve Unity Neural 6.7'de URP için geliyor [S003].
- **Q-003 yanıtı (kısmi):** HDRP'nin asgari şartı DX11 + Shader Model 5 + compute; ışın izleme şart değil [S011]. Bu makine (compute var, D3D11) kâğıt üzerinde karşılıyor. **Hız bilgisi yok**; belge tümleşik GPU'dan söz etmiyor. ÇIKARIM: offline kayıtta düşük fps kabul edilebilir ama paylaşılan bellek, sürücü kararlılığı ve 45 GB disk riskli; ölçmeden geçilmemeli. Işın/yol izleme bu donanımda yok.
- **Çelişki:** `Docs/VideoKalitesi.md` "HDRP'ye geçmek (ÖNERİLEN)" diyor. Bu, HDRP'nin özellik dondurma kararını hesaba katmıyor görünüyor. Ana araştırmacıya: C-adayı.
  - HDRP lehine: bugün hazır insan render özellikleri (ten, saç, yol izleme). Aleyhine: geleceği bakım modu, geçiş işçiliği, güçlü GPU ihtiyacı.

## S4: Sürüm ve uyumluluk
- 6000.6 bir **"Supported/Update" sürümü, LTS değil**; destek 6.7 çıkana kadar [S004, S005]. 6.3 LTS desteği 4 Ara 2027'ye kadar [S004].
- 6.7 LTS, Mono'lu son sürüm; Unity 7 CoreCLR, domain reload yok [S016]. Unity 7 Preview Aralık 2026, tam sürüm hedefi 2027 Q1; Unity "kırıcı değişiklik yok" diyor [S003, S018] (satıcı iddiası).
- Öneri (ÇIKARIM): proje 6.7 LTS çıkınca, test hikâyeleri ve referans karelerle, ona sabitlensin; Unity 7'ye acele etmesin. Geriye 6.3 LTS'e inmek önerilmez (paket sürümleri farklı; denenmedi).
- Unity 7 için bu projede dikkat: statik alanlar/olaylar (StoryPlayer.cs:31-57; `FfmpegPipeRecorder.seciliKodlayici`), `AppDomain.GetAssemblies` ile tip arama (StudyoKayit.cs:57), yansıma.
- 6.6'da Cinemachine ve Timeline "core package" oldu, sürümleri editörle birlikte (6.6) [S006, S017].
- Tekrarlanabilirlik riski: `FfmpegIndir.cs:13` BtbN `latest` indiriyor; içerik zamanla değişir [S028]. Sürüm sabitlenmeli. `Packages/manifest.json` depoda değil.

## S5: Cinemachine, Timeline, VFX Graph, Shader Graph
- **Cinemachine:** takip, kompozisyon (Position/Rotation Composer), spline dolly, gürültü, fiziksel lens, kamera karışımları [S019]. Kurulu; yeni paket onayı gerekmez. Mevcut KameraYonetmen'in plan SEÇİMİ korunup yalnız kameranın "sürülmesi" Cinemachine'e verilebilir. Tek başına daha iyi sinematografi üretmez; kararlar yine bizim kodda.
- **Timeline:** asıl değeri elle düzenleme. Bu üründe kaynak metin; çalışma anında Timeline üretmek ek katman. ÇIKARIM: önce kendi "çekim listesi" veri yapımız; Timeline yalnız gerekirse çıktı/önizleme hedefi. Recorder track'leri EXE'de desteklenmiyor [S008].
- **VFX Graph:** compute + SSBO ister; URP desteği hâlâ "preview dışı değil" ifadesiyle anlatılıyor; URP'de gamma renk uzayı yok [S012 (17.1 sayfası)]. Bu GPU'da çalışabilir olması muhtemel, hız bilinmiyor. Çalışma anında koddan graph üretilemez; önceden hazırlanmış efekt varlıkları gerekir (ÇIKARIM). A06'ya.
- **Shader Graph:** 6.6'da URP'de Built-in Particle System uyumlu parçacık shader'ı üretebiliyor [S006]. Göz, ten benzeri sarma (wrap) ışık, rim light için URP'de yol (A02/A04). HDRP'nin diffusion profile'ının birebir yerine geçmez [S013].

## S6: Offline kalite
- Recorder **accumulation (motion blur, süper örnekleme AA, path tracing) yalnız HDRP** [S007]. URP + EXE için hazır çözüm yok.
- Kendi kaydedicimizde yapılabilecekler (ÇIKARIM, denenmedi):
  1. Mekânsal süper örnekleme: 3D RT'yi 1.5-2x render edip küçültmek (K5'i de çözer).
  2. Zamansal alt kareler: `captureDeltaTime = 1/(fps*N)`, N kareyi GPU'da veya ffmpeg `tmix` ile [S026] birleştirmek -> motion blur. Maliyet yaklaşık N kat; ses toplama hızı ayrıca test edilmeli.
  3. K7 kaldırılmalı (kayıtta `targetFrameRate=-1`, `vSyncCount=0` [S010]).
- AA: MSAA geometri kenarı için; shader/doku parıltısına etkisi yok. TAA MSAA ile birlikte olmaz, hızlı harekette hayalet yapar [S020]. Offline için MSAA + SMAA veya süper örnekleme daha güvenli (ÇIKARIM).
- UE5 karşılığı: Movie Render Queue temporal sample, 16-bit HDR görüntü dizisi, ayrı süreç [S030].

## S7: FFmpeg, kuyruk, iptal, kurtarma
- Kodlayıcı: projedeki ffmpeg **LGPL** derlemesi; **libx264 yok** [S028]. Zincirdeki `libx264` seçeneği (FfmpegPipeRecorder.cs:72) bu derlemede hiç seçilemez; yedek `h264_mf` / `libopenh264`. Bu PC'de `h264_amf` kullanılıyor.
- AMF ayarı `-quality balanced -b:v 12M` (KodlayiciSecenekleri, 82). AMD önerisi: `-preset quality -rc vbr_peak`, preanalysis, vbaq [S027]; eski Vega sürücüsünde hangilerinin çalıştığı denenmeli. TikTok yeniden sıkıştırdığı için kazanç sınırlı olabilir (ÇIKARIM).
- **Dayanıklılık:** normal MP4 yarıda kesilirse kurtarmak çok zor; parçalı MP4 son tam parçaya kadar okunur [S024]. Ara dosya için `+frag_keyframe` veya `hybrid_fragmented` (derlemede varlığı doğrulanmalı) ya da MKV [S025].
- **Önerilen iş modeli (ÇIKARIM):**
  - Hikâye -> sahne parçaları (`mekan:` sınırları). Her parça ayrı video + ses + `manifest.json` (hikâye özeti/hash, varlık, ayar, tohum).
  - Birleştirme: concat demuxer, yeniden kodlamadan [S025]. Parçalar aynı kodlayıcı/zaman tabanıyla üretilmeli. Ses tek WAV olarak biriktirilip en sonda AAC'ye çevrilirse parça sınırında AAC boşluğu olmaz.
  - Çökme sonrası yalnız eksik parçalar render edilir. Ön koşul: K8 (tohum, kazanç) ve sahne durumunun atlayarak kurulması; `HS_StartLine` yolu (StoryPlayer.cs:349-378) bunun başlangıcı, ama tam durum kurduğu doğrulanmadı. Kesme noktasında süren müzik/efekt kuyruğu ayrıca ele alınmalı.
  - İptal: dosyayla "dur" isteği -> kaydedici `Durdur()` -> ffmpeg düzgün kapanır; `Kill` yalnız son çare.
  - Watchdog: beklenen sürenin ~3 katında veya kare sayacı ilerlemezse süreci bitir; `Application.logMessageReceived` ile istisnayı sonuç dosyasına yaz.
  - Kuyruk: arayüzde iş listesi (disk üstünde JSON), sırayla tek süreç. İki paralel render bu iGPU'da önerilmez.
  - Disk ön kontrolü: tahmini boyut + pay kadar boş alan yoksa başlama.
- Disk hesabı (hesap, ölçüm değil): ham 1080x1920 RGBA = 8,3 MB/kare; 30 fps = 249 MB/s video-süresi. 5 dk = 9000 kare: sıkıştırmasız 16-bit RGBA dizi ≈ 149 GB, 8-bit ≈ 75 GB. 45 GB boş C: için **kare dizisi saklanmamalı**; akış halinde kodlama doğru seçim.

## S8: Optimizasyon (CPU/GPU/bellek)
Sıra: ölç -> bellek -> kare süresi -> kalite ekle.
- Ölçüm: Unity Profiler + Frame Debugger (6.6'da URP Settings Analyzer da var [S006]); kayıt sonucuna `kare süresi ortalaması, toplam süre, tepe RAM` yazılmalı.
- Bellek (K6): `OyuncuListesi`'nde doğrudan referans yerine ad + `Resources/Oyuncular/<ad>` yüklemesi (paket gerektirmez; tek varlık boşaltma sınırlı [S022]); orta vadede AssetBundle/Content Directories (yerleşik) veya Addressables (paket, onay) [S022]. EXE yalnız kullanılan karakterleri içerebilir.
- GPU: offline kayıtta hedef akıcılık değil kalite; yine de iGPU'da gölge çözünürlüğü, gölge mesafesi, ek ışık sayısı ölçülerek seçilmeli. Statik mekânlar kodla kurulduğu için lightmap pişirme zor; probe/ön hazır mekân düşünülmeli (A05).
- 6.6'da dynamic batching kaldırıldı [S017]; çok parçalı prosedürel mekânlarda GPU instancing / SRP Batcher kontrol edilmeli (ÇIKARIM).

## S9: Alternatif mimariler

| Ölçüt | Unity URP (mevcut, geliştirilmiş) | Unity HDRP | Unreal 5 + MRQ | Blender (tam) | Hibrit: Unity yönetmen -> Blender render |
|---|---|---|---|---|---|
| Kod korunumu | ~tamamı | oynatma korunur; malzeme, ışık, post yeniden | yalnız hikâye sözdizimi/fikirler; ~34.700 satır C# yeniden | sözdizimi; sahne mantığı Python | ayrıştırma/yönetmen korunur; dışa aktarıcı + Blender betikleri yeni |
| Bu donanım | uygun | belirsiz (yalnız şart karşılanıyor) | Lumen desteklenmez (RX 6000+/RTX 2000+); önerilen 8 GB VRAM [S029] | 5.x GCN4+ ile açılır, Cycles GPU yok (RDNA gerekir) [S031, D] | aynı sınır |
| Görüntü tavanı (ÇIKARIM) | iyi oyun görüntüsü | daha iyi insan/ışık | en yüksek (güçlü GPU ile) | en yüksek (Cycles), CPU'da çok yavaş | yüksek ama aktarım kayıpları |
| Zahmet (kaba tahmin, ölçülmedi) | gün–hafta | 4–8+ hafta | aylar | aylar | 1–3 ay |
| Para | 0 | 0 yazılım; GPU gerekebilir | 0 yazılım (koşullar A12); GPU gerekir | 0; GPU/bulut tercih | 0; aynı |
| Ana risk | kalite tavanı | HDRP bakım modu | yeniden yazım, disk | süre | iki sistemi eşleme: malzeme, blendshape, saç, Humanoid retarget, efektler, altyazı |

- Blender süre göstergesi: Ryzen 5 5600G CPU, Blender 4.2, BMW27 sahnesi 247,72 sn [S032]. ÇIKARIM: bir hikâye karesi bu kadar sürseydi 60 sn video (1800 kare) ≈ 124 saat olurdu. Gerçek sahne çok farklı olabilir; yalnız büyüklük sırası uyarısı. EEVEE'nin bu iGPU'daki hızı bilinmiyor.
- Hibrit için aktarım: Unity'den animasyonlu sahne dışa aktarmak ek paket (FBX Exporter / USD) veya kendi JSON çekim dosyası + Blender tarafında aynı varlıkları yeniden kurma gerektirir; ikincisi daha sağlam (ÇIKARIM; doğrulanmadı).

---

## Teknoloji kayıtları
Alanlar: kategori · ne/nasıl · katkı · uyum · donanım · maliyet/lisans · artı · eksi · alternatif · kaynak · test · güven. Doğrulama tarihi hepsi için 2026-11-27. Lisans ayrıntısı A12'ye.

1. **URP 17.x** — render hattı · tek geçişli ölçeklenebilir SRP · mevcut hat · tam · iGPU'da çalışıyor · Unity ile gelir · Unity'nin yatırım alanı · SSGI, diffusion profile yok · HDRP · S001, S013 · projede çalışıyor · A
2. **HDRP** — render hattı · fiziksel ışık, volumetrik, ışın/yol izleme · insan/ışık kalitesi · malzeme dönüşümü gerekir · DX11 SM5 + compute; RT için özel GPU · Unity ile · güçlü özellikler · yeni özellik yok, ağır · URP / UE5 · S001, S011, S013 · TEST EDİLMEDİ · A
3. **Cinemachine 6.6** — kamera sistemi · sanal kameralar + Brain · kompozisyon, takip, karışım · core paket, kurulu · önemsiz yük (ÇIKARIM) · Unity ile · hazır davranışlar · yönetmen kararlarını vermez · KameraYonetmen · S006, S019 · TEST EDİLMEDİ · A
4. **Timeline** — sıralayıcı · PlayableDirector parçaları · elle kurgu · core paket · düşük · Unity ile · görsel düzenleme · metin kaynaklı akışa ek katman; Recorder track EXE'de yok · kendi çekim listesi · S006, S008 · TEST EDİLMEDİ · A
5. **VFX Graph** — GPU parçacık · compute · büyük efektler · URP'de sınırlı olgunluk · compute + SSBO · Unity ile · çok parçacık · iGPU hızı bilinmiyor · Particle System (mevcut) · S012 · TEST EDİLMEDİ · A (eski sürüm sayfası)
6. **Shader Graph** — görsel shader editörü · düğümler · göz/ten/rim · URP uyumlu · değişken · Unity ile · kod gerektirmez · diffusion profile yerine geçmez · HLSL · S006 · TEST EDİLMEDİ · B
7. **Unity Recorder 5.1** — kayıt · Editör'de kare+ses · Editör üretimi · EXE'de track desteklenmez; accumulation yalnız HDRP · düşük · Unity ile · hazır · ürün (EXE) için uygun değil · FfmpegPipeRecorder · S007, S008 · projede Editör'de kullanılmış · A
8. **FFmpeg (LGPL, BtbN)** — kodlama/birleştirme · ham kare borusu, concat, filtre · çıktı, loudnorm, kurtarma · ayrı süreç · AMF · LGPL (x264/x265 yok) · esnek · "latest" sabit değil · Media Foundation · S025, S026, S028 · projede çalışıyor · A
9. **Unreal 5 + Movie Render Queue** — motor + offline render · temporal sample, dizi çıktı · yüksek tavan · Unity kodu taşınmaz · önerilen 8 GB+ VRAM; Lumen RX6000+/RTX2000+ · Epic koşulları (A12) · kalite · yeniden yazım · HDRP · S029, S030 · TEST EDİLMEDİ · A
10. **Blender (EEVEE/Cycles)** — DCC + render · Python ile başsız render (genel bilgi, kaynak okunmadı) · fotogerçekçi tavan · aktarım gerekir · Cycles GPU RDNA+; burada CPU · GPL (A12) · ücretsiz, güçlü · yavaş · UE5 · S031, S032 · TEST EDİLMEDİ · B/D

---

## Karşılaştırma: mimari seçenekler

**Seçenek 0 — Sağlamlaştır (mevcut mimari, küçük değişiklik).** K1, K2, K3, K4, K5, K7, K10 + loudnorm + disk kontrolü. Yeni paket yok. En yüksek fayda/zahmet (ÇIKARIM). Her biri ayrı test edilebilir.

**Seçenek 1 — URP'de kontrollü yenileme (ÖNERİLEN ana yol).**
```
Metin -> Ayrıştırıcı (mevcut)
      -> DERLEYİCİ (yeni): "çekim listesi" {sahne, plan, süre = ses dosyasından, kamera, oyuncu hareketi, ışık hazır ayarı, efekt, altyazı zamanı, tohum}
      -> SAHNE YÜRÜTÜCÜ (StoryPlayer parçalanır: Sahne, Konuşma, Efekt, Altyazı yürütücüleri)
      -> KAMERA SÜRÜCÜ arayüzü (mevcut KameraYonetmen | Cinemachine)
      -> RENDER PROFİLİ arayüzü (Önizleme: düşük | Final: HDR, gölge, süper örnekleme, N alt kare)
      -> PARÇALI KAYIT + kuyruk + manifest + kurtarma (IVideoKaydedici tek yol)
```
- Geçiş adım adım: önce çekim listesi yalnız mevcut davranışı kaydetsin; referans kareler aynı kalınca yürütme ona taşınsın.
- Getirisi: kurgu/otomatik yönetmen (A08) için doğal yer; motor veya render hedefi değişirse aynı liste kullanılır.

**Seçenek 2 — HDRP denemesi (karar öncesi sınırlı prototip).** Ayrı kopya projede TEK sahne + TEK Rocketbox karakter; kare süresi ve görsel fark ölçülür. Kod bağımlılığı sınırlı: `Shader.Find("Universal Render Pipeline/Lit")` (Mekan3D.cs:193, KolPozu.cs:184, Karakter3DKurulum.cs:492/551/1168), `Universal.*` yansıması (AtmosferVolume.cs:615-650), yaratılan malzemeler, VRM (MToon) malzemeleri (HDRP desteği DOĞRULANAMADI), 2D kanvas + saydam 3D RT bileşimi (HDRP'de alfa davranışı DOĞRULANMADI). Güçlü GPU alınmadan ana yola alınmamalı.

**Seçenek 3 — Hibrit/harici render.** Çekim listesi (Seçenek 1) hazır olunca, yalnız seçili "kahraman planlar" Blender veya UE'de render edilip ffmpeg ile birleştirilebilir. Tam taşıma değil. Görsel tutarsızlık riski yüksek (ÇIKARIM).

**Çelişkiler / dikkat**
- VideoKalitesi.md "HDRP önerilir" <-> Unity "HDRP'ye yeni özellik yok" [S001].
- VideoKalitesi.md tablo satır 7 HDRP geçişi "tüm sahne kodu" diyor; kod okumasında bağımlılık birkaç dosyada toplanıyor. Asıl yük görsel ayar ve malzeme (ÇIKARIM).
- "Bu bilgisayarda kalite mümkün değil" ifadesi test edilmemiş bir sınır: gölge, HDR ve çözünürlük hataları giderilmeden mevcut hattın tavanı görülmedi.

## Riskler
| Risk | Olasılık/etki (ÇIKARIM) | Önlem |
|---|---|---|
| Gölge/HDR açınca iGPU kayıt süresi veya sürücü çökmesi | orta/orta | profil "Final/Düşük"; parçalı kayıt |
| StoryPlayer yeniden düzenlemesi mevcut davranışı bozar | yüksek/yüksek | referans hikâye + kare karşılaştırma önce |
| 6000.6 desteksiz kalır | kesin/orta | 6.7 LTS'e planlı geçiş |
| Unity 7 CoreCLR | orta/orta | statik durum temizliği, acele etme |
| HDRP'ye büyük yatırım sonrası bakım modu | orta/yüksek | prototip + ölçüm olmadan geçme |
| 45 GB disk (EXE derlemesi 10.8 GB, kurulu ~11 GB) | yüksek/orta | kare dizisi saklama; karakter seçerek derle |
| ffmpeg "latest" değişimi | düşük/orta | sürüm ve SHA sabitle |

## Test önerileri
**Bu bilgisayarda yapılabilir**
1. Temel ölçüm: 30 sn referans hikâye; toplam kayıt süresi, ortalama kare süresi, tepe RAM, `HataliKare`.
2. `targetFrameRate=-1` ile aynı kayıt: süre farkı, klaket senkronu.
3. Klaketli 5 dk kayıt: başta ve sonda ses-görüntü kayması (ms).
4. Gölge açık/kapalı, HDR açık/kapalı: aynı kareler yan yana + kare süresi.
5. 3D RT 1x / 1.5x / 2x: yakın planda keskinlik + süre.
6. DoF'lu VolumeProfile varlığı ekleyip EXE'de bulanıklık var mı.
7. Uygulama açılışında RAM: OyuncuListesi dolu / boş.
8. Kayıt sırasında süreci zorla kapatma: ara dosya okunuyor mu; parçalı MP4 ile tekrar.
9. İki kez aynı kayıt: kare farkı (tekrarlanabilirlik).
10. HDRP prototipi (Seçenek 2): çalışıyor mu, kare süresi. (Paket kurulumu: kullanıcı onayı.)
11. Blender: tek sahne EEVEE ve Cycles CPU kare süresi (kurulum: kullanıcı onayı, disk).

**Ayrı GPU ister:** HDRP ışın/yol izleme, UE5 Lumen/MRQ, Cycles GPU.

## Öz denetim
- Hiçbir şey çalıştırılmadı; performans yargıları ölçümsüz.
- URP varlığı, Quality ve Project Settings depoda yok: HDR, gölge mesafesi, MSAA, renk uzayı, vSync durumu bilinmiyor. K1/K2 kod düzeyinde kesin (ışık `None`, kamera `allowHDR=false`); ama içe aktarılan prefab'larda kendi gölgeli ışıkları olabilir.
- Dosyaların tamamı okunmadı: Sahne3D.Hareket, Mekan3D, YuzCanlilik, EfektKatmani, AnaEkran büyük ölçüde taranmadı.
- Zahmet süreleri tahmin.
- WebFetch özet döndürüyor; alıntılar kelimesi kelimesine doğrulanmış sayılmamalı. Feature comparison sayfasının bir bölümü okunmadı.
- Okunamayan: Unity Discussions "strategy" sayfasının `/print` sürümü (resmî unity.com sayfası okundu); Unity issue tracker kaydı (bulunamadı).
- Bağımsız test/kullanıcı deneyimi kaynağı az (özellikle HDRP/URP'nin Vega iGPU performansı).

## Eksik alanlar / açık sorular
- HDRP ve URP'nin Radeon Vega iGPU'da gerçek fps'i (Q-003'ün hız kısmı): kaynak bulunamadı -> test.
- UniVRM/MToon'un HDRP desteği: arama bütçesi bittiği için bakılamadı.
- Unity 6.7 LTS çıkış tarihi (27 Kas 2026'da endoflife listesinde yoktu).
- URP'de yerleşik hacimsel sis ve saç durumu.
- Blender resmî gereksinim sayfası; Unity -> Blender aktarım yolları; UE disk gereksinimi.
- `captureDeltaTime` + `targetFrameRate` etkileşimi (belgede yok).

## Diğer ajanlardan beklenen / ortak konular
- A04: K1, K2, K3 (gölge, HDR, post) görsel doğrulaması ve hedef ayarlar.
- A01/A08: çekim listesi şeması; Cinemachine'in hangi planlarda kullanılacağı.
- A03: parça bazlı render için animasyon durumunun yeniden kurulabilirliği; root motion.
- A07: ses süresi/altyazı zamanları çekim listesine; loudnorm vs SesOlcer.
- A02: ten/saç/göz ihtiyacı HDRP prototipine değer mi.
- A05/A06: mekân ve VFX'in URP bütçesi.
- A10: RTX sınıfı GPU alınırsa HDRP/UE kararı; disk.
- A12: LGPL ffmpeg dağıtımı, AMF/H.264 patent notu, UE/Blender lisansı.

## Öneriler (kanıtla)
1. **URP'de kal; HDRP'ye şimdi geçme.** Gerekçe: HDRP'ye yeni özellik yok [S001]; hız bilinmiyor; mevcut hatta temel eksikler var (K1, K2).
2. **Önce ölç ve düzelt (Seçenek 0):** gölge, HDR+tonemapping, DoF derleme sorunu, yakın plan çözünürlüğü, kare atlama, gerçek zaman kilidi, watchdog.
3. **Sonra Seçenek 1:** çekim listesi + parçalı, kurtarılabilir kayıt + tek kayıt yolu.
4. **Cinemachine'i** kamera sürücü arayüzü arkasında, tek sahnede dene (kurulu paket).
5. **K6'yı çöz** (varlıkları adla, gerektiğinde yükle) — disk ve bellek için.
6. **6.7 LTS'e planlı geçiş**, ffmpeg ve paket sürümlerini depoya sabitle (`Packages/manifest.json` dahil).
7. HDRP / Blender / UE yalnız ölçümlü küçük prototiple ve donanım kararıyla birlikte.
