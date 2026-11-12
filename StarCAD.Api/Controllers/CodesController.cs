using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Codes;

namespace StarCAD.Api.Controllers;

// Okuma her yetkili kullanıcıya açık; yazma işlemleri (metot düzeyinde) yalnızca Admin.
[Authorize]
[Tags("Kodlar")]
public class CodesController : BaseApiController
{
    private readonly ICodeService _codeService;

    public CodesController(ICodeService codeService)
    {
        _codeService = codeService;
    }

    [HttpGet("api/codes/by-group-name/{groupName}")]
    [EndpointSummary("Grup adına göre aktif kodları listeler (ör. InstallationPaletteTool)")]
    [ProducesResponseType(typeof(List<CodeListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGroupName(string groupName, CancellationToken cancellationToken)
    {
        var result = await _codeService.GetListByGroupNameAsync(groupName, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/codes")]
    [EndpointSummary("Yeni kod ekler (yalnızca yönetici)")]
    [ProducesResponseType(typeof(CodeListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CodeCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _codeService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPut("api/codes/{id:int}")]
    [EndpointSummary("Kodu günceller (yalnızca yönetici)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] CodeUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _codeService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpDelete("api/codes/{id:int}")]
    [EndpointSummary("Kodu pasifleştirir (yalnızca yönetici) — kullanımdaysa silinmez")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _codeService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}