using Obss.AAA.Domain.Events;
using Obss.AAA.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.AAA.Domain.Entities;

public class RadiusSession : AggregateRoot<Guid>, ITenantEntity
{
    private RadiusSession() { }

    private RadiusSession(
        Guid id,
        string tenantId,
        string sessionId,
        Guid nasId,
        string username,
        string? framedIpAddress,
        string calledStationId,
        string callingStationId)
        : base(id)
    {
        TenantId = tenantId;
        SessionId = sessionId;
        NasId = nasId;
        Username = username;
        FramedIpAddress = framedIpAddress;
        CalledStationId = calledStationId;
        CallingStationId = callingStationId;
        SessionStatus = SessionStatus.Active;
        AcctSessionTime = 0;
        InputOctets = 0;
        OutputOctets = 0;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string SessionId { get; private set; } = string.Empty;
    public Guid NasId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string? FramedIpAddress { get; private set; }
    public string CalledStationId { get; private set; } = string.Empty;
    public string CallingStationId { get; private set; } = string.Empty;
    public long AcctSessionTime { get; private set; }
    public long InputOctets { get; private set; }
    public long OutputOctets { get; private set; }
    public SessionStatus SessionStatus { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static RadiusSession Create(
        string tenantId,
        string sessionId,
        Guid nasId,
        string username,
        string? framedIpAddress,
        string calledStationId,
        string callingStationId)
    {
        var session = new RadiusSession(
            Guid.NewGuid(),
            tenantId,
            sessionId,
            nasId,
            username,
            framedIpAddress,
            calledStationId,
            callingStationId);

        session.AddDomainEvent(new RadiusSessionStartedDomainEvent(
            session.Id, session.SessionId, session.Username, session.NasId));

        return session;
    }

    public void Stop()
    {
        if (SessionStatus == SessionStatus.Stopped)
            return;

        SessionStatus = SessionStatus.Stopped;
        StoppedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RadiusSessionStoppedDomainEvent(
            Id, SessionId, Username, NasId, AcctSessionTime, InputOctets, OutputOctets));
    }

    public void RecordInterim(long inputOctets, long outputOctets, long acctSessionTime)
    {
        InputOctets = inputOctets;
        OutputOctets = outputOctets;
        AcctSessionTime = acctSessionTime;
        SessionStatus = SessionStatus.Interim;
        UpdatedAt = DateTime.UtcNow;
    }
}
