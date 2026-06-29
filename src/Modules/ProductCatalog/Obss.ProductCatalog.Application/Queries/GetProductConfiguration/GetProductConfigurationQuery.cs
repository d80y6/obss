using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductConfiguration;

public sealed record GetProductConfigurationQuery(Guid ProductId) : IRequest<Result<ProductConfigurationDto>>;
