using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class ProjectFirm : SoftDeleteEntity
{
    public byte CompanyType { get; set; }
    public string Title { get; set; } = null!;
    public string? TaxNumber { get; set; }
    public byte[]? NationalIdNumber { get; set; }
    // Deterministik HMAC(TC) — benzersizlik (UNIQUE) kontrolü için. Şifreli NationalIdNumber
    // randomize olduğundan UNIQUE'lenemez; bu kolon aynı TC → aynı hash sağlar.
    public byte[]? NationalIdNumberHash { get; set; }
    public string? AccountingCode { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public string? Address { get; set; }

    public ICollection<ProjectFirmAuthorization> ProjectFirmAuthorizations { get; set; } = new List<ProjectFirmAuthorization>();
}
