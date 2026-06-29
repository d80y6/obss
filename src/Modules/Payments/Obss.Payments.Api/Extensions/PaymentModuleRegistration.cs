using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Payments.Api.Endpoints;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.Mappings;
using Obss.Payments.Application.Services;
using Obss.Payments.Domain.Services;
using Obss.Payments.Infrastructure.Persistence;
using Obss.Payments.Infrastructure.Persistence.Repositories;
using Obss.Payments.Infrastructure.Services.Gateways;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Payments.Api.Extensions;

public static class PaymentModuleRegistration
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
        services.AddScoped<IPaymentReconciliationRepository, PaymentReconciliationRepository>();

        services.AddScoped<IPaymentGatewayService, PaymentGatewayManager>();

        services.AddScoped<IGatewayClient, StripeGateway>();
        services.AddScoped<IGatewayClient, PayPalGateway>();
        services.AddScoped<IGatewayClient, LocalBankGateway>();
        services.AddScoped<IGatewayClient, MobileMoneyGateway>();
        services.AddScoped<IGatewayClient, CashGateway>();

        PaymentMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/payments")
            .WithTags("Payments");

        PaymentEndpoints.Map(group);

        return app;
    }
}
