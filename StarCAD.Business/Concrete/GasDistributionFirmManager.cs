using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Core.Search;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.GasDistributionFirms;
using StarCAD.ViewModel.Common;


namespace StarCAD.Business.Concrete;

public class GasDistributionFirmManager : IGasDistributionFirmService
{
    private readonly StarCADContext _db;
    private readonly IValidator<GasDistributionFirmCreateDto> _createValidator;
    private readonly IValidator<GasDistributionFirmUpdateDto> _updateValidator;
    private readonly ISearchService _searchService;

    public GasDistributionFirmManager(
        StarCADContext db,
        IValidator<GasDistributionFirmCreateDto> createValidator,
        IValidator<GasDistributionFirmUpdateDto> updateValidator,
        ISearchService searchService)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchService = searchService;
    }

    public async Task<IDataResult<PagedResultDto<GasDistributionFirmListItemDto>>> GetListAsync(GasDistributionFirmListQueryDto query, CancellationToken cancellationToken = default)
    {
        var firms = _db.GasDistributionFirms.AsNoTracking().AsQueryable();

        if (query.GasDistributionGroupId is { } groupId)
            firms = firms.Where(f => f.GroupId == groupId);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var matchedIds = await _searchService.SearchIdsAsync("gas-distribution-firms", query.Q.Trim(), cancellationToken);
            firms = firms.Where(f => matchedIds.Contains(f.Id));
        }

        var totalCount = await firms.CountAsync(cancellationToken);

        firms = ApplySort(firms, query.SortBy, query.SortDir);

        const int maxPageSize = 100;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await firms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new GasDistributionFirmListItemDto
            {
                Id = f.Id,
                Title = f.Title,
                CompanyNumber = f.CompanyNumber,
                GroupId = f.GroupId,
                GroupName = f.Group!.Name
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<GasDistributionFirmListItemDto>>(new PagedResultDto<GasDistributionFirmListItemDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        });
    }

    private static IQueryable<GasDistributionFirm> ApplySort(
        IQueryable<GasDistributionFirm> firms, string sortBy, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            GasDistributionFirmSortKeys.CompanyNumber => descending
                ? firms.OrderByDescending(f => f.CompanyNumber).ThenByDescending(f => f.Id)
                : firms.OrderBy(f => f.CompanyNumber).ThenBy(f => f.Id),
            GasDistributionFirmSortKeys.GroupName => descending
                ? firms.OrderByDescending(f => f.Group!.Name).ThenByDescending(f => f.Id)
                : firms.OrderBy(f => f.Group!.Name).ThenBy(f => f.Id),
            _ => descending
                ? firms.OrderByDescending(f => f.Title).ThenByDescending(f => f.Id)
                : firms.OrderBy(f => f.Title).ThenBy(f => f.Id)
        };
    }

    public async Task<IDataResult<GasDistributionFirmDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dto = await _db.GasDistributionFirms.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new GasDistributionFirmDetailDto
            {
                Id = f.Id,
                Title = f.Title,
                CompanyNumber = f.CompanyNumber,
                GroupId = f.GroupId,
                GroupName = f.Group!.Name,
                Description = f.Description,
                ContactPerson = f.ContactPerson,
                Phone = f.Phone,
                Address = f.Address
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return new ErrorDataResult<GasDistributionFirmDetailDto>("Gaz dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<GasDistributionFirmDetailDto>(dto);
    }

    public async Task<IDataResult<GasDistributionFirmDetailDto>> CreateAsync(GasDistributionFirmCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<GasDistributionFirmDetailDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var existing = await _db.GasDistributionFirms
            .IgnoreQueryFilters() // HasQueryFilter özelliği ile silinmiş (IsActive=false) firmaları da kontrol etmemizi sağlayan yer.
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.CompanyNumber == dto.CompanyNumber, cancellationToken);

        if (existing is not null)
            return new ErrorDataResult<GasDistributionFirmDetailDto>(
                existing.IsActive
                    ? "Bu firma numarası zaten kullanılıyor."
                    : "Bu firma numarası silinmiş bir firmaya ait, tekrar kullanılamaz.",
                StatusCodes.Status409Conflict);

        // Grup opsiyonel; gönderilmişse gerçekten var mı diye bakılıyor, adı da
        // dönüş DTO'su için aynı sorgudan alınıyor.
        string? groupName = null;
        if (dto.GroupId.HasValue)
        {
            var group = await _db.GasDistributionGroups.AsNoTracking()
                .Where(g => g.Id == dto.GroupId.Value)
                .Select(g => new { g.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (group is null)
                return new ErrorDataResult<GasDistributionFirmDetailDto>("Grup firması bulunamadı.", StatusCodes.Status404NotFound);

            groupName = group.Name;
        }

        var entity = new GasDistributionFirm
        {
            Title = dto.Title.Trim(),
            CompanyNumber = dto.CompanyNumber,
            GroupId = dto.GroupId,
            Description = dto.Description,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Address = dto.Address
        };

        _db.GasDistributionFirms.Add(entity);
        await _db.SaveChangesWithOutboxAsync(nameof(GasDistributionFirm), "Index", () => entity.Id, cancellationToken);

        return new SuccessDataResult<GasDistributionFirmDetailDto>(ToDetail(entity, groupName), "Gaz dağıtım firması eklendi.");
    }

    public async Task<IResult> UpdateAsync(int id, GasDistributionFirmUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.GasDistributionFirms.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Gaz dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);

        // Numara başka bir kayda aitse çakışma var demektir. f.Id != id kaydın kendisini hariç tutmak için kullanılıyor.
        var conflict = await _db.GasDistributionFirms
            .IgnoreQueryFilters()
            .AnyAsync(f => f.CompanyNumber == dto.CompanyNumber && f.Id != id, cancellationToken);

        if (conflict)
            return new ErrorResult("Bu firma numarası başka bir firmaya ait.", StatusCodes.Status409Conflict);

        if (dto.GroupId.HasValue)
        {
            var groupExists = await _db.GasDistributionGroups
                .AnyAsync(g => g.Id == dto.GroupId.Value, cancellationToken);

            if (!groupExists)
                return new ErrorResult("Grup firması bulunamadı.", StatusCodes.Status404NotFound);
        }

        entity.Title = dto.Title.Trim();
        entity.CompanyNumber = dto.CompanyNumber;
        entity.GroupId = dto.GroupId;
        entity.Description = dto.Description;
        entity.ContactPerson = dto.ContactPerson;
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;

        await _db.SaveChangesWithOutboxAsync(nameof(GasDistributionFirm), "Index", () => entity.Id, cancellationToken);
        return new SuccessResult("Gaz dağıtım firması güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GasDistributionFirms.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Gaz dağıtım firması bulunamadı.", StatusCodes.Status404NotFound);

        // Remove çağrısı SaveChanges override'ında soft-delete'e (IsActive=false) dönüştürülür.
        _db.GasDistributionFirms.Remove(entity);
        await _db.SaveChangesWithOutboxAsync(nameof(GasDistributionFirm), "Delete", () => entity.Id, cancellationToken);

        return new SuccessResult("Gaz dağıtım firması silindi.");
    }

    private static GasDistributionFirmDetailDto ToDetail(GasDistributionFirm f, string? groupName) => new()
    {
        Id = f.Id,
        Title = f.Title,
        CompanyNumber = f.CompanyNumber,
        GroupId = f.GroupId,
        GroupName = groupName,
        Description = f.Description,
        ContactPerson = f.ContactPerson,
        Phone = f.Phone,
        Address = f.Address
    };
}