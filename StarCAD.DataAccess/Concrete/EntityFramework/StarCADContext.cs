using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StarCAD.Core.Entities.Concrete;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Concrete.EntityFramework;

public class StarCADContext : DbContext
{
    public StarCADContext(DbContextOptions<StarCADContext> options) : base(options)
    {
    }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    // Lookup
    public DbSet<CodeGroup> CodeGroups => Set<CodeGroup>();
    public DbSet<Code> Codes => Set<Code>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<InsuranceCompany> InsuranceCompanies => Set<InsuranceCompany>();

    // Firm
    public DbSet<ProjectFirm> ProjectFirms => Set<ProjectFirm>();
    public DbSet<ProjectFirmAuthorization> ProjectFirmAuthorizations => Set<ProjectFirmAuthorization>();
    public DbSet<GasDistributionGroup> GasDistributionGroups => Set<GasDistributionGroup>();
    public DbSet<GasDistributionFirm> GasDistributionFirms => Set<GasDistributionFirm>();

    // Project
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectVersion> ProjectVersions => Set<ProjectVersion>();
    public DbSet<ProjectUnit> ProjectUnits => Set<ProjectUnit>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<OperationHistory> OperationHistories => Set<OperationHistory>();

    // Insurance & Doc
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Doc> Docs => Set<Doc>();
    public DbSet<DocProjectUnit> DocProjectUnits => Set<DocProjectUnit>();

    // elastic
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) Tüm FK'lere varsayılan Restrict. Soft-delete kullandığımız için gerçek cascade
        //    delete'e ihtiyacımız yok; ayrıca SQL Server'ın "multiple cascade paths" hatasını
        //    da engeller. Config dosyalarındaki açık .OnDelete(...) çağrıları (ör. SetNull)
        //    bu döngüden SONRA çalışan ApplyConfigurationsFromAssembly ile üzerine yazılır.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StarCADContext).Assembly);

        // 2) SoftDeleteEntity'den türeyen her tabloya otomatik global query filter (IsActive == true).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var body = Expression.Equal(
                Expression.Property(parameter, nameof(SoftDeleteEntity.IsActive)),
                Expression.Constant(true));

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
        
        modelBuilder.Entity<OperationHistory>()
            .HasQueryFilter(oh => oh.Project.IsActive && oh.OperationCode.IsActive);

        modelBuilder.Entity<ProjectVersion>()
            .HasQueryFilter(pv => pv.Project.IsActive);

        modelBuilder.Entity<DocProjectUnit>()
            .HasQueryFilter(l => l.Doc.IsActive);

        modelBuilder.Entity<Device>()
            .HasQueryFilter(d => d.ProjectUnit.IsActive);

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.IsActive && u.Role.IsActive);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteAndAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplySoftDeleteAndAudit();
        return base.SaveChanges();
    }

    private void ApplySoftDeleteAndAudit()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Project>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ProjectVersion>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
        }
        // Append-only günlük: satır yalnızca eklenir, CreatedAt'i yazan koda bırakılmaz.
        foreach (var entry in ChangeTracker.Entries<OperationHistory>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = utcNow;
        }

        // Delete çağrılan soft-delete'li kayıt fiziksel silinmez, pasifleştirilir.
        foreach (var entry in ChangeTracker.Entries<SoftDeleteEntity>())
        {
            if (entry.State != EntityState.Deleted)
                continue;

            entry.State = EntityState.Modified;
            entry.Entity.IsActive = false;
        }
    }

    /// <summary>
    /// Yalnızca arama index'ine yansıyan 4 tablo (Project/ProjectFirm/GasDistributionFirm/User)
    /// için Manager'ların isteyerek çağırdığı kaydetme yolu. Normal SaveChanges'e dokunmaz —
    /// diğer tüm tablolar (Code, City, Role, Policy...) bu metottan hiç geçmez, davranışları değişmez.
    ///
    /// İki adımlı, tek transaction: önce ana veri kaydedilir (Added kayıtlarda Id burada atanır),
    /// sonra outbox satırı aynı transaction içinde eklenir. Süreç ikisi arasında çökerse transaction
    /// hiç commit olmaz — ne ana veri ne outbox mesajı yarım kalmış halde kalmaz.
    /// </summary>
    public async Task<int> SaveChangesWithOutboxAsync(
        string entityType, string operation, Func<int> resolveEntityId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        var result = await SaveChangesAsync(cancellationToken);

        OutboxMessages.Add(new OutboxMessage
        {
            EntityType = entityType,
            EntityId = resolveEntityId(),
            Operation = operation,
            CreatedAt = DateTime.UtcNow
        });
        await base.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}