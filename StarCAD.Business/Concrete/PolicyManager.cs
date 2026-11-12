using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Policies;

namespace StarCAD.Business.Concrete;

public class PolicyManager : IPolicyService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;

    public PolicyManager(StarCADContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IDataResult<PolicyDto>> CreateAsync(PolicyAddDto dto, CancellationToken cancellationToken = default)
    {
        var unit = await _db.ProjectUnits.AsNoTracking()
            .Where(u => u.Id == dto.ProjectUnitId)
            .Select(u => new { u.Id, u.ProjectId })
            .FirstOrDefaultAsync(cancellationToken);
        if (unit is null)
            return new ErrorDataResult<PolicyDto>("Bağımsız bölüm bulunamadı.", StatusCodes.Status404NotFound);

        var projectVisible = await _db.Projects.WhereVisibleTo(_currentUser)
            .AnyAsync(p => p.Id == unit.ProjectId, cancellationToken);
        if (!projectVisible)
            return new ErrorDataResult<PolicyDto>("Proje bulunamadı.", StatusCodes.Status404NotFound);

        if (dto.InsuranceCompanyId is { } insuranceCompanyId)
        {
            var insuranceCompanyExists = await _db.InsuranceCompanies.AsNoTracking()
                .AnyAsync(c => c.Id == insuranceCompanyId, cancellationToken);
            if (!insuranceCompanyExists)
                return new ErrorDataResult<PolicyDto>("Sigorta şirketi bulunamadı.", StatusCodes.Status404NotFound);
        }

        // Bir birimde aynı anda en fazla 1 aktif poliçe olur — otomatik kapatma yok, çağıran
        // önce mevcut poliçeyi DeleteAsync ile elle iptal etmeli.
        var hasActivePolicy = await _db.Policies
            .AnyAsync(p => p.ProjectUnitId == dto.ProjectUnitId, cancellationToken);
        if (hasActivePolicy)
            return new ErrorDataResult<PolicyDto>(
                "Bu bağımsız bölümde zaten aktif bir poliçe var. Yeni poliçe eklemeden önce mevcut poliçeyi iptal edin.",
                StatusCodes.Status400BadRequest);

        var policy = new Policy
        {
            ProjectId = unit.ProjectId,
            ProjectUnitId = dto.ProjectUnitId,
            InsuranceCompanyId = dto.InsuranceCompanyId,
            PolicyNumber = string.IsNullOrWhiteSpace(dto.PolicyNumber) ? null : dto.PolicyNumber.Trim(),
            Amount = dto.Amount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await _db.Policies.AsNoTracking()
            .Where(p => p.Id == policy.Id)
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                ProjectId = p.ProjectId,
                ProjectName = p.Project.Name,
                ProjectUnitId = p.ProjectUnitId,
                UnitNumber = p.ProjectUnit != null ? p.ProjectUnit.UnitNumber : null,
                InsuranceCompanyId = p.InsuranceCompanyId,
                InsuranceCompanyTitle = p.InsuranceCompany != null ? p.InsuranceCompany.Title : null,
                PolicyNumber = p.PolicyNumber,
                Amount = p.Amount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                IsUnitDeleted = p.IsUnitDeleted
            })
            .FirstAsync(cancellationToken);

        return new SuccessDataResult<PolicyDto>(created, "Poliçe oluşturuldu.");
    }

    public async Task<IResult> UpdateAsync(int id, PolicyUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy is null)
            return new ErrorResult("Poliçe bulunamadı.", StatusCodes.Status404NotFound);

        if (dto.InsuranceCompanyId is { } insuranceCompanyId)
        {
            var insuranceCompanyExists = await _db.InsuranceCompanies.AsNoTracking()
                .AnyAsync(c => c.Id == insuranceCompanyId, cancellationToken);
            if (!insuranceCompanyExists)
                return new ErrorResult("Sigorta şirketi bulunamadı.", StatusCodes.Status404NotFound);
        }

        policy.InsuranceCompanyId = dto.InsuranceCompanyId;
        policy.PolicyNumber = string.IsNullOrWhiteSpace(dto.PolicyNumber) ? null : dto.PolicyNumber.Trim();
        policy.Amount = dto.Amount;
        policy.StartDate = dto.StartDate;
        policy.EndDate = dto.EndDate;

        await _db.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Poliçe güncellendi.");
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var policy = await _db.Policies.WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (policy is null)
            return new ErrorResult("Poliçe bulunamadı.", StatusCodes.Status404NotFound);

        // Elle iptal: IsUnitDeleted dokunulmaz, false kalır — birim silindiği için otomatik
        // kapanmadan (bkz. UnitSyncManager) ayırt edilebilsin diye.
        _db.Policies.Remove(policy);
        await _db.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Poliçe iptal edildi.");
    }

    public async Task<IDataResult<PagedResultDto<PolicyDto>>> GetListAsync(
        PolicyListQueryDto query, CancellationToken cancellationToken = default)
    {
        // Global IsActive filtresi burada bilinçli olarak KALDIRILMAZ: elle iptal edilen poliçe
        // (IsActive=false) standart soft-delete gibi davranır, kullanıcıya bir daha görünmez.
        // Birim silindiği için kapanan poliçe ise IsActive=true kalır (bkz. UnitSyncManager),
        // bu listede otomatik olarak görünür — yalnızca IsUnitDeleted bayrağıyla işaretlenir.
        var policies = _db.Policies.AsNoTracking().WhereVisibleTo(_currentUser);

        // ProjectId verilmezse WhereVisibleTo'nun kendi kapsamı (admin hepsi, firma kullanıcısı
        // kendi projeleri) geçerli olur — "tüm poliçeler" ekranı.
        if (query.ProjectId is { } projectId)
        {
            var projectVisible = await _db.Projects.WhereVisibleTo(_currentUser)
                .AnyAsync(p => p.Id == projectId, cancellationToken);
            if (!projectVisible)
                return new ErrorDataResult<PagedResultDto<PolicyDto>>("Proje bulunamadı.", StatusCodes.Status404NotFound);

            policies = policies.Where(p => p.ProjectId == projectId);
        }

        if (query.InsuranceCompanyId is { } insuranceCompanyId)
            policies = policies.Where(p => p.InsuranceCompanyId == insuranceCompanyId);

        // excludeUnitDeleted: birim hâlâ bağlı, gerçekten yürürlükteki poliçelerle sınırlar —
        // birim silindiği için kapanmış olanları da (varsayılan listede görünen) dışarıda bırakır.
        if (query.ExcludeUnitDeleted)
            policies = policies.Where(p => !p.IsUnitDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            policies = policies.Where(p =>
                (p.PolicyNumber != null && p.PolicyNumber.Contains(search)) ||
                (p.ProjectUnit != null && p.ProjectUnit.UnitNumber != null && p.ProjectUnit.UnitNumber.Contains(search)) ||
                (p.ProjectUnit != null && p.ProjectUnit.SubscriberNo != null && p.ProjectUnit.SubscriberNo.Contains(search)));
        }

        var totalCount = await policies.CountAsync(cancellationToken);

        var page = query.Page < 1 ? 1 : query.Page;
        const int maxPageSize = 100;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await policies
            .OrderByDescending(p => p.StartDate)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                ProjectId = p.ProjectId,
                ProjectName = p.Project.Name,
                ProjectUnitId = p.ProjectUnitId,
                UnitNumber = p.ProjectUnit != null ? p.ProjectUnit.UnitNumber : null,
                InsuranceCompanyId = p.InsuranceCompanyId,
                InsuranceCompanyTitle = p.InsuranceCompany != null ? p.InsuranceCompany.Title : null,
                PolicyNumber = p.PolicyNumber,
                Amount = p.Amount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                IsUnitDeleted = p.IsUnitDeleted
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<PolicyDto>>(new PagedResultDto<PolicyDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<IDataResult<PolicyDto>> GetDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        // Global IsActive filtresi burada da kaldırılmaz: elle iptal edilen poliçe artık
        // kullanıcıya görünmemeli (standart soft-delete). Birim silindiği için kapanan poliçe
        // IsActive=true kaldığından zaten normal şekilde erişilebilir kalır.
        var dto = await _db.Policies.AsNoTracking().WhereVisibleTo(_currentUser)
            .Where(p => p.Id == id)
            .Select(p => new PolicyDto
            {
                Id = p.Id,
                ProjectId = p.ProjectId,
                ProjectName = p.Project.Name,
                ProjectUnitId = p.ProjectUnitId,
                UnitNumber = p.ProjectUnit != null ? p.ProjectUnit.UnitNumber : null,
                InsuranceCompanyId = p.InsuranceCompanyId,
                InsuranceCompanyTitle = p.InsuranceCompany != null ? p.InsuranceCompany.Title : null,
                PolicyNumber = p.PolicyNumber,
                Amount = p.Amount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                IsUnitDeleted = p.IsUnitDeleted
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
            return new ErrorDataResult<PolicyDto>("Poliçe bulunamadı.", StatusCodes.Status404NotFound);

        return new SuccessDataResult<PolicyDto>(dto);
    }
}
