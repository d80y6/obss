using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.Provisioning.Api.Endpoints;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.BackgroundJobs;
using Obss.Provisioning.Application.Diagnostics;
using Obss.Provisioning.Application.Mappings;
using Obss.Provisioning.Application.Services;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Adapters.Cisco;
using Obss.Provisioning.Infrastructure.Adapters.Huawei;
using Obss.Provisioning.Infrastructure.Adapters.Juniper;
using Obss.Provisioning.Infrastructure.Adapters.Nokia;
using Obss.Provisioning.Infrastructure.Persistence;
using Obss.Provisioning.Infrastructure.Persistence.Repositories;
using Obss.Provisioning.Infrastructure.Services;
using Obss.Provisioning.Infrastructure.Transports;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Obss.Provisioning.Infrastructure.Transports.Extensions;
using Obss.Provisioning.Infrastructure.Transports.Restconf;
using Obss.Provisioning.Infrastructure.Transports.Netconf;
using Obss.Provisioning.Infrastructure.Transports.Rest;
using Obss.Provisioning.Infrastructure.Transports.Snmp;
using Obss.Provisioning.Infrastructure.Transports.Ssh;

namespace Obss.Provisioning.Api.Extensions;

public static class ProvisioningModuleRegistration
{
    public static IServiceCollection AddProvisioningModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddScoped<IProvisioningJobRepository, ProvisioningJobRepository>();
        services.AddScoped<IProvisioningTemplateRepository, ProvisioningTemplateRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();

        services.AddSingleton<ProvisioningMetrics>();
        services.AddSingleton<IAdapterRegistry, AdapterRegistry>();
        services.AddSingleton<ITransportFactory, TransportFactory>();

        services.AddScoped<IProvisioningJobCoordinator, ProvisioningJobCoordinator>();
        services.AddHostedService<ProvisioningJobProcessor>();

        RegisterHuaweiAdapter(services, configuration);
        RegisterCiscoAdapter(services, configuration);
        RegisterJuniperAdapter(services, configuration);
        RegisterNokiaAdapter(services, configuration);

        services.AddScoped<IProvisioningAdapter, NetworkProvisioningAdapter>();
        services.AddScoped<IProvisioningAdapter, DnsSetupAdapter>();
        services.AddScoped<IProvisioningAdapter, AccountSetupAdapter>();
        services.AddScoped<IProvisioningAdapter, TestProvisioningAdapter>();

        ProvisioningMappingConfig.Configure();

        return services;
    }

    private static void RegisterHuaweiAdapter(IServiceCollection services, IConfiguration? configuration)
    {
        var config = new HuaweiAdapterConfig();

        if (configuration is not null)
        {
            var section = configuration.GetSection("Provisioning:Huawei");
            if (section.Exists())
            {
                section.Bind(config);

                config.SnmpTransport = section.GetSection("SnmpTransport").Exists()
                    ? section.GetSection("SnmpTransport").Get<SnmpTransportConfig>()
                    : null;

                config.SshTransport = section.GetSection("SshTransport").Exists()
                    ? section.GetSection("SshTransport").Get<SshTransportConfig>()
                    : null;

                config.NetconfTransport = section.GetSection("NetconfTransport").Exists()
                    ? section.GetSection("NetconfTransport").Get<NetconfTransportConfig>()
                    : null;

                config.RestTransport = section.GetSection("RestTransport").Exists()
                    ? section.GetSection("RestTransport").Get<RestTransportConfig>()
                    : null;
            }
        }

        services.AddSingleton(config);

        if (config.UseSimulator)
        {
            services.AddSingleton<IHuaweiBroadbandAdapter>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<HuaweiBroadbandSimulator>>();
                return new HuaweiBroadbandSimulator(logger, config);
            });
        }
        else
        {
            services.AddSingleton<IHuaweiBroadbandAdapter>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<HuaweiBroadbandAdapter>>();
                var transportFactory = sp.GetRequiredService<ITransportFactory>();
                return new HuaweiBroadbandAdapter(logger, config, transportFactory);
            });
        }

        services.AddScoped<IProvisioningAdapter, HuaweiProvisioningAdapter>();
    }

    private static void RegisterCiscoAdapter(IServiceCollection services, IConfiguration? configuration)
    {
        var config = new CiscoAdapterConfig();

        if (configuration is not null)
        {
            var section = configuration.GetSection("Provisioning:Cisco");
            if (section.Exists())
            {
                section.Bind(config);

                config.RestconfTransport = section.GetSection("RestconfTransport").Exists()
                    ? section.GetSection("RestconfTransport").Get<RestconfTransportConfig>()
                    : null;
            }
        }

        services.AddSingleton(config);

        if (config.UseSimulator)
        {
            services.AddSingleton<ICiscoRouterAdapter>(_ => new CiscoRouterSimulator());
        }
        else
        {
            services.AddSingleton<ICiscoRouterAdapter>(_ =>
            {
                var restconfTransport = new RestconfTransport(new RestconfTransportConfig
                {
                    BaseUri = config.BaseUri
                });
                return new CiscoRouterAdapter(config, restconfTransport);
            });
        }

        services.AddScoped<IProvisioningAdapter, CiscoProvisioningAdapter>();
    }

    private static void RegisterJuniperAdapter(IServiceCollection services, IConfiguration? configuration)
    {
        var config = new JuniperAdapterConfig();

        if (configuration is not null)
        {
            var section = configuration.GetSection("Provisioning:Juniper");
            if (section.Exists())
            {
                section.Bind(config);
            }
        }

        services.AddSingleton(config);

        if (config.UseSimulator)
        {
            services.AddSingleton<IJuniperRouterAdapter>(_ => new JuniperRouterSimulator());
        }
        else
        {
            services.AddSingleton<IJuniperRouterAdapter>(_ =>
            {
                var restconfTransport = new RestconfTransport(new RestconfTransportConfig
                {
                    BaseUri = config.BaseUri
                });
                return new JuniperRouterAdapter(config, restconfTransport);
            });
        }

        services.AddScoped<IProvisioningAdapter, JuniperProvisioningAdapter>();
    }

    private static void RegisterNokiaAdapter(IServiceCollection services, IConfiguration? configuration)
    {
        var config = new NokiaAdapterConfig();

        if (configuration is not null)
        {
            var section = configuration.GetSection("Provisioning:Nokia");
            if (section.Exists())
            {
                section.Bind(config);
            }
        }

        services.AddSingleton(config);

        if (config.UseSimulator)
        {
            services.AddSingleton<INokiaRouterAdapter>(_ => new NokiaRouterSimulator());
        }
        else
        {
            services.AddSingleton<INokiaRouterAdapter>(_ =>
            {
                var restconfTransport = new RestconfTransport(new RestconfTransportConfig
                {
                    BaseUri = config.BaseUri
                });
                return new NokiaRouterAdapter(config, restconfTransport);
            });
        }

        services.AddScoped<IProvisioningAdapter, NokiaProvisioningAdapter>();
    }

    public static IEndpointRouteBuilder MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/provisioning")
            .WithTags("Provisioning");

        ProvisioningEndpoints.Map(group);

        return app;
    }
}
