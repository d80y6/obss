using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.DeleteOffer;

public sealed record DeleteOfferCommand(Guid OfferId) : IRequest<Result>;
