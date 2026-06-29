using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Obss.ModuleRegistration;

public interface IModuleRegistration
{
    IServiceCollection AddModuleServices(IServiceCollection services);
    IEndpointRouteBuilder MapModuleEndpoints(IEndpointRouteBuilder endpoints);
}
