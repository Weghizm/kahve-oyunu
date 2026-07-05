# Diger Bilgisayar / Codex Devir Notu

Bu dosya, projeyi diger bilgisayarda veya yeni bir Codex oturumunda kaldigimiz yerden devam ettirmek icin hazirlandi.

## Proje

- Unity proje klasoru: `deneme`
- Ana script: `Assets/GameManager.cs`
- Ana not dosyasi: `DEVAM_NOTLARI.md`
- GitHub repo: `weghizmx/kahve-oyunu`
- Branch: `main`

Diger bilgisayara gecince:

1. GitHub Desktop ac.
2. Repo olarak `kahve-oyunu` sec.
3. Once `Fetch origin`, sonra cikarsa `Pull origin` yap.
4. Unity kapaliyken pull yapmak daha iyi.
5. Pull bitince Unity ile projeyi ac.

Is bitince:

1. Unity kapat veya kaydet.
2. GitHub Desktop'ta degisiklikleri kontrol et.
3. Commit yaz.
4. `Push origin` yap.

## Su Anki Durum

Oyun Android landscape prototipi olarak ilerliyor. Ana sahne izometrik kafe odasi gibi gorunuyor. Coin/gem kaynak barlari yeni ikonlu UI stiline gecirildi.

Son onemli durum:

- Coin ve gem ikonlari eklendi.
- Coin ve gem barlari ikon + bar olarak tek parca gibi gorunuyor.
- Coin/gem barlarinda ikon, arka plan veya sayi secilmeye calisilirsa Unity editorunde ana bar objesi secilecek sekilde duzenlendi.
- XP/level rozeti oyuna baglandi, ancak gorsel karar degisecek.
- Mevcut rozet kalkan gibi gorunuyor ve level sayisi icine tam oturmamis.
- Siradaki ilk is: rozeti kafe/level hissine uygun yeni ikonla degistirmek, sayiyi ortalamak, XP barindaki `0/100` yazisini kaldirip ilerlemeyi sadece bar dolumuyla gostermek.

## Son UI Karari

Coin ve gem icin istenen goruntu:

- Ikon solda olacak.
- Bar ikonla birlesik gibi duracak.
- Bar ikonun altindan rahatsiz edici sekilde gecmeyecek.
- Sayi barin icinde ortali duracak.
- Coin, gem ve XP barlari simdilik ayni boyut hissinde olmali.

Onemli: `Icon`, `Background` veya sayi yerine ana `CoinBar` / `GemBar` objesi tasinmali. Editor kolayligi icin child secilirse ana bar secilmeye calisir.

## Yapilan Son Isler

Son commitlerden bazilari:

- `9ccfada Add level badge icon asset`
- `6666537 Select whole currency bar from parts`
- `e71c247 Note xp hud movement behavior`
- `702f9a0 Set currency icon placement`
- `b415fc3 Blend currency bars with icons`
- `ecb49bc Match top hud bar sizes`
- `9e10f10 Compact top currency hud and add xp bar`
- `8390963 Add coin and gem resource bars`

## Dikkat Edilecekler

- Kodla olusturulan UI'lar Play kapaninca elle yapilan sahne degisikliklerini kaybedebilir. Kalici degisiklikler `GameManager.cs` icine islenmeli.
- Scene veya Game view'da UI objesi elle tasinirken Inspector degerleri alinip koda yazilabilir.
- Unity yeni PNG assetleri icin `.meta` dosyalari olusturursa bunlar da commitlenmeli.
- GitHub Desktop kullanirken her bilgisayarda ise baslamadan once `Fetch/Pull`, is bitince `Commit/Push` yap.

## Siradaki Muhtemel Isler

1. XP/level ikonunu duzelt.
   - Mevcut ikon kalkan gibi duruyor; daha kafe/level rozeti hissinde yeni ikon gerekli.
   - Level sayisi ikonun icine tam ortali ve dengeli oturmali.
   - XP barinda `0/100` yazisi gorunmemeli.
   - XP ilerlemesi direkt barin ic dolumuyla anlasilmali.

2. Coin/gem barlarini son kez telefonda kontrol et.
   - Gem ikonunun altinda mavi bar gorunuyor mu?
   - Sayi ortada mi?
   - Barlar ustte cok mu sikisik?

3. Sandik UI tarafini toparla.
   - Sandiklar penceresinde yazilarin tasmasi ve kalabaligi takip edilecek.
   - Sandik ikonlari ve boya ikonlari ileride daha iyi gorsel diline oturtulacak.

4. Market sistemi planina gec.
   - Market butonu.
   - Gem ile alinabilecek esyalar.
   - No Ads paketi notlari.
   - Kombin/tema paketleri.

5. Sahne ve oda sistemi.
   - My Cafe benzeri ama kopya olmayan oda perspektifi devam edecek.
   - Tek oda gorunumu, butonla oda degistirme mantigi korunacak.
   - Ileride yeni oda satin alma ve oda dekorasyonu eklenecek.

## Onemli Tasarim Notlari

- Oyuncular icin dekorasyon ve tema sistemi onemli olabilir.
- Hedefte kiz oyuncular da olabilir; pembe/tatli kafe temalari dusunuldu.
- Daha soguk/isletme odakli tema da olacak.
- Tema oyuncuyu kilitlememeli, sadece oneriler ve market siralamasini etkilemeli.
- Gem kolay kazanilabilir ama premium esyalar daha pahali olabilir.
- Reklam geliri ana gelir modeli, ancak `No Ads` paketi de ileride dusunuluyor.

## Diger Codex'e Kisa Mesaj

Bu projede kullanici teknik olmayan sekilde ilerliyor. Kod degisikligi gerekiyorsa genelde direkt uygula, ama UI tasarimi konusunda kullanici screenshot ile karar vermek istiyor. Her isten sonra kisa ozet ver, commit al, kullaniciya GitHub Desktop'tan `Push origin` hatirlat.

