using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StarCAD.Business.Abstract;
using StarCAD.Business.Concrete;
using StarCAD.Business.Security;
using StarCAD.Core.Security;
using System.Text;

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Common.Constants;

namespace StarCAD.Business.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JWTAuth:ValidIssuerURL"],
                    ValidAudience = config["JWTAuth:ValidAudienceURL"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JWTAuth:SecretKey"]!))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdClaim  = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var versionClaim = context.Principal?.FindFirst(ClaimNames.TokenVersion)?.Value;

                        if (!int.TryParse(userIdClaim, out var userId) ||
                            !int.TryParse(versionClaim, out var tokenVersion))
                        {
                            context.Fail("Geçersiz token.");
                            return;
                        }

                        var db = context.HttpContext.RequestServices.GetRequiredService<StarCADContext>();

                        var currentVersion = await db.Users
                            .AsNoTracking()
                            .Where(u => u.Id == userId)
                            .Select(u => (int?)u.TokenVersion)
                            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

                        if (currentVersion is null || currentVersion.Value != tokenVersion)
                            context.Fail("Oturum sonlandırılmış.");
                    }
                };
            });

        services.AddScoped<IProjectVersionService, ProjectVersionManager>();
        services.AddScoped<IUnitSyncService, UnitSyncManager>();
        services.AddScoped<IProjectUnitService, ProjectUnitManager>();

        services.AddHostedService<OutboxProcessor>();

        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IRoleService, RoleManager>();

        services.AddScoped<IProjectFirmService, ProjectFirmManager>();
        services.AddScoped<IGasDistributionFirmService, GasDistributionFirmManager>();
        services.AddScoped<IProjectService, ProjectManager>();
        services.AddScoped<IGasDistributionGroupService, GasDistributionGroupManager>();
        services.AddScoped<ICodeGroupService, CodeGroupManager>();
        services.AddScoped<ICodeService, CodeManager>();
        services.AddScoped<ICityService, CityManager>();
        services.AddScoped<IAdminDashboardService, AdminDashboardManager>();
        services.AddScoped<IProjectApprovalService, ProjectApprovalManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IProjectFirmAuthorizationService, ProjectFirmAuthorizationManager>();
        services.AddScoped<IDocService, DocManager>();
        services.AddScoped<IPolicyService, PolicyManager>();
        services.AddScoped<IInsuranceCompanyService, InsuranceCompanyManager>();

        // TC kimlik koruması (AES-GCM + HMAC). Anahtarlar config'ten base64; prod'da env ile override.
        // Anahtar eksikse null-ref yerine kurulum yönlendiren net hata (fail-fast Program.cs'te tetiklenir).
        services.AddSingleton<INationalIdProtector>(_ =>
        {
            var key = config["Encryption:Key"];
            var hmac = config["Encryption:HmacKey"];
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(hmac))
                throw new InvalidOperationException(
                    "Encryption:Key ve Encryption:HmacKey ayarlanmalı. Kurulum: README 'Şifreleme anahtarları' (.env veya user-secrets).");

            return new NationalIdProtector(Convert.FromBase64String(key), Convert.FromBase64String(hmac));
        });

        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly);

        return services;
    }
}