using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.Roles;
using Microsoft.AspNetCore.Authorization;


namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Roller")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("api/roles")]
    [EndpointSummary("Tüm aktif rolleri listeler")]
    [ProducesResponseType(typeof(List<RoleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetListAsync(cancellationToken);
        return FromResult(result);
    }
}