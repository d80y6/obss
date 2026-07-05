using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddCreditProfile;

public sealed record AddCreditProfileCommand(
    Guid CustomerId,
    int Score,
    string ScoreType,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? RiskRating) : IRequest<Result>;
