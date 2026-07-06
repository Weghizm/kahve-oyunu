# Kahve Oyunu - Yeni Codex Baslangic Notu

Bu notu okuyan Codex: kullanici teknik olmayan sekilde ilerliyor. Projeyi incele, mevcut notlari oku, sonra kaldigimiz yerden devam et. Gereksiz soru sorma; makul olan yerde direkt uygula, test et, kisa ozet ver ve commit al.

## Proje

- Unity proje yolu: `C:\Users\akkus\Documents\kahve-oyunu`
- Ana script: `C:\Users\akkus\Documents\kahve-oyunu\Assets\GameManager.cs`
- Ana devam notu: `C:\Users\akkus\Documents\kahve-oyunu\DEVAM_NOTLARI.md`
- Devir notu: `C:\Users\akkus\Documents\kahve-oyunu\DIGER_BILGISAYAR_DEVIR_NOTU.md`
- Unity: `C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe`
- Unity ADB: `C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe`
- Telefon: Samsung `SM_S721B`

## En Son Durum

- Android landscape kafe/clicker/idle prototipi.
- UI kodla `GameManager.cs` icinde olusturuluyor; kalici UI degisikligi icin genelde bu dosya duzenlenmeli.
- Coin, gem, XP/level HUD var.
- VIP sistemi var: VIP 0-5, gem ile yukseltme, test panelinde `TEST +1 VIP`.
- VIP paneli premium koyu panel + Bronz/Gumus/Altin/Premium/Elmas renkli kartlar olarak uygulandi.
- Offline kazanc duzeltildi:
  - Oyun arka plandan geri gelince de hesaplanir.
  - Aktif pasif gelirin `1/3`u verilir.
  - Maksimum `4 saat` hesaplanir.
  - Reklamla `2x coin` offline kazanca etki etmez.

## Son Commitler

- `9120c0b Limit offline earnings economy`
- `2f8b9aa Apply offline earnings on app resume`
- `019e273 Apply premium VIP panel UI`
- `17ed249 Restore shield level badge icon`
- `f6039c4 Refine XP level badge design`

## ADB / Telefon

Bu laptopta kablosuz ADB hotspot uzerinden ayarlandi.

- Telefon hotspot aciyor, laptop o agda.
- Son kablosuz ADB adresi: `10.129.254.149:5555`
- Kontrol:
  `& 'C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe' devices -l`
- Gerekirse kabloyla tak:
  `adb tcpip 5555`
  sonra gateway IP ile:
  `adb connect 10.129.254.149:5555`

## Siradaki Is

1. Telefonda Build and Run yap.
2. VIP paneli kontrol et:
   - `VIP 0` butonu aciyor mu?
   - Kartlar tasmadan gorunuyor mu?
   - `TEST +1 VIP` ile aktif kart dogru degisiyor mu?
3. Offline kazanci test et:
   - Makine/barista alip pasif gelir olustur.
   - Oyunu arka plana al/kapat, bekle, geri ac.
   - Offline kazanc pasif gelirin 1/3'u ve max 4 saat mantiginda mi bak.
4. Bunlar temizse sandik ekranini toparla:
   - Gem logu/son odul kalabaligini azalt.
   - Boya gruplarini `Siradan Boya Kutulari`, `Nadir Boya Kutulari`, `Ozel Boya Kutulari` olarak net goster.

## Yeni Codex'e Ilk Komut

Asagidaki komutu yeni Codex'e aynen yaz:

`C:\Users\akkus\Documents\kahve-oyunu\CODEX_BASLANGIC.md dosyasini oku, sonra projeyi C:\Users\akkus\Documents\kahve-oyunu yolundan acip DEVAM_NOTLARI.md ve DIGER_BILGISAYAR_DEVIR_NOTU.md ile kaldigimiz yerden devam et. Once git durumunu ve ADB cihaz baglantisini kontrol et.`
