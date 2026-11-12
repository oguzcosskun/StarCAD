using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Policies;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Poliçeler")]
public class PoliciesController : BaseApiController
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/policies")]
    [EndpointSummary("Bir bağımsız bölüme poliçe oluşturur; o birimde zaten aktif bir poliçe varsa 400 döner")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] PolicyAddDto dto, CancellationToken cancellationToken)
    {
        var result = await _policyService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPut("api/policies/{id:int}")]
    [EndpointSummary("Poliçe alanlarını günceller")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PolicyUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _policyService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpDelete("api/policies/{id:int}")]
    [EndpointSummary("Poliçeyi elle iptal eder (soft-delete)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _policyService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/policies")]
    [EndpointSummary("Poliçeleri filtreli ve sayfalı listeler; projectId verilmezse görünürlük kapsamındaki tüm poliçeler listelenir. Varsayılanla kapanmış/birim-silinmiş poliçeler de listede")]
    [ProducesResponseType(typeof(PagedResultDto<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList([FromQuery] PolicyListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _policyService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/policies/{id:int}")]
    [EndpointSummary("Poliçe detayını getirir (kapanmış olsa da erişilebilir)")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken cancellationToken)
    {
        var result = await _policyService.GetDetailAsync(id, cancellationToken);
        return FromResult(result);
    }
}
