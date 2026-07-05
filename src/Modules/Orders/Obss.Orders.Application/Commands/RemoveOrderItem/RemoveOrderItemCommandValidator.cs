using FluentValidation;

namespace Obss.Orders.Application.Commands.RemoveOrderItem;

internal sealed class RemoveOrderItemCommandValidator : AbstractValidator<RemoveOrderItemCommand>
{
    public RemoveOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");
    }
}
