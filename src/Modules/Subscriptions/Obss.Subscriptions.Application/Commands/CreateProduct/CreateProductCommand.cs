using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid TenantId,
    Guid CustomerId,
    string? Name,
    string? Description,
    Guid? ProductSpecificationId,
    Guid? ProductOfferingId) : IRequest<Result<ProductDto>>;