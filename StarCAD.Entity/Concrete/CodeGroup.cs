using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class CodeGroup : SoftDeleteEntity
{
    public string GroupName { get; set; } = null!;

    public ICollection<Code> Codes { get; set; } = new List<Code>();
}
