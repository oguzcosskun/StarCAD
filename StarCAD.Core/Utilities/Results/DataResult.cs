namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Veri taşıyan operasyon sonuçlarının temel gövdesi.
/// </summary>
public class DataResult<T> : Result, IDataResult<T>
{
    public DataResult(T? data, bool success) : base(success)
    {
        Data = data;
    }

    public DataResult(T? data, bool success, string message) : base(success, message)
    {
        Data = data;
    }

    public T? Data { get; }
}
