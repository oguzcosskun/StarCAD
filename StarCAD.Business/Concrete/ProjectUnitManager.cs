using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.ViewModel.ProjectUnits;

namespace StarCAD.Business.Concrete;

public class ProjectUnitManager : IProjectUnitService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;

    public ProjectUnitManager(StarCADContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IDataResult<List<ProjectUnitListItemDto>>> GetListByProjectAsync(
        int projectId, CancellationToken cancellationToken = default)
    {
        var projectVisible = await _db.Projects
            .WhereVisibleTo(_currentUser)
            .AnyAsync(p => p.Id == projectId, cancellationToken);

        if (!projectVisible)
            return new ErrorDataResult<List<ProjectUnitListItemDto>>("Proje bulunamadı.", StatusCodes.Status404NotFound);

        var list = await _db.ProjectUnits.AsNoTracking()
            .Where(u => u.ProjectId == projectId)
            .OrderBy(u => u.UnitNumber)
            .ThenBy(u => u.Id)
            .Select(u => new ProjectUnitListItemDto
            {
                Id = u.Id,
                UnitNumber = u.UnitNumber,
                SubscriberName = u.SubscriberName,
                SubscriberNo = u.SubscriberNo,
                MeterClassLabel = u.MeterClassLabel,
                Area = u.Area,
                FlowCubicMeterPerHour = u.FlowCubicMeterPerHour,
                PressureMbar = u.PressureMbar,
                PipeTypeName = u.PipeTypeName,
                Devices = u.Devices.Select(d => new ProjectUnitDeviceSummaryDto
                {
                    Id = d.Id,
                    DeviceType = d.DeviceType,
                    Brand = d.Brand,
                    Model = d.Model,
                    Capacity = d.Capacity,
                    FlowCubicMeterPerHour = d.FlowCubicMeterPerHour,
                    FlueLabel = d.FlueLabel
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<List<ProjectUnitListItemDto>>(list);
    }
}
