using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.Business.Security;
using StarCAD.Common.Constants;
using StarCAD.Core.Security;
using StarCAD.Core.Utilities.Results;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;
using StarCAD.ViewModel.Projects;

namespace StarCAD.Business.Concrete;

public class ProjectApprovalManager : IProjectApprovalService
{
    private readonly StarCADContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ProjectRejectDto> _rejectValidator;

    public ProjectApprovalManager(
        StarCADContext db,
        ICurrentUserService currentUser,
        IValidator<ProjectRejectDto> rejectValidator)
    {
        _db = db;
        _currentUser = currentUser;
        _rejectValidator = rejectValidator;
    }

    public async Task<IResult> SubmitAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateSubmissionRequirementsAsync(projectId, cancellationToken);
        if (validationError is not null)
            return validationError;

        return await TransitionAsync(
            projectId,
            CodeValues.PendingApproval,
            [CodeValues.Draft, CodeValues.Rejected],
            description: null,
            "Proje onaya gönderildi.",
            cancellationToken);
    }

    // Onaya gönderme, durum geçişinden ayrı iki iş kuralı gerektirir: her birimde müşteri
    // sözleşmesi + en az bir birimde poliçe. Approve/Reject bu kontrolden etkilenmez, o yüzden
    // TransitionAsync'e değil sadece Submit'e bağlı.
    private async Task<IResult?> ValidateSubmissionRequirementsAsync(int projectId, CancellationToken cancellationToken)
    {
        var projectExists = await _db.Projects.WhereVisibleTo(_currentUser)
            .AnyAsync(p => p.Id == projectId, cancellationToken);
        if (!projectExists)
            return new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound);

        var unitIds = await _db.ProjectUnits
            .Where(u => u.ProjectId == projectId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (unitIds.Count == 0)
            return new ErrorResult("Projede en az bir birim tanımlı olmalı.", StatusCodes.Status400BadRequest);

        var unitsWithAgreement = await _db.DocProjectUnits
            .Where(dpu => dpu.ProjectId == projectId
                       && dpu.Doc.DocTypeCode != null
                       && dpu.Doc.DocTypeCode.CodeValue == CodeValues.CustomerAgreement)
            .Select(dpu => dpu.ProjectUnitId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var missingAgreementCount = unitIds.Except(unitsWithAgreement).Count();
        if (missingAgreementCount > 0)
            return new ErrorResult(
                $"{missingAgreementCount} birimde müşteri sözleşmesi eksik. Onaya göndermeden önce her birim için müşteri sözleşmesi yüklenmelidir.",
                StatusCodes.Status400BadRequest);

        var hasPolicy = await _db.Policies
            .AnyAsync(p => p.ProjectId == projectId && p.ProjectUnitId != null, cancellationToken);
        if (!hasPolicy)
            return new ErrorResult("Projenin en az bir biriminde poliçe olmalı.", StatusCodes.Status400BadRequest);

        return null;
    }

    public async Task<IResult> ApproveAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await TransitionAsync(
            projectId,
            CodeValues.Approved,
            [CodeValues.PendingApproval],
            description: null,
            "Proje onaylandı.",
            cancellationToken);
    }

    public async Task<IResult> RejectAsync(int projectId, ProjectRejectDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _rejectValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return new ErrorResult(validation.Errors[0].ErrorMessage, StatusCodes.Status400BadRequest);

        return await TransitionAsync(
            projectId,
            CodeValues.Rejected,
            [CodeValues.PendingApproval],
            dto.Description.Trim(),
            "Proje reddedildi.",
            cancellationToken);
    }

    public async Task<IDataResult<List<OperationHistoryListItemDto>>> GetHistoryAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var visible = await _db.Projects
            .WhereVisibleTo(_currentUser)
            .AnyAsync(p => p.Id == projectId, cancellationToken);

        if (!visible)
            return new ErrorDataResult<List<OperationHistoryListItemDto>>("Proje bulunamadı.", StatusCodes.Status404NotFound);

        var list = await _db.OperationHistories.AsNoTracking()
            .Where(o => o.ProjectId == projectId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OperationHistoryListItemDto
            {
                Id = o.Id,
                OperationCode = o.OperationCode.CodeValue,
                OperationName = o.OperationCode.Name,
                Description = o.Description,
                RoleSnapshot = o.RoleSnapshot,
                UserId = o.UserId,
                UserFullName = o.User.FullName,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<List<OperationHistoryListItemDto>>(list);
    }

   
    /// Durum geçişi + günlük satırı. İkisi tek SaveChanges'te yazılır; ayrı kaydedilirse
    /// proje onaylanıp günlüğe geçmeyebilir ve denetim kaydı güvenilmez olur.
    private async Task<IResult> TransitionAsync(
        int projectId,
        string targetCodeValue,
        IReadOnlyCollection<string> allowedCurrentCodeValues,
        string? description,
        string successMessage,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
            return new ErrorResult("Oturum bilgisi okunamadı.", StatusCodes.Status401Unauthorized);

        var project = await _db.Projects
            .WhereVisibleTo(_currentUser)
            .Include(p => p.StatusCode)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return new ErrorResult("Proje bulunamadı.", StatusCodes.Status404NotFound);

        // Akıştan önce oluşturulmuş projelerde StatusCodeId null; taslak sayılır.
        var currentCodeValue = project.StatusCode?.CodeValue ?? CodeValues.Draft;

        if (!allowedCurrentCodeValues.Contains(currentCodeValue))
            return new ErrorResult(
                $"Bu işlem '{project.StatusCode?.Name ?? "Taslak"}' durumundaki bir projede yapılamaz.", StatusCodes.Status409Conflict);

        var targetCodeId = await GetStatusCodeIdAsync(targetCodeValue, cancellationToken);
        if (targetCodeId is null)
            return new ErrorResult($"'{targetCodeValue}' durum kodu tanımlı değil.", 500);

        project.StatusCodeId = targetCodeId;
        project.UpdatedByUserId = userId;

        _db.OperationHistories.Add(new OperationHistory
        {
            ProjectId = project.Id,
            UserId = userId,
            OperationCodeId = targetCodeId.Value,
            Description = description,
            // İşlem anındaki rolün donmuş kopyası; kullanıcının rolü sonradan değişse de günlük değişmez.
            RoleSnapshot = _currentUser.RoleCode
            // CreatedAt: StarCADContext.SaveChanges override'ında merkezî set edilir.
        });

        await _db.SaveChangesWithOutboxAsync(nameof(Project), "Index", () => project.Id, cancellationToken);
        return new SuccessResult(successMessage);
    }

    private async Task<int?> GetStatusCodeIdAsync(string codeValue, CancellationToken cancellationToken)
    {
        return await _db.Codes.AsNoTracking()
            .Where(c => c.Group.GroupName == CodeGroupNames.ProjectStatus && c.CodeValue == codeValue)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}