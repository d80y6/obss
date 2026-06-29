using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentsByInvoice;

public sealed record GetPaymentsByInvoiceQuery(Guid InvoiceId) : IRequest<Result<IReadOnlyList<PaymentDto>>>;
