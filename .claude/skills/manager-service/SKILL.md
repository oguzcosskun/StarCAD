---
name: manager-service
description: Business katmanında Manager (iş servisi) yazarken/değiştirirken oku. StarCADContext'i doğrudan enjekte etme, LINQ + .Select() projeksiyon, Result/DataResult döndürme, elle mapping, IXxxService test seam'i işlerinde gerekli. Generic repository/DAL yazmayı önler.
---

# Business Manager Deseni (DbContext-direct)

Generic repository YOK. Manager `StarCADContext`'i **doğrudan** enjekte eder. Bkz.
knowledge/generic-repo-removed.md.

## Kalıp

- Her özellik için `IXxxService` (arayüz, StarCAD.Business/Abstract) + `XxxManager : IXxxService`
  (StarCAD.Business/Concrete). DI'da `AddScoped<IXxxService, XxxManager>()`.
- Manager ctor'da `StarCADContext` (+ gerekiyorsa `IValidator<T>`, `IFileStorageService`).
  **AutoMapper kullanılmıyor — mapping elle** (bkz. knowledge/db-conventions.md).
- **Okuma:** LINQ + `.Select()` ile **ViewModel projeksiyonu** — entity'yi controller'a sızdırma.
  Sadece okuma sorgusunda `AsNoTracking()`.
- **Yazma:** context üzerinde ekle/güncelle → `await _db.SaveChangesAsync(ct)`. DTO→entity elle
  (`new Xxx { ... }`). Soft-delete + audit (`CreatedAt`/`UpdatedAt`, `IsActive=false`)
  `StarCADContext.SaveChanges` override'ında merkezî — **Manager'da tekrar etme.**
- **Doğrulama:** `IValidator<TDto>` enjekte et, `ValidateAsync` çağır, geçersizse
  `ErrorResult/ErrorDataResult(mesaj, StatusCodes.Status400BadRequest)` dön (validator'lar
  `AddValidatorsFromAssembly` ile kayıtlı).
- **Dönüş:** her public metot `Result` / `DataResult<T>` döner (StarCAD.Core/Utilities/Results).
  Hata kolu `StatusCode` taşıyabilir — **çıplak sayı değil**, `StatusCodes.Status404NotFound`/
  `Status409Conflict` sabitleri (Business'ta `GlobalUsings.cs` alias'ıyla gelir); başarı tarafı
  HTTP'den habersiz.
- **CancellationToken** her async metotta taşınır.
- Global query filter `IsActive == true` zaten uygulanıyor — pasifleri görmek istemiyorsan ek
  filtre yazma; istisnai olarak görmek istersen `IgnoreQueryFilters()`.

## Örnek

```csharp
public class ProjectFirmManager : IProjectFirmService
{
    private readonly StarCADContext _db;
    private readonly IValidator<ProjectFirmCreateDto> _createValidator;

    public ProjectFirmManager(StarCADContext db, IValidator<ProjectFirmCreateDto> createValidator)
    {
        _db = db;
        _createValidator = createValidator;
    }

    public async Task<IDataResult<ProjectFirmDetailDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _db.ProjectFirms
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new ProjectFirmDetailDto { Id = f.Id, Title = f.Title, TaxNumber = f.TaxNumber })
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? new ErrorDataResult<ProjectFirmDetailDto>("Proje firması bulunamadı.", StatusCodes.Status404NotFound)
            : new SuccessDataResult<ProjectFirmDetailDto>(dto);
    }

    public async Task<IDataResult<ProjectFirmDetailDto>> CreateAsync(ProjectFirmCreateDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return new ErrorDataResult<ProjectFirmDetailDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = new ProjectFirm { CompanyType = dto.CompanyType, Title = dto.Title.Trim(), TaxNumber = dto.TaxNumber };
        _db.ProjectFirms.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new SuccessDataResult<ProjectFirmDetailDto>(
            new ProjectFirmDetailDto { Id = entity.Id, Title = entity.Title, TaxNumber = entity.TaxNumber },
            "Proje firması eklendi.");
    }
}
```

## Yapma

- Repository/`IXxxDal`/`IUnitOfWork` yazma.
- **AutoMapper kullanma** — mapping elle (`.Select()` + `new Dto{}`/`new Entity{}`).
- Entity'yi doğrudan controller'a döndürme (projeksiyon kullan).
- Soft-delete/audit'i Manager'da elle yazma (DbContext override yapıyor).
- Validasyonu inline `if`lerle Manager'a gömme — FluentValidation validator'ı enjekte et.
- Concurrency'de kendi try-catch'inle yut — `DbUpdateConcurrencyException` üst katmanda 409'a
  map'lenir (bkz. code-review skill'i + CLAUDE.md).
