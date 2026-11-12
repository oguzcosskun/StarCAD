using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Policies;

namespace StarCAD.Business.Abstract;

public interface IPolicyService
{
    Task<IDataResult<PolicyDto>> CreateAsync(PolicyAddDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, PolicyUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IDataResult<PagedResultDto<PolicyDto>>> GetListAsync(PolicyListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<PolicyDto>> GetDetailAsync(int id, CancellationToken cancellationToken = default);
}
