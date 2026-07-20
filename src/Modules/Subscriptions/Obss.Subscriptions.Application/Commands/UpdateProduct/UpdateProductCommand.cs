using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? Name,
    string? Description,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId) : IRequest<Result<ProductDto>>;
