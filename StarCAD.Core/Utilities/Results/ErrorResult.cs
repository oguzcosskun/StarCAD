namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Başarısız, veri taşımayan sonuç. Opsiyonel <see cref="StatusCode"/>, controller'ın hatayı doğru HTTP
/// koduna çevirmesi içindir (varsayılan 400; ör. bulunamadı 404, eşzamanlılık çakışması 409).
/// </summary>
public class ErrorResult : Result
{
    public ErrorResult() : base(false)
    {
    }

    public ErrorResult(string message) : base(false, message)
    {
    }

    public ErrorResult(string message, int statusCode) : base(false, message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; } = 400;
}
