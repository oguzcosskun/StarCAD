using FluentValidation;
using StarCAD.ViewModel.Codes;

namespace StarCAD.Business.Validators;

public sealed class CodeCreateValidator : AbstractValidator<CodeCreateDto>
{
    public CodeCreateValidator()
    {
        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Kod grubu seçilmelidir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kod adı zorunludur.")
            .MaximumLength(200).WithMessage("Kod adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.CodeValue)
            .NotEmpty().WithMessage("Kod değeri zorunludur.")
            .MaximumLength(50).WithMessage("Kod değeri en fazla 50 karakter olabilir.");
    }
}

public sealed class CodeUpdateValidator : AbstractValidator<CodeUpdateDto>
{
    public CodeUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kod adı zorunludur.")
            .MaximumLength(200).WithMessage("Kod adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.CodeValue)
            .NotEmpty().WithMessage("Kod değeri zorunludur.")
            .MaximumLength(50).WithMessage("Kod değeri en fazla 50 karakter olabilir.");
    }
}