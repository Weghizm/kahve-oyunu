# Kahve Oyunu - Codex Devir Notu

Bu not diger bilgisayarda veya yeni Codex oturumunda kaldigimiz yerden devam etmek icin hazirlandi. Once bu dosyayi oku, sonra projeyi ac ve git durumunu kontrol et. Kullanici teknik detayla ugrasmak istemiyor; hedef netse soru sormadan uygula, test et, kisa ozet ver.

## Proje Bilgileri

- Proje yolu: `C:\Users\akkus\Documents\kahve-oyunu`
- Ana kod dosyasi: `C:\Users\akkus\Documents\kahve-oyunu\Assets\GameManager.cs`
- GitHub repo: `https://github.com/weghizmx/kahve-oyunu.git`
- Branch: `main`
- Unity surumu: `6000.5.2f1`
- Unity exe: `C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe`
- Git yolu: `C:\Users\X\AppData\Local\GitHubDesktop\app-3.6.2\resources\app\git\cmd\git.exe`
- Screenshot klasoru: `C:\Users\X\Pictures\Screenshots`

Kullanici `ss bak` derse yeni ekran goruntulerini screenshot klasorunden kontrol et.

## Calisma Tarzi

- Kullanici "hadi", "yap", "devam et" dediginde genelde direkt uygulama bekliyor.
- Geri alinabilir islerde gereksiz soru sorma.
- Gorsel/UI islerinde kullanici preview veya telefondaki screenshot ile karar veriyor.
- Her isten sonra derleme veya uygun kontrol yap, sonra commit al.
- Mevcut kirli/untracked dosyalari kullanici istemedikce silme veya geri alma.
- Oyuncuya "Gem" yazma; gorunen para birimi adi `Elmas`.

## Su Anki Temel Durum

Oyun Unity Android landscape kahve dukkani/clicker-idle prototipi. Ana UI ve gameplay buyuk oranda `Assets/GameManager.cs` icinde kodla olusuyor. Ana sahne yeni sicak kafe gorseliyle ilerliyor; eski mavi prototip arka plan kullanilmamali.

## Son Yapilan Onemli Isler

### Kahve Makinesi

- Makine arka plandan katmanlara ayrildi.
- Makinesiz temiz kafe arka plani kullaniliyor.
- Kilitli makine ve acik makine ayri sprite/layer olarak calisiyor.
- Kilitliyken arka planda eski makine gorunmesi problemi cozuldu.
- Son ilgili commit: `1e2c791 Use clean machine-free cafe background`

### Hotspot Konumlari

- Kahve paketleri hotspot manuel konumu:
  - x: `312.4`
  - y: `365.85`
  - width: `244.8`
  - height: `137.7`
- Barista/tezgah hotspot manuel konumu:
  - x: `-48.10552`
  - y: `178.6`
  - width: `158.7888`
  - height: `193.8`

### Barista

- Kilitli gri barista ve acik normal barista gorselleri eklendi.
- Assetler:
  - `Assets/Resources/Art/SceneObjects/barista_locked_placeholder.png`
  - `Assets/Resources/Art/SceneObjects/barista_unlocked.png`
  - `Assets/Resources/Art/SceneObjects/barista_counter_foreground.png`
- Barista tezgahin arkasinda duruyor; tezgah on maske sprite'i baristanin alt kismini kapatiyor.
- Editor icinde barista konumunu surukleyip Console'dan deger alma destegi var.
- Son durum kullanici tarafindan "mukemmel oldu" diye onaylandi.
- Son ilgili commitler:
  - `ef94975 Add locked and unlocked barista scene sprites`
  - `3da1435 Enable editor drag probe for barista sprites`
  - `655d049 Refine barista counter foreground mask`

### 2x Reklam / Ad Boost

- Sag taraftaki eski 2x coin reklam butonu ikonlu UI'a gecti.
- Assetler:
  - `Assets/Resources/Art/UI/ad_boost_2x_icon.png`
  - `Assets/Resources/Art/UI/ad_boost_timer_badge.png`
- Alt yazi artik sure yerine kisa durum gosteriyor:
  - `AD`
  - `AKTIF`
  - `MAX`
- Son kullanici istegi uzerine butona basinca onay penceresi eklendi.
- Pencere metni: reklam izleyerek 5 dakikalik 2x coin kazanmak isteyip istemedigini sorar.
- `Izle` butonu 5 dakikalik 2x'i baslatir, `Vazgec` kapatir.
- Sagdaki ikon artik sadece ustte duran gorsel degil; butonun kendi yuzeyi olarak ayarlandi.
- Son commit: `730f31b Add ad boost confirmation popup`

## Siradaki Is

Kullanici "artik acilir pencereleri duzenleyelim" dedi. Bundan sonra ilk buyuk is popup/panel tasarimlarini toparlamak.

Once incelenecek pencereler:

1. 2x reklam onay penceresi
2. Kahve gelisim paneli
3. Makine gelisim paneli
4. Barista paneli
5. Gorev/gunluk gorev pencereleri
6. Sandik pencereleri
7. Offline kazanc penceresi

Hedef: Butonlar, basliklar, metin boylari ve renkler ayni oyun diline otursun. Mobilde okunabilir olsun, sikismasin, fazla prototip gibi durmasin.

## Dikkat Edilecek Kirli Dosyalar

Bu dosyalar/dizinler calisma alaninda kirli veya untracked olabilir. Kullanici istemedikce silme/geri alma:

- `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`
- `CODEX_BASLANGIC.md`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/ShaderGraphSettings.asset`
- `Assets/_Recovery*`
- Cesitli preview PNG klasorleri ve dosyalari: `preview_*`, `*_preview*.png`

## Kontrol Komutlari

Proje klasorune gec:

```powershell
cd C:\Users\akkus\Documents\kahve-oyunu
```

Git durumunu kontrol et:

```powershell
& 'C:\Users\X\AppData\Local\GitHubDesktop\app-3.6.2\resources\app\git\cmd\git.exe' status --short
```

Derleme kontrolu:

```powershell
dotnet build --no-restore Assembly-CSharp.csproj
```

## Diger Bilgisayarda Baslama

1. GitHub Desktop'ta `kahve-oyunu` reposunu ac.
2. `Fetch origin` ve gerekiyorsa `Pull origin` yap.
3. Unity kapaliyken pull yapmak daha iyi.
4. Unity ile projeyi ac.
5. Codex'e su mesaji ver:

```text
C:\Users\akkus\Documents\kahve-oyunu\CODEX_BASLANGIC.md dosyasini oku. Projeyi C:\Users\akkus\Documents\kahve-oyunu yolundan ac. Once git durumunu kontrol et. Son durum: 2x reklam onay popup'i eklendi. Siradaki is acilir pencereleri duzenlemek.
```

## Son Commitler

- `730f31b Add ad boost confirmation popup`
- `8b0f5ed Use coffee cup ad boost icon`
- `bc16543 Show ad boost status without timer`
- `dc68eeb Redesign ad boost button timer badge`
- `655d049 Refine barista counter foreground mask`
- `3da1435 Enable editor drag probe for barista sprites`
- `1dadba2 Restore barista placement for manual tuning`
- `265d676 Lower barista scene placement`
