---
name: ef-core-config
description: EF Core entity konfigürasyonu + migration eklerken/değiştirirken oku. Yeni entity, IEntityTypeConfiguration, HasMaxLength/ToTable/HasIndex.IsUnique/HasCheckConstraint/OnDelete, RowVersion veya migration alma işlerinde gerekli. NVARCHAR(MAX)/eksik UNIQUE/eksik CHECK/multiple cascade paths hatalarını önler.
---

# EF Core Konfigürasyon & Migration

Şema kararları **DataAccess** katmanında Fluent API ile tanımlanır — entity'lere data annotation
(`[MaxLength]`, `[Required]`) **konmaz**. Entity saf POCO kalır. Kolon-kolon kaynak: tasarım
workspace'indeki `StarCAD_Database.sql` + `veritabani-rehberi.md` §5/§6 (yalnızca tip/kısıt
içeriği; ad/şema değil — aşağıya bkz.).

## Nereye yazılır

- Konfigürasyon sınıfları: `StarCAD.DataAccess/Concrete/EntityFramework/Configurations/` altında
  (**düz** — modül alt-klasörü YOK) entity başına bir `public class XxxConfiguration :
  IEntityTypeConfiguration<Xxx>`.
- `StarCADContext.OnModelCreating` bunları `ApplyConfigurationsFromAssembly(...)` ile toplar
  (zaten çağrılıyor). Global Restrict döngüsü ve query filter'lar da orada — onları bozma.

## Config kalıbı (kontrol listesi)

Her entity için:

- **Tablo adı:** `builder.ToTable("Projects");` — EF varsayılanı: DbSet'e göre **çoğul**, **şema
  KULLANILMAZ** (hepsi `dbo`). Modül şeması (common/company…) benimsenmedi — bkz.
  knowledge/db-conventions.md.
- **String uzunluğu:** her string alana `builder.Property(x => x.Email).HasMaxLength(255).IsRequired();`
  — **asla MAX** (bkz. knowledge/nvarchar-length-caps.md). NOT NULL olanlara `.IsRequired()`.
- **UNIQUE index:** `builder.HasIndex(x => x.Email).IsUnique();`
  bileşik: `builder.HasIndex(x => new { x.GroupId, x.CodeValue }).IsUnique();`
- **CHECK:** `builder.ToTable("Users", t => t.HasCheckConstraint("CK_User_FirmExclusive",
  "[ProjectFirmId] IS NULL OR [GasDistributionFirmId] IS NULL"));`
- **FK + cascade:** tüm FK'ler `StarCADContext.OnModelCreating`'deki global döngüde `Restrict`'e
  çekiliyor → config'te ayrıca yazmana gerek yok. Yalnızca **istisna** için açıkça yaz: atıf
  `Doc.UploadedByUserId` → `.OnDelete(SetNull)` (bkz. knowledge/cascade-restrict.md).
- **RowVersion:** `builder.Property(x => x.RowVersion).IsRowVersion();`

## Örnek

```csharp
public class ProjectFirmRegionConfiguration : IEntityTypeConfiguration<ProjectFirmRegion>
{
    public void Configure(EntityTypeBuilder<ProjectFirmRegion> builder)
    {
        builder.ToTable("ProjectFirmRegions", t =>
            t.HasCheckConstraint("CK_ProjectFirmRegion_Date",
                "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));

        builder.Property(x => x.CertificateNumber).HasMaxLength(50).IsRequired();

        // Aynı firma aynı bölgede iki kez kaydedilemez
        builder.HasIndex(x => new { x.ProjectFirmId, x.RegionId }).IsUnique();
    }
}
```

## Project → User üç FK (özel dikkat)

`Project`'in User'a üç FK'si var (`OwnerUserId`, `CreatedByUserId`, `UpdatedByUserId`). Global
döngü hepsini Restrict yaptığı için ek config gerekmez; multiple cascade paths de çıkmaz (hepsi
Restrict). Bkz. knowledge/project-audit-exceptions.md.

## Migration akışı

1. Config'leri yaz/güncelle.
2. `dotnet ef migrations add <AnlamlıAd> -p StarCAD.DataAccess -s StarCAD.Api`
3. Üretilen migration'ı **oku ve doğrula:** `maxLength:` dolu mu, `unique: true` index'ler var mı,
   `AddCheckConstraint` çağrıları var mı, tablolar **`dbo`'da + çoğul adla** mı (`schema:` satırı
   OLMAMALI), `Down()` geri-alınabilir mi. (Convention-only'e düşme — bkz. knowledge/schema-config-gap.md.)
4. `dotnet ef database update -p StarCAD.DataAccess -s StarCAD.Api`
5. Üretileni `StarCAD_Database.sql` ile karşılaştır — **kolon tipi/uzunluk/CHECK/unique** eşleşmeli.
   (Not: tasarım SQL'i modül şeması + tekil ad kullanır; kod `dbo` + çoğul kullanır — bu ad/şema
   farkı bilinçli, yalnızca kısıt/tip içeriğini kıyasla.) Sapma varsa config'i düzelt, migration'ı
   yeniden al (dev'de eskisini silip yeniden üretmek serbest; paylaşıldıysa yeni düzeltici migration ekle).

## Yaygın hatalar

- **"multiple cascade paths"** → global Restrict döngüsü bozulmuş / bir FK explicit Cascade olmuş.
- **`nvarchar(max)` üretildi** → o property'e `HasMaxLength` verilmemiş.
- **UNIQUE `nvarchar(max)`'a atılamıyor** → önce uzunluk ver, sonra `HasIndex().IsUnique()`.
- **Yanlışlıkla şema eklemek** → `ToTable("X","şema")` YAZMA; sadece `ToTable("Xs")`.
