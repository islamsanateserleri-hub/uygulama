# K01 Kullanıcının kendi araştırması (27 Kas 2026'da sohbete yapıştırdı)
İki belge geldi (büyük olasılıkla başka bir yardımcıdan alınmış metin; kaynak bağlantısı yok). Aşağıdaki özet belgelerin söylediğidir; **doğrulanmış bilgi DEĞİL** (güven sınıfı D, ta ki ayrıca doğrulanana kadar). Belgelerin sonundaki sorular kullanıcının değil, metni yazanın sorularıdır.

## Belge 1 (özet)
- Motor: Unreal Engine 5 (Lumen, Nanite, Movie Render Queue).
- Karakter: MetaHuman; hareket: mocap kütüphaneleri/Mixamo; dudak: kural tabanlı fonem→blendshape (OVR LipSync / JALI mantığı).
- Ortam/efekt: Quixel Megascans, Niagara; sanal sinema lensi, DoF, grain, LUT, lens flare.
- Yazılım: kural tabanlı ayrıştırıcı + otomatik sahne kurucu ("director"), ses/kurgu FFmpeg + Wwise/FMOD.

## Belge 2 (özet)
- Arayüz (Electron/React ya da C# WPF/Avalonia) + arka planda UE5.
- Python (spaCy/regex) senaryo ayrıştırıcı → JSON; şablon modu ve serbest hikâye modu.
- 10 dk film → 40–60 parça (10–15 sn) → UE komut satırından/MRQ ile çekim → FFmpeg montaj, müzik, LUT, 2,39:1 siyah bar.
- Yol haritası: 5 mekân, 4 MetaHuman, 30 animasyon → JSON'dan MP4 prototipi → kamera yönetmeni (4–5 sn'de açı) → arayüz.
- Donanım: RTX 3060+ ya da bulut GPU.

## Ana araştırmacının ilk karşılaştırması (yeni arama yapılmadan)
| Belgedeki öneri | Araştırma bulgusuyla ilişki | Durum |
|---|---|---|
| Kural tabanlı ayrıştırıcı + yönetmen, FFmpeg | Projede büyük ölçüde VAR (etiketli sözdizimi, Yonetmen3D, ffmpeg borusu) | uyumlu |
| Parça parça çekim + birleştirme | A09 "çekim listesi + kurtarılabilir kayıt", A08 "kurgu.json" ile aynı yönde | uyumlu |
| MetaHuman | A02: 1M $ gelir altı ücretsiz, Unity'ye de alınabiliyor; Creator güçlü GPU ister; uygulama içi kütüphane olarak dağıtım izni doğrulanamadı | koşullu |
| UE5'e geçmek | Uygulamanın sahne tarafının yeniden yazılması demek; bu PC'de GPU yok. A09 ve A10 ile birlikte karar konusu | açık (Q-013) |
| Quixel Megascans | A05 lisans önceliğine aldı (Fab Standard); fiyat/koşul A12 denetleyecek | doğrulanacak |
| OVR LipSync | A07 doğrulayamadı; güncel bakım durumu araştırılacak | doğrulanacak (Q-014) |
| Wwise / FMOD | Lisans koşulu araştırılmadı | doğrulanacak (A12) |
| 2,39:1 siyah bar | Hedef 9:16 dikey TikTok ile çelişir | uygulanmamalı (C-011) |
| 4–5 sn'de açı, 24 fps, 10 dk | A08: hedef medyan plan 2,5–3 sn; proje hedefi 30 fps ve kısa dikey drama | farklı |
| spaCy ile serbest metin | spaCy makine öğrenmesi modeli kullanır; "yapay zekâsız" şartıyla ilişkisi kullanıcıya bağlı; Türkçe doğruluğu araştırılmadı | açık (Q-015) |
