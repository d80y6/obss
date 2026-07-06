using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceSpecificationById;

internal sealed class GetServiceSpecificationByIdQueryHandler(IServiceSpecificationRepository repository) : IRequestHandler<GetServiceSpecificationByIdQuery, ServiceSpecificationDto?>
{
    public async Task<ServiceSpecificationDto?> Handle(GetServiceSpecificationByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.Id, cancellationToken);
        return spec?.Adapt<ServiceSpecificationDto>();
    }
}
