using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCandidates;

public sealed record GetServiceCandidatesQuery(
    string TenantId,
    Guid? CategoryId = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<(List<ServiceCandidateDto> Items, int TotalCount)>;
