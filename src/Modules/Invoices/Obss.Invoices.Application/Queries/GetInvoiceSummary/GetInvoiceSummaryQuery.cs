using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceSummary;

public sealed record GetInvoiceSummaryQuery : IRequest<Result<InvoiceSummaryDto>>;
