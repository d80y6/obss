using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Services;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Subscriptions.Application.Queries.GetActiveSubscriptionsByCustomer;

namespace Obss.Billing.Infrastructure.Services;

public sealed class BillingCalculator : IBillingCalculator
{
    private readonly IMediator _mediator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingCalculator> _logger;

    public BillingCalculator(
        IMediator mediator,
        ICurrentTenant currentTenant,
        ILogger<BillingCalculator> logger)
    {
        _mediator = mediator;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Bill> CalculateBill(Guid customerId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;
        var dueDate = periodEnd.AddDays(15);

        var bill = Bill.Create(
            tenantId,
            customerId,
            string.Empty,
            BillingPeriod.Monthly,
            periodStart,
            periodEnd,
            dueDate,
            "USD");

        var subscriptionsResult = await _mediator.Send(
            new GetActiveSubscriptionsByCustomerQuery(customerId),
            cancellationToken);

        if (subscriptionsResult.IsSuccess)
        {
            foreach (var subscription in subscriptionsResult.Value)
            {
                var line = BillLine.CreateRecurring(
                    bill.Id,
                    $"Subscription - {subscription.OfferName}",
                    subscription.Id,
                    null,
                    null,
                    subscription.Quantity,
                    subscription.Price,
                    0,
                    0.05m,
                    "USD",
                    periodStart);

                bill.AddLine(line);
            }
        }
        else
        {
            _logger.LogWarning(
                "No subscriptions found for customer {CustomerId}.",
                customerId);
        }

        bill.CalculateTotals();

        _logger.LogInformation(
            "Bill calculated for customer {CustomerId}: {LineCount} lines, grand total {GrandTotal} USD",
            customerId,
            bill.Lines.Count,
            bill.GrandTotal);

        return bill;
    }
}
