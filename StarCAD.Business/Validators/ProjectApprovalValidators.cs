using FluentValidation;
using StarCAD.ViewModel.Projects;

namespace StarCAD.Business.Validators;

public sealed class ProjectRejectValidator : AbstractValidator<ProjectRejectDto>
{
    public ProjectRejectValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Red gerekçesi zorunludur.")
            .MaximumLength(500).WithMessage("Red gerekçesi en fazla 500 karakter olabilir.");
    }
}