using FluentValidation;

namespace Obss.Rating.Application.Commands.ApplyPromotion;

internal sealed class ApplyPromotionCommandValidator : AbstractValidator<ApplyPromotionCommand>
{
    public ApplyPromotionCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PromotionCode) || x.PromotionId.HasValue)
            .WithMessage("Either promotion code or promotion ID must be provided.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
