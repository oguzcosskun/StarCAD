using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace StarCAD.Api.OpenApi;

/// <summary>
/// OpenAPI dokümanına Bearer security scheme'i ekler. Scalar yalnızca dokümanda
/// tanımlı şemayı görebilir — middleware'de JWT kurulu olması yetmez.
/// Token bir kez girilir, tüm isteklere otomatik eklenir.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        // JWT kayıtlı değilse dokümanı kirletme.
        if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
            return;

        document.Components ??= new OpenApiComponents();

        document.AddComponent("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "/api/auth/login ucundan alınan token"
        });

        document.AddComponent("OAuth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Kullanıcı adı/şifre ile token alır — kopyala-yapıştır yok.",
            Flows = new OpenApiOAuthFlows
            {
                Password = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri("/dev/token", UriKind.Relative),
                    Scopes = new Dictionary<string, string>()
                }
            }
        });
        
        var requirements = new List<OpenApiSecurityRequirement>
        {
            new() { [new OpenApiSecuritySchemeReference("OAuth2", document)] = [] },
            new() { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }
        };

        foreach (var operation in document.Paths.Values .SelectMany(path => path.Operations ?? new Dictionary<HttpMethod, OpenApiOperation>()))
        {
            operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
            foreach (var requirement in requirements)
                operation.Value.Security.Add(requirement);
        }
    }
}