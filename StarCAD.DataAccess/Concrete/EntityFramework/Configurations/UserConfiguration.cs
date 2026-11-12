using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TokenVersion).HasDefaultValue(0);

        builder.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProjectFirm)
            .WithMany()
            .HasForeignKey(x => x.ProjectFirmId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GasDistributionFirm)
            .WithMany()
            .HasForeignKey(x => x.GasDistributionFirmId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Users", t => t.HasCheckConstraint(
            "CK_Users_SingleFirmReference",
            "[ProjectFirmId] IS NULL OR [GasDistributionFirmId] IS NULL"));
    }
}