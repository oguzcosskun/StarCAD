using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.ProjectFirms;
using StarCAD.ViewModel.Common;

namespace StarCAD.Api.Controllers;

// Okuma (GET) her yetkili kullanıcıya açık ama manager'da firma bazında filtrelenir
// (WhereVisibleTo); yazma işlemleri yalnızca Admin.
[Authorize]
[Tags("Proje Firmaları")]
public class ProjectFirmsController : BaseApiController
{
    private readonly IProjectFirmService _projectFirmService;

    public ProjectFirmsController(IProjectFirmService projectFirmService)
    {
        _projectFirmService = projectFirmService;
    }

    [HttpGet("api/projectfirms")]
    [EndpointSummary("Görünürlük kapsamındaki proje firmalarını listeler (GD grup/firma filtreli, sayfalı)")]
    [ProducesResponseType(typeof(PagedResultDto<ProjectFirmListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ProjectFirmListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _projectFirmService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/projectfirms/{id:int}")]
    [EndpointSummary("Bir proje firmasının detayını getirir")]
    [ProducesResponseType(typeof(ProjectFirmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _projectFirmService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("api/projectfirms")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Yeni proje firması ekler (tüzel veya şahıs)")]
    [ProducesResponseType(typeof(ProjectFirmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ProjectFirmCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectFirmService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [HttpPut("api/projectfirms/{id:int}")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Proje firmasını günceller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectFirmUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectFirmService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("api/projectfirms/{id:int}")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Proje firmasını pasifleştirir (soft-delete)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _projectFirmService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
