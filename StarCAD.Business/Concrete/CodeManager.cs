using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Codes;
using System.Text.Json;
using static StarCAD.Common.Constants.CacheKeys;

namespace StarCAD.Business.Concrete;

public class CodeManager : ICodeService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    private readonly StarCADContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly IValidator<CodeCreateDto> _createValidator;
    private readonly IValidator<CodeUpdateDto> _updateValidator;

    public CodeManager(
        StarCADContext db,
        IConnectionMultiplexer redis,
        IValidator<CodeCreateDto> createValidator,
        IValidator<CodeUpdateDto> updateValidator)
    {
        _db = db;
        _redis = redis;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IDataResult<List<CodeListItemDto>>> GetListByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();
        var cacheKey = CodesByGroup(groupId);

        var cached = await cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return new SuccessDataResult<List<CodeListItemDto>>(
                JsonSerializer.Deserialize<List<CodeListItemDto>>((string)cached!)!);

        var list = await _db.Codes.AsNoTracking()
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.Name)
            .Select(c => new CodeListItemDto
            {
                Id = c.Id,
                GroupId = c.GroupId,
                GroupName = c.Group.GroupName,
                Name = c.Name,
                CodeValue = c.CodeValue
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<CodeListItemDto>>(list);
    }

    public async Task<IDataResult<List<CodeListItemDto>>> GetListByGroupNameAsync(string groupName, CancellationToken cancellationToken = default)
    {
        var name = groupName.Trim();
        var cache = _redis.GetDatabase();
        var cacheKey = CodesByGroupName(name);

        var cached = await cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return new SuccessDataResult<List<CodeListItemDto>>(
                JsonSerializer.Deserialize<List<CodeListItemDto>>((string)cached!)!);

        var list = await _db.Codes.AsNoTracking()
            .Where(c => c.Group.GroupName == name)
            .OrderBy(c => c.Name)
            .Select(c => new CodeListItemDto
            {
                Id = c.Id,
                GroupId = c.GroupId,
                GroupName = c.Group.GroupName,
                Name = c.Name,
                CodeValue = c.CodeValue
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<CodeListItemDto>>(list);
    }

    public async Task<IDataResult<CodeListItemDto>> CreateAsync(CodeCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<CodeListItemDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var group = await _db.CodeGroups.AsNoTracking()
            .Where(g => g.Id == dto.GroupId)
            .Select(g => new { g.Id, g.GroupName })
            .FirstOrDefaultAsync(cancellationToken);

        if (group is null)
            return new ErrorDataResult<CodeListItemDto>("Seçilen kod grubu bulunamadı.", StatusCodes.Status404NotFound);

        var codeValue = dto.CodeValue.Trim();

        var existing = await _db.Codes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.GroupId == dto.GroupId && c.CodeValue == codeValue, cancellationToken);

        if (existing is not null)
            return new ErrorDataResult<CodeListItemDto>(
                existing.IsActive
                    ? "Bu kod değeri bu grupta zaten kullanılıyor."
                    : "Bu kod değeri bu grupta silinmiş bir koda ait, tekrar kullanılamaz.",
                StatusCodes.Status409Conflict);

        var entity = new Code
        {
            GroupId = dto.GroupId,
            Name = dto.Name.Trim(),
            CodeValue = codeValue
        };

        _db.Codes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync(CodesByGroup(entity.GroupId));
        await cache.KeyDeleteAsync(CodesByGroupName(group.GroupName));
        await cache.KeyDeleteAsync(CodeGroupsList);

        return new SuccessDataResult<CodeListItemDto>(new CodeListItemDto
        {
            Id = entity.Id,
            GroupId = entity.GroupId,
            GroupName = group.GroupName,
            Name = entity.Name,
            CodeValue = entity.CodeValue
        }, "Kod eklendi.");
    }

    public async Task<IResult> UpdateAsync(int id, CodeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.Codes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Kod bulunamadı.", StatusCodes.Status404NotFound);

        var codeValue = dto.CodeValue.Trim();

        var conflict = await _db.Codes
            .IgnoreQueryFilters()
            .AnyAsync(c => c.GroupId == entity.GroupId && c.CodeValue == codeValue && c.Id != id, cancellationToken);

        if (conflict)
            return new ErrorResult("Bu kod değeri bu grupta başka bir koda ait.", StatusCodes.Status409Conflict);

        entity.Name = dto.Name.Trim();
        entity.CodeValue = codeValue;

        await _db.SaveChangesAsync(cancellationToken);

        var groupName = await _db.CodeGroups.AsNoTracking()
            .Where(g => g.Id == entity.GroupId)
            .Select(g => g.GroupName)
            .FirstOrDefaultAsync(cancellationToken);

        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync(CodesByGroup(entity.GroupId));
        if (groupName is not null)
            await cache.KeyDeleteAsync(CodesByGroupName(groupName));

        return new SuccessResult("Kod güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Codes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
            return new ErrorResult("Kod bulunamadı.", StatusCodes.Status404NotFound);

        var usedByProject = await _db.Projects
            .AnyAsync(p => p.ProjectTypeCodeId == id || p.HeatingTypeCodeId == id, cancellationToken);

        var usedByOperationHistory = await _db.OperationHistories
            .AnyAsync(o => o.OperationCodeId == id, cancellationToken);

        if (usedByProject || usedByOperationHistory)
            return new ErrorResult("Bu kod kullanımda olduğu için silinemez.", StatusCodes.Status409Conflict);

        var groupId = entity.GroupId;

        var groupName = await _db.CodeGroups.AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => g.GroupName)
            .FirstOrDefaultAsync(cancellationToken);

        _db.Codes.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync(CodesByGroup(groupId));
        if (groupName is not null)
            await cache.KeyDeleteAsync(CodesByGroupName(groupName));
        await cache.KeyDeleteAsync(CodeGroupsList);

        return new SuccessResult("Kod silindi.");
    }
}