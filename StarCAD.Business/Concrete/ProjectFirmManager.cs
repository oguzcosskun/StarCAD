using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Core.Search;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.ProjectFirms;
using StarCAD.ViewModel.Common;

namespace StarCAD.Business.Concrete;

public class ProjectFirmManager : IProjectFirmService
{
    private readonly StarCADContext _db;
    private readonly INationalIdProtector _protector;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ProjectFirmCreateDto> _createValidator;
    private readonly IValidator<ProjectFirmUpdateDto> _updateValidator;
    private readonly ISearchService _searchService;

    public ProjectFirmManager(
        StarCADContext db,
        INationalIdProtector protector,
        ICurrentUserService currentUser,
        IValidator<ProjectFirmCreateDto> createValidator,
        IValidator<ProjectFirmUpdateDto> updateValidator,
        ISearchService searchService)
    {
        _db = db;
        _protector = protector;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchService = searchService;
    }

    public async Task<IDataResult<PagedResultDto<ProjectFirmListItemDto>>> GetListAsync(ProjectFirmListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Global query filter yalnızca aktif (IsActive) firmaları getirir; WhereVisibleTo ise
        // rolü kadarını: admin hepsini, proje firması kendi firmasını, gaz dağıtım kendi bölgesini.
        var projectFirms = _db.ProjectFirms.AsNoTracking().WhereVisibleTo(_currentUser);

        if (query.GasDistributionFirmId is { } gasFirmId)
            projectFirms = projectFirms.Where(f =>
                f.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirmId == gasFirmId));

        if (query.GasDistributionGroupId is { } groupId)
            projectFirms = projectFirms.Where(f =>
                f.ProjectFirmAuthorizations.Any(a => a.GasDistributionFirm.GroupId == groupId));

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var matchedIds = await _searchService.SearchIdsAsync("project-firms", query.Q.Trim(), cancellationToken);
            projectFirms = projectFirms.Where(f => matchedIds.Contains(f.Id));
        }

        var totalCount = await projectFirms.CountAsync(cancellationToken);

        projectFirms = ApplySort(projectFirms, query.SortDir);

        const int maxPageSize = 100;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await projectFirms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new ProjectFirmListItemDto
            {
                Id = f.Id,
                CompanyType = f.CompanyType,
                Title = f.Title,
                TaxNumber = f.TaxNumber,
                ContactPerson = f.ContactPerson,
                Phone = f.Phone,
                Email = f.Email,
                AuthorizedGasDistributionFirms = f.ProjectFirmAuthorizations
                    .Where(a => a.ValidFrom <= DateTime.UtcNow && (a.ValidTo == null || a.ValidTo > DateTime.UtcNow))
                    .Select(a => new ProjectFirmAuthorizedGasDistributionFirmDto
                    {
                        Id = a.GasDistributionFirm.Id,
                        Title = a.GasDistributionFirm.Title
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<ProjectFirmListItemDto>>(new PagedResultDto<ProjectFirmListItemDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        });
    }

    private static IQueryable<ProjectFirm> ApplySort(IQueryable<ProjectFirm> projectFirms, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        // Tek sıralama alanı (Title) var; Id tiebreaker sayfalama sırasında kararlılık sağlar.
        return descending
            ? projectFirms.OrderByDescending(f => f.Title).ThenByDescending(f => f.Id)
            : projectFirms.OrderBy(f => f.Title).ThenBy(f => f.Id);
    } 
    
    public async Task<IDataResult<ProjectFirmDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Projeksiyon yerine entity: şahıs firmada TC'yi çözüp maskelemek bir servis çağrısı,
        // SQL'e çevrilemez. Tek kayıt okuması olduğu için maliyet önemsiz.
        // WhereVisibleTo: başka firmanın kaydına erişim 404 döner (var olmayanla aynı) — bilgi sızmaz.
        var entity = await _db.ProjectFirms.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (entity is null)
            return new ErrorDataResult<ProjectFirmDetailDto>("Proje firması bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<ProjectFirmDetailDto>(ToDetail(entity));
    }

    public async Task<IDataResult<ProjectFirmDetailDto>> CreateAsync(ProjectFirmCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<ProjectFirmDetailDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = new ProjectFirm
        {
            CompanyType = dto.CompanyType,
            Title = dto.Title.Trim(),
            AccountingCode = dto.AccountingCode,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Phone2 = dto.Phone2,
            Address = dto.Address
        };

        var identity = await ApplyIdentityAsync(entity, dto.CompanyType, dto.TaxNumber, dto.NationalIdNumber, id: null, cancellationToken);
        if (identity is not null)
            return new ErrorDataResult<ProjectFirmDetailDto>(identity.Message, ((ErrorResult)identity).StatusCode);

        _db.ProjectFirms.Add(entity);
        await _db.SaveChangesWithOutboxAsync(nameof(ProjectFirm), "Index", () => entity.Id, cancellationToken);

        return new SuccessDataResult<ProjectFirmDetailDto>(ToDetail(entity), "Proje firması eklendi.");
    }

    public async Task<IResult> UpdateAsync(int id, ProjectFirmUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.ProjectFirms.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Proje firması bulunamadı.", StatusCodes.Status404NotFound);

        entity.CompanyType = dto.CompanyType;
        entity.Title = dto.Title.Trim();
        entity.AccountingCode = dto.AccountingCode;
        entity.ContactPerson = dto.ContactPerson;
        entity.Email = dto.Email;
        entity.Phone = dto.Phone;
        entity.Phone2 = dto.Phone2;
        entity.Address = dto.Address;

        var identity = await ApplyIdentityAsync(entity, dto.CompanyType, dto.TaxNumber, dto.NationalIdNumber, id, cancellationToken);
        if (identity is not null)
            return identity;

        await _db.SaveChangesWithOutboxAsync(nameof(ProjectFirm), "Index", () => entity.Id, cancellationToken);
        return new SuccessResult("Proje firması güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ProjectFirms.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Proje firması bulunamadı.", StatusCodes.Status404NotFound);

        // Remove çağrısı SaveChanges override'ında soft-delete'e (IsActive=false) dönüştürülür.
        _db.ProjectFirms.Remove(entity);
        await _db.SaveChangesWithOutboxAsync(nameof(ProjectFirm), "Delete", () => entity.Id, cancellationToken);

        return new SuccessResult("Proje firması silindi.");
    }

    /// <summary>
    /// CompanyType'a göre kimlik alanlarını ayarlar: şahıs (1) → TC şifrele + hash + benzersizlik;
    /// tüzel (2) → vergi no, TC alanları null. Hata varsa ErrorResult, başarıda null döner.
    /// </summary>
    private async Task<ErrorResult?> ApplyIdentityAsync(
        ProjectFirm entity, byte companyType, string? taxNumber, string? nationalId, int? id,
        CancellationToken cancellationToken)
    {
        if (companyType == 1)
        {
            var tc = nationalId!.Trim();
            var hash = _protector.Hash(tc);

            var duplicate = await _db.ProjectFirms
                .AnyAsync(f => f.NationalIdNumberHash == hash && (id == null || f.Id != id), cancellationToken);
            if (duplicate)
                return new ErrorResult("Bu TC kimlik numarasıyla kayıtlı bir şahıs firma zaten var.", StatusCodes.Status409Conflict);

            entity.NationalIdNumber = _protector.Encrypt(tc);
            entity.NationalIdNumberHash = hash;
            entity.TaxNumber = null;
        }
        else
        {
            entity.TaxNumber = taxNumber;
            entity.NationalIdNumber = null;
            entity.NationalIdNumberHash = null;
        }

        return null;
    }

    private ProjectFirmDetailDto ToDetail(ProjectFirm f) => new()
    {
        Id = f.Id,
        CompanyType = f.CompanyType,
        Title = f.Title,
        TaxNumber = f.TaxNumber,
        // Şahıs firma: çöz → maskele (son 4 hane). Düz TC asla dönmez.
        NationalIdNumber = f.CompanyType == 1 && f.NationalIdNumber is not null
            ? _protector.Mask(_protector.Decrypt(f.NationalIdNumber))
            : null,
        AccountingCode = f.AccountingCode,
        ContactPerson = f.ContactPerson,
        Email = f.Email,
        Phone = f.Phone,
        Phone2 = f.Phone2,
        Address = f.Address
    };
}
