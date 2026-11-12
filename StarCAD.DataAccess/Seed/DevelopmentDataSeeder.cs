using System.Text;
using Microsoft.EntityFrameworkCore;
using StarCAD.Common.Constants;
using StarCAD.Core.Entities.Concrete;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.Core.Storage;
namespace StarCAD.DataAccess.Seed;
/// <summary>
/// Sadece yerel geliştirme için: DB boşsa uçtan uca test edilebilir, gerçekçi hacimli bir fixture
/// kurar (40 kullanıcı, 20 proje firması, 12 dağıtım firması, onlarca proje). Üretim mantığı
/// DEĞİLDİR. Her blok kendi bağımsız guard'ına sahiptir — yarım kalmış bir DB durumunda eksik
/// parçayı tamamlar, erken return ile atlamaz.
///
/// Şehir/ilçe master verisine bağımlı (bkz. <see cref="MasterDataSeeder"/>) — Program.cs'te bu
/// seeder'dan ÖNCE çalıştırılmalı, aksi halde Building.City/District boş kalır.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(StarCADContext db,IFileStorageService storage)
    {
        await SeedRolesAsync(db);
        await SeedGasDistributionGroupsAsync(db);
        await SeedGasDistributionFirmsAsync(db);
        await SeedProjectFirmsAsync(db);
        await SeedProjectFirmAuthorizationsAsync(db);
        await SeedUsersAsync(db);
        await SeedProjectsAsync(db, storage);
    }

    /// <summary>
    /// Az önce toplu eklenen kayıtlar için outbox mesajı kuyruklar — zaten çalışan
    /// OutboxProcessor worker'ı bunları normal bir Create ile aynı yoldan Elasticsearch'e işler.
    /// Ana SaveChangesAsync çoktan çalıştığı için Id'ler burada zaten atanmış durumda; tek-kayıtlık
    /// SaveChangesWithOutboxAsync'in çözdüğü "identity kolonu" sorunu burada yok.
    /// </summary>
    private static async Task EnqueueSearchBackfillAsync<TEntity>(StarCADContext db, IQueryable<TEntity> entities)
        where TEntity : BaseEntity
    {
        var utcNow = DateTime.UtcNow;
        var ids = await entities.Select(e => e.Id).ToListAsync();

        db.OutboxMessages.AddRange(ids.Select(id => new OutboxMessage
        {
            EntityType = typeof(TEntity).Name,
            EntityId = id,
            Operation = "Index",
            CreatedAt = utcNow
        }));

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(StarCADContext db)
    {
        if (await db.Roles.IgnoreQueryFilters().AnyAsync())
            return;

        db.Roles.AddRange(
            new Role { Name = "Yönetici", Code = RoleCodes.Admin },
            new Role { Name = "Proje Firması Kullanıcısı", Code = RoleCodes.ProjectFirmUser },
            new Role { Name = "Dağıtım Firması Kullanıcısı", Code = RoleCodes.GasDistributionUser });

        await db.SaveChangesAsync();
    }

    private static async Task SeedGasDistributionGroupsAsync(StarCADContext db)
    {
        if (await db.GasDistributionGroups.IgnoreQueryFilters().AnyAsync())
            return;

        db.GasDistributionGroups.AddRange(
            new GasDistributionGroup { Name = "AKSA" },
            new GasDistributionGroup { Name = "AKMERCAN" },
            new GasDistributionGroup { Name = "ENERYA" },
            new GasDistributionGroup { Name = "GAZDAŞ" },
            new GasDistributionGroup { Name = "TOROSGAZ" },
            new GasDistributionGroup { Name = "ÇEDAŞ" },
            new GasDistributionGroup { Name = "DOĞUGAZ" });

        await db.SaveChangesAsync();
    }

    // (Ünvan, Grup, Bölge ili) — CompanyNumber sırayla 1'den atanır.
    private static readonly (string Title, string GroupName, string City)[] GasFirmSeeds =
    [
        ("Aksa Çorum Doğalgaz Dağıtım A.Ş.", "AKSA", "Çorum"),
        ("Aksa Kırşehir Doğalgaz Dağıtım A.Ş.", "AKSA", "Kırşehir"),
        ("Akmercan Erzincan Doğalgaz A.Ş.", "AKMERCAN", "Erzincan"),
        ("Akmercan Bayburt Doğalgaz A.Ş.", "AKMERCAN", "Bayburt"),
        ("Enerya Kayseri Doğalgaz A.Ş.", "ENERYA", "Kayseri"),
        ("Enerya Sivas Doğalgaz A.Ş.", "ENERYA", "Sivas"),
        ("Gazdaş Gaziantep Doğalgaz A.Ş.", "GAZDAŞ", "Gaziantep"),
        ("Gazdaş Adıyaman Doğalgaz A.Ş.", "GAZDAŞ", "Adıyaman"),
        ("Torosgaz Mersin Doğalgaz A.Ş.", "TOROSGAZ", "Mersin"),
        ("Torosgaz Hatay Doğalgaz A.Ş.", "TOROSGAZ", "Hatay"),
        ("Çedaş Tekirdağ Doğalgaz A.Ş.", "ÇEDAŞ", "Tekirdağ"),
        ("Doğugaz Van Doğalgaz A.Ş.", "DOĞUGAZ", "Van")
    ];

    private static async Task SeedGasDistributionFirmsAsync(StarCADContext db)
    {
        if (await db.GasDistributionFirms.IgnoreQueryFilters().AnyAsync())
            return;

        var groups = await db.GasDistributionGroups.IgnoreQueryFilters()
            .ToDictionaryAsync(g => g.Name, g => g.Id);

        for (var i = 0; i < GasFirmSeeds.Length; i++)
        {
            var seed = GasFirmSeeds[i];
            db.GasDistributionFirms.Add(new GasDistributionFirm
            {
                Title = seed.Title,
                CompanyNumber = i + 1,
                GroupId = groups.GetValueOrDefault(seed.GroupName),
                Description = $"{seed.City} bölgesi doğalgaz dağıtım faaliyeti.",
                ContactPerson = FullName(i + 40),
                Phone = $"0312{i:D7}",
                Address = $"{seed.City} merkez, Sanayi Cad. No:{10 + i}"
            });
        }

        await db.SaveChangesAsync();
        await EnqueueSearchBackfillAsync(db, db.GasDistributionFirms);
    }

    private static readonly string[] ProjectFirmTitles =
    [
        "Anka Mühendislik Ltd. Şti.",
        "Boğaziçi Doğalgaz Tesisat A.Ş.",
        "Marmara Proje Mühendislik Ltd. Şti.",
        "Ege Isı Sistemleri A.Ş.",
        "Akdeniz Tesisat Mühendislik Ltd. Şti.",
        "Orta Anadolu Doğalgaz Proje A.Ş.",
        "Karadeniz Mühendislik Ltd. Şti.",
        "Toros Isıtma ve Tesisat A.Ş.",
        "Payas Mühendislik Ltd. Şti.",
        "Nova Doğalgaz Proje Ltd. Şti.",
        "Vizyon Tesisat Mühendislik A.Ş.",
        "Meridyen Proje ve Danışmanlık Ltd. Şti.",
        "Zirve Isı Mühendislik A.Ş.",
        "Pusula Doğalgaz Tesisat Ltd. Şti.",
        "Yıldız Mühendislik ve Müşavirlik A.Ş.",
        "Referans Tesisat Proje Ltd. Şti.",
        "Ufuk Doğalgaz Mühendislik A.Ş.",
        "Kalkan Isı Sistemleri Ltd. Şti.",
        "Doruk Proje Mühendislik A.Ş.",
        "Sentez Tesisat ve Danışmanlık Ltd. Şti."
    ];

    // Firmaların "merkez ili" — adres/telefon çeşitliliği için, ayrı bir master veri kaydı değil.
    private static readonly string[] ProjectFirmCities =
    [
        "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Konya", "Kayseri", "Gaziantep",
        "Mersin", "Samsun", "Denizli", "Eskişehir", "Kocaeli", "Manisa", "Aydın", "Balıkesir",
        "Sakarya", "Trabzon", "Elazığ", "Malatya"
    ];

    private static async Task SeedProjectFirmsAsync(StarCADContext db)
    {
        if (await db.ProjectFirms.IgnoreQueryFilters().AnyAsync())
            return;

        for (var i = 0; i < ProjectFirmTitles.Length; i++)
        {
            var city = ProjectFirmCities[i];

            // CompanyType 2 = Tüzel → TaxNumber zorunlu (CK_ProjectFirm_TypeIdentity). Şahıs (1)
            // firma NationalIdNumber şifrelemesi Business katmanında (INationalIdProtector) yapılır;
            // DataAccess seed'i bu servise erişemez, o yüzden seed verisi tüzel firmayla sınırlı tutuldu.
            db.ProjectFirms.Add(new ProjectFirm
            {
                CompanyType = 2,
                Title = ProjectFirmTitles[i],
                TaxNumber = $"10{i + 1:D8}",
                AccountingCode = $"MUH-{i + 1:D3}",
                ContactPerson = FullName(i),
                Email = $"info@{Slug(ProjectFirmTitles[i])}.com.tr",
                Phone = $"0212{i:D7}",
                Phone2 = $"0500{i:D7}",
                Address = $"{city} merkez, Mühendislik Cad. No:{5 + i}"
            });
        }

        await db.SaveChangesAsync();
        await EnqueueSearchBackfillAsync(db, db.ProjectFirms);
    }

    private static async Task SeedProjectFirmAuthorizationsAsync(StarCADContext db)
    {
        if (await db.ProjectFirmAuthorizations.IgnoreQueryFilters().AnyAsync())
            return;

        var projectFirms = await db.ProjectFirms.IgnoreQueryFilters().OrderBy(f => f.Id).ToListAsync();
        var gasFirms = await db.GasDistributionFirms.IgnoreQueryFilters().OrderBy(f => f.Id).ToListAsync();

        var anchor = DateTime.UtcNow;
        var authGlobalIndex = 0;

        // Her firma 1-3 dağıtım firmasına yetkili; +4/+8 kaydırma 12 GD firmasında hep farklı
        // indekse düşer (unique (ProjectFirmId, GasDistributionFirmId) filtered index'i korur).
        int[] offsets = [0, 4, 8];

        for (var i = 0; i < projectFirms.Count; i++)
        {
            var projectFirm = projectFirms[i];
            var authCount = (i % 3) + 1;

            for (var k = 0; k < authCount; k++)
            {
                var gasFirm = gasFirms[(i + offsets[k]) % gasFirms.Count];

                var validFrom = anchor.AddYears(-(1 + (i % 4)));
                // Her 5. yetki geçmişte sona ermiş — "aktif/süresi dolmuş" filtrelerini test edebilmek için.
                var validTo = authGlobalIndex % 5 == 4
                    ? anchor.AddMonths(-(3 + (authGlobalIndex % 6)))
                    : (DateTime?)null;

                db.ProjectFirmAuthorizations.Add(new ProjectFirmAuthorization
                {
                    ProjectFirm = projectFirm,
                    GasDistributionFirm = gasFirm,
                    CertificateNumber = $"SRT-{projectFirm.Id:D3}-{gasFirm.Id:D2}",
                    ValidFrom = validFrom,
                    ValidTo = validTo
                });

                authGlobalIndex++;
            }
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 3 yönetici + 20 proje firmasına dağılan ~24 proje firması kullanıcısı + 12 dağıtım
    /// firmasına dağılan ~13 dağıtım kullanıcısı = 40. Şifreler RegisterRequestValidator
    /// kurallarına uygun (8+ karakter, büyük/küçük harf, rakam); rol içinde ortak şifre —
    /// manuel test/demo girişini kolaylaştırmak için.
    /// </summary>
    private static async Task SeedUsersAsync(StarCADContext db)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync())
            return;

        var roles = await db.Roles.IgnoreQueryFilters().ToListAsync();
        var adminRole = roles.First(r => r.Code == RoleCodes.Admin);
        var projectFirmRole = roles.First(r => r.Code == RoleCodes.ProjectFirmUser);
        var gasFirmRole = roles.First(r => r.Code == RoleCodes.GasDistributionUser);

        var projectFirms = await db.ProjectFirms.IgnoreQueryFilters().OrderBy(f => f.Id).ToListAsync();
        var gasFirms = await db.GasDistributionFirms.IgnoreQueryFilters().OrderBy(f => f.Id).ToListAsync();

        var now = DateTime.UtcNow;
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin1234");
        var projeHash = BCrypt.Net.BCrypt.HashPassword("Proje1234");
        var dagitimHash = BCrypt.Net.BCrypt.HashPassword("Dagitim1234");

        // "admin"/"Admin1234" — Program.cs'teki Scalar OAuth2 password flow'unun ön dolu değeri,
        // kullanıcı adı/şifre burada değişirse Scalar login formu da güncellenmeli.
        db.Users.Add(new User
        {
            FullName = "Sistem Yöneticisi",
            Email = "admin@starcad.local",
            Username = "admin",
            Phone = "05000000000",
            PasswordHash = adminHash,
            RoleId = adminRole.Id,
            CreatedAt = now
        });

        for (var i = 1; i < 3; i++)
        {
            db.Users.Add(new User
            {
                FullName = FullName(200 + i),
                Email = $"admin{i + 1}@starcad.local",
                Username = $"admin{i + 1}",
                Phone = $"050000{i:D5}",
                PasswordHash = adminHash,
                RoleId = adminRole.Id,
                CreatedAt = now
            });
        }

        // Her firmaya en az 1 kullanıcı; bazı firmalara ikinci kullanıcı — toplam 24.
        var extraUserFirmIndexes = new HashSet<int> { 0, 4, 8, 12 };
        var seq = 0;
        for (var i = 0; i < projectFirms.Count; i++)
        {
            var usersForFirm = extraUserFirmIndexes.Contains(i) ? 2 : 1;
            for (var u = 0; u < usersForFirm; u++)
            {
                seq++;
                db.Users.Add(new User
                {
                    FullName = FullName(i * 3 + u),
                    Email = $"pf{seq:D2}@starcad.local",
                    Username = $"pf{seq:D2}",
                    Phone = $"05001{seq:D6}",
                    PasswordHash = projeHash,
                    RoleId = projectFirmRole.Id,
                    ProjectFirmId = projectFirms[i].Id,
                    CreatedAt = now
                });
            }
        }

        // Her dağıtım firmasına en az 1 kullanıcı; ilk firmaya ikinci kullanıcı — toplam 13.
        seq = 0;
        for (var i = 0; i < gasFirms.Count; i++)
        {
            var usersForFirm = i == 0 ? 2 : 1;
            for (var u = 0; u < usersForFirm; u++)
            {
                seq++;
                db.Users.Add(new User
                {
                    FullName = FullName(i * 5 + u + 100),
                    Email = $"gd{seq:D2}@starcad.local",
                    Username = $"gd{seq:D2}",
                    Phone = $"05002{seq:D6}",
                    PasswordHash = dagitimHash,
                    RoleId = gasFirmRole.Id,
                    GasDistributionFirmId = gasFirms[i].Id,
                    CreatedAt = now
                });
            }
        }

        await db.SaveChangesAsync();
        await EnqueueSearchBackfillAsync(db, db.Users);
    }

    private static readonly string[] ProjectTypeValues = ["ILAVE", "ILAVE_TADILAT", "KOLON", "KOLON_TADILAT"];
    private static readonly string[] HeatingTypeValues = ["bireysel", "merkezi"];
    private static readonly string[] BuildingUsageTypeValues = ["coklu", "mustakil"];
    private static readonly string[] StatusCycle =
        [CodeValues.Draft, CodeValues.Draft, CodeValues.PendingApproval, CodeValues.Approved, CodeValues.Approved, CodeValues.Rejected];

    private static readonly string[] ProjectCities =
    [
        "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Konya", "Kayseri", "Gaziantep",
        "Mersin", "Samsun", "Denizli", "Eskişehir", "Kocaeli", "Manisa", "Aydın", "Balıkesir",
        "Sakarya", "Trabzon", "Elazığ", "Malatya"
    ];

    private static readonly string[] StreetNames =
    [
        "Atatürk", "Cumhuriyet", "İstiklal", "Gazi", "Barış", "Yeni", "Fatih", "Çınar",
        "Menekşe", "Lale", "Vatan", "Millet", "Anadolu", "Sanayi", "Zübeyde Hanım"
    ];

    private static readonly string[] RejectionReasons =
    [
        "Statik hesap raporu eksik, tekrar gönderiniz.",
        "Kolon şeması ile saha ölçümleri uyuşmuyor.",
        "Bina kullanım tipi ruhsat ile örtüşmüyor, düzeltilmesi gerekiyor.",
        "Isı yalıtım raporu talep edilmektedir."
    ];

    /// <summary>
    /// Her yetki kaydı için 1-3 proje: Building (il/ilçe master veriden), tüm proje alanları,
    /// durum akışına uygun OperationHistory ve ProjectVersion kayıtları ile. Toplamda ~80 proje —
    /// list-filters/paging'i (varsayılan sayfa boyutu 30) birden çok sayfayla test edebilmek için.
    /// </summary>
    private static async Task SeedProjectsAsync(StarCADContext db, IFileStorageService storage)
    {
        if (await db.Projects.IgnoreQueryFilters().AnyAsync())
            return;

        var projectTypeCodes = await LoadCodesAsync(db, CodeGroupNames.ProjectType);
        var heatingTypeCodes = await LoadCodesAsync(db, CodeGroupNames.HeatingType);
        var buildingUsageCodes = await LoadCodesAsync(db, CodeGroupNames.BuildingUsageType);
        var statusCodes = await LoadCodesAsync(db, CodeGroupNames.ProjectStatus);

        var cities = await db.Cities.AsNoTracking()
            .Include(c => c.Districts)
            .Where(c => ProjectCities.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name);

        if (cities.Count != ProjectCities.Length)
            throw new InvalidOperationException(
                "Proje seed'i için beklenen illerden bazıları bulunamadı — MasterDataSeeder bu seeder'dan önce çalışmalı.");

        var authorizations = await db.ProjectFirmAuthorizations
            .Include(a => a.ProjectFirm)
            .Include(a => a.GasDistributionFirm)
            .OrderBy(a => a.Id)
            .ToListAsync();

        var projectFirmUsers = (await db.Users.Where(u => u.ProjectFirmId != null).ToListAsync())
            .GroupBy(u => u.ProjectFirmId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(u => u.Id).ToList());

        var gasFirmUsers = (await db.Users.Where(u => u.GasDistributionFirmId != null).ToListAsync())
            .GroupBy(u => u.GasDistributionFirmId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(u => u.Id).ToList());

        var adminId = (await db.Users.FirstAsync(u => u.Username == "admin")).Id;

        var usedBuildingCodes = new HashSet<string>();
        var globalIndex = 0;

        for (var authIndex = 0; authIndex < authorizations.Count; authIndex++)
        {
            var authorization = authorizations[authIndex];
            var projectCount = (authIndex % 3) + 1;

            var ownerCandidates = projectFirmUsers.GetValueOrDefault(authorization.ProjectFirmId) ?? [];
            var approverCandidates = gasFirmUsers.GetValueOrDefault(authorization.GasDistributionFirmId) ?? [];

            for (var p = 0; p < projectCount; p++, globalIndex++)
            {
                var owner = ownerCandidates.Count > 0 ? ownerCandidates[globalIndex % ownerCandidates.Count] : null;
                var approver = approverCandidates.Count > 0 ? approverCandidates[globalIndex % approverCandidates.Count] : null;

                var city = cities[ProjectCities[globalIndex % ProjectCities.Length]];
                var district = city.Districts.ElementAt(globalIndex % city.Districts.Count);

                var projectTypeValue = ProjectTypeValues[globalIndex % ProjectTypeValues.Length];
                var heatingTypeValue = HeatingTypeValues[globalIndex % HeatingTypeValues.Length];
                var usageTypeValue = BuildingUsageTypeValues[globalIndex % BuildingUsageTypeValues.Length];
                var statusValue = StatusCycle[globalIndex % StatusCycle.Length];

                var isCoklu = usageTypeValue == "coklu";
                var apartmentCount = isCoklu ? 6 + (globalIndex % 50) : (int?)null;
                var workplaceCount = isCoklu ? globalIndex % 5 : (globalIndex % 3 == 0 ? 1 : (int?)null);
                var totalArea = isCoklu ? (apartmentCount!.Value * 85) + 200 : 120 + (globalIndex % 300);

                var projectTypeName = projectTypeCodes[projectTypeValue].Name;

                var project = new Project
                {
                    Name = $"{district.Name} {projectTypeName} Projesi",
                    Description = $"{projectTypeName} kapsamında, {district.Name}/{city.Name} adresinde doğalgaz iç tesisat projesi.",
                    ProjectFirmAuthorization = authorization,
                    OwnerUserId = owner?.Id,
                    CreatedByUserId = globalIndex % 10 == 0 ? adminId : owner?.Id,
                    Building = new Building
                    {
                        CityId = city.Id,
                        DistrictId = district.Id,
                        AddressLine = $"{district.Name} Mah. {StreetNames[globalIndex % StreetNames.Length]} Sok. No:{(globalIndex % 40) + 1}",
                        BlockLotParcel = $"{100 + globalIndex}/{(globalIndex % 9) + 1}",
                        Code = GenerateBuildingCode(usedBuildingCodes),
                        FloorCount = (byte)(2 + (globalIndex % 8)),
                        BasementCount = (byte)(globalIndex % 2)
                    },
                    StatusCodeId = statusCodes[statusValue].Id,
                    ProjectTypeCodeId = projectTypeCodes[projectTypeValue].Id,
                    HeatingTypeCodeId = heatingTypeCodes[heatingTypeValue].Id,
                    BuildingUsageTypeCodeId = buildingUsageCodes[usageTypeValue].Id,
                    IsPermitProject = false,
                    ApartmentCount = apartmentCount,
                    WorkplaceCount = workplaceCount,
                    TotalArea = totalArea,
                    Capacity = 10 + (globalIndex % 190),
                    ServiceBoxPressureMbar = globalIndex % 2 == 0 ? 21 : 300,
                    IsOfficial = statusValue == CodeValues.Approved,
                    IsLocked = statusValue == CodeValues.Approved
                };

                AddApprovalTrail(db, project, statusValue, statusCodes, owner?.Id, approver?.Id, globalIndex);
                await AddVersions(storage, project, statusValue, owner?.Id, globalIndex);

                project.UpdatedByUserId = statusValue switch
                {
                    CodeValues.Approved or CodeValues.Rejected => approver?.Id ?? owner?.Id,
                    _ => owner?.Id
                };

                db.Projects.Add(project);
            }
        }

        await db.SaveChangesAsync();
        await EnqueueSearchBackfillAsync(db, db.Projects);
    }

    /// <summary>
    /// Draft dışı durumlar için gerçek onay akışını izleyen OperationHistory kayıtları
    /// (Submit → ProjectFirmUser, Approve/Reject → GasDistributionUser — bkz. ProjectApprovalController).
    /// </summary>
    private static void AddApprovalTrail(
        StarCADContext db,
        Project project,
        string statusValue,
        IReadOnlyDictionary<string, Code> statusCodes,
        int? ownerUserId,
        int? approverUserId,
        int index)
    {
        if (statusValue == CodeValues.Draft || ownerUserId is null)
            return;

        db.OperationHistories.Add(new OperationHistory
        {
            Project = project,
            UserId = ownerUserId.Value,
            OperationCodeId = statusCodes[CodeValues.PendingApproval].Id,
            RoleSnapshot = RoleCodes.ProjectFirmUser
        });

        if (statusValue == CodeValues.Draft || statusValue == CodeValues.PendingApproval || approverUserId is null)
            return;

        db.OperationHistories.Add(new OperationHistory
        {
            Project = project,
            UserId = approverUserId.Value,
            OperationCodeId = statusCodes[statusValue].Id,
            Description = statusValue == CodeValues.Rejected ? RejectionReasons[index % RejectionReasons.Length] : null,
            RoleSnapshot = RoleCodes.GasDistributionUser
        });
    }

    /// <summary>Draft=1, PendingApproval/Rejected=2, Approved=3 sürüm — hepsi proje mühendisi (owner) tarafından.</summary>
    private static async Task AddVersions(
        IFileStorageService storage, Project project, string statusValue, int? ownerUserId, int index)
    {
        project.ProjectVersions.Add(await NewVersionAsync(storage, "İlk Taslak", ownerUserId, index, 1));

        if (statusValue == CodeValues.Draft)
            return;

        project.ProjectVersions.Add(await NewVersionAsync(storage, "Onaya Gönderilen Sürüm", ownerUserId, index, 2));

        if (statusValue == CodeValues.Approved)
            project.ProjectVersions.Add(await NewVersionAsync(storage, "Onaylanan Sürüm", ownerUserId, index, 3));
    }

    private static async Task<ProjectVersion> NewVersionAsync(IFileStorageService storage, string label, int? createdByUserId, int index, int versionNo)
    {
        var objectKey = $"projects/seed-{index}/versions/v{versionNo}-{Guid.NewGuid():N}.json";
        const string emptyCanvas =
            """{"nextUniqueId":1,"activeFloorId":1,"floors":[{"id":1,"name":"Zemin Kat","heightCm":300,"isBasement":false}],"points":[],"walls":[],"openings":[]}""";

        await storage.PutObjectAsync(
            objectKey, new MemoryStream(Encoding.UTF8.GetBytes(emptyCanvas)), "application/json");

        return new ProjectVersion { JsonToken = objectKey, Label = label, CreatedByUserId = createdByUserId };
    }

    /// <summary>6 haneli rastgele sayısal bina kodu; benzersizlik bu seed turu içinde garanti edilir
    /// (DB'deki UNIQUE kısıt zaten var, ama çakışmayı burada önlemek gereksiz bir migration hatasını engeller).</summary>
    private static string GenerateBuildingCode(HashSet<string> usedCodes)
    {
        string code;
        do
        {
            code = string.Create(6, Random.Shared, static (span, random) =>
            {
                for (var i = 0; i < span.Length; i++)
                    span[i] = (char)('0' + random.Next(10));
            });
        } while (!usedCodes.Add(code));

        return code;
    }

    private static async Task<Dictionary<string, Code>> LoadCodesAsync(StarCADContext db, string groupName)
    {
        return await db.Codes.AsNoTracking()
            .Where(c => c.Group.GroupName == groupName)
            .ToDictionaryAsync(c => c.CodeValue);
    }

    private static readonly string[] FirstNames =
    [
        "Ahmet", "Ayşe", "Mehmet", "Fatma", "Mustafa", "Zeynep", "Ali", "Elif",
        "Hüseyin", "Merve", "Hasan", "Esra", "İbrahim", "Büşra", "Osman", "Sena",
        "Yusuf", "Derya", "Murat", "Gamze", "Emre", "Aylin", "Burak", "Kübra"
    ];

    private static readonly string[] LastNames =
    [
        "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Yıldırım", "Öztürk",
        "Aydın", "Özdemir", "Arslan", "Doğan", "Kılıç", "Aslan", "Çetin", "Kurt",
        "Özkan", "Şimşek", "Polat", "Erdoğan", "Güneş", "Aksoy", "Türk", "Bulut"
    ];

    private static string FullName(int index) =>
        $"{FirstNames[index % FirstNames.Length]} {LastNames[(index * 5 + 3) % LastNames.Length]}";

    private static string Slug(string title)
    {
        var firstWord = title.Split(' ')[0];
        return firstWord
            .Replace("ı", "i").Replace("İ", "I")
            .Replace("ş", "s").Replace("Ş", "S")
            .Replace("ğ", "g").Replace("Ğ", "G")
            .Replace("ü", "u").Replace("Ü", "U")
            .Replace("ö", "o").Replace("Ö", "O")
            .Replace("ç", "c").Replace("Ç", "C")
            .ToLowerInvariant();
    }
}
