using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Obss.ModuleRegistration;
using Obss.NumberInventory.Api.Extensions;

namespace Obss.Host.Modules;

public static class ModuleLoader
{
    private const string CategoryName = "ModuleLoader";

    public static WebApplicationBuilder LoadModules(this WebApplicationBuilder builder, Assembly? entryAssembly = null)
    {
        entryAssembly ??= Assembly.GetEntryAssembly();

        if (entryAssembly is null)
            return builder;

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var registrar = new ModuleRegistrar(builder.Services, loggerFactory.CreateLogger(CategoryName));

        foreach (var assembly in GetModuleAssemblies(entryAssembly))
        {
            registrar.AddModuleFromAssembly(assembly);
        }

        builder.Services.AddNumberInventoryModule();

        return builder;
    }

    public static WebApplication MapModuleEndpoints(this WebApplication app, Assembly? entryAssembly = null)
    {
        entryAssembly ??= Assembly.GetEntryAssembly();

        if (entryAssembly is null)
            return app;

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var registrar = new ModuleRegistrar(loggerFactory.CreateLogger(CategoryName));

        foreach (var assembly in GetModuleAssemblies(entryAssembly))
        {
            registrar.MapModuleEndpointsFromAssembly(assembly, app);
        }

        app.MapNumberInventoryEndpoints();

        return app;
    }

    private static IEnumerable<Assembly> GetModuleAssemblies(Assembly entryAssembly)
    {
        yield return entryAssembly;

        foreach (var referenced in entryAssembly.GetReferencedAssemblies())
        {
            yield return Assembly.Load(referenced);
        }
    }
}
