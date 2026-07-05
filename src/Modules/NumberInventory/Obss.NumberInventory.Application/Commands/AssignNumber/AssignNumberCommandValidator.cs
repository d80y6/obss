using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.AssignNumber;

internal sealed class AssignNumberCommandValidator : AbstractValidator<AssignNumberCommand>
{
    public AssignNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");
    }
}
