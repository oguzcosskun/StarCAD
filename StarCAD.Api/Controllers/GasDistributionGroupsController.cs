using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.GasDistributionGroups;

namespace StarCAD.Api.Controllers;

// Okuma her yetkili kullanıcıya açık; yazma işlemleri (metot düzeyinde) yalnızca Admin.
[Authorize]
[Tags("Gaz Dağıtım Grup Firmaları")]
public class GasDistributionGroupsController : BaseApiController
{
    private readonly IGasDistributionGroupService _gasDistributionGroupService;

    public GasDistributionGroupsController(IGasDistributionGroupService gasDistributionGroupService)
    {
        _gasDistributionGroupService = gasDistributionGroupService;
    }

    [HttpGet("api/gasdistributiongroups")]
    [EndpointSummary("Aktif grup firmalarını listeler")]
    [ProducesResponseType(typeof(List<GasDistributionGroupListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _gasDistributionGroupService.GetListAsync(cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/gasdistributiongroups")]
    [EndpointSummary("Yeni grup firması ekler (yalnızca yönetici)")]
    [ProducesResponseType(typeof(GasDistributionGroupListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] GasDistributionGroupCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionGroupService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPut("api/gasdistributiongroups/{id:int}")]
    [EndpointSummary("Grup firmasını günceller (yalnızca yönetici)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] GasDistributionGroupUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionGroupService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpDelete("api/gasdistributiongroups/{id:int}")]
    [EndpointSummary("Grup firmasını pasifleştirir (yalnızca yönetici) — bağlı firma varsa silinmez")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionGroupService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}