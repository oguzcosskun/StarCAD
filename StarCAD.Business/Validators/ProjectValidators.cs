using FluentValidation;
using StarCAD.ViewModel.Projects;

namespace StarCAD.Business.Validators;

public sealed class ProjectCreateValidator : AbstractValidator<ProjectCreateDto>
{
    public ProjectCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Proje adı zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.ProjectFirmAuthorizationId)
            .GreaterThan(0).WithMessage("Proje firması yetkisi (ProjectFirmAuthorizationId) zorunludur.");

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Geçerli bir il seçiniz.")
            .When(x => x.CityId.HasValue);

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("Geçerli bir ilçe seçiniz.")
            .When(x => x.DistrictId.HasValue);

        RuleFor(x => x.AddressLine)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.BlockLotParcel)
            .MaximumLength(50).WithMessage("Ada/Pafta/Parsel en fazla 50 karakter olabilir.");

        RuleFor(x => x.ProjectTypeCodeId)
            .GreaterThan(0).WithMessage("Proje tipi zorunludur.");

        RuleFor(x => x.HeatingTypeCodeId)
            .GreaterThan(0).WithMessage("Isınma tipi zorunludur.");

        RuleFor(x => x.BuildingUsageTypeCodeId)
            .GreaterThan(0).WithMessage("Bina kullanımı tipi zorunludur.");

        RuleFor(x => x.ApartmentCount)
            .GreaterThanOrEqualTo(0).WithMessage("Daire sayısı negatif olamaz.")
            .When(x => x.ApartmentCount.HasValue);

        RuleFor(x => x.WorkplaceCount)
            .GreaterThanOrEqualTo(0).WithMessage("İşyeri sayısı negatif olamaz.")
            .When(x => x.WorkplaceCount.HasValue);

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThanOrEqualTo(0).WithMessage("Alan negatif olamaz.")
            .When(x => x.AreaSquareMeters.HasValue);

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(0).WithMessage("Kapasite negatif olamaz.")
            .When(x => x.Capacity.HasValue);

        RuleFor(x => x.ServiceBoxPressureMbar)
            .GreaterThanOrEqualTo(0).WithMessage("S.K. basıncı negatif olamaz.")
            .When(x => x.ServiceBoxPressureMbar.HasValue);

    }
}

public sealed class ProjectUpdateValidator : AbstractValidator<ProjectUpdateDto>
{
    public ProjectUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Proje adı zorunludur.")
            .MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("Geçerli bir il seçiniz.")
            .When(x => x.CityId.HasValue);

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("Geçerli bir ilçe seçiniz.")
            .When(x => x.DistrictId.HasValue);

        RuleFor(x => x.AddressLine)
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.BlockLotParcel)
            .MaximumLength(50).WithMessage("Ada/Pafta/Parsel en fazla 50 karakter olabilir.");

        RuleFor(x => x.ProjectTypeCodeId)
            .GreaterThan(0).WithMessage("Proje tipi zorunludur.");

        RuleFor(x => x.HeatingTypeCodeId)
            .GreaterThan(0).WithMessage("Isınma tipi zorunludur.");

        RuleFor(x => x.BuildingUsageTypeCodeId)
            .GreaterThan(0).WithMessage("Bina kullanımı tipi zorunludur.");

        RuleFor(x => x.ApartmentCount)
            .GreaterThanOrEqualTo(0).WithMessage("Daire sayısı negatif olamaz.")
            .When(x => x.ApartmentCount.HasValue);

        RuleFor(x => x.WorkplaceCount)
            .GreaterThanOrEqualTo(0).WithMessage("İşyeri sayısı negatif olamaz.")
            .When(x => x.WorkplaceCount.HasValue);

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThanOrEqualTo(0).WithMessage("Alan negatif olamaz.")
            .When(x => x.AreaSquareMeters.HasValue);

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(0).WithMessage("Kapasite negatif olamaz.")
            .When(x => x.Capacity.HasValue);

        RuleFor(x => x.ServiceBoxPressureMbar)
            .GreaterThanOrEqualTo(0).WithMessage("S.K. basıncı negatif olamaz.")
            .When(x => x.ServiceBoxPressureMbar.HasValue);

    }
}