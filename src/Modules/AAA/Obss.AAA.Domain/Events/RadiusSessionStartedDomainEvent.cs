using Obss.SharedKernel.Domain.Common;

namespace Obss.AAA.Domain.Events;

public sealed class RadiusSessionStartedDomainEvent : DomainEvent
{
    public RadiusSessionStartedDomainEvent(
        Guid sessionId,
        string radiusSessionId,
        string username,
        Guid nasId)
    {
        SessionId = sessionId;
        RadiusSessionId = radiusSessionId;
        Username = username;
        NasId = nasId;
    }

    public Guid SessionId { get; }
    public string RadiusSessionId { get; }
    public string Username { get; }
    public Guid NasId { get; }
}
