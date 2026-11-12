using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Policy : SoftDeleteEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Birim canvas'tan silinince (UnitSyncManager hard-delete) null'a çekilir — poliçe kapanır
    // ama ProjectId sayesinde proje bazında görünür kalır (bkz. knowledge/unit-sync-hard-delete.md).
    public int? ProjectUnitId { get; set; }
    public ProjectUnit? ProjectUnit { get; set; }

    // Poliçenin IsActive=false olma nedenini ayırt eder: true = bağlı birim silindiği için
    // otomatik kapandı, false = elle iptal edildi/yenilendi.
    public bool IsUnitDeleted { get; set; }

    public int? InsuranceCompanyId { get; set; }
    public InsuranceCompany? InsuranceCompany { get; set; }

    public string? PolicyNumber { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
