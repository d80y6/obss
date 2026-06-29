using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.DTOs;
using Obss.Orders.Domain.Entities;
using Obss.Orders.Domain.ValueObjects;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.Orders.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOfferRepository offerRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _offerRepository = offerRepository;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return Result.Failure<OrderDto>(Error.Unauthorized("Tenant context is required."));

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Result.Failure<OrderDto>(Error.NotFound("Customer", request.CustomerId));

        if (!Enum.TryParse<OrderType>(request.OrderType, true, out var orderType))
            return Result.Failure<OrderDto>(Error.Validation($"Invalid order type: {request.OrderType}"));

        Address? billingAddress = null;
        if (!string.IsNullOrWhiteSpace(request.BillingAddressStreet))
        {
            billingAddress = Address.Create(
                request.BillingAddressStreet,
                request.BillingAddressCity ?? string.Empty,
                request.BillingAddressState,
                request.BillingAddressPostalCode,
                request.BillingAddressCountry ?? string.Empty);
        }

        Address? shippingAddress = null;
        if (!string.IsNullOrWhiteSpace(request.ShippingAddressStreet))
        {
            shippingAddress = Address.Create(
                request.ShippingAddressStreet,
                request.ShippingAddressCity ?? string.Empty,
                request.ShippingAddressState,
                request.ShippingAddressPostalCode,
                request.ShippingAddressCountry ?? string.Empty);
        }

        var createdById = _currentUser.UserId ?? "system";

        var order = Order.Create(
            tenantId,
            request.CustomerId,
            request.CustomerName,
            orderType,
            createdById,
            request.Notes,
            billingAddress,
            shippingAddress,
            request.Currency);

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure<OrderDto>(Error.NotFound("Product", item.ProductId));

            var offer = await _offerRepository.GetByIdAsync(item.OfferId, cancellationToken);
            if (offer is null)
                return Result.Failure<OrderDto>(Error.NotFound("Offer", item.OfferId));

            if (!Enum.TryParse<BillingPeriod>(item.BillingPeriod, true, out var billingPeriod))
                return Result.Failure<OrderDto>(Error.Validation($"Invalid billing period: {item.BillingPeriod}"));

            var serviceType = product is not null
                ? product.Specifications
                    .FirstOrDefault(s => s.Name.Equals("ServiceType", StringComparison.OrdinalIgnoreCase))
                    ?.Value
                : null;

            order.AddItem(
                item.ProductId,
                item.OfferId,
                item.ProductName,
                item.OfferName,
                item.Quantity,
                item.UnitPrice,
                item.RecurringPrice,
                item.DiscountAmount,
                item.TaxAmount,
                billingPeriod,
                serviceType: serviceType);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Adapt<OrderDto>());
    }
}
