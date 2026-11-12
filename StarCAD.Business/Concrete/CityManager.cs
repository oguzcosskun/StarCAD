using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StarCAD.Business.Abstract;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.ViewModel.Cities;
using static StarCAD.Common.Constants.CacheKeys;

namespace StarCAD.Business.Concrete;

public class CityManager : ICityService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);
    private readonly StarCADContext _db;
    private readonly IConnectionMultiplexer _redis;

    public CityManager(StarCADContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<IDataResult<List<CityListItemDto>>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();

        var cached = await cache.StringGetAsync(CitiesCacheKey);
        if (cached.HasValue)
            return new SuccessDataResult<List<CityListItemDto>>(JsonSerializer.Deserialize<List<CityListItemDto>>((string)cached!)!);

        var list = await _db.Cities.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CityListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                PlateCode = c.PlateCode
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(CitiesCacheKey, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<CityListItemDto>>(list);
    }

    public async Task<IDataResult<List<DistrictListItemDto>>> GetDistrictsAsync(int cityId, CancellationToken cancellationToken = default)
    {
        var cache = _redis.GetDatabase();
        var cacheKey = DistrictsByCity(cityId);

        var cached = await cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return new SuccessDataResult<List<DistrictListItemDto>>(JsonSerializer.Deserialize<List<DistrictListItemDto>>((string)cached!)!);

        var cityExists = await _db.Cities.AnyAsync(c => c.Id == cityId, cancellationToken);
        if (!cityExists)
            return new ErrorDataResult<List<DistrictListItemDto>>("İl bulunamadı.", StatusCodes.Status404NotFound);

        var list = await _db.Districts.AsNoTracking()
            .Where(d => d.CityId == cityId)
            .OrderBy(d => d.Name)
            .Select(d => new DistrictListItemDto
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToListAsync(cancellationToken);

        await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(list), CacheTtl);

        return new SuccessDataResult<List<DistrictListItemDto>>(list);
    }
}