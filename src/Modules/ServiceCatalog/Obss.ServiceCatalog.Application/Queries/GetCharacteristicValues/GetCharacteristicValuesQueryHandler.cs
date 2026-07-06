using Mapster;
using MediatR;
using Obss.ServiceCatalog.Application.Abstractions;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetCharacteristicValues;

internal sealed class GetCharacteristicValuesQueryHandler(IServiceSpecificationRepository repository) : IRequestHandler<GetCharacteristicValuesQuery, List<ServiceSpecCharValueDto>>
{
    public async Task<List<ServiceSpecCharValueDto>> Handle(GetCharacteristicValuesQuery request, CancellationToken cancellationToken)
    {
        var spec = await repository.GetByIdWithCharacteristicsAsync(request.ServiceSpecificationId, cancellationToken);
        var characteristic = spec?.Characteristics.FirstOrDefault(c => c.Id == request.CharacteristicId);
        return characteristic?.Values.Adapt<List<ServiceSpecCharValueDto>>() ?? [];
    }
}
