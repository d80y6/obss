using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed record ChangeFtthSpeedCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    int NewDownloadSpeedMbps,
    int NewUploadSpeedMbps,
    string? Reason) : IRequest<Result<FtthLifecycleResult>>;
