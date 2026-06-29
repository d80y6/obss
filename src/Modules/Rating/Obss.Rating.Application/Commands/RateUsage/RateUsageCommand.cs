using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.RateUsage;

public sealed record RateUsageCommand : IRequest<Result<int>>;
