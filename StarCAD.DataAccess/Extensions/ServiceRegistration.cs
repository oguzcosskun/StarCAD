using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarCAD.Core.Storage;
using StarCAD.DataAccess.Concrete;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.DataAccess.Configurations;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using StarCAD.Core.Search; 

namespace StarCAD.DataAccess.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MinioSettings>(config.GetSection("MinioSettings"));
        services.AddSingleton<IFileStorageService, MinioStorageManager>();
        
        services.Configure<RedisSettings>(config.GetSection("RedisSettings"));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
            return ConnectionMultiplexer.Connect(settings.Endpoint);
        });
        
        services.Configure<ElasticSettings>(config.GetSection("ElasticSettings"));
        services.AddSingleton<ISearchService, ElasticSearchManager>();
        
        services.AddDbContext<StarCADContext>(options =>
            options.UseSqlServer(config.GetConnectionString("StarCADContext")));

        return services;
    }
}