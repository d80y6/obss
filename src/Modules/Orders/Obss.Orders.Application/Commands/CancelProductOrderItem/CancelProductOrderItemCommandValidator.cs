using FluentValidation;

namespace Obss.Orders.Application.Commands.CancelProductOrderItem;

public sealed class CancelProductOrderItemCommandValidator : AbstractValidator<CancelProductOrderItemCommand>
{
    public CancelProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
