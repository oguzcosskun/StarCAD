using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class ProjectVersion : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string JsonToken { get; set; } = null!;
    public string? Label { get; set; }

    // Tüm rawJson'un MD5'i (hex). Bir önceki versiyonla aynıysa UnitSyncManager çağrısı atlanır.
    public string? ContentHash { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; }
}
