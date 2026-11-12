namespace StarCAD.ViewModel.Projects;

public sealed class ProjectCreateDto
{
    public string Name { get; init; } = string.Empty;
    public int ProjectFirmAuthorizationId { get; init; }
    public string? Description { get; init; }

    public int? CityId { get; init; }
    public int? DistrictId { get; init; }
    public string? AddressLine { get; init; }
    public string? BlockLotParcel { get; init; }
    public string? BuildingCode { get; init; }
    public byte? FloorCount { get; init; }
    public byte? BasementCount { get; init; }

    public int ProjectTypeCodeId { get; init; }
    public int HeatingTypeCodeId { get; init; }
    public int BuildingUsageTypeCodeId { get; init; }
    public bool IsPermitProject { get; init; }
    public int? ApartmentCount { get; init; }
    public int? WorkplaceCount { get; init; }
    public int? AreaSquareMeters { get; init; }
    public int? Capacity { get; init; }
    public int? ServiceBoxPressureMbar { get; init; }
}

public sealed class ProjectUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public int? CityId { get; init; }
    public int? DistrictId { get; init; }
    public string? AddressLine { get; init; }
    public string? BlockLotParcel { get; init; }
    public string? BuildingCode { get; init; }
    public byte? FloorCount { get; init; }
    public byte? BasementCount { get; init; }

    public int ProjectTypeCodeId { get; init; }
    public int HeatingTypeCodeId { get; init; }
    public int BuildingUsageTypeCodeId { get; init; }
    public bool IsPermitProject { get; init; }
    public int? ApartmentCount { get; init; }
    public int? WorkplaceCount { get; init; }
    public int? AreaSquareMeters { get; init; }
    public int? Capacity { get; init; }
    public int? ServiceBoxPressureMbar { get; init; }
    public byte[]? RowVersion { get; init; }
}

public sealed class ProjectListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? BuildingCode { get; init; }
    public string? Status { get; init; }
    public string? StatusName { get; init; }
    public string? ProjectTypeName { get; init; }
    public string? HeatingTypeName { get; init; }
    public string? FirmName { get; init; }
    public string? GasDistributionFirmName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProjectDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Status { get; init; }
    public string? StatusName { get; init; }
    public int ProjectFirmAuthorizationId { get; init; }
    public int ProjectFirmId { get; init; }
    public int GasDistributionFirmId { get; init; }
    public string? GasDistributionFirmName { get; init; }
    public int? CityId { get; init; }
    public string? CityName { get; init; }
    public int? DistrictId { get; init; }
    public string? DistrictName { get; init; }
    public string? AddressLine { get; init; }
    public string? BlockLotParcel { get; init; }
    public string? BuildingCode { get; init; }
    public byte? FloorCount { get; init; }
    public byte? BasementCount { get; init; }

    public int? ProjectTypeCodeId { get; init; }
    public string? ProjectTypeName { get; init; }
    public int? HeatingTypeCodeId { get; init; }
    public string? HeatingTypeName { get; init; }
    public int? BuildingUsageTypeCodeId { get; init; }
    public string? BuildingUsageTypeName { get; init; }
    public bool IsPermitProject { get; init; }
    public int? ApartmentCount { get; init; }
    public int? WorkplaceCount { get; init; }
    public int? AreaSquareMeters { get; init; }
    public int? Capacity { get; init; }
    public int? ServiceBoxPressureMbar { get; init; }
    public byte[]? RowVersion { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed class ProjectStatusCountsQueryDto
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? CityId { get; init; }
    public int? DistrictId { get; init; }
    public int? ProjectFirmId { get; init; }
    public int? GdGroupId { get; init; }
    public int? GdFirmId { get; init; }
}

public sealed class ProjectStatusCountsDto
{
    public int Draft { get; init; }
    public int PendingApproval { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
}

public static class ProjectSortKeys
{
    public const string UpdatedAt = "updatedAt";
    public const string CreatedAt = "createdAt";
    public const string Name = "name";
}

public sealed class ProjectListQueryDto
{
    public string? Q { get; init; }
    public string? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? CityId { get; init; }
    public int? DistrictId { get; init; }
    public int? GdGroupId { get; init; }
    public int? GdFirmId { get; init; }
    public int? ProjectFirmId { get; init; }
    public string SortBy { get; init; } = ProjectSortKeys.UpdatedAt;
    public string SortDir { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}