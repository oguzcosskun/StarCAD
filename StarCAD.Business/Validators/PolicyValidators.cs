using FluentValidation;
using StarCAD.ViewModel.Policies;

namespace StarCAD.Business.Validators;

public sealed class PolicyAddValidator : AbstractValidator<PolicyAddDto>
{
    public PolicyAddValidator()
    {
        RuleFor(x => x.ProjectUnitId)
            .GreaterThan(0).WithMessage("Bağımsız bölüm (ProjectUnitId) zorunludur.");

        RuleFor(x => x.PolicyNumber)
            .MaximumLength(50).WithMessage("Poliçe numarası en fazla 50 karakter olabilir.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Tutar negatif olamaz.")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value).WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public sealed class PolicyUpdateValidator : AbstractValidator<PolicyUpdateDto>
{
    public PolicyUpdateValidator()
    {
        RuleFor(x => x.PolicyNumber)
            .MaximumLength(50).WithMessage("Poliçe numarası en fazla 50 karakter olabilir.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Tutar negatif olamaz.")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value).WithMessage("Bitiş tarihi başlangıç tarihinden önce olamaz.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
