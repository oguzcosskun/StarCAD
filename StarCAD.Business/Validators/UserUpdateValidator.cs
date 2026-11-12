using FluentValidation;
using StarCAD.Common.Constants;
using StarCAD.ViewModel.Users;

namespace StarCAD.Business.Validators;

public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(200).WithMessage("Ad soyad en fazla 200 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon zorunludur.")
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.")
            .Matches(@"^[0-9\s()+-]+$").WithMessage("Telefon yalnızca rakam ve +()- karakterleri içerebilir.");

        RuleFor(x => x.RoleCode)
            .Must(code => RoleCodes.All.Contains(code!))
            .When(x => x.RoleCode is not null)
            .WithMessage($"Geçersiz rol kodu. Geçerli değerler: {string.Join(", ", RoleCodes.All)}");

        RuleFor(x => x)
            .Must(x => x.ProjectFirmId is null && x.GasDistributionFirmId is null)
            .When(x => x.RoleCode == RoleCodes.Admin)
            .WithMessage("Yönetici rolü bir firmaya bağlanamaz.")
            .OverridePropertyName(nameof(UserUpdateDto.RoleCode));

        RuleFor(x => x)
            .Must(x => x.ProjectFirmId is not null && x.GasDistributionFirmId is null)
            .When(x => x.RoleCode == RoleCodes.ProjectFirmUser)
            .WithMessage("Proje firması kullanıcısı yalnızca bir proje firmasına bağlanmalıdır.")
            .OverridePropertyName(nameof(UserUpdateDto.ProjectFirmId));

        RuleFor(x => x)
            .Must(x => x.GasDistributionFirmId is not null && x.ProjectFirmId is null)
            .When(x => x.RoleCode == RoleCodes.GasDistributionUser)
            .WithMessage("Dağıtım firması kullanıcısı yalnızca bir dağıtım firmasına bağlanmalıdır.")
            .OverridePropertyName(nameof(UserUpdateDto.GasDistributionFirmId));
    }
}