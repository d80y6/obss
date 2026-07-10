using FluentValidation;

namespace Obss.Orders.Application.Commands.AcknowledgeProductOrderItem;

public sealed class AcknowledgeProductOrderItemCommandValidator : AbstractValidator<AcknowledgeProductOrderItemCommand>
{
    public AcknowledgeProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
