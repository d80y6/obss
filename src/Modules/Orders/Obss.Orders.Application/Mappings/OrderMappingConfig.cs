using Mapster;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Application.Mappings;

public static class OrderMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Order, OrderDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.OrderType, src => src.OrderType.ToString())
            .Map(dest => dest.CreatedAt, src => src.OrderDate)
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
            .Map(dest => dest.Items, src => src.Items.Adapt<List<OrderItemDto>>())
            .Map(dest => dest.Payments, src => src.Payments.Adapt<List<OrderPaymentDto>>())
            .Map(dest => dest.Fulfillment, src => src.Fulfillment != null ? src.Fulfillment.Adapt<OrderFulfillmentDto>() : null);

        TypeAdapterConfig<OrderItem, OrderItemDto>.NewConfig()
            .Map(dest => dest.BillingPeriod, src => src.BillingPeriod.ToString());

        TypeAdapterConfig<OrderPayment, OrderPaymentDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<OrderFulfillment, OrderFulfillmentDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<Order, OrderSummaryDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.OrderType, src => src.OrderType.ToString());
    }
}
