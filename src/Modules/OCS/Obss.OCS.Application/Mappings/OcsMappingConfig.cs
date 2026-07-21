using Mapster;
using Obss.OCS.Application.DTOs;
using Obss.OCS.Domain.Entities;

namespace Obss.OCS.Application.Mappings;

public static class OcsMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Balance, BalanceDto>.NewConfig()
            .Map(d => d.EffectiveBalance, s => s.EffectiveBalance);

        TypeAdapterConfig<CreditPool, CreditPoolDto>.NewConfig()
            .Map(d => d.Status, s => s.Status.ToString());

        TypeAdapterConfig<OcsTransaction, OcsTransactionDto>.NewConfig()
            .Map(d => d.TransactionType, s => s.TransactionType.ToString());
    }
}
