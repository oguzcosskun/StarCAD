using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.GasDistributionGroups;

namespace StarCAD.Business.Abstract;

public interface IGasDistributionGroupService
{
    Task<IDataResult<List<GasDistributionGroupListItemDto>>> GetListAsync(CancellationToken cancellationToken = default);

    Task<IDataResult<GasDistributionGroupListItemDto>> CreateAsync(GasDistributionGroupCreateDto dto, CancellationToken cancellationToken = default);

    Task<IResult> UpdateAsync(int id, GasDistributionGroupUpdateDto dto, CancellationToken cancellationToken = default);

    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}