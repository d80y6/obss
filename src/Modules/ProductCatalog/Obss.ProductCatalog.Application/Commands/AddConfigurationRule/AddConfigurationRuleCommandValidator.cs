using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.AddConfigurationRule;

public sealed class AddConfigurationRuleCommandValidator : AbstractValidator<AddConfigurationRuleCommand>
{
    public AddConfigurationRuleCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.RuleType)
            .IsInEnum().WithMessage("Invalid rule type.");

        RuleFor(x => x.Condition)
            .MaximumLength(4000).WithMessage("Condition must not exceed 4000 characters.")
            .When(x => x.Condition is not null);

        RuleFor(x => x.TargetOption)
            .MaximumLength(200).WithMessage("Target option must not exceed 200 characters.")
            .When(x => x.TargetOption is not null);
    }
}
