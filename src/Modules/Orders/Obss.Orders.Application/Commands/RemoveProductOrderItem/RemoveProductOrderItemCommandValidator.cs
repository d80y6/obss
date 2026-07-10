using FluentValidation;

namespace Obss.Orders.Application.Commands.RemoveProductOrderItem;

public sealed class RemoveProductOrderItemCommandValidator : AbstractValidator<RemoveProductOrderItemCommand>
{
    public RemoveProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}
