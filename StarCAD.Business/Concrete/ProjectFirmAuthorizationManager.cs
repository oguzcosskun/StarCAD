using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.ProjectFirmAuthorizations;

namespace StarCAD.Business.Concrete;

public class ProjectFirmAuthorizationManager : IProjectFirmAuthorizationService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;

    public ProjectFirmAuthorizationManager(StarCADContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IDataResult<PagedResultDto<ProjectFirmAuthorizationListItemDto>>> GetListAsync(
        ProjectFirmAuthorizationListQueryDto query, CancellationToken cancellationToken = default)
    {
        var authorizations = _db.ProjectFirmAuthorizations.AsNoTracking().WhereVisibleTo(_currentUser);

        if (query.ProjectFirmId is { } projectFirmId)
            authorizations = authorizations.Where(a => a.ProjectFirmId == projectFirmId);

        if (query.GasDistributionFirmId is { } gasFirmId)
            authorizations = authorizations.Where(a => a.GasDistributionFirmId == gasFirmId);
        
        if (query.GasDistributionGroupId is { } groupId)
            authorizations = authorizations.Where(a => a.GasDistributionFirm.GroupId == groupId);

        var totalCount = await authorizations.CountAsync(cancellationToken);

        authorizations = ApplySort(authorizations, query.SortBy, query.SortDir);

        const int maxPageSize = 100;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await authorizations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ProjectFirmAuthorizationListItemDto
            {
                Id = a.Id,
                ProjectFirmId = a.ProjectFirmId,
                ProjectFirmName = a.ProjectFirm.Title,
                GasDistributionFirmId = a.GasDistributionFirmId,
                GasDistributionFirmName = a.GasDistributionFirm.Title,
                CertificateNumber = a.CertificateNumber,
                ValidFrom = a.ValidFrom,
                ValidTo = a.ValidTo
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<ProjectFirmAuthorizationListItemDto>>(new PagedResultDto<ProjectFirmAuthorizationListItemDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        });
    }

    private static IQueryable<ProjectFirmAuthorization> ApplySort(
        IQueryable<ProjectFirmAuthorization> authorizations, string sortBy, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            ProjectFirmAuthorizationSortKeys.ProjectFirmName => descending
                ? authorizations.OrderByDescending(a => a.ProjectFirm.Title).ThenByDescending(a => a.Id)
                : authorizations.OrderBy(a => a.ProjectFirm.Title).ThenBy(a => a.Id),
            _ => descending
                ? authorizations.OrderByDescending(a => a.ValidFrom).ThenByDescending(a => a.Id)
                : authorizations.OrderBy(a => a.ValidFrom).ThenBy(a => a.Id)
        };
    }

    public async Task<IDataResult<ProjectFirmAuthorizationListItemDto>> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var dto = await _db.ProjectFirmAuthorizations.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .Where(a => a.Id == id)
            .Select(a => new ProjectFirmAuthorizationListItemDto
            {
                Id = a.Id,
                ProjectFirmId = a.ProjectFirmId,
                ProjectFirmName = a.ProjectFirm.Title,
                GasDistributionFirmId = a.GasDistributionFirmId,
                GasDistributionFirmName = a.GasDistributionFirm.Title,
                CertificateNumber = a.CertificateNumber,
                ValidFrom = a.ValidFrom,
                ValidTo = a.ValidTo
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return new ErrorDataResult<ProjectFirmAuthorizationListItemDto>("Yetki kaydı bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<ProjectFirmAuthorizationListItemDto>(dto);
    }

    public async Task<IDataResult<ProjectFirmAuthorizationListItemDto>> CreateAsync(
        ProjectFirmAuthorizationCreateDto dto, CancellationToken cancellationToken = default)
    {
        var projectFirmExists = await _db.ProjectFirms.AnyAsync(f => f.Id == dto.ProjectFirmId, cancellationToken);
        if (!projectFirmExists)
            return new ErrorDataResult<ProjectFirmAuthorizationListItemDto>("Proje firması bulunamadı.", StatusCodes.Status404NotFound);

        var gasFirmExists = await _db.GasDistributionFirms.AnyAsync(f => f.Id == dto.GasDistributionFirmId, cancellationToken);
        if (!gasFirmExists)
            return new ErrorDataResult<ProjectFirmAuthorizationListItemDto>("Gaz dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);

        // IgnoreQueryFilters YOK — global filtre zaten sadece aktif satırları getiriyor,
        // filtreli index sayesinde pasif geçmiş kayıtlar bu kontrolü hiç etkilemiyor.
        var alreadyAuthorized = await _db.ProjectFirmAuthorizations
            .AnyAsync(a => a.ProjectFirmId == dto.ProjectFirmId && a.GasDistributionFirmId == dto.GasDistributionFirmId, cancellationToken);

        if (alreadyAuthorized)
            return new ErrorDataResult<ProjectFirmAuthorizationListItemDto>(
                "Bu proje firması bu gaz dağıtım firmasına zaten yetkilendirilmiş.", StatusCodes.Status409Conflict);

        var entity = new ProjectFirmAuthorization
        {
            ProjectFirmId = dto.ProjectFirmId,
            GasDistributionFirmId = dto.GasDistributionFirmId,
            CertificateNumber = dto.CertificateNumber.Trim(),
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo
        };

        _db.ProjectFirmAuthorizations.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new SuccessDataResult<ProjectFirmAuthorizationListItemDto>(
            await LoadAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException(),
            "Yetki kaydı oluşturuldu.");
    }

    public async Task<IResult> UpdateAsync(int id, ProjectFirmAuthorizationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ProjectFirmAuthorizations.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Yetki kaydı bulunamadı.", StatusCodes.Status404NotFound);

        entity.CertificateNumber = dto.CertificateNumber.Trim();
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;

        await _db.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Yetki kaydı güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ProjectFirmAuthorizations.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Yetki kaydı bulunamadı.", StatusCodes.Status404NotFound);

        _db.ProjectFirmAuthorizations.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Yetki kaydı silindi.");
    }

    private async Task<ProjectFirmAuthorizationListItemDto?> LoadAsync(int id, CancellationToken cancellationToken)
    {
        var result = await GetByIdAsync(id, cancellationToken);
        return result.Success ? result.Data : null;
    }
}