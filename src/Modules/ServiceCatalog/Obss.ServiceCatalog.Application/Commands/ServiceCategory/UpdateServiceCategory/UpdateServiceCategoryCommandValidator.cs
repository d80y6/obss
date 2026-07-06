using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;

public sealed class UpdateServiceCategoryCommandValidator : AbstractValidator<UpdateServiceCategoryCommand>
{
    public UpdateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
