using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.ApproveQuote;

public sealed record ApproveQuoteCommand(Guid Id) : IRequest<Result>;
