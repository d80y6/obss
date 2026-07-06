using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.UpdateServiceCandidate;

internal sealed class UpdateServiceCandidateCommandHandler(IServiceCandidateRepository repository) : IRequestHandler<UpdateServiceCandidateCommand>
{
    public async Task Handle(UpdateServiceCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service candidate {request.Id} not found");

        candidate.UpdateDetails(request.Name, request.Description, request.FeatureSpecification);
        await repository.UpdateAsync(candidate, cancellationToken);
    }
}
