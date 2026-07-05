using MediatR;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddContactMedium;

public sealed record AddContactMediumCommand(
    Guid CustomerId,
    ContactMediumType MediumType,
    bool IsPreferred,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    List<ContactCharValue> Characteristics) : IRequest<Result>;
