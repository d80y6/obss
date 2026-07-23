namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public enum AdapterOperationState
{
    Success,
    Failed,
    BlockedNeedsOperator,
    PendingVendorConfirmation,
    Simulated
}

public sealed record AdapterResult
{
    public bool Success { get; init; }
    public bool IsSuccess => Success;
    public string? ErrorMessage { get; init; }
    public string? ResultData { get; init; }
    public AdapterOperationState State { get; init; }
    public TimeSpan Duration { get; init; }
    public string AdapterName { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;

    public static AdapterResult Ok(
        string? resultData = null,
        TimeSpan? duration = null,
        string? adapterName = null,
        string? correlationId = null)
    {
        return new AdapterResult
        {
            Success = true,
            ResultData = resultData,
            Duration = duration ?? TimeSpan.Zero,
            State = AdapterOperationState.Success,
            AdapterName = adapterName ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static AdapterResult Fail(
        string error,
        TimeSpan? duration = null,
        string? adapterName = null,
        string? correlationId = null)
    {
        return new AdapterResult
        {
            Success = false,
            ErrorMessage = error,
            Duration = duration ?? TimeSpan.Zero,
            State = AdapterOperationState.Failed,
            AdapterName = adapterName ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static AdapterResult Blocked(
        string reason,
        string correlationId,
        string? adapterName = null)
    {
        return new AdapterResult
        {
            Success = false,
            ErrorMessage = reason,
            State = AdapterOperationState.BlockedNeedsOperator,
            AdapterName = adapterName ?? string.Empty,
            CorrelationId = correlationId
        };
    }

    public static AdapterResult VendorPending(
        string? resultData = null,
        string? adapterName = null,
        string? correlationId = null)
    {
        return new AdapterResult
        {
            Success = true,
            ResultData = resultData,
            State = AdapterOperationState.PendingVendorConfirmation,
            AdapterName = adapterName ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty
        };
    }

    public static AdapterResult Simulated(
        string? resultData = null,
        TimeSpan? duration = null,
        string? adapterName = null,
        string? correlationId = null)
    {
        return new AdapterResult
        {
            Success = true,
            ResultData = resultData,
            Duration = duration ?? TimeSpan.Zero,
            State = AdapterOperationState.Simulated,
            AdapterName = adapterName ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty
        };
    }
}

public sealed record AdapterResult<T>
{
    public bool IsSuccess { get; init; }
    public T Data { get; init; } = default!;
    public string? ErrorMessage { get; init; }

    public static AdapterResult<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static AdapterResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
