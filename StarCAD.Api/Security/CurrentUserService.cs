using System.Security.Claims;
using StarCAD.Common.Constants;
using StarCAD.Core.Security;

namespace StarCAD.Api.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    // HttpContext null olabilir (HTTP dışı çağrı) → her erişim null-safe.
    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int? UserId => ParseInt(Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    public string? Username => Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? RoleCode => Principal?.FindFirst(ClaimTypes.Role)?.Value;

    public int? ProjectFirmId => ParseInt(Principal?.FindFirst(ClaimNames.ProjectFirmId)?.Value);

    public int? GasDistributionFirmId => ParseInt(Principal?.FindFirst(ClaimNames.GasDistributionFirmId)?.Value);

    private static int? ParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;
}