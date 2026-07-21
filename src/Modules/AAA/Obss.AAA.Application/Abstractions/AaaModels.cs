using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Application.Abstractions;

public sealed record AaaAuthRequest(
    string Username,
    string Password,
    string NasIp,
    string? CalledStationId,
    string? CallingStationId,
    ServiceType ServiceType);

public sealed record AaaAuthResult(
    bool Success,
    string? SessionId,
    string? FramedIp,
    string? ErrorMessage);

public sealed record AaaAcctRequest(
    string Username,
    string SessionId,
    string NasIp,
    string AcctStatusType,
    long AcctSessionTime,
    long InputOctets,
    long OutputOctets,
    string? CalledStationId,
    string? CallingStationId);

public sealed record AaaAcctResult(
    bool Success,
    string? SessionId,
    string? ErrorMessage);

public sealed record AaaCoARequest(
    string SessionId,
    string NasIp,
    string Username,
    IReadOnlyDictionary<string, string> Attributes);

public sealed record AaaCoAResult(
    bool Success,
    string? ErrorMessage);
