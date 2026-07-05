using MediatR;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<OrderSummaryDto>>>;
