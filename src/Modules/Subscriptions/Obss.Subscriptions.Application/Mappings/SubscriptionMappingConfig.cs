using Mapster;
using Obss.Subscriptions.Application.DTOs;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Application.Mappings;

public static class SubscriptionMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Subscription, SubscriptionDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString())
            .Map(dest => dest.AddOns, src => src.AddOns.Adapt<List<SubscriptionAddOnDto>>())
            .Map(dest => dest.Services, src => src.Services.Adapt<List<SubscriptionServiceDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<SubscriptionAddOn, SubscriptionAddOnDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<SubscriptionService, SubscriptionServiceDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Subscription, SubscriptionSummaryDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString());

        TypeAdapterConfig<SubscriptionEntitlement, SubscriptionEntitlementDto>.NewConfig()
            .Map(dest => dest.EntitlementType, src => src.EntitlementType.ToString())
            .Map(dest => dest.Id, src => src.Id);
    }
}
