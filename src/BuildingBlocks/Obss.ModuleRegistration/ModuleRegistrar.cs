using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Obss.ModuleRegistration;

public sealed class ModuleRegistrar
{
    private readonly IServiceCollection? _services;
    private readonly ILogger? _logger;

    public ModuleRegistrar(ILogger? logger = null)
    {
        _logger = logger;
    }

    public ModuleRegistrar(IServiceCollection services, ILogger? logger = null)
    {
        _services = services;
        _logger = logger;
    }

    public ModuleRegistrar AddModuleFromAssembly<TMarker>()
    {
        return AddModuleFromAssembly(typeof(TMarker).Assembly);
    }

    public ModuleRegistrar AddModuleFromAssembly(Assembly assembly)
    {
        if (_services is null)
            throw new InvalidOperationException("Services collection was not provided. Use the constructor that accepts IServiceCollection.");

        foreach (var registration in DiscoverRegistrations(assembly))
        {
            _logger?.LogInformation("Registering module services from {ModuleType}", registration.GetType().Name);
            registration.AddModuleServices(_services);
        }

        return this;
    }

    public void MapModuleEndpointsFromAssembly(Assembly assembly, IEndpointRouteBuilder endpoints)
    {
        foreach (var registration in DiscoverRegistrations(assembly))
        {
            _logger?.LogInformation("Mapping module endpoints from {ModuleType}", registration.GetType().Name);
            registration.MapModuleEndpoints(endpoints);
        }
    }

    private static List<IModuleRegistration> DiscoverRegistrations(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        typeof(IModuleRegistration).IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<IModuleRegistration>()
            .ToList();
    }
}
