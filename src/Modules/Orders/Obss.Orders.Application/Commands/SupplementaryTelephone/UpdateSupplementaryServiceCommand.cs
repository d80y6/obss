using System.Text.Json;
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SupplementaryTelephone;

public sealed record UpdateSupplementaryServiceCommand(
    Guid OrderId,
    Guid OrderItemId,
    Guid SubscriptionId,
    string TelephoneNumber,
    ServiceFeature ServiceFeature,
    JsonDocument Configuration) : IRequest<Result<SupplementaryServiceLifecycleResult>>;
