using FluentValidation;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class SuspendSuperShamelCommandValidator : AbstractValidator<SuspendSuperShamelCommand>
{
    public SuspendSuperShamelCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
