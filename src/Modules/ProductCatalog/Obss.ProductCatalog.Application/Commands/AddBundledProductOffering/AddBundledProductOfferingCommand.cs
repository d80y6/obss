using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddBundledProductOffering;

public sealed record AddBundledProductOfferingCommand(
    Guid OfferId,
    Guid BundledOfferId,
    string? Name,
    int Quantity,
    string? ReferralType) : IRequest<Result<BundledProductOfferingDto>>;
