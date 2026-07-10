using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed record CreateBillPresentationMediaCommand(
    Guid BillingAccountId,
    string MediaType,
    string? EmailAddress,
    string? PaperFormat,
    string Language,
    bool IsPreferred) : IRequest<Result<BillPresentationMediaDto>>;
