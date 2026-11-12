namespace StarCAD.ViewModel.ProjectUnits;

public sealed class ProjectUnitDeviceSummaryDto
{
    public int Id { get; init; }
    public string? DeviceType { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Capacity { get; init; }
    public decimal? FlowCubicMeterPerHour { get; init; }
    public string? FlueLabel { get; init; }
}

public sealed class ProjectUnitListItemDto
{
    public int Id { get; init; }

    // unitReport'tan senkronize — kimlik alanları.
    public string? UnitNumber { get; init; }
    public string? SubscriberName { get; init; }
    public string? SubscriberNo { get; init; }
    public string? MeterClassLabel { get; init; }
    public int? Area { get; init; }
    public decimal? FlowCubicMeterPerHour { get; init; }
    public int? PressureMbar { get; init; }
    public string? PipeTypeName { get; init; }
    public List<ProjectUnitDeviceSummaryDto> Devices { get; init; } = [];
}
