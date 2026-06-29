using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceDisputes;

public sealed record GetInvoiceDisputesQuery(Guid? InvoiceId, string? Status) : IRequest<Result<IReadOnlyList<InvoiceDisputeDto>>>;
