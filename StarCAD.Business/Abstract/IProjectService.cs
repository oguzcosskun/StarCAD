using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Projects;
using StarCAD.ViewModel.Common;

namespace StarCAD.Business.Abstract;

public interface IProjectService
{
    Task<IDataResult<PagedResultDto<ProjectListItemDto>>> GetListAsync(ProjectListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectDetailDto>> CreateAsync(ProjectCreateDto dto, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectStatusCountsDto>> GetStatusCountsAsync(ProjectStatusCountsQueryDto query, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, ProjectUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
