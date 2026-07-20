using FluentValidation;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed class ResumeWebHostingCommandValidator : AbstractValidator<ResumeWebHostingCommand>
{
    public ResumeWebHostingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
