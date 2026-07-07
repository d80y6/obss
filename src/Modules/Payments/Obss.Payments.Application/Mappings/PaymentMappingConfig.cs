using Mapster;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;

namespace Obss.Payments.Application.Mappings;

public static class PaymentMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Payment, PaymentDto>.NewConfig()
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Allocations, src => src.Allocations.Adapt<List<PaymentAllocationDto>>())
            .Map(dest => dest.Refunds, src => src.Refunds.Adapt<List<RefundDto>>())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>());

        TypeAdapterConfig<PaymentAllocation, PaymentAllocationDto>.NewConfig();

        TypeAdapterConfig<Refund, RefundDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<PaymentGateway, PaymentGatewayDto>.NewConfig()
            .Map(dest => dest.Provider, src => src.Provider.ToString())
            .Map(dest => dest.FeeType, src => src.FeeType.ToString())
            .Map(dest => dest.SupportedCurrencies, src => src.SupportedCurrencies.ToList());

        TypeAdapterConfig<PaymentReconciliation, PaymentReconciliationDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items.Adapt<List<ReconciliationItemDto>>());

        TypeAdapterConfig<ReconciliationItem, ReconciliationItemDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
