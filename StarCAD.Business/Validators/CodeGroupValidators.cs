using FluentValidation;
using StarCAD.ViewModel.CodeGroups;

namespace StarCAD.Business.Validators;

public sealed class CodeGroupCreateValidator : AbstractValidator<CodeGroupCreateDto>
{
    public CodeGroupCreateValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Kod grubu adı zorunludur.")
            .MaximumLength(100).WithMessage("Kod grubu adı en fazla 100 karakter olabilir.");
    }
}

public sealed class CodeGroupUpdateValidator : AbstractValidator<CodeGroupUpdateDto>
{
    public CodeGroupUpdateValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Kod grubu adı zorunludur.")
            .MaximumLength(100).WithMessage("Kod grubu adı en fazla 100 karakter olabilir.");
    }
}