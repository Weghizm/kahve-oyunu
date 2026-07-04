# Kahve Oyunu Devam Notlari

Bu proje Android icin yatay ekranli kafe/clicker/idle oyun prototipidir.

## Proje Durumu

- Unity proje klasoru: `deneme`
- Ana script: `Assets/GameManager.cs`
- Hedef platform: Android
- Android build calisti ve telefonda acildi.
- Oyun yatay modda aciliyor.
- Canvas 1920x1080 landscape referansiyla calisiyor.
- Safe area + 16:9 GameFrame sistemi eklendi.

## Mevcut Sistemler

- Coin kazanma
- Bos alana tiklayarak satis
- Musteriye satis ile coin + XP
- XP/level sistemi
- Gem sistemi
- Sandik sistemi
- Gunluk odul sistemi
- Gorev ve gunluk gorev pencereleri
- Offline kazanc penceresi
- Reklamla 2x coin sistemi, sure stacklenir, maksimum 30 dakika
- Test paneli
- Oda sistemi: baslangic odasi, uretim odasi, VIP servis odasi
- Kahve cesitleri, kahve makineleri, barista panelleri
- Dekorasyon paneli prototipi

## Gorsel Yon

Ana hedef My Cafe benzeri, ama kopya olmayan izometrik tek oda sahnesidir.

- Tum bina degil, tek oda gorunecek.
- Odalar arasi butonla gecilecek.
- Oda zamanla dolacak.
- Oda icinde tezgah, makineler, baristalar, musteri masalari ve dekor slotlari olacak.
- Son referans karari: ikinci My Cafe ornegi ana sahne hissi icin daha uygun; ilk ornekten ekran dolulugu alinabilir.

## Tasarim Notlari

### Tema Sistemi

Oyuncu oda temasi secebilmeli:

- Tatli Kafe
- Modern Isletme
- Dogal Kafe
- Luks Kafe
- Retro Kafe

Tema oyuncuyu kilitlememeli; sadece onerilen esyalari ve market siralamasini etkilemeli.

### Market ve Kombin

- Market eklenecek.
- Esyalar tek tek satilacak.
- Market icinde tema kombinleri olacak.
- Kombinler bildirim olarak zorla cikmayacak.
- Kombinler gem veya gercek para ile satilabilir.

### ItemId / Database

Her esya, sandik odulu ve kombin unique kodla tanimlanacak.

Ornek:

- `wall_pink_soft_001`
- `floor_wood_light_001`
- `table_rose_round_001`
- `counter_modern_black_001`
- `plant_small_green_001`

Market, sandik, craft, kombin ve sahip olunan esya sistemi bu ItemId kodlariyla calismali.

### Craft Malzemeleri

`Boya Parcasi` ismi kullanilmayacak. Bunun yerine oyuncuya gorunen isim `Boya Kutusu` olacak.

Ileride renkli boya kutulari ayri item olarak tanimlanacak:

- `paint_can_pink_001` = Pembe Boya Kutusu
- `paint_can_red_001` = Kirmizi Boya Kutusu
- `paint_can_blue_001` = Mavi Boya Kutusu
- `paint_can_black_001` = Siyah Boya Kutusu
- `paint_can_white_001` = Beyaz Boya Kutusu
- `paint_can_gold_001` = Altin Boya Kutusu

Duvar, zemin ve tezgah kaplamalari bu renkli boya kutulariyla craft edilebilir.

Boya kutulari 3 sinifa ayrilacak:

- Siradan boya kutulari
- Nadir boya kutulari
- Luks boya kutulari

Sandiklar bu siniflara gore odul verecek:

- Siradan Sandik: sadece siradan boya kutulari verebilir.
- Nadir Sandik: siradan + nadir boya kutulari verebilir.
- Luks Sandik: siradan + nadir + luks boya kutulari verebilir.

Siradan sandiktan luks boya cikmamali. Bu ekonomi ve market degerini korumak icin onemli.

Test ve UI okunabilirligi icin boya kutularinin isminde nadirlik ibaresi gorunmeli:

- Siradan boyalar: ek ibare olmayabilir veya `(S)` kullanilabilir.
- Nadir boyalar: `(N)` ibaresi kullanilabilir.
- Luks boyalar: `(L)` ibaresi kullanilabilir.

Bu sayede test asamasinda sandiktan cikan boyanin siradan, nadir veya luks oldugu hemen anlasilir.

### VIP

VIP sistemi olabilir.

- Gem/coin marketinde indirim verilebilir.
- Gercek para paketlerinde fiyat dusurmek yerine ekstra bonus vermek daha mantikli.

## Sonraki Is Sirasi

1. Sahne perspektifini My Cafe benzeri izometrik oda olarak netlestir.
2. `Satis / Musteri / Kacan` yazisini kaldir veya daha iyi yere tasi.
3. Market butonu ve market penceresi ekle.
4. `ItemDatabase` altyapisini kur.
5. Ilk dekor itemlerini ItemId ile tanimla.
6. Sandik sistemini ItemId ile odul verecek hale getir.
7. Tema secim sistemi ekle.
8. Market icinde ilk kombin setlerini prototip olarak goster.
