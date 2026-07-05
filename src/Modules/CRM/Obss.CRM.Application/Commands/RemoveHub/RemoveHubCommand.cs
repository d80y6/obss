using MediatR;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveHub;

public sealed record RemoveHubCommand(
    Guid CustomerId,
    HubType HubType,
    string Identifier) : IRequest<Result>;
