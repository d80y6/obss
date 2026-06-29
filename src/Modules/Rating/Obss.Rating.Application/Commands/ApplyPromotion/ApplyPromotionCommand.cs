using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.ApplyPromotion;

public sealed record ApplyPromotionCommand(
    string? PromotionCode,
    Guid? PromotionId,
    decimal Amount,
    int Quantity,
    Guid? ProductId,
    Guid? SubscriptionId) : IRequest<Result<ApplicablePromotionDto>>;
