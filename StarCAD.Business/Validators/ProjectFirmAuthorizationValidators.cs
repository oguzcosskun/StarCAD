using FluentValidation;
using StarCAD.ViewModel.ProjectFirmAuthorizations;

namespace StarCAD.Business.Validators;

public class ProjectFirmAuthorizationCreateValidator : AbstractValidator<ProjectFirmAuthorizationCreateDto>
{
    public ProjectFirmAuthorizationCreateValidator()
    {
        RuleFor(x => x.ProjectFirmId).GreaterThan(0).WithMessage("Proje firması zorunludur.");
        RuleFor(x => x.GasDistributionFirmId).GreaterThan(0).WithMessage("Gaz dağıtım firması zorunludur.");
        RuleFor(x => x.CertificateNumber)
            .NotEmpty().WithMessage("Sertifika numarası zorunludur.")
            .MaximumLength(50).WithMessage("Sertifika numarası en fazla 50 karakter olabilir.");
        RuleFor(x => x.ValidFrom)
            .NotEqual(default(DateTime)).WithMessage("Geçerlilik başlangıç tarihi zorunludur.");
        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .When(x => x.ValidTo.HasValue)
            .WithMessage("Geçerlilik bitiş tarihi başlangıçtan sonra olmalıdır.");
    }
}

public class ProjectFirmAuthorizationUpdateValidator : AbstractValidator<ProjectFirmAuthorizationUpdateDto>
{
    public ProjectFirmAuthorizationUpdateValidator()
    {
        RuleFor(x => x.CertificateNumber)
            .NotEmpty().WithMessage("Sertifika numarası zorunludur.")
            .MaximumLength(50).WithMessage("Sertifika numarası en fazla 50 karakter olabilir.");
        RuleFor(x => x.ValidFrom)
            .NotEqual(default(DateTime)).WithMessage("Geçerlilik başlangıç tarihi zorunludur.");
        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom)
            .When(x => x.ValidTo.HasValue)
            .WithMessage("Geçerlilik bitiş tarihi başlangıçtan sonra olmalıdır.");
    }
}