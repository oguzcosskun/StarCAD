using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class OperationHistoryConfiguration : IEntityTypeConfiguration<OperationHistory>
{
    public void Configure(EntityTypeBuilder<OperationHistory> builder)
    {
        builder.ToTable("OperationHistories");

        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.RoleSnapshot).HasMaxLength(50);
    }
}