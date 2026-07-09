using Obss.SharedKernel.Domain.Events;

namespace Obss.Subscriptions.Application.IntegrationEvents;

public sealed class ProductStateChangedIntegrationEvent : IntegrationEvent
{
    public ProductStateChangedIntegrationEvent(Guid productId, string newState)
    {
        ProductId = productId;
        NewState = newState;
    }

    public Guid ProductId { get; }
    public string NewState { get; }
}