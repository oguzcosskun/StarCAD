namespace StarCAD.Core.Entities.Concrete;

/// <summary>
/// Tüm entity'lerin miras aldığı ortak temel sınıf.
/// Her tabloda bulunan birincil anahtarı tek yerde tanımlar. Soft-delete (IsActive) yalnızca
/// pasifleştirilebilen tablolarda olduğu için burada değil, <see cref="SoftDeleteEntity"/>'de tanımlıdır.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
