using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Domain.Entities;

public class NetworkAccessServer : AggregateRoot<Guid>, ITenantEntity
{
    private NetworkAccessServer() { }

    private NetworkAccessServer(
        Guid id,
        string tenantId,
        string name,
        string nasIpAddress,
        string nasSecret,
        NasType nasType,
        string? location)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        NasIpAddress = nasIpAddress;
        NasSecret = nasSecret;
        NasType = nasType;
        Location = location;
        Status = "Active";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string NasIpAddress { get; private set; } = string.Empty;
    public string NasSecret { get; private set; } = string.Empty;
    public NasType NasType { get; private set; }
    public string? Location { get; private set; }
    public string Status { get; private set; } = "Active";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static NetworkAccessServer Create(
        string tenantId,
        string name,
        string nasIpAddress,
        string nasSecret,
        NasType nasType,
        string? location = null)
    {
        return new NetworkAccessServer(
            Guid.NewGuid(),
            tenantId,
            name,
            nasIpAddress,
            nasSecret,
            nasType,
            location);
    }

    public void Activate()
    {
        if (Status == "Active")
            return;

        Status = "Active";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == "Inactive")
            return;

        Status = "Inactive";
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(
        string name,
        string nasIpAddress,
        string nasSecret,
        NasType nasType,
        string? location)
    {
        Name = name;
        NasIpAddress = nasIpAddress;
        NasSecret = nasSecret;
        NasType = nasType;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }
}
