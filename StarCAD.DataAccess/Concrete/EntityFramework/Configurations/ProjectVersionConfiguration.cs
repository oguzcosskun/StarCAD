using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class ProjectVersionConfiguration : IEntityTypeConfiguration<ProjectVersion>
{
    public void Configure(EntityTypeBuilder<ProjectVersion> builder)
    {
        builder.ToTable("ProjectVersions");

        builder.Property(x => x.JsonToken).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200);
        builder.Property(x => x.ContentHash).HasMaxLength(32);

        builder.HasIndex(x => x.ProjectId);
    }
}