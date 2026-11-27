# A07 — Ses, TTS, Diyalog ve Dudak Senkronizasyonu Uzmanı

Durum: TAMAMLANDI (WebSearch hakkı oturum sonunda doldu; bkz. Öz denetim). Tarih: 2026-11-27.

## Görev tanımı
Protokol ALT AJAN 7 + §9.8: TTS, ses klonlama ve lisans, duygu/ton, zamanlama, dudak senkronu, yüz animasyonu, foley/ambiyans/müzik/efekt, miksaj, altyazı senkronu, ses yönetimi, maliyet, offline/online, gelecek. Teslim: ekonomik ve kaliteli ses-diyalog-dudak-ses yönetim sistemi. Kod yazılmadı, ses üretilmedi, hiçbir test koşulmadı.

## Araştırma alanı ve soruları
- S0: Altyazı-ses uyumsuzluğunun koddaki olası nedenleri neler?
- S1: Türkçe TTS seçenekleri, kota/fiyat/lisans?
- S2: Ses klonlama lisans/izin; duygu/ton kontrolü?
- S3: Zaman damgalı TTS ve forced alignment (Türkçe)?
- S4: Altyazı senkronu iyi uygulamaları?
- S5: Dudak senkronu ve yüz animasyonu (Rocketbox uyumu)?
- S6: Foley/ambiyans/müzik/efekt lisanslı kaynaklar?
- S7: Miksaj ve TikTok ses düzeyi?
- S8: Offline/online, gelecek?

## Bulgular

### S0 — Kod incelemesi (hepsi ÇIKARIM; kod okundu, çalıştırılmadı, ses dinlenmedi)
Kısaltma: yollar `Assets/HikayeStudyo/` altındadır.

| # | Olası neden | Yer | Etki | Olasılık (reddedilen film için) |
|---|---|---|---|---|
| K1 | Toplu sesi satırlara bölme, elle/dışarıda hazırlanan `kesim.txt` örnek aralıklarına körü körüne uyuyor; içerik doğrulaması yok, sınır denetimi yok. Proje notu da "bölme tahminle yapıldı, sesler hiç dinlenmedi" diyor (`DURUM.md`). | Editor/TopluSes.cs:41-67 | Kayan/kesik kelime; altyazı bir cümle, ses başka | Yüksek (ana neden) |
| K2 | `NN.wav` (konuşma sıra numarası) yedek adlandırması: hikâyeye satır eklenip silinirse eski NN dosyaları başka satıra bağlanır; ClipKey yoksa NN sessizce kabul edilir ve `kaydet` denetimi de geçer. Toplu bölme yalnız NN adıyla yazıyor. | Scripts/StoryPlayer.cs:308-309; Editor/GeminiSes.cs:64-67; Editor/TopluSes.cs:62; Scripts/DisSes.cs:67-68; Scripts/StoryModel.cs:319 | Bütün satırların bir kayması | Orta (metin kayıttan sonra değiştiyse) |
| K3 | Karaoke kelime zamanları gerçek zaman damgası değil: kelime karakter sayısı + noktalama ağırlığı ile orantılı dağıtım. Duraklama, duygu, hız değişimi görülmez. | Scripts/KaraokeAltyazi.cs:157, 170-184, 228-243 | Doğru ses dosyasında bile kelime vurgusu kayar | Karaoke açık satırlarda kesin yaklaşık; büyüklüğü ölçülmedi |
| K4 | Ağırlık ekrandaki metinden, ses ise normalize metinden (`TurkceMetin.ForSpeech`: sayı vb. yazıya çevrilir). "1993" 4 karakter ağırlık alır, sesi ~5 kelime sürer. | Scripts/KaraokeAltyazi.cs:157,172 ↔ Editor/GeminiSes.cs:411-412 | Sayı/kısaltma içeren satırda belirgin kayma | Satıra bağlı |
| K5 | Ses içeriği denetlenmiyor: yalnız "fazla kısa" denetimi (beklenenin %70'i altı) var; fazladan söz, tekrar, stil metninin okunması, kelime atlama yakalanmaz. En UZUN deneme "en iyi" sayılıyor. | Editor/GeminiSes.cs:495-501, 545-550; Scripts/TurkceMetin.cs:392-402 | Altyazıda olmayan/eksik konuşma | Bilinmiyor (dinlenmedi) |
| K6 | `ProcAudio.Clean` çalışma anında baş/son sessizliği atıp 0,5 sn üstü iç boşlukları 0,32 sn'ye indiriyor. Bugün oyuncu kendi içinde tutarlı (zarf temizlenmiş klipten), ama dışarıdan (TTS/hizalayıcı) alınacak zaman damgaları ham dosyaya ait olur ve KAYAR. Eşik tepeye göre olduğundan fısıltılı baş/son heceler de kesilebilir. | Scripts/StoryPlayer.cs:311; Scripts/ProcAudio.cs:14-99 | Yeni hizalama sistemi için tasarım tuzağı | Bugün düşük, gelecekte kritik |
| K7 | Sert bölüm sonundan önce `dur` 0,3 sn kısaltılıyor; kelime zamanları kısaltılmış süreye sıkışıyor/son kelime kesiliyor. | Scripts/StoryPlayer.cs:1031-1036; Scripts/KaraokeAltyazi.cs:230-241 | Yalnız o satır | Düşük |
| K8 | Altyazı saati `Time.deltaTime` toplamı, ses saati DSP. Kayıtta sabit kare hızı + Recorder/AudioRenderer ile eşleşir; canlı önizlemede takılma olursa altyazı ve ağız geride kalır (yüz ayrıca dt'yi 0,1 sn ile sınırlar). `voiceSrc.timeSamples` kullanılmıyor. | Scripts/KaraokeAltyazi.cs:429-439; Scripts/StoryPlayer.cs:1048-1052; Scripts/YuzCanlilik.cs:531,559; Editor/StudyoKayit.cs:308-311; Uygulama/FfmpegPipeRecorder.cs:203, 275-281 | Önizlemede kayma; son videoda büyük olasılıkla yok (test edilmedi) | Düşük |
| K9 | Klasik altyazı modunda bütün satır tek seferde gösterilir; uzun satırda "uyumsuz" algısı. | Scripts/StoryPlayer.cs:1038-1042 | Algısal | Düşük |

K1 notu: Dosyalar repoda yok (`Resources/Ses` boş/izlenmiyor), `kesim.txt` bulunamadı; hangi satırın gerçekten bozuk olduğu doğrulanamadı.

**Yeni bulgu (ÇIKARIM, yüksek güven ama test edilmedi):** Rocketbox `_facial` modellerinde 15 viseme (`AA_VI_00_Sil` … `AA_VI_14_U`) ve 52 ARKit şekli (`AK_xx_*`) bulunur [A07-S031][A07-S032]; proje `_facial.fbx` kopyalıyor (Editor/RocketboxKur.cs:78). Fakat ad eşleyici (Scripts/YuzCanlilik.cs:441-456) `AA_VI_11_E/12_I/13_O/14_U/01_PP/02_FF` adlarını TANIMIYOR; yalnız `AA_VI_10_aa` eşleşiyor, o da `AK_25_JawOpen` daha öncelikli olduğu için eleniyor (satır 406-412). Yani gerçekçi karakterlerde büyük olasılıkla yalnız çene açılıp kapanıyor, hiç viseme kullanılmıyor. Doğrulama: `YuzTesti` / `rig_raporu` ile şekil adlarına bakmak.

Dudak (ÇIKARIM): ağız açıklığı yalnız ses genliğinden (50 Hz RMS zarfı, Scripts/AgizSenkron.cs:25-72); ünlü şekilleri Perlin gürültüsüyle RASTGELE seçiliyor (Scripts/YuzCanlilik.cs:853-863), yani konuşulan fonemle ilişkisi yok; dudak kapanışı (b, p, m) gösterilmiyor. İç seste ağız kapalı (doğru).

Ses düzeyi: `SesOlcer` BS.1770 ölçümü + öğrenen kazanç (sonraki kayda uygulanır) + anlık sınırlayıcı (Scripts/SesOlcer.cs:85-151); ilk kayıt hedefi tutturamayabilir (filmde -16,5 LUFS, DURUM.md). Müzik kısma (ducking) elle yazılmış basit mantık (Scripts/StoryPlayer.cs:1574-1579).

### S1 — Türkçe TTS seçenekleri (fiyatlar değişebilir)

| Seçenek | Türkçe | Duygu/ton | Zaman damgası | Fiyat / kota | Lisans / koşul | Bu bilgisayar |
|---|---|---|---|---|---|---|
| Gemini 3.8 Flash TTS (mevcut) | Evet [A07-S001] | `style` + `<sigh>` gibi etiketler; style yalnız söyleyiş/duyguyu değiştirir, sesin kimliğini değil [A07-S001][A07-S036] | Belgede yok [A07-S001] | Metin 0,50 $, ses 9 $/1M token (≈0,000225 $/sn); 1 Oca 2027'den itibaren 1 $/18 $. Lite 6→12 $ [A07-S002][A07-S005]. Ücretsiz katman var; model bazlı sınır yayımlanmıyor [A07-S004]; projenin gözlemi ~10 istek/gün | Çıktı sahipliği iddia edilmez; ücretsizde veri kullanılır ve insan inceleyebilir; 18+; AEA/CH/UK kullanıcılarına sunumda paralı zorunlu [A07-S006]. SynthID filigranı [A07-S048]. "Preview" [A07-S002] | Bulut |
| ElevenLabs (v3/v4, Multilingual v2) | Evet [A07-S044] | Ses etiketleri (v3 Conversational/v4 Turbo'da belirtilmiş) [A07-S044] | **Var:** karakter düzeyi + normalize metin hizası [A07-S014] | Free 10k kredi; Starter 6 $/30k; Creator 22 $/121k [A07-S013] | Free: ticari yasak + başlıkta atıf; ücretli: ticari serbest (Beta hariç) [A07-S012] | Bulut |
| Azure Speech (tr-TR nöral) | tr-TR sesleri (güncel liste bu oturumda okunamadı) | SSML; tr-TR stil desteği DOĞRULANAMADI | **Var:** WordBoundary; viseme olayı [A07-S041] (tr-TR viseme DOĞRULANAMADI) | F0: 0,5M karakter/ay [A07-S015] | Ticari koşul metni okunmadı | Bulut |
| Google Cloud Chirp 3 HD | tr-TR evet [A07-S042] | Duygu parametresi yok; hız/duraklama [A07-S042] | Belgede yok; `<mark>` desteklenen öğelerde yok [A07-S042] | 1M karakter/ay ücretsiz, sonra 30 $/1M; faturalama açık olmalı [A07-S002] | Cloud koşulları (okunmadı) | Bulut |
| OpenAI TTS | Evet ama sesler İngilizceye optimize [A07-S016] | `instructions` [A07-S016] | Belirtilmiyor | Okunmadı | AI sesi olduğu açıklanmalı; `gpt-4o-mini-tts`, `tts-1` 6 Oca 2027'de kapanıyor [A07-S016] — yeni entegrasyon ÖNERİLMEZ | Bulut |
| Chatterbox Multilingual | Evet [A07-S017] | Klonlama; Türkçe duygu kalitesi ölçülmemiş | Yok | Ücretsiz | MIT, her çıktıda PerTh filigran [A07-S017] | CPU hızı bilinmiyor |
| VoxCPM2 | Evet (30 dil) [A07-S018] | Ses tasarımı/klonlama | Yok | Ücretsiz | Apache-2.0 [A07-S018] | ~8 GB VRAM; bu bilgisayarda pratik değil (ÇIKARIM) |
| EMA Lightning / FreyaTTS-small | Türkçe'ye özel | Tek ses, duygu yok | Yok | Ücretsiz | Apache-2.0 (kart beyanı); eğitim verisi/ses hakkı belirsiz [A07-S019] | EMA CPU'da hızlı (yazar beyanı) |
| Piper tr_TR | dfki, fettah, fahrettin | Duygu yok | Yok (bu oturumda doğrulanmadı) | Ücretsiz | dfki verisi **CC BY-NC-SA: ticari yok** [A07-S021]; fettah verisi CC0 ama lessac temelinden ince ayar [A07-S020]; lessac lisans belirsizliği [A07-S030] → riskli | CPU |
| XTTS-v2, MMS-TTS, bazı F5-Turkish | Evet | - | - | Ücretsiz | **Ticari değil** [A07-S019][A07-S023] | - |
| Kullanıcının kendi sesi (mikrofon) | Doğal Türkçe | Tam kontrol | Hizalayıcıyla | 0 $ | En temiz hak durumu (ÇIKARIM) | Evet |

Kokoro: Türkçe desteği doğrulanamadı. F5-TTS: Türkçe topluluk ince ayarları var, lisansları birbiriyle çelişiyor (CC-BY-NC / MIT) [A07-S023].

Gemini bilinen sorunları (forum, C): kesilme, cümle atlama/tekrar, eklenen replik, ses değişmesi (2.5/3.1, çoklu konuşmacı) [A07-S035]; koşudan koşuya ses farkı, uzun metinde aksan kayması, bazı etiketlerin sesli okunması (3.1) [A07-S037]. 3.8 için aynen geçerli olduğu doğrulanmadı. Sonuç: her satır içerik denetiminden geçmeli.

Maliyet örneği (ÇIKARIM, hesap): 5 dk = 300 sn ses → Flash TTS ≈ 0,07 $ (2026), ≈ 0,14 $ (2027); iki kat deneme ile 0,14–0,27 $. Lite daha ucuz. Döküm denetimi (Gemini Transcribe ≈ 0,009 $/dk, üçüncü taraf) ≈ 0,05 $ [A07-S034]. 68 satır, günde ~10 ücretsiz istekle ~7 gün sürer; ödeme açmak toplu bölme ihtiyacını ortadan kaldırır (ödeme kararı kullanıcınındır).

### S2 — Ses klonlama, izin, duygu
- Gemini ses kopyalama: aynı yetişkinden 10–30 sn örnek + sesli onay cümlesi; Türkçe onay cümlesi tanımlı [A07-S038]. SynthID + C2PA; AI Studio'da AEA, UK, İsviçre, Hindistan, Illinois, Texas'ta yok [A07-S048].
- ElevenLabs profesyonel klon: yalnız KENDİ sesin, doğrulama zorunlu, Creator ve üstü [A07-S046]. Anlık klonda onay adımı belgede tarif edilmiyor; kullanım politikası bağlayıcı [A07-S046][A07-S045].
- OpenAI özel ses: onay kaydı ve onay süreci [A07-S016].
- KVKK Deepfake Bilgi Notu (19 Oca 2024): ses taklidi kişisel veri riski içerir [A07-S028] (ikincil özet; hukuk görüşü değil).
- TikTok: gerçekçi yapay ses/görüntü içeren içerik "AI-generated" etiketlenmeli [A07-S027].
- Öneri: gerçek kişi sesi klonlanmasın; hazır veya tarif ile tasarlanmış sesler. Kendi sesi için yazılı onay kaydı saklansın.
- Duygu: satır başına kısa `style` (mevcut) + karakter başına sabit ses; yaş/tını farkı gerekiyorsa Voice Design [A07-S036]. En iyi deneme "en uzun" değil, döküm eşleşmesi + kullanıcı dinlemesiyle seçilsin.

### S3 — Zaman damgası ve forced alignment (Türkçe)

Metin zaten bilindiği için doğru araç **forced alignment**tır (ASR ile "tahmin" değil).

| Araç | Türkçe | Çıktı | Lisans | Doğruluk kanıtı | Not |
|---|---|---|---|---|---|
| MFA 3.0 Turkish | Evet, 128 saat veri [A07-S008] | kelime + **fonem** | Model CC BY 4.0 [A07-S008] | İngilizce kelime sınırı ~20–25 ms [A07-S009]; Türkçe ölçüm yok | CPU; kurulum ağır (ÇIKARIM) |
| parseh/aligner-tr (ONNX) | Evet | kelime/karakter | CC-BY-4.0 [A07-S010] | **Gerçek konuşmada ölçülmemiş** | 30 sn ses 6,7 sn, ~2,9 GB RAM [A07-S010] |
| mpoyraz wav2vec2-tr (+WhisperX/CTC) | Evet (WER %8,6 CV7) [A07-S039] | kelime/karakter | Model CC-BY-4.0 [A07-S039]; WhisperX BSD-2 [A07-S033] | WhisperX İngilizcede >110 ms [A07-S009] | Rakam içeren kelimeler hizalanamaz [A07-S033] |
| faster-whisper / whisper.cpp / whisper.unity | Evet (çok dilli) | kelime (whisper.cpp'de deneysel) | whisper.cpp, whisper.unity MIT [A07-S040][A07-S043]; faster-whisper lisansı bu oturumda okunmadı | Türkçe ms ölçümü yok | CPU'da çalışır [A07-S011] |
| torchaudio MMS_FA | Evet (çok dilli) | kelime/harf | **CC-BY-NC: ticari yok** [A07-S047] | İngilizce ~43–50 ms [A07-S009] | Kullanma |
| ElevenLabs with-timestamps / Forced Alignment | with-timestamps: Türkçe modeller [A07-S014][A07-S044]; FA dil listesi doğrulanamadı | karakter | ElevenLabs koşulları | ölçüm yok | Kredi harcar |
| Azure WordBoundary | tr-TR sesleriyle | kelime başlangıcı | Azure koşulları | ölçüm yok | Yalnız kendi sentezi [A07-S041] |
| Gemini 3.5 Transcribe | tr-TR [A07-S007] | kelime | API koşulları | Zaman damgası döküm doğruluğunu düşürür [A07-S007]; eski genel Gemini modellerinde zaman kayması şikâyeti [A07-S029] | Bulut, ucuz |

Amazon Polly Türkçe fonem→viseme tablosu yayımlıyor [A07-S049]; Polly fiyat/lisansı incelenmedi.

### S4 — Altyazı senkronu iyi uygulamaları
- Netflix Türkçe: en çok 2 satır, yetişkin ≤17 karakter/sn, çocuk ≤13 [A07-S024]; olay süresi en az 5/6 sn, en çok 7 sn [A07-S025]. (Film altyazısı ölçütü; TikTok karaoke için yol gösterici.)
- Algı toleransı (ses-görüntü, ITU-R BT.1359): fark etme +45/−125 ms; kabul +90/−185 ms (artı: ses önde) [A07-S026, ikincil kaynak]. Dudak senkronu için geçerli ölçüt; altyazıya uyarlanması ÇIKARIM.
- ÇIKARIM kurallar: (1) zaman yalnız hizalama dosyasından; (2) hizalama yoksa sahte karaoke yok, satır/öbek altyazısı; (3) sayfa kesimi duraklama ve noktalamada, 2–5 kelime; (4) saat `voiceSrc.timeSamples / clip.frequency` [A07-S050] (kayıt modunda davranışı test edilmeli); (5) ekran metni ↔ okunan metin için kelime eşleme tablosu; (6) hedef: kelime başlangıcı hatası medyan ≤50 ms, %95 ≤120 ms.

### S5 — Dudak senkronu ve yüz animasyonu

| Yöntem | Nasıl | Türkçe | Rocketbox | Maliyet/lisans | Donanım | Yargı |
|---|---|---|---|---|---|---|
| Hizalama fonemi → viseme (öneri) | MFA fonem zamanı veya kelime zamanı içinde harf dağılımı | Türkçe yazım büyük ölçüde sese yakın (ÇIKARIM) | `AA_VI_*` 15 şekil [A07-S032] | 0, kendi kod | önceden hesap, render'a yük yok | Birincil |
| uLipSync | MFCC, kalibrasyonlu ünlü tanıma; BakedData, Timeline [A07-S051] | Ünlü profili elle kalibre edilir | BlendShape bileşeni | MIT; Burst + Mathematics [A07-S051] (paket = kullanıcı onayı) | CPU | Yedek |
| Rhubarb | Ses → 6–9 ağız şekli; İngilizce dışı "phonetic" tanıyıcı daha az hassas [A07-S052] | Dil bağımsız mod | eşleme gerekir; 2D'ye uygun | lisans dosyası okunmadı; son sürüm 1.14.0 [A07-S053] | CPU | 2D yedek |
| SALSA Suite | Ücretli Unity varlığı [A07-S054] | - | uyarlanır | 45 $/koltuk; son güncelleme Oca 2025; Unity 6 bilgisi sayfada yok [A07-S054] | CPU | Gerek yok (ÇIKARIM) |
| NVIDIA Audio2Face-3D | Sinir ağı: çene, dil, göz, deri [A07-S055] | Dil listesi yok | Eşleme işi; resmi Unity eklentisi duyurulmadı [A07-S056] | NVIDIA Open Model License, ticari "hazır" [A07-S055] | **NVIDIA GPU** [A07-S055] | Bu bilgisayarda yok; gelecek |
| NeuroSync | Ses → 68 katsayı | Yalnız İngilizce etiketli | ARKit | yıllık gelir <1M $ MIT, üstü ticari [A07-S057] | belirtilmemiş | Deneysel |
| Mevcut (genlik) | RMS → jawOpen + rastgele ünlü | - | yalnız çene (bulgu) | 0 | - | Yetersiz |

Oculus/OVR LipSync'in güncel destek/lisans durumu bu oturumda DOĞRULANAMADI; yeni bağımlılık yapılmamalı.

Türkçe harf → Rocketbox viseme taslağı (ÇIKARIM; [A07-S049] gruplarından uyarlandı, görsel testle ayarlanmalı): a→AA_VI_10_aa; e→11_E; ı,i,y→12_I; o,ö→13_O; u,ü→14_U (ö,ü: ek mouthPucker); b,p,m→01_PP (tam kapanış zorunlu); f,v→02_FF; d,t→04_DD; k,g→05_KK; c,ç,ş,j→06_CH; s,z→07_SS; n,l→08_nn; r→09_RR; h→sonraki ünlü; ğ→önceki ünlüyü uzat; duraklama→00_Sil. Geçişler 40–80 ms yumuşatılır; çene genliği ses zarfından; viseme ses önünde ~1 kare başlayabilir.

Yüz: duygu katmanı (kaş, göz kapağı: ARKit `AK_*`) viseme katmanından ayrı; konuşurken ağız bölgesinde duygu ağırlığı kısılır; mevcut göz kırpma/bakış sistemi korunur (ÇIKARIM).

### S6 — Foley, ambiyans, müzik, efekt kaynakları

| Kaynak | Ticari | Atıf | Uyarı |
|---|---|---|---|
| Sonniss GDC paketleri | Evet [A07-S058] | Hayır | Ham haliyle satış yasak. Üçüncü taraf "AI eğitimi yasak" diyor [A07-S060], okunan lisans sayfasında bu yok (çelişki) |
| Kenney ses | CC0 [A07-S060] | Hayır | İkincil kaynaktan |
| Freesound | CC0 evet; BY evet; BY-NC **hayır** [A07-S059] | BY'de evet | Yalnız CC0 tercih et; yükleyen hakkı olmayabilir [A07-S060] |
| Pixabay | Evet [A07-S061] | Hayır | Tek başına yeniden dağıtım yasak; bazı parçalarda Content ID talebi riski [A07-S060] |
| Incompetech | CC (atıflı) veya ücretli lisans [A07-S060] | Duruma göre | - |
| TikTok Commercial Music Library | İşletmeler için ücretsiz; TikTok ticari kullanımı [A07-S062] | - | Müziği videoya uygulamada eklemek gerekir; başka platforma taşınmaz varsayılmalı (ÇIKARIM) |
| Lyria 3 (üretim) | koşulları ayrıca okunmalı | - | 30 sn 0,04 $ [A07-S003] |
| ProcAudio (kodla) | Kendi üretimimiz | Hayır | Kalite sınırlı (ÇIKARIM) |

Her ses dosyasına lisans kaydı (kaynak, URL, lisans, tarih, atıf metni) tutulmalı. Varlık indirilmedi.

### S7 — Miksaj ve ses düzeyi
- TikTok resmi LUFS hedefi yayımlamıyor (ikincil kaynak) [A07-S063]; −14 LUFS / −1 dBTP pratik varsayılan [A07-S063]. AES TD1008 konuşma/drama için −18 LUFS önerir [A07-S063]. Yani "−14" platform kuralı değil, tercih.
- Mevcut ölçer BS.1770 tabanlı; kazanç ancak SONRAKİ kayda uygulanıyor, tepe sınırı örnek tepesi (gerçek tepe değil) (Scripts/SesOlcer.cs:37-47,143-151).
- ÇIKARIM öneri: veri yolları Konuşma / Ambiyans / Foley-Efekt / Müzik; replikler ayrı ayrı eşit düzeye; konuşma sırasında müzik −9…−12 dB (atak ~100 ms, bırakma ~400 ms), zamanlar bilindiği için kısma önceden planlanır; kayıttan sonra tek geçişte ölç → gerekli kazancı aynı videonun sesine uygula (ör. ffmpeg `loudnorm` iki geçiş; belgesi bu oturumda okunamadı, DOĞRULANAMADI). Foley: adım/kumaş/nesne sesleri animasyon olaylarından tetiklenir (A03 ile ortak); mekân başına döngülü ambiyans + ses geçişleri kesmeyi 0,2–0,5 sn örter (A08 ile ortak).

### S8 — Offline / online / gelecek
- Online (öneri): yüksek duygu kalitesi için TTS. Offline: hizalama, viseme, miks, döküm denetimi (hepsi CPU'da mümkün görünüyor; ölçülmedi).
- Gelecek (1–3 yıl, ÇIKARIM): açık Türkçe TTS hızla artıyor (2026'da birçok yeni model [A07-S019][A07-S022]); zaman damgalı TTS yaygınlaşıyor; ses→yüz modelleri açılıyor (Audio2Face açık kaynak, Eyl 2025 [A07-S056]). Mimari: `ITtsSaglayici`, `IHizalayici`, `IDudakSurucu` arayüzleri + ortak zaman dosyası; sağlayıcı değişse sahne kodu değişmez. Fiyat ve model kapanışları hızlı değişiyor (Gemini 2027 zammı, OpenAI kapanışı) → tek sağlayıcıya bağlanma.

## Teknoloji kayıtları
Biçim: ne yapar/nasıl · katkı · Unity uyumu · donanım · maliyet · lisans · +/− · alternatif · kaynak · test · doğrulama 2026-11-27 · güven.
1. **Gemini 3.8 Flash TTS** (bulut TTS): metin+style→24 kHz WAV · duygulu Türkçe replik · mevcut entegrasyon · bulut · ≈0,000225 $/sn (2027'de 2×) · API koşulları, SynthID · + ucuz, hazır / − zaman damgası belgede yok, preview, rastlantısallık · ElevenLabs, Azure · A07-S001,A07-S002,A07-S006 · test edilmedi (A07) · güven yüksek (fiyat/özellik), Türkçe kalite bilinmiyor.
2. **MFA 3.0 Türkçe** (hizalama): ses+metin→kelime/fonem zamanı (GMM-HMM) · altyazı + viseme tek kaynaktan · harici süreç, JSON/TextGrid içe aktarımı · CPU · 0 · model CC BY 4.0 (yazılım lisansı okunmadı) · + fonem / − kurulum, sözlük dışı sözcük · parseh, whisper · A07-S008,A07-S009 · test edilmedi · orta.
3. **parseh/aligner-tr** (ONNX CTC): bilinen metni hizalar · hafif hizalayıcı · harici süreç; ONNX'i Unity içinde çalıştırmak ayrı iş · CPU, ~2,9 GB RAM · 0 · CC-BY-4.0 · + küçük / − doğruluk ölçülmemiş · MFA · A07-S010 · test edilmedi · düşük.
4. **faster-whisper / whisper.cpp** (ASR): döküm + kelime zamanı · içerik denetim kapısı · harici exe (whisper.unity paketi de var) · CPU · 0 · MIT (whisper.cpp) · + kolay / − hizalamada daha kaba · Gemini Transcribe · A07-S011,A07-S040,A07-S043 · test edilmedi · orta.
5. **ElevenLabs with-timestamps**: sentez + karakter zamanı · en az iş ile kesin senkron adayı · REST · bulut · 6 $/ay'dan · ücretli planda ticari · + hizalama hazır / − kredi, bağımlılık · Azure · A07-S012…A07-S014 · test edilmedi (kredi yasak) · orta.
6. **Rocketbox viseme + kendi viseme sürücüsü**: zaman dosyasından şekil ağırlığı · gerçekçi dudak · tam uyumlu (kod) · ihmal edilebilir · 0 · MIT [A07-S032] · + paket yok / − eşleme ayarı · uLipSync · A07-S031,A07-S032,A07-S049 · test edilmedi · orta-yüksek.
7. **uLipSync**: MFCC ünlü tanıma · yedek dudak · Unity eklentisi (Burst, Mathematics) · CPU · 0 · MIT · A07-S051 · test edilmedi; Unity 6000.6 uyumu doğrulanmadı · orta.
8. **Audio2Face-3D**: ses→yüz hareketi · üst kalite dudak/yüz · dönüştürme gerekir · NVIDIA GPU · model 0 · NVIDIA Open Model License · A07-S055,A07-S056 · test edilemez (donanım) · orta.

## Karşılaştırma ve çelişkiler
- Piper dfki: model kartı veri lisansı CC BY-NC-SA [A07-S021]; akademik tarama "MIT" listeliyor [A07-S023]. Bağlayıcı olan kısıtlayıcı veri lisansı varsayılmalı → ticari kullanma.
- Gemini API fiyat sayfasının okunan kopyası eski (2026-06-02, 3.8 TTS yok) [A07-S003]; Cloud TTS sayfası 3.8'i listeliyor [A07-S002].
- Sonniss AI eğitim yasağı: [A07-S060] var diyor, [A07-S058] okumasında yok.
- WhisperX popüler ama İngilizce ölçümde en kaba hizalayıcı [A07-S009]; Türkçe için aynı sıralama garanti değil.
- Aşama16 belgesi "ticari kullanıma açık Türkçe ses olmadan kullanılmamalı" diyordu; 2026 sonunda Apache/MIT adaylar var (Chatterbox, VoxCPM2, EMA, FreyaTTS) [A07-S017…A07-S019], fakat kalite ve veri hakları doğrulanmadı.

## Riskler
| Risk | Etki | Önlem |
|---|---|---|
| TTS satırda söz atlar/ekler | yanlış altyazı | döküm denetim kapısı |
| Ücretsiz kota yetmez | toplu bölmeye dönüş | satır başı üretim + birkaç güne yay veya ödeme (kullanıcı kararı) |
| Hizalayıcı Türkçede zayıf | senkron hatası | 20 satırlık altın set ile seçim |
| Sağlayıcı fiyat/model değişimi | maliyet, kesinti | sağlayıcı arayüzü, ses önbelleği |
| NC lisanslı model/ses sızması | hukuki | lisans manifestosu, A12 denetimi |
| Ücretsiz API'de senaryo insan incelemesine açık | gizlilik | kişisel veri yazma / paralı katman |
| Etiketsiz AI ses | TikTok yaptırımı | yayın notuna etiket uyarısı (kodda var: StoryPlayer.cs:837) |

## Önerilen sistem (teslim)

```
Hikâye satırı → [ekran metni | okunan metin + kelime eşlemesi]
 → TTS (satır başına; sağlayıcı değiştirilebilir)
 → Son işleme BİR KEZ (kırpma, satır düzeyi) → kalıcı WAV
 → Denetim kapısı: süre alt+üst sınır, döküm-metin farkı, kullanıcı dinle/onayla
 → Forced alignment (son WAV üzerinde) → NN yerine <anahtar>.json
 → Altyazı sayfaları + viseme izi + ağız/duygu + müzik kısma (hepsi aynı JSON'dan)
 → Zaman çizelgesi → kayıt → tek geçiş ses düzeyi → A/V ölçümü → rapor
```
Zaman dosyası alanları: wav özeti (hash), metin özeti, sağlayıcı/model/ses/style, süre, kelimeler [ekran kelimesi, okunan kelimeler, başla, bitir, güven], fonemler, döküm sonucu, onay durumu.

Kurallar:
1. Tahminle bölme KALDIRILSIN (TopluSes.Bol). Toplu istek zorunluysa: kesim yalnız hizalama zamanından, her parça döküm denetiminden; geçemeyen satır tek tek üretilir.
2. `NN.wav` yedeği kaldırılsın ya da özet doğrulaması olmadan kabul edilmesin.
3. `kaydet` yalnız ses + zaman dosyası + özet eşleşmesi + onay varsa çalışsın.
4. Çalışma anında ses değiştirilmesin (`ProcAudio.Clean` içe aktarmaya taşınsın).
5. Güveni düşük hizalamada karaoke kapanır, satır altyazısı gösterilir.

Aşamalı yol (maliyet):
- **A (0 $):** K2/K5/K6 düzeltmeleri; satır başı üretim; klasik/öbek altyazı; mevcut sesleri yerel dökümle denetle.
- **B (0 $, kurulum onayı):** yerel hizalayıcı (MFA veya parseh; ölçümle seç) → gerçek karaoke + viseme.
- **C (film başına <1 $):** Gemini paralı katman (satır başı + yeniden deneme).
- **D (6–22 $/ay, isteğe bağlı):** ana karakterlerde ElevenLabs (zaman damgalı); yalnız dinleme karşılaştırması üstün çıkarsa.
- **E (gelecek, NVIDIA GPU alınırsa):** Audio2Face-3D, yerel VoxCPM2/Chatterbox.

## Test önerileri
Bu bilgisayarda (GPU'suz; kredi harcamadan olanlar önce):
- T1 Teşhis: `ucuncu_kisi` 68 `NN.wav` dosyasını yerel Whisper ile döküp metinle karşılaştır; kaç satır uyumsuz? (K1/K2 doğrulaması.)
- T2 Altın set: 20 satırda elle kelime sınırı işaretle; MFA, parseh, faster-whisper, mevcut karakter-orantı yöntemini medyan/%95 ms ile karşılaştır.
- T3 A/V ofseti: `FfmpegPipeRecorder`'daki klaket (flaş+bip; satır 22, 154, 282) ile ve Unity Recorder çıktısında altyazı-ses farkını kare kare ölç.
- T4 Rocketbox şekil adları listesi ve viseme A/B (genlik vs viseme), kullanıcı izlesin.
- T5 Aynı 5 replik: Gemini vs (izinle) ElevenLabs/Azure kör dinleme. Kredi gerektirir; kullanıcı onayı.
- T6 Ses düzeyi: LUFS ve tepe, tek geçiş düzeltme sonrası.
GPU isteyen: Audio2Face-3D, VoxCPM2, Chatterbox hız/kalite.
Kabul ölçütü (ÇIKARIM): içerik uyumsuz satır = 0; kelime başlangıcı medyan ≤50 ms, %95 ≤120 ms; −14±1 LUFS, tepe ≤−1 dB; önce 30–60 sn örnek kullanıcı onayı.

## Öz denetim
- Hiçbir ses dinlenmedi, üretilmedi; hiçbir test koşulmadı; kod çalıştırılmadı. Kod bulguları statik okuma.
- Web sayfaları WebFetch özet aracıyla okundu; ham metin satır satır doğrulanmadı.
- WebSearch hakkı (oturum genelinde 200) doldu; araştırılamayanlar: ffmpeg loudnorm belgesi, Netflix altyazı zamanlama kılavuzu, kelime-kelime altyazının okunurluk araştırmaları, OVR LipSync durumu. Devam araması gerekirse yeni mesajla yapılmalı.
- Doğrulanamadı: Türkçe TTS kalite karşılaştırması (bağımsız dinleme testi yok), Türkçe hizalama ms doğruluğu, Gemini ücretsiz güncel kota, Azure tr-TR viseme/stil, Azure/OpenAI/Polly güncel fiyat ve ticari metin, Rhubarb lisansı.
- Gerçek kullanıcı deneyimi (üçlü kaynak) yalnız Gemini için var.

## Eksik alanlar / açık sorular
1. Kullanıcı ücretli API (Gemini) açar mı? Video ticari mi olacak? (DURUM'daki "ticari değil" kararı güncel mi?)
2. Harici Python/conda aracı kurulmasına izin var mı (MFA)? EXE dağıtımında hizalayıcı nasıl paketlenecek?
3. Reddedilen filmde gerçekten hangi satırlar bozuk (T1)?
4. Kayıt modunda `AudioSource.timeSamples` kare ile uyumlu ilerliyor mu?

## Diğer ajanlardan beklenen / ortak konular
- A02: Rocketbox facial şekil adları, diş/dil görünürlüğü (DURUM: ağız açıkken diş görünmüyor), VRoid viseme eşlemesi.
- A03: konuşma jestleri ve ayak sesi olayları; replik zamanına bağlı jest.
- A09: Timeline/kayıt saati, AudioRenderer, ses son işleme.
- A08/A01: replik zamanlarından kesme; J/L-cut için ses ve görüntü zaman çizelgesi ayrı olmalı (bugün `Say` sırayla bekliyor).
- A10: MFA/whisper CPU süresi, disk (45 GB boş).
- A12: TTS, model, ses dosyası lisans denetimi.

## Öneriler (kanıtla)
1. Önce teşhis ve kapı (T1, K1, K2, K5): maliyetsiz, reddin doğrudan nedeni.
2. Senkronu tahminden ölçüme geçir: forced alignment [A07-S008…A07-S010].
3. Satır başı üretim; maliyet çok düşük [A07-S002].
4. Viseme'leri kullan: modellerde hazır [A07-S032], kod kullanmıyor.
5. Lisans temiz varlık: CC0/Sonniss, NC yasak [A07-S058…A07-S060].
6. Yayında AI etiketi [A07-S027].
