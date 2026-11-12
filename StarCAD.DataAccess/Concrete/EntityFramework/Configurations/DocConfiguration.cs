using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework.Configurations;

public class DocConfiguration : IEntityTypeConfiguration<Doc>
{
    public void Configure(EntityTypeBuilder<Doc> builder)
    {
        builder.ToTable("Docs");

        builder.Property(x => x.FileName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FilePath).HasMaxLength(500);
        builder.Property(x => x.ContentType).HasMaxLength(100);

        // Yükleyen kullanıcı silinirse evrak öksüz kalmaz, sadece atıf boşa düşer (§3 atıf/SetNull istisnası).
        builder.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Havuzdaki (ProjectId=null) evrakın firma bağını korur; yükleyen kullanıcı silinse de kalıcıdır.
        builder.HasOne(x => x.ProjectFirm)
            .WithMany()
            .HasForeignKey(x => x.ProjectFirmId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.ProjectFirmId);
    }
}
