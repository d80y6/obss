using MediatR;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.SetHubOptIn;

public sealed record SetHubOptInCommand(
    Guid CustomerId,
    HubType HubType,
    string Identifier,
    bool IsOptIn) : IRequest<Result>;
