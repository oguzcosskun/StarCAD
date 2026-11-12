using FluentValidation;
using StarCAD.ViewModel.GasDistributionFirms;

namespace StarCAD.Business.Validators;

public sealed class GasDistributionFirmCreateValidator : AbstractValidator<GasDistributionFirmCreateDto>
{
    public GasDistributionFirmCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Firma ünvanı zorunludur.")
            .MaximumLength(300).WithMessage("Firma ünvanı en fazla 300 karakter olabilir.");

        RuleFor(x => x.CompanyNumber)
            .GreaterThan(0).WithMessage("Firma numarası sıfırdan büyük olmalıdır.");

        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Geçerli bir grup firması seçiniz.")
            .When(x => x.GroupId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150).WithMessage("Yetkili kişi en fazla 150 karakter olabilir.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");
    }
}

public sealed class GasDistributionFirmUpdateValidator : AbstractValidator<GasDistributionFirmUpdateDto>
{
    public GasDistributionFirmUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Firma ünvanı zorunludur.")
            .MaximumLength(300).WithMessage("Firma ünvanı en fazla 300 karakter olabilir.");

        RuleFor(x => x.CompanyNumber)
            .GreaterThan(0).WithMessage("Firma numarası sıfırdan büyük olmalıdır.");

        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithMessage("Geçerli bir grup firması seçiniz.")
            .When(x => x.GroupId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(150).WithMessage("Yetkili kişi en fazla 150 karakter olabilir.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");
    }
}