using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.Diameter;

public sealed class DiameterAdapter : IAaaAdapter, IDisposable
{
    private readonly ILogger<DiameterAdapter> _logger;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private readonly ConcurrentDictionary<uint, TaskCompletionSource<DiameterMessage>> _pendingRequests = new();
    private uint _nextHopByHopId = 1;
    private uint _nextEndToEndId = 1;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private string? _connectedHost;
    private int _connectedPort;
    private bool _connected;
    private CancellationTokenSource? _receiveCts;

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private const string DefaultOriginHost = "obss.aaa.local";
    private const string DefaultOriginRealm = "aaa.local";
    private const string DefaultServiceContext = "service:obss.aaa@3gpp.org";

    public string AdapterName => "RealDiameter";
    public AaaProtocolType ProtocolType => AaaProtocolType.Diameter;

    public DiameterAdapter(ILogger<DiameterAdapter> logger)
    {
        _logger = logger;
    }

    public async Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter Credit-Control authentication for user {Username}", request.Username);

        var sessionId = $"obss-{Guid.NewGuid():N}";

        var ccr = DiameterMessage.CreateCreditControlRequest(
            1, 1, sessionId, DefaultOriginHost, DefaultOriginRealm, DefaultOriginRealm, null,
            DefaultServiceContext, _nextHopByHopId++, _nextEndToEndId++);

        var subIdData = new byte[4 + request.Username.Length];
        subIdData[3] = 1;
        Encoding.UTF8.GetBytes(request.Username).CopyTo(subIdData, 4);
        ccr.Avps.Add(new DiameterAvp(AvpCode.SubscriptionId, subIdData));

        try
        {
            var response = await SendRequestAsync(ccr, ct);

            var resultCode = response.GetUint32Avp(AvpCode.ResultCode);
            if (resultCode == (uint)DiameterResultCode.Success)
            {
                _logger.LogInformation("Diameter auth succeeded for {Username}", request.Username);
                return new AaaAuthResult(true, sessionId, request.NasIp, null);
            }

            return new AaaAuthResult(false, null, null,
                $"Diameter Credit-Control failed: Result-Code={resultCode}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Diameter Credit-Control authentication failed for {Username}", request.Username);
            return new AaaAuthResult(false, null, null, ex.Message);
        }
    }

    public async Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter Credit-Control accounting for session {SessionId}", request.SessionId);

        var ccRequestType = request.AcctStatusType.ToUpperInvariant() switch
        {
            "START" => 1u,
            "STOP" => 3u,
            "INTERIM-UPDATE" => 2u,
            _ => 4u
        };

        var ccr = DiameterMessage.CreateCreditControlRequest(
            ccRequestType, 1, request.SessionId, DefaultOriginHost, DefaultOriginRealm, DefaultOriginRealm, null,
            DefaultServiceContext, _nextHopByHopId++, _nextEndToEndId++);

        if (request.InputOctets > 0)
        {
            ccr.Avps.Add(new DiameterAvp(AvpCode.UsedServiceUnit, PackUsedServiceUnit(request.InputOctets)));
        }

        try
        {
            var response = await SendRequestAsync(ccr, ct);

            var resultCode = response.GetUint32Avp(AvpCode.ResultCode);
            return resultCode switch
            {
                (uint)DiameterResultCode.Success or (uint)DiameterResultCode.LimitedSuccess
                    => new AaaAcctResult(true, request.SessionId, null),
                _ => new AaaAcctResult(false, request.SessionId,
                    $"Diameter accounting failed: Result-Code={resultCode}")
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Diameter accounting failed for session {SessionId}", request.SessionId);
            return new AaaAcctResult(false, request.SessionId, ex.Message);
        }
    }

    public async Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Diameter Re-Auth for session {SessionId} with {Count} attributes",
            request.SessionId, request.Attributes.Count);

        var ccr = DiameterMessage.CreateCreditControlRequest(
            2, 1, request.SessionId, DefaultOriginHost, DefaultOriginRealm, DefaultOriginRealm, null,
            DefaultServiceContext, _nextHopByHopId++, _nextEndToEndId++);

        if (request.Attributes.TryGetValue("QUOTA", out var quota) && long.TryParse(quota, out var quotaBytes))
        {
            ccr.Avps.Add(new DiameterAvp(AvpCode.RequestedServiceUnit, PackRequestedServiceUnit(quotaBytes)));
        }

        if (request.Attributes.TryGetValue("RATING-GROUP", out var ratingGroup) && uint.TryParse(ratingGroup, out var rg))
        {
            ccr.Avps.Add(new DiameterAvp(AvpCode.RatingGroup, DiameterMessage.PackUint32(rg)));
        }

        try
        {
            var response = await SendRequestAsync(ccr, ct);

            var resultCode = response.GetUint32Avp(AvpCode.ResultCode);
            return resultCode switch
            {
                (uint)DiameterResultCode.Success => new AaaCoAResult(true, null),
                _ => new AaaCoAResult(false, $"CoA failed: Result-Code={resultCode}")
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Diameter CoA failed for session {SessionId}", request.SessionId);
            return new AaaCoAResult(false, ex.Message);
        }
    }

    public async Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        try
        {
            await EnsureConnectedAsync(nas.NasIpAddress, 3868, ct);
            return _connected;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_receiveCts is not null)
        {
            _receiveCts.Cancel(throwOnFirstException: false);
            _receiveCts.Dispose();
        }
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _connectionLock?.Dispose();
    }

    private async Task<DiameterMessage> SendRequestAsync(DiameterMessage request, CancellationToken ct)
    {
        await EnsureConnectedAsync(_connectedHost ?? "127.0.0.1", _connectedPort > 0 ? _connectedPort : 3868, ct);

        var hbh = request.Header.HopByHopId;
        var tcs = new TaskCompletionSource<DiameterMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[hbh] = tcs;

        try
        {
            var data = request.Encode();
            await _stream!.WriteAsync(data, ct);
            await _stream.FlushAsync(ct);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(DefaultTimeout);

            return await tcs.Task.WaitAsync(timeoutCts.Token);
        }
        finally
        {
            _pendingRequests.TryRemove(hbh, out _);
        }
    }

    private async Task EnsureConnectedAsync(string host, int port, CancellationToken ct)
    {
        if (_connected && _tcpClient?.Connected == true)
            return;

        await _connectionLock.WaitAsync(ct);
        try
        {
            if (_connected && _tcpClient?.Connected == true)
                return;

            _connectedHost = host;
            _connectedPort = port;

            _tcpClient?.Dispose();
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, ct);
            _stream = _tcpClient.GetStream();

            if (_receiveCts is not null)
            {
                await _receiveCts.CancelAsync();
                _receiveCts.Dispose();
            }
            _receiveCts = new CancellationTokenSource();
            _ = ReceiveLoopAsync(_receiveCts.Token);

            await ExchangeCapabilitiesAsync(ct);

            _connected = true;
            _logger.LogInformation("Diameter connected to {Host}:{Port}", host, port);
        }
        catch
        {
            _connected = false;
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ExchangeCapabilitiesAsync(CancellationToken ct)
    {
        var cer = new DiameterMessage
        {
            Header = new DiameterHeader
            {
                Version = 1,
                Flags = DiameterFlag.Request,
                CommandCode = DiameterCommandCode.CapabilitiesExchange,
                ApplicationId = DiameterApplicationId.DiameterCommon,
                HopByHopId = _nextHopByHopId++,
                EndToEndId = _nextEndToEndId++,
            }
        };

        cer.Avps.Add(new DiameterAvp(AvpCode.OriginHost, Encoding.UTF8.GetBytes(DefaultOriginHost)));
        cer.Avps.Add(new DiameterAvp(AvpCode.OriginRealm, Encoding.UTF8.GetBytes(DefaultOriginRealm)));
        cer.Avps.Add(new DiameterAvp(AvpCode.ProductName, Encoding.UTF8.GetBytes("OBSS AAA Module")));

        var vendorAppId = new byte[16];
        Buffer.BlockCopy(DiameterMessage.PackUint32(0), 0, vendorAppId, 0, 4);
        Buffer.BlockCopy(DiameterMessage.PackUint32(0), 0, vendorAppId, 4, 4);
        Buffer.BlockCopy(DiameterMessage.PackUint32((uint)DiameterApplicationId.CreditControl), 0, vendorAppId, 8, 4);
        Buffer.BlockCopy(DiameterMessage.PackUint32(0), 0, vendorAppId, 12, 4);

        cer.Avps.Add(new DiameterAvp(AvpCode.VendorSpecificApplicationId, vendorAppId));

        var hbh = cer.Header.HopByHopId;
        var tcs = new TaskCompletionSource<DiameterMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[hbh] = tcs;

        try
        {
            var data = cer.Encode();
            await _stream!.WriteAsync(data, ct);
            await _stream.FlushAsync(ct);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

            var cea = await tcs.Task.WaitAsync(timeoutCts.Token);
            var resultCode = cea.GetUint32Avp(AvpCode.ResultCode);

            if (resultCode != (uint)DiameterResultCode.Success)
            {
                _logger.LogWarning("Diameter CER/CEA failed with result code {ResultCode}", resultCode);
            }
        }
        finally
        {
            _pendingRequests.TryRemove(hbh, out _);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        var offset = 0;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var read = await _stream!.ReadAsync(buffer.AsMemory(offset), ct);
                if (read == 0)
                {
                    _logger.LogWarning("Diameter connection closed by peer");
                    _connected = false;
                    break;
                }

                offset += read;

                while (offset >= 20)
                {
                    var msgLen = (buffer[2] << 16) | (buffer[3] << 8) | buffer[4];
                    if (msgLen > buffer.Length)
                    {
                        _logger.LogError("Diameter message too large: {Length}", msgLen);
                        offset = 0;
                        break;
                    }

                    if (offset < msgLen)
                        break;

                    var msgData = new byte[msgLen];
                    Buffer.BlockCopy(buffer, 0, msgData, 0, msgLen);
                    offset -= msgLen;
                    if (offset > 0)
                        Buffer.BlockCopy(buffer, msgLen, buffer, 0, offset);

                    var message = DiameterMessage.Decode(msgData);
                    var hbh = message.Header.HopByHopId;

                    if (message.Header.CommandCode == DiameterCommandCode.DeviceWatchdog)
                    {
                        await SendDwaAsync(message.Header.HopByHopId, message.Header.EndToEndId, ct);
                        continue;
                    }

                    if (_pendingRequests.TryRemove(hbh, out var tcs))
                    {
                        tcs.TrySetResult(message);
                    }
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Diameter receive loop cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Diameter receive loop error");
            _connected = false;

            foreach (var (_, pending) in _pendingRequests)
                pending.TrySetException(new DiameterTransportException("Connection lost", ex));
            _pendingRequests.Clear();
        }
    }

    private async Task SendDwaAsync(uint hopByHopId, uint endToEndId, CancellationToken ct)
    {
        var dwa = new DiameterMessage
        {
            Header = new DiameterHeader
            {
                Version = 1,
                Flags = DiameterFlag.None,
                CommandCode = DiameterCommandCode.DeviceWatchdog,
                ApplicationId = DiameterApplicationId.DiameterCommon,
                HopByHopId = hopByHopId,
                EndToEndId = endToEndId,
            }
        };

        dwa.Avps.Add(new DiameterAvp(AvpCode.OriginHost, Encoding.UTF8.GetBytes(DefaultOriginHost)));
        dwa.Avps.Add(new DiameterAvp(AvpCode.OriginRealm, Encoding.UTF8.GetBytes(DefaultOriginRealm)));

        var data = dwa.Encode();
        await _stream!.WriteAsync(data, ct);
        await _stream.FlushAsync(ct);
    }

    private static byte[] PackUsedServiceUnit(long octets)
    {
        return
        [
            (byte)(octets >> 24),
            (byte)(octets >> 16),
            (byte)(octets >> 8),
            (byte)octets,
            0, 0, 0, 0,
            0, 0, 0, 0,
        ];
    }

    private static byte[] PackRequestedServiceUnit(long quotaBytes)
    {
        var data = new byte[12];
        data[0] = (byte)(quotaBytes >> 24);
        data[1] = (byte)(quotaBytes >> 16);
        data[2] = (byte)(quotaBytes >> 8);
        data[3] = (byte)quotaBytes;
        return data;
    }
}

public sealed class DiameterTransportException : Exception
{
    public DiameterTransportException() { }
    public DiameterTransportException(string message) : base(message) { }
    public DiameterTransportException(string message, Exception inner) : base(message, inner) { }
}
