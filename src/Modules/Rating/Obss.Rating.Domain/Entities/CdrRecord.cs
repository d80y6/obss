using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Domain.Entities;

public class CdrRecord : AggregateRoot<Guid>, ITenantEntity
{
    private CdrRecord() { }

    private CdrRecord(
        Guid id,
        string tenantId,
        string correlationId,
        string vendor,
        string sourceIp,
        string payload,
        string normalizedData,
        string status,
        string? errorReason,
        DateTime receivedAt)
        : base(id)
    {
        TenantId = tenantId;
        CorrelationId = correlationId;
        Vendor = vendor;
        SourceIp = sourceIp;
        Payload = payload;
        NormalizedData = normalizedData;
        Status = status;
        ErrorReason = errorReason;
        ReceivedAt = receivedAt;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string Vendor { get; private set; } = string.Empty;
    public string SourceIp { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string NormalizedData { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Accepted";
    public string? ErrorReason { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static CdrRecord Create(
        string tenantId,
        string correlationId,
        string vendor,
        string sourceIp,
        string payload,
        string normalizedData,
        string status,
        string? errorReason,
        DateTime receivedAt)
    {
        return new CdrRecord(
            Guid.NewGuid(),
            tenantId,
            correlationId,
            vendor,
            sourceIp,
            payload,
            normalizedData,
            status,
            errorReason,
            receivedAt);
    }
}
