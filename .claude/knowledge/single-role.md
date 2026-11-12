# decision: Tek rol — User.RoleId (UserRole N:N kaldırıldı)

Bir kullanıcının **tam bir** rolü vardır: `user.User.RoleId → user.Role` doğrudan FK.
Eski `UserRole` N:N junction **kaldırıldı**
(`refactor(entity): replace UserRole join table with direct User-Role FK`).

## Kurallar

- Üç rol, birbirini dışlar (firma tarafına göre): **Admin / GasDistributionUser /
  ProjectFirmUser**. `user.User`'daki CHECK: aynı anda hem `ProjectFirmId` hem
  `GasDistributionFirmId` dolu olamaz (görevler ayrılığı).
- Yetkiler **rolün içinde** tanımlanır. İleride "proje firması yöneticisi" gibi bir ihtiyaç
  çıksa bile bu **tek yeni rol** olur — ikinci bir rol ataması değil.
- N:N rol bağını **yeni açık karar olmadan geri ekleme.**

## Rol kontrolü

Kırılgan `Id` sabiti veya `Name` metniyle değil, `Role.Code` (Admin/GasDistributionUser/
ProjectFirmUser) sabitine dayan.
