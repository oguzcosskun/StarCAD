using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class ProjectUnit : SoftDeleteEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string? UnitNumber { get; set; }
    public string? SubscriberNo { get; set; }

    public int? Area { get; set; }

    // Canvas JSON senkronu — bkz. UnitSyncManager. Eşleştirme anahtarı: installationElements'teki
    // gasMeter'ın id'si, proje genelinde nextUniqueId'den geldiği için tekil (frontend garantisi).
    public int? SourceMeterElementId { get; set; }

    public string? SubscriberName { get; set; }
    public string? MeterClassLabel { get; set; }
    public decimal? FlowCubicMeterPerHour { get; set; }
    public int? PressureMbar { get; set; }
    public string? PipeTypeName { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
