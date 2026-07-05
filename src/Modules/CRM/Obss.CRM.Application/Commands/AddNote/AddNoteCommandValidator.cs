using FluentValidation;

namespace Obss.CRM.Application.Commands.AddNote;

internal sealed class AddNoteCommandValidator : AbstractValidator<AddNoteCommand>
{
    public AddNoteCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");

        RuleFor(x => x.CreatedById)
            .NotEmpty().WithMessage("Created by ID is required.")
            .MaximumLength(200).WithMessage("Created by ID must not exceed 200 characters.");
    }
}
