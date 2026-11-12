using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class ProjectFirmAuthorization : SoftDeleteEntity
{
    public int ProjectFirmId { get; set; }
    public ProjectFirm ProjectFirm { get; set; } = null!;

    public int GasDistributionFirmId { get; set; }
    public GasDistributionFirm GasDistributionFirm { get; set; } = null!;

    public string CertificateNumber { get; set; } = null!;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}