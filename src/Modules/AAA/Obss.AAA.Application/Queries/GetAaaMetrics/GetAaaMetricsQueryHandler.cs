using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAaaMetrics;

public sealed class GetAaaMetricsQueryHandler : IRequestHandler<GetAaaMetricsQuery, Result<AaaMetricsDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IRadiusSessionRepository _sessionRepository;

    public GetAaaMetricsQueryHandler(
        INasRepository nasRepository,
        IRadiusSessionRepository sessionRepository)
    {
        _nasRepository = nasRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<AaaMetricsDto>> Handle(GetAaaMetricsQuery request, CancellationToken cancellationToken)
    {
        var allNas = await _nasRepository.GetAllAsync(cancellationToken);
        var activeNas = allNas.Count(n => n.Status == "Active");
        var inactiveNas = allNas.Count(n => n.Status == "Inactive");
        var activeSessions = await _sessionRepository.CountActiveSessionsAsync(cancellationToken);

        var allSessions = await _sessionRepository.GetAllAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var todaySessions = allSessions.Count(s => s.StartedAt >= today);
        var totalInput = allSessions.Sum(s => s.InputOctets);
        var totalOutput = allSessions.Sum(s => s.OutputOctets);

        return Result.Success(new AaaMetricsDto(
            allNas.Count,
            activeNas,
            inactiveNas,
            activeSessions,
            todaySessions,
            totalInput,
            totalOutput));
    }
}
