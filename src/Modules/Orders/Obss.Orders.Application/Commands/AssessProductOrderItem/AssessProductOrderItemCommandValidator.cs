using FluentValidation;

namespace Obss.Orders.Application.Commands.AssessProductOrderItem;

public sealed class AssessProductOrderItemCommandValidator : AbstractValidator<AssessProductOrderItemCommand>
{
    public AssessProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
