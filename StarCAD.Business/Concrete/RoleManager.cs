using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.ViewModel.Roles;

namespace StarCAD.Business.Concrete;

public class RoleManager : IRoleService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);
    private const string RolesCacheKey = "roles:list";

    private readonly StarCADContext _db;
    private readonly IConnectionMultiplexer _redis;

    public RoleManager(StarCADContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<IDataResult<List<RoleListItemDto>>> GetListAsync(
        CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();

        var cached = await cache.StringGetAsync(RolesCacheKey);
        if (cached.HasValue)
            return new SuccessDataResult<List<RoleListItemDto>>(JsonSerializer.Deserialize<List<RoleListItemDto>>((string)cached!)!);

        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(RolesCacheKey, JsonSerializer.Serialize(roles), CacheTtl);

        return new SuccessDataResult<List<RoleListItemDto>>(roles);
    }
}