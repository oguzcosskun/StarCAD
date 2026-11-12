using FluentValidation;
using StarCAD.ViewModel.Auth;

namespace StarCAD.Business.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(100).WithMessage("Kullanıcı adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir.");
    }
}