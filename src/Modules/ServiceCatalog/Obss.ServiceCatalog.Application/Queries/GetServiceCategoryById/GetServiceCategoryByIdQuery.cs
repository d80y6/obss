using MediatR;
using Obss.ServiceCatalog.Application.DTOs;

namespace Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

public sealed record GetServiceCategoryByIdQuery(Guid Id) : IRequest<ServiceCategoryDto?>;
