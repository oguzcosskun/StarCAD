using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Role : SoftDeleteEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}