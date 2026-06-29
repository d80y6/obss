using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetApplicablePromotions;

public sealed record GetApplicablePromotionsQuery(
    decimal? Amount,
    int? Quantity,
    Guid? ProductId,
    Guid? SubscriptionId) : IRequest<Result<IReadOnlyList<ApplicablePromotionDto>>>;
