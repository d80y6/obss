using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Obss.Provisioning.Infrastructure.Transports.Rest;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Obss.Provisioning.Infrastructure.Transports.Ssh;

namespace Obss.Provisioning.Infrastructure.Transports.Extensions;

public static class TransportServiceCollectionExtensions
{
    public static IServiceCollection AddNetworkTransports(this IServiceCollection services)
    {
        services.TryAddSingleton<ITransportFactory, TransportFactory>();
        return services;
    }

    public static IServiceCollection AddSnmpTransport(this IServiceCollection services, SnmpTransportConfig config)
    {
        services.AddSingleton<ISnmpTransport>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SnmpTransport>>();
            return new SnmpTransport(config, logger);
        });
        return services;
    }

    public static IServiceCollection AddSshTransport(this IServiceCollection services, SshTransportConfig config)
    {
        services.AddSingleton<ISshTransport>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SshTransport>>();
            return new SshTransport(config, logger);
        });
        return services;
    }

    public static IServiceCollection AddNetconfTransport(this IServiceCollection services, NetconfTransportConfig config)
    {
        services.AddSingleton<INetconfTransport>(_ =>
        {
            return new NetconfTransport(config);
        });
        return services;
    }

    public static IServiceCollection AddRestTransport(this IServiceCollection services, RestTransportConfig config)
    {
        services.AddSingleton<IRestTransport>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RestTransport>>();
            var httpClientFactory = sp.GetService<IHttpClientFactory>();
            return new RestTransport(config, logger, httpClientFactory);
        });
        return services;
    }
}
