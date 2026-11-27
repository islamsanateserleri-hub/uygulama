# 08 TEKNOLOJİ VERİTABANI (dizin)
Protokol §7.5 alanları ilgili ajan dosyasının "Teknoloji kayıtları" bölümündedir; lisans sınıfı: AGENTS/A12_lisans.md ana tablo. Test durumu: hiçbiri bu projede test edilmedi.

| Ajan dosyası | Teknoloji kayıtları bölümü (satır) |
|---|---|
| AGENTS/A00_takip.md | 97 |
| AGENTS/A01_sinematografi.md | 235 |
| AGENTS/A02_karakter.md | 185 |
| AGENTS/A03_animasyon.md | 138 |
| AGENTS/A04_isik_goruntu.md | 147 |
| AGENTS/A05_ortam.md | 189 |
| AGENTS/A06_vfx.md | 160 |
| AGENTS/A07_ses.md | 139 |
| AGENTS/A08_kurgu.md | 279 |
| AGENTS/A09_unity_mimari.md | 176 |
| AGENTS/A10_donanim.md | 182 |
| AGENTS/A11_gelecek.md | 168 |
| AGENTS/A12_lisans.md | 143 |

## Teknoloji başına tek satır (DEN2 sonrası; hiçbiri bu projede test edilmedi)
"Güven" sütunu kaba ÇIKARIM'dır (kaynak türüne göre: yüksek/orta/düşük). Lisanslar hukuki görüş değildir. "okunmadı" = bakılmadı. K-020: kart ya da ücret isteyenler bütçe dışı.

| Teknoloji | Lisans / koşul | Donanım | Maliyet | Test | Güven | Dosya |
|---|---|---|---|---|---|---|
| Unity URP (mevcut) | Unity koşulları (Q-016) | mevcut PC (hız bilinmiyor) | 0 | yok | orta | A09, A04, R03 |
| Unity HDRP | Unity koşulları | belirsiz | — | yok | yüksek (kaldırılacak) | A04, C-003 |
| Unreal 5 + MetaHuman yolu | Epic koşulları | ayrı kart | bütçe dışı | yok | orta | A09, K-022 |
| Blender | GPL; çıktı kullanıcının | CPU yavaş (Vega'da Cycles GPU yok) | 0, disk | yok | orta | A04, D1 |
| Cinemachine 6.6 | okunmadı | mevcut | 0 | yok | düşük-orta | A01, D1 |
| Rocketbox | MIT | mevcut | 0 | yok | yüksek (lisans) | A02, A12 |
| VRoid + MToon/UniVRM (stilize) | MToon/UniVRM MIT; VRoid model başına | mevcut (tahmin) | 0 | yok (G-1) | orta | D1 |
| Unity Toon Shader | okunmadı | bakılmadı | okunmadı | yok | düşük | D1 |
| MPFB / MakeHuman | varlık CC0; kod GPL/AGPL | olası | 0 | yok | orta | A02, D2 |
| Character Creator 5 | mağaza; dağıtımda riskli | ayrı kart | bütçe dışı | yok | düşük-orta | A02, A12 |
| MetaHuman | Epic; dağıtım belirsiz | güçlü PC | bütçe dışı (donanım) | yok | orta | A02, Q-011 |
| UAL hareketleri | CC0 | mevcut | 0 | yok | yüksek (lisans) | A03 |
| Mixamo | dağıtımda riskli | mevcut | 0 | yok | orta | A03, C-021 |
| Gemini TTS | Google koşulları (18+) | bulut | kota; 3.8 TTS ücretsiz katmanı DOĞRULANMADI | yok (sistematik) | düşük-orta | A07, D3 |
| Gemini metin (3.5 Flash-Lite) | Google koşulları; ücretsizde insan incelemesi | bulut | fiyat/katman DOĞRULANMADI | yok (AS-1) | düşük (fiyat) | D3 |
| Claude (abonelik, dışarıdan) | Anthropic koşulları; uygulama içine konamaz | bulut | mevcut abonelik | yok (AS-1) | orta | D3 |
| Yerel model (Ollama / llama.cpp) | llama.cpp MIT; model başına | CPU, 16-23 GB indirme; hız bilinmiyor | 0, disk | yok (AS-1) | düşük | D3 |
| Unity CLI | Unity koşulları; yeni paket | mevcut | 0 | yok | orta | D1 |
| MFA / parseh / faster-whisper (hizalama) | model başına | CPU | 0 | yok | düşük (Türkçe) | A07, Q-007 |
| uLipSync | MIT | mevcut | 0 | yok | orta | A07, R03 |
| Düz yazı çeviricisi (`DuzMetniCevir`, mevcut) | proje kodu | mevcut | 0 | yok (DY-1) | orta (kod okuması) | D2 |
| ZemberekDotNet | MIT | CPU | 0; yeni paket | yok | düşük | D2 |
| FSR 1 / STP (URP) | URP ile | STP compute ister; bu PC kâğıt üzerinde karşılıyor | 0 | yok | düşük-orta | A04, D2 |
| Real-ESRGAN vb. / Video2X (yapay zekâ büyütme) | Video2X AGPL-3.0 | Vulkan; hız bilinmiyor | 0 | yok | düşük | D2 |
| 4K dikey kayıt (`h264_amf`) | FFmpeg LGPL | dikey kabul doğrulanmadı | disk | yok (4K-1) | düşük | D2 |
| 2D paketler (mağaza/itch/OpenGameArt) | paket başına; Asset Store riskli | hafif (tahmin) | 0 – 25 $ örnekler | yok | düşük | D2 |
| iClone 8 | Reallusion; ücretsizde ticari yok | en az 4 GB GPU | okunamadı | yok | düşük | D1 |
| DaVinci Resolve / Natron | özel lisans (okunmadı) / GPL | Resolve en az 4 GB VRAM | 0 | yok | düşük | D1 |
| VFX Graph / Shuriken | Unity | VFX Graph compute ister | 0 | yok | orta | A06 |
| CC0 varlıklar (Poly Haven, ambientCG) | CC0 | disk | 0 | yok | yüksek (lisans) | A05 |
