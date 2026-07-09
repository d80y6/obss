using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AcceptQuote;

public sealed record AcceptQuoteCommand(Guid Id) : IRequest<Result>;
