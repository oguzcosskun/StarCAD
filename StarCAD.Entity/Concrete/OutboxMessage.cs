using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

/// <summary>
/// Append-only kuyruk: SQL yazımıyla aynı transaction'da eklenir, arka plan worker'ı okuyup
/// Elasticsearch'e yazdıktan sonra <see cref="ProcessedAt"/>'i işaretler. Soft-delete'e tabi
/// değil (bkz. SoftDeleteEntity — append-only log tabloları doğrudan BaseEntity'den türer).
/// </summary>
public class OutboxMessage : BaseEntity
{
    public string EntityType { get; set; } = null!;
    public int EntityId { get; set; }
    public string Operation { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}