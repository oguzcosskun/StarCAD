using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Admin;

namespace StarCAD.Api.Controllers;

[Authorize(Roles = RoleCodes.Admin)]
[Tags("Yönetici Paneli")]
public class AdminDashboardController : BaseApiController
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet("api/admin/dashboard")]
    [EndpointSummary("Yönetici paneli özet verileri (opsiyonel gaz dağıtım grubu/firma filtresi)")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] AdminDashboardQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _adminDashboardService.GetAsync(query, cancellationToken);
        return FromResult(result);
    }
}