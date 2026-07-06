using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCandidateById;

internal sealed class GetServiceCandidateByIdQueryHandler(IServiceCandidateRepository repository) : IRequestHandler<GetServiceCandidateByIdQuery, ServiceCandidateDto?>
{
    public async Task<ServiceCandidateDto?> Handle(GetServiceCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var candidate = await repository.GetByIdAsync(request.Id, cancellationToken);
        return candidate?.Adapt<ServiceCandidateDto>();
    }
}
