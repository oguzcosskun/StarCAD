using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.InsuranceCompanies;

namespace StarCAD.Api.Controllers;

// Salt-okunur master veri; giriş yapmış her kullanıcıya açık (bkz. CitiesController).
[Authorize]
[Tags("Sigorta Şirketleri")]
public class InsuranceCompaniesController : BaseApiController
{
    private readonly IInsuranceCompanyService _insuranceCompanyService;

    public InsuranceCompaniesController(IInsuranceCompanyService insuranceCompanyService)
    {
        _insuranceCompanyService = insuranceCompanyService;
    }

    [HttpGet("api/insurance-companies")]
    [EndpointSummary("Sigorta şirketlerini listeler")]
    [ProducesResponseType(typeof(List<InsuranceCompanyListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _insuranceCompanyService.GetListAsync(cancellationToken);
        return FromResult(result);
    }
}
