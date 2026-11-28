# PATCH_B (Ajan B: kamera, 3D mekan, yonetmenlik, kayit guvenilirligi)

Uygulama sirasi: once PATCH_A.md, sonra bu dosya. Asagidaki BUL metinleri PATCH_A uygulanmis ya da uygulanmamis dosyada da tek ve aynen bulunur.
Not: StoryModel.cs, Sahne3D.cs, StudyoKayit.cs ve yeni dosyalar zaten cikti klasorunde (A'nin StoryModel eklemeleri birlestirildi). Asagisi sadece sahibi olmadigim StoryPlayer.cs icin.

---

## StoryPlayer.cs  (Assets/HikayeStudyo/Scripts/StoryPlayer.cs)

### 1) Run(): "bu satirdan oynat" atlama dalinda yonetmen komutlari (hizli, beklemesiz)

BUL:
```csharp
                        case ItemType.Camera: CameraCommand(it.Arg); break;
                        case ItemType.Insert: ShowInsert(it.Arg); break;
```

DEGISTIR:
```csharp
                        case ItemType.Camera: CameraCommand(it.Arg); break;
                        case ItemType.Direct: if (s3d != null) Yonetmen3D.Hizli(s3d, it); break;
                        case ItemType.Insert: ShowInsert(it.Arg); break;
```


### 2) Run(): canli oynatmada yonetmen komutlari (gir:, gecis:, sessizlik: ... bitene kadar bekler)

BUL:
```csharp
                    case ItemType.Music:
                        SetMusic(it.Arg);
                        break;
                    case ItemType.Pose:
```

DEGISTIR:
```csharp
                    case ItemType.Music:
                        SetMusic(it.Arg);
                        break;
                    case ItemType.Direct:
                        if (s3d != null) yield return Yonetmen3D.Uygula(s3d, it);
                        else Debug.LogWarning("[Hikaye] " + it.Line + ". satır: '" + it.Character + ":' için 3D sahne yok (hikayede 3D karakter kadrosu gerekir).");
                        break;
                    case ItemType.Pose:
```


### 3) BuildCast: hikayede yonetmen komutu varsa 3D sahneyi karakter olmasa da kur; oyuncu yaratma ve figuran havuzu

BUL:
```csharp
            if (cast.Count > 0)
            {
                var map = new Dictionary<string, string[]>(Sahne3D.Varsayilan);
```

DEGISTIR:
```csharp
            if (cast.Count > 0 || Sahne3D.HikayeGerektirir(items))
            {
                var map = new Dictionary<string, string[]>(Sahne3D.Varsayilan);
```

(`items`, BuildCast(List<StoryItem> items) parametresidir.)

BUL:
```csharp
                s3d.Harita = map;
            }
        }
```

DEGISTIR:
```csharp
                s3d.Harita = map;
                s3d.OyuncuYarat = YaratOyuncu;
                s3d.FiguranPrefablari = oyuncular;
            }
        }

        // yer:/gir: komutlari henuz konusmamis bir karakteri sahneye sokarken Sahne3D bunu cagirir
        bool YaratOyuncu(string cid)
        {
            var def = library.FindCharacter(cid);
            if (def == null || s3d == null || Actor3D(def) == null) return false;
            Ensure3D(def, "");
            return s3d.Has(def.id);
        }
```


### 4) CameraCommand: yeni kamera komutlari (tepki, yaklas, uzaklas, goz, yukaridan, asagidan, bulanik, net)

BUL:
```csharp
        void CameraCommand(string a)
        {
            var n = Util.Norm(a);
```

DEGISTIR:
```csharp
        void CameraCommand(string a)
        {
            if (s3d != null)
            {
                string eski;
                if (KameraYonetmen.Komut(s3d, a, out eski)) return;   // 3D yonetmen tamamen isledi
                if (eski != null) a = eski;                           // yaklas/uzaklas -> eski yakin/genis (2D yakinlastirma)
            }
            var n = Util.Norm(a);
```


### 5) Say(): konusan icin 3D yonetmen plani (omuz / yakin / tepki, ayni plan 3 kez tekrarlanmaz)

BUL:
```csharp
            if (is3D) ChooseCamera3D(it, def.id); else { cam3D = null; ChooseCamera(it, view); }
```

DEGISTIR:
```csharp
            if (is3D)
            {
                ChooseCamera3D(it, def.id);
                if (s3d.KameraSatiri(def.id, it.Emotion, it.Inner)) { cam3D = null; camScaleT = 1f; camPosT = Vector2.zero; }
            }
            else { cam3D = null; ChooseCamera(it, view); }
```


### 6) SetBackground: 2D mekan degisince 3D oda kapansin

BUL:
```csharp
            ClearChars();
            ShowInsert("yok");
            var b = library.FindBackground(id);
```

DEGISTIR:
```csharp
            ClearChars();
            if (s3d != null) s3d.MekanKapat();
            ShowInsert("yok");
            var b = library.FindBackground(id);
```


### 7) Update(): sessizlik: sirasinda muzik kapansin

BUL:
```csharp
            ambSrc.volume = Mathf.MoveTowards(ambSrc.volume, amb, dt * 0.25f);
```

DEGISTIR:
```csharp
            amb *= AtmosferVolume.MuzikCarpani;
            ambSrc.volume = Mathf.MoveTowards(ambSrc.volume, amb, dt * 0.25f);
```


### 8) (Istege bagli) eski `atmosfer: yok` yeni atmosfer rengini de kaldirsin

BUL (canli oynatma dali, cok satirli):
```csharp
                    case ItemType.Atmosphere:
                        fx.SetAtmosphere(Util.Norm(it.Arg) == "yok" ? "" : it.Arg);
                        break;
```

DEGISTIR:
```csharp
                    case ItemType.Atmosphere:
                        fx.SetAtmosphere(Util.Norm(it.Arg) == "yok" ? "" : it.Arg);
                        if (s3d != null && Util.Norm(it.Arg) == "yok") s3d.AtmosferAyarla("normal");
                        break;
```

---

## Diger dosyalar (Ajan C icin bilgi, yama degil)

- `ItemType.Direct` gorsel olmayan bir komuttur. StudyoDogrulama'daki "kanca/ilk kare" kontrol listesinde atlanan tiplere eklenmeli; StudyoWindow.EstimateSeconds `gecis:` icin yaklasik 0.8 sn, `sessizlik: N` icin N sn saymali.
- Gecerli degerler tek yerde: `YonetmenTablo.Mekanlar / Zamanlar / Atmosferler / Animlar / Gecisler / Isiklar / Kameralar` (Yonetmen3D.cs). StudyoDogrulama bunlari kullanabilir.
- `RitimDenetci.Rapor(ParseResult, StoryLibrary)` C'nin yansima ile arayacagi giris noktasidir; satirlar "[KIRMIZI] " / "[SARI] " ile baslar.
