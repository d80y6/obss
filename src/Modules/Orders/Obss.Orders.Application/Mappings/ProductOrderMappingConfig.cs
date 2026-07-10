using Mapster;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Application.Mappings;

public static class ProductOrderMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ProductOrder, ProductOrderDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.OrderType, src => src.OrderType.ToString())
            .Map(dest => dest.CreatedAt, src => src.OrderDate)
            .Map(dest => dest.Priority, src => src.OrderPriority.ToString())
            .Map(dest => dest.OrderPriority, src => src.OrderPriority.ToString())
            .Map(dest => dest.BillingAddressStreet, src => src.BillingAddress != null ? src.BillingAddress.Street : null)
            .Map(dest => dest.BillingAddressCity, src => src.BillingAddress != null ? src.BillingAddress.City : null)
            .Map(dest => dest.BillingAddressState, src => src.BillingAddress != null ? src.BillingAddress.State : null)
            .Map(dest => dest.BillingAddressPostalCode, src => src.BillingAddress != null ? src.BillingAddress.PostalCode : null)
            .Map(dest => dest.BillingAddressCountry, src => src.BillingAddress != null ? src.BillingAddress.Country : null)
            .Map(dest => dest.ShippingAddressStreet, src => src.ShippingAddress != null ? src.ShippingAddress.Street : null)
            .Map(dest => dest.ShippingAddressCity, src => src.ShippingAddress != null ? src.ShippingAddress.City : null)
            .Map(dest => dest.ShippingAddressState, src => src.ShippingAddress != null ? src.ShippingAddress.State : null)
            .Map(dest => dest.ShippingAddressPostalCode, src => src.ShippingAddress != null ? src.ShippingAddress.PostalCode : null)
            .Map(dest => dest.ShippingAddressCountry, src => src.ShippingAddress != null ? src.ShippingAddress.Country : null)
            .Map(dest => dest.Items, src => src.Items.Adapt<List<ProductOrderItemDto>>())
            .Map(dest => dest.Payments, src => src.Payments.Adapt<List<ProductOrderPaymentDto>>())
            .Map(dest => dest.Fulfillment, src => src.Fulfillment != null ? src.Fulfillment.Adapt<OrderFulfillmentDto>() : null)
            .Map(dest => dest.Milestones, src => src.Milestones.Adapt<List<ProductOrderMilestoneDto>>())
            .Map(dest => dest.ItemRelationships, src => src.ItemRelationships.Adapt<List<ProductOrderItemRelationshipDto>>());

        TypeAdapterConfig<ProductOrderItem, ProductOrderItemDto>.NewConfig()
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString())
            .Map(dest => dest.State, src => src.State.ToString());

        TypeAdapterConfig<ProductOrderPayment, ProductOrderPaymentDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<OrderFulfillment, OrderFulfillmentDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<ProductOrder, ProductOrderSummaryDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.OrderType, src => src.OrderType.ToString())
            .Map(dest => dest.OrderPriority, src => src.OrderPriority.ToString());

        TypeAdapterConfig<ProductOrderItemRelationship, ProductOrderItemRelationshipDto>.NewConfig()
            .Map(d => d.Type, s => s.Type.ToString());

        TypeAdapterConfig<ProductOrderMilestone, ProductOrderMilestoneDto>.NewConfig()
            .Map(d => d.Status, s => s.Status.ToString());

        TypeAdapterConfig<ProductOrderItem, ProductOrderItemPriceDto>.NewConfig()
            .Map(d => d.UnitPrice, s => s.UnitPrice)
            .Map(d => d.RecurringPrice, s => s.RecurringPrice)
            .Map(d => d.DiscountAmount, s => s.DiscountAmount)
            .Map(d => d.TaxAmount, s => s.TaxAmount)
            .Map(d => d.TotalPrice, s => s.TotalPrice);
    }
}
