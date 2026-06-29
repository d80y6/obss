using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.CancelInvoice;

public sealed record CancelInvoiceCommand(Guid InvoiceId, string Reason) : IRequest<Result>;
