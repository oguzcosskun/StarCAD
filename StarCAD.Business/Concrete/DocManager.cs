using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Common.Constants;
using StarCAD.Core.Security;
using StarCAD.Core.Storage;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Common;
using StarCAD.ViewModel.Docs;

namespace StarCAD.Business.Concrete;

public class DocManager : IDocService
{
    private readonly StarCADContext _db;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<DocUploadDto> _uploadValidator;

    public DocManager(
        StarCADContext db,
        IFileStorageService storage,
        ICurrentUserService currentUser,
        IValidator<DocUploadDto> uploadValidator)
    {
        _db = db;
        _storage = storage;
        _currentUser = currentUser;
        _uploadValidator = uploadValidator;
    }

    public async Task<IDataResult<DocDetailDto>> UploadAsync(
        int? projectId, int? projectUnitId, int? docTypeCodeId, int? projectFirmId,
        string fileName, string contentType, long sizeBytes, Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var dto = new DocUploadDto
        {
            ProjectId = projectId,
            ProjectUnitId = projectUnitId,
            DocTypeCodeId = docTypeCodeId,
            ProjectFirmId = projectFirmId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes
        };

        var validation = await _uploadValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorDataResult<DocDetailDto>(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        var (target, targetError) = await ResolveTargetAsync(dto.ProjectId, dto.ProjectUnitId, dto.ProjectFirmId, cancellationToken);
        if (targetError is not null)
            return new ErrorDataResult<DocDetailDto>(targetError.Message, targetError.StatusCode);

        if (dto.DocTypeCodeId is { } docTypeCodeValue)
        {
            var docTypeValid = await _db.Codes.AsNoTracking()
                .AnyAsync(c => c.Id == docTypeCodeValue && c.Group.GroupName == CodeGroupNames.DocumentType, cancellationToken);
            if (!docTypeValid)
                return new ErrorDataResult<DocDetailDto>("Geçersiz evrak türü.", StatusCodes.Status400BadRequest);
        }

        // Orijinal dosya adı object key'e gömülmez (path traversal / garip karakter riski); GUID + uzantı yeterli.
        // Firma id'sine göre klasörlenir — havuzdaki evrak da ResolveTargetAsync'in çözdüğü firmaya göre yazılır.
        var extension = Path.GetExtension(dto.FileName);
        var objectKey = $"docs/{target!.Value.ProjectFirmId}/{Guid.NewGuid():N}{extension}";

        await _storage.PutObjectAsync(objectKey, fileStream, dto.ContentType, cancellationToken);

        var doc = new Doc
        {
            DocTypeCodeId = dto.DocTypeCodeId,
            FileName = dto.FileName.Trim(),
            FilePath = objectKey,
            ContentType = dto.ContentType,
            SizeBytes = dto.SizeBytes,
            ReceivedAt = DateTime.UtcNow,
            UploadedByUserId = _currentUser.UserId,
            ProjectId = target!.Value.ProjectId,
            ProjectFirmId = target.Value.ProjectFirmId
        };

        _db.Docs.Add(doc);

        // Doc navigasyonu üzerinden ekleniyor ki EF, henüz kaydedilmemiş doc.Id'yi tek
        // SaveChangesAsync'te otomatik çözsün (FK fixup) — ayrı bir round-trip gerekmez.
        if (target.Value.ProjectUnitId is { } initialUnitId)
        {
            _db.DocProjectUnits.Add(new DocProjectUnit
            {
                Doc = doc,
                ProjectUnitId = initialUnitId,
                ProjectId = target.Value.ProjectId!.Value,
                ProjectFirmId = target.Value.ProjectFirmId,
                CreatedAt = DateTime.UtcNow
            });
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Temizlik istemci gitse bile tamamlanmalı → CancellationToken.None.
            await _storage.RemoveObjectAsync(objectKey, CancellationToken.None);
            throw;
        }

        return new SuccessDataResult<DocDetailDto>(await GetDetailAsync(doc.Id, cancellationToken), "Evrak yüklendi.");
    }

    public async Task<IResult> AssignAsync(int id, DocAssignDto dto, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Docs.WhereVisibleTo(_currentUser).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doc is null)
            return new ErrorResult("Evrak bulunamadı.", StatusCodes.Status404NotFound);

        if (dto.ProjectId is null)
        {
            // Havuza geri al: firma bağı (ProjectFirmId) değişmez, yalnızca proje bağı kalkar.
            doc.ProjectId = null;
        }
        else
        {
            // explicitProjectFirmId burada uygulanamaz — bu dal yalnızca projectId doluyken çalışır,
            // firma zaten projeden çözülür. projectUnitId de burada yok — birim bağlama artık
            // ayrı LinkUnitAsync/UnlinkUnitAsync ile yönetiliyor (many-to-many).
            var (target, targetError) = await ResolveTargetAsync(dto.ProjectId, null, null, cancellationToken);
            if (targetError is not null)
                return targetError;

            // Bir firmanın evrakı başka bir firmanın projesine bağlanamaz.
            if (target!.Value.ProjectFirmId != doc.ProjectFirmId)
                return new ErrorResult("Bu evrak başka bir firmaya ait, bu projeye bağlanamaz.", StatusCodes.Status403Forbidden);

            doc.ProjectId = target.Value.ProjectId;
        }

        // Proje değişti/havuza döndü: eski projenin birim bağlantıları yeni bağlamda anlamsız kalır.
        await _db.DocProjectUnits.Where(l => l.DocId == id).ExecuteDeleteAsync(cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Evrak güncellendi.");
    }

    /// <summary>
    /// Havuzdaki (ProjectId=null) bir evrakı doğrudan bir birime bağlar — önce ayrıca
    /// AssignAsync ile projeye atanmasına gerek yok, proje/firma birimden çözülüp Doc'a geri
    /// doldurulur. Proje zaten atanmışsa hedef birim o projeye ait olmak zorundadır.
    /// </summary>
    public async Task<IResult> LinkUnitAsync(int docId, int projectUnitId, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Docs.WhereVisibleTo(_currentUser).FirstOrDefaultAsync(d => d.Id == docId, cancellationToken);
        if (doc is null)
            return new ErrorResult("Evrak bulunamadı.", StatusCodes.Status404NotFound);

        var unit = await _db.ProjectUnits.AsNoTracking()
            .Where(u => u.Id == projectUnitId)
            .Select(u => new { u.Id, u.ProjectId })
            .FirstOrDefaultAsync(cancellationToken);
        if (unit is null)
            return new ErrorResult("Bağımsız bölüm bulunamadı.", StatusCodes.Status404NotFound);

        var project = await _db.Projects.AsNoTracking()
            .Where(p => p.Id == unit.ProjectId)
            .Select(p => new { p.Id, ProjectFirmId = p.ProjectFirmAuthorization.ProjectFirmId })
            .FirstOrDefaultAsync(cancellationToken);
        if (project is null)
            return new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound);

        if (doc.ProjectId is { } currentProjectId && currentProjectId != project.Id)
            return new ErrorResult("Bu evrak zaten başka bir projeye bağlı.", StatusCodes.Status400BadRequest);

        if (doc.ProjectFirmId != project.ProjectFirmId)
            return new ErrorResult("Bu evrak başka bir firmaya ait, bu birime bağlanamaz.", StatusCodes.Status403Forbidden);

        var alreadyLinked = await _db.DocProjectUnits.AsNoTracking()
            .AnyAsync(l => l.DocId == docId && l.ProjectUnitId == projectUnitId, cancellationToken);
        if (alreadyLinked)
            return new SuccessResult("Evrak zaten bu bağımsız bölüme bağlı.");

        // Havuzdaysa (ProjectId=null) proje bilgisi birimden geri doldurulur.
        doc.ProjectId ??= project.Id;

        _db.DocProjectUnits.Add(new DocProjectUnit
        {
            DocId = docId,
            ProjectUnitId = projectUnitId,
            ProjectId = project.Id,
            ProjectFirmId = project.ProjectFirmId,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Evrak bağımsız bölüme bağlandı.");
    }

    public async Task<IResult> UnlinkUnitAsync(int docId, int projectUnitId, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Docs.WhereVisibleTo(_currentUser).FirstOrDefaultAsync(d => d.Id == docId, cancellationToken);
        if (doc is null)
            return new ErrorResult("Evrak bulunamadı.", StatusCodes.Status404NotFound);

        var link = await _db.DocProjectUnits
            .FirstOrDefaultAsync(l => l.DocId == docId && l.ProjectUnitId == projectUnitId, cancellationToken);
        if (link is null)
            return new ErrorResult("Bağlantı bulunamadı.", StatusCodes.Status404NotFound);

        // DocProjectUnit BaseEntity'den türer (SoftDeleteEntity değil) — Remove doğrudan hard-delete'tir.
        _db.DocProjectUnits.Remove(link);

        // LinkUnitAsync'in geri doldurmasıyla simetrik: bu son bağlantıysa evrak havuza döner.
        // ProjectFirmId asla dokunulmaz — WhereVisibleTo firma kullanıcısını buna göre filtreliyor,
        // null olursa evrak Admin dışında kimseye görünmez hale gelirdi.
        var hasOtherLinks = await _db.DocProjectUnits
            .AnyAsync(l => l.DocId == docId && l.Id != link.Id, cancellationToken);
        if (!hasOtherLinks)
            doc.ProjectId = null;

        await _db.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Bağlantı kaldırıldı.");
    }

    public async Task<IDataResult<PagedResultDto<DocListItemDto>>> GetListAsync(
        DocListQueryDto query, CancellationToken cancellationToken = default)
    {
        var docs = _db.Docs.AsNoTracking().WhereVisibleTo(_currentUser);

        if (query.ProjectId is { } projectId)
            docs = docs.Where(d => d.ProjectId == projectId);

        if (query.ProjectUnitId is { } projectUnitId)
            docs = docs.Where(d => _db.DocProjectUnits.Any(l => l.DocId == d.Id && l.ProjectUnitId == projectUnitId));

        if (query.ProjectFirmId is { } projectFirmId)
            docs = docs.Where(d => d.ProjectFirmId == projectFirmId);

        if (query.DocTypeCodeId is { } docTypeCodeId)
            docs = docs.Where(d => d.DocTypeCodeId == docTypeCodeId);

        if (query.ReceivedFrom is { } receivedFrom)
            docs = docs.Where(d => d.ReceivedAt >= receivedFrom.Date);

        if (query.ReceivedTo is { } receivedTo)
            docs = docs.Where(d => d.ReceivedAt < receivedTo.Date.AddDays(1));

        if (query.PoolOnly)
            docs = docs.Where(d => d.ProjectId == null);

        var totalCount = await docs.CountAsync(cancellationToken);

        docs = ApplySort(docs, query.SortBy, query.SortDir);

        const int maxPageSize = 100;
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 30 : Math.Min(query.PageSize, maxPageSize);

        var items = await docs
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DocListItemDto
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes,
                DocTypeCodeId = d.DocTypeCodeId,
                DocTypeName = d.DocTypeCode!.Name,
                ProjectId = d.ProjectId,
                ProjectName = d.Project != null ? d.Project.Name : null,
                ProjectUnits = _db.DocProjectUnits
                    .Where(l => l.DocId == d.Id)
                    .Select(l => new DocProjectUnitSummaryDto
                    {
                        Id = l.ProjectUnitId,
                        UnitNumber = l.ProjectUnit.UnitNumber,
                        SubscriberNo = l.ProjectUnit.SubscriberNo
                    })
                    .ToList(),
                ProjectFirmId = d.ProjectFirmId,
                ProjectFirmName = d.ProjectFirm != null ? d.ProjectFirm.Title : null,
                ReceivedAt = d.ReceivedAt
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<PagedResultDto<DocListItemDto>>(new PagedResultDto<DocListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<IDataResult<DocDownloadDto>> GetDownloadUrlAsync(int id, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Docs.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doc is null)
            return new ErrorDataResult<DocDownloadDto>("Evrak bulunamadı.", StatusCodes.Status404NotFound);

        if (string.IsNullOrEmpty(doc.FilePath) || !await _storage.ObjectExistsAsync(doc.FilePath, cancellationToken))
            return new ErrorDataResult<DocDownloadDto>("Evrak dosyası depoda bulunamadı.", StatusCodes.Status404NotFound);

        var url = await _storage.CreatePresignedGetUrlAsync(doc.FilePath, cancellationToken);

        return new SuccessDataResult<DocDownloadDto>(new DocDownloadDto { Url = url });
    }

    public async Task<IResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var doc = await _db.Docs.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (doc is null)
            return new ErrorResult("Evrak bulunamadı.", StatusCodes.Status404NotFound);

        // §2'nin genel soft-delete kuralına bilinçli istisna (bkz. knowledge/doc-delete-hard-delete.md):
        // asıl dosya MinIO'dan da silinecek, IsActive=false bırakmak kaynağı olmayan bir hayalet
        // satır bırakmaktan başka bir şey yapmaz. Önce SQL (ExecuteDeleteAsync — SaveChanges'i
        // atlayan bulk silme, soft-delete interceptor'ını görmez), sonra MinIO: DB silme
        // başarısız olursa MinIO'ya hiç dokunulmamış olur; MinIO silme başarısız olursa geriye
        // yalnızca kullanıcıya hiç görünmeyen, zararsız bir artık nesne kalır (güvenli yön).
        await _db.Docs.Where(d => d.Id == id).ExecuteDeleteAsync(cancellationToken);

        if (!string.IsNullOrEmpty(doc.FilePath))
            await _storage.RemoveObjectAsync(doc.FilePath, cancellationToken);

        return new SuccessResult("Evrak silindi.");
    }

    private readonly record struct ResolvedTarget(int? ProjectId, int? ProjectUnitId, int ProjectFirmId);

    /// <summary>
    /// projectId null ise havuz hedefi çözülür: firma kullanıcısı kendi firmasına yükler
    /// (explicitProjectFirmId varsa bile yok sayılır — başka firmanın havuzuna yazamaz);
    /// Admin'in firma bağı olmadığı için hangi firmanın havuzuna yüklediğini explicitProjectFirmId
    /// ile açıkça belirtmesi gerekir. projectId doluysa proje + (varsa) bağımsız bölüm doğrulanır
    /// ve projenin firma bağı döner.
    /// </summary>
    private async Task<(ResolvedTarget? Target, ErrorResult? Error)> ResolveTargetAsync(
        int? projectId, int? projectUnitId, int? explicitProjectFirmId, CancellationToken cancellationToken)
    {
        if (projectId is null)
        {
            if (_currentUser.ProjectFirmId is { } firmId)
                return (new ResolvedTarget(null, null, firmId), null);

            if (explicitProjectFirmId is not { } targetFirmId)
                return (null, new ErrorResult(
                    "Havuza yüklerken bir proje firması belirtilmelidir.", StatusCodes.Status400BadRequest));

            var firmExists = await _db.ProjectFirms.AsNoTracking()
                .AnyAsync(f => f.Id == targetFirmId, cancellationToken);
            if (!firmExists)
                return (null, new ErrorResult("Proje firması bulunamadı.", StatusCodes.Status404NotFound));

            return (new ResolvedTarget(null, null, targetFirmId), null);
        }

        var project = await _db.Projects.AsNoTracking()
            .WhereVisibleTo(_currentUser)
            .Where(p => p.Id == projectId)
            .Select(p => new { p.Id, ProjectFirmId = p.ProjectFirmAuthorization.ProjectFirmId })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return (null, new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound));

        if (projectUnitId is { } unitId)
        {
            var unitBelongs = await _db.ProjectUnits.AsNoTracking()
                .AnyAsync(u => u.Id == unitId && u.ProjectId == projectId, cancellationToken);
            if (!unitBelongs)
                return (null, new ErrorResult("Bağımsız bölüm bu projeye ait değil.", StatusCodes.Status400BadRequest));
        }

        return (new ResolvedTarget(projectId, projectUnitId, project.ProjectFirmId), null);
    }

    private async Task<DocDetailDto> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Docs.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DocDetailDto
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes,
                DocTypeCodeId = d.DocTypeCodeId,
                DocTypeName = d.DocTypeCode!.Name,
                ProjectId = d.ProjectId,
                ProjectName = d.Project != null ? d.Project.Name : null,
                ProjectUnits = _db.DocProjectUnits
                    .Where(l => l.DocId == d.Id)
                    .Select(l => new DocProjectUnitSummaryDto
                    {
                        Id = l.ProjectUnitId,
                        UnitNumber = l.ProjectUnit.UnitNumber,
                        SubscriberNo = l.ProjectUnit.SubscriberNo
                    })
                    .ToList(),
                ProjectFirmId = d.ProjectFirmId,
                ProjectFirmName = d.ProjectFirm != null ? d.ProjectFirm.Title : null,
                UploadedByUserId = d.UploadedByUserId,
                UploadedByUserFullName = d.UploadedByUser!.FullName,
                ReceivedAt = d.ReceivedAt
            })
            .FirstAsync(cancellationToken);
    }

    private static IQueryable<Doc> ApplySort(IQueryable<Doc> docs, string sortBy, string sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return string.Equals(sortBy, DocSortKeys.FileName, StringComparison.OrdinalIgnoreCase)
            ? (descending
                ? docs.OrderByDescending(d => d.FileName).ThenByDescending(d => d.Id)
                : docs.OrderBy(d => d.FileName).ThenBy(d => d.Id))
            : (descending
                ? docs.OrderByDescending(d => d.ReceivedAt).ThenByDescending(d => d.Id)
                : docs.OrderBy(d => d.ReceivedAt).ThenBy(d => d.Id));
    }
}
