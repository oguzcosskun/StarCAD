using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class GasDistributionGroup : SoftDeleteEntity
{
    public string Name { get; set; } = null!;

    public ICollection<GasDistributionFirm> GasDistributionFirms { get; set; }
        = new List<GasDistributionFirm>();
}