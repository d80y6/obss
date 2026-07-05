using FluentValidation;

namespace Obss.Rating.Application.Commands.DeactivatePromotion;

internal sealed class DeactivatePromotionCommandValidator : AbstractValidator<DeactivatePromotionCommand>
{
    public DeactivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId)
            .NotEmpty().WithMessage("Promotion ID is required.");
    }
}
