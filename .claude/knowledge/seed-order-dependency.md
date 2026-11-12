# gotcha: Seed sırası — Migrate → MasterDataSeeder → DevelopmentDataSeeder

`DevelopmentDataSeeder` artık Building/Project seed'i için il/ilçe (`City`/`District`) satırlarına
bağımlı — `MasterDataSeeder`'dan ÖNCE çalışırsa `Cities` tablosu boş olur ve proje/bina seed'i
sessizce eksik kalır (`Building.CityId` null, ya da SeedProjectsAsync fail-fast fırlatır).

`Program.cs`'te doğru sıra, tek `if (IsDevelopment()) { ... } else { ... }` içinde:

- **Development**: tek scope/context üzerinden sırayla `MigrateAsync()` → `MasterDataSeeder` →
  `DevelopmentDataSeeder`. Migration burada otomatik çünkü yerel DB'nin elle migrate edilmesini
  beklemek gereksiz sürtünme.
- **prod (else)**: yalnızca `MasterDataSeeder` — migration YOK, şema `dotnet ef database update`
  ile elle uygulanır (bkz. CLAUDE.md Komutlar), `MasterDataSeeder` bunun zaten yapıldığını varsayar.

Yeni bir seeder eklerken bağımlılık zincirini bu sıraya göre yerleştir; Development bloğu içindeki
`await` sırasını değiştirme.
