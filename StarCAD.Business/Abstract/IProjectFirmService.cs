using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.ProjectFirms;

namespace StarCAD.Business.Abstract;

public interface IProjectFirmService
{
    Task<IDataResult<PagedResultDto<ProjectFirmListItemDto>>> GetListAsync(ProjectFirmListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectFirmDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectFirmDetailDto>> CreateAsync(ProjectFirmCreateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, ProjectFirmUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}