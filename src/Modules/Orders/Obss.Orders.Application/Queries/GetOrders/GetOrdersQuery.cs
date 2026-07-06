using MediatR;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetOrders;

public sealed record GetOrdersQuery(
    Guid? CustomerId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    string? OrderType,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<OrderSummaryDto>>>;
