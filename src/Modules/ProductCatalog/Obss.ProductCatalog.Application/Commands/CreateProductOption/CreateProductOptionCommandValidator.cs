using FluentValidation;

namespace Obss.ProductCatalog.Application.Commands.CreateProductOption;

public sealed class CreateProductOptionCommandValidator : AbstractValidator<CreateProductOptionCommand>
{
    public CreateProductOptionCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Option name is required.")
            .MaximumLength(200).WithMessage("Option name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.OptionType)
            .IsInEnum().WithMessage("Invalid option type.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be non-negative.");
    }
}
