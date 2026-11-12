using System.Linq;
using FluentValidation;
using StarCAD.ViewModel.ProjectFirms;

namespace StarCAD.Business.Validators;

/// <summary>Resmi TC kimlik numarası doğrulaması (11 hane + checksum).</summary>
internal static class TcKimlik
{
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 11 || !value.All(char.IsDigit))
            return false;

        var digits = value.Select(c => c - '0').ToArray();
        if (digits[0] == 0) return false;

        int oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];   // 1., 3., 5., 7., 9. haneler
        int evenSum = digits[1] + digits[3] + digits[5] + digits[7];              // 2., 4., 6., 8. haneler

        int digit10 = ((oddSum * 7) - evenSum) % 10;
        if (digit10 < 0) digit10 += 10;
        if (digit10 != digits[9]) return false;

        int sumFirst10 = 0;
        for (int i = 0; i < 10; i++) sumFirst10 += digits[i];
        return sumFirst10 % 10 == digits[10];
    }
}

public sealed class ProjectFirmCreateValidator : AbstractValidator<ProjectFirmCreateDto>
{
    public ProjectFirmCreateValidator()
    {
        RuleFor(x => x.CompanyType)
            .Must(t => t is 1 or 2)
            .WithMessage("Firma tipi 1 (şahıs) veya 2 (tüzel) olmalıdır.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Firma ünvanı zorunludur.")
            .MaximumLength(300);

        // Tüzel (2): vergi no zorunlu, TC boş olmalı.
        When(x => x.CompanyType == 2, () =>
        {
            RuleFor(x => x.TaxNumber)
                .NotEmpty().WithMessage("Tüzel firma için vergi numarası zorunludur.")
                .Matches(@"^\d{10,11}$").WithMessage("Vergi numarası 10-11 haneli olmalıdır.");

            RuleFor(x => x.NationalIdNumber)
                .Empty().WithMessage("Tüzel firmada TC kimlik numarası girilemez.");
        });

        // Şahıs (1): TC zorunlu + checksum, vergi no boş olmalı.
        When(x => x.CompanyType == 1, () =>
        {
            RuleFor(x => x.NationalIdNumber)
                .NotEmpty().WithMessage("Şahıs firma için TC kimlik numarası zorunludur.")
                .Must(TcKimlik.IsValid).WithMessage("Geçerli bir TC kimlik numarası giriniz.");

            RuleFor(x => x.TaxNumber)
                .Empty().WithMessage("Şahıs firmada vergi numarası girilemez.");
        });

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir e-posta giriniz.")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.AccountingCode).MaximumLength(50);
        RuleFor(x => x.ContactPerson).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Phone2).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}

public sealed class ProjectFirmUpdateValidator : AbstractValidator<ProjectFirmUpdateDto>
{
    public ProjectFirmUpdateValidator()
    {
        RuleFor(x => x.CompanyType)
            .Must(t => t is 1 or 2)
            .WithMessage("Firma tipi 1 (şahıs) veya 2 (tüzel) olmalıdır.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Firma ünvanı zorunludur.")
            .MaximumLength(300);

        When(x => x.CompanyType == 2, () =>
        {
            RuleFor(x => x.TaxNumber)
                .NotEmpty().WithMessage("Tüzel firma için vergi numarası zorunludur.")
                .Matches(@"^\d{10,11}$").WithMessage("Vergi numarası 10-11 haneli olmalıdır.");

            RuleFor(x => x.NationalIdNumber)
                .Empty().WithMessage("Tüzel firmada TC kimlik numarası girilemez.");
        });

        When(x => x.CompanyType == 1, () =>
        {
            RuleFor(x => x.NationalIdNumber)
                .NotEmpty().WithMessage("Şahıs firma için TC kimlik numarası zorunludur.")
                .Must(TcKimlik.IsValid).WithMessage("Geçerli bir TC kimlik numarası giriniz.");

            RuleFor(x => x.TaxNumber)
                .Empty().WithMessage("Şahıs firmada vergi numarası girilemez.");
        });

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Geçerli bir e-posta giriniz.")
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.AccountingCode).MaximumLength(50);
        RuleFor(x => x.ContactPerson).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Phone2).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}
