using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetQuoteById;

public sealed record GetQuoteByIdQuery(Guid Id) : IRequest<Result<QuoteDto>>;
