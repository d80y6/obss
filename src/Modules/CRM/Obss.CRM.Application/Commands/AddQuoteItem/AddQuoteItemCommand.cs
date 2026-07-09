using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddQuoteItem;

public sealed record AddQuoteItemCommand(
    Guid QuoteId,
    string Action,
    int Quantity,
    Guid? ProductOfferingId,
    string? ProductOfferingName,
    Guid? ProductId) : IRequest<Result>;
