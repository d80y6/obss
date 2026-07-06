using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Entities;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;

internal sealed class CreateServiceCandidateCommandHandler(
    IServiceCandidateRepository candidateRepository,
    IServiceCategoryRepository categoryRepository) : IRequestHandler<CreateServiceCandidateCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = Domain.Entities.ServiceCandidate.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ServiceSpecificationId,
            request.BaseCandidateId,
            request.FeatureSpecification,
            request.ValidFrom,
            request.ValidTo);

        if (request.CategoryIds?.Count > 0)
        {
            foreach (var catId in request.CategoryIds)
            {
                var category = await categoryRepository.GetByIdAsync(catId, cancellationToken);
                if (category != null)
                    category.AddCandidate(candidate);
            }
        }

        await candidateRepository.AddAsync(candidate, cancellationToken);
        return candidate.Id;
    }
}
