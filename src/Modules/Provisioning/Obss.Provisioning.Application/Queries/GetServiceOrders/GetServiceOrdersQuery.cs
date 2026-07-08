using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrders;

public sealed record GetServiceOrdersQuery(
    string? State,
    string? ExternalId,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<ServiceOrderDto>>>;

public sealed record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
