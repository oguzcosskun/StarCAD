namespace StarCAD.ViewModel.ProjectFirmAuthorizations;

public sealed class ProjectFirmAuthorizationCreateDto
{
    public int ProjectFirmId { get; init; }
    public int GasDistributionFirmId { get; init; }
    public string CertificateNumber { get; init; } = string.Empty;
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
}

public sealed class ProjectFirmAuthorizationUpdateDto
{
    public string CertificateNumber { get; init; } = string.Empty;
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
}

public sealed class ProjectFirmAuthorizationListItemDto
{
    public int Id { get; init; }
    public int ProjectFirmId { get; init; }
    public string ProjectFirmName { get; init; } = string.Empty;
    public int GasDistributionFirmId { get; init; }
    public string GasDistributionFirmName { get; init; } = string.Empty;
    public string CertificateNumber { get; init; } = string.Empty;
    public DateTime ValidFrom { get; init; }
    public DateTime? ValidTo { get; init; }
}

public static class ProjectFirmAuthorizationSortKeys
{
    public const string ValidFrom = "validFrom";
    public const string ProjectFirmName = "projectFirmName";
}

public sealed class ProjectFirmAuthorizationListQueryDto
{
    public int? ProjectFirmId { get; init; }
    public int? GasDistributionFirmId { get; init; }
    public int? GasDistributionGroupId { get; init; }
    public string SortBy { get; init; } = ProjectFirmAuthorizationSortKeys.ValidFrom;
    public string SortDir { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}