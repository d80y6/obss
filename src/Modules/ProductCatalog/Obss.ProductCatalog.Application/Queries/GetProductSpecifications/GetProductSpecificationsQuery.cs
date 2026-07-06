using MediatR;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecifications;

public sealed record GetProductSpecificationsQuery(
    string? SearchTerm,
    LifecycleStatus? Status,
    string? Brand,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<ProductSpecificationDto>>>;
