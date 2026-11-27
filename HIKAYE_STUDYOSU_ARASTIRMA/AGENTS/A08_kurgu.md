# A08 — Sinematik Kurgu ve Hikâye Anlatımı Uzmanı

> Tarih: 2026-11-27. Kaynaklar: `SOURCES/A08_kaynaklar.md` (metinde `[S001]` = A08-S001). 26 WebSearch.
> Kod yazılmadı/değiştirilmedi; yalnız okundu. Unity çalıştırılmadı, video izlenmedi, hiçbir test yapılmadı.
> "ÇIKARIM:" = benim yorumum ya da önerim. "KOD:" = kaynak kod okuması. Mimarideki bütün sayılar başlangıç değeridir, denenmedi.

## 0. En önemli sonuçlar
1. **Plan süresi için ölçüm bulundu** (A01'in "DOĞRULANAMADI"sı kısmen kapanır): 10 ReelShort dizisinin ilk bölümlerinde ortalama plan **2,31 sn** [S001]; 300 mikro drama klibinde dakikada 15,06 kesme (≈4 sn/plan) [S002]. İki ölçüm farklı; güvenli hedef bandı **medyan 2,5–3 sn, ortalama 2,3–4 sn** (ÇIKARIM).
2. **Bugün kurgu diye bir katman yok.** KOD: `StoryPlayer.Run` öğeleri sırayla işler; `Say` ses bitene kadar bekler, sonra sabit 0,22 sn. Görüntü ve ses aynı anda kesilmek zorunda; J/L kesme, kurgu sonrası düzenleme ve "bütün sahneye bakarak karar" mümkün değil.
3. **Açılış kancayı boşa harcıyor.** KOD: 0,35 sn siyah + `Hook` siyah ekranda 1,7 sn yazı. İlk yaklaşık 2 sn'de yüz/görüntü yok. Hedef örnek ilk karede aşırı yakın planla açıyor (`VideoKalitesi.md`).
4. **Öneri:** metin → vuruş listesi → **önce ses zaman çizelgesi, sonra görüntü kurgusu** → düzenlenebilir, kilitlenebilir **kurgu dosyası** → oynatıcı yalnız uygular → otomatik denetim.
5. **İzlenme garantisi veren kanıt yok.** Cliffhanger uyarılmayı artırdı ama devam niyetini artırmadı [S008]. En güvenilir ölçüm, kullanıcının kendi TikTok analitiği [S025].

## Görev tanımı
Protokol §2 "ALT AJAN 8", §9.9: storytelling, storyboard, sahne/plan planlama, plan süresi, süreklilik, tepki, diyalog ve aksiyon kurgusu, montaj, tempo, geçiş, anlatı kamerası, otomatik kurgu. Teslim: otomatik sinematik yönetmen ve kurgu sistemi mimarisi.

## Araştırma alanı ve soruları
S1 kısa dikey drama yapısı · S2 kanca, gerilim, cliffhanger · S3 plan süresi · S4 süreklilik ve kesme algısı · S5 tepki planı ve duyguya göre kamera · S6 diyalog kurgusu, J/L · S7 aksiyon, montaj, geçiş · S8 tempo, müzik, ses · S9 otomatik kurgu · S10 izlenme tutma verileri · S11 mevcut kod.

---

## Bulgular

### S1 — Kısa dikey drama yapısı
- Biçim: 9:16, bölümler 1–3 dk; veri kümesinde ortalama 100 sn [S002].
- Sektör kalıbı (60–90 sn bölüm): **kanca** (en yüksek yoğunlukla aç) → **sürtünme** (görünür çatışma) → **sıçrama** (önceki sahneleri yeniden okutan dönüş) → **buton** (soru üzerinde, "güvenli hissettiğinden biraz erken" kes) [S003] (C; ölçüm yok).
- Diyalog: her replik çatışmayı ilerletir ya da karakteri açar; selamlaşma ve açıklama atılır; kısa replik [S003] (C).
- ReelShort ilk bölümlerinde orta plan baskın, kurucu (mekân tanıtan) plan az [S001].
- Proje içi gözlem (`VideoKalitesi.md`): ezilen kahraman → gizli güç → herkes şok → daha büyük tehdit → merak.

### S2 — Kanca, gerilim, cliffhanger
- Yapısal duygu kuramı: izleyiciye tehdidi önceden bildirip sonucu geciktirmek (gerilim) en çok beğenilen sürümdü; tehditsiz sürüm en düşük; **çözümü atılan sürüm "hikâye" sayılmadı** [S019] (C: ikincil özet, yazılı metin).
- N=133 laboratuvar deneyi: cliffhanger uyarılmayı artırdı; **keyfi ve izlemeye devam niyetini artırmadı** [S008]. Uzun TV dizisi; TikTok'a doğrudan taşınamaz.
- ÇIKARIM: her bölüm (a) küçük bir soruyu **kapatmalı**, (b) daha büyük bir soru açmalı, (c) sonraki bölüm o soruyu ilk saniyelerde ele almalı. Yalnız "yarıda kesmek" yeterli değil.

### S3 — Plan süresi (ölçüme dayalı)
| Kaynak | Örneklem | Değer | Güven / sınır |
|---|---|---|---|
| Aktaş 2026 [S001] | 10 ReelShort dizisi, 1. bölümler, otomatik kesme tespiti | ASL **2,31 sn** | A; yalnız özet okundu |
| M-Drama [S002] | 300 klip (≤2 dk) | 15,06 kesme/dk → ≈**4,0 sn** (hesap) | B; ön baskı, yöntem okunmadı |
| Örnek video | 1 video | ≈40 kesme / 117 sn ≈ 2,9 sn; 0,7–6 sn | proje içi gözlem |
| Sinema [S009] | 295 film; alt örnek 24 film | ASL ≈10 sn (1950) → ≈4 sn (2010); alt örnekte ort. 5,5 / medyan 3,1 / mod 1,5 | A; yatay uzun metraj |
- Süreler sağa çarpık: çok sayıda kısa, az sayıda uzun plan [S009]. Galvane modeli log-normal dağılım ve kullanıcının seçtiği ASL kullanıyor [S023].
- Plan süreleri tekdüze değil; zaman içinde dalgalanma yapısı gösteriyor [S009] (dikkatle ilişkisi korelasyonel). ÇIKARIM: "her 3 sn'de bir kes" metronomu yanlış; kümelenen hızlı kesmeler + duruş.
- Kesme hızı artınca uyarılma ve hatırlama arttı [S015] (yalnız özet; hangi hızdan sonra bozulduğu bilinmiyor).

### S4 — Süreklilik ve kesme algısı
- Kaçırılan (fark edilmeyen) kesme: sahne içi %25, **hareket üstünde kesme %32**, sahne arası %9 [S010] (7 kişi; küçük).
- Süreklilik korunan kurgu daha uyarıcı ve daha uzun algılandı; yazarlar kesmenin hemen ardından gösterilen bilginin kodlanmasının bozulabileceğini öne sürüyor [S016]. ÇIKARIM: kritik görsel bilgi (mektuptaki yazı, göz parlaması) kesmeden en az ~0,2–0,3 sn sonra başlasın.
- 180°, 30°, bakış, ekran konumu kuralları: A01 §S4; Galvane aynı kuralları sayısal geçiş maliyetine çeviriyor (sıçrama, ekran konumu, hareket yönü, bakış yönü, sol-sağ sırası) [S023].
- ÇIKARIM: metin kaynaklı üretimde ayrıca **durum sürekliliği** gerekir: kim nerede, ayakta/oturuyor, elinde ne, kapı açık mı, kıyafet, ışık/saat, duygu. Kurgu her planın başında bu durumu bilmeli.

### S5 — Tepki planı ve duyguya göre kamera
- Üzgün ifadeli yakın plan, izleyicinin karakterin zihin durumundan söz etmesini artırdı; **nötr yakın plan artırmadı** [S018].
- Kuleshov etkisi sanıldığı kadar güçlü olmayabilir: deneyimli izleyicide yalnız üzüntü koşulunda açık görüldü [S017] (N=40).
- Sonuç (ÇIKARIM): tepki planı **gerçek bir yüz/beden tepkisi** gerektirir. Karakter yalnız duruyorsa kurgu "anlam kendiliğinden oluşur" diye güvenmemeli. A02/A03 için bağımlılık.
- Yaygın kalıp: olay → tepki; yakın planlar en duygulu ana saklanır (A01-S028; Leake "intensify emotion" [S011]).

### S6 — Diyalog kurgusu, J/L, boşluklar
- Doğal konuşmada soru–yanıt arası: ortalama +208 ms, medyan +100 ms, mod 0 (10 dil, Türkçe yok) [S021]. Kurmacada duraklama duygu için bilinçli uzatılır (ÇIKARIM).
- KOD: her satır arasına sabit 0,22 sn; buna TTS dosyasının baş/son sessizliği eklenir (miktarı ölçülmedi).
- Diyalog kurgu deyimleri formelleştirilebilir (geniş aç, konuşanı göster, duyguyu yoğunlaştır, karakteri vurgula); HMM ile 2 dk altı sahne 2–3 sn'de kurgulanıyor [S011].
- UIST 2026: ritim düzenlemesi, J/L kesme ve tepki planı ekleme ayrı "mikro düzenlemeler"; öneri: **aday aramayı otomatikleştir, zamanlama kararını kullanıcıya bırak**, güvenli pencereyi (dudak uyumu) hesapla [S012].
- J/L için kaynaklardan güvenilir süre istatistiği **bulunamadı**; A01'in 0,3–1,0 sn değeri ve benimkiler ÇIKARIM.

### S7 — Aksiyon, montaj, geçişler
- Aksiyon kuralları A01 §S7'de (coğrafya, hareket tamamlansın, darbe sonrası tepki). Ek: hareket üstünde kesme kesmeyi gizlemeye en çok yardım eden durum [S010] → kesme noktası animasyon olayından (adım, darbe, dönüş) seçilsin.
- Geçişlerin anlamı (dissolve = zaman geçişi, karartma = bölüm, beyaz flaş = darbe/anı) için **deneysel kaynak bulunamadı** (DOĞRULANAMADI); yaygın film uzlaşımı olarak kullanılabilir (ÇIKARIM). Varsayılan sert kesme.
- Montaj (zaman sıkıştırma): replik yerine 0,5–1,5 sn'lik eylem planları + müzik; vuruşa kesme yalnız burada (ÇIKARIM; [S014]).

### S8 — Tempo, müzik, ses
- Kesmeler ölçü başındaki vurguya denk gelince hoşnutluk arttı (d≈0,33–0,40), katılımcılar fark etmedi [S014]. Müzik videosu; diyaloglu sahneye genellenemez.
- Aynı sahne farklı müzikle farklı empati ve farklı niyet beklentisi doğurdu [S024]. ÇIKARIM: müzik "dolgu" değil, vuruşun niyetine göre seçilmeli; yanlış müzik anlamı bozar. Bugünkü varsayılan sürekli `pad`.
- Anlatıyı valence–arousal eğrisine çevirip müziği buna bağlayan çalışma var [S022] (kurguyu kapsamıyor).
- Ses köprüsü (ambiyans/efekt kesmeyi aşar): A07 ile ortak; bu oturumda ayrı deneysel kaynak aranmadı.

### S9 — Otomatik kurgu araştırmaları
| Çalışma | Yöntem | Bize yarayan | Sınır |
|---|---|---|---|
| He vd. 1996 (A01-S019) | deyim = durum makinesi | anlaşılır kurallar | yerel karar |
| Galvane vd. 2015 [S023] | atış + geçiş + ritim maliyeti, **semi-Markov DP** (kesme anı ve kamera birlikte) | **çekirdek algoritma**; ASL tek ayar; alt-iyi çözümler seçenek olarak sunulabilir | tek 80 sn sahne; 21 kişi; her terim katkılı; uzman kurgusu daha yüksek puan aldı (fark anlamlı değil) |
| Leake vd. 2017 [S011] | HMM deyimleri | kullanıcıya "kurgu stili" | canlı çekim görüntüsü |
| Micro-Edits 2026 [S012] | güvenli pencere + aday listesi | kullanıcı düzenleme arayüzü modeli | canlı çekim |
| Mind-of-Director 2026 [S013] | Unity'de şablon planlar + çok ajanlı LLM + motorla çarpışma/oklüzyon doğrulaması + Timeline | önerilen yapının Unity'de benzeri; doğrulama döngüsü (çarpışma %2,1 ↔ %13,7) | LLM; fikir başına ~25 dk; küçük kıyas |
| FilmAgent 2025 (A01-S015) | LLM satır başı plan seçimi | — | satır düzeyi kaba |

ÇIKARIM: LLM **zorunlu değil**. Metinde karakter, duygu, hareket, komut zaten yapılı. LLM yalnız isteğe bağlı öneri katmanı (vuruşun dramatik ağırlığını tahmin) olabilir; sonucu kurgu dosyasına yazılır, kullanıcı görür.

### S10 — TikTok izlenme tutma verileri (güven sınıfıyla)
| Bulgu | Güven | Sınır |
|---|---|---|
| Creator Rewards: video >1 dk; ödül etkenlerinden "play duration" = izlenme süresi + bitirme oranı [S005] | A (resmî) | 2024; güncel şart ve Türkiye uygunluğu doğrulanmadı |
| Uzun videoyu sonuna kadar izlemek güçlü öneri sinyali [S006] | A (resmî) | 2020 |
| Önerilen videoların ~%55'i bitirilmedi, çoğu yarıdan önce geçildi [S007] | A | 2020–22 verisi, çoğu 10–35 sn video |
| İçerik önerisi ilk 3 sn'de, kanca ilk 6 sn'de [S004] | A **yalnız reklam için** | organik drama için kanıt değil |
| Yaratıcı analitiği: ortalama izlenme süresi, tamamını izleme %, ilgi kaybı anı [S025] | A | içerik ölçütleri son 7 gün |
| "İlk 3 sn'de %X kaybedilir", "55. sn'de donuk kare", ideal kesme süresi iddiaları | D | **karar dayanağı yapılmadı** |

Not: "61+ sn" (proje kuralı) ile tutma çelişebilir: uzun video bitirilmesi daha zor. ÇIKARIM: 61–75 sn ile başla; dolgu ekleyip süreyi uzatma.

### S11 — Mevcut kod (yalnız okuma)
| # | Gözlem | Yer | Etki |
|---|---|---|---|
| E1 | Sıralı coroutine; önceden zaman çizelgesi yok | `StoryPlayer.Run` | sahne bütününde karar yok (A09 §"çekim listesi" ile aynı bulgu) |
| E2 | `Say` ses boyunca bekler + `lineGap` 0,22 sabit | `StoryPlayer.Say`, `Run` | J/L yok, duygusal duraklama yalnız elle (`sessizlik:`) |
| E3 | Kanca siyah ekranda 1,7 sn | `StoryPlayer.Hook` | ilk karede görüntü yok |
| E4 | Kartlar sabit süreli (Title 2,0/2,6 sn) | `Title` | tempo metne göre ayarlanmıyor |
| E5 | Hareket komutları (git, gir, sarılma) sonraki satırı bekletiyor (`tepki_ver` bekletmez) | `Run` Direct dalı; sözdizimi kılavuzu | olası ölü zaman |
| E6 | Tepki kesmesi: satır ≥3 sn, 3 satırda bir; zamanı sürenin %62'si, kelime sınırı bilmiyor | `KameraYonetmen.SatirSuresi` | kesme kelime ortasına düşebilir |
| E7 | `RitimDenetci` eşiği 5/8 sn; çalışma zamanını **ayrıca taklit ediyor** | `Editor/RitimDenetci.cs` | tepki sayacı, hareket planları, eş planı simüle edilmiyor; rapor gerçek videodan sapabilir |
| E8 | Kamera kararı yalnız duygu + son iki plan | `PlanSec` | hikâye işlevi (kanca, dönüş, buton) bilinmiyor |
| E9 | `Random` kullanımı (ör. `TiltTarget`, kamera tohumu) | `Say`, `Kur` | aynı hikâye birebir aynı çıkmayabilir |

---

## Teslim: Otomatik sinematik yönetmen ve kurgu mimarisi (öneri; denenmedi)

### 1. Boru hattı
```
Hikâye metni (.txt)  ──ayrıştırıcı──▶ StoryItem listesi
  1 Analiz      : sahne → vuruş (beat) listesi + durum sürekliliği
  2 Ses çizelgesi: kırpılmış ses dosyaları + kelime zamanları (A07) + değişken boşluklar
  3 Sahneleme   : oyuncu yeri/hareketi, animasyon olayları (A03), kesin süreler
  4 Aday üretimi: kesme noktaları × plan tarifleri (A01 PlanTarifi)
  5 Çözücü      : sert eleme + semi-Markov dinamik programlama
  6 Katmanlar   : altyazı, müzik, ambiyans, efekt, geçiş, kart
  7 Kurgu dosyası (kurgu.json)  ◀── kullanıcı düzenlemesi / kilitler
  8 Denetçi     : rapor (hata engeller, uyarı gösterir)
  9 Oynatıcı    : dosyayı kare kare uygular (karar vermez) → kayıt (A09)
```
Temel ilke: **karar bir kez, kayıttan önce; oynatıcı yalnız uygular.** Önizleme, denetçi ve kayıt aynı dosyayı okur (E7'deki iki ayrı mantık biter).

### 2. Zaman çizelgesi veri modeli
Hiyerarşi: `Bolum → Sahne → Vurus → Klip`. İzler (paralel, birbirinden bağımsız kesilebilir):

| İz | Klip alanları |
|---|---|
| `diyalog` | satır kimliği, ses dosyası, kırpma baş/son, kelime zamanları, kazanç |
| `goruntu` | plan tarifi (A01 alanları), kaynak/bitiş karesi, kesme nedeni |
| `oyuncu` | hareket, duygu, bakış hedefi, başlangıç/bitiş durumu, olay işaretleri (adım, darbe) |
| `altyazi` | sayfa, kelime zamanları, güvenli alan konumu |
| `muzik` / `ambiyans` / `efekt` | dosya, giriş/çıkış, kısma eğrisi, vuruş işaretleri |
| `kart_gecis` | kanca yazısı, seçim, devam kartı, geçiş türü ve süresi |

Ortak klip alanları: `id` (kalıcı), `bas`, `sure` (30 fps **tam sayı kare**), `capa`, `kaynak` (otomatik / kullanıcı / komut), `kilit`, `gerekce`.

**Çapa** en önemli karar: klip saniyeye değil metne bağlanır (`satir_id + kelime_no + kayma_kare`, ya da `olay_id`). Satır yeniden seslendirilince süre değişir ama kullanıcının "bu kelimede tepki planı" düzenlemesi kaybolmaz. `satir_id` satır numarası olmamalı (araya satır eklenince kayar).

`Vurus` alanları: `islev` (kanca, kurulum, catisma, donus, doruk, tepki, nefes, buton), `katilimcilar`, `odak` (kimin deneyimi), `duygu`, `yogunluk` 0–1, `yeni_bilgi` (var/yok), `onemli_nesne`, `hareket`, `en_az_sure`.
`Durum` (vuruş başı anlık görüntü): oyuncu konumu/pozu/baktığı yön/elindeki, nesneler, kapı, ışık/saat/hava, çift başına 180° ekseni.

Biçim: kendi sade JSON'umuz. OpenTimelineIO'nun Track/Clip/Gap/Transition/Marker + `metadata` düzeni [S020] örnek alınabilir ve ileride dışa aktarım hedefi olabilir; bağımlılık gerekmez. Unity Timeline zorunlu değil (A09 ile aynı görüş: önce kendi listemiz).

### 3. Vuruş analizi (metinden, kurallarla)
- `kanca:` ya da ilk replik → `kanca`. `son:` öncesi son 1–2 satır → `buton`.
- `sasirmis/korkmus` + yeni mekân/nesne/yaratık/efekt → `donus`. En yüksek yoğunluk, bölümün %60–85'inde → `doruk` (yoksa uyarı).
- `git/kos/kilic…` → aksiyon; `ic` → iç ses; `sessizlik/bekle` → nefes.
- Yoğunluk = duygu taban değeri + noktalama (`! ? ...`) + vurgu `*…*` + efekt + hareket (ağırlıklar ayarlanacak).
- ÇIKARIM: metinden "dönüş"ü güvenilir anlamak zor; kullanıcı elle işaretleyebilmeli: `vurus: donus`.

### 4. Kural seti
**SERT** (çözücü ihlal edemez; kullanıcı bilerek ezerse kayıtta işaretlenir)
- R1 180° ekseni çift başına; ancak nötr/nesne planı ya da görünen hareketle değişir.
- R2 Aynı özneye ardışık planda ≥30° açı **ya da** ölçek oranı ≥1,5 (A01 ile aynı).
- R3 Yüz/ana olay güvenli alanda ve açık.
- R4 Kesme kelimenin ortasında olmaz (görüntü kesmesi kelime sınırı, duraklama ya da hareket olayına oturur). Ses hiçbir zaman kurgu yüzünden kesilmez (`son: sert` hariç).
- R5 İlk kare: yüz ya da hareket içeren görüntü; kanca yazısı görüntünün **üstüne**. İlk ses ≤0,3 sn.
- R6 Durum sürekliliği: oturan kişi kesmede ayakta görünmez, eşya el değiştirmez.
- R7 Plan ≥0,6 sn (flaş montaj hariç).

**YUMUŞAK** (maliyet ağırlıkları)
- R8 Süre: log-normal, hedef medyan 2,7 sn (ASL ayarı 2,3–4) [S001][S002][S023]. 6 sn üstü uyarı, 8 sn üstü yalnız plan içinde belirgin hareket varsa. (A01 "en çok 6" der; ben yumuşak sınır öneriyorum.)
- R9 Konuşan çoğunlukla görünür; konuşan görünmeden geçen süre sınırlı (yeni bilgi veren repliğin ilk kelimeleri konuşanda).
- R10 Ölçek ↔ yoğunluk: en yakın plan doruğa saklanır; art arda 2'den fazla CU/ECU cezalı.
- R11 Dönüş/büyük olaydan sonra tepki (yalnız o karakter için gerçek tepki animasyonu varsa) [S018].
- R12 Hareket üstünde kes [S010]; yön ve bakış sürekliliği [S023].
- R13 Çeşitlilik: son 3 planla aynı (özne, ölçek, açı) tekrarı cezalı.
- R14 Coğrafya: yeni mekânda ilk 2–4 plan içinde bir kez kim nerede gösterilir (kanca varsa ilk plan olmak zorunda değil; A01 Ç5 ile aynı).
- R15 Kritik görsel bilgi kesmeden ≥8 kare sonra [S016 çıkarımı].

**ZAMANLAMA (ses çizelgesi)** — tümü ÇIKARIM, [S021] yalnız doğal konuşma tabanı
| Durum | Satır arası | J/L |
|---|---|---|
| normal diyalog | 0,10–0,30 sn | L: dinleyene 0,2–0,6 sn önce geç |
| kavga / panik | 0–0,12 sn (üst üste bindirme yalnız kullanıcı isterse) | J: 0,1–0,3 |
| açıklama/itiraf öncesi | 0,5–1,2 sn | kesme yok, yüzde kal |
| şok sonrası | 0,6–1,5 sn (tepki planı + ses susar) | sonraki replik J ile 0,3–0,5 |
| sahne değişimi | yeni ambiyans/replik 0,3–0,8 sn önden (J) | — |
| buton | son kelimeden 0,1–0,4 sn sonra kes | müzik de aynı karede biter ya da tek vurguyla sürer |

Ses dosyalarının baş/son sessizliği önce kırpılır; boşluk tek yerden yönetilir.

### 5. Duyguya göre kurgu (A01 deyim tablosunu tamamlar: kamera orada, ritim burada)
| Vuruş/duygu | Plan süresi | Ritim | Ses/müzik |
|---|---|---|---|
| kanca | 0,8–2 sn | hemen aksiyon ortasından | tek net ses; müzik sonra da girebilir |
| kurulum | 2–3 sn | düzenli | ambiyans önde |
| çatışma (kizgin) | 1,5–2,5 sn | hızlanan | düşük nabız, replik arası dar |
| korku/gerilim | uzun duruş → kısa patlama | dalgalı | sessizlik + tek efekt |
| üzüntü | 3–5 sn, yavaş push-in | seyrek kesme | müzik az; duraklama |
| şaşkınlık/dönüş | olay 1–2 sn → tepki 1–2 → ikinci tepki 0,8–1,5 | küme | müzik kesilir, sonra vurgu |
| aksiyon | 0,8–2 sn | hareket üstünde | vuruş işaretine kesme serbest |
| buton | 1,5–3 sn push-in → sert kesme | ani dur | tek vurgu |

**Tempo eğrisi:** her vuruşun yoğunluğundan bölüm eğrisi çıkarılır; yerel ASL = taban ASL × (1,3 − 0,6×yoğunluk). Doruktan sonra en az bir nefes vuruşu. (ÇIKARIM; formül denenmedi.)

### 6. Bölüm yapısı şablonu (61–75 sn; ÇIKARIM, [S003][S004][S019] temelli)
| Zaman | İçerik |
|---|---|
| 0–2 sn | görüntü + çarpıcı eylem/replik |
| 2–6 sn | kim, ne tehlikede (tek cümle/görüntü) |
| 6–40 sn | yükselen sürtünme; ~8–12 sn'de bir yeni bilgi ya da güç değişimi |
| 40–55 sn | dönüş (öncekini yeniden okutan) |
| 55–65 sn | küçük soru kapanır |
| son 3–6 sn | büyük soru açılır, erken kes |

Yaklaşık 10–16 replik. Bu şablon **denetçi uyarısı** olur, zorlama değil. 5 dk'lık tek film yerine 3–4 bağımsız kancalı bölüm; önce 20–30 sn onay örneği (`VideoKalitesi.md` kuralı).

### 7. Çözücü
- Kesme adayları: satır başı/sonu (± J/L kaymaları), kelime duraklamaları, animasyon olayları, efekt anları, müzik vurguları (yalnız montaj/aksiyon).
- Her aralık için 4–10 plan adayı (A01 deyim tablosu); geometri sert eleme.
- Maliyet = atış (görünürlük, ölçek↔önem, odak karakter, kompozisyon) + geçiş (R1–R2, bakış, hareket) + ritim (R8) + anlatı (R9–R11, R14) + çeşitlilik (R13).
- Semi-Markov DP: kesme anı **ve** plan birlikte seçilir [S023]. Sahne sahne çözülür; sahne sınırında durum aktarılır. Aday kesme noktaları seyrek olduğundan CPU yükü küçük beklenir (ÇIKARIM; ölçülmedi).
- Dikkat: çeşitlilik (son 3 plan) saf Markov değildir; durum içine son 1–2 plan eklenmeli ya da ikinci geçişte düzeltilmeli.
- Deterministik: sabit tohum; eşitlikte sabit sıra (E9).
- Stil ayarları (kullanıcıya): `hizli dram` (ASL 2,3), `duygusal` (3,5), `aksiyon` (2,0).

### 8. Kullanıcı düzenlemesi
1. **Storyboard şeridi:** her plan için kare, süre, tür, neden ("dönüş sonrası tepki"). 
2. Tek tıkla: sonraki en iyi 3 aday; ölçek yakın/uzak; kesmeyi ±kelime kaydır; tepki ekle/sil; J/L kaydır; boşluk uzat/kısalt; geçiş türü.
3. **Kilit:** kullanıcının dokunduğu klip kilitlenir; yeniden çözümde sert kısıt olur, yalnız komşular değişir [S012 ilkesi: aramayı otomatikleştir, kararı kullanıcıya bırak].
4. Metin komutları (geriye uyumlu): `cekim: cu Ayse` (A01 önerisi; `cekim`, `vurus`, `kurgu` adları `Scripts/` içinde aramada kullanılmıyor görünüyor; `plan:` dolu), `vurus: donus`, `kurgu: stil duygusal`, var olan `sessizlik:` ve `kamera:`.
5. Öncelik: arayüz kilidi > metin komutu > otomatik. Çakışma raporlanır.
6. Metin değişince: çapası kalan kilitler korunur; çapası silinenler listelenir, sessizce atılmaz.
7. Geri al + "otomatiğe döndür".

### 9. Kalite denetimi (kayıttan önce; kurgu dosyasından, ayrıca simülasyon yok)
| Kod önerisi | Ölçüt | Seviye |
|---|---|---|
| `SES_ESLESME` | her replikte ses ve kelime zamanı var (A07) | hata |
| `KESME_KELIMEDE` | R4 ihlali = 0 | hata |
| `SUREKLILIK` | R1, R2, R6 ihlali = 0 | hata |
| `GUVENLI_ALAN` | yüz/altyazı taşması, yüzü kapalı plan = 0 | hata |
| `ILK_KARE` | R5 | hata |
| `OLU_ZAMAN` | konuşma, anlamlı hareket ve gerekçeli sessizlik yokken >1,5 sn | uyarı |
| `PLAN_UZUN` | >6 sn | uyarı |
| `RITIM` | medyan, ASL, dağılım raporu; tüm süreler neredeyse eşitse "metronom" | uyarı |
| `TEPKI_YOK` / `TEPKI_BOS` | dönüşten sonra tepki yok / tepki planında yüz değişmiyor | uyarı |
| `DONUS_YOK`, `BUTON_YOK`, `BILGI_DURGUN` (>15 sn yeni bilgi yok) | yapı | uyarı |
| `OLCEK_DAGILIMI` | yalnız **rapor**, hedef zorlanmaz | bilgi |

Sayısal denetim "iyi film" demek değildir: son adım, sesli videoyu izleyen kullanıcı onayı. Yayından sonra: ortalama izlenme süresi, tamamlama %, ilgi kaybı anı [S025] kurgu dosyasındaki klip kimliğiyle eşlenip not edilir. Tek videodan sonuç çıkarılmaz.

### 10. Aşamalar
- **K0 (küçük):** açılışta görüntü üstünde kanca; ses sessizliğini kırpma + değişken boşluk; `RitimDenetci` eşikleri.
- **K1:** kurgu dosyası + oynatıcı takipçi (davranış aynı kalacak şekilde; A09 "çekim listesi" ile **tek** yapı).
- **K2:** vuruş analizi + kural tabanlı satır içi kesme, tepki, J/L.
- **K3:** puanlama + DP.
- **K4:** storyboard arayüzü + kilitler. (Kullanıcı kontrolü önemliyse K4'ün basit hali K2 ile birlikte.)
- **K5 (isteğe bağlı):** LLM öneri katmanı, müzik vuruşu.
K1'den önce **T1** yapılmalı: kurgu tek başına reddedilen kusurları (animasyon, senkron, görüntü) çözmez.

---

## A01 ile uyum ve farklar
**Uyumlu:** vuruş → aday → eleme → puan → DP; PlanTarifi; 180° / 30° kuralları; deyim tablosu; `cekim:` komutu; deterministik; LLM zorunlu değil; düzenlenebilir storyboard; açılışta kanca varsa yakın plan.

| Konu | A01 | A08 | Neden |
|---|---|---|---|
| Plan süresi kanıtı | "ölçüm bulunamadı" | 2,31 sn [S001] ve ≈4 sn [S002] | yeni kaynak |
| DP neyi seçer | vuruş başına plan | kesme anı **+** plan (semi-Markov) | J/L ve satır içi kesme için [S023] |
| Ses/görüntü | plan listesinde ses zamanı | **ayrı izler; önce ses çizelgesi** | J/L, duraklama (A07 de ayrı çizelge istiyor) |
| En uzun plan | 6 sn | 6 uyarı, hareket varsa 8 | duygusal duruş |
| Ölçek dağılımı | yakın %40–55, orta %30–40, geniş %10–20 hedef | yalnız rapor | ReelShort'ta **orta plan baskın** [S001] (yüzdesiz; "orta" tanımı bilinmiyor) |
| L-cut | 0,3–1,0 sn | duruma göre 0,2–0,6; açıklamada yok | ikisi de ÇIKARIM; test |
| Uygulama sırası | önce kamera düzeltmeleri | aynı; yanında K0 | çelişmez |
C-006 (geniş plan): S001 "kurucu plan az, orta baskın" diyor → yakın/orta ağırlık, geniş yalnız coğrafya ve gösteri için. A01 geçici kararıyla uyumlu.

## Teknoloji kayıtları
| Ad | Kategori | Ne yapar / nasıl | Katkı | Unity uyumu | Donanım | Maliyet / lisans | Avantaj | Dezavantaj | Alternatif | Kaynak | Test | Tarih / güven |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Semi-Markov DP kurgu | algoritma | atış/geçiş/ritim maliyetiyle en iyi kesme listesi | sahne bütününde karar | saf C# yazılabilir | CPU (ÇIKARIM) | yayımlanmış yöntem; patent denetimi A12 (A01 notu) | deterministik | ağırlık ayarı | durum makinesi, HMM | S023 | yok | 27.11 / A |
| HMM deyim kurgusu | algoritma | deyimleri olasılıkla birleştirir | stil seçimi | saf C# | CPU | yöntem | birleştirilebilir | görüntü etiketi ister | DP | S011 | yok | 27.11 / A |
| Kendi kurgu.json | veri modeli | izler + çapa + kilit | düzenleme, tekrar üretim | tam | yok | yok | sade | araçlar yazılmalı | Unity Timeline, OTIO | — | yok | ÇIKARIM |
| OpenTimelineIO | değişim biçimi | Track/Clip/Gap/Transition | dış kurgu programına aktarım (isteğe bağlı) | yerel Unity desteği araştırılmadı | yok | açık kaynak (lisans bu oturumda okunmadı) | standart | gereksiz katman olabilir | FCPXML/EDL (araştırılmadı) | S020 | yok | A (şema) |
| LLM yönetmen ajanları | AI araç (video üretici DEĞİL) | planı tartışarak seçer, motorla doğrular | öneri kalitesi | örnek Unity'de | bulut | ücretli/kotalı | esnek | yavaş, tekrarlanamaz | kurallar | S013 | yok | B |
| Anlatı→valence/arousal müzik | AI araştırma | müziği gerilim eğrisine bağlar | tempo eğrisi fikri | doğrudan değil | GPU (ÇIKARIM) | lisans incelenmedi | uzun video | 10 kişilik test | elle eğri | S022 | yok | B |

## Karşılaştırma ve çelişkiler
- Ç1 ASL: 2,31 sn [S001] ↔ ≈4 sn [S002]. Olası neden: örneklem (yalnız ilk bölümler), platform, tespit yöntemi (hipotez). Aralık olarak kullan.
- Ç2 Cliffhanger: sektör "vazgeçilmez" [S003] ↔ deneyde devam niyeti artmadı [S008]. Kullan, ama tek başına tutma aracı sayma.
- Ç3 Hızlı kesme: uyarılma/hatırlama artışı [S015] ↔ kesmede kodlama bozulması [S016]. Hız sınırsız iyi değil.
- Ç4 İlk 3/6 sn: reklam kılavuzu [S004]; organik dramaya aktarımı doğrulanmadı.
- Ç5 Süre: ödül için >1 dk [S005] ↔ uzun videoyu bitirtmek zor [S007].

## Riskler
| Risk | Etki | Önlem |
|---|---|---|
| Kurgu kötü animasyon/yüz/senkronu gizleyemez; sık yakın plan büyütür | yüksek | A02/A03/A07 kapıları önce |
| Tepki planı boş yüzle anlamsız | yüksek | `TEPKI_BOS`; ifade yoksa tepki yerine nesne/el planı |
| Kural ağırlıkları "mekanik" kurgu üretir | orta | storyboard + kilit + A/B |
| Metinden vuruş türü yanlış anlaşılır | orta | elle `vurus:`; gerekçe görünür |
| İki ayrı "doğru" (metin ve kurgu dosyası) ayrışır | orta | kalıcı kimlik, çapa, kayıp kilit listesi |
| Sık kesme + yüksek kalite = kesme başına ışık/durum kurulum maliyeti | orta | A09 ölçsün |
| Örnek videonun hikâyesini kopyalama | yüksek | yalnız yapı; özgün metin (`VideoKalitesi.md`) |

## Test önerileri
**Bu bilgisayarda (GPU istemez):**
- T1 Aynı 30 sn sahne, iki sürüm: mevcut ↔ elle yazılmış kurgu (12–14 plan, değişken boşluk, 3 L/J). Sahne farklı `kamera:` ayarlarıyla birkaç kez kaydedilip dışarıda (ffmpeg ya da kurgu programı) kesilebilir; kod gerekmez, ama yalnız mevcut plan ölçekleri denenebilir. Kullanıcı kör seçer. Başarı: kullanıcı yeni sürümü seçer ve nedenini söyler.
- T2 Boşluk: sabit 0,22 ↔ duruma göre (yalnız ses dinleme).
- T3 Açılış: siyah kanca ↔ görüntü üstü kanca.
- T4 Kullanıcının yükleyeceği 3–5 örnek videoda kesme sayımı ve ölçek dağılımı (S001/S002 ile karşılaştır).
- T5 Denetçi ↔ gerçek kayıt: `RitimDenetci` raporu ile videodan sayılan planlar (E7).
- T6 Yayında: aynı bölümün iki açılışı; ortalama izlenme ve tamamlama (küçük örneklemde kesin sonuç yok).
**GPU isteyen:** kurgu için yok; yakın plan kalitesi A01/A04 testleri.

## Öz denetim
- S001 yalnız özet; S002 ilk 100 bin karakter; Lang yalnız özet; Brewer & Lichtenstein ikincil özet.
- J/L süreleri, boşluk tablosu, tempo formülü, bölüm şablonu zamanları, denetçi eşikleri **kanıtlanmış değil**.
- Geçiş türlerinin anlamı ve montaj için deneysel kaynak bulamadım.
- Türk izleyicisine ve Türkçe diyalog ritmine özgü veri yok (S017 Türkiye'de ama farklı konu).
- Klasik kitaplar (Murch, Katz, McKee, Snyder) okunmadı; Murch önceliği A01-S031'den.
- WebFetch özetleri küçük modelle üretiliyor; kritik sayılar denetçi tarafından asıl sayfadan kontrol edilmeli.
- E1–E9 kod okuması; çalışan videoda doğrulanmadı.

## Eksik alanlar / açık sorular
1. Dikey dramalarda ölçek dağılımı yüzdeleri (S001 tam metni).
2. Kısa dikey dramada bölüm içi izlenme kaybı verisi (bulunamadı).
3. Kullanıcı kararı: tam otomatik mi, onaylı storyboard mu? (Öneri: otomatik taslak + onay.)
4. Kullanıcı kararı: bölüm süresi 61–75 sn mi?
5. Türkçe TTS çıktısında baş/son sessizlik ve kelime zamanı doğruluğu (A07 testi).
6. "virtual cinematography" patent durumu (A12).

## Diğer ajanlardan beklenen / ortak konular
- **A07:** kırpılmış ses, kelime/duraklama zamanı (çapa ve R4 buna bağlı); müzik kısma; ambiyans köprüsü; efekt vuruş işaretleri.
- **A09:** kurgu dosyası = "çekim listesi" (tek şema); kare tabanlı saat; sahne durumunu atlayarak kurma; Recorder/ffmpeg tarafında ses izlerinin bağımsız zamanlanması.
- **A03:** süreleri önceden bilinen hareket klipleri ve olay işaretleri; tepki animasyonları.
- **A02:** tepki/yakın plan için okunur yüz ifadesi.
- **A01:** PlanTarifi ve geometri eleme; yukarıdaki fark tablosu ana araştırmacıca bağlanmalı.
- **A05/A06:** nesne detayı ve kalabalık planları için varlık; olay anı zamanlaması.
- **A12:** otomatik kurgu patentleri; müzik lisansları.

## Öneriler (kanıtla)
1. Açılışı görüntüyle yap; kanca yazısı üstte (E3; örnek video; [S004] yalnız reklam).
2. Sabit 0,22 sn yerine kırpılmış ses + duruma bağlı boşluk ([S021]; E2).
3. Kurguyu ayrı veri katmanına al: ses/görüntü ayrı izler, çapa, kilit; A09 çekim listesiyle birleştir (E1, E7; [S012][S020]).
4. Ritim hedefi medyan 2,5–3 sn, dalgalı ([S001][S002][S009]).
5. Kural + semi-Markov DP; LLM yalnız isteğe bağlı ([S011][S023]).
6. Tepki planını yalnız gerçek tepki varken kullan ([S017][S018]).
7. Her bölüm küçük bir soruyu kapatıp büyüğünü açsın ([S008][S019]).
8. Yayından sonra gerçek analitikle ayarla; internet "tutma kuralları"na göre karar verme ([S025]).
