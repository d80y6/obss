using FluentValidation;

namespace Obss.Orders.Application.Commands.RejectProductOrderItem;

public sealed class RejectProductOrderItemCommandValidator : AbstractValidator<RejectProductOrderItemCommand>
{
    public RejectProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
