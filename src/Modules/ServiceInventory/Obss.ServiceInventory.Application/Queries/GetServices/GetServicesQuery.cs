using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServices;

public sealed record GetServicesQuery(
    Guid? CustomerId,
    string? ServiceType,
    string? Status,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<IReadOnlyList<ServiceDto>>>;
