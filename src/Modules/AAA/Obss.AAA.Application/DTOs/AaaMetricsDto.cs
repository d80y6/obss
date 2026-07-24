namespace Obss.AAA.Application.DTOs;

public sealed record AaaMetricsDto(
    int TotalNas,
    int ActiveNas,
    int InactiveNas,
    int ActiveSessions,
    int SessionsToday,
    long TotalInputOctets,
    long TotalOutputOctets);
