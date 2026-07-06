using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceSpecifications;

public sealed record GetServiceSpecificationsQuery(
    string TenantId,
    string? Status = null,
    string? Brand = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<(List<ServiceSpecificationDto> Items, int TotalCount)>;
