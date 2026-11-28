# Mevcut proje (27 Kas 2026'da incelendi)

## Yapı
- Unity 6000.6.9f1, URP 17.6.0. Kod: `Assets/HikayeStudyo/` (~105 C# dosyası, ~34.700 satır). asmdef: Hikaye.Runtime (Scripts/), Hikaye.App (Uygulama/), Hikaye.Editor (Editor/). C# 9, namespace HikayeStudyo.
- Kurulu paketler (Packages/manifest.json, cihazdan okundu): **cinemachine 6.6.0 (zaten kurulu)**, cloud.gltfast 6.21.0, cloud.draco 5.4.3, recorder 5.1.8, timelinefoundation modülü, inputsystem, ugui. Proje kuralı: yeni paket için kullanıcı onayı.
- Hikâye metni (`Docs/HikayeSozdizimi.md`) → ayrıştırıcı → `StoryPlayer.cs` (1609 satır) → `Sahne3D*.cs`, `Yonetmen3D.cs`, `KameraYonetmen.cs`, `Mekan3D*.cs` (kodla üretilen mekânlar), `Sahne3D.Hareket.cs`, `YuzCanlilik.cs`, `AgizSenkron.cs`, `KaraokeAltyazi.cs`, `EfektKatmani.cs`, `SinematikEfekt.cs`.
- Karakterler: Microsoft Rocketbox 115 gerçekçi karakter (`Gercek_*`, MIT), VRoid/VRM, uygulamanın kendi karakterleri.
- Sesler: Gemini TTS (ücretsiz ~10 istek/gün), satır başına `Resources/Ses/<hikaye>/NN.wav` (NN 01'den).
- Video: Editörde Unity Recorder (`StudyoKayit.cs`); EXE'de ffmpeg borusu (`FfmpegPipeRecorder`). 1080x1920, H.264+AAC.
- Köprü: `HikayeKomut/` dosya komutları (CLAUDE.md).

## Donanım (HikayeKomut/donanim.txt, 26 Kas'ta okundu)
AMD Ryzen 5 5600GT (12 iş parçacığı), 64 GB RAM, Gigabyte A520M K V2, yalnız tümleşik AMD Radeon (ayrı kart yok), ray tracing yok, compute var, Direct3D11, C: 45 GB boş, Windows 10. (Güç kaynağı ve kasa BİLİNMİYOR.)

## Bilinen kusurlar (gerçek gözlem)
- Kullanıcı 26 Kas'ta 5 dk'lık filmi reddetti: altyazı-konuşma uyumsuz (toplu sesin tahminle bölünmesi olası neden; dinlenmedi), karakterler yerinde saçma hareket, yürüme/koşma yok.
- 26 Kas akşam 30 sn'lik sessiz önizleme (EXPERIMENTS/D001): kusurlar orada.
