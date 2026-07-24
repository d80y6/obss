using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaLogs;

public sealed class GetAaaLogsQueryHandler : IRequestHandler<GetAaaLogsQuery, Result<PaginatedResult<AaaAuditLogDto>>>
{
    private readonly IAaaAuditLogRepository _logRepository;

    public GetAaaLogsQueryHandler(IAaaAuditLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<PaginatedResult<AaaAuditLogDto>>> Handle(GetAaaLogsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _logRepository.GetPaginatedAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.EventType,
            request.Username,
            request.NasId,
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        return Result.Success(new PaginatedResult<AaaAuditLogDto>(
            items.Adapt<IReadOnlyList<AaaAuditLogDto>>(),
            totalCount));
    }
}
