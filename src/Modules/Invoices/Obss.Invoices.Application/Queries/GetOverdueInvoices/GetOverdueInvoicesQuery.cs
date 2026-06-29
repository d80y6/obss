using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetOverdueInvoices;

public sealed record GetOverdueInvoicesQuery : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
