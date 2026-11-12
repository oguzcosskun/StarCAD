using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.GasDistributionFirms;
using StarCAD.ViewModel.Common;

namespace StarCAD.Api.Controllers;

// Liste (GET) her yetkili kullanıcıya açık ama "lite"tir (iletişim/adres taşımaz).
// Detay ve tüm yazma işlemleri yalnızca Admin.
[Authorize]
[Tags("Gaz Dağıtım Firmaları")]
public class GasDistributionFirmsController : BaseApiController
{
    private readonly IGasDistributionFirmService _gasDistributionFirmService;

    public GasDistributionFirmsController(IGasDistributionFirmService gasDistributionFirmService)
    {
        _gasDistributionFirmService = gasDistributionFirmService;
    }

    [HttpGet("api/gasdistributionfirms")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Gaz dağıtım firmalarını listeler (grup filtreli, aranabilir, sayfalı)")]
    [ProducesResponseType(typeof(PagedResultDto<GasDistributionFirmListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] GasDistributionFirmListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionFirmService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/gasdistributionfirms/{id:int}")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Bir gaz dağıtım firmasının detayını getirir (iletişim/adres dahil — Admin)")]
    [ProducesResponseType(typeof(GasDistributionFirmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionFirmService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("api/gasdistributionfirms")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Yeni gaz dağıtım firması ekler")]
    [ProducesResponseType(typeof(GasDistributionFirmDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] GasDistributionFirmCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionFirmService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [HttpPut("api/gasdistributionfirms/{id:int}")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Gaz dağıtım firmasını günceller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] GasDistributionFirmUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionFirmService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("api/gasdistributionfirms/{id:int}")]
    [Authorize(Roles = RoleCodes.Admin)]
    [EndpointSummary("Gaz dağıtım firmasını pasifleştirir (soft-delete)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _gasDistributionFirmService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}