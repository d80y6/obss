using MediatR;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrdersByCustomer;

public sealed record GetProductOrdersByCustomerQuery(
    Guid CustomerId,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<ProductOrderSummaryDto>>>;
