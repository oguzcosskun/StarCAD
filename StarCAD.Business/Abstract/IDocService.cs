using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Docs;

namespace StarCAD.Business.Abstract;

public interface IDocService
{
    Task<IDataResult<DocDetailDto>> UploadAsync(
        int? projectId, int? projectUnitId, int? docTypeCodeId, int? projectFirmId,
        string fileName, string contentType, long sizeBytes, Stream fileStream,
        CancellationToken cancellationToken = default);
    Task<IResult> AssignAsync(int id, DocAssignDto dto, CancellationToken cancellationToken = default);
    Task<IResult> LinkUnitAsync(int docId, int projectUnitId, CancellationToken cancellationToken = default);
    Task<IResult> UnlinkUnitAsync(int docId, int projectUnitId, CancellationToken cancellationToken = default);
    Task<IDataResult<PagedResultDto<DocListItemDto>>> GetListAsync(DocListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<DocDownloadDto>> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
