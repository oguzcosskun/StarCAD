using Microsoft.EntityFrameworkCore;


namespace StarCAD.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (DbUpdateConcurrencyException)
        {
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { message = "Kayıt başka biri tarafından güncellendi. Sayfayı yenileyip tekrar deneyin." });
        }
        // İstemci isteği iptal ettiğinde (sayfadan ayrılma, dev'de StrictMode çift fetch) EF sorguları
        // bu istisnayı fırlatır — gerçek bir hata değil, karşı taraf zaten gitti; response yazmaya
        // çalışmıyoruz (yazsak da kimse okumaz) ve Error yerine Information'da logluyoruz.
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("İstek istemci tarafından iptal edildi: {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen hata");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new { message = "Beklenmeyen bir hata oluştu." });
        }
        
    }
}