using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        // Aynı ilde aynı ilçe adı iki kez olamaz; farklı illerde olabilir (her ilde bir "Merkez" var).
        builder.HasIndex(x => new { x.CityId, x.Name }).IsUnique();
    }
}
