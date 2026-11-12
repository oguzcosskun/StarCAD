namespace StarCAD.Business.Abstract;

/// <summary>
/// Canvas JSON'undaki `unitReport` bölümünü `ProjectUnit` tablosuyla senkronize eder.
/// Çağıran (ProjectVersionManager) aynı StarCADContext'i paylaşır; bu servis kendi
/// SaveChanges'ini çağırmaz, çağıranın transaction'ına dahil olur.
/// </summary>
public interface IUnitSyncService
{
    Task SyncAsync(int projectId, string rawJson, CancellationToken cancellationToken = default);
}
