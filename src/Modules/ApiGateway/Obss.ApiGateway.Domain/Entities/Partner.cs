using Obss.ApiGateway.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ApiGateway.Domain.Entities;

public class Partner : AggregateRoot<Guid>
{
    private readonly List<ApiKey> _apiKeys = [];

    private Partner() { }

    private Partner(
        Guid id,
        string tenantId,
        string name,
        string contactName,
        string contactEmail,
        List<string> allowedIPs,
        SlaLevel slaLevel,
        int maxRequestsPerDay)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        ContactName = contactName;
        ContactEmail = contactEmail;
        AllowedIPs = allowedIPs;
        SlaLevel = slaLevel;
        MaxRequestsPerDay = maxRequestsPerDay;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    public List<string> AllowedIPs { get; private set; } = [];
    public bool IsActive { get; private set; }
    public SlaLevel SlaLevel { get; private set; }
    public int MaxRequestsPerDay { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();

    public static Partner Create(
        string tenantId,
        string name,
        string contactName,
        string contactEmail,
        List<string>? allowedIPs = null,
        SlaLevel slaLevel = SlaLevel.Basic,
        int maxRequestsPerDay = 10000)
    {
        return new Partner(
            Guid.NewGuid(),
            tenantId,
            name,
            contactName,
            contactEmail,
            allowedIPs ?? [],
            slaLevel,
            maxRequestsPerDay);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddApiKey(ApiKey apiKey)
    {
        _apiKeys.Add(apiKey);
    }

    public void UpdateContact(string contactName, string contactEmail)
    {
        ContactName = contactName;
        ContactEmail = contactEmail;
    }

    public void UpdateSla(SlaLevel slaLevel, int maxRequestsPerDay)
    {
        SlaLevel = slaLevel;
        MaxRequestsPerDay = maxRequestsPerDay;
    }
}
