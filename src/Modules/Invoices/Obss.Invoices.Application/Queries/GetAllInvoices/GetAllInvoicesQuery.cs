using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetAllInvoices;

public sealed record GetAllInvoicesQuery : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
