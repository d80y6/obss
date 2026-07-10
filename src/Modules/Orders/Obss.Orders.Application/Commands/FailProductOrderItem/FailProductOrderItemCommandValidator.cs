using FluentValidation;

namespace Obss.Orders.Application.Commands.FailProductOrderItem;

public sealed class FailProductOrderItemCommandValidator : AbstractValidator<FailProductOrderItemCommand>
{
    public FailProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Error).NotEmpty().MaximumLength(500);
    }
}
