using FluentValidation;

namespace Obss.Orders.Application.Commands.ResumeProductOrderItem;

public sealed class ResumeProductOrderItemCommandValidator : AbstractValidator<ResumeProductOrderItemCommand>
{
    public ResumeProductOrderItemCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}
