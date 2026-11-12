using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;
using StarCAD.Common.Constants;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class CodeGroupConfiguration : IEntityTypeConfiguration<CodeGroup>
{
    public void Configure(EntityTypeBuilder<CodeGroup> builder)
    {
        builder.ToTable("CodeGroups");

        builder.Property(x => x.GroupName).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.GroupName).IsUnique();

        builder.HasData(
            new CodeGroup { Id = 1,  GroupName = CodeGroupNames.ArchitecturalPaletteTool, IsActive = true },
            new CodeGroup { Id = 2,  GroupName = CodeGroupNames.ProjectStatus,            IsActive = true },
            new CodeGroup { Id = 3,  GroupName = CodeGroupNames.InstallationPaletteTool,  IsActive = true },
            new CodeGroup { Id = 5,  GroupName = CodeGroupNames.DocumentType,             IsActive = true },
            new CodeGroup { Id = 7,  GroupName = CodeGroupNames.ProjectType,              IsActive = true },
            new CodeGroup { Id = 8,  GroupName = CodeGroupNames.HeatingType,              IsActive = true },
            new CodeGroup { Id = 9,  GroupName = CodeGroupNames.BuildingUsageType,        IsActive = true },
            new CodeGroup { Id = 32, GroupName = CodeGroupNames.ServiceBoxModel,          IsActive = true });
    }
}