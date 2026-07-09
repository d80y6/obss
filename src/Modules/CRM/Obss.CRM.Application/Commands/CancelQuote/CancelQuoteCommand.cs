using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CancelQuote;

public sealed record CancelQuoteCommand(Guid Id) : IRequest<Result>;
