using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.CodeGroups;
using System.Text.Json;
using static StarCAD.Common.Constants.CacheKeys;


namespace StarCAD.Business.Concrete;

public class CodeGroupManager : ICodeGroupService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly StarCADContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly IValidator<CodeGroupCreateDto> _createValidator;
    private readonly IValidator<CodeGroupUpdateDto> _updateValidator;

    public CodeGroupManager(
        StarCADContext db,
        IConnectionMultiplexer redis,
        IValidator<CodeGroupCreateDto> createValidator,
        IValidator<CodeGroupUpdateDto> updateValidator)
    {
        _db = db;
        _redis = redis;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IDataResult<List<CodeGroupListItemDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();

        var cached = await cache.StringGetAsync(CodeGroupsList);
        if (cached.HasValue)
            return new SuccessDataResult<List<CodeGroupListItemDto>>(
                JsonSerializer.Deserialize<List<CodeGroupListItemDto>>((string)cached!)!);

        var list = await _db.CodeGroups.AsNoTracking()
            .OrderBy(g => g.GroupName)
            .Select(g => new CodeGroupListItemDto
            {
                Id = g.Id,
                GroupName = g.GroupName,
                CodeCount = g.Codes.Count(c => c.IsActive)
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(CodeGroupsList, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<CodeGroupListItemDto>>(list);
    }

    public async Task<IDataResult<CodeGroupListItemDto>> CreateAsync(CodeGroupCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<CodeGroupListItemDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var groupName = dto.GroupName.Trim();

        var existing = await _db.CodeGroups
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GroupName == groupName, cancellationToken);

        if (existing is not null)
            return new ErrorDataResult<CodeGroupListItemDto>(
                existing.IsActive
                    ? "Bu kod grubu adı zaten kullanılıyor."
                    : "Bu kod grubu adı silinmiş bir gruba ait, tekrar kullanılamaz.",
                StatusCodes.Status409Conflict);

        var entity = new CodeGroup { GroupName = groupName };

        _db.CodeGroups.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(CodeGroupsList);

        return new SuccessDataResult<CodeGroupListItemDto>(
            new CodeGroupListItemDto { Id = entity.Id, GroupName = entity.GroupName, CodeCount = 0 },
            "Kod grubu eklendi.");
    }

    public async Task<IResult> UpdateAsync(int id, CodeGroupUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.CodeGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Kod grubu bulunamadı.", StatusCodes.Status404NotFound);

        var groupName = dto.GroupName.Trim();

        var conflict = await _db.CodeGroups
            .IgnoreQueryFilters()
            .AnyAsync(g => g.GroupName == groupName && g.Id != id, cancellationToken);

        if (conflict)
            return new ErrorResult("Bu kod grubu adı başka bir gruba ait.", StatusCodes.Status409Conflict);

        entity.GroupName = groupName;

        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(CodeGroupsList);

        return new SuccessResult("Kod grubu güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.CodeGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Kod grubu bulunamadı.", StatusCodes.Status404NotFound);

        var hasCodes = await _db.Codes.AnyAsync(c => c.GroupId == id, cancellationToken);
        if (hasCodes)
            return new ErrorResult("Gruba bağlı kodlar bulunduğu için silinemez.", StatusCodes.Status409Conflict);

        _db.CodeGroups.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _redis.GetDatabase().KeyDeleteAsync(CodeGroupsList);

        return new SuccessResult("Kod grubu silindi.");
    }
}