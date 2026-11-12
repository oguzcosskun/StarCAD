using FluentValidation;
using StarCAD.ViewModel.Auth;
using StarCAD.Common.Constants;
namespace StarCAD.Business.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(200).WithMessage("Ad soyad en fazla 200 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Kullanıcı adı en fazla 100 karakter olabilir.")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Kullanıcı adı yalnızca harf, rakam, nokta, alt çizgi ve tire içerebilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
            .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir.")
            .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon zorunludur.")
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.")
            .Matches(@"^[0-9\s()+-]+$").WithMessage("Telefon yalnızca rakam ve +()- karakterleri içerebilir.");

        RuleFor(x => x.RoleCode)
            .NotEmpty().WithMessage("Rol seçimi zorunludur.")
            .Must(code => RoleCodes.All.Contains(code))
            .WithMessage($"Geçersiz rol kodu. Geçerli değerler: {string.Join(", ", RoleCodes.All)}");
        
        RuleFor(x => x)
            .Must(x => x.ProjectFirmId is null && x.GasDistributionFirmId is null)
            .When(x => x.RoleCode == RoleCodes.Admin)
            .WithMessage("Yönetici rolü bir firmaya bağlanamaz.")
            .OverridePropertyName(nameof(RegisterRequest.RoleCode));

        RuleFor(x => x)
            .Must(x => x.ProjectFirmId is not null && x.GasDistributionFirmId is null)
            .When(x => x.RoleCode == RoleCodes.ProjectFirmUser)
            .WithMessage("Proje firması kullanıcısı yalnızca bir proje firmasına bağlanmalıdır.")
            .OverridePropertyName(nameof(RegisterRequest.ProjectFirmId));

        RuleFor(x => x)
            .Must(x => x.GasDistributionFirmId is not null && x.ProjectFirmId is null)
            .When(x => x.RoleCode == RoleCodes.GasDistributionUser)
            .WithMessage("Dağıtım firması kullanıcısı yalnızca bir dağıtım firmasına bağlanmalıdır.")
            .OverridePropertyName(nameof(RegisterRequest.GasDistributionFirmId));
    }
}