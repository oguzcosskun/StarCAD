namespace StarCAD.ViewModel.Admin;

public sealed class AdminDashboardDto
{
    public AdminDashboardSummaryDto Summary { get; init; } = new();
    public AdminDashboardTodayDto Today { get; init; } = new();
    public string DensityBy { get; init; } = string.Empty; // "group" | "region"
    public List<DensityItemDto> Density { get; init; } = [];
    public DateTime GeneratedAt { get; init; }
}

public sealed class AdminDashboardSummaryDto
{
    public int GasDistributionUserCount { get; init; }
    public int ProjectFirmCount { get; init; }
    public int ProjectFirmUserCount { get; init; }
}

public sealed class AdminDashboardTodayDto
{
    public int NewProjectCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
}

public sealed class DensityItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int ProjectCount { get; init; }
}
public sealed class AdminDashboardQueryDto
{
    public int? GdGroupId { get; init; }
    public int? GdFirmId { get; init; }
}