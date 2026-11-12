using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class GasDistributionFirm : SoftDeleteEntity
{
    public int? GroupId { get; set; }
    public GasDistributionGroup? Group { get; set; }

    public string Title { get; set; } = null!;
    public int CompanyNumber { get; set; }
    public string? Description { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public ICollection<ProjectFirmAuthorization> ProjectFirmAuthorizations { get; set; } = new List<ProjectFirmAuthorization>();
}