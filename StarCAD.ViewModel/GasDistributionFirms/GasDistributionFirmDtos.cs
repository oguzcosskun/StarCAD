namespace StarCAD.ViewModel.GasDistributionFirms;

public sealed class GasDistributionFirmCreateDto
{
    public string Title { get; init; } = string.Empty;
    public int CompanyNumber { get; init; }
    public int? GroupId { get; init; }
    public string? Description { get; init; }
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public sealed class GasDistributionFirmUpdateDto
{
    public string Title { get; init; } = string.Empty;
    public int CompanyNumber { get; init; }
    public int? GroupId { get; init; }
    public string? Description { get; init; }
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

// Lite liste: iletişim (ContactPerson/Phone) ve adres taşımaz — bunlar yalnızca
// Admin'e açık detay uçundan döner. Liste her yetkili kullanıcıya açıktır.
public sealed class GasDistributionFirmListItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CompanyNumber { get; init; }
    public int? GroupId { get; init; }
    public string? GroupName { get; init; }
}

public sealed class GasDistributionFirmDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CompanyNumber { get; init; }
    public int? GroupId { get; init; }
    public string? GroupName { get; init; }
    public string? Description { get; init; }
    public string? ContactPerson { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public static class GasDistributionFirmSortKeys
{
    public const string Title = "title";
    public const string CompanyNumber = "companyNumber";
    public const string GroupName = "groupName";
}

public sealed class GasDistributionFirmListQueryDto
{
    public string? Q { get; init; }
    public int? GasDistributionGroupId { get; init; }
    public string SortBy { get; init; } = GasDistributionFirmSortKeys.Title;
    public string SortDir { get; init; } = "asc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}