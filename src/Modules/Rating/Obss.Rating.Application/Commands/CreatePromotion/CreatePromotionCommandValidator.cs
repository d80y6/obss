using FluentValidation;

namespace Obss.Rating.Application.Commands.CreatePromotion;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.PromotionType)
            .NotEmpty().WithMessage("Promotion type is required.")
            .Must(v => v is "Percentage" or "FixedAmount" or "FreePeriod" or "Bundle" or "Volume")
            .WithMessage("Promotion type must be Percentage, FixedAmount, FreePeriod, Bundle, or Volume.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("Valid from date is required.");

        RuleFor(x => x.ValidTo)
            .GreaterThan(x => x.ValidFrom).WithMessage("Valid to must be after valid from.")
            .When(x => x.ValidTo.HasValue);

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be non-negative.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .When(x => x.Code is not null);

        RuleFor(x => x.MaxRedemptions)
            .GreaterThan(0).WithMessage("Max redemptions must be greater than zero.")
            .When(x => x.MaxRedemptions.HasValue);

        RuleFor(x => x.Rules)
            .NotNull().WithMessage("Rules list is required.");

        RuleForEach(x => x.Rules).ChildRules(rule =>
        {
            rule.RuleFor(r => r.RuleType)
                .NotEmpty().WithMessage("Rule type is required.");

            rule.RuleFor(r => r.Operator)
                .NotEmpty().WithMessage("Operator is required.");

            rule.RuleFor(r => r.Value)
                .NotEmpty().WithMessage("Value is required.");

            rule.RuleFor(r => r.Logic)
                .NotEmpty().WithMessage("Logic is required.");
        });
    }
}
