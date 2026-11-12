using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Code : SoftDeleteEntity
{
    public int GroupId { get; set; }
    public CodeGroup Group { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string CodeValue { get; set; } = null!;
}
