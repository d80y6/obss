using FluentValidation;

namespace Obss.Orders.Application.Commands.Colocation;

public sealed class ResumeColocationCommandValidator : AbstractValidator<ResumeColocationCommand>
{
    public ResumeColocationCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
