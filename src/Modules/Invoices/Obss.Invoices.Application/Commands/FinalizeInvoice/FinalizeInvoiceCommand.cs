using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.FinalizeInvoice;

public sealed record FinalizeInvoiceCommand(Guid InvoiceId) : IRequest<Result>;
