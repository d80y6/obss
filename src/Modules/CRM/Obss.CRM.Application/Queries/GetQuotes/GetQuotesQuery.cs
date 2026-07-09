using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetQuotes;

public sealed record GetQuotesQuery : IRequest<Result<List<QuoteDto>>>;
