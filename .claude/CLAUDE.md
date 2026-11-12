# StarCAD — Backend (cadapi)

Doğalgaz iç tesisat projelendirme sisteminin sunucu tarafı. Kullanıcı web'de bina/kat/oda
çizer, üstüne gaz hattı + cihaz ekler; bu repo kimlik/yetki/proje yaşam döngüsü/evrak/poliçe
verisini yönetir ve çizim JSON'unu MinIO'ya köprüler. Ekip: 4 stajyer.

## Teknoloji

- C# / .NET 10, geleneksel **N-Layer** mimari (8 proje).
- **EF Core** (Code-First, migrations) — SQL Server (MSSQL).
- **MinIO** (S3 uyumlu nesne deposu) — çizim canvas JSON'u ve yüklenen evrak dosyaları.
- **Scalar** (OpenAPI UI, Swagger yerine).
- FluentValidation + JWT auth (Business katmanında); mapping **elle** yapılır (AutoMapper kullanılmıyor — bkz. knowledge/db-conventions.md).

## Katman haritası ve bağımlılık yönü

| Proje | Rol |
|---|---|
| `StarCAD.Api` | Controller'lar, `Program.cs`, Scalar/OpenAPI, middleware |
| `StarCAD.Business` | Manager (iş servisleri), FluentValidation, JWT, MinIO kullanımı; mapping elle |
| `StarCAD.Core` | Domain-agnostik altyapı: `BaseEntity`(Id) + `SoftDeleteEntity`(IsActive), `Utilities/Results`, `Storage/IFileStorageService`. **Saf POCO, EF paketi YOK.** |
| `StarCAD.DataAccess` | EF Core: `Concrete/EntityFramework/StarCADContext`, `Migrations/`; MinIO (`Concrete/MinioStorageManager`) |
| `StarCAD.Entity` | POCO entity'ler, `BaseEntity`/`SoftDeleteEntity`'den türer |
| `StarCAD.ViewModel` | DTO'lar (özellik başına Add/Update/Detail) |
| `StarCAD.Common` | Paylaşılan sabit/enum/`Messages.cs` |
| `StarCAD.Tests` | xUnit + Moq (henüz aktif değil; entegrasyon testi ağırlıklı planlanıyor) |

Bağımlılık: `Api → Business → DataAccess → Entity → Core` (`Business` ayrıca → `ViewModel`,
`Common`). Bir katman yukarıya bağımlı olamaz. Bir dosyanın işini o dosyada/katmanda yap.

## Değişmez mimari kuralları (bunları ihlal eden kod üretme)

1. **Generic repository YOK.** `DbContext` zaten Unit of Work + repository'dir. Manager'lar
   `StarCADContext`'i **doğrudan** enjekte eder, LINQ + `.Select()` projeksiyon yazar, `Result`
   döner. `IEntityRepository`/`IXxxDal`/`IUnitOfWork` **ekleme** (bilinçli kaldırıldı — bkz.
   knowledge/generic-repo-removed.md). Test seam'i = `IXxxService` arayüzü, repository değil.
2. **Soft-delete `IsActive` ile.** Günlük "silme" = `IsActive = 0`, fiziksel `DELETE` değil
   (neredeyse her iş tablosu `SoftDeleteEntity`). Soft-delete + audit `StarCADContext.SaveChanges`
   override'ında merkezî; Manager'da tekrar etme. Global query filter `IsActive == true`.
3. **Cascade = üç kategori + EF'de her FK'ye AÇIK `Restrict`.** Sahiplik→CASCADE (şu an örnek
   yok), Referans→NO ACTION (neredeyse tümü), Atıf→SET NULL (tek örnek `Doc.UploadedByUserId`).
   EF zorunlu FK'yi varsayılan `Cascade` üretir → her ilişkide açıkça
   `OnDelete(DeleteBehavior.Restrict)` yaz, yoksa SQL Server "multiple cascade paths" ile migration
   reddeder (bkz. knowledge/cascade-restrict.md).
4. **Tek rol.** `User.RoleId → Role` doğrudan FK; bir kullanıcının **tam bir** rolü var. UserRole
   N:N junction kaldırıldı; N:N geri ekleme (bkz. knowledge/single-role.md).
5. **Tek tip `NVARCHAR` + her string'e uzunluk.** Her string alan `NVARCHAR(n)` (config'te
   `HasMaxLength(n)`) — **asla `NVARCHAR(MAX)`** (index atılamaz, tasarım ihlali). Binary/şifreli
   veri (`ProjectFirm.NationalIdNumber`) `VARBINARY` (bkz. knowledge/nvarchar-length-caps.md).
6. **Çizim SQL'e girmez.** Projenin tüm çizimi (kat/duvar/oda/cihaz/boru/topoloji) **tek JSON
   nesnesi** olarak MinIO'da; SQL'den `ProjectVersion.JsonToken` ile referanslanır. SQL'e
   granüler çizim tablosu **ekleme** (bkz. knowledge/minio-canvas-json.md).
7. **Kaydet akışı — autosave YOK.** Yalnızca "Kaydet"te yazılır: istemci tam canvas JSON'u
   gönderir → API yeni değişmez MinIO nesnesi yazar → `ProjectVersion` satırı eklenir →
   `Project.UpdatedAt`/`RowVersion` güncellenir. Retention: son 20 sıradan checkpoint (FIFO) +
   kalıcı olarak Onaylandı ve en son Reddedildi checkpoint'i.
8. **Hesaplanan mühendislik çıktıları app katmanında.** Boru çapı/debi gibi değerler C#'ta
   hesaplanıp yazılır — şemada kullanıcıya açık giriş yolu verilmez.
9. **Yeni kolon/tablo/desen eklemeden önce** neyin bilinçle reddedildiğine bak: blanket audit
   kolonları, 4. rol, GUID PK, VARCHAR/NVARCHAR ayrımı, per-enum lookup tablosu, generic
   repository — yeni açık karar olmadan geri getirme.

## Yazılım ilkeleri (SOLID · DI · DRY)

Genel tanım değil, bizim mimarimize somut karşılıkları:

- **SRP:** her Manager/servis tek iş yapar; 200+ satıra şişen sınıf/metot bölünür.
- **OCP:** kural setleri (FluentValidation validator'ları, evrak tipi doğrulama) yeni tip
  eklenerek genişletilir — mevcut Manager'ı değiştirerek değil.
- **LSP:** `BaseEntity`/`SoftDeleteEntity` yerine geçebilirlik korunur; soft-delete + query filter
  her `SoftDeleteEntity` için aynı davranır, özel-case'lenmez.
- **ISP:** arayüzler ince tutulur (`IXxxService`, `IFileStorageService`) — "her şeyi yapan" şişkin
  arayüz yok.
- **DIP:** `Business` somut EF/MinIO SDK'sına değil kendi arayüzlerine (`IXxxService`,
  `IFileStorageService`) bağımlı; `Core` hiçbir dış pakete bağımlı değil (EF paketi yok). Bağımlılık
  yönü yukarıdaki katman tablosuyla aynı.
- **DI:** .NET yerleşik container; servisler arayüz üzerinden kaydedilir. **Yaşam döngüsü:**
  `StarCADContext` + Manager'lar = `Scoped`; MinIO client (durumsuz/thread-safe) = `Singleton`;
  mapper/validator framework kaydına göre.
- **DRY:** aynı mantık 3+ yerde → ortak metoda çıkar. Ama farklı bağlamdaki iki şeyi "benziyor"
  diye birleştirme — yanlış soyutlama, tekrarlayan koddan kötüdür.

## İsimlendirme & dil

- **Kod tümüyle İngilizce:** sınıf/metot/değişken/dosya/namespace.
- **Yorum:** gerekiyorsa Türkçe olabilir, ama sadece **"neden"** için (workaround, domain kısıtı,
  birim dönüşümü) — dosya adının/koddan zaten anlaşılan şeye yorum yazma. Sahipsiz TODO bırakma:
  `// TODO(isim): ...`.
- **Kullanıcıya görünen metin Türkçe** (mesaj/etiket) — `StarCAD.Common/Messages.cs` dahil sınıf
  adı İngilizce, içerik Türkçe.
- İsimlendirme: sınıf/tip `PascalCase`, değişken/parametre `camelCase`, sabit `PascalCase`
  (C# konvansiyonu), boolean `Is`/`Has`/`Should`. Kısaltma yok.

## Güvenlik & kod hijyeni

- **Yıkıcı komut öncesi ONAY al:** `DROP`/`DELETE`/`TRUNCATE` (SQL), `rm -rf`, `main`'e force
  push, `dotnet ef database drop`, `docker system prune`. "Ufak/geçici" istisnası yoktur.
- **Çalışan/testi geçen koda dokunma** — görev doğrudan onu değiştirmeyi gerektirmiyorsa.
  Değişiklik mevcut testi kırıyorsa: önce durup bildir, sessizce değiştirme.
- **Async disiplini:** `async` yukarıdan aşağı; `CancellationToken` taşınır; **asla** `.Result` /
  `.Wait()` / `.GetAwaiter().GetResult()` (deadlock/thread starvation).
- **EF sorgu disiplini:** lazy loading kapalı; N+1'e dikkat; okumada `.Select()` projeksiyon +
  `AsNoTracking()`. **Paylaşılmış/uygulanmış migration elle düzenlenmez** — yeni düzeltici migration
  eklenir.
- **Zaman = UTC:** her yerde `DateTime.UtcNow` (`StarCADContext` zaten böyle), **asla `DateTime.Now`**.
- **Girdi doğrulama:** dış girdi (request/form) API sınırında FluentValidation ile doğrulanır;
  client'tan gelene güvenilmez.
- **Migration geri-alınabilir olmalı:** her `migrations add` sonrası üretilen `Down()`'a bak;
  breaking değişiklikte (kolon drop, tip değişimi) özel dikkat, gerekirse rollback adımını yaz.
- **Sır koda yazılmaz:** connection string, MinIO access/secret key, JWT secret →
  `appsettings.*.json` (git-ignore'lu) / user-secrets / env. Yeni ayar eklenince
  `appsettings.Example`/README güncelle.
- `Console.WriteLine`/debug logu commit'ten önce kaldır; yapılandırılmış log kullan.
- **DRY ama aşırıya kaçmadan:** aynı kod 3+ yerde → extract. Ama "benziyor" diye farklı bağlamdaki
  iki şeyi birleştirme — yanlış soyutlama, tekrardan kötüdür. Refactor'ü feature commit'inden ayır.
- Concurrency: `DbUpdateConcurrencyException` tutarlı bir **409 Conflict**'e map'lenir (ortak
  exception middleware). Error `Result`'ları `StatusCode` taşır (varsayılan 400).

## Git akışı

- **Semantic / Conventional Commits:** `<type>(<scope>): <subject>`. `type` =
  feat/fix/refactor/docs/style/test/chore; `scope` opsiyonel, parantez içinde; **iki nokta sonrası
  boşluk şart**. `subject` kısa **İngilizce** imperative (add/remove/fix), ≤72 karakter. Issue
  **sonda** `(#12)` ya da gövdede `Closes #12`. `EA/#1` gibi yazar-inisyali prefix **yok** (git
  zaten yazarı kaydeder). Örn: `feat(entity): add 24 entity classes (#1)`.
- **Branch:** `<type>/<kebab-desc>` (ör. `feat/entity-layer`, `refactor/remove-generic-repository`,
  `fix/policy-date-check`). `fix/` yalnızca gerçek bug için. Branch adına issue no konmaz.
- **`main`'e doğrudan commit/push YOK.** Dal aç → push → **Pull Request**.
- Yeni işe başlamadan `main`'i güncelle: `main`'e geç → **Pull** → feature dalını üstünden aç.

## Komutlar

- `dotnet build` — çözümü derle (0 hata beklenir).
- `dotnet test` — testler.
- `dotnet ef migrations add <Ad> -p StarCAD.DataAccess -s StarCAD.Api` — yeni migration.
- `dotnet ef database update -p StarCAD.DataAccess -s StarCAD.Api` — DB'ye uygula.
- API çalışınca OpenAPI/Scalar arayüzü (`/scalar`) üzerinden endpoint'ler denenir.

## Bilgi tabanı & çalışma şekli

- **Yeni oturumda ve bir modüle başlarken önce `.claude/knowledge/INDEX.md`'yi tara**, bugünkü işe
  dair `decision`/`gotcha`/`open-question` satırlarını aç ve oku. En ucuz hata önlemidir.
  `open-question` işaretli konularda **varsayarak kod yazma** — önce netleştir.
- Yeni bir mimari karar veya tuzak çıkınca: `knowledge/` altına dosyasını yaz, `INDEX.md`'ye satır
  ekle, gerekiyorsa bu CLAUDE.md'yi + ilgili `skills/*/SKILL.md`'yi güncelle.
- İş tipine uygun skill varsa (`ef-core-config`, `manager-service`, `code-review`) ona uy.
- **Not:** Ayrıntılı şema tasarımı, tablo-tablo gerekçe ve entity karşılıkları bu repoda değil,
  ekibin tasarım workspace'indeki dokümanlarda (`veritabani-rehberi.md`, `StarCAD_Database.sql`,
  `sema.dbml`, `proje-sablonu.md`) yaşıyor — şema kararı gerektiğinde referans odur. **Ama dikkat:**
  tasarım docs'u modül şeması + tekil tablo adı gösterir; **kod `dbo` + çoğul benimsedi** (bkz.
  knowledge/db-conventions.md) — docs'tan tip/kısıt içeriğini al, ad/şema kalıbını değil.
