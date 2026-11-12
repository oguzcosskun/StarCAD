using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.CodeGroups;
using StarCAD.ViewModel.Codes;

namespace StarCAD.Api.Controllers;

// Okuma her yetkili kullanıcıya açık; yazma işlemleri (metot düzeyinde) yalnızca Admin.
[Authorize]
[Tags("Kod Grupları")]
public class CodeGroupsController : BaseApiController
{
    private readonly ICodeGroupService _codeGroupService;
    private readonly ICodeService _codeService;

    public CodeGroupsController(ICodeGroupService codeGroupService, ICodeService codeService)
    {
        _codeGroupService = codeGroupService;
        _codeService = codeService;
    }

    [HttpGet("api/code-groups")]
    [EndpointSummary("Aktif kod gruplarını listeler")]
    [ProducesResponseType(typeof(List<CodeGroupListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await _codeGroupService.GetListAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/code-groups/{groupId:int}/codes")]
    [EndpointSummary("Bir gruba ait aktif kodları listeler")]
    [ProducesResponseType(typeof(List<CodeListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCodes(int groupId, CancellationToken cancellationToken)
    {
        var result = await _codeService.GetListByGroupAsync(groupId, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPost("api/code-groups")]
    [EndpointSummary("Yeni kod grubu ekler (yalnızca yönetici)")]
    [ProducesResponseType(typeof(CodeGroupListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CodeGroupCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _codeGroupService.CreateAsync(dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpPut("api/code-groups/{id:int}")]
    [EndpointSummary("Kod grubunu günceller (yalnızca yönetici)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] CodeGroupUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _codeGroupService.UpdateAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = RoleCodes.Admin)]
    [HttpDelete("api/code-groups/{id:int}")]
    [EndpointSummary("Kod grubunu pasifleştirir (yalnızca yönetici) — bağlı kod varsa silinmez")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _codeGroupService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}