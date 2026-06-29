using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetOfferById;

public sealed record GetOfferByIdQuery(Guid OfferId) : IRequest<Result<OfferDto>>;
