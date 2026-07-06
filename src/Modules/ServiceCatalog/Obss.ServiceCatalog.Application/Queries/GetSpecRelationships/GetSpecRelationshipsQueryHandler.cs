using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetSpecRelationships;

internal sealed class GetSpecRelationshipsQueryHandler(IServiceSpecificationRepository repository) : IRequestHandler<GetSpecRelationshipsQuery, List<ServiceSpecRelationshipDto>>
{
    public async Task<List<ServiceSpecRelationshipDto>> Handle(GetSpecRelationshipsQuery request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdAsync(request.ServiceSpecificationId, cancellationToken);
        return spec?.Relationships.Adapt<List<ServiceSpecRelationshipDto>>() ?? [];
    }
}
