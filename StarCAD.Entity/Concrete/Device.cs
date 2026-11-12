using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

// Canvas JSON senkronu — bkz. UnitSyncManager. Stabil bir id/eşleştirme anahtarı YOK: her
// "Kaydet"te bir birimin tüm cihazları silinip unitReport'taki güncel listeden yeniden yazılır
// (wholesale replace). BaseEntity (soft-delete değil) — hiçbir dış kayıt Device.Id'ye referans
// vermiyor, bu satırların türetilmiş/geçici olmanın ötesinde kalıcı bir değeri yok.
public class Device : BaseEntity
{
    public int ProjectUnitId { get; set; }
    public ProjectUnit ProjectUnit { get; set; } = null!;
    public string? DeviceType { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Capacity { get; set; }
    public decimal? FlowCubicMeterPerHour { get; set; }
    public string? FlueLabel { get; set; }
}
