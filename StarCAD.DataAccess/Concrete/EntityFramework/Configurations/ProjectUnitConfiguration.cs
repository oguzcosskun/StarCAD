using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class ProjectUnitConfiguration : IEntityTypeConfiguration<ProjectUnit>
{
    public void Configure(EntityTypeBuilder<ProjectUnit> builder)
    {
        builder.ToTable("ProjectUnits");

        builder.Property(x => x.UnitNumber).HasMaxLength(50);
        builder.Property(x => x.SubscriberNo).HasMaxLength(50);
        builder.Property(x => x.SubscriberName).HasMaxLength(200);
        builder.Property(x => x.MeterClassLabel).HasMaxLength(20);
        builder.Property(x => x.PipeTypeName).HasMaxLength(50);
        builder.Property(x => x.FlowCubicMeterPerHour).HasPrecision(10, 2);

        builder.HasIndex(x => x.ProjectId);

        // Bir projede aynı sayaç elemanı iki birime eşleşemez (UnitSyncManager'ın dayandığı garanti).
        builder.HasIndex(x => new { x.ProjectId, x.SourceMeterElementId })
            .IsUnique()
            .HasFilter("[SourceMeterElementId] IS NOT NULL");
    }
}
