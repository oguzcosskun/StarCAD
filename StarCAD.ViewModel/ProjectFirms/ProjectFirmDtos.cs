namespace StarCAD.ViewModel.ProjectFirms;

public static class ProjectFirmSortKeys
{
    public const string Title = "title";
}
public sealed class ProjectFirmCreateDto
{
    public byte CompanyType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    // Şahıs firma (CompanyType=1) için düz TC. Şifreli saklanır; API'den asla düz dönmez.
    public string? NationalIdNumber { get; init; }
    public string? AccountingCode { get; init; }
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Phone2 { get; init; }
    public string? Address { get; init; }
}

public sealed class ProjectFirmUpdateDto
{
    public byte CompanyType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? NationalIdNumber { get; init; }
    public string? AccountingCode { get; init; }
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Phone2 { get; init; }
    public string? Address { get; init; }
}

public sealed class ProjectFirmAuthorizedGasDistributionFirmDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
}

public sealed class ProjectFirmListItemDto
{
    public int Id { get; init; }
    public byte CompanyType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<ProjectFirmAuthorizedGasDistributionFirmDto> AuthorizedGasDistributionFirms { get; init; } = [];
}

public sealed class ProjectFirmDetailDto
{
    public int Id { get; init; }
    public byte CompanyType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? TaxNumber { get; init; }
    // Şahıs firma için MASKELİ TC (ör. "*******1234"). Düz TC asla dönmez.
    public string? NationalIdNumber { get; init; }
    public string? AccountingCode { get; init; }
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Phone2 { get; init; }
    public string? Address { get; init; }
}

public sealed class ProjectFirmListQueryDto
{
    public string? Q { get; init; }
    public int? GasDistributionFirmId { get; init; }
    public int? GasDistributionGroupId { get; init; }
    public string SortBy { get; init; } = ProjectFirmSortKeys.Title;
    public string SortDir { get; init; } = "asc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}