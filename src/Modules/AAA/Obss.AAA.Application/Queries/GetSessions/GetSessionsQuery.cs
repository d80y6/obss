using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetSessions;

public sealed record GetSessionsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? NasId = null,
    string? Username = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginatedResult<RadiusSessionDto>>>;
