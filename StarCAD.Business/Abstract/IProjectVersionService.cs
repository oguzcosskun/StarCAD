using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.ProjectVersions;

namespace StarCAD.Business.Abstract;

public interface IProjectVersionService
{
    Task<IDataResult<ProjectVersionCreatedDto>> CreateAsync(int projectId, string? label, string rawJson, CancellationToken cancellationToken = default);
    Task<IDataResult<List<ProjectVersionListItemDto>>> GetListByProjectAsync(int projectId, CancellationToken cancellationToken = default);
    Task<IDataResult<ProjectVersionDownloadDto>> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default);
}