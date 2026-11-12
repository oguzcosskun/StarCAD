using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class GasDistributionGroupConfiguration : IEntityTypeConfiguration<GasDistributionGroup>
{
    public void Configure(EntityTypeBuilder<GasDistributionGroup> builder)
    {
        builder.ToTable("GasDistributionGroups");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}