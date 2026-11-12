using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class InsuranceCompanyConfiguration : IEntityTypeConfiguration<InsuranceCompany>
{
    public void Configure(EntityTypeBuilder<InsuranceCompany> builder)
    {
        builder.ToTable("InsuranceCompanies");

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();

        builder.HasData(
            new InsuranceCompany { Id = 1, Title = "Anadolu Sigorta", IsActive = true },
            new InsuranceCompany { Id = 2, Title = "Allianz Sigorta", IsActive = true },
            new InsuranceCompany { Id = 3, Title = "Aksigorta", IsActive = true },
            new InsuranceCompany { Id = 4, Title = "AXA Sigorta", IsActive = true },
            new InsuranceCompany { Id = 5, Title = "Türkiye Sigorta", IsActive = true },
            new InsuranceCompany { Id = 6, Title = "HDI Sigorta", IsActive = true },
            new InsuranceCompany { Id = 7, Title = "Ray Sigorta", IsActive = true },
            new InsuranceCompany { Id = 8, Title = "Sompo Sigorta", IsActive = true },
            new InsuranceCompany { Id = 9, Title = "Zurich Sigorta", IsActive = true },
            new InsuranceCompany { Id = 10, Title = "Mapfre Sigorta", IsActive = true });
    }
}
