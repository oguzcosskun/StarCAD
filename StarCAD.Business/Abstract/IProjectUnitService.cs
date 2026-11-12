using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.ProjectUnits;

namespace StarCAD.Business.Abstract;

// Yalnızca listeleme: birimler UnitSyncManager tarafından canvas'tan otomatik üretilir/güncellenir,
// elle CRUD yok (bkz. knowledge/unit-sync-hard-delete.md).
public interface IProjectUnitService
{
    Task<IDataResult<List<ProjectUnitListItemDto>>> GetListByProjectAsync(int projectId, CancellationToken cancellationToken = default);
}
