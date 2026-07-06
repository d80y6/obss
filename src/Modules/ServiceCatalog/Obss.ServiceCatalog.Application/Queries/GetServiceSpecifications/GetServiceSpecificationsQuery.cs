using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceSpecifications;

public sealed record GetServiceSpecificationsQuery(
    string TenantId,
    string? Status = null,
    string? Brand = null,
    int Offset = 0,
    int Limit = 20
) : IRequest<(List<ServiceSpecificationDto> Items, int TotalCount)>;
