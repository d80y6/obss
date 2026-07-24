using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Radius;

public sealed class RadiusAdapter : IAaaAdapter, IDisposable
{
    private readonly ILogger<RadiusAdapter> _logger;
    private readonly INasRepository _nasRepository;
    private readonly Socket _socket;
    private byte _nextIdentifier;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
    private static readonly int DefaultRetries = 3;

    public RadiusAdapter(ILogger<RadiusAdapter> logger, INasRepository nasRepository)
    {
        _logger = logger;
        _nasRepository = nasRepository;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
    }

    public string AdapterName => "RealRadius";
    public AaaProtocolType ProtocolType => AaaProtocolType.Radius;

    public async Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        var nas = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (nas is null)
            return new AaaAuthResult(false, null, null, $"NAS {request.NasIp} not found");

        var packet = CreatePacket(RadiusCode.AccessRequest);
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes(request.Username)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserPassword, Encoding.UTF8.GetBytes(request.Password)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse(nas.NasIpAddress).GetAddressBytes()));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.ServiceType, PackUint((uint)request.ServiceType)));

        if (!string.IsNullOrEmpty(request.CalledStationId))
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.CalledStationId, Encoding.UTF8.GetBytes(request.CalledStationId)));
        if (!string.IsNullOrEmpty(request.CallingStationId))
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.CallingStationId, Encoding.UTF8.GetBytes(request.CallingStationId)));

        var response = await SendAndReceiveAsync(packet, nas.NasIpAddress, 1812, nas.NasSecret, ct);

        return response.Code switch
        {
            RadiusCode.AccessAccept => new AaaAuthResult(
                true,
                Guid.NewGuid().ToString("N"),
                response.GetIpAttribute(RadiusAttributeType.FramedIpAddress)?.ToString(),
                null),
            RadiusCode.AccessChallenge => new AaaAuthResult(
                false, null, null, "Access-Challenge received"),
            _ => new AaaAuthResult(
                false, null, null,
                response.GetStringAttribute(RadiusAttributeType.ReplyMessage) ?? "Access-Rejected"),
        };
    }

    public async Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        var nas = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (nas is null)
            return new AaaAcctResult(false, request.SessionId, $"NAS {request.NasIp} not found");

        var acctStatusType = request.AcctStatusType.ToUpperInvariant() switch
        {
            "START" => 1,
            "STOP" => 2,
            "INTERIM-UPDATE" => 3,
            "ACCOUNTING-ON" => 7,
            "ACCOUNTING-OFF" => 8,
            _ => 1
        };

        var packet = CreatePacket(RadiusCode.AccountingRequest);
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctStatusType, PackUint((uint)acctStatusType)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctSessionId, Encoding.UTF8.GetBytes(request.SessionId)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes(request.Username)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse(nas.NasIpAddress).GetAddressBytes()));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctSessionTime, PackUint((uint)request.AcctSessionTime)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctInputOctets, PackUint((uint)request.InputOctets)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctOutputOctets, PackUint((uint)request.OutputOctets)));

        if (!string.IsNullOrEmpty(request.CalledStationId))
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.CalledStationId, Encoding.UTF8.GetBytes(request.CalledStationId)));
        if (!string.IsNullOrEmpty(request.CallingStationId))
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.CallingStationId, Encoding.UTF8.GetBytes(request.CallingStationId)));

        var response = await SendAndReceiveAsync(packet, nas.NasIpAddress, 1813, nas.NasSecret, ct);

        return response.Code switch
        {
            RadiusCode.AccountingResponse => new AaaAcctResult(true, request.SessionId, null),
            _ => new AaaAcctResult(false, request.SessionId, $"Unexpected response code: {response.Code}")
        };
    }

    public async Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        var nas = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (nas is null)
            return new AaaCoAResult(false, $"NAS {request.NasIp} not found");

        var packet = CreatePacket(RadiusCode.CoARequest);
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes(request.Username)));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse(nas.NasIpAddress).GetAddressBytes()));
        packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.AcctSessionId, Encoding.UTF8.GetBytes(request.SessionId)));

        foreach (var (key, value) in request.Attributes)
        {
            if (TryParseAttribute(key, value, out var attr))
                packet.Attributes.Add(attr);
        }

        var response = await SendAndReceiveAsync(packet, nas.NasIpAddress, 3799, nas.NasSecret, ct);

        return response.Code switch
        {
            RadiusCode.CoAACK => new AaaCoAResult(true, null),
            RadiusCode.CoANAK => new AaaCoAResult(false,
                $"CoA-NAK: Error-Cause={response.GetUintAttribute(RadiusAttributeType.ErrorCause)}"),
            _ => new AaaCoAResult(false, $"Unexpected response: {response.Code}")
        };
    }

    public async Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        try
        {
            var packet = CreatePacket(RadiusCode.AccessRequest);
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserName, Encoding.UTF8.GetBytes("healthcheck")));
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.UserPassword, Encoding.UTF8.GetBytes("healthcheck")));
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.NasIpAddress, IPAddress.Parse(nas.NasIpAddress).GetAddressBytes()));
            packet.Attributes.Add(new RadiusAttribute(RadiusAttributeType.ServiceType, PackUint(1)));

            await SendAndReceiveAsync(packet, nas.NasIpAddress, 1812, nas.NasSecret, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RADIUS connection test failed for NAS {Name} at {Ip}", nas.Name, nas.NasIpAddress);
            return false;
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
    }

    private RadiusPacket CreatePacket(RadiusCode code)
    {
        return new RadiusPacket
        {
            Code = code,
            Identifier = _nextIdentifier++,
            Authenticator = new byte[16],
        };
    }

    private async Task<RadiusPacket> SendAndReceiveAsync(
        RadiusPacket request, string nasIp, int port, string secret, CancellationToken ct)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(nasIp), port);
        var packetBytes = request.Encode(secret);

        RadiusException? lastError = null;

        for (var attempt = 0; attempt < DefaultRetries; attempt++)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                await _socket.SendToAsync(packetBytes, SocketFlags.None, endpoint, ct);

                var buffer = new byte[4096];
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(DefaultTimeout);
                var result = await _socket.ReceiveFromAsync(buffer, SocketFlags.None, endpoint, timeoutCts.Token);
                var responseData = buffer[..result.ReceivedBytes];

                var response = RadiusPacket.Parse(responseData, secret, request.Authenticator);

                if (response.Identifier != request.Identifier)
                {
                    _logger.LogWarning("RADIUS identifier mismatch: expected {Expected}, got {Actual}",
                        request.Identifier, response.Identifier);
                    continue;
                }

                return response;
            }
            catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
            {
                lastError = new RadiusException($"RADIUS timeout after attempt {attempt + 1}/{DefaultRetries}", ex);
                _logger.LogWarning(ex, "RADIUS timeout for NAS {NasIp}, attempt {Attempt}/{MaxRetries}", nasIp, attempt + 1, DefaultRetries);
            }
            catch (SocketException ex) when (ex.SocketErrorCode is SocketError.TimedOut or SocketError.WouldBlock)
            {
                lastError = new RadiusException($"RADIUS timeout after attempt {attempt + 1}/{DefaultRetries}", ex);
                _logger.LogWarning(ex, "RADIUS timeout for NAS {NasIp}, attempt {Attempt}/{MaxRetries}", nasIp, attempt + 1, DefaultRetries);
            }
        }

        throw lastError ?? new RadiusException("RADIUS communication failed");
    }

    private static byte[] PackUint(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return bytes;
    }

    private static bool TryParseAttribute(string key, string value, out RadiusAttribute attr)
    {
        attr = default!;
        try
        {
            var type = key.ToUpperInvariant() switch
            {
                "SESSION-TIMEOUT" => RadiusAttributeType.SessionTimeout,
                "IDLE-TIMEOUT" => RadiusAttributeType.IdleTimeout,
                "FILTER-ID" => RadiusAttributeType.FilterId,
                "FRAMED-IP-ADDRESS" => RadiusAttributeType.FramedIpAddress,
                "FRAMED-ROUTE" => RadiusAttributeType.FramedRoute,
                "REPLY-MESSAGE" => RadiusAttributeType.ReplyMessage,
                _ => (RadiusAttributeType?)null
            };

            if (type is null)
                return false;

            attr = type.Value switch
            {
                RadiusAttributeType.SessionTimeout or RadiusAttributeType.IdleTimeout
                    => new RadiusAttribute(type.Value, PackUint(uint.Parse(value))),
                RadiusAttributeType.FramedIpAddress
                    => new RadiusAttribute(type.Value, IPAddress.Parse(value).GetAddressBytes()),
                _ => new RadiusAttribute(type.Value, Encoding.UTF8.GetBytes(value))
            };

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class RadiusException : Exception
{
    public RadiusException() { }
    public RadiusException(string message) : base(message) { }
    public RadiusException(string message, Exception inner) : base(message, inner) { }
}
