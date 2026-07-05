using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.ValidateProductConfiguration;

internal sealed class ValidateProductConfigurationCommandValidator : AbstractValidator<ValidateProductConfigurationCommand>
{
    public ValidateProductConfigurationCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.SelectedOptions)
            .NotNull().WithMessage("Selected options are required.")
            .NotEmpty().WithMessage("At least one selected option is required.");
    }
}
