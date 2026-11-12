using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using StarCAD.Api.Filters;
using StarCAD.Api.Middleware;
using StarCAD.Api.Security;
using StarCAD.Business.Extensions;
using StarCAD.Core.Security;
using StarCAD.Core.Storage;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.DataAccess.Diagnostics;
using StarCAD.DataAccess.Extensions;
using StarCAD.DataAccess.Seed;
using StarCAD.Api.OpenApi;
using StarCAD.Business.Abstract;
using StarCAD.ViewModel.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

const string corsPolicy = "StarCadCors";

builder.Services.AddControllers(options =>
    options.Filters.Add<FluentValidationActionFilter>());
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddBusinessServices(builder.Configuration);
builder.Services.AddDataAccessServices(builder.Configuration);

builder.Services.AddCors(o => o.AddPolicy(corsPolicy, p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));   // dev; prod'da origin kısıtlanır

var app = builder.Build();

// Fail-fast: şifreleme anahtarları eksik/geçersizse UYGULAMA AÇILIŞINDA net hata ver
// (ilk /api/projectfirms isteğinde gizemli 500 yerine). EF design-time host'u builder.Build()'te
// durduğu için bu satır 'dotnet ef' migration komutlarını ETKİLEMEZ.
_ = app.Services.GetRequiredService<INationalIdProtector>();

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StarCADContext>();

    // Migration geçmişi squash'lanınca (ya da başka bir dalda yeniden yazılınca)
    // mevcut yerel veritabanları EF'e "hiç migration uygulanmamış" gibi görünür ve
    // MigrateAsync zaten var olan tabloları yeniden oluşturmaya çalışıp SQL 2714 ile
    // düşer. Ham yığın izinden "veritabanımı sıfırlamam gerekiyor" sonucuna kimse
    // varmıyor — üstelik belirti frontend'de `ERR_CONNECTION_RESET` olarak görünüyor,
    // çünkü uygulama app.Run()'a hiç gelmiyor ama konteyner ayakta kalıyor.
    var unknownAppliedMigrations =
        await MigrationHistoryCheck.GetUnknownAppliedMigrationsAsync(db.Database);

    if (unknownAppliedMigrations.Count > 0)
    {
        app.Logger.LogWarning("{Message}", MigrationHistoryCheck.BuildMismatchMessage(unknownAppliedMigrations));
    }

    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception exception) when (unknownAppliedMigrations.Count > 0)
    {
        // Uyuşmazlık ÖNCEDEN saptandığında bile akış durdurulmuyor, yalnız uyarılıyordu:
        // dal değiştirmiş bir geliştiricide fazladan bir migration bulunabilir ve
        // migration yine de sorunsuz uygulanabilir. Asıl mesajı, gerçekten düştüğünde
        // en görünür yere koyuyoruz.
        throw new InvalidOperationException(
            MigrationHistoryCheck.BuildMismatchMessage(unknownAppliedMigrations),
            exception);
    }

    await MasterDataSeeder.SeedAsync(db);
    await DevelopmentDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IFileStorageService>());

    // Scalar'ın password flow'u OAuth2 sözleşmesi bekler: form-urlencoded istek,
    // // access_token içeren JSON yanıt. LoginAsync'i sarar, yalnızca Development'ta açılır.
    app.MapPost("/dev/token", async (HttpRequest request, IAuthService authService, CancellationToken ct) =>
        {
            var form = await request.ReadFormAsync(ct);

            var result = await authService.LoginAsync(new LoginRequest
            {
                Username = form["username"].ToString(),
                Password = form["password"].ToString()
            }, ct);

            if (!result.Success || result.Data is null)
                return Results.BadRequest(new { error = "invalid_grant", error_description = result.Message });

            return Results.Ok(new
            {
                access_token = result.Data.Token,
                token_type = "Bearer",
                expires_in = (int)(result.Data.ExpiresAt - DateTime.UtcNow).TotalSeconds
            });
        })
        .AllowAnonymous()
        .ExcludeFromDescription();

    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options => options
            .AddPreferredSecuritySchemes("OAuth2")
            .AddPasswordFlow("OAuth2", flow =>
            {
                flow.TokenUrl = "/dev/token";
                flow.Username = "admin";       // ön dolu; başka kullanıcı için üzerine yaz
                flow.Password = "Admin1234";
            })
            .EnablePersistentAuthentication())
            .AllowAnonymous();
}
else
{
    // İl/ilçe master veridir
    using var masterScope = app.Services.CreateScope();
    await MasterDataSeeder.SeedAsync(masterScope.ServiceProvider.GetRequiredService<StarCADContext>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
