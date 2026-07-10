using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.RemoveItemRelationship;

public sealed record RemoveItemRelationshipCommand(Guid OrderId, Guid RelationshipId) : IRequest<Result>;
