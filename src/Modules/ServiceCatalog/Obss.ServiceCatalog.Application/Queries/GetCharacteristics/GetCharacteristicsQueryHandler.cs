using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetCharacteristics;

internal sealed class GetCharacteristicsQueryHandler(IServiceSpecificationRepository repository) : IRequestHandler<GetCharacteristicsQuery, List<ServiceSpecCharacteristicDto>>
{
    public async Task<List<ServiceSpecCharacteristicDto>> Handle(GetCharacteristicsQuery request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken);
        return spec?.Characteristics.Adapt<List<ServiceSpecCharacteristicDto>>() ?? [];
    }
}
