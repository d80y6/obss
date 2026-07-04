using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveBundledProductOffering;

public sealed record RemoveBundledProductOfferingCommand(
    Guid OfferId,
    Guid BundledOfferingId) : IRequest<Result>;
