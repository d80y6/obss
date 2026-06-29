using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceView;

public sealed record GetInvoiceViewQuery(Guid InvoiceId) : IRequest<Result<InvoiceViewModel>>;
