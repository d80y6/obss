using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateQuote;

public sealed record UpdateQuoteCommand(
    Guid Id,
    string? ExternalId,
    string? Category,
    string? Description) : IRequest<Result<QuoteDto>>;
