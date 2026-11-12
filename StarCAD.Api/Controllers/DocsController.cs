using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Api.Requests;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Docs;

namespace StarCAD.Api.Controllers;

[Authorize]
[Tags("Evraklar")]
public class DocsController : BaseApiController
{
    private readonly IDocService _docService;

    public DocsController(IDocService docService)
    {
        _docService = docService;
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/docs")]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Evrak yükler; projectId verilmezse havuza yüklenir")]
    [ProducesResponseType(typeof(DocDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload([FromForm] DocUploadRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "Dosya boş olamaz." });

        await using var stream = request.File.OpenReadStream();
        var result = await _docService.UploadAsync(
            request.ProjectId, request.ProjectUnitId, request.DocTypeCodeId, request.ProjectFirmId,
            request.File.FileName, request.File.ContentType, request.File.Length, stream,
            cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPut("api/docs/{id:int}/assign")]
    [EndpointSummary("Bir evrakı bir projeye bağlar ya da havuza geri alır (projectId=null); birim bağlantıları temizlenir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int id, [FromBody] DocAssignDto dto, CancellationToken cancellationToken)
    {
        var result = await _docService.AssignAsync(id, dto, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/docs/{id:int}/units/{unitId:int}")]
    [EndpointSummary("Evrakı bir bağımsız bölüme bağlar (many-to-many); evrak havuzdaysa proje/firma bilgisi birimden geri doldurulur")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkUnit(int id, int unitId, CancellationToken cancellationToken)
    {
        var result = await _docService.LinkUnitAsync(id, unitId, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpDelete("api/docs/{id:int}/units/{unitId:int}")]
    [EndpointSummary("Evrakın bir bağımsız bölümle bağlantısını kaldırır")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkUnit(int id, int unitId, CancellationToken cancellationToken)
    {
        var result = await _docService.UnlinkUnitAsync(id, unitId, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/docs")]
    [EndpointSummary("Evrak havuzunu filtreli ve sayfalı listeler")]
    [ProducesResponseType(typeof(PagedResultDto<DocListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] DocListQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _docService.GetListAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/docs/{id:int}/download")]
    [EndpointSummary("Evrak için süreli indirme linki üretir")]
    [ProducesResponseType(typeof(DocDownloadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDownloadUrl(int id, CancellationToken cancellationToken)
    {
        var result = await _docService.GetDownloadUrlAsync(id, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpDelete("api/docs/{id:int}")]
    [EndpointSummary("Evrakı siler")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _docService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
