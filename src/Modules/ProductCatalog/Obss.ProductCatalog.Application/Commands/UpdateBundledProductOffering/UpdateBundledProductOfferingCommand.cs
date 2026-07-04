using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.UpdateBundledProductOffering;

public sealed record UpdateBundledProductOfferingCommand(
    Guid OfferId,
    Guid BundledOfferingId,
    string? Name,
    int Quantity,
    string? ReferralType) : IRequest<Result<BundledProductOfferingDto>>;
