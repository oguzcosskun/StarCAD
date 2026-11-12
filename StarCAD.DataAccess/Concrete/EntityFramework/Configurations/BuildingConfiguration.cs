using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        
        builder.Property(x => x.AddressLine).HasMaxLength(500);
        builder.Property(x => x.BlockLotParcel).HasMaxLength(50);
        builder.Property(x => x.Code).HasMaxLength(50);
        
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL AND [IsActive] = 1");
    }
}