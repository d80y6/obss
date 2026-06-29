using Obss.SharedKernel.Domain.Common;

namespace Obss.Subscriptions.Domain.Entities;

public class SubscriptionAddOn : Entity<Guid>
{
    private SubscriptionAddOn() { }

    public SubscriptionAddOn(
        Guid id,
        Guid subscriptionId,
        Guid offerId,
        string offerName,
        decimal price,
        int quantity,
        DateTime startDate,
        DateTime? endDate = null)
        : base(id)
    {
        SubscriptionId = subscriptionId;
        OfferId = offerId;
        OfferName = offerName;
        Price = price;
        Quantity = quantity;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;
    }

    public Guid SubscriptionId { get; private set; }
    public Guid OfferId { get; private set; }
    public string OfferName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }

    public Subscription Subscription { get; private set; } = null!;

    public void Deactivate()
    {
        IsActive = false;
        EndDate = DateTime.UtcNow;
    }

    public void ChangeQuantity(int newQuantity)
    {
        if (newQuantity < 1)
            throw new ArgumentException("Quantity must be at least 1.", nameof(newQuantity));

        Quantity = newQuantity;
    }

    public void ExtendEndDate(DateTime newEndDate)
    {
        if (newEndDate <= EndDate)
            throw new ArgumentException("New end date must be after current end date.", nameof(newEndDate));

        EndDate = newEndDate;
    }
}
