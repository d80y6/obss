using FluentValidation;

namespace Obss.Orders.Application.Commands.SubmitProductOrder;

public sealed class SubmitProductOrderCommandValidator : AbstractValidator<SubmitProductOrderCommand>
{
    public SubmitProductOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
