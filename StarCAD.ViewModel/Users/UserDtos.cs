namespace StarCAD.ViewModel.Users;

public static class UserSortKeys
{
    public const string CreatedAt = "createdAt";
    public const string FullName = "fullName";
}

public sealed class UserListQueryDto
{
    public string? Q { get; init; }
    public string? RoleCode { get; init; }
    public int? ProjectFirmId { get; init; }
    public int? GasDistributionFirmId { get; init; }
    public int? GasDistributionGroupId { get; init; }
    public string SortBy { get; init; } = UserSortKeys.CreatedAt;
    public string SortDir { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 30;
}

public sealed class UserListItemDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public int? ProjectFirmId { get; init; }
    public string? ProjectFirmName { get; init; }
    public int? GasDistributionFirmId { get; init; }
    public string? GasDistributionFirmName { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ResetPasswordRequestDto
{
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class UserUpdateDto
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? RoleCode { get; init; }
    public int? ProjectFirmId { get; init; }
    public int? GasDistributionFirmId { get; init; }
}