using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchProduct;

public sealed record PatchProductCommand(
    Guid ProductId,
    Optional<string> Name,
    Optional<string?> Description,
    Optional<Guid?> CategoryId,
    Optional<bool> IsShippable,
    Optional<bool> Taxable,
    Optional<string?> TaxCategory) : IRequest<Result<ProductDto>>;
