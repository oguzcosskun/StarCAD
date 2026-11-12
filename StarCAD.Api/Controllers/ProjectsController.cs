using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Projects;
using StarCAD.ViewModel.Common;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Projeler")]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("api/projects")]
    [EndpointSummary("Aktif projeleri listeler (durum/tarih/il/ilçe/proje firması filtreli, sayfalı)")]
    [ProducesResponseType(typeof(PagedResultDto<ProjectListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ProjectListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/projects/{id:int}")]
    [EndpointSummary("Bir projenin detayını getirir")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }
    
    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/projects")]
    [EndpointSummary("Yeni proje oluşturur")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] ProjectCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }
    
    [HttpGet("api/projects/status-counts")]
    [EndpointSummary("Filtre kriterlerine göre durum başına proje adedini döner (sekme rozetleri)")]
    [ProducesResponseType(typeof(ProjectStatusCountsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusCounts(
        [FromQuery] ProjectStatusCountsQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetStatusCountsAsync(query, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPut("api/projects/{id:int}")]
    [EndpointSummary("Projeyi günceller (ad/açıklama/kod)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpDelete("api/projects/{id:int}")]
    [EndpointSummary("Projeyi pasifleştirir (soft-delete)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
