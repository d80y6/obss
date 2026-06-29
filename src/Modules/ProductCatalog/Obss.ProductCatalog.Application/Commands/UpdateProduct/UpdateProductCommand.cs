using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string? Description,
    Guid? CategoryId,
    bool IsShippable,
    bool Taxable,
    string? TaxCategory) : IRequest<Result<ProductDto>>;
