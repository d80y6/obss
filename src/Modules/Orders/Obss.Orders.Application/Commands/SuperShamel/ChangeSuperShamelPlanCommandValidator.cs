using FluentValidation;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class ChangeSuperShamelPlanCommandValidator : AbstractValidator<ChangeSuperShamelPlanCommand>
{
    public ChangeSuperShamelPlanCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewFtthSpeedMbps)
            .Must(x => !x.HasValue || x is 50 or 100)
            .When(x => x.NewFtthSpeedMbps.HasValue);
        RuleFor(x => x.NewHatifTawasolPackage)
            .MaximumLength(100)
            .When(x => x.NewHatifTawasolPackage is not null);
        RuleFor(x => x.NewYemen4GDataPlan)
            .MaximumLength(100)
            .When(x => x.NewYemen4GDataPlan is not null);
        RuleFor(x => x)
            .Must(x => x.NewFtthSpeedMbps.HasValue || x.NewHatifTawasolPackage is not null || x.NewYemen4GDataPlan is not null)
            .WithMessage("At least one component change must be specified.");
    }
}
