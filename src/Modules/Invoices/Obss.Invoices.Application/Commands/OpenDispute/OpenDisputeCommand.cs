using MediatR;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.OpenDispute;

public sealed record OpenDisputeCommand(
    Guid InvoiceId,
    string Reason,
    string Description,
    decimal DisputedAmount) : IRequest<Result<InvoiceDisputeDto>>;
