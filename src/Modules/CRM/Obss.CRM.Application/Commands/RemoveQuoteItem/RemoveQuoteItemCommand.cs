using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveQuoteItem;

public sealed record RemoveQuoteItemCommand(Guid QuoteId, Guid ItemId) : IRequest<Result>;
