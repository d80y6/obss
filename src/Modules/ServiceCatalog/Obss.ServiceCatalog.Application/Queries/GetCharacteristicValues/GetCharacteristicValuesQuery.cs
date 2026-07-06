using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetCharacteristicValues;

public sealed record GetCharacteristicValuesQuery(
    Guid ServiceSpecificationId,
    Guid CharacteristicId
) : IRequest<List<ServiceSpecCharValueDto>>;
