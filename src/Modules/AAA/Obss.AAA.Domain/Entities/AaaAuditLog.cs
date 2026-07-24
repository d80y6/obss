using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Domain.Entities;

public class AaaAuditLog : AggregateRoot<Guid>, ITenantEntity
{
    private AaaAuditLog() { }

    private AaaAuditLog(
        Guid id,
        string tenantId,
        string eventType,
        string? username,
        Guid? nasId,
        string? nasIpAddress,
        string? detail)
        : base(id)
    {
        TenantId = tenantId;
        EventType = eventType;
        Username = username;
        NasId = nasId;
        NasIpAddress = nasIpAddress;
        Detail = detail;
        Timestamp = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string? Username { get; private set; }
    public Guid? NasId { get; private set; }
    public string? NasIpAddress { get; private set; }
    public string? Detail { get; private set; }
    public DateTime Timestamp { get; private set; }

    public static AaaAuditLog Create(
        string tenantId,
        string eventType,
        string? username = null,
        Guid? nasId = null,
        string? nasIpAddress = null,
        string? detail = null)
    {
        return new AaaAuditLog(Guid.NewGuid(), tenantId, eventType, username, nasId, nasIpAddress, detail);
    }
}
