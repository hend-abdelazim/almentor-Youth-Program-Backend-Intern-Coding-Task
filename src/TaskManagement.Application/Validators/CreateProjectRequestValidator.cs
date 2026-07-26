using FluentValidation;
using TaskManagement.Application.DTOs.Projects;

namespace TaskManagement.Application.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequestDto>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .Length(1, 200).WithMessage("Project name must be between 1 and 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Project description cannot exceed 2000 characters.");
    }
}
