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
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>())
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

        TypeAdapterConfig<BillingAccount, BillingAccountDto>.NewConfig()
            .Map(dest => dest.AccountType, src => src.AccountType.ToString())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>())
            .Map(dest => dest.AccountHolder, src => src.AccountHolder.Adapt<AccountHolderDto>())
            .Map(dest => dest.BillPresentationMedia, src => src.BillPresentationMedia.Adapt<List<BillPresentationMediaDto>>())
            .Map(dest => dest.PaymentMethodId, src => src.PaymentMethodId)
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AccountBalance, AccountBalanceDto>.NewConfig()
            .Map(dest => dest.Transactions, src => src.Transactions.Adapt<List<BalanceTransactionDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BalanceTransaction, BalanceTransactionDto>.NewConfig()
            .Map(dest => dest.TransactionType, src => src.TransactionType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BillPresentationMedia, BillPresentationMediaDto>.NewConfig()
            .Map(dest => dest.MediaType, src => src.MediaType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AccountHolder, AccountHolderDto>.NewConfig();
    }
}
