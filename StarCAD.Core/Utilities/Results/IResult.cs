namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Bir iş operasyonunun sonucunu taşıyan temel sözleşme: başarılı mı ve kullanıcıya mesaj.
/// Servisler exception fırlatmak/null döndürmek yerine bu zarfı döndürür.
/// </summary>
public interface IResult
{
    bool Success { get; }
    string Message { get; }
}
