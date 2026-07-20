using FluentValidation;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class ResumeVpsCommandValidator : AbstractValidator<ResumeVpsCommand>
{
    public ResumeVpsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
