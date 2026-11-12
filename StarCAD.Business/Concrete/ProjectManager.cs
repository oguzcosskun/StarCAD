using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Extensions;
using StarCAD.Business.Security;
using StarCAD.Common.Constants;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Projects;
using StarCAD.ViewModel.Common;
using StarCAD.Core.Search;

namespace StarCAD.Business.Concrete;

public class ProjectManager : IProjectService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ProjectCreateDto> _createValidator;
    private readonly IValidator<ProjectUpdateDto> _updateValidator;
    private readonly ISearchService _searchService;
    
    public ProjectManager(
        StarCADContext db,
        ICurrentUserService currentUser,
        IValidator<ProjectCreateDto> createValidator,
        IValidator<ProjectUpdateDto> updateValidator,
        ISearchService searchService)
    {
        _db = db;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchService = searchService;
    }

    public async Task<IDataResult<PagedResultDto<ProjectListItemDto>>> GetListAsync(
    ProjectListQueryDto query, CancellationToken cancellationToken = default)
    {

        var projects = ApplyCommonFilters(
            _db.Projects.AsNoTracking().WhereVisibleTo(_currentUser),
            query.DateFrom, query.DateTo, query.CityId, query.DistrictId, query.ProjectFirmId, query.GdFirmId, query.GdGroupId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim();
            projects = projects.Where(p =>
                (p.StatusCode != null ? p.StatusCode.CodeValue : CodeValues.Draft) == status);
        }
        
        var (scopeField, scopeValue) = _currentUser.GetSearchScope();
        projects = await projects.ApplyTextSearchAsync(_searchService, "projects", query.Q, scopeField, scopeValue, cancellationToken);
        
        var totalCount = await projects.CountAsync(cancellationToken);

        projects = ApplySort(projects, query.SortBy, query.SortDir);

        var page = query.Page < 1 ? 1 : query.Page;
        const int maxPageSize = 100;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await projects
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.StatusCode != null ? p.StatusCode.CodeValue : CodeValues.Draft,
                StatusName = p.StatusCode != null ? p.StatusCode.Name : "Taslak",
                ProjectTypeName = p.ProjectTypeCode != null ? p.ProjectTypeCode.Name : null,
                HeatingTypeName = p.HeatingTypeCode != null ? p.HeatingTypeCode.Name : null,
                FirmName = p.ProjectFirmAuthorization.ProjectFirm.Title,
                GasDistributionFirmName = p.ProjectFirmAuthorization.GasDistributionFirm.Title,
                BuildingCode = p.Building != null ? p.Building.Code : null,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<ProjectListItemDto>>(new PagedResultDto<ProjectListItemDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize
        });
}

    private static IQueryable<Project> ApplySort(IQueryable<Project> projects, string sortBy, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy switch
        {
            ProjectSortKeys.Name => descending
                ? projects.OrderByDescending(p => p.Name)
                : projects.OrderBy(p => p.Name),
            ProjectSortKeys.CreatedAt => descending
                ? projects.OrderByDescending(p => p.CreatedAt)
                : projects.OrderBy(p => p.CreatedAt),
            _ => descending
                ? projects.OrderByDescending(p => p.UpdatedAt)
                : projects.OrderBy(p => p.UpdatedAt)
        };
    }

    public async Task<IDataResult<ProjectDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dto = await _db.Projects.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .Where(p => p.Id == id)
            .Select(p => new ProjectDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.StatusCode != null ? p.StatusCode.CodeValue : CodeValues.Draft,
                StatusName = p.StatusCode != null ? p.StatusCode.Name : "Taslak",
                ProjectFirmAuthorizationId = p.ProjectFirmAuthorizationId,
                ProjectFirmId = p.ProjectFirmAuthorization.ProjectFirmId,
                GasDistributionFirmId = p.ProjectFirmAuthorization.GasDistributionFirmId,
                GasDistributionFirmName = p.ProjectFirmAuthorization.GasDistributionFirm.Title,
                CityId = p.Building!.CityId,
                CityName = p.Building!.City!.Name,
                DistrictId = p.Building!.DistrictId,
                DistrictName = p.Building!.District!.Name,
                AddressLine = p.Building!.AddressLine,
                BuildingCode = p.Building!.Code,
                BlockLotParcel = p.Building!.BlockLotParcel,
                FloorCount = p.Building!.FloorCount,
                BasementCount = p.Building!.BasementCount,
                ProjectTypeCodeId = p.ProjectTypeCodeId,
                ProjectTypeName = p.ProjectTypeCode != null ? p.ProjectTypeCode.Name : null,
                HeatingTypeCodeId = p.HeatingTypeCodeId,
                HeatingTypeName = p.HeatingTypeCode != null ? p.HeatingTypeCode.Name : null,
                BuildingUsageTypeCodeId = p.BuildingUsageTypeCodeId,
                BuildingUsageTypeName = p.BuildingUsageTypeCode != null ? p.BuildingUsageTypeCode.Name : null,
                IsPermitProject = p.IsPermitProject,
                ApartmentCount = p.ApartmentCount,
                WorkplaceCount = p.WorkplaceCount,
                AreaSquareMeters = p.TotalArea,
                Capacity = p.Capacity,
                ServiceBoxPressureMbar = p.ServiceBoxPressureMbar,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RowVersion = p.RowVersion,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return new ErrorDataResult<ProjectDetailDto>("Proje bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<ProjectDetailDto>(dto);
    }

    public async Task<IDataResult<ProjectDetailDto>> CreateAsync(ProjectCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<ProjectDetailDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var authorization = await _db.ProjectFirmAuthorizations.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == dto.ProjectFirmAuthorizationId, cancellationToken);

        if (authorization is null)
            return new ErrorDataResult<ProjectDetailDto>("Proje firması yetkisi bulunamadı.", StatusCodes.Status404NotFound);

        var isAdmin = _currentUser.RoleCode == RoleCodes.Admin;
        if (!isAdmin && _currentUser.ProjectFirmId != authorization.ProjectFirmId)
            return new ErrorDataResult<ProjectDetailDto>(
                "Başka bir firma adına proje oluşturamazsınız.", StatusCodes.Status403Forbidden);

        var location = await ResolveLocationAsync(dto.CityId, dto.DistrictId, cancellationToken);
        if (location.Error is not null)
            return new ErrorDataResult<ProjectDetailDto>(location.Error, location.StatusCode);

        var projectTypeCheck = await ResolveTypeCodeAsync(
            dto.ProjectTypeCodeId, CodeGroupNames.ProjectType, "proje tipi", cancellationToken);
        if (projectTypeCheck.Error is not null)
            return new ErrorDataResult<ProjectDetailDto>(projectTypeCheck.Error, projectTypeCheck.StatusCode);

        var heatingTypeCheck = await ResolveTypeCodeAsync(
            dto.HeatingTypeCodeId, CodeGroupNames.HeatingType, "ısınma tipi", cancellationToken);
        if (heatingTypeCheck.Error is not null)
            return new ErrorDataResult<ProjectDetailDto>(heatingTypeCheck.Error, heatingTypeCheck.StatusCode);

        var buildingUsageCheck = await ResolveTypeCodeAsync(
            dto.BuildingUsageTypeCodeId, CodeGroupNames.BuildingUsageType, "bina kullanımı tipi", cancellationToken);
        if (buildingUsageCheck.Error is not null)
            return new ErrorDataResult<ProjectDetailDto>(buildingUsageCheck.Error, buildingUsageCheck.StatusCode);

        var buildingCode = dto.BuildingCode?.Trim();
        if (!string.IsNullOrWhiteSpace(buildingCode))
        {
            var codeTaken = await _db.Buildings.AnyAsync(b => b.Code == buildingCode, cancellationToken);
            if (codeTaken)
                return new ErrorDataResult<ProjectDetailDto>("Bu bina kodu başka bir binaya ait.", StatusCodes.Status409Conflict);
        }

        Building? building = null;
        if (dto.CityId.HasValue
            || !string.IsNullOrWhiteSpace(dto.AddressLine)
            || !string.IsNullOrWhiteSpace(dto.BlockLotParcel)
            || !string.IsNullOrWhiteSpace(buildingCode)
            || dto.FloorCount.HasValue
            || dto.BasementCount.HasValue)
        {
            building = new Building
            {
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                AddressLine = dto.AddressLine?.Trim(),
                BlockLotParcel = dto.BlockLotParcel?.Trim(),
                Code = buildingCode,
                FloorCount = dto.FloorCount,
                BasementCount = dto.BasementCount
            };
        }

        var draftStatusId = await _db.Codes.AsNoTracking()
            .Where(c => c.Group.GroupName == CodeGroupNames.ProjectStatus
                     && c.CodeValue == CodeValues.Draft)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var entity = new Project
        {
            Name = dto.Name.Trim(),
            ProjectFirmAuthorizationId = dto.ProjectFirmAuthorizationId,
            Description = dto.Description,
            CreatedByUserId = _currentUser.UserId,
            OwnerUserId = _currentUser.UserId,
            Building = building,
            StatusCodeId = draftStatusId,
            ProjectTypeCodeId = dto.ProjectTypeCodeId,
            HeatingTypeCodeId = dto.HeatingTypeCodeId,
            BuildingUsageTypeCodeId = dto.BuildingUsageTypeCodeId,
            IsPermitProject = dto.IsPermitProject,
            ApartmentCount = dto.ApartmentCount,
            WorkplaceCount = dto.WorkplaceCount,
            TotalArea = dto.AreaSquareMeters,
            Capacity = dto.Capacity,
            ServiceBoxPressureMbar = dto.ServiceBoxPressureMbar
        };

        _db.Projects.Add(entity);
        await _db.SaveChangesWithOutboxAsync(nameof(Project), "Index", () => entity.Id, cancellationToken);

        return new SuccessDataResult<ProjectDetailDto>(
            await LoadDetailAsync(entity.Id, cancellationToken) ?? throw new InvalidOperationException(),
            "Proje oluşturuldu.");
    }

    public async Task<IResult> UpdateAsync(int id, ProjectUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var entity = await _db.Projects
            .WhereVisibleTo(_currentUser)
            .Include(p => p.Building)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null)
            return new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound);
        
        _db.Entry(entity).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;

        var location = await ResolveLocationAsync(dto.CityId, dto.DistrictId, cancellationToken);
        if (location.Error is not null)
            return new ErrorResult(location.Error, location.StatusCode);

        var projectTypeCheck = await ResolveTypeCodeAsync(
            dto.ProjectTypeCodeId, CodeGroupNames.ProjectType, "proje tipi", cancellationToken);
        if (projectTypeCheck.Error is not null)
            return new ErrorResult(projectTypeCheck.Error, projectTypeCheck.StatusCode);

        var heatingTypeCheck = await ResolveTypeCodeAsync(
            dto.HeatingTypeCodeId, CodeGroupNames.HeatingType, "ısınma tipi", cancellationToken);
        if (heatingTypeCheck.Error is not null)
            return new ErrorResult(heatingTypeCheck.Error, heatingTypeCheck.StatusCode);

        var buildingUsageCheck = await ResolveTypeCodeAsync(
            dto.BuildingUsageTypeCodeId, CodeGroupNames.BuildingUsageType, "bina kullanımı tipi", cancellationToken);
        if (buildingUsageCheck.Error is not null)
            return new ErrorResult(buildingUsageCheck.Error, buildingUsageCheck.StatusCode);

        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.UpdatedByUserId = _currentUser.UserId;
        entity.ProjectTypeCodeId = dto.ProjectTypeCodeId;
        entity.HeatingTypeCodeId = dto.HeatingTypeCodeId;
        entity.BuildingUsageTypeCodeId = dto.BuildingUsageTypeCodeId;
        entity.IsPermitProject = dto.IsPermitProject;
        entity.ApartmentCount = dto.ApartmentCount;
        entity.WorkplaceCount = dto.WorkplaceCount;
        entity.TotalArea = dto.AreaSquareMeters;
        entity.Capacity = dto.Capacity;
        entity.ServiceBoxPressureMbar = dto.ServiceBoxPressureMbar;

        var addressLine = dto.AddressLine?.Trim();
        var blockLotParcel = dto.BlockLotParcel?.Trim();
        var buildingCode = dto.BuildingCode?.Trim();

        if (!string.IsNullOrWhiteSpace(buildingCode))
        {
            var codeTaken = await _db.Buildings.AnyAsync(
                b => b.Code == buildingCode && b.Id != (entity.Building != null ? entity.Building.Id : 0),
                cancellationToken);
            if (codeTaken)
                return new ErrorResult("Bu bina kodu başka bir binaya ait.", StatusCodes.Status409Conflict);
        }

        if (entity.Building is not null)
        {
            entity.Building.CityId = dto.CityId;
            entity.Building.DistrictId = dto.DistrictId;
            entity.Building.AddressLine = addressLine;
            entity.Building.BlockLotParcel = blockLotParcel;
            entity.Building.Code = buildingCode;
            entity.Building.FloorCount = dto.FloorCount;
            entity.Building.BasementCount = dto.BasementCount;
        }
        else if (dto.CityId.HasValue
                 || !string.IsNullOrWhiteSpace(addressLine)
                 || !string.IsNullOrWhiteSpace(blockLotParcel)
                 || !string.IsNullOrWhiteSpace(buildingCode)
                 || dto.FloorCount.HasValue
                 || dto.BasementCount.HasValue)
        {
            entity.Building = new Building
            {
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                AddressLine = addressLine,
                BlockLotParcel = blockLotParcel,
                Code = buildingCode,
                FloorCount = dto.FloorCount,
                BasementCount = dto.BasementCount
            };
        }

        await _db.SaveChangesWithOutboxAsync(nameof(Project), "Index", () => entity.Id, cancellationToken);
        return new SuccessResult("Proje güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Projects
            .WhereVisibleTo(_currentUser)
            .Include(p => p.Building)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null)
            return new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound);

        _db.Projects.Remove(entity);

        if (entity.Building is not null)
            _db.Buildings.Remove(entity.Building);

        await _db.SaveChangesWithOutboxAsync(nameof(Project), "Delete", () => entity.Id, cancellationToken);

        return new SuccessResult("Proje silindi.");
    }

    /// <summary>
    /// Bir Kod alanının (ProjectTypeCodeId vb.) var olduğunu VE beklenen gruba ait olduğunu doğrular.
    /// Aynı yardımcı proje tipi, ısınma tipi ve bina kullanımı tipi için tekrar kullanılıyor —
    /// üçü de aynı "kimlik var mı + doğru grupta mı" sorusunu soruyor.
    /// </summary>
    private async Task<(string? Error, int StatusCode)> ResolveTypeCodeAsync(
        int kodId, string groupName, string fieldLabel, CancellationToken cancellationToken)
    {
        var exists = await _db.Codes.AsNoTracking()
            .AnyAsync(c => c.Id == kodId && c.Group.GroupName == groupName, cancellationToken);

        return exists ? (null, StatusCodes.Status200OK) : ($"Geçerli bir {fieldLabel} seçiniz.", StatusCodes.Status404NotFound);
    }

    private async Task<(string? Error, int StatusCode, string? CityName, string? DistrictName)> ResolveLocationAsync(
        int? cityId, int? districtId, CancellationToken cancellationToken)
    {
        if (districtId.HasValue && !cityId.HasValue)
            return ("İlçe seçmek için önce il seçilmelidir.", StatusCodes.Status400BadRequest, null, null);

        string? cityName = null;
        if (cityId.HasValue)
        {
            cityName = await _db.Cities.AsNoTracking()
                .Where(c => c.Id == cityId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (cityName is null)
                return ("İl bulunamadı.", StatusCodes.Status404NotFound, null, null);
        }

        string? districtName = null;
        if (districtId.HasValue)
        {
            districtName = await _db.Districts.AsNoTracking()
                .Where(d => d.Id == districtId.Value && d.CityId == cityId!.Value)
                .Select(d => d.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (districtName is null)
                return ("İlçe bulunamadı veya seçilen ile ait değil.", StatusCodes.Status404NotFound, null, null);
        }

        return (null, StatusCodes.Status200OK, cityName, districtName);
    }
    
    public async Task<IDataResult<ProjectStatusCountsDto>> GetStatusCountsAsync(
        ProjectStatusCountsQueryDto query, CancellationToken cancellationToken = default)
    {
        var projects = ApplyCommonFilters(
            _db.Projects.AsNoTracking().WhereVisibleTo(_currentUser),
            query.DateFrom, query.DateTo, query.CityId, query.DistrictId, query.ProjectFirmId, query.GdFirmId, query.GdGroupId);


        var counts = await projects
            .GroupBy(p => p.StatusCode != null ? p.StatusCode.CodeValue : CodeValues.Draft)
            .Select(g => new { CodeValue = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CodeValue, x => x.Count, cancellationToken);

        return new SuccessDataResult<ProjectStatusCountsDto>(new ProjectStatusCountsDto
        {
            Draft           = counts.GetValueOrDefault(CodeValues.Draft),
            PendingApproval = counts.GetValueOrDefault(CodeValues.PendingApproval),
            Approved        = counts.GetValueOrDefault(CodeValues.Approved),
            Rejected        = counts.GetValueOrDefault(CodeValues.Rejected)
        });
    }
    
    private static IQueryable<Project> ApplyCommonFilters(
        IQueryable<Project> projects,
        DateTime? dateFrom, DateTime? dateTo, int? cityId, int? districtId, int? projectFirmId, int? gdFirmId, int? gdGroupId)
    {
        if (dateFrom is { } from)
            projects = projects.Where(p => p.CreatedAt >= from.Date);

        if (dateTo is { } to)
            projects = projects.Where(p => p.CreatedAt < to.Date.AddDays(1));

        if (districtId is { } d)
            projects = projects.Where(p => p.Building != null && p.Building.DistrictId == d);

        if (cityId is { } c)
            projects = projects.Where(p => p.Building != null && p.Building.CityId == c);

        if (projectFirmId is { } f)
            projects = projects.Where(p => p.ProjectFirmAuthorization.ProjectFirmId == f);
        
        if (gdGroupId is { } groupId)
            projects = projects.Where(p => p.ProjectFirmAuthorization.GasDistributionFirm.GroupId == groupId);

        if (gdFirmId is { } firmId)
            projects = projects.Where(p => p.ProjectFirmAuthorization.GasDistributionFirmId == firmId);

        return projects;
    }
    
    /// <summary>
    /// Create sonrası tek kayıttan tam detay okur — GetByIdAsync ile aynı projeksiyonu
    /// tekrar yazmak yerine paylaşıyor, Kod adları (Name) dahil tüm alanlar tutarlı kalır.
    /// </summary>
    private async Task<ProjectDetailDto?> LoadDetailAsync(int id, CancellationToken cancellationToken)
    {
        var result = await GetByIdAsync(id, cancellationToken);
        return result.Success ? result.Data : null;
    }
}