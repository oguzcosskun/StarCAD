using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Projects;

namespace StarCAD.Business.Abstract;

public interface IProjectApprovalService
{
    Task<IResult> SubmitAsync(int projectId, CancellationToken cancellationToken = default);

    Task<IResult> ApproveAsync(int projectId, CancellationToken cancellationToken = default);

    Task<IResult> RejectAsync(int projectId, ProjectRejectDto dto, CancellationToken cancellationToken = default);

    Task<IDataResult<List<OperationHistoryListItemDto>>> GetHistoryAsync(int projectId, CancellationToken cancellationToken = default);
}