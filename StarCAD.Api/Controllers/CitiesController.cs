using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.Cities;

namespace StarCAD.Api.Controllers;

// Salt-okunur master veri; giriş yapmış her kullanıcıya açık.
[Authorize]
[Tags("İller ve İlçeler")]
public class CitiesController : BaseApiController
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet("api/cities")]
    [EndpointSummary("İlleri listeler")]
    [ProducesResponseType(typeof(List<CityListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _cityService.GetListAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/cities/{cityId:int}/districts")]
    [EndpointSummary("Bir ilin ilçelerini listeler")]
    [ProducesResponseType(typeof(List<DistrictListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDistricts(int cityId, CancellationToken cancellationToken)
    {
        var result = await _cityService.GetDistrictsAsync(cityId, cancellationToken);
        return FromResult(result);
    }
}