using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarCAD.Business.Abstract;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Projects;

namespace StarCAD.Api.Controllers;

[Tags("Proje Onay Akışı")]
public class ProjectApprovalController : BaseApiController
{
    private readonly IProjectApprovalService _projectApprovalService;

    public ProjectApprovalController(IProjectApprovalService projectApprovalService)
    {
        _projectApprovalService = projectApprovalService;
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.ProjectFirmUser}")]
    [HttpPost("api/projects/{projectId:int}/submit")]
    [EndpointSummary("Projeyi onaya gönderir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(int projectId, CancellationToken cancellationToken)
    {
        var result = await _projectApprovalService.SubmitAsync(projectId, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.GasDistributionUser}")]
    [HttpPost("api/projects/{projectId:int}/approve")]
    [EndpointSummary("Projeyi onaylar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int projectId, CancellationToken cancellationToken)
    {
        var result = await _projectApprovalService.ApproveAsync(projectId, cancellationToken);
        return FromResult(result);
    }

    [Authorize(Roles = $"{RoleCodes.Admin},{RoleCodes.GasDistributionUser}")]
    [HttpPost("api/projects/{projectId:int}/reject")]
    [EndpointSummary("Projeyi reddeder (gerekçe zorunlu)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(int projectId, [FromBody] ProjectRejectDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectApprovalService.RejectAsync(projectId, dto, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("api/projects/{projectId:int}/history")]
    [EndpointSummary("Projenin işlem geçmişini listeler")]
    [ProducesResponseType(typeof(List<OperationHistoryListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(int projectId, CancellationToken cancellationToken)
    {
        var result = await _projectApprovalService.GetHistoryAsync(projectId, cancellationToken);
        return FromResult(result);
    }
}