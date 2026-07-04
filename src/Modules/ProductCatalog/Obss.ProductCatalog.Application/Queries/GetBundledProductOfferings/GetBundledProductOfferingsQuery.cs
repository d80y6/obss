using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetBundledProductOfferings;

public sealed record GetBundledProductOfferingsQuery(Guid OfferId) : IRequest<Result<List<BundledProductOfferingDto>>>;
