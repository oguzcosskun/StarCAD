using Microsoft.EntityFrameworkCore;
using StarCAD.Common.Constants;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.ViewModel.Admin;

namespace StarCAD.Business.Concrete;

public class AdminDashboardManager : IAdminDashboardService
{
    private static readonly TimeZoneInfo TurkeyTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

    private readonly StarCADContext _db;

    public AdminDashboardManager(StarCADContext db)
    {
        _db = db;
    }

    public async Task<IDataResult<AdminDashboardDto>> GetAsync(AdminDashboardQueryDto query, CancellationToken cancellationToken = default)
    {
        var gdGroupId = query.GdGroupId;
        var gdFirmId  = query.GdFirmId;

        if (gdFirmId is not null)
        {
            var firmExists = await _db.GasDistributionFirms.AnyAsync(f => f.Id == gdFirmId.Value, cancellationToken);
            if (!firmExists)
                return new ErrorDataResult<AdminDashboardDto>("Seçilen firma bulunamadı.", StatusCodes.Status404NotFound);
        }
        else if (gdGroupId is not null)
        {
            var groupExists = await _db.GasDistributionGroups.AnyAsync(g => g.Id == gdGroupId.Value, cancellationToken);
            if (!groupExists)
                return new ErrorDataResult<AdminDashboardDto>("Seçilen Şirket bulunamadı.", StatusCodes.Status404NotFound);
        }


        var (startUtc, endUtc) = GetTodayBoundsUtc();

        var projects = _db.Projects.AsNoTracking();
        var firms    = _db.ProjectFirms.AsNoTracking();
        var users    = _db.Users.AsNoTracking();
        var history  = _db.OperationHistories.AsNoTracking();

        if (gdFirmId is { } firmId)
        {
            projects = projects.Where(p => p.ProjectFirmAuthorization.GasDistributionFirmId == firmId);

            firms = firms.Where(f => f.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirmId == firmId));

            users = users.Where(u =>
                (u.ProjectFirmId != null &&
                 u.ProjectFirm!.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirmId == firmId)) ||
                (u.GasDistributionFirmId != null && u.GasDistributionFirmId == firmId));

            history = history.Where(o => o.Project.ProjectFirmAuthorization.GasDistributionFirmId == firmId);
        }
        else if (gdGroupId is { } group)
        {
            projects = projects.Where(p =>
                p.ProjectFirmAuthorization.GasDistributionFirm.GroupId == group);

            firms = firms.Where(f =>
                f.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirm.GroupId == group));

            // Kullanıcı → grup tek yol değil: proje firması birden çok GD firmasına/gruba
            // yetkili olabilir, bu durumda kullanıcı her grupta sayılır (toplamı > genel toplam).
            users = users.Where(u =>
                (u.ProjectFirmId != null &&
                 u.ProjectFirm!.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirm.GroupId == group)) ||
                (u.GasDistributionFirmId != null &&
                 u.GasDistributionFirm!.GroupId == group));

            history = history.Where(o =>
                o.Project.ProjectFirmAuthorization.GasDistributionFirm.GroupId == group);
        }

        var summary = new AdminDashboardSummaryDto
        {
            GasDistributionUserCount = await users.CountAsync(u => u.GasDistributionFirmId != null, cancellationToken),
            ProjectFirmCount         = await firms.CountAsync(cancellationToken),
            ProjectFirmUserCount     = await users.CountAsync(u => u.ProjectFirmId != null, cancellationToken)
        };

        var newProjectCount = await projects
            .CountAsync(p => p.CreatedAt >= startUtc && p.CreatedAt < endUtc, cancellationToken);

        var operationCounts = await history
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
            .GroupBy(o => o.OperationCode.CodeValue)
            .Select(g => new { CodeValue = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CodeValue, x => x.Count, cancellationToken);

        var today = new AdminDashboardTodayDto
        {
            NewProjectCount = newProjectCount,
            ApprovedCount   = operationCounts.GetValueOrDefault(CodeValues.Approved),
            RejectedCount   = operationCounts.GetValueOrDefault(CodeValues.Rejected)
        };

        // Hiç filtre yokken: grup şirketi bazlı yoğunluk (grubu olmayan firmalar "-" satırında toplanır).
        // Grup ya da belirli bir firma seçiliyken: firmaya göre kırılım — `projects` zaten süzülü
        // olduğu için firma seçiliyken doğal olarak tek satıra düşer (bilinçli, şimdilik böyle kalacak).
        string densityBy;
        List<DensityItemDto> density;

        if (gdGroupId is null && gdFirmId is null)
        {
            densityBy = "group";
            density = await projects
                .Where(p => p.CreatedAt >= startUtc && p.CreatedAt < endUtc)
                .GroupBy(p => new
                {
                    GroupId = p.ProjectFirmAuthorization.GasDistributionFirm.GroupId,
                    GroupName = p.ProjectFirmAuthorization.GasDistributionFirm.Group != null
                        ? p.ProjectFirmAuthorization.GasDistributionFirm.Group.Name
                        : null
                })
                .Select(g => new DensityItemDto
                {
                    Id           = g.Key.GroupId ?? 0,
                    Name         = g.Key.GroupName ?? "-",
                    ProjectCount = g.Count()
                })
                .OrderByDescending(x => x.ProjectCount)
                .ToListAsync(cancellationToken);
        }
        else
        {
            densityBy = "firm";
            density = await projects
                .Where(p => p.CreatedAt >= startUtc && p.CreatedAt < endUtc)
                .GroupBy(p => new
                {
                    FirmId   = p.ProjectFirmAuthorization.GasDistributionFirmId,
                    FirmName = p.ProjectFirmAuthorization.GasDistributionFirm.Title
                })
                .Select(g => new DensityItemDto
                {
                    Id           = g.Key.FirmId,
                    Name         = g.Key.FirmName,
                    ProjectCount = g.Count()
                })
                .OrderByDescending(x => x.ProjectCount)
                .ToListAsync(cancellationToken);
        }

        return new SuccessDataResult<AdminDashboardDto>(new AdminDashboardDto
        {
            Summary     = summary,
            Today       = today,
            DensityBy   = densityBy,
            Density     = density,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private static (DateTime StartUtc, DateTime EndUtc) GetTodayBoundsUtc()
    {
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(nowLocal.Date, TurkeyTimeZone);
        return (startUtc, startUtc.AddDays(1));
    }
}