using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Cities;

namespace StarCAD.Business.Abstract;

public interface ICityService
{
    Task<IDataResult<List<CityListItemDto>>> GetListAsync(CancellationToken cancellationToken = default);

    Task<IDataResult<List<DistrictListItemDto>>> GetDistrictsAsync(int cityId, CancellationToken cancellationToken = default);
}