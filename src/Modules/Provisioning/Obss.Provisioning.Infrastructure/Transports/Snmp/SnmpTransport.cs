using System.Diagnostics;
using System.Net;
using System.Text;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Snmp;

public sealed class SnmpTransport : ISnmpTransport
{
    private readonly SnmpTransportConfig _config;
    private readonly ILogger<SnmpTransport> _logger;
    private readonly IPEndPoint _endpoint;

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
            var result = await Messenger.GetAsync(
                _config.SnmpVersion switch
                {
                    SnmpVersion.V1 => VersionCode.V1,
                    SnmpVersion.V2C => VersionCode.V2,
                    SnmpVersion.V3 => VersionCode.V3,
                    _ => VersionCode.V2
                },
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var data = result.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(data, sw.Elapsed, Protocol);
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
            var variables = oids.Select(o => new Variable(new ObjectIdentifier(o))).ToList();
            var result = await Messenger.GetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                variables,
                cancellationToken);

            sw.Stop();
            var data = string.Join(Environment.NewLine,
                result.Select(v => $"{v.Id}={v.Data}"));
            return TransportResult.Ok(data, sw.Elapsed, Protocol);
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
            var data = string.Join(Environment.NewLine,
                results.Select(v => $"{v.Id}={v.Data}"));
            return TransportResult.Ok(data, sw.Elapsed, Protocol);
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
            var result = await Messenger.SetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var resultData = result.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(resultData, sw.Elapsed, Protocol);
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
            var result = await Messenger.GetAsync(
                VersionCode.V2,
                _endpoint,
                new OctetString(_config.Community),
                [variable],
                cancellationToken);

            sw.Stop();
            var data = result.FirstOrDefault()?.Data?.ToString();
            return TransportResult.Ok(data, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SNMP GETNEXT failed for {oid}: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    private static IPAddress ResolveHost(string host)
    {
        if (IPAddress.TryParse(host, out var address))
            return address;

        var entries = System.Net.Dns.GetHostEntry(host);
        return entries.AddressList.FirstOrDefault() ?? IPAddress.Loopback;
    }
}
