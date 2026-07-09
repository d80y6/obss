using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateQuote;

public sealed record CreateQuoteCommand(
    string TenantId,
    Guid CustomerId,
    string? ExternalId,
    string? Category,
    string? Description,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    DateTime? ExpectedQuoteCompletionDate,
    DateTime? ExpectedFulfillmentStartDate) : IRequest<Result<QuoteDto>>;
