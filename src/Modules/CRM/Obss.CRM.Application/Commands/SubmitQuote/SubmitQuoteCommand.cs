using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.SubmitQuote;

public sealed record SubmitQuoteCommand(Guid Id) : IRequest<Result>;
