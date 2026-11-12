using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.GasDistributionGroups;
using System.Text.Json;
using static StarCAD.Common.Constants.CacheKeys;

namespace StarCAD.Business.Concrete;

public class GasDistributionGroupManager : IGasDistributionGroupService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly StarCADContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly IValidator<GasDistributionGroupCreateDto> _createValidator;
    private readonly IValidator<GasDistributionGroupUpdateDto> _updateValidator;

    public GasDistributionGroupManager(
        StarCADContext db,
        IConnectionMultiplexer redis,
        IValidator<GasDistributionGroupCreateDto> createValidator,
        IValidator<GasDistributionGroupUpdateDto> updateValidator)
    {
        _db = db;
        _redis = redis;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IDataResult<List<GasDistributionGroupListItemDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();

        var cached = await cache.StringGetAsync(GasDistributionGroupsList);
        if (cached.HasValue)
            return new SuccessDataResult<List<GasDistributionGroupListItemDto>>(JsonSerializer.Deserialize<List<GasDistributionGroupListItemDto>>((string)cached!)!);

        var list = await _db.GasDistributionGroups.AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GasDistributionGroupListItemDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(GasDistributionGroupsList, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<GasDistributionGroupListItemDto>>(list);
    }

    public async Task<IDataResult<GasDistributionGroupListItemDto>> CreateAsync(GasDistributionGroupCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<GasDistributionGroupListItemDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var name = dto.Name.Trim();

        var existing = await _db.GasDistributionGroups
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Name == name, cancellationToken);

        if (existing is not null)
            return new ErrorDataResult<GasDistributionGroupListItemDto>(
                existing.IsActive
                    ? "Bu grup firması adı zaten kullanılıyor."
                    : "Bu grup firması adı silinmiş bir kayda ait, tekrar kullanılamaz.",
                StatusCodes.Status409Conflict);

        var entity = new GasDistributionGroup { Name = name };

        _db.GasDistributionGroups.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(GasDistributionGroupsList);

        return new SuccessDataResult<GasDistributionGroupListItemDto>(ToListItem(entity), "Grup firması eklendi.");
    }

    public async Task<IResult> UpdateAsync(int id, GasDistributionGroupUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.GasDistributionGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Grup firması bulunamadı.", StatusCodes.Status404NotFound);

        var name = dto.Name.Trim();

        var conflict = await _db.GasDistributionGroups
            .IgnoreQueryFilters()
            .AnyAsync(g => g.Name == name && g.Id != id, cancellationToken);

        if (conflict)
            return new ErrorResult("Bu grup firması adı başka bir kayda ait.", StatusCodes.Status409Conflict);

        entity.Name = name;

        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(GasDistributionGroupsList);

        return new SuccessResult("Grup firması güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.GasDistributionGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Grup firması bulunamadı.", StatusCodes.Status404NotFound);

        var inUse = await _db.GasDistributionFirms
            .AnyAsync(f => f.GroupId == id, cancellationToken);

        if (inUse)
            return new ErrorResult("Bu gruba bağlı firmalar bulunduğu için silinemez.", StatusCodes.Status409Conflict);

        _db.GasDistributionGroups.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(GasDistributionGroupsList);

        return new SuccessResult("Grup firması silindi.");
    }

    private static GasDistributionGroupListItemDto ToListItem(GasDistributionGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name
    };
}