using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Obss.Provisioning.Infrastructure.Transports.Rest;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Obss.Provisioning.Infrastructure.Transports.Ssh;

namespace Obss.Provisioning.Infrastructure.Transports;

public sealed class TransportFactory : ITransportFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TransportFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyCollection<TransportProtocol> SupportedProtocols { get; } =
    [
        TransportProtocol.SnmpV1,
        TransportProtocol.SnmpV2C,
        TransportProtocol.SnmpV3,
        TransportProtocol.Ssh,
        TransportProtocol.Netconf,
        TransportProtocol.Rest,
        TransportProtocol.Restconf
    ];

    public bool SupportsProtocol(TransportProtocol protocol) => SupportedProtocols.Contains(protocol);

    public INetworkTransport CreateTransport(ITransportConfig config)
    {
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

        return config.Protocol switch
        {
            TransportProtocol.SnmpV1 or TransportProtocol.SnmpV2C or TransportProtocol.SnmpV3
                => CreateSnmpTransport(config, loggerFactory),

            TransportProtocol.Ssh or TransportProtocol.Cli
                => CreateSshTransport(config, loggerFactory),

            TransportProtocol.Netconf
                => CreateNetconfTransport(config),

            TransportProtocol.Rest
                => CreateRestTransport(config, loggerFactory),

            TransportProtocol.Restconf
                => CreateRestconfTransport(config, loggerFactory),

            _ => throw new NotSupportedException($"Transport protocol {config.Protocol} is not supported")
        };
    }

    private static INetworkTransport CreateSnmpTransport(ITransportConfig config, ILoggerFactory loggerFactory)
    {
        if (config is not SnmpTransportConfig snmpConfig)
            throw new InvalidOperationException($"Expected SnmpTransportConfig but got {config.GetType().Name}");

        return new SnmpTransport(snmpConfig, loggerFactory.CreateLogger<SnmpTransport>());
    }

    private static INetworkTransport CreateSshTransport(ITransportConfig config, ILoggerFactory loggerFactory)
    {
        if (config is not SshTransportConfig sshConfig)
            throw new InvalidOperationException($"Expected SshTransportConfig but got {config.GetType().Name}");

        return new SshTransport(sshConfig, loggerFactory.CreateLogger<SshTransport>());
    }

    private static INetworkTransport CreateNetconfTransport(ITransportConfig config)
    {
        if (config is not NetconfTransportConfig netconfConfig)
            throw new InvalidOperationException($"Expected NetconfTransportConfig but got {config.GetType().Name}");

        return new NetconfTransport(netconfConfig);
    }

    private static INetworkTransport CreateRestconfTransport(ITransportConfig config, ILoggerFactory loggerFactory)
    {
        if (config is not RestconfTransportConfig restconfConfig)
            throw new InvalidOperationException($"Expected RestconfTransportConfig but got {config.GetType().Name}");

        return new RestconfTransport(restconfConfig, loggerFactory.CreateLogger<RestconfTransport>());
    }

    private INetworkTransport CreateRestTransport(ITransportConfig config, ILoggerFactory loggerFactory)
    {
        if (config is not RestTransportConfig restConfig)
            throw new InvalidOperationException($"Expected RestTransportConfig but got {config.GetType().Name}");

        var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
        return new RestTransport(restConfig, loggerFactory.CreateLogger<RestTransport>(), httpClientFactory);
    }
}
