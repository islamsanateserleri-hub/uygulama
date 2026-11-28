# D6: UE5'i zayıf, uzak (RDP) PC'de açmak ve MetaHuman

Tarih: 2026-11-28. Yalnızca web araştırması yapıldı, hiçbir şey denenmedi.
Kapsam: D4 §2'yi tekrar etmez, onu tamamlar. Resmi gereksinimler, TDR uyarısı, `-nullrhi` notu ve DX11'de Lumen'in olmaması D4'te yazılı.
İşaretler: **[DOĞRULANDI]** kaynak sayfada açıkça yazıyor. **[KISMİ]** kaynak benzer donanımı anlatıyor ama bu PC'yi değil. **[DOĞRULANMADI]** çıkarım ya da yalnızca öneri.

Makine: Win10 22H2, RDP ile bağlanılıyor. Ryzen 5 5600GT ve içindeki Vega 7 iGPU (GCN5). 64 GB RAM. UE 5.8.4 iki kez çöktü: ilk karede D3D12 "GPU hung / device removed" verdi, ikinci açılışta tüm PC dondu.

---

## 0. Kısa sonuç

1. **Asıl şüpheli D3D12'nin kendisi.** UE 5.6'da Ryzen APU'larda (5600G, 4500U, 5500U, 3250U, 5800H) D3D12 açılışta çöküyordu. `-d3d11` ile açılış, deneyen **herkeste çalıştı** [KISMİ, aynı iGPU ailesi].
2. **Vega "yalnız wave64" bir mimari.** UE 5.7'de wave32 isteyen hair shader'ı, GCN kartlarda PSO hatasıyla çöktürdü. Epic düzeltmeyi 5.8'e koydu [DOĞRULANDI]. Başka wave32 varsayımı kalmış olabilir [DOĞRULANMADI].
3. **MetaHuman Creator/Animator bu PC'ye uygun değil.** Animator açıkça DX12 istiyor. Creator için önerilen GPU RTX 3070 / RX 6800 XT ve 8 GB VRAM; strands saç için SM6 gerekiyor [DOĞRULANDI]. DX11'de Creator'ın çalıştığına dair hiçbir kanıt bulunamadı.
4. **GPU'suz MetaHuman (Python, 5.8+):** Resmi belge "headless/batch/commandlet" diyor ama `-nullrhi` demiyor. Epic destek personeli başka bir bağlamda (malzeme pişirme) **`-warp -RenderOffscreen`** önerdi. WARP, Windows'un CPU ile çalışan D3D sürücüsüdür. MetaHuman için denenmemiş, en ilginç yol bu [DOĞRULANMADI].
5. **Eski UE + Quixel Bridge yolu 2026 sonunda fiilen kapandı.** MetaHuman web uygulaması 5 Kasım 2026'da kapandı. Yalnızca hesabın galerisinde zaten karakteri olanlar, yaklaşık 3 Şubat 2027'ye kadar Bridge'den indirebilir [DOĞRULANDI].

---

## 1. UE5 editörünü Vega iGPU'da ve RDP altında açmak

### 1.1 Benzer donanımda kanıtlar

**UE 5.6, Ryzen APU'da açılışta çökme** [KISMİ, en güçlü kanıt]
https://forums.unrealengine.com/t/ue5-6-crashes-immediately-on-startup/2548251 (2. sayfa: `?page=2`)
- Etkilenenler: Ryzen 5 5600G, 4500U, 3250U, 5500U, 5800H iGPU'ları. Birkaç RX 6900 XT / RTX 3060 Ti / RX 9070 kullanıcısı da etkilendi.
- Hata: D3D12RHI `GetClockCalibration(...) failed with error E_FAIL`.
- **Çözüm:** kısayolun Hedef alanına `-d3d11` eklemek. Birden çok kullanıcı onayladı ve çözüm olarak işaretlendi. Bir kullanıcı uyarıyor: Epic Launcher'dan değil, kısayoldan başlatmak gerekiyor.
- Bazı kullanıcılarda yardımcı olan diğer adımlar:
  - Windows'ta "Hardware-accelerated GPU scheduling" ve "Optimizations for windowed games" kapatmak.
  - Bink Media Player eklentisini kapatmak.
  - VPN'i kapatmak.
- Epic resmi açıklama yapmadı. Bir kullanıcı 5.6.1'de bile paketli oyunun çöktüğünü yazdı.
- Bizim hatamız farklı (GetClockCalibration değil, "GPU hung"). Ama donanım ailesi ve "DX12 çöküyor, DX11 açılıyor" deseni aynı.

**UE 5.7 → 5.8, GCN (wave64) kartlarda compute PSO E_INVALIDARG** [DOĞRULANDI, Epic yanıtı 2026-08-03]
https://forums.unrealengine.com/t/compute-psos-fail-with-e-invalidarg-on-amd-gcn-wave64-only-gpus/2753988
https://forums.unrealengine.com/t/error-shader-global-fhairinterpolationcs-207-engine-private-hairstrands-hairstrandsinterpolation-usf-maincs/2753985
- `FHairInterpolationCS` shader'ı `WAVESIZE(32)` istiyor. Windows SM6'da çalışma anı kontrolü hep geçtiği için, wave32 desteklemeyen AMD'lerde de bu yol seçiliyor.
- Düzeltme 5.8'de: CL 51782802 ve CL 53630446.
- `r.HairStrands.Strands 0` sorunu **çözmüyor**. Cards yolu da aynı interpolasyonu çalıştırıyor.
- Epic'in istediği tanı satırı: `LogD3D12RHI: Wave Operations are supported (wave size: min=.. max=..)`. Vega'da `min=64 max=64` beklenir.
- Bizim için anlamı: Vega (GCN5) da yalnız wave64. **MetaHuman saçı (groom) bu PC'de ayrıca riskli.** Epic örneği RX 590 (Polaris) idi, Vega adıyla geçmiyor.

**Intel UHD 630 / UE 5.3.2** [KISMİ]
https://forums.unrealengine.com/t/cannot-launch-unreal-engine-5-3-2/1473125
- Hata: D3D12 aygıtı başlatılırken access violation.
- Önerilen ayar: `DefaultEngine.ini` içine `DefaultGraphicsRHI=DefaultGraphicsRHI_DX11`. Onaylanmadı; kullanıcı sonunda UE4'e geçti.

**Intel iGPU / UE 5.2** [KISMİ]
https://forums.unrealengine.com/t/unrealeditor-on-an-integrated-gpu/1215121
- Windows'ta çalıştı, Linux'ta Vulkan bellek hatası verdi.
- Öneri: yeni projeyi "Scalable" ön ayarıyla açmak.
- Vulkan'ın DX12'den çok bellek kullandığı belirtiliyor. `-vulkan` için Vega iGPU başarı raporu **bulunamadı**.

**AMD sürücü sürümleri** [KISMİ, iGPU değil]
- UE 5.5'te RX 6800/6900 XT: Adrenalin 25.3.1 ile kararsızlık başladı, 25.2.1'e dönünce düzeldi. Epic "muhtemelen sürücü" dedi.
  https://forums.unrealengine.com/t/amd-instability-starting-25-3-1/2598151
- 5.6.1'de sürücüyü geri almak DX12'yi düzeltti (D4'te var).
- 5.7 Preview'da 25.10.12 ile DX12 bozuldu (RX 7900XT):
  https://forums.unrealengine.com/t/ue-5-7-preview-dx12-broken-on-amd-7900xt-driver-25-10-12-released-10-13-2025/2670872
- **Vega iGPU + UE5 için "iyi sürücü" listesi bulunamadı.**
- AMD'nin 5600G sayfası Win10 için ana dal Adrenalin 26.11.2'yi (2026-11-25) gösteriyor. Yani 5000G iGPU, ayrık Vega/Polaris'in bakım dalında (23.19.x, ör. 26.5.2) **değil** [KISMİ: sayfadaki tarihler "güncellendi" tarihinden yeni, kendisi de tutarsız].
  https://www.amd.com/en/support/downloads/drivers.html/processors/ryzen/ryzen-5000-series/amd-ryzen-5-5600g.html
  https://www.pcgameshardware.de/Grafikkarten-Grafikkarte-97980/News/AMD-Radeon-Adrenalin-Edition-26-5-2-Polaris-Vega-Treiber-1536146/

**Çin, Rusya, Japonya, Kore ve Türkiye kaynakları**
- Bilibili'deki Çince yazı, Epic'in "GPU çökmesi" belgesinin çevirisi: TdrDelay/TdrDdiDelay=60, `-gpucrashdebugging`, DX11 deneyin. iGPU'ya özel bilgi yok.
  https://www.bilibili.com/opus/913260001636646920
- Çince "MetaHuman oluştururken 5.8 AMD'de çöküyor" başlığı yanıtsız:
  https://forums.unrealengine.com/t/ue-5-8-crashes-immediately-when-creating-a-metahuman-with-an-amd-graphics-card-ue5-8-metahuman-ue5-8-metahuman-amd-ue5-8-amd/2730056
- Rusça "5.6'da MetaHuman oluşturunca çöküyor" başlığı yanıtsız:
  https://forums.unrealengine.com/t/topic/2672013
- Türkçe 0x887A0006 HUNG başlığı: RTX 5080'de. Önerilenler DDU ile temiz sürücü, overlay kapatma, TDR. Onaylanmış çözüm yok.
  https://forums.unrealengine.com/t/unreal-engine-is-exiting-due-to-d3d-device-being-lost-hata-kodu-0x887a0006-hung-hatasi/2692221
- Japonca ve Korece aramalar yalnızca aynı Epic forumlarına ve Epic belgesinin çevirilerine çıktı. **iGPU'ya özel ek çözüm yok.**

### 1.2 Epic'in resmi "GPU çökmesi" belgesi [DOĞRULANDI]
https://dev.epicgames.com/documentation/en-us/unreal-engine/dealing-with-a-gpu-crash-when-using-unreal-engine
- `-nogpucrashdebugging` tüm GPU çökme hata ayıklamasını kapatır. Parça parça kapatmak için `-gpubreadcrumbs=0`, `-lightdred=0`, `-dred=0` kullanılır.
- Breadcrumbs ve lightweight DRED yalnızca Development/Debug derlemelerde varsayılan açıktır. Bunları kapatmak çökmeyi **çözmez**, yalnız yükü biraz azaltır.
- Log'da `DXGI_ERROR_DEVICE_REMOVED` ve nedenine bakılmalı. `DEVICE_RESET` ise suçlu başka bir uygulamadır.
- Belge DX11'i bir adım olarak önermiyor ("DX11 çökmeleri genelde incelenemez" diyor). iGPU ve uzak masaüstünden hiç söz etmiyor.

### 1.3 RDP'ye özgü durumlar
- Win10'da RDP oturumu varsayılan olarak "Microsoft Basic Render Driver" (yazılımla çizim) kullanır. Donanım GPU'su için Grup İlkesi şu: `Bilgisayar Yapılandırması > Yönetim Şablonları > Windows Bileşenleri > Uzak Masaüstü Hizmetleri > Uzak Masaüstü Oturumu Ana Bilgisayarı > Uzak Oturum Ortamı > "Use hardware graphics adapters for all Remote Desktop Services sessions"` = Etkin [DOĞRULANDI, birden çok kaynak].
  https://forums.unrealengine.com/t/work-from-home-how-to-use-unreal-through-remote-desktop/141157
  https://en.immers.cloud/faq/view/?id=38
  https://ansyshelp.ansys.com/public/Views/Secured/corp/v251/en/install_discovery/disc_trouble_remote_desktop.html
  https://learn.microsoft.com/en-us/answers/questions/1684976/bug-in-win11-rdp-host-hardware-graphics-are-used-b
- **Bizim log'umuz:** UE AMD adaptörünü seçip D3D12 ile ilk kareye kadar geldi ve gerçek bir GPU hang aldı. Yani **donanım GPU'su zaten kullanılıyordu**. İlke ya açık ya da UE adaptörü yine de görebiliyor [DOĞRULANMADI].
- "Aynı AMD adaptörü 3 kez listelenmesi" RDP/sanal ekran tuhaflığı olabilir. Bu konuda kaynak bulunamadı [DOĞRULANMADI].
- UE'de adaptör seçimi `-graphicsadapter=N` / `r.GraphicsAdapter` ile yapılır (UE'nin bilinen bayrağı, bu PC'de denenmedi).
- Çoklu GPU'lu bir makinede RDP, oturumu yanlış GPU'ya bağlamıştı. Çözüm, Uzak Masaüstü uygulamasına Windows grafik ayarlarından GPU atamak oldu [KISMİ].
  https://forums.unrealengine.com/t/gpu-prioritization-for-multi-gpu-setup-going-wrong-when-working-remote/755156
- RDP'de editörde fare/kamera döndürme sorunu bilinen bir konu. `-norelativemousemode` bayrağı bazılarına yardım etti, bazılarına etmedi. Bazı kullanıcılar Parsec öneriyor [KISMİ].
- **iGPU + RDP riski:** iGPU hem ekranı hem işi taşıyor. TDR kurtaramazsa tüm PC donar; ikinci açılışta olan buydu. Kiralık PC'de bu, sağlayıcının yeniden başlatmasına muhtaç kalmak demek. TDR süresini uzatmak (Epic: 60 sn) donmayı **uzatır**. Bu PC'de önerilmez (D4 §2.3).

### 1.4 Editörü açmadan ayarları düşürmek (değerler bu sürümde denenmedi)
Proje klasöründe `Config/DefaultEngine.ini`:
```
[/Script/WindowsTargetPlatform.WindowsTargetSettings]
DefaultGraphicsRHI=DefaultGraphicsRHI_DX11

[/Script/Engine.RendererSettings]
r.DynamicGlobalIlluminationMethod=0
r.ReflectionMethod=2
r.Shadow.Virtual.Enable=0
r.RayTracing=False
r.Nanite.ProjectEnabled=False
```
- `DefaultGraphicsRHI_DX11` anahtarının adı forumda geçiyor [KISMİ].
- Diğer cvar adları UE'de bilinen adlar. 5.8.4'te hepsinin geçerli olduğu doğrulanmadı [DOĞRULANMADI].
- Önce **yeni, boş bir proje** kullanılmalı: Blank şablonu, "Scalable" ön ayarı, Starter Content ve ışın izleme kapalı.

---

## 2. MetaHuman'ı güçlü GPU olmadan oluşturmak / dışa aktarmak

### 2.1 Resmi belgeler
- **Donanım** [DOĞRULANDI]: Creator ve Animator için önerilen GPU RTX 3070 / RX 6800 XT, 8 GB VRAM. Ayrı bir "minimum" yok. Animator: "DirectX 12 ... default RHI set to DirectX 12". Creator strands: SM6. Assembly büyük dokularda 32 GB RAM'i aşabilir. DX11 ve iGPU'dan söz edilmiyor.
  https://dev.epicgames.com/documentation/metahuman/metahuman-hardware-requirements-in-unreal-engine
- **Python API (5.8+)** [DOĞRULANDI]:
  - Belge "headless or batch scripts" ve "batch and commandlet contexts" ifadelerini kullanıyor.
  - Fonksiyonlar: `build_meta_human(character, params)` (`examples/example_assembly.py`) ve `export_dcc`, `export_dna`, `export_geometry`, `export_materials`, `export_posed_dna` (`examples/example_export_tools.py`).
  - Bulut gerektiren adımlar: `request_texture_sources`, `request_auto_rigging` (Epic hesabı ile).
  - **`-nullrhi`, `-RenderOffscreen` ve `-run=pythonscript` geçmiyor. Assembly/export için GPU gereksinimi yazılmamış.**
  https://dev.epicgames.com/documentation/metahuman/metahuman-creator-python-scripting-in-unreal-engine
- **Bilinen sorun** [DOĞRULANDI]: harici süreçten/CLI'dan Python ile `request_auto_rigging` + `blocking=True` çağrısı 5.7'de çöktü. Epic 5.7.4'te bunu yeniden üretemedi; çözüm yok. Epic'in notuna göre Python belgesi bir süre kayıptı, sonra geri getirildi.
  https://forums.unrealengine.com/t/metahuman-export-crash/2727915
- **GPU'suz pişirme ipucu** [KISMİ]: GPU'suz bulut makinede glTF malzeme pişirme sorusuna Epic destek personeli şöyle sordu: "Have you tried the -warp -RenderOffscreen options?" Onaylanmadı.
  https://forums.unrealengine.com/t/export-gltf-with-material-baking-on-cloud-instance-without-a-gpu/2724659

### 2.2 Bu PC için anlamı [DOĞRULANMADI]
- Denenebilecek komut: `UnrealEditor-Cmd.exe Proje.uproject -ExecutePythonScript="...py" -warp -RenderOffscreen -unattended`.
  - WARP, CPU ile D3D12 çizer. Vega iGPU'yu hiç kullanmadığı için TDR/donma riski düşük, ama çok yavaş.
  - Assembly'deki doku/malzeme pişirme WARP'ta çalışabilir de, çökebilir de.
  - 64 GB RAM bunun için avantaj.
- `-nullrhi` ile assembly'nin doku adımları büyük olasılıkla çalışmaz, çünkü RHI yok. Belgelenmemiş.
- Ön koşul olarak elde bir **MetaHuman Character** varlığı olmalı. Bunu üretmenin yolları: Python ile parametre/preset'ten oluşturmak, ya da güçlü GPU'lu bir makinede (kiralık bulut, D4 §1) Creator'da yapmak. Autorig ve dokular bulutta yapılır, internet ve Epic hesabı gerekir.
- En gerçekçi yol: karakteri güçlü bir bulut GPU'sunda bir kez oluşturup DCC Export almak, ardından Unity'de kullanmak (D4 §3). Bu PC'de yalnız Unity tarafı çalışır.

---

## 3. Eski UE (4.27 / 5.0–5.5) ve Quixel Bridge MetaHuman'ları

- **Takvim** [DOĞRULANDI, Epic duyurusu]:
  - 5 Nisan 2026: 4.27/5.0/5.1 oturumları kapandı.
  - 5 Haziran 2026: 5.2–5.4 kapandı.
  - **5 Kasım 2026:** 5.5 kapandı ve web uygulaması tamamen kapandı.
  - Ardından 90 gün yalnızca **galerideki mevcut** karakterler Bridge'den indirilebilir (≈ 3 Şubat 2027). Sonra silinir.
  - Web uygulaması kapandıktan sonra 5.2–5.5 Animator'daki "Auto-Rig Identity (Skeletal Mesh + Full MetaHuman)" özelliği de çalışmaz.
  https://forums.unrealengine.com/t/metahuman-creator-web-application-is-being-discontinued/2695297
  https://unrealsource.com/d/metahuman-creator-web-app-discontinued/
- Bugün (28 Kasım 2026) **yeni** bir web MetaHuman oluşturmak mümkün değil. Hesapta daha önce yapılmış karakter yoksa Bridge yolu **kapalı**.
- Bridge'de MetaHuman görünmeme sorunları 2026'da da raporlandı (5.5/5.7). Önerilen çözüm (tüm hesaplardan çıkış yapmak) işe yaramadı.
  https://forums.unrealengine.com/t/metahumans-will-not-appear-in-quixel-bridge/2701083
- Eski UE'nin zayıf GPU'da çalışıp çalışmadığı:
  - 4.27 varsayılan olarak DX11 kullanır ve MetaHuman ilk kez 4.26/4.27 için çıktı. Teknik olarak iGPU'da düşük LOD ve cards saçla açılması **olası** [DOĞRULANMADI].
  - Bir kullanıcıya göre UE 5.1'de DX11 ile MetaHuman Identity "Track Markers" çalıştı [KISMİ].
    https://forums.unrealengine.com/t/unsupported-rhi/1356379/17
  - Ama karakter **edinilemediği** için bu yol artık pratik değil.
- Performans için LOD/cards saç/ışın izleme kapatma rehberi (Chris Evans, Epic topluluk eğitimi):
  https://forums.unrealengine.com/t/tutorial-metahuman-performance-and-scalability-settings/1200072

---

## 4. Önerilen deneme sırası (düşük risk önce)

| # | Adım | Kim yapar | Risk |
|---|---|---|---|
| 1 | Son çökmenin `Saved/Logs` dosyasında `DXGI_ERROR_DEVICE_REMOVED` nedenine ve `Wave Operations ... min/max` satırına bak | Biz/kullanıcı | Yok |
| 2 | Sağlayıcıdan, PC donarsa uzaktan yeniden başlatma yapabileceği sözünü al | Kullanıcı → sağlayıcı | Yok |
| 3 | Yeni boş proje (Blank, Scalable) aç; `Config/DefaultEngine.ini` içine §1.4'teki DX11 ayarını önceden yaz; kısayola `-d3d11 -nogpucrashdebugging` ekleyerek **kısayoldan** başlat | Kullanıcı | Düşük-orta (yine donabilir) |
| 4 | Windows'ta HAGS ve "pencereli oyun iyileştirmeleri"ni kapat; Bink Media eklentisini kapat; VPN'i kapat | Kullanıcı | Düşük |
| 5 | DX11 açılırsa: MetaHuman eklentilerini etkinleştir, Creator'ı **açmayı dene** (kanıt yok, büyük olasılıkla açılmaz ya da saç/doku eksik olur) | Kullanıcı | Orta (çökme) |
| 6 | GPU'suz yol: `-warp -RenderOffscreen` ile Python batch assembly/export denemesi (5.8 API) | Biz komut dosyasını hazırlarız, kullanıcı çalıştırır | Düşük donma riski, çok yavaş, başarısı belirsiz |
| 7 | AMD sürücüsünü değiştirmek (temiz kurulum / geri alma) | **Sağlayıcı** (RDP kopabilir) | Orta-yüksek |
| 8 | TDR kayıt anahtarlarını uzatmak | Önerilmez | Yüksek (uzun donma) |
| 9 | Gerçekçi alternatif: karakteri kiralık güçlü GPU'da bir kez yapıp DCC Export almak, bu PC'de yalnızca Unity kullanmak (D4) | Kullanıcı (hesap gerekir) | Düşük |

Değiştirilemeyenler: BIOS'taki UMA frame buffer boyutu (kiralık PC), Grup İlkesi (yönetici yetkisi gerekebilir; donanım GPU'su zaten kullanılıyor gibi görünüyor).
