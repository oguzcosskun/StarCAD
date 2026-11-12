---
type: gotcha
date: 2026-08
---

# Migration geçmişini yeniden yazmak herkesin yerel veritabanını bozar

`a75c7e8` (19 Ağu) bütün migration'ları tek bir `20260819140648_InitialCreate`'e
squash etti. Yayın öncesi bunu yapmak doğru — 24 migration'ın içinde
"önceki düzeltmenin düzeltmesi" olanlar birikmişti ve canlıya çıktıktan sonra
imkânsız olurdu. **Ama mevcut her yerel veritabanını kırar.**

## Neden kırılıyor

`__EFMigrationsHistory` eski kimlikleri taşır, yeni `InitialCreate` kimliği
listede yoktur. EF onu **hiç uygulanmamış** sayar ve baştan `CREATE TABLE`
üretir → `SqlException 2714: There is already an object named 'Cities'` →
`Program.cs`'teki `MigrateAsync` `app.Run()`'dan ÖNCE patlar.

## ⚠️ Belirti sebebi HİÇ ele vermiyor

Frontend'de görünen şey: **`net::ERR_CONNECTION_RESET`**. Ağ/CORS sorunu değil —
konteynerin içinde 8080'i dinleyen kimse yok, Docker'ın port proxy'si TCP'yi
kabul edip resetliyor.

⚠️ **`docker ps` "Up" gösterir.** PID 1 `dotnet` ayakta kalıyor. Teşhisin tek
doğru yeri `docker compose logs api`; konteyner içinde `/proc/net/tcp`'de 8080
yoksa dinleyici yoktur.

Bu zincir (ERR_CONNECTION_RESET → konteyner çöküşü → migration squash) kimsenin
kendiliğinden kuracağı bir şey değil. Bir frontend geliştiricisinin yarım saatine
mal oldu.

## Kod artık bunu söylüyor

`MigrationHistoryCheck` (StarCAD.DataAccess/Diagnostics) veritabanında
uygulanmış görünen ama **bu derlemede karşılığı olmayan** migration'ları bulur.
`Program.cs` migrate'ten önce uyarır, migrate düşerse ham yığın izi yerine
çözümü içeren `InvalidOperationException` fırlatır (orijinal `SqlException` iç
istisna olarak korunur).

Neden önden BLOKLAMIYOR: dal değiştirmiş bir geliştiricide fazladan bir
migration bulunabilir ve migration yine de sorunsuz uygulanabilir. Önden hata
verilseydi o durumda çalışan bir ortam durdurulurdu. Bu yüzden desen
"önce uyar, gerçekten düşerse açıkla".

## Bir dahaki sefere

Migration geçmişini yeniden yazan değişiklik:

1. **Ayrı commit** olsun — feature commit'inin son satırına gömülmesin.
2. Mesajda **sonucu** yazsın, yapılanı değil: "bu mevcut yerel veritabanlarını
   bozar, README > 8.1'i uygulayın". *"Squash migration history"* ifadesi repo
   hakkında bir cümledir, okuyanı harekete geçirmez.
3. Ekibe haber verilsin.

Sıfırlama yordamı: README > **8.1 Yerel veritabanını sıfırlama**
(yalnız SQL'i düşürür, MinIO/Redis'e dokunmaz).
