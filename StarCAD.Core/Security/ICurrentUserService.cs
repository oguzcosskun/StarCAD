namespace StarCAD.Core.Security;

/// <summary>
/// İsteği yapan kullanıcının kimliğini iş katmanına taşır. Business, HttpContext'i
/// göremez (ASP.NET bağımlılığı yok) — bu arayüz tek köprüdür.
/// Implementasyon Api katmanındadır; bkz. IFileStorageService ile aynı desen.
///
/// HTTP dışı çağrılarda (seeder, zamanlanmış görev) tüm değerler null döner.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Username { get; }
    string? RoleCode { get; }
    int? ProjectFirmId { get; }
    int? GasDistributionFirmId { get; }
}