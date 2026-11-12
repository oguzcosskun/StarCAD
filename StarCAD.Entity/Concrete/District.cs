using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class District : BaseEntity
{
    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public string Name { get; set; } = null!;
}
