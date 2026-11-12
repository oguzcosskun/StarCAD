using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.ProjectVersions;
using Microsoft.AspNetCore.Authorization;
using StarCAD.Common.Constants;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Çizim Versiyonları")]
public class ProjectVersionsController : BaseApiController
{
    private readonly IProjectVersionService _projectVersionService;

    public ProjectVersionsController(IProjectVersionService projectVersionService)
    {
        _projectVersionService = projectVersionService;
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/projects/{projectId:int}/newversion")]
    [EndpointSummary("Çizim JSON'unu depoya yükler ve yeni versiyon kaydı oluşturur")]
    [ProducesResponseType(typeof(ProjectVersionCreatedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(int projectId, [FromQuery] string? label, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawJson = await reader.ReadToEndAsync(cancellationToken);

        var result = await _projectVersionService.CreateAsync(projectId, label, rawJson, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/projects/{projectId:int}/versions")]
    [EndpointSummary("Bir projenin çizim versiyonlarını listeler")]
    [ProducesResponseType(typeof(List<ProjectVersionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(int projectId, CancellationToken cancellationToken)
    {
        var result = await _projectVersionService.GetListByProjectAsync(projectId, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/projectversions/{id:int}/get")]
    [EndpointSummary("Çizim JSON'u için süreli indirme linki üretir")]
    [ProducesResponseType(typeof(ProjectVersionDownloadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(int id, CancellationToken cancellationToken)
    {
        var result = await _projectVersionService.GetDownloadUrlAsync(id, cancellationToken);
        return FromResult(result);
    }
}