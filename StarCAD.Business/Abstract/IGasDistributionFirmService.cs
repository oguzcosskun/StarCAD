using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.GasDistributionFirms;
using StarCAD.ViewModel.Common;

namespace StarCAD.Business.Abstract;

public interface IGasDistributionFirmService
{
    Task<IDataResult<PagedResultDto<GasDistributionFirmListItemDto>>> GetListAsync(GasDistributionFirmListQueryDto query, CancellationToken cancellationToken = default);
    Task<IDataResult<GasDistributionFirmDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IDataResult<GasDistributionFirmDetailDto>> CreateAsync(GasDistributionFirmCreateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> UpdateAsync(int id, GasDistributionFirmUpdateDto dto, CancellationToken cancellationToken = default);
    Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}