using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class OperationHistory : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int OperationCodeId { get; set; }
    public Code OperationCode { get; set; } = null!;

    public string? Description { get; set; }
    public string? RoleSnapshot { get; set; }
    public DateTime CreatedAt { get; set; }
}
