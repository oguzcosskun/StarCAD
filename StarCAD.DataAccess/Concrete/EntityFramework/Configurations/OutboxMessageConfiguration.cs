using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(20).IsRequired();

        // Worker'ın "işlenmemiş mesajları getir" sorgusu bu indeksi kullanacak.
        builder.HasIndex(x => x.ProcessedAt);
    }
}