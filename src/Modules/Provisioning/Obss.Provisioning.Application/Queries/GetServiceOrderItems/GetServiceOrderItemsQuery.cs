using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderItems;

public sealed record GetServiceOrderItemsQuery(Guid ServiceOrderId) : IRequest<Result<List<ServiceOrderItemDto>>>;
