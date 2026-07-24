using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetSessions;

public sealed class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<PaginatedResult<RadiusSessionDto>>>
{
    private readonly IRadiusSessionRepository _sessionRepository;

    public GetSessionsQueryHandler(IRadiusSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<PaginatedResult<RadiusSessionDto>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var allSessions = await _sessionRepository.GetAllAsync(cancellationToken);

        var filtered = allSessions.AsEnumerable();

        if (!string.IsNullOrEmpty(request.Status))
            filtered = filtered.Where(s => s.SessionStatus.ToString() == request.Status);
        if (request.NasId.HasValue)
            filtered = filtered.Where(s => s.NasId == request.NasId.Value);
        if (!string.IsNullOrEmpty(request.Username))
            filtered = filtered.Where(s => s.Username.Contains(request.Username, StringComparison.OrdinalIgnoreCase));
        if (request.DateFrom.HasValue)
            filtered = filtered.Where(s => s.StartedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            filtered = filtered.Where(s => s.StartedAt <= request.DateTo.Value);

        var list = filtered.OrderByDescending(s => s.StartedAt).ToList();
        var total = list.Count;
        var paged = list.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

        return Result.Success(new PaginatedResult<RadiusSessionDto>(
            paged.Adapt<IReadOnlyList<RadiusSessionDto>>(),
            total));
    }
}
