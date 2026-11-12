using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class City : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? PlateCode { get; set; }

    public ICollection<District> Districts { get; set; } = new List<District>();
}
