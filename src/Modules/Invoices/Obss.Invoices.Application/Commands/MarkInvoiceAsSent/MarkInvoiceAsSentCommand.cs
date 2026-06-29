using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.MarkInvoiceAsSent;

public sealed record MarkInvoiceAsSentCommand(Guid InvoiceId) : IRequest<Result>;
