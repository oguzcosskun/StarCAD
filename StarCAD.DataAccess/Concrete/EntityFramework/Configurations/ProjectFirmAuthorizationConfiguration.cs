using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class ProjectFirmAuthorizationConfiguration : IEntityTypeConfiguration<ProjectFirmAuthorization>
{
    public void Configure(EntityTypeBuilder<ProjectFirmAuthorization> builder)
    {
        builder.ToTable("ProjectFirmAuthorizations", t =>
            t.HasCheckConstraint(
                "CK_ProjectFirmAuthorization_Date",
                "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]"));

        builder.Property(x => x.CertificateNumber).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.ProjectFirmId, x.GasDistributionFirmId })
            .IsUnique()
            .HasFilter("[IsActive] = 1");
        
        builder.HasOne(x => x.GasDistributionFirm)
            .WithMany(x => x.ProjectFirmAuthorizations)   
            .HasForeignKey(x => x.GasDistributionFirmId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
    
}