using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.ProjectFirmAuthorizations;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Proje Firması Yetkileri")]
public class ProjectFirmAuthorizationsController : BaseApiController
{
    private readonly IProjectFirmAuthorizationService _projectFirmAuthorizationService;

    public ProjectFirmAuthorizationsController(IProjectFirmAuthorizationService projectFirmAuthorizationService)
    {
        _projectFirmAuthorizationService = projectFirmAuthorizationService;
    }

    [HttpGet("api/project-firm-authorizations")]
    [EndpointSummary("Görünürlük kapsamındaki proje firması yetkilerini listeler")]
    [ProducesResponseType(typeof(PagedResultDto<ProjectFirmAuthorizationListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] ProjectFirmAuthorizationListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _projectFirmAuthorizationService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/project-firm-authorizations/{id:int}")]
    [EndpointSummary("Bir yetki kaydının detayını getirir")]
    [ProducesResponseType(typeof(ProjectFirmAuthorizationListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _projectFirmAuthorizationService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/project-firm-authorizations")]
    [EndpointSummary("Yeni bir proje firması yetkisi/sertifikası oluşturur (yalnızca yönetici)")]
    [ProducesResponseType(typeof(ProjectFirmAuthorizationListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ProjectFirmAuthorizationCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectFirmAuthorizationService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPut("api/project-firm-authorizations/{id:int}")]
    [EndpointSummary("Yetki kaydını günceller (yalnızca yönetici)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectFirmAuthorizationUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectFirmAuthorizationService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpDelete("api/project-firm-authorizations/{id:int}")]
    [EndpointSummary("Yetki kaydını siler (yalnızca yönetici)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _projectFirmAuthorizationService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}