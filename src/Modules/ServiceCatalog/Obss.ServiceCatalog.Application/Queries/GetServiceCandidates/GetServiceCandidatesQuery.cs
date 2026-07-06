using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCandidates;

public sealed record GetServiceCandidatesQuery(
    string TenantId,
    Guid? CategoryId = null,
    string? Status = null,
    int Offset = 0,
    int Limit = 20
) : IRequest<(List<ServiceCandidateDto> Items, int TotalCount)>;
