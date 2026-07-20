using FluentValidation;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed class ChangeWebHostingPlanCommandValidator : AbstractValidator<ChangeWebHostingPlanCommand>
{
    public ChangeWebHostingPlanCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewHostingPlan).NotEmpty().Must(p => p is "shared" or "business" or "enterprise");
    }
}
