using Obss.CRM.Application.Abstractions;
using Obss.CRM.Domain.Entities;
using Obss.Orders.Domain.Entities;
using Obss.ProductCatalog.Application.Abstractions;

namespace Obss.Orders.Application.Services;

public sealed record ValidationError(string Code, string Message, string Severity);
public sealed record ValidationWarning(string Code, string Message);
public sealed record OrderValidationResult(
    bool IsValid,
    List<ValidationError> Errors,
    List<ValidationWarning> Warnings);

public class OrderValidationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOfferRepository _offerRepository;

    public OrderValidationService(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOfferRepository offerRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _offerRepository = offerRepository;
    }

    public virtual async Task<OrderValidationResult> ValidateAsync(ProductOrder order, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);

        await ValidateCustomerAsync(customer, errors);
        await ValidateProductsAndOffersAsync(order, errors, cancellationToken);
        await ValidateCreditLimitAsync(order, customer, errors);
        ValidateBillingInformation(order, errors, warnings);

        return new OrderValidationResult(errors.Count == 0, errors, warnings);
    }

    private static Task ValidateCustomerAsync(Customer? customer, List<ValidationError> errors)
    {
        if (customer is null)
        {
            errors.Add(new ValidationError("CUSTOMER_NOT_FOUND", $"Customer does not exist.", "Error"));
            return Task.CompletedTask;
        }

        if (!customer.IsActive)
        {
            errors.Add(new ValidationError("CUSTOMER_INACTIVE", $"Customer '{customer.DisplayName}' is not active.", "Error"));
        }

        return Task.CompletedTask;
    }

    private async Task ValidateProductsAndOffersAsync(
        ProductOrder order,
        List<ValidationError> errors,
        CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                errors.Add(new ValidationError("PRODUCT_NOT_FOUND", $"Product '{item.ProductId}' ({item.ProductName}) not found.", "Error"));
                continue;
            }

            if (!product.IsActive)
            {
                errors.Add(new ValidationError("PRODUCT_INACTIVE", $"Product '{product.Name}' is not active.", "Error"));
            }

            var offer = await _offerRepository.GetByIdAsync(item.OfferId, cancellationToken);
            if (offer is null)
            {
                errors.Add(new ValidationError("OFFER_NOT_FOUND", $"Offer '{item.OfferId}' ({item.OfferName}) not found.", "Error"));
                continue;
            }

            if (!offer.IsActive)
            {
                errors.Add(new ValidationError("OFFER_INACTIVE", $"Offer '{offer.Name}' is not active.", "Error"));
            }

            if (offer.ValidFrom.HasValue && offer.ValidTo.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now < offer.ValidFrom.Value || now > offer.ValidTo.Value)
                {
                    errors.Add(new ValidationError("OFFER_EXPIRED", $"Offer '{offer.Name}' is not valid for the current date.", "Error"));
                }
            }

            if (offer.IsContract && (!offer.ContractDurationMonths.HasValue || offer.ContractDurationMonths <= 0))
            {
                errors.Add(new ValidationError("INVALID_CONTRACT", $"Offer '{offer.Name}' is marked as contract but has no duration.", "Error"));
            }
        }
    }

    private static Task ValidateCreditLimitAsync(ProductOrder order, Customer? customer, List<ValidationError> errors)
    {
        if (customer is null || order.GrandTotal <= 0)
            return Task.CompletedTask;

        if (customer.CreditLimit > 0 && order.GrandTotal > customer.CreditLimit)
        {
            errors.Add(new ValidationError("CREDIT_LIMIT_EXCEEDED",
                $"Order total {order.GrandTotal} {order.Currency} exceeds credit limit {customer.CreditLimit} {customer.Currency}.", "Error"));
        }

        return Task.CompletedTask;
    }

    private static void ValidateBillingInformation(ProductOrder order, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        if (order.BillingAddress is null)
        {
            errors.Add(new ValidationError("BILLING_ADDRESS_REQUIRED", "Billing address is required.", "Error"));
        }

        if (order.BillingAddress is not null && string.IsNullOrWhiteSpace(order.BillingAddress.Country))
        {
            errors.Add(new ValidationError("BILLING_COUNTRY_REQUIRED", "Billing country is required.", "Error"));
        }

        if (order.Payments.Count == 0)
        {
            warnings.Add(new ValidationWarning("NO_PAYMENTS", "No payment information provided."));
        }
    }
}
