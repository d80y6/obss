using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchProductSpecification;

public sealed record PatchProductSpecificationCommand(
    Guid ProductSpecificationId,
    string? Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber,
    LifecycleStatus? LifecycleStatus) : IRequest<Result<ProductSpecificationDto>>;
