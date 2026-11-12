# decision: Generic repository kaldırıldı — DbContext-direct

Başlangıçta `IEntityRepository<T>` + `EfEntityRepositoryBase` + `IUnitOfWork` diye bir generic
repo katmanı eklenmişti. Mentörün gerçek StarCAD örneği görülünce **kaldırıldı**
(`refactor/remove-generic-repository`, #2). Core tekrar tertemiz: **EF Core paketi yok.**

## Desen (bunu uygula)

- Manager'lar `StarCADContext`'i **doğrudan** enjekte eder — arada DAL/repository yok.
- Sorgular: LINQ + `.Select()` ile **ViewModel/DTO projeksiyonu** (entity'yi olduğu gibi dışarı
  sızdırma).
- Dönüş tipi: `Result` / `DataResult<T>` (StarCAD.Core/Utilities/Results).
- Test seam'i: `IXxxService` arayüzü (Moq'la mock'lanır) — repository değil.
- Çok-tablo yazımı zaten tek `DbContext` + tek `SaveChanges` ile atomik → ayrı `IUnitOfWork`
  gerekmez.

## Neden

`DbContext` zaten bir Unit of Work + repository'dir; üstüne generic repo koymak EF üstünde
gereksiz bir soyutlama katmanıydı. Bkz. [[manager-service]] skill'i.

## Geri getirme

`IEntityRepository`/`IXxxDal`/`IUnitOfWork`/generic DAL — yeni açık karar olmadan **ekleme**.
