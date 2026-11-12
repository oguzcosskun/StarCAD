using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PlateCode).HasMaxLength(2);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
