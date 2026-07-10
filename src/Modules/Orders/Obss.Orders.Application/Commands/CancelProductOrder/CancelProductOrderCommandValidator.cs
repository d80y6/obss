using FluentValidation;

namespace Obss.Orders.Application.Commands.CancelProductOrder;

public sealed class CancelProductOrderCommandValidator : AbstractValidator<CancelProductOrderCommand>
{
    public CancelProductOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
