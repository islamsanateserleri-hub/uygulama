# HEDEF VİDEO TARZI VE KALİTE NOTLARI (26 Kas 2026)

**Her yeni video işinden ÖNCE oku.** Kullanıcı: "Bu tarz videolar yapacağız; daha iyi kurgulu, daha kaliteli."

## Örnek video (kullanıcının gönderdiği, TikTok @diziko2, "AI-generated" etiketli)
576x1024, 30 fps, 127 sn (sonu TikTok kapanışı), ses -25 LUFS. İncelenen: kareler, kesme anları, ses düzeyi. SES/MÜZİK DİNLENEMEDİ.

### Neden iyi
- **Kanca ilk karede:** parlayan altın gözlü aşırı yakın plan; ilk cümle merak/aşağılama: "Owen bir ezikti."
- **Hikâye kalıbı:** ezilen kahraman → gizli güç patlar → herkes şok → onu reddeden aile → daha büyük düşman → savaş → devamı merak.
- **Kurgu:** ~40 kesme / 117 sn, plan ortalama ~3 sn (0,7–6 sn). Aynı kadraj uzun süre kalmaz.
- **Plan çeşidi:** aşırı yakın, arkadan, boydan, çok geniş (şato+meydan+ejderha), kalabalık, yukarıdan, nesne detayı (çatlayan fincan), gökyüzü, savaş.
- **Tepki planları:** her büyük olaydan sonra şaşkın/öfkeli yüzler. Duyguyu bu taşıyor.
- **Hareket:** ejderha uçuşu, diz çöken kalabalık, savaş; boş duran konuşan insan yok.
- **Görsel dil:** sinematik gerçekçi ışık, soğuk gri dünya + kahramanda sıcak altın (göz hep ona gider).
- **Altyazı:** küçük-orta beyaz, 1–2 satır, alt-orta, kısa parçalar, konuşmayla birlikte değişiyor; renkli karaoke yok.

## Dürüst sınır
Bu video yapay zekâ GÖRÜNTÜ/VİDEO üreticileriyle yapılmış. Hikâye Stüdyosu'nun Unity 3D karakterleri bu görünüme ULAŞAMAZ; küçük ayarla kapanacak bir fark değil.

## Gidilecek yol (genel bilgi; araç ve fiyatlar henüz doğrulanmadı)
1. Özgün hikâye (örnekteki karakter/sahne/metni KOPYALAMA), 60–90 sn ile başla.
2. Sahne listesi: 25–35 plan, her plan 2–4 sn.
3. Her karakter için sabit referans görsel (tutarlılık).
4. Her plan: önce görsel, sonra görselden videoya.
5. Seslendirme CÜMLE CÜMLE ayrı dosya (tahminle bölme YOK); altyazı süresi ses dosyasından.
6. Kurgu: kesmeler, müzik, efekt, altyazı (ffmpeg ile yapılabilir).
7. Video üretimi ücretli kredi ister (bağlı araçlar: ElevenLabs görsel/video; ücretsiz kredi büyük olasılıkla yetmez). ÖNCE fiyatı öğren, kullanıcıya söyle; ödeme kullanıcıya ait.
8. ÖNCE 20–30 sn örnek göster; onaysız tamamını yapma. Dinlemediğin/izlemediğin şeye "hazır" deme.

## Yapılacaklar (limit yenilenince)
- Araç + fiyat araştırması (karakter tutarlılığı, görselden videoya). TikTok/YouTube videoları İZLENEMİYOR; yalnız yazılı kaynak okunabilir ve kullanıcının yüklediği videolar kare kare incelenebilir. Yeni örnek gelirse buraya ekle.

---
# ARAŞTIRMA: BU KALİTE YAPAY ZEKÂ OLMADAN NASIL ÇIKAR (26 Kas 2026, 18:20)
Kullanıcı şartı: videolar UYGULAMAMIZDAN, yapay zekâ üreticisi olmadan, bu kalitede ya da daha iyi.

## Sonuç
- **Bu bilgisayarda MÜMKÜN DEĞİL:** ekran kartı AMD Radeon tümleşik (işlemciye gömülü; Unity kaydında görüldü). UE5 için yayımlanan öneri 8 GB+ ekran belleği, 32 GB RAM; sinematik ışık (Lumen) RTX 2000+/RX 6000+ ister.
- Güçlü donanımla (UE5 + MetaHuman) "oyun sinematiği" kalitesine çıkılır. Örnekteki "gerçek film" görünümünün AYNISI garanti DEĞİL; sahne sahne elle emek ister (tahmin; denenmedi).
- **8K çözüm değil:** örnek video 576x1024. Kaliteyi karakter, ışık, hareket, kurgu belirliyor. 1080x1920 kalsın; 8K yalnız süreyi ve donanım ihtiyacını büyütür.

## Doğrulanan bilgiler
- MetaHuman: yıllık geliri 1 milyon $ altındakilere ücretsiz; "any engine" (Unity dahil). Karakter oluşturma bulut hizmeti kullanıyor; Epic hesabı gerekebilir (sayfalarda açık yazmıyor). Kaynak: metahuman.com/en-US/license, cgchannel.com (Haz 2025).
- Mixamo karakter+animasyon (yürüme, koşma, savaş): Adobe hesabıyla ücretsiz, filmde kullanılabilir. helpx.adobe.com/creative-cloud/faq/mixamo-faq.html
- Poly Haven (gökyüzü/ışık, doku, model): CC0, serbest, atıf yok. polyhaven.com/license
- Fab ücretsiz varlıkları Unreal dışında da kullanılabilir (özet sayfaya göre). fab.com/eula
- Fab'da MetaHuman ortaçağ zırh/kıyafet paketleri görülen fiyat: 350–600 $.
- Donanım (makingport.com, Kas 2026): taban RTX 3060 12GB + 32 GB RAM; önerilen RTX 4070 / RX 7800 XT 16GB + 64 GB RAM.
- Bulut ekran kartı RTX 4090: saati 0,33–0,89 $ (amcompute.com, 22 Kas 2026). Hesap ve ödeme kullanıcıya ait; kurulum denenmedi.

## Yol haritası
**A) Şimdi, bu bilgisayarda, bedava (Unity):** satır satır doğru ses-altyazı; ~3 sn planlar + tepki planları; yürüme/koşma (Mixamo); saçma hareketleri kaldır; Poly Haven ışıkları, sinematik renk; parıltı/duman/ejderha efektleri; sade altyazı. Sonuç bugünkünden belirgin iyi OLMALI ama örnek seviyesinde OLMAYACAK.
**B) Güçlü ekran kartı (satın alma ya da kiralama) + Unreal Engine 5 + MetaHuman:** hedef kaliteye en yakın yol. Uygulamanın sahne tarafı Unreal'e yeniden yazılır (büyük iş, haftalar; tahmin). Hikâye sözdizimi korunabilir.

## Açık sorular (kullanıcı kararı)
Güçlü bilgisayar/ekran kartı alınacak mı, kiralanacak mı, yoksa A ile mi devam?
## İncelenmeyenler
TikTok/YouTube videoları izlenemiyor. Unity HDRP, Blender işleme süreleri, ücretsiz ejderha/zırh seçenekleri ayrıntılı denenmedi; bilgisayarın işlemci/RAM modeli doğrulanmadı.

---
# 2. RAPOR (26 Kas 2026 akşam) — DONANIM KONTROLÜ ve KAPSAMLI ARAŞTIRMA

## 1. Bilgisayar (okunarak doğrulandı: `Hikaye > Donanim Raporu`)
| Parça | Durum |
|---|---|
| İşlemci | AMD Ryzen 5 5600GT (12 iş parçacığı) — İYİ |
| Bellek | 64 GB — ÇOK İYİ |
| Anakart | Gigabyte A520M K V2 — 1 adet PCIe 3.0 x16 yuvası BOŞ görünüyor: ekran kartı takılabilir |
| Ekran kartı | YALNIZ işlemci içi Radeon. Ayrı kart yok. Işın izleme YOK. ("32 GB ekran belleği" paylaşılan RAM'dir) |
| Disk C: | 45 GB boş — AZ (büyük motor ve modeller sığmaz) |

**Sonuç:** Yeni bilgisayar GEREKMİYOR. Ekran kartı + disk alanı yeter. (Önceki "bu bilgisayarda mümkün değil" sözü kartsız hali içindir.)

## 2. Donanım alışverişi
- Ekran kartı: en az RTX 3060 **12 GB**; daha iyisi RTX 5060 Ti 16 GB ya da RTX 4070 sınıfı. NVIDIA: Unity HDRP ışın/yol izleme RTX ister.
- ALMADAN ÖNCE: güç kaynağının wattı (genel bilgi: bu sınıf için ~550–650 W kaliteli) ve kasaya sığma kontrol edilmeli. Bilgisayarcıya sordur.
- SSD: ek 1–2 TB (genel öneri).
- TL fiyatı ÖĞRENİLEMEDİ (fiyat siteleri otomatik okumayı engelledi). Akakçe / itopya / incehesap'tan bak.

## 3. Motor: Unity'de kalıp HDRP'ye geçmek (ÖNERİLEN)
Unity belgesine göre YALNIZ HDRP'de: saç gölgelendirici, ten altı ışık (gerçekçi cilt), hacimli bulut/sis, yol izleme, ışın izleme. Alan derinliği ve VFX Graph ikisinde de var. Projemiz şu an URP.
(Bir kısmı belgenin özetinden; geçmeden doğrula.) Seçenek 2: Unreal Engine 5 — görüntü kolay yükselir ama uygulama baştan yazılır.

## 4. Gereken kod işleri (sıra ile)
| # | İş | Nerede | Kart gerekir mi |
|---|---|---|---|
| 1 | Satır satır ses; altyazı süresi ses dosyasından; kayıt öncesi eşleşme denetimi | GeminiSes, StoryPlayer, KaraokeAltyazi | Hayır |
| 2 | Plan yöneticisi: ~3 sn'de kesme, aşırı yakın/geniş/tepki/üstten/nesne planı | Yonetmen3D, yeni PlanListesi; komut `plan:` | Hayır |
| 3 | Yürüme, koşma, hedefe gitme, dövüş; abartılı yerinde hareketleri kapatma | Sahne3D, Mixamo hareketleri | Hayır |
| 4 | Kalabalık (kopyalanan figüran), diz çökme, kaçışma | yeni Kalabalik | Hayır (az sayıda) |
| 5 | Parlayan damar/göz, ateş, duman, yıkım efektleri | emissive doku, VFX Graph/parçacık | Kısmen |
| 6 | Gerçek ejderha modeli (iskeletli) + uçuş yolu; koddaki ejderha yerine | Yaratik, NesneKatalogu | Kısmen |
| 7 | HDRP geçişi: malzemeler, ışık, cilt, saç, sis, gölgelendiriciler | tüm sahne kodu | EVET |
| 8 | Yeni karakterler (MetaHuman ya da Character Creator), yüz ifadesi ve dudak eşleme | Karakter3DKurulum | EVET |
| 9 | Yüksek kalite çevrim dışı kayıt (örnek sayısı, süper örnekleme) | StudyoKayit | EVET |
| 10 | Müzik, ses efekti karışımı, ses düzeyi, renk | ffmpeg adımı | Hayır |
Büyüklük: tahminen haftalar sürer; hiçbiri henüz yapılmadı.

## 5. Yardımcı uygulamalar
| Uygulama | İşe yarar | Ücret / koşul |
|---|---|---|
| Unity HDRP, Cinemachine, Timeline, VFX Graph | görüntü, kamera, efekt | Unity paketleri (proje kuralı: eklemek için KULLANICI ONAYI) |
| MetaHuman Creator | gerçekçi insan | gelir 1M$ altı ücretsiz; Unity'de kullanılabilir |
| Character Creator / iClone | gerçekçi karakter + animasyon | CC aylık ~8,30$, iClone ~16,90$ (yıllık, Oca 2026) |
| iClone Personal | animasyon, RTX render | ücretsiz; yalnız kişisel/sosyal paylaşım; dışarı 30 aktarım sınırı |
| Mixamo | yürüme, koşma, dövüş | Adobe hesabıyla ücretsiz |
| Poly Haven | ışık, doku, model | CC0, serbest |
| Fab | zırh, şato, ejderha | ücretsiz + ücretli (zırh paketleri 350–600$) |
| Cascadeur | elle animasyon | ücretsiz sürüm FBX vermiyor → işimize yaramaz |
| Blender | model düzeltme, ayrı render | ücretsiz (genel bilgi) |
| DaVinci Resolve | kurgu, renk | ücretsiz sürümü var (genel bilgi) |

## 6. Plan
1. KART YOKKEN (hemen yapılabilir): işler 1, 2, 3, 4, 10 + parıltı efekti → 30 sn örnek → onay.
2. KART + SSD gelince: 7, 8, 9, 6 → 30 sn örnek → onay → tam film.
Kullanıcı yapacak: kart/SSD alımı, Epic/Adobe/Reallusion hesapları, ödemeler, paket onayı.
**Dürüst sınır:** örnekteki "gerçek insan filmi" görünümünün birebir aynısı garanti değil; hedef iyi bir oyun sinematiği.
