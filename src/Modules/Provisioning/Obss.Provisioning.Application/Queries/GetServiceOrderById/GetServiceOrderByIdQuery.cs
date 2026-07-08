using MediatR;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Queries.GetServiceOrderById;

public sealed record GetServiceOrderByIdQuery(Guid Id) : IRequest<Result<ServiceOrderDto>>;
