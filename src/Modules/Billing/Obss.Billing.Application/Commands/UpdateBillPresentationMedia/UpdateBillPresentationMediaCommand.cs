using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillPresentationMedia;

public sealed record UpdateBillPresentationMediaCommand(
    Guid BillingAccountId,
    Guid MediaId,
    string? EmailAddress,
    string? PaperFormat,
    string? Language,
    bool? IsPreferred) : IRequest<Result<BillPresentationMediaDto>>;
