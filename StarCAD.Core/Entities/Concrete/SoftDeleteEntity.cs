namespace StarCAD.Core.Entities.Concrete;

/// <summary>
/// Soft-delete destekleyen entity'lerin temel sınıfı. Fiziksel silme yerine <see cref="IsActive"/> = false
/// ile pasifleştirilen tablolar bundan türer. Append-only log, junction ve checkpoint tabloları (IsActive
/// taşımayanlar) doğrudan <see cref="BaseEntity"/>'den türer.
/// </summary>
public abstract class SoftDeleteEntity : BaseEntity
{
    public bool IsActive { get; set; } = true;
}
