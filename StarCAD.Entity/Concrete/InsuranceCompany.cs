using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class InsuranceCompany : SoftDeleteEntity
{
    public string Title { get; set; } = null!;
}
