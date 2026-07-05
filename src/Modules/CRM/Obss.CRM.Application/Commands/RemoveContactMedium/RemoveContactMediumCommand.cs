using MediatR;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveContactMedium;

public sealed record RemoveContactMediumCommand(Guid CustomerId, ContactMediumType MediumType) : IRequest<Result>;
