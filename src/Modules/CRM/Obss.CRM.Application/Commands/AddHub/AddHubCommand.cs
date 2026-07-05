using MediatR;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddHub;

public sealed record AddHubCommand(
    Guid CustomerId,
    HubType HubType,
    string Identifier,
    bool IsOptIn,
    DateTime? ValidFrom,
    DateTime? ValidUntil) : IRequest<Result>;
