using MediatR;
using Obss.ServiceInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Queries.GetServiceById;

public sealed record GetServiceByIdQuery(Guid ServiceId) : IRequest<Result<ServiceDto>>;
