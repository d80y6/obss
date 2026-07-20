using Microsoft.Extensions.Logging;
using Obss.Rating.Domain.Entities;
using Obss.Rating.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Services;

public sealed class CdrQuarantineRecord
{
    public string CorrelationId { get; init; } = string.Empty;
    public string Vendor { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public string ErrorReason { get; init; } = string.Empty;
    public DateTime ReceivedAt { get; init; }
    public DateTime QuarantinedAt { get; init; }
}

public sealed class CdrMediationService : ICdrMediationService
{
    private readonly IHuaweiCdrParser _huaweiParser;
    private readonly IZteCdrParser _zteParser;
    private readonly ILogger<CdrMediationService> _logger;
    private readonly HashSet<string> _seenCorrelationIds = [];
    private readonly List<CdrQuarantineRecord> _quarantine = [];

    public CdrMediationService(
        IHuaweiCdrParser huaweiParser,
        IZteCdrParser zteParser,
        ILogger<CdrMediationService> logger)
    {
        _huaweiParser = huaweiParser;
        _zteParser = zteParser;
        _logger = logger;
    }

    public Task<CdrIngestResult> IngestBatchAsync(
        IReadOnlyCollection<RawCdrRecord> records,
        CancellationToken cancellationToken = default)
    {
        var accepted = 0;
        var duplicates = 0;
        var quarantined = 0;
        var errors = new List<string>();

        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_seenCorrelationIds.Add(record.CorrelationId))
            {
                duplicates++;
                _logger.LogWarning("Duplicate CDR correlation {CorrelationId} from {Vendor}", record.CorrelationId, record.Vendor);
                continue;
            }

            var validation = Validate(record);
            if (!validation.IsValid)
            {
                quarantined++;
                _quarantine.Add(new CdrQuarantineRecord
                {
                    CorrelationId = record.CorrelationId,
                    Vendor = record.Vendor,
                    Payload = record.Payload,
                    ErrorReason = validation.ErrorReason ?? "Unknown validation error",
                    ReceivedAt = record.ReceivedAt,
                    QuarantinedAt = DateTime.UtcNow
                });
                errors.Add($"Correlation {record.CorrelationId}: {validation.ErrorReason}");
                _logger.LogWarning("CDR {CorrelationId} quarantined: {Reason}", record.CorrelationId, validation.ErrorReason);
                continue;
            }

            var normalized = Normalize(record);
            if (normalized is null)
            {
                quarantined++;
                _quarantine.Add(new CdrQuarantineRecord
                {
                    CorrelationId = record.CorrelationId,
                    Vendor = record.Vendor,
                    Payload = record.Payload,
                    ErrorReason = "Normalization failed",
                    ReceivedAt = record.ReceivedAt,
                    QuarantinedAt = DateTime.UtcNow
                });
                errors.Add($"Correlation {record.CorrelationId}: normalization failed");
                continue;
            }

            accepted++;
        }

        var result = new CdrIngestResult(accepted, duplicates, quarantined, errors.AsReadOnly());
        _logger.LogInformation(
            "CDR ingest complete: {Accepted} accepted, {Duplicates} duplicates, {Quarantined} quarantined",
            result.Accepted, result.Duplicates, result.Quarantined);

        return Task.FromResult(result);
    }

    public Task<CdrReplayResult> ReplayQuarantinedAsync(CancellationToken cancellationToken = default)
    {
        var replayed = 0;
        var stillInvalid = 0;
        var errors = new List<string>();

        var remaining = new List<CdrQuarantineRecord>();

        foreach (var quarantined in _quarantine)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var rawRecord = new RawCdrRecord(
                quarantined.CorrelationId,
                quarantined.Vendor,
                quarantined.Payload,
                string.Empty,
                quarantined.ReceivedAt);

            var validation = Validate(rawRecord);
            if (validation.IsValid)
            {
                replayed++;
                _logger.LogInformation("Replayed quarantined CDR {CorrelationId}", quarantined.CorrelationId);
            }
            else
            {
                stillInvalid++;
                remaining.Add(quarantined);
                errors.Add($"Correlation {quarantined.CorrelationId}: still invalid - {validation.ErrorReason}");
            }
        }

        _quarantine.Clear();
        _quarantine.AddRange(remaining);

        return Task.FromResult(new CdrReplayResult(replayed, stillInvalid, errors.AsReadOnly()));
    }

    private static CdrValidationResult Validate(RawCdrRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.CorrelationId))
        {
            return new CdrValidationResult(false, "Correlation ID is empty");
        }

        if (string.IsNullOrWhiteSpace(record.Vendor))
        {
            return new CdrValidationResult(false, "Vendor is empty");
        }

        if (string.IsNullOrWhiteSpace(record.Payload))
        {
            return new CdrValidationResult(false, "Payload is empty");
        }

        if (record.Vendor is not ("Huawei" or "ZTE"))
        {
            return new CdrValidationResult(false, $"Unsupported vendor: {record.Vendor}");
        }

        return new CdrValidationResult(true, null);
    }

    private NormalizedCdrData? Normalize(RawCdrRecord record)
    {
        var result = record.Vendor switch
        {
            "Huawei" => _huaweiParser.Parse(record.Payload),
            "ZTE" => _zteParser.Parse(record.Payload),
            _ => Result.Failure<NormalizedCdrData>(Error.Validation($"Unsupported vendor: {record.Vendor}"))
        };

        return result.IsSuccess ? result.Value : null;
    }
}
