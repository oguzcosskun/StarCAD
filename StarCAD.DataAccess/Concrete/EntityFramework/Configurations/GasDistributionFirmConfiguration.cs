using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class GasDistributionFirmConfiguration : IEntityTypeConfiguration<GasDistributionFirm>
{
    public void Configure(EntityTypeBuilder<GasDistributionFirm> builder)
    {
        builder.ToTable("GasDistributionFirms");

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ContactPerson).HasMaxLength(150);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(500);

        builder.HasIndex(x => x.CompanyNumber).IsUnique();

        // Grup → firma: "Reference" FK, master satır korunur → Restrict (NO ACTION).
        builder.HasOne(x => x.Group)
               .WithMany(g => g.GasDistributionFirms)
               .HasForeignKey(x => x.GroupId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}