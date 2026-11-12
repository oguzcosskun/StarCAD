namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Veri taşımayan operasyon sonuçlarının temel gövdesi (ör. silme/güncelleme).
/// </summary>
public class Result : IResult
{
    public Result(bool success)
    {
        Success = success;
    }

    public Result(bool success, string message) : this(success)
    {
        Message = message;
    }

    public bool Success { get; }

    public string Message { get; } = string.Empty;
}
