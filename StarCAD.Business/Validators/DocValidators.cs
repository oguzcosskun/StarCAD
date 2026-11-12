using FluentValidation;
using StarCAD.ViewModel.Docs;

namespace StarCAD.Business.Validators;

internal static class DocUploadRules
{
    // Dış girdiye güvenilmez: yalnızca PDF kabul edilir.
    public static readonly string[] AllowedContentTypes = ["application/pdf"];

    public const long MaxSizeBytes = 20 * 1024 * 1024; // 20 MB
}

public sealed class DocUploadValidator : AbstractValidator<DocUploadDto>
{
    public DocUploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Dosya adı zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Dosya türü belirlenemedi.")
            .Must(ct => DocUploadRules.AllowedContentTypes.Contains(ct))
            .WithMessage("Desteklenmeyen dosya türü. Yalnızca PDF yüklenebilir.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0).WithMessage("Dosya boş olamaz.")
            .LessThanOrEqualTo(DocUploadRules.MaxSizeBytes).WithMessage("Dosya boyutu 20 MB'ı aşamaz.");

        RuleFor(x => x.ProjectUnitId)
            .Null().WithMessage("Bağımsız bölüm yalnızca bir projeyle birlikte belirtilebilir.")
            .When(x => x.ProjectId is null);
    }
}

public sealed class DocAssignValidator : AbstractValidator<DocAssignDto>
{
    // DocAssignDto artık yalnızca ProjectId taşıyor (nullable int) — şekil kuralı gerektiren
    // bir alan yok; birim bağlama LinkUnitAsync/UnlinkUnitAsync'e taşındı.
}
