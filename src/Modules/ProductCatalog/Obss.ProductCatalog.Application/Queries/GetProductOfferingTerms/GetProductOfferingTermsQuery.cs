using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductOfferingTerms;

public sealed record GetProductOfferingTermsQuery(Guid OfferId) : IRequest<Result<List<ProductOfferingTermDto>>>;
