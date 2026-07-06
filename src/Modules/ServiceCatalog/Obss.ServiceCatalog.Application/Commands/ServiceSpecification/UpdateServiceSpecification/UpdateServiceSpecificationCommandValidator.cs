using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateServiceSpecification;

public sealed class UpdateServiceSpecificationCommandValidator : AbstractValidator<UpdateServiceSpecificationCommand>
{
    public UpdateServiceSpecificationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.Version).MaximumLength(100);
    }
}
