using FluentValidation;

namespace Obss.Orders.Application.Commands.CompleteProductOrderItem;

public sealed class CompleteProductOrderItemCommandValidator : AbstractValidator<CompleteProductOrderItemCommand>
{
    public CompleteProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
