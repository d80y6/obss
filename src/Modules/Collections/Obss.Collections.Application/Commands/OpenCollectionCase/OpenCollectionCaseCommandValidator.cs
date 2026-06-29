using FluentValidation;

namespace Obss.Collections.Application.Commands.OpenCollectionCase;

public sealed class OpenCollectionCaseCommandValidator : AbstractValidator<OpenCollectionCaseCommand>
{
    public OpenCollectionCaseCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");

        RuleFor(x => x.TotalOverdueAmount)
            .GreaterThan(0).WithMessage("Overdue amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.");
    }
}
