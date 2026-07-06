using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetCharacteristics;

public sealed record GetCharacteristicsQuery(Guid ServiceSpecificationId) : IRequest<List<ServiceSpecCharacteristicDto>>;
