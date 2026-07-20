using Obss.EventManagement.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.EventManagement.Domain.Entities;

public class EventSubscription : AggregateRoot<Guid>
{
    private readonly List<EventFilter> _filters = [];

    private EventSubscription() { }

    private EventSubscription(
        Guid id,
        string name,
        string callbackUrl,
        string? signingSecret,
        string? query,
        string status,
        string? description)
        : base(id)
    {
        Name = name;
        CallbackUrl = callbackUrl;
        SigningSecret = signingSecret;
        Query = query;
        Status = status;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string CallbackUrl { get; private set; } = string.Empty;
    public string? SigningSecret { get; private set; }
    public string? Query { get; private set; }
    public string Status { get; private set; } = "active";
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<EventFilter> Filters => _filters.AsReadOnly();

    public static EventSubscription Create(
        string name,
        string callbackUrl,
        string? signingSecret,
        string? query,
        string? description)
    {
        return new EventSubscription(
            Guid.NewGuid(),
            name,
            callbackUrl,
            signingSecret,
            query,
            "active",
            description);
    }

    public void AddFilter(EventFilter filter)
    {
        _filters.Add(filter);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = "active";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = "inactive";
        UpdatedAt = DateTime.UtcNow;
    }

    public void RotateSigningSecret(string newSecret)
    {
        SigningSecret = newSecret;
        UpdatedAt = DateTime.UtcNow;
    }
}
