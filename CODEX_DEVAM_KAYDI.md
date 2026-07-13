# Codex Devam Kaydi - Kahve Oyunu

Bu dosya diger Codex hesabinda/profilinde kaldigimiz yerden devam etmek icin hazirlandi.

## Proje Yolu

Bu bilgisayardaki aktif Unity projesi:

`C:\Users\akkus\Documents\Codex\2026-07-02\me\deneme`

Kullanici en basta repo yolunun diger bilgisayarda farkli oldugunu soyledi. Bu bilgisayarda dogru proje bu klasor.

Unity:

`C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe`

Ana sahne:

`Assets\Scenes\SampleScene.unity`

Ana dosya:

`Assets\GameManager.cs`

## Kullanici Tercihleri

- Kullanici "oyuna ekle" demedikce uretilen yeni gorseller once onizleme olarak gosterilmeli.
- Ancak koordinat veya net uygulama istediginde direkt uygulanmali.
- Yama gibi gorunen kotu gorseller istenmiyor. Gerekirse bastan temiz tasarla.
- PNG panel olabilir ama ustundeki metinler, ikonlar, butonlar mumkun oldugunca ayri UI elemanlari olmali.
- UI elemanlari kart/panel icinde local koordinatla durmali; kart hareket edince ic elemanlar bozulmamali.
- Her onemli degisiklikten sonra git checkpoint/commit olustur.
- Kirli worktree olabilir; kullanicinin veya Unity'nin alakasiz dosyalarini geri alma.

## Onemli Git Checkpointleri

Son ilgili checkpointler:

- `5ec3afa` - Food panel v3, alt summary kutulari kaldirildi, dinamik kategori seridi ve watermark eklendi.
- `6ed0208` - Food panel icin yeni close icon asset eklendi, text X yerine sprite kullanimi baglandi.
- `456b1cc` - `FoodPanelCloseButton` koordinati ayarlandi.

Son bilinen alakasiz kirli dosyalar, dokunma:

- `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset`
- `Assets/_Recovery.meta`
- `Assets/_Recovery/`

## Son Durum - Food/Gida Paneli

Gida paneli yeni kalite sistemine gecirildi:

- Arka panel: `Assets/Resources/Art/UI/Food/food_upgrade_panel_background_v3.png`
- Yeni kategori bilgi seridi: `food_category_info_ribbon.png`
- Alt watermark: `food_parchment_watermark.png`
- Close button: `food_close_button.png`

`GameManager.cs` icinde:

- `GetFoodPanelBackgroundSprite()` -> `Art/UI/Food/food_upgrade_panel_background_v3`
- `GetFoodCloseButtonSprite()` artik `LoadSpriteFromResources("Art/UI/Food/food_close_button")` kullaniyor.
- `FoodPanelCloseButton` son koordinat:
  - x `699.2`
  - y `402.3`
  - w `76`
  - h `76`

Food kategori tab pozisyonlari:

- `FoodTabCoffee` x `-665` y `272` w `220` h `165`
- `FoodTabSweet` x `-665` y `93` w `220` h `165`
- `FoodTabSnack` x `-665` y `-87.2` w `220` h `165`
- `FoodTabCold` x `-665` y `-268` w `220` h `165`

Food kart grid:

- Kahve kategorisinde tek urun varsa ortada degil ilk slotta durmasi icin grid `UpperLeft` yapildi.

## Son Kullanici Istegi / Siradaki Is

Kullanici ustteki kategori bilgi seridini gosterdi ve "burayi napcaz" dedi.

Konusulan karar:

Mevcut serit cok kalabalik ve yazi kucuk. Yapilacak yeni tasarim:

- Solda sade kategori adi:
  - `Kahve`, `Tatli`, `Atistirmalik`, `Soguk`
  - `Kahve Koleksiyonu` gibi uzun metin kullanma.
- Ortada/yaninda ayri rozet:
  - `0/1 Alindi`
- Sagda iki ayri okunur pill:
  - `Satis +%0`
  - `Pasif +%0`
- Ikonlar biraz daha buyuk ve aralikli olsun.
- Serit ayni kalabilir ama icindeki elemanlar ayri UI olarak konumlandirilmali.
- Alternatif olarak seridi tamamen kaldirip ustte 3 ayri rozet de dusunulebilir, ama kullanici "tamam bunu yapacagiz" dedi. Yani oncelik mevcut seridi daha okunur hale getirmek.

Bu is henuz uygulanmadi. Kullanici uygulamayi guncelleyecegini ve sonra devam edecegini soyledi.

## Food Category Info Mevcut Runtime Elemanlari

`CreateFoodCategoryInfo()` icinde su elemanlar var:

- `FoodParchmentWatermark`
- `FoodCategoryInfoRibbon`
- `ProgressIcon`
- `SalesIcon`
- `PassiveIcon`
- `CategoryName`
- `CategoryProgress`
- `CategorySales`
- `CategoryPassive`

Son bilinen eski layout:

- `FoodCategoryInfoRibbon` panel local position `(90, 330)`, size `(1260, 88)`, sliced.
- `ProgressIcon` `(-210, 0)` size `28x28`
- `SalesIcon` `(45, 0)` size `28x28`
- `PassiveIcon` `(285, 0)` size `28x28`
- `CategoryName` `(-365, 0)` size `250x46`
- `CategoryProgress` `(-115, 0)` size `160x46`
- `CategorySales` `(145, 0)` size `160x46`
- `CategoryPassive` `(385, 0)` size `160x46`

Bunlar okunurluk icin yeniden ayarlanacak.

## Food Ekonomi

Gida urunleri tek sefer alinacak, seviye yetmeden alinmayacak.

Fiyatlar:

- Coffee `250k`, level 1
- Cookie `750k`, level 2
- Donut `2m`, level 3
- Cake `5m`, level 5
- Croissant `12m`, level 7
- Cupcake `25m`, level 9
- Cheesecake `50m`, level 12
- Sandwich `100m`, level 15
- Cold Coffee `200m`, level 18

Her urun:

- `+5% Satis Geliri`
- `+5% Pasif Gelir`

## Food Kart Ic Eleman Koordinatlari

Acik kart:

- `LevelLabel` `17.99998 -30.04402 85.427 22.5021`
- `LevelText` `-52 -26.024 38 30.543`
- `LevelBadge` `-52 -26.4 42 42`
- `OrderText` `10.7209 -69.911 114.4419 19`
- `OrderStrip` `0.49998 -69.911 147 18`
- `OrderIcon` `-59.99997 -72.481 20 20`
- `PassiveText` `9.564996 -95.7 112.13 19`
- `PassiveStrip` `-0.00002718 -95.2 147 18`
- `PassiveIcon` `-60 -96.19999 20 20`
- `Icon` `-0.000035286 53 112.0013 94.2971`
- `UpgradeButton` `0 -127 140 30`
- `Text` local `-29 0 60 25`
- `PriceText` `24.10001 -127 72 28`
- `PriceCoin` `50.7 -127 15 15`

Kahve dekor cekirdekleri:

- `BeanDecorLeft` `-24.89997 -4.900003 17 17`
- `BeanDecorCenter` `-2.9 -8.9 18 18`
- `BeanDecorRight` `19.10003 -3.900003 16 16`

Kilitli kart son ayari:

- `LevelLabel` `11.714 -26.024 85.427 22.5021`
- `LevelRowBackground` `9.815 -26.4 100.57 21.959`

## Derleme / Kontrol

Son derlemelerde:

`dotnet build --no-restore Assembly-CSharp.csproj`

Sonuc:

- `0 hata`
- 12 adet eski/var olan obsolete warning

Bu warningler yeni isten kaynakli degil.

## Notlar

- Unity bazen Play mode icindeyken eski runtime UI nesnesini/cache'i gosterebiliyor. Kod degisse bile panel tekrar olusmadan gorsel yenilenmeyebilir.
- `FoodUISetupTool.cs` var ve Food UI import ayarlarini guvenli uygulamak icin kullaniliyor.
- Assets klasorunde yeni UI PNGleri `Assets/Resources/Art/UI/Food` altinda.
- Kullanici teknik detay istemezse sade anlat.
