using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public abstract class ZteSoftswitchAdapterBase : IZteSoftswitchAdapter
{
    private readonly ILogger _logger;
    private readonly ZteAdapterConfig _config;
    private readonly ZteOperationProfile _profile;
    private readonly IBlockedOperationStore _blockedOperationStore;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected ZteSoftswitchAdapterBase(
        ILogger logger,
        ZteAdapterConfig config,
        ZteOperationProfile profile,
        IBlockedOperationStore blockedOperationStore)
    {
        _logger = logger;
        _config = config;
        _profile = profile;
        _blockedOperationStore = blockedOperationStore;
    }

    public abstract string AdapterName { get; }

    protected ILogger Logger => _logger;
    protected ZteAdapterConfig Config => _config;
    protected ZteOperationProfile Profile => _profile;

    protected static string GenerateCorrelationId() => $"ZTE-{Guid.NewGuid():N}-{DateTime.UtcNow:yyyyMMddHHmmss}";

    protected static bool IsValidYemenNumber(string? telephoneNumber)
    {
        if (string.IsNullOrWhiteSpace(telephoneNumber))
            return false;

        var trimmed = telephoneNumber.Trim();
        return trimmed.StartsWith(ZteAdapterConstants.YemenCountryCode) &&
               trimmed.Length >= 12 && trimmed.Length <= 15 &&
               trimmed[ZteAdapterConstants.YemenCountryCode.Length..].All(char.IsDigit);
    }

    protected static bool IsValidSubscriberId(string? subscriberId)
    {
        return !string.IsNullOrWhiteSpace(subscriberId) &&
               Guid.TryParse(subscriberId, out _);
    }

    protected async Task<AdapterResult<T>> BlockedAsync<T>(string operationName, object request, CancellationToken cancellationToken)
    {
        var correlationId = GenerateCorrelationId();
        var payload = JsonSerializer.Serialize(request, _jsonOptions);

        _logger.LogWarning(
            "Blocked ZTE operation [{Operation}] {CorrelationId} (profile v{Profile}): {Payload}",
            operationName, correlationId, _profile.ProfileVersion, payload);

        var blocked = new BlockedOperation
        {
            OperationName = operationName,
            CorrelationId = correlationId,
            RequestPayload = payload,
            Reason = $"Operation '{operationName}' not yet confirmed with ZTE vendor API contract.",
        };

        await _blockedOperationStore.SaveAsync(blocked, cancellationToken);

        return AdapterResult<T>.Blocked(
            $"Operation '{operationName}' not yet confirmed with ZTE vendor API contract. Requires manual intervention.",
            correlationId);
    }

    protected async Task<AdapterResult<T>> ExecuteAsync<T>(
        string operationName,
        Func<Task<AdapterResult<T>>> operation,
        CancellationToken cancellationToken)
    {
        var correlationId = GenerateCorrelationId();

        try
        {
            _logger.LogInformation("Executing ZTE operation [{Operation}] {CorrelationId}", operationName, correlationId);
            var result = await operation();
            _logger.LogInformation(
                "ZTE operation [{Operation}] {CorrelationId} completed: IsSuccess={IsSuccess}, IsBlocked={IsBlocked}",
                operationName, correlationId, result.IsSuccess, result.IsBlocked);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZTE operation [{Operation}] {CorrelationId} failed with exception", operationName, correlationId);
            return AdapterResult<T>.Failed($"ZTE operation '{operationName}' failed: {ex.Message}", correlationId);
        }
    }

    public virtual Task<AdapterResult<ActivateNumberResponse>> ActivateNumberAsync(ActivateNumberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<ActivateNumberResponse>(ZteAdapterConstants.OperationActivateNumber, request, cancellationToken);

    public virtual Task<AdapterResult<DeactivateNumberResponse>> DeactivateNumberAsync(DeactivateNumberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<DeactivateNumberResponse>(ZteAdapterConstants.OperationDeactivateNumber, request, cancellationToken);

    public virtual Task<AdapterResult<PortNumberResponse>> PortNumberAsync(PortNumberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<PortNumberResponse>(ZteAdapterConstants.OperationPortNumber, request, cancellationToken);

    public virtual Task<AdapterResult<ReserveNumberResponse>> ReserveNumberAsync(ReserveNumberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<ReserveNumberResponse>(ZteAdapterConstants.OperationReserveNumber, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberCreatedResponse>> CreateSubscriberAsync(CreateSubscriberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberCreatedResponse>(ZteAdapterConstants.OperationCreateSubscriber, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberProfileUpdatedResponse>> UpdateSubscriberProfileAsync(UpdateSubscriberProfileRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberProfileUpdatedResponse>(ZteAdapterConstants.OperationUpdateSubscriberProfile, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberSuspendedResponse>> SuspendSubscriberAsync(SuspendSubscriberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberSuspendedResponse>(ZteAdapterConstants.OperationSuspendSubscriber, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberResumedResponse>> ResumeSubscriberAsync(ResumeSubscriberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberResumedResponse>(ZteAdapterConstants.OperationResumeSubscriber, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberTerminatedResponse>> TerminateSubscriberAsync(TerminateSubscriberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberTerminatedResponse>(ZteAdapterConstants.OperationTerminateSubscriber, request, cancellationToken);

    public virtual Task<AdapterResult<CallForwardingResponse>> SetCallForwardingAsync(SetCallForwardingRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CallForwardingResponse>(ZteAdapterConstants.OperationSetCallForwarding, request, cancellationToken);

    public virtual Task<AdapterResult<CallBarringResponse>> SetCallBarringAsync(SetCallBarringRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CallBarringResponse>(ZteAdapterConstants.OperationSetCallBarring, request, cancellationToken);

    public virtual Task<AdapterResult<CallWaitingResponse>> SetCallWaitingAsync(SetCallWaitingRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CallWaitingResponse>(ZteAdapterConstants.OperationSetCallWaiting, request, cancellationToken);

    public virtual Task<AdapterResult<CallerIdResponse>> SetCallerIdAsync(SetCallerIdRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CallerIdResponse>(ZteAdapterConstants.OperationSetCallerId, request, cancellationToken);

    public virtual Task<AdapterResult<VoicemailResponse>> SetVoicemailAsync(SetVoicemailRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<VoicemailResponse>(ZteAdapterConstants.OperationSetVoicemail, request, cancellationToken);

    public virtual Task<AdapterResult<SupplementaryServiceResponse>> ConfigureSupplementaryServiceAsync(ConfigureSupplementaryServiceRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SupplementaryServiceResponse>(ZteAdapterConstants.OperationConfigureSupplementaryService, request, cancellationToken);

    public virtual Task<AdapterResult<PriTrunkResponse>> ConfigurePriTrunkAsync(ConfigurePriTrunkRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<PriTrunkResponse>(ZteAdapterConstants.OperationConfigurePriTrunk, request, cancellationToken);

    public virtual Task<AdapterResult<TdmCircuitResponse>> ConfigureTdmCircuitAsync(ConfigureTdmCircuitRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<TdmCircuitResponse>(ZteAdapterConstants.OperationConfigureTdmCircuit, request, cancellationToken);

    public virtual Task<AdapterResult<FreePhoneNumberResponse>> ConfigureFreePhoneNumberAsync(ConfigureFreePhoneNumberRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<FreePhoneNumberResponse>(ZteAdapterConstants.OperationConfigureFreePhoneNumber, request, cancellationToken);

    public virtual Task<AdapterResult<FreePhoneRoutingResponse>> UpdateFreePhoneRoutingAsync(UpdateFreePhoneRoutingRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<FreePhoneRoutingResponse>(ZteAdapterConstants.OperationUpdateFreePhoneRouting, request, cancellationToken);

    public virtual Task<AdapterResult<CdrIngestResponse>> IngestCallDataRecordsAsync(CdrIngestRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CdrIngestResponse>(ZteAdapterConstants.OperationIngestCallDataRecords, request, cancellationToken);

    public virtual Task<AdapterResult<CdrBatchAcknowledgedResponse>> AcknowledgeCdrBatchAsync(AcknowledgeCdrBatchRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<CdrBatchAcknowledgedResponse>(ZteAdapterConstants.OperationAcknowledgeCdrBatch, request, cancellationToken);

    public virtual Task<AdapterResult<SubscriberStatusResponse>> GetSubscriberStatusAsync(SubscriberStatusRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<SubscriberStatusResponse>(ZteAdapterConstants.OperationGetSubscriberStatus, request, cancellationToken);

    public virtual Task<AdapterResult<ZteAlarmQueryResponse>> GetAlarmsAsync(ZteAlarmQueryRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<ZteAlarmQueryResponse>(ZteAdapterConstants.OperationGetAlarms, request, cancellationToken);

    public virtual Task<AdapterResult<TrunkStatusResponse>> GetTrunkStatusAsync(TrunkStatusRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<TrunkStatusResponse>(ZteAdapterConstants.OperationGetTrunkStatus, request, cancellationToken);

    public virtual Task<AdapterResult<ReconcileSubscribersResponse>> ReconcileSubscribersAsync(ReconcileSubscribersRequest request, CancellationToken cancellationToken = default)
        => BlockedAsync<ReconcileSubscribersResponse>(ZteAdapterConstants.OperationReconcileSubscribers, request, cancellationToken);

    public virtual Task<AdapterResult<ConnectionTestResult>> TestConnectionAsync(CancellationToken cancellationToken = default)
        => BlockedAsync<ConnectionTestResult>(ZteAdapterConstants.OperationTestConnection, null!, cancellationToken);
}
