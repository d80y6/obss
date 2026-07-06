using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceSpecificationById;

public sealed record GetServiceSpecificationByIdQuery(Guid Id) : IRequest<ServiceSpecificationDto?>;
