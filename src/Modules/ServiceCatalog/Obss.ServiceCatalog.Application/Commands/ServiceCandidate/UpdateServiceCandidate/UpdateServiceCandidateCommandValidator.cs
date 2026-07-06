using FluentValidation;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.UpdateServiceCandidate;

public sealed class UpdateServiceCandidateCommandValidator : AbstractValidator<UpdateServiceCandidateCommand>
{
    public UpdateServiceCandidateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FeatureSpecification).MaximumLength(4000);
    }
}
