# NOTLAR_B (kamera, 3D mekan, yonetmenlik, kayit)

## Ne yapildi
- Scripts/KameraYonetmen.cs: orbit tabanli 3D kamera yonetmeni. Otomatik plan (genis / orta / omuz / yakin / tepki), ayni plan ust uste 3 kez olmaz, 6 plan icinde mutlaka bir genis. El titremesi Perlin ile ~0.2 derece, panikte daha guclu.
- Scripts/Mekan3D.cs: koddan kurulan oda, sokak, koy, hastane (Figuran3D dahil). Dahili esyalar yoksa basit seklerle kurulur.
- Scripts/AtmosferVolume.cs: calisma zamaninda URP Volume (yansima ile, bulunamazsa sessizce atlar) + 2D mekan icin ust katman yaklasimi; atmosfer, ani, zaman, gecis, sessizlik, isik.
- Scripts/Yonetmen3D.cs: yeni komutlari calistirir ve dogrular (`YonetmenTablo`).
- Scripts/Sahne3D.cs: yeniden yazildi; eski imzalar korundu. Yeni: mekan, noktalar, gir/cikar, figuran, yonetmen baglantisi.
- Scripts/StoryModel.cs: yeni komutlar eklendi, A'nin bak/dinle/altyazi eklemeleri birlestirildi. Eski hikayeler ayni sekilde cozulur; bilinmeyen komut uyari verir, hata vermez.
- Editor/StudyoKayit.cs: kayit oncesi/sonrasi kontroller, guvenli alan cizgisi kayitta gizlenir, kayit sonrasi dosya dogrulama.
- Editor/HikayeKuyrugu.cs: "Hikaye/Hepsini uret ve kaydet" toplu kuyruk (Progress API, domain reload'dan sonra devam eder), "Hikaye/Kuyrugu durdur".
- Editor/RitimDenetci.cs: 5 sn ustu sabit plan sari, 8 sn ustu kirmizi; genis/orta/yakin dagilimi. Menu: "Hikaye/Ritim denetimi (secili hikaye)".
- StoryPlayer.cs icin yamalar: PATCH_B.md (PATCH_A'dan sonra uygula).

## Yeni hikaye komutlari
```
mekan3d: oda            (oda | sokak | koy | hastane | yok)
yer: Ayse yatak         (noktalar: kapi, pencere, yatak, sofra; sokakta lamba, bank; koyde kuyu; ayrica sol, sag, orta)
gir: Ali kapi           (kapi acilir, Ali yerine yuruyerek gelir)
cikar: Ali kapi         (kapidan cikar; kapisiz eski cikar: Ali ayni sekilde calisir)
figuran: 6              (arka planda 0-14 kisi, rastgele bekleme animasyonu)
kamera: tepki Ali       (dinleyenin yuz tepkisine kesim)
kamera: yaklas | uzaklas
kamera: goz | yukaridan | asagidan
kamera: bulanik | net  (URP DepthOfField; yoksa atlanir)
atmosfer: huzun | sicak_ani | kabus | normal
ani: sepya | vhs | soluk | bitir
zaman: sabah | gun_batimi | gece
gecis: karar | capraz | beyaz
sessizlik: 3            (muzigi keser, sadece oda sesi kalir)
isik: <on ayar>
```
Ornek:
```
mekan3d: oda
zaman: gece
atmosfer: huzun
figuran: 3
gir: Ayse kapi
kamera: tepki Ali
sessizlik: 2
gecis: karar
```

## Bilinen riskler
- Derleyici yok, tum kod elle kontrol edildi; ilk acilista Console'da hata cikarsa dosya ve satiri bildirin.
- Yatak noktasinda yatan karakterin yonu (yaw) dogrulanmadi; gerekirse Mekan3D.KurOda'daki yatak yonunu degistirin.
- Animator.speed, Playable graph ile oynayan animasyonlarda figuran hizini etkilemeyebilir.
- `gecis: capraz` ScreenCapture ile ekran goruntusu alir; kayitta bir kare gecikme olabilir.
- Sessizlik oda sesi ve sure icin Resources.Load kullanilir; ses klasoru farkliysa sure tahmin edilir.
- Esya klasor adlari tahmin edildi (Karakterler/Esyalar); bulunamazsa basit seklerle kurulur.
- GameView ve AudioManager kontrolleri dahili alan adlarina (m_DefaultSpeakerMode vb.) yansima ile bakar; Unity surumunde degisirse sadece uyari yazar, kayit durmaz.
- 2D mekanda (opak 3D oda yokken) URP Volume etkisizdir (Overlay UI); gorunum ust katman resimleriyle taklit edilir.
- Ajan C: StudyoDogrulama / StudyoWindow `ItemType.Direct` ve yeni kamera degerlerini henuz tanimiyor (PATCH_B.md sonundaki not).
