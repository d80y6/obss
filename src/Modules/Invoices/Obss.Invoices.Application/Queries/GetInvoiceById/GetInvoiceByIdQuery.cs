using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceById;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
