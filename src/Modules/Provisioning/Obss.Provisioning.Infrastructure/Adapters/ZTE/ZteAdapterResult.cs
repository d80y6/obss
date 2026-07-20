using System.Text.Json;

namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed record AdapterResult<T>
{
    public bool IsSuccess => !IsBlocked && ErrorMessage is null;
    public bool IsBlocked { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? BlockedReason { get; init; }
    public string CorrelationId { get; init; } = string.Empty;

    public static AdapterResult<T> Succeeded(T data, string correlationId) =>
        new() { Data = data, CorrelationId = correlationId };

    public static AdapterResult<T> Failed(string error, string correlationId) =>
        new() { ErrorMessage = error, CorrelationId = correlationId };

    public static AdapterResult<T> Blocked(string reason, string correlationId) =>
        new() { IsBlocked = true, BlockedReason = reason, CorrelationId = correlationId };
}
