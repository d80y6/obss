using System.Diagnostics;
using System.Net;
using System.Text;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Snmp;

public sealed class SnmpTransport : ISnmpTransport
{
    private readonly SnmpTransportConfig _config;
    private readonly ILogger<SnmpTransport> _logger;
    private readonly IPEndPoint _endpoint;
    private const int DefaultMaxMessageSize = 65535;

    public SnmpTransport(SnmpTransportConfig config, ILogger<SnmpTransport> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _endpoint = new IPEndPoint(ResolveHost(config.Host), config.Port);
    }

    public TransportProtocol Protocol => _config.Protocol;
    public ITransportConfig Config => _config;

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await GetAsync("1.3.6.1.2.1.1.1.0", cancellationToken);
            sw.Stop();

            if (result.Success)
            {
                return TransportConnectionResult.Ok(
                    $"SNMP {_config.SnmpVersion} device at {_config.Host}:{_config.Port}",
                    sw.Elapsed);
            }

            return TransportConnectionResult.Fail(result.ErrorMessage ?? "SNMP connection test failed", sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "SNMP connection test failed for {Host}:{Port}", _config.Host, _config.Port);
            return TransportConnectionResult.Fail(ex.Message, sw.Elapsed);
        }
    }

    public async Task<TransportResult> GetAsync(string oid, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var variable = new Variable(new ObjectIdentifier(oid));

            if (_config.SnmpVersion == SnmpVersion.V3)
            {
                var response = await SendV3RequestAsync(
                    (msgId, reqId, userName, contextName, privacy) =>
                        new GetRequestMessage(
                            VersionCode.V3, msgId, reqId, userName, contextName,
                            [variable], privacy, DefaultMaxMessageSize, null!),
                    cancellationToken);

                sw.Stop();
                if (response.Pdu().ErrorStatus.ToErrorCode() != ErrorCode.NoError)
                {
                    return TransportResult.Fail(
                        $"SNMP GET failed: {response.Pdu().ErrorStatus}",
                        sw.Elapsed, Protocol);
                }

                var data = response.Variables().FirstOrDefault()?.Data?.ToString();
                return TransportResult.Ok(data, sw.Elapsed, Protocol);
            }

            var communityResult = await Messenger.GetAsync(
                _config.SnmpVersion switch
                {
                    SnmpVersion.V1 => VersionCode.V1,
                    _ => VersionCode.V2
                },
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var v2Data = communityResult.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(v2Data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "SNMP GET failed for OID {Oid} on {Host}", oid, _config.Host);
            return TransportResult.Fail($"SNMP GET failed: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    public async Task<TransportResult> GetBulkAsync(IEnumerable<string> oids, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var oidList = oids.ToList();
            if (oidList.Count == 0)
                return TransportResult.Ok(string.Empty, sw.Elapsed, Protocol);

            var variables = oidList.Select(o => new Variable(new ObjectIdentifier(o))).ToList();

            if (_config.SnmpVersion == SnmpVersion.V3)
            {
                var response = await SendV3RequestAsync(
                    (msgId, reqId, userName, contextName, privacy) =>
                        new GetBulkRequestMessage(
                            VersionCode.V3, msgId, reqId, userName, contextName,
                            0, 10, variables,
                            privacy, DefaultMaxMessageSize, null!),
                    cancellationToken);

                sw.Stop();
                if (response.Pdu().ErrorStatus.ToErrorCode() != ErrorCode.NoError)
                {
                    return TransportResult.Fail(
                        $"SNMP GETBULK failed: {response.Pdu().ErrorStatus}",
                        sw.Elapsed, Protocol);
                }

                var data = string.Join(Environment.NewLine,
                    response.Variables().Select(v => $"{v.Id}={v.Data}"));
                return TransportResult.Ok(data, sw.Elapsed, Protocol);
            }

            var communityResult = await Messenger.GetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                variables,
                cancellationToken);

            sw.Stop();
            var v2Data = string.Join(Environment.NewLine,
                communityResult.Select(v => $"{v.Id}={v.Data}"));
            return TransportResult.Ok(v2Data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SNMP GETBULK failed: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    public async Task<TransportResult> WalkAsync(string rootOid, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var root = new ObjectIdentifier(rootOid);
            var results = new List<Variable>();

            if (_config.SnmpVersion == SnmpVersion.V3)
            {
                var (userName, privacy, _) = CreateV3Context();

                await Messenger.BulkWalkAsync(
                    VersionCode.V3,
                    _endpoint,
                    userName,
                    new OctetString(_config.V3ContextName ?? string.Empty),
                    root,
                    results,
                    10,
                    WalkMode.WithinSubtree,
                    privacy,
                    null!,
                    cancellationToken);

                sw.Stop();
                var data = string.Join(Environment.NewLine,
                    results.Select(v => $"{v.Id}={v.Data}"));
                return TransportResult.Ok(data, sw.Elapsed, Protocol);
            }

            await Messenger.WalkAsync(
                _config.SnmpVersion switch
                {
                    SnmpVersion.V1 => VersionCode.V1,
                    _ => VersionCode.V2
                },
                _endpoint,
                new OctetString(_config.Community),
                root,
                results,
                WalkMode.WithinSubtree,
                cancellationToken);

            sw.Stop();
            var v2Data = string.Join(Environment.NewLine,
                results.Select(v => $"{v.Id}={v.Data}"));
            return TransportResult.Ok(v2Data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SNMP WALK failed for {rootOid}: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    public async Task<TransportResult> SetAsync(string oid, string data, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var variable = new Variable(new ObjectIdentifier(oid), new OctetString(data));

            if (_config.SnmpVersion == SnmpVersion.V3)
            {
                var response = await SendV3RequestAsync(
                    (msgId, reqId, userName, contextName, privacy) =>
                        new SetRequestMessage(
                            VersionCode.V3, msgId, reqId, userName, contextName,
                            [variable], privacy, DefaultMaxMessageSize, null!),
                    cancellationToken);

                sw.Stop();
                if (response.Pdu().ErrorStatus.ToErrorCode() != ErrorCode.NoError)
                {
                    return TransportResult.Fail(
                        $"SNMP SET failed: {response.Pdu().ErrorStatus}",
                        sw.Elapsed, Protocol);
                }

                var resultData = response.Variables().FirstOrDefault()?.Data?.ToString();
                return TransportResult.Ok(resultData, sw.Elapsed, Protocol);
            }

            var communityResult = await Messenger.SetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var v2Data = communityResult.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(v2Data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SNMP SET failed for {oid}: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    public async Task<TransportResult> GetNextAsync(string oid, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var variable = new Variable(new ObjectIdentifier(oid));

            if (_config.SnmpVersion == SnmpVersion.V3)
            {
                var response = await SendV3RequestAsync(
                    (msgId, reqId, userName, contextName, privacy) =>
                        new GetNextRequestMessage(
                            VersionCode.V3, msgId, reqId, userName, contextName,
                            [variable], privacy, DefaultMaxMessageSize, null!),
                    cancellationToken);

                sw.Stop();
                if (response.Pdu().ErrorStatus.ToErrorCode() != ErrorCode.NoError)
                {
                    return TransportResult.Fail(
                        $"SNMP GETNEXT failed: {response.Pdu().ErrorStatus}",
                        sw.Elapsed, Protocol);
                }

                var data = response.Variables().FirstOrDefault()?.Data?.ToString();
                return TransportResult.Ok(data, sw.Elapsed, Protocol);
            }

            var communityResult = await Messenger.GetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var v2Data = communityResult.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(v2Data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SNMP GETNEXT failed for {oid}: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    private async Task<ISnmpMessage> SendV3RequestAsync(
        Func<int, int, OctetString, OctetString, IPrivacyProvider, ISnmpMessage> createMessage,
        CancellationToken cancellationToken)
    {
        var (userName, privacy, registry) = CreateV3Context();
        var messageId = Messenger.NextMessageId;
        var requestId = Messenger.NextRequestId;
        var contextName = new OctetString(_config.V3ContextName ?? string.Empty);

        var message = createMessage(messageId, requestId, userName, contextName, privacy);
        return await message.GetResponseAsync(_endpoint, registry, cancellationToken);
    }

    private (OctetString UserName, IPrivacyProvider Privacy, UserRegistry Registry) CreateV3Context()
    {
        var userName = new OctetString(_config.V3UserName ?? _config.Community);
        var authPassword = _config.V3AuthPassword ?? _config.Community;
        var privPassword = _config.V3PrivPassword ?? _config.Community;

#pragma warning disable CS0618
        var authProtocol = (_config.V3AuthProtocol ?? "SHA256").ToUpperInvariant();
        IAuthenticationProvider authProvider = authProtocol switch
        {
            "MD5" => new MD5AuthenticationProvider(new OctetString(authPassword)),
            "SHA1" or "HMAC_MD5" => new SHA1AuthenticationProvider(new OctetString(authPassword)),
            "SHA256" or "SHA" => new SHA256AuthenticationProvider(new OctetString(authPassword)),
            "SHA384" => new SHA384AuthenticationProvider(new OctetString(authPassword)),
            "SHA512" => new SHA512AuthenticationProvider(new OctetString(authPassword)),
            _ => new SHA256AuthenticationProvider(new OctetString(authPassword)),
        };

        var privProtocol = (_config.V3PrivProtocol ?? "AES256").ToUpperInvariant();
        IPrivacyProvider privProvider = privProtocol switch
        {
            "DES" => new DESPrivacyProvider(new OctetString(privPassword), authProvider),
            "AES" => new AESPrivacyProvider(new OctetString(privPassword), authProvider),
            "AES192" => new AES192PrivacyProvider(new OctetString(privPassword), authProvider),
            "AES256" => new AES256PrivacyProvider(new OctetString(privPassword), authProvider),
            "NONE" or "" => new DefaultPrivacyProvider(authProvider),
            _ => new AES256PrivacyProvider(new OctetString(privPassword), authProvider),
        };
#pragma warning restore CS0618

        var registry = new UserRegistry();
        registry.Add(userName, privProvider);

        return (userName, privProvider, registry);
    }

    private static IPAddress ResolveHost(string host)
    {
        if (IPAddress.TryParse(host, out var address))
            return address;

        var entries = System.Net.Dns.GetHostEntry(host);
        return entries.AddressList.FirstOrDefault() ?? IPAddress.Loopback;
    }
}
