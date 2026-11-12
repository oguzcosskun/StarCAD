using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.ProjectFirmAuthorizations;

namespace StarCAD.Business.Abstract;

public interface IProjectFirmAuthorizationService
{
    Task<IDataResult<PagedResultDto<ProjectFirmAuthorizationListItemDto>>> GetListAsync(ProjectFirmAuthorizationListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectFirmAuthorizationListItemDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectFirmAuthorizationListItemDto>> CreateAsync(ProjectFirmAuthorizationCreateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, ProjectFirmAuthorizationUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}