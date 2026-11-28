# PATCH_A: Agent A (canlilik + altyazi) icin diger dosyalara yapilacak degisiklikler

Her yama: dosyada BUL kismi **tam olarak bir kez** gecer (dogrulandi, orijinal yuklenen dosyalara karsi). BUL metnini aynen DEGISTIR metniyle degistir. Sirayla uygula.

Not: Baska ajanlarin yamalari ayni satirlara dokunuyorsa (ozellikle `ItemType` enum'u, `Say()` ve `Run()` switch'i), degisikligi elle birlestir; her yamanin amaci yukaridaki basliktadir.

Yeni dosyalar (yamasiz derlenir): AgizSenkron.cs, KaraokeAltyazi.cs, YuzCanlilik.cs, YuzSahne.cs ve degistirilmis Karakter3D.cs. Yamalar uygulanmazsa sadece yeni komutlar/altyazi stilleri devreye girmez; 3D karakter canliligi (agiz, kirpma, nefes, bakis) Karakter3D.cs sayesinde yine calisir, ama agiz sese bagli degil sentetik olur ve dinleyici tepkileri olmaz.


---

## StoryModel.cs  (Assets/HikayeStudyo/Scripts/StoryModel.cs)


### 1) ItemType'a 3 yeni komut turu ekle (enum'un ILK satiri)

BUL:
```csharp
        Dialogue, Background, Hook, Rewind, Title, Wait, Sfx, Remove, Clear,
```

DEGISTIR:
```csharp
        Bak, Dinle, AltyaziStil,   // bak: / dinle: / altyazi: komutlari (YuzSahne.cs, KaraokeAltyazi.cs)
        Dialogue, Background, Hook, Rewind, Title, Wait, Sfx, Remove, Clear,
```


### 2) StoryItem'a *vurgu* isaretli altyazi metni alani ekle

BUL:
```csharp
        public string Speech = "";       // seslendirmeye giden metin (<sigh>, <sob> gibi etiketlerle)
```

DEGISTIR:
```csharp
        public string Speech = "";       // seslendirmeye giden metin (<sigh>, <sob> gibi etiketlerle)
        public string Vurgulu = "";      // altyazi metni, *vurgu* isaretleri korunmus (karaoke altyazi icin)
```


### 3) Konusma satirinda Vurgulu alanini doldur

BUL:
```csharp
                    it.Speech = SesEtiketleri.ToSpeech(it.Text);
                    it.Text = SesEtiketleri.ToDisplay(it.Text);
```

DEGISTIR:
```csharp
                    it.Speech = SesEtiketleri.ToSpeech(it.Text);
                    it.Vurgulu = KaraokeAltyazi.VurguluMetin(it.Text);
                    it.Text = SesEtiketleri.ToDisplay(it.Text);
```


### 4) Parser'a bak: / dinle: / altyazi: komutlarini ekle

BUL:
```csharp
                    case "seri": r.Seri = val; continue;
```

DEGISTIR:
```csharp
                    case "bak": item.Type = ItemType.Bak; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "dinle": item.Type = ItemType.Dinle; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "altyazi": item.Type = ItemType.AltyaziStil; if (!YuzSahne.KomutCoz(key, item, r.Warnings, lineNo)) continue; break;
                    case "seri": r.Seri = val; continue;
```


---

## StoryPlayer.cs  (Assets/HikayeStudyo/Scripts/StoryPlayer.cs)


### 5) Alanlar: altyazi stili ve karaoke nesnesi

BUL:
```csharp
        string cam3D;
```

DEGISTIR:
```csharp
        string cam3D;
        string altyaziStil = KaraokeAltyazi.Klasik;   // altyazi: komutu ile degisir
        KaraokeAltyazi karaoke;
```


### 6) BuildUI: karaoke altyazi katmanini kur (klasik altyazi Text'inden hemen sonra)

BUL:
```csharp
            AddTextEffects(subtitle, 4);
```

DEGISTIR:
```csharp
            AddTextEffects(subtitle, 4);
            karaoke = KaraokeAltyazi.Create((RectTransform)subtitle.transform.parent, font, fontBlack);
```


### 7) Run(): 'bu satirdan oynat' atlama dalinda altyazi stilini uygula

BUL:
```csharp
                        case ItemType.Remove: StartCoroutine(Exit(it.Character)); break;
```

DEGISTIR:
```csharp
                        case ItemType.Remove: StartCoroutine(Exit(it.Character)); break;
                        case ItemType.AltyaziStil: altyaziStil = it.Arg; break;
```


### 8) Run(): yeni komutlar (Insert ile Dialogue case'leri arasina)

BUL:
```csharp
                    case ItemType.Insert:
                        ShowInsert(it.Arg);
                        break;
                    case ItemType.Dialogue:
```

DEGISTIR:
```csharp
                    case ItemType.Insert:
                        ShowInsert(it.Arg);
                        break;
                    case ItemType.Bak:
                        YuzSahne.Bak(it.Character, it.Arg);
                        break;
                    case ItemType.Dinle:
                        YuzSahne.Dinle(it.Character, it.Arg);
                        break;
                    case ItemType.AltyaziStil:
                        altyaziStil = it.Arg;
                        break;
                    case ItemType.Dialogue:
```


### 9) Say(): altyazi (karaoke/klasik) + 3D agiz/dinleyici tepkisi. Eski 9 satirlik blok tamamen bununla degisir

BUL:
```csharp
            subtitle.text = it.Text;
            subtitle.color = def.subtitleColor;
            StartCoroutine(PopText(subtitle.rectTransform, 0.92f));

            AudioClip clip;
            voices.TryGetValue(it, out clip);
            float dur = clip != null ? clip.length : Mathf.Max(1.2f, it.Text.Length / charsPerSecond + 0.3f);
            if (clip != null) { voiceSrc.clip = clip; voiceSrc.volume = voiceVolume; voiceSrc.Play(); }
            if (cutEarly > 0f) dur = Mathf.Max(0.4f, dur - cutEarly);
```

DEGISTIR:
```csharp
            AudioClip clip;
            voices.TryGetValue(it, out clip);
            float dur = clip != null ? clip.length : Mathf.Max(1.2f, it.Text.Length / charsPerSecond + 0.3f);
            if (clip != null) { voiceSrc.clip = clip; voiceSrc.volume = voiceVolume; voiceSrc.Play(); }
            if (cutEarly > 0f) dur = Mathf.Max(0.4f, dur - cutEarly);

            string vurgulu = string.IsNullOrEmpty(it.Vurgulu) ? it.Text : it.Vurgulu;
            bool karaokeAcik = karaoke != null && altyaziStil != KaraokeAltyazi.Klasik &&
                               karaoke.Baslat(vurgulu, clip, dur, altyaziStil, def.subtitleColor);
            if (karaokeAcik) subtitle.text = "";
            else
            {
                subtitle.text = KaraokeAltyazi.KlasikMetin(vurgulu, new Color(1f, 0.85f, 0.25f));
                subtitle.color = def.subtitleColor;
                StartCoroutine(PopText(subtitle.rectTransform, 0.92f));
            }
            YuzSahne.Konus(def.id, clip, dur, it.Emotion, it.Inner);   // 3D agiz hareketi + dinleyici tepkileri
```


### 10) Say() sonu: karaoke ve konusma durumunu kapat

BUL:
```csharp
            fx.Dim(0f);
            subtitle.text = "";
```

DEGISTIR:
```csharp
            fx.Dim(0f);
            subtitle.text = "";
            if (karaoke != null) karaoke.Durdur();
            YuzSahne.Sustur();
```
