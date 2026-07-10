using FluentValidation;

namespace Obss.Orders.Application.Commands.HoldProductOrderItem;

public sealed class HoldProductOrderItemCommandValidator : AbstractValidator<HoldProductOrderItemCommand>
{
    public HoldProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
