using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.DeactivatePromotion;

public sealed record DeactivatePromotionCommand(Guid PromotionId) : IRequest<Result>;
