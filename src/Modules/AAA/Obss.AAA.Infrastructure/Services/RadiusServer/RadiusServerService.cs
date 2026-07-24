using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;
using Obss.AAA.Infrastructure.Services.Radius;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Infrastructure.Services.RadiusServer;

public sealed class RadiusServerService : BackgroundService
{
    private readonly ILogger<RadiusServerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, DateTime> _nasEndpoints = new();

    private const int AuthPort = 1812;
    private const int AcctPort = 1813;

    public RadiusServerService(ILogger<RadiusServerService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RADIUS server starting on ports {AuthPort} (auth) and {AcctPort} (acct)", AuthPort, AcctPort);

        var authTask = ListenOnPortAsync(AuthPort, stoppingToken);
        var acctTask = ListenOnPortAsync(AcctPort, stoppingToken);

        await Task.WhenAll(authTask, acctTask);
    }

    private async Task ListenOnPortAsync(int port, CancellationToken ct)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

        try
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind RADIUS server to port {Port}", port);
            return;
        }

        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var remoteEp = new IPEndPoint(IPAddress.Any, 0);
                var result = await socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEp, ct);
                var remoteEndPoint = (IPEndPoint)result.RemoteEndPoint;
                var data = buffer[..result.ReceivedBytes];

                _ = ProcessPacketAsync(data, remoteEndPoint, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RADIUS server receive error on port {Port}", port);
            }
        }
    }

    private async Task ProcessPacketAsync(byte[] data, IPEndPoint remoteEp, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var nasRepository = scope.ServiceProvider.GetRequiredService<INasRepository>();
            var sessionRepository = scope.ServiceProvider.GetRequiredService<IRadiusSessionRepository>();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

            var nas = await nasRepository.GetByIpAddressAsync(remoteEp.Address.ToString(), ct);
            if (nas is null)
            {
                _logger.LogWarning("RADIUS packet from unknown NAS {Ip}", remoteEp.Address);
                return;
            }

            var packet = RadiusPacket.Parse(data, nas.NasSecret);
            _nasEndpoints[remoteEp.Address.ToString()] = DateTime.UtcNow;

            switch (packet.Code)
            {
                case RadiusCode.AccountingRequest:
                    await HandleAccountingAsync(packet, nas, sessionRepository, currentTenant, ct);
                    break;

                case RadiusCode.AccessRequest:
                case RadiusCode.DisconnectRequest:
                case RadiusCode.CoARequest:
                    _logger.LogInformation("RADIUS {Code} from {Name} at {Ip}", packet.Code, nas.Name, nas.NasIpAddress);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process RADIUS packet from {RemoteEp}", remoteEp);
        }
    }

    private async Task HandleAccountingAsync(
        RadiusPacket packet,
        NetworkAccessServer nas,
        IRadiusSessionRepository sessionRepository,
        ICurrentTenant currentTenant,
        CancellationToken ct)
    {
        var acctStatusType = packet.GetUintAttribute(RadiusAttributeType.AcctStatusType);
        var sessionId = packet.GetStringAttribute(RadiusAttributeType.AcctSessionId);
        var username = packet.GetStringAttribute(RadiusAttributeType.UserName) ?? "unknown";
        var framedIp = packet.GetIpAttribute(RadiusAttributeType.FramedIpAddress)?.ToString();
        var calledStationId = packet.GetStringAttribute(RadiusAttributeType.CalledStationId) ?? "";
        var callingStationId = packet.GetStringAttribute(RadiusAttributeType.CallingStationId) ?? "";
        var acctSessionTime = packet.GetUintAttribute(RadiusAttributeType.AcctSessionTime) ?? 0;
        var inputOctets = packet.GetUintAttribute(RadiusAttributeType.AcctInputOctets) ?? 0;
        var outputOctets = packet.GetUintAttribute(RadiusAttributeType.AcctOutputOctets) ?? 0;

        var tenantId = currentTenant.TenantId ?? nas.TenantId;

        switch (acctStatusType)
        {
            case 1:
            {
                var session = RadiusSession.Create(
                    tenantId,
                    sessionId ?? Guid.NewGuid().ToString("N"),
                    nas.Id,
                    username,
                    framedIp,
                    calledStationId,
                    callingStationId);

                await sessionRepository.AddAsync(session, ct);
                _logger.LogInformation("RADIUS session started: {SessionId} for {User} on {Nas}", session.SessionId, username, nas.Name);
                break;
            }

            case 2:
            {
                var session = await sessionRepository.GetBySessionIdAsync(sessionId ?? string.Empty, ct);
                if (session is not null)
                {
                    session.Stop();
                    await sessionRepository.UpdateAsync(session, ct);
                    _logger.LogInformation("RADIUS session stopped: {SessionId} ({Time}s, {In} in / {Out} out)",
                        sessionId, acctSessionTime, inputOctets, outputOctets);
                }
                break;
            }

            case 3:
            {
                var session = await sessionRepository.GetBySessionIdAsync(sessionId ?? string.Empty, ct);
                if (session is not null)
                {
                    session.RecordInterim(inputOctets, outputOctets, acctSessionTime);
                    await sessionRepository.UpdateAsync(session, ct);
                }
                break;
            }

            case 7:
                _logger.LogInformation("RADIUS Accounting-On from NAS {Name}", nas.Name);
                break;

            case 8:
                _logger.LogInformation("RADIUS Accounting-Off from NAS {Name}", nas.Name);
                break;
        }
    }
}
