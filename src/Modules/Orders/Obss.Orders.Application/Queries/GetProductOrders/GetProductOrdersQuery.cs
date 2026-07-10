using MediatR;
using Obss.Orders.Application.Contracts;
using Obss.Orders.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Queries.GetProductOrders;

public sealed record GetProductOrdersQuery(
    Guid? CustomerId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    string? OrderType,
    string? SearchTerm,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<ProductOrderSummaryDto>>>;
