using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaLogs;

public sealed record GetAaaLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? EventType = null,
    string? Username = null,
    Guid? NasId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginatedResult<AaaAuditLogDto>>>;
