using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServices;

public sealed record GetServicesQuery(
    Guid? CustomerId,
    string? ServiceType,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<ServiceDto>>>;
