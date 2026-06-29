using Mapster;
using Obss.NumberInventory.Application.DTOs;
using Obss.NumberInventory.Domain.Entities;

namespace Obss.NumberInventory.Application.Mappings;

public static class NumberInventoryMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<TelephoneNumber, TelephoneNumberDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.NumberType, src => src.NumberType.ToString())
            .Map(dest => dest.Id, src => src.Id);
    }
}
