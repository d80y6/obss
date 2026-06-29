using Mapster;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;

namespace Obss.Billing.Application.Mappings;

public static class BillingMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Bill, BillDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString())
            .Map(dest => dest.Lines, src => src.Lines.Adapt<List<BillLineDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BillLine, BillLineDto>.NewConfig()
            .Map(dest => dest.LineType, src => src.LineType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BillingCycle, BillingCycleDto>.NewConfig()
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<TaxRule, TaxRuleDto>.NewConfig()
            .Map(dest => dest.TaxType, src => src.TaxType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<TaxExemption, TaxExemptionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
