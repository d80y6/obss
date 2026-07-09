using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateQuoteItem;

public sealed record UpdateQuoteItemCommand(
    Guid QuoteId,
    Guid ItemId,
    string Action,
    int Quantity,
    Guid? ProductOfferingId,
    string? ProductOfferingName,
    Guid? ProductId) : IRequest<Result>;
