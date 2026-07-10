using FluentValidation;

namespace Obss.Orders.Application.Commands.StartProductOrderItem;

public sealed class StartProductOrderItemCommandValidator : AbstractValidator<StartProductOrderItemCommand>
{
    public StartProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
