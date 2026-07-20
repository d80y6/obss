namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed record ActivateNumberRequest(string TelephoneNumber, string? CustomerId, string? ServiceType);

public sealed record ActivateNumberResponse(string TelephoneNumber, string ActivationReference, DateTime ActivatedAt);

public sealed record DeactivateNumberRequest(string TelephoneNumber, string? Reason);

public sealed record DeactivateNumberResponse(string TelephoneNumber, DateTime DeactivatedAt);

public sealed record PortNumberRequest(string TelephoneNumber, string? DonorOperator, string? RecipientOperator, string? PortingReference);

public sealed record PortNumberResponse(string TelephoneNumber, string PortingReference, string Status, DateTime EstimatedCompletion);

public sealed record ReserveNumberRequest(string TelephoneNumber, string? CustomerId, string? ReservationDuration);

public sealed record ReserveNumberResponse(string TelephoneNumber, string ReservationId, DateTime ReservedAt, DateTime ExpiresAt);

public sealed record CreateSubscriberRequest(
    string TelephoneNumber,
    string? CustomerName,
    string? CustomerNameArabic,
    string? Address,
    string? BillingType,
    string? ServicePackage,
    IReadOnlyDictionary<string, string>? CustomAttributes
);

public sealed record SubscriberCreatedResponse(string SubscriberId, string TelephoneNumber, DateTime CreatedAt, string? ServicePackage);

public sealed record UpdateSubscriberProfileRequest(
    string SubscriberId,
    string? CustomerName,
    string? CustomerNameArabic,
    string? Address,
    string? BillingType,
    string? ServicePackage,
    IReadOnlyDictionary<string, string>? CustomAttributes
);

public sealed record SubscriberProfileUpdatedResponse(string SubscriberId, DateTime UpdatedAt);

public sealed record SuspendSubscriberRequest(string SubscriberId, string? Reason);

public sealed record SubscriberSuspendedResponse(string SubscriberId, DateTime SuspendedAt);

public sealed record ResumeSubscriberRequest(string SubscriberId);

public sealed record SubscriberResumedResponse(string SubscriberId, DateTime ResumedAt);

public sealed record TerminateSubscriberRequest(string SubscriberId, string? Reason);

public sealed record SubscriberTerminatedResponse(string SubscriberId, DateTime TerminatedAt);

public sealed record SetCallForwardingRequest(
    string SubscriberId,
    string ForwardingType,
    string ForwardToNumber,
    bool? RingTimeBeforeForward,
    string? Schedule
);

public sealed record CallForwardingResponse(string SubscriberId, string ForwardingType, string Status);

public sealed record SetCallBarringRequest(
    string SubscriberId,
    string BarringType,
    IReadOnlyList<string>? BarredPrefixes,
    bool? InternationalBarring,
    bool? PremiumRateBarring
);

public sealed record CallBarringResponse(string SubscriberId, string BarringType, string Status);

public sealed record SetCallWaitingRequest(string SubscriberId, bool Enabled);

public sealed record CallWaitingResponse(string SubscriberId, bool Enabled, string Status);

public sealed record SetCallerIdRequest(string SubscriberId, string PresentationMode);

public sealed record CallerIdResponse(string SubscriberId, string PresentationMode, string Status);

public sealed record SetVoicemailRequest(string SubscriberId, bool Enabled, string? PinCode, string? GreetingMessage);

public sealed record VoicemailResponse(string SubscriberId, bool Enabled, string Status);

public sealed record ConfigureSupplementaryServiceRequest(string SubscriberId, string ServiceCode, IReadOnlyDictionary<string, string> Parameters);

public sealed record SupplementaryServiceResponse(string SubscriberId, string ServiceCode, string Status);

public sealed record ConfigurePriTrunkRequest(
    string TrunkId,
    string InterfaceType,
    int ChannelCount,
    string? SignalingProtocol,
    string? Framing,
    string? Coding,
    string? TimingSource
);

public sealed record PriTrunkResponse(string TrunkId, string Status, int ActiveChannels);

public sealed record ConfigureTdmCircuitRequest(
    string CircuitId,
    string CircuitType,
    int Timeslot,
    string? TerminationPointA,
    string? TerminationPointZ,
    string? Encoding
);

public sealed record TdmCircuitResponse(string CircuitId, string Status, int Timeslot);

public sealed record FreePhoneRoutingEntry(int Priority, string DestinationNumber, string? TimeOfDay, string? DayOfWeek);

public sealed record ConfigureFreePhoneNumberRequest(
    string TelephoneNumber,
    string? CustomerId,
    string? RoutingPlan,
    IReadOnlyList<FreePhoneRoutingEntry>? RoutingEntries
);

public sealed record FreePhoneNumberResponse(string FreePhoneNumberId, string TelephoneNumber, string Status);

public sealed record UpdateFreePhoneRoutingRequest(
    string FreePhoneNumberId,
    IReadOnlyList<FreePhoneRoutingEntry> RoutingEntries,
    string? Description
);

public sealed record FreePhoneRoutingResponse(string FreePhoneNumberId, string Status, int ActiveRoutes);

public sealed record CallDataRecord(
    string CallId,
    string CallingNumber,
    string CalledNumber,
    DateTime StartTime,
    DateTime? EndTime,
    int? DurationSeconds,
    string? CallType,
    decimal? Charge,
    string? TrunkId
);

public sealed record CdrIngestRequest(string BatchId, IReadOnlyList<CallDataRecord> Records);

public sealed record CdrIngestResponse(string BatchId, int RecordsAccepted, int RecordsRejected, IReadOnlyList<string>? RejectionReasons);

public sealed record AcknowledgeCdrBatchRequest(string BatchId, bool Success, string? ErrorDetails);

public sealed record CdrBatchAcknowledgedResponse(string BatchId, bool Acknowledged);

public sealed record SubscriberStatusRequest(string SubscriberId);

public sealed record SubscriberStatusResponse(string SubscriberId, string Status, string? Imsi, string? Msisdn, string? CurrentVlr, DateTime? LastActivity);

public sealed record ZteAlarm(string AlarmId, string Severity, string Source, string Description, DateTime RaisedAt, bool Acknowledged);

public sealed record ZteAlarmQueryRequest(string? Severity, DateTime? From, DateTime? To, int? MaxResults);

public sealed record ZteAlarmQueryResponse(int TotalAlarms, IReadOnlyList<ZteAlarm>? Alarms);

public sealed record TrunkStatusRequest(string? TrunkId);

public sealed record TrunkStatusResponse(string? TrunkId, string Status, int TotalChannels, int UsedChannels, int AvailableChannels, double UtilizationPercent);

public sealed record ReconcileSubscribersRequest(IReadOnlyList<string>? SubscriberIds, DateTime? LastReconciledSince);

public sealed record ReconcileSubscribersResponse(
    int TotalOnSoftswitch,
    int TotalInSystem,
    IReadOnlyList<string>? MissingInSystem,
    IReadOnlyList<string>? MissingOnSoftswitch,
    IReadOnlyList<string>? Matched
);

public sealed record ConnectionTestResult(bool Connected, TimeSpan Latency, string? ServerVersion, string? Protocol);

public interface IZteSoftswitchAdapter
{
    string AdapterName { get; }

    Task<AdapterResult<ActivateNumberResponse>> ActivateNumberAsync(ActivateNumberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<DeactivateNumberResponse>> DeactivateNumberAsync(DeactivateNumberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<PortNumberResponse>> PortNumberAsync(PortNumberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<ReserveNumberResponse>> ReserveNumberAsync(ReserveNumberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberCreatedResponse>> CreateSubscriberAsync(CreateSubscriberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberProfileUpdatedResponse>> UpdateSubscriberProfileAsync(UpdateSubscriberProfileRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberSuspendedResponse>> SuspendSubscriberAsync(SuspendSubscriberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberResumedResponse>> ResumeSubscriberAsync(ResumeSubscriberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberTerminatedResponse>> TerminateSubscriberAsync(TerminateSubscriberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CallForwardingResponse>> SetCallForwardingAsync(SetCallForwardingRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CallBarringResponse>> SetCallBarringAsync(SetCallBarringRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CallWaitingResponse>> SetCallWaitingAsync(SetCallWaitingRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CallerIdResponse>> SetCallerIdAsync(SetCallerIdRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<VoicemailResponse>> SetVoicemailAsync(SetVoicemailRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SupplementaryServiceResponse>> ConfigureSupplementaryServiceAsync(ConfigureSupplementaryServiceRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<PriTrunkResponse>> ConfigurePriTrunkAsync(ConfigurePriTrunkRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<TdmCircuitResponse>> ConfigureTdmCircuitAsync(ConfigureTdmCircuitRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<FreePhoneNumberResponse>> ConfigureFreePhoneNumberAsync(ConfigureFreePhoneNumberRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<FreePhoneRoutingResponse>> UpdateFreePhoneRoutingAsync(UpdateFreePhoneRoutingRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CdrIngestResponse>> IngestCallDataRecordsAsync(CdrIngestRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<CdrBatchAcknowledgedResponse>> AcknowledgeCdrBatchAsync(AcknowledgeCdrBatchRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<SubscriberStatusResponse>> GetSubscriberStatusAsync(SubscriberStatusRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<ZteAlarmQueryResponse>> GetAlarmsAsync(ZteAlarmQueryRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<TrunkStatusResponse>> GetTrunkStatusAsync(TrunkStatusRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<ReconcileSubscribersResponse>> ReconcileSubscribersAsync(ReconcileSubscribersRequest request, CancellationToken cancellationToken = default);

    Task<AdapterResult<ConnectionTestResult>> TestConnectionAsync(CancellationToken cancellationToken = default);
}
