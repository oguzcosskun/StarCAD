using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

// Doc <-> ProjectUnit many-to-many junction: tek bir evrak birden fazla bağımsız bölüme
// bağlanabilir (ör. birkaç daireyi kapsayan tek bir sözleşme). BaseEntity (soft-delete değil) —
// saf bir ilişki kaydı, kaynak (Doc/ProjectUnit) silinmeden bu satırın tek başına iş değeri yok.
public class DocProjectUnit : BaseEntity
{
    public int DocId { get; set; }
    public Doc Doc { get; set; } = null!;

    public int ProjectUnitId { get; set; }
    public ProjectUnit ProjectUnit { get; set; } = null!;

    // Denormalize: bağlantı kurulduğu andaki birimin proje/firma bağı. Bağlantılar hiç
    // güncellenmez, sadece silinip yeniden kurulur (bkz. DocManager.AssignAsync — proje
    // değişince tüm bağlantılar silinir) — o yüzden bu kopyanın bayatlama riski yok.
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int ProjectFirmId { get; set; }
    public ProjectFirm ProjectFirm { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
