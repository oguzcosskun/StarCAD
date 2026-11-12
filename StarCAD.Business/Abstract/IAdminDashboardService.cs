using StarCAD.ViewModel.Admin;
using StarCAD.Core.Utilities.Results;

public interface IAdminDashboardService
{
    Task<IDataResult<AdminDashboardDto>> GetAsync(AdminDashboardQueryDto query, CancellationToken cancellationToken = default);
}