using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;

public sealed class CreateServiceCategoryCommandValidator : AbstractValidator<CreateServiceCategoryCommand>
{
    public CreateServiceCategoryCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
