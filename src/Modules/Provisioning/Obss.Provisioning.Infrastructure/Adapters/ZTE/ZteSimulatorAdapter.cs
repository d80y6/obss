using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Obss.Provisioning.Infrastructure.Adapters.ZTE;

public sealed class ZteSimulatorAdapter : ZteSoftswitchAdapterBase
{
    private readonly ConcurrentDictionary<string, SimulatedSubscriber> _subscribers = new();
    private readonly ConcurrentDictionary<string, SimulatedActivation> _activations = new();
    private readonly List<ZteAlarm> _alarms = [];
    private int _alarmCounter;
    private readonly Random _random = new();
    private double _failureRate;

    public ZteSimulatorAdapter(
        ILogger<ZteSimulatorAdapter> logger,
        ZteAdapterConfig config,
        ZteOperationProfile profile,
        IBlockedOperationStore blockedOperationStore)
        : base(logger, config, profile, blockedOperationStore)
    {
        SeedAlarms();
    }

    public override string AdapterName => $"{ZteAdapterConstants.AdapterName} (Simulator)";

    public double FailureRate
    {
        get => _failureRate;
        set => _failureRate = Math.Clamp(value, 0, 1);
    }

    public int SubscriberCount => _subscribers.Count;

    public void ClearState()
    {
        _subscribers.Clear();
        _activations.Clear();
        _alarms.Clear();
        _alarmCounter = 0;
        SeedAlarms();
    }

    private bool ShouldInjectFailure() => _random.NextDouble() < _failureRate;

    public override Task<AdapterResult<ActivateNumberResponse>> ActivateNumberAsync(ActivateNumberRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationActivateNumber, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidYemenNumber(request.TelephoneNumber))
                return Task.FromResult(AdapterResult<ActivateNumberResponse>.Failed(
                    $"Invalid Yemen telephone number format: '{request.TelephoneNumber}'. Must start with +967 and be 12-15 digits.",
                    correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<ActivateNumberResponse>.Failed(
                    "Simulated activation failure: softswitch timeout.", correlationId));

            var activationRef = $"ACT-{Guid.NewGuid():N}"[..20];

            _activations[request.TelephoneNumber] = new SimulatedActivation
            {
                TelephoneNumber = request.TelephoneNumber,
                ActivationReference = activationRef,
                ActivatedAt = DateTime.UtcNow,
                CustomerId = request.CustomerId,
                ServiceType = request.ServiceType,
            };

            var response = new ActivateNumberResponse(
                request.TelephoneNumber,
                activationRef,
                DateTime.UtcNow);

            return Task.FromResult(AdapterResult<ActivateNumberResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<DeactivateNumberResponse>> DeactivateNumberAsync(DeactivateNumberRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationDeactivateNumber, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidYemenNumber(request.TelephoneNumber))
                return Task.FromResult(AdapterResult<DeactivateNumberResponse>.Failed(
                    $"Invalid Yemen telephone number format: '{request.TelephoneNumber}'.",
                    correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<DeactivateNumberResponse>.Failed(
                    "Simulated deactivation failure: subscriber not found.", correlationId));

            _activations.TryRemove(request.TelephoneNumber, out _);

            var response = new DeactivateNumberResponse(request.TelephoneNumber, DateTime.UtcNow);
            return Task.FromResult(AdapterResult<DeactivateNumberResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<SubscriberCreatedResponse>> CreateSubscriberAsync(CreateSubscriberRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationCreateSubscriber, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidYemenNumber(request.TelephoneNumber))
                return Task.FromResult(AdapterResult<SubscriberCreatedResponse>.Failed(
                    $"Invalid Yemen telephone number format: '{request.TelephoneNumber}'.",
                    correlationId));

            if (string.IsNullOrWhiteSpace(request.CustomerName) && string.IsNullOrWhiteSpace(request.CustomerNameArabic))
                return Task.FromResult(AdapterResult<SubscriberCreatedResponse>.Failed(
                    "At least one of CustomerName or CustomerNameArabic is required.", correlationId));

            if (_subscribers.Values.Any(s => s.TelephoneNumber == request.TelephoneNumber))
                return Task.FromResult(AdapterResult<SubscriberCreatedResponse>.Failed(
                    $"Subscriber with telephone number '{request.TelephoneNumber}' already exists.", correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<SubscriberCreatedResponse>.Failed(
                    "Simulated subscriber creation failure: HLR update timeout.", correlationId));

            var subscriberId = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            _subscribers[subscriberId] = new SimulatedSubscriber
            {
                SubscriberId = subscriberId,
                TelephoneNumber = request.TelephoneNumber,
                CustomerName = request.CustomerName,
                CustomerNameArabic = request.CustomerNameArabic,
                Address = request.Address,
                BillingType = request.BillingType ?? "postpaid",
                ServicePackage = request.ServicePackage,
                CustomAttributes = request.CustomAttributes,
                CreatedAt = now,
                Status = "Active",
            };

            var response = new SubscriberCreatedResponse(
                subscriberId,
                request.TelephoneNumber,
                now,
                request.ServicePackage);

            return Task.FromResult(AdapterResult<SubscriberCreatedResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<SubscriberProfileUpdatedResponse>> UpdateSubscriberProfileAsync(UpdateSubscriberProfileRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationUpdateSubscriberProfile, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidSubscriberId(request.SubscriberId))
                return Task.FromResult(AdapterResult<SubscriberProfileUpdatedResponse>.Failed(
                    $"Invalid subscriber ID format: '{request.SubscriberId}'.",
                    correlationId));

            if (!_subscribers.TryGetValue(request.SubscriberId, out var existing))
                return Task.FromResult(AdapterResult<SubscriberProfileUpdatedResponse>.Failed(
                    $"Subscriber '{request.SubscriberId}' not found.", correlationId));

            if (existing.Status == "Terminated")
                return Task.FromResult(AdapterResult<SubscriberProfileUpdatedResponse>.Failed(
                    $"Cannot update terminated subscriber '{request.SubscriberId}'.", correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<SubscriberProfileUpdatedResponse>.Failed(
                    "Simulated profile update failure: database lock.", correlationId));

            if (request.CustomerName is not null) existing.CustomerName = request.CustomerName;
            if (request.CustomerNameArabic is not null) existing.CustomerNameArabic = request.CustomerNameArabic;
            if (request.Address is not null) existing.Address = request.Address;
            if (request.BillingType is not null) existing.BillingType = request.BillingType;
            if (request.ServicePackage is not null) existing.ServicePackage = request.ServicePackage;
            if (request.CustomAttributes is not null) existing.CustomAttributes = request.CustomAttributes;

            var response = new SubscriberProfileUpdatedResponse(request.SubscriberId, DateTime.UtcNow);
            return Task.FromResult(AdapterResult<SubscriberProfileUpdatedResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<SubscriberSuspendedResponse>> SuspendSubscriberAsync(SuspendSubscriberRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationSuspendSubscriber, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidSubscriberId(request.SubscriberId))
                return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Failed(
                    $"Invalid subscriber ID format: '{request.SubscriberId}'.",
                    correlationId));

            if (!_subscribers.TryGetValue(request.SubscriberId, out var existing))
                return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Failed(
                    $"Subscriber '{request.SubscriberId}' not found.", correlationId));

            if (existing.Status == "Suspended")
                return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Failed(
                    $"Subscriber '{request.SubscriberId}' is already suspended.", correlationId));

            if (existing.Status == "Terminated")
                return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Failed(
                    $"Cannot suspend terminated subscriber '{request.SubscriberId}'.", correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Failed(
                    "Simulated suspension failure: HLR update timeout.", correlationId));

            existing.Status = "Suspended";

            var response = new SubscriberSuspendedResponse(request.SubscriberId, DateTime.UtcNow);
            return Task.FromResult(AdapterResult<SubscriberSuspendedResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<SubscriberResumedResponse>> ResumeSubscriberAsync(ResumeSubscriberRequest request, CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationResumeSubscriber, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (!IsValidSubscriberId(request.SubscriberId))
                return Task.FromResult(AdapterResult<SubscriberResumedResponse>.Failed(
                    $"Invalid subscriber ID format: '{request.SubscriberId}'.",
                    correlationId));

            if (!_subscribers.TryGetValue(request.SubscriberId, out var existing))
                return Task.FromResult(AdapterResult<SubscriberResumedResponse>.Failed(
                    $"Subscriber '{request.SubscriberId}' not found.", correlationId));

            if (existing.Status != "Suspended")
                return Task.FromResult(AdapterResult<SubscriberResumedResponse>.Failed(
                    $"Subscriber '{request.SubscriberId}' is not suspended. Current status: {existing.Status}.",
                    correlationId));

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<SubscriberResumedResponse>.Failed(
                    "Simulated resume failure: network error.", correlationId));

            existing.Status = "Active";

            var response = new SubscriberResumedResponse(request.SubscriberId, DateTime.UtcNow);
            return Task.FromResult(AdapterResult<SubscriberResumedResponse>.Succeeded(response, correlationId));
        }, cancellationToken);

    public override Task<AdapterResult<ConnectionTestResult>> TestConnectionAsync(CancellationToken cancellationToken = default)
        => ExecuteAsync(ZteAdapterConstants.OperationTestConnection, () =>
        {
            var correlationId = GenerateCorrelationId();

            if (ShouldInjectFailure())
                return Task.FromResult(AdapterResult<ConnectionTestResult>.Failed(
                    "Simulated connection failure: no route to host.", correlationId));

            var result = new ConnectionTestResult(
                true,
                TimeSpan.FromMilliseconds(_random.Next(5, 200)),
                "ZTE-SIM/v1.0",
                Config.Protocol ?? ZteAdapterConstants.DefaultProtocol);

            return Task.FromResult(AdapterResult<ConnectionTestResult>.Succeeded(result, correlationId));
        }, cancellationToken);

    private void SeedAlarms()
    {
        _alarms.Add(new ZteAlarm(
            $"ALM-{++_alarmCounter}", "INFO", "HLR-SIM",
            "Simulator initialized successfully", DateTime.UtcNow.AddHours(-1), true));

        _alarms.Add(new ZteAlarm(
            $"ALM-{++_alarmCounter}", "WARNING", "MGW-SIM",
            "Simulated media gateway at 70% capacity", DateTime.UtcNow.AddMinutes(-30), false));

        _alarms.Add(new ZteAlarm(
            $"ALM-{++_alarmCounter}", "INFO", "SIGTRAN-SIM",
            "Signaling link established", DateTime.UtcNow.AddMinutes(-15), true));
    }

    private sealed class SimulatedSubscriber
    {
        public string SubscriberId { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerNameArabic { get; set; }
        public string? Address { get; set; }
        public string? BillingType { get; set; }
        public string? ServicePackage { get; set; }
        public IReadOnlyDictionary<string, string>? CustomAttributes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Active";
    }

    private sealed class SimulatedActivation
    {
        public string TelephoneNumber { get; set; } = string.Empty;
        public string ActivationReference { get; set; } = string.Empty;
        public DateTime ActivatedAt { get; set; }
        public string? CustomerId { get; set; }
        public string? ServiceType { get; set; }
    }

}
