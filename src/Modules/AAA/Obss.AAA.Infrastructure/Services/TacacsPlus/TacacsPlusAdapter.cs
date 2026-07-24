using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Infrastructure.Services.TacacsPlus;

public sealed class TacacsPlusAdapter : IAaaAdapter, IDisposable
{
    private readonly ILogger<TacacsPlusAdapter> _logger;
    private readonly INasRepository _nasRepository;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private uint _sessionId;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public string AdapterName => "RealTacacsPlus";
    public AaaProtocolType ProtocolType => AaaProtocolType.TacacsPlus;

    public TacacsPlusAdapter(ILogger<TacacsPlusAdapter> logger, INasRepository nasRepository)
    {
        _logger = logger;
        _nasRepository = nasRepository;
    }

    public async Task<AaaAuthResult> AuthenticateAsync(AaaAuthRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+ authenticating admin {Username}", request.Username);

        var server = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (server is null)
            return new AaaAuthResult(false, null, null, $"TACACS+ server {request.NasIp} not found");

        try
        {
            await ConnectAsync(server.NasIpAddress, ct);

            var usernameBytes = Encoding.UTF8.GetBytes(request.Username);
            var portBytes = Encoding.UTF8.GetBytes("tty1");
            var remAddrBytes = Encoding.UTF8.GetBytes(request.NasIp);

            var authStart = new TacacsAuthStart
            {
                Action = TacacsAuthAction.Login,
                AuthType = TacacsAuthType.Ascii,
                PrivLvl = 15,
                Service = 1,
                UserLen = (byte)usernameBytes.Length,
                PortLen = (byte)portBytes.Length,
                RemAddrLen = (byte)remAddrBytes.Length,
                User = request.Username,
                Port = "tty1",
                RemAddr = request.NasIp,
            };

            await SendPacketAsync(TacacsPacketType.Authentication, 1, authStart.Encode(), server.NasSecret, ct);
            var replyHeader = await ReceiveHeaderAsync(ct);
            var replyBody = await ReceiveBodyAsync(replyHeader, server.NasSecret, ct);
            var reply = TacacsAuthReply.Decode(replyBody);

            if (reply.Status == TacacsAuthStatus.GetPassword)
            {
                var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
                var continuePacket = new TacacsAuthContinue
                {
                    UserMsgLen = 0,
                    UserDataLen = (byte)passwordBytes.Length,
                    UserData = passwordBytes,
                };

                await SendPacketAsync(TacacsPacketType.Authentication, 3, continuePacket.Encode(), server.NasSecret, ct);
                replyHeader = await ReceiveHeaderAsync(ct);
                replyBody = await ReceiveBodyAsync(replyHeader, server.NasSecret, ct);
                reply = TacacsAuthReply.Decode(replyBody);
            }

            return reply.Status switch
            {
                TacacsAuthStatus.Pass => new AaaAuthResult(true, Guid.NewGuid().ToString("N"), null, null),
                TacacsAuthStatus.Fail => new AaaAuthResult(false, null, null, reply.ServerMsg ?? "Authentication failed"),
                TacacsAuthStatus.Error => new AaaAuthResult(false, null, null, reply.ServerMsg ?? "TACACS+ server error"),
                _ => new AaaAuthResult(false, null, null, $"TACACS+ returned status {reply.Status}")
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "TACACS+ authentication failed for {Username}", request.Username);
            return new AaaAuthResult(false, null, null, ex.Message);
        }
    }

    public async Task<AaaAcctResult> AccountingAsync(AaaAcctRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+ accounting for session {SessionId}", request.SessionId);

        var server = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (server is null)
            return new AaaAcctResult(false, request.SessionId, $"TACACS+ server {request.NasIp} not found");

        try
        {
            await ConnectAsync(server.NasIpAddress, ct);

            var body = BuildAcctBody(request);
            await SendPacketAsync(TacacsPacketType.Accounting, 1, body, server.NasSecret, ct);
            var header = await ReceiveHeaderAsync(ct);
            var replyBody = await ReceiveBodyAsync(header, server.NasSecret, ct);

            var status = replyBody.Length > 0 ? replyBody[0] : (byte)TacacsAcctStatus.Error;
            return status == (byte)TacacsAcctStatus.Success
                ? new AaaAcctResult(true, request.SessionId, null)
                : new AaaAcctResult(false, request.SessionId, "TACACS+ accounting was not successful");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "TACACS+ accounting failed for session {SessionId}", request.SessionId);
            return new AaaAcctResult(false, request.SessionId, ex.Message);
        }
    }

    public async Task<AaaCoAResult> ChangeOfAuthorizationAsync(AaaCoARequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("TACACS+ authorization for session {SessionId}", request.SessionId);

        var server = await _nasRepository.GetByIpAddressAsync(request.NasIp, ct);
        if (server is null)
            return new AaaCoAResult(false, $"TACACS+ server {request.NasIp} not found");

        try
        {
            await ConnectAsync(server.NasIpAddress, ct);

            var body = BuildAuthorBody(request);
            await SendPacketAsync(TacacsPacketType.Authorization, 1, body, server.NasSecret, ct);
            var header = await ReceiveHeaderAsync(ct);
            var replyBody = await ReceiveBodyAsync(header, server.NasSecret, ct);

            var status = replyBody.Length > 1 ? replyBody[1] : (byte)TacacsAuthorStatus.Error;
            return status switch
            {
                (byte)TacacsAuthorStatus.PassAdd or (byte)TacacsAuthorStatus.PassReply
                    => new AaaCoAResult(true, null),
                (byte)TacacsAuthorStatus.Fail => new AaaCoAResult(false, "Authorization denied"),
                _ => new AaaCoAResult(false, $"Authorization returned status {status}")
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "TACACS+ authorization failed for session {SessionId}", request.SessionId);
            return new AaaCoAResult(false, ex.Message);
        }
    }

    public async Task<bool> TestConnectionAsync(NetworkAccessServer nas, CancellationToken ct = default)
    {
        try
        {
            await ConnectAsync(nas.NasIpAddress, ct);
            return _tcpClient?.Connected == true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _connectionLock?.Dispose();
    }

    private async Task ConnectAsync(string host, CancellationToken ct)
    {
        if (_tcpClient?.Connected == true)
            return;

        await _connectionLock.WaitAsync(ct);
        try
        {
            if (_tcpClient?.Connected == true)
                return;

            _tcpClient?.Dispose();
            _tcpClient = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(DefaultTimeout);
            await _tcpClient.ConnectAsync(host, 49, timeoutCts.Token);
            _stream = _tcpClient.GetStream();
            _sessionId = (uint)(Random.Shared.NextInt64() & 0xFFFFFFFF);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task SendPacketAsync(TacacsPacketType type, byte seqNo, byte[] body, string secret, CancellationToken ct)
    {
        var header = new TacacsHeader
        {
            Version = 0xC0,
            PacketType = type,
            SeqNo = seqNo,
            SessionId = _sessionId,
            Length = body.Length,
            Flags = TacacsFlag.None,
        };

        var encryptedBody = TacacsCrypto.Encrypt(body, secret, header);
        var headerBytes = header.Encode();
        var packet = new byte[headerBytes.Length + encryptedBody.Length];
        headerBytes.CopyTo(packet, 0);
        encryptedBody.CopyTo(packet, 12);

        await _stream!.WriteAsync(packet, ct);
        await _stream.FlushAsync(ct);
    }

    private async Task<TacacsHeader> ReceiveHeaderAsync(CancellationToken ct)
    {
        var buf = new byte[12];
        var total = 0;
        while (total < 12)
        {
            var read = await _stream!.ReadAsync(buf.AsMemory(total, 12 - total), ct);
            if (read == 0) throw new EndOfStreamException("TACACS+ connection closed");
            total += read;
        }
        return TacacsHeader.Decode(buf);
    }

    private async Task<byte[]> ReceiveBodyAsync(TacacsHeader header, string secret, CancellationToken ct)
    {
        var body = new byte[header.Length];
        var total = 0;
        while (total < header.Length)
        {
            var read = await _stream!.ReadAsync(body.AsMemory(total, header.Length - total), ct);
            if (read == 0) throw new EndOfStreamException("TACACS+ connection closed");
            total += read;
        }
        return TacacsCrypto.Decrypt(body, secret, header);
    }

    private static byte[] BuildAcctBody(AaaAcctRequest request)
    {
        var usernameBytes = Encoding.UTF8.GetBytes(request.Username);
        var sessionBytes = Encoding.UTF8.GetBytes(request.SessionId);
        var taskBytes = Encoding.UTF8.GetBytes("network-services");

        var body = new byte[8 + usernameBytes.Length + sessionBytes.Length + taskBytes.Length];
        body[0] = 2;
        body[1] = (byte)usernameBytes.Length;
        body[2] = 0;
        body[3] = (byte)sessionBytes.Length;
        body[4] = (byte)taskBytes.Length;
        body[5] = 0;
        body[6] = 0;
        body[7] = 0;

        usernameBytes.CopyTo(body, 8);
        sessionBytes.CopyTo(body, 8 + usernameBytes.Length);
        taskBytes.CopyTo(body, 8 + usernameBytes.Length + sessionBytes.Length);

        return body;
    }

    private static byte[] BuildAuthorBody(AaaCoARequest request)
    {
        var usernameBytes = Encoding.UTF8.GetBytes(request.Username);
        var portBytes = Encoding.UTF8.GetBytes("tty1");
        var remAddrBytes = Encoding.UTF8.GetBytes(request.NasIp);
        var argCount = (byte)Math.Min(request.Attributes.Count, 255);

        var argPairs = request.Attributes
            .Take(argCount)
            .Select(kv => (Key: Encoding.UTF8.GetBytes(kv.Key), Value: Encoding.UTF8.GetBytes(kv.Value)))
            .ToList();

        var body = new byte[8 + usernameBytes.Length + portBytes.Length + remAddrBytes.Length
            + argPairs.Sum(p => 2 + p.Key.Length + p.Value.Length)];

        body[0] = (byte)usernameBytes.Length;
        body[1] = (byte)portBytes.Length;
        body[2] = (byte)remAddrBytes.Length;
        body[3] = argCount;

        var offset = 4;
        usernameBytes.CopyTo(body, offset);
        offset += usernameBytes.Length;
        portBytes.CopyTo(body, offset);
        offset += portBytes.Length;
        remAddrBytes.CopyTo(body, offset);
        offset += remAddrBytes.Length;

        body[offset++] = 0;
        body[offset++] = 0;
        body[offset++] = 0;
        body[offset++] = 0;

        foreach (var (key, value) in argPairs)
        {
            body[offset++] = (byte)key.Length;
            body[offset++] = (byte)value.Length;
            key.CopyTo(body, offset);
            offset += key.Length;
            value.CopyTo(body, offset);
            offset += value.Length;
        }

        return body;
    }
}
