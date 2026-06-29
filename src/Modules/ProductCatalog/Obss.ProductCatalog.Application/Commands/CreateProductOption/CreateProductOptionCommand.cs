using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateProductOption;

public sealed record CreateProductOptionCommand(
    Guid ProductId,
    string Name,
    string? Description,
    OptionType OptionType,
    bool IsRequired,
    bool IsMultiSelect,
    int SortOrder,
    List<CreateOptionValueDto>? Values) : IRequest<Result<ProductOptionDto>>;
