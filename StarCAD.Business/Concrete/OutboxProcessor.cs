using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StarCAD.Common.Constants;
using StarCAD.Core.Search;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;

namespace StarCAD.Business.Concrete;

/// <summary>
/// Outbox tablosunu periyodik okuyup Elasticsearch'e yansıtan arka plan servisi.
/// StarCADContext scoped olduğu için doğrudan enjekte edilmez — her turda IServiceScopeFactory
/// ile yeni bir scope açılır (singleton bir BackgroundService'in scoped bağımlılık tüketmesinin
/// tek doğru yolu).
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox işleme turu başarısız oldu.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StarCADContext>();
        var search = scope.ServiceProvider.GetRequiredService<ISearchService>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.Id)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(db, search, message, ct);
                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // İşaretlenmez: ProcessedAt null kalır, bir sonraki turda tekrar denenir.
                _logger.LogWarning(ex,
                    "Outbox mesajı işlenemedi, tekrar denenecek: {EntityType} {EntityId}",
                    message.EntityType, message.EntityId);
            }
        }

        if (messages.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private static async Task ProcessMessageAsync(
        StarCADContext db, ISearchService search, OutboxMessage message, CancellationToken ct)
    {
        var (indexName, document) = message.EntityType switch
        {
            nameof(Project) => ("projects", await LoadProjectDocumentAsync(db, message.EntityId, ct)),
            nameof(ProjectFirm) => ("project-firms", await LoadProjectFirmDocumentAsync(db, message.EntityId, ct)),
            nameof(GasDistributionFirm) => ("gas-distribution-firms", await LoadGasDistributionFirmDocumentAsync(db, message.EntityId, ct)),
            nameof(User) => ("users", await LoadUserDocumentAsync(db, message.EntityId, ct)),
            _ => throw new InvalidOperationException($"Bilinmeyen outbox varlık tipi: {message.EntityType}")
        };

        var documentId = message.EntityId.ToString();

        // Silme işlemiyse, ya da satır artık aktif değilse (arada soft-delete olmuş olabilir) index'ten kaldır.
        if (message.Operation == "Delete" || document is null)
            await search.DeleteDocumentAsync(indexName, documentId, ct);
        else
            await search.IndexDocumentAsync(indexName, documentId, document, ct);
    }

    // ProjectListItemDto ile birebir aynı alan seti — kullanıcı listede neyi görüyorsa aramada da
    // onu bulabilmeli (bkz. StarCAD.ViewModel.Projects.ProjectListItemDto).
    private static Task<object?> LoadProjectDocumentAsync(StarCADContext db, int id, CancellationToken ct) =>
        db.Projects.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                BuildingCode = p.Building != null ? p.Building.Code : null,
                Status = p.StatusCode != null ? p.StatusCode.CodeValue : CodeValues.Draft,
                StatusName = p.StatusCode != null ? p.StatusCode.Name : "Taslak",
                ProjectTypeName = p.ProjectTypeCode != null ? p.ProjectTypeCode.Name : null,
                HeatingTypeName = p.HeatingTypeCode != null ? p.HeatingTypeCode.Name : null,
                FirmName = p.ProjectFirmAuthorization.ProjectFirm.Title,
                GasDistributionFirmName = p.ProjectFirmAuthorization.GasDistributionFirm.Title,
                ProjectFirmId = p.ProjectFirmAuthorization.ProjectFirmId.ToString(),
                GasDistributionFirmId = p.ProjectFirmAuthorization.GasDistributionFirmId.ToString()
            } as object)
            .FirstOrDefaultAsync(ct);

    // ProjectFirmListItemDto ile birebir aynı alan seti.
    private static Task<object?> LoadProjectFirmDocumentAsync(StarCADContext db, int id, CancellationToken ct) =>
        db.ProjectFirms.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new { f.Id, f.Title, f.ContactPerson, f.Phone, f.Email, f.TaxNumber } as object)
            .FirstOrDefaultAsync(ct);

    // GasDistributionFirmListItemDto ile birebir aynı alan seti.
    private static Task<object?> LoadGasDistributionFirmDocumentAsync(StarCADContext db, int id, CancellationToken ct) =>
        db.GasDistributionFirms.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new { f.Id, f.Title, f.CompanyNumber, GroupName = f.Group != null ? f.Group.Name : null } as object)
            .FirstOrDefaultAsync(ct);

    // UserListItemDto ile birebir aynı alan seti. PasswordHash BİLEREK projekte edilmiyor —
    // arama index'ine asla girmemeli.
    private static Task<object?> LoadUserDocumentAsync(StarCADContext db, int id, CancellationToken ct) =>
        db.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Username,
                u.Phone,
                RoleName = u.Role.Name,
                ProjectFirmName = u.ProjectFirm != null ? u.ProjectFirm.Title : null,
                GasDistributionFirmName = u.GasDistributionFirm != null ? u.GasDistributionFirm.Title : null,
                ProjectFirmId = u.ProjectFirmId != null ? u.ProjectFirmId.ToString() : null,
                GasDistributionFirmId = u.GasDistributionFirmId != null ? u.GasDistributionFirmId.ToString() : null
            } as object)
            .FirstOrDefaultAsync(ct);
}