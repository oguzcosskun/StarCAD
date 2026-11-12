using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class ProjectFirmConfiguration : IEntityTypeConfiguration<ProjectFirm>
{
    public void Configure(EntityTypeBuilder<ProjectFirm> builder)
    {
        // 1=Şahıs → NationalIdNumber zorunlu, 2=Tüzel → TaxNumber zorunlu
        builder.ToTable("ProjectFirms", t =>
            t.HasCheckConstraint(
                "CK_ProjectFirm_TypeIdentity",
                "([CompanyType] = 1 AND [NationalIdNumber] IS NOT NULL) OR " +
                "([CompanyType] = 2 AND [TaxNumber] IS NOT NULL)"));

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TaxNumber).HasMaxLength(11);
        builder.Property(x => x.NationalIdNumber).HasMaxLength(256);   // varbinary(256), şifreli (KVKK)
        builder.Property(x => x.NationalIdNumberHash).HasMaxLength(32); // varbinary(32), HMAC-SHA256

        // Aynı TC ile iki AKTİF şahıs firma olamaz. Filtered: tüzel firmada hash NULL; ayrıca
        // IsActive=1 → soft-delete edilen firmanın TC'si yeniden kullanılabilir + app pre-check
        // (IsActive query filter'lı) DB index'iyle hizalı kalır.
        builder.HasIndex(x => x.NationalIdNumberHash)
            .IsUnique()
            .HasFilter("[NationalIdNumberHash] IS NOT NULL AND [IsActive] = 1");

        builder.Property(x => x.AccountingCode).HasMaxLength(50);
        builder.Property(x => x.ContactPerson).HasMaxLength(150);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Phone2).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(500);
    }
}
