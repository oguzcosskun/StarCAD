# convention: Config kapatma — her tabloya IEntityTypeConfiguration (feature başına)

İlk migration **convention-only** çıkmıştı: string'ler `NVARCHAR(MAX)`, UNIQUE/CHECK yok. Tablo
tablo `IEntityTypeConfiguration` yazılıp düzeltici migration alınarak kapatıldı.

## Durum — büyük ölçüde KAPANDI (2026-08 güncel)

`Configurations/` altında mevcut tüm tablolar için config yazıldı; convention-only açık kalan
tablo yok. Öne çıkanlar:
- ✅ **ProjectFirm + ProjectFirmRegion** (`ConfigureProjectFirm`): uzunluklar,
  `CK_ProjectFirm_TypeIdentity`, `CK_ProjectFirmRegion_Date`, `(ProjectFirmId,RegionId)` unique,
  `NationalIdNumber` → `varbinary(256)`.
- ✅ **ProjectVersion, Policy**.
- ✅ **User** — `Email`/`Username` UNIQUE + firma-exclusivity CHECK (`CK_User_...`) + uzunluklar.
- ✅ **Code / CodeGroup** — `Code(GroupId,CodeValue)` unique, `CodeGroup.GroupName` unique.
  (Not: eski `ProjectStatus` tablosu **kaldırıldı** — proje durumu artık `Code` Group 2; o satırdaki
  eski "ProjectStatus(StatusCode,StepCode) unique" ihtiyacı geçersiz.)
- ✅ Coğrafya/bina/cihaz/evrak/log tabloları (City/District/Region/Building/Device/Doc/
  OperationHistory/GasDistribution*) — config'li.

> Kural olarak açık: **yeni tablo eklenince** o tablonun config'i de aynı feature'da yazılır
> (aşağıdaki "Nasıl" akışı). Yeni bir tablo config'siz kalırsa bu bir regresyondur.

## Not: modül şeması bir EKSİK DEĞİL — karar

`dbo` + **çoğul** tablo adı **bilinçli** (bkz. knowledge/db-conventions.md). Tasarım SQL'i
şema/tekil kullanır ama kod dbo/çoğul benimsedi — **config yazarken şema EKLEME.**

## Nasıl kapatılır

Her tablo için `IEntityTypeConfiguration<T>` (HasMaxLength + gereken unique/check) + düzeltici
migration. Rehber: [[ef-core-config]]. Kolon kaynağı: `StarCAD_Database.sql` (yalnızca **tip/kısıt**
içeriği; ad/şema değil). Feature geldikçe o tablonun config'i de yazılır (feature başına).
