namespace Obss.Subscriptions.Domain.Services;

public interface IEntitlementValidator
{
    bool ValidateEntitlement(Guid subscriptionId, string entitlementType, decimal requestedAmount);
    decimal GetAvailableEntitlement(Guid subscriptionId, string entitlementType);
}
