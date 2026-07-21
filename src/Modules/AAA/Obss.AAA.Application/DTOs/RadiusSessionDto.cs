namespace Obss.AAA.Application.DTOs;

public sealed record RadiusSessionDto(
    Guid Id,
    string TenantId,
    string SessionId,
    Guid NasId,
    string Username,
    string? FramedIpAddress,
    string CalledStationId,
    string CallingStationId,
    long AcctSessionTime,
    long InputOctets,
    long OutputOctets,
    string SessionStatus,
    DateTime StartedAt,
    DateTime? StoppedAt,
    DateTime UpdatedAt);
