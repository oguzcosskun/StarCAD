namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Başarısız, veri taşıyan sonuç. Opsiyonel <see cref="StatusCode"/> için bkz. <see cref="ErrorResult"/>.
/// </summary>
public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult() : base(default, false)
    {
    }

    public ErrorDataResult(string message) : base(default, false, message)
    {
    }

    public ErrorDataResult(string message, int statusCode) : base(default, false, message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; } = 400;
}
