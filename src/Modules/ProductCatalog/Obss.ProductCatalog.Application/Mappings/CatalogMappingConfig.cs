using Mapster;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using ProductSpecification = Obss.ProductCatalog.Domain.Domain.Entities.ProductSpecification;

namespace Obss.ProductCatalog.Application.Mappings;

public static class CatalogMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
            .Map(dest => dest.Specifications, src => src.Specifications.Adapt<List<ProductSpecValueDto>>())
            .Map(dest => dest.Offers, src => src.ProductOffers.Select(po => po.Offer).Adapt<List<OfferDto>>())
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductNumber, src => src.ProductNumber)
            .Map(dest => dest.ProductSpecificationId, src => src.ProductSpecificationId);

        TypeAdapterConfig<Obss.ProductCatalog.Domain.Domain.ValueObjects.ProductSpecification, ProductSpecValueDto>.NewConfig()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Value, src => src.Value)
            .Map(dest => dest.IsRequired, src => src.IsRequired);

        TypeAdapterConfig<Offer, OfferDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Pricings, src => src.OfferPricings.Adapt<List<OfferPricingDto>>())
            .Map(dest => dest.Discounts, src => src.Discounts.Adapt<List<OfferDiscountDto>>());

        TypeAdapterConfig<OfferPricing, OfferPricingDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.OfferId, src => src.OfferId);

        TypeAdapterConfig<OfferDiscount, OfferDiscountDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Category, CategoryDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProductConfigurationRule, ProductConfigurationRuleDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProductOption, ProductOptionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Values, src => src.Values.Adapt<List<OptionValueDto>>());

        TypeAdapterConfig<OptionValue, OptionValueDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Catalog, CatalogDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProductSpecification, ProductSpecificationDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Characteristics, src => src.Characteristics.Adapt<List<ProductSpecificationCharacteristicDto>>())
            .Map(dest => dest.Relationships, src => src.Relationships.Adapt<List<ProductSpecificationRelationshipDto>>());

        TypeAdapterConfig<ProductSpecificationCharacteristic, ProductSpecificationCharacteristicDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Values, src => src.Values.Adapt<List<ProductSpecificationCharacteristicValueDto>>());

        TypeAdapterConfig<ProductSpecificationCharacteristicValue, ProductSpecificationCharacteristicValueDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProductSpecificationRelationship, ProductSpecificationRelationshipDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
