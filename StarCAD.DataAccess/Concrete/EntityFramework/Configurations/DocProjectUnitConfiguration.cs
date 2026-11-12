using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class DocProjectUnitConfiguration : IEntityTypeConfiguration<DocProjectUnit>
{
    public void Configure(EntityTypeBuilder<DocProjectUnit> builder)
    {
        builder.ToTable("DocProjectUnits");

        builder.HasOne(x => x.Doc)
            .WithMany()
            .HasForeignKey(x => x.DocId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProjectUnit)
            .WithMany()
            .HasForeignKey(x => x.ProjectUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProjectFirm)
            .WithMany()
            .HasForeignKey(x => x.ProjectFirmId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı evrak aynı birime iki kez bağlanamaz.
        builder.HasIndex(x => new { x.DocId, x.ProjectUnitId }).IsUnique();
        builder.HasIndex(x => x.ProjectUnitId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.ProjectFirmId);
    }
}
