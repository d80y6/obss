using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveSpecificationRelationship;

public sealed record RemoveSpecificationRelationshipCommand(Guid ProductSpecificationId, Guid RelationshipId) : IRequest<Result>;
