using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoicesByCustomer;

public sealed record GetInvoicesByCustomerQuery(
    Guid CustomerId,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<Result<IReadOnlyList<InvoiceDto>>>;
