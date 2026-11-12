namespace StarCAD.Core.Utilities.Results;

/// <summary>
/// Başarılı, veri taşımayan sonuç. Çağrı yerinde okunaklılık için Success=true'yu sabitler.
/// </summary>
public class SuccessResult : Result
{
    public SuccessResult() : base(true)
    {
    }

    public SuccessResult(string message) : base(true, message)
    {
    }
}
