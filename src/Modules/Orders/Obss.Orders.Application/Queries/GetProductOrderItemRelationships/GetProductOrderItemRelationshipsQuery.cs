using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderItemRelationships;

public sealed record GetProductOrderItemRelationshipsQuery(Guid OrderId, Guid? ItemId) : IRequest<Result<List<ProductOrderItemRelationshipDto>>>;
