using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateProductSpecification;

public sealed record UpdateProductSpecificationCommand(
    Guid ProductSpecificationId,
    string Name,
    string? Description,
    string? Brand,
    string? Version,
    string? ProductNumber) : IRequest<Result<ProductSpecificationDto>>;
