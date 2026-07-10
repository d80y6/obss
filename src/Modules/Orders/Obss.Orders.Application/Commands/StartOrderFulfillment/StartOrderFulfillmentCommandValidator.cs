using FluentValidation;

namespace Obss.Orders.Application.Commands.StartOrderFulfillment;

public sealed class StartOrderFulfillmentCommandValidator : AbstractValidator<StartOrderFulfillmentCommand>
{
    public StartOrderFulfillmentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
