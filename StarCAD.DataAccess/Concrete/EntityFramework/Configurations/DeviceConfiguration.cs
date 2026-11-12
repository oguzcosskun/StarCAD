using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.Property(x => x.DeviceType).HasMaxLength(20);
        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.Capacity).HasMaxLength(50);
        builder.Property(x => x.FlueLabel).HasMaxLength(20);
        builder.Property(x => x.FlowCubicMeterPerHour).HasPrecision(10, 2);

        builder.HasIndex(x => x.ProjectUnitId);
    }
}
