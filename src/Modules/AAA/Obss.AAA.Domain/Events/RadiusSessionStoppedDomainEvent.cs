using Obss.SharedKernel.Domain.Common;

namespace Obss.AAA.Domain.Events;

public sealed class RadiusSessionStoppedDomainEvent : DomainEvent
{
    public RadiusSessionStoppedDomainEvent(
        Guid sessionId,
        string radiusSessionId,
        string username,
        Guid nasId,
        long acctSessionTime,
        long inputOctets,
        long outputOctets)
    {
        SessionId = sessionId;
        RadiusSessionId = radiusSessionId;
        Username = username;
        NasId = nasId;
        AcctSessionTime = acctSessionTime;
        InputOctets = inputOctets;
        OutputOctets = outputOctets;
    }

    public Guid SessionId { get; }
    public string RadiusSessionId { get; }
    public string Username { get; }
    public Guid NasId { get; }
    public long AcctSessionTime { get; }
    public long InputOctets { get; }
    public long OutputOctets { get; }
}
