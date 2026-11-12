using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.ProjectUnits;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Proje Birimleri")]
public class ProjectUnitsController : BaseApiController
{
    private readonly IProjectUnitService _projectUnitService;

    public ProjectUnitsController(IProjectUnitService projectUnitService)
    {
        _projectUnitService = projectUnitService;
    }

    [HttpGet("api/projects/{projectId:int}/units")]
    [EndpointSummary("Bir projenin bağımsız bölümlerini listeler (canvas'tan otomatik senkronize edilir)")]
    [ProducesResponseType(typeof(List<ProjectUnitListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(int projectId, CancellationToken cancellationToken)
    {
        var result = await _projectUnitService.GetListByProjectAsync(projectId, cancellationToken);
        return FromResult(result);
    }
}
