using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.EventManagement.Application.Abstractions;

public interface IWebhookEventRepository : IRepository<WebhookEvent>
{
    Task<IReadOnlyList<WebhookEvent>> GetPendingEventsAsync(CancellationToken cancellationToken = default);
}
