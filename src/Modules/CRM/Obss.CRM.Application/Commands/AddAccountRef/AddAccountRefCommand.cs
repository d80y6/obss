using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddAccountRef;

public sealed record AddAccountRefCommand(
    Guid CustomerId,
    Guid BillingAccountId,
    string Name,
    string AccountType,
    string Role,
    string? Href) : IRequest<Result>;
