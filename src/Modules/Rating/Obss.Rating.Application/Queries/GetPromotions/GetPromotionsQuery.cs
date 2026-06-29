using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetPromotions;

public sealed record GetPromotionsQuery : IRequest<Result<IReadOnlyList<PromotionDto>>>;
