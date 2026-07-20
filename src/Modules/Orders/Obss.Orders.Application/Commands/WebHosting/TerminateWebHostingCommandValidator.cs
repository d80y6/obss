using FluentValidation;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed class TerminateWebHostingCommandValidator : AbstractValidator<TerminateWebHostingCommand>
{
    public TerminateWebHostingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
