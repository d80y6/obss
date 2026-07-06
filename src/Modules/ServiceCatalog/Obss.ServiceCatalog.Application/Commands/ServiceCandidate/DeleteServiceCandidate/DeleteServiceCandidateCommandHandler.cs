using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Domain.Exceptions;

namespace Obss.ServiceCatalog.Application.Commands.ServiceCandidate.DeleteServiceCandidate;

internal sealed class DeleteServiceCandidateCommandHandler(IServiceCandidateRepository repository) : IRequestHandler<DeleteServiceCandidateCommand>
{
    public async Task Handle(DeleteServiceCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ServiceCatalogDomainException($"Service candidate {request.Id} not found");

        await repository.DeleteAsync(candidate, cancellationToken);
    }
}
