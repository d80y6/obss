using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AddItemRelationship;

public sealed record AddItemRelationshipCommand(Guid OrderId, Guid ItemId, Guid TargetItemId, string Type) : IRequest<Result>;
