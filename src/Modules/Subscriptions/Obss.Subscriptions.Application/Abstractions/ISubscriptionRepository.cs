using Obss.SharedKernel.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Abstractions;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetByIdWithDetailsAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetFilteredAsync(
        Guid? customerId,
        SubscriptionStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetSubscriptionsDueForRenewalAsync(DateTime upToDate, CancellationToken cancellationToken = default);
    Task<int> GetFilteredCountAsync(
        Guid? customerId,
        SubscriptionStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        CancellationToken cancellationToken = default);
}
