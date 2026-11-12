using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Codes;

namespace StarCAD.Business.Abstract;

public interface ICodeService
{
    Task<IDataResult<List<CodeListItemDto>>> GetListByGroupAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IDataResult<List<CodeListItemDto>>> GetListByGroupNameAsync(string groupName, CancellationToken cancellationToken = default);
    Task<IDataResult<CodeListItemDto>> CreateAsync(CodeCreateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, CodeUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}