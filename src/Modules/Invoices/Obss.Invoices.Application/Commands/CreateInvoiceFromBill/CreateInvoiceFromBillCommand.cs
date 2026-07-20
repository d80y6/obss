using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.CreateInvoiceFromBill;

public sealed record CreateInvoiceFromBillCommand(
    string TenantId,
    Guid BillId,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerAddress,
    string Currency) : IRequest<Result<InvoiceDto>>;
