using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.CodeGroups;

namespace StarCAD.Business.Abstract;

public interface ICodeGroupService
{
    Task<IDataResult<List<CodeGroupListItemDto>>> GetListAsync(CancellationToken cancellationToken = default);
    Task<IDataResult<CodeGroupListItemDto>> CreateAsync(CodeGroupCreateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, CodeGroupUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}