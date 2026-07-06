using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.CreateServiceSpecification;

public sealed class CreateServiceSpecificationCommandValidator : AbstractValidator<CreateServiceSpecificationCommand>
{
    public CreateServiceSpecificationCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.Version).MaximumLength(100);
    }
}
