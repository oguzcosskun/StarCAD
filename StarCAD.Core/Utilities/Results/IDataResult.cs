namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Veri taşıyan operasyon sonucu (ör. bir DTO döndüren sorgu).
/// </summary>
public interface IDataResult<out T> : IResult
{
    T? Data { get; }
}
