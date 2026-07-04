using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;

public sealed record GetProductSpecificationByIdQuery(Guid Id) : IRequest<Result<ProductSpecificationDto>>;
