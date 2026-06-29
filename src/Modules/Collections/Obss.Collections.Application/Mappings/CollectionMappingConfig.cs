using Mapster;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;

namespace Obss.Collections.Application.Mappings;

public static class CollectionMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<CollectionCase, CollectionCaseDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Actions, src => src.Actions.Adapt<List<CollectionActionDto>>())
            .Map(dest => dest.PaymentArrangements, src => src.PaymentArrangements.Adapt<List<PaymentArrangementDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CollectionAction, CollectionActionDto>.NewConfig()
            .Map(dest => dest.ActionType, src => src.ActionType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<PaymentArrangement, PaymentArrangementDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Frequency, src => src.Frequency.ToString())
            .Map(dest => dest.Id, src => src.Id);
    }
}
