using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetDisputeById;

public sealed record GetDisputeByIdQuery(Guid DisputeId) : IRequest<Result<InvoiceDisputeDto>>;
