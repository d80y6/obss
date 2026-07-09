using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Queries.GetQuotesByCustomer;

public sealed record GetQuotesByCustomerQuery(Guid CustomerId) : IRequest<Result<List<QuoteDto>>>;
