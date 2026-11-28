# R10 — Risk analizi

Tarih: 2026-11-27. Olasılık ve etki değerlendirmeleri **ÇIKARIM**; ölçülmedi. Ana kayıt: `../07_RISK_REGISTER.md` (R-001…R-011). Aşağıda o kayıtlar ve ajan dosyalarındaki başlıca riskler birlikte verilmiştir. Buradaki riskler 07'ye kimlikle taşındı (R-012…R-041).

> **K-020:** kartla ilgili riskler (4, 5) bütçe değişmedikçe konusuz; bilgi için saklandı. Yeni riskler §6'da.

## 1. En ciddi riskler
| # | Risk | Olasılık / Etki | Önlem | Alternatif | Kaynak |
|---|---|---|---|---|---|
| 1 | Beklenti: örnek videodaki fotogerçek görünüme 3D ile ulaşılamaması | yüksek / yüksek | baştan açık söylemek; küçük örneklerle ilerlemek | stilize tutarlı görünüm; gelecek teknolojiler | R-008, A11 |
| 2 | "Hazır" denip izlenmeden teslim (26 Kasım'da yaşandı) | orta / yüksek | her adımda sesli video + kullanıcı onayı | — | R-005 |
| 3 | EXE bu hâliyle dağıtılırsa lisans ihlali (36 Mixamo karakteri, Mixamo animasyonları, FFmpeg ve açık kaynak bildirimleri) | dağıtımda yüksek / yüksek | dağıtma; dağıtım profili | yalnız MIT/CC0 çekirdek | R-009, A12, C-021 |
| 4 | Kart alınır, sorun kodda kalır | yüksek / yüksek | önce düzeltme ve ölçüm | — | A10-R7 |
| 5 | Yetersiz güç kaynağıyla kart takmak | bilinmiyor / çok yüksek | etiketi öğrenmeden alma | bilgisayarcıya danışmak | A10-R1 |
| 6 | Disk dolması, kayıt yarıda kalır | yüksek / yüksek | envanter, ön kontrol, SSD | harici disk | R-004, A10-R4 |

## 2. Teknik, performans ve uyumluluk
| Risk | Olasılık / Etki | Önlem | Kaynak |
|---|---|---|---|
| Tümleşik kart kaliteyi sınırlar | yüksek / yüksek | profiller, ölçüm | R-003 |
| Gölge/HDR açınca kayıt uzar ya da sürücü çöker | orta / orta | kademeli aç; bekçi | A09, A10-R6 |
| HDR açınca 2D/3D bindirme bozulur | orta / orta | iki durumda test | X1 Ç-A |
| EXE'de efekt shader'ları silinir (yaşanıyor) | gözlendi / orta | kalıcı profil; her shader testini EXE ile bitir | A09 K3, X1 U7 |
| Ana oynatıcı sınıfını yeniden düzenlerken mevcut davranış bozulur | yüksek / yüksek | referans hikâye ve kare karşılaştırması | A09 |
| Yeni klip eklemek bulanık ad eşlemesi yüzünden başka yeri bozar | orta / orta | açık hareket kataloğu | A03 |
| Eski satır numaralı sesler bir anda "eksik" olur | yüksek (geçişte) / orta | önce geçiş planı | X2 S4 |
| Ses düzeyinin iki kez işlenmesi | orta / orta | loudnorm gelirse eski kazancı kapat | X2 S8, A00-S008 |
| Yeni karakter aracı bu Unity sürümünde/DX11'de açılmaz | orta-yüksek / orta | kopya projede kabul testi | X1 U3, C-028 |
| Unity 6000.6 uzun destekli değil; Unity 7 değişiklikleri | kesin / orta | planlı 6.7 LTS geçişi | A09 |
| Aynı hikâye her kayıtta farklı | kesin / orta | tohumlu rastgelelik | A09 K8 |
| Karakter listesi hepsini belleğe yükler; yeni ağır karakterle büyür | orta / orta | adla yükleme | A09 K6, X1 U5 |
| VFX Graph tümleşik kartta kararsız | orta / orta | ana yol parçacık sistemi | A06 |
| Yakın plan yüz/ağız kusurlarını büyütür | yüksek / yüksek | yüz testi geçene kadar sınırlı | A01, A02, X2 G3 |
| Gerçekçi karakter + low-poly eşya uyumsuzluğu | yüksek / yüksek | tek stil kuralı, pilot | A05-R1 |
| Neural kare iyileştirme yüzü değiştirir | belirsiz / yüksek | yalnız onayla ve tutarlılık testiyle | A11 |

## 3. Maliyet, süre, sürdürülebilirlik
| Risk | Önlem | Kaynak |
|---|---|---|
| Fiyat artışı, stok, güncel TL fiyatı yok | karar anında yeniden bak | A10-R8 |
| Elle sanat ve ayar emeği hafife alınır | önce tek mekân/tek karakter pilotu | A05-R5 |
| Ücretsiz ses kotası yetmez ya da koşul değişir | sağlayıcı arayüzü, ses saklama | A07 |
| Terk edilen bağımlılık (OVR arşivlendi; bulut avatar servisleri kapandı) | yerel dosya, lisans kopyası | A02-R8, A12-R8 |
| Doğrulamasız indirme ("latest" ffmpeg) | sürüm ve özet sabitle | A12-R7 |
| Windows 10 güncelleme süresi (Ekim 2027) | kayıt ve Windows 11 planı | A10-R9 |

## 4. Lisans ve hukuk (hukuki görüş değildir)
| Risk | Önlem | Kaynak |
|---|---|---|
| Asset Store / Reallusion / MetaHuman varlığının uygulama kütüphanesine girmesi | dağıtımda kullanma; yazılı teyit | A12-R4, C-016 |
| TikTok'ta etiketsiz yapay ses | "AI-generated" etiketini aç | A12-R5 |
| ElevenLabs ücretsiz sesli videoda para kazanma / atıfsız paylaşım | atıf yaz, gelir elde etme | A12 |
| Gemini koşulları (18+, AB'ye sunum, ücretsizde veri incelemesi) | kişisel bilgi yazma | A12-R9 |
| Unity koşulları yapay zekâ ajanının Unity ile etkileşimini yalnız yetkili yolla serbest bırakıyor; dosya köprümüz bu yol değil görünüyor (önceki "kapsamıyor" çıkarımı GERİ ÇEKİLDİ, C-030) | köprü durduruldu (K-004); Unity CLI ya da elle çalıştırma; yazılı soru | R-010, Q-016, D1 §3 |
| Gerçek kişi yüz/sesi, telifli karakter/müzik | kullanma | A12-R11 |

## 5. Araştırma sürecinin riskleri
| Risk | Durum |
|---|---|
| Kullanım sınırı, çalışma alanının silinmesi | açık; sık kayıt ve yedek (R-001, R-002) |
| Arama hakkı dolması | yaşandı; bazı konular eksik (R-006) |
| İki ajan salt okunur git komutu çalıştırdı | kapalı; depo değişmedi (R-011) |
| Bulguların çoğu statik kod okuması | testle doğrulanana kadar kesin sayılmamalı |
| Kaynaklar özet araçla okundu | kritik lisans cümleleri asıl metinden denetlenmeli |

## 6. Asistan, talimat dosyaları ve çocuk karakter (07: R-042…R-047)
| Risk | Olasılık / Etki | Önlem |
|---|---|---|
| Proje talimat dosyaları hâlâ köprüyü anlatıyor | orta-yüksek / yüksek | kullanıcı onayıyla uyarı eklemek; köprü kapalı (R-042) |
| Asistan geçerli biçimde ama yanlış sahne üretir | yüksek / orta | onay ve AS-1 testi (R-043) |
| Uygulama içi dil modeli için Unity koşulu belirsiz | bilinmiyor / yüksek | eylem yaptırmamak; yazılı soru (R-044) |
| TikTok 18 yaş altı benzerliği; çocuk karakter | bilinmiyor / çok yüksek | kıyafet/kamera kuralı; yayın öncesi kontrol (R-045) |
| Yerel model yavaş, disk ister | orta-yüksek / orta | önce ölçüm (R-046) |
| Gemini: 18 altının erişmesi muhtemel uygulamada kullanılamaz | bilinmiyor / yüksek | kullanıcı kitlesi netleşmeden dağıtmamak (R-047) |
