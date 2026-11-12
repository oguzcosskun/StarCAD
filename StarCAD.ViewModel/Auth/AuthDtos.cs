namespace StarCAD.ViewModel.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public int? ProjectFirmId { get; set; }
    public int? GasDistributionFirmId { get; set; }
}

public sealed class LoginResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
}

public sealed class RegisteredUserDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
}

public sealed class CurrentUserDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public int? ProjectFirmId { get; init; }
    public int? GasDistributionFirmId { get; init; }
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}