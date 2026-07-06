using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCandidateById;

public sealed record GetServiceCandidateByIdQuery(Guid Id) : IRequest<ServiceCandidateDto?>;
