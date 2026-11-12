using FluentValidation;
using StarCAD.ViewModel.GasDistributionGroups;

namespace StarCAD.Business.Validators;

public sealed class GasDistributionGroupCreateValidator : AbstractValidator<GasDistributionGroupCreateDto>
{
    public GasDistributionGroupCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Grup firması adı zorunludur.")
            .MaximumLength(200).WithMessage("Grup firması adı en fazla 200 karakter olabilir.");
    }
}

public sealed class GasDistributionGroupUpdateValidator : AbstractValidator<GasDistributionGroupUpdateDto>
{
    public GasDistributionGroupUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Grup firması adı zorunludur.")
            .MaximumLength(200).WithMessage("Grup firması adı en fazla 200 karakter olabilir.");
    }
}