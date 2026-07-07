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
- Offline kazanc aktif pasif gelirin 1/3'u olarak hesaplanir ve maksimum 4 saatle sinirlidir; reklamla 2x coin offline kazanca etki etmez.
- Test paneli
- Oda sistemi: baslangic odasi, uretim odasi, VIP servis odasi
- VIP seviye sistemi: 0-5 seviye, gem ile yukseltme prototipi, HUD VIP rozeti, VIP paneli, kit/bonus metinleri
- Kahve cesitleri, kahve makineleri, barista panelleri
- Dekorasyon paneli prototipi

## Son Yapilanlar

### XP / Level HUD

- Kullanici telefonda kontrol etti; yeni level rozeti iyi gorunuyor.
- XP yazisi kaldirildi, ilerleme bar dolumuyla anlasiliyor.
- Level sayisi yeni rozet icinde daha dengeli duruyor.
- Level sayisi otomatik kuculerek iki haneli seviyelerde de rozet icinde kalacak.
- Coin/gem barlarinda sayi buyudukce ikonun icine girmemesi icin metin alani saga alindi ve otomatik kuculme eklendi.

### VIP Sistem Altyapisi

- `Assets/GameManager.cs` icine VIP seviye sistemi eklendi.
- VIP seviyesi 0-5 arasi tutuluyor.
- VIP seviyesi `PlayerPrefs` ile kaydediliyor ve resetlenebiliyor.
- HUD ust barina `VIP 0` butonu eklendi.
- VIP paneli acma/kapama akisi eklendi.
- VIP seviyesi simdilik gem ile yukseliyor; bu sadece prototip ekonomi.
- Test paneline `TEST +1 VIP` butonu eklendi.
- VIP bonuslari su alanlara baglandi:
  - Coin kazanci
  - XP kazanci
  - Pasif gelir
  - Musteri bekleme suresi
- Ayrica offline kazanc VIP pasif gelir bonusunu hesaba katiyor.

### VIP UI Onizleme

- Kullanici laptopta calisirken screenshot yolu olarak `C:\Users\X\Pictures\Screenshots` kullanilacak.
- Kullanicinin gonderdigi VIP referans screenshoti incelendi.
- Referanstaki koyu arka plan + altin premium kart hissi begenildi.
- Yatay ekran icin yeni VIP panel onizlemesi olusturuldu:
  - `vip_ui_landscape_preview.png`
  - `vip_ui_landscape_preview_tier_colors.png`
- Ilk onizlemede koyu zemin, buyuk uyelik karti, VIP seviye kartlari ve bonus ikonlari denendi.
- Son onizlemede VIP sinif kartlari kendi renklerine ayrildi:
  - VIP 1: Bronz Servis Kiti, bronz kart
  - VIP 2: Gumus Kazanc Kiti, gumus kart
  - VIP 3: Altin Dekor Kiti, altin kart
  - VIP 4: Premium Barista Kiti, mor/premium kart
  - VIP 5: Elmas VIP Kiti, acik mavi/elmas kart
- Renkli sinif kartlari fikri Unity paneline uygulandi.
- VIP paneli artik daha buyuk koyu premium panel, ustte aktif uyelik karti ve altta Bronz/Gumus/Altin/Premium/Elmas kartlariyla aciliyor.
- VIP kartlari kisa bonus metinleri kullaniyor; kilitli kartlar daha sonuk, aktif kart daha parlak gorunuyor.
- Yukseltme butonu `VIP X Al / Gem` formatina gecti.

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
- Ozel boya kutulari

Sandiklar bu siniflara gore odul verecek:

- Siradan Sandik: sadece siradan boya kutulari verebilir.
- Nadir Sandik: siradan + nadir boya kutulari verebilir.
- Ozel Sandik: siradan + nadir + ozel boya kutulari verebilir.

Siradan sandiktan ozel boya cikmamali. Bu ekonomi ve market degerini korumak icin onemli.

Sandiklar ve envanter ekraninda boya gruplari su isimlerle gorunmeli:

- Siradan Boya Kutulari
- Nadir Boya Kutulari
- Ozel Boya Kutulari

`Genel Boya Kutusu` ve `Renkli Boya Kutusu` ifadeleri karisik duruyor; boyalar zaten renkli oldugu icin bu isimler kullanilmamali.

Test ve UI okunabilirligi icin boya kutularinin isminde nadirlik ibaresi gorunmeli:

- Siradan boyalar: ek ibare olmayabilir veya `(S)` kullanilabilir.
- Nadir boyalar: `(N)` ibaresi kullanilabilir.
- Ozel boyalar: `(O)` ibaresi kullanilabilir.

Bu sayede test asamasinda sandiktan cikan boyanin siradan, nadir veya ozel oldugu hemen anlasilir.

### VIP

VIP sistemi prototip olarak eklendi.

- VIP 0-5 arasi seviye tutar.
- HUD ust barinda `VIP 0` butonu bulunur; VIP panelini acar.
- VIP paneli mevcut seviye, aktif kit ve 1-5 seviye bonuslarini gosterir.
- Simdilik VIP seviyesi gem ile yukselir; ileride gercek satin alma, gorev odulu veya reklam odulu akisi buraya baglanabilir.
- Test panelinde `TEST +1 VIP` butonu vardir.
- VIP bonuslari coin, XP, pasif gelir ve musteri bekleme suresine etki eder.
- VIP UI hedefi: koyu premium panel, sinif rengine gore VIP kartlari ve buyuk aktif uyelik karti.
- Bronz/Gumus/Altin/Premium/Elmas siniflari gorsel olarak birbirinden net ayrilmali.
- Gem/coin marketinde indirim verilebilir.
- Gercek para paketlerinde fiyat dusurmek yerine ekstra bonus vermek daha mantikli.

### UI / HUD Notlari

Coin ve gem barlari ikon + bar olarak tek butun gibi davranmali.

- `CoinBar` icindeki ikon veya arka plan secilip tasinsa bile tum coin bari hareket etmeli.
- `GemBar` icindeki ikon veya arka plan secilip tasinsa bile tum gem bari hareket etmeli.
- XP/level UI tamamlandiginda ayni davranis XP bari icin de eklenmeli.
- XP ileride coin barinin altina alinabilir; bu yerlesim yeniden degerlendirilecek.


### Bir Sonraki Ilk Is: VIP Kontrolu

XP/level rozeti toparlandi. Siradaki ilk is VIP HUD butonu ve yeni VIP panelini Unity/telefonda kontrol etmek olacak.

## Sonraki Is Sirasi

1. Unity icinde VIP HUD butonu ve yeni VIP panelini telefonda/gorunumde kontrol et.
2. VIP bonuslarini test et: TEST +1 VIP ile coin, XP, pasif gelir ve musteri surelerinin arttigini dogrula.
3. VIP kart metinlerini mobilde kontrol et; tasma/ust uste binme varsa font ve kart bosluklarini ayarla.
4. Sandiklar ekranini duzelt: gem logu/son gem odulu kalabaligini kaldir; boya gruplari `Siradan Boya Kutulari`, `Nadir Boya Kutulari`, `Ozel Boya Kutulari` olarak net gorunsun.
5. Sahne perspektifini My Cafe benzeri izometrik oda olarak netlestir.
6. `Satis / Musteri / Kacan` yazisini kaldir veya daha iyi yere tasi.
7. Market butonu ve market penceresi ekle.
8. `ItemDatabase` altyapisini kur.
9. Ilk dekor itemlerini ItemId ile tanimla.
10. Sandik sistemini ItemId ile odul verecek hale getir.
11. Tema secim sistemi ekle.
12. Market icinde ilk kombin setlerini prototip olarak goster.
