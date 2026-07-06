using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

public sealed class CreateServiceCandidateCommandValidator : AbstractValidator<CreateServiceCandidateCommand>
{
    public CreateServiceCandidateCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FeatureSpecification).MaximumLength(4000);
    }
}
