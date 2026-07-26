using FluentValidation;
using TaskManagement.Application.DTOs.Tasks;

namespace TaskManagement.Application.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequestDto>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .Length(1, 500).WithMessage("Task title must be between 1 and 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Task description cannot exceed 5000 characters.");

        RuleFor(x => x.DueDate)
            .Must(BeTodayOrFuture)
            .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");
    }

    private bool BeTodayOrFuture(DateTime? dueDate)
    {
        if (!dueDate.HasValue)
            return true;

        return dueDate.Value.Date >= DateTime.UtcNow.Date;
    }
}
