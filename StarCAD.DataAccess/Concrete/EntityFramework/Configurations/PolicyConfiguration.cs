using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.Property(p => p.PolicyNumber).HasMaxLength(50);
        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nullable: birim hard-delete edilince UnitSyncManager bunu null'a çeker (bkz.
        // knowledge/unit-sync-hard-delete.md) — FK yine de Restrict, nulling app katmanında yapılır.
        builder.HasOne(p => p.ProjectUnit)
            .WithMany()
            .HasForeignKey(p => p.ProjectUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.InsuranceCompany)
            .WithMany()
            .HasForeignKey(p => p.InsuranceCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.ProjectUnitId);
    }
}