using MediatR;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrderMilestones;

public sealed record GetProductOrderMilestonesQuery(Guid OrderId) : IRequest<Result<List<ProductOrderMilestoneDto>>>;
