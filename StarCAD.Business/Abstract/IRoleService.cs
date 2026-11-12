using StarCAD.Core.Utilities.Results;
using StarCAD.ViewModel.Roles;

namespace StarCAD.Business.Abstract;

public interface IRoleService
{
    Task<IDataResult<List<RoleListItemDto>>> GetListAsync(CancellationToken cancellationToken = default);
}