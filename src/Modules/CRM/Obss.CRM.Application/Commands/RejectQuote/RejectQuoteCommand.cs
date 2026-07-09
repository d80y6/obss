using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RejectQuote;

public sealed record RejectQuoteCommand(Guid Id) : IRequest<Result>;
